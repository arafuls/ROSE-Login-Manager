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
            await SaveLocalManifest(newManifest);
        }



        /// <summary>
        ///     Verifies the integrity of game files and performs updates if necessary.
        /// </summary>
        /// <remarks>
        ///     This method checks the integrity of local game files against the remote manifest. If any files need to be updated,
        ///     it downloads the updated files and replaces the local copies.
        /// </remarks>
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
        ///     Asynchronously verifies the integrity of local files against the remote manifest.
        /// </summary>
        /// <remarks>
        ///     This method iterates through the list of files in the remote manifest and checks if they exist locally.
        ///     If a file is missing locally or its hash and size differ from the corresponding entry in the remote manifest,
        ///     it is considered outdated and added to the list of files to update. Otherwise, if the file exists locally
        ///     and matches the remote entry, it is skipped.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains the verification results.</returns>
        private async Task<VerificationResults> VerifyLocalFiles()
        {
            List<(Uri, RemoteManifestFileEntry)> filesToUpdate = [];
            long totalSize = 0;
            long alreadyDownloadedSize = 0;

            // Pre-calculate the full file paths for local files
            HashSet<string> localFullPaths = LocalManifest.Files.Select(file => Path.Combine(RootFolder, file.Path)).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (RemoteManifestFileEntry remoteEntry in remoteManifest.Files)
            {
                // Skip null entries
                if (remoteEntry == null)
                    continue;

                string localFilePath = Path.Combine(RootFolder, remoteEntry.SourcePath);
                totalSize += remoteEntry.SourceSize;

                bool fileIsOutdated = true;

                // Check if the file exists locally in the manifest and on disk
                if (localFullPaths.Contains(localFilePath) || File.Exists(localFilePath))
                {
                    if (File.Exists(localFilePath))
                    {
                        // Check if the local manifest contains an entry matching the remote entry
                        LocalManifestFileEntry? localEntry = LocalManifest.Files
                            .FirstOrDefault(entry => entry.Path == remoteEntry.SourcePath);

                        if (localEntry != null && localEntry.Hash.SequenceEqual(remoteEntry.SourceHash))
                        {
                            fileIsOutdated = false;
                        }
                        else
                        {
                            (long fileSize, byte[] fileHash) = await GetFileSizeAndHashAsync(localFilePath).ConfigureAwait(false);

                            // Check if the local file hash and size match the remote entry
                            if (fileHash.SequenceEqual(remoteEntry.SourceHash) && fileSize == remoteEntry.SourceSize)
                            {
                                fileIsOutdated = false;
                            }
                        }
                    }
                }

                // Update the lists based on the file status
                if (fileIsOutdated)
                {
                    filesToUpdate.Add((new Uri(new Uri(RemoteUrl), remoteEntry.Path), remoteEntry));
                }
                else
                {
                    alreadyDownloadedSize += remoteEntry.SourceSize;
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
        ///     Asynchronously calculates the size and hash of the specified file.
        /// </summary>
        /// <param name="localFilePath">The path to the local file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the file size and its hash.</returns>
        private static async Task<(long size, byte[] hash)> GetFileSizeAndHashAsync(string localFilePath)
        {
            // Get the size of the file
            FileInfo fileInfo = new(localFilePath);
            long fileSize = fileInfo.Length;

            // Get the hash of the file using BLAKE2b-512 used by Bitar
            byte[] fileHash;
            using (FileStream stream = File.OpenRead(localFilePath))
            {
                var blake2b = new Blake2bDigest(512);
                byte[] buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    blake2b.BlockUpdate(buffer, 0, bytesRead);
                }

                fileHash = new byte[blake2b.GetDigestSize()];
                blake2b.DoFinal(fileHash, 0);
            }

            return (fileSize, fileHash);
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
                await File.WriteAllTextAsync(localManifestPath, json);
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