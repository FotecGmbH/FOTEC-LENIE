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
    ///     <para>Helfer für Idee</para>
    ///     Klasse DcExIdeaHelpers. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Helper Update an alle relevanten User schicken
        /// </summary>
        /// <param name="helperId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SendHelperUpdate(long helperId, Db db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // Get Idea
            var ideaId = db.TblIdeaHelpers.FirstOrDefault(x => x.Id == helperId)?.TblIdeaId;
            var idea = db.TblIdeas
                .Include(x => x.TblUser)
                .Include(x => x.TblIdeaOrganizations)
                .First(x => x.Id == ideaId);

            // senden je Idee an Admins, und alle in Company
            var affectedOrganizations = idea.TblIdeaOrganizations
                .Select(org => org.TblOrganizationId);
            var receiversDb = db.TblPermissions
                .Include(x => x.TblUser)
                .Include(x => x.TblOrganization)
                .Where(p =>
                    p.TblUser.IsAdmin ||
                    affectedOrganizations.Contains(p.TblOrganization.Id))
                .Select(p => p.TblUser).ToList();

            foreach (var admin in db.TblUsers.Where(x => x.IsAdmin))
            {
                receiversDb.Add(admin);
            }

            receiversDb.Add(idea.TblUser);

            await SyncDcExIdeas(receiversDb.Select(x => x.Id).Distinct().ToList()).ConfigureAwait(true);

            // Nur für die, die gerade mit DC verbunden
            var receiversa = ClientConnection.GetClients();
            var receivers = receiversa
                .Where(x => receiversDb.Any(y => y.Id == x.UserId))
                .Select(x => x.UserId).Distinct();

            if (receivers != null!)
            {
                _ = await SyncDcExIdeaHelpers(receivers.ToList()).ConfigureAwait(true);
            }
        }

        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeaHelpers
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
        public async Task<List<DcServerListItem<ExIdeaHelper>>> GetDcExIdeaHelpers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[GetDcExIdeaHelpers] DeviceId {deviceId} UserId {userId} Filter {filter}");
            if (userId <= 0)
            {
                throw new Exception($"[GetDcExIdeaHelpers] UserId {userId} invalid!");
            }

            var exFilter = new ExIdeaHelperFilter();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var res = filter.FromJson<ExIdeaHelperFilter>();
                if (res != null!)
                {
                    exFilter = res;
                }
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbEntries = db.TblIdeaHelpers
                .Include(x => x.TblUser)
                .Include(x => x.TblIdeaSupplies)
                .Include(x => x.TblIdea)
                .ThenInclude(x => x.TblIdeaOrganizations)
                .Include(x => x.TblIdea)
                .ThenInclude(x => x.TblIdeaNeeds)
                .ThenInclude(x => x.TblIdeaSupplies)
                .Include(x => x.TblIdea)
                .ThenInclude(x => x.TblChat)
                .ThenInclude(x => x.TblChatUsers)
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

            var userAdminCompanys = db.GetUserPlusOrganizationForUser(userId).Select(x => x.Id).ToList();

            var result = new List<DcServerListItem<ExIdeaHelper>>();
            foreach (var item in dbEntries)
            {
                var (uId, uName, uImage) = db.GetUserNameAndImage(item.TblUserId);

                var sendData = item.ToExIdeaHelper();

                sendData.UserId = uId;
                sendData.UserName = uName;
                sendData.UserImage = uImage;

                var hasPermission = item.TblIdea.TblIdeaOrganizations.Any(i => userAdminCompanys.Contains(i.TblOrganizationId));

                sendData.CanEdit = item.TblUserId == userId || hasPermission;
                sendData.CanDelete = item.TblUserId == userId || item.TblIdea.TblUserId == userId || hasPermission;
                sendData.IsMine = item.TblUserId == userId;

                // Private Chat ID
                foreach (var chat in item.TblIdea.TblChat
                             .Where(x => !x.PublicChat))
                {
                    if (sendData.PrivateChatId == null && // noch kein private chat gesetzt
                        chat.TblChatUsers != null! && // chatusers geladen
                        userId != sendData.UserId && // aktueller user ist nicht helfer
                        chat.TblChatUsers.Any(x => x.TblUserId == userId) && // aktueller User ist im chat dabei
                        chat.TblChatUsers.Any(x => x.TblUserId == sendData.UserId)) // helfer ist im chat dabei
                    {
                        sendData.PrivateChatId = chat.Id;
                    }
                }

                // Für jedes Need eine Supply laden
                foreach (var ideaNeed in item.TblIdea.TblIdeaNeeds)
                {
                    var supply = item.TblIdeaSupplies.FirstOrDefault(x => x.TblIdeaNeedId == ideaNeed.Id);
                    if (supply != null)
                    {
                        supply.TblIdeaNeed = ideaNeed;
                        sendData.Supplies.Add(supply.ToExIdeaSupply());
                    }
                    else
                    {
                        var tmp = new TableIdeaSupply
                                  {
                                      TblIdeaNeed = ideaNeed,
                                      TblIdeaNeedId = ideaNeed.Id,
                                      Amount = 0,
                                  };
                        sendData.Supplies.Add(tmp.ToExIdeaSupply());
                    }
                }

                result.Add(new DcServerListItem<ExIdeaHelper>
                           {
                               Data = sendData,
                               Index = item.Id,
                               SortIndex = item.Id,
                           });
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExIdeaHelpers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExIdeaHelpers(long deviceId, long userId, List<DcStoreListItem<ExIdeaHelper>> data, long secondId)
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

            var ideaHelperIdsToSend = new List<long>();
            var ideaNeedIdsToSend = new List<long>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            TableIdeaHelper p = null!;
            var anyDelete = false;
            var users2Inform = new List<long>(db.GetSysAdmins()) {userId};

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        var i = db.TblIdeas
                            .Include(x => x.TblIdeaOrganizations)
                            .FirstOrDefault(x => x.Id == d.Data.IdeaId);
                        if (i == null)
                        {
                            throw new Exception("Darf nicht sein");
                        }

                        p = new TableIdeaHelper
                            {
                                TblIdeaId = d.Data.IdeaId,
                                TblIdea = i,
                                TblUserId = d.Data.UserId,
                                CreatedAtUtc = DateTime.UtcNow,
                            };
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Modified:
                        p = await db.TblIdeaHelpers.Where(f => f.Id == d.Index)
                            .Include(o => o.TblIdeaSupplies)
                            .Include(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaOrganizations)
                            .FirstAsync().ConfigureAwait(false);
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        p = await db.TblIdeaHelpers.Where(f => f.Id == d.Index)
                            .Include(o => o.TblIdeaSupplies)
                            .Include(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaOrganizations)
                            .FirstAsync().ConfigureAwait(false);
                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                users2Inform.AddRange(db.TblPermissions
                    .Where(x => p.TblIdea.TblIdeaOrganizations.Select(y => y.TblOrganizationId).Contains(x.TblOrganizationId))
                    .Select(x => x.TblUserId));

                if (d.State == EnumDcListElementState.Deleted)
                {
                    db.TblIdeaSupplies.RemoveRange(p.TblIdeaSupplies);
                    db.TblIdeaHelpers.Remove(p);
                    anyDelete = true;
                }
                else
                {
                    d.Data.ToTableHelper(p);

                    // Supplies
                    db.TblIdeaSupplies.RemoveRange(p.TblIdeaSupplies);

                    foreach (var ideaSupply in d.Data.Supplies)
                    {
                        ideaNeedIdsToSend.Add(ideaSupply.NeedId);

                        db.TblIdeaSupplies.Add(new TableIdeaSupply
                                               {
                                                   TblIdeaNeedId = ideaSupply.NeedId,
                                                   TblIdeaHelper = p,

                                                   Amount = ideaSupply.Amount,
                                                   CreatedAtUtc = DateTime.UtcNow,
                                               });
                    }
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblIdeaHelpers.Add(p);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);

                if (d.State != EnumDcListElementState.Deleted)
                {
                    ideaHelperIdsToSend.Add(p.Id);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    tmp.NewIndex = p.Id;
                    tmp.NewSortIndex = p.Id;
                    r.NewIndex.Add(tmp);

                    var idea = db.TblIdeas
                        .Include(x => x.TblUser)
                        .FirstOrDefault(x => x.Id == p.TblIdeaId);

                    // Senden Push an User, wenn gewünscht
                    if (idea?.TblUser != null && idea.TblUser.NotificationPushSupport && idea.TblUserId != userId)
                    {
                        var devices = db.TblDevices.Where(x => x.TblUserId == idea.TblUserId);

                        await PushHelper.SendSupportForIdea(devices.Select(x => x.DeviceToken).ToList(), idea.Id).ConfigureAwait(true);
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

                if (anyDelete)
                {
                    await SendReloadList(EnumReloadDcList.IdeaHelpers).ConfigureAwait(false);
                    await SendReloadList(EnumReloadDcList.IdeaNeeds).ConfigureAwait(false);
                }
                else
                {
                    foreach (var helperId in ideaHelperIdsToSend)
                    {
                        // Schicken Helper Updates - dort sind die NeedAmounts auch drinnen
                        await SendHelperUpdate(helperId, db2).ConfigureAwait(true);

                        // alle Helper für dieses Need rausfinden
                        var needs = db2.TblIdeaSupplies
                            .Where(x => x.TblIdeaHelperId == helperId)
                            .Select(x => x.TblIdeaNeedId);

                        ideaNeedIdsToSend.AddRange(needs);
                    }

                    ideaNeedIdsToSend = ideaNeedIdsToSend.Distinct().ToList();

                    foreach (var needId in ideaNeedIdsToSend)
                    {
                        // Schicken Need Updates
                        await SendNeedUpdate(needId, db2).ConfigureAwait(true);
                    }
                }
            });

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeaHelpers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public Task<DcListSyncResultData<ExIdeaHelper>> SyncDcExIdeaHelpers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}