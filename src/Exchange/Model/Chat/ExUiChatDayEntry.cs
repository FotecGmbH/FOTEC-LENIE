// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using Biss.Dc.Client.Chat;
using Biss.Dc.Core.DcChat;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>Chateinträge auf Tagesbasis für einen Chat für Binding</para>
    ///     Klasse ExDcDayChatEntry. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
#pragma warning disable CA1005 // Avoid excessive parameters on generic types
    public class ExUiChatDayEntry<TEntry, TUser, TUiChatEntry> : DcUiChatDayEntryBase<TEntry, TUser, TUiChatEntry>
#pragma warning restore CA1005 // Avoid excessive parameters on generic types
        where TEntry : IDcChatEntry
        where TUser : IDcChatUser
        where TUiChatEntry : DcUiChatEntryBase<TEntry, TUser>
    {
        /// <summary>
        ///     Chateinträge auf Tagesbasis für einen Chat für Binding
        /// </summary>
        /// <param name="day">Tag der Chateinträge</param>
        /// <param name="entries">Chateinträge vom Server/Db</param>
        public ExUiChatDayEntry(DateTime day, ObservableCollection<TUiChatEntry> entries) : base(day, entries)
        {
        }
    }
}