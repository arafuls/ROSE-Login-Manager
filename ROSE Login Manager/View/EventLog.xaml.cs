using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Services.Infrastructure;
using ROSE_Login_Manager.ViewModel;
using System.Windows;
using System.Windows.Controls;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for the EventLog user control.
    ///     Displays log entries and supports automatic scrolling to the bottom when new entries are added.
    /// </summary>
    public partial class EventLog : UserControl
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventLog"/> class.
        ///     Sets up message handling for auto-scrolling when new log entries are added.
        /// </summary>
        public EventLog()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;

            WeakReferenceMessenger.Default.Register<EventLogAddedMessage>(this, OnEventLogAdded);
        }



        /// <summary>
        ///     Handles the Loaded event of the UserControl to scroll to the bottom when the view is first displayed.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ScrollToBottom();
        }



        /// <summary>
        ///     Handles the EventLogAddedMessage to trigger scrolling to the bottom of the DataGrid.
        /// </summary>
        /// <param name="sender">The source of the message, usually the view model.</param>
        /// <param name="message">The message containing the event details.</param>
        private void OnEventLogAdded(object sender, EventLogAddedMessage message)
        {
            ScrollToBottom();
        }



        /// <summary>
        ///     Scrolls the DataGrid to the bottom if auto-scrolling is enabled.
        /// </summary>
        private void ScrollToBottom()
        {
            if (DataContext is EventLogViewModel viewModel && viewModel.IsAutoScrollEnabled)
            {
                if (EventLogDataGrid.Items.Count > 0)
                {
                    EventLogDataGrid.ScrollIntoView(EventLogDataGrid.Items[EventLogDataGrid.Items.Count - 1]);
                }
            }
        }
    }
}