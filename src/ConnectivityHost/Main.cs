// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Biss.Apps.Blazor;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp;
using Exchange;
using Exchange.Resources;

namespace ConnectivityHost
{
    /// <summary>
    ///     <para>Main</para>
    ///     Klasse Main.cs (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class Main
    {
        /// <summary>
        ///     Init
        /// </summary>
        public static void Init()
        {
            Logging.Log.LogInfo("Blazor: Init started");

            AppSettings.Current().DefaultViewNamespace = "ConnectivityHost.Pages.";
            AppSettings.Current().DefaultViewAssembly = "ConnectivityHost";

            var init = new BissInitializer();
            init.Initialize(AppSettings.Current(), new Language());
            VmProjectBase.InitializeApp().ConfigureAwait(true);

            Logging.Log.LogInfo("Blazor: Init finished");
        }
    }
}