// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Log.Producer;
using ConnectivityHost.Helper;
using Database;
using Database.Converter;
using Database.Tables;
using Exchange.Enum;
using Exchange.Model.Organization;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ConnectivityHost.DataConnector;

/// <summary>
///     <para>Datenaustausch für DcExOrganization</para>
///     Klasse DcExOrganization. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public partial class ServerRemoteCalls
{
    #region Interface Implementations

    /// <summary>
    ///     Device fordert Listen Daten für DcExOrganization
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
    public async Task<List<DcServerListItem<ExOrganization>>> GetDcExOrganization(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter)
    {
        var returnList = new List<DcServerListItem<ExOrganization>>();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var companies = db.TblOrganizations
            .Where(x => x.OrganizationType != EnumOrganizationTypes.NoOrganization)
            .Include(x => x.TblAccessToken)
            .Include(x => x.TblPermissions)
            .Include(x => x.TblIdeaOrganisations);

        foreach (var tableCompany in companies)
        {
            var exOrg = tableCompany.ToExExOrganization();

            // check Permission
            exOrg.UserIsAdmin = db.IsUserSysAdmin(userId) ||
                                tableCompany.TblPermissions.Any(x => x.TblUserId == userId && x.UserRole == EnumUserRole.Admin);

            exOrg.UserIsUserPlus = exOrg.UserIsAdmin ||
                                   tableCompany.TblPermissions.Any(x => x.TblUserId == userId && x.UserRole == EnumUserRole.UserPlus);

            exOrg.UserIsInOrg = tableCompany.TblPermissions.Any(x => x.TblUserId == userId);

            returnList.Add(new DcServerListItem<ExOrganization>
                           {
                               Index = tableCompany.Id,
                               SortIndex = tableCompany.Id,
                               Data = exOrg,
                           });
        }

        return returnList;
    }

    /// <summary>
    ///     Device will Listen Daten für DcExOrganization sichern
    /// </summary>
    /// <param name="deviceId">Id des Gerätes</param>
    /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
    /// <param name="data">Eingetliche Daten</param>
    /// <param name="secondId">
    ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
    ///     für Chats
    /// </param>
    /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
    public async Task<DcListStoreResult> StoreDcExOrganization(long deviceId, long userId, List<DcStoreListItem<ExOrganization>> data, long secondId)
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

        var users2Inform = new List<long>(db.GetSysAdmins()) {userId};

        TableOrganization p = null!;
#pragma warning disable CS0219
        var anyDelete = false;
#pragma warning restore CS0219

        foreach (var d in data)
        {
            var tmp = new DcListStoreResultIndexAndData();
            switch (d.State)
            {
                case EnumDcListElementState.New:
                    p = new TableOrganization();
                    tmp.BeforeStoreIndex = d.Index;
                    r.ElementsStored++;
                    break;
                case EnumDcListElementState.Modified:
                    p = await db.TblOrganizations
                        .Where(f => f.Id == d.Index)
                        .Include(i => i.TblPermissions)
                        .Include(i => i.TblAccessToken)
                        .FirstAsync().ConfigureAwait(false);
                    r.ElementsStored++;
                    users2Inform.AddRange(p.TblPermissions.Select(f => f.TblUserId));
                    break;
                case EnumDcListElementState.Deleted:
                    p = await db.TblOrganizations
                        .Where(f => f.Id == d.Index)
                        .Include(i => i.TblPermissions)
                        .Include(i => i.TblAccessToken)
                        .FirstAsync().ConfigureAwait(false);
                    users2Inform.AddRange(p.TblPermissions.Select(f => f.TblUserId));
                    break;
                case EnumDcListElementState.None:
                    continue;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (d.State == EnumDcListElementState.Deleted)
            {
                db.TblAccessToken.RemoveRange(p.TblAccessToken);
                db.TblPermissions.RemoveRange(p.TblPermissions);
                db.TblOrganizations.Remove(p);
                anyDelete = true;
            }
            else
            {
                d.Data.ToTableOrganization(p);
            }

            if (d.State == EnumDcListElementState.New)
            {
                db.TblOrganizations.Add(p);
            }

            //Gelöschte Token
            foreach (var l in p.TblAccessToken.ToList())
            {
                if (!d.Data.Tokens.Select(s => s.DbId).Contains(l.Id))
                {
                    db.TblAccessToken.Remove(l);
                }
            }

            //Neue Token
            foreach (var t in d.Data.Tokens)
            {
                if (t.DbId <= 0)
                {
                    p.TblAccessToken.Add(new TableAccessToken
                                         {
                                             GuiltyUntilUtc = t.GuiltyUntilUtc,
                                             TblOrganizationId = d.Index,
                                             Token = t.Token
                                         });
                }
            }


            await db.SaveChangesAsync().ConfigureAwait(true);
            if (d.State == EnumDcListElementState.New)
            {
                tmp.NewIndex = p.Id;
                tmp.NewSortIndex = p.Id;
                r.NewIndex.Add(tmp);
            }
        }

        _ = Task.Run(async () =>
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            //Alle Benuter über die Änderung informieren
            users2Inform = users2Inform.Distinct().ToList();

            await SyncDcExOrganization(users2Inform, deviceId).ConfigureAwait(false);
            await SyncDcExOrganizationUsers(users2Inform).ConfigureAwait(false);
        });

        return r;
    }

    /// <summary>
    ///     Daten Synchronisieren für
    /// </summary>
    /// <param name="deviceId">Gerät</param>
    /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
    /// <param name="current">Aktuelle Datensätze am Gerät</param>
    /// <param name="props">Zusätzliche Optionen</param>
    /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
    public async Task<DcListSyncResultData<ExOrganization>> SyncDcExOrganization(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props)
    {
        if (current == null!)
        {
            throw new ArgumentNullException(nameof(current));
        }

        Logging.Log.LogInfo($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExOrganization)}): Start Org Sync - {userId} - " +
                            string.Join(",", current.CurrentListEntries.Select(x => x.Id)));
        var sw = new Stopwatch();
        sw.Start();

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
        await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

        var result = new DcListSyncResultData<ExOrganization>();

        var t1 = new DcDbSyncHelper<TableOrganization>(db.TblOrganizations
            .Include(i => i.TblPermissions)
            .Include(i => i.TblIdeaOrganisations)
            .AsNoTracking());
        var orgs = db.TblOrganizations
            .Where(x => x.OrganizationType != EnumOrganizationTypes.NoOrganization)
            .Select(x => x.Id).ToList();
        result.ServerItemCount = orgs.Count;

        var dvEmpty = new byte[db.TblOrganizations.FirstOrDefault()?.DataVersion.Length ?? 16];
        for (var i = 0; i < dvEmpty.Length; i++)
        {
            dvEmpty[i] = 0;
        }

        var t2 = t1.GetSyncData(current,
            o => orgs.Contains(o.Id),
            t => new DcSyncElement(t.Id, new List<byte[]>
                                         {
                                             t.DataVersion,
                                             t.TblPermissions.Any() ? new DcSyncElement(t.Id, t.TblPermissions.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                             t.TblIdeaOrganisations.Any() ? new DcSyncElement(t.Id, t.TblIdeaOrganisations.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                         }));
        result.ItemsToRemoveOnClient = t2.intemsToRemove;
        if (t2.modifiedElementsDb != null)
        {
            var userIsAdmin = db.IsUserSysAdmin(userId);

            foreach (var t in t2.modifiedElementsDb)
            {
                var exOrg = t.ToExExOrganization();

                var tmp = new DcSyncElement(t.Id, new List<byte[]>
                                                  {
                                                      t.DataVersion,
                                                      t.TblPermissions.Any() ? new DcSyncElement(t.Id, t.TblPermissions.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                      t.TblIdeaOrganisations.Any() ? new DcSyncElement(t.Id, t.TblIdeaOrganisations.Select(x => x.DataVersion).ToList()).DataVersion : dvEmpty,
                                                  });

                // check Permission
                exOrg.UserIsAdmin = userIsAdmin ||
                                    t.TblPermissions.Any(x => x.TblUserId == userId && x.UserRole == EnumUserRole.Admin);

                exOrg.UserIsUserPlus = exOrg.UserIsAdmin ||
                                       t.TblPermissions.Any(x => x.TblUserId == userId && x.UserRole == EnumUserRole.UserPlus);

                exOrg.UserIsInOrg = t.TblPermissions.Any(x => x.TblUserId == userId);

                result.NewOrModifiedItems.Add(new DcServerListItem<ExOrganization>
                                              {
                                                  Index = t.Id,
                                                  SortIndex = t.Id,
                                                  DataVersion = tmp.DataVersion,
                                                  Data = exOrg,
                                              });
            }
        }

        sw.Stop();

        Logging.Log.LogInfo($"[{nameof(ServerRemoteCalls)}]({nameof(SyncDcExOrganization)}): Org Sync finished - {sw.ElapsedMilliseconds} ms - n/u:" +
                            string.Join(",", result.NewOrModifiedItems.Select(x => x.Index)) + " - d:" +
                            string.Join(",", result.ItemsToRemoveOnClient));

        return result;
    }

    #endregion
}