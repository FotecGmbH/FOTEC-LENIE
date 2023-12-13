// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Biss.Dc.Core;
using Biss.Dc.Core.DcChat;
using Biss.Interfaces;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ExDcChat. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDcChat : IBissModel, IDcChat, IBissSelectable
    {
        private bool _isSelected;

        #region Properties

        /// <summary>
        ///     Ist das aktuelle Element selektiert
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                if (CanEnableIsSelect)
                {
                    _isSelected = value;
                }
                else
                {
                    _isSelected = false;
                }

                OnSelected(new BissSelectableEventArgs(_isSelected));
            }
        }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; } = true;

        /// <summary>
        ///     Eindeutige Id des Chat
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Zur Idee
        /// </summary>
        public long? IdeaId { get; set; }

        /// <summary>
        ///     Sichtbarer Chat Name
        /// </summary>
        public string ChatName { get; set; } = string.Empty;

        /// <summary>
        ///     "Allgemeiner" Chat - jeder kann posten
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        ///     User des Chats - mehr als 2 nur bei "IsGroup"
        /// </summary>
        public List<long> ChatUsers { get; set; } = new List<long>();


        /// <summary>
        ///     Kann noch gepostet werden?
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     (Meine) ungelesenen Meldungen in dem Chat
        /// </summary>
        public int UnreadMessages { get; set; }

        /// <summary>
        ///     Datum von letzter Chatnachricht
        /// </summary>
        public DateTime LatestMessageDate { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     Nachrichtentext von letzter Chatnachricht
        /// </summary>
        public string LatestMessageText { get; set; } = string.Empty;

        /// <summary>
        ///     Version der Daten (aus Db)
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] DataVersion { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Ist archiviert.
        /// </summary>
        public bool IsArchived { get; set; }

        #endregion

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return ((IDcChangeTracking) this).DebugInfos();
        }

        /// <summary>
        ///     Methode von Ereignis für Description
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

        /// <summary>
        ///     Ereignis wenn das Element selektiert wurde im Element
        /// </summary>
        public event EventHandler<BissSelectableEventArgs>? Selected;

        #endregion
    }
}