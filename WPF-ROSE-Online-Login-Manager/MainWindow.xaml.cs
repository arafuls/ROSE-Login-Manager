using ROSE_Online_Login_Manager.Resources.Util;
using System.Windows;

namespace ROSE_Online_Login_Manager
{
    public static class GlobalVariables
    {
        public static string? RoseDir { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}