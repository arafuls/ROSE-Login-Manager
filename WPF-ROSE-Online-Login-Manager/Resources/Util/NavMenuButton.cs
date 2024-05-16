using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace ROSE_Online_Login_Manager.Resources.Util
{
    internal class NavMenuButton : RadioButton
    {
        static NavMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavMenuButton), new FrameworkPropertyMetadata(typeof(NavMenuButton)));
        }
    }
}
