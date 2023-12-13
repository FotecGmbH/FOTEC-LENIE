// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Message.
    /// </summary>
    public partial class ViewMessage
    {
        /// <summary>
        ///     Entspricht VM OnLoaded, wird einmal pro View aufgerufen
        /// </summary>
        /// <returns></returns>
        protected override Task OnViewLoaded()
        {
            if (ViewModel != null)
            {
                ViewModel.DcConnections = DcConnections;
                ViewModel.ServerRemoteCalls = ServerRemoteCalls;
                ViewModel.ServerRemoteCalls.SetClientConnection(ViewModel.DcConnections);
                ViewModel.RazorEngine = Razorengine;
            }

            return base.OnViewLoaded();
        }
    }
}