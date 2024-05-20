using ROSE_Online_Login_Manager.Resources.Util;
using ROSE_Online_Login_Manager.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;



namespace ROSE_Online_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        private readonly List<ProfileCard> profileCards;



        public Home()
        {
            InitializeComponent();
            Loaded += Home_Loaded;

            profileCards = [];
        }



        /// <summary>
        ///     Event handler for the Loaded event of the Home window.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new HomeViewModel(new DialogService());

            UpdateProfileCards();
            SubscribeToPropertyChanged();
        }



        /// <summary>
        ///     Subscribes to the PropertyChanged event of the HomeViewModel.
        /// </summary>
        private void SubscribeToPropertyChanged()
        {
            if (DataContext is HomeViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }



        /// <summary>
        ///     Event handler for the PropertyChanged event of the HomeViewModel.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update profile cards when the 'Profiles' property changes.
            if (e.PropertyName == nameof(HomeViewModel.Profiles))
            {
                UpdateProfileCards();
            }
        }



        /// <summary>
        ///     Updates the profile cards displayed in the UI.
        /// </summary>
        private void UpdateProfileCards()
        {
            // Clears existing profile cards
            ProfileStackPanel.Children.Clear();
            profileCards.Clear();

            // Create new ones based on the data context
            if (DataContext is HomeViewModel viewModel)
            {
                foreach (var profile in viewModel.Profiles)
                {
                    ProfileCard profileCard = new()
                    {
                        DataContext = profile
                    };

                    // Subscribe to launch events for each profile card.
                    profileCard.ProfileLaunchEvent += ProfileLaunchEventHandler;

                    profileCards.Add(profileCard);
                    ProfileStackPanel.Children.Add(profileCard);
                }
            }
        }



        /// <summary>
        ///     Event handler for the ProfileLaunchEvent of a ProfileCard.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event data.</param>
        private void ProfileLaunchEventHandler(object? sender, EventArgs e)
        {
            // Extract the email from the sender or any relevant property of the sender
            if (sender is ProfileCard profileCard)
            {
                string email = profileCard.ProfileEmailTextBlock.Text;

                // Launch the profile corresponding to the email
                if (DataContext is HomeViewModel viewModel)
                {
                    viewModel.LaunchProfile(email);
                }
            }
        }
    }
}
