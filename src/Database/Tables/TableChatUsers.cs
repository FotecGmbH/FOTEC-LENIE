// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Dc.Core;

namespace Database.Tables;

/// <summary>
///     <para>DESCRIPTION</para>
///     Klasse TableChatUsers. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("ChatUsers")]
public class TableChatUsers : IDcChangeTracking
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
    ///     Ist archiviert.
    /// </summary>
    public bool IsArchived { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     User des Chat
    /// </summary>
    public long TblUserId { get; set; }

    /// <summary>
    ///     User des Chats
    /// </summary>
    [ForeignKey(nameof(TblUserId))]
    public virtual TableUser TblUser { get; set; } = null!;

    /// <summary>
    ///     Chat Id
    /// </summary>
    public long TblChatId { get; set; }

    /// <summary>
    ///     Charts des User
    /// </summary>
    [ForeignKey(nameof(TblChatId))]
    public virtual TableChat TblChat { get; set; } = null!;

    #endregion
}