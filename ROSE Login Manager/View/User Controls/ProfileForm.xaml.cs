using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.ViewModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;



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

            // Set focus to the ProfileNameTextBox when the UserControl is loaded
            Loaded += (s, e) => ProfileNameTextBox.Focus();

            WeakReferenceMessenger.Default.Register<ResetPasswordFieldMessage>(this, ResetPasswordField);
        }



        /// <summary>
        ///     Event handler for the Unloaded event of the UserControl.
        ///     This method ensures that sensitive data in the password fields is cleared when the UserControl is unloaded.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ProfilePasswordTextBox.Password = string.Empty;
            ProfilePasswordTextBoxVisible.Text = string.Empty;
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
        ///     Resets the password field by clearing the text in the associated PasswordBox.
        /// </summary>
        /// <param name="recipient">The recipient object.</param>
        /// <param name="message">The message containing the reset password field indicator.</param>
        private void ResetPasswordField(object recipient, ResetPasswordFieldMessage message)
        {
            if (ProfilePasswordTextBox != null && ProfilePasswordTextBox.IsLoaded)
            {
                ProfilePasswordTextBox.Clear();
            }
        }



        /// <summary>
        ///     Handles the PasswordChanged event of the profile password field. Updates the corresponding view model's
        ///     profile password property and determines the button state.
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
        ///     Handles the PreviewKeyDown event of the profile password field.
        ///     Cancels the input event if the entered character is not a UTF-8 character.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ProfilePasswordTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the entered character is a UTF-8 character
            if ((e.Key < Key.Space || e.Key > Key.OemClear) && e.Key != Key.Back)
            {
                e.Handled = true;   // Cancel the input event
                return;
            }
        }



        /// <summary>
        ///     Validates whether the given string is in a valid email format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns><c>true</c> if the email address is in a valid format; otherwise, <c>false</c>.</returns>
        private static bool IsValidEmail(string email)
        {
            // Check if the email address is null, empty, or contains only white space
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use regular expression to validate email format
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                Regex regex = new(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\
                                    x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[
                                    a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]
                                    |[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1
                                    -9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[
                                    \x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
                return regex.IsMatch(email);
            }
            catch (RegexMatchTimeoutException)
            {
                // In case of regex match timeout, return false
                return false;
            }
        }



        /// <summary>
        ///     Determines the state of the add profile button based on the validity of profile data.
        /// </summary>
        public void DetermineButtonState()
        {
            bool nameIsValid = !string.IsNullOrEmpty(ProfileNameTextBox.Text);
            bool emailIsValid = IsValidEmail(ProfileEmailTextBox.Text);
            bool passwordIsValid = ProfilePasswordTextBox.Password != null && ProfilePasswordTextBox.Password.Length >= 8;

            AllowProfileButton = nameIsValid && emailIsValid && passwordIsValid;
        }

        private void ShowPasswordToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            // Update the image source
            ShowPasswordImage.Source = new BitmapImage(new Uri("pack://application:,,,/ROSE Login Manager;component/Resources/Images/eye-outline.png"));

            // Make the password visible
            ProfilePasswordTextBoxVisible.Visibility = Visibility.Visible;
            ProfilePasswordTextBox.Visibility = Visibility.Collapsed;
            ProfilePasswordTextBoxVisible.Text = ProfilePasswordTextBox.Password;
        }

        private void ShowPasswordToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            // Update the image source
            ShowPasswordImage.Source = new BitmapImage(new Uri("pack://application:,,,/ROSE Login Manager;component/Resources/Images/eye-off-outline.png"));

            // Mask the password
            ProfilePasswordTextBoxVisible.Visibility = Visibility.Collapsed;
            ProfilePasswordTextBox.Visibility = Visibility.Visible;
            ProfilePasswordTextBox.Password = ProfilePasswordTextBoxVisible.Text;
        }

    }
}
