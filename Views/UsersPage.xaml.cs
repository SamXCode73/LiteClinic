using LiteClinic.Services;
using LiteClinic.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.System;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LiteClinic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UsersPage : Page
    {
        public UsersViewModel ViewModel { get; set; }


        public UsersPage()
        {
            InitializeComponent();
            ViewModel = new UsersViewModel();
            this.DataContext = ViewModel; // or assign your existing ViewModel

            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };

            ViewModel!.ClearPasswordBoxesAction = () =>
            {
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
            };
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //     // This will run after the page is shown
        //}

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is UsersViewModel vm)
            {
                vm.ClearMemoryUser();
            }
        }

        private void Button_Click_OpenUserRolePage(object sender, RoutedEventArgs e)
        {
            var frame = MainPage.GetContentFrame();
            frame?.Navigate(typeof(UserRoleManagerPage));
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }

        private void PasswordBoxConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && vm.SelectedUser != null)
            {
                // Populate fields from SelectedUser
                vm.Username = vm.SelectedUser.Username;
                vm.FullName = vm.SelectedUser.FullName;
                vm.Email = vm.SelectedUser.Email;
                vm.PhoneNumber = vm.SelectedUser.PhoneNumber;
                vm.LandLineNumber = vm.SelectedUser.LandLineNumber;
                vm.Language = vm.SelectedUser.Language;
                vm.RoleId = vm.SelectedUser.RoleId;
                vm.RoleName = vm.SelectedUser.RoleName;
                vm.IsActive = vm.SelectedUser.IsActive;

                // Add any other fields you want to populate
            }
        }

        private void UserPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_SaveUserCommand!.CanExecute(null))
                        ViewModel.Btn_SaveUserCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.Btn_UpdateUserCommand!.CanExecute(null))
                        ViewModel.Btn_UpdateUserCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivateRoleCommadn!.CanExecute(null))
                        ViewModel.Btn_DeactivateRoleCommadn.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearCommand!.CanExecute(null))
                        ViewModel.Btn_ClearCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }

    }
}
    

