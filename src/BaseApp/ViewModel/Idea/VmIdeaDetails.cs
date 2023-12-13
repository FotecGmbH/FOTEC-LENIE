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
using BaseApp.Connectivity;
using BaseApp.ViewModel.Chat;
using BaseApp.ViewModel.Reports;
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Interfaces;
using Biss.Apps.Map.Base;
using Biss.Apps.Map.Component;
using Biss.Apps.Map.Helper;
using Biss.Apps.Map.Model;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Model.Report;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace BaseApp.ViewModel.Idea
{
    /// <summary>
    ///     <para>Details zu einer einzelnen Idee</para>
    ///     Klasse VmIdeaDetails. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewIdeaDetails")]
    public class VmIdeaDetails : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmIdeaDetails.DesignInstance}"
        /// </summary>
        public static VmIdeaDetails DesignInstance = new VmIdeaDetails();

        /// <summary>
        ///     VmIdeaDetails
        /// </summary>
        public VmIdeaDetails() : base("Details zur Idee")
        {
            SetViewProperties(true);
            View.ShowUser = false;
            View.BusySet("Lade ...", 100);
        }

        #region Properties

        /// <summary>
        ///     Aktuelle Idee
        /// </summary>
        public ExIdea Data { get; set; } = null!;

        /// <summary>
        ///     Aktuelle Idee
        /// </summary>
        public DcExIdeasType DcListDataPoint { get; set; } = null!;

        /// <summary>
        ///     Helfer bei dieser Idee
        /// </summary>
        public List<DcListDataPoint<ExIdeaHelper>> UiHelpers { get; set; } = new List<DcListDataPoint<ExIdeaHelper>>();

        /// <summary>
        ///     benötigte Sachen dieser Idee
        /// </summary>
        public List<DcListDataPoint<ExIdeaNeed>> UiNeeds { get; set; } = new List<DcListDataPoint<ExIdeaNeed>>();

        /// <summary>
        ///     Karte
        /// </summary>
        public BissMap Map => this.BcBissMap()!.BissMap;

        #endregion


        private async void UpdateNeedUiList()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            await Dc.DcExIdeaNeeds.WaitDataFromServerAsync(filter: new ExIdeaNeedFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete

            var need = new List<DcListDataPoint<ExIdeaNeed>>();
            foreach (var dataPoint in Dc.DcExIdeaNeeds.Where(x => x.Data.IdeaId == DcListDataPoint.Index))
            {
                need.Add(dataPoint);
            }

            UiNeeds = need;
        }

        private async void UpdateHelperUiList()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            await Dc.DcExIdeaHelpers.WaitDataFromServerAsync(filter: new ExIdeaHelperFilter {IdeaId = DcListDataPoint.Index}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete

            var helpers = new List<DcListDataPoint<ExIdeaHelper>>();
            foreach (var dataPoint in Dc.DcExIdeaHelpers.Where(x => x.Data.IdeaId == DcListDataPoint.Index))
            {
                helpers.Add(dataPoint);
            }

            UiHelpers = helpers;
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            CheckPhoneConfirmation().ConfigureAwait(true);

            Dc.DcExIdeas.FilterClear();

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            if (args is DcExIdeasType idea)
            {
                DcListDataPoint = idea;
                Data = idea.Data;
                PageSubTitle = idea.Data.Title;

                UpdateHelperUiList();
                UpdateNeedUiList();

                Map.MapItems.Add(new BmPoint(idea.Data.Title)
                                 {
                                     Position = idea.Data.Location,
                                 });
                Map.SetCenterAndZoom(idea.Data.Location, BmDistance.FromKilometers(1));
            }
            else
            {
                throw new Exception("Invalid Type or NULL!");
            }

            return base.OnActivated(args);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            CmdLike.DisplayName = Data.IsLiked ? ResViewIdeaDetails.LblIsLikedTrue : ResViewIdeaDetails.LblIsLikedFalse;
            View.BusyClear();
            return base.OnLoaded();
        }

        #endregion


        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdReportIdea = new VmCommand(ResViewAddIdea.CmdReportIdea, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);

                var newReport = new ExReport
                                {
                                    UserId = Dc.DcExUser.Data.Id,
                                    UserImage = Dc.DcExUser.Data.UserImageLink,
                                    UserName = Dc.DcExUser.Data.Fullname,
                                    IdeaId = DcListDataPoint.Id
                                };

                var item = new DcListDataPoint<ExReport>(newReport);
                Dc.DcExReports.Add(item);

                var r = await Nav.ToViewWithResult(typeof(VmAddReport), item).ConfigureAwait(true);
                try
                {
                    await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): {e}");
                }

                View.BusyClear();

                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        try
                        {
                            Dc.DcExReports.Remove(item);
                        }
                        catch (Exception)
                        {
                            Logging.Log.LogWarning($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                        }
                    }
                    else
                    {
                        await Nav.Back().ConfigureAwait(true);
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }
            }, glyph: Glyphs.Flag_warning);


            CmdAddToCalendar = new VmCommand(ResViewAddIdea.CmdAddToCalendar, async () =>
            {
                var appointment = new BissAppointment
                                  {
                                      StartTime = Data.From,
                                      EndTime = Data.To,
                                      Title = Data.Title,
                                      Description = Data.Description
                                  };
                await Open.Calendar(appointment).ConfigureAwait(true);
            }, glyph: Glyphs.Calendar_add_1);


            CmdShowMap = new VmCommand(ResViewIdeaDetails.CmdShowMap, async p => { await Nav.ToViewWithResult(typeof(VmIdeaMap), DcListDataPoint).ConfigureAwait(true); }, glyph: Glyphs.Maps_pin_1);

            CmdShowNeedInfo = new VmCommand("i", p =>
            {
                if (p is DcListDataPoint<ExIdeaNeed> need)
                {
                    MsgBox.Show(need.Data.Infotext).ConfigureAwait(true);
                }
            }, glyph: Glyphs.Information_circle);

            CmdShowHelperInfo = new VmCommand("i", p =>
            {
                if (p is DcListDataPoint<ExIdeaHelper> helper)
                {
                    MsgBox.Show(helper.Data.Info).ConfigureAwait(true);
                }
            }, glyph: Glyphs.Information_circle);

            CmdChatHelper = new VmCommand(ResViewAddIdea.CmdChatHelper, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);

                if (p is DcListDataPoint<ExIdeaHelper> item)
                {
                    Dc.Chat.SelectedUiChat = null!;

                    if (!item.Data.PrivateChatId.HasValue)
                    {
                        if (!await Dc.Chat.NewChat(item.Data.UserId, item.Index.ToString()).ConfigureAwait(true))
                        {
                            await MsgBox.Show($"Privater Chat für Idee {DcListDataPoint.Data.Title} mit Helper Id {item.Data.UserId} konnte nicht erstellt werden.").ConfigureAwait(true);
                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PrivateChatId.Value);
                        }
                        catch (Exception)
                        {
                            try
                            {
                                await Dc.Chat.Sync(item.Data.PrivateChatId).ConfigureAwait(true);
                                Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(item.Data.PrivateChatId.Value);
                            }
                            catch (Exception)
                            {
                                await MsgBox.Show($"Privater Chat für Private Chat Id{item.Data.PrivateChatId.Value} konnte nicht geöffnet werden.").ConfigureAwait(true);
                            }
                        }
                    }

                    if (Dc.Chat.SelectedUiChat != null!)
                    {
                        await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                    }

                    View.BusyClear();
                }
            }, glyph: Glyphs.Conversation_chat_1);

            CmdAddHelper = new VmCommand(ResViewIdeaDetails.CmdAddHelper, async () =>
            {
                // Bei neuen Ideen nicht möglich
                if (DcListDataPoint.State == EnumDcListElementState.New)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);

                if (Dc.DcExIdeaNeeds.Any(x =>
                        x.State != EnumDcListElementState.None &&
                        x.Data.IdeaId == DcListDataPoint.Index))
                {
                    await MsgBox.Show(ResViewAddIdea.MsgNeedsSave).ConfigureAwait(true);
                    return;
                }

                var isNew = false;
                var item = Dc.DcExIdeaHelpers.FirstOrDefault(f => f.Data.IdeaId == DcListDataPoint.Id && f.Data.UserId == Dc.DcExUser.Data.Id);
                if (item == null)
                {
                    var newItem = new ExIdeaHelper
                                  {
                                      IdeaId = DcListDataPoint.Index,

                                      UserId = Dc.DcExUser.Data.Id,
                                      UserName = Dc.DcExUser.Data.Fullname,
                                      UserImage = Dc.DcExUser.Data.UserImageLink,

                                      From = Data.HasTimespan ? Data.From : DateTime.Now,
                                      To = Data.HasTimespan ? Data.To : DateTime.Now,

                                      CanEdit = true,
                                      CanDelete = true,
                                      IsMine = true,
                                  };
                    item = new DcListDataPoint<ExIdeaHelper>(newItem);
                    Dc.DcExIdeaHelpers.Add(item);
                    isNew = true;
                }

                var r = await Nav.ToViewWithResult(typeof(VmAddHelper), item).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result == EnumVmEditResult.NotModified)
                    {
                        if (isNew)
                        {
                            try
                            {
                                Dc.DcExIdeaHelpers.Remove(item);
                            }
                            catch (Exception)
                            {
                                Logging.Log.LogWarning($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                            }
                        }
                    }
                    else
                    {
                        if (DcListDataPoint.State != EnumDcListElementState.New)
                        {
                            var helperRes = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                            if (!helperRes.DataOk)
                            {
                                Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(OnDisappearing)}): {helperRes.ServerExceptionText}");
                            }
                        }

                        item.Update();
                    }
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                UpdateHelperUiList();
                UpdateNeedUiList();

                View.BusyClear();
            }, glyph: Glyphs.Add_circle);

            CmdChatIdea = new VmCommand(string.Empty, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);


                Dc.Chat.SelectedUiChat = null!;
                try
                {
                    Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(DcListDataPoint.Data.PublicChatId);
                }
                catch (Exception)
                {
                    try
                    {
                        await Dc.Chat.Sync(DcListDataPoint.Data.PublicChatId).ConfigureAwait(true);
                        Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(DcListDataPoint.Data.PublicChatId);
                    }
                    catch (Exception)
                    {
                        await MsgBox.Show($"Public Chat (id {DcListDataPoint.Data.PublicChatId}) für Idee {DcListDataPoint.Data.Title} konnte nicht geöffnet werden").ConfigureAwait(true);
                        View.BusyClear();
                        return;
                    }
                }

                if (Dc.Chat.SelectedUiChat != null!)
                {
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                }

                View.BusyClear();
            }, glyph: Glyphs.Messaging_we_chat);

            CmdChatUser = new VmCommand(string.Empty, async () =>
            {
                if (DcListDataPoint.Data.IsMine)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);


                Dc.Chat.SelectedUiChat = null!;

                if (!DcListDataPoint.Data.PrivateChatId.HasValue)
                {
                    try
                    {
                        if (!await Dc.Chat.NewChat(DcListDataPoint.Data.CreatorUserId, DcListDataPoint.Index.ToString()).ConfigureAwait(true))
                        {
                            await MsgBox.Show($"Privater Chat für Idee {DcListDataPoint.Data.Title} mit Ersteller Id {DcListDataPoint.Data.CreatorUserId} konnte nicht erstellt werden.").ConfigureAwait(true);

                            View.BusyClear();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log.LogError($"[{GetType().Name}]({nameof(InitializeCommands)}): {ex}");
                        View.BusyClear();
                    }
                }
                else
                {
                    try
                    {
                        Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(DcListDataPoint.Data.PrivateChatId.Value);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            await Dc.Chat.Sync(DcListDataPoint.Data.PrivateChatId).ConfigureAwait(true);
                            Dc.Chat.SelectedUiChat = Dc.Chat.GetChat(DcListDataPoint.Data.PrivateChatId.Value);
                        }
                        catch (Exception)
                        {
                            await MsgBox.Show($"Privater Chat für Private Chat Id{DcListDataPoint.Data.PrivateChatId.Value} konnte nicht geöffnet werden.").ConfigureAwait(true);

                            View.BusyClear();
                            return;
                        }
                    }
                }

                if (Dc.Chat.SelectedUiChat != null!)
                {
                    await Nav.ToViewWithResult(typeof(VmChat), cachePage: false).ConfigureAwait(true);
                }

                View.BusyClear();
            }, glyph: Glyphs.Conversation_chat_1);


            CmdEditItem = new VmCommand(ResViewIdeaDetails.CmdEditItem, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);
                var r = await Nav.ToViewWithResult(typeof(VmAddIdea), DcListDataPoint).ConfigureAwait(true);
                if (r is EnumVmEditResult result)
                {
                    if (result != EnumVmEditResult.ModifiedAndStored)
                    {
                        var p = DcListDataPoint;
                        if (p.PossibleNewDataOnServer)
                        {
                            // p.Update();
                        }
                    }
                }
                else if (r == null)
                {
                    // Chat
                }
                else
                {
                    throw new ArgumentException("Wrong result!");
                }

                View.BusyClear();
            }, glyph: Glyphs.Pencil_1);


            CmdDeleteItem = new VmCommand(ResViewIdeaDetails.CmdDeleteItem, async () =>
            {
                var res = await MsgBox.Show(ResViewMainIdea.MsgDeleteConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);

                try
                {
                    Dc.DcExIdeas.Remove(DcListDataPoint);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                }

                var r = await Dc.DcExIdeas.StoreAll().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Löschen fehlgeschlagen - {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewMainIdea.MsgDeleteError +
                                      (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.CustomerRelease ? string.Empty : r.ServerExceptionText))
                        .ConfigureAwait(true);
                    Dc.DcExIdeas.Add(DcListDataPoint);
                }
                else
                {
                    CheckSaveBehavior = null;
                    ViewResult = EnumVmEditResult.ModifiedAndStored;
                    await Nav.Back().ConfigureAwait(true);
                }

                View.BusyClear();
            }, glyph: Glyphs.Bin_1);

            CmdDeleteHelper = new VmCommand(ResViewIdeaDetails.CmdDeleteHelper, async h =>
            {
                var res = await MsgBox.Show(ResViewMainIdea.MsgDeleteHelperConfirm, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                if (res != VmMessageBoxResult.Yes)
                {
                    return;
                }

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    GCmdHome.Execute(null!);
                    return;
                }

                View.BusySet(delay: 50);

                if (h is DcListDataPoint<ExIdeaHelper> helper)
                {
                    try
                    {
                        Dc.DcExIdeaHelpers.Remove(helper);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }


                    var r = await Dc.DcExIdeaHelpers.StoreAll().ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        Logging.Log.LogError($"[{nameof(VmAddIdea)}]({nameof(InitializeCommands)}): Löschen fehlgeschlagen - {r.ServerExceptionText}");
                        await MsgBox.Show(ResViewMainIdea.MsgDeleteError +
                                          (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.CustomerRelease ? string.Empty : r.ServerExceptionText))
                            .ConfigureAwait(true);
                        Dc.DcExIdeaHelpers.Add(helper);
                    }
                    else
                    {
                        CheckSaveBehavior = null;
                        ViewResult = EnumVmEditResult.ModifiedAndStored;
                        await Nav.Back().ConfigureAwait(true);
                    }
                }

                View.BusyClear();
            }, glyph: Glyphs.Bin_1);

            CmdLike = new VmCommand(string.Empty, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                View.BusySet("Sichere ...", 50);

                var res = await Dc.LikeIdea(DcListDataPoint.Index).ConfigureAwait(true);
                if (!res.Ok)
                {
                    await MsgBox.Show(ResViewMainIdea.MsgLikeError +
                                      (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.CustomerRelease ? string.Empty : res.ServerExceptionText))
                        .ConfigureAwait(true);
                }
                else
                {
                    if (DcListDataPoint.DataSource == EnumDcDataSource.LocalSet)
                    {
                        DcListDataPoint.DataSource = EnumDcDataSource.LocalCache;
                        DcListDataPoint.State = EnumDcListElementState.None;
                    }

                    try
                    {
                        await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmIdeaDetails)}]({nameof(InitializeCommands)}): {e}");
                    }
                }

                CmdLike.DisplayName = Data.IsLiked ? ResViewIdeaDetails.LblIsLikedTrue : ResViewIdeaDetails.LblIsLikedFalse;

                View.BusyClear();
            }, glyph: Glyphs.Like);
        }

        /// <summary>
        ///     Beitrag Liken
        /// </summary>
        public VmCommand CmdLike { get; set; } = null!;

        /// <summary>
        ///     Idee bearbeiten
        /// </summary>
        public VmCommand CmdEditItem { get; set; } = null!;

        /// <summary>
        ///     Idee löschen
        /// </summary>
        public VmCommand CmdDeleteItem { get; set; } = null!;

        /// <summary>
        ///     Helfer löschen
        /// </summary>
        public VmCommand CmdDeleteHelper { get; set; } = null!;

        /// <summary>
        ///     Helfer hinzufügen
        /// </summary>
        public VmCommand CmdAddHelper { get; set; } = null!;

        /// <summary>
        ///     Chat mit helfer öffnen
        /// </summary>
        public VmCommand CmdChatHelper { get; set; } = null!;

        /// <summary>
        ///     Idee zum Kalender hinzufügen
        /// </summary>
        public VmCommand CmdAddToCalendar { get; set; } = null!;

        /// <summary>
        ///     Idee melden
        /// </summary>
        public VmCommand CmdReportIdea { get; set; } = null!;

        /// <summary>
        ///     Position auf Karte zeigen
        /// </summary>
        public VmCommand CmdShowMap { get; set; } = null!;

        /// <summary>
        ///     Infotext für Sache anzeigen
        /// </summary>
        public VmCommand CmdShowNeedInfo { get; set; } = null!;

        /// <summary>
        ///     Infotext von helfer öffnen
        /// </summary>
        public VmCommand CmdShowHelperInfo { get; set; } = null!;

        /// <summary>
        ///     Privat mit jemanden Chatten
        /// </summary>
        public VmCommand CmdChatUser { get; set; } = null!;

        /// <summary>
        ///     über Beitrag mit Usern Chatten
        /// </summary>
        public VmCommand CmdChatIdea { get; set; } = null!;

        #endregion
    }
}