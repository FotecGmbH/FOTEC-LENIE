// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;

namespace Exchange.Model
{
    /// <summary>
    ///     <para>Item für Intro Bildschirm</para>
    ///     Klasse ExIntroItem. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIntroItem : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Quelle für HTML
        /// </summary>
        public string HtmlSource { get; set; } = string.Empty;

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