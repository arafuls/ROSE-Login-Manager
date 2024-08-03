using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using ROSE_Login_Manager.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;



namespace ROSE_Login_Manager.ViewModel
{
    internal class EventLogViewModel : ObservableObject
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



        private ObservableCollection<string> _logEntries;
        public ObservableCollection<string> LogEntries
        {
            get { return _logEntries; }
            set { _logEntries = value; OnPropertyChanged(); }
        }



        public EventLogViewModel()
        {
            var logFileService = new LogFileService();
            var logParser = new LogParser();

            string latestLogFile = logFileService.GetLatestLogFile("your_logs_directory_path_here");
            if (latestLogFile != null)
            {
                var logs = logParser.ParseLogFile(latestLogFile);
                LogEntries = new ObservableCollection<string>(logs);
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
