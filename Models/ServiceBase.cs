using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public partial class ServiceBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ProviderType _serviceName;
        public ProviderType ServiceName
        {
            get => _serviceName;
            set
            {
                if (_serviceName != value)
                {
                    _serviceName = value;
                    OnPropertyChanged(nameof(ServiceName));
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

        // Other properties can stay as auto-properties
        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        private bool _notifyEn = true;
        public bool NotifyEn
        {
            get => _notifyEn;
            set
            {
                if (_notifyEn != value)
                {
                    _notifyEn = value;
                    if (_notifyEn) NotifyAr = false; // clear the other
                    OnPropertyChanged(nameof(NotifyEn));
                }
            }
        }

        private bool _notifyAr;
        public bool NotifyAr
        {
            get => _notifyAr;
            set
            {
                if (_notifyAr != value)
                {
                    _notifyAr = value;
                    if (_notifyAr) NotifyEn = false; // clear the other
                    OnPropertyChanged(nameof(NotifyAr));
                }
            }
        }

        public string? AddedByUser { get; set; }
        public string? UpdatedByUser { get; set; }
        public DateTime? AddedAt { get; set; }
        public string? AddedAttext => AddedAt?.ToString("F");
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedAttext => UpdatedAt?.ToString("F");

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

