using CommunityToolkit.Mvvm.Messaging;
using ROSE_Online_Login_Manager.ViewModel;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace ROSE_Online_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for AddProfile.xaml
    /// </summary>
    public partial class AddProfile : Window
    {
        private bool passwordBoxLoaded = false;



        public AddProfile()
        {
            InitializeComponent();

            // Set window properties
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Height = 300;
            this.Width = 250;

            // Register to receive password reset messages
            WeakReferenceMessenger.Default.Register(this, new MessageHandler<object, string>(HandleMessage));

            // Subscribe to the Loaded event of the PasswordBox
            ProfilePasswordTextBox.Loaded += ProfilePasswordTextBox_Loaded;
        }



        /// <summary>
        ///     Handles the password reset message.
        /// </summary>
        /// <param name="sender">The message sender.</param>
        /// <param name="message">The message content.</param>
        private void HandleMessage(object sender, string message)
        {
            if (message == "ResetPasswordField" && passwordBoxLoaded)
            {
                ProfilePasswordTextBox.Clear();
            }
        }



        private void ProfilePasswordTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the flag to indicate that the PasswordBox is loaded
            passwordBoxLoaded = true;
        }



        /// <summary>
        ///     Handles the password changed event for the profile password text box.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The routed event arguments.</param>
        private void ProfilePasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Update the ProfilePassword property in the view model
                ((AddProfileViewModel)DataContext).ProfilePassword = passwordBox.SecurePassword.Copy();
            }
        }



        /// <summary>
        ///     Handles the preview key down event for the profile password text box.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The key event arguments.</param>
        private void ProfilePasswordTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the entered character is whitespace or non-ASCII
            if (e.Key == Key.Space || !IsAscii(e.Key))
            {
                e.Handled = true;   // Cancel the input event
                return;
            }

            // Check if the key combination for paste (Ctrl + V) is pressed
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true;   // Cancel the paste operation
            }
        }



        /// <summary>
        ///     Checks if the key represents an ASCII character.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key represents an ASCII character, otherwise false.</returns>
        private static bool IsAscii(Key key)
        {
            return (key >= Key.A && key <= Key.Z) || (key >= Key.D0 && key <= Key.D9) || key == Key.Back ||
                   key == Key.Oem3 || key == Key.OemMinus || key == Key.OemPlus ||
                   key == Key.OemOpenBrackets || key == Key.OemCloseBrackets || key == Key.OemPipe ||
                   key == Key.OemQuotes || key == Key.OemComma || key == Key.OemPeriod || key == Key.OemQuestion ||
                   key == Key.OemTilde || key == Key.Oem8 || key == Key.OemSemicolon;
        }
    }
}
