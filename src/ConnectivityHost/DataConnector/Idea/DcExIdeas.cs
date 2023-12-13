// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using ConnectivityHost.Helper;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Enum;
using Exchange.Model.Idea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Ideenwerkstatt</para>
    ///     Klasse DcExIdeas. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Public Chat für die Idee anlegen
        /// </summary>
        /// <param name="ideaId"></param>
        /// <returns></returns>
        public async Task<long> CreatePublicChat(long ideaId)
        {
            using var db = new Db();
            var idea = db.TblIdeas
                .Include(x => x.TblIdeaOrganizations)
                .AsNoTracking()
                .First(x => x.Id == ideaId);

            var pubChat = new TableChat
                          {
                              TblIdeaId = ideaId,

                              PublicChat = true,
                              ChatName = (idea.Title) + " (Gruppenchat)",
                          };

            pubChat.TblChatUsers.Add(new TableChatUsers
                                     {
                                         TblUserId = idea.TblUserId,
                                         TblChat = pubChat,
                                     });

            pubChat.TblChatEntries.Add(new TableChatEntry
                                       {
                                           TblChat = pubChat,
                                           TblUserWriterId = idea.TblUserId,
                                           EntryDateTimeUtc = DateTime.UtcNow,
                                           Text = "Ich habe meine Idee angelegt!",
                                       });

            db.TblChat.Add(pubChat);

            await db.SaveChangesAsync().ConfigureAwait(true);

            return pubChat.Id;
        }

        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeas
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
        [Obsolete("Sync benutzen")]
        public async Task<List<DcServerListItem<ExIdea>>> GetDcExIdeas(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[GetDcExIdeas] DeviceId {deviceId} UserId {userId} Start {startIndex} Elems {elementsToRead} Filter {filter} start");

            if (userId <= 0)
            {
                throw new Exception($"[GetDcExIdeas] UserId {userId} invalid!");
            }

            var sw = new Stopwatch();
            sw.Start();

            var exFilter = new ExIdeaFilter();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var res = filter.FromJson<ExIdeaFilter>();
                if (res != null!)
                {
                    exFilter = res;
                }
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var userRegions = db.GetUserOrganizationsForUser(userId).Select(x => x.Id).ToList();

            var dbEntries = db.TblIdeas
                .Include(x => x.TblIdeaOrganizations)
                .ThenInclude(x => x.TblOrganization)
                .Include(x => x.TblUser)
                .Include(x => x.TblChat)
                .ThenInclude(x => x.TblChatUsers)
                .Include(x => x.TblIdeaHelpers)
                .Include(x => x.TblIdeaLikes)
                .Include(x => x.TblIdeaReports)
                .Include(x => x.TblIdeaImage)
                .OrderBy(x => x.Id)
                .AsNoTracking();

            if (exFilter.OrganizationId is > 0)
            {
                dbEntries = dbEntries
                    .Where(x => x.TblIdeaOrganizations.Any(z => z.TblOrganizationId == exFilter.OrganizationId));
            }
            else
            {
                dbEntries = dbEntries
                    .Where(x => x.TblIdeaOrganizations.Any(z => userRegions.Contains(z.TblOrganizationId)));
            }

            if (exFilter.OnlyMyIdeas && exFilter.OnlyMyHelpingIdeas)
            {
                dbEntries = dbEntries
                    .Where(x => x.TblUserId == userId || x.TblIdeaHelpers.Any(y => y.TblUserId == userId));
            }
            else if (exFilter.OnlyMyIdeas)
            {
                dbEntries = dbEntries
                    .Where(x => x.TblUserId == userId);
            }
            else if (exFilter.OnlyMyHelpingIdeas)
            {
                dbEntries = dbEntries
                    .Where(x => x.TblIdeaHelpers.Any(y => y.TblUserId == userId));
            }

            // TextSuche
            if (!string.IsNullOrWhiteSpace(exFilter.SearchText))
            {
                dbEntries = dbEntries.Where(x =>
                    x.Title.ToLower(CultureInfo.CurrentCulture).Contains(exFilter.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                    x.Description.ToLower(CultureInfo.CurrentCulture).Contains(exFilter.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                    x.TblUser.FirstName.ToLower(CultureInfo.CurrentCulture).Contains(exFilter.SearchText.ToLower(CultureInfo.CurrentCulture)) ||
                    x.TblUser.LastName.ToLower(CultureInfo.CurrentCulture).Contains(exFilter.SearchText.ToLower(CultureInfo.CurrentCulture)));
            }

            if (startIndex > 0 && !exFilter.LoadReverse)
            {
                dbEntries = dbEntries
                    .Where(x => x.Id >= startIndex);
            }
            else if (startIndex > 0 && exFilter.LoadReverse)
            {
                dbEntries = dbEntries
                    .Where(x => x.Id <= startIndex)
                    .OrderByDescending(x => x.Id);
            }

            if (elementsToRead > 0)
            {
                dbEntries = dbEntries
                    .Take((int) elementsToRead);
            }

            var userAdminCompanys = db.GetUserPlusOrganizationForUser(userId).Select(org => org.Id).ToList();

            var result = new List<DcServerListItem<ExIdea>>();
            foreach (var item in dbEntries)
            {
                var (uId, uName, uImage) = db.GetUserNameAndImage(item.TblUserId);

                var sendData = item.ToExIdea(userId);

                if (!db.TblChat.Any(x => x.TblIdeaId == item.Id && x.PublicChat))
                {
                    sendData.PublicChatId = await CreatePublicChat(item.Id).ConfigureAwait(true);
                }

                sendData.CreatorUserId = uId;
                sendData.CreatorUserName = uName;
                sendData.CreatorUserImage = uImage;

                sendData.CanEdit = item.TblUserId == userId || item.TblIdeaOrganizations.Any(org => userAdminCompanys.Contains(org.TblOrganizationId));
                sendData.CanSeeReports = sendData.CanEdit && userId != item.TblUserId;

                result.Add(new DcServerListItem<ExIdea>
                           {
                               Data = sendData,
                               Index = item.Id,
                               SortIndex = item.Id,
                           });
            }

            result = result.ToList();

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(GetDcExIdeas)}): DeviceId {deviceId} UserId {userId} Start {startIndex} Elems {elementsToRead} Filter {filter} finished in {sw.ElapsedMilliseconds} ms");

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExIdeas sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExIdeas(long deviceId, long userId, List<DcStoreListItem<ExIdea>> data, long secondId)
        {
            if (data == null!)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExIdeas)}): " +
                                 $"Request {userId} -> {data.Count}");

            var r = new DcListStoreResult
                    {
                        SecondId = secondId,
                        StoreResult = new(),
                        NewIndex = new()
                    };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            TableIdea p = null!;
            var anyDelete = false;
            var users2Inform = new List<long>(db.GetSysAdmins()) {userId};

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        p = new TableIdea
                            {
                                TblUserId = userId,
                                CreatedAtUtc = DateTime.UtcNow,
                            };
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Modified:
                        p = await db.TblIdeas.Where(f => f.Id == d.Index)
                            .Include(i => i.TblChat)
                            .ThenInclude(i => i.TblChatUsers)
                            .Include(i => i.TblIdeaLikes)
                            .Include(i => i.TblIdeaReports)
                            .Include(i => i.TblIdeaHelpers)
                            .Include(i => i.TblIdeaOrganizations)
                            .FirstAsync().ConfigureAwait(false);
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        p = (await db.TblIdeas.Where(f => f.Id == d.Index)
                            .Include(i => i.TblChat)
                            .ThenInclude(i => i.TblChatUsers)
                            .Include(i => i.TblIdeaLikes)
                            .Include(i => i.TblIdeaReports)
                            .Include(i => i.TblIdeaHelpers)
                            .Include(i => i.TblIdeaOrganizations)
                            .Include(i => i.TblIdeaNeeds)
                            .ThenInclude(i => i.TblIdeaSupplies)
                            .FirstOrDefaultAsync().ConfigureAwait(false))!;

                        // bereits gelöscht?
                        if (p == null!)
                        {
                            continue;
                        }

                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (d.State == EnumDcListElementState.Deleted)
                {
                    // User vor Löschen
                    users2Inform.AddRange(db.TblPermissions
                        .Where(x => p.TblIdeaOrganizations.Select(y => y.TblOrganizationId).Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId));

                    db.TblIdeaLikes.RemoveRange(p.TblIdeaLikes);
                    db.TblIdeaReports.RemoveRange(p.TblIdeaReports);
                    db.TblIdeaHelpers.RemoveRange(p.TblIdeaHelpers);
                    db.TblIdeaOrganizations.RemoveRange(p.TblIdeaOrganizations);
                    foreach (var need in p.TblIdeaNeeds)
                    {
                        db.TblIdeaSupplies.RemoveRange(need.TblIdeaSupplies);
                    }

                    db.TblIdeaNeeds.RemoveRange(p.TblIdeaNeeds);
                    db.TblIdeas.Remove(p);

                    anyDelete = true;
                }
                else
                {
                    d.Data.ToTableIdea(p);

                    // User vor Änderung
                    users2Inform.AddRange(db.TblPermissions
                        .Where(x => p.TblIdeaOrganizations.Select(y => y.TblOrganizationId).Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId));

                    db.TblIdeaOrganizations.RemoveRange(p.TblIdeaOrganizations);
                    foreach (var dataCompany in d.Data.Companies)
                    {
                        p.TblIdeaOrganizations.Add(new TableIdeaOrganization
                                                   {
                                                       TblOrganizationId = dataCompany.OrganizationId,
                                                   });
                    }

                    // User nach Änderung
                    users2Inform.AddRange(db.TblPermissions
                        .Where(x => p.TblIdeaOrganizations.Select(y => y.TblOrganizationId).Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId));
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblIdeas.Add(p);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);

                if (d.State != EnumDcListElementState.Deleted &&
                    !db.TblChat.Any(x => x.TblIdeaId == p.Id && x.PublicChat))
                {
                    await CreatePublicChat(p.Id).ConfigureAwait(true);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    tmp.NewIndex = p.Id;
                    tmp.NewSortIndex = p.Id;
                    r.NewIndex.Add(tmp);

                    var idea = db.TblIdeas
                        .Include(x => x.TblIdeaOrganizations)
                        .First(x => x.Id == p.Id);
                    var ideaOrgs = idea.TblIdeaOrganizations.Select(x => x.TblOrganizationId).ToList();

                    var users = db.TblPermissions
                        .Include(x => x.TblUser)
                        .Where(x =>
                            x.TblUser.NotificationPushIdea &&
                            ideaOrgs.Contains(x.TblOrganizationId))
                        .Select(x => x.TblUserId)
                        .ToList();

                    users.AddRange(db.TblUsers
                        .Where(x => x.IsAdmin && x.NotificationPushIdea)
                        .Select(x => x.Id));

                    users = users.Distinct().ToList();

                    // Senden Push an User, wenn gewünscht
                    if (users.Any())
                    {
                        var devices = db.TblDevices.Where(x => x.TblUserId.HasValue && users.Contains(x.TblUserId.Value) && x.TblUserId != userId);

                        await PushHelper.SendNewIdea(devices.Select(x => x.DeviceToken).ToList(), idea.Id).ConfigureAwait(true);
                    }
                }
            }

            _ = Task.Run(async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                await Task.Delay(300).ConfigureAwait(true);

                users2Inform = users2Inform.Distinct().ToList();

                // Ideen syncen
                await SyncDcExIdeas(users2Inform, deviceId).ConfigureAwait(false);

                users2Inform.Add(-1);
                users2Inform = users2Inform.Distinct().ToList();

                await SyncDcExOrganization(users2Inform).ConfigureAwait(false);

                if (anyDelete)
                {
                    await SendReloadList(EnumReloadDcList.IdeaNeeds).ConfigureAwait(false);
                    await SendReloadList(EnumReloadDcList.IdeaHelpers).ConfigureAwait(false);
                }
            });

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeas
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public async Task<DcListSyncResultData<ExIdea>> SyncDcExIdeas(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIdeas)}): " +
                                 $"Request {userId} -> " +
                                 (current.CurrentListEntries != null!
                                     ? string.Join(", ", current.CurrentListEntries
                                         .Select(x => $"{x.Id}-{string.Join("", (x.DataVersion))}"))
                                     : "NULL"));

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var userRegions = db.GetUserOrganizationsForUser(userId).Select(x => x.Id).ToList();

            var ideaIds = db.TblIdeas
                .Include(x => x.TblIdeaOrganizations)
                .Where(x => x.TblIdeaOrganizations.Any(z => userRegions.Contains(z.TblOrganizationId)))
                .OrderBy(x => x.Id)
                .Select(x => x.Id);

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIdeas)}): Ideas for user: {string.Join(", ", ideaIds)}");

            var result = new DcListSyncResultData<ExIdea>
                         {
                             ServerItemCount = ideaIds.Count(),
                         };
            try
            {
                var t1 = new DcDbSyncHelper<TableIdea>(db.TblIdeas
                    // in Welchen Gemeinden ist die Idee sichtbar
                    .Include(x => x.TblIdeaOrganizations)
                    // User, der die Idee erstellt hat
                    .Include(x => x.TblUser)
                    // Public Chat & Private Chat
                    .Include(x => x.TblChat)
                    // Unterstütze ich die Idee
                    .Include(x => x.TblIdeaHelpers)
                    // Habe ich die Idee geliked
                    .Include(x => x.TblIdeaLikes)
                    // Habe ich die Idee reportet
                    .Include(x => x.TblIdeaReports)
                    // Bild zur Idee
                    .Include(x => x.TblIdeaImage)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .AsNoTracking());

                var dvEmpty = new byte[db.TblIdeas.FirstOrDefault()?.DataVersion.Length ?? 16];
                for (var i = 0; i < dvEmpty.Length; i++)
                {
                    dvEmpty[i] = 0;
                }

                var t2 = t1.GetSyncData(current,
                    o => ideaIds.Contains(o.Id),
                    t => new DcSyncElement(t.Id, new List<byte[]>
                                                 {
                                                     t.DataVersion,
                                                     t.TblIdeaOrganizations.Any() ? new DcSyncElement(t.Id, t.TblIdeaOrganizations.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                     t.TblUser.DataVersion,
                                                     t.TblChat.Any() ? new DcSyncElement(t.Id, t.TblChat.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                     t.TblIdeaHelpers.Any() ? new DcSyncElement(t.Id, t.TblIdeaHelpers.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                     t.TblIdeaLikes.Any() ? new DcSyncElement(t.Id, t.TblIdeaLikes.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                     t.TblIdeaReports.Any() ? new DcSyncElement(t.Id, t.TblIdeaReports.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                 }));
                result.ItemsToRemoveOnClient = t2.intemsToRemove;
                if (t2.modifiedElementsDb != null && t2.modifiedElementsDb.Any())
                {
                    var userAdminCompanys = db.GetUserPlusOrganizationForUser(userId).Select(org => org.Id).ToList();
                    foreach (var t in t2.modifiedElementsDb
                                 .Include(x => x.TblIdeaImage)
                                 .Include(x => x.TblIdeaOrganizations)
                                 .ThenInclude(x => x.TblOrganization)
                                 .Include(x => x.TblChat)
                                 .ThenInclude(x => x.TblChatUsers)
                                 .OrderByDescending(x => x.CreatedAtUtc))
                    {
                        var currentItem = current.CurrentListEntries?.FirstOrDefault(x => x.Id == t.Id);

                        var tmp = new DcSyncElement(t.Id, new List<byte[]>
                                                          {
                                                              t.DataVersion,
                                                              t.TblIdeaOrganizations.Any() ? new DcSyncElement(t.Id, t.TblIdeaOrganizations.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                              t.TblUser.DataVersion,
                                                              t.TblChat.Any() ? new DcSyncElement(t.Id, t.TblChat.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                              t.TblIdeaHelpers.Any() ? new DcSyncElement(t.Id, t.TblIdeaHelpers.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                              t.TblIdeaLikes.Any() ? new DcSyncElement(t.Id, t.TblIdeaLikes.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                              t.TblIdeaReports.Any() ? new DcSyncElement(t.Id, t.TblIdeaReports.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                          });

                        var item = t;

                        var (uId, uName, uImage) = db.GetUserNameAndImage(item.TblUserId);

                        var sendData = item.ToExIdea(userId);

                        if (!db.TblChat.Any(x => x.TblIdeaId == item.Id && x.PublicChat))
                        {
                            sendData.PublicChatId = await CreatePublicChat(item.Id).ConfigureAwait(true);
                        }

                        sendData.CreatorUserId = uId;
                        sendData.CreatorUserName = uName;
                        sendData.CreatorUserImage = uImage;

                        sendData.CanEdit = item.TblUserId == userId || item.TblIdeaOrganizations.Any(org => userAdminCompanys.Contains(org.TblOrganizationId));
                        sendData.CanSeeReports = sendData.CanEdit && userId != item.TblUserId;

                        Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIdeas)}): Update: {item.Id} - {string.Join("", currentItem?.DataVersion ?? Array.Empty<byte>())} -> {string.Join("", tmp.DataVersion)}");

                        result.NewOrModifiedItems.Add(new DcServerListItem<ExIdea>
                                                      {
                                                          Data = sendData,
                                                          Index = item.Id,
                                                          SortIndex = item.Id,
                                                          DataVersion = tmp.DataVersion,
                                                      });
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIdeas)}): {e}");
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIdeas)}): result: " +
                                 $"a/u: {string.Join(",", result.NewOrModifiedItems.Select(x => $"{x.Index}-{string.Join("", x.DataVersion)}"))} " +
                                 $"d: {string.Join(",", result.ItemsToRemoveOnClient)}");

            return result;
        }

        #endregion
    }
}