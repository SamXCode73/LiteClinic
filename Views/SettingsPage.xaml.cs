using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.Data.Sqlite;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get; }


        public SettingsPage()
        {
            InitializeComponent();

            // Pass it into the ViewModel
            ViewModel = new SettingsPageViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                try
                {
                    await ViewModel.InitializeSettingsAsync();
                }
                catch (TaskCanceledException) { return; }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "SettingsPage initialization failed");
                }
                // Default navigation: select Theme and navigate
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(i => i.Tag?.ToString() == "Theme");

                SettingsFrame.Navigate(typeof(ThemePage));
            };

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is SettingsPageViewModel vm)
            {
                vm.ClearSettingsMemory();
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;

            if (selectedItem == null)
                return;

            switch (selectedItem.Tag?.ToString())
            {
                case "Theme":
                    SettingsFrame.Navigate(typeof(ThemePage)); // this page is wroking
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "Language":
                    SettingsFrame.Navigate(typeof(LanguagePage)); // this page is working
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "Calendar":
                    SettingsFrame.Navigate(typeof(CalendarPage)); // this page is wokring
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "WorkingDays":
                    SettingsFrame.Navigate(typeof(WorkingDaysPage)); // this page not working
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "Notifications":
                    SettingsFrame.Navigate(typeof(NotificationsPage));
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "ClinicName":
                    SettingsFrame.Navigate(typeof(ClinicNamePage));
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "Backup":
                    SettingsFrame.Navigate(typeof(BackupPage));
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
                case "Logs":
                    SettingsFrame.Navigate(typeof(LogsPage));
                    NavView.IsBackEnabled = SettingsFrame.CanGoBack;
                    break;
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (SettingsFrame.CanGoBack)
            {
                SettingsFrame.GoBack();
                NavView.IsBackEnabled = SettingsFrame.CanGoBack;
            }
        }

        private static async Task ShakeMenu(TranslateTransform transform)
        {
            double[] offsets = { -5, 5, -3, 3, 0 };
            foreach (var offset in offsets)
            {
                transform.X = offset;
                await Task.Delay(50);
            }
        }
    }
}


