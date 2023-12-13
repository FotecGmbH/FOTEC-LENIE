// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.ComponentModel;
using Biss.Common;
using Biss.Interfaces;

namespace Exchange.Model.User
{
    /// <summary>
    ///     <para>Gerät eines User</para>
    ///     Klasse ExUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUserDevice : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Datenbank Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Name des Gerätes - vom User gesetzt
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the model of the device.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the manufacturer of the device.
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        ///     Plattform des Gerätes
        /// </summary>
        public EnumPlattform Plattform { get; set; }

        /// <summary>
        ///     Geräteart
        /// </summary>
        public EnumDeviceIdiom DeviceIdiom { get; set; }

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