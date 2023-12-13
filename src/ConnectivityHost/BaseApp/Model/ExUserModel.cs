// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;

namespace ConnectivityHost.BaseApp.Model
{
    /// <summary>
    /// </summary>
    public class ExUserModel : TableUser
    {
        #region Properties

        /// <summary>
        ///     last Login
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     DisplayName
        /// </summary>
        public string DisplayName => $"{FirstName} {LastName} ({LoginName})";

        #endregion
    }
}