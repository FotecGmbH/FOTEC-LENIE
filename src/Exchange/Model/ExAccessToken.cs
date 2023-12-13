// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model
{
    /// <summary>
    ///     <para>Zugriffstoken der Benutzer</para>
    ///     Klasse ExAccessToken. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExAccessToken : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Id des Token in der DB
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        ///     Gültig bis
        /// </summary>
        public DateTime GuiltyUntilUtc { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}