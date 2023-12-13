// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Biss.ObjectEx;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel.User
{
    /// <summary>
    ///     <para>Passwort verändern</para>
    ///     Klasse VmEditUserPassword. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditUserPassword")]
    public class VmEditUserPassword : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUserPassword.DesignInstance}"
        /// </summary>
        public static VmEditUserPassword DesignInstance = new VmEditUserPassword();

        /// <summary>
        ///     VmEditUserPassword
        /// </summary>
        public VmEditUserPassword() : base(ResViewEditUserPassword.LblTitle, subTitle: ResViewEditUserPassword.LblSubTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Aktuelles Passwort
        /// </summary>
        public VmEntry EntryCurrentPassword { get; set; } = null!;

        /// <summary>
        ///     Neues Passwort
        /// </summary>
        public VmEntry EntryNewPassword { get; set; } = null!;

        /// <summary>
        ///     In der View die Eingaben als Passwortfelder anzeigen
        /// </summary>
        public bool ShowEntriesAsPassword { get; set; } = true;

        /// <summary>
        ///     Binding für Toggle
        /// </summary>
        public bool ShowEntriesNotAsPassword
        {
            get => !ShowEntriesAsPassword;
            set => ShowEntriesAsPassword = !value;
        }

        /// <summary>
        ///     Passwort togglen
        /// </summary>
        public VmCommand CmdTogglePassword { get; set; } = null!;

        #endregion

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var r = base.OnActivated(args);

            EntryCurrentPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUserPassword.EntryTitleCurrentPassword,
                ResViewEditUserPassword.EntryPlaceholderCurrentPassword,
                validateFunc: VmEntryValidators.ValidatePwdFunc,
                returnAction: () =>
                {
                    EntryCurrentPassword.ValidateData();
                    EntryNewPassword.Focus(EnumVmEntrySetFocusMode.FocusAndSelect);
                },
                showTitle: !TabletMode);
            EntryNewPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUserPassword.EntryTitleNewPassword,
                ResViewEditUserPassword.EntryPlaceholderNewPassword,
                validateFunc: VmEntryValidators.ValidatePwdFunc,
                returnAction: () =>
                {
                    EntryNewPassword.ValidateData();
                    View.CmdSaveHeader!.Execute(null!);
                },
                showTitle: !TabletMode);

            EntryCurrentPassword.ValidChanged += (sender, a) => View.CmdSaveHeader!.CanExecute();
            EntryNewPassword.ValidChanged += (sender, a) => View.CmdSaveHeader!.CanExecute();
            return r;
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdTogglePassword = new VmCommand(string.Empty, () => { ShowEntriesAsPassword = !ShowEntriesAsPassword; });

            View.CmdSaveHeader = new VmCommand(ResViewEditUserPassword.CmdSave, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (this.GetAllInstancesWithType<VmEntry>().Any(p => !p.DataValid))
                {
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    return;
                }

                Dc.DcExUserPassword.Data.CurrentPasswordHash = AppCrypt.CumputeHash(EntryCurrentPassword.Value);
                Dc.DcExUserPassword.Data.NewPasswordHash = AppCrypt.CumputeHash(EntryNewPassword.Value);
                var r = await Dc.DcExUserPassword.StoreData(true).ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError($"[VmEditUserPassword:CmdSave] StoreData result is false. Type: {r.ErrorType} Msg: {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewEditUserPassword.MsgChangeError, ResViewEditUserPassword.MsgTitleChangeError).ConfigureAwait(true);
                }
                else
                {
                    await Nav.Back().ConfigureAwait(true);
                }
            }, glyph: Glyphs.Floppy_disk);
        }
    }
}