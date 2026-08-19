using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace LiteClinic.Repository
{
    public class AppointmentsRepository
    {
        public List<AppointmentModel> GetAllAppointments()
        {
            var appointments = new List<AppointmentModel>();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT ScheduleId, AppointmentID, PatientId, DoctorId, AppointmentDate, 
                    AppointmentTime, AppointmentType, Notes, CreatedAt, IsActive, IsMissed, IsAttending
            FROM ScheduledAppointments
            WHERE IsActive = 1;";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Safely parse AppointmentDate from database
                    DateTime appointmentDate = DateTime.Now;
                    if (!reader.IsDBNull(4))
                    {

                        var dateString = reader.GetString(4).Trim(); // Read as string
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            appointmentDate = parsedDate;

                        }
                        else
                        {
                            Logger.LogError(new FormatException($"Invalid date format: {dateString}"), "Error parsing appointment date.");
                        }
                    }

                    appointments.Add(new AppointmentModel
                    {
                        ScheduleId = reader.GetInt32(0),
                        AppointmentID = reader.IsDBNull(1) ? null : reader.GetString(1),
                        PatientAutoId = reader.GetInt32(2),
                        DoctorId = reader.GetInt32(3),
                        AppointmentDate = appointmentDate,
                        AppointmentTime = TimeSpan.Parse(reader.GetString(5)),
                        AppointmentType = reader.IsDBNull(6) ? default(AppointmentTypes)
                        : Enum.Parse<AppointmentTypes>(reader.GetString(6)),
                        Notes = reader.IsDBNull(7) ? null : reader.GetString(7),
                        CreatedAt = reader.GetDateTime(8),
                        IsActive = reader.GetBoolean(9),
                        IsMissed = reader.GetBoolean(10),
                        IsAttending = reader.GetBoolean(11)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving appointments.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return appointments;
        }

        public AppointmentModel GetLastppointment()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT seq FROM sqlite_sequence WHERE name='ScheduledAppointments';";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new AppointmentModel
                    {
                        ScheduleId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving appointments.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
            return new AppointmentModel { ScheduleId = 0 };
        }

        public List<ScheduledAppointmentDisplay> GetDisplayedAppointments()
        {
            var displayList = new List<ScheduledAppointmentDisplay>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT ScheduleId, AppointmentID, PatientId, PatientName, 
                                    PatientMotherName, PatientDOB, DoctorId, DoctorName, 
                                    Specialty, AppointmentDate, AppointmentTime, 
                                    AppointmentType, Notes, IsActive, IsMissed, IsAttending, AttendStatus
                                    FROM ViewScheduledAppointments
                                    WHERE IsActive = 1 
                                    ORDER BY AppointmentDate ASC, AppointmentTime ASC;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    displayList.Add(new ScheduledAppointmentDisplay
                    {
                        ScheduleId = reader.GetInt32(0),
                        AppointmentID = reader.GetString(1),
                        PatientId = reader.GetInt32(2),
                        PatientName = reader.GetString(3),
                        PatientMotherName = reader.GetString(4),
                        PatientDOB = DateTime.ParseExact(reader.GetString(5), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        DoctorId = reader.GetInt32(6),
                        DoctorName = reader.GetString(7),
                        Specialty = reader.GetString(8),
                        AppointmentDate = DateTime.ParseExact(reader.GetString(9), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        AppointmentTime = TimeSpan.Parse(reader.GetString(10)),
                        AppointmentType = reader.IsDBNull(10) ? null : reader.GetString(11),
                        Notes = reader.IsDBNull(11) ? null : reader.GetString(12),
                        IsActive = reader.GetBoolean(13),
                        IsMissed = reader.GetBoolean(14),
                        IsAttending = reader.GetBoolean(15),
                        AttendStatus = (AttendStatus)reader.GetInt32(16)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving displayed appointments.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
            return displayList;
        }

        public List<ScheduledAppointmentDisplay> GetAppointmentsForWeek(DateTime startDate)
        {
            var appointments = new List<ScheduledAppointmentDisplay>();
            var endDate = startDate.AddDays(20);

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                SELECT DoctorId, DoctorName, Specialization,
                       ScheduleId, AppointmentID, AppointmentDate, AppointmentTime,
                       AppointmentType, Notes, IsMissed, IsAttending, AttendStatus,
                       PatientName, PatientMotherName, PatientDOB
                FROM ViewDoctorAppointments
                WHERE AppointmentDate BETWEEN @StartDate AND @EndDate
                ORDER BY AppointmentDate, AppointmentTime;";


                cmd.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyy-MM-dd"));

                //Debug.WriteLine($"Executing SQL: {cmd.CommandText} with StartDate={startDate:yyyy-MM-dd} and EndDate={endDate:yyyy-MM-dd}");

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    appointments.Add(new ScheduledAppointmentDisplay
                    {
                        DoctorId = reader.GetInt32(0),
                        DoctorName = reader.GetString(1),
                        Specialty = reader.GetString(2),
                        ScheduleId = reader.GetInt32(3),
                        AppointmentID = reader.GetString(4),
                        AppointmentDate = DateTime.ParseExact(reader.GetString(5), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        AppointmentTime = TimeSpan.ParseExact(reader.GetString(6), @"hh\:mm", CultureInfo.InvariantCulture),
                        AppointmentType = reader.IsDBNull(7) ? null : reader.GetString(7),
                        Notes = reader.IsDBNull(8) ? null : reader.GetString(8),
                        IsMissed = reader.GetBoolean(9),
                        IsAttending = reader.GetBoolean(10),
                        AttendStatus = (AttendStatus)reader.GetInt32(11),
                        PatientName = reader.GetString(12),
                        PatientMotherName = reader.GetString(13),
                        PatientDOB = DateTime.ParseExact(reader.GetString(14), "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving weekly appointments.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return appointments;
        }


        public bool SaveAppointment(AppointmentModel appointment)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
            INSERT INTO ScheduledAppointments (
                AppointmentID, PatientId, DoctorId, AppointmentDate, AppointmentTime,
                AppointmentType,IsActive, Notes, CreatedBy, CreatedAt
            ) VALUES (
                @AppointmentID, @PatientId, @DoctorId, @AppointmentDate, @AppointmentTime,
                @AppointmentType, @IsActive, @Notes, @CreatedBy, @CreatedAt
            );";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@AppointmentID", appointment.AppointmentID);
                cmd.Parameters.AddWithValue("@PatientId", appointment.PatientAutoId);
                cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime.ToString(@"hh\:mm"));
                cmd.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType.ToString());
                cmd.Parameters.AddWithValue("@Notes", appointment.Notes ?? "");
                cmd.Parameters.AddWithValue("@IsActive", appointment.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedBy", appointment.CreatedBy ?? "");
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("F"));

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving appointment.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdateAppointment(AppointmentModel appointment)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
            UPDATE ScheduledAppointments SET
                PatientId = @PatientId,
                DoctorId = @DoctorId,
                AppointmentDate = @AppointmentDate,
                AppointmentTime = @AppointmentTime,
                AppointmentType = @AppointmentType,
                Notes = @Notes,
                UpdatedBy = @UpdatedBy,
                UpdatedAT = @UpdatedAT,
                IsMissed = @IsMissed
            WHERE ScheduleId = @ScheduleId;";                

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@PatientId", appointment.PatientAutoId);
                cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime.ToString(@"hh\:mm"));
                cmd.Parameters.AddWithValue("@AppointmentType", appointment.AppointmentType.ToString());
                cmd.Parameters.AddWithValue("@Notes", appointment.Notes ?? "");
                cmd.Parameters.AddWithValue("@UpdatedBy", appointment.UpdatedBy ?? "");
                cmd.Parameters.AddWithValue("@UpdatedAT", appointment.UpdatedAt.ToString("F"));
                cmd.Parameters.AddWithValue("@IsMissed", appointment.IsMissed ? 1 : 0);

                // Where Cluase
                cmd.Parameters.AddWithValue("@ScheduleId", appointment.ScheduleId);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating appointment.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdateAppointmentStatus(AppointmentModel appointment)
        {
            try
            { 
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            UPDATE ScheduledAppointments SET
                UpdatedBy = @UpdatedBy,
                UpdatedAt = @UpdatedAt,
                AttendStatus = @AttendStatus
            WHERE ScheduleId = @ScheduleId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UpdatedBy", appointment.UpdatedBy);
                cmd.Parameters.AddWithValue("@UpdatedAt", appointment.UpdatedAt.ToString("F"));
                cmd.Parameters.AddWithValue("@AttendStatus", appointment.AttendStatus);
                cmd.Parameters.AddWithValue("@ScheduleId", appointment.ScheduleId);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating appointment.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }


        public bool DeactivateAppointment(AppointmentModel appointment)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            UPDATE ScheduledAppointments SET
                IsActive = @IsActive,
                UpdatedBy = @UpdatedBy,
                UpdatedAt = @UpdatedAt
            WHERE ScheduleId = @ScheduleId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ScheduleId", appointment.ScheduleId);
                cmd.Parameters.AddWithValue("@IsActive", appointment.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedBy", appointment.UpdatedBy);
                cmd.Parameters.AddWithValue("@UpdatedAt", appointment.UpdatedAt.ToString("F"));

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deactivating appointment.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }
    }
}
