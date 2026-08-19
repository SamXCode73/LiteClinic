using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace LiteClinic.Models
{

    public class SlotModel
    {
        public string TimeSelected { get; set; } = string.Empty;
        public Brush SlotForeground { get; set; } = new SolidColorBrush(Colors.Black);
        public FontWeight SlotFontWeight { get; set; } = FontWeights.Normal;
    }
    public class DaySlotModel
    {
        public string DaySelected { get; set; } = string.Empty;
        // Foreground color for the day text
        public Brush DayForeground { get; set; } = new SolidColorBrush(Colors.Black);  // #D0F0F2

        //public Brush DayForeground { get; set; } = new SolidColorBrush(ColorHelper.FromArgb(255, 208, 240, 242));  // #D0F0F2

        // Font weight for the day text
        public FontWeight DayFontWeight { get; set; } = FontWeights.Normal;
        //public List<string> SlotsSelected { get; set; } = new();
        public ObservableCollection<SlotModel> SlotsSelected { get; set; } = new();
    }
}
