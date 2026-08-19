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
    public sealed partial class ServiceIdsPage : Page
    {
        public ServiceIdsPageViewModel ViewModel { get; set; }

        public ServiceIdsPage()
        {
            InitializeComponent();
            ViewModel = new ServiceIdsPageViewModel();
            this.DataContext = ViewModel;
            Loaded += async (s, e) =>
            {
                await ViewModel!.InitializeAsync();
            };
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (DataContext is ServiceIdsPageViewModel vm)
            {
                vm.ClearPatientServicesMemory();
            }
        }

        private void PatientListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (DataContext is ServiceIdsPageViewModel vm && vm.SelectedPatientServiceIdsDisplay != null)
            {
                // Populate fields from the selected patient

                vm.PatientAutoId = vm.SelectedPatientServiceIdsDisplay.PatientAutoId.ToString();
                vm.PatientIdText = vm.SelectedPatientServiceIdsDisplay.PatientId;

                // Suppose you got "Telegram" from the DataGrid
                string serviceName = vm.SelectedPatientServiceIdsDisplay.ServiceName.ToString(); // "Telegram"

                // Convert back to enum
                if (Enum.TryParse<ProviderType>(serviceName, out var parsedEnum))
                {
                    // parsedEnum == ProviderType.Telegram
                    int enumValue = (int)parsedEnum; // 0

                    // Assign back to your ViewModel
                    //vm.PatientServiceBase.ServiceName = parsedEnum;
                    vm.SelectedPatientService = parsedEnum.ToString();
                }

                //vm.SelectedPatientService = vm.SelectedPatientServiceIdsDisplay.ServiceName.ToString();
                vm.PatientServiceBase.ServiceId = vm.SelectedPatientServiceIdsDisplay.ServiceId;
                vm.PatientServiceBase.IsActive = vm.SelectedPatientServiceIdsDisplay.IsActive;
                vm.PatientServiceBase.NotifyEn = vm.SelectedPatientServiceIdsDisplay.NotifyEn;
                vm.PatientServiceBase.NotifyFr = vm.SelectedPatientServiceIdsDisplay.NotifyFr;
                vm.PatientServiceBase.NotifyAr = vm.SelectedPatientServiceIdsDisplay.NotifyAr;

            }

        }

        private void ServiceIdsPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Key)
            {
                case VirtualKey.F1:
                    if (ViewModel.Btn_ApplyPatientCommand!.CanExecute(null))
                        ViewModel.Btn_ApplyPatientCommand.Execute(null);
                    break;
                case VirtualKey.F4:
                    if (ViewModel.Btn_ClearPatientCommand!.CanExecute(null))
                        ViewModel.Btn_ClearPatientCommand.Execute(null);
                    break;
            }

            args.Handled = true;
        }

    }
}
