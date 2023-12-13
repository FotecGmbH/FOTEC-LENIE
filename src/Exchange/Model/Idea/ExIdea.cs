// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Biss.Apps.Map.Model;
using Biss.Interfaces;
using Exchange.Model.Organization;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.Idea
{
    /// <summary>
    ///     <para>Idee</para>
    ///     Klasse ExIdea. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdea : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Titel
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Beschreibung
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Erstellt am
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     ab wann
        /// </summary>
        public DateTime From { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

        /// <summary>
        ///     bis wann
        /// </summary>
        public DateTime To { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).AddHours(1);

        /// <summary>
        ///     Ist vergangene Idee
        /// </summary>
        public bool Archived { get; set; }

        /// <summary>
        ///     Nur an einem Tag
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(From), nameof(To))]
        public bool IsOnOneDay => From.Date == To.Date;

        /// <summary>
        ///     Datum UI friendly
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(From), nameof(To))]
        public string DateUi
        {
            get
            {
                if (IsOnOneDay)
                {
                    return $"{From.ToShortDateString()}";
                }

                return $"{From.ToShortDateString()}\n{To.ToShortDateString()}";
            }
        }

        /// <summary>
        ///     Time UI friendly
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(From), nameof(To))]
        public string TimeUi
        {
            get
            {
                if (IsOnOneDay)
                {
                    return $"{From.ToShortTimeString()} - {To.ToShortTimeString()}";
                }

                return $"{From.ToShortTimeString()}\n{To.ToShortTimeString()}";
            }
        }

        /// <summary>
        ///     Hat Zeitspanne
        /// </summary>
        public bool HasTimespan { get; set; }

        /// <summary>
        ///     Position
        /// </summary>
        public BissPosition Location { get; set; } = new BissPosition();

        /// <summary>
        ///     Addresse
        /// </summary>
        public string LocationAddress { get; set; } = string.Empty;

        /// <summary>
        ///     Hat Position
        /// </summary>
        public bool HasPosition { get; set; }

        /// <summary>
        ///     Position ist auf Karte
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(HasPosition), nameof(LocationAsText))]
        public bool LocationOnMap => HasPosition && !LocationAsText;

        /// <summary>
        ///     Position ist als Text angegeben
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(HasPosition), nameof(LocationAddress))]
        public bool LocationAsText => HasPosition && !string.IsNullOrWhiteSpace(LocationAddress);

        /// <summary>
        ///     User der die Idee angelegt hat
        /// </summary>
        public long CreatorUserId { get; set; }

        /// <summary>
        ///     Gemeinden der diese Idee zugehörig ist
        /// </summary>
        public List<ExOrganization> Companies { get; set; } = new List<ExOrganization>();

        /// <summary>
        ///     Name des Erstellers
        /// </summary>
        public string CreatorUserName { get; set; } = string.Empty;

        /// <summary>
        ///     Bildlink für Ersteller
        /// </summary>
        public string CreatorUserImage { get; set; } = string.Empty;

        /// <summary>
        ///     Hat der aktuelle User diese Idee geliket?
        /// </summary>
        public bool IsLiked { get; set; }

        /// <summary>
        ///     Anzahl Likes
        /// </summary>
        public long LikeCount { get; set; }

        /// <summary>
        ///     Hat der aktuelle User diese Idee reportet?
        /// </summary>
        public bool IsReported { get; set; }

        /// <summary>
        ///     Anzahl Reports
        /// </summary>
        public long ReportCount { get; set; }

        /// <summary>
        ///     Kann der aktuelle User diese Idee editeren
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        ///     Kann der aktuelle User Meldungen dieser Idee sehen
        /// </summary>
        public bool CanSeeReports { get; set; }

        /// <summary>
        ///     Hat der aktuelle User diese Idee erstellt
        /// </summary>
        public bool IsMine { get; set; }

        /// <summary>
        ///     Hilft der aktuelle User bei dieser Idee
        /// </summary>
        public bool IsHelping { get; set; }

        /// <summary>
        ///     Id für Public Chat
        /// </summary>
        public long PublicChatId { get; set; }

        /// <summary>
        ///     Id für privaten Chat mit ersteller
        /// </summary>
        public long? PrivateChatId { get; set; }

        /// <summary>
        ///     Sichtbar im UI
        /// </summary>
        [JsonIgnore]
        public bool IsVisible { get; set; } = true;

        /// <summary>
        ///     Link zum Bild der Idee
        /// </summary>
        public string IdeaImage { get; set; } = string.Empty;

        /// <summary>
        ///     Db Id für Idee Bild
        /// </summary>
        public long? IdeaImageId { get; set; }

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}