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
        ///     Parses a log file into a list of <see cref="LogEntry"/> objects.
        /// </summary>
        /// <param name="filePath">The path to the log file to parse.</param>
        /// <returns>A list of parsed log entries.</returns>
        public static List<LogEntry> ParseLogFile(string filePath)
        {
            var logEntries = new List<LogEntry>();

            if (File.Exists(filePath))
            {
                try
                {
                    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader reader = new(fileStream);
                    
                    int id = 1;
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = SplitLogLine(line);

                        if (parts.Length >= 3)
                        {
                            var logEntry = new LogEntry
                            {
                                Id = id++,
                                Timestamp = ParseDateTime(parts[0] + ' ' + parts[1]),
                                Level = parts[2],
                                Logger = parts[3],
                                Message = parts[4]
                            };
                            logEntries.Add(logEntry);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Logger.Error($"IOException: {ex.Message}");
                }
                catch (FormatException ex)
                {
                    Logger.Error($"FormatException: {ex.Message}");
                }
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
        private static string ParseDateTime(string dateTimeString)
        {
            DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture);
            return parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
