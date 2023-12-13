// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;
using PropertyChanged;

namespace Exchange.Model.Idea
{
    /// <summary>
    ///     <para>Dinge die ein Helfer zur Verfügung stellt</para>
    ///     Klasse ExIdeaSupply. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExIdeaSupply : IBissModel
    {
        #region Properties

        /// <summary>
        ///     benötigte Sache die zugewiesen ist
        /// </summary>
        public long NeedId { get; set; }

        /// <summary>
        ///     Bezeichnung der benötigten Sache
        /// </summary>
        public string NeedName { get; set; } = string.Empty;

        /// <summary>
        ///     Benötigte Menge
        /// </summary>
        public long NeedAmount { get; set; }

        /// <summary>
        ///     aktuell zur Verfügung stehende Menge
        /// </summary>
        public long NeedAmountCurrent { get; set; }

        /// <summary>
        ///     benötigte Sache hat Infotext
        /// </summary>
        public bool NeedHasInfo { get; set; }

        /// <summary>
        ///     Infotext der benötigten Sache
        /// </summary>
        public string NeedInfo { get; set; } = string.Empty;

        /// <summary>
        ///     Menge die zur Verfügung gestellt wird
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        ///     Binding für Ui -> Amount Eingabefeld
        /// </summary>
        [DependsOn(nameof(Amount))]
        public string BindingAmount
        {
            get => Amount.ToString();
            set
            {
                if (long.TryParse(value, out var val))
                {
                    Amount = val;
                }
            }
        }


        /// <summary>
        ///     Text für Ui
        /// </summary>
        public string UiText => $"{NeedName} ({NeedAmountCurrent}/{NeedAmount})";

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