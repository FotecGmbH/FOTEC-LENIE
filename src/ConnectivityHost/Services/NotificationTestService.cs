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
using Biss.AppConfiguration;
using Biss.Apps.Service.Push;
using Biss.Dc.Core;
using Biss.Dc.Server;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Exchange;
using WebExchange;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     Notification Test Service
    /// </summary>
    public class NotificationTestService
    {
        /// <summary>
        ///     Send Test Email
        /// </summary>
        /// <param name="razorEngine"></param>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<bool> SendTestEmail(ICustomRazorEngine razorEngine, TableUser user, string text)
        {
            if (razorEngine == null)
            {
                throw new ArgumentNullException(nameof(razorEngine));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var notificationUser = new EMailNotificationUser
                                   {
                                       Firstname = user.FirstName,
                                       Lastname = user.LastName,
                                       Text = text
                                   };

            var bem = WebConstants.Email;
            var subject = "Notification Test!";
            var receiverBetaInfo = "";
            var receiver = user.LoginName;
            List<string> ccReceifer = new()
                                      {
                                          "benni.moser@outlook.de"
                                      };
            Logging.Log.LogInfo("Send Notification Test Mail to: " + user.LoginName);
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                receiverBetaInfo = $"To: {receiver}, CC: ";
                foreach (var cc in ccReceifer)
                {
                    receiverBetaInfo += $"{cc}; ";
                }

                ccReceifer = new();
            }

            var htmlRendered = await razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailNotificationTest.cshtml", notificationUser).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Sends Test Dc Notification
        /// </summary>
        /// <param name="dcConnection"></param>
        /// <param name="user"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<int> SendTestDcNotification(IDcConnections dcConnection, TableUser? user, string text)
        {
            if (dcConnection == null)
            {
                throw new ArgumentNullException(nameof(dcConnection));
            }

            if (user == null)
            {
                return await dcConnection.SendCommonData(new DcCommonData {Key = "Test", Value = text}).ConfigureAwait(true);
            }


            var deviceIds = new List<long>();
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            deviceIds = db.TblDevices.Where(d => d.TblUserId == user.Id).Select(d => d.Id).ToList();

            var counter = 0;
            foreach (var deviceId in deviceIds)
            {
                var result = await dcConnection.SendCommonDataToDevice(deviceId, new DcCommonData {Key = "Test", Value = text}).ConfigureAwait(true);
                if (result)
                {
                    counter++;
                }
            }

            return counter;
        }

        /// <summary>
        ///     Send Test Push Notification
        /// </summary>
        /// <param name="push"></param>
        /// <param name="user"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<PushResult> SendTestPushNotification(PushService push, TableUser? user, string title, string value)
        {
            PushResult result;

            if (push == null)
            {
                throw new ArgumentNullException(nameof(push));
            }

            if (user == null)
            {
                result = await push.SendBroadcast(title, value).ConfigureAwait(true);
                return result;
            }

            var pushTokens = new List<string>();
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            pushTokens = db.TblDevices.Where(d => d.TblUserId == user.Id).Select(t => t.DeviceToken).ToList();

            if (pushTokens.Count > 0)
            {
                return await push.SendMessageToDevices(title, value, pushTokens).ConfigureAwait(true);
            }

            return new PushResult(0, 0, null!);
        }
    }
}