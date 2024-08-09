using Microsoft.Web.WebView2.Core;
using NLog;
using ROSE_Login_Manager.Model;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;



namespace ROSE_Login_Manager.View
{
    /// <summary>
    ///     Interaction logic for WebView2Control.xaml
    /// </summary>
    public partial class WebView2Control : UserControl
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            try
            {
                var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GlobalVariables.APP_NAME);
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await webView.EnsureCoreWebView2Async(env);

                if (!string.IsNullOrEmpty(SourceUrl))
                {
                    webView.CoreWebView2.Navigate(SourceUrl);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize WebView2 control.");
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
                    try
                    {
                        // Ensure CoreWebView2 is initialized
                        if (webViewControl.webView.CoreWebView2 == null)
                        {
                            await webViewControl.webView.EnsureCoreWebView2Async();
                        }

                        // Navigate to the new URL
                        webViewControl.webView.CoreWebView2?.Navigate(newUrl);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.Error(ex, "Failed to navigate to the new URL due to invalid operation.");
                    }
                    catch (COMException ex)
                    {
                        Logger.Error(ex, "Failed to navigate to the new URL due to COM exception.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "An unexpected error occurred while navigating to the new URL.");
                    }
                }
            }
        }
    }
}
