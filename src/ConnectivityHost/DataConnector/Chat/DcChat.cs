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
using Biss.Dc.Core.DcChat;
using Biss.Dc.Server.DcChat;
using Biss.Log.Producer;
using ConnectivityHost.Helper;
using Database;
using Database.Tables;
using Exchange.Model.Chat;
using Exchange.Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.DataConnector.Chat
{
    /// <summary>
    ///     <para>DESCRIPTION</para>
    ///     Klasse DcChat. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
#pragma warning disable CA1724
    public class DcChat : DcChatServerBase
#pragma warning restore CA1724
    {
        /// <summary>
        ///     Eigenen User aus Datenban lesen
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public override async Task<IDcChatUser> GetUser(long userId)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
            var user = db.TblUsers.AsNoTracking()
                .Where(w => w.Id == userId)
                .Include(i => i.TblUserImage)
                .Select(s => (IDcChatUser) new ExDcChatUser
                                           {
                                               FullName = $"{s.FirstName} {s.LastName}",
                                               ImageLink = s.TblUserImage == null ? string.Empty : s.TblUserImage.PublicLink,
                                               Id = s.Id,
                                               DataVersion = s.DataVersion
                                           }).First();
            return user;
        }

        /// <summary>
        ///     Alle User mit welcher "userId" einen Chat hat oder mit welchem ein Chat möglich wäre
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="currentUsers">Liste der aktuellen Benutzer auf dem Gerät</param>
        /// <param name="_">Sync Mode</param>
        /// <returns></returns>
        public override async Task<(List<IDcChatUser>, List<long>)> GetChatUsers(long userId, List<DcSyncElement> currentUsers, DcEnumChatSyncMode _)
        {
            Logging.Log.LogTrace($"[DcChat]({nameof(GetChatUsers)}): " +
                                 $"Request {userId} -> {string.Join(", ", currentUsers.Select(x => x.Id))}");


#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            //Alle Id's für den aktuellen User
            List<DcSyncElement> chatUsersDb;
            var isAdmin = db.IsUserSysAdmin(userId) || true; // Immer alle User schicken
            if (isAdmin)
            {
                // Alle User
                chatUsersDb = db.TblUsers.AsNoTracking()
                    .Select(s => new DcSyncElement
                                 {
                                     DataVersion = s.DataVersion,
                                     Id = s.Id
                                 }).ToList();
            }
            else
            {
                var userOrgs = db.TblPermissions
                    .Where(w => w.TblUserId == userId)
                    .Select(s => s.TblOrganizationId).ToList();

                // Alle User meiner Gemeinden
                chatUsersDb = db.TblPermissions.AsNoTracking()
                    .Include(x => x.TblUser)
                    .Where(w => (userOrgs.Contains(w.TblOrganizationId)))
                    .Select(s => new DcSyncElement
                                 {
                                     DataVersion = s.TblUser.DataVersion,
                                     Id = s.TblUser.Id,
                                 }).Distinct().ToList();

                // Alle Admins
                chatUsersDb.AddRange(db.TblUsers.AsNoTracking()
                    .Where(x => x.IsAdmin)
                    .Select(s => new DcSyncElement
                                 {
                                     DataVersion = s.DataVersion,
                                     Id = s.Id
                                 }));

                chatUsersDb = chatUsersDb.Distinct().ToList();
            }

            //Gelöschte User
            var deletedUser = currentUsers
                .Select(s => s.Id)
                .Except(chatUsersDb.Select(s => s.Id)).ToList();

            //Prüfen ob sich Datensätze geändert haben
            if (currentUsers != null! && currentUsers.Count > 0)
            {
                foreach (var user in currentUsers)
                {
                    if (chatUsersDb.Any(f => f == user))
                    {
                        chatUsersDb.Remove(chatUsersDb.First(f => f == user));
                    }
                }
            }

            List<IDcChatUser> users;
            if (chatUsersDb.Count > 0)
            {
                var itemIds = chatUsersDb.Select(s => s.Id).ToList();

                //var tmpusers = db.TblUsers.AsNoTracking().Where(w => itemIds.Contains(w.Id)).Include(i=>i.TblUserImage).ToList();
                //var tmpf = tmpusers.First();
                //var FullName = $"{tmpf.FirstName} {tmpf.LastName}";
                //var ImageLink = tmpf.TblUserImage == null ? string.Empty : tmpf.TblUserImage.PublicLink;
                //var Id = tmpf.Id;
                //var DataVersion = tmpf.DataVersion;

                users = db.TblUsers.AsNoTracking()
                    .Where(w => itemIds.Contains(w.Id))
                    .Include(i => i.TblUserImage)
                    .Select(s => (IDcChatUser) new ExDcChatUser
                                               {
                                                   FullName = $"{s.FirstName} {s.LastName}",
                                                   ImageLink = s.TblUserImage == null ? string.Empty : s.TblUserImage.PublicLink,
                                                   Id = s.Id,
                                                   DataVersion = s.DataVersion
                                               }).ToList();
            }
            else
            {
                users = new List<IDcChatUser>();
            }

            Logging.Log.LogTrace($"[DcChat]({nameof(GetChatUsers)}): " +
                                 $"Return {userId} -> {string.Join(", ", users.Select(x => x.Id))} - {string.Join(", ", deletedUser)}");

            return (users, deletedUser);
        }

        /// <inheritdoc />
        public override async Task<(List<IDcChat> chats, List<long> removeChats)> GetChats(long userId, List<DcSyncElement> currentChats, DcEnumChatSyncMode _)
        {
            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): " +
                                 $"Request: {userId} -> {string.Join(", ", currentChats.Select(x => x.Id))}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var userRegions = db.GetUserOrganizationsForUser(userId).Select(x => x.Id).ToList();

            // Ideen für den User
            var ideasUser = db.TblIdeas
                .Include(x => x.TblIdeaOrganizations)
                .Where(x => x.TblIdeaOrganizations.Any(z => userRegions.Contains(z.TblOrganizationId)))
                .Select(x => x.Id).Distinct().ToList();

            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): Ideas for User: {string.Join(",", ideasUser)}");

            var dvEmpty = new byte[db.TblChat.FirstOrDefault()?.DataVersion.Length ?? 16];
            for (var i = 0; i < dvEmpty.Length; i++)
            {
                dvEmpty[i] = 0;
            }

            //Alle Chats für den aktuellen User oder Public
            var chatsDb = db.TblChat.AsNoTracking()
                .Include(x => x.TblChatUsers)
                .Include(x => x.TblChatEntries)
                .Where(w => (w.PublicChat && w.TblIdeaId != null &&
                             ideasUser.Contains(w.TblIdeaId.Value)) ||
                            w.TblChatUsers.Any(a => a.TblUserId == userId))
                .ToList()
                .Select(s => new DcSyncElement(s.Id, new List<byte[]>
                                                     {
                                                         s.DataVersion,
                                                         (s.TblChatEntries.Any()
                                                             ? new DcSyncElement(s.Id, s.TblChatEntries.Select(x => x.DataVersion).ToList()).DataVersion
                                                             : dvEmpty)
                                                     })).ToList();

            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): Chats for User: {string.Join(",", chatsDb.Select(x => x.Id))}");

            //Gelöschte Chats
            var deletedChats = currentChats.Select(s => s.Id).Except(chatsDb.Select(s => s.Id)).ToList();
            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): Chats to remove for User: {string.Join(",", deletedChats)}");

            //Prüfen ob sich Datensätze geändert haben
            if (currentChats != null! && currentChats.Count > 0)
            {
                foreach (var c in currentChats)
                {
                    var chatItem = chatsDb.FirstOrDefault(f => f.Id == c.Id);

                    Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): Check {c.Id} -> {chatItem?.Id}");

                    if (chatItem! != null! && chatItem == c)
                    {
                        chatsDb.Remove(chatItem);
                    }
                }
            }

            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): Chats to add/update for User: {string.Join(",", chatsDb.Select(x => x.Id))}");

            List<IDcChat> chats;
            if (chatsDb.Count > 0)
            {
                var itemIds = chatsDb.Select(s => s.Id).ToList();
                chats = db.TblChat.AsNoTracking()
                    .Where(w => itemIds.Contains(w.Id))
                    .Include(i => i.TblChatUsers)
                    .Include(x => x.TblIdea)
                    .Include(x => x.TblChatEntries)
                    .ToList()
                    .Select(s => (IDcChat) new ExDcChat
                                           {
                                               Id = s.Id,
                                               DataVersion = new DcSyncElement(-1, new List<byte[]>
                                                                                   {
                                                                                       s.DataVersion,
                                                                                       (s.TblChatEntries.Any()
                                                                                           ? new DcSyncElement(s.Id, s.TblChatEntries.Select(x => x.DataVersion).ToList()).DataVersion
                                                                                           : dvEmpty)
                                                                                   }).DataVersion,
                                               ChatName = s.ChatName,
                                               IdeaId = s.TblIdeaId,
                                               IsActive = true,
                                               IsGroup = s.PublicChat,
                                               ChatUsers = s.TblChatUsers.Select(su => su.TblUserId).ToList(),
                                               LatestMessageDate = s.TblChatEntries.Any()
                                                   ? s.TblChatEntries.MaxBy(x => x.EntryDateTimeUtc)!.EntryDateTimeUtc
                                                   : s.TblIdea != null!
                                                       ? s.TblIdea.CreatedAtUtc
                                                       : DateTime.MinValue,
                                               LatestMessageText = s.TblChatEntries.Any()
                                                   ? s.TblChatEntries.MaxBy(x => x.EntryDateTimeUtc)!.Text
                                                   : string.Empty,
                                           }).ToList();
            }
            else
            {
                chats = new List<IDcChat>();
            }


            Logging.Log.LogTrace($"[DcChat]({nameof(GetChats)}): " +
                                 $"Result: {userId} -> {string.Join(", ", chats.Select(x => x.Id))} - {string.Join(", ", deletedChats)}");

            return (chats, deletedChats);
        }

        /// <inheritdoc />
        public override async Task<(Dictionary<long, List<IDcChatEntry>> chatEntries, List<long> removeChatEntries)> GetChatEntries(long userId, Dictionary<long, List<DcSyncElement>> currentChatEntries, DcEnumChatSyncMode _)
        {
            Logging.Log.LogTrace($"[DcChat]({nameof(GetChatEntries)}): " +
                                 $"Request: {userId} -> {string.Join(", ", currentChatEntries.Select(x => x.Key + "-" + (string.Join(",", x.Value.Select(y => y.Id)))))}");

            if (currentChatEntries == null!)
            {
                throw new ArgumentNullException($"[DcChat]({nameof(GetChatEntries)}): {nameof(currentChatEntries)}");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            //Alle Chats für den aktuellen User oder Public
            var chatsDb = db.TblChatUser.AsNoTracking()
                .Include(x => x.TblChat)
                .Where(w => w.TblUserId == userId || w.TblChat.PublicChat)
                .Select(s => s.TblChatId).Distinct().ToList();
            var chatEntries = new Dictionary<long, List<IDcChatEntry>>();
            var chatEntriesDelete = new List<long>();

            foreach (var dbChatId in chatsDb)
            {
                try
                {
                    if (!currentChatEntries.TryGetValue(dbChatId, out var _))
                    {
                        var dbEntries = db.TblChatEntry.AsNoTracking()
                            .Where(e => e.TblChatId == dbChatId)
                            .Select(s => (IDcChatEntry) new ExDcChatEntry
                                                        {
                                                            DataVersion = s.DataVersion,
                                                            ChatId = dbChatId,
                                                            Id = s.Id,
                                                            Message = s.Text,
                                                            TimeStampUtc = s.EntryDateTimeUtc,
                                                            UserId = s.TblUserWriterId!.Value
                                                        }).ToList();
                        chatEntries.Add(dbChatId, dbEntries);
                    }
                    else
                    {
                        var entriesDb = db.TblChatEntry
                            .Where(w => w.TblChatId == dbChatId)
                            .Select(s => new DcSyncElement(s.Id, s.DataVersion))
                            .ToList();

                        //Gelöschte Chat-Einträge
                        var deletedChats = currentChatEntries[dbChatId]
                            .Select(s => s.Id)
                            .Except(entriesDb.Select(s => s.Id)).ToList();
                        chatEntriesDelete.AddRange(deletedChats);

                        //Prüfen ob sich Datensätze geändert haben
                        if (currentChatEntries[dbChatId] != null! && currentChatEntries[dbChatId].Count > 0)
                        {
                            foreach (var c in currentChatEntries[dbChatId])
                            {
                                var check = entriesDb.FirstOrDefault(x => x.Id == c.Id);

                                if (check! != null! && check == c)
                                {
                                    entriesDb.Remove(check);
                                }
                            }
                        }

                        List<IDcChatEntry> chatentries;
                        if (entriesDb.Count > 0)
                        {
                            var itemIds = entriesDb
                                .Select(s => s.Id)
                                .ToList();
                            chatentries = db.TblChatEntry.AsNoTracking()
                                .Where(w => itemIds.Contains(w.Id))
                                .Select(s => (IDcChatEntry) new ExDcChatEntry
                                                            {
                                                                DataVersion = s.DataVersion,
                                                                ChatId = dbChatId,
                                                                Id = s.Id,
                                                                Message = s.Text,
                                                                TimeStampUtc = s.EntryDateTimeUtc,
                                                                UserId = s.TblUserWriterId!.Value
                                                            }).ToList();
                        }
                        else
                        {
                            chatentries = new List<IDcChatEntry>();
                        }

                        chatEntries.Add(dbChatId, chatentries);
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[DcChat]({nameof(GetChatEntries)}): Chat:{dbChatId} - {e}");
                    throw;
                }
            }

            Logging.Log.LogTrace($"[DcChat]({nameof(GetChatEntries)}): " +
                                 $"Result: {userId} -> " +
                                 $"{string.Join(", ", chatEntries.Select(x => x.Key + "-" + (string.Join(",", x.Value.Select(y => y.Id)))))} - " +
                                 $"{string.Join(", ", chatEntriesDelete)}");


            return (chatEntries, chatEntriesDelete);
        }

        /// <inheritdoc />
        public override async Task<DcChatData> Post(long deviceId, long userId, string msg, long? chatId = null, long? otherUserId = null, string chatName = "", string addData = "")
        {
            Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): " +
                                 $"Request: {deviceId}, {userId}, '{msg}', {chatId}, {otherUserId}, {chatName}, {addData}");

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.First(f => f.Id == userId);
            var newChatEntry = new TableChatEntry
                               {
                                   EntryDateTimeUtc = DateTime.UtcNow,
                                   Text = msg,
                                   TblUserWriterId = userId
                               };
            TableChat? newChat = null;
            long currentChatId = 0;
            var noMessage = false;

            long? ideaId = null;
            if (long.TryParse(addData, out var parseId))
            {
                ideaId = parseId;
            }

            //Neuer Public Chat
            if (chatId == null && otherUserId == null && !string.IsNullOrEmpty(chatName))
            {
                Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): Create Public Chat {chatName} ({ideaId})");

                var idea = db.TblIdeas.FirstOrDefault(x => x.Id == ideaId);
                var publicName = idea == null
                    ? chatName
                    : (idea.Title) +
                      " (Gruppenchat)";

                var existingChat = db.TblChat
                    .Where(x => x.PublicChat && x.TblIdeaId == ideaId);
                if (existingChat.Any())
                {
                    // Chat schon vorhanden, diesen Chat zurückliefern und nix neues anlegen!
                    currentChatId = existingChat.First().Id;
                    noMessage = true;
                }
                else
                {
                    newChat = new TableChat
                              {
                                  ChatName = publicName,
                                  TblIdeaId = ideaId,
                                  PublicChat = true,
                                  TblChatEntries = new List<TableChatEntry>
                                                   {
                                                       newChatEntry
                                                   }
                              };
                    newChat.TblChatUsers = new List<TableChatUsers>
                                           {
                                               new()
                                               {
                                                   TblUser = user,
                                                   TblChat = newChat
                                               }
                                           };
                    db.TblChat.Add(newChat);
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
            }
            //Neuer Chat mit einer anderen Person
            else if (chatId == null && otherUserId != null)
            {
                Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): Create Private Chat {user.Id} <=> {otherUserId} ({ideaId})");

                var other = db.TblUsers.First(f => f.Id == otherUserId);

                var idea = db.TblIdeas.FirstOrDefault(x => x.Id == ideaId);
                var nonCreatorId = idea == null
                    ? -1
                    : userId != idea.TblUserId
                        ? userId
                        : otherUserId != idea.TblUserId
                            ? otherUserId
                            : -1;
                var nonCreator = db.TblUsers.FirstOrDefault(f => f.Id == nonCreatorId);

                var privateName = (idea?.Title ?? "Keine Idee") +
                                  " (Chat " +
                                  (nonCreator != null ? $"{nonCreator.FirstName} {nonCreator.LastName}" : $"User[{nonCreatorId}]") +
                                  ")";

                var existingChat = db.TblChat
                    .Include(x => x.TblChatUsers)
                    .Where(x => !x.PublicChat && x.TblIdeaId == ideaId);
                if (existingChat.Any() &&
                    existingChat.Any(x =>
                        x.TblChatUsers.Any(y => y.TblUserId == userId) &&
                        x.TblChatUsers.Any(y => y.TblUserId == otherUserId)))
                {
                    // Chat schon vorhanden, diesen Chat zurückliefern und nix neues anlegen!
                    currentChatId = existingChat.First(x =>
                            x.TblChatUsers.Any(y => y.TblUserId == userId) &&
                            x.TblChatUsers.Any(y => y.TblUserId == otherUserId))
                        .Id;
                    noMessage = true;
                }
                else
                {
                    newChat = new TableChat
                              {
                                  ChatName = privateName,
                                  TblIdeaId = ideaId,
                                  PublicChat = false,
                                  TblChatEntries = new List<TableChatEntry>
                                                   {
                                                       newChatEntry
                                                   },
                              };
                    newChat.TblChatUsers = new List<TableChatUsers>
                                           {
                                               new()
                                               {
                                                   TblUser = user,
                                                   TblChat = newChat
                                               },
                                               new()
                                               {
                                                   TblUser = other,
                                                   TblChat = newChat
                                               }
                                           };
                    db.TblChat.Add(newChat);
                    await db.SaveChangesAsync().ConfigureAwait(true);
                }
            }
            else if (chatId.HasValue)
            {
                Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): New Message in Chat {chatId}");

                newChatEntry.TblChatId = chatId;
                db.TblChatEntry.Add(newChatEntry);
                await db.SaveChangesAsync().ConfigureAwait(true);

                currentChatId = chatId.Value;
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (newChat != null)
            {
                currentChatId = newChat.Id;
            }

            if (currentChatId <= 0)
            {
                throw new InvalidOperationException();
            }

            //Prüfen ob die aktuelle Person berits im Chat ist (sonst diese Hinzufügen)
            if (!db.TblChatUser.Any(a => a.TblUserId == userId && a.TblChatId == currentChatId))
            {
                Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): Add User {userId} -> {currentChatId}");

                db.TblChatUser.Add(new TableChatUsers
                                   {
                                       TblChatId = currentChatId,
                                       TblUserId = userId
                                   });
                await db.SaveChangesAsync().ConfigureAwait(true);
            }

            var r = new DcChatData();
            if (newChat != null)
            {
                var u = db.TblChatUser.AsNoTracking()
                    .Where(c => c.TblChatId == currentChatId)
                    .Select(s => s.TblUserId).ToList();
                var chats = new List<IDcChat>
                            {
                                new ExDcChat
                                {
                                    Id = currentChatId,
                                    DataVersion = newChat.DataVersion,
                                    IdeaId = newChat.TblIdeaId,
                                    ChatName = newChat.ChatName,
                                    IsActive = true,
                                    IsGroup = newChat.PublicChat,
                                    ChatUsers = u,
                                    UnreadMessages = 0,
                                    LatestMessageDate = newChatEntry.EntryDateTimeUtc,
                                    LatestMessageText = newChatEntry.Text,
                                }
                            };
                r.Chats = chats;
            }

            var chatentries = new List<IDcChatEntry>();

            if (noMessage)
            {
                // bestehende Chats laden?
            }
            else
            {
                chatentries.Add(new ExDcChatEntry
                                {
                                    ChatId = currentChatId,
                                    DataVersion = newChatEntry.DataVersion,
                                    Id = newChatEntry.Id,
                                    Message = newChatEntry.Text,
                                    TimeStampUtc = newChatEntry.EntryDateTimeUtc,
                                    UserId = newChatEntry.TblUserWriterId.Value
                                });
            }

            var dic = new Dictionary<long, List<IDcChatEntry>> {{currentChatId, chatentries}};
            r.Entries = dic;

            var userIds = db.TblChatUser.AsNoTracking()
                .Where(c => c.TblChatId == currentChatId)
                .Select(s => s.TblUserId).ToList();
            var users = db.TblUsers.AsNoTracking()
                .Where(w => userIds.Contains(w.Id))
                .Include(i => i.TblUserImage)
                .Select(s => (IDcChatUser) new ExDcChatUser
                                           {
                                               FullName = $"{s.FirstName} {s.LastName}",
                                               ImageLink = s.TblUserImage == null ? string.Empty : s.TblUserImage.PublicLink,
                                               Id = s.Id,
                                               DataVersion = s.DataVersion
                                           }).ToList();
            r.Users = users;

            if (!noMessage)
            {
                _ = Task.Run(async () =>
                {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                    await using var db2 = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                    var devicesToNotify = db2.TblDevices
                        .Include(x => x.TblUser)
                        .AsNoTracking()
                        .Where(x =>
                            x.TblUserId != null &&
                            x.TblUserId != userId &&
                            userIds.Contains(x.TblUserId.Value) &&
                            x.TblUser!.NotificationPushChat &&
                            !string.IsNullOrWhiteSpace(x.DeviceToken))
                        .Select(x => x.DeviceToken)
                        .ToList();
                    var chat = db2.TblChat.FirstOrDefault(x => x.Id == currentChatId);

                    var title = string.Format(ResWebCommon.PushChatTitle, chat?.ChatName);
                    var message = newChatEntry.Text;

                    var res = await PushHelper.SendChatMessage(devicesToNotify, currentChatId, message, title).ConfigureAwait(true);

                    if (res == null! || res.FailureCount > 0)
                    {
                        Logging.Log.LogWarning($"[DcChat]({nameof(Post)}): Fehler beim Senden der Notifications");
                    }
                });
            }

            Logging.Log.LogTrace($"[DcChat]({nameof(Post)}): " +
                                 $"Result: {r}");

            return r;
        }
    }
}