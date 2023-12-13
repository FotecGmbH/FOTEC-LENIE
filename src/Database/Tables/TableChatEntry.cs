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

namespace Database.Tables;

/// <summary>
///     <para>Einzelner Eintrag in einem bestimmten Chat</para>
///     Klasse TableChatEntry. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("ChatEntry")]
public class TableChatEntry : IDcChangeTracking
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Chat Eintrag
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     Zeitpunkt
    /// </summary>
    public DateTime EntryDateTimeUtc { get; set; }

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

    #endregion


    #region Foreign Keys

    /// <summary>
    ///     Eintrag gehört zum Chat
    /// </summary>
    [ForeignKey(nameof(TblChat))]
    public long? TblChatId { get; set; }

    /// <summary>
    ///     Verfasser des Eintrags
    /// </summary>
    public virtual TableChat? TblChat { get; set; }

    /// <summary>
    ///     Verfasser des Eintrags
    /// </summary>
    [ForeignKey(nameof(TblUserWriter))]
    public long? TblUserWriterId { get; set; }

    /// <summary>
    ///     Verfasser des Eintrags
    /// </summary>
    public virtual TableUser? TblUserWriter { get; set; }

    #endregion
}