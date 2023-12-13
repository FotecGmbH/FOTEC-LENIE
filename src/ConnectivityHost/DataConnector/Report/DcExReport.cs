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
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Enum;
using Exchange.Model.Report;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Meldungen</para>
    ///     Klasse DcExReport. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExReports
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
        /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <param name="filter">Optionaler Filter für die Daten</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public async Task<List<DcServerListItem<ExReport>>> GetDcExReports(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[GetDcExReports] DeviceId {deviceId} UserId {userId} Filter {filter}");
            if (userId <= 0)
            {
                throw new Exception($"[GetDcExReports] UserId {userId} invalid!");
            }

            var exFilter = new ExReportFilter();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var res = filter.FromJson<ExReportFilter>();
                if (res != null!)
                {
                    exFilter = res;
                }
            }

            if (exFilter.IdeaId < 0 &&
                exFilter.UserId < 0)
            {
                return new List<DcServerListItem<ExReport>>();
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbEntries = db.TblIdeaReports
                .Include(x => x.TblUser)
                .Include(x => x.TblIdea)
                .AsNoTracking();

            if (exFilter.UserId > 0)
            {
                dbEntries = dbEntries.Where(x => x.TblUserId == exFilter.UserId);
            }

            if (exFilter.IdeaId > 0)
            {
                dbEntries = dbEntries.Where(x => x.TblIdeaId == exFilter.IdeaId);
            }

            if (startIndex > 0)
            {
                dbEntries = dbEntries.Where(x => x.Id >= startIndex);
            }

            if (elementsToRead > 0)
            {
                dbEntries = dbEntries.Take((int) elementsToRead);
            }

            var result = new List<DcServerListItem<ExReport>>();
            foreach (var item in dbEntries)
            {
                var (_, uName, uImage) = db.GetUserNameAndImage(item.TblUserId);

                var sendData = item.ToExReport();

                sendData.UserName = uName;
                sendData.UserImage = uImage;

                result.Add(new DcServerListItem<ExReport>
                           {
                               Data = sendData,
                               Index = item.Id,
                               SortIndex = item.Id,
                           });
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExReports sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExReports(long deviceId, long userId, List<DcStoreListItem<ExReport>> data, long secondId)
        {
            if (data == null!)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var r = new DcListStoreResult
                    {
                        SecondId = secondId,
                        StoreResult = new(),
                        NewIndex = new()
                    };

            var ideaIdsToSend = new List<long>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            TableIdeaReport p = null!;

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        p = new TableIdeaReport
                            {
                                TblUserId = userId,
                                TblIdeaId = d.Data.IdeaId,
                                CreatedAtUtc = DateTime.UtcNow,
                            };
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        ideaIdsToSend.Add(d.Data.IdeaId);
                        break;
                    case EnumDcListElementState.Modified:
                        p = await db.TblIdeaReports.Where(f => f.Id == d.Index)
                            .FirstAsync().ConfigureAwait(false);
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        p = await db.TblIdeaReports.Where(f => f.Id == d.Index)
                            .FirstAsync().ConfigureAwait(false);
                        ideaIdsToSend.Add(d.Data.IdeaId);
                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (d.State == EnumDcListElementState.Deleted)
                {
                    db.TblIdeaReports.Remove(p);
                }
                else
                {
                    d.Data.ToTableReport(p);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblIdeaReports.Add(p);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);
                if (d.State == EnumDcListElementState.New)
                {
                    tmp.NewIndex = p.Id;
                    tmp.NewSortIndex = p.Id;
                    r.NewIndex.Add(tmp);

                    var idea = db.TblIdeas
                        .Include(x => x.TblIdeaOrganizations)
                        .First(x => x.Id == p.TblIdeaId);
                    var ideaOrgs = idea.TblIdeaOrganizations.Select(x => x.TblOrganizationId).ToList();

                    var users = db.TblPermissions
                        .Include(x => x.TblUser)
                        .Where(x =>
                            x.TblUser.NotificationPushReport &&
                            (x.UserRole == EnumUserRole.UserPlus || x.UserRole == EnumUserRole.Admin) &&
                            ideaOrgs.Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId)
                        .ToList();

                    users.AddRange(db.TblUsers
                        .Where(x => x.IsAdmin && x.NotificationPushReport)
                        .Select(x => x.Id));

                    users = users.Distinct().ToList();

                    // Senden Push an User, wenn gewünscht
                    if (users.Any())
                    {
                        var devices = db.TblDevices.Where(x => x.TblUserId.HasValue && users.Contains(x.TblUserId.Value));

                        await PushHelper.SendReportForIdea(devices.Select(x => x.DeviceToken).ToList(), idea.Id).ConfigureAwait(true);
                    }

                    var usersMail = db.TblPermissions
                        .Include(x => x.TblUser)
                        .Where(x =>
                            x.TblUser.NotificationMailReport &&
                            (x.UserRole == EnumUserRole.UserPlus || x.UserRole == EnumUserRole.Admin) &&
                            ideaOrgs.Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId)
                        .ToList();

                    usersMail.AddRange(db.TblUsers
                        .Where(x => x.IsAdmin && x.NotificationMailReport)
                        .Select(x => x.Id));

                    usersMail = usersMail.Distinct().ToList();

                    if (usersMail.Any())
                    {
                        var uas = new UserAccountEmailService(_razorEngine);
                        await uas.SendReportMail(db, usersMail, userId, idea, p).ConfigureAwait(true);
                    }
                }
            }

            _ = Task.Run(async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                await Task.Delay(300).ConfigureAwait(true);

                // Anzahl der Reports haben sich bei deisen Ideen geändert -> An die User verschicken
                foreach (var ideaId in ideaIdsToSend)
                {
                    var idea = db2.TblIdeas.First(x => x.Id == ideaId);

                    // senden je Idee an Admins, und alle in Company
                    var receiversDb = db2.TblUsers
                        .Include(x => x.TblPermissions)
                        .Where(x => x.IsAdmin ||
                                    x.TblPermissions.Any(y =>
                                        idea.TblIdeaOrganizations.Any(i =>
                                            i.TblOrganizationId == y.TblOrganizationId)));

                    await SyncDcExIdeas(receiversDb.Select(x => x.Id).ToList(), deviceId).ConfigureAwait(true);
                    await SyncDcExReports(receiversDb.Select(x => x.Id).ToList(), deviceId).ConfigureAwait(true);
                }
            });

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExReports
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public async Task<DcListSyncResultData<ExReport>> SyncDcExReports(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExReports)}): " +
                                 $"Request {userId} -> {string.Join(", ", current.CurrentListEntries.Select(x => $"{x.Id}-{string.Join("", x.DataVersion)}"))}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var result = new DcListSyncResultData<ExReport>();

            var userRegions = db.GetUserOrganizationsForUser(userId).Select(x => x.Id).ToList();

            var dbItems = db.TblIdeaReports
                .Include(x => x.TblUser)
                .Include(x => x.TblIdea)
                .ThenInclude(x => x.TblIdeaOrganizations)
                .Where(x => x.TblIdea.TblIdeaOrganizations.Any(z => userRegions.Contains(z.TblOrganizationId)))
                .AsNoTracking();

            var t1 = new DcDbSyncHelper<TableIdeaReport>(dbItems);
            result.ServerItemCount = dbItems.Count();

            var dvEmpty = new byte[db.TblIdeaReports.FirstOrDefault()?.DataVersion.Length ?? 16];
            for (var i = 0; i < dvEmpty.Length; i++)
            {
                dvEmpty[i] = 0;
            }

            var t2 = t1.GetSyncData(current,
                t => true,
                t => new DcSyncElement(t.Id, new List<byte[]>
                                             {
                                                 t.DataVersion,
                                                 t.TblUser != null! ? t.TblUser.DataVersion : dvEmpty,
                                                 t.TblIdea != null! ? t.TblIdea.DataVersion : dvEmpty,
                                             }));
            result.ItemsToRemoveOnClient = t2.intemsToRemove;
            if (t2.modifiedElementsDb != null)
            {
                foreach (var t in t2.modifiedElementsDb)
                {
                    var exReport = t.ToExReport();

                    var (_, uName, uImage) = db.GetUserNameAndImage(t.TblUserId);

                    var tmp = new DcSyncElement(t.Id, new List<byte[]>
                                                      {
                                                          t.DataVersion,
                                                          t.TblUser != null! ? t.TblUser.DataVersion : dvEmpty,
                                                          t.TblIdea != null! ? t.TblIdea.DataVersion : dvEmpty,
                                                      });

                    exReport.UserName = uName;
                    exReport.UserImage = uImage;

                    result.NewOrModifiedItems.Add(new DcServerListItem<ExReport>
                                                  {
                                                      Index = t.Id,
                                                      SortIndex = t.Id,
                                                      DataVersion = tmp.DataVersion,
                                                      Data = exReport,
                                                  });
                }
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExReports)}): result: " +
                                 $"a/u: {string.Join(",", result.NewOrModifiedItems.Select(x => $"{x.Index}-{string.Join("", x.DataVersion)}"))} " +
                                 $"d: {string.Join(",", result.ItemsToRemoveOnClient)}");

            return result;
        }

        #endregion
    }
}