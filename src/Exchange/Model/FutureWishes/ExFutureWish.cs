// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model.FutureWishes
{
    /// <summary>
    ///     <para>Zukünftiger Wunsch</para>
    ///     Klasse ExFutureWish. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExFutureWish : IBissModel
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
        ///     Link zu Website, Content, etc.
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        ///     aktueller User wünscht sich dieses Feature
        /// </summary>
        public bool Liked { get; set; }

        /// <summary>
        ///     Entfernt
        /// </summary>
        public bool Archived { get; set; }

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