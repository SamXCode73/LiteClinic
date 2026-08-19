using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using LiteClinic.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace LiteClinic
{
    public partial class App : Application
    {
        private Window? _window;
        public Window? MainWindow => _window;
        internal static Window? MainAppWindow { get; set; }
        //internal static string? SelectedLanguage { get; set; }
        public static AppState GlobalState { get; } = new AppState();


        public App()
        {
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JAaF5cXmNCd1p/TH5YfUNzdUVEY1ZUTXxaS1ZhSXxVdkJiXH9ddXVQQWRdU0d9XEY=");
            var savedLang = ApplicationData.Current.LocalSettings.Values["SelectedLanguage"] as string;
            if (!string.IsNullOrEmpty(savedLang))
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = savedLang;
                Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
                
            }
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Clear all saved values in LocalSettings
            //ApplicationData.Current.LocalSettings.Values.Clear();

            // --- 2. Initialize Window & Splash Screen ---
            _window = new MainWindow();
            var rootFrame = new Frame();
            rootFrame.Content = new SplashScreen();
            _window.Content = rootFrame;
            MainAppWindow = _window;
            _window.Activate();


            // --- 3. Single Instance Wake-Up Logic ---
            // Listen for when a second instance tries to start and redirect focus here
            Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += (sender, e) =>
            {
                _window.DispatcherQueue.TryEnqueue(() =>
                {
                    if (_window != null)
                    {
                        _window.Activate(); // Bring existing LiteClinic to front
                    }
                });
            };

        }

    }

}
