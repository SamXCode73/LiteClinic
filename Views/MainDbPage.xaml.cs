using LiteClinic.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainDbPage : Page
    {
        public MainDbPage()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                try
                {
                     _= MDbPNavView.SelectedItem = MDbPNavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(i => i.Tag?.ToString() == "Dashboard");

                    MDbPFrame.Navigate(typeof(DashboardPage));

                }
                catch (TaskCanceledException) { return; }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "MainDbPage initialization failed");
                }
                // Default navigation: select Theme and navigate
            };

        }


        private void MDbPNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as NavigationViewItem;

            if (selectedItem == null)
                return;

            switch (selectedItem.Tag?.ToString())
            {
                case "Dashboard":
                    MDbPFrame.Navigate(typeof(DashboardPage)); // this page is wroking
                    MDbPNavView.IsBackEnabled = MDbPFrame.CanGoBack;
                    App.GlobalState.UpdateSubtitle("Dashboard/Text");
                    break;
                case "DoctorPage":
                    MDbPFrame.Navigate(typeof(DoctorDirectoryPage)); // this page is working
                    MDbPNavView.IsBackEnabled = MDbPFrame.CanGoBack;
                    App.GlobalState.UpdateSubtitle("MainDashboardDocotrDirectory/Text");
                    break;
            }
        }

        private void MDbPNavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (MDbPFrame.CanGoBack)
            {
                MDbPFrame.GoBack();
                MDbPNavView.IsBackEnabled = MDbPFrame.CanGoBack;
            }
        }

    }
}
