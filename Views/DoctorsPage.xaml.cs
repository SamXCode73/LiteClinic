using LiteClinic.Services;
using LiteClinic.ViewModels;
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
    public sealed partial class DoctorsPage : Page
    {
        public DoctorsViewModel ViewModel { get; set; }

        public DoctorsPage()
        {
            InitializeComponent();
            ViewModel = new DoctorsViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is DoctorsViewModel vm)
            {
                vm.ClearDoctorsMemory();
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is DoctorsViewModel vm && vm.SelectedDisplayDoctor != null)
            {
                // Populate fields from SelectedDoctor

                vm.FullName = vm.SelectedDisplayDoctor.FullName;     
                vm.Specialization = vm.SelectedDisplayDoctor.Specialization;
                vm.PhoneNumber = vm.SelectedDisplayDoctor.PhoneNumber;
                vm.LandLineNumber = vm.SelectedDisplayDoctor.LandLineNumber;
                vm.IsActive = vm.SelectedDisplayDoctor.IsActive;
                vm.Gender = vm.SelectedDisplayDoctor.Gender;
                vm.DoctorId = vm.SelectedDisplayDoctor.DoctorId;
                vm.ProfilePicturePath = vm.SelectedDisplayDoctor.ProfilePicturePath;
                vm.DoctorCode = vm.SelectedDisplayDoctor.DoctorCode;
            }
        }

        private void UserPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_SaveDcotorCommand!.CanExecute(null))
                        ViewModel.Btn_SaveDcotorCommand.Execute(null);
                    break;
                case VirtualKey.F2:
                    if (ViewModel.Btn_UpdateDcotorCommand!.CanExecute(null))
                        ViewModel.Btn_UpdateDcotorCommand.Execute(null);
                    break;
                case VirtualKey.F3:
                    if (ViewModel.Btn_DeactivateDoctorCommadn!.CanExecute(null))
                        ViewModel.Btn_DeactivateDoctorCommadn.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearCommand!.CanExecute(null))
                        ViewModel.Btn_ClearCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }

        //private void Button_Click_OpenSchedledpage(object sender, RoutedEventArgs e)
        //{
        //    var frame = MainPage.GetContentFrame();
        //    frame?.Navigate(typeof(ScheduledDoctorPage));
        //}

        //private void Button_ClickGotoServicePage(object sender, RoutedEventArgs e)
        //{
        //    var frame = MainPage.GetContentFrame();
        //    frame?.Navigate(typeof(DrServiceIdsPage));

        //}


    }
}
