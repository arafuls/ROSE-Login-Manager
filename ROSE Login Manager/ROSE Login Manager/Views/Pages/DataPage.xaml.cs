using ROSE_Login_Manager.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ROSE_Login_Manager.Views.Pages
{
    public partial class DataPage : INavigableView<DataViewModel>
    {
        public DataViewModel ViewModel { get; }

        public DataPage(DataViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
