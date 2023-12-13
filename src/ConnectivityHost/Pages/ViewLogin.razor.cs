// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Login.
    /// </summary>
    public partial class ViewLogin
    {
        /// <summary>
        ///     Passwort
        /// </summary>
        private string _password = null !;

        /// <summary>
        ///     Benutzername
        /// </summary>
        private string _username = null !;

        /// <summary>
        ///     Method invoked when the component is ready to start, having received its
        ///     initial parameters from its parent in the render tree.
        ///     Override this method if you will perform an asynchronous operation and
        ///     want the component to refresh when that operation is completed.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing any asynchronous operation.</returns>
        protected override async Task OnInitializedAsync()
        {
            await AuthService.Logout().ConfigureAwait(true);
            await base.OnInitializedAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///     Login
        /// </summary>
        /// <returns></returns>
        private async Task Login()
        {
            try
            {
                await AuthService.Login(_username, _password).ConfigureAwait(true);
                NavigationManager.NavigateTo("/admin");
            }
            catch (Exception)
            {
                await JsRuntime.InvokeVoidAsync("alert", "Login fehlgeschlagen!").ConfigureAwait(true);
            }
        }
    }
}