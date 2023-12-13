// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Biss.Interfaces;
using Exchange.Enum;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.Organization
{
    /// <summary>
    ///     <para>ExOrganization</para>
    ///     Klasse ExOrganization. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExOrganization : IBissModel, IBissSelectable
    {
        #region Properties

        /// <summary>
        ///     DB Id
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        ///     Name Firma, Gemeinde, Abteilung ...
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Postleitzahl
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        ///     Anzahl der Ideen für diese Gemeinde
        /// </summary>
        public long IdeasCount { get; set; }

        /// <summary>
        ///     Anzahl User in der Gemeinde
        /// </summary>
        public long UsersCount { get; set; }

        /// <summary>
        ///     Typ der Organisation
        /// </summary>
        public EnumOrganizationTypes OrganizationType { get; set; } = EnumOrganizationTypes.Organization;

        /// <summary>
        ///     Organisation Token
        /// </summary>
        public ObservableCollection<ExAccessToken> Tokens { get; set; } = new ObservableCollection<ExAccessToken>();

        /// <summary>
        ///     Ausgewählt
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann selektiert werden?
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Aktueller User ist Admin für die Gemeinde
        /// </summary>
        public bool UserIsAdmin { get; set; }

        /// <summary>
        ///     Aktueller User ist UserPlus für die Gemeinde
        /// </summary>
        public bool UserIsUserPlus { get; set; }

        /// <summary>
        ///     Aktueller User ist in der Gemeinde
        /// </summary>
        public bool UserIsInOrg { get; set; }

        /// <summary>
        ///     PLZ und Name in einem String
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Name), nameof(PostalCode))]
        public string NamePlzString => $"{PostalCode}, {Name}";

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

#pragma warning disable CS0414
        /// <inheritdoc />
        public event EventHandler<BissSelectableEventArgs> Selected = null!;
#pragma warning restore CS0414

        #endregion
    }
}