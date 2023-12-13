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
using ConnectivityHost.Helper;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Datenaustausch für DcExIntro</para>
    ///     Klasse ServerRemoteCalls. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExIntros
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
        public Task<List<DcServerListItem<ExIntroItem>>> GetDcExIntros(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Device will Listen Daten für DcExIntros sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExIntros(long deviceId, long userId, List<DcStoreListItem<ExIntroItem>> data, long secondId)
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

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            TableIntro p = null!;

            foreach (var d in data)
            {
                var tmp = new DcListStoreResultIndexAndData();
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                        p = new TableIntro();
                        tmp.BeforeStoreIndex = d.Index;
                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Modified:

                        var item = await db.TblIntro.Where(f => f.Id == d.Index)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (item == null)
                        {
                            continue;
                        }

                        p = item;

                        r.ElementsStored++;
                        break;
                    case EnumDcListElementState.Deleted:
                        var i = await db.TblIntro.Where(f => f.Id == d.Index)
                            .FirstOrDefaultAsync().ConfigureAwait(false)!;

                        if (i == null)
                        {
                            continue;
                        }

                        p = i;
                        break;
                    case EnumDcListElementState.None:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (d.State == EnumDcListElementState.Deleted)
                {
                    db.TblIntro.Remove(p);
                }
                else
                {
                    p.Weblink = d.Data.HtmlSource;
                }

                if (d.State == EnumDcListElementState.New)
                {
                    db.TblIntro.Add(p);
                }

                await db.SaveChangesAsync().ConfigureAwait(true);

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

                var receiversDb = db2.TblUsers.Select(x => x.Id).ToList();

                receiversDb.Add(-1);

                await SyncDcExIntros(receiversDb).ConfigureAwait(true);
            });

            return r;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExIntros
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public async Task<DcListSyncResultData<ExIntroItem>> SyncDcExIntros(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }

            if (current.CurrentListEntries == null!)
            {
                current = new DcListSyncData
                          {
                              CurrentListEntries = new List<DcSyncElement>()
                          };
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIntros)}): " +
                                 $"Request {userId} -> {string.Join(", ", current.CurrentListEntries.Select(x => $"{x.Id}-{string.Join("", x.DataVersion)}"))}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var result = new DcListSyncResultData<ExIntroItem>();

            var dbItems = db.TblIntro.AsNoTracking();

            var t1 = new DcDbSyncHelper<TableIntro>(dbItems);
            result.ServerItemCount = dbItems.Count();

            var t2 = t1.GetSyncData(current,
                t => true,
                t => new DcSyncElement(t.Id, t.DataVersion));
            result.ItemsToRemoveOnClient = t2.intemsToRemove;
            if (t2.modifiedElementsDb != null)
            {
                foreach (var t in t2.modifiedElementsDb)
                {
                    result.NewOrModifiedItems.Add(new DcServerListItem<ExIntroItem>
                                                  {
                                                      Index = t.Id,
                                                      SortIndex = t.Id,
                                                      DataVersion = t.DataVersion,
                                                      Data = t.ToExIntro(),
                                                  });
                }
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExIntros)}): result: " +
                                 $"a/u: {string.Join(",", result.NewOrModifiedItems.Select(x => $"{x.Index}-{string.Join("", x.DataVersion)}"))} " +
                                 $"d: {string.Join(",", result.ItemsToRemoveOnClient)}");

            return result;
        }

        #endregion
    }
}