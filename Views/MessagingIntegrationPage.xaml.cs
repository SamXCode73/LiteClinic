using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.ViewModels;
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
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessagingIntegrationPage : Page
    {
        public MessagingIntegrationPageViewModel ViewModel { get; }

        public MessagingIntegrationPage()
        {
            InitializeComponent();

            // Create the ViewModel and set it as DataContext
            ViewModel = new MessagingIntegrationPageViewModel();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is MessagingIntegrationPageViewModel vm)
            {
                vm.ClearMemory();
            }
        }
    }
}
