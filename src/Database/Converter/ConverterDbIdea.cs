// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using Biss.Apps.Map.Model;
using Database.Tables;
using Exchange.Model.Idea;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Db Convert</para>
    ///     Klasse ConverterDbIdea. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbIdea
    {
        /// <summary>
        ///     Umwandeln in ExIdea
        /// </summary>
        /// <param name="dbItem"></param>
        /// <param name="userId">optional aktuelle UserId</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ExIdea ToExIdea(this TableIdea dbItem, long? userId = null)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var exItem = new ExIdea
                         {
                             Title = dbItem.Title,
                             Description = dbItem.Description,
                             CreatedAtUtc = dbItem.CreatedAtUtc,
                             CreatorUserId = dbItem.TblUserId,
                             HasTimespan = dbItem.From.HasValue && dbItem.To.HasValue,
                             From = dbItem.From ?? DateTime.UtcNow,
                             To = dbItem.To ?? DateTime.UtcNow,
                             HasPosition = (dbItem.LocationLat != null && dbItem.LocationLon != null) ||
                                           !string.IsNullOrWhiteSpace(dbItem.LocationAddress),
                             Location = new BissPosition(dbItem.LocationLat ?? 0, dbItem.LocationLon ?? 0),
                             LocationAddress = dbItem.LocationAddress ?? string.Empty,
                             PrivateChatId = null,
                             IdeaImageId = dbItem.TblIdeaImageId,
                             Archived = dbItem.Archived,
                         };

            if (dbItem.TblIdeaImage != null)
            {
                exItem.IdeaImage = dbItem.TblIdeaImage.PublicLink;
            }

            if (dbItem.TblIdeaOrganizations != null!)
            {
                foreach (var org in dbItem.TblIdeaOrganizations)
                {
                    if (org.TblOrganization != null!)
                    {
                        exItem.Companies.Add(org.TblOrganization.ToExExOrganization());
                    }
                }
            }

            if (dbItem.TblChat != null!)
            {
                foreach (var chat in dbItem.TblChat)
                {
                    if (chat.PublicChat)
                    {
                        exItem.PublicChatId = chat.Id;
                    }
                    else if (userId != null && // aktueller user gesetzt
                             userId != exItem.CreatorUserId && // aktueller user ist nicht ideen ersteller
                             exItem.PrivateChatId == null && // noch kein private chat gesetzt
                             !chat.PublicChat && // privater chat
                             chat.TblChatUsers != null! && // chatusers geladen
                             chat.TblChatUsers.Any(x => x.TblUserId == userId) && // aktueller User ist im chat dabei
                             chat.TblChatUsers.Any(x => x.TblUserId == exItem.CreatorUserId)) // ersteller ist im chat dabei
                    {
                        exItem.PrivateChatId = chat.Id;
                    }
                }
            }

            if (dbItem.TblIdeaLikes != null!)
            {
                exItem.LikeCount = dbItem.TblIdeaLikes.Count;
            }

            if (dbItem.TblIdeaReports != null!)
            {
                exItem.ReportCount = dbItem.TblIdeaReports.Count;
            }

            if (userId != null)
            {
                exItem.IsMine = dbItem.TblUserId == userId;
                exItem.IsHelping = dbItem.TblIdeaHelpers != null! && dbItem.TblIdeaHelpers.Any(x => x.TblUserId == userId);
                exItem.IsLiked = dbItem.TblIdeaLikes != null! && dbItem.TblIdeaLikes.Any(x => x.TblUserId == userId);
                exItem.IsReported = dbItem.TblIdeaReports != null! && dbItem.TblIdeaReports.Any(x => x.TblUserId == userId);
            }

            return exItem;
        }

        /// <summary>
        ///     Umwandeln für DB
        /// </summary>
        /// <param name="exItem"></param>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ToTableIdea(this ExIdea exItem, TableIdea dbItem)
        {
            if (exItem == null)
            {
                throw new ArgumentNullException(nameof(exItem));
            }

            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            dbItem.Title = exItem.Title;
            dbItem.Description = exItem.Description;

            dbItem.From = exItem.HasTimespan ? exItem.From : null;
            dbItem.To = exItem.HasTimespan ? exItem.To : null;

            dbItem.LocationLat = exItem.HasPosition ? exItem.Location.Latitude : null;
            dbItem.LocationLon = exItem.HasPosition ? exItem.Location.Longitude : null;
            dbItem.LocationAddress = exItem.HasPosition ? exItem.LocationAddress : null;

            dbItem.TblIdeaImageId = exItem.IdeaImageId;

            dbItem.CheckArchived();
        }

        /// <summary>
        ///     Check, ob die Idee archiviert sein sollte
        /// </summary>
        /// <param name="dbItem"></param>
        /// <returns></returns>
        public static bool CheckArchived(this TableIdea dbItem)
        {
            if (dbItem == null)
            {
                throw new ArgumentNullException(nameof(dbItem));
            }

            var checkDate = dbItem.To ?? dbItem.CreatedAtUtc.AddDays(90);

            dbItem.Archived = checkDate < DateTime.Today;

            return dbItem.Archived;
        }
    }
}