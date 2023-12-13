// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Database;
using Database.Converter;
using Exchange.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        ///     Device fordert Daten für DcExDeviceInfo
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        public async Task<ExDeviceInfo> GetDcExDeviceInfo(long deviceId, long userId)
        {
            Logging.Log.LogTrace($"[GetDcExDevice] device id {deviceId} user id {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[GetDcExDevice] DeviceId {deviceId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var d = await db.TblDevices.FirstOrDefaultAsync(i => i.Id == deviceId).ConfigureAwait(false);
            if (d == null)
            {
                throw new Exception($"[GetDcExDevice] No Device with Id {deviceId} found in database!");
            }

            return d.ToExDeviceInfo();
        }

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        public async Task<DcStoreResult> SetDcExDeviceInfo(long deviceId, long userId, ExDeviceInfo data)
        {
            Logging.Log.LogTrace($"[SetDcExDevice] device id {deviceId} user id {userId}");
            if (deviceId <= 0)
            {
                throw new Exception($"[SetDcExDevice] DeviceId {deviceId} invalid!");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var d = await db.TblDevices.FirstOrDefaultAsync(i => i.Id == deviceId).ConfigureAwait(false);
            if (d == null)
            {
                throw new Exception($"[SetDcExDevice] No Device with Id {deviceId} found in database!");
            }

            data.ToTableDevice(d);
            await db.SaveChangesAsync().ConfigureAwait(false);

            return new DcStoreResult();
        }

        #endregion
    }
}