using System.Windows;



namespace ROSE_Login_Manager.Services
{
    /// <summary>
    ///     Interface for dialog services to show message boxes and confirmation dialogs.
    /// </summary>
    internal interface IDialogService
    {
        void ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon);
        bool ShowConfirmationDialog(string message, string title);
    }



    /// <summary>
    ///     Implementation of the IDialogService interface for showing message boxes and confirmation dialogs.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        ///     Shows a message box with specified message, title, buttons, and icon.
        /// </summary>
        /// <param name="message">The message to display in the message box.</param>
        /// <param name="title">The title of the message box.</param>
        /// <param name="button">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        public void ShowMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox.Show(message, title, button, icon);
        }



        /// <summary>
        ///     Shows a confirmation dialog with specified message and title.
        /// </summary>
        /// <param name="message">The message to display in the confirmation dialog.</param>
        /// <param name="title">The title of the confirmation dialog.</param>
        /// <returns>True if the user clicks "Yes", false otherwise.</returns>
        public bool ShowConfirmationDialog(string message, string title)
        {
            MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }

}
