using System.Windows.Controls;
using System.Windows.Input;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        /// <summary>
        ///     Default Constructor
        /// </summary>
        public Home()
        {
            InitializeComponent();
        }
        private bool isDragging = false;

        private void ProfileScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
            }
        }

        private void ProfileScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Allow mouse events to be handled normally during dragging
                e.Handled = false;
            }
        }

        private void ProfileScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                isDragging = false;
            }
        }
    }
}
