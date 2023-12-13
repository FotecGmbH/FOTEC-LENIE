// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using Database.Tables;
using Exchange.Enum;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>Organization</para>
///     Klasse HelperDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle Admins einer Organisation
    /// </summary>
    /// <param name="orgId"></param>
    /// <returns></returns>
    public List<long> GetOrgAdmins(long orgId)
    {
        return TblPermissions.AsNoTracking().Where(w => w.TblOrganizationId == orgId).Select(s => s.TblUserId).ToList();
    }


    /// <summary>
    ///     Alle Organisationen für einen bestimmten Benutzer
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public List<long> GetOrganizationsIdsForUser(long userId)
    {
        return GetTableOrganizationForUser(userId).Distinct().Select(s => s.Id).ToList();
    }

    /// <summary>
    ///     Alle Organizationen für einen User
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableOrganization> GetTableOrganizationForUser(long userId)
    {
        if (userId <= 0)
        {
            return TblOrganizations
                .AsNoTracking()
                .Where(c => c.OrganizationType == EnumOrganizationTypes.PublicOrganization)
                .Include(i => i.TblIdeaOrganisations);
        }

        var isAdmin = IsUserSysAdmin(userId);
        var r = TblOrganizations.AsNoTracking()
            .Include(x => x.TblPermissions)
            .Where(c =>
                isAdmin ||
                c.OrganizationType == EnumOrganizationTypes.PublicOrganization ||
                (c.TblPermissions.Any(a => a.TblUserId == userId)))
            .Include(i => i.TblIdeaOrganisations);

        return r;
    }

    /// <summary>
    ///     Alle Organisationen, bei denen der User UserPlus oder Admin ist.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableOrganization> GetUserOrganizationsForUser(long userId)
    {
        if (userId <= 0)
        {
            return Enumerable.Empty<TableOrganization>().AsQueryable();
        }

        var isAdmin = IsUserSysAdmin(userId);
        var r = TblOrganizations.AsNoTracking()
            .Include(x => x.TblPermissions)
            .Where(c =>
                isAdmin ||
                c.TblPermissions.Any(a => a.TblUserId == userId));
        return r;
    }

    /// <summary>
    ///     Alle Organisationen, bei denen der User UserPlus oder Admin ist.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IQueryable<TableOrganization> GetUserPlusOrganizationForUser(long userId)
    {
        if (userId <= 0)
        {
            return Enumerable.Empty<TableOrganization>().AsQueryable();
        }

        var isAdmin = IsUserSysAdmin(userId);
        var r = TblOrganizations.AsNoTracking()
            .Include(x => x.TblPermissions)
            .Where(c =>
                isAdmin ||
                c.TblPermissions.Any(a => a.TblUserId == userId && a.UserRole != EnumUserRole.User));
        return r;
    }

    /// <summary>
    ///     Organization mit abhängigkeiten
    /// </summary>
    /// <param name="organizationId"></param>
    /// <param name="noTracking"></param>
    /// <returns></returns>
    public TableOrganization? GetOrganizationWithDependencies(long organizationId, bool noTracking = true)
    {
        if (noTracking)
        {
            return TblOrganizations.AsNoTracking().Include(x => x.TblPermissions).FirstOrDefault(c => c.Id == organizationId);
        }

        return TblOrganizations.Include(x => x.TblPermissions).FirstOrDefault(c => c.Id == organizationId);
    }
}