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



        public ProfileCard()
        {
            InitializeComponent();
        }



        private void ProfileLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && ProfileLaunchEvent != null)
            {
                ProfileLaunchEvent(this, e);
            }
        }
    }
}
