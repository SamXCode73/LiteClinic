using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorGroup
    {
        public string? DateLabel { get; set; }
        public ObservableCollection<DoctorWeeklySummary>? Doctors { get; set; }
    }
}
