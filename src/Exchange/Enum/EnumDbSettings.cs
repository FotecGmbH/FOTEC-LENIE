// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Allgemeine Datenbank Settings für das Projekt</para>
    ///     Klasse EnumDbSettings. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDbSettings
    {
        /// <summary>
        ///     Details zu den AGB's
        /// </summary>
        Agb = 0,

        /// <summary>
        ///     Aktuelle Version in den Stores
        /// </summary>
        CurrentAppVersion,

        /// <summary>
        ///     Minimale App die auf den Clients sein muss
        /// </summary>
        MinAppVersion,

        /// <summary>
        ///     Allgemeine Meldung für die Clients (wird einmal beim App-Start angezeigt)
        /// </summary>
        CommonMessage,

        /// <summary>
        ///     Gemeinde Ids für Wöchentlichen Report
        /// </summary>
        TownIds,

        /// <summary>
        ///     Empfänger für Wöchentlichen Report
        /// </summary>
        ReportReceivers,
    }
}