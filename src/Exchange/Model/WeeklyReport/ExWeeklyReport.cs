// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model.WeeklyReport
{
    /// <summary>
    ///     <para>Wöchentlicher Report</para>
    ///     Klasse ExWeeklyReport. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExWeeklyReport : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Erstellt am
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Start Zeitraum
        /// </summary>
        public DateTime StartReport { get; set; }

        /// <summary>
        ///     Ende Zeitraum
        /// </summary>
        public DateTime EndReport { get; set; }

        /// <summary>
        ///     Für Gemeinden
        /// </summary>
        public List<ExWeeklyReportItem> Towns { get; set; } = new List<ExWeeklyReportItem>();

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion

        #region Users

        /// <summary>
        ///     Gesamte Userzahl
        /// </summary>
        public long UsersTotal { get; set; }

        /// <summary>
        ///     Neue User
        /// </summary>
        public long NewUsers { get; set; }

        /// <summary>
        ///     User eingeloggt
        /// </summary>
        public long LoggedInUsers { get; set; }

        #endregion

        #region Ideas

        /// <summary>
        ///     gesamte Ideenanzahl
        /// </summary>
        public long IdeasTotal { get; set; }

        /// <summary>
        ///     neue Ideen im Zeitraum
        /// </summary>
        public long IdeasNewTime { get; set; }

        #endregion

        #region Idea Likes

        /// <summary>
        ///     gesamte Ideen Likes
        /// </summary>
        public long IdeaLikesTotal { get; set; }

        /// <summary>
        ///     Ideen Likes im Zeitraum
        /// </summary>
        public long IdeaLikesTime { get; set; }

        #endregion

        #region Idea Supports

        /// <summary>
        ///     Idee Unterstützungen gesamt
        /// </summary>
        public long IdeaSupportsTotal { get; set; }

        /// <summary>
        ///     Idee Unterstützungen im Zeitraum
        /// </summary>
        public long IdeaSupportsTime { get; set; }

        #endregion
    }

    /// <summary>
    ///     <para>Wöchentlicher Report</para>
    ///     Klasse ExWeeklyReport. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExWeeklyReportItem : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Db Id
        /// </summary>
        public long TblOrganizationId { get; set; }

        /// <summary>
        ///     Gemeindename
        /// </summary>
        public string TownName { get; set; } = string.Empty;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion

        #region Users

        /// <summary>
        ///     Gesamte Userzahl
        /// </summary>
        public long UsersTotal { get; set; }

        /// <summary>
        ///     Neue User
        /// </summary>
        public long NewUsers { get; set; }

        /// <summary>
        ///     User eingeloggt
        /// </summary>
        public long LoggedInUsers { get; set; }

        #endregion

        #region Ideas

        /// <summary>
        ///     gesamte Ideenanzahl
        /// </summary>
        public long IdeasTotal { get; set; }

        /// <summary>
        ///     neue Ideen im Zeitraum
        /// </summary>
        public long IdeasNewTime { get; set; }

        #endregion

        #region Idea Likes

        /// <summary>
        ///     gesamte Ideen Likes
        /// </summary>
        public long IdeaLikesTotal { get; set; }

        /// <summary>
        ///     Ideen Likes im Zeitraum
        /// </summary>
        public long IdeaLikesTime { get; set; }

        #endregion

        #region Idea Supports

        /// <summary>
        ///     Idee Unterstützungen gesamt
        /// </summary>
        public long IdeaSupportsTotal { get; set; }

        /// <summary>
        ///     Idee Unterstützungen im Zeitraum
        /// </summary>
        public long IdeaSupportsTime { get; set; }

        #endregion
    }
}