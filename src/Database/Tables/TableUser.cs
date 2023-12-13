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
///     <para>TableUser</para>
///     Klasse TableUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
[Table("User")]
public class TableUser : IDcChangeTracking
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
    ///     Vorname
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    ///     Nachname
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    ///     Telefonnummer
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Benutzer ist System-Administrator
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    ///     akzeptierte agb version
    /// </summary>
    public string AgbVersion { get; set; } = string.Empty;

    /// <summary>
    ///     Name für Login (Email, Telefonnummer, etc)
    /// </summary>
    public string LoginName { get; set; } = string.Empty;

    /// <summary>
    ///     Passworthash für Login
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    ///     Refreshtoken für Serverzugriff
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    ///     JwtToken für Serverzugriff
    /// </summary>
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>
    ///     Account gesperrt
    /// </summary>
    public bool Locked { get; set; }

    /// <summary>
    ///     Login bestätigt (Email)
    /// </summary>
    public bool LoginConfirmed { get; set; }

    /// <summary>
    ///     Telefonnummer bestätigt
    /// </summary>
    public bool PhoneConfirmed { get; set; }

    /// <summary>
    ///     Erstellt am
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    ///     Standardsprache
    /// </summary>
    public string DefaultLanguage { get; set; } = string.Empty;

    /// <summary>
    ///     GUID für Account-Bestätigung und Passwort-Reset (im Link enthalten)
    /// </summary>
    public string ConfirmationToken { get; set; } = string.Empty;

    /// <summary>
    ///     Sms bestätigungscode
    /// </summary>
    public string SmsCode { get; set; } = string.Empty;

    /// <summary>
    ///     Registrierte Tags für Push - Json Objekt der Liste
    /// </summary>
    public string? PushTags { get; set; }

    /// <summary>
    ///     Demo Einstellung für User (alle 10 min eine Push Nachricht senden)
    /// </summary>
    public bool Setting10MinPush { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei Termin
    /// </summary>
    public bool NotificationPushMeeting { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuem Chat
    /// </summary>
    public bool NotificationPushChat { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuem Kommentar
    /// </summary>
    public bool NotificationPushComment { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuem Like
    /// </summary>
    public bool NotificationPushLike { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuer Idee
    /// </summary>
    public bool NotificationPushIdea { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuer Unterstützung
    /// </summary>
    public bool NotificationPushSupport { get; set; }

    /// <summary>
    ///     Pushbenachrichtigung bei neuer Meldung
    /// </summary>
    public bool NotificationPushReport { get; set; }

    /// <summary>
    ///     email Benachrichtigung bei Termin
    /// </summary>
    public bool NotificationMailMeeting { get; set; }

    /// <summary>
    ///     email Benachrichtigung bei neuem Chat
    /// </summary>
    public bool NotificationMailChat { get; set; }

    /// <summary>
    ///     email Benachrichtigung bei neuem Report
    /// </summary>
    public bool NotificationMailReport { get; set; }

    #endregion

    #region Foreign Keys

    /// <summary>
    ///     UserProfilbild
    /// </summary>
    [ForeignKey(nameof(TblUserImage))]
    public long? TblUserImageId { get; set; }

    /// <summary>
    ///     UserProfilbild
    /// </summary>
    public virtual TableFile? TblUserImage { get; set; }

    /// <summary>
    ///     Geräte des Benutzers
    /// </summary>
    public virtual ICollection<TableDevice> TblDevices { get; set; } = new List<TableDevice>();

    /// <summary>
    ///     Permissions des Benutzers
    /// </summary>
    public virtual ICollection<TablePermission> TblPermissions { get; set; } = new List<TablePermission>();

    /// <summary>
    ///     AccessToken des Benutzers
    /// </summary>
    public ICollection<TableAccessToken> TblAccessToken { get; set; } = new List<TableAccessToken>();

    /// <summary>
    ///     Likes auf neue Features
    /// </summary>
    public ICollection<TableFutureWishLike> TblFutureWishLikes { get; set; } = new List<TableFutureWishLike>();

    /// <summary>
    ///     Ideen in dieses Users
    /// </summary>
    public ICollection<TableIdea> TblIdeas { get; set; } = new List<TableIdea>();

    /// <summary>
    ///     Ideen bei denen dieser User hilft
    /// </summary>
    public ICollection<TableIdeaHelper> TblIdeaHelpers { get; set; } = new List<TableIdeaHelper>();

    /// <summary>
    ///     Ideen die dieser User gefällt
    /// </summary>
    public ICollection<TableIdeaLike> TblIdeaLikes { get; set; } = new List<TableIdeaLike>();

    /// <summary>
    ///     Ideen bei denen dieser User reportet hat
    /// </summary>
    public ICollection<TableIdeaReport> TblIdeaReports { get; set; } = new List<TableIdeaReport>();

    /// <summary>
    ///     Mitgliede in Chats
    /// </summary>
    public virtual ICollection<TableChatUsers> TblChatUsers { get; set; } = new List<TableChatUsers>();

    #endregion
}