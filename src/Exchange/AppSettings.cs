// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss;
using Biss.Apps.Components;
using Biss.Apps.Connectivity.Interfaces;
using Biss.Apps.Connectivity.Sa;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Map.Interface;
using Biss.Apps.Model;
using Biss.Apps.Push.Interfaces;
using Biss.Attributes;
using Biss.Dc.Client;
using Exchange.Interfaces;

namespace Exchange
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class AppSettings :
        IAppSettings,
        IAppSettingsNavigation,
        IAppSettingsFiles,
        IAppSettingConnectivity,
        IAppSettingsPush,
        IAppSettingsLinks,
        IAppSettingsMap,
        IAppSettingsAppCenter
    {
        private static AppSettings _current = null!;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        public AppSettings()
        {
            DefaultViewNamespace = BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DefaultViewNamespace");
            DefaultViewAssembly = BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DefaultViewAssembly");
            DcUseUser = BissSettingCsHelper.GetValue<bool>(this, "DIHOST.Lenie:Exchange:DcUseUser");
        }

        #region Properties

        #region IAppSettingsFiles

        public VmFiles BaseFiles { get; set; }

        #endregion IAppSettingsFiles

        #endregion

        /// <summary>
        ///     Get default Settings for AppSettings
        /// </summary>
        /// <returns></returns>
        public static AppSettings Current()
        {
            if (_current == null!)
            {
                _current = new AppSettings();
            }

            return _current;
        }

        #region IAppSettings

        public string License => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:License");

        /// <summary>
        ///     Branch für diese Konfiguration
        /// </summary>
        public string BranchName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:BranchName");

        /// <summary>
        ///     Mode für Constants 0 - DEFAULT RELEASE 1 - DEFAULT CUSTOMER BETA >1 - DEVELOPER
        /// </summary>
        public int AppConfigurationConstants => BissSettingCsHelper.GetValue<int>(this, "DIHOST.Lenie:Exchange:AppConfigurationConstants");

        /// <summary>
        ///     Produktversion
        /// </summary>
        [BissRequiredProperty]
        public string AppVersion => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:AppVersion");

        /// <summary>
        ///     App Name
        /// </summary>
        public string AppName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:AppName");

        /// <summary>
        ///     App Ordner auf Plattform
        /// </summary>
        public string ProjectWorkUserFolder => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:ProjectWorkUserFolder");

        /// <summary>
        ///     App Identifier
        /// </summary>
        public string PackageName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:PackageName");

        public ExLanguageContent LanguageContent { get; set; }

        #endregion IAppSettings

        #region IAppSettingsNavigation

        /// <summary>
        ///     App Orientation
        /// </summary>
        public EnumAppOrientation AppOrientationOverride => BissSettingCsHelper.GetValue<EnumAppOrientation>(this, "DIHOST.Lenie:Exchange:AppOrientationOverride");

        /// <summary>
        ///     In welchen Namespace befinden sich die Xamarin.Forms Views
        /// </summary>
        public string DefaultViewNamespace { get; set; }

        /// <summary>
        ///     In welchen Assembly befinden sich die Xamarin.Forms Views
        /// </summary>
        public string DefaultViewAssembly { get; set; }

        public IQuitApplication QuitApplication { get; set; }
        public object Master { get; set; }
        public object MasterDetail { get; set; }
        public object Navigation { get; set; }
        public object Shell { get; set; }
        public object NavigationManager { get; set; }
        public INavArgsHelper NavArgsHelper { get; set; }
        public VmNavigator BaseNavigator { get; set; }

        #endregion IAppSettingsNavigation

        #region IAppSettingConnectivity

        /// <summary>
        ///     DC Host - SignalR
        /// </summary>
        public string DcSignalHost => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DcSignalHost");

        /// <summary>
        ///     SA Host - REST
        /// </summary>
        public string SaApiHost => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:SaApiHost");

        public DcDataRoot DcClient { get; set; }
        public IDcClientInfoStorage DcAppStorage { get; set; }
        public IDcClientCacheStorage DcAppCache { get; set; }
        public bool DcEnabled { get; set; }

        /// <summary>
        ///     App mit User
        /// </summary>
        public bool DcUseUser { get; set; }

        public bool SaEnabled { get; set; }
        public RestAccessBase SaClient { get; set; }

        #endregion IAppSettingConnectivity

        #region IAppSettingsPush

        /// <summary>
        ///     Id des Notification-Channels
        /// </summary>
        public string NotificationChannelId => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:NotificationChannelId");

        /// <summary>
        ///     Name des Notification-Channels
        /// </summary>
        public string NotificationChannelName => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:NotificationChannelName");

        /// <summary>
        ///     Standard Topic
        /// </summary>
        public string DefaultTopic => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DefaultTopic");

        public IPlatformPush Platform { get; set; }

        #endregion IAppSettingsPush

        #region IAppSettingsLinks

        /// <summary>
        ///     App im Playstore
        /// </summary>
        public string DroidLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DroidLink");

        /// <summary>
        ///     App im Appstore
        /// </summary>
        public string IosLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:IosLink");

        /// <summary>
        ///     App im Windows Store
        /// </summary>
        public string WindowsLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:WindowsLink");

        /// <summary>
        ///     Deployte BlazorApp
        /// </summary>
        public string BlazorLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:BlazorLink");

        /// <summary>
        ///     Link zum Appcenter iOS
        /// </summary>
        public string IosTelemetryLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:IosTelemetryLink");

        /// <summary>
        ///     Link zum Appcenter Android
        /// </summary>
        public string DroidTelemetryLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DroidTelemetryLink");

        /// <summary>
        ///     Link zu Application Insights
        /// </summary>
        public string BlazorTelemetryLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:BlazorTelemetryLink");

        /// <summary>
        ///     Link zu Portal.azure
        /// </summary>
        public string AzureResourceLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:AzureResourceLink");

        /// <summary>
        ///     Link zu Fotec DevOps
        /// </summary>
        public string DevOpsLink => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DevOpsLink");

        #endregion IAppSettingsLinks

        #region IAppSettingsMap

        /// <summary>
        ///     Bing Maps Key - WPF (VWT)
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string BingMapsKey => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:BingMapsKey");

        /// <summary>
        ///     Google Maps Key Android und Blazor
        /// </summary>
        [BissRequiredProperty]
        [BissSecureProperty]
        public string GoogleMapsKey => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:GoogleMapsKey");

        #endregion IAppSettingsMap

        #region IAppSettingsAppCenter

        /// <summary>
        ///     Appcenter Secret Android
        /// </summary>
        [BissSecureProperty]
        public string DroidAppcenter => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:DroidAppcenter");

        /// <summary>
        ///     Appcenter Secret iOS
        /// </summary>
        [BissSecureProperty]
        public string IosAppcenter => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:IosAppcenter");

        /// <summary>
        ///     Appcenter Secret Wpf
        /// </summary>
        [BissSecureProperty]
        public string WpfAppcenter => BissSettingCsHelper.GetValue<string>(this, "DIHOST.Lenie:Exchange:WpfAppcenter");

        #endregion IAppSettingsAppCenter
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
}