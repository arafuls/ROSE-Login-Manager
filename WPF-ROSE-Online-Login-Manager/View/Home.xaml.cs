using ROSE_Online_Login_Manager.Model;
using ROSE_Online_Login_Manager.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace ROSE_Online_Login_Manager.View
{
    /// <summary>
    /// Interaction logic for Home.xaml
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
        /// Handles the Loaded event of the Home view.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new HomeViewModel();

            UpdateProfileCards();
            SubscribeToPropertyChanged();
        }



        private void SubscribeToPropertyChanged()
        {
            if (DataContext is HomeViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }



        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HomeViewModel.Profiles))
            {
                UpdateProfileCards();
            }
        }



        private void UpdateProfileCards()
        {
            ProfileStackPanel.Children.Clear();
            profileCards.Clear();

            if (DataContext is HomeViewModel viewModel)
            {
                foreach (var profile in viewModel.Profiles)
                {
                    ProfileCard profileCard = new()
                    {
                        DataContext = profile
                    };
                    profileCard.ProfileLaunchEvent += ProfileLaunchEventHandler;
                    profileCards.Add(profileCard);
                    ProfileStackPanel.Children.Add(profileCard);
                }
            }
        }



        private void ProfileLaunchEventHandler(object? sender, EventArgs e)
        {
            // Extract the email from the sender or any relevant property of the sender
            if (sender is ProfileCard profileCard)
            {
                string email = profileCard.ProfileEmailTextBlock.Text;

                if (DataContext is HomeViewModel viewModel)
                {
                    viewModel.LaunchProfile(email);
                }
            }
        }
    }
}
