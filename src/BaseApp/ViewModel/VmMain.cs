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
using Biss.Apps.Attributes;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange;
using Exchange.Model.Organization;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Main wenn kein User angemeldet ist(Model) bzw. Info wenn angemeldet</para>
    ///     Klasse VmMain. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewMain")]
    public class VmMain : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEntry.DesignInstance}"
        /// </summary>
        public static DcListDataPoint<ExOrganization> DesignInstanceOrg = new DcListDataPoint<ExOrganization>(new ExOrganization());

        /// <summary>
        ///     ViewModel Template
        /// </summary>
        public VmMain() : base(ResViewMain.LblTitle, subTitle: ResViewMain.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEntry.DesignInstance}"
        /// </summary>
        public static VmMain DesignInstance => new VmMain();

        /// <summary>
        ///     Appname
        /// </summary>
        public string AppName => AppSettings.Current().AppName;

        /// <summary>
        ///     Appversion
        /// </summary>
        public string AppVersion => AppSettings.Current().AppVersion;

        /// <summary>
        ///     Daten werden aktualisiert
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        ///     Aktuelle Settings
        /// </summary>
        public AppSettings Settings => AppSettings.Current();

        /// <summary>
        ///     Gemeinden für Ui
        /// </summary>
        public List<DcListDataPoint<ExOrganization>> UiOrganizations { get; set; } = new List<DcListDataPoint<ExOrganization>>();

        #endregion

        private void DcExOrganizationOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UiOrganizations = Dc.DcExOrganization
                .Where(x => x.Data.IdeasCount > 0)
                .OrderByDescending(x => x.Data.IdeasCount)
                .Take(10)
                .ToList();
        }

        private async Task Reload()
        {
            IsRefreshing = true;

            if (Dc.ConnectionState == EnumDcConnectionState.Connected)
            {
                try
                {
                    await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmMain)}]({nameof(Reload)}): {e}");
                }
            }

            IsRefreshing = false;
        }

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            GCmdHeaderCommon = new VmCommand("", async () => { await Reload().ConfigureAwait(true); }, glyph: Glyphs.Cloud_refresh)
                               {
                                   IsVisible = false
                               };

            CmdIntro = new VmCommand(ResViewMain.CmdIntro, () => { Nav.ToView(typeof(VmFirstLaunch)); });
        }

        /// <summary>
        ///     Intro nochmal
        /// </summary>
        public VmCommand CmdIntro { get; set; } = null!;

        #endregion

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            Dc.DcExOrganization.FilterClear();

            View.ShowUser = Dc.CoreConnectionInfos.UserOk;

            Dc.DcExOrganization.CollectionChanged += DcExOrganizationOnCollectionChanged;
            DcExOrganizationOnCollectionChanged(null!, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            await WaitForConnected().ConfigureAwait(true);

            await Reload().ConfigureAwait(true);

            await base.OnActivated(args).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.DcExOrganization.CollectionChanged -= DcExOrganizationOnCollectionChanged;
            return base.OnDisappearing(view);
        }

        #endregion
    }
}