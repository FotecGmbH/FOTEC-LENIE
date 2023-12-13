// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     Email Notification User
    /// </summary>
    public class EMailNotificationUser
    {
        #region Properties

        /// <summary>
        ///     Vorname
        /// </summary>
        public string Firstname { get; set; } = string.Empty;

        /// <summary>
        ///     Nachname
        /// </summary>
        public string Lastname { get; set; } = string.Empty;

        /// <summary>
        ///     Beispieltext
        /// </summary>
        public string Text { get; set; } = string.Empty;

        #endregion
    }
}