using ROSE_Login_Manager.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ROSE_Login_Manager.Views.Pages
{
    public partial class ProfilePage : INavigableView<ProfileViewModel>
    {
        public ProfileViewModel ViewModel { get; }

        public ProfilePage(ProfileViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
