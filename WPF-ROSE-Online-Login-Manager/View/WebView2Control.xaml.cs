﻿using ROSE_Online_Login_Manager.Services;
using System.Windows;
using System.Windows.Controls;



namespace ROSE_Online_Login_Manager.View
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
            try
            {
                await webView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                new DialogService().ShowMessageBox(
                    title: "ROSE Online Login Manager - WebView2Control::InitializeWebView",
                    message: "WebView initialization failed: " + ex.Message,
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
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
                        await webViewControl.webView.EnsureCoreWebView2Async();
                        webViewControl.webView.CoreWebView2.Navigate(newUrl);
                    }
                    catch (Exception ex)
                    {
                        new DialogService().ShowMessageBox(
                            title: "ROSE Online Login Manager - WebView2Control::OnSourceUrlChanged",
                            message: "WebView navigation failed: " + ex.Message,
                            button: MessageBoxButton.OK,
                            icon: MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
