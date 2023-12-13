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

namespace Database.Tables
{
    /// <summary>
    ///     <para>zur verfügung Stehende Sache für Idee</para>
    ///     Klasse TableIdeaSupply. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("IdeaSupply")]
    public class TableIdeaSupply : IDcChangeTracking
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
        ///     zur verfügung gestellte Menge
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Erstellt am
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     Sache die benötigt wird, und hiermit zur verfügung gestellt wird
        /// </summary>
        public long TblIdeaNeedId { get; set; }

        /// <summary>
        ///     Sache die benötigt wird, und hiermit zur verfügung gestellt wird
        /// </summary>
        [ForeignKey(nameof(TblIdeaNeedId))]
        public virtual TableIdeaNeed TblIdeaNeed { get; set; } = null!;

        /// <summary>
        ///     Helfer, der die Sachen zur verfügung stellt
        /// </summary>
        public long TblIdeaHelperId { get; set; }

        /// <summary>
        ///     Helfer, der die Sachen zur verfügung stellt
        /// </summary>
        [ForeignKey(nameof(TblIdeaHelperId))]
        public virtual TableIdeaHelper TblIdeaHelper { get; set; } = null!;

        #endregion
    }
}