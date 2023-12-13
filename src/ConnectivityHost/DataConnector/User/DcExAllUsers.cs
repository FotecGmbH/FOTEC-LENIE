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
        ///     Device fordert Listen Daten für DcExAllUsers
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
        public async Task<List<DcServerListItem<ExUser>>> GetDcExAllUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
        {
            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(GetDcExAllUsers)}): DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[GetDcExAllUsers] DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[GetDcExAllUsers] UserId {userId} invalid!");
            }
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var sysAdminCheck = await db.TblUsers.FirstOrDefaultAsync(f => f.Id == userId).ConfigureAwait(false);
            if (sysAdminCheck == null || !sysAdminCheck.IsAdmin)
            {
                throw new Exception("[GetDcExAllUsers] User not found or is no Sys-Admin!");
            }

            var ids = new List<long>();
            if (startIndex > 0)
            {
                if (elementsToRead > 0)
                {
                    ids = await db.TblUsers.Where(w => w.Id >= startIndex).Take((int) elementsToRead).Select(s => s.Id).ToListAsync().ConfigureAwait(false);
                }
                else
                {
                    ids = await db.TblUsers.Where(w => w.Id >= startIndex).Select(s => s.Id).ToListAsync().ConfigureAwait(false);
                }
            }
            else
            {
                ids = await db.TblUsers.Select(f => f.Id).ToListAsync().ConfigureAwait(true);
            }

            var result = new List<DcServerListItem<ExUser>>();
            foreach (var id in ids)
            {
                var u = db.GetUserWithdependences(id);
                if (u == null!)
                {
                    throw new ArgumentNullException($"[{nameof(ServerRemoteCalls)}]({nameof(GetDcExAllUsers)}): {nameof(u)}");
                }

                result.Add(new DcServerListItem<ExUser>
                           {
                               Index = u.Id,
                               SortIndex = u.Id,
                               Data = u.ToExUser()
                           });
            }

            return result;
        }

        /// <summary>
        ///     Device will Listen Daten für DcExAllUsers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcListStoreResult> StoreDcExAllUsers(long deviceId, long userId, List<DcStoreListItem<ExUser>> data, long secondId)
        {
            if (data == null!)
            {
                throw new ArgumentNullException($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExAllUsers)}): {nameof(data)}");
            }

            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExAllUsers)}): {deviceId} {userId} {string.Join(";", data.Select(x => x.Index + " - " + x.Data.LoginName))}");

            var r = new DcListStoreResult
                    {
                        SecondId = secondId,
                        StoreResult = new(),
                        ElementsStored = new()
                    };

            foreach (var d in data)
            {
                switch (d.State)
                {
                    case EnumDcListElementState.New:
                    case EnumDcListElementState.Deleted:
                        throw new NotImplementedException($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExAllUsers)})");
                    case EnumDcListElementState.None:
                        break;
                    case EnumDcListElementState.Modified:
                        var tmp = await SetDcExUser(deviceId, d.Index, d.Data).ConfigureAwait(false);
                        if (tmp == null! || !tmp.DataOk)
                        {
                            throw new Exception($"[{nameof(ServerRemoteCalls)}]({nameof(StoreDcExAllUsers)}): Store fails for User {d.Index}");
                        }

                        r.ElementsStored++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //Info über Änderungen der Liste für andere Geräte/Personen erfolgt über die "SetDcExUser"

            return r;
        }


        /// <summary>
        ///     Daten Synchronisieren für DcExAllUsers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        public async Task<DcListSyncResultData<ExUser>> SyncDcExAllUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
        {
            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExAllUsers)}): DeviceId {deviceId} UserId {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[GetDcExAllUsers] DeviceId {deviceId} invalid!");
            }

            if (userId <= 0)
            {
                throw new Exception($"[GetDcExAllUsers] UserId {userId} invalid!");
            }
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var sysAdminCheck = await db.TblUsers.FirstOrDefaultAsync(f => f.Id == userId).ConfigureAwait(false);
            if (sysAdminCheck == null || !sysAdminCheck.IsAdmin)
            {
                throw new Exception("[GetDcExAllUsers] User not found or is no Sys-Admin!");
            }

            var result = new DcListSyncResultData<ExUser>
                         {
                             ServerItemCount = await db.TblUsers.AsNoTracking().LongCountAsync().ConfigureAwait(false)
                         };
            var t1 = new DcDbSyncHelper<TableUser>(db.TblUsers.AsNoTracking());
            // Eventuell einen Filter mit geben abhängig von User "var isAdmin = db.IsUserSysAdmin(userId);" bzw. von den props:
            // t1.GetSyncData(current,e=>e.Archive == prop.IsArchive && isAdmin);
            var t2 = t1.GetSyncData(current);
            result.ItemsToRemoveOnClient = t2.intemsToRemove;
            if (t2.modifiedElementsDb != null)
            {
                foreach (var t in t2.modifiedElementsDb)
                {
                    result.NewOrModifiedItems.Add(new DcServerListItem<ExUser>
                                                  {
                                                      Index = t.Id,
                                                      SortIndex = t.Id,
                                                      DataVersion = t.DataVersion,
                                                      Data = t.ToExUser()
                                                  });
                }
            }

            return result;
        }

        #endregion
    }
}