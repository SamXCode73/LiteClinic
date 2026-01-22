using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public partial class DoctorServiceIdsDisplay : ServiceBase
    {
        public int DoctorServiceId { get; set; }
        public int DoctorAutoId { get; set; }
        public string? DoctorCodeText { get; set; }   // external ID (string if PatientId is TEXT in PatientTable)

    }
}
