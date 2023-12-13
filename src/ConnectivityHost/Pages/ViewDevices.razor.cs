// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Tables;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Devices.
    /// </summary>
    public partial class ViewDevices
    {
        /// <summary>
        ///     On After Render Methode
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (ViewModel != null!)
            {
                ViewModel.DcConnections = DcConnections;
                ViewModel.ServerRemoteCalls = ServerRemoteCalls;
                ViewModel.ServerRemoteCalls.SetClientConnection(ViewModel.DcConnections);
            }

            if (firstRender)
            {
                StateHasChanged();
            }

            return base.OnAfterRenderAsync(firstRender);
        }

        private string GetPushTags(TableDevice device)
        {
            using var db = new Db();
            var user = db.TblUsers.FirstOrDefault(t => t.Id == device.TblUserId);
            return user?.PushTags ?? string.Empty;
        }
    }
}