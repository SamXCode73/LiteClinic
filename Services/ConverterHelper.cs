using LiteClinic.Models.Enums;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.ApplicationModel.Resources;

namespace LiteClinic.Services
{
    public partial class ConverterHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b ? !b : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is bool b ? !b : value;
        }
    }

    public partial class DeactivationToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isDeactivated)
            {
                return new SolidColorBrush(isDeactivated ? Colors.IndianRed : Colors.Gray);
            }

            // Fallback if value is not a bool
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public partial class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return new SolidColorBrush(isActive ? Colors.Black : Colors.IndianRed);
            }

            // Fallback if value is not a bool
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is Visibility v && v == Visibility.Visible);
        }
    }

    public partial class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If the input is a boolean, return its negated value.
            // If the input is not a boolean, default to false.
            return(value is bool b) ? !b : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b) ? !b : false;
        }
    }

    public partial class DateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
                return dt.ToString("dd/MM/yyyy");
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // string → DateTime
            if (value is string str && DateTime.TryParse(str, out DateTime date))
                return date;

            return DateTime.Today; // or DependencyProperty.UnsetValue
        }
    }

    public partial class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
                return dt;

            if (value is string str && DateTime.TryParse(str, out var parsed))
                return parsed;

            return DateTime.Today;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
                return dt.ToString("MM/dd/yyyy"); // or your preferred format
            return string.Empty;
        }
    }

    public partial class TimeFormatterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan time)
                return DateTime.Today.Add(time).ToString("hh:mm tt"); // e.g., "08:30 AM"
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public partial class BoolToBrushConverterForCheckBox : IValueConverter
    {
        public Brush? TrueBrush { get; set; }
        public Brush? FalseBrush { get; set; }

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = value is bool b && b;
            return flag ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BoolToVisibilityConverterForCheckBox : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class EnumToBoolConverter : IValueConverter
    {
        // Store the last known valid enum value
        private static AttendStatus _lastKnownValue = AttendStatus.None;

        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString()!.Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
                return null;

            if (value is bool isChecked && isChecked)
            {
                _lastKnownValue = (AttendStatus)Enum.Parse(typeof(AttendStatus), parameter.ToString()!);
                return _lastKnownValue;
            }

            // Prevent resetting to None when unchecked
            return _lastKnownValue;
        }
    }

    public partial class BoolToBrushConverterForDoctors : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool IsScheduleActiveDis)
            {
                return new SolidColorBrush(IsScheduleActiveDis ? Colors.Teal : Colors.RoyalBlue);
            }

            // Fallback if value is not a bool
            return new SolidColorBrush(Colors.RoyalBlue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BoolToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isChecked = (bool)value;
            // If checked → bigger size, else smaller
            return isChecked ? 12.0 : 7.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class StringToProviderTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str && Enum.TryParse(typeof(ProviderType), str, out var result))
            {
                return result;
            }
            return ProviderType.Undefined; // fallback
        }

        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }
    }

    public partial class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool b && b) ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BoolToBrushConverterForRed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                // Checked → Red
                return new SolidColorBrush(Colors.Red);
            }
            else
            {
                // Unchecked → Black (or keep Red if you want always red)
                return new SolidColorBrush(Colors.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public partial class ThemeTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ThemeType theme)
            {
                return theme switch
                {
                    ThemeType.Light => new SolidColorBrush(Colors.White),
                    ThemeType.Dark => new SolidColorBrush(Colors.Black),
                    ThemeType.Pink => new SolidColorBrush(Colors.Pink),
                    ThemeType.RoyalBlue => new SolidColorBrush(Colors.RoyalBlue),
                    ThemeType.Teal => new SolidColorBrush(Colors.Teal),
                    ThemeType.Violet => new SolidColorBrush(Colors.Violet),
                    ThemeType.MintGreen => new SolidColorBrush(Colors.MediumSeaGreen),
                    ThemeType.Coral => new SolidColorBrush(Colors.Coral),
                    ThemeType.Lavender => new SolidColorBrush(Colors.Lavender),
                    ThemeType.Sandstone => new SolidColorBrush(Colors.BurlyWood),
                    ThemeType.Monochrome => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    public partial class ProfilePictureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var path = value as string;
            if (string.IsNullOrEmpty(path))
            {
                return "ms-appx:///Assets/Profiles/Defaults/default_avatar.png";
            }
            return path;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

    }
    public partial class AttendStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AttendStatus status)
            {
                return status switch
                {
                    AttendStatus.None => new SolidColorBrush(Colors.Gray),
                    AttendStatus.Missed => new SolidColorBrush(Colors.IndianRed),
                    AttendStatus.Attended => new SolidColorBrush(Colors.RoyalBlue),
                    AttendStatus.CurrentlyAttending => new SolidColorBrush(Colors.Teal),
                    _ => new SolidColorBrush(Colors.Gray),
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }

    //public partial class AttendStatusToColorConverterToolTip : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        if (value is AttendStatus status)
    //        {
    //            return status switch
    //            {
    //                AttendStatus.None => "Not Attended",
    //                AttendStatus.Missed => "Missed",
    //                AttendStatus.Attended => "Attended",
    //                AttendStatus.CurrentlyAttending => "Currently Attending",
    //                _ => "Unknown",
    //            };
    //        }
    //        return new SolidColorBrush(Colors.Gray);
    //    }
    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }

    //}

    public partial class AttendStatusToColorConverterToolTip : IValueConverter
    {
        private readonly ResourceLoader _resourceLoader = new();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AttendStatus status)
            {
                string key = status switch
                {
                    AttendStatus.None => "AppP_AttendStatus_None",
                    AttendStatus.Missed => "AppP_AttendStatus_Missed",
                    AttendStatus.Attended => "AppP_AttendStatus_Attended",
                    AttendStatus.CurrentlyAttending => "AppP_AttendStatus_CurrentlyAttending",
                    _ => "AppP_AttendStatus_Unknown"
                };

                // Look up localized string from Resources.resw
                return _resourceLoader.GetString(key);
            }
            return _resourceLoader.GetString("AppP_AttendStatus_Unknown");
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}





