// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.Model;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Interfaces;
using Biss.Log.Producer;
using Exchange.Enum;
using Exchange.Helper;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace BaseApp.ViewModel.User
{
    /// <summary>
    ///     <para>VmEditUser</para>
    ///     Klasse VmEditUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditUser", true)]
    public class VmEditUser : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUser.DesignInstance}"
        /// </summary>
        public static VmEditUser DesignInstance = new VmEditUser();

        private readonly AsyncAutoResetEvent townLock = new AsyncAutoResetEvent(true);

        private ExFile? _newImage;
        private bool _takingPicture;

        /// <summary>
        ///     VmEditUser
        /// </summary>
        public VmEditUser() : base(ResViewEditUser.LblTitle, subTitle: ResViewEditUser.LblSubTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     User für Ui
        /// </summary>
        public DcDataPoint<ExUser> UiUser { get; set; } = null!;

        /// <summary>
        ///     Vorname
        /// </summary>
        public VmEntry EntryFirstName { get; set; } = null!;

        /// <summary>
        ///     In der View die Eingaben als Passwortfelder anzeigen
        /// </summary>
        public bool ShowEntriesAsPassword { get; set; } = true;

        /// <summary>
        ///     Nachname
        /// </summary>
        public VmEntry EntryLastName { get; set; } = null!;

        /// <summary>
        ///     Passwort bei neuem User
        /// </summary>
        public VmEntry EntryNewPassword { get; set; } = null!;

        /// <summary>
        ///     Telefonnummer
        /// </summary>
        public VmEntry EntryPhoneNumber { get; set; } = null!;

        /// <summary>
        ///     Neuer Benutzer
        ///     Nur wenn sich der User selbst in der App registrieren (kann)
        /// </summary>
        public bool IsNewUser => !Dc.CoreConnectionInfos.UserOk;

        /// <summary>
        ///     Aktuelles Bild
        /// </summary>
        public object CurrentImage { get; set; } = null!;

        #endregion

        /// <summary>
        ///     String nicht leer
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public (string hint, bool valid) ValidateFuncStringSearch(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     Updated die Liste der verfügbaren Regionen
        /// </summary>
        public async void UpdatePickerTowns()
        {
            await townLock.WaitOne().ConfigureAwait(true);
            PickerTowns.SelectedItemChanged -= PickerTownsOnSelectedItemChanged;

            var selected = PickerTowns.SelectedItem;

            IEnumerable<ExTown>? towns = new List<ExTown>();
            if (EntryTownSearch.DataValid)
            {
                towns = Towns.Where(t =>
                    t.Name.ToUpperInvariant().StartsWith(TownSearchText.ToUpperInvariant(), StringComparison.InvariantCulture) ||
                    t.PostalCode.StartsWith(TownSearchText, StringComparison.InvariantCulture));
            }

            Dispatcher!.RunOnDispatcher(() =>
            {
                PickerTowns.Clear();

                if (EntryTownSearch.DataValid)
                {
                    foreach (var t in towns)
                    {
                        PickerTowns.AddKey(t, t.NamePlzString);
                    }

                    if (selected == null!)
                    {
                        selected = PickerTowns.FirstOrDefault();
                    }

                    if (PickerTowns.Any(p => p.Key.TownCode == selected.Key.TownCode))
                    {
                        PickerTowns.SelectKey(selected.Key);
                    }
                }

                this.InvokeOnPropertyChanged(nameof(PickerTownsHasElements));
                PickerTowns.SelectedItemChanged += PickerTownsOnSelectedItemChanged;
                if (PickerTowns.SelectedItem != null!)
                {
                    PickerTownsOnSelectedItemChanged(PickerTowns, new SelectedItemEventArgs<VmPickerElement<ExTown>>(null!, PickerTowns.SelectedItem));
                }

                townLock.Set();
            });
        }

        private void InitAferOnAppearingOrOnActivated()
        {
            if (!_takingPicture)
            {
                Dispatcher?.RunOnDispatcher(() => { CmdLoginUser.IsVisible = false; });

                CurrentImage = UiUser.Data.UserImageLink;
                if (!IsNewUser)
                {
                    CheckSaveBehavior = new CheckSaveDcBehavior<ExUser>(UiUser);
                }
                else
                {
                    UiUser.Data.PropertyChanged += DataOnPropertyChanged;
                }
            }
        }

        /// <summary>
        ///     Wenn der Account bestätigt wurde am Server automatisch einloggen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExUser.LoginConfirmed))
            {
                if (UiUser.Data.LoginConfirmed)
                {
                    Dispatcher!.RunOnDispatcher(() => CmdLoginUser.Execute(null!));
                }
            }
        }

        /// <summary>
        ///     Kann speichern gedrückt werden
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCmdSave()
        {
            if (savingLock != null)
            {
                return false;
            }

            var check = true;
            if (!IsLoaded)
            {
                check = false;
            }
            else if (!IsNewUser && CheckSaveBehavior != null)
            {
                check = CheckSaveBehavior.Check() || _newImage != null;
            }

            return check;
        }

        /// <summary>
        ///     Auswahl von Gemeinde aus CSV geändert -> Hauptgemeinde für neuen User setzen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerTownsOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<ExTown>> e)
        {
            //  Wenn nicht vorhanden - neue Hauptgemeinde anlegen
            if (!UiUser.Data.Permissions.Any())
            {
                UiUser.Data.Permissions.Add(new ExUserPermission
                                            {
                                                IsMainCompany = true,
                                                UserRight = EnumUserRight.Read,
                                                UserRole = EnumUserRole.User,
                                                DbId = -1,
                                            });
            }

            var mainTown = UiUser.Data.Permissions.FirstOrDefault();

            if (e.CurrentItem != null!)
            {
                var town = e.CurrentItem.Key;
                var org = Dc.DcExOrganization
                    .FirstOrDefault(x =>
                        x.Data.Name == town.Name &&
                        x.Data.PostalCode == town.PostalCode);

                mainTown.CompanyId = org?.Index ?? -1;
                mainTown.Town = org?.Data ?? new ExOrganization
                                             {
                                                 Name = town.Name,
                                                 PostalCode = town.PostalCode,
                                             };
            }
            else
            {
                mainTown.CompanyId = -1;
                mainTown.Town = null;
            }

            CheckCommandsCanExecute();
            View.CmdSaveHeader?.CanExecute();
        }

        /// <summary>
        ///     Picker für Auswahl der Hauptgemeinde neu befüllen
        /// </summary>
        private void InitPickerMainOrganization()
        {
            PickerMainOrganization.SelectedItemChanged -= PickerMainOrganization_SelectedItemChanged;

            PickerMainOrganization.Clear();

            foreach (var userPermission in UiUser.Data.Permissions.Where(x => x.Town != null))
            {
                PickerMainOrganization.AddKey(userPermission, userPermission.Town!.NamePlzString);
            }

            if (UiUser.Data.Permissions.Any(x => x.IsMainCompany))
            {
                PickerMainOrganization.SelectKey(UiUser.Data.Permissions.FirstOrDefault(x => x.IsMainCompany));
            }

            PickerMainOrganization.SelectedItemChanged += PickerMainOrganization_SelectedItemChanged;

            this.InvokeOnPropertyChanged(nameof(PickerMainOrganization));
        }

        /// <summary>
        ///     Auswahl der Hauptgemeinde hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PickerMainOrganization_SelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<ExUserPermission>> e)
        {
            // Main Organization ändern, ggf direkt sichern?
            foreach (var userPermission in UiUser.Data.Permissions)
            {
                userPermission.IsMainCompany = e.CurrentItem != null! && userPermission.CompanyId == e.CurrentItem.Key.CompanyId;
            }

            CheckCommandsCanExecute();
            View.CmdSaveHeader?.CanExecute();
        }

        #region Towns - für neuen User

        /// <summary>
        ///     Alle verfügbaren Gemeinden
        /// </summary>
        public static List<ExTown> Towns { get; set; } = new List<ExTown>();

        /// <summary>
        ///     Suche Gemeinden
        /// </summary>
        public VmEntry EntryTownSearch { get; set; } = null!;

        /// <summary>
        ///     Suchtext Gemeinden
        /// </summary>
        public string TownSearchText { get; set; } = string.Empty;

        /// <summary>
        ///     Gemeindeauswahl für Hauptgemeinde
        /// </summary>
        public VmPicker<ExTown> PickerTowns { get; set; } = new VmPicker<ExTown>(nameof(PickerTowns));

        /// <summary>
        ///     Picker enthält Elemente.
        /// </summary>
        [DependsOn(nameof(PickerTowns))]
        public bool PickerTownsHasElements => PickerTowns.Any();

        #endregion

        #region Organizations - für bestehenden User

        /// <summary>
        ///     Auswahl Main Organization
        /// </summary>
        public VmPicker<ExUserPermission> PickerMainOrganization { get; set; } = new VmPicker<ExUserPermission>(nameof(PickerMainOrganization));

        /// <summary>
        ///     Picker enthält Elemente.s
        /// </summary>
        [DependsOn(nameof(PickerMainOrganization))]
        public bool PickerMainOrganizationHasElements => PickerMainOrganization.Any();

        #endregion

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            if (UiUser != null! && !_takingPicture)
            {
                InitAferOnAppearingOrOnActivated();
                await UiUser.WaitDataFromServerAsync().ConfigureAwait(false);
            }

            await base.OnAppearing(view).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (args is DcListDataPoint<ExUser> u)
            {
                UiUser = u;
            }
            else
            {
                UiUser = Dc.DcExUser;
            }

            if (!IsNewUser)
            {
                await UiUser.WaitDataFromServerAsync().ConfigureAwait(true);
            }

            this.InvokeOnPropertyChanged(nameof(UiUser));

            EntryFirstName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleFirstName,
                ResViewEditUser.EntryPlaceholderFirstName,
                UiUser.Data,
                nameof(UiUser.Data.FirstName),
                VmEntryValidators.ValidateFuncStringEmpty,
                () => EntryLastName.Focus(EnumVmEntrySetFocusMode.FocusAndSelect),
                50,
                !TabletMode);

            EntryLastName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleLastName,
                ResViewEditUser.EntryPlaceholderLastName,
                UiUser.Data,
                nameof(UiUser.Data.LastName),
                VmEntryValidators.ValidateFuncStringEmpty,
                maxChar: 50,
                showTitle: !TabletMode);

            EntryNewPassword = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleNewPassword,
                ResViewEditUser.EntryPlaceholderNewPassword,
                validateFunc: VmEntryValidators.ValidatePwdFunc,
                showTitle: !TabletMode);

            EntryPhoneNumber = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryPhoneTitle,
                ResViewEditUser.EntryPlaceholderPhone,
                UiUser.Data,
                nameof(UiUser.Data.PhoneNumber),
                VmEntryValidators.ValidateFuncTelephone,
                maxChar: 50,
                showTitle: !TabletMode);
            EntryPhoneNumber.Keyboard = EnumVmEntryKeyboard.Telephone;

            if (IsNewUser)
            {
                if (!Towns.Any())
                {
                    Towns = TownHelper.GetTowns().Where(x => x.IsMainPostalCode).ToList();
                }

                EntryTownSearch = new VmEntry(EnumVmEntryBehavior.StopTyping,
                    ResViewEditUser.EntryTitleTownSearch,
                    ResViewEditUser.EntryPlaceholderTownSearch,
                    this,
                    nameof(TownSearchText),
                    ValidateFuncStringSearch,
                    maxChar: 50,
                    showTitle: !TabletMode);

                EntryTownSearch.ValidChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.Valid)
                    {
                        _ = Task.Run(UpdatePickerTowns);
                    }
                };

                //  Alle bisherigen Gemeinden laden für Zuweisung
                try
                {
                    await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmEditUser)}]({nameof(OnActivated)}): {e}");
                }

                PickerTowns.SelectedItemChanged += PickerTownsOnSelectedItemChanged;
            }
            else
            {
                InitPickerMainOrganization();
            }

            UiUser.DataChangedEvent += (sender, a) =>
            {
                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            };

            EntryPhoneNumber.PropertyChanged += (sender, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(VmEntry.BindingData))
                {
                    #region TelNr

                    if (EntryPhoneNumber.BindingData.StartsWith("00"))
                    {
                        EntryPhoneNumber.BindingData = "+43" + EntryPhoneNumber.BindingData.Substring(2);
                    }
                    else if (EntryPhoneNumber.BindingData.StartsWith("0"))
                    {
                        EntryPhoneNumber.BindingData = "+43" + EntryPhoneNumber.BindingData.Substring(1);
                    }

                    #endregion
                }
            };

            EntryLastName.ValidChanged += (sender, a) =>
            {
                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            };
            EntryFirstName.ValidChanged += (sender, a) =>
            {
                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            };
            EntryPhoneNumber.ValidChanged += (sender, a) =>
            {
                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            };
            EntryNewPassword.ValidChanged += (sender, a) =>
            {
                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            };

            InitAferOnAppearingOrOnActivated();

            await base.OnActivated(args).ConfigureAwait(true);
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            if (!_takingPicture)
            {
                if (CheckSaveBehavior == null)
                {
                    UiUser.EndEdit();
                }
                else
                {
                    UiUser.EndEdit(true);
                }
            }

            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Bild bearbeiten
        /// </summary>
        public VmCommand CmdEditPicture { get; set; } = null!;

        /// <summary>
        ///     Bild Löschen
        /// </summary>
        public VmCommand CmdDeletePicture { get; set; } = null!;

        /// <summary>
        ///     Neuen User einloggen
        /// </summary>
        public VmCommand CmdLoginUser { get; set; } = null!;

        /// <summary>
        ///     Region verwalten
        /// </summary>
        public VmCommand CmdEditRegion { get; set; } = null!;

        /// <summary>
        ///     Passwort togglen
        /// </summary>
        public VmCommand CmdTogglePassword { get; set; } = null!;

        private object? savingLock;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand(ResCommon.CmdSave, async () =>
            {
                if (savingLock != null)
                {
                    return;
                }

                savingLock = new object();

                View.BusySet();

                if (!await CheckConnected().ConfigureAwait(true))
                {
                    savingLock = null;
                    View.BusyClear();
                    return;
                }

                var mainOrg = UiUser.Data.Permissions
                    .FirstOrDefault(x => x.IsMainCompany && x.Town != null!);

                if (!EntryLastName.DataValid ||
                    !EntryFirstName.DataValid ||
                    !EntryPhoneNumber.DataValid ||
                    (IsNewUser && !EntryNewPassword.DataValid) ||
                    (mainOrg == null && !UiUser.Data.IsSysAdmin))
                {
                    var error = !EntryFirstName.DataValid ? $"\n{ResViewEditUser.MsgErrorFirstName}" :
                        !EntryLastName.DataValid ? $"\n{ResViewEditUser.MsgErrorLastName}" :
                        !EntryPhoneNumber.DataValid ? $"\n{ResViewEditUser.MsgErrorPhone}" :
                        (IsNewUser && !EntryNewPassword.DataValid) ? $"\n{ResViewEditUser.MsgErrorPassword}" :
                        (mainOrg == null && !UiUser.Data.IsSysAdmin) ? $"\n{ResViewEditUser.MsgErrorTown}" : "";

                    await MsgBox.Show(ResCommon.MsgCannotSave + error, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    savingLock = null;
                    View.BusyClear();
                    return;
                }

                //Neues Bild?
                if (_newImage != null)
                {
                    var newImage = await Dc.TransferFile(_newImage.Name, new MemoryStream(_newImage.Bytes), string.Empty).ConfigureAwait(true);
                    if (newImage.StoreResult.DataOk && newImage.DbId.HasValue)
                    {
                        UiUser.Data.UserImageDbId = newImage.DbId.Value;
                        UiUser.Data.UserImageLink = newImage.FileLink;
                    }
                    else
                    {
                        Logging.Log.LogError($"[VmEditUser:CmdSave] StoreImage result is false. Type: {newImage.StoreResult.ErrorType} Msg: {newImage.StoreResult.ServerExceptionText}");
                        await MsgBox.Show(ResCommon.MsgCannotSave + $"\n{ResViewEditUser.MsgErrorImage}", ResCommon.MsgTitleError).ConfigureAwait(true);
                        savingLock = null;
                        View.BusyClear();
                        return;
                    }
                }

                // Neue Gemeinde
                if (mainOrg != null && mainOrg.CompanyId < 0)
                {
                    // Company nicht in DC - anlegen
                    try
                    {
                        await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmEditUser)}]({nameof(InitializeCommands)}): {e}");
                    }

                    var mainTown = mainOrg.Town!;

                    if (!Dc.DcExOrganization.Any(x =>
                            x.Data.Name == mainTown.Name &&
                            x.Data.PostalCode == mainTown.PostalCode))
                    {
                        // Add Organization
                        var dcOrg = new DcListDataPoint<ExOrganization>(new ExOrganization
                                                                        {
                                                                            PostalCode = mainTown.PostalCode,
                                                                            Name = mainTown.Name,
                                                                            OrganizationType = EnumOrganizationTypes.PublicOrganization,
                                                                        });
                        Dc.DcExOrganization.Add(dcOrg);
                        var res = await Dc.DcExOrganization.StoreAll().ConfigureAwait(true);
                        if (!res.DataOk)
                        {
                            await MsgBox.Show("Speicherfehler! (Organization) " + res.ServerExceptionText).ConfigureAwait(true);
                            savingLock = null;
                            View.BusyClear();
                            return;
                        }

                        mainOrg.CompanyId = dcOrg.Index;
                        mainOrg.Town = dcOrg.Data;
                    }
                    else
                    {
                        var dcOrg = Dc.DcExOrganization.FirstOrDefault(x =>
                            x.Data.Name == mainTown.Name &&
                            x.Data.PostalCode == mainTown.PostalCode);

                        mainOrg.CompanyId = dcOrg.Index;
                        mainOrg.Town = dcOrg.Data;
                    }
                }

                if (IsNewUser)
                {
                    UiUser.Data.PasswordHash4NewUser = AppCrypt.CumputeHash(EntryNewPassword.Value);
                    UiUser.Data.DefaultLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                }

                var r = await UiUser.StoreData().ConfigureAwait(true);
                if (!r.DataOk)
                {
                    Logging.Log.LogError($"[VmEditUser:CmdSave] StoreData result is false. Type: {r.ErrorType} Msg: {r.ServerExceptionText}");
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleError).ConfigureAwait(true);
                    savingLock = null;
                    View.BusyClear();
                    return;
                }

                CheckSaveBehavior = null;
                ViewResult = true;

                if (IsNewUser)
                {
                    View.CmdSaveHeader = null;
                    CmdLoginUser.IsVisible = true;
                    await MsgBox.Show(ResViewLogin.MsgConfirmationSent, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                }
                else
                {
                    await Nav.Back().ConfigureAwait(true);
                }

                savingLock = null;
                View.BusyClear();
            }, CanExecuteCmdSave, glyph: Glyphs.Floppy_disk);

            CmdEditRegion = new VmCommand(ResViewEditUser.CmdEditRegion, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    await MsgBox.Show(ResCommon.MsgTitleError, ResCommon.MsgTitleServerError).ConfigureAwait(false);
                }

                _takingPicture = true;

                await Nav.ToViewWithResult(typeof(VmEditUserRegion), UiUser).ConfigureAwait(true);

                _takingPicture = false;

                InitPickerMainOrganization();
            });

            CmdEditPicture = new VmCommand("bearbeiten", async () =>
            {
                _takingPicture = true;
                DcDoNotAutoDisconnect = true;

                var res = await Nav.ToViewWithResult(typeof(VmImageEditor), new ImageEditRequest
                                                                            {
                                                                                File = (CurrentImage as ExFile) ?? new ExFile
                                                                                                                   {
                                                                                                                       DownloadLink = (CurrentImage as string) ?? string.Empty,
                                                                                                                   },
                                                                                IsIdeaPicture = false,
                                                                                IsProfilePicture = true,
                                                                            }).ConfigureAwait(true);

                if (res is ExFile file)
                {
                    CurrentImage = _newImage = file;
                    UiUser.Data.UserImageDbId = -2;
                }

                _takingPicture = false;
                DcDoNotAutoDisconnect = false;

                if (!IsLoaded)
                {
                    IsLoaded = true;
                }

                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            }, glyph: Glyphs.Pencil);

            CmdDeletePicture = new VmCommand(ResViewEditUser.CmdDeleteImage, () =>
            {
                UiUser.Data.UserImageLink = string.Empty;
                UiUser.Data.UserImageDbId = -3;
                CurrentImage = string.Empty;

                CheckCommandsCanExecute();
                View.CmdSaveHeader?.CanExecute();
            }, glyph: Glyphs.Bin_2);

            CmdLoginUser = new VmCommand(ResViewEditUser.CmdLoginUser, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                View.BusySet(ResViewEditUser.BsyCheckAccount, 0);

                var checkUser = await Dc.CheckUserLoginName(UiUser.Data.LoginName).ConfigureAwait(true);
                if (checkUser != null && checkUser.UserId != -1)
                {
                    if (checkUser.LoginConfirmed)
                    {
                        var pwdCheck = await Dc.CheckUserPassword(checkUser.UserId, AppCrypt.CumputeHash(EntryNewPassword.Value)).ConfigureAwait(true);
                        if (pwdCheck)
                        {
                            UiUser.Update();
                            CurrentVmMenu?.UpdateMenu();
                            View.BusyClear();
                            Dispatcher!.RunOnDispatcher(() => GCmdHome.Execute(null!));
                        }
                    }
                    else
                    {
                        View.BusyClear();
                        await MsgBox.Show(ResViewLogin.MsgUserNotValidated, ResCommon.MsgTitleEmail).ConfigureAwait(true);
                    }
                }

                View.BusyClear();
            });

            CmdTogglePassword = new VmCommand(string.Empty, () => { ShowEntriesAsPassword = !ShowEntriesAsPassword; });
        }

        #endregion
    }
}