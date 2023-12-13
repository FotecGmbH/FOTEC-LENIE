// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.Helper;
using BaseApp.ViewModel.Idea;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Chat
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse VmChat. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewChat")]
    public class VmChat : VmProjectBase
    {
        /// <summary>
        ///     Delegate fürs scrollen.
        /// </summary>
        public delegate Task ScrollDelegate();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmChat.DesignInstance}"
        /// </summary>
        public static VmChat DesignInstance = new VmChat();

        /// <summary>
        ///     DesignInstance für DaychatEntries
        /// </summary>
        public static ObservableCollection<DcVmUiChatDayEntry> DesignInstanceChatDayEntries = new ObservableCollection<DcVmUiChatDayEntry>();

        /// <summary>
        ///     VmChat
        /// </summary>
        public VmChat() : base(string.Empty, ResViewChatOverview.LblSubTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Name einer neuen Chatnachricht
        /// </summary>
        public VmEntry EntryChatMessage { get; set; } = null!;

        /// <summary>
        ///     Delegate fürs scrollen.
        /// </summary>
        public ScrollDelegate ScrollTo { get; set; } = null!;

        /// <summary>
        ///     Schwebender Text mit Info
        /// </summary>
        public string ChatHeaderText { get; set; } = string.Empty;

        /// <summary>
        ///     DESCRIPTION
        /// </summary>
        public bool CheckingBlocked { get; set; } = true;

        #endregion

        private async void Chat_NewItemsInSelectedChat(object sender, EventArgs e)
        {
            if (Dc.Chat.SelectedUiChat != null)
            {
                Dc.Chat.ResetUnreadForChat(Dc.Chat.SelectedUiChat.Chat.Id);
            }

            if (ScrollTo != null!)
            {
                await ScrollTo().ConfigureAwait(true);
            }
        }

        private (string, bool) ValidateChatEntry(string value)
        {
            if (CheckingBlocked)
            {
                return (string.Empty, true);
            }

            return VmEntryValidators.ValidateFuncStringEmptySearch(value);
        }

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            if (Dc.Chat.SelectedUiChat != null)
            {
                Dc.Chat.ResetUnreadForChat(Dc.Chat.SelectedUiChat.Chat.Id);
            }

            EntryChatMessage = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewChatOverview.EntryChatTitle,
                ResViewChatOverview.EntryChatPlaceholder,
                validateFunc: ValidateChatEntry,
                showTitle: false,
                showHint: false,
                showMaxChar: false,
                returnAction: () => CmdSendMessage.Execute(null!));
            return base.OnActivated(args);
        }

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            Dc.Chat.NewItemsInSelectedChat += Chat_NewItemsInSelectedChat;
            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            try
            {
                if (ScrollTo != null!)
                {
                    ScrollTo();
                }

                if (Dc.Chat.SelectedUiChat == null!)
                {
                    MsgBox.Show(ShowDeveloperInfos ? "Es war kein Chat selektiert - Dc.Chat.SelectedUiChat == null!" : ResViewChatOverview.MsgCouldNotOpenChat);
                    Nav.Back();
                    return Task.CompletedTask;
                }

                var idea = string.Empty;
                if (Dc.Chat.SelectedUiChat.Chat.IdeaId.HasValue &&
                    Dc.DcExIdeas.Any(x => x.Index == Dc.Chat.SelectedUiChat.Chat.IdeaId))
                {
                    idea = Dc.DcExIdeas.FirstOrDefault(i => i.Index == Dc.Chat.SelectedUiChat.Chat.IdeaId).Data.Title;
                }

                PageTitle = ChatHeaderText = !string.IsNullOrWhiteSpace(idea) ? idea : Dc.Chat.SelectedUiChat.Chat.ChatName;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmChat)}]({nameof(OnLoaded)}): {e}");
                LogCrash4Appcenter(e);
            }

            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.Chat.NewItemsInSelectedChat -= Chat_NewItemsInSelectedChat;
            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Message senden
        /// </summary>
        public VmCommand CmdSendMessage { get; private set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdSendMessage = new VmCommand(string.Empty, async () =>
            {
                if (CheckingBlocked)
                {
                    CheckingBlocked = false;
                }

                EntryChatMessage.ValidateData();

                if (Dc.Chat.SelectedUiChat != null! && EntryChatMessage.DataValid)
                {
                    await Dc.Chat.PostMessage(EntryChatMessage.Value, Dc.Chat.SelectedUiChat.Chat.Id).ConfigureAwait(true);
                    CheckingBlocked = true;
                    EntryChatMessage.Value = string.Empty;

                    if (ScrollTo != null!)
                    {
                        await ScrollTo().ConfigureAwait(true);
                    }
                }
            }, glyph: Glyphs.Send_email);

            View.CmdHeaderCommon = new VmCommand(string.Empty, async () =>
            {
                if (Dc.Chat.SelectedUiChat?.Chat?.IdeaId > 0)
                {
                    var idea = Dc.DcExIdeas.AllItems.FirstOrDefault(x => x.Id == Dc.Chat.SelectedUiChat.Chat.IdeaId);

                    if (idea != null)
                    {
                        await Nav.ToViewWithResult(typeof(VmIdeaDetails), idea).ConfigureAwait(true);
                    }
                    else
                    {
                        await MsgBox.Show(ResViewChatOverview.MsgCouldNotNavToIdea, ResCommon.MsgTitleError).ConfigureAwait(true);
                    }
                }
            }, glyph: Glyphs.Information_circle);
        }

        #endregion
    }
}