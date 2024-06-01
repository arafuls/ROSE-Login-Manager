using Newtonsoft.Json;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Rose_Updater;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace ROSE_Login_Manager.Services
{
    public class RoseUpdater
    {
        private const string RemoteUrl = "https://updates.roseonlinegame.com";
        private const string RemoteManifestUrl = RemoteUrl + "/manifest.json";
        private const string LocalManifestFileName = "local_manifest.json";

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



        /// <summary>
        ///     Default constructor
        /// </summary>
        public RoseUpdater()
        {
            InitializeAsync().GetAwaiter().GetResult();
        }



        /// <summary>
        ///     Asynchronously initializes the RoseUpdater by setting up the root folder, retrieving the local and remote manifests,
        ///     and executing the main operations.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            RootFolder = GlobalVariables.Instance.RoseGameFolder;
            LocalManifest = GetLocalManifest();
            RemoteManifest = DownloadRemoteManifest();

            await Run();
        }



        /// <summary>
        ///     Runs the main operations of the updater, including verifying the updater and game file integrity.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task Run()
        {
            VerifyRoseUpdater();
            VerifyGameFileIntegrity();
        }



        /// <summary>
        ///     Verifies the Rose Online updater integrity and updates if necessary.
        /// </summary>
        private async void VerifyRoseUpdater()
        {
            if (UpdaterIsLatestAndExists)
            {
                return;
            }

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

            _ = DownloadUpdaterWithBitaAsync();
            await SaveLocalManifest(newManifest);
        }



        /// <summary>
        ///     Verifies the integrity of game files and performs updates if necessary.
        /// </summary>
        /// <remarks>
        ///     This method checks the integrity of local game files against the remote manifest. If any files need to be updated,
        ///     it downloads the updated files and replaces the local copies.
        /// </remarks>
        private async Task VerifyGameFileIntegrity()
        {
            VerificationResults verificationResults = VerifyLocalFiles();
            if (verificationResults.FilesToUpdate.Count == 0)
            {
                return;
            }

            // TODO: Send update started message

            await UpdateLocalFiles(verificationResults.FilesToUpdate).ConfigureAwait(false);

            // TODO: Send update complete message
        }



        /// <summary>
        ///     Verifies the local files against the remote manifest and identifies files that need to be updated.
        ///     Calculates the total size of the files listed in the remote manifest and the size of files that are already downloaded.
        /// </summary>
        /// <returns>
        ///     A VerificationResults object containing:
        ///         - FilesToUpdate: A list of tuples where each tuple contains the URI of the remote file and its corresponding RemoteManifestFileEntry.
        ///         - TotalSize: The total size of all files listed in the remote manifest.
        ///         - AlreadyDownloadedSize: The total size of files that are already downloaded and up-to-date.
        /// </returns>
        private VerificationResults VerifyLocalFiles()
        {
            List<(Uri, RemoteManifestFileEntry)> filesToUpdate = [];
            long totalSize = 0;
            long alreadyDownloadedSize = 0;

            // Pre-calculate the full file paths for local files
            HashSet<string> localFullPaths = LocalManifest.Files.Select(file => Path.Combine(RootFolder, file.Path)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (RemoteManifestFileEntry remoteEntry in remoteManifest.Files)
            {
                string localFilePath = Path.Combine(RootFolder, remoteEntry.SourcePath);
                totalSize += remoteEntry.SourceSize;

                // Check if the file exists locally and if it matches the remote hash
                bool fileIsOutdated = !localFullPaths.Contains(localFilePath) ||
                                      !LocalManifest.Files.Any(localEntry =>
                                          localEntry.Path == remoteEntry.SourcePath &&
                                          localEntry.Hash.SequenceEqual(remoteEntry.SourceHash));

                // Update the lists based on the file status
                if (!fileIsOutdated)
                {
                    alreadyDownloadedSize += remoteEntry.SourceSize;
                }
                else
                {
                    filesToUpdate.Add((new Uri(new Uri(RemoteUrl), remoteEntry.Path), remoteEntry));
                }
            }

            return new VerificationResults
            {
                FilesToUpdate = filesToUpdate,
                TotalSize = totalSize,
                AlreadyDownloadedSize = alreadyDownloadedSize
            };
        }



        /// <summary>
        ///     Asynchronously updates local files based on the provided list of file URIs and their corresponding remote manifest entries.
        /// </summary>
        /// <param name="files">A list of tuples containing the file URIs and their corresponding remote manifest entries.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task UpdateLocalFiles(List<(Uri, RemoteManifestFileEntry)> files)
        {
            // Create a dictionary from the remote manifest entries for quick lookup by file path
            Dictionary<string, RemoteManifestFileEntry> remoteFilePaths = RemoteManifest.Files.ToDictionary(file => file.SourcePath);

            LocalManifest newLocalManifest = GetLocalManifest();

            foreach ((Uri, RemoteManifestFileEntry) file in files)
            {
                if (await DownloadFileWithBitaAsync(remoteFilePaths[file.Item2.SourcePath]).ConfigureAwait(false))
                {
                    newLocalManifest.Files.Add(ConvertRemoteFileEntryToLocal(remoteFilePaths[file.Item2.SourcePath]));
                }
            }

            await SaveLocalManifest(newLocalManifest).ConfigureAwait(false);
        }



        /// <summary>
        ///     Converts a remote manifest file entry to a local manifest file entry.
        /// </summary>
        /// <param name="remoteFile">The remote manifest file entry to be converted.</param>
        /// <returns>The corresponding local manifest file entry.</returns>
        private LocalManifestFileEntry ConvertRemoteFileEntryToLocal(RemoteManifestFileEntry remoteFile)
        {
            LocalManifestFileEntry localFileEntry = new()
            {
                Path = remoteFile.SourcePath,
                Hash = remoteFile.SourceHash,
                Size = remoteFile.SourceSize
            };
            return localFileEntry;
        }



        /// <summary>
        ///     Retrieves the remote manifest asynchronously from the specified URL.
        /// </summary>
        /// <returns>The remote manifest object.</returns>
        public static async Task<RemoteManifest> GetRemoteManifest()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            HttpResponseMessage response = await httpClient.GetAsync(RemoteManifestUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RemoteManifest>(responseBody);
        }



        /// <summary>
        ///     Downloads the updater file using Bita tool asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns true if the download is successful, otherwise false.</returns>
        private async Task<bool> DownloadUpdaterWithBitaAsync()
        {
            try
            {
                string bitaExecutablePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName, "Tools", "Bita", "bita.exe");
                if (!File.Exists(bitaExecutablePath))
                    throw new FileNotFoundException("Bita executable not found", bitaExecutablePath);

                string archiveUrl = Path.Combine(RemoteUrl, RemoteManifest.Updater.Path);
                string outputPath = Path.Combine(RootFolder, RemoteManifest.Updater.SourcePath);
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

                string output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                string error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

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




        /// <summary>
        ///     Downloads a remote file using the Bita tool asynchronously.
        /// </summary>
        /// <param name="file">The file entry from the remote manifest to be downloaded.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the download is successful, otherwise false.</returns>
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





        /// <summary>
        ///     Compares the hash of a remote file with the hash of the corresponding local file.
        /// </summary>
        /// <param name="remoteEntryPath">The path of the remote file.</param>
        /// <param name="remoteHash">The hash of the remote file.</param>
        /// <returns>True if the hashes match, false otherwise.</returns>
        private bool CompareHash(string remoteEntryPath, byte[] remoteHash) =>
            LocalManifest.Files.Any(file => file.Path == remoteEntryPath && file.Hash.SequenceEqual(remoteHash));



        /// <summary>
        ///     Indicates whether the updater in the local manifest is both present and up to date.
        /// </summary>
        public bool UpdaterIsLatestAndExists
        {
            get
            {
                // Check if the updater exists and if its hash matches the hash in the remote manifest
                try
                {
                    return !string.IsNullOrEmpty(LocalManifest.Updater.Path) &&
                           File.Exists(Path.Combine(RootFolder, LocalManifest.Updater.Path)) &&
                           LocalManifest.Updater.Hash.SequenceEqual(RemoteManifest.Updater.SourceHash);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error validating updater: {ex.Message}");
                    return false;
                }
            }
        }



        /// <summary>
        ///     Retrieves the local manifest if it exists; otherwise, returns a new instance of LocalManifest.
        /// </summary>
        /// <returns>The local manifest if it exists; otherwise, a new instance of LocalManifest.</returns>
        private static LocalManifest GetLocalManifest()
        {
            string localManifestPath = Path.Combine(RootFolder, "updater", new Uri(RemoteUrl).Host, LocalManifestFileName);

            if (!File.Exists(localManifestPath))
                return new LocalManifest();

            string json = File.ReadAllText(localManifestPath);
            return JsonConvert.DeserializeObject<LocalManifest>(json);
        }



        /// <summary>
        ///     Downloads the remote manifest file asynchronously from the specified URL.
        /// </summary>
        /// <returns>The deserialized remote manifest object.</returns>
        private static RemoteManifest DownloadRemoteManifest()
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            HttpResponseMessage response = httpClient.GetAsync(RemoteManifestUrl).Result; // Blocking call
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result; // Blocking call
            return JsonConvert.DeserializeObject<RemoteManifest>(responseBody);
        }



        /// <summary>
        ///     Saves the provided local manifest to the file system.
        /// </summary>
        /// <param name="newLocalManifest">The local manifest to save.</param>
        /// <exception cref="IOException">Thrown if an I/O error occurs while writing to the file.</exception>
        private static async Task SaveLocalManifest(LocalManifest newLocalManifest)
        {
            string localManifestPath = Path.Combine(RootFolder, "updater", new Uri(RemoteUrl).Host, LocalManifestFileName);

            try
            {
                // Ensure parent directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(localManifestPath));

                // Serialize the object to a JSON string
                string json = JsonConvert.SerializeObject(newLocalManifest, new JsonSerializerSettings
                {
                    Converters = { new LocalManifestFileEntryConverter() }
                });

                // Write the JSON string to the file
                await File.WriteAllTextAsync(localManifestPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save local manifest to {localManifestPath}: {ex.Message}");
                throw;
            }
        }
    }
}