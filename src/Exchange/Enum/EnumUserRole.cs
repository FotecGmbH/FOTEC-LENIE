// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Enum für Benutzerrolle</para>
    ///     Klasse EnumUserRole. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumUserRole
    {
        /// <summary>
        ///     Normaler Benutzer in Organization
        /// </summary>
        User,

        /// <summary>
        ///     Moderator in Organization
        /// </summary>
        UserPlus,

        /// <summary>
        ///     Administrator der Organization
        /// </summary>
        Admin,
    }
}