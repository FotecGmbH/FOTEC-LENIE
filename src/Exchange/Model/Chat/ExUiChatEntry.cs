// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Dc.Client.Chat;
using Biss.Dc.Core.DcChat;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>Chateinträge für einen Tag mit Verfasser für Binding</para>
    ///     Klasse ExUiChatEntry. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUiChatEntry<TEntry, TUser> : DcUiChatEntryBase<TEntry, TUser>
        where TEntry : IDcChatEntry
        where TUser : IDcChatUser
    {
        /// <summary>
        ///     Chateinträge für einen Tag mit Verfasser für Binding
        /// </summary>
        /// <param name="entry">Chateintrag vom Server/Db</param>
        /// <param name="user">Verfasser des Chat vom Server/Ui</param>
        public ExUiChatEntry(TEntry entry, TUser user) : base(entry, user)
        {
        }
    }
}