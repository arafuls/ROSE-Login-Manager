using System.Diagnostics;
using System.Windows.Controls;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for AboutMe.xaml
    /// </summary>
    public partial class AboutMe : UserControl
    {
        public AboutMe()
        {
            InitializeComponent();
        }

        private void Git_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/arafuls/ROSE-Login-Manager/",
                UseShellExecute = true
            });
        }

        private void ROSE_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://forum.roseonlinegame.com/topic/6068-tool-open-source-login-manager/",
                UseShellExecute = true
            });
        }

        private void BMC_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://buymeacoffee.com/rose.login.manager",
                UseShellExecute = true
            });
        }
    }
}
