// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model.Idea
{
    /// <summary>
    ///     <para>Filter für Ideen</para>
    ///     Klasse ExIdeaFilter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdeaFilter : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Nur meine eigenen Ideen anzeigen
        /// </summary>
        public bool OnlyMyIdeas { get; set; } = false;

        /// <summary>
        ///     Nur Ideen anzeigen bei denen ich helfe
        /// </summary>
        public bool OnlyMyHelpingIdeas { get; set; } = false;

        /// <summary>
        ///     Suchtext von UI
        /// </summary>
        public string SearchText { get; set; } = string.Empty;

        /// <summary>
        ///     Laden in umgekehrter Reihenfolge - für ältere Ideen
        /// </summary>
        public bool LoadReverse { get; set; } = false;

        /// <summary>
        ///     Filtern nach Gemeinde
        /// </summary>
        public long? OrganizationId { get; set; } = null;

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