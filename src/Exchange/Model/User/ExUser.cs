// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Biss.Interfaces;
using Exchange.Enum;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.User
{
    /// <summary>
    ///     <para>Benutzerstammdaten</para>
    ///     Klasse ExUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUser : IBissModel, IComparable<ExUser>, IComparable
    {
        #region Properties

        /// <summary>
        ///     Vorname
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        ///     Nachname
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        ///     Name
        /// </summary>
        [DependsOn(nameof(FirstName), nameof(LastName))]
        [JsonIgnore]
        public string Fullname => $"{FirstName} {LastName}";

        /// <summary>
        ///     Name für Login (Email, Telefonnummer, etc)
        /// </summary>
        public string LoginName { get; set; } = string.Empty;

        /// <summary>
        ///     Telephonnummer des Users
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        ///     Bild des User
        /// </summary>
        public string UserImageLink { get; set; } = string.Empty;

        /// <summary>
        ///     Bild id in Files Tabelle
        /// </summary>
        public long UserImageDbId { get; set; }

        /// <summary>
        ///     Bild OK?
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(UserImageLink))]
        public bool HasImage => !string.IsNullOrEmpty(UserImageLink);

        /// <summary>
        ///     Bei einem neuem Benutzer das Passwort als Hash
        /// </summary>
        public string PasswordHash4NewUser { get; set; } = string.Empty;

        /// <summary>
        ///     Standardsprache des Benutzers
        /// </summary>
        public string DefaultLanguage { get; set; } = string.Empty;

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

        /// <summary>
        ///     Geräte eines User
        /// </summary>
        public ObservableCollection<ExUserDevice> UserDevices { get; set; } = new ObservableCollection<ExUserDevice>();

        /// <summary>
        ///     Login bestätigt (Email, Telefonnummer, etc)
        /// </summary>
        public bool LoginConfirmed { get; set; }

        /// <summary>
        ///     Telefonnummer bestätigt
        /// </summary>
        public bool PhoneConfirmed { get; set; }

        /// <summary>
        ///     Sms bestätigungscode
        /// </summary>
        public string SmsCode { get; set; } = string.Empty;

        /// <summary>
        ///     Account ist gesperrt
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        ///     Registrierte Tags für Push - Json Objekt der Liste
        /// </summary>
        public string? PushTags { get; set; }

        /// <summary>
        ///     Sys-Admin Account
        /// </summary>
        public bool IsSysAdmin { get; set; }

        /// <summary>
        ///     Sys Admin oder Admin in mindestens einer Organisation
        /// </summary>
        [JsonIgnore]
        public bool IsAdmin
        {
            get
            {
                if (IsSysAdmin)
                {
                    return true;
                }

                if (Permissions != null!)
                {
                    return Permissions.Any(a => a.UserRole == EnumUserRole.Admin);
                }

                return false;
            }
        }

        /// <summary>
        ///     Sys Admin, Admin oder UserPlus in mindestens einer Organisation
        /// </summary>
        [JsonIgnore]
        public bool IsUserPlus
        {
            get
            {
                if (IsSysAdmin)
                {
                    return true;
                }

                if (Permissions != null!)
                {
                    return Permissions.Any(a => a.UserRole != EnumUserRole.User);
                }

                return false;
            }
        }

        /// <summary>
        ///     Datenbank Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Berechtigungen auf Firmenebene - nur relevat für Admin == False
        /// </summary>
        public List<ExUserPermission> Permissions { get; set; } = new List<ExUserPermission>();

        /// <summary>
        ///     User Token
        /// </summary>
        public ObservableCollection<ExAccessToken> Tokens { get; set; } = new ObservableCollection<ExAccessToken>();

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This instance precedes <paramref name="obj" /> in the sort order.
        ///     Zero
        ///     This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///     Greater than zero
        ///     This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ExUser other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ExUser)}");
        }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This instance precedes <paramref name="other" /> in the sort order.
        ///     Zero
        ///     This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///     Greater than zero
        ///     This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(ExUser? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var isSysAdminComparison = IsSysAdmin.CompareTo(other.IsSysAdmin);
            if (isSysAdminComparison != 0)
            {
                return isSysAdminComparison;
            }

            return string.Compare(LastName, other.LastName, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}