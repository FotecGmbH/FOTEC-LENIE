﻿// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange.Enum;
using Exchange.Model;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     <para>Infos an Geräte senden</para>
    ///     Klasse DcCommonCommandsClient. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class ServerRemoteCalls
    {
        /// <summary>
        ///     Allgemeine Meldung via DC senden
        /// </summary>
        /// <param name="msg">Meldung</param>
        /// <param name="title">Titel der Meldung</param>
        /// <param name="deviceId">[Optional] Device Id</param>
        /// <param name="userId">[Optional] User Id</param>
        /// <returns></returns>
        public async Task SendMessage(string msg, string title, long? deviceId, long? userId)
        {
            var data = new DcCommonData
                       {
                           Key = EnumDcCommonCommandsClient.CommonMsg.ToString(),
                           Value = new EcDcCommonMessage {Message = msg, Title = title}.ToJson()
                       };

            if (deviceId.HasValue)
            {
                Logging.Log.LogTrace($"[DC SendMessage] {msg},{title} to device {deviceId.Value}");
                await ClientConnection.SendCommonDataToDevice(deviceId.Value, data).ConfigureAwait(false);
            }
            else if (userId.HasValue)
            {
                Logging.Log.LogTrace($"[DC SendMessage] {msg},{title} to all devices of user {userId.Value}");
                await ClientConnection.SendCommonToUser(userId.Value, data).ConfigureAwait(false);
            }
            else
            {
                Logging.Log.LogTrace($"[DC SendMessage] {msg},{title} to all devices");
                await ClientConnection.SendCommonData(data).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Client anweisen eine bestimmte Dc Liste komplett neu zu laden
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task SendReloadList(EnumReloadDcList list)
        {
            Logging.Log.LogTrace($"[{nameof(ServerRemoteCalls)}]({nameof(SendReloadList)}): Send Reload List {list} to all Devices.");
            var data = new DcCommonData
                       {
                           Key = EnumDcCommonCommandsClient.ReloadDcList.ToString(),
                           Value = list.ToString()
                       };
            await ClientConnection.SendCommonData(data).ConfigureAwait(false);
        }
    }
}