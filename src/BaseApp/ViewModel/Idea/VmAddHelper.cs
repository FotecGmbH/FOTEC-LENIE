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
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace BaseApp.ViewModel.Idea
{
    /// <summary>
    ///     <para>Helper für Idee hinzufügen</para>
    ///     Klasse VmAddHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddHelper")]
    public class VmAddHelper : VmEditDcListPoint<ExIdeaHelper>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddHelper.DesignInstance}"
        /// </summary>
        public static VmAddHelper DesignInstance = new VmAddHelper();

        private DateTime _fromDate;
        private TimeSpan _fromTime;
        private DateTime _toDate;
        private TimeSpan _toTime;

        /// <summary>
        ///     VmAddHelper
        /// </summary>
        public VmAddHelper() : base(ResViewAddHelper.LblTitle, subTitle: ResViewAddHelper.LblSubTitle, autoStoreDc: false)
        {
            SetViewProperties(true);
            View.ShowUser = false;
        }

        #region Properties

        /// <summary>
        ///     From - nur DatumsTeil
        /// </summary>
        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                if (Data != null!)
                {
                    var dateShift = value.Date.Subtract(Data.From.Date);

                    Data.From = value.Add(FromTime);

                    ToDate = ToDate.Add(dateShift);
                }
            }
        }

        /// <summary>
        ///     From - nur Zeitteil
        /// </summary>
        public TimeSpan FromTime
        {
            get => _fromTime;
            set
            {
                _fromTime = value;
                if (Data != null!)
                {
                    var timeShift = value.Subtract(Data.From.TimeOfDay);

                    Data.From = FromDate.Add(value);

                    ToTime = ToTime.Add(timeShift);
                }
            }
        }

        /// <summary>
        ///     To - nur Datumsteil
        /// </summary>
        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                if (Data != null!)
                {
                    Data.To = value.Add(ToTime);
                }
            }
        }

        /// <summary>
        ///     To - nur Zeitteil
        /// </summary>
        public TimeSpan ToTime
        {
            get => _toTime;
            set
            {
                _toTime = value;
                if (Data != null!)
                {
                    Data.To = ToDate.Add(value);
                }
            }
        }

        /// <summary>
        ///     Eingabefeld Infotext
        /// </summary>
        public VmEntry EntryInfoText { get; set; } = null!;

        #endregion

        /// <summary>
        ///     String nicht leer
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public (string hint, bool valid) ValidateFuncStringEmptyInfo(string arg)
        {
            if (!Data.HasInfo)
            {
                return (string.Empty, true);
            }

            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.IsSelected = true;
                return;
            }

            if (args is DcListDataPoint<ExIdeaHelper> idea)
            {
                FromDate = idea.Data.From.Date;
                FromTime = idea.Data.From.TimeOfDay;

                ToDate = idea.Data.To.Date;
                ToTime = idea.Data.To.TimeOfDay;
            }

            await base.OnActivated(args).ConfigureAwait(true);

            {
                EntryInfoText = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddHelper.EntryInfoTitle,
                    ResViewAddHelper.EntryInfoPlaceholder,
                    Data,
                    nameof(ExIdeaHelper.Info),
                    ValidateFuncStringEmptyInfo,
                    showTitle: false,
                    showMaxChar: false);
            }

            if (DcListDataPoint.State == EnumDcListElementState.New)
            {
                View.BusySet();

                var supplies = new List<ExIdeaSupply>();

#pragma warning disable CS0618 // Type or member is obsolete
                await Dc.DcExIdeaNeeds.WaitDataFromServerAsync(filter: new ExIdeaNeedFilter {IdeaId = Data.IdeaId}.ToJson(), reload: true).ConfigureAwait(true);
#pragma warning restore CS0618 // Type or member is obsolete

                foreach (var dcNeed in Dc.DcExIdeaNeeds.Where(x => x.Data.IdeaId == Data.IdeaId))
                {
                    supplies.Add(new ExIdeaSupply
                                 {
                                     Amount = 0,
                                     NeedAmount = dcNeed.Data.AmountNeed,
                                     NeedAmountCurrent = dcNeed.Data.AmountSupplied,
                                     NeedHasInfo = dcNeed.Data.HasInfo,
                                     NeedInfo = dcNeed.Data.Infotext,
                                     NeedId = dcNeed.Index,
                                     NeedName = dcNeed.Data.Title,
                                 });
                }

                Data.Supplies = supplies;

                View.BusyClear();
            }

            CheckBeforeSaving = async () =>
            {
                if (Data.HasTimespan)
                {
                    if (Data.To < Data.From)
                    {
                        await MsgBox.Show(ResViewAddHelper.ValTimeFrameError).ConfigureAwait(true);
                        return false;
                    }
                }

                if (Data.Supplies.Any(x => x.Amount < 0))
                {
                    await MsgBox.Show(ResViewAddHelper.ValAmountNotNegative).ConfigureAwait(true);
                    return false;
                }

                return true;
            };

            _autoStoreDc = Data.IdeaId > 0;
            CheckSaveOverride = true;
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Infotext anzeigen
        /// </summary>
        public VmCommand CmdResetAmount { get; set; } = null!;

        /// <summary>
        ///     Zeitraum togglen
        /// </summary>
        public VmCommand CmdToggleTimeSpan { get; set; } = null!;

        /// <summary>
        ///     Infotext togglen
        /// </summary>
        public VmCommand CmdToggleInfo { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdResetAmount = new VmCommand(string.Empty, p =>
            {
                if (p is ExIdeaSupply supply)
                {
                    supply.Amount = 0;
                }
            }, glyph: Glyphs.Remove_circle);

            CmdToggleTimeSpan = new VmCommand(string.Empty, () => { Data.HasTimespan = !Data.HasTimespan; });

            CmdToggleInfo = new VmCommand(string.Empty, () => { Data.HasInfo = !Data.HasInfo; });

            // Cmd Cancel Edit -> Undo, NavBack, Clear CheckSave, set Result NotModified
            GCmdBack = new VmCommand(ResCommon.CmdBack, async () =>
            {
                DcListDataPoint.EndEdit(true);
                CheckSaveBehavior = null;
                ViewResult = EnumVmEditResult.NotModified;
                await Nav.Back().ConfigureAwait(true);
            }, glyph: Glyphs.Arrow_button_left);

            // Cmd End Edit -> !Undo, NavBack, Clear CheckSave, Set Result Modified
            View.CmdSaveHeader = new VmCommand(ResCommon.CmdEditFinish, async () =>
            {
                if (_pageVmEntries.Any(p => !p.DataValid))
                {
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    return;
                }

                if (DcListDataPoint.State != EnumDcListElementState.New)
                {
                    DcListDataPoint.State = EnumDcListElementState.Modified;
                }

                if (_autoStoreDc)
                {
                    if (CheckBeforeSaving != null)
                    {
                        var check = await CheckBeforeSaving().ConfigureAwait(true);
                        if (!check)
                        {
                            Logging.Log.LogInfo($"[VmEditDcListPoint]({nameof(InitializeCommands)}): StoreData canceled by BeforeSave!");
                            return;
                        }
                    }

                    var r = await DcListDataPoint.StoreData(true).ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        Logging.Log.LogWarning($"[VmEditDcListPoint]({nameof(InitializeCommands)}): {r.ErrorType}-{r.ServerExceptionText}");

                        var msg = ResCommon.MsgCannotSave;
                        if (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.Developer)
                        {
                            msg += $"\r\n{r.ErrorType}\r\n{r.ServerExceptionText}";
                        }

                        await MsgBox.Show(msg, ResCommon.MsgTitleServerError).ConfigureAwait(false);
                        return;
                    }

                    CheckSaveBehavior = null;
                    ViewResult = EnumVmEditResult.ModifiedAndStored;
                    await Nav.Back().ConfigureAwait(true);
                }
                else
                {
                    CheckSaveBehavior = null;
                    ViewResult = EnumVmEditResult.Modified;
                    await Nav.Back().ConfigureAwait(true);
                }
            }, glyph: Glyphs.Check);
        }

        #endregion
    }
}