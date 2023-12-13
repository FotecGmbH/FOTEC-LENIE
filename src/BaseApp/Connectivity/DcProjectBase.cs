// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Exchange.Enum;
using Exchange.Model;
using Exchange.Model.Chat;
using Exchange.Model.FutureWishes;
using Exchange.Model.Idea;
using Exchange.Model.Organization;
using Exchange.Model.Report;
using Exchange.Model.Settings;
using Exchange.Model.User;


namespace BaseApp.Connectivity
{
    #region DataType in XAML

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class DcExIdeasType : DcListDataPoint<ExIdea>
    {
        public DcExIdeasType(IDcDataRoot root, ExIdea data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        public DcExIdeasType(ExIdea data) : base(data)
        {
        }
    }

    public class DcIntroItem : DcListDataPoint<ExIntroItem>
    {
        public DcIntroItem(IDcDataRoot root, ExIntroItem data, string pointId, EnumDcDataSource source, long index, EnumDcListElementState state, long sortIdex, byte[] dataVersion) :
            base(root, data, pointId, source, index, state, sortIdex, dataVersion)
        {
        }

        public DcIntroItem(ExIntroItem data) : base(data)
        {
        }
    }

    #region Chat

    public class DcVmUiChat : ExUiChat<ExDcChat, ExDcChatEntry, ExDcChatUser, DcVmUiChatDayEntry, DcVmUiChatEntry>
    {
        public DcVmUiChat(ExDcChat chat, List<ExDcChatEntry> chatEntries, List<ExDcChatUser> chatsUsers) : base(chat, chatEntries, chatsUsers)
        {
        }
    }

    public class DcVmUiChatDayEntry : ExUiChatDayEntry<ExDcChatEntry, ExDcChatUser, DcVmUiChatEntry>
    {
        public DcVmUiChatDayEntry(DateTime day, ObservableCollection<DcVmUiChatEntry> entries) : base(day, entries)
        {
        }
    }

    public class DcVmUiChatEntry : ExUiChatEntry<ExDcChatEntry, ExDcChatUser>
    {
        public DcVmUiChatEntry(ExDcChatEntry entry, ExDcChatUser user) : base(entry, user)
        {
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    #endregion

    #endregion


    /// <summary>
    ///     <para>Datenconnector für das aktuelle Projekt</para>
    ///     Klasse DcProjectBase. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class DcProjectBase : DcDataRootChat<
        ExDcChat,
        ExDcChatEntry,
        ExDcChatUser,
        DcVmUiChat,
        DcVmUiChatDayEntry,
        DcVmUiChatEntry>
    {
        #region Properties

        /// <summary>
        ///     Infos vom Gerät, werden nur an den Server gesendet beim Start der App
        /// </summary>
        public DcDataPoint<ExDeviceInfo> DcExDeviceInfo { get; set; } = new DcDataPoint<ExDeviceInfo>(cacheDataPoint: false, takeDefaultInstance: true);

        /// <summary>
        ///     Benutzerinfos
        /// </summary>
        public DcDataPoint<ExUser> DcExUser { get; set; } = new DcDataPoint<ExUser>(EnumDcPointBehavior.LoadWhenNeeded, true, true);

        /// <summary>
        ///     Benutzerpasswort (bestehender User) ändern
        /// </summary>
        public DcDataPoint<ExUserPassword> DcExUserPassword { get; set; } = new DcDataPoint<ExUserPassword>();

        /// <summary>
        ///     Lokale App Daten
        /// </summary>
        public DcDataPoint<ExLocalAppSettings> DcExLocalAppData { get; set; } = new DcDataPoint<ExLocalAppSettings>(EnumDcPointBehavior.LocalOnly, takeDefaultInstance: true);

        /// <summary>
        ///     Einstellungen in der DB für Update Check, Allgemeine Meldung, ...
        /// </summary>
        public DcDataPoint<ExSettingsInDb> DcExSettingsInDb { get; set; } = new DcDataPoint<ExSettingsInDb>(EnumDcPointBehavior.LoadWhenNeeded, true, true, cacheDataPoint: false);

        /// <summary>
        ///     Firma, Gemeinde, Abteilung ...
        /// </summary>
        public DcDataList<DcListDataPoint<ExOrganization>, ExOrganization> DcExOrganization { get; } = new DcDataList<DcListDataPoint<ExOrganization>, ExOrganization>();

        /// <summary>
        ///     Benutzer der Firma, Gemeinde, Abteilung ...
        /// </summary>
        public DcDataList<DcListDataPoint<ExOrganizationUser>, ExOrganizationUser> DcExOrganizationUsers { get; } = new DcDataList<DcListDataPoint<ExOrganizationUser>, ExOrganizationUser>(loadFromCacheInFilterList: true);

        /// <summary>
        ///     Alle Benutzer im System (für Sysadmin)
        /// </summary>
        public DcDataList<DcListDataPoint<ExUser>, ExUser> DcExAllUsers { get; } = new DcDataList<DcListDataPoint<ExUser>, ExUser>(loadFromCacheInFilterList: true);

        /// <summary>
        ///     Zukünftige Features
        /// </summary>
        public DcDataList<DcListDataPoint<ExFutureWish>, ExFutureWish> DcExFutureWishes { get; } = new DcDataList<DcListDataPoint<ExFutureWish>, ExFutureWish>(false);

        /// <summary>
        ///     Ideen Im Cache alle meiner Region, können beim Laden gefiltert werden, im UI dann filterung auf alle, nur meine, wo
        ///     ich helfe
        /// </summary>
        public DcDataList<DcExIdeasType, ExIdea> DcExIdeas { get; } = new DcDataList<DcExIdeasType, ExIdea>(loadFromCacheInFilterList: true);

        /// <summary>
        ///     benötigte Sachen für Ideen, können beim Laden auf Idee gefiltert werden
        /// </summary>
        public DcDataList<DcListDataPoint<ExIdeaNeed>, ExIdeaNeed> DcExIdeaNeeds { get; } = new DcDataList<DcListDataPoint<ExIdeaNeed>, ExIdeaNeed>(false);

        /// <summary>
        ///     Helfer für Ideen, können beim Laden gefiltert werden
        /// </summary>
        public DcDataList<DcListDataPoint<ExIdeaHelper>, ExIdeaHelper> DcExIdeaHelpers { get; } = new DcDataList<DcListDataPoint<ExIdeaHelper>, ExIdeaHelper>(false);

        /// <summary>
        ///     Alle Meldungen von Ideen durch User, können beim Laden auf User und/oder Idee gefiltert werden
        /// </summary>
        public DcDataList<DcListDataPoint<ExReport>, ExReport> DcExReports { get; } = new DcDataList<DcListDataPoint<ExReport>, ExReport>();

        /// <summary>
        ///     Daten für Intro Bildschirm
        /// </summary>
        public DcDataList<DcIntroItem, ExIntroItem> DcExIntros { get; set; } = new DcDataList<DcIntroItem, ExIntroItem>();

        #endregion

        #region DcCommonCommands

        /// <summary>
        ///     Datei in der Datenbank und im Azure BlobStorage löschen
        /// </summary>
        /// <param name="fileId">Id der Datei aus der Datenbank</param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> DeleteFile(long fileId)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.FileDelete.ToString(),
                                      Value = fileId.ToString()
                                  });
        }

        /// <summary>
        ///     Bestätigungs E-Mail erneut senden
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> ResendAccessEMail(long userId)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.ResendAccessEMail.ToString(),
                                      Value = userId.ToString()
                                  });
        }

        /// <summary>
        ///     Bestätigungs SMS erneut senden
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> ResendConfirmationSms(long userId)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.ResendConfirmationSms.ToString(),
                                      Value = userId.ToString()
                                  });
        }

        /// <summary>
        ///     Bestätigungs SMS
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> ConfirmSmsCode(string code)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.ConfirmSms.ToString(),
                                      Value = code
                                  });
        }

        /// <summary>
        ///     Passwort zurücksetzen starten
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> ResetPassword(long userId)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.ResetPassword.ToString(),
                                      Value = userId.ToString()
                                  });
        }

        /// <summary>
        ///     Allgemeinen (Warn)hinweis aktualisieren
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> UpdateCommonMessage(string msg)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.SendCommonMsg.ToString(),
                                      Value = msg
                                  });
        }

        /// <summary>
        ///     Idee in DB liken/disliken
        /// </summary>
        /// <param name="ideaId">Id der Idee aus der Datenbank</param>
        /// <returns></returns>
        public Task<DcCommonCommandResult> LikeIdea(long ideaId)
        {
            return SendCommonData(new DcCommonData
                                  {
                                      Key = EnumDcCommonCommands.LikeIdea.ToString(),
                                      Value = ideaId.ToString()
                                  });
        }

        #endregion
    }
}