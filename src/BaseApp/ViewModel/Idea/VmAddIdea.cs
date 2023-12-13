// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.Helper;
using BaseApp.ViewModel.Chat;
using BaseApp.ViewModel.Reports;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Map.Base;
using Biss.Apps.Map.Component;
using Biss.Apps.Map.Helper;
using Biss.Apps.Map.Model;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Interfaces;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Model.Report;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

// ReSharper disable All

namespace BaseApp.ViewModel.Idea
{
    /// <summary>
    ///     <para>ViewModel Add/EditIdea</para>
    ///     Klasse VmAddIdea. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddIdea")]
    public class VmAddIdea : VmEditDcListPoint<ExIdea>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMainIdea.DesignInstance}"
        /// </summary>
        public static VmAddIdea DesignInstance = new VmAddIdea();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMainIdea.DesignInstance}"
        /// </summary>
        public static ExOrganization DesignInstancePermission = new ExOrganization();

        private long _beforeStoreIndex;

        private DateTime _fromDate;
        private TimeSpan _fromTime;
        private DateTime _toDate;
        private TimeSpan _toTime;

        /// <summary>
        ///     Bearbeiten eines DC Listenpunkts
        /// </summary>
        public VmAddIdea() : base(ResViewAddIdea.LblTitle, subTitle: ResViewAddIdea.LblSubTitle)
        {
            SetViewProperties(true);
            View.ShowUser = false;
            View.BusySet("Lade ...", 100);
        }

        #region Properties

        /// <summary>
        ///     Platform Mobilgerät
        /// </summary>
        public bool PlatformMobile => DeviceInfo.Plattform == EnumPlattform.XamarinAndroid ||
                                      DeviceInfo.Plattform == EnumPlattform.XamarinIos;

        /// <summary>
        ///     Eingabefeld Titel
        /// </summary>
        public VmEntry EntryTitle { get; set; } = null!;

        /// <summary>
        ///     Eingabefeld Beschreibung
        /// </summary>
        public VmEntry EntryDescription { get; set; } = null!;

        /// <summary>
        ///     Picker für Gemeinden
        /// </summary>
        public VmPicker<ExOrganization> PickerOrganizations { get; private set; } = new VmPicker<ExOrganization>(nameof(PickerOrganizations));

        /// <summary>
        ///     From - nur DatumsTeil
        /// </summary>
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                if (Data != null!)
                {
                    var dateShift = value.Date.Subtract(Data.From.Date);

                    Data.From = value.Add(FromTime);

                    ToDate = ToDate.Add(dateShift);
                }
            }
        }

        /// <summary>
        ///     From - nur Zeitteil
        /// </summary>
        public TimeSpan FromTime
        {
            get => _fromTime;
            set
            {
                _fromTime = value;
                if (Data != null!)
                {
                    var timeShift = value.Subtract(Data.From.TimeOfDay);

                    Data.From = FromDate.Add(value);

                    ToTime = ToTime.Add(timeShift);
                }
            }
        }

        /// <summary>
        ///     To - nur Datumsteil
        /// </summary>
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                if (Data != null!)
                {
                    Data.To = value.Add(ToTime);
                }
            }
        }

        /// <summary>
        ///     To - nur Zeitteil
        /// </summary>
        public TimeSpan ToTime
        {
            get => _toTime;
            set
            {
                _toTime = value;
                if (Data != null!)
                {
                    Data.To = ToDate.Add(value);
                }
            }
        }

        /// <summary>
        ///     Zum Kalender navigieren
        /// </summary>
        public bool NavToCal { get; set; }

        /// <summary>
        ///     Kartenfunktionen
        /// </summary>
        public BissMap Map => this.BcBissMap()!.BissMap;

        /// <summary>
        ///     Pin auf Karte
        /// </summary>
        public BmPoint MapPin { get; set; } = null!;

        /// <summary>
        ///     Position bei MapKlick ändern
        /// </summary>
        public bool EditPosition { get; set; }

        /// <summary>
        ///     Eingabefeld für Addresse
        /// </summary>
        public VmEntry EntryAddress { get; set; } = null!;

        /// <summary>
        ///     Helfer bei dieser Idee
        /// </summary>
        public List<DcListDataPoint<ExIdeaHelper>> UiHelpers { get; set; } = new List<DcListDataPoint<ExIdeaHelper>>();

        /// <summary>
        ///     benötigte Sachen dieser Idee
        /// </summary>
        public List<DcListDataPoint<ExIdeaNeed>> UiNeeds { get; set; } = new List<DcListDataPoint<ExIdeaNeed>>();

        /// <summary>
        ///     Die Regionen des Users die nicht der Idee hinzugefügt wurden.
        /// </summary>
        public List<ExOrganization> AvailableRegions => Dc.DcExUser.Data.Permissions
            .Where(p => Data.Companies.All(s => p.CompanyId != s.OrganizationId))
            .Select(p => p.Town!).Where(t => t != null).ToList() ?? new List<ExOrganization>();

        /// <summary>
        ///     Die Regionen die der Idee hinzugefügt wurden
        /// </summary>
        public List<ExOrganization> SelectedRegions => Data?.Companies ?? new List<ExOrganization>();

        #endregion

        private async void GetAdressFromPosition()
        {
            if (!Data.HasPosition) return;

            if (Data.Location == null!)
                return;

            try
            {
                var addr = await this.BcBissMap()!.GetAddressFromPosition(Data.Location).ConfigureAwait(true);

                if (addr != null)
                {
                    Data.LocationAddress = addr.ToString();
                    if (EntryAddress != null!)
                        EntryAddress.Value = Data.LocationAddress;
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}) - {nameof(CmdGetAddressForPosition)}: {e}");
            }

            await MsgBox.Show(ResViewAddIdea.MsgNoAddressFound).ConfigureAwait(true);
        }

        private void DcNeedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher!.RunOnDispatcher(async () =>
            {
                UpdateNeedUiList();
                // Doppelt, weil die Liste manchmal leer bleibt
                await Task.Delay(50).ConfigureAwait(true);
                UpdateNeedUiList();
            });
        }

        private void DcHelpersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher!.RunOnDispatcher(async () =>
            {
                UpdateHelperUiList();
                // Doppelt, weil die Liste manchmal leer bleibt
                await Task.Delay(50).ConfigureAwait(true);
                UpdateHelperUiList();
            });
        }

        private void DcOnUserChanged(object sender, DataChangedEventArgs e)
        {
            UpdatePicker();
        }

        /// <summary>
        ///     Listen im UI aktualisieren
        /// </summary>
        private void UpdateUiLists()
        {
            UpdateHelperUiList();
            UpdateNeedUiList();
        }

        private void UpdateNeedUiList()
        {
            var need = new List<DcListDataPoint<ExIdeaNeed>>();
            foreach (var dataPoint in Dc.DcExIdeaNeeds.Where(x => x.Data.IdeaId == DcListDataPoint.Index))
            {
                need.Add(dataPoint);
            }

            UiNeeds = need;
        }

        private void UpdateHelperUiList()
        {
            var helpers = new List<DcListDataPoint<ExIdeaHelper>>();
            foreach (var dataPoint in Dc.DcExIdeaHelpers.Where(x => x.Data.IdeaId == DcListDataPoint.Index))
            {
                helpers.Add(dataPoint);
            }

            UiHelpers = helpers;
        }

        /// <summary>
        ///     Picker für Gemeinden neu Befüllen.
        /// </summary>
        private void UpdatePicker()
        {
            PickerOrganizations.Clear();

            if (AvailableRegions.Count == 0)
                PickerOrganizations.AddKey(new ExOrganization()
                                           {
                                               Name = ResViewAddIdea.LblNoOtherTowns,
                                               OrganizationId = -1,
                                           }, ResViewAddIdea.LblNoOtherTowns);

            foreach (var org in AvailableRegions)
            {
                PickerOrganizations.AddKey(org, org.NamePlzString);
            }

            var first = PickerOrganizations.FirstOrDefault();

            if (first != null)
                PickerOrganizations.SelectKey(first.Key);
        }

        private void UpdateIdeaRegions()
        {
            this.InvokeOnPropertyChanged(nameof(AvailableRegions));
            this.InvokeOnPropertyChanged(nameof(SelectedRegions));
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            Dc.DcExIdeas.FilterClear();

            if (NavToCal)
            {
                IsNavigatedToNavToViewWithResult = false;
                NavToCal = false;
                IsLoaded = true;
                View.CmdSaveHeader?.CanExecute();
            }

            Dc.DcExIdeaHelpers.CollectionChanged += DcHelpersOnCollectionChanged;
            Dc.DcExIdeaNeeds.CollectionChanged += DcNeedsOnCollectionChanged;
            Dc.DcExUser.DataChangedEvent += DcOnUserChanged;

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.IsSelected = true;
                return;
            }

            await WaitForConnected().ConfigureAwait(true);
            await CheckPhoneConfirmation().ConfigureAwait(true);

            await base.OnActivated(args).ConfigureAwait(true);

            if (args is DcListDataPoint<ExIdea> idea)
            {
                FromDate = idea.Data.From.Date;
                FromTime = idea.Data.From.TimeOfDay;

                ToDate = idea.Data.To.Date;
                ToTime = idea.Data.To.TimeOfDay;
            }

            if (DcListDataPoint.State == EnumDcListElementState.New)
            {
                PageSubTitle = "Neue Idee anlegen";
            }
            else
            {
                PageSubTitle = "Idee bearbeiten";
            }

            if (Data.CanEdit)
            {
                EntryTitle = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddIdea.EntryTitleTitle,
                    ResViewAddIdea.EntryPlaceholderTitle,
                    Data,
                    nameof(ExIdea.Title),
                    VmEntryValidators.ValidateFuncStringEmpty,
                    showTitle: false,
                    maxChar: 50);

                EntryDescription = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddIdea.EntryTitleDescription,
                    ResViewAddIdea.EntryPlaceholderDescription,
                    Data,
                    nameof(ExIdea.Description),
                    VmEntryValidators.ValidateFuncStringEmpty,
                    showTitle: false,
                    showMaxChar: false);

                EntryAddress = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddIdea.EntryAddressTitle,
                    ResViewAddIdea.EntryAddressPlaceholder,
                    Data,
                    nameof(ExIdea.LocationAddress),
                    showTitle: false,
                    showMaxChar: false);
            }
            else
            {
                throw new Exception("Nicht mehr erlaubt! Eigene View.");
                //_commandSaveHeaderBackup = View.CmdSaveHeader;
                //View.CmdSaveHeader = null;

                //// TODO Sollte so eigentlich auch funktionieren
                ////View.CmdSaveHeader!.IsVisible = false;

                //CheckSaveBehavior = null;
                //ViewResult = EnumVmEditResult.ModifiedAndStored;
                //PageTitle = Data.Title;
            }

            #region Gemeinden

            UpdatePicker();
            UpdateIdeaRegions();

            #endregion

            #region Map, Pin und Position

            Map.MapItems.Clear();

            var uiTitle = string.IsNullOrWhiteSpace(Data.Title) ? "Idee" : Data.Title;
            var defLoc = new BissPosition(48.30058479926201, 15.654511760954717);

            // Liefert dissapearing Event auf Android
            IsNavigatedToNavToViewWithResult = true;
            await this.BcBissMap()!.CheckUserLocationPermission().ConfigureAwait(true);
            IsNavigatedToNavToViewWithResult = false;

            if (Data.HasPosition && BmDistance.BetweenPoints(Data.Location, new BissPosition()).Meters > 1)
            {
                Map.SetCenterAndZoom(Data.Location, BmDistance.FromMeters(500));
            }
            else
            {
                Map.SetCenterAndZoom(defLoc, BmDistance.FromKilometers(150));

                if (Map.CanShowUser)
                {
                    // Keine Location und User sagt ja zur Location
                    var loc = await this.BcBissMap()!.GetUserLocation().ConfigureAwait(true);

                    if (loc != null && BmDistance.BetweenPoints(loc, new BissPosition()).Meters > 1)
                    {
                        Data.Location = loc;
                        Map.SetCenterAndZoom(loc, BmDistance.FromMeters(loc.Accuracy.Meters + 500));
                    }
                    else
                    {
                        // Standardmässig mal iwo in Mitte von NOE hin
                        Data.Location = defLoc;
                    }
                }
                else
                {
                    // Standardmässig mal iwo in Mitte von NOE hin
                    Data.Location = defLoc;
                }
            }

            MapPin = new BmPoint(uiTitle)
                     {
                         Position = Data.Location,
                     };

            // PropChanged für Drag'n'Drop vom Pin
            MapPin.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(BmPoint.Position))
                {
                    if (EditPosition)
                    {
                        if (BmDistance.BetweenPoints(Data.Location, MapPin.Position).Meters > 1)
                        {
                            Data.Location = MapPin.Position;
                            GetAdressFromPosition();
                        }
                    }
                }
            };

            if (Data.HasPosition)
            {
                Map.MapItems.Add(MapPin);
            }

            Data.PropertyChanged += async (sender, eventArgs) =>
            {
                // Wenn Postion für Idee eingeschaltet wird - direkt die eigene Position abfragen und Karte dorthin setzen
                if (eventArgs.PropertyName == nameof(ExIdea.HasPosition))
                {
                    Map.MapItems.Clear();

                    if (Data.HasPosition)
                    {
                        View.BusySet("Aktualisiere Position", 50);
                        Map.MapItems.Add(MapPin);
                        var loc = await this.BcBissMap()!.GetUserLocation().ConfigureAwait(true);
                        if (loc != null)
                        {
                            Map.SetCenterAndZoom(loc, BmDistance.FromMeters(loc.Accuracy.Meters + 500));
                            Data.Location = loc;
                            GetAdressFromPosition();
                        }

                        View.BusyClear();
                    }
                    else
                    {
                        Data.Location = new BissPosition();
                        Data.LocationAddress = string.Empty;
                    }
                }

                // Location geändert -> Pin verschieben
                if (eventArgs.PropertyName == nameof(ExIdea.Location))
                {
                    if (Data.HasPosition)
                    {
                        MapPin.Position = Data.Location;
                    }
                }

                // Titel geändert -> Pin umbenennen
                if (eventArgs.PropertyName == nameof(ExIdea.Title))
                {
                    if (MapPin != null!)
                    {
                        MapPin.Label = string.IsNullOrWhiteSpace(Data.Title) ? "Idee" : Data.Title;
                    }
                }
            };

            // Koordinaten angeklickt -> Pin dort hin setzen
            Map.CoordinatesClicked += (sender, eventArgs) =>
            {
                if (Data.HasPosition && EditPosition)
                {
                    Data.Location = eventArgs.Position;
                    GetAdressFromPosition();
                }
            };

            #endregion

            if (DcListDataPoint.State != EnumDcListElementState.New)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                await Dc.DcExIdeaHelpers.WaitDataFromServerAsync(filter: new ExIdeaHelperFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
                await Dc.DcExIdeaNeeds.WaitDataFromServerAsync(filter: new ExIdeaNeedFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                //CmdAddToCalendar.IsVisible = false;
            }

            UpdateUiLists();

            View.BusyClear();

            CheckBeforeSaving = async () =>
            {
                _beforeStoreIndex = DcListDataPoint.Index;

                if (Data.HasTimespan)
                {
                    if (Data.To < Data.From)
                    {
                        await MsgBox.Show(ResViewAddIdea.ValErrorTimeFrame).ConfigureAwait(true);
                        return false;
                    }
                }

                if (Data.HasPosition)
                {
                    if (Data.LocationAsText)
                    {
                        if (string.IsNullOrWhiteSpace(Data.LocationAddress))
                        {
                            await MsgBox.Show(ResViewAddIdea.ValErrorAddress).ConfigureAwait(true);
                            return false;
                        }
                    }
                    else
                    {
                        if (BmDistance.BetweenPoints(Data.Location, new BissPosition()).Meters < 1)
                        {
                            await MsgBox.Show(ResViewAddIdea.ValErrorPosition).ConfigureAwait(true);
                            return false;
                        }
                    }
                }

                if (Data.Companies.Count == 0)
                {
                    await MsgBox.Show(ResViewAddIdea.ValErrorNoCompanies).ConfigureAwait(true);
                    return false;
                }

                if (Data.IdeaImageId != null && Data.IdeaImageId < 0)
                {
                    try
                    {
                        var res = await Dc.TransferFile(new FileInfo(Data.IdeaImage), string.Empty).ConfigureAwait(true);

                        if (res != null && res.StoreResult.DataOk)
                        {
                            Data.IdeaImage = res.FileLink;
                            Data.IdeaImageId = res.DbId;
                        }
                        else
                        {
                            Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnActivated)}): {res?.StoreResult?.ServerExceptionText}");
                            await MsgBox.Show("Bild nicht gepseichert!").ConfigureAwait(true);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnActivated)}): {e}");
                        await MsgBox.Show("Bild nicht gepseichert!").ConfigureAwait(true);
                        return false;
                    }
                }

                return true;
            };
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            View.BusyClear();
            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override async Task OnDisappearing(IView view)
        {
            Dc.DcExIdeaHelpers.CollectionChanged -= DcHelpersOnCollectionChanged;
            Dc.DcExIdeaNeeds.CollectionChanged -= DcNeedsOnCollectionChanged;
            Dc.DcExUser.DataChangedEvent -= DcOnUserChanged;

            // Es wird final wegnavigiert
            if (!IsNavigatedToNavToViewWithResult &&
                ViewResult is EnumVmEditResult editRes)
            {
                // Änderungen an der Idee wurden erfolgreich verspeichert
                if (editRes == EnumVmEditResult.ModifiedAndStored)
                {
                    // Die Needs sichern
                    if (Dc.DcExIdeaNeeds.Any())
                    {
                        if (_beforeStoreIndex < 0)
                        {
                            foreach (var dataPoint in Dc.DcExIdeaNeeds.Where(x => x.Data.IdeaId == _beforeStoreIndex))
                            {
                                dataPoint.Data.IdeaId = DcListDataPoint.Index;
                            }
                        }

                        var needRes = await Dc.DcExIdeaNeeds.StoreAll().ConfigureAwait(true);
                        if (!needRes.DataOk)
                        {
                            Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {needRes.ServerExceptionText}");
                        }
                    }

                    // Die Helper sichern
                    if (Dc.DcExIdeaHelpers.Any())
                    {
                        if (_beforeStoreIndex < 0)
                        {
                            foreach (var dataPoint in Dc.DcExIdeaHelpers.Where(x => x.Data.IdeaId == _beforeStoreIndex))
                            {
                                dataPoint.Data.IdeaId = DcListDataPoint.Index;
                            }
                        }

                        var helperRes = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                        if (!helperRes.DataOk)
                        {
                            Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {helperRes.ServerExceptionText}");
                        }
                    }
                }
            }

            await base.OnDisappearing(view).ConfigureAwait(true);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
#pragma warning disable CA1505
        protected override void InitializeCommands()
#pragma warning restore CA1505
        {
            base.InitializeCommands();

            View.CmdSaveHeader!.Glyph = Glyphs.Check;

            #region Regions

            CmdAddRegion = new VmCommand(ResViewAddIdea.CmdAddRegion, () =>
            {
                if (PickerOrganizations.SelectedItem == null! || PickerOrganizations.SelectedItem.Key.OrganizationId < 0)
                    return;

                var selectedTown = PickerOrganizations.SelectedItem.Key;
                if (selectedTown != null!)
                {
                    Data.Companies.Add(selectedTown);
                }

                Data.Companies = Data.Companies.ToList();

                UpdateIdeaRegions();
                UpdatePicker();
            }, glyph: Glyphs.Add_circle);

            CmdRemoveRegion = new VmCommand(ResViewAddIdea.CmdRemoveRegion, async r =>
            {
                if (r is ExOrganization org)
                {
                    var res = await MsgBox.Show(ResViewAddIdea.MsgConfirmRegionDelete, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                    if (res != VmMessageBoxResult.Yes)
                    {
                        return;
                    }

                    Data.Companies.Remove(org);

                    Data.Companies = Data.Companies.ToList();

                    UpdateIdeaRegions();
                    UpdatePicker();
                }
            }, glyph: Glyphs.Bin);

            #endregion

            //CmdAddToCalendar = new VmCommand(ResViewAddIdea.CmdAddToCalendar, async () =>
            //{
            //    var appointment = new BissAppointment()
            //    {
            //        StartTime = Data.From,
            //        EndTime = Data.To,
            //        Title = Data.Title,
            //        Description = Data.Description
            //    };

            //    NavToCal = true;
            //    IsNavigatedToNavToViewWithResult = true;

            //    await Open.Calendar(appointment).ConfigureAwait(true);
            //}, glyph: Glyphs.Calendar_add_1);

            CmdShowMap = new VmCommand(string.Empty, async p =>
            {
                IsNavigatedToNavToViewWithResult = true;

                await Nav.ToViewWithResult(typeof(VmIdeaMap), DcListDataPoint).ConfigureAwait(true);

                IsNavigatedToNavToViewWithResult = false;
            }, glyph: Glyphs.Maps_pin_1);

            CmdChatUser = new VmCommand(string.Empty, async p =>
            {
                if (!(p is DcListDataPoint<ExIdea> item))
                    return;


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
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
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
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
            }, glyph: Glyphs.Conversation_chat_1);

            CmdReportIdea = new VmCommand(ResViewAddIdea.CmdReportIdea, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                DcListDataPoint<ExReport>? item = null;

                var newReport = new ExReport
                                {
                                    UserId = Dc.DcExUser.Data.Id,
                                    UserImage = Dc.DcExUser.Data.UserImageLink,
                                    UserName = Dc.DcExUser.Data.Fullname,
                                    IdeaId = DcListDataPoint.Index
                                };

                item = new DcListDataPoint<ExReport>(newReport);

                Dc.DcExReports.Add(item);

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
                            Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }, glyph: Glyphs.Flag_warning);

            CmdDeleteIdea = new VmCommand(ResViewAddIdea.CmdDeleteIdea, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                var res = await MsgBox.Show(ResViewAddIdea.MsgConfirmIdeaDelete, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    Dc.DcExIdeas.Remove((DcExIdeasType) DcListDataPoint);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                }

                var r = await Dc.DcExIdeas.StoreAll().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Löschen fehlgeschlagen - {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewAddIdea.MsgDeleteFail).ConfigureAwait(true);
                    Dc.DcExIdeas.Add((DcExIdeasType) DcListDataPoint);
                }
                else
                {
                    CheckSaveBehavior = null;
                    ViewResult = EnumVmEditResult.ModifiedAndStored;
                    await Nav.Back().ConfigureAwait(true);
                }
            }, glyph: Glyphs.Bin_1);

            CmdToggleTimeSpan = new VmCommand(string.Empty, () => { Data.HasTimespan = !Data.HasTimespan; });

            #region Position

            CmdTogglePosition = new VmCommand(string.Empty, () => { Data.HasPosition = !Data.HasPosition; });

            CmdEditPosition = new VmCommand(ResViewAddIdea.CmdEditPosition, () => { EditPosition = true; }, glyph: Glyphs.Earth_edit);

            CmdEndEditPosition = new VmCommand(ResViewAddIdea.CmdEndEditPosition, () => { EditPosition = false; }, glyph: Glyphs.Earth_check);

            CmdGetAddressForPosition = new VmCommand(string.Empty, async () =>
                {
                    if (!Data.HasPosition) return;

                    if (Data.Location == null!)
                        return;

                    try
                    {
                        var addr = await this.BcBissMap()!.GetAddressFromPosition(Data.Location).ConfigureAwait(true);

                        if (addr != null)
                        {
                            Data.LocationAddress = addr.ToString();
                            if (EntryAddress != null!)
                                EntryAddress.Value = Data.LocationAddress;
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}) - {nameof(CmdGetAddressForPosition)}: {e}");
                    }

                    await MsgBox.Show(ResViewAddIdea.MsgNoAddressFound).ConfigureAwait(true);
                }, glyph: $"{Glyphs.Maps_pin_2} {Glyphs.Arrow_right} {Glyphs.Real_estate_search_house_1}");

            CmdGetPositionFromAddress = new VmCommand(string.Empty, async () =>
            {
                if (!Data.HasPosition) return;

                if (string.IsNullOrWhiteSpace(Data.LocationAddress))
                {
                    await MsgBox.Show(ResViewAddIdea.MsgNoAddressToSearch).ConfigureAwait(true);
                    return;
                }

                try
                {
                    var loc = await this.BcBissMap()!.GetPositionFromAddress(new BissAddress()
                                                                             {
                                                                                 StreetName = Data.LocationAddress,
                                                                                 StreetNumber = string.Empty,
                                                                                 ZipCode = 0,
                                                                                 City = string.Empty,
                                                                                 Region = string.Empty,
                                                                                 State = string.Empty,
                                                                                 Country = string.Empty,
                                                                             }).ConfigureAwait(true);

                    if (loc != null)
                    {
                        Data.Location = loc;
                        if (MapPin != null!)
                        {
                            MapPin.Position = Data.Location;
                            Map.SetCenterAndZoom(Data.Location, BmDistance.FromMeters(500));
                        }

                        return;
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}) - {nameof(CmdGetPositionFromAddress)}: {e}");
                }

                await MsgBox.Show(ResViewAddIdea.MsgNoPositionFound).ConfigureAwait(true);
            }, glyph: Glyphs.Maps_search);

            #endregion

            #region Helper

            CmdAddHelper = new VmCommand(ResViewAddIdea.CmdAddHelper, async () =>
            {
                // Bei neuen Ideen nicht möglich
                if (DcListDataPoint.State == EnumDcListElementState.New)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                if (Dc.DcExIdeaNeeds.Any(x =>
                        x.State != EnumDcListElementState.None &&
                        x.Data.IdeaId == DcListDataPoint.Index))
                {
                    await MsgBox.Show(ResViewAddIdea.MsgNeedsSave).ConfigureAwait(true);
                    return;
                }

                IsNavigatedToNavToViewWithResult = true;

                DcListDataPoint<ExIdeaHelper>? item = null;

                var newItem = new ExIdeaHelper
                              {
                                  IdeaId = DcListDataPoint.Index,

                                  UserId = Dc.DcExUser.Data.Id,
                                  UserName = Dc.DcExUser.Data.Fullname,
                                  UserImage = Dc.DcExUser.Data.UserImageLink,

                                  From = Data.HasTimespan ? Data.From : DateTime.Now,
                                  To = Data.HasTimespan ? Data.To : DateTime.Now,

                                  CanEdit = true,
                                  CanDelete = true,
                                  IsMine = true,
                              };
                item = new DcListDataPoint<ExIdeaHelper>(newItem);
                Dc.DcExIdeaHelpers.Add(item);
                var r = await Nav.ToViewWithResult(typeof(VmAddHelper), item).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result == EnumVmEditResult.NotModified)
                    {
                        try
                        {
                            Dc.DcExIdeaHelpers.Remove(item);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                        }
                    }
                    else
                    {
                        CheckSaveOverride = true;

                        if (DcListDataPoint.State != EnumDcListElementState.New)
                        {
                            var helperRes = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                            if (!helperRes.DataOk)
                            {
                                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {helperRes.ServerExceptionText}");
                            }
                        }

                        item.Update();
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                IsNavigatedToNavToViewWithResult = false;
                UpdateUiLists();
            }, glyph: Glyphs.Add_circle);

            CmdEditHelper = new VmCommand(ResViewAddIdea.CmdEditHelper, async helper =>
            {
                // Bei neuen Ideen nicht möglich
                if (DcListDataPoint.State == EnumDcListElementState.New)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                if (Dc.DcExIdeaNeeds.Any(x =>
                        x.State != EnumDcListElementState.None &&
                        x.Data.IdeaId == DcListDataPoint.Index))
                {
                    await MsgBox.Show(ResViewAddIdea.MsgNeedsSave).ConfigureAwait(true);
                    return;
                }

                IsNavigatedToNavToViewWithResult = true;

                if (helper is DcListDataPoint<ExIdeaHelper> item)
                {
                    var r = await Nav.ToViewWithResult(typeof(VmAddHelper), item).ConfigureAwait(true);
                    if (r is EnumVmEditResult result)
                    {
                        if (result == EnumVmEditResult.NotModified)
                        {
                            var p = item;
                            if (p.PossibleNewDataOnServer)
                            {
                                p.Update();
                            }
                        }
                        else
                        {
                            CheckSaveOverride = true;

                            if (DcListDataPoint.State != EnumDcListElementState.New)
                            {
                                var helperRes = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                                if (!helperRes.DataOk)
                                {
                                    Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {helperRes.ServerExceptionText}");
                                }

                                item.Update();
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }
                }

                IsNavigatedToNavToViewWithResult = false;
                UpdateUiLists();
            }, glyph: Glyphs.Pencil_1);

            CmdDeleteHelper = new VmCommand(ResViewAddIdea.CmdDeleteHelper, async p =>
            {
                // Bei neuen Ideen nicht möglich
                if (DcListDataPoint.State == EnumDcListElementState.New)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                if (Dc.DcExIdeaNeeds.Any(x =>
                        x.State != EnumDcListElementState.None &&
                        x.Data.IdeaId == DcListDataPoint.Index))
                {
                    await MsgBox.Show(ResViewAddIdea.MsgNeedsSave).ConfigureAwait(true);
                    return;
                }

                var res = await MsgBox.Show(ResViewAddIdea.MsgConfirmHelperDelete, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (p is DcListDataPoint<ExIdeaHelper> item)
                {
                    try
                    {
                        Dc.DcExIdeaHelpers.Remove(item);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }

                    if (DcListDataPoint.State != EnumDcListElementState.New)
                    {
                        var r = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                        if (!r.DataOk)
                        {
                            await MsgBox.Show(ResViewAddIdea.MsgDeleteFail).ConfigureAwait(true);
                            Dc.DcExIdeaHelpers.Add(item);
                        }
                    }
                    else
                    {
                        CheckSaveOverride = true;

                        if (DcListDataPoint.State != EnumDcListElementState.New)
                        {
                            var helperRes = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                            if (!helperRes.DataOk)
                            {
                                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {helperRes.ServerExceptionText}");
                            }

                            item.Update();
                        }
                    }
                }

                UpdateUiLists();
            }, glyph: Glyphs.Bin_1);

            CmdChatHelper = new VmCommand(ResViewAddIdea.CmdChatHelper, async p =>
            {
                if (p is DcListDataPoint<ExIdeaHelper> item)
                {
                    Dc.Chat.SelectedUiChat = null!;

                    if (!item.Data.PrivateChatId.HasValue)
                    {
                        if (!await Dc.Chat.NewChat(item.Data.UserId, item.Index.ToString()).ConfigureAwait(true))
                        {
                            await MsgBox.Show($"Privater Chat für Idee {DcListDataPoint.Data.Title} mit Helper Id {item.Data.UserId} konnte nicht erstellt werden.").ConfigureAwait(true);
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
                        await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                }
            }, glyph: Glyphs.Conversation_chat_1);

            CmdShowHelperInfo = new VmCommand("i", p =>
            {
                if (p is DcListDataPoint<ExIdeaHelper> helper)
                {
                    MsgBox.Show(helper.Data.Info).ConfigureAwait(true);
                }
            }, glyph: Glyphs.Information_circle);

            #endregion

            #region Need

            CmdAddNeed = new VmCommand(ResViewAddIdea.CmdAddNeed, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                IsNavigatedToNavToViewWithResult = true;

                DcListDataPoint<ExIdeaNeed>? item = null;

                var newItem = new ExIdeaNeed
                              {
                                  IdeaId = DcListDataPoint.Index,
                                  AmountNeed = 1,
                                  AmountSupplied = 0,
                                  CanEdit = true,
                                  CanDelete = true,
                              };
                item = new DcListDataPoint<ExIdeaNeed>(newItem);
                Dc.DcExIdeaNeeds.Add(item);
                var r = await Nav.ToViewWithResult(typeof(VmAddNeed), item).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result == EnumVmEditResult.NotModified)
                    {
                        try
                        {
                            Dc.DcExIdeaNeeds.Remove(item);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                        }
                    }
                    else
                    {
                        CheckSaveOverride = true;

                        if (DcListDataPoint.State != EnumDcListElementState.New)
                        {
                            var needRes = await Dc.DcExIdeaNeeds.StoreAll().ConfigureAwait(true);
                            if (!needRes.DataOk)
                            {
                                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {needRes.ServerExceptionText}");
                            }

                            item.Update();
#pragma warning disable CS0618 // Type or member is obsolete
                            await Dc.DcExIdeaHelpers.WaitDataFromServerAsync(filter: new ExIdeaHelperFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                IsNavigatedToNavToViewWithResult = false;
                UpdateUiLists();
            }, glyph: Glyphs.Add_circle);

            CmdEditNeed = new VmCommand(ResViewAddIdea.CmdEditNeed, async need =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                IsNavigatedToNavToViewWithResult = true;

                if (need is DcListDataPoint<ExIdeaNeed> dcItem)
                {
                    var r = await Nav.ToViewWithResult(typeof(VmAddNeed), dcItem).ConfigureAwait(true);
                    if (r is EnumVmEditResult result)
                    {
                        if (result == EnumVmEditResult.NotModified)
                        {
                            var p = dcItem;
                            if (p.PossibleNewDataOnServer)
                            {
                                p.Update();
                            }
                        }
                        else
                        {
                            CheckSaveOverride = true;

                            if (DcListDataPoint.State != EnumDcListElementState.New)
                            {
                                var needRes = await Dc.DcExIdeaNeeds.StoreAll().ConfigureAwait(true);
                                if (!needRes.DataOk)
                                {
                                    Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {needRes.ServerExceptionText}");
                                }

                                dcItem.Update();
#pragma warning disable CS0618 // Type or member is obsolete
                                await Dc.DcExIdeaHelpers.WaitDataFromServerAsync(filter: new ExIdeaHelperFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Wrong result!");
                    }
                }

                IsNavigatedToNavToViewWithResult = false;
                UpdateUiLists();
            }, glyph: Glyphs.Pencil_1);

            CmdDeleteNeed = new VmCommand(ResViewAddIdea.CmdDeleteNeed, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                var res = await MsgBox.Show(ResViewAddIdea.MsgConfirmNeedDelete, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (p is DcListDataPoint<ExIdeaNeed> need)
                {
                    try
                    {
                        Dc.DcExIdeaNeeds.Remove(need);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }

                    if (DcListDataPoint.State != EnumDcListElementState.New)
                    {
                        var r = await Dc.DcExIdeaNeeds.StoreAll().ConfigureAwait(true);
                        if (!r.DataOk)
                        {
                            await MsgBox.Show(ResViewAddIdea.MsgDeleteFail).ConfigureAwait(true);
                            Dc.DcExIdeaNeeds.Add(need);
                        }
                    }
                    else
                    {
                        CheckSaveOverride = true;

                        if (DcListDataPoint.State != EnumDcListElementState.New)
                        {
                            var needRes = await Dc.DcExIdeaNeeds.StoreAll().ConfigureAwait(true);
                            if (!needRes.DataOk)
                            {
                                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {needRes.ServerExceptionText}");
                            }

                            need.Update();
#pragma warning disable CS0618 // Type or member is obsolete
                            await Dc.DcExIdeaHelpers.WaitDataFromServerAsync(filter: new ExIdeaHelperFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete
                        }
                    }
                }

                UpdateUiLists();
            }, glyph: Glyphs.Bin_1);

            CmdShowNeedInfo = new VmCommand("i", p =>
            {
                if (p is DcListDataPoint<ExIdeaNeed> need)
                {
                    MsgBox.Show(need.Data.Infotext).ConfigureAwait(true);
                }
            }, glyph: Glyphs.Information_circle);

            #endregion

            #region Image

            CmdDeleteImage = new VmCommand(string.Empty, () =>
            {
                DcListDataPoint.Data.IdeaImage = string.Empty;
                DcListDataPoint.Data.IdeaImageId = null;

                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            }, glyph: Glyphs.Bin_1);

            CmdModifyImage = new VmCommand(string.Empty, async () =>
            {
                DcDoNotAutoDisconnect = true;
                IsNavigatedToNavToViewWithResult = true;

                var res = await Nav.ToViewWithResult(typeof(VmImageEditor), new ImageEditRequest()
                                                                            {
                                                                                File = new ExFile()
                                                                                       {
                                                                                           DownloadLink = DcListDataPoint.Data.IdeaImage ?? string.Empty,
                                                                                           Bytes = null!,
                                                                                       },
                                                                                IsIdeaPicture = true,
                                                                                IsProfilePicture = false,
                                                                            }).ConfigureAwait(true);

                if (res is ExFile file)
                {
                    DcListDataPoint.Data.IdeaImage = file.DownloadLink;
                    DcListDataPoint.Data.IdeaImageId = -1;
                }

                IsNavigatedToNavToViewWithResult = false;
                DcDoNotAutoDisconnect = false;

                if (!IsLoaded)
                    IsLoaded = true;

                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            }, glyph: Glyphs.Pencil_1);

            #endregion
        }

        /// <summary>
        ///     Bildidee löschen
        /// </summary>
        public VmCommand CmdDeleteImage { get; set; } = null!;

        /// <summary>
        ///     BildIdee neu / ändern
        /// </summary>
        public VmCommand CmdModifyImage { get; set; } = null!;

        /// <summary>
        ///     Privat mit jemanden Chatten
        /// </summary>
        public VmCommand CmdChatUser { get; set; } = null!;

        /// <summary>
        ///     über Beitrag mit Usern Chatten
        /// </summary>
        public VmCommand CmdChatIdea { get; set; } = null!;

        /// <summary>
        ///     Position auf Karte zeigen
        /// </summary>
        public VmCommand CmdShowMap { get; set; } = null!;

        /// <summary>
        ///     Idee löschen
        /// </summary>
        public VmCommand CmdDeleteIdea { get; set; } = null!;

        /// <summary>
        ///     Idee melden
        /// </summary>
        public VmCommand CmdReportIdea { get; set; } = null!;

        /// <summary>
        ///     Helfer hinzufügen
        /// </summary>
        public VmCommand CmdAddHelper { get; set; } = null!;

        /// <summary>
        ///     Helfer editieren
        /// </summary>
        public VmCommand CmdEditHelper { get; set; } = null!;

        /// <summary>
        ///     Helfer entfernen
        /// </summary>
        public VmCommand CmdDeleteHelper { get; set; } = null!;

        /// <summary>
        ///     Chat mit helfer öffnen
        /// </summary>
        public VmCommand CmdChatHelper { get; set; } = null!;

        /// <summary>
        ///     Infotext von helfer öffnen
        /// </summary>
        public VmCommand CmdShowHelperInfo { get; set; } = null!;

        /// <summary>
        ///     benötigte Sache hinzufügen
        /// </summary>
        public VmCommand CmdAddNeed { get; set; } = null!;

        /// <summary>
        ///     benötigte Sache editieren
        /// </summary>
        public VmCommand CmdEditNeed { get; set; } = null!;

        /// <summary>
        ///     Infotext für Sache anzeigen
        /// </summary>
        public VmCommand CmdShowNeedInfo { get; set; } = null!;

        /// <summary>
        ///     benötigte Sache entfernen
        /// </summary>
        public VmCommand CmdDeleteNeed { get; set; } = null!;

        /// <summary>
        ///     benötigte Sache entfernen
        /// </summary>
        public VmCommand CmdAddRegion { get; set; } = null!;

        /// <summary>
        ///     Entfernen einer Gemeinde
        /// </summary>
        public VmCommand CmdRemoveRegion { get; set; } = null!;

        /// <summary>
        ///     Position bearbeiten
        /// </summary>
        public VmCommand CmdEditPosition { get; set; } = null!;

        /// <summary>
        ///     Position bearbeiten beenden
        /// </summary>
        public VmCommand CmdEndEditPosition { get; set; } = null!;

        /// <summary>
        ///     Switch für Zeitraum togglen
        /// </summary>
        public VmCommand CmdToggleTimeSpan { get; set; } = null!;

        /// <summary>
        ///     Switch für Position togglen
        /// </summary>
        public VmCommand CmdTogglePosition { get; set; } = null!;

        /// <summary>
        ///     Adresse via Position laden
        /// </summary>
        public VmCommand CmdGetAddressForPosition { get; set; } = null!;

        /// <summary>
        ///     Position via Addresse laden
        /// </summary>
        public VmCommand CmdGetPositionFromAddress { get; set; } = null!;

        #endregion
    }
}