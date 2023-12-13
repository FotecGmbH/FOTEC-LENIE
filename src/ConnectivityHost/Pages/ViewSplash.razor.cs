// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using ConnectivityHost.BaseApp;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Splash.
    /// </summary>
    public partial class ViewSplash
    {
        /// <summary>
        ///     Entspricht VM OnLoaded, wird einmal pro View aufgerufen
        /// </summary>
        /// <returns></returns>
        protected override async Task OnViewLoaded()
        {
            await base.OnViewLoaded().ConfigureAwait(true);

            if (VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem == null!)
            {
                VmProjectBase.LaunchFirstView();
            }
            else if (ViewModel != null! && VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem != ViewModel.GCmdHome)
            {
                VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem = ViewModel.GCmdHome;
            }
            else
            {
                NavigationManager.NavigateTo("viewmain");
            }
        }
    }
}