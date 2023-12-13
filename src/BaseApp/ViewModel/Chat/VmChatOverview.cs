// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange.Enum;
using Exchange.Model.Chat;
using Exchange.Model.User;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Chat
{
    /// <summary>
    ///     <para>Chat starten mit Arguments</para>
    ///     Klasse ChatInit. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ChatInit : IBissModel
    {
        /// <summary>
        /// </summary>
        protected ChatInit()
        {
        }

        /// <summary>
        ///     Public Chat zur Idee
        /// </summary>
        /// <param name="ideaId"></param>
        public ChatInit(long ideaId)
        {
            IdeaId = ideaId;
            UserId = null;
            PublicChat = true;
        }

        /// <summary>
        ///     Private Chat zur Idee mit einem User
        /// </summary>
        /// <param name="ideaId"></param>
        /// <param name="userId"></param>
        public ChatInit(long ideaId, long userId)
        {
            IdeaId = ideaId;
            UserId = userId;
            PublicChat = false;
        }

        #region Properties

        /// <summary>
        ///     Ideen Id
        /// </summary>
        public long IdeaId { get; protected set; }

        /// <summary>
        ///     Mit welchem User - bei Public null
        /// </summary>
        public long? UserId { get; protected set; }

        /// <summary>
        ///     Öffentlicher Chat
        /// </summary>
        public bool PublicChat { get; protected set; }

        #endregion

        #region Interface Implementations

        /// <inheritdoc />
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        #endregion
    }

    /// <summary>
    ///     <para>Übersicht aller Chats des Users</para>
    ///     Klasse VmChatOverview. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewChatOverview", true)]
    public class VmChatOverview : VmProjectBase
    {
        private static long? _selectedTownId;
        private static long? _selectedIdeaId;
        private static long? _selectedChatId;

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmChatOverview.DesignInstance}"
        /// </summary>
        public static VmChatOverview DesignInstance = new VmChatOverview();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmChatOverview.DesignInstance}"
        /// </summary>
        public static VmPickerElement<long> DesignInstanceOrg = new VmPickerElement<long>();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmChatOverview.DesignInstance}"
        /// </summary>
        public static VmPickerElement<long> DesignInstanceIdea = new VmPickerElement<long>();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmChatOverview.DesignInstance}"
        /// </summary>
        public static ExDcChat DesignInstanceChat = new ExDcChat();

        /// <summary>
        ///     VmChatOverview
        /// </summary>
        public VmChatOverview() : base(ResViewChatOverview.LblTitle, subTitle: ResViewChatOverview.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Debug Text für Chat
        /// </summary>
        public string ChatInfoText { get; set; } = string.Empty;

        /// <summary>
        ///     Suchtext
        /// </summary>
        public string SearchText { get; set; } = string.Empty;

        /// <summary>
        ///     Eingabe Suchtext
        /// </summary>
        public VmEntry EntrySearchText { get; set; } = null!;

        /// <summary>
        ///     Picker für Gemeindefilterung
        /// </summary>
        public VmPicker<long> PickerOrganizations { get; set; } = new VmPicker<long>(nameof(PickerOrganizations));

        /// <summary>
        ///     Picker für Ideenfilterung
        /// </summary>
        public VmPicker<long> PickerIdeas { get; set; } = new VmPicker<long>(nameof(PickerIdeas));

        /// <summary>
        ///     Auswahl Chats für Ui
        /// </summary>
        public BxObservableCollection<ExDcChat> ChatList { get; set; } = new BxObservableCollection<ExDcChat>();

        /// <summary>
        ///     Chats verfügbar
        /// </summary>
        public bool ChatsAvailable { get; set; }

        #endregion

        /// <summary>
        ///     Ereignis für Chat öffnen
        /// </summary>
#pragma warning disable CA1003 // Use generic event handler instances
        public static event EventHandler<long>? OpenChat;
#pragma warning restore CA1003 // Use generic event handler instances

        /// <summary>
        ///     Filter setzen, damit beim nächsten öffnen zum Chat navigiert wird
        /// </summary>
        /// <param name="chatId"></param>
        public static void SetFilterForChat(long chatId)
        {
            // Start von Notification -> Chat öffnen
            _selectedChatId = chatId;
            OpenChat?.Invoke(null, chatId);
        }

        /// <summary>
        ///     Chat öffnen von Notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnOpenChat(object sender, long e)
        {
            View.BusySet("Lade Chat");

            await WaitForConnected().ConfigureAwait(true);

            if (!Dc.Chat.Chats.Any(x => x.Chat.Id == e))
            {
                try
                {
                    await Dc.Chat.Sync().ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"[{nameof(VmChatOverview)}]({nameof(OnOpenChat)}): {ex}");
                }
            }

            var chat = Dc.Chat.Chats.FirstOrDefault(x => x.Chat.Id == e);
            if (chat != null)
            {
                _selectedChatId = -1;
                CmdOpenChat.Execute(chat.Chat);
            }

            View.BusyClear();
        }

        /// <summary>
        ///     Gemeinden im Ui neu Laden
        /// </summary>
        private void ReloadOrganizations()
        {
            View.BusySet(ResViewChatOverview.LblLoadingTowns);

            PickerOrganizations.SelectedItemChanged -= PickerOrganizationsOnSelectedItemChanged;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var oldSelected = _selectedTownId ?? PickerOrganizations.SelectedItem?.Key;

            PickerOrganizations.Clear();
            PickerOrganizations.AddKey(-1, ResViewChatOverview.LblPickerAllTowns);
            if (Dc.DcExUser.Data.IsSysAdmin)
            {
                foreach (var company in Dc.DcExOrganization
                             .Where(x => x.Data.OrganizationType != EnumOrganizationTypes.NoOrganization)
                             .OrderBy(x => x.Data.Name))
                {
                    PickerOrganizations.AddKey(company.Index, company.Data.NamePlzString);
                }
            }
            else
            {
                foreach (var permission in Dc.DcExUser.Data.Permissions.OrderBy(x => x.Town!.Name))
                {
                    PickerOrganizations.AddKey(permission.CompanyId, permission.Town!.NamePlzString);
                }
            }

            if (oldSelected != null)
            {
                PickerOrganizations.SelectKey(oldSelected.Value);
                _selectedTownId = null!;
            }
            else
            {
                PickerOrganizations.SelectKey(-1);
            }

            PickerOrganizations.SelectedItemChanged += PickerOrganizationsOnSelectedItemChanged;

            View.BusyClear();
        }

        /// <summary>
        ///     Ideen im Ui neu Laden - Filerung auf Gemeinde
        /// </summary>
        private void ReloadIdeas()
        {
            View.BusySet(ResViewChatOverview.LblLoadingIdeas);

            PickerIdeas.SelectedItemChanged -= PickerIdeasOnSelectedItemChanged;

            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            var oldSelected = _selectedIdeaId ?? PickerIdeas.SelectedItem?.Key;

            PickerIdeas.Clear();
            PickerIdeas.AddKey(-1, ResViewChatOverview.LblPickerAllIdeas);
            foreach (var dcIdea in Dc.DcExIdeas)
            {
                if (PickerOrganizations.SelectedItem == null! || PickerOrganizations.SelectedItem.Key < 0 ||
                    dcIdea.Data.Companies.Any(x => x.OrganizationId == PickerOrganizations.SelectedItem.Key))
                {
                    PickerIdeas.AddKey(dcIdea.Index, dcIdea.Data.Title);
                }
            }

            if (oldSelected != null)
            {
                PickerIdeas.SelectKey(oldSelected.Value);
                _selectedIdeaId = null!;
            }
            else
            {
                PickerIdeas.SelectKey(-1);
            }

            PickerIdeas.SelectedItem!.IsSelected = true;

            PickerIdeas.SelectedItemChanged += PickerIdeasOnSelectedItemChanged;

            View.BusyClear();
        }

        /// <summary>
        ///     Chats für Ui neu Laden - filtern etc.
        /// </summary>
        private void ReloadChats()
        {
            View.BusySet(ResViewChatOverview.LblLoadingChats);

            // Filtern der Chats
            ChatList.Clear();
            ChatsAvailable = false;

            // Sortieren der Cahts in absteigender Reihenfolge der erhaltenen Nachrichten
            var orderedChats = Dc.Chat.Chats
                .OrderByDescending(x => x.Chat.LatestMessageDate).ToList();

            foreach (var dcChat in orderedChats)
            {
                var dcc = dcChat.Chat;
                Logging.Log.LogWarning($"[{nameof(VmChatOverview)}]({nameof(ReloadChats)}): Checking: {dcc.Id} - {dcc.IdeaId}");

                if (PickerIdeas.SelectedItem != null! && PickerIdeas.SelectedItem.Key > 0)
                {
                    // Nur Chats dieser Idee
                    ChatInfoText = ResViewChatOverview.LblChatInfoIdea;

                    if (dcc.IdeaId == PickerIdeas.SelectedItem.Key)
                    {
                        ChatList.Add(dcc);
                    }
                }
                else if (PickerOrganizations.SelectedItem != null! && PickerOrganizations.SelectedItem.Key > 0)
                {
                    // Nur Chats, wo die Idee im PickerIdeas drin ist
                    ChatInfoText = string.Format(ResViewChatOverview.LblChatInfoTown, PickerOrganizations.SelectedItem.Description);

                    if (PickerIdeas.Any(x => x.Key == dcc.IdeaId))
                    {
                        ChatList.Add(dcc);
                    }
                }
                else
                {
                    // Alle Chats
                    ChatInfoText = ResViewChatOverview.LblChatInfoAll;

                    ChatList.Add(dcc);
                }
            }

            ChatsAvailable = ChatList.Count > 0;
            View.BusyClear();
        }

        private void UserOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExUser.Permissions))
            {
                Dispatcher!.RunOnDispatcher(() =>
                {
                    ReloadOrganizations();
                    ReloadIdeas();
                    ReloadChats();
                });
            }
        }

        private void DcExOrganizationOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                ReloadOrganizations();
                ReloadIdeas();
                ReloadChats();
            });
        }

        private void PickerOrganizationsOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<long>> e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                PickerIdeas.SelectKey(-1);
                ReloadIdeas();

                ChatList.SelectedItem = null!;
                ReloadChats();
            });
        }

        private void DcExIdeasOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                ReloadIdeas();
                ReloadChats();
            });
        }

        private void PickerIdeasOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<long>> e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                ChatList.SelectedItem = null!;
                ReloadChats();
            });
        }

        private void ChatOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExDcChat.LatestMessageDate))
            {
                Dispatcher!.RunOnDispatcher(() => { ReloadChats(); });
            }
        }

        private void ChatsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null!)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is DcVmUiChat uiChat)
                    {
                        uiChat.Chat.PropertyChanged -= ChatOnPropertyChanged;
                    }
                }
            }

            if (e.NewItems != null!)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is DcVmUiChat uiChat)
                    {
                        uiChat.Chat.PropertyChanged += ChatOnPropertyChanged;
                    }
                }
            }

            if ((e.OldItems != null! && e.OldItems.Count > 0) ||
                (e.NewItems != null! && e.NewItems.Count > 0) ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                Dispatcher!.RunOnDispatcher(() => { ReloadChats(); });
            }
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            CheckPhoneConfirmation().ConfigureAwait(true);

            if (Dc.DcExUser.Data.IsSysAdmin)
            {
                Dc.DcExOrganization.CollectionChanged += DcExOrganizationOnCollectionChanged;
            }
            else
            {
                Dc.DcExUser.Data.PropertyChanged += UserOnPropertyChanged;
            }

            Dc.DcExIdeas.CollectionChanged += DcExIdeasOnCollectionChanged;

            Dc.Chat.Chats.CollectionChanged += ChatsOnCollectionChanged;
            foreach (var chat in Dc.Chat.Chats)
            {
                chat.Chat.PropertyChanged += ChatOnPropertyChanged;
            }

            OpenChat += OnOpenChat;

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            var startForIdea = -1L;

            if (args is ChatInit chatArgs)
            {
                startForIdea = chatArgs.IdeaId;
            }
            else if (args is long chatId)
            {
                // Start von Notification -> Chat öffnen
                _selectedChatId = chatId;
            }
            else if (args != null!)
            {
                // ??
                // ggf Chat erstellen und auf ViewChat weiterleiten
                _ = MsgBox.Show("In Arbeit");
            }
            else
            {
                // Menu
                // Übersicht anzeigen

                startForIdea = -1;
            }

            _selectedIdeaId = startForIdea;

            await base.OnActivated(args).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override async Task OnLoaded()
        {
            await base.OnLoaded().ConfigureAwait(true);

            ReloadOrganizations();
            ReloadIdeas();
            ReloadChats();

            if (_selectedChatId > 0)
            {
                OnOpenChat(this, _selectedChatId.Value);
            }
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.DcExOrganization.CollectionChanged -= DcExOrganizationOnCollectionChanged;
            Dc.DcExUser.Data.PropertyChanged -= UserOnPropertyChanged;
            Dc.DcExIdeas.CollectionChanged -= DcExIdeasOnCollectionChanged;

            Dc.Chat.Chats.CollectionChanged -= ChatsOnCollectionChanged;
            foreach (var chat in Dc.Chat.Chats)
            {
                chat.Chat.PropertyChanged -= ChatOnPropertyChanged;
            }

            OpenChat -= OnOpenChat;

            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Chats syncen
        /// </summary>
        public VmCommand CmdSync { get; set; } = null!;

        /// <summary>
        ///     Suche
        /// </summary>
        public VmCommand CmdSearch { get; set; } = null!;

        /// <summary>
        ///     Chat öffnen
        /// </summary>
        public VmCommand CmdOpenChat { get; set; } = null!;

        /// <summary>
        ///     Organisation auswählen
        /// </summary>
        public VmCommand CmdSelectOrganization { get; set; } = null!;

        /// <summary>
        ///     Idee auswählen
        /// </summary>
        public VmCommand CmdSelectIdea { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            EntrySearchText = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewChatOverview.EntrySearchTextTitle,
                ResViewChatOverview.EntrySearchTextPlaceholder,
                this,
                nameof(SearchText),
                VmEntryValidators.ValidateFuncStringEmptySearch,
                () => CmdSearch.Execute(null!),
                showTitle: !TabletMode,
                showMaxChar: false);

            CmdSearch = new VmCommand(ResViewChatOverview.CmdSearch, () =>
            {
                // TODO Suche
            });

            CmdSync = new VmCommand("Sync", async () =>
            {
                View.BusySet(ResViewChatOverview.LblLoadingChats);
                try
                {
                    await Dc.Chat.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmChatOverview)}]({nameof(InitializeCommands)}): Sync - {e}");
                }

                View.BusyClear();
            }, glyph: Glyphs.Button_refresh_arrows);
            View.CmdSaveHeader = CmdSync;

            CmdOpenChat = new VmCommand(string.Empty, async p =>
            {
                if (p is ExDcChat chat)
                {
                    Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(chat.Id);
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                    try
                    {
                        CmdSync.Execute(null!);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmChatOverview)}]({nameof(InitializeCommands)}): Open Chat - {e}");
                    }
                }
            }, glyph: Glyphs.Arrow_right);

            CmdSelectOrganization = new VmCommand(string.Empty, p =>
            {
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                if (p is VmPickerElement<long> item && PickerOrganizations.SelectedItem?.Key != item.Key)
                {
                    PickerOrganizations.SelectKey(item.Key);
                }
                else
                {
                    PickerOrganizations.SelectKey(-1);
                }
            });

            CmdSelectIdea = new VmCommand(string.Empty, p =>
            {
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                if (p is VmPickerElement<long> item && PickerIdeas.SelectedItem?.Key != item.Key)
                {
                    PickerIdeas.SelectKey(item.Key);
                }
                else
                {
                    PickerIdeas.SelectKey(-1);
                }
            });
        }

        #endregion
    }
}