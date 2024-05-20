using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ROSE_Online_Login_Manager.View
{
    /// <summary>
    /// Interaction logic for WebView2Control.xaml
    /// </summary>
    public partial class WebView2Control : UserControl
    {
        public static readonly DependencyProperty SourceUrlProperty = DependencyProperty.Register(
            nameof(SourceUrl), typeof(string), typeof(WebView2Control), new PropertyMetadata(default(string), OnSourceUrlChanged));

        public string SourceUrl
        {
            get => (string)GetValue(SourceUrlProperty);
            set => SetValue(SourceUrlProperty, value);
        }

        public WebView2Control()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async();
        }

        private static async void OnSourceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2Control webViewControl && e.NewValue is string newUrl)
            {
                if (webViewControl.webView != null)
                {
                    await webViewControl.webView.EnsureCoreWebView2Async();
                    webViewControl.webView.CoreWebView2.Navigate(newUrl);
                }
            }
        }
    }
}
