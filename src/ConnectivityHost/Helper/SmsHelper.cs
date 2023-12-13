// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Apps.Base;
using Biss.Log.Producer;
using Database;
using Database.Tables;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WebExchange;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Helper für SMS</para>
    ///     Klasse SmsHelper. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class SmsHelper
    {
        /// <summary>
        ///     Validierungs SMS
        /// </summary>
        /// <param name="user"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<bool> SendValidationSms(TableUser user, Db db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SmsCode = AppCrypt.GeneratePassword(7);
            _ = await db.SaveChangesAsync().ConfigureAwait(true);

            var msg = ResWebCommon.SmsRegistration + user.SmsCode;

            return SendSms(user.PhoneNumber, msg);
        }

        /// <summary>
        ///     SMS versenden
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool SendSms(string receiver, string text)
        {
            try
            {
                TwilioClient.Init(WebSettings.Current().AccountSid, WebSettings.Current().AuthToken);

                var message = MessageResource.Create(
                    new PhoneNumber(receiver),
                    from: new PhoneNumber(WebSettings.Current().SenderNumber),
                    body: text
                );

                if (message.ErrorCode.HasValue)
                {
                    Logging.Log.LogError($"[{nameof(SmsHelper)}]({nameof(SendSms)}): {message.ErrorCode} - {message.ErrorMessage}");
                }
                else
                {
                    Logging.Log.LogInfo($"[{nameof(SmsHelper)}]({nameof(SendSms)}): {message.Status} - {message.Sid}");
                }

                return !message.ErrorCode.HasValue;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(SmsHelper)}]({nameof(SendSms)}): {e}");
                return false;
            }
        }
    }
}