using System.Windows;
using System.Windows.Controls;



namespace ROSE_Online_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for ProfileCard.xaml
    /// </summary>
    public partial class ProfileCard : UserControl
    {
        public event EventHandler ProfileLaunchEvent;



        /// <summary>
        ///     Default Constructor
        /// </summary>
        public ProfileCard()
        {
            InitializeComponent();
        }



        /// <summary>
        ///     Handles the click event of the profile launch button.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        private void ProfileLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && ProfileLaunchEvent != null)
            {
                ProfileLaunchEvent(this, e);
            }
        }
    }
}
