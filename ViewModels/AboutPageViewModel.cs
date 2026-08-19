using LiteClinic.Models.Enums;
using LiteClinic.Repository;
using LiteClinic.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace LiteClinic.ViewModels
{
    public partial class AboutPageViewModel : INotifyPropertyChanged
    {

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

        public Visibility GregorianDateVisibility => App.GlobalState.ShowGregorianDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HijriDateVisibility => App.GlobalState.ShowHijriDate ? Visibility.Visible : Visibility.Collapsed;

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

        private string? _appVersion;

        public string? AppVersion
        {
            get => _appVersion ??= GetAppVersion();
            set { _appVersion = value; OnPropertyChanged(nameof(AppVersion)); }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public async Task GetHiriRomanDate()
        {
            RomanDate = DateHelper.GetRomanDate();
            HijriDate = DateHelper.GetHijriDate();
            await Task.CompletedTask;
        }

        public async Task InitializeSettingsAsync()
        {
            try
            {
                // Show/ Hide date
                await GetHiriRomanDate();
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to initialize settings from database");
            }
        }


        private string GetAppVersion()
        {
            try
            {
                var v = Package.Current.Id.Version;
                AppVersion = $"LiteClinic, V {v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                return AppVersion;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"ERROR APP VERSION. {this.GetType().Name}");
                return "version unknown)";
            }
        }


    }
}
