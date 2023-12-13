// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Exchange.Model;

namespace Exchange.Interfaces
{
    /// <summary>
    ///     <para>Device Infos Interface</para>
    ///     Klasse IDeviceInfos. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IDeviceInfos
    {
        /// <summary>
        ///     Plattform Infos auslesen
        /// </summary>
        /// <returns></returns>
        ExDeviceInfo GetInfosDeviceInfo();
    }
}