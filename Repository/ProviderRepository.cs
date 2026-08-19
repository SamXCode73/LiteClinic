using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Repository
{
    public class ProviderRepository
    {
        public List<PatientServiceIds> GetAllPatientProvider()
        {
            var patientServiceIds = new List<PatientServiceIds>();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT 
                PatientServiceId,
                PatientId,
                PatientIdText,
                ServiceName,
                ServiceId,
                IsActive,
                NotifyEn,
                NotifyAr,
                NotifyFr
            FROM PatientServiceIds;";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    patientServiceIds.Add(new PatientServiceIds
                    {
                        PatientServiceId = reader.GetInt32(0),
                        PatientAutoId = reader.GetInt32(1),
                        PatientIdText = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ServiceName = (ProviderType)reader.GetInt32(3),   // enum stored as int
                        ServiceId = reader.IsDBNull(4) ? null : reader.GetString(4),
                        IsActive = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                        NotifyEn = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                        NotifyAr = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        NotifyFr = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving Service IDs.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return patientServiceIds;
        }

        public List<PatientServiceIdsDisplay> GetAllPatientWithServicesDisplay()
        {
            var patientWithServices = new List<PatientServiceIdsDisplay>();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
            SELECT 
                PatientServiceId,
                PatientAutoId,
                PatientId,
                ServiceName,
                ServiceId,
                IsActive,
                NotifyEn,
                NotifyAr,
                NotifyFr,
                AddedByUser,
                AddedAt,
                UpdatedByUser,
                UpdatedAt
            FROM ViewPatientWithServices;";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    patientWithServices.Add(new PatientServiceIdsDisplay
                    {
                        PatientServiceId = reader.GetInt32(0),
                        PatientAutoId = reader.GetInt32(1),
                        PatientId = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ServiceName = (ProviderType)reader.GetInt32(3),   // enum stored as int
                        ServiceId = reader.IsDBNull(4) ? null : reader.GetString(4), //Chat ID or Service ID
                        IsActive = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                        NotifyEn = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                        NotifyAr = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        NotifyFr = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                        AddedByUser = reader.GetString(9),
                        AddedAt = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
                        UpdatedByUser = reader.IsDBNull(11) ? null : reader.GetString(11),
                        UpdatedAt = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12))
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving Patient With Services.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return patientWithServices;
        }



        public List<DoctorServiceIds> GetAllDoctorProvider()
        {
            var doctorServiceIds = new List<DoctorServiceIds>();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                                    SELECT 
                                        DoctorServiceId,
                                        DoctorId,
                                        DoctorCode,
                                        ServiceName,
                                        ServiceId,
                                        IsActive,
                                        NotifyEn,
                                        NotifyFr,
                                        NotifyAr
                                    FROM DoctorServiceIds;";

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    doctorServiceIds.Add(new DoctorServiceIds
                    {
                        DoctorServiceId = reader.GetInt32(0),
                        DoctorId = reader.GetInt32(1),
                        DoctorCode = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ServiceName = (ProviderType)reader.GetInt32(3),   // enum stored as int
                        ServiceId = reader.IsDBNull(4) ? null : reader.GetString(4),
                        IsActive = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                        NotifyEn = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                        NotifyFr = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        NotifyAr = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving Service IDs.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return doctorServiceIds;
        }

public List<DoctorServiceIdsDisplay> GetAllDoctorsWithServicesDisplay()
{
    var doctorWithServices = new List<DoctorServiceIdsDisplay>();

    try
    {
        using var conn = DatabaseHelper.GetConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT 
                DoctorServiceId,
                DoctorId,
                DoctorCode,
                ServiceName,
                ServiceId,
                IsActive,
                NotifyEn,
                NotifyFr,
                NotifyAr,
                AddedByUser,
                AddedAt,
                UpdatedByUser,
                UpdatedAt
            FROM ViewDoctorWithServices
            ORDER BY DoctorServiceId;";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            doctorWithServices.Add(new DoctorServiceIdsDisplay
            {
                DoctorServiceId = reader.GetInt32(0),
                DoctorAutoId    = reader.GetInt32(1),
                DoctorCodeText  = reader.IsDBNull(2) ? null : reader.GetString(2),
                ServiceName     = (ProviderType)reader.GetInt32(3),
                ServiceId       = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive        = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                NotifyEn        = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                NotifyFr        = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                NotifyAr        = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                AddedByUser     = reader.IsDBNull(9) ? null : reader.GetString(9),
                AddedAt         = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                UpdatedByUser   = reader.IsDBNull(11) ? null : reader.GetString(11),
                UpdatedAt       = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12)
            });
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error retrieving Doctor With Services.");
    }
    finally
    {
        DatabaseHelper.CloseConnection();
    }

    return doctorWithServices;
}

        public bool ApplyPatientService(PatientServiceIds patientServiceIds)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                                INSERT INTO PatientServiceIds
                                (PatientId, PatientIdText, ServiceName, ServiceId, IsActive, NotifyEn, NotifyAr, NotifyFr, AddedByUser, AddedAt)
                                VALUES
                                (@PatientId, @PatientIdText, @ServiceName, @ServiceId, @IsActive, @NotifyEn, @NotifyAr, @NotifyFr, @AddedByUser, @AddedAt);";

                cmd.Parameters.AddWithValue("@PatientId", patientServiceIds.PatientAutoId);
                cmd.Parameters.AddWithValue("@PatientIdText", patientServiceIds.PatientIdText ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ServiceName", (int)patientServiceIds.ServiceName); // enum → int
                cmd.Parameters.AddWithValue("@ServiceId", patientServiceIds.ServiceId);
                cmd.Parameters.AddWithValue("@IsActive", patientServiceIds.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyEn", patientServiceIds.NotifyEn ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyAr", patientServiceIds.NotifyAr ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyFr", patientServiceIds.NotifyFr ? 1 : 0);
                cmd.Parameters.AddWithValue("@AddedByUser", patientServiceIds.AddedByUser);                
                cmd.Parameters.AddWithValue("@AddedAt", patientServiceIds.AddedAt?.ToString("F"));

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving service ID for Patient.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool ApplyDocotrService(DoctorServiceIds doctorServiceIds)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                INSERT INTO DoctorServiceIds
                        (DoctorId, DoctorCode, ServiceName, ServiceId, IsActive, NotifyEn, NotifyAr, NotifyFr, AddedByUser, AddedAt)
                VALUES
                        (@DoctorId, @DoctorCode, @ServiceName, @ServiceId, @IsActive, @NotifyEn, @NotifyAr, @NotifyFr, @AddedByUser, @AddedAt);";

                cmd.Parameters.AddWithValue("@DoctorId", doctorServiceIds.DoctorId);
                cmd.Parameters.AddWithValue("@DoctorCode", doctorServiceIds.DoctorCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ServiceName", (int)doctorServiceIds.ServiceName); // enum → int
                cmd.Parameters.AddWithValue("@ServiceId", doctorServiceIds.ServiceId);
                cmd.Parameters.AddWithValue("@IsActive", doctorServiceIds.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyEn", doctorServiceIds.NotifyEn ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyAr", doctorServiceIds.NotifyAr ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyFr", doctorServiceIds.NotifyFr ? 1 : 0);
                cmd.Parameters.AddWithValue("@AddedByUser", doctorServiceIds.AddedByUser); // string (window user = currentr user)
                cmd.Parameters.AddWithValue("@AddedAt", doctorServiceIds.AddedAt?.ToString("F")); // Full Long date

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving service ID for Doctor.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }


        public bool UpdatePatientService(PatientServiceIds patientServiceIds)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                                    UPDATE PatientServiceIds
                                    SET
                                        PatientId     = @PatientId,
                                        PatientIdText = @PatientIdText,
                                        ServiceName   = @ServiceName,
                                        ServiceId     = @ServiceId,
                                        IsActive      = @IsActive,
                                        NotifyEn      = @NotifyEn, 
	                                    NotifyAr      = @NotifyAr,
                                        NotifyFr      = @NotifyFr,
                                        UpdatedByUser = @UpdatedByUser,
                                        UpdatedAt     = @UpdatedAt
                                    WHERE PatientServiceId = @PatientServiceId;";

                cmd.Parameters.AddWithValue("@PatientServiceId", patientServiceIds.PatientServiceId);
                cmd.Parameters.AddWithValue("@PatientId", patientServiceIds.PatientAutoId);
                cmd.Parameters.AddWithValue("@PatientIdText", patientServiceIds.PatientIdText ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ServiceName", (int)patientServiceIds.ServiceName); // enum → int
                cmd.Parameters.AddWithValue("@ServiceId", patientServiceIds.ServiceId);
                cmd.Parameters.AddWithValue("@IsActive", patientServiceIds.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyEn", patientServiceIds.NotifyEn ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyAr", patientServiceIds.NotifyAr ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyFr", patientServiceIds.NotifyFr ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedByUser", patientServiceIds.UpdatedByUser ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAt", patientServiceIds.UpdatedAt?.ToString("F") ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating Servie ID for Patient.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdateDoctorService(DoctorServiceIds doctorServiceIds)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                            UPDATE DoctorServiceIds
                            SET
                                DoctorId      = @DoctorId,
                                DoctorCode    = @DoctorCode,
                                ServiceName   = @ServiceName,
                                ServiceId     = @ServiceId,
                                IsActive      = @IsActive,
                                NotifyEn      = @NotifyEn, 
	                            NotifyAr      = @NotifyAr,
	                            NotifyFr      = @NotifyFr,
                                UpdatedByUser = @UpdatedByUser,
                                UpdatedAt     = @UpdatedAt
                            WHERE DoctorServiceId = @DoctorServiceId;";

                cmd.Parameters.AddWithValue("@DoctorId", doctorServiceIds.DoctorId);
                cmd.Parameters.AddWithValue("@DoctorCode", doctorServiceIds.DoctorCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ServiceName", (int)doctorServiceIds.ServiceName); // enum → int
                cmd.Parameters.AddWithValue("@ServiceId", doctorServiceIds.ServiceId);
                cmd.Parameters.AddWithValue("@IsActive", doctorServiceIds.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyEn", doctorServiceIds.NotifyEn ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyAr", doctorServiceIds.NotifyAr ? 1 : 0);
                cmd.Parameters.AddWithValue("@NotifyFr", doctorServiceIds.NotifyFr ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedByUser", doctorServiceIds.UpdatedByUser ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedAt", doctorServiceIds.UpdatedAt?.ToString("F") ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DoctorServiceId", doctorServiceIds.DoctorServiceId);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating service ID for doctor.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

    }
}
