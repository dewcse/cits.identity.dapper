using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SqlClient;
using System;
using Dapper;
using CITS.Identity.Dapper;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace CITS.Identity.Dapper.Data
{ 
    public class DapperUsersTable
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        public DapperUsersTable(IDatabaseConnectionFactory databaseConnectionFactory) => _databaseConnectionFactory = databaseConnectionFactory;

        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            //const string command = "INSERT INTO \"IdentityUsers\" " +
            //                       "VALUES (@Id, @UserName, @FirstName, @LastName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp, " +
            //                               "@PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnabled);";

            string rowsInserted;

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                user.LockoutEnd = DateTime.Today;

                rowsInserted = await connection.QuerySingleAsync<string>(
                            "INSERT INTO \"IdentityUsers\" " +
                            "VALUES (@UserName, @FirstName, @LastName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed, @PasswordHash, @SecurityStamp, @ConcurrencyStamp, " +
                            "@PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @LockoutEnabled) RETURNING \"Id\"",
                                   user);

                //rowsInserted = await connection.ExecuteAsync(command, new
                //{
                //    user.Id,
                //    user.UserName,
                //    user.FirstName,
                //    user.LastName,
                //    user.NormalizedUserName,
                //    user.Email,
                //    user.NormalizedEmail,
                //    user.EmailConfirmed,
                //    user.PasswordHash,
                //    user.SecurityStamp,
                //    user.ConcurrencyStamp,
                //    user.PhoneNumber,
                //    user.PhoneNumberConfirmed,
                //    user.TwoFactorEnabled,
                //    //user.LockoutEnd,
                //    user.LockoutEnabled
                //    //user.AccessFailedCount
                //});
            }

            //return rowsInserted;

            return rowsInserted != null ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
            {
                Code = nameof(CreateAsync),
                Description = $"User with email {user.Email} could not be inserted."
            });
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user)
        {
            const string command = "DELETE " +
                                    "FROM \"IdentityUsers\"" +
                                   "WHERE \"Id\" = @Id;";

            int rowsDeleted;

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                rowsDeleted = await connection.ExecuteAsync(command, new
                {
                    user.Id
                });
            }

            return rowsDeleted == 1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError
            {
                Code = nameof(DeleteAsync),
                Description = $"User with email {user.Email} could not be deleted."
            });
        }

        public async Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            const string command = "SELECT * FROM \"IdentityUsers\" WHERE \"Id\" = @Id;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                return await connection.QuerySingleOrDefaultAsync<IdentityUser>(command, new
                {
                    Id = userId
                });
            }
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName)
        {
            string sql = "SELECT * FROM \"IdentityUsers\" WHERE \"NormalizedUserName\" = @NormalizedUserName;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                try
                {
                    return await connection.QueryFirstOrDefaultAsync<IdentityUser>(sql,
                               new { NormalizedUserName = normalizedUserName });
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail)
        {
            const string command = "SELECT * FROM \"IdentityUsers\" WHERE \"NormalizedEmail\" = @NormalizedEmail;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                return await connection.QuerySingleOrDefaultAsync<IdentityUser>(command, new
                {
                    NormalizedEmail = normalizedEmail
                });
            }
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user)
        {
            // The implementation here might look a little strange, however there is a reason for this.
            // ASP.NET Core Identity stores follow a UOW (Unit of Work) pattern which practically means that when an operation is called it does not necessarily commits to the database.
            // It tracks the changes made and finally commits to the database. UserStore methods just manipulates the user and only CreateAsync, UpdateAsync and DeleteAsync of IUserStore<>
            // write to the database. This makes sense because this way we avoid connection to the database all the time and also we can commit all changes at once by using a transaction.
            const string updateUserCommand =
                "UPDATE \"IdentityUsers\"" +
                "SET \"UserName\" = @UserName, \"NormalizedUserName\" = @NormalizedUserName, \"Email\" = @Email, \"NormalizedEmail\" = @NormalizedEmail, \"EmailConfirmed\" = @EmailConfirmed, " +
                    "\"PasswordHash\" = @PasswordHash, \"SecurityStamp\" = @SecurityStamp, \"ConcurrencyStamp\" = @ConcurrencyStamp, \"PhoneNumber\" = @PhoneNumber, " +
                    "\"PhoneNumberConfirmed\" = @PhoneNumberConfirmed, \"TwoFactorEnabled\" = @TwoFactorEnabled, \"LockoutEnd\" = @LockoutEnd, \"LockoutEnabled\" = @LockoutEnabled, " +
                    "\"AccessFailedCount\" = @AccessFailedCount " +
                "WHERE \"Id\" = @Id;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    await connection.ExecuteAsync(updateUserCommand, new
                    {
                        user.UserName,
                        user.NormalizedUserName,
                        user.Email,
                        user.NormalizedEmail,
                        user.EmailConfirmed,
                        user.PasswordHash,
                        user.SecurityStamp,
                        user.ConcurrencyStamp,
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed,
                        user.TwoFactorEnabled,
                        user.LockoutEnd,
                        user.LockoutEnabled,
                        user.AccessFailedCount,
                        user.Id
                    }, transaction);

                    //if (user.Claims?.Count() > 0)
                    //{
                    //    const string deleteClaimsCommand = "DELETE " +
                    //                                       "FROM \"IdentityUserClaims\" " +
                    //                                       "WHERE \"UserId\" = @UserId;";

                    //    await connection.ExecuteAsync(deleteClaimsCommand, new
                    //    {
                    //        UserId = user.Id
                    //    }, transaction);

                    //    const string insertClaimsCommand = "INSERT INTO \"IdentityUserClaims\" (\"UserId\", \"ClaimType\", \"ClaimValue\") " +
                    //                                       "VALUES (@UserId, @ClaimType, @ClaimValue);";

                    //    await connection.ExecuteAsync(insertClaimsCommand, user.Claims.Select(x => new {
                    //        UserId = user.Id,
                    //        ClaimType = x.Type,
                    //        ClaimValue = x.Value
                    //    }), transaction);
                    //}

                    //if (user.Logins?.Count() > 0)
                    //{
                    //    const string deleteLoginsCommand = "DELETE " +
                    //                                       "FROM \"IdentityUserLogins\" " +
                    //                                       "WHERE \"UserId\" = @UserId;";

                    //    await connection.ExecuteAsync(deleteLoginsCommand, new
                    //    {
                    //        UserId = user.Id
                    //    }, transaction);

                    //    const string insertLoginsCommand = "INSERT INTO \"IdentityUserLogins\" (\"LoginProvider\", \"ProviderKey\", \"ProviderDisplayName\", \"UserId\") " +
                    //                                       "VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId);";

                    //    await connection.ExecuteAsync(insertLoginsCommand, user.Logins.Select(x => new {
                    //        x.LoginProvider,
                    //        x.ProviderKey,
                    //        x.ProviderDisplayName,
                    //        UserId = user.Id
                    //    }), transaction);
                    //}

                    //if (user.Roles?.Count() > 0)
                    //{
                    //    const string deleteRolesCommand = "DELETE " +
                    //                                      "FROM \"IdentityUserRoles\" " +
                    //                                      "WHERE \"UserId\" = @UserId;";

                    //    await connection.ExecuteAsync(deleteRolesCommand, new
                    //    {
                    //        UserId = user.Id
                    //    }, transaction);

                    //    const string insertRolesCommand = "INSERT INTO \"IdentityUserRoles\" (\"UserId\", \"RoleId\") " +
                    //                                      "VALUES (@UserId, @RoleId);";

                    //    await connection.ExecuteAsync(insertRolesCommand, user.Roles.Select(x => new {
                    //        UserId = user.Id,
                    //        x.RoleId
                    //    }), transaction);
                    //}

                    //if (user.Tokens?.Count() > 0)
                    //{
                    //    const string deleteTokensCommand = "DELETE " +
                    //                                       "FROM \"IdentityUserTokens\" " +
                    //                                       "WHERE \"UserId\" = @UserId;";

                    //    await connection.ExecuteAsync(deleteTokensCommand, new
                    //    {
                    //        UserId = user.Id
                    //    }, transaction);

                    //    const string insertTokensCommand = "INSERT INTO \"IdentityUserTokens\" (\"UserId\", \"LoginProvider\", \"Name\", \"Value\") " +
                    //                                       "VALUES (@UserId, @LoginProvider, @Name, @Value);";

                    //    await connection.ExecuteAsync(insertTokensCommand, user.Tokens.Select(x => new {
                    //        x.UserId,
                    //        x.LoginProvider,
                    //        x.Name,
                    //        x.Value
                    //    }), transaction);
                    //}

                    try
                    {
                        transaction.Commit();
                    }
                    catch
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                            return IdentityResult.Failed(new IdentityError
                            {
                                Code = nameof(UpdateAsync),
                                Description = $"User with email {user.Email} could not be updated. Operation could not be rolled back."
                            });
                        }

                        return IdentityResult.Failed(new IdentityError
                        {
                            Code = nameof(UpdateAsync),
                            Description = $"User with email {user.Email} could not be updated. Operation was rolled back."
                        });
                    }
                }
            }

            return IdentityResult.Success;
        }

        public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName)
        {
            const string command = "SELECT * " +
                                   "FROM \"IdentityUsers\" AS u " +
                                   "INNER JOIN \"IdentityUserRoles\" AS ur ON \"u.Id\" = \"ur.UserId\" " +
                                   "INNER JOIN \"IdentityRoles\" AS r ON \"ur.RoleId\" = \"r.Id\" " +
                                   "WHERE \"r.Name\" = @RoleName;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                return (await connection.QueryAsync<IdentityUser>(command, new
                {
                    RoleName = roleName
                })).ToList();
            }
        }

        public async Task<IList<IdentityUser>> GetUsersForClaimAsync(Claim claim)
        {
            const string command = "SELECT * " +
                                   "FROM \"IdentityUsers\" AS u " +
                                   "INNER JOIN \"IdentityUserClaims\" AS uc ON \"u.Id\" = \"uc.UserId\" " +
                                   "WHERE \"uc.ClaimType\" = @ClaimType AND \"uc.ClaimValue\" = @ClaimValue;";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                return (await connection.QueryAsync<IdentityUser>(command, new
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                })).ToList();
            }
        }

        public async Task<IEnumerable<IdentityUser>> GetAllUsers()
        {
            const string command = "SELECT * " +
                                   "FROM \"IdentityUsers\";";

            using (var connection = await _databaseConnectionFactory.CreateConnectionAsync())
            {
                return await connection.QueryAsync<IdentityUser>(command);
            }
        }
    }
}
