using Newtonsoft.Json;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Rose_Updater;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;



namespace ROSE_Login_Manager.Services
{
    public class RoseUpdater
    {
        private const string RemoteUrl = "https://updates.roseonlinegame.com";
        private const string RemoteManifestUrl = RemoteUrl + "/manifest.json";
        private const string LocalManifestFileName = "local_manifest.json";

        private static readonly HttpClient _client = new();

        private static string? RootFolder { get; set; }


        private RemoteManifest remoteManifest;
        private RemoteManifest RemoteManifest
        {
            get => remoteManifest;
            set => remoteManifest = value;
        }

        private LocalManifest localManifest;
        private LocalManifest LocalManifest
        {
            get => localManifest;
            set => localManifest = value;
        }



        public RoseUpdater()
        {
            RootFolder = GlobalVariables.Instance.RoseGameFolder;

            RunUpdater();
        }


        public async void RunUpdater()
        {
            // Check if we are out of date
            if (!await CompareManifests())
            {
                // TODO: Send message to disable launch buttons until complete
                MessageBox.Show("Update in progress");

                await ParseManifestFilesAsync();

                // Update the local manifest with only the data for the updater
                LocalManifest newManifest = new()
                {
                    Version = 1,
                    Updater = new LocalManifestFileEntry
                    {
                        Path = RemoteManifest.Updater.SourcePath,
                        Hash = RemoteManifest.Updater.SourceHash,
                        Size = RemoteManifest.Updater.SourceSize
                    },
                    Files = LocalManifest.Files // Preserving existing files data
                };

                await SaveLocalManifest(newManifest);
            }



            // Channel for communication
            var channel = Channel.CreateUnbounded<LocalManifestFileEntry>();
            var rx = channel.Reader;
            var tx = channel.Writer;

            var currentLocalFileData = new Dictionary<string, LocalManifestFileEntry>();

            VerificationResults verificationResults = VerifyLocalFiles();

            var hashNewLocalManifest = new HashSet<string>();
            var newLocalManifest = new LocalManifest
            {
                Version = 1,
                Updater = localManifest.Updater,
                Files = []
            };

            // Define the work task
            var work = Task.Run(async () =>
            {
                var hashNewLocalManifest = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var newLocalManifest = new LocalManifest
                {
                    Version = 1,
                    Updater = localManifest.Updater
                };

                await foreach (var manifest in rx.ReadAllAsync())
                {
                    hashNewLocalManifest.Add(Path.GetFullPath(manifest.Path));
                    newLocalManifest.Files.Add(manifest);
                }

                return (hashNewLocalManifest, newLocalManifest);
            });

            // Start the asynchronous file download tasks
            List<Task> cloneTasks = await GetRemoteFiles(verificationResults.FilesToUpdate, tx);

            // Wait for all clone tasks to complete
            await Task.WhenAll(cloneTasks);

            // Await the 'work' task to get the results
            (hashNewLocalManifest, newLocalManifest) = await work;

            // Iterate through the current local file data
            foreach (var kvp in currentLocalFileData)
            {
                string path = kvp.Key;
                LocalManifestFileEntry localEntry = kvp.Value;

                // If the path is not in the hash set of new local manifest, add it to the new local manifest
                if (!hashNewLocalManifest.Contains(Path.GetFullPath(path)))
                {
                    newLocalManifest.Files.Add(localEntry);
                }
            }

            await SaveLocalManifest(newLocalManifest);

            // TODO: Send message to enable launch buttons until complete
            MessageBox.Show("Update complete");
        }



        private VerificationResults VerifyLocalFiles()
        {
            List<(Uri, RemoteManifestFileEntry)> filesToUpdate = [];

            long totalSize = 0;
            long alreadyDownloadedSize = 0;

            // Pre-calculate the full file paths for local files
            HashSet<string> localFullPaths = LocalManifest.Files.Select(file => Path.Combine(RootFolder, file.Path)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (RemoteManifestFileEntry remoteEntry in remoteManifest.Files)
            {
                string outputPath = Path.Combine(RootFolder, remoteEntry.SourcePath);

                bool needsUpdate()
                {
                    if (!localFullPaths.Contains(outputPath))
                    {
                        return true;
                    }

                    LocalManifestFileEntry? localEntry = LocalManifest.Files.FirstOrDefault(entry => entry.Path == remoteEntry.SourcePath);
                    if (localEntry != null && File.Exists(outputPath))
                    {
                        if (localEntry.Hash.SequenceEqual(remoteEntry.SourceHash))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                totalSize += remoteEntry.SourceSize;

                if (!needsUpdate())
                {
                    Console.WriteLine($"Skipping file {outputPath} as it is already present");
                    alreadyDownloadedSize += remoteEntry.SourceSize;
                    continue;
                }

                //Uri remoteUrl = new Uri(RemoteUrl);
                //filesToUpdate.Add((new Uri(Path.Combine(remoteUrl.Host, remoteEntry.Path)), remoteEntry));
                //filesToUpdate.Add((new Uri(Path.Combine(RemoteUrl, remoteEntry.Path)), remoteEntry));
                filesToUpdate.Add((new Uri(new Uri(RemoteUrl), remoteEntry.Path), remoteEntry));
            }

            return new VerificationResults
            {
                FilesToUpdate = filesToUpdate,
                TotalSize = totalSize,
                AlreadyDownloadedSize = alreadyDownloadedSize
            };
        }



        public static async Task<RemoteManifest> GetRemoteManifest()
        {
            Console.WriteLine("Downloading remote manifest");

            HttpResponseMessage response = await _client.GetAsync(RemoteManifestUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<RemoteManifest>(responseBody);
        }



        public async Task ParseManifestFilesAsync()
        {
            // List to store the tasks for updating files
            List<Task> updateTasks = [];

            foreach (RemoteManifestFileEntry fileEntry in RemoteManifest.Files)
            {
                string localfile = Path.Combine(RootFolder, fileEntry.SourcePath);
                if (LocalManifest.Files != null && File.Exists(localfile) && CompareHash(fileEntry.SourcePath, fileEntry.SourceHash))
                {   // Up to date entry
                    continue;
                }

                // Start the update task and add it to the list
                Task updateTask = UpdateFileAsync(fileEntry);
                updateTasks.Add(updateTask);
            }

            // Wait for all update tasks to complete
            await Task.WhenAll(updateTasks);
        }




        private static async Task UpdateFileAsync(RemoteManifestFileEntry fileEntry)
        {
            string directoryPath = Path.Combine(RootFolder, Path.GetDirectoryName(fileEntry.SourcePath) ?? string.Empty);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Download the file using DownloadFileWithBitaAsync
            if (await DownloadFileWithBitaAsync(fileEntry))
            {
                //Console.WriteLine($"Successfully updated {fileEntry.Path}");
            }
            else
            {
                throw new Exception("File update failed");
            }
        }



        private static async Task<bool> DownloadFileWithBitaAsync(RemoteManifestFileEntry file)
        {
            try
            {
                string bitaExecutablePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName, "Tools", "Bita", "bita.exe");
                if (!File.Exists(bitaExecutablePath))
                    throw new FileNotFoundException("Bita executable not found", bitaExecutablePath);

                string archiveUrl = Path.Combine(RemoteUrl, file.Path);
                string outputPath = Path.Combine(RootFolder, file.SourcePath);
                string arguments = $"clone --seed-output \"{archiveUrl}\" \"{outputPath}\"";

                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = bitaExecutablePath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error running bita: {error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running bita: {ex.Message}");
                return false;
            }
        }





        private bool CompareHash(string remoteEntryPath, byte[] remoteHash)
        {
            LocalManifestFileEntry? local = LocalManifest.Files.FirstOrDefault(file => file.Path == remoteEntryPath);

            if (local == null) 
            { 
                return false; 
            }

            return local.Hash.SequenceEqual(remoteHash);
        }



        /// <summary>
        ///     Compares the hash values of the local and remote manifests to validate the updater integrity.
        /// </summary>
        /// <returns>
        ///     True if the hash values match, indicating that the updater is valid and up-to-date; otherwise, false.
        /// </returns>
        public async Task<bool> CompareManifests()
        {
            try
            {
                // Load and Fetch Manifests
                LocalManifest = GetLocalManifest();
                RemoteManifest = await DownloadRemoteManifestAsync().ConfigureAwait(false);

                // Get the hash values from the updaters
                byte[] localHash = LocalManifest.Updater.Hash;
                byte[] remoteHash = RemoteManifest.Updater.SourceHash;

                return localHash.SequenceEqual(remoteHash);
            }
            catch (Exception ex)
            {   
                Console.WriteLine($"Error validating updater: {ex.Message}");
                return false;
            }
        }



        /// <summary>
        ///     Loads the local manifest file from the file system.
        /// </summary>
        /// <returns>The deserialized local manifest object.</returns>
        private static LocalManifest GetLocalManifest()
        {
            Uri remoteUri = new(RemoteUrl);
            string localManifestPath = Path.Combine(RootFolder, "updater", remoteUri.Host, LocalManifestFileName);

            if (!File.Exists(localManifestPath))
            {
                // If the file does not exist, return a new LocalManifest
                return new LocalManifest();
            }

            // If the file exists, read its contents and deserialize
            string json = File.ReadAllText(localManifestPath);
            return JsonConvert.DeserializeObject<LocalManifest>(json);
        }



        /// <summary>
        ///     Downloads the remote manifest file asynchronously from the specified URL.
        /// </summary>
        /// <returns>The deserialized remote manifest object.</returns>
        private static async Task<RemoteManifest> DownloadRemoteManifestAsync()
        {
            HttpResponseMessage response = await _client.GetAsync(RemoteManifestUrl).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<RemoteManifest>(responseBody);
        }




        private static async Task SaveLocalManifest(LocalManifest newLocalManifest)
        {
            Uri remoteUri = new(RemoteUrl);
            string localManifestParentDir = Path.Combine(RootFolder, "updater", remoteUri.Host);
            string localManifestPath = Path.Combine(localManifestParentDir, LocalManifestFileName);

            try
            {
                if (!string.IsNullOrEmpty(localManifestParentDir))
                {
                    Directory.CreateDirectory(localManifestParentDir);
                }

                // Serialize the object to a JSON string
                string json = JsonConvert.SerializeObject(newLocalManifest, Formatting.Indented);

                // Write the JSON string to the file
                await File.WriteAllTextAsync(localManifestPath, json);

                Console.WriteLine($"Saved local manifest to {localManifestPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save local manifest to {localManifestPath}: {ex.Message}");
                throw;
            }
        }


        public static Task<List<Task>> GetRemoteFiles(
            List<(Uri, RemoteManifestFileEntry)> filesToUpdate,
            ChannelWriter<LocalManifestFileEntry> channelWriter)
        {
            return Task.Run(() =>
            {
                string[] TEXT_FILE_EXTENSIONS = [".txt", ".json", ".xml"];
                var cloneTasks = new List<Task>();

                foreach (var entry in filesToUpdate)
                {
                    var (cloneUrl, remoteEntry) = entry;
                    var outputPath = Path.Combine(RootFolder, remoteEntry.SourcePath);
                    var clonedWriter = channelWriter; // Use the same channel writer

                    // Handle text files
                    var ext = Path.GetExtension(outputPath);
                    if (TEXT_FILE_EXTENSIONS.Contains(ext))
                    {
                        try
                        {
                            if (File.Exists(outputPath))
                            {
                                File.Delete(outputPath);
                                Console.WriteLine($"Deleted text file: {outputPath}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to delete text file {outputPath}: {e.Message}");
                        }
                    }

                    cloneTasks.Add(Task.Run(async () =>
                    {
                        Console.WriteLine($"Downloading {cloneUrl}");
                        try
                        {
                            var response = await _client.GetAsync(cloneUrl);
                            response.EnsureSuccessStatusCode();

                            var content = await response.Content.ReadAsByteArrayAsync();
                            await File.WriteAllBytesAsync(outputPath, content);

                            Console.WriteLine($"Cloned {cloneUrl} to {outputPath}");

                            var localEntry = new LocalManifestFileEntry
                            {
                                Path = remoteEntry.SourcePath,
                                Hash = remoteEntry.SourceHash,
                                Size = remoteEntry.SourceSize
                            };

                            await clonedWriter.WriteAsync(localEntry);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Failed to clone {cloneUrl}: {e.Message}");
                        }
                    }));
                }

                return cloneTasks;
            });
        }
    }
}