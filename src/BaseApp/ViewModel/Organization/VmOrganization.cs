// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.ViewModel.Idea;
using BaseApp.ViewModel.Reports;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Dc.Client;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Organization
{
    /// <summary>
    ///     <para>Organisationen Übersicht</para>
    ///     Klasse VmOrganization. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewOrganization", true)]
    public class VmOrganization : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmOrganization.DesignInstance}"
        /// </summary>
        public static VmOrganization DesignInstance = new VmOrganization();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceDpExAccessToken}"
        /// </summary>
        public static DcListDataPoint<ExIdea> DesignInstanceDpExIdea = new DcListDataPoint<ExIdea>(new ExIdea());

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUsersAll.DesignInstance}"
        /// </summary>
        public static DcListDataPoint<ExOrganizationUser> DesignInstanceExOrganizationUser = new DcListDataPoint<ExOrganizationUser>(new ExOrganizationUser());

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmOrganization.DesignInstanceExOrganization}"
        /// </summary>
        public static DcListDataPoint<ExOrganization> DesignInstanceExOrganization = new DcListDataPoint<ExOrganization>(new ExOrganization());

        private bool _eventsAttached;
        private bool _isSwitchAllOrg;
        private bool _lockOverrides;

        /// <summary>
        ///     VmOrganization
        /// </summary>
        public VmOrganization() : base(ResViewOrganization.LblTitle, subTitle: ResViewOrganization.LblSubTitle)
        {
            View.BusySet(String.Empty, 50);
            SetViewProperties();
            View.ShowUser = false;
        }

        #region Properties

        /// <summary>
        ///     Org User sichtbar
        /// </summary>
        public bool IsOrgUsersVisible { get; set; }

        /// <summary>
        ///     Access Token sichtbar
        /// </summary>
        public bool IsIdeaListVisible { get; set; }

        /// <summary>
        ///     Phone View Alle Gemeinden
        /// </summary>
        public bool IsSwitchAllOrg
        {
            get => _isSwitchAllOrg;
            set
            {
                _isSwitchAllOrg = value;
                if (_isSwitchAllOrg)
                {
                    Dc.DcExOrganization.SelectedItem = null;
                }
                else
                {
                    PickerOrganization.SelectedItem = PickerOrganization.FirstOrDefault();
                    Dc.DcExOrganization.SelectedItem = Dc.DcExOrganization.FirstOrDefault();
                }
            }
        }

        /// <summary>
        ///     Organisationen Picker
        /// </summary>
        public VmPicker<DcListDataPoint<ExOrganization>> PickerOrganization { get; } = new VmPicker<DcListDataPoint<ExOrganization>>(nameof(PickerOrganization));

        /// <summary>
        ///     Refresh View
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        ///     Header
        /// </summary>
        public string HeaderUser { get; set; } = "Alle Benutzer meiner Gemeinden";

        /// <summary>
        ///     Header
        /// </summary>
        public string HeaderIdeas { get; set; } = "Alle Ideen meiner Gemeinden";

        /// <summary>
        ///     DESCRIPTION
        /// </summary>
        public bool UserCompananyDetailsActive { get; set; }

        #endregion

        /// <summary>
        ///     Picker mag CollectionChanged von dc nicht
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExOrganizationOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                PickerOrganization.Clear();
                foreach (var dataPoint in Dc.DcExOrganization)
                {
                    PickerOrganization.AddKey(dataPoint, dataPoint.Data.NamePlzString);
                }

                FilterLists();
            });
        }

        /// <summary>
        ///     Organisationen-User hinzufügen/bearbeiten/löschen/Details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task DcExOrganizationUsersOnCollectionEvent(object sender, CollectionEventArgs<DcListDataPoint<ExOrganizationUser>> e)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            var user = e.Item;
            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
            {
                if (Dc.DcExOrganization.SelectedItem == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationUsersOnCollectionEvent)}): {nameof(Dc.DcExOrganization.SelectedItem)}");
                }

                DcListDataPoint<ExOrganizationUser>? item = null;
                var newUser = new ExOrganizationUser
                              {
                                  OrganizationId = Dc.DcExOrganization.SelectedItem.Index,
                                  UserRight = EnumUserRight.ReadWrite,
                                  UserRole = EnumUserRole.User
                              };
                item = new DcListDataPoint<ExOrganizationUser>(newUser);
                Dc.DcExOrganizationUsers.Add(item);

                _lockOverrides = true;
                var r = await Nav.ToViewWithResult(typeof(VmAddOrganizationUser), item).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        try
                        {
                            Dc.DcExOrganizationUsers.Remove(item);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationUsersOnCollectionEvent)}): Workaroud - neues NuGet");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.EditRequest)
            {
                if (user == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {nameof(user)}");
                }

                _lockOverrides = true;
                var r = await Nav.ToViewWithResult(typeof(VmAddOrganizationUser), user).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        var p = user;
                        if (p.PossibleNewDataOnServer)
                        {
                            p.Update();
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                if (user == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {nameof(user)}");
                }

                var res = await MsgBox.Show(ResViewOrganization.MsgConfirmUserDelete, ResViewOrganization.MsgConfirmUserDeleteTitle, VmMessageBoxButton.YesNo).ConfigureAwait(true);

                if (res == VmMessageBoxResult.Yes)
                {
                    try
                    {
                        Dc.DcExOrganizationUsers.Remove(user);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationUsersOnCollectionEvent)}): Workaroud - neues NuGet");
                    }

                    var r = await Dc.DcExOrganizationUsers.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        await MsgBox.Show(ResViewOrganization.MsgDeleteError).ConfigureAwait(true);
                    }
                }
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.InfoRequest)
            {
                await MsgBox.Show($"{user.Data.Fullname}").ConfigureAwait(true);
            }
            else
            {
                Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationOnCollectionEvent)}): Not supported!");
            }
        }

        /// <summary>
        ///     Access Token hinzufügen/löschen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task DcExIdeasOnCollectionEvent(object sender, CollectionEventArgs<DcExIdeasType> e)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            if (e.TypeOfEvent == EnumCollectionEventType.InfoRequest)
            {
                if (e.Item == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmMainIdea)}]({nameof(InitializeCommands)}): {nameof(e.Item)}");
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                _lockOverrides = true;

                await Nav.ToViewWithResult(typeof(VmIdeaDetails), e.Item).ConfigureAwait(true);

                e.Item.Update();
            }
            else
            {
                Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationOnCollectionEvent)}): Not supported!");
            }
        }

        /// <summary>
        ///     Organisationen hinzufügen/bearbeiten/löschen/Details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task DcExOrganizationOnCollectionEvent(object sender, CollectionEventArgs<DcListDataPoint<ExOrganization>> e)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
            {
                DcListDataPoint<ExOrganization>? item = null;
                var newOrganization = new ExOrganization
                                      {
                                          Name = string.Empty,
                                          PostalCode = string.Empty,
                                          OrganizationType = EnumOrganizationTypes.PublicOrganization,
                                          UserIsAdmin = true,
                                          UserIsUserPlus = true,
                                      };
                item = new DcListDataPoint<ExOrganization>(newOrganization);
                Dc.DcExOrganization.Add(item);

                _lockOverrides = true;
                var r = await Nav.ToViewWithResult(typeof(VmAddOrganization), item).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        try
                        {
                            Dc.DcExOrganization.Remove(item);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationOnCollectionEvent)}): Workaroud - neues NuGet");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                await MsgBox.Show("Diese Funktion ist zurzeit deaktiviert!").ConfigureAwait(true);
            }
            else
            {
                Logging.Log.LogWarning($"[{nameof(VmOrganization)}]({nameof(DcExOrganizationOnCollectionEvent)}): Not supported!");
            }
        }

        /// <summary>
        ///     Picker - Selected Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerOrganizationOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<DcListDataPoint<ExOrganization>>> e)
        {
            Dc.DcExOrganization.SelectedItem = e.CurrentItem.Key;
        }

        /// <summary>
        ///     Selektierte Organisation hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExOrganizationOnSelectedItemChanged(object sender, SelectedItemEventArgs<DcListDataPoint<ExOrganization>> e)
        {
            View.BusySet(ResCommon.LblLoading, 0);

            IsOrgUsersVisible = false;
            IsIdeaListVisible = false;

            Dispatcher!.RunOnDispatcher(async () =>
            {
                if (e.CurrentItem != null!)
                {
                    HeaderUser = $"Benutzer in {e.CurrentItem.Data.Name}";
                    HeaderIdeas = $"Ideen in {e.CurrentItem.Data.Name}";
                    UserCompananyDetailsActive = true;

                    FilterLists();

                    if (!Dc.DcExLocalAppData.Data.LastSelectedOrganization.HasValue || Dc.DcExLocalAppData.Data.LastSelectedOrganization != e.CurrentItem.Id)
                    {
                        Dc.DcExLocalAppData.Data.LastSelectedOrganization = e.CurrentItem.Id;
                        await Dc.DcExLocalAppData.StoreData().ConfigureAwait(false);
                    }
                }
                else
                {
                    HeaderUser = "Alle Benutzer meiner Gemeinden";
                    HeaderIdeas = "Alle Ideen meiner Gemeinden";
                    UserCompananyDetailsActive = false;

                    FilterLists();

                    Dc.DcExLocalAppData.Data.LastSelectedOrganization = null;
                    await Dc.DcExLocalAppData.StoreData().ConfigureAwait(false);
                }

                IsOrgUsersVisible = true;
                IsIdeaListVisible = true;

                View.BusyClear();
            });
        }

        private void FilterLists()
        {
            FilterOrganisations();
            FilterUsersAndIdeas();
        }

        private void FilterOrganisations()
        {
            #region Organisations

            Dc.DcExOrganization.FilterList(x =>
                Dc.DcExUser.Data.IsSysAdmin ||
                x.Data.UserIsUserPlus);

            #endregion
        }

        private void FilterUsersAndIdeas()
        {
            #region Users

            if (_isSwitchAllOrg || Dc.DcExOrganization.SelectedItem == null!)
            {
                Dc.DcExOrganizationUsers.FilterList(x => Dc.DcExOrganization.Any(y => y.Id == x.Data.OrganizationId));
            }
            else
            {
                Dc.DcExOrganizationUsers.FilterList(x => Dc.DcExOrganization.SelectedItem.Id == x.Data.OrganizationId);
            }

            #endregion

            #region Ideas

            if (_isSwitchAllOrg || Dc.DcExOrganization.SelectedItem == null!)
            {
                Dc.DcExIdeas.FilterList(x => x.Data.Companies.Any(y => Dc.DcExOrganization.Any(z => z.Id == y.OrganizationId)));
            }
            else
            {
                Dc.DcExIdeas.FilterList(x => x.Data.Companies.Any(y => y.OrganizationId == Dc.DcExOrganization.SelectedItem.Id));
            }

            #endregion
        }

        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attach"></param>
        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                if (!_eventsAttached)
                {
                    _eventsAttached = true;
                    Dc.DcExOrganization.SelectedItemChanged += DcExOrganizationOnSelectedItemChanged;
                    Dc.DcExOrganizationUsers.CollectionEvent += DcExOrganizationUsersOnCollectionEvent;
                    Dc.DcExOrganization.CollectionEvent += DcExOrganizationOnCollectionEvent;
                    Dc.DcExIdeas.CollectionEvent += DcExIdeasOnCollectionEvent;

                    Dc.DcExOrganization.CollectionChanged += DcExOrganizationOnCollectionChanged;
                    PickerOrganization.SelectedItemChanged += PickerOrganizationOnSelectedItemChanged;
                }
            }
            else
            {
                if (_eventsAttached)
                {
                    _eventsAttached = false;
                    Dc.DcExOrganization.SelectedItemChanged -= DcExOrganizationOnSelectedItemChanged;
                    Dc.DcExOrganizationUsers.CollectionEvent -= DcExOrganizationUsersOnCollectionEvent;
                    Dc.DcExOrganization.CollectionEvent -= DcExOrganizationOnCollectionEvent;
                    Dc.DcExIdeas.CollectionEvent -= DcExIdeasOnCollectionEvent;

                    Dc.DcExOrganization.CollectionChanged -= DcExOrganizationOnCollectionChanged;
                    PickerOrganization.SelectedItemChanged -= PickerOrganizationOnSelectedItemChanged;
                }
            }
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            await base.OnAppearing(view).ConfigureAwait(true);

            await CheckPhoneConfirmation().ConfigureAwait(true);

            if (_lockOverrides)
            {
                return;
            }

            FilterOrganisations();

            if (Dc.DcExLocalAppData.Data.LastSelectedOrganization.HasValue)
            {
                var tmp = Dc.DcExOrganization.FirstOrDefault(a => a.Id == Dc.DcExLocalAppData.Data.LastSelectedOrganization.Value);
                if (tmp == null)
                {
                    Dc.DcExOrganization.SelectedItem = null;
                }
                else
                {
                    Dc.DcExOrganization.SelectedItem = tmp;
                }
            }
            else
            {
                Dc.DcExOrganization.SelectedItem = null;
            }

            if (!TabletMode)
            {
                PickerOrganization.Clear();
                foreach (var dataPoint in Dc.DcExOrganization)
                {
                    PickerOrganization.AddKey(dataPoint, dataPoint.Data.NamePlzString);
                }

                _isSwitchAllOrg = true;
                this.InvokeOnPropertyChanged(nameof(IsSwitchAllOrg));
            }

            PickerOrganization.SelectKey(Dc.DcExOrganization.SelectedItem!);

            DcExOrganizationOnSelectedItemChanged(this, new SelectedItemEventArgs<DcListDataPoint<ExOrganization>>(null!, Dc.DcExOrganization.SelectedItem!));
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await base.OnActivated(args).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override async Task OnLoaded()
        {
            await base.OnLoaded().ConfigureAwait(true);

            if (_lockOverrides)
            {
                _lockOverrides = false;
                return;
            }

            Dispatcher!.RunOnDispatcher(async () =>
            {
                IsOrgUsersVisible = true;
                IsIdeaListVisible = true;

                if (!Dc.DcExOrganization.SyncedSinceUserRegistered)
                {
                    try
                    {
                        await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(OnLoaded)}): {e}");
                    }
                }

                if (!Dc.DcExOrganizationUsers.SyncedSinceUserRegistered)
                {
                    try
                    {
                        await Dc.DcExOrganizationUsers.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(OnLoaded)}): {e}");
                    }
                }

                if (!Dc.DcExIdeas.SyncedSinceUserRegistered)
                {
                    try
                    {
                        await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(OnLoaded)}): {e}");
                    }
                }
            });

            AttachDetachVmEvents(true);

            View.BusyClear();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            if (_lockOverrides)
            {
                return Task.CompletedTask;
            }

            IsOrgUsersVisible = false;
            IsIdeaListVisible = false;

            AttachDetachVmEvents(false);

            Dc.DcExOrganization.FilterClear();
            Dc.DcExOrganizationUsers.FilterClear();
            Dc.DcExIdeas.FilterClear();

            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            #region Commands Organization User

            CmdLockUser = new VmCommand("Sperren", async user =>
            {
                if (user == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {nameof(user)}");
                }

                if (user is DcListDataPoint<ExOrganizationUser> dcPoint)
                {
                    if (!await CheckConnected().ConfigureAwait(true))
                    {
                        return;
                    }

                    dcPoint.Data.Locked = !dcPoint.Data.Locked;

                    var r = await dcPoint.StoreData(true).ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        var msg = ResSysAdmin.ErrorCommon;
                        if (ShowDeveloperInfos)
                        {
                            msg = $"{msg}: {r.ServerExceptionText}";
                        }

                        await MsgBox.Show(msg).ConfigureAwait(true);

                        dcPoint.Data.Locked = !dcPoint.Data.Locked;
                    }
                }
            }, glyph: Glyphs.Lock);

            CmdPhoneUser = new VmCommand(string.Empty, p =>
            {
                try
                {
                    if (p is DcListDataPoint<ExOrganizationUser> user)
                    {
                        Open.Phone(user.Data.UserPhoneNumber);
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {e}");
                }
            }, glyph: Glyphs.Phone);

            CmdMailUser = new VmCommand(string.Empty, p =>
            {
                try
                {
                    if (p is DcListDataPoint<ExOrganizationUser> user)
                    {
                        Open.EMail(new List<string> {user.Data.UserLoginEmail}, string.Empty, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {e}");
                }
            }, glyph: Glyphs.Email_action_add);

            #endregion

            CmdShowReports = new VmCommand("Meldungen zeigen", async idea =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                var _ = await Nav.ToViewWithResult(typeof(VmShowReports), idea).ConfigureAwait(false);

                try
                {
                    await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {e}");
                }
            }, glyph: Glyphs.Earth_warning);

            View.CmdHeaderCommon = new VmCommand("", () =>
            {
                Dispatcher!.RunOnDispatcher(async () =>
                {
                    View.BusySet("Aktualisiere ...", 0);
                    IsRefreshing = true;

                    var r = new List<long>();

                    try
                    {
                        r.Add(await Dc.DcExOrganization.Sync().ConfigureAwait(true));
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): 1 - {e}");
                        r.Add(-1);
                    }

                    try
                    {
                        r.Add(await Dc.DcExOrganizationUsers.Sync().ConfigureAwait(true));
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): 2 - {e}");
                        r.Add(-1);
                    }

                    try
                    {
                        r.Add(await Dc.DcExIdeas.Sync().ConfigureAwait(true));
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): 3 - {e}");
                        r.Add(-1);
                    }

                    FilterLists();

                    if (r.Any(a => a == -1))
                    {
                        await MsgBox.Show("Fehler beim Sync!").ConfigureAwait(true);
                    }

                    var count = r.Sum();
                    if (count == 0)
                    {
                        await MsgBox.Show("Sync erfolgreich!\r\nKeine Änderungen.").ConfigureAwait(true);
                    }
                    else
                    {
                        await MsgBox.Show($"Sync erfolgreich!\r\n{count} Änderungen.").ConfigureAwait(true);
                    }

                    View.BusyClear();
                    IsRefreshing = false;
                });
            }, glyph: Glyphs.Button_refresh_arrows);

            CmdSelectOrg = new VmCommand(string.Empty, org =>
            {
                if (org == null!)
                {
                    throw new ArgumentNullException($"[{nameof(VmOrganization)}]({nameof(InitializeCommands)}): {nameof(org)}");
                }

                if (org is DcListDataPoint<ExOrganization> item && Dc.DcExOrganization.SelectedItem != item)
                {
                    Dc.DcExOrganization.SelectedItem = item;
                }
                else
                {
                    Dc.DcExOrganization.SelectedItem = null;
                }
            });

            CmdToggleAllOrg = new VmCommand(string.Empty, () => { IsSwitchAllOrg = !IsSwitchAllOrg; });
        }

        /// <summary>
        ///     Alle Gemeinden togglen
        /// </summary>
        public VmCommand CmdToggleAllOrg { get; set; } = null!;

        /// <summary>
        ///     Gemeinde wurde ausgewählt
        /// </summary>
        public VmCommand CmdSelectOrg { get; private set; } = null!;

        /// <summary>
        ///     Meldungen zur Idee
        /// </summary>
        public VmCommand CmdShowReports { get; private set; } = null!;

        /// <summary>
        ///     Benuter sperren
        /// </summary>
        public VmCommand CmdLockUser { get; set; } = null!;

        /// <summary>
        ///     User anrufen
        /// </summary>
        public VmCommand CmdPhoneUser { get; set; } = null!;

        /// <summary>
        ///     Mail an User
        /// </summary>
        public VmCommand CmdMailUser { get; set; } = null!;

        #endregion
    }
}