// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     <para>Model für Report Idea Email</para>
    ///     Klasse EMailModelReportIdea. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class EMailModelReportIdea : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Titel Idee
        /// </summary>
        public string IdeaTitle { get; set; } = string.Empty;

        /// <summary>
        ///     Begründung
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        ///     Name Reporting User
        /// </summary>
        public string ReportUserName { get; set; } = string.Empty;

        /// <summary>
        ///     Name Reporting User
        /// </summary>
        public string ReportUserFirstName { get; set; } = string.Empty;

        /// <summary>
        ///     Name Reporting User
        /// </summary>
        public string ReportUserLastName { get; set; } = string.Empty;

        /// <summary>
        ///     Name Reporting User
        /// </summary>
        public string ReportUserPhone { get; set; } = string.Empty;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}