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
    ///     <para>Filterung für Report</para>
    ///     Klasse ExReportFilter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExReportFilter : IBissModel
    {
        #region Properties

        /// <summary>
        ///     UserId für die gefiltert werden soll
        ///     -1 für alle User
        /// </summary>
        public long UserId { get; set; } = -1;

        /// <summary>
        ///     IdeaId für die gefiltert werden soll
        ///     -1 für alle Ideas
        /// </summary>
        public long IdeaId { get; set; } = -1;

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