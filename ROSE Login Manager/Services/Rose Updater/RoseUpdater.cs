using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Rose_Updater;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
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

            await Run().ConfigureAwait(false);
        }



        /// <summary>
        ///     Runs the main operations of the updater, including verifying the updater and game file integrity.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task Run()
        {
            VerifyRoseUpdater();
            await VerifyGameFileIntegrity().ConfigureAwait(false);
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
            await SaveLocalManifest(newManifest).ConfigureAwait(false);
        }



        /// <summary>
        ///     Asynchronously verifies the integrity of game files against the remote manifest and performs updates if necessary.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task VerifyGameFileIntegrity()
        {
            VerificationResults verificationResults = await VerifyLocalFiles().ConfigureAwait(false);
            if (verificationResults.FilesToUpdate.Count == 0)
            {
                return;
            }

            // TODO: Send update started message

            await UpdateLocalFiles(verificationResults.FilesToUpdate).ConfigureAwait(false);

            // TODO: Send update complete message
        }



        /// <summary>
        ///     Asynchronously verifies the integrity of local files against the remote manifest and identifies files that need updating.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the verification results.</returns>
        private Task<VerificationResults> VerifyLocalFiles()
        {
            List<(Uri, RemoteManifestFileEntry)> filesToUpdate = [];
            long totalSize = 0;
            long alreadyDownloadedSize = 0;

            // Dictionary to store local file data for efficient lookup
            Dictionary<string, LocalManifestFileEntry> localFileData = [];
            foreach (LocalManifestFileEntry entry in localManifest.Files)
            {
                localFileData[entry.Path] = entry;
            }

            foreach (var remoteEntry in remoteManifest.Files)
            {
                string outputPath = Path.Combine(RootFolder, remoteEntry.SourcePath);
                bool needsUpdate()
                {
                    if (!File.Exists(outputPath))
                    {
                        return true;
                    }

                    if (localFileData.TryGetValue(remoteEntry.SourcePath, out var localEntry))
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
                    alreadyDownloadedSize += remoteEntry.SourceSize;
                    continue;
                }

                filesToUpdate.Add((new Uri(new Uri(RemoteUrl), remoteEntry.Path), remoteEntry));
            }

            return Task.FromResult(new VerificationResults
            {
                FilesToUpdate = filesToUpdate,
                TotalSize = totalSize,
                AlreadyDownloadedSize = alreadyDownloadedSize
            });
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
        ///     Downloads the updater file using Bita tool asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns true if the download is successful, otherwise false.</returns>
        private async Task<bool> DownloadUpdaterWithBitaAsync()
        {
            string archiveUrl = Path.Combine(RemoteUrl, RemoteManifest.Updater.Path);
            string outputPath = Path.Combine(RootFolder, RemoteManifest.Updater.SourcePath);

            return await DownloadFileWithBitaAsync(archiveUrl, outputPath).ConfigureAwait(false);
        }



        /// <summary>
        ///     Downloads a remote file using the Bita tool asynchronously.
        /// </summary>
        /// <param name="file">The file entry from the remote manifest to be downloaded.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the download is successful, otherwise false.</returns>
        private static async Task<bool> DownloadFileWithBitaAsync(RemoteManifestFileEntry file)
        {
            string archiveUrl = Path.Combine(RemoteUrl, file.Path);
            string outputPath = Path.Combine(RootFolder, file.SourcePath);

            return await DownloadFileWithBitaAsync(archiveUrl, outputPath).ConfigureAwait(false);
        }



        /// <summary>
        ///     Downloads a remote file using the Bita tool asynchronously.
        /// </summary>
        /// <param name="archiveUrl">The URL of the remote file to be downloaded.</param>
        /// <param name="outputPath">The local path where the file will be saved.</param>
        /// <returns>A task representing the asynchronous operation. Returns true if the download is successful, otherwise false.</returns>
        private static async Task<bool> DownloadFileWithBitaAsync(string archiveUrl, string outputPath)
        {
            try
            {
                string bitaExecutablePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName, "Tools", "Bita", "bita.exe");
                if (!File.Exists(bitaExecutablePath))
                    throw new FileNotFoundException("Bita executable not found", bitaExecutablePath);

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
                await process.WaitForExitAsync().ConfigureAwait(false);

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
                await File.WriteAllTextAsync(localManifestPath, json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save local manifest to {localManifestPath}: {ex.Message}");
                throw;
            }
        }



        /// <summary>
        ///     Converts a remote manifest file entry to a local manifest file entry.
        /// </summary>
        /// <param name="remoteFile">The remote manifest file entry to be converted.</param>
        /// <returns>The corresponding local manifest file entry.</returns>
        private static LocalManifestFileEntry ConvertRemoteFileEntryToLocal(RemoteManifestFileEntry remoteFile)
        {
            LocalManifestFileEntry localFileEntry = new()
            {
                Path = remoteFile.SourcePath,
                Hash = remoteFile.SourceHash,
                Size = remoteFile.SourceSize
            };
            return localFileEntry;
        }
    }
}