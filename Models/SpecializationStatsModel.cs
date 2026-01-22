using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class SpecializationStatsModel
    {
        public string Specialization { get; set; } = string.Empty;
        public int PatientCount { get; set; }
    }
}
