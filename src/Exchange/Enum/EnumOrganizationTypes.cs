// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Möchliche Organisationstypen</para>
    ///     Klasse EnumCompanyTypes. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumOrganizationTypes
    {
        /// <summary>
        ///     Spezielle Organisation (nur 1x in Db)
        /// </summary>
        NoOrganization,

        /// <summary>
        ///     Diese Organisation kann jeder User (auch User der App die keinen Account besitzen "Gäste") sehen
        /// </summary>
        PublicOrganization,

        /// <summary>
        ///     "Normale" Organisation
        /// </summary>
        Organization
    }
}