// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.Organization
{
    /// <summary>
    ///     <para>Gemeinde für Suche</para>
    ///     Klasse ExTown. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExTown : IBissModel, IBissSelectable
    {
        private bool _isSelected;

        #region Properties

        /// <summary>
        ///     Gemeindekennzahl
        /// </summary>
        public string TownCode { get; set; } = string.Empty;

        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Postleitzahl
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        ///     Ist die Postleitzahl die Haupt Postleitzahl der Gemeinde - nur diese ist in DB, andere PLZ können gefunden werden
        /// </summary>
        public bool IsMainPostalCode { get; set; }

        /// <summary>
        ///     PLZ und Name in einem String
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Name), nameof(PostalCode))]
        public string NamePlzString => $"{PostalCode}, {Name}";

        /// <summary>
        ///     If the item is selected or not
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnSelected(new BissSelectableEventArgs(_isSelected));
            }
        }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        #endregion

        /// <summary>
        ///     Methode von Ereignis für Selected
        /// </summary>
        /// <param name="eventData"></param>
        protected virtual void OnSelected(BissSelectableEventArgs eventData)
        {
            var handler = Selected;
            handler?.Invoke(this, eventData);
        }

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0067
#pragma warning restore CS0414

        /// <inheritdoc />
        public event EventHandler<BissSelectableEventArgs>? Selected;

        #endregion
    }
}