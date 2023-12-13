// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Components;
using Biss.Apps.Interfaces;
using Biss.Core.Logging.Events;
using Biss.Dc.Core;
using Exchange;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Infos für Entwickler direkt in der laufenden App</para>
    ///     Klasse VmDeveloperInfos. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewDeveloperInfos")]
    public class VmDeveloperInfos : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstance}"
        /// </summary>
        public static VmDeveloperInfos DesignInstance = new VmDeveloperInfos();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceComponentDevInfo}"
        /// </summary>
        public static ComponentDevInfo DesignInstanceComponentDevInfo = new ComponentDevInfo();

        /// <summary>
        ///     VmDeveloperInfos
        /// </summary>
        public VmDeveloperInfos() : base("DEV Infos", subTitle: "Aktuelle Informationen für Entwickler")
        {
            SetViewProperties();
            DbSettingsLoaded = Dc.DcExSettingsInDb.DataSource == EnumDcDataSource.FromServer;
        }

        #region Properties

        /// <summary>
        ///     Letzten (max. 100) Logeinträge
        /// </summary>
        public ObservableCollection<BissEventsLoggerEventArgs> Log => LogEntries;

        /// <summary>
        ///     Dev Infos
        /// </summary>
        public ObservableCollection<ComponentDevInfo> ComponentsDevInfos { get; private set; } = new ObservableCollection<ComponentDevInfo>();

        /// <summary>
        ///     App Settings
        /// </summary>
        public AppSettings CurrentSettings => AppSettings.Current();

        /// <summary>
        ///     Settings vom Server geladen
        /// </summary>
        public bool DbSettingsLoaded { get; set; }

        #endregion

        /// <summary>
        ///     View wurde erzeugt und geladen - Aber noch nicht sichtbar (gerendert)
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            ComponentsDevInfos = new ObservableCollection<ComponentDevInfo>(await CManager!.GetDeveloperInfos().ConfigureAwait(true));
        }

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            View.ViewSizeChanged += (sender, args) => { PageSubTitle = $"Breite: {args.Width:##.00} Höhe: {args.Height:##.00}"; };
            return base.OnAppearing(view);
        }
    }
}