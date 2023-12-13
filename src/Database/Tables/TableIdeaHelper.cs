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
    ///     <para>Helfer bei Idee</para>
    ///     Klasse TableIdeaHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("IdeaHelper")]
    public class TableIdeaHelper : IDcChangeTracking
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
        ///     Info vom Helfer
        /// </summary>
        public string Info { get; set; } = string.Empty;

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     User dem der Helfer zugewiesen ist
        /// </summary>
        public long TblUserId { get; set; }

        /// <summary>
        ///     User dem der Helfer zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblUserId))]
        public virtual TableUser TblUser { get; set; } = null!;

        /// <summary>
        ///     Idee die dem Helfer zugewiesen ist
        /// </summary>
        public long TblIdeaId { get; set; }

        /// <summary>
        ///     Idee die dem Helfer zugewiesen ist
        /// </summary>
        [ForeignKey(nameof(TblIdeaId))]
        public virtual TableIdea TblIdea { get; set; } = null!;

        /// <summary>
        ///     zur Verfügung Stehende Sachen
        /// </summary>
        public ICollection<TableIdeaSupply> TblIdeaSupplies { get; set; } = new List<TableIdeaSupply>();

        #endregion
    }
}