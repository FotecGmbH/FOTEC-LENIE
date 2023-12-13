// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model.Settings
{
    /// <summary>
    ///     <para>Lokale Einstellungen in der App</para>
    ///     Klasse ExLocalAppSettings. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExLocalAppSettings : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Dunkles Design verwenden
        /// </summary>
        public bool UseDarkTheme { get; set; } = true;

        /// <summary>
        ///     Letzte gewählte Organisation
        /// </summary>
        public long? LastSelectedOrganization { get; set; }

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