// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model.Organization;


namespace Database.Converter;

/// <summary>
///     <para>DbPosition konvertieren</para>
///     Klasse ConverterDbPosition. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbPermission
{
    /// <summary>
    ///     TablePermission nach ExCompanyUser
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static ExOrganizationUser ToExOrganizationUser(this TablePermission t)
    {
        if (t == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToExOrganizationUser)}): {nameof(t)}");
        }

        return new ExOrganizationUser
               {
                   OrganizationId = t.TblOrganizationId,
                   OrganizationPostalCode = t.TblOrganization.PostalCode,
                   LoginDoneByUser = t.TblUser.LoginConfirmed,
                   UserId = t.TblUserId,
                   UserLoginEmail = t.TblUser.LoginName,
                   UserPhoneNumber = t.TblUser.PhoneNumber,
                   UserRight = t.UserRight,
                   UserRole = t.UserRole,
                   Fullname = $"{t.TblUser.FirstName} {t.TblUser.LastName}",
                   UserImageLink = t.TblUser.TblUserImage == null! ? string.Empty : t.TblUser.TblUserImage.PublicLink,
                   Locked = t.TblUser.Locked,
               };
    }

    /// <summary>
    ///     ExCompanyUser nach TablePermission
    /// </summary>
    /// <param name="u"></param>
    /// <param name="p"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ToTablePermission(this ExOrganizationUser u, TablePermission p)
    {
        if (p == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToTablePermission)}): {nameof(p)}");
        }

        if (u == null!)
        {
            throw new ArgumentNullException($"[{nameof(ConverterDbPermission)}]({nameof(ToTablePermission)}): {nameof(u)}");
        }

        p.TblOrganizationId = u.OrganizationId;
        p.TblUserId = u.UserId;
        p.UserRight = u.UserRight;
        p.UserRole = u.UserRole;

        if (p.TblUser != null!)
        {
            p.TblUser.Locked = u.Locked;
        }
    }
}