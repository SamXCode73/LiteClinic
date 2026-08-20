<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorWeeklySummary
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public string? DoctorSpecialty { get; set; }
        public DateTimeOffset AppointmentDate { get; set; }
        public DateTime AppointmentTime { get; set; }
        public int PatientCount { get; set; }

        public string AppointmentDateFormatted => AppointmentDate.ToString("MMMM dd");

    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    public class DoctorWeeklySummary
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTimeOffset AppointmentDate { get; set; }
        public DateTime AppointmentTime { get; set; }
        public int PatientCount { get; set; }

        public string AppointmentDateFormatted => AppointmentDate.ToString("MMMM dd");

    }
}
>>>>>>> 9bd97308ed79d11fb3a9601f83e76357c193962c
