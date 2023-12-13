// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Serialize;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;
// ReSharper disable once RedundantUsingDirective
using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Dc.Server;
using Exchange.Model;
using Exchange.Model.FutureWishes;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Model.Report;
using Exchange.Model.Settings;
using Exchange.Model.User;

namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>ServerRemoteCallBase</para>
    ///     Klasse ServerRemoteCallBase. (C) 2020 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract class ServerRemoteCallBase
    {
        /// <summary>
        ///     Zugriff auf die Sync-Funktionen der Listen
        /// </summary>
        private static Func<string, long, long, DcListSyncData, DcListSyncProperties, Task<DcListSyncTransferData>> _syncFunc = null!;

        #region Properties

        /// <summary>
        ///     Zugriff auf die Kommunikation mit den angemeldeten Clients
        /// </summary>
        public IDcConnections ClientConnection { get; set; } = null!;

        #endregion

        /// <summary>
        ///     Workaround um in den Server-Funktionen den Zugriff auf alle angemeldeten Clients zu ermöglichen
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="syncFunc"></param>
        public void SetClientConnection(object connection, Func<string, long, long, DcListSyncData, DcListSyncProperties, Task<DcListSyncTransferData>> syncFunc = null!)
        {
            if (connection is IDcConnections con)
            {
                ClientConnection = con;
            }
            else
            {
                throw new InvalidCastException();
            }

            if (syncFunc != null! && _syncFunc == null!)
            {
                _syncFunc = syncFunc;
            }
        }

        #region Sendefunktionen

        /// <summary>
        ///     Daten an DcExDeviceInfo senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        public async Task<int> SendDcExDeviceInfo(ExDeviceInfo data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            return await SendInternal("DcExDeviceInfo", data.ToJson(), deviceId, userId, excludeDeviceId).ConfigureAwait(false);
        }


        /// <summary>
        ///     Daten an DcExUser senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        public async Task<int> SendDcExUser(ExUser data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            return await SendInternal("DcExUser", data.ToJson(), deviceId, userId, excludeDeviceId).ConfigureAwait(false);
        }


        /// <summary>
        ///     Daten an DcExUserPassword senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        public async Task<int> SendDcExUserPassword(ExUserPassword data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            return await SendInternal("DcExUserPassword", data.ToJson(), deviceId, userId, excludeDeviceId).ConfigureAwait(false);
        }


        /// <summary>
        ///     Daten an DcExLocalAppData senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        public async Task<int> SendDcExLocalAppData(ExLocalAppSettings data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            return await SendInternal("DcExLocalAppData", data.ToJson(), deviceId, userId, excludeDeviceId).ConfigureAwait(false);
        }


        /// <summary>
        ///     Daten an DcExSettingsInDb senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        public async Task<int> SendDcExSettingsInDb(ExSettingsInDb data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            return await SendInternal("DcExSettingsInDb", data.ToJson(), deviceId, userId, excludeDeviceId).ConfigureAwait(false);
        }


        /// <summary>
        ///     Listen Daten an DcExOrganization senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExOrganization verwenden!")]
        public async Task<int> SendDcExOrganization(List<DcServerListItem<ExOrganization>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExOrganization", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExOrganization an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExOrganization(DcListSyncResultData<ExOrganization> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExOrganization")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExOrganization", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExOrganization has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExOrganization an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExOrganization(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExOrganization"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExOrganization"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExOrganization", device.DeviceId, device.UserId, device.ListLastSyncData["DcExOrganization"].DcListSyncData!, device.ListLastSyncData["DcExOrganization"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExOrganization")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExOrganization", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExOrganizationUsers senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExOrganizationUsers verwenden!")]
        public async Task<int> SendDcExOrganizationUsers(List<DcServerListItem<ExOrganizationUser>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExOrganizationUsers", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExOrganizationUsers an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExOrganizationUsers(DcListSyncResultData<ExOrganizationUser> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExOrganizationUsers")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExOrganizationUsers", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExOrganizationUsers has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExOrganizationUsers an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExOrganizationUsers(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExOrganizationUsers"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExOrganizationUsers"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExOrganizationUsers", device.DeviceId, device.UserId, device.ListLastSyncData["DcExOrganizationUsers"].DcListSyncData!, device.ListLastSyncData["DcExOrganizationUsers"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExOrganizationUsers")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExOrganizationUsers", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExAllUsers senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExAllUsers verwenden!")]
        public async Task<int> SendDcExAllUsers(List<DcServerListItem<ExUser>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExAllUsers", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExAllUsers an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExAllUsers(DcListSyncResultData<ExUser> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExAllUsers")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExAllUsers", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExAllUsers has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExAllUsers an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExAllUsers(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExAllUsers"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExAllUsers"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExAllUsers", device.DeviceId, device.UserId, device.ListLastSyncData["DcExAllUsers"].DcListSyncData!, device.ListLastSyncData["DcExAllUsers"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExAllUsers")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExAllUsers", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExFutureWishes senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExFutureWishes verwenden!")]
        public async Task<int> SendDcExFutureWishes(List<DcServerListItem<ExFutureWish>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExFutureWishes", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExFutureWishes an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExFutureWishes(DcListSyncResultData<ExFutureWish> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExFutureWishes")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExFutureWishes", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExFutureWishes has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExFutureWishes an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExFutureWishes(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExFutureWishes"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExFutureWishes"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExFutureWishes", device.DeviceId, device.UserId, device.ListLastSyncData["DcExFutureWishes"].DcListSyncData!, device.ListLastSyncData["DcExFutureWishes"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExFutureWishes")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExFutureWishes", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExIdeas senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExIdeas verwenden!")]
        public async Task<int> SendDcExIdeas(List<DcServerListItem<ExIdea>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExIdeas", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeas an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExIdeas(DcListSyncResultData<ExIdea> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExIdeas")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExIdeas", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExIdeas has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeas an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExIdeas(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExIdeas"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExIdeas"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExIdeas", device.DeviceId, device.UserId, device.ListLastSyncData["DcExIdeas"].DcListSyncData!, device.ListLastSyncData["DcExIdeas"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExIdeas")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExIdeas", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExIdeaNeeds senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExIdeaNeeds verwenden!")]
        public async Task<int> SendDcExIdeaNeeds(List<DcServerListItem<ExIdeaNeed>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExIdeaNeeds", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeaNeeds an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExIdeaNeeds(DcListSyncResultData<ExIdeaNeed> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExIdeaNeeds")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExIdeaNeeds", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExIdeaNeeds has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeaNeeds an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExIdeaNeeds(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExIdeaNeeds"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExIdeaNeeds"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExIdeaNeeds", device.DeviceId, device.UserId, device.ListLastSyncData["DcExIdeaNeeds"].DcListSyncData!, device.ListLastSyncData["DcExIdeaNeeds"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExIdeaNeeds")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExIdeaNeeds", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExIdeaHelpers senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExIdeaHelpers verwenden!")]
        public async Task<int> SendDcExIdeaHelpers(List<DcServerListItem<ExIdeaHelper>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExIdeaHelpers", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeaHelpers an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExIdeaHelpers(DcListSyncResultData<ExIdeaHelper> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExIdeaHelpers")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExIdeaHelpers", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExIdeaHelpers has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExIdeaHelpers an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExIdeaHelpers(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExIdeaHelpers"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExIdeaHelpers"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExIdeaHelpers", device.DeviceId, device.UserId, device.ListLastSyncData["DcExIdeaHelpers"].DcListSyncData!, device.ListLastSyncData["DcExIdeaHelpers"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExIdeaHelpers")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExIdeaHelpers", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExReports senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExReports verwenden!")]
        public async Task<int> SendDcExReports(List<DcServerListItem<ExReport>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExReports", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExReports an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExReports(DcListSyncResultData<ExReport> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExReports")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExReports", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExReports has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExReports an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExReports(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExReports"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExReports"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExReports", device.DeviceId, device.UserId, device.ListLastSyncData["DcExReports"].DcListSyncData!, device.ListLastSyncData["DcExReports"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExReports")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExReports", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Listen Daten an DcExIntros senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="data">Daten</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        [Obsolete("Bitte SyncDcExIntros verwenden!")]
        public async Task<int> SendDcExIntros(List<DcServerListItem<ExIntroItem>> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            var tmp = data.Select(item => item.ToDcTransferListItem()).ToList();
            return await SendInternal("DcExIntros", tmp.ToJson(), deviceId, userId).ConfigureAwait(false);
        }

        /// <summary>
        ///     Datenänderungen für DcExIntros an Client weiter geben
        /// </summary>
        /// <param name="data">Sync-Daten</param>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<int> SyncDcExIntros(DcListSyncResultData<ExIntroItem> data, long? deviceId = null, long? userId = null, long? excludeDeviceId = null)
        {
            if (data is IDcListSyncResultData result)
            {
                var tmp = result.ToDcListSyncTransferData();
                var tmp2 = new DcListSyncResult("DcExIntros")
                           {
                               Data = tmp
                           };

                return await SendInternal("DcExIntros", tmp2.ToJson(), deviceId, userId, excludeDeviceId, true).ConfigureAwait(false);
            }

            throw new ArgumentException("SyncDcExIntros has wrong Data");
        }

        /// <summary>
        ///     Datenänderungen für DcExIntros an Client weiter geben
        /// </summary>
        /// <param name="userIds">Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf</param>
        /// <returns></returns>
        public async Task<int> SyncDcExIntros(List<long> userIds, long? excludeDeviceId = null)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            var couter = 0;
            foreach (var userId in userIds)
            {
                var clientDevices = ClientConnection.GetClients().Where(w => w.UserId == userId);
                foreach (var device in clientDevices)
                {
                    if (excludeDeviceId.HasValue && excludeDeviceId.Value == device.DeviceId)
                    {
                        continue;
                    }

                    if (!device.ListLastSyncData.ContainsKey("DcExIntros"))
                    {
                        //Noch kein Sync
                        continue;
                    }

                    if (device.ListLastSyncData["DcExIntros"].DcListSyncData == null!)
                    {
                        continue;
                    }

                    var data = await _syncFunc("DcExIntros", device.DeviceId, device.UserId, device.ListLastSyncData["DcExIntros"].DcListSyncData!, device.ListLastSyncData["DcExIntros"].DcListSyncProperties!).ConfigureAwait(false);
                    var tmp2 = new DcListSyncResult("DcExIntros")
                               {
                                   Data = data
                               };
                    await SendInternal("DcExIntros", tmp2.ToJson(), device.DeviceId, device.UserId, excludeDeviceId, true).ConfigureAwait(false);
                    couter++;
                }
            }

            return couter;
        }


        /// <summary>
        ///     Daten senden.
        ///     Wenn deviceId und userId null sind werden die Daten an alle Geräte gesendet
        /// </summary>
        /// <param name="dpId">Datenpunkt Id</param>
        /// <param name="data">Daten in Json serialisiert</param>
        /// <param name="deviceId">An ein bestimmtes Gerät</param>
        /// <param name="userId">An einen bestimmten Benutzer</param>
        /// <param name="excludeDeviceId">An alle Geräte auf Dieses</param>
        /// <param name="sync">Neuer Sync Mode ab Version 8.x</param>
        /// <returns>Anzahl der erreichten Geräte</returns>
        private async Task<int> SendInternal(string dpId, string data, long? deviceId, long? userId, long? excludeDeviceId = null, bool sync = false)
        {
            var d = new DcResult(dpId, sync) {JsonData = data};
            d.Checksum = DcChecksum.Generate(d.JsonData);

            var result = 0;

            if (deviceId == null && userId == null)
            {
                result = await ClientConnection.SendData(d, excludeDeviceId).ConfigureAwait(false);
            }
            else if (userId != null)
            {
                result = await ClientConnection.SendDataToUser(userId.Value, d, excludeDeviceId).ConfigureAwait(false);
            }
            else if (deviceId != null)
            {
                if (await ClientConnection.SendDataToDevice(deviceId.Value, d).ConfigureAwait(false))
                {
                    result = 1;
                }
            }

            return result;
        }

        #endregion
    }
}