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
using BaseApp.ViewModel.Idea;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Log.Producer;
using Exchange.Model.Report;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Reports
{
    /// <summary>
    ///     <para>Übersicht Reports</para>
    ///     Klasse VmReportsOverview. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewReportsOverview")]
    public class VmReportsOverview : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmReportsOverview.DesignInstance}"
        /// </summary>
        public static VmReportsOverview DesignInstance = new VmReportsOverview();

        private bool _navToSubPage;

        /// <summary>
        ///     VmReportsOverview
        /// </summary>
        public VmReportsOverview() : base(ResViewReportsOverview.LblTitle, subTitle: ResViewReportsOverview.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Liste anzeigen
        /// </summary>
        public bool ShowList { get; set; }

        /// <summary>
        ///     Reports UI
        /// </summary>
        public List<DcListDataPoint<ExReport>> UiReports { get; set; } = new List<DcListDataPoint<ExReport>>();

        #endregion

        private void DcExReportsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UiReports = Dc.DcExReports.OrderBy(x => x.Data.IdeaTitle).ToList();
            ShowList = Dc.DcExReports.Any();
        }

        private async Task DcExReportsOnCollectionEvent(object sender, CollectionEventArgs<DcListDataPoint<ExReport>> e)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            if (e.TypeOfEvent == EnumCollectionEventType.InfoRequest && e.Item != null!)
            {
                await Nav.ToViewWithResult(typeof(VmAddReport), e.Item).ConfigureAwait(true);
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest && e.Item != null!)
            {
                var res = await MsgBox.Show(ResViewShowReports.MsgDeleteReportConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    Dc.DcExReports.Remove(e.Item);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(VmReportsOverview)}]({nameof(DcExReportsOnCollectionEvent)}): Workaroud - neues NuGet");
                }

                View.BusySet(ResCommon.LblLoading);
                await Dc.DcExReports.StoreAll().ConfigureAwait(true);

                try
                {
                    await Dc.DcExReports.Sync().ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"[{nameof(VmReportsOverview)}]({nameof(DcExReportsOnCollectionEvent)}): {ex}");
                }

                View.BusyClear();
            }
        }

        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attach"></param>
        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                Dc.DcExReports.CollectionEvent += DcExReportsOnCollectionEvent;
                Dc.DcExReports.CollectionChanged += DcExReportsOnCollectionChanged;
            }
            else
            {
                Dc.DcExReports.CollectionEvent -= DcExReportsOnCollectionEvent;
                Dc.DcExReports.CollectionChanged -= DcExReportsOnCollectionChanged;
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
            await CheckPhoneConfirmation().ConfigureAwait(true);

            if (!Dc.DcExReports.SyncedSinceUserRegistered)
            {
                View.BusySet();
                ShowList = false;
                UiReports.Clear();

                try
                {
                    await Dc.DcExReports.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmReportsOverview)}]({nameof(OnAppearing)}): {e}");
                }

                UiReports = Dc.DcExReports.OrderBy(x => x.Data.IdeaTitle).ToList();
                View.BusyClear();
            }

            await base.OnAppearing(view).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            return base.OnActivated(args);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            Dc.DcExReports.FilterClear();

            if (!_navToSubPage)
            {
                AttachDetachVmEvents(true);
            }

            UiReports = Dc.DcExReports.OrderBy(x => x.Data.IdeaTitle).ToList();
            ShowList = Dc.DcExReports.Any();

            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            if (!_navToSubPage)
            {
                AttachDetachVmEvents(false);
            }

            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Command zum Anzeigen der Idee
        /// </summary>
        public VmCommand CmdShowIdea { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdShowIdea = new VmCommand(string.Empty, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                if (p is DcListDataPoint<ExReport> report)
                {
                    var idea = Dc.DcExIdeas.AllItems.FirstOrDefault(x => x.Id == report.Data.IdeaId);
                    if (idea == null)
                    {
                        try
                        {
                            await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmReportsOverview)}]({nameof(InitializeCommands)}): {e}");
                        }

                        idea = Dc.DcExIdeas.AllItems.FirstOrDefault(x => x.Id == report.Data.IdeaId);
                        if (idea == null)
                        {
                            return;
                        }
                    }

                    _navToSubPage = true;
                    await Nav.ToViewWithResult(typeof(VmIdeaDetails), idea).ConfigureAwait(true);
                    _navToSubPage = false;
                }
            }, glyph: Glyphs.Information_circle);
        }

        #endregion
    }
}