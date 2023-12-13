// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.ViewModel.User;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange.Model.User;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Sys Admin ViewModel</para>
    ///     Klasse VmUsersAll. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewSysAdmin", true)]
    public class VmSysAdmin : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUsersAll.DesignInstance}"
        /// </summary>
        public static VmSysAdmin DesignInstance = new VmSysAdmin();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUsersAll.DesignInstance}"
        /// </summary>
        public static DcListDataPoint<ExUser> DesignInstanceExUser = new DcListDataPoint<ExUser>(new ExUser());


        /// <summary>
        ///     VmUsersAll
        /// </summary>
        public VmSysAdmin() : base(ResSysAdmin.Titel, subTitle: ResSysAdmin.SubTitel)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     EntryCommonMsg
        /// </summary>
        public VmEntry EntryCommonMsg { get; set; } = null!;


        /// <summary>
        ///     User für UI
        /// </summary>
        public BxObservableCollection<DcListDataPoint<ExUser>> UiUsers { get; set; } = null!;

        /// <summary>
        ///     Aktueller Filtereintrag im Ui
        /// </summary>
        public string Filter { get; set; } = string.Empty;

        /// <summary>
        ///     Anzahl der User
        /// </summary>
        public string UiUsersCount { get; set; } = string.Empty;

        /// <summary>
        ///     Test Command
        /// </summary>
        public VmCommand CmdUpdateInfoText { get; set; } = null!;

        /// <summary>
        ///     Benutzer bearbeiten
        /// </summary>
        public VmCommand CmdEditUser { get; set; } = null!;

        /// <summary>
        ///     Suchen Button
        /// </summary>
        public VmCommand CmdSearch { get; set; } = null!;

        /// <summary>
        ///     Allgemeine Nachricht
        /// </summary>
        public string CommonMsg { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.Execute(null!);
                return;
            }

            View.BusySet(ResSysAdmin.BusyLoading);
            await base.OnAppearing(view).ConfigureAwait(true);

            Dc.DcExAllUsers.FilterClear();

            Dc.DcExAllUsers.LoadingFromHostEvent += DcExAllUsersOnLoadingFromHostEvent;

            try
            {
                await Dc.DcExAllUsers.Sync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmSysAdmin)}]({nameof(OnAppearing)}): {e}");
            }

            CommonMsg = Dc.DcExSettingsInDb.Data.CommonMessage;

            EntryCommonMsg = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResSysAdmin.MsgCommonInfo,
                ResSysAdmin.MsgCommonInfoPlaceholder,
                this,
                nameof(CommonMsg),
                maxChar: 50
            );

            CheckLoadedData();
            View.BusyClear();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            if (UiUsers != null!)
            {
                Dc.DcExAllUsers.LoadingFromHostEvent -= DcExAllUsersOnLoadingFromHostEvent;
                UiUsers.FilterClear();
                foreach (var user in UiUsers)
                {
                    user.Data.PropertyChanged -= DataOnPropertyChanged;
                }
            }

            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdSearch = new VmCommand("", FilterList, glyph: Glyphs.Search);

            CmdUpdateInfoText = new VmCommand(ResSysAdmin.CmdCommonInfo, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                var r = await Dc.UpdateCommonMessage(CommonMsg).ConfigureAwait(true);
                if (!r.Ok)
                {
                    var msg = ResSysAdmin.ErrorCommon;
                    if (ShowDeveloperInfos)
                    {
                        msg = $"{msg}: {r.ServerExceptionText}";
                    }

                    await MsgBox.Show(msg).ConfigureAwait(true);
                }
            });

            CmdEditUser = new VmCommand("", async e =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (e is DcListDataPoint<ExUser> u)
                {
                    var r = await Nav.ToViewWithResult(typeof(VmEditUser), u).ConfigureAwait(true);
                    if (r is bool b)
                    {
                        if (!b)
                        {
                            await MsgBox.Show(ResSysAdmin.ErrorCommon).ConfigureAwait(true);
                            try
                            {
                                await Dc.DcExAllUsers.Sync().ConfigureAwait(true);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log.LogError($"[{nameof(VmSysAdmin)}]({nameof(InitializeCommands)}): {ex}");
                            }
                        }
                    }
                }
            }, glyph: Glyphs.Notes_edit);
        }

        /// <summary>
        ///     Neue Daten vom Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExAllUsersOnLoadingFromHostEvent(object sender, bool e)
        {
            CheckLoadedData();
        }

        /// <summary>
        ///     Neu geladene Daten aufbereiten
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void CheckLoadedData()
        {
            var tmp = new BxObservableCollection<DcListDataPoint<ExUser>>(Dc.DcExAllUsers);
            var me = tmp.FirstOrDefault(f => f.Index == Dc.CoreConnectionInfos.UserId);
            if (me == null)
            {
                return;
            }

            try
            {
                tmp.Remove(me);
            }
            catch (Exception)
            {
                Logging.Log.LogWarning($"[{nameof(VmSysAdmin)}]({nameof(CheckLoadedData)}): Workaroud - neues NuGet");
            }

            tmp.Sort((point, dataPoint) => dataPoint.Data.CompareTo(point.Data));
            UiUsers = tmp;
            FilterList();

            foreach (var user in UiUsers)
            {
                user.Data.PropertyChanged += DataOnPropertyChanged;
            }
        }

        /// <summary>
        ///     Switches Änderungen speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExUser.IsSysAdmin) || e.PropertyName == nameof(ExUser.Locked))
            {
                var dcPoint = Dc.DcExAllUsers.FirstOrDefault(f => f.Data == sender);
                if (dcPoint == null)
                {
                    return;
                    //throw new ArgumentNullException($"[{nameof(VmSysAdmin)}]({nameof(DataOnPropertyChanged)}): {nameof(dcPoint)}");
                }

                dcPoint.Data.PropertyChanged -= DataOnPropertyChanged;
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    if (e.PropertyName == nameof(ExUser.IsSysAdmin))
                    {
                        dcPoint.Data.IsSysAdmin = !dcPoint.Data.IsAdmin;
                    }
                    else
                    {
                        dcPoint.Data.Locked = !dcPoint.Data.Locked;
                    }

                    dcPoint.Data.PropertyChanged += DataOnPropertyChanged;
                    return;
                }

                var r = await dcPoint.StoreData(true).ConfigureAwait(true);
                if (!r.DataOk)
                {
                    var msg = ResSysAdmin.ErrorCommon;
                    if (ShowDeveloperInfos)
                    {
                        msg = $"{msg}: {r.ServerExceptionText}";
                    }

                    await MsgBox.Show(msg).ConfigureAwait(true);
                }

                dcPoint.Data.PropertyChanged += DataOnPropertyChanged;
            }
        }

        /// <summary>
        ///     Filterung für Suchfeld
        /// </summary>
        private void FilterList()
        {
            if (UiUsers == null! || UiUsers.Count == 0)
            {
                UiUsersCount = "0/0";
                return;
            }

            if (string.IsNullOrEmpty(Filter))
            {
                UiUsers.FilterClear();
            }
            else
            {
                UiUsers.FilterList(point =>
                    point.Data.Fullname.Contains(Filter, StringComparison.InvariantCultureIgnoreCase) ||
                    point.Data.LoginName.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)
                );
            }

            UiUsersCount = $"{UiUsers.Count}/{Dc.DcExAllUsers.Count}";

            if (DeviceInfo.Plattform == EnumPlattform.Web)
            {
                this.InvokeOnPropertyChanged(nameof(UiUsers));
            }
        }
    }
}