﻿// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model;
using Exchange.Model.User;

namespace Database.Converter;

/// <summary>
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbUser
{
    /// <summary>
    ///     TableUser nach ExUser konvertieren
    /// </summary>
    /// <param name="u">Tablle</param>
    /// <returns></returns>
    public static ExUser ToExUser(this TableUser u)
    {
        if (u == null!)
        {
            throw new NullReferenceException($"[{nameof(ConverterDbUser)}]({nameof(ToExUser)}): {nameof(TableUser)} is null");
        }

        var r = new ExUser
                {
                    IsSysAdmin = u.IsAdmin,
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    LoginName = u.LoginName,
                    UserImageDbId = u.TblUserImage != null! ? u.TblUserImage.Id : -1,
                    UserImageLink = u.TblUserImage != null! ? u.TblUserImage.PublicLink : string.Empty,
                    LoginConfirmed = u.LoginConfirmed,
                    PushTags = u.PushTags,
                    Locked = u.Locked,
                    Setting10MinPush = u.Setting10MinPush,
                    PhoneNumber = u.PhoneNumber,
                    NotificationMailChat = u.NotificationMailChat,
                    NotificationMailMeeting = u.NotificationMailMeeting,
                    NotificationMailReport = u.NotificationMailReport,
                    NotificationPushChat = u.NotificationPushChat,
                    NotificationPushMeeting = u.NotificationPushMeeting,
                    NotificationPushComment = u.NotificationPushComment,
                    NotificationPushLike = u.NotificationPushLike,
                    NotificationPushSupport = u.NotificationPushSupport,
                    NotificationPushReport = u.NotificationPushReport,
                    NotificationPushIdea = u.NotificationPushIdea,
                    PhoneConfirmed = u.PhoneConfirmed,
                    SmsCode = string.Empty,
                };

        if (u.TblDevices != null!)
        {
            foreach (var d in u.TblDevices)
            {
                r.UserDevices.Add(new ExUserDevice
                                  {
                                      Manufacturer = d.Manufacturer,
                                      Model = d.Model,
                                      DeviceIdiom = d.DeviceIdiom,
                                      DeviceName = d.DeviceName,
                                      Id = d.Id,
                                      Plattform = d.Plattform
                                  });
            }
        }

        if (u.TblPermissions != null!)
        {
            foreach (var permission in u.TblPermissions)
            {
                r.Permissions.Add(new ExUserPermission
                                  {
                                      CompanyId = permission.TblOrganizationId,
                                      Town = permission.TblOrganization.ToExExOrganization(),
                                      UserRight = permission.UserRight,
                                      UserRole = permission.UserRole,
                                      IsMainCompany = permission.MainOrganization,
                                      DbId = permission.Id
                                  });
            }
        }

        if (u.TblAccessToken != null!)
        {
            foreach (var token in u.TblAccessToken)
            {
                r.Tokens.Add(new ExAccessToken
                             {
                                 DbId = token.Id,
                                 GuiltyUntilUtc = token.GuiltyUntilUtc,
                                 Token = token.Token
                             });
            }
        }

        return r;
    }
}