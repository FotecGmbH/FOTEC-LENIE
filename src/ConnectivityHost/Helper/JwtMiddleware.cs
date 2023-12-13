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
using Database.Converter;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     Middleware JWT (Um Token im Request abzufangen)
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="next">next</param>
        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        ///     Ausführen
        /// </summary>
        /// <param name="context">Kontext</param>
        /// <param name="db">Datenbank</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, Db db)
        {
            if (context == null!)
            {
                throw new ArgumentNullException($"[{nameof(JwtMiddleware)}]({nameof(Invoke)}): {nameof(context)}");
            }

            if (db == null!)
            {
                throw new ArgumentNullException($"[{nameof(JwtMiddleware)}]({nameof(Invoke)}): {nameof(db)}");
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await attachUserOrCompanyToContext(context, db, token).ConfigureAwait(true);
            }

            await _next(context).ConfigureAwait(true);
        }

        /// <summary>
        ///     User oder Company dem HttpContext hinzufügen, damit bei Controllern gleich direkt darauf zugegriffen werden kann
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="db">Datenbank</param>
        /// <param name="token">Token</param>
        /// <returns></returns>
        private async Task attachUserOrCompanyToContext(HttpContext context, Db db, string token)
        {
            try
            {
                // attach user to context on successful jwt validation
                var accessToken = await db.TblAccessToken.FirstOrDefaultAsync(a => a.Token == token).ConfigureAwait(true);

                if (accessToken != null)
                {
                    if (accessToken.TblUserId != null)
                    {
                        var user = await db.TblUsers.Include(a => a.TblAccessToken).Include(a => a.TblDevices).Include(a => a.TblPermissions).AsNoTracking().FirstOrDefaultAsync(a => a.Id == accessToken.TblUserId).ConfigureAwait(true);
                        if (user != null)
                        {
                            context.Items["User"] = user.ToExUser();
                        }
                    }
                    else if (accessToken.TblOrganizationId != null)
                    {
                        var org = await db.TblOrganizations.Include(a => a.TblAccessToken).Include(a => a.TblPermissions).AsNoTracking().FirstOrDefaultAsync(a => a.Id == accessToken.TblOrganizationId).ConfigureAwait(true);
                        if (org != null)
                        {
                            context.Items["Org"] = org.ToExExOrganization();
                        }
                    }
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}