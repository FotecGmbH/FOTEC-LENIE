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
using Biss.Dc.Core;
using Biss.EMail;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Enum;
using Exchange.Helper;
using Exchange.Model.Organization;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExCompanyUsers</para>
///     Klasse DcExCompanyUsers. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcCompanyUsers
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="startIndex">Lesen ab Index (-1 für Start)</param>
    /// <param name="elementsToRead">Anzahl der Elemente welche maximal gelesen werden sollen (-1 für alle verfügbaren Daten)</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <param name="filter">Optionaler Filter für die Daten</param>
    /// <returns>Daten oder eine Exception auslösen</returns>
    [Obsolete("Sync benutzen")]
    public async Task<List<DcServerListItem<ExOrganizationUser>>> GetDcExOrganizationUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var result = new List<DcServerListItem<ExOrganizationUser>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        //Gastbenutzer
        if (userId <= 0)
        {
            return result;
        }

        if (db.IsUserSysAdmin(userId))
        {
            //Superadmins
            var admins = db.TblUsers.Where(u => u.IsAdmin).Select(s => new
                                                                       {
                                                                           s.Id, s.FirstName, s.LastName, s.LoginName, s.PhoneNumber,
                                                                       }).ToList();
            var noCompanyId = db.TblOrganizations.Where(w => w.OrganizationType == EnumOrganizationTypes.NoOrganization).Select(s => s.Id).First();

            var startId = long.MaxValue - 1 - admins.Count;
            foreach (var admin in admins)
            {
                var img = db.TblUsers.Where(f => f.Id == admin.Id).Include(i => i.TblUserImage).FirstOrDefault()?.TblUserImage?.PublicLink;
                var image = img == null! ? string.Empty : img;

                var d = new DcServerListItem<ExOrganizationUser>
                        {
                            Data = new()
                                   {
                                       IsSuperadmin = true,
                                       UserRole = EnumUserRole.Admin,
                                       OrganizationId = noCompanyId,
                                       Fullname = $"{admin.FirstName} {admin.LastName}",
                                       LoginDoneByUser = true,
                                       UserId = admin.Id,
                                       UserLoginEmail = admin.LoginName,
                                       UserPhoneNumber = admin.PhoneNumber,
                                       UserRight = EnumUserRight.ReadWrite,
                                       UserImageLink = image,
                                       OrganizationPostalCode = "ADMIN",
                                   },
                            SortIndex = startId,
                            Index = startId,
                        };
                startId++;
                result.Add(d);
            }
        }

        foreach (var p in db.GetOrganizationUsers(userId))
        {
            var d = new DcServerListItem<ExOrganizationUser>
                    {
                        Data = p.ToExOrganizationUser(),
                        SortIndex = p.Id,
                        Index = p.Id,
                    };
            result.Add(d);
        }

        return result;
    }

    /// <summary>
    ///     Device will Listen Daten für DcCompanyUsers sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExOrganizationUsers(long deviceId, long userId, List<DcStoreListItem<ExOrganizationUser>> data, long secondId)
    {
        if (data == null!)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var r = new DcListStoreResult
                {
                    SecondId = secondId,
                    StoreResult = new(),
                    ElementsStored = new()
                };

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        TablePermission p = null!;
#pragma warning disable CS0219
        var anyDelete = false;
#pragma warning restore CS0219
        var modifiedUsers = new List<long>();
        var users2Inform = new List<long>(db.GetSysAdmins()) {userId};

        foreach (var d in data)
        {
            (bool newUser, TableUser user, string pwd) informNewUserForNewAccount = (false, null!, string.Empty);
            var tmp = new DcListStoreResultIndexAndData();

            switch (d.State)
            {
                case EnumDcListElementState.New:
                    p = new TablePermission();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    informNewUserForNewAccount = db.CheckNewUserForPremission(d.Data);
                    break;
                case EnumDcListElementState.Modified:
                    p = db.TblPermissions
                        .Include(x => x.TblUser)
                        .First(f => f.Id == d.Index);
                    r.ElementsStored++;
                    modifiedUsers.Add(p.TblUserId);
                    users2Inform.Add(p.TblUserId);
                    users2Inform.AddRange(db.GetOrgAdmins(p.TblOrganizationId));
                    break;
                case EnumDcListElementState.Deleted:
                    p = db.TblPermissions
                        .Include(x => x.TblUser)
                        .First(f => f.Id == d.Index);
                    modifiedUsers.Add(p.TblUserId);
                    users2Inform.Add(p.TblUserId);
                    users2Inform.AddRange(db.GetOrgAdmins(p.TblOrganizationId));
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                db.TblPermissions.Remove(p);
                anyDelete = true;
            }
            else
            {
                d.Data.ToTablePermission(p);
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblPermissions.Add(p);
            }

            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = p.Id;
                tmp.NewSortIndex = p.Id;
                r.NewIndex.Add(tmp);
            }

            if (informNewUserForNewAccount.newUser)
            {
                modifiedUsers.Add(informNewUserForNewAccount.user.Id);

                if (!string.IsNullOrWhiteSpace(informNewUserForNewAccount.user.LoginName) && Validator.Check(informNewUserForNewAccount.user.LoginName))
                {
                    var email = new UserAccountEmailService(_razorEngine);
                    await email.SendValidationMailWithoutDevice(informNewUserForNewAccount.user, db, informNewUserForNewAccount.pwd).ConfigureAwait(false);
                }

                if (!string.IsNullOrWhiteSpace(informNewUserForNewAccount.user.PhoneNumber) && PhoneNumberValidator.IsValid(informNewUserForNewAccount.user.PhoneNumber))
                {
                    await SmsHelper.SendValidationSms(informNewUserForNewAccount.user, db).ConfigureAwait(false);
                }
            }
        }

        _ = Task.Run(async () =>
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            // User welche über die Änderung informiert werden müssen:
            // Sys-Admin, Org-Admins und der (neue) User der bearbeitet wurde (wurden oben in die Liste "" hinzugefügt)
            users2Inform = users2Inform.Distinct().ToList();

            // Alle Admins über die Änderungen benachrichtigen
            await SyncDcExOrganizationUsers(users2Inform, deviceId).ConfigureAwait(false);

            // Die modifizierten User auf den Geräten benachrichtigen
            foreach (var id in modifiedUsers)
            {
                var dbUser = db2.GetUserWithdependences(id);
                if (dbUser == null)
                {
                    continue;
                }

                var data2Send = dbUser.ToExUser();
                await SendDcExUser(data2Send, userId: id).ConfigureAwait(false);
            }
        });
        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für DcExOrganizationUsers
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public async Task<DcListSyncResultData<ExOrganizationUser>> SyncDcExOrganizationUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var users4User = db.GetOrganizationUsersIds(userId);
        var result = new DcListSyncResultData<ExOrganizationUser>
                     {
                         ServerItemCount = users4User.Count
                     };
        var t1 = new DcDbSyncHelper<TablePermission>(db.TblPermissions.AsNoTracking()
            .Include(i => i.TblOrganization)
            .Include(i => i.TblUser)
            .ThenInclude(x => x.TblUserImage));
        var t2 = t1.GetSyncData(current, o => users4User.Contains(o.Id), s => new DcSyncElement(s.Id, new List<byte[]> {s.DataVersion, s.TblOrganization.DataVersion, s.TblUser.DataVersion}));
        result.ItemsToRemoveOnClient = t2.intemsToRemove;
        if (t2.modifiedElementsDb != null)
        {
            foreach (var t in t2.modifiedElementsDb)
            {
                var tmp = new DcSyncElement(t.Id, new List<byte[]>
                                                  {
                                                      t.DataVersion,
                                                      t.TblOrganization.DataVersion,
                                                      t.TblUser.DataVersion
                                                  });
                result.NewOrModifiedItems.Add(new DcServerListItem<ExOrganizationUser>
                                              {
                                                  Index = t.Id,
                                                  SortIndex = t.Id,
                                                  DataVersion = tmp.DataVersion,
                                                  Data = t.ToExOrganizationUser()
                                              });
            }
        }

        return result;
    }

    #endregion
}