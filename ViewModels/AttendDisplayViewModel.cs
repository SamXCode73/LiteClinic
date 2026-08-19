using LiteClinic.Models.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.ViewModels
{
    public partial class AttendDisplayViewModel : INotifyPropertyChanged
    {
        public AttendStatus Status { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Visibility AttendingVisibility => Status == AttendStatus.CurrentlyAttending ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AttendedVisibility => Status == AttendStatus.Attended ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MissedVisibility => Status == AttendStatus.Missed ? Visibility.Visible : Visibility.Collapsed;


        public Brush CircleColor
        {
            get
            {
                return Status switch
                {
                    AttendStatus.Missed => new SolidColorBrush(Colors.IndianRed),
                    AttendStatus.Attended => new SolidColorBrush(Colors.RoyalBlue),
                    AttendStatus.CurrentlyAttending => new SolidColorBrush(Colors.Teal),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
        }

        public void RefreshVisuals()
        {
            OnPropertyChanged(nameof(CircleColor));
            OnPropertyChanged(nameof(AttendingVisibility));
            OnPropertyChanged(nameof(AttendedVisibility));
            OnPropertyChanged(nameof(MissedVisibility));
        }

    }
}
