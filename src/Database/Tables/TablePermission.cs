// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Dc.Core;
using Exchange.Enum;

namespace Database.Tables;

/// <summary>
///     <para>Berechtigung eines Benutzers</para>
///     Klasse TableUserPermission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Permission")]
public class TablePermission : IDcChangeTracking
{
    #region Properties

    /// <summary>
    ///     DB Id
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
    ///     Recht
    /// </summary>
    public EnumUserRight UserRight { get; set; }

    /// <summary>
    ///     Die Rolle des Benutzers.
    /// </summary>
    public EnumUserRole UserRole { get; set; }

    /// <summary>
    ///     Hauptwohnort - einer je User
    /// </summary>
    public bool MainOrganization { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     User dem die Berechtigung zugewiesen ist
    /// </summary>
    public long TblUserId { get; set; }

    /// <summary>
    ///     User dem die Berechtigung zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblUserId))]
    public virtual TableUser TblUser { get; set; } = null!;

    /// <summary>
    ///     Organisation die der Berechtigung zugewiesen ist
    /// </summary>
    public long TblOrganizationId { get; set; }

    /// <summary>
    ///     Firma die der Berechtigung zugewiesen ist
    /// </summary>
    [ForeignKey(nameof(TblOrganizationId))]
    public virtual TableOrganization TblOrganization { get; set; } = null!;

    #endregion
}