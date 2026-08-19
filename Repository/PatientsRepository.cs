using LiteClinic.Models;
using LiteClinic.Models.Enums;
using LiteClinic.Services;
using LiteClinic.Views;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.System;

namespace LiteClinic.Repository
{

    public class PatientsRepository
    {

        public List<PatientsModel> GetAllPatients()
        {
            var patients = new List<PatientsModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT PatientAutoId, PatientId, FirstName, MiddleName, LastName, FullMotherName,
                       CivilRecord, Gender, DateOfBirth, PatientAge, PhoneNumber, Email, Address, City, Country,
                       GotInsurance, InsuranceName, InsuranceNumber, GotNSN, NSNName, NSNNumber, BloodType,
                       Allergies, MedicalHistory, Language, IsActive, CreatedBy, CreatedAt, UpdatedAt, UpdatedBy
                FROM PatientTable;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    patients.Add(new PatientsModel
                    {
                        PatientAutoId = reader.GetInt32(0),
                        PatientId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        LastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        FullMotherName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CivilRecord = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Gender = reader.IsDBNull(7) ? null : reader.GetString(7),
                        DateOfBirth = reader.IsDBNull(8) ? null : reader.GetString(8),
                        PatientAge = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                        PhoneNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Email = reader.IsDBNull(11) ? null : reader.GetString(11),
                        Address = reader.IsDBNull(12) ? null : reader.GetString(12),
                        City = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Country = reader.IsDBNull(14) ? null : reader.GetString(14),
                        GotInsurance = !reader.IsDBNull(15) && reader.GetInt32(15) == 1,
                        InsuranceName = reader.IsDBNull(16) ? null : reader.GetString(15),
                        InsuranceNumber = reader.IsDBNull(17) ? null : reader.GetString(17),
                        GotNSN = !reader.IsDBNull(18) && reader.GetInt32(18) == 1,
                        NSNName = reader.IsDBNull(19) ? null : reader.GetString(19),
                        NSNNumber = reader.IsDBNull(20) ? null : reader.GetString(20),
                        BloodType = reader.IsDBNull(21) ? null : reader.GetString(21),
                        Allergies = reader.IsDBNull(22) ? null : reader.GetString(22),
                        MedicalHistory = reader.IsDBNull(23) ? null : reader.GetString(23),
                        Language = reader.IsDBNull(24) ? "en" : reader.GetString(24),
                        IsActive = !reader.IsDBNull(25) && reader.GetInt32(25) == 1,
                        CreatedBy = !reader.IsDBNull(26) ? null : reader.GetString(26),
                        CreatedAt = reader.IsDBNull(27) ? null : (DateTime?)reader.GetDateTime(27),
                        UpdatedAt = reader.IsDBNull(28) ? null : (DateTime?)reader.GetDateTime(28),
                        UpdatedBy = reader.IsDBNull(29) ? null : reader.GetString(29)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving patients from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return patients;
        }

        public List<PatientsModel> GetAllActivePatients()
        {
            var patients = new List<PatientsModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT PatientAutoId, PatientId, FirstName, MiddleName, LastName, FullMotherName,
                       CivilRecord, Gender, DateOfBirth, PatientAge, PhoneNumber, Email, Address, City, Country,
                       GotInsurance, InsuranceName, InsuranceNumber, GotNSN, NSNName, NSNNumber, BloodType,
                       Allergies, MedicalHistory, Language, IsActive, CreatedBy, CreatedAt, UpdatedAt, UpdatedBy
                FROM PatientTable
                WHERE 
                    IsActive = 1;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    patients.Add(new PatientsModel
                    {
                        PatientAutoId = reader.GetInt32(0),
                        PatientId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        FirstName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        LastName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        FullMotherName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CivilRecord = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Gender = reader.IsDBNull(7) ? null : reader.GetString(7),
                        DateOfBirth = reader.IsDBNull(8) ? null : reader.GetString(8),
                        PatientAge = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                        PhoneNumber = reader.IsDBNull(10) ? null : reader.GetString(10),
                        Email = reader.IsDBNull(11) ? null : reader.GetString(11),
                        Address = reader.IsDBNull(12) ? null : reader.GetString(12),
                        City = reader.IsDBNull(13) ? null : reader.GetString(13),
                        Country = reader.IsDBNull(14) ? null : reader.GetString(14),
                        GotInsurance = !reader.IsDBNull(15) && reader.GetInt32(15) == 1,
                        InsuranceName = reader.IsDBNull(16) ? null : reader.GetString(15),
                        InsuranceNumber = reader.IsDBNull(17) ? null : reader.GetString(17),
                        GotNSN = !reader.IsDBNull(18) && reader.GetInt32(18) == 1,
                        NSNName = reader.IsDBNull(19) ? null : reader.GetString(19),
                        NSNNumber = reader.IsDBNull(20) ? null : reader.GetString(20),
                        BloodType = reader.IsDBNull(21) ? null : reader.GetString(21),
                        Allergies = reader.IsDBNull(22) ? null : reader.GetString(22),
                        MedicalHistory = reader.IsDBNull(23) ? null : reader.GetString(23),
                        Language = reader.IsDBNull(24) ? "en" : reader.GetString(24),
                        IsActive = !reader.IsDBNull(25) && reader.GetInt32(25) == 1,
                        CreatedBy = !reader.IsDBNull(26) ? null : reader.GetString(26),
                        CreatedAt = reader.IsDBNull(27) ? null : (DateTime?)reader.GetDateTime(27),
                        UpdatedAt = reader.IsDBNull(28) ? null : (DateTime?)reader.GetDateTime(28),
                        UpdatedBy = reader.IsDBNull(29) ? null : reader.GetString(29)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving patients from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return patients;
        }

        public List<PatientsModel> GetActivePatientsForServiceCode()
        {
            var patients = new List<PatientsModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT PatientAutoId, PatientId, IsActive
                FROM PatientTable;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    patients.Add(new PatientsModel
                    {
                        PatientAutoId = reader.GetInt32(0),
                        PatientId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        IsActive = !reader.IsDBNull(2) && reader.GetInt32(2) == 1
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving patients from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return patients;
        }


        public bool SavePatient(PatientsModel patient)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                INSERT INTO PatientTable (
                    PatientId, FirstName, MiddleName, LastName, FullMotherName,
                    CivilRecord, Gender, DateOfBirth, PatientAge, PhoneNumber, Email, Address,
                    City, Country, GotInsurance, InsuranceName, InsuranceNumber, GotNSN, NSNName,
                    NSNNumber, BloodType, Allergies, MedicalHistory, Language,
                    IsActive, CreatedBy, CreatedAt, UpdatedBy
                ) VALUES (
                    @PatientId, @FirstName, @MiddleName, @LastName, @FullMotherName,
                    @CivilRecord, @Gender, @DateOfBirth, @PatientAge, @PhoneNumber, @Email, @Address,
                    @City, @Country, @GotInsurance, @InsuranceName, @InsuranceNumber, @GotNSN, @NSNName,
                    @NSNNumber, @BloodType, @Allergies, @MedicalHistory, @Language,
                    @IsActive, @CreatedBy, @CreatedAt, @UpdatedBy
                );";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@PatientId", patient.PatientId);
                cmd.Parameters.AddWithValue("@FirstName", patient.FirstName ?? "");
                cmd.Parameters.AddWithValue("@MiddleName", patient.MiddleName ?? "");
                cmd.Parameters.AddWithValue("@LastName", patient.LastName ?? "");
                cmd.Parameters.AddWithValue("@FullMotherName", patient.FullMotherName ?? "");
                cmd.Parameters.AddWithValue("@CivilRecord", patient.CivilRecord ?? "");
                cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? "");
                cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth ?? "");
                cmd.Parameters.AddWithValue("@PatientAge", patient.PatientAge);
                cmd.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@Email", patient.Email ?? "");
                cmd.Parameters.AddWithValue("@Address", patient.Address ?? "");
                cmd.Parameters.AddWithValue("@City", patient.City ?? "");
                cmd.Parameters.AddWithValue("@Country", patient.Country ?? "");
                cmd.Parameters.AddWithValue("@GotInsurance", patient.GotInsurance ? 1 : 0);
                cmd.Parameters.AddWithValue("@InsuranceName", patient.InsuranceName ?? "");
                cmd.Parameters.AddWithValue("@InsuranceNumber", patient.InsuranceNumber ?? "");
                cmd.Parameters.AddWithValue("@GotNSN", patient.GotNSN ? 1 : 0);
                cmd.Parameters.AddWithValue("@NSNName", patient.NSNName ?? "");
                cmd.Parameters.AddWithValue("@NSNNumber", patient.NSNNumber ?? "");
                cmd.Parameters.AddWithValue("@BloodType", patient.BloodType ?? "");
                cmd.Parameters.AddWithValue("@Allergies", patient.Allergies ?? "");
                cmd.Parameters.AddWithValue("@MedicalHistory", patient.MedicalHistory ?? "");
                cmd.Parameters.AddWithValue("@Language", patient.Language ?? "en");
                cmd.Parameters.AddWithValue("@IsActive", patient.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedBy", patient.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("F"));
                cmd.Parameters.AddWithValue("@UpdatedBy", patient.UpdatedBy ?? "");

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving patient.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdatePatient(PatientsModel patient)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                UPDATE PatientTable SET
                    FirstName = @FirstName,
                    MiddleName = @MiddleName,
                    LastName = @LastName,
                    FullMotherName = @FullMotherName,
                    CivilRecord = @CivilRecord,
                    Gender = @Gender,
                    DateOfBirth = @DateOfBirth,
                    PatientAge = @PatientAge,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email,
                    Address = @Address,
                    City = @City,
                    Country = @Country,
                    GotInsurance = @GotInsurance,
                    InsuranceName = @InsuranceName,                    
                    InsuranceNumber = @InsuranceNumber,
                    GotNSN = @GotNSN,
                    NSNName = @NSNName,
                    NSNNumber = @NSNNumber,
                    BloodType = @BloodType,
                    Allergies = @Allergies,
                    MedicalHistory = @MedicalHistory,
                    Language = @Language,
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE PatientAutoId = @PatientAutoId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@PatientAutoId", patient.PatientAutoId);
                cmd.Parameters.AddWithValue("@FirstName", patient.FirstName ?? "");
                cmd.Parameters.AddWithValue("@MiddleName", patient.MiddleName ?? "");
                cmd.Parameters.AddWithValue("@LastName", patient.LastName ?? "");
                cmd.Parameters.AddWithValue("@FullMotherName", patient.FullMotherName ?? "");
                cmd.Parameters.AddWithValue("@CivilRecord", patient.CivilRecord ?? "");
                cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? "");
                cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth ?? "");
                cmd.Parameters.AddWithValue("@PatientAge", patient.PatientAge);
                cmd.Parameters.AddWithValue("@PhoneNumber", patient.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@Email", patient.Email ?? "");
                cmd.Parameters.AddWithValue("@Address", patient.Address ?? "");
                cmd.Parameters.AddWithValue("@City", patient.City ?? "");
                cmd.Parameters.AddWithValue("@Country", patient.Country ?? "");
                cmd.Parameters.AddWithValue("@GotInsurance", patient.GotInsurance ? 1 : 0);
                cmd.Parameters.AddWithValue("@InsuranceName", patient.InsuranceName ?? "");
                cmd.Parameters.AddWithValue("@InsuranceNumber", patient.InsuranceNumber ?? "");
                cmd.Parameters.AddWithValue("@GotNSN", patient.GotNSN ? 1 : 0);
                cmd.Parameters.AddWithValue("@NSNName", patient.NSNName ?? "");
                cmd.Parameters.AddWithValue("@NSNNumber", patient.NSNNumber ?? "");
                cmd.Parameters.AddWithValue("@BloodType", patient.BloodType ?? "");
                cmd.Parameters.AddWithValue("@Allergies", patient.Allergies ?? "");
                cmd.Parameters.AddWithValue("@MedicalHistory", patient.MedicalHistory ?? "");
                cmd.Parameters.AddWithValue("@Language", patient.Language ?? "en");
                cmd.Parameters.AddWithValue("@IsActive", patient.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
                cmd.Parameters.AddWithValue("@UpdatedBy", patient.UpdatedBy ?? "");

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating patient.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool DeactivatePatient(PatientsModel patient)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
            UPDATE PatientTable
            SET IsActive = @IsActive,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE PatientAutoId = @PatientAutoId;";

                cmd.Parameters.Clear();                
                cmd.Parameters.AddWithValue("@IsActive", patient.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
                cmd.Parameters.AddWithValue("@UpdatedBy", patient.UpdatedBy);
                cmd.Parameters.AddWithValue("@PatientAutoId", patient.PatientAutoId);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deactivating patient.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        // Notification servicee
        public async Task<List<NotificationDataPatient>> GetAppointmentsWithServices()
        {
            var results = new List<NotificationDataPatient>();

            try

            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
        SELECT 
            ScheduleId,
            AppointmentID,
            AppointmentDate,
            AppointmentTime,
            AppointmentType,
            Notes,
            PatientAutoId,
            PatientExternalId,
            PatientFullName,
            Email,
            PhoneNumber,
            DoctorId,
            DoctorName,
            Specialty,
            ServiceName,
            ServiceId,
            ServiceIsActive,
            NotifyEn,
            NotifyAr,
            NotifyFr
        FROM ViewScheduledAppointmentsWithServices
        WHERE ServiceIsActive = 1
          AND DATE(AppointmentDate) BETWEEN DATE('now') AND DATE('now', '+1 day')
        ORDER BY AppointmentDate ASC, AppointmentTime ASC;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new NotificationDataPatient
                    {
                        ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                        AppointmentID = reader.GetInt32(reader.GetOrdinal("AppointmentID")),
                        AppointmentDate = DateTime.ParseExact(reader.GetString(
                            reader.GetOrdinal("AppointmentDate")), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        AppointmentTime = DateTime.ParseExact(
                            reader.GetString(reader.GetOrdinal("AppointmentTime")), "HH:mm", CultureInfo.InvariantCulture),
                        AppointmentType = reader.GetString(reader.GetOrdinal("AppointmentType")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("Notes")),
                        PatientAutoId = reader.GetInt32(reader.GetOrdinal("PatientAutoId")),
                        PatientExternalId = reader.GetString(reader.GetOrdinal("PatientExternalId")),
                        PatientFullName = reader.GetString(reader.GetOrdinal("PatientFullName")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("Email")),
                        PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                        DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                        DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
                        Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                        ServiceName = reader.GetInt32(reader.GetOrdinal("ServiceName")),
                        ServiceId = reader.IsDBNull(reader.GetOrdinal("ServiceId"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("ServiceId")),
                        ServiceIsActive = reader.GetInt32(reader.GetOrdinal("ServiceIsActive")) == 1,
                        NotifyEn = reader.GetInt32(reader.GetOrdinal("NotifyEn")) == 1,
                        NotifyAr = reader.GetInt32(reader.GetOrdinal("NotifyAr")) == 1,
                        NotifyFr = reader.GetInt32(reader.GetOrdinal("NotifyFr")) == 1
                    });
                }
            }
            catch (Exception ex)
            {
                string key = "ERROR_RETRIEVING_APPOINTMENTS";
                Logger.LogError(ex, $"{key} | Error retrieving appointments with services for notifications. | {this.GetType().Name}");
            }


#pragma warning disable CsWinRT1030 // Project does not enable unsafe blocks
            return results;
#pragma warning restore CsWinRT1030 // Project does not enable unsafe blocks
        }

    }
}