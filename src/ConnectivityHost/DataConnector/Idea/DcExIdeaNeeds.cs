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
    ///     <para>benötigte Sache je Idee</para>
    ///     Klasse DcExIdeaNeeds. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Update für benötigte Sache an alle relevanten User schicken
        /// </summary>
        /// <param name="needId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SendNeedUpdate(long needId, Db db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // Get Idea
            var ideaId = db.TblIdeaNeeds.FirstOrDefault(x => x.Id == needId)?.TblIdeaId;
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

            // Nur für die, die gerade mit DC verbunden
            var receiversa = ClientConnection.GetClients();
            var receivers = receiversa
                .Where(x => receiversDb.Any(y => y.Id == x.UserId))
                .Select(x => x.UserId).Distinct();

            if (receivers != null!)
            {
                _ = await SyncDcExIdeaNeeds(receivers.ToList()).ConfigureAwait(true);
            }
        }

        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeaNeeds
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
        public async Task<List<DcServerListItem<ExIdeaNeed>>> GetDcExIdeaNeeds(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[GetDcExIdeaNeeds] DeviceId {deviceId} UserId {userId} Filter {filter}");
            if (userId <= 0)
            {
                throw new Exception($"[GetDcExIdeaNeeds] UserId {userId} invalid!");
            }

            var exFilter = new ExIdeaNeedFilter();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var res = filter.FromJson<ExIdeaNeedFilter>();
                if (res != null!)
                {
                    exFilter = res;
                }
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbEntries = db.TblIdeaNeeds
                .Include(x => x.TblIdea)
                .ThenInclude(x => x.TblIdeaOrganizations)
                .Include(x => x.TblIdeaSupplies)
                .AsNoTracking();

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

            var result = new List<DcServerListItem<ExIdeaNeed>>();
            foreach (var item in dbEntries)
            {
                var sendData = item.ToExIdeaNeed();

                var canDo = item.TblIdea.TblIdeaOrganizations.Any(i => userAdminCompanys.Contains(i.TblOrganizationId));

                sendData.CanEdit = item.TblIdea.TblUserId == userId || canDo;
                sendData.CanDelete = item.TblIdea.TblUserId == userId || canDo;

                result.Add(new DcServerListItem<ExIdeaNeed>
                           {
                               Data = sendData,
                               Index = item.Id,
                               SortIndex = item.Id,
                           });
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExIdeaNeeds sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExIdeaNeeds(long deviceId, long userId, List<DcStoreListItem<ExIdeaNeed>> data, long secondId)
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

            TableIdeaNeed p = null!;
#pragma warning disable CS0219
            var anyDelete = false;
#pragma warning restore CS0219

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        p = new TableIdeaNeed
                            {
                                TblIdeaId = d.Data.IdeaId,
                                CreatedAtUtc = DateTime.UtcNow,
                            };
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Modified:
                        p = await db.TblIdeaNeeds.Where(f => f.Id == d.Index)
                            .FirstAsync().ConfigureAwait(false);
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        p = await db.TblIdeaNeeds.Where(f => f.Id == d.Index)
                            .Include(i => i.TblIdeaSupplies)
                            .FirstAsync().ConfigureAwait(false);
                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (d.State == EnumDcListElementState.Deleted)
                {
                    db.TblIdeaSupplies.RemoveRange(p.TblIdeaSupplies);
                    db.TblIdeaNeeds.Remove(p);
                    anyDelete = true;
                }
                else
                {
                    d.Data.ToTableIdeaNeed(p);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblIdeaNeeds.Add(p);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);

                if (d.State != EnumDcListElementState.Deleted)
                {
                    ideaNeedIdsToSend.Add(p.Id);
                }

                if (d.State == EnumDcListElementState.New)
                {
                    tmp.NewIndex = p.Id;
                    tmp.NewSortIndex = p.Id;
                    r.NewIndex.Add(tmp);
                }
            }

            _ = Task.Run(async () =>
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                await Task.Delay(300).ConfigureAwait(true);

                if (anyDelete)
                {
                    //ToDo: Mko Geht besser in kommendem Connectivity nuget!
                    await SendReloadList(EnumReloadDcList.IdeaNeeds).ConfigureAwait(false);
                    await SendReloadList(EnumReloadDcList.IdeaHelpers).ConfigureAwait(false);
                }
                else
                {
                    foreach (var needId in ideaNeedIdsToSend)
                    {
                        // Schicken Need Updates
                        await SendNeedUpdate(needId, db2).ConfigureAwait(true);

                        // alle Helper für dieses Need rausfinden
                        var supplies = db2.TblIdeaSupplies
                            .Where(x => x.TblIdeaNeedId == needId)
                            .Select(x => x.TblIdeaHelperId);

                        ideaHelperIdsToSend.AddRange(supplies);
                    }

                    ideaHelperIdsToSend = ideaHelperIdsToSend.Distinct().ToList();

                    foreach (var helperId in ideaHelperIdsToSend)
                    {
                        // Schicken Helper Updates - dort sind die NeedAmounts auch drinnen
                        await SendHelperUpdate(helperId, db2).ConfigureAwait(true);
                    }
                }
            });

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeaNeeds
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public Task<DcListSyncResultData<ExIdeaNeed>> SyncDcExIdeaNeeds(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}