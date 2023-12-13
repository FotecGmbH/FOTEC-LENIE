// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model.Report
{
    /// <summary>
    ///     <para>Meldung einer Idee</para>
    ///     Klasse ExReport. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExReport : IBissModel
    {
        #region Properties

        /// <summary>
        ///     User der etwas meldet
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///     Username des Melders
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Userbild
        /// </summary>
        public string UserImage { get; set; } = string.Empty;

        /// <summary>
        ///     Begründung der Meldung
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        ///     Idee die gemeldet wird
        /// </summary>
        public long IdeaId { get; set; }

        /// <summary>
        ///     Titel Idee
        /// </summary>
        public string IdeaTitle { get; set; } = string.Empty;

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