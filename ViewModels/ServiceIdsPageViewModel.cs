using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.ApplicationModel.Resources;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Telegram.Bot.Types;

namespace LiteClinic.ViewModels
{
    public partial class ServiceIdsPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PatientServiceIds> PatientServices { get; private set; } = [];
        public ObservableCollection<PatientServiceIdsDisplay> PatientServicesDisplay { get; private set; } = [];
        public ObservableCollection<DoctorsModel> Doctors { get; private set; } = []; // For Updateing the Doctor List with ID and Code only
        public ObservableCollection<PatientsModel> Patients { get; private set; } = []; // For Updateing the Patient List with ID and Code only


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IAsyncRelayCommand? Btn_ApplyPatientCommand { get; }

        public IAsyncRelayCommand? Btn_ClearPatientCommand { get; }


        public bool CanAddChaId => PermissionHelper.CanManageRecords; // Add New Record
        public bool CanEditChatId => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateChatId => PermissionHelper.CanEditRecords; // Deactivate Record

        private readonly PatientsRepository _patientsRepository = new();
        private readonly ProviderRepository _providerRepository = new();

        private List<PatientServiceIdsDisplay> _patientServiceIdsDisplay = [];
        private List<PatientsModel> _patients = [];
        private List<DoctorServiceIdsDisplay> _doctorServiceIdsDisplay = [];
        private List<DoctorsModel> _doctors = [];
        private CancellationTokenSource _cts = new();

        private PatientServiceIdsDisplay? _selectedPatientServiceIdsDisplay;

        private readonly ResourceLoader _loader = new();
        public ServiceIdsPageViewModel()
        {
            Btn_ApplyPatientCommand = new AsyncRelayCommand(ApplyPatientCommand);

            Btn_ClearPatientCommand = new AsyncRelayCommand(async () =>
            {
                await ClearDataAsync();
            });
        }

        // Arrange List 
        private List<PatientServiceIds> _allPatientServices = [];
        private string? _statusMessage;
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);

        private string? _patientAutoId;
        public string? PatientAutoId
        {
            get => _patientAutoId;
            set
            {
                if (_patientAutoId != value)
                {
                    _patientAutoId = value;

                    if (int.TryParse(_patientAutoId, out var id))
                    {
                        var match = Patients.FirstOrDefault(p => p.PatientAutoId == id);

                        if (match != null)
                        {
                            if (match.IsActive)
                            {
                                PatientIdText = match.PatientId; // string from model
                            }
                            else
                            {
                                //StatusMessage = $"WARNING!! Wrong ID or Patient {match.PatientId} is deactivated.";
                                StatusColor = new SolidColorBrush(Colors.IndianRed);
                                //_ = ShowStatusMessage($"WARNING!! Wrong ID or Patient {match.PatientId} is deactivated.", 
                                //    5000);
                                _ = ShowStatusMessage(
                                    string.Format(
                                        _loader.GetString("SIDp_StatusMessageWarningWrongIdOrDeactivated"),
                                        match.PatientId
                                    )
                                );
                                return;
                            }
                        }
                        else                         {
                            //StatusMessage = $"WARNING! Patient ID not found.";
                            StatusColor = new SolidColorBrush(Colors.OrangeRed);
                            //_ = ShowStatusMessage($"WARNING! Patient ID not found.", 5000);
                            _ = ShowStatusMessage(
                                _loader.GetString("SIDp_StatusMessageWarningPatientIdNotFound"),
                                5000
                            );
                        }
                    }

                    OnPropertyChanged(nameof(PatientAutoId));
                }
            }
        }

        private string? _patientIdText;
        public string? PatientIdText
        {
            get => _patientIdText;
            set
            {
                if (_patientIdText != value)
                {
                    _patientIdText = value;

                    var match = Patients.FirstOrDefault(p =>
                        string.Equals(p.PatientId, _patientIdText, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        if (match.IsActive)
                        {
                            PatientAutoId = match.PatientAutoId.ToString();
                        }
                        else
                        {
                            //StatusMessage = $"WARNING! Wrong ID or Patient {_patientIdText} is deactivated.";
                            StatusColor = new SolidColorBrush(Colors.IndianRed);
                            //_ = ShowStatusMessage($"WARNING! Wrong ID or Patient {_patientIdText} is deactivated.", 
                            //    5000);
                            _ = ShowStatusMessage(
                                string.Format(
                                    _loader.GetString("SIDp_StatusMessageWarningWrongIdOrDeactivated"),
                                    match.PatientId
                                )
                            );
                            return;
                        }
                    }
                    else
                    {
                        //StatusMessage = $"WARNING! Patient ID not found.";
                        StatusColor = new SolidColorBrush(Colors.OrangeRed);
                        //_ = ShowStatusMessage($"WARNING! Patient ID not found.", 5000);
                        _ = ShowStatusMessage(
                            _loader.GetString("SIDp_StatusMessageWarningPatientIdNotFound"),
                            5000
                        );
                    }

                    OnPropertyChanged(nameof(PatientIdText));
                }
            }
        }

        // Patient Filter
        private string? _patientFilter;
        public string? PatientFilter
        {
            get => _patientFilter;
            set
            {
                if (_patientFilter != value)
                {
                    _patientFilter = value;
                    OnPropertyChanged(nameof(PatientFilter));
                    ApplyPatientFilter();
                }
            }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public string? SelectedPatientService
        {
            get => PatientServiceBase.ServiceName.ToString();
            set
            {
                if (PatientServiceBase.ServiceName.ToString() != value &&
                    Enum.TryParse<ProviderType>(value, out var parsed))
                {
                    PatientServiceBase.ServiceName = parsed;
                    OnPropertyChanged(nameof(SelectedPatientService));
                }
            }
        }

        public ServiceBase PatientServiceBase { get; set; } = new ServiceBase();

        public PatientServiceIdsDisplay? SelectedPatientServiceIdsDisplay
            {
            get => _selectedPatientServiceIdsDisplay!;
            set
            {
                if (_selectedPatientServiceIdsDisplay != value)
                {
                    _selectedPatientServiceIdsDisplay = value;
                    OnPropertyChanged(nameof(SelectedPatientServiceIdsDisplay));
                }
            }
        }


        private string? _serviceId;
        public string? ServiceId
        {
            get => _serviceId;
            set
            {
                if (_serviceId != value)
                {
                    _serviceId = value;
                    OnPropertyChanged(nameof(ServiceId));
                }
            }
        }

        private string? _hijriDate;
        public string? HijriDate
        {
            get => _hijriDate;
            set
            {
                _hijriDate = value;
                OnPropertyChanged(nameof(HijriDate));
            }
        }

        private string? _romanDate;
        public string? RomanDate
        {
            get => _romanDate;
            set
            {
                _romanDate = value;
                OnPropertyChanged(nameof(RomanDate));
            }
        }

        // -------------------------
        // Show/Hide Date
        // -------------------------
        private bool _showGregorianDate = App.GlobalState.ShowGregorianDate;
        public bool ShowGregorianDate
        {
            get => _showGregorianDate;
            set
            {
                if (_showGregorianDate != value)
                {
                    _showGregorianDate = value;
                    OnPropertyChanged(nameof(ShowGregorianDate));
                }
            }
        }

        private bool _showHijriDate = App.GlobalState.ShowHijriDate;
        public bool ShowHijriDate
        {
            get => _showHijriDate;
            set
            {
                if (_showHijriDate != value)
                {
                    _showHijriDate = value;
                    OnPropertyChanged(nameof(ShowHijriDate));

                }
            }
        }

        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

        public async Task LoadPatientsServiceAsync()
        {
            try
            {
                // If your repository is synchronous:
                var list = await Task.Run(() => _providerRepository.GetAllPatientProvider());

                PatientServices.Clear();
                foreach (var patient in list)
                    PatientServices.Add(patient);

                // Cache for local filtering (search box)
                _allPatientServices = list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading patients.");
            }
        }

        public async Task LoadPatientsCode()
        {
            try
            {
                // If your repository is synchronous:
                _patients = await Task.Run(() => _patientsRepository.GetActivePatientsForServiceCode());

                Patients.Clear();
                foreach (var patient in _patients)
                {
                    Patients.Add(patient);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading patients.");
            }
        }

        public async Task LoadPatientsServiceDisplayAsync()
        {
            try
            {
                // If your repository is synchronous:
                var list = await Task.Run(() => _providerRepository.GetAllPatientWithServicesDisplay());

                PatientServicesDisplay.Clear();
                foreach (var patient in list)
                    PatientServicesDisplay.Add(patient);

                // Cache for local filtering (search box)
                _patientServiceIdsDisplay = list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading patients.");
            }
        }


        public async Task InitializeAsync()
        {
            await LoadPatientsServiceAsync();
            await LoadPatientsCode();
            await LoadPatientsServiceDisplayAsync();
            await GetHiriRomanDate();
        }

        public async Task ApplyPatientCommand()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PatientAutoId) || string.IsNullOrWhiteSpace(PatientServiceBase.ServiceId))
                {
                    StatusColor = new SolidColorBrush(Colors.IndianRed);
                    StatusMessage = _loader.GetString("SIDp_StatusMessagePatientIdServiceRequired");
                    return;
                }

                int patientId = int.Parse(PatientAutoId);
                var serviceName = PatientServiceBase.ServiceName;
                var serviceId = PatientServiceBase.ServiceId;
                var activation = PatientServiceBase.IsActive;
                var notifyEn = PatientServiceBase.NotifyEn;
                var notifyAr = PatientServiceBase.NotifyAr;
                var notifyFr = PatientServiceBase.NotifyFr;

                // Find existing by PatientId + ServiceName
                var existing = PatientServicesDisplay
                    .FirstOrDefault(p => p.PatientAutoId == patientId && p.ServiceName == serviceName);

                if (existing == null)
                {
                    // Insert new
                    var newService = new PatientServiceIds
                    {
                        PatientAutoId = patientId,
                        PatientIdText = PatientIdText,
                        ServiceName = serviceName,
                        ServiceId = serviceId,
                        IsActive = activation,
                        NotifyEn = notifyEn,
                        NotifyAr = notifyAr,
                        NotifyFr = notifyFr,
                        AddedByUser = $"{Environment.UserName} - {App.GlobalState.LoggedUserName}",
                        AddedAt = DateTime.Now
                    };

                    if (_providerRepository.ApplyPatientService(newService))
                    {
                        
                        StatusColor = new SolidColorBrush(Colors.Green);
                        StatusMessage = _loader.GetString("SIDp_StatusMessageNewPatientServiceAdded");
                        await Task.Delay(3000, _cts.Token);
                        ClearPatienData();
                    }
                }
                else
                {
                    bool isDuplicate = 
                        string.Equals(existing.ServiceId, serviceId, StringComparison.OrdinalIgnoreCase) && 
                        existing.NotifyEn == notifyEn && 
                        existing.NotifyAr == notifyAr &&
                        existing.NotifyFr == notifyFr &&
                        existing.IsActive == activation;

                    if (isDuplicate)
                    {
                        // Exact duplicate
                        StatusColor = new SolidColorBrush(Colors.OrangeRed);
                        StatusMessage = string.Format(
                            _loader.GetString("SIDp_StatusMessageServiceAlreadyExists"),existing.ServiceName, existing.PatientAutoId, existing.PatientId);                        
                        await Task.Delay(5000);
                        ClearPatienData();
                    }
                    else
                    {
                        // Update ServiceId
                        var updateService = new PatientServiceIds
                        {
                            PatientServiceId = existing.PatientServiceId,
                            PatientAutoId = patientId,
                            PatientIdText = PatientIdText,
                            ServiceName = serviceName,
                            ServiceId = serviceId,
                            IsActive = activation,
                            NotifyEn = notifyEn,
                            NotifyAr = notifyAr,
                            NotifyFr = notifyFr,
                            UpdatedByUser = $"{Environment.UserName} - {App.GlobalState.LoggedUserName}",
                            UpdatedAt = DateTime.Now
                        };

                        if (_providerRepository.UpdatePatientService(updateService))
                        {
                            StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                            StatusMessage = _loader.GetString("SIDp_StatusMessagePatientServiceUpdated");
                            
                            await Task.Delay(3000, _cts.Token);
                            ClearPatienData();
                        }
                    }
                }

                await LoadPatientsServiceDisplayAsync();
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error applying patient service.");
                StatusColor = new SolidColorBrush(Colors.Red);
                StatusMessage = _loader.GetString("SIDp_StatusMessageErrorSavingPatientService");
                
            }
            finally
            {
                OnPropertyChanged(nameof(StatusMessage));
                OnPropertyChanged(nameof(StatusColor));
                _cts.TryReset();
            }
        }

        private void ApplyPatientFilter()
        {
            if (_patientServiceIdsDisplay == null)
                return;

            var query = PatientFilter?.Trim().ToLower();

            PatientServicesDisplay.Clear();

            IEnumerable<PatientServiceIdsDisplay> filtered;

            if (string.IsNullOrEmpty(query))
            {
                filtered = _patientServiceIdsDisplay;
            }
            else
            {
                filtered = _patientServiceIdsDisplay.Where(p =>
                    (!string.IsNullOrEmpty(p.PatientId) && p.PatientId.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.PatientAutoId.ToString()) && p.PatientAutoId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.PatientServiceId.ToString()) && p.PatientServiceId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase))
                );
            }

            foreach (var patient in filtered)
                PatientServicesDisplay.Add(patient);
        }

        private async Task ShowStatusMessage(string message, int delayMs = 3000)
        {
            StatusMessage = message;
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(StatusColor));

            await Task.Delay(delayMs);
            //ClearData();
        }

        private void ClearPatienData()
        {
            PatientAutoId = string.Empty;
            PatientIdText = string.Empty;
            ClearData();

        }

        private void ClearData()
        {
            PatientServiceBase.ServiceId = string.Empty;
            PatientServiceBase.IsActive = true;
            PatientServiceBase.NotifyEn = true;
            PatientServiceBase.NotifyAr = false;
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedPatientService = string.Empty;

        }

        private async Task ClearDataAsync()
        {
            PatientAutoId = string.Empty;
            PatientIdText = string.Empty;
            PatientServiceBase.ServiceId = string.Empty;
            PatientServiceBase.IsActive = true;
            PatientServiceBase.NotifyEn = true;
            PatientServiceBase.NotifyAr = false;
            PatientServiceBase.NotifyFr = false;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedPatientService = string.Empty;

            // Show status while reload is happening
            StatusMessage = _loader.GetString("SIDp_StatusMessageClearingAndRefreshing");

            // Perform the reload
            await InitializeAsync();

            // Keep the message visible for a short grace period
            await Task.Delay(2000, _cts.Token);

            // Clear the message after reload + delay
            StatusMessage = string.Empty;

        }

        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public void ClearPatientServicesMemory()
        {
            PatientAutoId = string.Empty;
            PatientIdText = string.Empty;
            ServiceId = string.Empty;

            PatientServiceBase.ServiceId = string.Empty;
            PatientServiceBase.IsActive = true;
            PatientServiceBase.NotifyEn = true;
            PatientServiceBase.NotifyAr = false;

            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedPatientService = string.Empty;
            SelectedPatientServiceIdsDisplay = null;

            PatientServices.Clear();
            PatientServicesDisplay.Clear();
            Doctors.Clear();
            Patients.Clear();

            _allPatientServices.Clear();
            _patientServiceIdsDisplay.Clear();
            _patients.Clear();
            _doctorServiceIdsDisplay.Clear();
            _doctors.Clear();

            _cts.Cancel();   // stop any pending delays

        }
    }
}
