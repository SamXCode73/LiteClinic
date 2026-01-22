using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorServiceIds : ServiceBase
    {
        public int DoctorServiceId { get; set; }
        public int DoctorId { get; set; }
        public string? DoctorCode { get; set; }
    }
}
