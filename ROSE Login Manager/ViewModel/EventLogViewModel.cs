using CommunityToolkit.Mvvm.ComponentModel;
using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel for managing and displaying log entries in the application's event log.
    /// </summary>
    public class EventLogViewModel : ObservableObject
    {
        private readonly string _logDirectory;
        private readonly string _logFile;



        /// <summary>
        ///     Gets or sets the collection of log entries.
        /// </summary>
        private ObservableCollection<LogEntry> _logEntries;
        public ObservableCollection<LogEntry> LogEntries
        {
            get { return _logEntries; }
            set { _logEntries = value; OnPropertyChanged(); }
        }



        /// <summary>
        ///     Initializes a new instance of the <see cref="EventLogViewModel"/> class.
        ///     Sets up the log directory, loads the latest log file, and subscribes to the log collector target.
        /// </summary>
        public EventLogViewModel()
        {
            _logDirectory = Path.Combine(GlobalVariables.Instance.AppPath, "logs");
            _logFile = GetLatestLogFile(_logDirectory);

            if (_logFile != null)
            {
                LoadLogEntries();
            }

            // Hook up the LogCollector target to handle logs
            var logCollectorTarget = LogManager.Configuration.FindTargetByName<LogCollectorTarget>("logCollector");
            if (logCollectorTarget != null)
            {
                logCollectorTarget.LogCollected += OnLogCollected;
            }
        }



        /// <summary>
        ///     Loads log entries from the latest log file into the <see cref="LogEntries"/> collection.
        /// </summary>
        private void LoadLogEntries()
        {
            var logEntries = LogParser.ParseLogFile(_logFile);
            LogEntries = new ObservableCollection<LogEntry>(logEntries);
        }



        /// <summary>
        ///     Retrieves the path of the latest log file in the specified directory.
        /// </summary>
        /// <param name="logDirectory">The directory to search for log files.</param>
        /// <returns>The path to the latest log file, or null if no log files are found.</returns>
        private static string GetLatestLogFile(string logDirectory)
        {
            var directoryInfo = new DirectoryInfo(logDirectory);
            var latestFile = directoryInfo.GetFiles("*.log")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            return latestFile?.FullName;
        }



        /// <summary>
        ///     Event handler for when a new log entry is collected by the <see cref="LogCollectorTarget"/>.
        ///     Adds the new log entry to the <see cref="LogEntries"/> collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="logEntry">The log entry that was collected.</param>
        private void OnLogCollected(object sender, LogEntry logEntry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(logEntry);
            });
        }



        /// <summary>
        ///     Event for notifying when a property value changes.
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
