using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public partial class PatientServiceIdsDisplay : ServiceBase
    {
        public int PatientServiceId { get; set; }
        public int PatientAutoId { get; set; }
        public string? PatientId { get; set; }   // external ID (string if PatientId is TEXT in PatientTable)

    }
}
