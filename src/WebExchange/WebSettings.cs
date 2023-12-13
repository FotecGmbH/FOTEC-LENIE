// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss;
using Biss.Apps.Service.Push;
using Biss.Attributes;
using WebExchange.Interfaces;

namespace WebExchange
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class WebSettings :
        IWebSettingsAzureFiles,
        IAppServiceSettingPush,
        IAppSettingsDataBase,
        IAppSettingsEMail,
        IWebSettingsSms
    {
        private static WebSettings _current = null!;

        /// <summary>
        ///     Get default Settings for WebSettings
        /// </summary>
        /// <returns></returns>
        public static WebSettings Current()
        {
            if (_current == null!)
            {
                _current = new WebSettings();
            }

            return _current;
        }

        #region IWebSettingsAzureFiles

        /// <summary>
        ///     Connection string für den Blob
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string BlobConnectionString => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:BlobConnectionString");

        /// <summary>
        ///     Container Name im Blob
        /// </summary>
        public string BlobContainerName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:BlobContainerName");

        /// <summary>
        ///     Cdn link oder public Bloblink für Filelink
        /// </summary>
        public string CdnLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:CdnLink");

        #endregion IWebSettingsAzureFiles

        #region IAppServiceSettingPush

        /// <summary>
        ///     Push - Firebase Project Id - <inheritdoc cref="IAppServiceSettingPush.PushProjectId" />
        /// </summary>
        public string PushProjectId => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:PushProjectId");

        /// <summary>
        ///     Push - Firebase Service Account Id - <inheritdoc cref="IAppServiceSettingPush.PushServiceAccountId" />
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string PushServiceAccountId => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:PushServiceAccountId");

        /// <summary>
        ///     Push - Firebase Private Key Id - <inheritdoc cref="IAppServiceSettingPush.PushPrivateKeyId" />
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string PushPrivateKeyId => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:PushPrivateKeyId");

        /// <summary>
        ///     Push - Firebase Private Key - <inheritdoc cref="IAppServiceSettingPush.PushPrivateKey" />
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string PushPrivateKey => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:PushPrivateKey");

        #endregion IAppServiceSettingPush

        #region IAppSettingsDataBase

        public string ConnectionString => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:ConnectionString");

        /// <summary>
        ///     Datenbank
        /// </summary>
        public string ConnectionStringDb => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:ConnectionStringDb");

        /// <summary>
        ///     Datenbank-Server
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string ConnectionStringDbServer => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:ConnectionStringDbServer");

        /// <summary>
        ///     Datenbank User
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string ConnectionStringUser => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:ConnectionStringUser");

        /// <summary>
        ///     Datenbank User Passwort
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string ConnectionStringUserPwd => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:ConnectionStringUserPwd");

        #endregion IAppSettingsDataBase

        #region IAppSettingsEMail

        /// <summary>
        ///     Sendgrid Email Mit welcher E-Mail Adresse wird gesendet
        /// </summary>
        public string SendEMailAs => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:SendEMailAs");

        /// <summary>
        ///     Sendgrid Email Welcher Name des Senders wird angezeigt
        /// </summary>
        public string SendEMailAsDisplayName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:SendEMailAsDisplayName");

        /// <summary>
        ///     Sendgrid Email Key
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string SendGridApiKey => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:SendGridApiKey");

        #endregion IAppSettingsEMail

        #region IWebSettingsSms

        /// <summary>
        ///     Twilio SMS Sender Account SID
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string AccountSid => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:AccountSid");

        /// <summary>
        ///     Twilio SMS Auth Token
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string AuthToken => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:AuthToken");

        /// <summary>
        ///     Twilio SMS Telefonnummer
        /// </summary>
        public string SenderNumber => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:WebExchange:SenderNumber");

        #endregion IWebSettingsSms
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
}