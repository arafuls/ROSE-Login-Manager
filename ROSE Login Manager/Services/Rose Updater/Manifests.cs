using Newtonsoft.Json;



namespace ROSE_Login_Manager.Services.Rose_Updater
{
    /// <summary>
    ///     Represents the structure of a remote manifest.
    /// </summary>
    public class RemoteManifest
    {
        public int Version { get; set; }
        public RemoteManifestFileEntry Updater { get; set; }
        public List<RemoteManifestFileEntry> Files { get; set; }
    }



    /// <summary>
    ///     Represents a file entry in the remote manifest.
    /// </summary>
    public class RemoteManifestFileEntry
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("source_path")]
        public string SourcePath { get; set; }

        [JsonProperty("source_hash")]
        public byte[] SourceHash { get; set; }

        [JsonProperty("source_size")]
        public long SourceSize { get; set; }
    }



    /// <summary>
    ///     Represents the structure of a local manifest.
    /// </summary>
    public class LocalManifest
    {
        public int Version { get; set; }
        public LocalManifestFileEntry Updater { get; set; }
        public List<LocalManifestFileEntry> Files { get; set; }

        // Constructor to initialize default values
        public LocalManifest()
        {
            Version = 1;
            Updater = new LocalManifestFileEntry();
            Files = [];
        }
    }



    /// <summary>
    ///     Represents a file entry in the local manifest.
    /// </summary>
    public class LocalManifestFileEntry
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("hash")]
        public byte[] Hash { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        // Constructor to initialize default values
        public LocalManifestFileEntry()
        {
            Path = string.Empty;
            Hash = [];
            Size = 0;
        }
    }



    /// <summary>
    ///     Represents the verification results after comparing local and remote files.
    /// </summary>
    public class VerificationResults
    {
        public List<(Uri, RemoteManifestFileEntry)> FilesToUpdate { get; set; }
        public long TotalSize { get; set; }
        public long AlreadyDownloadedSize { get; set; }
    }



    /// <summary>
    ///     Custom JSON converter for serializing/deserializing LocalManifestFileEntry objects.
    /// </summary>
    public class LocalManifestFileEntryConverter : JsonConverter<LocalManifestFileEntry>
    {
        public override void WriteJson(JsonWriter writer, LocalManifestFileEntry? value, JsonSerializer serializer)
        {
            if (value == null) { return; }

            writer.WriteStartObject();

            // Write other properties except the 'Hash' property
            writer.WritePropertyName("path");
            writer.WriteValue(value.Path);

            writer.WritePropertyName("size");
            writer.WriteValue(value.Size);

            // Write 'Hash' property using the ByteArrayConverter
            writer.WritePropertyName("hash");
            writer.WriteStartArray();
            foreach (byte b in value.Hash)
            {
                writer.WriteValue(b);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        public override LocalManifestFileEntry ReadJson(JsonReader reader, Type objectType, LocalManifestFileEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
