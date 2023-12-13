// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.Helper;
using BaseApp.ViewModel.Chat;
using BaseApp.ViewModel.Reports;
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Model.Report;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Idea
{
    #region Hilfsobjekte

    /// <summary>
    ///     Filter für Ideen
    /// </summary>
    public enum EnumVmMainIdeaCommonFilter
    {
#pragma warning disable CS1591
        None,
        MyIdeas,
        IdeasIFollow
#pragma warning restore CS1591
    }

    /// <summary>
    ///     Filter für abgelaufene Ideen
    /// </summary>
    public enum EnumVmMainIdeaArchivedFilter
    {
#pragma warning disable CS1591
        Current,
        Old,
        All
#pragma warning restore CS1591
    }

    #endregion

    /// <summary>
    ///     <para>Mainview bei eingeloggtem User</para>
    ///     Klasse VmMainIdea. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewMainIdea")]
    public class VmMainIdea : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMainIdea.DesignInstance}"
        /// </summary>
        public static VmMainIdea DesignInstance = new VmMainIdea();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMainIdea.DesignInstance}"
        /// </summary>
        public static DcListDataPoint<ExIdea> DesignInstanceDcIdea = new DcListDataPoint<ExIdea>(new ExIdea());

        private static VmPickerElement<long>? _lastOrgFilter;
        private static VmPickerElement<EnumVmMainIdeaCommonFilter>? _lastCommonFilter;
        private static VmPickerElement<EnumVmMainIdeaArchivedFilter>? _lastArchivedFilter;
        private static string _searchText = string.Empty;
        private static long? _ideaToOpen;
        private readonly AsyncAutoResetEvent lfh = new AsyncAutoResetEvent();
        private bool _eventsAttached;

        private bool _isInNavToView;
        private bool _noUiListUpdate;
        private bool _searchFocus;

        /// <summary>
        ///     VmMainIdea
        /// </summary>
        public VmMainIdea() : base(ResViewMainIdea.LblTitle, subTitle: ResViewMainIdea.LblSubTitle)
        {
            SetViewProperties();
            View.ShowHeader = false;
            SearchFocus = DeviceInfo.Plattform == EnumPlattform.XamarinWpf;
        }

        #region Properties

        /// <summary>
        ///     Ideenliste sichtbar
        /// </summary>
        public bool UiIdeasVisible { get; set; }

        /// <summary>
        ///     Filterdetails sichtbar
        /// </summary>
        public bool FilterDetailGridEnabled { get; set; }

        /// <summary>
        ///     Ideen werden gerade neu geladen
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        ///     Suchtext
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set => _searchText = value;
        }

        /// <summary>
        ///     Ist die Suche im Focus
        /// </summary>
        public bool SearchFocus
        {
            get => _searchFocus;
            set => _searchFocus = value || DeviceInfo.Plattform == EnumPlattform.XamarinWpf;
        }

        /// <summary>
        ///     Eingabe Suchtext
        /// </summary>
        public VmEntry EntrySearchText { get; set; } = null!;

        /// <summary>
        ///     Titel für neues Element
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Eingabe neuer Beitrag
        /// </summary>
        public VmEntry EntryTitle { get; set; } = null!;

        /// <summary>
        ///     Picker um die ideen nach Gemeinden zu filtern.
        /// </summary>
        public VmPicker<long> PickerOrganizationFilter { get; set; } = new VmPicker<long>(nameof(PickerOrganizationFilter));

        /// <summary>
        ///     Allgemeiner Filter
        /// </summary>
        public VmPicker<EnumVmMainIdeaCommonFilter> PickerCommonFilter { get; set; } = new VmPicker<EnumVmMainIdeaCommonFilter>(nameof(PickerCommonFilter));

        /// <summary>
        ///     Archiv Filter
        /// </summary>
        public VmPicker<EnumVmMainIdeaArchivedFilter> PickerArchivedFilter { get; set; } = new VmPicker<EnumVmMainIdeaArchivedFilter>(nameof(PickerArchivedFilter));

        /// <summary>
        ///     Ideas im UI
        /// </summary>
        public List<DcExIdeasType> UiIdeas { get; set; } = new List<DcExIdeasType>();

        #endregion

        /// <summary>
        ///     Filter leeren
        /// </summary>
        public static void ClearFilters()
        {
            _searchText = string.Empty;
            _lastCommonFilter = null;
            _lastArchivedFilter = null;
            _lastOrgFilter = null;
            _ideaToOpen = null;
        }

        /// <summary>
        ///     Filter auf meine Ideen
        /// </summary>
        public static void SetFilterMyIdeas()
        {
            _searchText = string.Empty;
            _lastCommonFilter = new VmPickerElement<EnumVmMainIdeaCommonFilter>
                                {
                                    Key = EnumVmMainIdeaCommonFilter.MyIdeas,
                                };
            _lastArchivedFilter = new VmPickerElement<EnumVmMainIdeaArchivedFilter>
                                  {
                                      Key = EnumVmMainIdeaArchivedFilter.All,
                                  };
            _lastOrgFilter = null;
            _ideaToOpen = null;
        }

        /// <summary>
        ///     Filter für eine Idee, diese beim Start direkt öffnen
        /// </summary>
        /// <param name="ideaId"></param>
        public static void SetFilterForIdea(long ideaId)
        {
            // von Notification - nur die Idee anzeigen und direkt öffenen
            _ideaToOpen = ideaId;
            CheckLaunchIdea();
        }

        /// <summary>
        ///     Picker neu Befüllen
        /// </summary>
        private void UpdatePickerElements()
        {
            PickerOrganizationFilter.SelectedItemChanged -= PickerOrganizationFilterChanged;
            PickerCommonFilter.SelectedItemChanged -= PickerCommonFilterChanged;
            PickerArchivedFilter.SelectedItemChanged -= PickerArchivedFilterOnSelectedItemChanged;

            PickerCommonFilter.Clear();
            PickerCommonFilter.AddKey(EnumVmMainIdeaCommonFilter.None, ResCommon.EnumFilterNone);
            PickerCommonFilter.AddKey(EnumVmMainIdeaCommonFilter.MyIdeas, ResCommon.EnumFilterMyIdeas);
            PickerCommonFilter.AddKey(EnumVmMainIdeaCommonFilter.IdeasIFollow, ResCommon.EnumFilterFollowingIdeas);

            PickerArchivedFilter.Clear();
            PickerArchivedFilter.AddKey(EnumVmMainIdeaArchivedFilter.All, ResCommon.EnumArchivedAll);
            PickerArchivedFilter.AddKey(EnumVmMainIdeaArchivedFilter.Current, ResCommon.EnumArchivedCurrent);
            PickerArchivedFilter.AddKey(EnumVmMainIdeaArchivedFilter.Old, ResCommon.EnumArchivedOld);

            PickerOrganizationFilter.Clear();
            PickerOrganizationFilter.AddKey(-1, ResViewMainIdea.LblFilterAllTowns);

            foreach (var org in Dc.DcExUser.Data.Permissions)
            {
                if (org != null && org.Town != null)
                {
                    PickerOrganizationFilter.AddKey(org.CompanyId, org.Town.NamePlzString);
                }
            }

            SetPickerDefaults();

            PickerOrganizationFilter.SelectedItemChanged += PickerOrganizationFilterChanged;
            PickerCommonFilter.SelectedItemChanged += PickerCommonFilterChanged;
            PickerArchivedFilter.SelectedItemChanged += PickerArchivedFilterOnSelectedItemChanged;
        }

        /// <summary>
        ///     Standardwerte für Picker setzen
        /// </summary>
        private void SetPickerDefaults()
        {
            if (_lastCommonFilter != null)
            {
                PickerCommonFilter.SelectKey(_lastCommonFilter.Key);
            }
            else
            {
                PickerCommonFilter.SelectKey(EnumVmMainIdeaCommonFilter.None);
            }

            if (_lastArchivedFilter != null)
            {
                PickerArchivedFilter.SelectKey(_lastArchivedFilter.Key);
            }
            else
            {
                PickerArchivedFilter.SelectKey(EnumVmMainIdeaArchivedFilter.Current);
            }

            if (_lastOrgFilter != null)
            {
                PickerOrganizationFilter.SelectKey(_lastOrgFilter.Key);
            }
            else
            {
                PickerOrganizationFilter.SelectKey(-1);
            }
        }

        /// <summary>
        ///     Ideen im UI neu Filtern
        /// </summary>
        /// <returns></returns>
        private void FilterUiIdeas()
        {
            //lock (_lockObject)
            //{
            Dc.DcExIdeas.FilterList(dcIdea => ((PickerCommonFilter.SelectedItem?.Key != EnumVmMainIdeaCommonFilter.MyIdeas ||
                                                dcIdea.Data.IsMine) &&

                                               // Filter Meine Unterstützungen
                                               (PickerCommonFilter.SelectedItem?.Key != EnumVmMainIdeaCommonFilter.IdeasIFollow ||
                                                dcIdea.Data.IsHelping) &&

                                               // Filter historisch
                                               (PickerArchivedFilter.SelectedItem?.Key == EnumVmMainIdeaArchivedFilter.All ||
                                                (PickerArchivedFilter.SelectedItem?.Key == EnumVmMainIdeaArchivedFilter.Current && !dcIdea.Data.Archived) ||
                                                (PickerArchivedFilter.SelectedItem?.Key == EnumVmMainIdeaArchivedFilter.Old && dcIdea.Data.Archived)) &&

                                               // Textfilter
                                               (string.IsNullOrWhiteSpace(SearchText) ||
                                                dcIdea.Data.Title.ToLower().Contains(SearchText.ToLower(), StringComparison.InvariantCultureIgnoreCase) ||
                                                dcIdea.Data.Description.ToLower().Contains(SearchText.ToLower(), StringComparison.InvariantCultureIgnoreCase) ||
                                                dcIdea.Data.CreatorUserName.ToLower().Contains(SearchText.ToLower(), StringComparison.InvariantCultureIgnoreCase))) &&

                                              // Gemeindefilter
                                              (!PickerOrganizationFilter.Any() ||
                                               PickerOrganizationFilter.SelectedItem == null! ||
                                               PickerOrganizationFilter.SelectedItem.Key == -1 ||
                                               dcIdea.Data.Companies.Any(x => x.OrganizationId == PickerOrganizationFilter.SelectedItem.Key)));
            UiIdeasVisible = Dc.DcExIdeas.Any();
            //}

            UiIdeas = Dc.DcExIdeas.OrderByDescending(x => x.SortIndex).ToList();
        }

        #region Eventhandler

        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attatch"></param>
        private void AttachDetachVmEvents(bool attatch)
        {
            if (attatch)
            {
                if (!_eventsAttached)
                {
                    _eventsAttached = true;
                    PickerOrganizationFilter.SelectedItemChanged += PickerOrganizationFilterChanged;
                    PickerCommonFilter.SelectedItemChanged += PickerCommonFilterChanged;

                    Dc.DcExIdeas.CollectionEvent += DcExIdeasOnCollectionEvent;
                    Dc.DcExIdeas.CollectionChanged += DcExIdeasOnCollectionChanged;
                    foreach (var dcIdea in Dc.DcExIdeas)
                    {
                        dcIdea.Data.PropertyChanged += DcIdeaOnPropertyChanged;
                    }
                }
            }
            else
            {
                if (_eventsAttached)
                {
                    _eventsAttached = false;
                    PickerOrganizationFilter.SelectedItemChanged -= PickerOrganizationFilterChanged;
                    PickerCommonFilter.SelectedItemChanged -= PickerCommonFilterChanged;

                    Dc.DcExIdeas.CollectionEvent -= DcExIdeasOnCollectionEvent;
                    Dc.DcExIdeas.CollectionChanged -= DcExIdeasOnCollectionChanged;
                    foreach (var dcIdea in Dc.DcExIdeas)
                    {
                        dcIdea.Data.PropertyChanged -= DcIdeaOnPropertyChanged;
                    }
                }
            }
        }

        private void DcIdeaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExIdea.CreatorUserName) ||
                e.PropertyName == nameof(ExIdea.Title) ||
                e.PropertyName == nameof(ExIdea.Description) ||
                e.PropertyName == nameof(ExIdea.IsMine) ||
                e.PropertyName == nameof(ExIdea.IsHelping) ||
                e.PropertyName == nameof(ExIdea.Companies))
            {
                Dispatcher!.RunOnDispatcher(FilterUiIdeas);
            }
        }

        private void DcExIdeasOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var refresh = false;

            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is DcListDataPoint<ExIdea> dcIdea)
                    {
                        dcIdea.Data.PropertyChanged -= DcIdeaOnPropertyChanged;
                    }

                    refresh = true;
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem is DcListDataPoint<ExIdea> dcIdea)
                    {
                        dcIdea.Data.PropertyChanged += DcIdeaOnPropertyChanged;
                    }

                    refresh = true;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var dcIdea in Dc.DcExIdeas)
                {
                    dcIdea.Data.PropertyChanged -= DcIdeaOnPropertyChanged;
                    dcIdea.Data.PropertyChanged += DcIdeaOnPropertyChanged;
                }

                refresh = true;
            }

            if (refresh)
            {
                Dispatcher!.RunOnDispatcher(FilterUiIdeas);
            }
        }

        /// <summary>
        ///     Auswahl Filter Gemeinde geändert -> Daten neu Laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerOrganizationFilterChanged(object sender, SelectedItemEventArgs<VmPickerElement<long>> e)
        {
            _lastOrgFilter = e.CurrentItem;

            if (!View.IsBusy)
            {
                FilterUiIdeas();
            }
        }

        /// <summary>
        ///     Filterung Meine Ideen/Idee Unterstützungen geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerCommonFilterChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumVmMainIdeaCommonFilter>> e)
        {
            _lastCommonFilter = e.CurrentItem;

            if (!View.IsBusy)
            {
                FilterUiIdeas();
            }
        }

        private void PickerArchivedFilterOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<EnumVmMainIdeaArchivedFilter>> e)
        {
            _lastArchivedFilter = e.CurrentItem;

            if (!View.IsBusy)
            {
                FilterUiIdeas();
            }
        }

        /// <summary>
        ///     Collection Event für Ideen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task DcExIdeasOnCollectionEvent(object sender, CollectionEventArgs<DcExIdeasType> e)
        {
            Logging.Log.LogInfo($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): {e.TypeOfEvent} {e.Item.ToString() ?? "NULL"}");

            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            _noUiListUpdate = true;

            try
            {
                if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
                {
                    var homeCompany = Dc.DcExUser.Data.Permissions.FirstOrDefault(x => x.IsMainCompany && x.Town != null);

                    if (homeCompany is null)
                    {
                        await MsgBox.Show(ResViewMainIdea.MsgNeedHometown, ResCommon.MsgTitleError).ConfigureAwait(true);
                        return;
                    }

                    DcExIdeasType? item = null;
                    var newIdea = new ExIdea
                                  {
                                      CreatorUserId = Dc.DcExUser.Data.Id,
                                      CreatorUserName = Dc.DcExUser.Data.Fullname,
                                      CreatorUserImage = Dc.DcExUser.Data.UserImageLink,

                                      Title = Title,
                                      HasPosition = false,
                                      HasTimespan = false,

                                      CanEdit = true,
                                      IsMine = true,
                                      CanSeeReports = Dc.DcExUser.Data.IsSysAdmin ||
                                                      Dc.DcExUser.Data.Permissions.FirstOrDefault(x => x.IsMainCompany)?.UserRole != EnumUserRole.User,

                                      Companies = new List<ExOrganization>
                                                  {
                                                      homeCompany.Town!,
                                                  }
                                  };

                    item = new DcExIdeasType(newIdea);
                    Dc.DcExIdeas.Insert(0, item);
                    item.SortIndex = Dc.DcExIdeas.AllItems.Any() ? Dc.DcExIdeas.AllItems.Max(x => x.SortIndex) + 1 : 1;

                    var r = await Nav.ToViewWithResult(typeof(VmAddIdea), item).ConfigureAwait(true);
                    if (r is EnumVmEditResult result)
                    {
                        if (result != EnumVmEditResult.ModifiedAndStored)
                        {
                            foreach (var need in Dc.DcExIdeaNeeds
                                         .Where(x => x.Data.IdeaId == item.Index)
                                         .ToList())
                            {
                                try
                                {
                                    Dc.DcExIdeaNeeds.Remove(need);
                                }
                                catch (Exception)
                                {
                                    Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Workaroud - neues NuGet");
                                }
                            }

                            try
                            {
                                Dc.DcExIdeas.Remove(item);
                            }
                            catch (Exception)
                            {
                                Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Workaroud - neues NuGet");
                            }
                        }
                        else
                        {
                            Title = string.Empty;
                            EntryTitle.Value = string.Empty;

                            try
                            {
                                await Dc.Chat.Sync().ConfigureAwait(true);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): {ex}");
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }
                }
                else if (e.TypeOfEvent == EnumCollectionEventType.EditRequest && e.Item != null!)
                {
                    var r = await Nav.ToViewWithResult(typeof(VmAddIdea), e.Item).ConfigureAwait(true);
                    if (r is EnumVmEditResult result)
                    {
                        if (result != EnumVmEditResult.ModifiedAndStored)
                        {
                            var p = e.Item;
                            if (p.PossibleNewDataOnServer)
                            {
                                // p.Update();
                            }
                        }
                    }
                    else if (r == null)
                    {
                        // Chat
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }
                }
                else if (e.TypeOfEvent == EnumCollectionEventType.InfoRequest && e.Item != null)
                {
                    View.BusySet(delay: 50);
                    await Nav.ToViewWithResult(typeof(VmIdeaDetails), e.Item).ConfigureAwait(true);
                    View.BusyClear();
                }
                else if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest && e.Item != null)
                {
                    var res = await MsgBox.Show(ResViewMainIdea.MsgDeleteConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                    if (res != VmMessageBoxResult.Yes)
                    {
                        return;
                    }

                    try
                    {
                        Dc.DcExIdeas.Remove(e.Item);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Workaroud - neues NuGet");
                    }

                    var r = await Dc.DcExIdeas.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Löschen fehlgeschlagen - {r.ServerExceptionText}");
                        await MsgBox.Show(ResViewMainIdea.MsgDeleteError +
                                          (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.CustomerRelease ? string.Empty : r.ServerExceptionText))
                            .ConfigureAwait(true);
                        Dc.DcExIdeas.Add(e.Item);
                    }
                    else
                    {
                        CheckSaveBehavior = null;
                        ViewResult = EnumVmEditResult.ModifiedAndStored;
                    }
                }
                else if (e.TypeOfEvent == EnumCollectionEventType.UpdateRequest)
                {
                    IsRefreshing = true;

                    //  Wenn gerade Daten geladen werden - das laden abwarten
                    if (Dc.DcExIdeas.LoadingHost)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Is already Loading - wait for finish");

                        Dc.DcExIdeas.LoadingFromHostEvent += DcExIdeasOnLoadingFromHostEvent;
                        await lfh.WaitOne().ConfigureAwait(true);
                        Dc.DcExIdeas.LoadingFromHostEvent -= DcExIdeasOnLoadingFromHostEvent;
                        await Task.Delay(100).ConfigureAwait(true);
                    }

                    if (await CheckConnected().ConfigureAwait(true))
                    {
                        Dispatcher!.RunOnDispatcher(async () =>
                        {
                            View.BusySet("Lade Ideen", 10);

                            var sw = new Stopwatch();
                            sw.Start();

                            Logging.Log.LogInfo($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Load Ideas start");

                            //FilterUiIdeas();
                            try
                            {
                                await Dc.DcExIdeas.Sync(true).ConfigureAwait(true);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): {ex}");
                            }

                            Logging.Log.LogInfo($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Load Ideas DC ({sw.ElapsedMilliseconds} ms)");

                            sw.Stop();

                            IsRefreshing = false;

                            View.BusyClear();
                        });
                    }
                    else
                    {
                        IsRefreshing = false;
                    }
                }
                else
                {
                    Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): Not implemented!");
                }
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): {ex}");
            }
            finally
            {
                _noUiListUpdate = false;

                if (e.TypeOfEvent != EnumCollectionEventType.UpdateRequest)
                {
                    Dispatcher!.RunOnDispatcher(async () =>
                    {
                        if (e.Item != null && e.Item.DataSource == EnumDcDataSource.LocalSet)
                        {
                            e.Item.DataSource = EnumDcDataSource.LocalCache;
                            e.Item.State = EnumDcListElementState.None;

                            try
                            {
                                await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(DcExIdeasOnCollectionEvent)}): {ex}");
                            }
                        }

                        FilterUiIdeas();
                    });
                }
            }
        }

        private void DcExIdeasOnLoadingFromHostEvent(object sender, bool e)
        {
            lfh.Set();
        }

        /// <summary>
        ///     Notification angeklickt - Idee anzeigen
        /// </summary>
        public static async void CheckLaunchIdea()
        {
            if (_ideaToOpen > 0)
            {
                await CurrentVmMenu!.WaitForConnected().ConfigureAwait(true);

                if (!CurrentVmMenu.Dc.DcExIdeas.AllItems.Any(x => x.Id == _ideaToOpen))
                {
                    var res = -1L;
                    try
                    {
                        res = await CurrentVmMenu.Dc.DcExIdeas.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(CheckLaunchIdea)}): {e}");
                    }

                    var tries = 0;

                    while (res < 0 && tries < 3)
                    {
                        await Task.Delay(100).ConfigureAwait(true);
                        try
                        {
                            res = await CurrentVmMenu.Dc.DcExIdeas.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(CheckLaunchIdea)}): {e}");
                        }

                        tries++;
                    }
                }

                var idea = CurrentVmMenu.Dc.DcExIdeas.AllItems.FirstOrDefault(x => x.Id == _ideaToOpen);
                if (idea != null)
                {
                    _ideaToOpen = null;
                    CurrentVmMenu.Dc.DcExIdeas.CmdItemInfo.Execute(idea);
                }
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            if (_isInNavToView)
            {
                return;
            }

            UpdatePickerElements();
            AttachDetachVmEvents(true);
            FilterUiIdeas();
            await base.OnAppearing(view).ConfigureAwait(true);
            await CheckPhoneConfirmation().ConfigureAwait(true);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override async Task OnLoaded()
        {
            if (_isInNavToView)
            {
                return;
            }

            await base.OnLoaded().ConfigureAwait(true);

            if (Dc.UserRegisteredOnline)
            {
                try
                {
                    await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(OnLoaded)}): {e}");
                }

                CheckLaunchIdea();
            }
            else
            {
                Dc.UserAndDeviceOnlineConnected += DcOnUserAndDeviceOnlineConnected;
            }
        }

        private async void DcOnUserAndDeviceOnlineConnected(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                Dc.UserAndDeviceOnlineConnected -= DcOnUserAndDeviceOnlineConnected;
                FilterUiIdeas();
                try
                {
                    await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(DcOnUserAndDeviceOnlineConnected)}): {ex}");
                }

                CheckLaunchIdea();
            }
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            if (_isInNavToView)
            {
                return base.OnDisappearing(view);
            }

            AttachDetachVmEvents(false);

            if (!_noUiListUpdate)
            {
                UiIdeasVisible = false;
                foreach (var dcListDataPoint in Dc.DcExIdeas)
                {
                    dcListDataPoint.Data.IsVisible = false;
                }
            }

            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Suchen
        /// </summary>
        public VmCommand CmdSearch { get; set; } = null!;

        /// <summary>
        ///     Filter zurücksetzen
        /// </summary>
        public VmCommand CmdFilterClear { get; set; } = null!;

        /// <summary>
        ///     Beitrag Liken
        /// </summary>
        public VmCommand CmdLike { get; set; } = null!;

        /// <summary>
        ///     Privat mit jemanden Chatten
        /// </summary>
        public VmCommand CmdChatUser { get; set; } = null!;

        /// <summary>
        ///     über Beitrag mit Usern Chatten
        /// </summary>
        public VmCommand CmdChatIdea { get; set; } = null!;

        /// <summary>
        ///     Idee melden
        /// </summary>
        public VmCommand CmdReportIdea { get; set; } = null!;

        /// <summary>
        ///     Meldungen der Idee anzeigen
        /// </summary>
        public VmCommand CmdShowReports { get; set; } = null!;

        /// <summary>
        ///     Suche zurücksetzen
        /// </summary>
        public VmCommand GCmdSearchDelete { get; private set; } = null!;

        /// <summary>
        ///     FDilter auf/zu
        /// </summary>
        public VmCommand CmdToggleGrid { get; private set; } = null!;

        /// <summary>
        ///     Position auf Karte zeigen
        /// </summary>
        public VmCommand CmdShowMap { get; set; } = null!;

        /// <summary>
        ///     Kalender öffnen
        /// </summary>
        public VmCommand CmdCalendar { get; set; } = null!;

        /// <summary>
        ///     Filter schließen
        /// </summary>
        public VmCommand CmdCloseFilter { get; set; } = null!;

        /// <summary>
        ///     Filter öffnen
        /// </summary>
        public VmCommand CmdOpenFilter { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            #region allgemein

            EntrySearchText = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewMainIdea.EntrySearchTextTitle,
                ResViewMainIdea.EntrySearchTextPlaceholder,
                this,
                nameof(SearchText),
                showTitle: false,
                showMaxChar: false,
                showHint: false,
                returnAction: () => CmdSearch.Execute(true)
            );
            EntrySearchText.ValidChanged += (sender, args) =>
            {
                CmdSearch.CanExecute();

                if (EntrySearchText.DataValid)
                {
                    CmdSearch.Execute(null!);
                }
            };
            EntrySearchText.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(VmEntry.Value))
                {
                    CmdSearch.CanExecute();

                    if (EntrySearchText.DataValid)
                    {
                        CmdSearch.Execute(null!);
                    }
                }
            };
            EntrySearchText.LostFocusEvent += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(200).ConfigureAwait(true);
                        SearchFocus = false;
                    });
                }
            };

            EntryTitle = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewMainIdea.EntryNewTitle,
                ResViewMainIdea.EntryNewPlaceholder,
                this,
                nameof(Title),
                VmEntryValidators.ValidateFuncStringEmptySearch,
                showTitle: false,
                maxChar: 50,
                showMaxChar: false,
                showHint: false,
                returnAction: () => { Dc.DcExIdeas.CmdAddItem.Execute(null!); });

            Dc.DcExIdeas.CmdUpdateItems.DisplayName = string.Empty;
            Dc.DcExIdeas.CmdUpdateItems.Glyph = Glyphs.Cloud_refresh;
            Dc.DcExIdeas.PullToRefresh = true;

            CmdFilterClear = new VmCommand(ResViewMainIdea.CmdFilterClear, () =>
            {
                EntrySearchText.Value = string.Empty;
                ClearFilters();

                SetPickerDefaults();
            }, glyph: Glyphs.Arrow_left_x);

            CmdSearch = new VmCommand(ResViewMainIdea.CmdSearch, async p =>
            {
                if (SearchFocus)
                {
                    // User Informieren, ob man connected ist. Filtern geht aber auch offline 
                    if (!(p is bool ignoreOnline && ignoreOnline))
                    {
                        await CheckConnected().ConfigureAwait(true);
                    }

                    // LoadDcList via SearchText
                    FilterUiIdeas();

                    if (string.IsNullOrWhiteSpace(SearchText))
                    {
                        SearchFocus = false;
                    }
                }
                else
                {
                    SearchFocus = true;
                    EntrySearchText.Focus();
                }
            }, glyph: Glyphs.Search);

            GCmdSearchDelete = new VmCommand(string.Empty, () =>
            {
                EntrySearchText.Value = string.Empty;
                SearchText = string.Empty;
                FilterUiIdeas();
            }, glyph: Glyphs.Search_remove);

            CmdToggleGrid = new VmCommand(string.Empty, () =>
                FilterDetailGridEnabled = !FilterDetailGridEnabled, glyph: Glyphs.Filter_1);

            CmdCloseFilter = new VmCommand(string.Empty, () => { FilterDetailGridEnabled = false; });
            CmdOpenFilter = new VmCommand(string.Empty, () => { FilterDetailGridEnabled = true; });

            Dc.DcExIdeas.CmdAddItem.DisplayName = ResViewMainIdea.CmdAddIdea;
            Dc.DcExIdeas.CmdAddItem.Glyph = Glyphs.Add_circle;

            #endregion

            #region Je Idee

            CmdLike = new VmCommand(string.Empty, async p =>
            {
                if (!(p is DcExIdeasType idea))
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                View.BusySet("Sichere ...", 50);

                var res = await Dc.LikeIdea(idea.Index).ConfigureAwait(true);
                if (!res.Ok)
                {
                    await MsgBox.Show(ResViewMainIdea.MsgLikeError +
                                      (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.CustomerRelease ? string.Empty : res.ServerExceptionText))
                        .ConfigureAwait(true);
                }
                else
                {
                    if (idea.DataSource == EnumDcDataSource.LocalSet)
                    {
                        idea.DataSource = EnumDcDataSource.LocalCache;
                        idea.State = EnumDcListElementState.None;
                    }

                    try
                    {
                        await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(InitializeCommands)}): {e}");
                    }
                }


                CmdLike.DisplayName = idea.Data.IsLiked ? "Du magst diese Idee" : "Du kannst die Idee liken";

                View.BusyClear();
            }, glyph: Glyphs.Like);

            CmdChatUser = new VmCommand(string.Empty, async p =>
            {
                if (!(p is DcListDataPoint<ExIdea> item))
                {
                    return;
                }

                if (item.Data.IsMine)
                {
                    return;
                }

                Dc.Chat.SelectedUiChat = null!;

                if (!item.Data.PrivateChatId.HasValue)
                {
                    if (!await Dc.Chat.NewChat(item.Data.CreatorUserId, item.Index.ToString()).ConfigureAwait(true))
                    {
                        await MsgBox.Show($"Privater Chat für Idee {item.Data.Title} mit Ersteller Id {item.Data.CreatorUserId} konnte nicht erstellt werden.").ConfigureAwait(true);
                        return;
                    }
                }
                else
                {
                    try
                    {
                        Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PrivateChatId.Value);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            await Dc.Chat.Sync(item.Data.PrivateChatId).ConfigureAwait(true);
                            Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PrivateChatId.Value);
                        }
                        catch (Exception)
                        {
                            await MsgBox.Show($"Privater Chat für Private Chat Id{item.Data.PrivateChatId.Value} konnte nicht geöffnet werden.").ConfigureAwait(true);
                        }
                    }
                }

                if (Dc.Chat.SelectedUiChat != null!)
                {
                    _isInNavToView = true;
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                    _isInNavToView = true;
                }
            }, glyph: Glyphs.Conversation_chat_1);

            CmdChatIdea = new VmCommand(string.Empty, async p =>
            {
                if (!(p is DcListDataPoint<ExIdea> item))
                {
                    return;
                }

                Dc.Chat.SelectedUiChat = null!;

                try
                {
                    Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PublicChatId);
                }
                catch (Exception)
                {
                    try
                    {
                        await Dc.Chat.Sync(item.Data.PublicChatId).ConfigureAwait(true);
                        Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PublicChatId);
                    }
                    catch (Exception)
                    {
                        await MsgBox.Show($"Public Chat (id {item.Data.PublicChatId}) für Idee {item.Data.Title} konnte nicht geöffnet werden").ConfigureAwait(true);
                    }
                }

                if (Dc.Chat.SelectedUiChat != null!)
                {
                    _isInNavToView = true;
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                    _isInNavToView = false;
                }
            }, glyph: Glyphs.Conversation_chat_1);

            CmdShowMap = new VmCommand(string.Empty, async p =>
            {
                if (p is DcListDataPoint<ExIdea> item)
                {
                    _noUiListUpdate = true;
                    _isInNavToView = true;

                    _ = await Nav.ToViewWithResult(typeof(VmIdeaMap), item).ConfigureAwait(true);
                    _isInNavToView = false;

                    _noUiListUpdate = false;
                }
            }, glyph: Glyphs.Maps_pin_1);

            CmdCalendar = new VmCommand(string.Empty, async p =>
            {
                if (p is DcListDataPoint<ExIdea> item)
                {
                    var appointment = new BissAppointment
                                      {
                                          StartTime = item.Data.From,
                                          EndTime = item.Data.To,
                                          Title = item.Data.Title,
                                          Description = item.Data.Description
                                      };

                    _isInNavToView = true;
                    await Open.Calendar(appointment).ConfigureAwait(true);
                    _isInNavToView = false;
                }
            }, glyph: Glyphs.Calendar_add_1);

            Dc.DcExIdeas.CmdItemInfo.DisplayName = string.Empty;
            Dc.DcExIdeas.CmdItemInfo.Glyph = Glyphs.Information_circle;

            Dc.DcExIdeas.CmdEditItem.DisplayName = string.Empty;
            Dc.DcExIdeas.CmdEditItem.Glyph = Glyphs.Pencil_1;

            CmdReportIdea = new VmCommand(string.Empty, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (p is DcListDataPoint<ExIdea> idea)
                {
                    DcListDataPoint<ExReport>? item = null;
                    _noUiListUpdate = true;

                    var newReport = new ExReport
                                    {
                                        UserId = Dc.DcExUser.Data.Id,
                                        UserImage = Dc.DcExUser.Data.UserImageLink,
                                        UserName = Dc.DcExUser.Data.Fullname,
                                        IdeaId = idea.Index
                                    };

                    item = new DcListDataPoint<ExReport>(newReport);

                    Dc.DcExReports.Add(item);

                    _isInNavToView = true;
                    var r = await Nav.ToViewWithResult(typeof(VmAddReport), item).ConfigureAwait(true);

                    if (r is EnumVmEditResult result)
                    {
                        if (result != EnumVmEditResult.ModifiedAndStored)
                        {
                            try
                            {
                                Dc.DcExReports.Remove(item);
                            }
                            catch (Exception)
                            {
                                Logging.Log.LogWarning($"[{nameof(VmMainIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }

                    _isInNavToView = false;

                    _noUiListUpdate = false;

                    if (idea.DataSource == EnumDcDataSource.LocalSet)
                    {
                        idea.DataSource = EnumDcDataSource.LocalCache;
                        idea.State = EnumDcListElementState.None;

                        try
                        {
                            await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmMainIdea)}]({nameof(InitializeCommands)}): {e}");
                        }
                    }
                }
            }, glyph: Glyphs.Flag_warning);

            Dc.DcExIdeas.CmdRemoveItem.DisplayName = string.Empty;
            Dc.DcExIdeas.CmdRemoveItem.Glyph = Glyphs.Bin_1;

            CmdShowReports = new VmCommand(string.Empty, async idea =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                _isInNavToView = true;
                var _ = await Nav.ToViewWithResult(typeof(VmShowReports), idea).ConfigureAwait(false);
                _isInNavToView = false;
            }, glyph: Glyphs.Earth_warning);

            #endregion
        }

        #endregion
    }
}