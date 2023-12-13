// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using Biss.Apps.Attributes;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Log.Producer;
using Exchange.Model;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Willkommens screen</para>
    ///     Klasse VmFirstLaunch. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewFirstLaunch")]
    public class VmFirstLaunch : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmFirstLaunch.DesignInstance}"
        /// </summary>
        public static VmFirstLaunch DesignInstance = new VmFirstLaunch();

        /// <summary>
        ///     VmFirstLaunch
        /// </summary>
        public VmFirstLaunch() : base(ResViewFirstLaunch.LblTitle, subTitle: ResViewFirstLaunch.LblSubTitle)
        {
            View.ShowMenu = false;
            View.ShowBack = false;
            View.ShowUser = false;
            View.ShowFooter = false;
            View.ShowHeader = true;
            View.ShowTitle = true;
            View.ShowSubTitle = true;

            WaitForProjectDataLoadedAfterDcConnected = true;
        }

        #region Properties

        /// <summary>
        ///     aktuelles Item für anzeige
        /// </summary>
        public DcIntroItem? SelectedItem { get; set; }

        /// <summary>
        ///     Hat Caroussel im UI
        /// </summary>
        public bool HasCustomView => DeviceInfo.Plattform == EnumPlattform.XamarinAndroid || DeviceInfo.Plattform == EnumPlattform.XamarinIos;

        #endregion

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            MenuGestureEnabled = false;
            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            View.BusySet(ResCommon.LblLoading);

            if (!Dc.AutoConnect)
            {
                DcStartAutoConnect();
            }

            await WaitForConnected().ConfigureAwait(true);
            var res = -1L;
            try
            {
                res = await Dc.DcExIntros.Sync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmFirstLaunch)}]({nameof(OnActivated)}): {e}");
            }

            var tries = 0;

            while (res < 0 && tries < 3)
            {
                Logging.Log.LogInfo($"[{nameof(VmFirstLaunch)}]({nameof(OnActivated)}): Retry Load Data");
                try
                {
                    res = await Dc.DcExIntros.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmFirstLaunch)}]({nameof(OnActivated)}): {e}");
                }

                tries++;
                await Task.Delay(100).ConfigureAwait(true);
            }

            SelectedItem = Dc.DcExIntros.FirstOrDefault();
            await base.OnActivated(args).ConfigureAwait(true);

            View.BusyClear();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            MenuGestureEnabled = true;
            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdNavLeft = new VmCommand(string.Empty, () =>
            {
                if (!Dc.DcExIntros.Any() || SelectedItem == null)
                {
                    return;
                }

                var currentIndex = Dc.DcExIntros.IndexOf(SelectedItem);
                var nextIndex = currentIndex - 1;

                if (nextIndex < 0)
                {
                    nextIndex = Dc.DcExIntros.Count - 1;
                }

                SelectedItem = Dc.DcExIntros[nextIndex];
            }, glyph: Glyphs.Arrow_circle_left);

            CmdNavRight = new VmCommand(string.Empty, () =>
            {
                if (!Dc.DcExIntros.Any() || SelectedItem == null)
                {
                    return;
                }

                var currentIndex = Dc.DcExIntros.IndexOf(SelectedItem);
                var nextIndex = currentIndex + 1;

                if (nextIndex >= Dc.DcExIntros.Count)
                {
                    nextIndex = 0;
                }

                SelectedItem = Dc.DcExIntros[nextIndex];
            }, glyph: Glyphs.Arrow_circle_right);

            CmdOpenLink = new VmCommand("öffnen", p =>
            {
                if (p is ExIntroItem item)
                {
                    Open.Browser(item.HtmlSource);
                }
            }, glyph: Glyphs.Share);

            CmdLaunch = new VmCommand(ResViewFirstLaunch.CmdLaunch, () =>
            {
                GCmdHome.IsSelected = true;
                GCmdHome.Execute(null!);
            });
        }

        /// <summary>
        ///     vorheriger Tipp
        /// </summary>
        public VmCommand CmdNavLeft { get; set; } = null!;

        /// <summary>
        ///     nächster Tipp
        /// </summary>
        public VmCommand CmdNavRight { get; set; } = null!;

        /// <summary>
        ///     Link öffnen
        /// </summary>
        public VmCommand CmdOpenLink { get; set; } = null!;

        /// <summary>
        ///     Starten
        /// </summary>
        public VmCommand CmdLaunch { get; set; } = null!;

        #endregion
    }
}