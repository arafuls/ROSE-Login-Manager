using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using ROSE_Login_Manager.Model;
using ROSE_Login_Manager.Services;
using ROSE_Login_Manager.Services.Infrastructure;
using ROSE_Login_Manager.Services.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     ViewModel for managing and displaying log entries in the application's event log.
    /// </summary>
    public class EventLogViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _logDirectory;
        private readonly string _logFile;



        #region Accessors

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
        ///     Gets or sets a value indicating whether automatic scrolling to the bottom is enabled.
        ///     When set to <c>true</c>, the DataGrid will automatically scroll to the bottom when new log entries are added.
        ///     When set to <c>false</c>, the DataGrid will not automatically scroll, allowing the user to manually scroll through the log entries.
        /// </summary>
        private bool _isAutoScrollEnabled;
        public bool IsAutoScrollEnabled
        {
            get { return _isAutoScrollEnabled; }
            set { _isAutoScrollEnabled = value; OnPropertyChanged(); }
        }

        #endregion



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
            LogCollectorTarget logCollectorTarget = LogManager.Configuration.FindTargetByName<LogCollectorTarget>("logCollector");
            if (logCollectorTarget != null)
            {
                logCollectorTarget.LogCollected += OnLogCollected;
            }

            // Initialize ICommand
            OpenLogFolderCommand = new RelayCommand(OpenLogFolder);
            ClearLogsCommand = new RelayCommand(ClearLogs);

            // Initialize Settings
            IsAutoScrollEnabled = true; // Default value for auto-scroll
        }



        #region ICommands

        public ICommand OpenLogFolderCommand { get; set; }
        public ICommand ClearLogsCommand { get; set; }



        /// <summary>
        ///     Opens the log directory in the system's file explorer.
        /// </summary>
        /// <param name="obj">Unused parameter.</param>
        private void OpenLogFolder(object obj)
        {
            if (Directory.Exists(_logDirectory))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _logDirectory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                Logger.Warn("Unable to locate or open the log folder.");
            }
        }



        /// <summary>
        ///     Clears all log entries from the <see cref="LogEntries"/> collection.
        ///     This method removes all items from the collection, which will update any UI elements bound to it.
        /// </summary>
        /// <param name="obj">An unused parameter. This method does not utilize this parameter.</param>
        private void ClearLogs(object obj)
        {
            LogEntries.Clear();
        }

        #endregion



        #region Methods

        /// <summary>
        ///     Loads log entries from the latest log file into the <see cref="LogEntries"/> collection.
        /// </summary>
        private void LoadLogEntries()
        {
            List<LogEntry> logEntries = LogParser.ParseLogFile(_logFile);
            LogEntries = new ObservableCollection<LogEntry>(logEntries);
            WeakReferenceMessenger.Default.Send(new EventLogAddedMessage());    // Notify the view to scroll
        }



        /// <summary>
        ///     Retrieves the path of the latest log file in the specified directory.
        /// </summary>
        /// <param name="logDirectory">The directory to search for log files.</param>
        /// <returns>The path to the latest log file, or null if no log files are found.</returns>
        private static string GetLatestLogFile(string logDirectory)
        {
            DirectoryInfo directoryInfo = new(logDirectory);
            FileInfo? latestFile = directoryInfo.GetFiles("*.log")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            return latestFile?.FullName;
        }

        #endregion



        #region Event Handlers

        /// <summary>
        ///     Handles the event when a new log entry is collected by the log collector target.
        ///     This method is invoked on the UI thread and adds the new log entry to the <see cref="LogEntries"/> collection.
        /// </summary>
        /// <param name="sender">The source of the event, which is the log collector target.</param>
        /// <param name="logEntry">The log entry that was collected and needs to be added to the collection.</param>
        private void OnLogCollected(object sender, LogEntry logEntry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(logEntry);
                WeakReferenceMessenger.Default.Send(new EventLogAddedMessage());    // Notify the view to scroll
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

        #endregion
    }
}
