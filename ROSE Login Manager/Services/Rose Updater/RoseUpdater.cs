using Newtonsoft.Json;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Rose_Updater;
using System.Diagnostics;
using System.IO;
using System.Net.Http;



namespace ROSE_Login_Manager.Services
{
    public class RoseUpdater
    {
        private const string RemoteUrl = "https://updates.roseonlinegame.com";
        private const string RemoteManifestUrl = RemoteUrl + "/manifest.json";
        private const string LocalManifestFileName = "local_manifest.json";
        private const string ManifestFileName = "manifest.json";
        private const string GameExeFileName = "trose.exe";

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
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            Run();
        }



        public async void Run()
        {
            if (await CompareManifests())
            {   // Updater is up-to-date
                return;
            }

            await ParseManifestFilesAsync();
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
            // TODO: Send message to disable launch buttons until complete

            // List to store the tasks for updating files
            var updateTasks = new List<Task>();

            foreach (RemoteManifestFileEntry fileEntry in RemoteManifest.Files)
            {
                string localfile = Path.Combine(RootFolder, fileEntry.SourcePath);
                if (File.Exists(localfile) && CompareHash(fileEntry.SourcePath, fileEntry.SourceHash))
                {   // Up to date entry
                    continue;
                }

                // Start the update task and add it to the list
                Task updateTask = UpdateFileAsync(fileEntry);
                updateTasks.Add(updateTask);
            }

            // Wait for all update tasks to complete
            await Task.WhenAll(updateTasks);

            // TODO: Save local manifest

            // TODO: Send message to enable launch buttons until complete
        }


        private static async Task UpdateFileAsync(RemoteManifestFileEntry fileEntry)
        {
            string? directoryPath = Path.GetDirectoryName(fileEntry.SourcePath);
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Creating possibly deleted directory {directoryPath}");
                Directory.CreateDirectory(directoryPath);
            }

            if (await DownloadFileWithBitaAsync(fileEntry))
            {
                Console.WriteLine($"Successfully updated {fileEntry.Path}");
            }
            else
            {
                Console.WriteLine("Failed to update file!");
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
                LocalManifest = LoadLocalManifest();
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
        private LocalManifest LoadLocalManifest()
        {
            Uri remoteUri = new(RemoteUrl);
            string localManifestPath = Path.Combine(RootFolder, "updater", remoteUri.Host, LocalManifestFileName);
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
    }
}