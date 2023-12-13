// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Aktuelle App Typen für Projelte mit mehr als einer Applikationen</para>
    ///     Klasse EnumAppType. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumAppType
    {
        /// <summary>
        ///     End (User) Applikation
        /// </summary>
        User = 0,

        /// <summary>
        ///     Biss App im Connectivity Host mit Dev Infos
        /// </summary>
        ConnectivityHost,

        /// <summary>
        ///     Admin (Backend) Applikation
        /// </summary>
        Admin
    }
}