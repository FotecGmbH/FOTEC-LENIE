// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.EMail;
using Biss.Log.Producer;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Database.Tables;
using Exchange.Enum;
using Exchange.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Allgemeine "Remote" Funktionen via DC</para>
    ///     Klasse DcCommonData. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Datei löschen aus DB und aus Blob
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task DeleteFile(string data)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var fileId = long.Parse(data);
            await FilesDbBlob.DeleteFile(db, fileId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Bestätigungs E-Mail erneut senden
        /// </summary>
        /// <param name="data"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task ResendAccessEMail(string data, long deviceId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var userId = long.Parse(data);
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            if (dbUser != null && !string.IsNullOrWhiteSpace(dbUser.LoginName) && Validator.Check(dbUser.LoginName))
            {
                var email = new UserAccountEmailService(_razorEngine);
                await email.SendValidationMail(dbUser, deviceId, db).ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Bestätigungs Sms erneut senden
        /// </summary>
        /// <param name="data"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task ResendAccessSms(string data, long deviceId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var userId = long.Parse(data);
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(dbUser?.PhoneNumber) && !dbUser.PhoneConfirmed && PhoneNumberValidator.IsValid(dbUser.PhoneNumber))
            {
                await SmsHelper.SendValidationSms(dbUser, db).ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Bestätigung für Account
        /// </summary>
        /// <param name="text"></param>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task<bool> ConfirmSms(string text, long userId, long deviceId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            if (dbUser != null &&
                dbUser.PhoneConfirmed)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(dbUser?.SmsCode) &&
                !string.IsNullOrWhiteSpace(text) &&
                !dbUser.PhoneConfirmed &&
                dbUser.SmsCode == text)
            {
                dbUser.SmsCode = string.Empty;
                dbUser.PhoneConfirmed = true;

                try
                {
                    await db.SaveChangesAsync().ConfigureAwait(true);

                    var usr = await GetDcExUser(deviceId, userId).ConfigureAwait(true);
                    var send = await SendDcExUser(usr, null, userId).ConfigureAwait(true);

                    return true;
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(ServerRemoteCalls)}]({nameof(ConfirmSms)}): {userId} {text} - {e}");
                }
            }

            return false;
        }

        /// <summary>
        ///     Bestätigungs E-Mail erneut senden
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task ResetPassword(string data)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var userId = long.Parse(data);
            var dbUser = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(dbUser?.LoginName) && Validator.Check(dbUser.LoginName))
            {
                var email = new UserAccountEmailService(_razorEngine);
                await email.SendPasswordResetMail(dbUser!, db).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Pushnachricht senden
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task SendPush(string data)
        {
            await PushHelper.SendTestMessage(data).ConfigureAwait(false);
        }

        /// <summary>
        ///     Allgemeinen
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private async Task UpdateCommonInfo(string text)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            CurrentSettingsInDb.UpdateElement(EnumDbSettings.CommonMessage, text, db);
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000).ConfigureAwait(true);
                await SendDcExSettingsInDb(CurrentSettingsInDb.Current).ConfigureAwait(false);
            });
        }

        /// <summary>
        ///     User hat eine Idee geliket
        /// </summary>
        /// <param name="text"></param>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        private async Task LikeIdea(string text, long userId, long deviceId)
        {
            // Parse IdeaId
            var ideaId = long.Parse(text);

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            // Like/Unlike for User
            var item = db.TblIdeas
                .Include(x => x.TblIdeaLikes)
                .Include(x => x.TblUser)
                .First(x => x.Id == ideaId);

            var isLike = false;

            if (item.TblIdeaLikes.All(x => x.TblUserId != userId))
            {
                // Ensure Contains
                item.TblIdeaLikes.Add(new TableIdeaLike
                                      {
                                          TblUserId = userId,
                                          TblIdeaId = item.Id,
                                          CreatedAtUtc = DateTime.UtcNow,
                                      });

                isLike = true;
            }
            else
            {
                // Ensure !Contains
                var likes = item.TblIdeaLikes
                    .Where(x => x.TblUserId == userId);
                db.TblIdeaLikes.RemoveRange(likes);
            }

            await db.SaveChangesAsync().ConfigureAwait(true);

            // Send Idea Update for All
            _ = Task.Run(async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                await Task.Delay(300).ConfigureAwait(true);

                var idea = db2.TblIdeas
                    .Include(x => x.TblUser)
                    .Include(x => x.TblIdeaOrganizations)
                    .First(x => x.Id == ideaId);

                // Senden Push an User, wenn gewünscht
                if (idea.TblUser.NotificationPushLike && isLike && idea.TblUserId != userId)
                {
                    var devices = db2.TblDevices.Where(x => x.TblUserId == idea.TblUserId);

                    await PushHelper.SendIdeaLike(devices.Select(x => x.DeviceToken).ToList(), idea.Id).ConfigureAwait(true);
                }

                var users2Inform = new List<long>(db2.GetSysAdmins()) {userId};
                users2Inform.AddRange(db2.TblPermissions
                    .Where(x => idea.TblIdeaOrganizations.Select(y => y.TblOrganizationId).Contains(x.TblOrganizationId))
                    .Select(x => x.TblUserId));

                users2Inform = users2Inform.Distinct().ToList();

                // Ideen syncen
                await SyncDcExIdeas(users2Inform, deviceId).ConfigureAwait(false);
            });
        }

        #region Interface Implementations

        /// <summary>Allgemeine Daten vom Device empfangen</summary>
        /// <param name="deviceId">Geräte Id</param>
        /// <param name="userId">Benutzer Id</param>
        /// <param name="data">Daten</param>
        /// <returns></returns>
        public async Task<string> ReceivedDcCommonData(long deviceId, long userId, DcCommonData data)
        {
            if (data == null!)
            {
                throw new InvalidEnumArgumentException();
            }

            Logging.Log.LogTrace($"[DC] ReceivedDcCommonData UserId {userId} from device {deviceId} key {data.Key}");

            if (!Enum.TryParse(data.Key, true, out EnumDcCommonCommands command))
            {
                throw new InvalidEnumArgumentException($"[DC] ReceivedDcCommonData Key {data.Key} is not a valid Member of EnumDcCommonCommands");
            }

            switch (command)
            {
                case EnumDcCommonCommands.FileDelete:
                    await DeleteFile(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ResendAccessEMail:
                    await ResendAccessEMail(data.Value, deviceId).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ResendConfirmationSms:
                    await ResendAccessSms(data.Value, deviceId).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ResetPassword:
                    await ResetPassword(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.SendTestPush:
                    await SendPush(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.SendCommonMsg:
                    await UpdateCommonInfo(data.Value).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.LikeIdea:
                    await LikeIdea(data.Value, userId, deviceId).ConfigureAwait(false);
                    break;
                case EnumDcCommonCommands.ConfirmSms:
                    var res = await ConfirmSms(data.Value, userId, deviceId).ConfigureAwait(false);
                    return res.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }

            return string.Empty;
        }

        #endregion
    }
}