// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;
using System.Threading.Tasks;
using Biss.Dc.Core;
using Biss.Dc.Server;
using Exchange.Model;
using Exchange.Model.FutureWishes;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Model.Report;
using Exchange.Model.Settings;
using Exchange.Model.User;

namespace ConnectivityHost.DataConnector
{
    /// <summary>
    ///     Diese Funktionen müssen am Server implementiert werden
    /// </summary>
    public interface IServerRemoteCalls : IDcCoreRemoteCalls
    {
        #region DcExDeviceInfo

        /// <summary>
        ///     Device fordert Daten für DcExDeviceInfo
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExDeviceInfo> GetDcExDeviceInfo(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExDeviceInfo(long deviceId, long userId, ExDeviceInfo data);

        #endregion


        #region DcExUser

        /// <summary>
        ///     Device fordert Daten für DcExUser
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExUser> GetDcExUser(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExUser(long deviceId, long userId, ExUser data);

        #endregion


        #region DcExUserPassword

        /// <summary>
        ///     Device fordert Daten für DcExUserPassword
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExUserPassword> GetDcExUserPassword(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExUserPassword(long deviceId, long userId, ExUserPassword data);

        #endregion


        #region DcExLocalAppData

        /// <summary>
        ///     Device fordert Daten für DcExLocalAppData
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExLocalAppSettings> GetDcExLocalAppData(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExLocalAppData(long deviceId, long userId, ExLocalAppSettings data);

        #endregion


        #region DcExSettingsInDb

        /// <summary>
        ///     Device fordert Daten für DcExSettingsInDb
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <returns>Daten oder eine Exception auslösen</returns>
        Task<ExSettingsInDb> GetDcExSettingsInDb(long deviceId, long userId);

        /// <summary>
        ///     Device will Daten für DcExNewUserData sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcStoreResult> SetDcExSettingsInDb(long deviceId, long userId, ExSettingsInDb data);

        #endregion


        #region DcExOrganization

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
        [Obsolete]
        Task<List<DcServerListItem<ExOrganization>>> GetDcExOrganization(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

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
        Task<DcListStoreResult> StoreDcExOrganization(long deviceId, long userId, List<DcStoreListItem<ExOrganization>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExOrganization
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExOrganization>> SyncDcExOrganization(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExOrganizationUsers

        /// <summary>
        ///     Device fordert Listen Daten für DcExOrganizationUsers
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
        [Obsolete]
        Task<List<DcServerListItem<ExOrganizationUser>>> GetDcExOrganizationUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExOrganizationUsers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExOrganizationUsers(long deviceId, long userId, List<DcStoreListItem<ExOrganizationUser>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExOrganizationUsers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExOrganizationUser>> SyncDcExOrganizationUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExAllUsers

        /// <summary>
        ///     Device fordert Listen Daten für DcExAllUsers
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
        [Obsolete]
        Task<List<DcServerListItem<ExUser>>> GetDcExAllUsers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExAllUsers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExAllUsers(long deviceId, long userId, List<DcStoreListItem<ExUser>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExAllUsers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExUser>> SyncDcExAllUsers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExFutureWishes

        /// <summary>
        ///     Device fordert Listen Daten für DcExFutureWishes
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
        [Obsolete]
        Task<List<DcServerListItem<ExFutureWish>>> GetDcExFutureWishes(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExFutureWishes sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExFutureWishes(long deviceId, long userId, List<DcStoreListItem<ExFutureWish>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExFutureWishes
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExFutureWish>> SyncDcExFutureWishes(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExIdeas

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeas
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
        [Obsolete]
        Task<List<DcServerListItem<ExIdea>>> GetDcExIdeas(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExIdeas sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExIdeas(long deviceId, long userId, List<DcStoreListItem<ExIdea>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeas
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExIdea>> SyncDcExIdeas(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExIdeaNeeds

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeaNeeds
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
        [Obsolete]
        Task<List<DcServerListItem<ExIdeaNeed>>> GetDcExIdeaNeeds(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExIdeaNeeds sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExIdeaNeeds(long deviceId, long userId, List<DcStoreListItem<ExIdeaNeed>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeaNeeds
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExIdeaNeed>> SyncDcExIdeaNeeds(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExIdeaHelpers

        /// <summary>
        ///     Device fordert Listen Daten für DcExIdeaHelpers
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
        [Obsolete]
        Task<List<DcServerListItem<ExIdeaHelper>>> GetDcExIdeaHelpers(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExIdeaHelpers sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExIdeaHelpers(long deviceId, long userId, List<DcStoreListItem<ExIdeaHelper>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExIdeaHelpers
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExIdeaHelper>> SyncDcExIdeaHelpers(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExReports

        /// <summary>
        ///     Device fordert Listen Daten für DcExReports
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
        [Obsolete]
        Task<List<DcServerListItem<ExReport>>> GetDcExReports(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExReports sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExReports(long deviceId, long userId, List<DcStoreListItem<ExReport>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExReports
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExReport>> SyncDcExReports(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion


        #region DcExIntros

        /// <summary>
        ///     Device fordert Listen Daten für DcExIntros
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
        [Obsolete]
        Task<List<DcServerListItem<ExIntroItem>>> GetDcExIntros(long deviceId, long userId, long startIndex, long elementsToRead, long secondId, string filter);

        /// <summary>
        ///     Device will Listen Daten für DcExIntros sichern
        /// </summary>
        /// <param name="deviceId">Id des Gerätes</param>
        /// <param name="userId">Id des Benutzers oder -1 wenn nicht angemeldet</param>
        /// <param name="data">Eingetliche Daten</param>
        /// <param name="secondId">
        ///     Optionale 2te Id um schnellen Wechsel zwischen Listen zu ermöglichen bzw. dynamische Listen. Zb.
        ///     für Chats
        /// </param>
        /// <returns>Ergebnis (bzw. Infos zum Fehler)</returns>
        Task<DcListStoreResult> StoreDcExIntros(long deviceId, long userId, List<DcStoreListItem<ExIntroItem>> data, long secondId);

        /// <summary>
        ///     Daten Synchronisieren für DcExIntros
        /// </summary>
        /// <param name="deviceId">Gerät</param>
        /// <param name="userId">User Id oder -1 wenn nicht angemeldet</param>
        /// <param name="current">Aktuelle Datensätze am Gerät</param>
        /// <param name="props">Zusätzliche Optionen</param>
        /// <returns>Neuer, aktualisierte und gelöschte Datensätze</returns>
        Task<DcListSyncResultData<ExIntroItem>> SyncDcExIntros(long deviceId, long userId, DcListSyncData current, DcListSyncProperties props);

        #endregion
    }
}