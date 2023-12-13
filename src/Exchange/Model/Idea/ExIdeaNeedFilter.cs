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
    ///     <para>Filter für benötigte Sache</para>
    ///     Klasse ExIdeaNeedFilter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdeaNeedFilter : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Ideen ID - bei -1 alle Ideen
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