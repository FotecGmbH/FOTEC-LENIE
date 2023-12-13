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
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Model.FutureWishes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Zukünftige Features</para>
    ///     Klasse DcExFutureWishes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        #region Interface Implementations

        /// <summary>
        ///     Device fordert Listen Daten für DcExFutureWishes
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
        public async Task<List<DcServerListItem<ExFutureWish>>> GetDcExFutureWishes(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[GetDcExFutureWishes] DeviceId {deviceId} UserId {userId}");
            if (userId <= 0)
            {
                throw new Exception($"[GetDcExFutureWishes] UserId {userId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var dbEntries = db.TblFutureWishes
                .Include(x => x.TblFutureWishLikes)
                .AsNoTracking();

            if (startIndex > 0)
            {
                dbEntries = dbEntries.Where(x => x.Id >= startIndex);
            }

            if (elementsToRead > 0)
            {
                dbEntries = dbEntries.Take((int) elementsToRead);
            }

            var result = new List<DcServerListItem<ExFutureWish>>();
            foreach (var item in dbEntries)
            {
                result.Add(new DcServerListItem<ExFutureWish>
                           {
                               Data = item.ToExFutureWish(userId),
                               Index = item.Id,
                               SortIndex = item.Id,
                           });
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExFutureWishes sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExFutureWishes(long deviceId, long userId, List<DcStoreListItem<ExFutureWish>> data, long secondId)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (userId == -1)
            {
                throw new ArgumentException("[DcExObjects].Set - Für Demouser nicht möglich!");
            }

            var result = new DcListStoreResult
                         {
                             ElementsStored = 0,
                             NewIndex = new List<DcListStoreResultIndexAndData>(),
                             SecondId = secondId,
                             StoreResult = new DcStoreResult(),
                         };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task


            foreach (var dcStoreListItem in data)
            {
                switch (dcStoreListItem.State)
                {
                    case EnumDcListElementState.Modified:
                        // liked
                        var item = db.TblFutureWishes
                            .Include(x => x.TblFutureWishLikes)
                            .First(x => x.Id == dcStoreListItem.Index);

                        if (dcStoreListItem.Data.Liked)
                        {
                            // Ensure Contains
                            if (item.TblFutureWishLikes.All(x => x.TblUserId != userId))
                            {
                                item.TblFutureWishLikes.Add(new TableFutureWishLike
                                                            {
                                                                TblUserId = userId,
                                                                TblFutureWishId = item.Id,
                                                            });
                            }
                        }
                        else
                        {
                            // Ensure !Contains
                            var likes = item.TblFutureWishLikes.Where(x => x.TblUserId == userId);
                            db.TblFutureWishLikes.RemoveRange(likes);
                        }

                        await db.SaveChangesAsync().ConfigureAwait(true);
                        result.ElementsStored++;

                        var sendData = item.ToExFutureWish();
                        sendData.Liked = dcStoreListItem.Data.Liked;
                        break;
                    default:
                        // Not allowed!
                        throw new NotImplementedException("Anlegen, löschen von Wünschen nicht möglich!");
                }
            }

            return result;
        }

        /// <summary>
        ///     Daten Synchronisieren für DcExFutureWishes
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public Task<DcListSyncResultData<ExFutureWish>> SyncDcExFutureWishes(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}