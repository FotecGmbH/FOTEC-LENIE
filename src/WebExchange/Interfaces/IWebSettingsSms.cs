// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Apps.Interfaces;

namespace WebExchange.Interfaces
{
    /// <summary>
    ///     <para>Settings für SMS</para>
    ///     Interface IWebSettingsSms. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IWebSettingsSms : IAppSettingsBase
    {
        #region Properties

        /// <summary>
        ///     Sender Account
        /// </summary>
        public string AccountSid { get; }

        /// <summary>
        ///     Auth Token
        /// </summary>
        public string AuthToken { get; }

        /// <summary>
        ///     Senden mit Telefonnummer
        /// </summary>
        public string SenderNumber { get; }

        #endregion
    }
}