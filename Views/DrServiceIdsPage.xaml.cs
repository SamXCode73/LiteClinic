using LiteClinic.Models;
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
using System.Collections.Immutable;
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
    public sealed partial class DrServiceIdsPage : Page
    {
        public DrServiceIdsPageViewModel ViewModel { get; set; }

        public DrServiceIdsPage()
        {
            InitializeComponent();
            ViewModel = new DrServiceIdsPageViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();                
            };
        }

        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //    var lang = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
        //    Debug.WriteLine($"DrServiceIdsPage loaded with language: {lang}");
        //    var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        //    var localizedTitle = loader.GetString("DrSp_PageTitle.Text");
        //    Debug.WriteLine($"Localized title: {localizedTitle}");
        //}
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is DrServiceIdsPageViewModel vm)
            {
                vm.ClearDrServiceIdsMemory();
            }
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is DrServiceIdsPageViewModel vm && vm.SelectedDoctorServiceIdsDisplay != null)
            {
                // Populate fields from the selected doctor
                vm.DoctorIdText = vm.SelectedDoctorServiceIdsDisplay.DoctorAutoId.ToString();
                vm.DoctorCodeText = vm.SelectedDoctorServiceIdsDisplay.DoctorCodeText;
                //vm.DoctorServiceBase.ServiceName = vm.SelectedDoctorServiceIdsDisplay.ServiceName;

                string serviceName = vm.SelectedDoctorServiceIdsDisplay.ServiceName.ToString(); // "Telegram"

                // Convert back to enum
                if (Enum.TryParse<ProviderType>(serviceName, out var parsedEnum))
                {
                    // parsedEnum == ProviderType.Telegram
                    int enumValue = (int)parsedEnum; // 0

                    // Assign back to your ViewModel
                    //vm.PatientServiceBase.ServiceName = parsedEnum;
                    vm.SelectedDoctorService = parsedEnum.ToString();
                }

                vm.DoctorServiceBase.ServiceId = vm.SelectedDoctorServiceIdsDisplay.ServiceId;
                vm.DoctorServiceBase.IsActive = vm.SelectedDoctorServiceIdsDisplay.IsActive;
                vm.DoctorServiceBase.NotifyEn = vm.SelectedDoctorServiceIdsDisplay.NotifyEn;
                vm.DoctorServiceBase.NotifyFr = vm.SelectedDoctorServiceIdsDisplay.NotifyFr;
                vm.DoctorServiceBase.NotifyAr = vm.SelectedDoctorServiceIdsDisplay.NotifyAr;
            }
        }

        private void DrServiceIdsPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_ApplydoctorCommand!.CanExecute(null))
                        ViewModel.Btn_ApplydoctorCommand.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearDoctorCommand!.CanExecute(null))
                        ViewModel.Btn_ClearDoctorCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }


    }
}
