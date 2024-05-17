using System.Windows;
using System.Windows.Controls;



namespace ROSE_Online_Login_Manager.Resources.Util
{
    /// <summary>
    ///     Represents a custom RadioButton control for navigation menus.
    /// </summary>
    internal class NavMenuButton : RadioButton
    {
        /// <summary>
        ///     Initializes static members of the NavMenuButton class.
        /// </summary>
        static NavMenuButton()
        {
            // Overrides the default style key to use the NavMenuButton style defined in XAML.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavMenuButton), new FrameworkPropertyMetadata(typeof(NavMenuButton)));
        }
    }
}
