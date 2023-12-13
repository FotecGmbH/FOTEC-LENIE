// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biss.Apps.Base;
using Biss.Dc.Core;
using Biss.EMail;
using Biss.Log.Producer;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Helper;
using Exchange.Model.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>DcExPersons</para>
    ///     Klasse DcExPersons. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

        /// <summary>
        ///     Device fordert Daten für DcExUser
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public async Task<ExUser> GetDcExUser(long deviceId, long userId)
        {
            Logging.Log.LogTrace($"[GetDcExUser] DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[GetDcExUser] DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[GetDcExUser] UserId {userId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var u = db.GetUserWithdependences(userId);

            if (u == null)
            {
                throw new Exception($"[GetDcExUser] UserId {userId} does not exist in Database!");
            }

            return u.ToExUser();
        }

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcStoreResult> SetDcExUser(long deviceId, long userId, ExUser data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Logging.Log.LogTrace($"[SetDcExUser] DeviceId {deviceId} UserId {userId} Login {data.LoginName}");

            if (deviceId <= 0)
            {
                throw new Exception($"[SetDcExUser] DeviceId {deviceId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var un = data.LoginName.ToUpperInvariant();
            TableUser? dbUser;
            var newUser = false;
            var resendSms = false;

            //Neuer User?
            if (userId <= 0)
            {
                Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): Create New User");
                var checkUsers = db.TblUsers
                    .Select(s => new {s.LoginName, s.Id}).ToList();
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                checkUsers.ForEach(f => f.LoginName.ToUpperInvariant());
                var check = checkUsers.FirstOrDefault(c =>
                    (un.Contains('@', StringComparison.CurrentCulture) && !string.IsNullOrWhiteSpace(c.LoginName) && c.LoginName == un));
                Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): Search {un} in {string.Join(";", checkUsers.Select(x => x.Id + " - " + x.LoginName))}");

                if (check != null)
                {
                    Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): New but User exists!");
                    dbUser = db.GetUserWithdependences(userId, false);
                }
                else
                {
                    Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): New User");
                    newUser = true;
                    dbUser = new TableUser
                             {
                                 PasswordHash = data.PasswordHash4NewUser,
                                 LoginName = data.LoginName,
                                 PhoneNumber = data.PhoneNumber,
                                 DefaultLanguage = data.DefaultLanguage,
                                 LoginConfirmed = false,
                                 PhoneConfirmed = false,
                                 IsAdmin = false,
                                 AgbVersion = "1.0.0",
                                 CreatedAtUtc = DateTime.UtcNow,
                                 RefreshToken = AppCrypt.GeneratePassword(),
                                 JwtToken = AppCrypt.GeneratePassword(),
                                 ConfirmationToken = AppCrypt.GeneratePassword(),
                                 Locked = false,
                             };
                }
            }
            else
            {
                Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): Set Existing User");
                dbUser = db.GetUserWithdependences(userId, false);
            }

            if (dbUser == null)
            {
                throw new ArgumentException("[SetDcExUser] dbUser is null!");
            }

            dbUser.FirstName = data.FirstName;
            dbUser.LastName = data.LastName;

            if (dbUser.PhoneNumber != data.PhoneNumber)
            {
                dbUser.PhoneNumber = data.PhoneNumber;
                dbUser.PhoneConfirmed = false;
                resendSms = true;
            }

            dbUser.PushTags = data.PushTags;
            dbUser.Setting10MinPush = data.Setting10MinPush;
            dbUser.NotificationMailChat = data.NotificationMailChat;
            dbUser.NotificationMailMeeting = data.NotificationMailMeeting;
            dbUser.NotificationMailReport = data.NotificationMailReport;
            dbUser.NotificationPushChat = data.NotificationPushChat;
            dbUser.NotificationPushMeeting = data.NotificationPushMeeting;
            dbUser.NotificationPushLike = data.NotificationPushLike;
            dbUser.NotificationPushSupport = data.NotificationPushSupport;
            dbUser.NotificationPushComment = data.NotificationPushComment;
            dbUser.NotificationPushReport = data.NotificationPushReport;
            dbUser.NotificationPushIdea = data.NotificationPushIdea;

            dbUser.IsAdmin = data.IsSysAdmin;
            dbUser.Locked = data.Locked;

            //Gelöschte Token
            foreach (var l in dbUser.TblAccessToken.ToList())
            {
                if (!data.Tokens.Select(s => s.DbId).Contains(l.Id))
                {
                    db.TblAccessToken.Remove(l);
                }
            }

            //Neue Token
            foreach (var d in data.Tokens)
            {
                if (d.DbId <= 0)
                {
                    dbUser.TblAccessToken.Add(new TableAccessToken
                                              {
                                                  GuiltyUntilUtc = d.GuiltyUntilUtc,
                                                  TblUserId = dbUser.Id,
                                                  Token = d.Token
                                              });
                }
            }

            //Gelöschte Permissions
            if (dbUser.TblPermissions.Any())
            {
                foreach (var l in dbUser.TblPermissions.ToList())
                {
                    if (!data.Permissions.Select(s => s.CompanyId).Contains(l.TblOrganizationId))
                    {
                        db.TblPermissions.Remove(l);
                    }
                }
            }

            //Neue Permissions
            foreach (var d in data.Permissions)
            {
                if (d.DbId <= 0)
                {
                    if (d.Town != null)
                    {
                        var organisation = await db.TblOrganizations.FirstAsync(o => o.PostalCode == d.Town.PostalCode && o.Name == d.Town.Name).ConfigureAwait(true);
                        if (dbUser.TblPermissions.All(p => p.TblOrganization != organisation))
                        {
                            dbUser.TblPermissions.Add(new TablePermission
                                                      {
                                                          TblUserId = dbUser.Id,
                                                          TblOrganization = organisation,
                                                          UserRight = d.UserRight,
                                                          UserRole = d.UserRole,
                                                          MainOrganization = d.IsMainCompany,
                                                          TblUser = dbUser,
                                                      });
                        }
                    }
                }
            }

            // Permissions Aktualisieren 
            foreach (var d in data.Permissions)
            {
                if (d.DbId > 0)
                {
                    if (d.Town != null)
                    {
                        var permission = await db.TblPermissions.FirstOrDefaultAsync(x => x.Id == d.DbId).ConfigureAwait(true);
                        if (permission != null)
                        {
                            permission.UserRight = d.UserRight;
                            permission.UserRole = d.UserRole;
                            permission.MainOrganization = d.IsMainCompany;
                        }
                    }
                }
            }

            // Bild löschen
            if (dbUser.TblUserImageId.HasValue && dbUser.TblUserImageId.Value != data.UserImageDbId)
            {
                await FilesDbBlob.DeleteFile(db, dbUser.TblUserImageId.Value).ConfigureAwait(true);
                dbUser.TblUserImageId = null;
            }

            // Bild aktualisieren
            if (data.UserImageDbId > 0 && !dbUser.TblUserImageId.HasValue)
            {
                var image = await db.TblFiles.FirstOrDefaultAsync(i => i.Id == data.UserImageDbId).ConfigureAwait(true);
                if (image == null)
                {
                    throw new ArgumentException($"[SetDcExUser] Image with Id {data.UserImageDbId} not found in TableFiles!");
                }

                dbUser.TblUserImage = image;
            }

            if (newUser)
            {
                await db.TblUsers.AddAsync(dbUser).ConfigureAwait(true);
            }

            await db.SaveChangesAsync().ConfigureAwait(true);

            if (newUser || resendSms)
            {
                if (!string.IsNullOrWhiteSpace(dbUser.PhoneNumber) && PhoneNumberValidator.IsValid(dbUser.PhoneNumber))
                {
                    await SmsHelper.SendValidationSms(dbUser, db).ConfigureAwait(true);
                }
            }

            if (newUser)
            {
                if (!string.IsNullOrWhiteSpace(dbUser.LoginName) && Validator.Check(dbUser.LoginName))
                {
                    var email = new UserAccountEmailService(_razorEngine);
                    await email.SendValidationMail(dbUser, deviceId, db).ConfigureAwait(true);
                }
            }
            else
            {
                var data2Send = dbUser.ToExUser();
                await SendDcExUser(data2Send, userId: userId).ConfigureAwait(true);
            }

            //Listen mit Bezug zum Datensatz aktualisieren
            _ = Task.Run(async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                await SyncDcExIdeas(new List<long> {dbUser.Id}).ConfigureAwait(false);

                var u = db2.GetUserWithdependences(dbUser.Id);
                if (u == null!)
                {
                    throw new ArgumentNullException($"[{nameof(ServerRemoteCalls)}]({nameof(SetDcExUser)}): {nameof(u)}");
                }

                var users2Inform = new List<long>(db2.GetSysAdmins()) {userId};
                var userOrgs = u.TblPermissions.Select(s => s.TblOrganizationId).ToList();
                foreach (var org in userOrgs)
                {
                    users2Inform.AddRange(db2.GetOrgAdmins(org));
                }

                users2Inform = users2Inform.Distinct().ToList();
                await SyncDcExOrganization(users2Inform).ConfigureAwait(false);
            });

            return new DcStoreResult();
        }

        #endregion

        #region Passwort

        /// <summary>
        ///     Device fordert Daten für DcExUserPassword
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public Task<ExUserPassword> GetDcExUserPassword(long deviceId, long userId)
        {
            throw new InvalidOperationException("[GetDcExUserPassword] Can not read this data!");
        }

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcStoreResult> SetDcExUserPassword(long deviceId, long userId, ExUserPassword data)
        {
            Logging.Log.LogTrace($"[SetDcExUserPassword] DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[SetDcExUserPassword] DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[SetDcExUserPassword] UserId {userId} invalid!");
            }

            if (data == null!)
            {
                throw new ArgumentException("[SetDcExUserPassword] Data is null!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(true);
            if (dbUser == null)
            {
                throw new Exception($"[SetDcExUserPassword] UserId {userId} not found in Database!");
            }

            if (dbUser.PasswordHash != data.CurrentPasswordHash)
            {
                throw new Exception($"[SetDcExUserPassword] UserId {userId} Password does not match!");
            }

            dbUser.PasswordHash = data.NewPasswordHash;
            await db.SaveChangesAsync().ConfigureAwait(true);

            return new DcStoreResult();
        }

        #endregion
    }
}