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

namespace Database.Tables;

/// <summary>
///     <para>Tabelle für Dateien</para>
///     Klasse TableFile. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("File")]
public class TableFile : IDcChangeTracking
{
    #region Properties

    /// <summary>
    ///     Db Id
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
    ///     Dateiname inkl. Endung
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Name im Blob
    /// </summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>
    ///     Öffentlicher Link im CDN
    /// </summary>
    public string PublicLink { get; set; } = string.Empty;

    /// <summary>
    ///     Optionale, zusätzliche Daten zur Datei
    /// </summary>
    public string AdditionalData { get; set; } = string.Empty;

    /// <summary>
    ///     Falls die Datei direkt in der DB gesichert werden sollte
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
    public byte[]? Bytes { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

    #region Bild zu Idee

    /// <summary>
    ///     Ideen mit diesem Bild
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableIdea> TblIdeas { get; set; } = new List<TableIdea>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion

    #region Profilbild User

    /// <summary>
    ///     User mit diesem Bild als Profilbild
    /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only
    public virtual ICollection<TableUser> TblUserImages { get; set; } = new List<TableUser>();
#pragma warning restore CA2227 // Collection properties should be read only

    #endregion

    #endregion
}