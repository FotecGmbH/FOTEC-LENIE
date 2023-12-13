// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Map.Component;
using Biss.Apps.Map.Helper;
using Biss.Apps.Map.Model;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Exchange;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>View Infos</para>
    ///     Klasse VmInfo. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewInfo", true)]
    public class VmInfo : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmInfo.DesignInstance}"
        /// </summary>
        public static VmInfo DesignInstance = new VmInfo();

        /// <summary>
        ///     VmInfo
        /// </summary>
        public VmInfo() : base(ResViewInfo.LblTitle, subTitle: ResViewInfo.LblSubTitle)
        {
            SetViewProperties();
        }

        #region Properties

        /// <summary>
        ///     Appname
        /// </summary>
        public string AppName => AppSettings.Current().AppName;

        /// <summary>
        ///     Appversion
        /// </summary>
        public string AppVersion => AppSettings.Current().AppVersion;

        /// <summary>
        ///     Zur Fotec springen
        /// </summary>
        public VmCommand CmdGoToFotec { get; private set; } = null!;

        /// <summary>
        ///     Zur aktuellen Userposition springen
        /// </summary>
        public VmCommand CmdGoToMyPosition { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var fotec = new BmPoint(ResViewInfo.LblFotec)
                        {
                            Position = new BissPosition(47.8374117, 16.2523985),
                            Color = Color.FromArgb(97, 164, 215),
                        };
            this.BcBissMap()!.BissMap.MapItems.Clear();
            this.BcBissMap()!.BissMap.MapItems.Add(fotec);

            CmdGoToFotec.Execute(null!);

            return base.OnActivated(args);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            CmdGoToFotec = new VmCommand(ResViewInfo.CmdFotec, () =>
            {
                var fotec = this.BcBissMap()!.BissMap.MapItems.FirstOrDefault();

                if (fotec != null)
                {
                    this.BcBissMap()!.BissMap.SetCenterAndZoom(fotec.Position, BmDistance.FromKilometers(2));
                }
            });

            CmdGoToMyPosition = new VmCommand(ResViewInfo.CmdMyPosition, async p =>
            {
                try
                {
                    var loc = await this.BcBissMap()!.GetUserLocation().ConfigureAwait(true);

                    if (loc != null)
                    {
                        this.BcBissMap()?.BissMap.SetCenterAndZoom(loc, BmDistance.FromMeters(500));
                    }
                    else if (p == null || (p is bool supressError && !supressError))
                    {
                        await MsgBox.Show(ResViewInfo.MsgPositionUnavailable).ConfigureAwait(true);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log.LogError($"[{nameof(VmInfo)}]({nameof(InitializeCommands)}): MyPosition Error: {ex}");
                }
            });
        }
    }
}