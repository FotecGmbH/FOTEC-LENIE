// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Threading.Tasks;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Settings
    /// </summary>
    public partial class ViewSettings
    {
        /// <summary>
        ///     Entspricht VM OnLoaded, wird einmal pro View aufgerufen
        /// </summary>
        /// <returns></returns>
        protected override Task OnViewLoaded()
        {
            if (ViewModel != null!)
            {
                ViewModel.DcConnection = DcConnections;
                ViewModel.ServerRemoteCalls = ServerRemoteCalls;
                ViewModel.ServerRemoteCalls.SetClientConnection(ViewModel.DcConnection);
            }

            return base.OnViewLoaded();
        }
    }
}