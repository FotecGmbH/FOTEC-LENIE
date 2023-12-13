// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Dc.Core;
using Exchange.Enum;

namespace Database.Tables
{
    /// <summary>
    ///     <para>Organisation Tabelle für DB - Kann Firma, Gemeinde, Abteilung ... sein</para>
    ///     Klasse TableOrganization. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("Organization")]
    public class TableOrganization : IDcChangeTracking
    {
        #region Properties

        /// <summary>
        ///     Device ID für DB
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        ///     Ist archiviert.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        ///     Version der Zeile. Wird automatisch durch den SQL Server aktualisiert
        ///     https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application#add-an-optimistic-concurrency-property-to-the-department-entity
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        [Timestamp]
        public byte[] DataVersion { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Name Firma, Gemeinde, Abteilung ...
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Postleitzahl
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        ///     Typ der Organisation
        /// </summary>
        public EnumOrganizationTypes OrganizationType { get; set; } = EnumOrganizationTypes.Organization;

        /// <summary>
        ///     Permissions der Firma
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
        public ICollection<TablePermission> TblPermissions { get; set; } = new List<TablePermission>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        ///     AccessToken des Benutzers
        /// </summary>
        public ICollection<TableAccessToken> TblAccessToken { get; set; } = new List<TableAccessToken>();

        /// <summary>
        ///     Ideen in dieser Gemeinde
        /// </summary>
        public ICollection<TableIdeaOrganization> TblIdeaOrganisations { get; set; } = new List<TableIdeaOrganization>();

        #endregion
    }
}