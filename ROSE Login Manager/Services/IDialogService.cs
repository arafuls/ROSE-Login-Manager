using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ROSE_Login_Manager.Services
{
    internal interface IDialogService
    {
        void ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon);
    }

    public class DialogService : IDialogService
    {
        public void ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, button, icon);
        }
    }

}
