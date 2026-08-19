using LiteClinic.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Services
{
    public static class AppointmentTypeLocalization
    {
        public static readonly Dictionary<AppointmentTypes, string> English =
            new Dictionary<AppointmentTypes, string>
                {
                    { AppointmentTypes.Checkup, "Checkup" },
                    { AppointmentTypes.Consultation, "Consultation" },
                    { AppointmentTypes.FollowUp, "Follow-up" },
                    { AppointmentTypes.LabTest, "Lab Test" },
                    { AppointmentTypes.Vaccination, "Vaccination" },
                    { AppointmentTypes.Procedure, "Procedure" },
                    { AppointmentTypes.TherapySession, "Therapy Session" },
                    { AppointmentTypes.Telehealth, "Telehealth" }
                 };

        public static readonly Dictionary<AppointmentTypes, string> Arabic =
            new()
            {
            { AppointmentTypes.Checkup, "فحص دوري" },
            { AppointmentTypes.Consultation, "استشارة" },
            { AppointmentTypes.FollowUp, "متابعة" },
            { AppointmentTypes.LabTest, "تحليل مخبري" },
            { AppointmentTypes.Vaccination, "تلقيح" },
            { AppointmentTypes.Procedure, "إجراء طبي" },
            { AppointmentTypes.TherapySession, "جلسة علاج" },
            { AppointmentTypes.Telehealth, "استشارة عن بعد" }
            };

        public static readonly Dictionary<AppointmentTypes, string> French =
            new()
            {
            { AppointmentTypes.Checkup, "Bilan de santé" },
            { AppointmentTypes.Consultation, "Consultation" },
            { AppointmentTypes.FollowUp, "Suivi" },
            { AppointmentTypes.LabTest, "Analyse de laboratoire" },
            { AppointmentTypes.Vaccination, "Vaccination" },
            { AppointmentTypes.Procedure, "Procédure médicale" },
            { AppointmentTypes.TherapySession, "Séance de thérapie" },
            { AppointmentTypes.Telehealth, "Téléconsultation" }
            };
    }
}

