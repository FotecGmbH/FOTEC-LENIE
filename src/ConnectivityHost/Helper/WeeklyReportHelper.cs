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
using Biss.Log.Producer;
using Database;
using Exchange.Enum;
using Exchange.Model.WeeklyReport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Helper für Auswertung</para>
    ///     Klasse WeeklyReportHelper. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class WeeklyReportHelper
    {
        /// <summary>
        ///     Wöchentlichen Report erstellen
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static async Task<ExWeeklyReport> GetWeeklyReport(DateTime startDate, DateTime endDate, Db? db = null)
        {
            var result = new ExWeeklyReport
                         {
                             CreatedAt = DateTime.Now,
                             StartReport = startDate,
                             EndReport = endDate,
                         };

            try
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                db ??= new Db();
#pragma warning restore CA2000 // Dispose objects before losing scope

                #region Generell

                #region Users

                var users = db.TblUsers.AsNoTracking()
                    .Include(x => x.TblDevices);

                result.UsersTotal = users.Count();
                result.NewUsers = users.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);
                result.LoggedInUsers = users.Count(x => x.TblDevices.Any(d => d.LastLogin >= startDate && d.LastLogin < endDate));

                #endregion

                #region Ideas

                var ideas = db.TblIdeas.AsNoTracking();
                var ideaLikes = db.TblIdeaLikes.AsNoTracking();
                var ideaSupports = db.TblIdeaHelpers.AsNoTracking();

                result.IdeasTotal = ideas.Count();
                result.IdeasNewTime = ideas.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                result.IdeaLikesTotal = ideaLikes.Count();
                result.IdeaLikesTime = ideaLikes.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                result.IdeaSupportsTotal = ideaSupports.Count();
                result.IdeaSupportsTime = ideaSupports.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                #endregion

                #endregion

                #region Auswahl Gemeinden

                var townIdSetting = await db.TblSettings.FirstOrDefaultAsync(x => x.Key == EnumDbSettings.TownIds).ConfigureAwait(true);
                var townString = townIdSetting?.Value;
                var townIdsString = townString?.Split(',');

                var townIds = new List<long>();

                if (townIdsString != null)
                {
                    foreach (var townIdString in townIdsString)
                    {
                        if (long.TryParse(townIdString, out var id))
                        {
                            townIds.Add(id);
                        }
                    }
                }

                // DEBUG
                if (!townIds.Any())
                {
                    townIds.Add(15); // Hirschbach
                    townIds.Add(17); // Kirchberg an der Pielach
                    townIds.Add(18); // Maria Enzersdorf
                }

                #endregion

                #region Je Gemeinden

                foreach (var townId in townIds)
                {
                    try
                    {
                        var town = db.TblOrganizations.AsNoTracking()
                            .Include(x => x.TblPermissions)
                            .ThenInclude(x => x.TblUser)
                            .Include(x => x.TblIdeaOrganisations)
                            .ThenInclude(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaLikes)
                            .Include(x => x.TblIdeaOrganisations)
                            .ThenInclude(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaHelpers)
                            .First(x => x.Id == townId);

                        var townItem = new ExWeeklyReportItem
                                       {
                                           TblOrganizationId = town.Id,
                                           TownName = $"{town.PostalCode}, {town.Name}",
                                       };

                        #region Users

                        var townUsers = db.TblUsers.AsNoTracking()
                            .Include(x => x.TblDevices)
                            .Include(x => x.TblPermissions)
                            .Where(x => x.TblPermissions.Any(y => y.TblOrganizationId == townId));

                        townItem.UsersTotal = townUsers.Count();
                        townItem.NewUsers = townUsers.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);
                        townItem.LoggedInUsers = townUsers.Count(x => x.TblDevices.Any(d => d.LastLogin >= startDate && d.LastLogin < endDate));

                        #endregion

                        #region Ideas

                        var townIdeas = db.TblIdeas.AsNoTracking()
                            .Include(x => x.TblIdeaOrganizations)
                            .Where(x => x.TblIdeaOrganizations.Any(y => y.TblOrganizationId == townId));
                        var townIdeaLikes = db.TblIdeaLikes.AsNoTracking()
                            .Include(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaOrganizations)
                            .Where(x => x.TblIdea.TblIdeaOrganizations.Any(y => y.TblOrganizationId == townId));
                        var townIdeaSupports = db.TblIdeaHelpers.AsNoTracking()
                            .Include(x => x.TblIdea)
                            .ThenInclude(x => x.TblIdeaOrganizations)
                            .Where(x => x.TblIdea.TblIdeaOrganizations.Any(y => y.TblOrganizationId == townId));

                        townItem.IdeasTotal = townIdeas.Count();
                        townItem.IdeasNewTime = townIdeas.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                        townItem.IdeaLikesTotal = townIdeaLikes.Count();
                        townItem.IdeaLikesTime = townIdeaLikes.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                        townItem.IdeaSupportsTotal = townIdeaSupports.Count();
                        townItem.IdeaSupportsTime = townIdeaSupports.Count(x => x.CreatedAtUtc >= startDate && x.CreatedAtUtc < endDate);

                        #endregion

                        result.Towns.Add(townItem);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(WeeklyReportHelper)}]({nameof(GetWeeklyReport)}): {e}");
                        result.Towns.Add(new ExWeeklyReportItem
                                         {
                                             TblOrganizationId = townId,
                                             TownName = $"ERROR: {e}",
                                         });
                    }
                }

                #endregion
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(WeeklyReportHelper)}]({nameof(GetWeeklyReport)}): {e}");
            }

            return result;
        }
    }
}