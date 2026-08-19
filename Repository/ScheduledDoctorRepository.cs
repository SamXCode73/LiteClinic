using LiteClinic.Models;
using LiteClinic.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Repository
{
    public class ScheduledDoctorRepository
    {
        public List<ScheduledDoctor> GetAllScheduledDoctors()
        {
            var scheduledDoctors = new List<ScheduledDoctor>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT ScheduleAutoId, ScheduleId, DoctorId, DayOfWeek, Notify, IsActive
                FROM DoctorSchedule
                WHERE IsActive = 1;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    scheduledDoctors.Add(new ScheduledDoctor
                    {
                        ScheduleAutoId = reader.GetInt32(0),
                        ScheduleId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        DoctorId = reader.GetInt32(2),
                        DayOfWeek = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Notify = reader.GetBoolean(4),
                        IsActive = reader.GetBoolean(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving scheduled doctors.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return scheduledDoctors;
        }

        public List<ScheduledDoctorDisplayModel> GetAllScheduledDoctorsView()
        {
            var scheduledDoctorDisplayModel = new List<ScheduledDoctorDisplayModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT ScheduleAutoId, ScheduleId, DoctorId, DoctorCode, FullName, Specialization, 
                       PhoneNumber, LandLineNumber, DayOfWeek, Notify, ScheduleIsActive, DoctorIsActive, WeekNumbers, TimeFromTo
                FROM DoctorScheduleView;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    scheduledDoctorDisplayModel.Add(new ScheduledDoctorDisplayModel
                    {
                        ScheduleAutoIdDis = reader.GetInt32(0),
                        ScheduleIdDis = reader.IsDBNull(1) ? null : reader.GetString(1),
                        DoctorIdDis = reader.GetInt32(2),
                        DoctorCodeDis = reader.IsDBNull(3) ? null : reader.GetString(3),
                        FullNameDis = reader.IsDBNull(4) ? null : reader.GetString(4),
                        SpecializationDis = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PhoneNumberDis = reader.IsDBNull(6) ? null : reader.GetString(6),
                        DayOfWeekDis = reader.IsDBNull(8) ? null : reader.GetString(8),
                        NotifyDis = reader.GetBoolean(9),
                        IsScheduleActiveDis = reader.GetBoolean(10),
                        IsDoctorActiveDis = reader.GetBoolean(11),
                        WeekNumbersDis = reader.IsDBNull(12) ? null : reader.GetString(12),
                        TimeDis = reader.IsDBNull(13) ? null : reader.GetString(13)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving scheduled doctors.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return scheduledDoctorDisplayModel;
        }

        public bool SaveScheduledDoctor(ScheduledDoctor schedule)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            INSERT INTO DoctorSchedule (
                ScheduleId, DoctorId, DayOfWeek, Notify, IsActive, WeekNumbers, TimeFromTo) 
            VALUES (
                @ScheduleId, @DoctorId, @DayOfWeek, @Notify, @IsActive, @WeekNumbers, @TimeFromTo);";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ScheduleId", schedule.ScheduleId);
                cmd.Parameters.AddWithValue("@DoctorId", schedule.DoctorId);
                cmd.Parameters.AddWithValue("@DayOfWeek", schedule.DayOfWeek ?? "");
                cmd.Parameters.AddWithValue("@Notify", schedule.Notify ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsActive", schedule.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@WeekNumbers", schedule.WeekNumbers);
                cmd.Parameters.AddWithValue("@TimeFromTo", schedule.DisTime ?? "Not Set");

                   cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving scheduled doctor.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdateScheduledDoctor(ScheduledDoctor schedule)
        {
            if (schedule == null)
            { 
                throw new ArgumentNullException(nameof(schedule), "ScheduledDoctor cannot be null.");
            }
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            UPDATE DoctorSchedule SET
                DoctorId = @DoctorId,
                DayOfWeek = @DayOfWeek,
                Notify = @Notify,
                IsActive = @IsActive,
                WeekNumbers = @WeekNumbers,
                TimeFromTo = @TimeFromTo
            WHERE ScheduleAutoId = @ScheduleAutoId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@DoctorId", schedule.DoctorId);
                cmd.Parameters.AddWithValue("@DayOfWeek", schedule.DayOfWeek ?? "");
                cmd.Parameters.AddWithValue("@Notify", schedule.Notify ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsActive", schedule.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@WeekNumbers", schedule.WeekNumbers);
                cmd.Parameters.AddWithValue("@ScheduleAutoId", schedule.ScheduleAutoId);
                cmd.Parameters.AddWithValue("@TimeFromTo", schedule.DisTime ?? "Not Set");


                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating scheduled doctor.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool DeactivateScheduledDoctor(ScheduledDoctor schedule)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            UPDATE DoctorSchedule SET
                IsActive = @IsActive
            WHERE ScheduleAutoId = @ScheduleAutoId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ScheduleAutoId", schedule.ScheduleAutoId);
                cmd.Parameters.AddWithValue("@IsActive", schedule.IsActive ? 1 : 0);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deactivating scheduled doctor.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public List<ScheduledDoctorDisplayModel> GetDoctorSchedules()
        {
            var doctorSchedules = new List<ScheduledDoctorDisplayModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT 
                                    ScheduleAutoId, ScheduleId, DoctorId, DoctorCode,
                                    FullName, Specialization, PhoneNumber, LandLineNumber,
                                    DayOfWeek, Notify, ScheduleIsActive, DoctorIsActive
                                FROM DoctorScheduleView;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    doctorSchedules.Add(new ScheduledDoctorDisplayModel
                    {
                        ScheduleAutoIdDis = reader.GetInt32(0),
                        ScheduleIdDis = reader.GetString(1),
                        DoctorIdDis = reader.GetInt32(2),
                        DoctorCodeDis = reader.IsDBNull(3) ? null : reader.GetString(3),
                        FullNameDis = reader.IsDBNull(4) ? null : reader.GetString(4),
                        SpecializationDis = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PhoneNumberDis = reader.IsDBNull(6) ? null : reader.GetString(6),
                        LandLineNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                        DayOfWeekDis = reader.IsDBNull(8) ? null : reader.GetString(8),
                        NotifyDis = reader.GetBoolean(9),
                        IsScheduleActiveDis = reader.GetBoolean(10),
                        IsDoctorActiveDis = reader.GetBoolean(11)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving doctor schedules from DoctorScheduleView.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return doctorSchedules;
        }

        // Just for doctor Card
        public async Task<List<DoctorCardModel>> GetAllDoctorCardsViewAsync()
        {
            var doctorCards = new List<DoctorCardModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            SELECT DoctorId, FullName, Specialization, DayOfWeek, TimeFromTo, Gender, ProfilePicturePath, 
                    PhoneNumber, LandLineNumber, ServiceId
            FROM DoctorScheduleView
            WHERE ScheduleIsActive = 1 AND DoctorIsActive = 1;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var fullName = reader.IsDBNull(1) ? null : reader.GetString(1);

                    doctorCards.Add(new DoctorCardModel
                    {
                        DoctorId = reader.GetInt32(0),
                        FullName = fullName,
                        Specialization = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        AttendingDays = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        TimeSlots = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        Gender = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        ProfilePicturePath = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        PhoneNumber = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        LandLineNumber = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        ServiceId = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),

                        // Compute initials directly here
                        Initials = ComputeInitials(fullName ?? "")
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving doctor cards.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return doctorCards;

        }




        private static string ComputeInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;

            string first = parts[0].Substring(0, 1).ToUpper();
            string last = parts.Length > 1 ? parts[^1].Substring(0, 1).ToUpper() : string.Empty;

            return first + last;
        }


    }
}
