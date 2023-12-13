// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Threading.Tasks;
using ConnectivityHost.Helper;
using Database;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Microsoft.AspNetCore.Mvc;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     <para>Zugriff</para>
    ///     Klasse AccessController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class AccessControlController : Controller
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="db">Database</param>
        public AccessControlController(Db db)
        {
            _db = db;
        }

        /// <summary>
        ///     Zugriffsberechtigungen abfragen
        /// </summary>
        /// <returns>Zugriffsberechtigungen</returns>
        [HttpGet("/api/accesscontrol/list")]
        [Authorize]
        public virtual async Task<IActionResult> AccessControlList()
        {
            if (HttpContext.Items["User"] is ExUser user)
            {
                return Ok(await user.GetAccessControlList(_db).ConfigureAwait(true));
            }

            if (HttpContext.Items["Org"] is ExOrganization org)
            {
                return Ok(org.GetAccessControlList(_db));
            }

            return Ok(false);
        }
    }
}