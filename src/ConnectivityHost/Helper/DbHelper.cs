// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Allgemeine Datenbankhilfsmethoden</para>
    ///     Klasse DbHelper. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class DbHelper
    {
        /// <summary>
        ///     User mit allen Abhängigkeiten löschen
        ///     DataConnector könnte (noch) Probleme damit haben
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteUserAccount(Db db, long userId)
        {
            if (db == null)
            {
                throw new NullReferenceException("[DeleteUserAccount] db is NULL");
            }

            var dbUser = db.GetUserWithdependences(userId, false);
            if (dbUser == null)
            {
                Logging.Log.LogWarning($"[DeleteUserAccount] User {userId} does not exist.");
                return false;
            }

            //Check Last Admin
            if (dbUser.IsAdmin)
            {
                if (db.TblUsers.Count(c => c.IsAdmin) == 1)
                {
                    Logging.Log.LogWarning($"[DeleteUserAccount] Can not delete User {userId}. Is Last Admin!");
                    return false;
                }
            }

            //Bild
            if (dbUser.TblUserImageId.HasValue)
            {
                await FilesDbBlob.DeleteFile(db, dbUser.TblUserImageId.Value).ConfigureAwait(false);
                dbUser.TblUserImageId = null;
            }

            //Geräte
            foreach (var device in dbUser.TblDevices)
            {
                device.TblUserId = null;
            }

            //User
            db.TblUsers.Remove(dbUser);

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[DeleteUserAccount] DbSaveChanges fail for User {userId}: {e}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Device mit allen Abhängigkeiten löschen
        ///     DataConnector hat (noch) Probleme damit
        /// </summary>
        /// <param name="db"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteDevice(Db db, long deviceId)
        {
            if (db == null)
            {
                throw new NullReferenceException("[DeleteDevice] db is NULL");
            }

            var dbDevice = await db.TblDevices.FirstOrDefaultAsync(d => d.Id == deviceId).ConfigureAwait(false);
            if (dbDevice == null)
            {
                Logging.Log.LogWarning($"[DeleteDevice] Device {deviceId} does not exist.");
                return false;
            }

            dbDevice.TblUserId = null;
            db.TblDevices.Remove(dbDevice);

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[DeleteDevice] DbSaveChanges fail for Device {deviceId}: {e}");
                return false;
            }

            return true;
        }
    }
}