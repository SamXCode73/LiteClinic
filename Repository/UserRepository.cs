using LiteClinic.Models;
using LiteClinic.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Repository
{
    public class UserRepository
    {
        public List<UserModel> GetAllUsers()
        {
            var users = new List<UserModel>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT UserAutoId, UserId, Username, PasswordHash, FullName, RoleId, Email, 
PhoneNumber, LandLineNumber, Language, IsActive, CreatedAt, UpdatedAt,
UpdatedBy FROM UserTable;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add (new UserModel
                    {
                        UserAutoId = reader.GetInt32(0),
                        UserId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                        PasswordHash = reader.IsDBNull(3) ? null : reader.GetString(3),
                        FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        RoleId = reader.GetInt32(5),
                        Email = reader.IsDBNull(6) ? null : reader.GetString(6),
                        PhoneNumber = reader.IsDBNull(7) ? null : reader.GetString(7),
                        LandLineNumber = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Language = reader.IsDBNull(9) ? "en-US" : reader.GetString(9),
                        IsActive = reader.GetBoolean(10),
                        CreatedAt = reader.IsDBNull(11) ? null : (DateTime?)reader.GetDateTime(11),
                        UpdatedAt = reader.IsDBNull(12) ? null : (DateTime?)reader.GetDateTime(12),
                        UpdatedBy = reader.IsDBNull(13) ? null : reader.GetString(13)
                    });
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Logger.LogError(ex, "Error retrieving users from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
            return users;
        }

        //public List<UserWithRole> GetAllUsersForAuthentication()
        //{
        //    var users = new List<UserWithRole>();
        //    try
        //    {
        //        using var conn = DatabaseHelper.GetConnection();
        //        conn.Open();
        //        using var cmd = conn.CreateCommand();
        //        cmd.CommandText = @"
        //    SELECT 
        //        UserAutoId,
        //        Username,
        //        PasswordHash,
        //        RoleId,
        //        FullName,
        //        Email,
        //        RoleName,
        //        CanManageUsers,
        //        CanAccessDashboard,
        //        CanViewAppointments,
        //        CanEditRecords,
        //        IsDeactivated,
        //        CanManageReports,
        //        CanViewReports,
        //        CanManageSettings,
        //        CanViewSettings,
        //        CanManageRecords,
        //        CanManageDoctors
        //    FROM UserWithRolesView;
        //";

        //        using var reader = cmd.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            var user = new UserModel
        //            {
        //                UserAutoId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
        //                Username = reader.IsDBNull(1) ? null : reader.GetString(1),
        //                PasswordHash = reader.IsDBNull(2) ? null : reader.GetString(2),
        //                RoleId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
        //                FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
        //                Email = reader.IsDBNull(5) ? null : reader.GetString(5)
        //            };

        //            var role = new RoleModel
        //            {
        //                RoleId = user.RoleId,
        //                RoleName = reader.IsDBNull(6) ? null : reader.GetString(6),
        //                CanManageUsers = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
        //                CanAccessDashboard = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
        //                CanViewAppointments = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
        //                CanEditRecords = !reader.IsDBNull(10) && reader.GetInt32(10) == 1,
        //                IsDeactivated = !reader.IsDBNull(11) && reader.GetInt32(11) == 1,
        //                CanManageReports = !reader.IsDBNull(12) && reader.GetInt32(12) == 1,
        //                CanViewReports = !reader.IsDBNull(13) && reader.GetInt32(13) == 1,
        //                CanManageSettings = !reader.IsDBNull(14) && reader.GetInt32(14) == 1,
        //                CanViewSettings = !reader.IsDBNull(15) && reader.GetInt32(15) == 1,
        //                CanManageRecords = !reader.IsDBNull(16) && reader.GetInt32(16) == 1,
        //                CanManageDoctors = !reader.IsDBNull(17) && reader.GetInt32(17) == 1
        //            };

        //            users.Add(new UserWithRole { User = user, Role = role });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, "Error retrieving users with roles from the database.");
        //    }
        //    finally
        //    {
        //        DatabaseHelper.CloseConnection();
        //    }
        //    return users;
        //}

        public async Task<List<UserWithRole>> GetAllUsersForAuthenticationAsync()
        {
            var users = new List<UserWithRole>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            SELECT 
                UserAutoId,
                Username,
                PasswordHash,
                RoleId,
                FullName,
                Email,
                RoleName,
                CanManageUsers,
                CanAccessDashboard,
                CanViewAppointments,
                CanEditRecords,
                IsDeactivated,
                CanManageReports,
                CanViewReports,
                CanManageSettings,
                CanViewSettings,
                CanManageRecords,
                CanManageDoctors
            FROM UserWithRolesView;
        ";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new UserModel
                    {
                        UserAutoId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        Username = reader.IsDBNull(1) ? null : reader.GetString(1),
                        PasswordHash = reader.IsDBNull(2) ? null : reader.GetString(2),
                        RoleId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        FullName = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Email = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };

                    var role = new RoleModel
                    {
                        RoleId = user.RoleId,
                        RoleName = reader.IsDBNull(6) ? null : reader.GetString(6),
                        CanManageUsers = !reader.IsDBNull(7) && reader.GetInt32(7) == 1,
                        CanAccessDashboard = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                        CanViewAppointments = !reader.IsDBNull(9) && reader.GetInt32(9) == 1,
                        CanEditRecords = !reader.IsDBNull(10) && reader.GetInt32(10) == 1,
                        IsDeactivated = !reader.IsDBNull(11) && reader.GetInt32(11) == 1,
                        CanManageReports = !reader.IsDBNull(12) && reader.GetInt32(12) == 1,
                        CanViewReports = !reader.IsDBNull(13) && reader.GetInt32(13) == 1,
                        CanManageSettings = !reader.IsDBNull(14) && reader.GetInt32(14) == 1,
                        CanViewSettings = !reader.IsDBNull(15) && reader.GetInt32(15) == 1,
                        CanManageRecords = !reader.IsDBNull(16) && reader.GetInt32(16) == 1,
                        CanManageDoctors = !reader.IsDBNull(17) && reader.GetInt32(17) == 1
                    };

                    users.Add(new UserWithRole { User = user, Role = role });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving users with roles from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
            return users;
        }

        public bool SaveUser(UserModel user)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            INSERT INTO UserTable (
                UserId, Username, PasswordHash, FullName, RoleId, Email,
                PhoneNumber, LandLineNumber, Language, IsActive, CreatedAt, UpdatedBy
            ) VALUES (
                @UserId, @Username, @PasswordHash, @FullName, @RoleId, @Email,
                @PhoneNumber, @LandLineNumber, @Language, @IsActive, @CreatedAt, @UpdatedBy
            );";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? "");
                cmd.Parameters.AddWithValue("@LandLineNumber", user.LandLineNumber ?? "");
                cmd.Parameters.AddWithValue("@Language", user.Language ?? "en");
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("F"));
                cmd.Parameters.AddWithValue("@UpdatedBy", user.UpdatedBy ?? "");

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving user.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }


        public bool UpdateUser(UserModel user)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Parameters.Clear();
                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    cmd.CommandText = @"
        UPDATE UserTable SET
            Username = @Username,
            PasswordHash = @PasswordHash,
            FullName = @FullName,
            RoleId = @RoleId,
            Email = @Email,
            PhoneNumber = @PhoneNumber,
            LandLineNumber = @LandLineNumber,
            Language = @Language,
            UpdatedAt = @UpdatedAt,
            UpdatedBy = @UpdatedBy
        WHERE UserAutoId = @UserAutoId;";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@UserAutoId", user.UserAutoId);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? "");
                    cmd.Parameters.AddWithValue("@LandLineNumber", user.LandLineNumber ?? "");
                    cmd.Parameters.AddWithValue("@Language", user.Language);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UpdatedBy ?? "");
                }
                else
                {
                    cmd.CommandText = @"
        UPDATE UserTable SET
            Username = @Username,
            FullName = @FullName,
            RoleId = @RoleId,
            Email = @Email,
            PhoneNumber = @PhoneNumber,
            LandLineNumber = @LandLineNumber,
            Language = @Language,
            IsActive = @IsActive,
            UpdatedAt = @UpdatedAt,
            UpdatedBy = @UpdatedBy
        WHERE UserAutoId = @UserAutoId;";

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserAutoId", user.UserAutoId);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                    cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
                    cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? "");
                    cmd.Parameters.AddWithValue("@LandLineNumber", user.LandLineNumber ?? "");
                    cmd.Parameters.AddWithValue("@Language", user.Language);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
                    cmd.Parameters.AddWithValue("@UpdatedBy", user.UpdatedBy ?? "");
                }


                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating user.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }


        public bool DeactivateUser(UserModel user)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var transaction = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                        UPDATE UserTable SET
                            IsActive = @IsActive,
                            UpdatedAt = @UpdatedAt,
                            UpdatedBy = @UpdatedBy
                        WHERE UserAutoId = @UserAutoId;";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UserAutoId", user.UserAutoId);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("F"));
                cmd.Parameters.AddWithValue("@UpdatedBy", user.UpdatedBy);

                cmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deactivating user in Database.");
                return false;
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }
        }

        public List<UserModel> GetAllUsersWithRoles()
        {

            var users = new List<UserModel>();
            try {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM UserWithRoleView;";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var user = new UserModel
                        {
                            UserAutoId = reader.GetInt32(0),
                            UserId = reader.GetString(1),
                            Username = reader.GetString(2),
                            FullName = reader.GetString(3),
                            Email = reader.GetString(4),
                            PhoneNumber = reader.GetString(5),
                            LandLineNumber = reader.GetString(6),
                            Language = reader.GetString(7),
                            RoleId = reader.GetInt32(8),
                            RoleName = reader.GetString(9),
                            IsActive = reader.GetBoolean(10),
                            CreatedAt = reader.IsDBNull(11) ? null : (DateTime?)reader.GetDateTime(11),
                            UpdatedAt = reader.IsDBNull(12) ? null : (DateTime?)reader.GetDateTime(12),
                            UpdatedBy = reader.IsDBNull(13) ? null : reader.GetString(13)
                        };
                    users.Add(user);
                    }
                }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Logger.LogError(ex, "Error retrieving users with roles from the database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }                          
            return users;
        }

        public List<UsersWithRolesDisplay> GetUsersWithRoles()
        {
            var usersWithRoles = new List<UsersWithRolesDisplay>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT 
                                    UserAutoId, UserId, Username, FullName, Email,
                                    PhoneNumber, LandLineNumber, Language, IsActive,
                                    CreatedAt, UpdatedAt, UpdatedBy,
                                    RoleId, RoleName,
                                    CanManageUsers, CanAccessDashboard, CanViewAppointments,
                                    CanEditRecords, CanManageReports, CanViewReports,
                                    CanManageSettings, CanViewSettings
                                FROM UsersWithRolesView;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usersWithRoles.Add(new UsersWithRolesDisplay
                    {
                        UserAutoId = reader.GetInt32(0),
                        UserId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                        FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                        PhoneNumber = reader.IsDBNull(5) ? null : reader.GetString(5),
                        LandLineNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                        Language = reader.IsDBNull(7) ? "en-US" : reader.GetString(7),
                        IsActive = reader.GetBoolean(8),
                        CreatedAt = reader.IsDBNull(9) ? null : (DateTime?)reader.GetDateTime(9),
                        UpdatedAt = reader.IsDBNull(10) ? null : (DateTime?)reader.GetDateTime(10),
                        UpdatedBy = reader.IsDBNull(11) ? null : reader.GetString(11),
                        RoleId = reader.GetInt32(12),
                        RoleName = reader.IsDBNull(13) ? null : reader.GetString(13),
                        CanManageUsers = reader.GetBoolean(14),
                        CanAccessDashboard = reader.GetBoolean(15),
                        CanViewAppointments = reader.GetBoolean(16),
                        CanEditRecords = reader.GetBoolean(17),
                        CanManageReports = reader.GetBoolean(18),
                        CanViewReports = reader.GetBoolean(19),
                        CanManageSettings = reader.GetBoolean(20),
                        CanViewSettings = reader.GetBoolean(21)
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving UsersWithRolesView from database.");
            }
            finally
            {
                DatabaseHelper.CloseConnection();
            }

            return usersWithRoles;
        }

    }
}
