using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserRoleManagerPage : Page
    {
        public UserRoleViewModel ViewModel { get; set; } = new();


        public UserRoleManagerPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                try
                {
                    await ViewModel.InitializeUserRoleAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "SettingsPage initialization failed");
                }
            };

        }

        private void Button_Click_BackToUserPage(object? sender, RoutedEventArgs? e)
        {
            NavigateBackToUserPage();
        }

        private static void NavigateBackToUserPage()
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(UsersPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is UserRoleViewModel vm)
            {
                vm.ClearMemoryRoles();
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is UserRoleViewModel vm && vm.SelectedRole != null)
            {
                vm.LoadRoleForEditing(vm.SelectedRole);
            }
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.SaveRoleCommand!.CanExecute(null))
                        ViewModel.SaveRoleCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.UpdateRoleCommand!.CanExecute(null))
                        ViewModel.UpdateRoleCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivateRoleCommadn!.CanExecute(null))
                        ViewModel.Btn_DeactivateRoleCommadn.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Button_ClearRoleData!.CanExecute(null))
                        ViewModel.Button_ClearRoleData.Execute(null);
                    break;
                case VirtualKey.F5:
                    NavigateBackToUserPage(); // Reuse your existing method
                    break;
            }

            args.Handled = true;
        }

    }
}