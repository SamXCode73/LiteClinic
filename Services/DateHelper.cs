using System;
using System.Globalization;


namespace LiteClinic.Services
{
    internal static class DateHelper
    {
        internal static string GetRomanDate()
        {
            return DateTime.Now.ToString("dddd, MMMM dd yyyy", new CultureInfo("en-US"));
        }

        public static string GetHijriDate()
        {
            try
            {
                HijriCalendar hijri = new HijriCalendar();
                DateTime now = DateTime.Now;
                return $"{hijri.GetDayOfMonth(now)} {GetHijriMonthName(hijri.GetMonth(now))} {hijri.GetYear(now)} هـ";
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                Logger.LogError(ex, "Failed to get Hijri date, falling back to Gregorian date.");
                return DateTime.Now.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
                
            }
        }

        private static string GetHijriMonthName(int month)
        {
            string[] months = {
            "محرم", "صفر", "ربيع الأول", "ربيع الآخر",
            "جمادى الأولى", "جمادى الآخرة", "رجب", "شعبان",
            "رمضان", "شوال", "ذو القعدة", "ذو الحجة"
        };
            return months[month - 1];
        }
    }
}
