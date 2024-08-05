using CommunityToolkit.Mvvm.ComponentModel;
using ROSE_Login_Manager.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;



namespace ROSE_Login_Manager.ViewModel
{
    public class EventLogViewModel : ObservableObject
    {
        private readonly FileSystemWatcher _fileWatcher;
        private readonly string _logDirectory;
        private string _logFile;



        private ObservableCollection<LogEntry> _logEntries;
        public ObservableCollection<LogEntry> LogEntries
        {
            get { return _logEntries; }
            set { _logEntries = value; OnPropertyChanged(); }
        }



        public EventLogViewModel()
        {
            _logDirectory = Path.Combine(GlobalVariables.Instance.AppPath, "logs");
            _logFile = GetLatestLogFile(_logDirectory);
            LoadLogEntries();

            _fileWatcher = new FileSystemWatcher(_logDirectory, "*.log")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnLogFileChanged;
            _fileWatcher.Created += OnLogFileChanged;
            _fileWatcher.Renamed += OnLogFileChanged;
        }



        private void OnLogFileChanged(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _logFile = GetLatestLogFile(_logDirectory);
                LoadLogEntries();
            });
        }



        private void LoadLogEntries()
        {
            if (_logFile != null)
            {
                List<LogEntry> logs = LogParser.ParseLogFile(_logFile);
                LogEntries = new ObservableCollection<LogEntry>(logs);
            }
        }



        private static string GetLatestLogFile(string logDirectory)
        {
            var directoryInfo = new DirectoryInfo(logDirectory);
            var latestFile = directoryInfo.GetFiles("*.log")
                                          .OrderByDescending(f => f.LastWriteTime)
                                          .FirstOrDefault();
            return latestFile?.FullName;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}