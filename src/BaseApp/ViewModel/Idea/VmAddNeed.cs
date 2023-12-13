// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using Biss.AppConfiguration;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange;
using Exchange.Enum;
using Exchange.Model.Idea;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.Idea
{
    /// <summary>
    ///     <para>Need hinzufügen/editieren</para>
    ///     Klasse VmAddNeed. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddNeed")]
    public class VmAddNeed : VmEditDcListPoint<ExIdeaNeed>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddNeed.DesignInstance}"
        /// </summary>
        public static VmAddNeed DesignInstance = new VmAddNeed();

        /// <summary>
        ///     VmAddNeed
        /// </summary>
        public VmAddNeed() : base(ResViewAddNeed.LblTitle, subTitle: ResViewAddNeed.LblSubTitle, autoStoreDc: false)
        {
            SetViewProperties(true);
            View.ShowUser = false;
        }

        #region Properties

        /// <summary>
        ///     Eingabefeld Titel
        /// </summary>
        public VmEntry EntryTitle { get; set; } = null!;

        /// <summary>
        ///     Eingabefeld Menge
        /// </summary>
        public VmEntry EntryAmount { get; set; } = null!;

        /// <summary>
        ///     Eingabefeld Menge Label
        /// </summary>
        public VmEntry EntryAmountLabel { get; set; } = null!;

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

            await base.OnActivated(args).ConfigureAwait(true);

            if (Data.CanEdit)
            {
                EntryTitle = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddNeed.EntryNameTitle,
                    ResViewAddNeed.EntryNamePlaceholder,
                    Data,
                    nameof(ExIdeaNeed.Title),
                    VmEntryValidators.ValidateFuncStringEmpty,
                    showTitle: true,
                    maxChar: 50);

                EntryAmount = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddNeed.EntryAmountTitle,
                    ResViewAddNeed.EntryAmountPlaceholder,
                    Data,
                    nameof(ExIdeaNeed.AmountNeed),
                    VmEntryValidators.ValidateFuncLongPositive,
                    showTitle: true,
                    showMaxChar: false);

                EntryAmountLabel = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddNeed.EntryAmountLabelTitle,
                    ResViewAddNeed.EntryAmountLabelPlaceholder,
                    Data,
                    nameof(ExIdeaNeed.AmountLabel),
                    VmEntryValidators.ValidateFuncStringEmpty,
                    showTitle: true,
                    showMaxChar: false);

                EntryInfoText = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewAddNeed.EntryInfoTitle,
                    ResViewAddNeed.EntryInfoPlaceholder,
                    Data,
                    nameof(ExIdeaNeed.Infotext),
                    ValidateFuncStringEmptyInfo,
                    showTitle: false,
                    showMaxChar: false);
            }

            CheckBeforeSaving = async () =>
            {
                if (Data.AmountNeed < 0)
                {
                    await MsgBox.Show(ResViewAddNeed.ValAmountNotNegative).ConfigureAwait(true);
                    return false;
                }

                if (Data.HasInfo && string.IsNullOrWhiteSpace(Data.Infotext))
                {
                    Data.HasInfo = false;
                }

                return true;
            };
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Infotext togglen
        /// </summary>
        public VmCommand CmdToggleInfo { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
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

                if (CheckBeforeSaving != null)
                {
                    var check = await CheckBeforeSaving().ConfigureAwait(true);
                    if (!check)
                    {
                        Logging.Log.LogInfo($"[VmEditDcListPoint]({nameof(InitializeCommands)}): StoreData canceled by BeforeSave!");
                        return;
                    }
                }

                if (DcListDataPoint.State != EnumDcListElementState.New)
                {
                    DcListDataPoint.State = EnumDcListElementState.Modified;
                }

                // Bei bestehenden Ideen direkt sichern - wegen checkSave bei AddIdea
                if (Data.IdeaId > 0)
                {
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