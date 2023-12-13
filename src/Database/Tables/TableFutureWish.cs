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
    ///     <para>Wunsch für die zukunft</para>
    ///     Klasse TableFutureWish. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Table("FutureWish")]
    public class TableFutureWish : IDcChangeTracking
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
        ///     genauere Beschreibung
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Link für weiterleitung
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        ///     entfernt
        /// </summary>
        public bool Archived { get; set; }

        /// <summary>
        ///     Likes auf diese neue Feature
        /// </summary>
        public ICollection<TableFutureWishLike> TblFutureWishLikes { get; set; } = new List<TableFutureWishLike>();

        #endregion
    }
}