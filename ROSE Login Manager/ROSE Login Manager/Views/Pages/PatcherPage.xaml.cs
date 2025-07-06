using ROSE_Login_Manager.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ROSE_Login_Manager.Views.Pages
{
    public partial class PatcherPage : INavigableView<PatcherViewModel>
    {
        public PatcherViewModel ViewModel { get; }

        public PatcherPage(PatcherViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
