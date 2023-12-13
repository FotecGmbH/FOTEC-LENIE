// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     Klasse AuthorizeAttribute.cs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        #region Interface Implementations

        /// <summary>
        ///     Authorize Attribut
        /// </summary>
        /// <param name="context">Kontext</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentException(null, nameof(context));
            }

            var user = context.HttpContext.Items["User"] as ExUser;
            var org = context.HttpContext.Items["Org"] as ExOrganization;
            if (user == null && org == null)
            {
                // not logged in
                context.Result = new JsonResult(new {message = "Unauthorized"}) {StatusCode = StatusCodes.Status401Unauthorized};
            }
        }

        #endregion
    }
}