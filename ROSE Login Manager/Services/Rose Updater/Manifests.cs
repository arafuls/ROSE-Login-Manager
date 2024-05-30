﻿using Newtonsoft.Json;



namespace ROSE_Login_Manager.Services.Rose_Updater
{
    public class RemoteManifest
    {
        public int Version { get; set; }
        public RemoteManifestFileEntry Updater { get; set; }
        public List<RemoteManifestFileEntry> Files { get; set; }
    }

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




    public class LocalManifest
    {
        public int Version { get; set; }
        public LocalManifestFileEntry Updater { get; set; }
        public List<LocalManifestFileEntry> Files { get; set; }
    }

    public class LocalManifestFileEntry
    {
        public string Path { get; set; }
        public byte[] Hash { get; set; }
        public long Size { get; set; }
    }
}