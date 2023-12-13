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
using Microsoft.EntityFrameworkCore;


// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>Hifsfunktionen "db" für User</para>
///     Klasse HelperUser. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle SysAdmins
    /// </summary>
    /// <returns></returns>
    public List<long> GetSysAdmins()
    {
        return TblUsers.AsNoTracking().Where(u => u.IsAdmin).Select(s => s.Id).ToList();
    }

    /// <summary>
    ///     Ist ein bestimmter Benutzer System-Admin
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public bool IsUserSysAdmin(long userId)
    {
        if (userId <= 0)
        {
            return false;
        }

        return TblUsers.Where(f => f.Id == userId).Select(s => s.IsAdmin).FirstOrDefault();
    }

    /// <summary>
    ///     Einen bestimmten Benutzer mit dessen Abhängigkeiten laden
    /// </summary>
    /// <param name="userId">Id des Users</param>
    /// <param name="doNotTrack">Abfrage mit AsNoTracking()</param>
    /// <returns></returns>
    public TableUser? GetUserWithdependences(long userId, bool doNotTrack = true)
    {
        if (doNotTrack)
        {
            return TblUsers.AsNoTracking()
                .Where(i => i.Id == userId)
                .Include(x => x.TblUserImage)
                .Include(y => y.TblDevices)
                .Include(i => i.TblAccessToken)
                .Include(i => i.TblPermissions)
                .ThenInclude(ti => ti.TblOrganization)
                .FirstOrDefault();
        }

        return TblUsers
            .Where(i => i.Id == userId)
            .Include(x => x.TblUserImage)
            .Include(i => i.TblAccessToken)
            .Include(y => y.TblDevices)
            .Include(i => i.TblPermissions).ThenInclude(ti => ti.TblOrganization)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Username und Bild für ID holen
    ///     TODO Cache?
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public (long, string, string) GetUserNameAndImage(long userId)
    {
        var user = TblUsers
            .Include(x => x.TblUserImage)
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
        {
            return (userId, string.Empty, string.Empty);
        }

        var userName = $"{user.FirstName} {user.LastName}";

        var userImage = user.TblUserImage != null ? user.TblUserImage.PublicLink : string.Empty;

        return (userId, userName, userImage);
    }
}