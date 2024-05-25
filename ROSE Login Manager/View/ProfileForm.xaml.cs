using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for ProfileForm.xaml
    /// </summary>
    public partial class ProfileForm : UserControl
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the profile button is allowed.
        /// </summary>
        public static readonly DependencyProperty AllowProfileButtonProperty =
            DependencyProperty.Register(
                nameof(AllowProfileButton),
                typeof(bool),
                typeof(ProfileForm),
                new PropertyMetadata(false));

        public bool AllowProfileButton
        {
            get => (bool)GetValue(AllowProfileButtonProperty);
            set => SetValue(AllowProfileButtonProperty, value);
        }



        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfileForm"/> class.
        /// </summary>
        public ProfileForm()
        {
            InitializeComponent();
            AttachEventHandlers();
            RegisterMessenger();
        }



        /// <summary>
        ///     Attaches event handlers for text changed events of profile name, email, and password fields.
        /// </summary>
        private void AttachEventHandlers()
        {
            ProfileNameTextBox.TextChanged += (s, e) => DetermineButtonState();
            ProfileEmailTextBox.TextChanged += (s, e) => DetermineButtonState();
            ProfilePasswordTextBox.PreviewKeyDown += ProfilePasswordTextBox_PreviewKeyDown;
        }



        /// <summary>
        ///     Registers a messenger to listen for messages to reset the password field.
        /// </summary>
        private void RegisterMessenger()
        {
            WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
            {
                if (m == "ResetPasswordField")
                {
                    ProfilePasswordTextBox.Clear();
                }
            });
        }



        /// <summary>
        ///     Handles the PasswordChanged event of the profile password field. Updates the corresponding view model's profile password property and determines the button state.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ProfilePasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddProfileViewModel addProfileViewModel)
            {
                addProfileViewModel.ProfilePassword = ((PasswordBox)sender).SecurePassword;
            }
            else if (DataContext is EditProfileViewModel editProfileViewModel)
            {
                editProfileViewModel.ProfilePassword = ((PasswordBox)sender).SecurePassword;
            }

            DetermineButtonState();
        }



        /// <summary>
        ///  Handles the PreviewKeyDown event of the profile password field. Cancels the input event if the entered character is whitespace or non-ASCII, and cancels the paste operation if the Ctrl + V key combination is pressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ProfilePasswordTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the key combination for paste (Ctrl + V) is pressed
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                e.Handled = true;   // Cancel the paste operation
            }

            // Check if the entered character is whitespace or non-ASCII
            if (e.Key == Key.Space || !IsAscii(e.Key))
            {
                e.Handled = true;   // Cancel the input event
                return;
            }
        }



        /// <summary>
        ///     Determines whether the specified key represents an ASCII character.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if the key represents an ASCII character; otherwise, <c>false</c>.</returns>
        private static bool IsAscii(Key key)
        {
            return (key >= Key.A && key <= Key.Z) || (key >= Key.D0 && key <= Key.D9) || key == Key.Back ||
                   key == Key.Oem3 || key == Key.OemMinus || key == Key.OemPlus ||
                   key == Key.OemOpenBrackets || key == Key.OemCloseBrackets || key == Key.OemPipe ||
                   key == Key.OemQuotes || key == Key.OemComma || key == Key.OemPeriod || key == Key.OemQuestion ||
                   key == Key.OemTilde || key == Key.Oem8 || key == Key.OemSemicolon;
        }



        /// <summary>
        ///     Determines the state of the add profile button based on the validity of profile data.
        /// </summary>
        public void DetermineButtonState()
        {
            bool nameValid = !string.IsNullOrEmpty(ProfileNameTextBox.Text);
            bool emailValid = !string.IsNullOrEmpty(ProfileEmailTextBox.Text);
            bool passwordValid = ProfilePasswordTextBox.Password != null && ProfilePasswordTextBox.Password.Length >= 8;

            AllowProfileButton = nameValid && emailValid && passwordValid;
        }
    }
}
