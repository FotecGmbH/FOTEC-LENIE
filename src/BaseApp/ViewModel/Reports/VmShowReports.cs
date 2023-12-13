// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using Biss.Apps.Attributes;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Log.Producer;
using Exchange.Model.Idea;
using Exchange.Model.Report;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Reports
{
    /// <summary>
    ///     <para>VmShowReports</para>
    ///     Klasse VmShowReports. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewShowReports")]
    public class VmShowReports : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmShowReports.DesignInstance}"
        /// </summary>
        public static VmShowReports DesignInstance = new VmShowReports();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmShowReports.DesignInstance}"
        /// </summary>
        public static DcListDataPoint<ExReport> DesignInstanceReport = new DcListDataPoint<ExReport>(new ExReport());

        private DcListDataPoint<ExIdea>? _exIdea;
        private int _loading;

        /// <summary>
        ///     VmShowReports
        /// </summary>
        public VmShowReports() : base(ResViewShowReports.LblTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Observablecollection mit allen Reports für de Idea.
        /// </summary>
        public List<DcListDataPoint<ExReport>> UiReports { get; set; } = new List<DcListDataPoint<ExReport>>();

        #endregion

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.Execute(null!);
                return;
            }

            await CheckPhoneConfirmation().ConfigureAwait(true);

            await base.OnActivated(args).ConfigureAwait(true);

            if (args is DcListDataPoint<ExIdea> idea)
            {
                _exIdea = idea;
                View.BusySet(ResCommon.LblLoading, 200);

                // Laden der relevanten Reports
                await LoadData().ConfigureAwait(true);

                View.BusyClear();
            }
            else
            {
                await MsgBox.Show(ResCommon.MsgTitleError).ConfigureAwait(false);
                GCmdHome.Execute(null!);
            }
        }

        #endregion

        private async Task LoadData()
        {
            if (!await CheckConnected().ConfigureAwait(true) || _exIdea is null)
            {
                await MsgBox.Show(ResCommon.MsgTitleError).ConfigureAwait(false);
                GCmdHome.Execute(null!);
                return;
            }

            if (Interlocked.CompareExchange(ref _loading, 1, 0) == 0)
            {
                Dc.DcExReports.FilterList(x => x.Data.IdeaId == _exIdea.Index);

                try
                {
                    await Dc.DcExReports.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmShowReports)}]({nameof(LoadData)}): {e}");
                }

                UpdateUiData();
                _loading = 0;
            }
        }

        private void UpdateUiData()
        {
            Dispatcher?.RunOnDispatcher(() =>
            {
                var reports = new List<DcListDataPoint<ExReport>>();
                foreach (var r in Dc.DcExReports)
                {
                    reports.Add(r);
                }

                UiReports = reports;
            });
        }

        #region Commands

        /// <summary>
        ///     Command zum Löschen eines einzelnen Reports
        /// </summary>
        public VmCommand CmdDeleteReport { get; set; } = null!;

        /// <summary>
        ///     Command zum Löschen der Idea
        /// </summary>
        public VmCommand CmdDeleteIdea { get; set; } = null!;


        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdDeleteReport = new VmCommand(ResViewShowReports.CmdDeleteReport, async rep =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                var res = await MsgBox.Show(ResViewShowReports.MsgDeleteReportConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (rep is DcListDataPoint<ExReport> report)
                {
                    try
                    {
                        Dc.DcExReports.Remove(report);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmShowReports)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }

                    View.BusySet(ResCommon.LblLoading);
                    await Dc.DcExReports.StoreAll().ConfigureAwait(true);
                    await LoadData().ConfigureAwait(true);
                    View.BusyClear();
                }
            }, glyph: Glyphs.Bin);

            CmdDeleteIdea = new VmCommand(ResViewShowReports.CmdDeleteIdea, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                var res = await MsgBox.Show(ResViewShowReports.MsgDeleteIdeaConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (_exIdea != null)
                {
                    try
                    {
                        Dc.DcExIdeas.Remove((DcExIdeasType) _exIdea);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmShowReports)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }

                    View.BusySet(ResCommon.LblLoading);
                    await Dc.DcExIdeas.StoreAll().ConfigureAwait(true);
                    View.BusyClear();
                    await Nav.Back().ConfigureAwait(false);
                }
            });
        }

        #endregion
    }
}