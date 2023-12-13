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
///     <para>Einzelner Eintrag in einem bestimmten Chat</para>
///     Klasse TableChatEntry. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("Chat")]
public class TableChat : IDcChangeTracking
{
    #region Properties

    /// <summary>
    ///     DB Id
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    ///     Version der Zeile. Wird automatisch durch den SQL Server aktualisiert
    ///     https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/handling-concurrency-with-the-entity-framework-in-an-asp-net-mvc-application#add-an-optimistic-concurrency-property-to-the-department-entity
    /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
    [Timestamp]
    public byte[] DataVersion { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

    /// <summary>
    ///     Name des Chats
    /// </summary>
    public string ChatName { get; set; } = string.Empty;

    /// <summary>
    ///     Ist archiviert.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    ///     Jeder kann dem Chat beitreten
    /// </summary>
    public bool PublicChat { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     Gehört zu Idee Id
    /// </summary>
    [ForeignKey(nameof(TblIdea))]
    public long? TblIdeaId { get; set; }

    /// <summary>
    ///     Gehört zu Idee
    /// </summary>
    public virtual TableIdea? TblIdea { get; set; }

    /// <summary>
    ///     Chat Mitglieder
    /// </summary>
    public virtual ICollection<TableChatUsers> TblChatUsers { get; set; } = new List<TableChatUsers>();

    /// <summary>
    ///     Einträge im Chat
    /// </summary>
    public virtual ICollection<TableChatEntry> TblChatEntries { get; set; } = new List<TableChatEntry>();

    #endregion
}