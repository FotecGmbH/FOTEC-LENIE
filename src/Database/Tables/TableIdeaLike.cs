﻿// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Biss.Dc.Core;

namespace Database.Tables
{
    /// <summary>
    ///     <para>Like für Idee</para>
    ///     Klasse TableIdeaLike. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("IdeaLike")]
    public class TableIdeaLike : IDcChangeTracking
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
        ///     Erstellt am
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     User dem der Like zugewiesen ist
        /// </summary>
        public long TblUserId { get; set; }

        /// <summary>
        ///     User dem der Like zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
        public virtual TableUser TblUser { get; set; } = null!;

        /// <summary>
        ///     Idee die dem Like zugewiesen ist
        /// </summary>
        public long TblIdeaId { get; set; }

        /// <summary>
        ///     Idee die dem Like zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblIdeaId))]
        public virtual TableIdea TblIdea { get; set; } = null!;

        #endregion
    }
}