// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Exchange.Enum;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Zugrifssberechtigung</para>
    ///     Klasse AccessControl. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class TokenAccessControl
    {
        /// <summary>
        ///     Hat der Benutzer die notwendigen Berechtigungen
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">Datenbank</param>
        /// <param name="companyId">Firma</param>
        /// <param name="readAndWrite">LesUndSchreibrechte</param>
        /// <returns>Berechtigt/Nicht berechtigt</returns>
        public static async Task<bool> HasPermissionInCompany(this ExUser user, Db db, long companyId, bool readAndWrite = false)
        {
            if (db == null!)
            {
                throw new ArgumentNullException($"[{nameof(TokenAccessControl)}]({nameof(HasPermissionInCompany)}): {nameof(db)}");
            }

            var acl = await user.GetAccessControlList(db).ConfigureAwait(true);

            if (readAndWrite)
            {
                var permission = acl.CompanyPermissions.FirstOrDefault(a => a.Id == companyId && a.UserRight == EnumUserRight.ReadWrite);
                return permission != null;
            }
            else
            {
                var permission = acl.CompanyPermissions.FirstOrDefault(a => a.Id == companyId);
                return permission != null;
            }
        }

        /// <summary>
        ///     Abfrage ob der Org-Token für die Organisation gültig ist
        /// </summary>
        /// <param name="org">Organisation</param>
        /// <param name="db">Datenbank</param>
        /// <param name="companyId">Firma</param>
        /// <returns>Berechtigt/Nicht berechtigt</returns>
        public static bool IsInAccessControlList(this ExOrganization org, Db db, long companyId)
        {
            var acl = org.GetAccessControlList(db);

            var permission = acl.CompanyPermissions.FirstOrDefault(a => a.Id == companyId);
            return permission != null;
        }

        /// <summary>
        ///     Zugriffsberechtigungen abfragen
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">Datenbank</param>
        /// <returns>Liste mit den Berechtigungen</returns>
        public static async Task<AccessControlList> GetAccessControlList(this ExUser user, Db db)
        {
            if (user == null!)
            {
                throw new ArgumentNullException($"[{nameof(TokenAccessControl)}]({nameof(GetAccessControlList)}): {nameof(user)}");
            }

            if (db == null!)
            {
                throw new ArgumentNullException($"[{nameof(TokenAccessControl)}]({nameof(GetAccessControlList)}): {nameof(db)}");
            }

            var accessTokenInformation = new AccessControlList("User", new List<CompanyPermission>());

            if (user.IsSysAdmin)
            {
                accessTokenInformation.CompanyPermissions.AddRange(
                    await db.TblOrganizations.Select(a => new CompanyPermission(a.Id, EnumUserRight.ReadWrite)).ToListAsync().ConfigureAwait(true));
            }
            else
            {
                if (user.Permissions.Any())
                {
                    accessTokenInformation.CompanyPermissions.AddRange(
                        user.Permissions.Select(p => new CompanyPermission(p.CompanyId, p.UserRight)));
                }
            }

            return accessTokenInformation;
        }

        /// <summary>
        ///     Zugriffsberechtigungen abfragen
        /// </summary>
        /// <param name="org">Organisation</param>
        /// <param name="db">Datenbank</param>
        /// <returns>Liste mit den Berechtigungen</returns>
        public static AccessControlList GetAccessControlList(this ExOrganization org, Db db)
        {
            if (db == null!)
            {
                throw new ArgumentNullException($"[{nameof(TokenAccessControl)}]({nameof(GetAccessControlList)}): {nameof(db)}");
            }

            if (org == null!)
            {
                throw new ArgumentNullException($"[{nameof(TokenAccessControl)}]({nameof(GetAccessControlList)}): {nameof(org)}");
            }


            var accessTokenInformation = new AccessControlList("Organization", new List<CompanyPermission>());

            accessTokenInformation.CompanyPermissions.Add(new CompanyPermission(org.OrganizationId, EnumUserRight.ReadWrite));

            return accessTokenInformation;
        }
    }

    /// <summary>
    ///     Zugriffberechtigungen
    /// </summary>
    /// <param name="Type">Typ der Berechtigung (User oder Organisation)</param>
    /// <param name="CompanyPermissions">Liste der Organisationsberechtigungen</param>
    public record AccessControlList(string Type, List<CompanyPermission> CompanyPermissions);

    /// <summary>
    ///     Organisationsberechtigung
    /// </summary>
    /// <param name="Id">ID der Organisation</param>
    /// <param name="UserRight">Benutzerrecht</param>
    public record CompanyPermission(long Id, EnumUserRight UserRight);
}