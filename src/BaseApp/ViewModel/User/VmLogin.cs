// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange.Enum;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace BaseApp.ViewModel.User
{
    #region Hilfsklassen

    /// <summary>
    ///     States der View
    /// </summary>
    public enum EnumViewLoginStates
    {
        /// <summary>
        ///     Login wird angezigt
        /// </summary>
        Login,

        /// <summary>
        ///     Login und Passwort
        /// </summary>
        Password,

        /// <summary>
        ///     Passwort ist falsch - neues zusenden anzeigen
        /// </summary>
        PasswordWrong,

        /// <summary>
        ///     User hat seine E-Mail (noch) nicht validiert
        /// </summary>
        UserNotValidated,

        /// <summary>
        ///     Account ist gesperrt
        /// </summary>
        UserAccountLocked
    }

    #endregion

    /// <summary>
    ///     <para>Login</para>
    ///     Klasse VmLogin. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewLogin", true)]
    public class VmLogin : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmLogin.DesignInstance}"
        /// </summary>
        public static VmLogin DesignInstance = new VmLogin();

        private DcCheckUserLoginResult? _checkUser;
        private EnumViewLoginStates _currentState = EnumViewLoginStates.Login;

        /// <summary>
        ///     VmLogin
        /// </summary>
        public VmLogin() : base(ResViewLogin.LblTitle, subTitle: ResViewLogin.LblSubTitle)
        {
            SetViewProperties();

            EntryLoginName = new VmEntry(title: ResViewLogin.EntryTitleLoginName,
                placeholder: ResViewLogin.EntryPlaceholderLoginName,
                validateFunc: ValidateLoginName,
                returnAction: () =>
                {
                    EntryLoginName!.ValidateData();
                    if (!EntryLoginName.DataValid)
                    {
                        return;
                    }

                    EntryPassword!.Focus(EnumVmEntrySetFocusMode.FocusAndSelect);
                },
                showTitle: !TabletMode,
                showMaxChar: false);
            EntryLoginName.ValidChanged += (sender, args) =>
            {
                if (!EntryLoginName.DataValid && _currentState != EnumViewLoginStates.Login)
                {
                    CurrentState = EnumViewLoginStates.Login;
                }

                CmdLogin.CanExecute();
            };

            EntryPassword = new VmEntry(title: ResViewLogin.EntryTitlePassword,
                placeholder: ResViewLogin.EntryPlaceholderPassword,
                returnAction: () =>
                {
                    EntryPassword!.ValidateData();
                    if (EntryPassword.DataValid)
                    {
                        CmdLogin.Execute(null!);
                    }
                },
                showTitle: !TabletMode,
                showMaxChar: false);

#if DEBUG
            EntryLoginName.Value = "biss@fotec.at";
            EntryPassword.Value = "biss";
#endif
        }

        #region Properties

        /// <summary>
        ///     Im Web angemeldet bleiben
        /// </summary>
        public bool KeepLogin { get; set; } = true;

        /// <summary>
        ///     Aktueller State
        /// </summary>
        public EnumViewLoginStates CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState == value)
                {
                    return;
                }

                var nextState = value;
                switch (nextState)
                {
                    case EnumViewLoginStates.Login:
                        _checkUser = null;
                        CmdLogin.DisplayName = ResViewLogin.CmdLogin;
                        break;
                    case EnumViewLoginStates.Password:
                        CmdLogin.DisplayName = ResViewLogin.CmdLogin;
                        break;
                    case EnumViewLoginStates.PasswordWrong:
                        CmdLogin.DisplayName = ResViewLogin.CmdLogin;
                        break;
                    case EnumViewLoginStates.UserNotValidated:
                        CmdLogin.DisplayName = ResViewLogin.CmdLogin;
                        break;
                    case EnumViewLoginStates.UserAccountLocked:
                        _checkUser = null;
                        CmdLogin.DisplayName = ResViewLogin.CmdLogin;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _currentState = nextState;
            }
        }


        /// <summary>
        ///     Passwort Eingabe
        /// </summary>
        [DependsOn(nameof(CurrentState))]
        public bool IsPasswordEntryVisible => CurrentState == EnumViewLoginStates.Password || CurrentState == EnumViewLoginStates.PasswordWrong;

        /// <summary>
        ///     E-Mail nicht bestätigt
        /// </summary>
        [DependsOn(nameof(CurrentState))]
        public bool IsResendAccessLinkVisible => CurrentState == EnumViewLoginStates.UserNotValidated;

        /// <summary>
        ///     Passwort zurücksetzen sichtbar
        /// </summary>
        [DependsOn(nameof(CurrentState))]
        public bool IsForgotPasswordVisible => CurrentState == EnumViewLoginStates.PasswordWrong;

        /// <summary>
        ///     Login Name
        /// </summary>
        public VmEntry EntryLoginName { get; set; }

        /// <summary>
        ///     Passwort
        /// </summary>
        public VmEntry EntryPassword { get; set; }

        /// <summary>
        ///     Login Command
        /// </summary>
        public VmCommand CmdLogin { get; set; } = null!;

        /// <summary>
        ///     Passwort vergessen Command
        /// </summary>
        public VmCommand CmdForgotPassword { get; set; } = null!;

        /// <summary>
        ///     Passwort vergessen Command
        /// </summary>
        public VmCommand CmdResendAccessLink { get; set; } = null!;

        /// <summary>
        ///     Registrieren
        /// </summary>
        public VmCommand CmdRegister { get; set; } = null!;

        #endregion

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdLogin = new VmCommand(ResViewLogin.CmdLogin, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                EntryLoginName.ValidateData();
                if (!EntryLoginName.DataValid)
                {
                    await MsgBox.Show(ResViewLogin.MsgEnterMail).ConfigureAwait(true);
                    return;
                }

                if (CurrentState == EnumViewLoginStates.Login || CurrentState == EnumViewLoginStates.UserAccountLocked)
                {
                    //Weiter würde gedrückt
                    _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                    if (_checkUser != null && _checkUser.UserId != -1)
                    {
                        if (_checkUser.Locked)
                        {
                            CurrentState = EnumViewLoginStates.UserAccountLocked;
                            await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                        }
                        else
                        {
                            if (_checkUser.LoginConfirmed)
                            {
                                CurrentState = EnumViewLoginStates.Password;
                                CmdLogin.Execute(null!);
                            }
                            else
                            {
                                CurrentState = EnumViewLoginStates.UserNotValidated;
                                await MsgBox.Show(ResViewLogin.MsgUserNotValidated, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                            }
                        }
                    }
                    else
                    {
                        if (CurrentAppType == EnumAppType.User)
                        {
                            var yes = await MsgBox.Show(ResViewLogin.MsgNewUser, ResViewLogin.MsgTitleNewUser, VmMessageBoxButton.YesNo).ConfigureAwait(true);
                            if (yes == VmMessageBoxResult.Yes)
                            {
                                Dc.DcExUser.Data.LoginName = EntryLoginName.Value;
                                var r = await Nav.ToViewWithResult(typeof(VmEditUser)).ConfigureAwait(true);
                                if (r is bool result && result)
                                {
                                    _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                                    if (_checkUser != null && _checkUser.UserId != -1)
                                    {
                                        if (_checkUser.LoginConfirmed)
                                        {
                                            CurrentState = EnumViewLoginStates.Password;
                                            CmdLogin.Execute(null!);
                                        }
                                        else
                                        {
                                            CurrentState = EnumViewLoginStates.UserNotValidated;
                                        }
                                    }
                                }
                                else
                                {
                                    Dc.DcExUser.SetDefault();
                                }
                            }
                        }
                        else
                        {
                            await MsgBox.Show(ResViewLogin.MsgCannotCreateAccount).ConfigureAwait(true);
                        }
                    }
                }
                else if (CurrentState == EnumViewLoginStates.Password || CurrentState == EnumViewLoginStates.PasswordWrong)
                {
                    var pwdCheck = await Dc.CheckUserPassword(_checkUser!.UserId, AppCrypt.CumputeHash(EntryPassword.Value)).ConfigureAwait(true);
                    if (pwdCheck)
                    {
                        View.BusySet(ResViewLogin.BsyLogin);
                        Dc.DcExOrganization.ResetList();
                        await Dc.DcExUser.WaitDataFromServerAsync(forceUpdate: true).ConfigureAwait(true);
                        CurrentVmMenu?.UpdateMenu();
                        GCmdHome.Execute(null!);
                    }
                    else
                    {
                        await MsgBox.Show(ResViewLogin.MsgWrongPassword, ResViewLogin.MsgTitleWrongPassword).ConfigureAwait(true);
                        CurrentState = EnumViewLoginStates.PasswordWrong;
                    }
                }
                else if (CurrentState == EnumViewLoginStates.UserNotValidated)
                {
                    //Weiter würde gedrückt
                    _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                    if (_checkUser != null && _checkUser.UserId != -1)
                    {
                        if (_checkUser.LoginConfirmed)
                        {
                            CurrentState = EnumViewLoginStates.Password;
                            CmdLogin.Execute(null!);
                        }
                        else
                        {
                            CurrentState = EnumViewLoginStates.UserNotValidated;
                            await MsgBox.Show(ResViewLogin.MsgUserNotValidated, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                        }
                    }
                }
            }, CanExecuteLoginCommand);

            CmdForgotPassword = new VmCommand(ResViewLogin.CmdForgotPassword, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                EntryLoginName.ValidateData();
                if (!EntryLoginName.DataValid)
                {
                    await MsgBox.Show(ResViewLogin.MsgEnterMail).ConfigureAwait(true);
                    return;
                }

                _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                if (_checkUser != null && _checkUser.UserId != -1)
                {
                    var r = await Dc.ResetPassword(_checkUser.UserId).ConfigureAwait(true);
                    if (!r.Ok)
                    {
                        Logging.Log.LogWarning($"[CmdForgotPassword] Error: {r.ServerExceptionText}");
                        await MsgBox.Show(ResViewLogin.MsgSendError, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                    }
                    else
                    {
                        await MsgBox.Show(ResViewLogin.MsgResetSent, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                    }
                }
                else
                {
                    var yes = await MsgBox.Show(ResViewLogin.MsgNewUser, ResViewLogin.MsgTitleNewUser, VmMessageBoxButton.YesNo).ConfigureAwait(true);
                    if (yes == VmMessageBoxResult.Yes)
                    {
                        CmdRegister.Execute(null!);
                    }
                }
            });

            CmdRegister = new VmCommand(ResViewLogin.CmdRegister, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                EntryLoginName.ValidateData();
                if (!EntryLoginName.DataValid)
                {
                    await MsgBox.Show(ResViewLogin.MsgEnterMail).ConfigureAwait(true);
                    return;
                }

                _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                if (_checkUser != null && _checkUser.UserId != -1)
                {
                    var yes = await MsgBox.Show(ResViewLogin.MsgExistingUser, ResViewLogin.MsgTitleExistingUser, VmMessageBoxButton.YesNo).ConfigureAwait(true);
                    if (yes == VmMessageBoxResult.Yes)
                    {
                        if (_checkUser.Locked)
                        {
                            CurrentState = EnumViewLoginStates.UserAccountLocked;
                            await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                        }
                        else
                        {
                            if (_checkUser.LoginConfirmed)
                            {
                                CurrentState = EnumViewLoginStates.Password;
                                CmdLogin.Execute(null!);
                            }
                            else
                            {
                                CurrentState = EnumViewLoginStates.UserNotValidated;
                                await MsgBox.Show(ResViewLogin.MsgUserNotValidated, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                            }
                        }
                    }
                }
                else
                {
                    if (CurrentAppType == EnumAppType.User)
                    {
                        Dc.DcExUser.Data.LoginName = EntryLoginName.Value;
                        var r = await Nav.ToViewWithResult(typeof(VmEditUser)).ConfigureAwait(true);
                        if (r is bool result && result)
                        {
                            _checkUser = await Dc.CheckUserLoginName(EntryLoginName.Value).ConfigureAwait(true);
                            if (_checkUser != null && _checkUser.UserId != -1)
                            {
                                if (_checkUser.LoginConfirmed)
                                {
                                    CurrentState = EnumViewLoginStates.Password;
                                    CmdLogin.Execute(null!);
                                }
                                else
                                {
                                    CurrentState = EnumViewLoginStates.UserNotValidated;
                                }
                            }
                        }
                        else
                        {
                            Dc.DcExUser.SetDefault();
                        }
                    }
                    else
                    {
                        await MsgBox.Show(ResViewLogin.MsgCannotCreateAccount).ConfigureAwait(true);
                    }
                }
            });

            CmdResendAccessLink = new VmCommand(ResViewLogin.CmdResendAccessLink, async () =>
            {
                var r = await Dc.ResendAccessEMail(_checkUser!.UserId).ConfigureAwait(true);
                if (r.Ok)
                {
                    await MsgBox.Show(ResViewLogin.MsgConfirmationSent, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                }
                else
                {
                    Logging.Log.LogWarning($"[CmdForgotPassword] Error: {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewLogin.MsgSendError, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                }
            }, () => EntryLoginName != null! && EntryLoginName.DataValid);
        }

        /// <summary>
        ///     Validierung für Login Name
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateLoginName(string arg)
        {
            (string, bool) result;

            result = VmEntryValidators.ValidateFuncEmail(arg);

            if (!result.Item2)
            {
                CurrentState = EnumViewLoginStates.Login;
            }

            return result;
        }

        /// <summary>
        ///     Kann der Button Login ausgeführt werden
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteLoginCommand()
        {
            if (EntryLoginName == null!)
            {
                return false;
            }

            return EntryLoginName.DataValid;
        }
    }
}