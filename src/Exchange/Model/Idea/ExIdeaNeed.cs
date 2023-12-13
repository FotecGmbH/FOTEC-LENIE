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
    ///     <para>Benötigte Sache für Idee</para>
    ///     Klasse ExIdeaNeed. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdeaNeed : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Ideen Id
        /// </summary>
        public long IdeaId { get; set; }

        /// <summary>
        ///     Titel
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     gesamt benötigte Menge
        /// </summary>
        public long AmountNeed { get; set; }

        /// <summary>
        ///     Aktuell verfügbare Menge
        /// </summary>
        public long AmountSupplied { get; set; }

        /// <summary>
        ///     Label für Menge
        /// </summary>
        public string AmountLabel { get; set; } = string.Empty;

        /// <summary>
        ///     Hat Infotext
        /// </summary>
        public bool HasInfo { get; set; }

        /// <summary>
        ///     Infotext
        /// </summary>
        public string Infotext { get; set; } = string.Empty;

        /// <summary>
        ///     Text für Ui
        /// </summary>
        public string UiText => $"{Title} ({AmountSupplied}/{AmountNeed} {AmountLabel})";

        /// <summary>
        ///     Kann aktueller User Editieren
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        ///     Kann aktueller User löschen
        /// </summary>
        public bool CanDelete { get; set; }

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