// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using Biss.Dc.Client.Chat;
using Biss.Dc.Core.DcChat;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>Kompletter einzelner Chat für Binding</para>
    ///     Klasse ExDcUiChat. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
#pragma warning disable CA1005 // Avoid excessive parameters on generic types
    public class ExUiChat<TChat, TEntry, TUser, TUiDayChatEntry, TUiChatEntry> : DcUiChatBase<TChat, TEntry, TUser, TUiDayChatEntry, TUiChatEntry>
#pragma warning restore CA1005 // Avoid excessive parameters on generic types
        where TChat : IDcChat
        where TEntry : IDcChatEntry
        where TUser : IDcChatUser
        where TUiDayChatEntry : DcUiChatDayEntryBase<TEntry, TUser, TUiChatEntry>
        where TUiChatEntry : DcUiChatEntryBase<TEntry, TUser>
    {
        /// <summary>
        ///     Kompletter einzelner Chat für Binding
        /// </summary>
        /// <param name="chat">Chat Stammdaten vom Server/DB</param>
        /// <param name="chatEntries">Alle Einträge für diesen Chat vom Server/DB</param>
        /// <param name="chatsUsers">(Alle)ChatUser aller Chats</param>
        public ExUiChat(TChat chat, List<TEntry> chatEntries, List<TUser> chatsUsers) : base(chat, chatEntries, chatsUsers)
        {
        }
    }
}