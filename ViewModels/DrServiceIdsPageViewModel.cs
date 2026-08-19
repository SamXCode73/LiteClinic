using CommunityToolkit.Mvvm.Input;
using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Windows.ApplicationModel.Resources;

namespace LiteClinic.ViewModels
{
    public partial class DrServiceIdsPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DoctorServiceIds> DoctorServices { get; private set; } = [];
        public ObservableCollection<DoctorServiceIdsDisplay> DoctorServicesDisplay { get; private set; } = [];
        public ObservableCollection<DoctorsModel> Doctors { get; private set; } = []; // For Updateing the Doctor List with ID and Code only
     


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //public ICommand? Btn_ApplyPatientCommand { get; }
        //public ICommand? Btn_ApplydoctorCommand { get; }

     
        public IAsyncRelayCommand? Btn_ApplydoctorCommand { get; }  
        public IAsyncRelayCommand? Btn_ClearDoctorCommand { get; }

        public bool CanAddDrChatId => PermissionHelper.CanManageRecords; // Add New Record
        public bool CanEditDrChatId => PermissionHelper.CanEditRecords; // Edit existing Record
        public bool CanDeactivateDrChatId => PermissionHelper.CanEditRecords; // Deactivate Record


        private readonly ResourceLoader _loader = new();
        private readonly DoctorsRepository _doctorsRepository = new();
        private readonly ProviderRepository _providerRepository = new();

        private List<DoctorServiceIdsDisplay> _doctorServiceIdsDisplay = [];
        private List<DoctorsModel> _doctors = [];
        private CancellationTokenSource _cts = new();

        private DoctorServiceIdsDisplay? _selectedDoctorServiceIdsDisplay;



        public DrServiceIdsPageViewModel()
        {

            Btn_ApplydoctorCommand = new AsyncRelayCommand(ApplyDoctorCommand);
            Btn_ClearDoctorCommand = new AsyncRelayCommand(async () =>
            {
                await ClearDataAsync(); 
            });
        }

        // Arrange List 

        private List<DoctorServiceIds> _allDoctorServices = [];
        private string? _statusMessage;
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Black);


        private string? _doctorIdText;
        public string? DoctorIdText
        {
            get => _doctorIdText;
            set
            {
                if (_doctorIdText != value)
                {
                    _doctorIdText = value;

                    if (int.TryParse(_doctorIdText, out var id))
                    {
                        var match = Doctors.FirstOrDefault(d => d.DoctorId == id);

                        if (match != null)
                        {
                            if (match.IsActive)
                            {
                                DoctorCodeText = match.DoctorCode;
                            }
                            else
                            {
                                StatusColor = new SolidColorBrush(Colors.IndianRed);
                                _ = ShowStatusMessage(
                                    $"WARNING! Wrong ID or Doctor {match.DoctorCode} is deactivated.",
                                    5000
                                );
                                return;
                            }
                        }
                        else
                        {
                            StatusColor = new SolidColorBrush(Colors.OrangeRed);
                            _ = ShowStatusMessage("WARNING! Doctor ID not found.", 5000);
                        }
                    }

                    OnPropertyChanged(nameof(DoctorIdText));
                }
            }
        }

        private string? _doctorCodeText;
        public string? DoctorCodeText
        {
            get => _doctorCodeText;
            set
            {
                if (_doctorCodeText != value)
                {
                    _doctorCodeText = value;

                    var match = Doctors.FirstOrDefault(
                        d => string.Equals(d.DoctorCode, _doctorCodeText, StringComparison.OrdinalIgnoreCase)
                    );

                    if (match != null)
                    {
                        if (match.IsActive)
                        {
                            DoctorIdText = match.DoctorId.ToString();
                        }
                        else
                        {
                            StatusColor = new SolidColorBrush(Colors.IndianRed);
                            _ = ShowStatusMessage(
                                $"WARNING! Wrong ID or Doctor {_doctorCodeText} is deactivated.",
                                5000
                            );
                            return;
                        }
                    }
                    else
                    {
                        StatusColor = new SolidColorBrush(Colors.OrangeRed);
                        _ = ShowStatusMessage("WARNING! Doctor code not found.", 5000);
                    }

                    OnPropertyChanged(nameof(DoctorCodeText));
                }
            }
        }

        // Filter properties

        private string? _doctorFilter;
        public string? DoctorFilter
        {
            get => _doctorFilter;
            set
            {
                if (_doctorFilter != value)
                {
                    _doctorFilter = value;
                    OnPropertyChanged(nameof(DoctorFilter));
                    ApplyDoctorFilter();
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


        public ServiceBase DoctorServiceBase { get; set; } = new ServiceBase();

        public string? SelectedDoctorService
        {
            get => DoctorServiceBase.ServiceName.ToString();
            set
            {
                if (DoctorServiceBase.ServiceName.ToString() != value &&
                    Enum.TryParse<ProviderType>(value, out var parsed))
                {
                    DoctorServiceBase.ServiceName = parsed;
                    OnPropertyChanged(nameof(SelectedDoctorService));
                }
            }
        }

        public DoctorServiceIdsDisplay? SelectedDoctorServiceIdsDisplay
        {
            get => _selectedDoctorServiceIdsDisplay!;
            set
            {
                if (_selectedDoctorServiceIdsDisplay != value)
                {
                    _selectedDoctorServiceIdsDisplay = value;
                    OnPropertyChanged(nameof(SelectedDoctorServiceIdsDisplay));
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

        public async Task LoadDoctorsCode()
        {
            try
            {
                // If your repository is synchronous:
                _doctors = await Task.Run(() => _doctorsRepository.GetActiveDoctorsForSericeCode());

                Doctors.Clear();
                foreach (var doctors in _doctors)
                {
                    Doctors.Add(doctors);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading Doctors.");
            }
        }

        public async Task LoadDoctorsServiceAsync()
        {
            try
            {
                // If your repository is synchronous:
                var list = await Task.Run(() => _providerRepository.GetAllDoctorProvider());

                DoctorServices.Clear();
                foreach (var doctor in list)
                    DoctorServices.Add(doctor);

                // Cache for local filtering (search box)
                _allDoctorServices = list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading Doctors.");
            }
        }

        public async Task LoadDoctorsServiceDisplayAsync()
        {
            try
            {
                // If your repository is synchronous:
                var list = await Task.Run(() => _providerRepository.GetAllDoctorsWithServicesDisplay());

                DoctorServicesDisplay.Clear();
                foreach (var doctor in list)
                    DoctorServicesDisplay.Add(doctor);

                // Cache for local filtering (search box)
                _doctorServiceIdsDisplay = list;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading Doctors.");
            }
        }

        public async Task InitializeAsync()
        {
            await GetHiriRomanDate();
            await LoadDoctorsCode();
            await LoadDoctorsServiceAsync();
            await LoadDoctorsServiceDisplayAsync();
        }

        public async Task ApplyDoctorCommand()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DoctorIdText) || string.IsNullOrWhiteSpace(DoctorServiceBase.ServiceId))
                {
                    StatusColor = new SolidColorBrush(Colors.IndianRed);
                    StatusMessage = _loader.GetString("DrSp_Status_DoctorIdServiceRequired");
                    return;
                }

                int doctorId = int.Parse(DoctorIdText);
                var doctorCode = DoctorCodeText;
                var serviceName = DoctorServiceBase.ServiceName;
                var serviceId = DoctorServiceBase.ServiceId;
                var activation = DoctorServiceBase.IsActive;
                var notifyEn = DoctorServiceBase.NotifyEn;
                var notifyfr = DoctorServiceBase.NotifyFr;
                var notifyAr = DoctorServiceBase.NotifyAr;

                // Find existing by DoctorId + ServiceName
                var existing = DoctorServicesDisplay
                    .FirstOrDefault(d => d.DoctorAutoId == doctorId && d.ServiceName == serviceName);

                if (existing == null)
                {
                    // Insert new
                    var newService = new DoctorServiceIds
                    {
                        DoctorId = doctorId,
                        DoctorCode = doctorCode,
                        ServiceName = serviceName,
                        ServiceId = serviceId,
                        IsActive = activation,
                        NotifyEn = notifyEn,
                        NotifyFr = notifyfr,
                        NotifyAr = notifyAr,
                        AddedByUser = $"{Environment.UserName} - {App.GlobalState.LoggedUserName}",
                        AddedAt = DateTime.Now
                    };
                    Logger.LogInfo($"Adding new Doctor Service: DoctorId={doctorId}, DoctorCode={doctorCode}, ServiceName={serviceName}, ServiceId={serviceId}", "ApplyDoctorCommand");

                    if (_providerRepository.ApplyDocotrService(newService))
                    {
                        StatusMessage = _loader.GetString("DrSp_Status_DoctorServiceAdded");
                        StatusColor = new SolidColorBrush(Colors.Green);
                        await Task.Delay(3000, _cts.Token);
                        ClearDoctorData();
                    }
                }
                else
                {
                    bool isDuplicate = string.Equals(existing.ServiceId, serviceId, StringComparison.OrdinalIgnoreCase) && 
                        existing.NotifyEn == notifyEn && 
                        existing.NotifyFr == notifyfr &&
                        existing.NotifyAr == notifyAr &&
                        existing.IsActive == activation;

                    if (isDuplicate)
                    {
                        // Exact duplicate
                        StatusMessage = string.Format(_loader.GetString("DrSp_Status_DoctorServiceExists"),
                            existing.ServiceName,
                            existing.DoctorAutoId,
                            existing.DoctorCodeText);

                        StatusColor = new SolidColorBrush(Colors.OrangeRed);
                        await Task.Delay(5000, _cts.Token);
                        ClearDoctorData();
                    }
                    else
                    {
                        // Update ServiceId
                        var updateService = new DoctorServiceIds
                        {
                            DoctorServiceId = existing.DoctorServiceId,
                            DoctorId = doctorId,
                            DoctorCode = doctorCode,
                            ServiceName = serviceName,
                            ServiceId = serviceId,
                            IsActive = activation,
                            NotifyEn = notifyEn,
                            NotifyFr = notifyfr,
                            NotifyAr = notifyAr,
                            UpdatedByUser = $"{Environment.UserName} - {App.GlobalState.LoggedUserName}",
                            UpdatedAt = DateTime.Now
                        };

                        if (_providerRepository.UpdateDoctorService(updateService))
                        {
                            StatusMessage = _loader.GetString("DrSp_Status_DoctorServiceUpdated");
                            StatusColor = new SolidColorBrush(Colors.RoyalBlue);
                            await Task.Delay(3000);
                            ClearDoctorData();
                        }
                    }
                }

                await LoadDoctorsServiceDisplayAsync();
            }
            catch (TaskCanceledException)
            {
                // Ignore if the task was canceled
                return;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error applying doctor service.");
                StatusMessage = _loader.GetString("DrSp_Status_DoctorServiceSaveError");
                StatusColor = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                _cts.TryReset(); // Reset the CancellationTokenSource for future use
                OnPropertyChanged(nameof(StatusMessage));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        private void ApplyDoctorFilter()
        {
            if (_doctorServiceIdsDisplay == null)
                return;

            var query = DoctorFilter?.Trim().ToLower();

            DoctorServicesDisplay.Clear();

            IEnumerable<DoctorServiceIdsDisplay> filtered;

            if (string.IsNullOrEmpty(query))
            {
                filtered = _doctorServiceIdsDisplay;
            }
            else
            {
                filtered = _doctorServiceIdsDisplay.Where(d =>
                    (!string.IsNullOrEmpty(d.DoctorCodeText) && d.DoctorCodeText.Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.DoctorServiceId.ToString()) && d.DoctorServiceId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase)) ||
                    (!string.IsNullOrEmpty(d.DoctorAutoId.ToString()) && d.DoctorAutoId.ToString().Contains(query, StringComparison.CurrentCultureIgnoreCase))
                );
            }

            foreach (var doctor in filtered)
                DoctorServicesDisplay.Add(doctor);
        }

        private async Task ShowStatusMessage(string message, int delayMs = 3000)
        {
            StatusMessage = message;
            OnPropertyChanged(nameof(StatusMessage));
            OnPropertyChanged(nameof(StatusColor));

            await Task.Delay(delayMs, _cts.Token);
            //ClearData();
        }

        private void ClearDoctorData()
        {
            DoctorIdText = string.Empty;
            DoctorCodeText = string.Empty;
            ClearData();
        }

        private void ClearData()
        {
            DoctorServiceBase.ServiceId = string.Empty;
            DoctorServiceBase.NotifyEn = true;
            DoctorServiceBase.NotifyAr = false;
            DoctorServiceBase.NotifyFr = false;
            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedDoctorService = string.Empty;
        }

        private async Task ClearDataAsync()
        {
            // Reset fields
            DoctorIdText = string.Empty;
            DoctorCodeText = string.Empty;
            DoctorServiceBase.ServiceId = string.Empty;
            DoctorServiceBase.NotifyEn = true;
            DoctorServiceBase.NotifyFr = false;
            DoctorServiceBase.NotifyAr = false;
            StatusColor = new SolidColorBrush(Colors.Black);
            SelectedDoctorService = string.Empty;

            // Show status while reload is happening
            StatusMessage = _loader.GetString("DrSp_Status_ClearingRefreshing");

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

        public void ClearDrServiceIdsMemory()
        {
            DoctorIdText = string.Empty;
            DoctorCodeText = string.Empty;
            ServiceId = string.Empty;

            DoctorServiceBase.ServiceId = string.Empty;
            DoctorServiceBase.NotifyEn = true;
            DoctorServiceBase.NotifyAr = false;
            DoctorServiceBase.NotifyFr = false;
            DoctorServiceBase.IsActive = true;
            DoctorServiceBase.ServiceName = ProviderType.Telegram; // reset to default enum value

            StatusMessage = string.Empty;
            StatusColor = new SolidColorBrush(Colors.Black);

            SelectedDoctorService = string.Empty;
            SelectedDoctorServiceIdsDisplay = null;

            DoctorServices.Clear();
            DoctorServicesDisplay.Clear();
            Doctors.Clear();

            _doctorServiceIdsDisplay.Clear();
            _doctors.Clear();
            _allDoctorServices.Clear();
            _cts.Cancel();
        }
    }
}
