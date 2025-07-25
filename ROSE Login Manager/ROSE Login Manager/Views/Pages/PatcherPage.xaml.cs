using ROSE_Login_Manager.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Documents;

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
            
            // Subscribe to status line updates
            ViewModel.StatusLineAdded += OnStatusLineAdded;
        }

        private void OnStatusLineAdded(string line)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(line));
            StatusLogTextBox.Document.Blocks.Add(paragraph);
            
            // Auto-scroll to the bottom
            StatusLogTextBox.Document.Blocks.LastBlock?.BringIntoView();
        }

        public void Dispose()
        {
            ViewModel.StatusLineAdded -= OnStatusLineAdded;
        }
    }
}
