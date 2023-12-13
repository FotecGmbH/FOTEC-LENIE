// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Dc.Core;
using Biss.Dc.Core.DcChat;
using Biss.Interfaces;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse ExDcChatUser. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDcChatUser : IBissModel, IDcChatUser
    {
        #region Properties

        /// <summary>
        ///     User Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Anzeigename
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        ///     Bild des User
        /// </summary>
        public string ImageLink { get; set; } = string.Empty;

        /// <summary>
        ///     Version der Daten (aus Db)
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] DataVersion { get; set; } = null!;
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        ///     Test Propery Projektabhängig
        /// </summary>
        public string Test { get; set; } = string.Empty;


        /// <summary>Ist das aktuelle Element selektiert</summary>
        public bool IsSelected { get; set; }

        /// <summary>
        ///     Kann das IsSelected aktiviert werden (es kann sein bei BissCommands das es nicht gewünscht wird)
        /// </summary>
        public bool CanEnableIsSelect { get; set; }

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
#pragma warning disable CS0067
        public event EventHandler<BissSelectableEventArgs>? Selected;
#pragma warning restore CS0067

        #endregion
    }
}