using NLog;
using System.Globalization;
using System.IO;



namespace ROSE_Login_Manager.Services.Logging
{
    /// <summary>
    ///     Represents a single log entry with details such as timestamp, log level, logger name, and message.
    /// </summary>
    public class LogEntry
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public string Timestamp { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }



    /// <summary>
    ///     Provides methods to interact with log files, such as retrieving the latest log file.
    /// </summary>
    public class LogFileService
    {
        /// <summary>
        ///     Retrieves the path of the latest log file in the specified directory.
        /// </summary>
        /// <param name="logDirectory">The directory to search for log files.</param>
        /// <returns>The path to the latest log file, or null if no log files are found.</returns>
        public static string GetLatestLogFile(string logDirectory)
        {
            var directoryInfo = new DirectoryInfo(logDirectory);
            var latestFile = directoryInfo.GetFiles("*.log")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            return latestFile?.FullName;
        }
    }



    /// <summary>
    ///     Provides methods for parsing log files into a list of <see cref="LogEntry"/> objects.
    /// </summary>
    public class LogParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        /// <summary>
        ///     Parses the specified log file and extracts log entries into a list of <see cref="LogEntry"/> objects.
        /// </summary>
        /// <param name="filePath">The path to the log file to be parsed.</param>
        /// <returns>A list of <see cref="LogEntry"/> objects representing the parsed log entries. If the file does not exist or an error occurs, the list will be empty.</returns>
        public static List<LogEntry> ParseLogFile(string filePath)
        {
            var logEntries = new List<LogEntry>();

            if (!File.Exists(filePath))
            {
                Logger.Error($"File does not exist: {filePath}");
                return logEntries;
            }

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                int id = 1;
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    var parts = SplitLogLine(line);

                    if (parts.Length == 5)
                    {
                        string? timestamp = ParseDateTime($"{parts[0]} {parts[1]}");

                        if (!string.IsNullOrEmpty(timestamp))
                        {
                            logEntries.Add(new LogEntry
                            {
                                Id = id++,
                                Timestamp = timestamp,
                                Level = parts[2],
                                Logger = StripNamespace(parts[3]),
                                Message = parts[4]
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "An error occurred while reading the log file.");
            }

            return logEntries;
        }



        /// <summary>
        ///     Splits a log line into parts based on spaces.
        /// </summary>
        /// <param name="line">The log line to split.</param>
        /// <returns>An array of parts extracted from the log line.</returns>
        private static string[] SplitLogLine(string line)
        {
            string[] parts = line.Split(' ', 5, StringSplitOptions.None);
            return parts;
        }



        /// <summary>
        ///     Parses a date-time string into a standardized format.
        /// </summary>
        /// <param name="dateTimeString">The date-time string to parse.</param>
        /// <returns>A standardized date-time string in the format "yyyy-MM-dd HH:mm:ss".</returns>
        private static string? ParseDateTime(string dateTimeString)
        {
            if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                return parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        ///     Strips the "ROSE_Login_Manager." namespace from the logger name.
        /// </summary>
        /// <param name="loggerName">The logger name to process.</param>
        /// <returns>The logger name with the namespace stripped out.</returns>
        private static string StripNamespace(string loggerName)
        {
            if (loggerName != null)
            {
                return loggerName.Replace("ROSE_Login_Manager.", string.Empty);
            }
            return loggerName;
        }
    }
}
