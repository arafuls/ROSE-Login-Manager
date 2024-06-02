using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ROSE_Login_Manager.Services.Infrastructure;
using System.Windows.Input;



namespace ROSE_Login_Manager.ViewModel
{
    /// <summary>
    ///     Message class used to convey information about launching a profile.
    /// </summary>
    public class LaunchProfileMessage(string profileEmail)
    {
        public string ProfileEmail { get; } = profileEmail;
    }



    /// <summary>
    ///     View model for a profile card.
    /// </summary>
    internal class ProfileCardViewModel : ObservableObject
    {
        #region Accessors
        private string _cosmeticEmail;
        public string CosmeticEmail
        {
            get => _cosmeticEmail;
            set { SetProperty(ref _cosmeticEmail, value); }
        }


        /// <summary>
        ///     Gets or sets the name of the profile.
        /// </summary>
        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set { SetProperty(ref _profileName, value); }
        }



        /// <summary>
        ///     Gets or sets the email of the profile.
        /// </summary>
        private string _profileEmail;
        public string ProfileEmail
        {
            get => _profileEmail;
            set { SetProperty(ref _profileEmail, value); }
        }



        /// <summary>
        ///     Gets or sets the display email setting of the profile.
        /// </summary>
        private bool _displayEmail;
        public bool DisplayEmail
        {
            get => _displayEmail;
            set
            {
                if (SetProperty(ref _displayEmail, value))
                {
                    CosmeticEmail = value ? ProfileEmail : "";
                }
            }
        }



        /// <summary>
        ///     Gets or sets the mask email setting of the profile.
        /// </summary>
        private bool _maskEmail;
        public bool MaskEmail
        {
            get => _maskEmail;
            set
            {
                if (SetProperty(ref _maskEmail, value))
                {
                    CosmeticEmail = value ? Mask(ProfileEmail) : ProfileEmail;
                }
            }
        }


        private bool _launchButtonEnabled;
        public bool LaunchButtonEnabled
        {
            get => _launchButtonEnabled;
            set => SetProperty(ref _launchButtonEnabled, value);
        }
        #endregion



        public ICommand LaunchProfileCommand { get; }



        /// <summary>
        ///     Initializes a new instance of the <see cref="ProfileCardViewModel"/> class.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <param name="email">The email of the profile.</param>
        /// <param name="display">Optional. Indicates whether the email should be displayed. Default is true.</param>
        /// <param name="mask">Optional. Indicates whether the email should be masked. Default is false.</param>
        public ProfileCardViewModel(string name, string email, bool display = true, bool mask = false)
        {
            // Required Data
            ProfileName = name;
            ProfileEmail = email;

            // Setting Data
            CosmeticEmail = string.Empty;
            DisplayEmail = display;
            MaskEmail = mask;

            LaunchProfileCommand = new RelayCommand(LaunchProfile);

            WeakReferenceMessenger.Default.Register<DisplayEmailCheckedMessage>(this, SettingsViewModel_DisplayEmailCheckedChanged);
            WeakReferenceMessenger.Default.Register<MaskEmailCheckedMessage>(this, SettingsViewModel_MaskEmailCheckedChanged);
            WeakReferenceMessenger.Default.Register<ProgressMessage>(this, OnProgressMessageReceived);
        }



        #region Message Handlers
        private void SettingsViewModel_DisplayEmailCheckedChanged(object recipient, DisplayEmailCheckedMessage message)
        {
            DisplayEmail = message.IsChecked;
        }



        private void SettingsViewModel_MaskEmailCheckedChanged(object recipient, MaskEmailCheckedMessage message)
        {
            MaskEmail = message.IsChecked;
        }



        private void OnProgressMessageReceived(object recipient, ProgressMessage message)
        {
            LaunchButtonEnabled = (message.ProgressPercentage == 100);
        }
        #endregion



        /// <summary>
        ///     Sends a message to launch the profile.
        /// </summary>
        public void LaunchProfile()
        {
            WeakReferenceMessenger.Default.Send(new LaunchProfileMessage(ProfileEmail));
        }



        /// <summary>
        ///     Masks the provided email address by replacing characters in the local part with asterisks (*) while keeping the domain part intact.
        /// </summary>
        /// <param name="profileEmail">The email address to be masked.</param>
        /// <returns>The masked email address.</returns>
        private static string Mask(string profileEmail)
        {
            if (string.IsNullOrEmpty(profileEmail))
                return string.Empty;

            // Split the email into local part and domain part
            string[] parts = profileEmail.Split('@');
            string localPart = parts[0];
            string domainPart = parts[1];

            // Mask the local part
            string maskedLocalPart = localPart.Length switch
            {
                0 => string.Empty,
                1 => localPart[..1] + new string('*', localPart.Length - 1),
                2 => string.Concat(localPart.AsSpan(0, 1), new string('*', 1), localPart.AsSpan(2)),
                _ => string.Concat(localPart.AsSpan(0, 1), new string('*', localPart.Length - 2), localPart.AsSpan(localPart.Length - 1))
            };

            // Concatenate the masked local part and the domain part
            return $"{maskedLocalPart}@{domainPart}";
        }
    }
}
