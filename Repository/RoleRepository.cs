using LiteClinic.Models;
using LiteClinic.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Repository
{
    public class RoleRepository
    {

        public async Task<List<RoleManager>> GetAllRolesAsync()
        {
            var roles = new List<RoleManager>();

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            SELECT RoleId, RoleName, CanManageUsers, CanAccessDashboard,
                   CanViewAppointments, CanEditRecords, IsDeactivated, CanManageReports,
                   CanViewReports, CanManageSettings, CanViewSettings, CanManageRecords, CanManageDoctors
            FROM RolesTable;
        ";

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    roles.Add(new RoleManager
                    {
                        RoleId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        RoleName = reader.IsDBNull(1) ? "NULL" : reader.GetString(1),
                        CanManageUsers = !reader.IsDBNull(2) && reader.GetInt32(2) == 1,
                        CanAccessDashboard = !reader.IsDBNull(3) && reader.GetInt32(3) == 1,
                        CanViewAppointments = !reader.IsDBNull(4) && reader.GetInt32(4) == 1,
                        CanEditRecords = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                        IsDeactivated = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                        CanManageReports = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        CanViewReports = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                        CanManageSettings = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
                        CanViewSettings = !reader.IsDBNull(10) && reader.GetInt32(10) == 1,
                        CanManageRecords = !reader.IsDBNull(11) && reader.GetInt32(11) == 1,
                        CanManageDoctors = !reader.IsDBNull(12) && reader.GetInt32(12) == 1,
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetAllRolesAsync");
            
            }

            return roles;
        }

        public RoleManager? GetRoleById(int roleID)
        {
            RoleManager? role = null;

            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                            SELECT 
                                   RoleId, RoleName, CanManageUsers, CanAccessDashboard,
                                   CanViewAppointments, CanEditRecords, IsDeactivated, CanManageReports,
                                   CanViewReports, CanManageSettings, CanViewSettings, CanManageRecords,
                                   CanManageDoctors
                            FROM RolesTable
                            WHERE RoleId = @RoleID;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RoleID", roleID);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    role = new RoleManager
                    {
                        RoleId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        RoleName = reader.IsDBNull(1) ? "NULL" : reader.GetString(1),
                        CanManageUsers = !reader.IsDBNull(2) && reader.GetInt32(2) == 1,
                        CanAccessDashboard = !reader.IsDBNull(3) && reader.GetInt32(3) == 1,
                        CanViewAppointments = !reader.IsDBNull(4) && reader.GetInt32(4) == 1,
                        CanEditRecords = !reader.IsDBNull(5) && reader.GetInt32(5) == 1,
                        IsDeactivated = !reader.IsDBNull(6) && reader.GetInt32(6) == 1,
                        CanManageReports = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        CanViewReports = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                        CanManageSettings = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
                        CanViewSettings = !reader.IsDBNull(10) && reader.GetInt32(10) == 1,
                        CanManageRecords = !reader.IsDBNull(11) && reader.GetInt32(11) == 1,
                        CanManageDoctors = !reader.IsDBNull(12) && reader.GetInt32(12) == 1,
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetRoleById");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return role;
        }

        public bool SaveRole(RoleManager role)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();

                using var transaction = conn.BeginTransaction();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT INTO RolesTable (
                    RoleName, CanManageUsers, CanAccessDashboard, CanViewAppointments,
                    CanEditRecords, IsDeactivated, CanManageReports, CanViewReports,
                    CanManageSettings, CanViewSettings, CanManageRecords, CanManageDoctors
                ) VALUES (
                    @RoleName, @CanManageUsers, @CanAccessDashboard, @CanViewAppointments,
                    @CanEditRecords, @IsDeactivated, @CanManageReports, @CanViewReports,
                    @CanManageSettings, @CanViewSettings, @CanManageRecords, @CanManageDoctors
                );";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.AddWithValue("@CanManageUsers", role.CanManageUsers ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanAccessDashboard", role.CanAccessDashboard ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewAppointments", role.CanViewAppointments ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanEditRecords", role.CanEditRecords ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsDeactivated", role.IsDeactivated ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageReports", role.CanManageReports ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewReports", role.CanViewReports ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageSettings", role.CanManageSettings ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewSettings", role.CanViewSettings ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageRecords", role.CanManageRecords ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageDoctors", role.CanManageDoctors ? 1 : 0);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "SaveRole");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool UpdateRole(RoleManager role)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();

                using var transaction = conn.BeginTransaction();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                UPDATE RolesTable SET
                    RoleName = @RoleName,
                    CanManageUsers = @CanManageUsers,
                    CanAccessDashboard = @CanAccessDashboard,
                    CanViewAppointments = @CanViewAppointments,
                    CanEditRecords = @CanEditRecords,
                    IsDeactivated = @IsDeactivated,
                    CanManageReports = @CanManageReports,
                    CanViewReports = @CanViewReports,
                    CanManageSettings = @CanManageSettings,
                    CanViewSettings = @CanViewSettings,
                    CanManageRecords = @CanManageRecords,
                    CanManageDoctors = @CanManageDoctors
                WHERE RoleId = @RoleId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.AddWithValue("@CanManageUsers", role.CanManageUsers ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanAccessDashboard", role.CanAccessDashboard ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewAppointments", role.CanViewAppointments ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanEditRecords", role.CanEditRecords ? 1 : 0);
                cmd.Parameters.AddWithValue("@IsDeactivated", role.IsDeactivated ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageReports", role.CanManageReports ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewReports", role.CanViewReports ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageSettings", role.CanManageSettings ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanViewSettings", role.CanViewSettings ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageRecords", role.CanManageRecords ? 1 : 0);
                cmd.Parameters.AddWithValue("@CanManageDoctors", role.CanManageDoctors ? 1 : 0);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UpdateRole");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public bool ToggleDeactivation(int roleId, bool isCurrentlyDeactivated)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();

                using var transaction = conn.BeginTransaction();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                UPDATE RolesTable SET
                    IsDeactivated = @IsDeactivated
                WHERE RoleId = @RoleId;";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@IsDeactivated", isCurrentlyDeactivated ? 0 : 1);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ToggleDeactivation");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

    }

}

