// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace ConnectivityHost.Shared
{
    /// <summary>
    ///     Navigationscontrol.
    /// </summary>
    public partial class NavControl
    {
        /// <summary>
        ///     Initializierungsmethode.
        /// </summary>
        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += async (_, _) => { await InvokeAsync(StateHasChanged).ConfigureAwait(true); };

            base.OnInitialized();
        }
    }
}