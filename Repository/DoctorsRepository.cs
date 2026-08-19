using LiteClinic.Models;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.System;

namespace LiteClinic.Repository;

    public class DoctorsRepository
{

    public List<DoctorsModel> GetAllDoctors()
    {
        var doctors = new List<DoctorsModel>();
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT DoctorId, DoctorCode, FullName, Specialization, PhoneNumber, 
LandlineNumber, IsActive, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, Gender, ProfilePicturePath FROM Doctors;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctors.Add(new DoctorsModel
                {
                    DoctorId = reader.GetInt32(0),
                    DoctorCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                    FullName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LandLineNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetBoolean(6),
                    CreatedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CreatedAt = reader.IsDBNull(8) ? null : (DateTime?)reader.GetDateTime(8),
                    UpdatedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
                    UpdatedAt = reader.IsDBNull(10) ? null : (DateTime?)reader.GetDateTime(10),
                    Gender = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ProfilePicturePath = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving doctors from the database.");
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
        return doctors;
    }


    public List<DoctorDisplayModel> GetAllDisplayDoctorAsync()
    {
        var doctors = new List<DoctorDisplayModel>();
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT DoctorId, DoctorCode, FullName, Specialization, PhoneNumber, 
LandlineNumber, IsActive, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt, Gender, ProfilePicturePath FROM Doctors;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var fullName = reader.IsDBNull(2) ? null : reader.GetString(2);

                doctors.Add(new DoctorDisplayModel
                {
                    DoctorId = reader.GetInt32(0),
                    DoctorCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                    FullName = fullName,
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LandLineNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsActive = reader.GetBoolean(6),
                    CreatedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CreatedAt = reader.IsDBNull(8) ? null : (DateTime?)reader.GetDateTime(8),
                    UpdatedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
                    UpdatedAt = reader.IsDBNull(10) ? null : (DateTime?)reader.GetDateTime(10),
                    Gender = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ProfilePicturePath = reader.IsDBNull(12) ? null : reader.GetString(12),

                    // Compute initials directly here
                    Initials = ComputeInitials(fullName ?? "")
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving doctors from the database.");
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
        return doctors;
    }

    // Helper method
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


    public List<DoctorsModel> GetAllActiveDoctors()
    {
        var doctors = new List<DoctorsModel>();
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT DoctorId, DoctorCode, FullName, Specialization, PhoneNumber, 
                                FullNameWithSpecialty FROM DoctorsView;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctors.Add(new DoctorsModel
                {
                    DoctorId = reader.GetInt32(0),
                    DoctorCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                    FullName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Specialization = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving doctors from the database.");
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
        return doctors;
    }

    public List<DoctorsModel> GetActiveDoctorsForSericeCode()
    {
        var doctors = new List<DoctorsModel>();
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT 
                                    DoctorId, DoctorCode, IsActive
                                FROM Doctors;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                doctors.Add(new DoctorsModel
                {
                    DoctorId = reader.GetInt32(0),
                    DoctorCode = reader.IsDBNull(1) ? null : reader.GetString(1),
                    IsActive = !reader.IsDBNull(2) && reader.GetInt32(2) == 1
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving doctors from the database.");
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
        return doctors;
    }

    public List<DoctorScheduleViewRow> GetDoctorScheduleViewRows()
    {
        var list = new List<DoctorScheduleViewRow>();

        using var conn = DatabaseHelper.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"SELECT ScheduleAutoId, ScheduleId, DoctorId, DoctorCode, FullName, Specialization,
                   PhoneNumber, LandLineNumber, DayOfWeek, Notify, ScheduleIsActive,
                   DoctorIsActive, WeekNumbers
            FROM DoctorScheduleView;";

        conn.Open();

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            list.Add(new DoctorScheduleViewRow
            {
                ScheduleAutoId = rdr.GetInt32(0),
                ScheduleId = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                DoctorId = rdr.GetInt32(2),
                DoctorCode = rdr.IsDBNull(3) ? null : rdr.GetString(3),
                FullName = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                Specialization = rdr.IsDBNull(5) ? null : rdr.GetString(5),
                PhoneNumber = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                LandLineNumber = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                DayOfWeek = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                Notify = rdr.GetBoolean(9),
                ScheduleIsActive = rdr.GetBoolean(10),
                DoctorIsActive = rdr.GetBoolean(11),
                WeekNumbers = rdr.IsDBNull(12) ? null : rdr.GetString(12),
            });
        }

        return list;
    }


    public bool SaveDoctor(DoctorsModel doctor)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO Doctors (
            DoctorCode, FullName, Specialization, PhoneNumber, LandLineNumber,
            IsActive, CreatedBy, CreatedAt
        ) VALUES (
            @DoctorCode, @FullName, @Specialization, @PhoneNumber, @LandLineNumber,
            @IsActive, @CreatedBy, @CreatedAt
        );";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@DoctorCode", doctor.DoctorCode);
            cmd.Parameters.AddWithValue("@FullName", doctor.FullName ?? "");
            cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization ?? "");
            cmd.Parameters.AddWithValue("@PhoneNumber", doctor.PhoneNumber ?? "");
            cmd.Parameters.AddWithValue("@LandLineNumber", doctor.LandLineNumber ?? "");
            cmd.Parameters.AddWithValue("@IsActive", doctor.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@CreatedBy", doctor.CreatedBy ?? "");
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("F"));

            
            cmd.ExecuteNonQuery();
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving doctor.");
            return false;
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
    }

    public bool UpdateDoctor(DoctorsModel doctor)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        UPDATE Doctors SET
            FullName = @FullName,
            Specialization = @Specialization,
            Gender = @Gender,
            PhoneNumber = @PhoneNumber,
            LandLineNumber = @LandLineNumber,
            IsActive = @IsActive,
            UpdatedBy = @UpdatedBy,
            UpdatedAt = @UpdatedAt,
            ProfilePicturePath = @ProfilePicturePath
        WHERE DoctorId = @DoctorId;";

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@FullName", doctor.FullName ?? "");
            cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization ?? "");
            cmd.Parameters.AddWithValue("@Gender", doctor.Gender ?? ""); // added now
            cmd.Parameters.AddWithValue("@PhoneNumber", doctor.PhoneNumber ?? "");
            cmd.Parameters.AddWithValue("@LandLineNumber", doctor.LandLineNumber ?? "");
            cmd.Parameters.AddWithValue("@IsActive", doctor.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@UpdatedBy", doctor.UpdatedBy ?? "");
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
            cmd.Parameters.AddWithValue("@DoctorId", doctor.DoctorId);
            cmd.Parameters.AddWithValue("@ProfilePicturePath", doctor.ProfilePicturePath);
;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating doctor.");
            return false;
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
    }

    public bool DeactivateDoctor(DoctorsModel doctor)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var transaction = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        UPDATE Doctors SET
            IsActive = @IsActive,
            UpdatedAt = @UpdatedAt
        WHERE DoctorId = @DoctorId;";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@DoctorId", doctor.DoctorId);
            cmd.Parameters.AddWithValue("@IsActive", doctor.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
            cmd.Parameters.AddWithValue("@UpdatedBy", doctor.UpdatedBy ?? "");

            cmd.ExecuteNonQuery();
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deactivating doctor.");
            return false;
        }
        finally
        {
            DatabaseHelper.CloseConnection();
        }
    }

        // Notification Service
public async Task<List<NotificationDataDoctor>> GetSchedulesWithServices()
    {
        var results = new List<NotificationDataDoctor>();

        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
        SELECT 
            ScheduleAutoId,
            ScheduleId,
            DoctorId,
            DoctorCode,
            DoctorFullName,
            Specialization,
            PhoneNumber,
            LandLineNumber,
            DayOfWeek,
            Notify,
            ScheduleIsActive,
            DoctorIsActive,
            WeekNumbers,
            ServiceName,
            ServiceId,
            ServiceIsActive,
            NotifyEn,
            NotifyFr,
            NotifyAr,
            AddedByUser,
            AddedAt,
            UpdatedByUser,
            UpdatedAt,
            TimeFromTo
        FROM DoctorScheduleWithServices
        WHERE ServiceIsActive = 1
        ORDER BY DayOfWeek ASC, DoctorFullName ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // as TimeFromTo can be null, we need to handle it properly
                var value = reader.IsDBNull(reader.GetOrdinal("TimeFromTo"))
                    ? "Not Set": reader.GetString(reader.GetOrdinal("TimeFromTo"));
                results.Add(new NotificationDataDoctor
                {
                    ScheduleAutoId = reader.GetInt32(reader.GetOrdinal("ScheduleAutoId")),
                    ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                    DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    DoctorCode = reader.GetString(reader.GetOrdinal("DoctorCode")),
                    DoctorFullName = reader.GetString(reader.GetOrdinal("DoctorFullName")),
                    Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    LandLineNumber = reader.IsDBNull(reader.GetOrdinal("LandLineNumber"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("LandLineNumber")),
                    DayOfWeek = reader.GetString(reader.GetOrdinal("DayOfWeek")),
                    Notify = reader.GetInt32(reader.GetOrdinal("Notify")) == 1,
                    ScheduleIsActive = reader.GetInt32(reader.GetOrdinal("ScheduleIsActive")) == 1,
                    DoctorIsActive = reader.GetInt32(reader.GetOrdinal("DoctorIsActive")) == 1,
                    WeekNumbers = reader.IsDBNull(reader.GetOrdinal("WeekNumbers"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("WeekNumbers")),
                    ServiceName = reader.GetInt32(reader.GetOrdinal("ServiceName")),
                    ServiceId = reader.IsDBNull(reader.GetOrdinal("ServiceId"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("ServiceId")),
                    ServiceIsActive = reader.GetInt32(reader.GetOrdinal("ServiceIsActive")) == 1,
                    NotifyEn = reader.GetInt32(reader.GetOrdinal("NotifyEn")) == 1,
                    NotifyFr = reader.GetInt32(reader.GetOrdinal("NotifyFr")) == 1,
                    NotifyAr = reader.GetInt32(reader.GetOrdinal("NotifyAr")) == 1,
                    AddedByUser = reader.IsDBNull(reader.GetOrdinal("AddedByUser"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("AddedByUser")),
                    AddedAt = reader.IsDBNull(reader.GetOrdinal("AddedAt"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("AddedAt")),
                    UpdatedByUser = reader.IsDBNull(reader.GetOrdinal("UpdatedByUser"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("UpdatedByUser")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("UpdatedAt")),
                    TimeFromTo = string.Equals(value, "Not Set", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty : value

                });
            }
        }
        catch (Exception ex)
        {
            string key = "ERROR_RETRIEVING_DOCTOR_SCHEDULE";
            Logger.LogError(ex, $" {key} | Error retrieving doctor schedule with services from the database. | {GetType().Name}");
        }

#pragma warning disable CsWinRT1030 // Project does not enable unsafe blocks
        return results;
#pragma warning restore CsWinRT1030 // Project does not enable unsafe blocks
    }
}


