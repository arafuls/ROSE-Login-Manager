using NLog;
using System.Diagnostics;
using System.Globalization;
using System.IO;



namespace ROSE_Login_Manager.Model
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public String Timestamp { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
    }



    public class LogFileService
    {
        public static string GetLatestLogFile(string logDirectory)
        {
            var directoryInfo = new DirectoryInfo(logDirectory);
            var latestFile = directoryInfo.GetFiles("*.log")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            return latestFile?.FullName;
        }
    }



    public class LogParser
    {
        public static List<LogEntry> ParseLogFile(string filePath)
        {
            var logEntries = new List<LogEntry>();

            if (File.Exists(filePath))
            {
                try
                {
                    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader reader = new(fileStream);
                    
                    string line;
                    int id = 1;
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
                                Message = string.Join(" ", parts.Skip(3))
                            };
                            logEntries.Add(logEntry);
                        }
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"IOException: {ex.Message}");
                }
                catch (FormatException ex)
                {
                    Debug.WriteLine($"FormatException: {ex.Message}");
                }
            }
            return logEntries;
        }



        private static string[] SplitLogLine(string line)
        {
            // Split based on the assumption that the first part is timestamp, second is level
            // and the rest is the message. Adjust this based on your exact log format.
            string[] parts = line.Split(' ', 5, StringSplitOptions.None);
            return parts;
        }



        private static String ParseDateTime(string dateTimeString)
        {
            string incomingFormat = "yyyy-MM-dd HH:mm:ss.ffff";
            DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, incomingFormat, CultureInfo.InvariantCulture);
            string outgoingFormat = parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return outgoingFormat;
        }
    }

}
