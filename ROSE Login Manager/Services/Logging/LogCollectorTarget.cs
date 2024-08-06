using NLog;
using NLog.Targets;



namespace ROSE_Login_Manager.Services.Logging
{
    /// <summary>
    ///     A custom NLog target for collecting log entries in memory.
    /// </summary>
    [Target("LogCollector")]
    public class LogCollectorTarget : TargetWithLayout
    {
        private readonly List<LogEntry> _logEntries = [];

        public event EventHandler<LogEntry> LogCollected;



        /// <summary>
        ///     Processes and stores a log event, then raises the <see cref="LogCollected"/> event.
        /// </summary>
        /// <param name="logEvent">The log event to process.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logEntry = new LogEntry
            {
                Timestamp = logEvent.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Level = logEvent.Level.Name.ToUpper(),
                Logger = logEvent.LoggerName,
                Message = logEvent.FormattedMessage
            };

            _logEntries.Add(logEntry);
            OnLogCollected(logEntry);
        }



        /// <summary>
        ///     Raises the <see cref="LogCollected"/> event.
        /// </summary>
        /// <param name="logEntry">The log entry that was collected.</param>
        protected virtual void OnLogCollected(LogEntry logEntry)
        {
            LogCollected?.Invoke(this, logEntry);
        }



        /// <summary>
        ///     Retrieves the collected log entries.
        /// </summary>
        /// <returns>An enumerable collection of log entries.</returns>
        public IEnumerable<LogEntry> GetLogEntries()
        {
            return _logEntries;
        }
    }
}
