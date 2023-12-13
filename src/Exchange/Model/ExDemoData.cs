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
    ///     <para>Demo Daten für Tests/Demo des SA</para>
    ///     Klasse ExDemoData. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDemoData : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Test Daten
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        ///     Counter (ev. vom Server)
        /// </summary>
        public int Counter { get; set; }

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