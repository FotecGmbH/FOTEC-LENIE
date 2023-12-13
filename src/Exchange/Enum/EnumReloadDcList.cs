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
    ///     <para>Client anweisen eine Dc-Liste komplett neu zu landen</para>
    ///     Klasse EnumReloadDcList. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumReloadDcList
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Obsolete("Sync")] Organization,
        [Obsolete("Sync")] OrganizationUsers,
        [Obsolete("Sync")] UsersAll,
        [Obsolete("Sync")] Ideas,
        IdeaNeeds,
        IdeaHelpers,
        [Obsolete("Sync")] Reports,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}