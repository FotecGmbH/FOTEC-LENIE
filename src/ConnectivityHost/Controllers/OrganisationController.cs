// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Server;
using ConnectivityHost.DataConnector;
using ConnectivityHost.Helper;
using Database;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     <para>Firmen</para>
    ///     Klasse CompanyController.cs (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class OrganisationController : Controller
    {
        /// <summary>
        ///     Datenbank Context
        /// </summary>
        private readonly Db _db;

        /// <summary>
        ///     ServerRemot Calls
        /// </summary>
        private readonly ServerRemoteCalls _hub;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="db">Database</param>
        /// <param name="connections">DcConnection</param>
        /// <param name="calls">Server Remote Calls</param>
        public OrganisationController(Db db, IDcConnections connections, IServerRemoteCalls calls)
        {
            _db = db;
            _hub = (ServerRemoteCalls) calls;
            _hub.SetClientConnection(connections);
        }

        /// <summary>
        ///     Firmeninformationen abfragen
        /// </summary>
        /// <param name="id">ID der Firma</param>
        /// <returns>Informationen über die Firma</returns>
        [HttpGet("/api/company/{id}")]
        [Authorize]
        public virtual async Task<IActionResult> CompanyGet(long id)
        {
            if (id <= 0)
            {
                return Ok(false);
            }

            var tblOrg = await _db.TblOrganizations.Select(a => new {a.Id, a.Name, Type = a.OrganizationType}).FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);

            if (tblOrg == null)
            {
                return Ok(null);
            }

            if (HttpContext.Items["User"] is ExUser user)
            {
                if (await user.HasPermissionInCompany(_db, id).ConfigureAwait(true))
                {
                    return Ok(tblOrg);
                }
            }
            else if (HttpContext.Items["Org"] is ExOrganization org)
            {
                if (org.IsInAccessControlList(_db, id))
                {
                    return Ok(tblOrg);
                }
            }

            return Unauthorized();
        }

        /// <summary>
        ///     Firmeninformationen (Name) ändern
        /// </summary>
        /// <param name="id">ID der Firma</param>
        /// <param name="companyName">Neuer Name der Firma</param>
        /// <returns>Informationen über die Firma</returns>
        [HttpPost("/api/company/update/{id}")]
        [Authorize]
        public virtual async Task<IActionResult> CompanyUpdate(long id, [FromBody] string companyName)
        {
            if (id <= 0)
            {
                return Ok(false);
            }

            var tblOrg = await _db.TblOrganizations.FirstOrDefaultAsync(a => a.Id == id).ConfigureAwait(true);

            if (tblOrg == null)
            {
                return Ok(null);
            }

            if (HttpContext.Items["User"] is ExUser user)
            {
                if (await user.HasPermissionInCompany(_db, id, true).ConfigureAwait(true))
                {
                    tblOrg.Name = companyName;
                    await _db.SaveChangesAsync().ConfigureAwait(true);
                    await _hub.SyncDcExOrganization(_db.TblPermissions
                            .Where(x => x.TblOrganizationId == id)
                            .Select(x => x.TblUserId).ToList())
                        .ConfigureAwait(true);

                    return Ok(true);
                }
            }
            else if (HttpContext.Items["Org"] is ExOrganization org)
            {
                if (org.IsInAccessControlList(_db, id))
                {
                    tblOrg.Name = companyName;
                    await _db.SaveChangesAsync().ConfigureAwait(true);
                    await _hub.SyncDcExOrganization(_db.TblPermissions
                            .Where(x => x.TblOrganizationId == id)
                            .Select(x => x.TblUserId).ToList())
                        .ConfigureAwait(true);
                    return Ok(true);
                }
            }

            return Unauthorized();
        }
    }
}