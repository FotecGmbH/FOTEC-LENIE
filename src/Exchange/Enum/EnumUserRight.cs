// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Benutzerrechte in einer einzelnen Firma</para>
    ///     Klasse EnumUserRight. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumUserRight
    {
        /// <summary>
        ///     Leserechte
        /// </summary>
        Read = 0,

        /// <summary>
        ///     Lese und Schreibrechte (bei Firmen-Admins immer)
        /// </summary>
        ReadWrite = 1
    }
}