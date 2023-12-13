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

namespace Database.Tables
{
    /// <summary>
    ///     <para>Idee</para>
    ///     Klasse TableIdea. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("Idea")]
    public class TableIdea : IDcChangeTracking
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
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        ///     Start
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        ///     Ende
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        ///     Archiviert
        /// </summary>
        public bool Archived { get; set; }

        /// <summary>
        ///     Position Lat
        /// </summary>
        public double? LocationLat { get; set; }

        /// <summary>
        ///     Position Lon
        /// </summary>
        public double? LocationLon { get; set; }

        /// <summary>
        ///     Addresse der Position
        /// </summary>
        public string? LocationAddress { get; set; }

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     User dem die Idee zugewiesen ist
        /// </summary>
        public long TblUserId { get; set; }

        /// <summary>
        ///     User dem die Idee zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
        public virtual TableUser TblUser { get; set; } = null!;

        /// <summary>
        ///     Bild zu Idee
        /// </summary>
        [ForeignKey(nameof(TblIdeaImage))]
        public long? TblIdeaImageId { get; set; }

        /// <summary>
        ///     Bild zu Idee
        /// </summary>
        public virtual TableFile? TblIdeaImage { get; set; }

        /// <summary>
        ///     Organisationen die zu der Idea gehören
        /// </summary>
        public virtual ICollection<TableIdeaOrganization> TblIdeaOrganizations { get; set; } = new List<TableIdeaOrganization>();

        /// <summary>
        ///     Benötigte Sachen für die Idee
        /// </summary>
        public ICollection<TableIdeaNeed> TblIdeaNeeds { get; set; } = new List<TableIdeaNeed>();

        /// <summary>
        ///     Helfer für die Idee
        /// </summary>
        public ICollection<TableIdeaHelper> TblIdeaHelpers { get; set; } = new List<TableIdeaHelper>();

        /// <summary>
        ///     Likes für die Idee
        /// </summary>
        public ICollection<TableIdeaLike> TblIdeaLikes { get; set; } = new List<TableIdeaLike>();

        /// <summary>
        ///     Meldungen der Idee
        /// </summary>
        public ICollection<TableIdeaReport> TblIdeaReports { get; set; } = new List<TableIdeaReport>();

        /// <summary>
        ///     Chat(s) zur Idee
        /// </summary>
        public virtual ICollection<TableChat> TblChat { get; set; } = new List<TableChat>();

        #endregion
    }
}