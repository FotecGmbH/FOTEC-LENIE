// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BaseApp.ViewModel.Idea;
using BaseApp.ViewModel.Settings;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange.Enum;
using Exchange.Model;
using Exchange.Model.Organization;
using Exchange.Model.User;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace BaseApp.ViewModel.User
{
    #region Hilfsklassen

    /// <summary>
    ///     <para>Zugriffsrechte auf Firmen des User</para>
    ///     Klasse UiPermission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UiPermission
    {
        #region Properties

        /// <summary>
        ///     DB ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Rolle des Users in der Firma
        /// </summary>
        public EnumUserRole UserRole { get; set; }

        /// <summary>
        ///     Rechte des Users bei der Firma
        /// </summary>
        public EnumUserRight UserRight { get; set; }

        /// <summary>
        ///     Hauptwohnsitz für User
        /// </summary>
        public bool IsMainCompany { get; set; }

        /// <summary>
        ///     Firma
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        ///     Region
        /// </summary>
        public ExOrganization? Organization { get; set; }

        /// <summary>
        ///     Ui Rolle
        /// </summary>
        public string UserRoleUi
        {
            get
            {
                switch (UserRole)
                {
                    case EnumUserRole.User:
                        return ResCommon.EnumUserRoleUser;
                    case EnumUserRole.UserPlus:
                        return ResCommon.EnumUserRoleUserPlus;
                    case EnumUserRole.Admin:
                        return ResCommon.EnumUserRoleAdmin;
                    default:
                        return ResCommon.LblUnknown;
                }
            }
        }

        /// <summary>
        ///     Ui Rechte
        /// </summary>
        public string UserRightUi
        {
            get
            {
                switch (UserRight)
                {
                    case EnumUserRight.Read:
                        return ResCommon.EnumUserRightRead;
                    case EnumUserRight.ReadWrite:
                        return ResCommon.EnumUserRightReadWrite;
                    default:
                        return ResCommon.LblUnknown;
                }
            }
        }

        #endregion
    }

    #endregion

    /// <summary>
    ///     <para>View Model für Benutzer</para>
    ///     Klasse VmUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewUser", true)]
    public class VmUser : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmUser.DesignInstance}"
        /// </summary>
        public static VmUser DesignInstance = new VmUser();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static ExUserDevice DesignInstanceExUserDevice = new ExUserDevice();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static UiPermission DesignInstanceExUserPremission = new UiPermission();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static ExAccessToken DesignInstanceExAccessToken = new ExAccessToken();

        /// <summary>
        ///     VmUser
        /// </summary>
        public VmUser() : base(ResViewUser.LblTitle, subTitle: ResViewUser.LblSubTitle)
        {
            SetViewProperties();
            WaitForProjectDataLoadedAfterDcConnected = true;
            View.ShowUser = false;

            EntrySmsCode = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewUser.EntrySmsTitle,
                ResViewUser.EntrySmsPlaceholder,
                returnAction: () => CmdConfirmSms.Execute(null!),
                showMaxChar: false);
        }

        #region Properties

        /// <summary>
        ///     Auswahl Haupt gemeinde
        /// </summary>
        public VmPicker<UiPermission> PickerMainOrganization { get; } = new VmPicker<UiPermission>(nameof(PickerMainOrganization));

        /// <summary>
        ///     Zugriffsrechte
        /// </summary>
        public ObservableCollection<UiPermission> UiPermissions { get; set; } = new ObservableCollection<UiPermission>();

        /// <summary>
        ///     Anzahl der Gemeinden
        /// </summary>
        [DependsOn(nameof(UiPermissions))]
        public int PermissionCount
        {
            get => UiPermissions.Count;
        }

        /// <summary>
        ///     Benutzer Id des User
        /// </summary>
        public string UserId => $"{ResViewUser.LblUserId} {Dc.CoreConnectionInfos.UserId}";

        /// <summary>
        ///     Geräte Id des User
        /// </summary>
        public string DeviceId => $"{ResViewUser.LblDeviceId} {Dc.CoreConnectionInfos.DeviceId}";

        /// <summary>
        ///     Lädt Daten
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        ///     Eingabe SMS bestätigungscode
        /// </summary>
        public VmEntry EntrySmsCode { get; set; }

        #endregion

        private async Task LoadUiPermissions()
        {
            try
            {
                PickerMainOrganization.SelectedItemChanged -= PickerMainOrganizationOnSelectedItemChanged;

                UiPermissions.Clear();
                PickerMainOrganization.Clear();

                if (Dc.DcExUser.DataSource == EnumDcDataSource.NotLoaded ||
                    Dc.DcExUser.DataSource == EnumDcDataSource.LocalDefault)
                {
                    Logging.Log.LogInfo($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): Load UserData");
                    await Dc.DcExUser.WaitDataFromServerAsync().ConfigureAwait(true);
                }

                if (Dc.DcExUser.Data.IsSysAdmin)
                {
                    if (!Dc.DcExOrganization.SyncedSinceUserRegistered)
                    {
                        Logging.Log.LogInfo($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): Load Orgs");
                        try
                        {
                            await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): {e}");
                        }
                    }

                    foreach (var company in Dc.DcExOrganization
                                 .Where(x => x.Data.OrganizationType != EnumOrganizationTypes.NoOrganization)
                                 .OrderBy(x => x.Data.Name))
                    {
                        UiPermissions.Add(new UiPermission
                                          {
                                              CompanyId = company.Index,
                                              Organization = company.Data,
                                              Company = company.Data.Name,
                                              UserRight = EnumUserRight.ReadWrite,
                                              UserRole = EnumUserRole.Admin,
                                              IsMainCompany = Dc.DcExUser.Data.Permissions.FirstOrDefault(x => x.IsMainCompany)?.CompanyId == company.Index,
                                          });
                    }
                }
                else
                {
                    foreach (var permission in Dc.DcExUser.Data.Permissions.OrderBy(x => x.Town!.Name))
                    {
                        UiPermissions.Add(new UiPermission
                                          {
                                              CompanyId = permission.CompanyId,
                                              Organization = permission.Town,
                                              Company = permission.Town!.Name,
                                              UserRight = permission.UserRight,
                                              UserRole = permission.UserRole,
                                              IsMainCompany = permission.IsMainCompany,
                                          });
                    }
                }

                Logging.Log.LogWarning($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): UiPermissions: {UiPermissions.Count()}");

                foreach (var uiPermission in UiPermissions)
                {
                    PickerMainOrganization.AddKey(uiPermission, uiPermission.Organization!.NamePlzString);
                }

                var main = UiPermissions.FirstOrDefault(x => x.IsMainCompany);
                try
                {
                    UiPermissions.Remove(main);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): Workaroud - neues NuGet");
                }

                PickerMainOrganization.SelectKey(main);

                PickerMainOrganization.SelectedItemChanged += PickerMainOrganizationOnSelectedItemChanged;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmUser)}]({nameof(LoadUiPermissions)}): {e}");
            }
        }

        private void PickerMainOrganizationOnSelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<UiPermission>> e)
        {
            // Hauptgemeinde geändert
            _ = Task.Run(() =>
            {
                Task.Delay(100);
                Dispatcher!.RunOnDispatcher(() => { CmdSetMainCompany.Execute(e.CurrentItem.Key); });
            });
        }

        private async void OnLogoutClientDcConnected(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                DcOnDeviceOnlineConnectedForUpdateDeviceInfos(null!, e);
                await LoadUiPermissions().ConfigureAwait(true);
                Dc.ConnectionChanged -= OnLogoutClientDcConnected;
            }
        }

        private async void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExUser.Permissions))
            {
                await LoadUiPermissions().ConfigureAwait(true);
            }
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            Dc.DcExOrganization.FilterClear();

            await base.OnAppearing(view).ConfigureAwait(true);
            await LoadUiPermissions().ConfigureAwait(true);
            Dc.DcExUser.Data.PropertyChanged += DataOnPropertyChanged;
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.DcExUser.Data.PropertyChanged -= DataOnPropertyChanged;
            return base.OnDisappearing(view);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdLogout = new VmCommand(ResViewUser.CmdLogout, async () =>
            {
                ProjectDataLoadedAfterDcConnected = false;
                Dc.ConnectionChanged += OnLogoutClientDcConnected;
                VmMainIdea.ClearFilters();
                await Dc.Logout().ConfigureAwait(true);
                CurrentVmMenu?.UpdateMenu();
                Nav.ClearCachedPages();
                GCmdHome.Execute(null!);
            });

            CmdEdit = new VmCommand(ResViewUser.CmdEdit, async () => { await Nav.ToViewWithResult(typeof(VmEditUser)).ConfigureAwait(false); });

            CmdChangePassword = new VmCommand(ResViewUser.CmdChangePassword, async () => { await Nav.ToViewWithResult(typeof(VmEditUserPassword)).ConfigureAwait(false); });

            #region Token

            CmdAddToken = new VmCommand(ResCommon.CmdAddToken, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                var g = Guid.NewGuid().ToString();
                g += Guid.NewGuid().ToString();
                g = g.Replace("-", "", StringComparison.InvariantCultureIgnoreCase).Trim();

                var t = new ExAccessToken
                        {
                            DbId = 0,
                            GuiltyUntilUtc = DateTime.UtcNow.AddYears(1),
                            Token = g
                        };
                Dc.DcExUser.Data.Tokens.Add(t);

                View.BusySet(ResCommon.BusySaveToken);
                var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
                View.BusyClear();
                if (!(r is {DataOk: true}))
                {
                    await MsgBox.Show($"{ResCommon.MsgErrorSaveToken} {r.ServerExceptionText}").ConfigureAwait(true);
                    try
                    {
                        Dc.DcExUser.Data.Tokens.Remove(t);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmUser)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }
                }
            });

            CmdDeleteToken = new VmCommand("", async t =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (t is ExAccessToken token)
                {
                    try
                    {
                        Dc.DcExUser.Data.Tokens.Remove(token);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmUser)}]({nameof(InitializeCommands)}): Workaroud - neues NuGet");
                    }

                    View.BusySet(ResCommon.BusyDeleteToken);
                    var r = await Dc.DcExUser.StoreData().ConfigureAwait(true);
                    View.BusyClear();
                    if (!(r is {DataOk: true}))
                    {
                        await MsgBox.Show($"{ResCommon.MsgErrorDeleteToken} {r.ServerExceptionText}").ConfigureAwait(true);
                        Dc.DcExUser.Data.Tokens.Add(token);
                    }
                }
            }, glyph: Glyphs.Bin_2);

            CmdCopyToken = new VmCommand("", async t =>
            {
                if (t is ExAccessToken token)
                {
                    View.BusySet(ResCommon.BusyCopyToken, 200);
                    var r = false;
                    await Task.Run(async () => { r = await Open.ClipboardSetText(token.Token).ConfigureAwait(false); }).ConfigureAwait(true);
                    View.BusyClear();
                    if (r)
                    {
                        await MsgBox.Show(ResCommon.MsgTokenInClipboard).ConfigureAwait(true);
                    }
                    else
                    {
                        Logging.Log.LogError($"[{nameof(VmUser)}]({nameof(InitializeCommands)}): Copy to clipboard fails!");
                    }
                }
            }, glyph: Glyphs.Copy_paste_1);

            #endregion

            #region Region

            CmdRemoveCompany = new VmCommand(ResViewUser.CmdEdit, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    return;
                }

                if (p is UiPermission {Organization: { }} permission)
                {
                    if (permission.IsMainCompany)
                    {
                        await MsgBox.Show(ResViewEditUserRegion.MsgErrorTextDefaultRegionRemoved, ResViewEditUserRegion.MsgErrorCaptionDefaultRegionRemoved).ConfigureAwait(true);
                    }
                    else
                    {
                        var res = await MsgBox.Show(ResViewUser.MsgConfirmCompanyDelete, button: VmMessageBoxButton.YesNo).ConfigureAwait(true);
                        if (res != VmMessageBoxResult.Yes)
                        {
                            return;
                        }

                        var toRemove = Dc.DcExUser.Data.Permissions.FirstOrDefault(t => t.Town == permission.Organization);
                        if (toRemove != null)
                        {
                            Dc.DcExUser.Data.Permissions.Remove(toRemove);
                            await Dc.DcExUser.StoreData().ConfigureAwait(true);
                            await LoadUiPermissions().ConfigureAwait(true);
                        }

                        try
                        {
                            await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmUser)}]({nameof(InitializeCommands)}): {e}");
                        }
                    }
                }
            }, glyph: Glyphs.Bin);

            CmdSetMainCompany = new VmCommand(ResViewUser.CmdEdit, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    return;
                }

                if (p is UiPermission permission)
                {
                    var newMain = Dc.DcExUser.Data.Permissions
                        .FirstOrDefault(t => t.CompanyId == permission.CompanyId);

                    if (newMain == null)
                    {
                        await MsgBox.Show("nicht möglich für Superadmin!", ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                        return;
                    }

                    foreach (var perm in Dc.DcExUser.Data.Permissions)
                    {
                        perm.IsMainCompany = false;
                    }

                    newMain.IsMainCompany = true;

                    await Dc.DcExUser.StoreData().ConfigureAwait(true);
                    await LoadUiPermissions().ConfigureAwait(true);
                }
            }, glyph: Glyphs.House_2);

            CmdEditRegion = new VmCommand(ResViewUser.CmdEdit, async () => { await Nav.ToViewWithResult(typeof(VmEditUserRegion)).ConfigureAwait(false); }, glyph: Glyphs.Add_circle);

            #endregion

            GCmdHeaderCommon = new VmCommand(ResViewOrganization.CmdReloadOrganizations, async () =>
            {
                IsRefreshing = true;

                Dispatcher!.RunOnDispatcher(() => View.BusySet("Aktualisiere Daten ..."));

                await Dc.DcExUser.WaitDataFromServerAsync(forceUpdate: true).ConfigureAwait(true);

                try
                {
                    await Dc.DcExOrganization.Sync().ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmUser)}]({nameof(InitializeCommands)}): {e}");
                }

                Dispatcher.RunOnDispatcher(() => View.BusyClear());

                IsRefreshing = false;
            }, glyph: Glyphs.Cloud_refresh);

            CmdSettingsPush = new VmCommandSelectable(ResViewSettings.CmdPushSettings, async () => { await Nav.ToViewWithResult(typeof(VmSettingsPush)).ConfigureAwait(true); });

            CmdMyIdeas = new VmCommand(ResViewUser.CmdMyIdeas, () =>
            {
                VmMainIdea.SetFilterMyIdeas();
                GCmdHome.IsSelected = true;
            });

            CmdConfirmSms = new VmCommand("SMS Code bestätigen", async () =>
            {
                var r = await Dc.ConfirmSmsCode(EntrySmsCode.Value).ConfigureAwait(true);
                if (r.Ok && r.Data == true.ToString())
                {
                    // Success
                    await MsgBox.Show(ResViewUser.MsgConfirmSms, ResViewUser.MsgConfirmSmsTitle).ConfigureAwait(true);
                }
                else
                {
                    // Fehler falscher Code
                    await MsgBox.Show(ResViewUser.MsgConfirmSmsError, ResCommon.MsgTitleError).ConfigureAwait(true);
                }
            });

            CmdResendSms = new VmCommand("SMS Code erneut senden", async () =>
            {
                var r = await Dc.ResendConfirmationSms(Dc.CoreConnectionInfos.UserId).ConfigureAwait(true);
                if (r.Ok)
                {
                    await MsgBox.Show(ResViewUser.MsgResendSms, ResViewUser.MsgResendSmsTitle).ConfigureAwait(true);
                }
                else
                {
                    Logging.Log.LogWarning($"[CmdForgotPassword] Error: {r.ServerExceptionText}");
                    await MsgBox.Show(ResViewUser.MsgResendSmsError, ResCommon.MsgTitleError).ConfigureAwait(true);
                }
            });
        }

        /// <summary>
        ///     Test Command
        /// </summary>
        public VmCommand CmdLogout { get; set; } = null!;

        /// <summary>
        ///     Benutzer bearbeiten
        /// </summary>
        public VmCommand CmdEdit { get; set; } = null!;

        /// <summary>
        ///     Passwort ändern
        /// </summary>
        public VmCommand CmdChangePassword { get; set; } = null!;

        /// <summary>
        ///     Token hinzufügen
        /// </summary>
        public VmCommand CmdAddToken { get; private set; } = null!;

        /// <summary>
        ///     Token löschen
        /// </summary>
        public VmCommand CmdDeleteToken { get; private set; } = null!;

        /// <summary>
        ///     Token in die Zwischenablage kopieren
        /// </summary>
        public VmCommand CmdCopyToken { get; private set; } = null!;

        /// <summary>
        ///     Region als Hauptregion setzen
        /// </summary>
        public VmCommand CmdSetMainCompany { get; private set; } = null!;

        /// <summary>
        ///     Region entfernen
        /// </summary>
        public VmCommand CmdRemoveCompany { get; private set; } = null!;

        /// <summary>
        ///     Region Bearbeiten
        /// </summary>
        public VmCommand CmdEditRegion { get; private set; } = null!;

        /// <summary>
        ///     Benachrichtigungseinstellungen
        /// </summary>
        public VmCommand CmdSettingsPush { get; set; } = null!;

        /// <summary>
        ///     Meine Ideen
        /// </summary>
        public VmCommand CmdMyIdeas { get; set; } = null!;

        /// <summary>
        ///     Sms Code erneut senden
        /// </summary>
        public VmCommand CmdResendSms { get; set; } = null!;

        /// <summary>
        ///     Sms Code bestätigen
        /// </summary>
        public VmCommand CmdConfirmSms { get; set; } = null!;

        #endregion
    }
}