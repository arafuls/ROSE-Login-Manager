using Microsoft.Web.WebView2.Core;
using System.Windows;
using System.Windows.Controls;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for WebView2Control.xaml
    /// </summary>
    public partial class WebView2Control : UserControl
    {
        public static readonly DependencyProperty SourceUrlProperty = DependencyProperty.Register(
            nameof(SourceUrl), typeof(string), typeof(WebView2Control), new PropertyMetadata(default(string), OnSourceUrlChanged));



        /// <summary>
        ///     Gets or sets the URL of the content to display in the WebView2Control.
        /// </summary>
        public string SourceUrl
        {
            get => (string)GetValue(SourceUrlProperty);
            set => SetValue(SourceUrlProperty, value);
        }



        /// <summary>
        ///     Initializes a new instance of the WebView2Control class.
        /// </summary>
        public WebView2Control()
        {
            InitializeComponent();
            InitializeWebView();
        }



        /// <summary>
        ///     Initializes the WebView2 control asynchronously.
        /// </summary>
        private async void InitializeWebView()
        {
            var userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ROSE Login Manager";
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);

            if (!string.IsNullOrEmpty(SourceUrl))
            {
                webView.CoreWebView2.Navigate(SourceUrl);
            }
        }



        /// <summary>
        ///     Handles the change of the SourceUrl property by navigating the WebView2 control to the new URL.
        /// </summary>
        /// <param name="d">The dependency object whose property changed.</param>
        /// <param name="e">The event data.</param>
        private static async void OnSourceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebView2Control webViewControl && e.NewValue is string newUrl)
            {
                if (webViewControl.webView != null)
                {
                    if (webViewControl.webView.CoreWebView2 == null)
                    {
                        await webViewControl.webView.EnsureCoreWebView2Async();
                    }

                    if (webViewControl.webView != null && e.NewValue != null)
                    {
                        webViewControl.webView.CoreWebView2?.Navigate(newUrl);
                    }
                }
            }
        }
    }
}
