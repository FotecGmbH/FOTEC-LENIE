// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using Biss.Apps.Base;
using Database.Tables;
using Exchange;
using Exchange.Enum;
using Exchange.Model.Organization;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Database;

/// <summary>
///     <para>Hilfmethoden für die Berechtigungen</para>
///     Klasse HelperDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class Db
{
    /// <summary>
    ///     Alle OrganizationUsers (Permissions) welche der User "sehen" sollte
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public List<long> GetOrganizationUsersIds(long currentUserId)
    {
        return GetOrganizationUsers(currentUserId).Select(s => s.Id).ToList();
    }

    /// <summary>
    ///     Alle User und Organizationen in welchen der aktuelle Benutzer Firmenadmin (oder globaler Admins) ist
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public IQueryable<TablePermission> GetOrganizationUsers(long currentUserId)
    {
        var isAdmin = IsUserSysAdmin(currentUserId);
        var organizationUserIsAdmin = new List<long>();
        if (!isAdmin)
        {
            organizationUserIsAdmin = TblPermissions
                .Where(w => w.TblUserId == currentUserId
                            && (w.UserRole == EnumUserRole.Admin || w.UserRole == EnumUserRole.UserPlus))
                .Select(s => s.TblOrganizationId).ToList();
        }

        var r = TblPermissions.AsNoTracking()
            .Where(w => isAdmin || (organizationUserIsAdmin.Contains(w.TblOrganizationId)))
            .Include(i => i.TblOrganization)
            .Include(i => i.TblUser)
            .ThenInclude(i => i.TblUserImage);

        return r;
    }

    /// <summary>
    ///     Einer Firma wurde ein (neuer) User zugewiesen. Ist dieser noch nicht im System dann Account erstellen (damit
    ///     Registrierung starten).
    ///     Datensatz mit dem richtigen dann sicher vorhandenen User verknüpfen.
    /// </summary>
    /// <param name="user"></param>
    public (bool newUser, TableUser user, string pwd) CheckNewUserForPremission(ExOrganizationUser user)
    {
        if (user == null!)
        {
            throw new ArgumentNullException($"[{nameof(Db)}]({nameof(CheckNewUserForPremission)}): {nameof(user)}");
        }

        var tmpList = TblUsers.Select(s => new {s.LoginName, s.Id}).ToList();
        var tmp = tmpList.FirstOrDefault(u => string.Equals(u.LoginName, user.UserLoginEmail, StringComparison.InvariantCultureIgnoreCase));

        //Nuer User
        if (tmp == null)
        {
            var newUserPassword = AppCrypt.GeneratePassword(10);
            if (AppSettings.Current().AppConfigurationConstants > 1)
            {
                newUserPassword = "biss";
            }

            var u = new TableUser
                    {
                        PasswordHash = AppCrypt.CumputeHash(newUserPassword),
                        LoginName = user.UserLoginEmail,
                        PhoneNumber = user.UserPhoneNumber,
                        DefaultLanguage = "de",
                        LoginConfirmed = false,
                        IsAdmin = false,
                        AgbVersion = "1.0.0",
                        CreatedAtUtc = DateTime.UtcNow,
                        RefreshToken = AppCrypt.GeneratePassword(),
                        JwtToken = AppCrypt.GeneratePassword(),
                        ConfirmationToken = AppCrypt.GeneratePassword(),
                        Locked = false
                    };
            TblUsers.Add(u);
            SaveChanges();
            user.UserId = u.Id;
            return (true, u, newUserPassword);
        }

        user.UserId = tmp.Id;
        return (false, null!, string.Empty);
    }
}