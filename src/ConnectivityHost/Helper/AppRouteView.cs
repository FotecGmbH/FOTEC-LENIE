// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using ConnectivityHost.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     Authorization Attribut
    /// </summary>
    public class AppRouteView : RouteView
    {
        #region Properties

        /// <summary>
        ///     Navigation Manager
        /// </summary>
        [Inject]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public NavigationManager NavigationManager { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        ///     Auth Service
        /// </summary>
        [Inject]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IAuthenticationService AuthService { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion

        /// <summary>
        ///     Render
        /// </summary>
        /// <param name="builder">builder</param>
        protected override void Render(RenderTreeBuilder builder)
        {
            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (authorize && AuthenticationService.User == null!)
            {
                NavigationManager.NavigateTo("login");
            }
            else
            {
                base.Render(builder);
            }
        }
    }
}