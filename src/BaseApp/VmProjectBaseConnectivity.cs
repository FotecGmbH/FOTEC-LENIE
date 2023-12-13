// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using BaseApp.ViewModel.Idea;
using BaseApp.ViewModel.User;
using Biss.Apps.Connectivity;
using Biss.Apps.Enum;
using Biss.Common;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange;
using Exchange.Enum;
using Exchange.Model;
using Exchange.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BaseApp
{
    /// <summary>
    ///     <para>Gemeinsame Methoden für Connectivity / DC</para>
    ///     Klasse VmProjectBaseConnectivity. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        /// <summary>
        ///     Für OnSleep
        /// </summary>
        public static bool DcDoNotAutoDisconnect = false;

        /// <summary>
        ///     Wurde (diese) allgemeine Meldung bereits angezeigt?
        /// </summary>
        private static string _lastShownCommonMessageInSession = string.Empty;

        /// <summary>
        ///     Wurde die Meldung das es (diese) Neue Version bereits gibt?
        /// </summary>
        private static Version? _lastVersionChecked;

        private static bool _projectDataLoadedAfterDcConnected;
        private bool _isReconnectedInSession;

        #region Properties

        /// <summary>
        ///     Projektbezogene Daten geladen
        /// </summary>
        public bool ProjectDataLoadedAfterDcConnected
        {
            get => _projectDataLoadedAfterDcConnected;
            set => _projectDataLoadedAfterDcConnected = value;
        }

        #endregion

        /// <summary>
        ///     Ereignis für Projekt Daten geladen
        /// </summary>
        public static event EventHandler? ProjectDataLoaded;

        /// <summary>
        ///     Auto Connect starten - in der richtigen View ausführen
        /// </summary>
        public void DcStartAutoConnect()
        {
            try
            {
                CurrentVmMenu?.UpdateMenu();
                _ = Task.Run(() =>
                {
                    Dispatcher!.RunOnDispatcher(async () =>
                    {
                        Dc.DeviceOnlineConnected += DcOnDeviceOnlineConnectedForUpdateDeviceInfos;
                        Dc.UserAndDeviceOnlineConnected += DcOnUserAndDeviceOnlineConnected;

                        try
                        {
                            await Dc.OpenConnection(true).ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(DcStartAutoConnect)}): {e}");
                        }
                    });
                });
                Dc.CommonDataFromServerReceived += DcOnCommonDataFromServerReceived;
                Dc.DcExSettingsInDb.Data.PropertyChanged += DataOnPropertyChanged;
                Dc.ConnectionChanged += DcOnConnectionChanged;
                Dc.Chat.ChatError += Chat_ChatError;
                Dc.Error += Dc_Error;
                Dc.Chat.PropertyChanged += Chat_PropertyChanged;
                Dc.LogoutByDc += DcOnLogoutByDc;
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(DcStartAutoConnect)}): {e}");
                LogCrash4Appcenter(e);
            }
        }

        /// <summary>
        ///     Prüfen ob gerade eine Verbindung mit dem Server besteht inklusive MsgBox Ausgabe.
        /// </summary>
        /// <returns>Verbunden</returns>
        public async Task<bool> CheckConnected()
        {
            if (Dc.ConnectionState != EnumDcConnectionState.Connected)
            {
                await MsgBox.Show(ResCommon.MsgNotConnected, ResCommon.MsgTitleNotConnected).ConfigureAwait(true);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Warten bis der DC gestartet und verbunden ist.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForConnected()
        {
            if (!Dc.AutoConnect)
            {
                DcStartAutoConnect();
            }

            if (Dc.ConnectionState != EnumDcConnectionState.Connected)
            {
                var connectionWaiter = new AsyncAutoResetEvent();

                Dc.ConnectionChanged += (sender, state) =>
                {
                    if (state == EnumDcConnectionState.Connected)
                    {
                        connectionWaiter.Set();
                    }
                };

                await connectionWaiter.WaitOne().ConfigureAwait(true);

                // Warten, damit auch alle Properties gesetzt sind
                await Task.Delay(200).ConfigureAwait(true);
            }

            // TODO sollte nochmal syncen oder so,damit wirklich überall gespeichert is, dass der DC jetzt online ist!
        }

        /// <summary>
        ///     Check ob der User seine Telefonnummer bestätigt hat - bei nein, weiterleiten zu "Ich"
        /// </summary>
        /// <returns></returns>
        public async Task CheckPhoneConfirmation()
        {
            if (Dc.CoreConnectionInfos.UserOk &&
                Dc.DcExUser.DataSource != EnumDcDataSource.LocalDefault &&
                !Dc.DcExUser.Data.PhoneConfirmed &&
                !Dc.DcExUser.Data.IsSysAdmin)
            {
                if (CurrentVmMenu != null && CurrentVmMenu.CmdAllMenuCommands.Contains(View.GCmdUser))
                {
                    if (!View.GCmdUser.IsSelected)
                    {
                        View.GCmdUser.IsSelected = true;
                        await MsgBox.Show(ResCommon.MsgPhoneConfirmation, ResCommon.MsgPhoneConfirmationTitle).ConfigureAwait(true);
                    }
                }
                // User button nicht im Menü?
            }
        }

        /// <summary>
        ///     Methode von Ereignis für Projekt Daten geladen
        /// </summary>
        protected virtual void OnProjectDataLoaded()
        {
            var handler = ProjectDataLoaded;
            handler?.Invoke(this, null!);
        }

        /// <summary>
        ///     Sobald das Gerät online verbunden ist die Gerätestammdaten am Server aktualisieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void DcOnDeviceOnlineConnectedForUpdateDeviceInfos(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                await DeviceInfoUpdate().ConfigureAwait(false);
                await Dc.DcExSettingsInDb.WaitDataFromServerAsync().ConfigureAwait(false);
                CheckSettingsInDb();

                //App ohne User bzw. nicht angemeldet
                if (!Dc.CoreConnectionInfos.UserOk)
                {
                    UpdateProjektData();
                }
            }
        }

        private void Dc_Error(object sender, ServerErrorEventArgs e)
        {
            Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(Dc_Error)}): {e.ServerErrorType} - {e.ServerExceptionText}");
        }

        private void Chat_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Dc.Chat.AllUnreadMessages))
            {
                Dispatcher!.RunOnDispatcher(() => { Push.SetBadgeCount(Dc.Chat.AllUnreadMessages); });
            }
        }

        private void Chat_ChatError(object sender, ServerErrorEventArgs e)
        {
            Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(Chat_ChatError)}): {e.Pid} {e.ServerErrorType} - {e.ServerExceptionText}");

            LogCrash4Appcenter(new Exception(e.ServerExceptionText), JsonConvert.SerializeObject(e));
        }

        /// <summary>
        ///     DC Logt User automatisch ab wenn Account nicht mehr passt (oder gelöscht wurde)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DcOnLogoutByDc(object sender, EventArgs e)
        {
            Dispatcher!.RunOnDispatcher(async () =>
            {
                var isOnLogin = (GetCurrentView?.GetViewModel() is VmLogin);

                if (!isOnLogin)
                {
                    await MsgBox.Show("Automatischer Login mit dem aktuellen Benutzer ist nicht möglich.", "Login nicht möglich").ConfigureAwait(true);
                }

                CurrentVmMenu?.UpdateMenu();
                if (!isOnLogin)
                {
                    Nav.ClearCachedPages();
                    GCmdHome.Execute(null!);
                }
            });
        }

        #region Projektabhängig Daten laden

        /// <summary>
        ///     Listen bei eingeloggten User laden wenn dieser (wieder) eingeloggt wird
        /// </summary>
        private void UpdateProjektData()
        {
            Dispatcher!.RunOnDispatcher(() =>
            {
                //App ohne User bzw. nicht angemeldet
                Push.SetBadgeCount(Dc.Chat.AllUnreadMessages);

                _ctsLoaded?.Cancel();
                ProjectDataLoadedAfterDcConnected = true;
                OnProjectDataLoaded();
            });
        }

        #endregion

        /// <summary>
        ///     Verbindungs - Header
        /// </summary>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void UpdateConnectionState(EnumDcConnectionState e)
        {
            if (Dc.ConnectingCounter <= 1)
            {
                View.EnumHeaderOfflineType = EnumHeaderInfo.None;
                return;
            }

            switch (e)
            {
                case EnumDcConnectionState.Connected:
                    View.EnumHeaderOfflineType = EnumHeaderInfo.None;
                    break;
                case EnumDcConnectionState.Disconnected:
                    View.EnumHeaderOfflineType = EnumHeaderInfo.Warning;
                    View.MessageOffline = ResCommon.EnumConnDisonnected;
                    break;
                case EnumDcConnectionState.Connecting:
                    View.EnumHeaderOfflineType = EnumHeaderInfo.Info;
                    View.MessageOffline = ResCommon.EnumConnConnecting;
                    break;
                case EnumDcConnectionState.Disconeccting:
                    View.EnumHeaderOfflineType = EnumHeaderInfo.None;
                    View.MessageOffline = ResCommon.EnumConnDisconnecting;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e, null);
            }
        }

        /// <summary>
        ///     Verbindungsstatus hat sich geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcOnConnectionChanged(object sender, EnumDcConnectionState e)
        {
            if (e == EnumDcConnectionState.Connected)
            {
                if (_isReconnectedInSession)
                {
                    Dc.UserAndDeviceOnlineConnected += DcOnUserAndDeviceOnlineConnected;
                }

                _isReconnectedInSession = true;
            }
            else if (e == EnumDcConnectionState.Disconnected)
            {
                ProjectDataLoadedAfterDcConnected = false;
            }
        }

        /// <summary>
        ///     Neue DbSettings wurden vom Server empfangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckSettingsInDb();
        }

        /// <summary>
        ///     Versionsüberprüfung und allgemeine Server Meldung
        /// </summary>
        private void CheckSettingsInDb()
        {
            if (Dc.DcExSettingsInDb.DataSource != EnumDcDataSource.FromServer)
            {
                return;
            }

            //Allgemeine Meldung als MsgBox und als Info - Projektabhängig finalisieren
            if (!string.IsNullOrEmpty(Dc.DcExSettingsInDb.Data.CommonMessage))
            {
                if (string.IsNullOrEmpty(_lastShownCommonMessageInSession) ||
                    !string.Equals(_lastShownCommonMessageInSession, Dc.DcExSettingsInDb.Data.CommonMessage, StringComparison.CurrentCultureIgnoreCase))
                {
                    _lastShownCommonMessageInSession = Dc.DcExSettingsInDb.Data.CommonMessage;
                    Logging.Log.LogTrace($"[Common Message]: {_lastShownCommonMessageInSession}");
                    Dispatcher!.RunOnDispatcher(async () => { await MsgBox.Show(_lastShownCommonMessageInSession, ResCommon.CmdInfo).ConfigureAwait(true); });
                }

                Dispatcher!.RunOnDispatcher(() => View.SetMessage(Dc.DcExSettingsInDb.Data.CommonMessage));
            }
            else
            {
                Dispatcher!.RunOnDispatcher(() => View.EnumHeaderInfoType = EnumHeaderInfo.None);
            }

            var currentVersion = new Version(AppSettings.Current().AppVersion);
            Logging.Log.LogTrace($"[Version]: App: {currentVersion}, DbCurrent: {Dc.DcExSettingsInDb.Data.CurrentAppVersion}, DbMin: {Dc.DcExSettingsInDb.Data.MinAppVersion}");
            if (currentVersion < Dc.DcExSettingsInDb.Data.MinAppVersion)
            {
                Dispatcher.RunOnDispatcher(async () =>
                {
                    await MsgBox.Show(string.Format(ResCommon.MsgUpdateMandatory, Dc.DcExSettingsInDb.Data.CurrentAppVersion), ResCommon.MsgTitleUpdateMandatory).ConfigureAwait(true);
                    Nav.QuitApp();
                });
            }
            else if (currentVersion < Dc.DcExSettingsInDb.Data.CurrentAppVersion)
            {
                if (_lastVersionChecked == null || _lastVersionChecked != Dc.DcExSettingsInDb.Data.CurrentAppVersion)
                {
                    Dispatcher.RunOnDispatcher(async () => { await MsgBox.Show(string.Format(ResCommon.MsgUpdateAvailable, Dc.DcExSettingsInDb.Data.CurrentAppVersion), ResCommon.MsgTitleUpdateAvailable).ConfigureAwait(true); });
                }

                _lastVersionChecked = Dc.DcExSettingsInDb.Data.CurrentAppVersion;
            }
        }

        /// <summary>
        ///     User Stammdaten haben sich geändert.
        ///     Trigger auf "Locked" mit automatischem ausloggen eines Users wenn das Gerät gerade online ist.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcExUserOnDataChangedEvent(object sender, DataChangedEventArgs e)
        {
            if (e.Changed)
            {
                if (Dc.DcExUser.Data.Locked)
                {
                    Dispatcher!.RunOnDispatcher(async () =>
                    {
                        await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                        VmMainIdea.ClearFilters();
                        await Dc.Logout().ConfigureAwait(true);
                        CurrentVmMenu?.UpdateMenu();
                        Nav.ClearCachedPages();
                        GCmdHome.Execute(null!);
                    });
                }
                else if (!Dc.DcExUser.Data.PhoneConfirmed)
                {
                    CheckPhoneConfirmation().ConfigureAwait(true);
                }
            }
        }

        /// <summary>
        ///     User (und Device) sind online (registriert)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DcOnUserAndDeviceOnlineConnected(object sender, EnumDcConnectionState e)
        {
            await Dc.DcExUser.WaitDataFromServerAsync(forceUpdate: true).ConfigureAwait(true);
            Dc.UserAndDeviceOnlineConnected -= DcOnUserAndDeviceOnlineConnected;

            Dispatcher!.RunOnDispatcher(async () =>
            {
                if (Dc.DcExUser.Data.Locked)
                {
                    await MsgBox.Show(ResCommon.MsgLocked, ResCommon.MsgTitleLocked).ConfigureAwait(true);
                    VmMainIdea.ClearFilters();
                    await Dc.Logout().ConfigureAwait(true);
                    CurrentVmMenu?.UpdateMenu();
                    Nav.ClearCachedPages();
                    GCmdHome.Execute(null!);
                }
                else
                {
                    if (!Dc.DcExUser.Data.PhoneConfirmed)
                    {
                        await CheckPhoneConfirmation().ConfigureAwait(true);
                    }

                    CurrentVmMenu?.UpdateMenu();
                    Dc.DcExUser.DataChangedEvent += DcExUserOnDataChangedEvent;

                    //// Test Deeplinks
                    //Push.AddLaunchOptions(new BissNotificationOptions()
                    //                      {
                    //                          CustomArgs = new Dictionary<string, string>()
                    //                                       {
                    //                                           //{Constants.DeepLinkChat, 1.ToString()},
                    //                                           //{Constants.DeepLinkIdea, 1.ToString()},
                    //                                       }
                    //                      });

                    if (CurrentVmMenu != null)
                    {
                        CurrentVmMenu.CheckLaunch();
                    }
                }

                UpdateProjektData();
            });
        }

        /// <summary>
        ///     Daten über das aktuelle Device an die Cloud senden. Wird unter anderem für die Notifizierungen benötigt.
        ///     Sollte aufgerufen werden wenn der User eingeloggt ist. Wenn die App keine User unterstützt:
        ///     Db und Funktionen umbauen (das die Devices ohne User angelegt werden) ODER
        ///     Einen "ALLUSER" anlegen => wenn ersichtlich das eventuell mal ein Login hinzukommt
        /// </summary>
        private async Task DeviceInfoUpdate()
        {
            if (!Dc.DeviceRegisteredOnline)
            {
                return;
            }

            if (DeviceInfo.Plattform == EnumPlattform.XamarinIos || DeviceInfo.Plattform == EnumPlattform.XamarinAndroid)
            {
                var token = string.Empty;

                try
                {
                    token = Push.Token;
                }
                catch (InvalidOperationException e)
                {
                    Logging.Log.LogError($"[Push] Token fetch failed. Error: {e}");
                }

                Dc.DcExDeviceInfo.Data.DeviceToken = string.IsNullOrEmpty(token) ? string.Empty : token;
            }

            Dc.DcExDeviceInfo.Data.DeviceHardwareId = DeviceInfo.DeviceHardwareId;
            Dc.DcExDeviceInfo.Data.Plattform = DeviceInfo.Plattform;
            Dc.DcExDeviceInfo.Data.DeviceIdiom = DeviceInfo.DeviceIdiom;
            Dc.DcExDeviceInfo.Data.OperatingSystemVersion = DeviceInfo.OperatingSystemVersion;
            Dc.DcExDeviceInfo.Data.DeviceType = DeviceInfo.DeviceType;
            Dc.DcExDeviceInfo.Data.Manufacturer = DeviceInfo.Manufacturer;
            Dc.DcExDeviceInfo.Data.Model = DeviceInfo.Model;
            Dc.DcExDeviceInfo.Data.AppVersion = AppSettings.Current().AppVersion;
            Dc.DcExDeviceInfo.Data.CurrentAppType = CurrentAppType;
            Dc.DcExDeviceInfo.Data.DeviceName = DeviceInfo.DeviceName;

            /*
            if (DeviceInfo)
            {
                try
                {
                    Dc.DcExDeviceInfo.Data.ScreenResolution = DeviceDisplay.MainDisplayInfo.Width + " x " + DeviceDisplay.MainDisplayInfo.Height + " (" + DeviceDisplay.MainDisplayInfo.Density + ")";
                }
                catch
                {
                    // ios MainThread
                }
            }
            else
            {
                Dc.DcExDeviceInfo.Data.ScreenResolution = string.Empty;
            }
            */
            if (DeviceInfo.Plattform == EnumPlattform.Wpf || DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
            {
                Dc.DcExDeviceInfo.Data.DeviceType = ResCommon.EnumDeviceTypePc;
                Dc.DcExDeviceInfo.Data.DeviceName = Environment.MachineName;
                Dc.DcExDeviceInfo.Data.OperatingSystemVersion = Environment.OSVersion.Version.Build.ToString();

                //var r = DependencyService.Get<IDeviceInfos>().GetInfosDeviceInfo();
                //Dc.DcExDeviceInfo.Data.Manufacturer = r.Manufacturer;
                //Dc.DcExDeviceInfo.Data.Model = r.Model;
                //Dc.DcExDeviceInfo.Data.ScreenResolution = r.ScreenResolution;
                //Dc.DcExDeviceInfo.Data.DeviceHardwareId = r.DeviceHardwareId;
            }

            if (DeviceInfo.Plattform == EnumPlattform.Web)
            {
                Dc.DcExDeviceInfo.Data.DeviceType = $"{ResCommon.EnumDeviceTypeBrowser} {DeviceInfo.DeviceType}";
                Dc.DcExDeviceInfo.Data.DeviceName = DeviceInfo.DeviceName;
                Dc.DcExDeviceInfo.Data.OperatingSystemVersion = DeviceInfo.OperatingSystemVersion;
                Dc.DcExDeviceInfo.Data.Manufacturer = DeviceInfo.Manufacturer;
                Dc.DcExDeviceInfo.Data.Model = DeviceInfo.Model;
                Dc.DcExDeviceInfo.Data.ScreenResolution = string.Empty;
                Dc.DcExDeviceInfo.Data.DeviceHardwareId = string.Empty;
            }

            var storeRes = await Dc.DcExDeviceInfo.StoreData().ConfigureAwait(false);
            if (!storeRes.DataOk)
            {
                Logging.Log.LogError($"[DC] DeviceInfoUpdate Error({storeRes.ErrorType}): {storeRes.ServerExceptionText}");
            }
            else
            {
                Logging.Log.LogTrace("[DC] DeviceInfo Updated");
                Dc.DeviceOnlineConnected -= DcOnDeviceOnlineConnectedForUpdateDeviceInfos;
            }
        }

        #region Connectivity

        /// <summary>
        ///     Data Connector
        /// </summary>
        public DcProjectBase Dc => this.GetDc<DcProjectBase>();

        /// <summary>
        ///     Service Access
        /// </summary>
        public SaProjectBase Sa => this.GetSa<SaProjectBase>();

        #endregion

        #region CommonCommands vom Server

        /// <summary>
        ///     Allgemeine Daten wurden vom Server empfangen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DcOnCommonDataFromServerReceived(object sender, CommonDataEventArgs e)
        {
            if (e == null! || e.Data == null!)
            {
                return;
            }

            Logging.Log.LogTrace($"[CommonDataFromServerReceived] Key {e.Data.Key} Data {e.Data.Value}");

            if (!Enum.TryParse(e.Data.Key, true, out EnumDcCommonCommandsClient command))
            {
                Logging.Log.LogError($"[DC] ReceivedDcCommonData Key {e.Data.Key} is not a valid Member of EnumDcCommonCommandsClient");
                return;
            }

            switch (command)
            {
                case EnumDcCommonCommandsClient.CommonMsg:
                    CallCommonMsg(e.Data.Value);
                    break;
                case EnumDcCommonCommandsClient.ReloadDcList:
                    if (!Enum.TryParse(e.Data.Value, true, out EnumReloadDcList list))
                    {
                        Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(DcOnCommonDataFromServerReceived)}): [DC] Key {e.Data.Value} is not a valid Member of EnumReloadDcList");
                        return;
                    }

                    ReloadDcList(list);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     DC Liste neu laden
        /// </summary>
        /// <param name="list"></param>
        private void ReloadDcList(EnumReloadDcList list)
        {
            switch (list)
            {
#pragma warning disable CS0618
                case EnumReloadDcList.Organization:
                case EnumReloadDcList.OrganizationUsers:
                case EnumReloadDcList.UsersAll:
                case EnumReloadDcList.Ideas:
                case EnumReloadDcList.Reports:
                    Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(ReloadDcList)}): Reload {list} - Sync benutzen!");
                    break;
#pragma warning restore CS0618

                case EnumReloadDcList.IdeaHelpers:
#pragma warning disable CS0618 // Type or member is obsolete
                    Dc.DcExIdeaHelpers.ReadListData(reload: true);
#pragma warning restore CS0618 // Type or member is obsolete
                    break;
                case EnumReloadDcList.IdeaNeeds:
#pragma warning disable CS0618 // Type or member is obsolete
                    Dc.DcExIdeaNeeds.ReadListData(reload: true);
#pragma warning restore CS0618 // Type or member is obsolete
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(list), list, null);
            }
        }

        /// <summary>
        ///     Allgemeine Meldung von ConnectivityHost empfangen - diese als MsgBox anzeigen
        /// </summary>
        /// <param name="data"></param>
        private async void CallCommonMsg(string data)
        {
            var msg = BissDeserialize.FromJson<EcDcCommonMessage>(data);
            if (msg == null!)
            {
                Logging.Log.LogError("[DC CallCommonMsg] Can not deserialize data");
                return;
            }

            Logging.Log.LogInfo($"[DC CallCommonMsg] {msg.Title}:{msg.Message}");
            await MsgBox.Show(msg.Message, msg.Title).ConfigureAwait(true);
        }

        #endregion
    }
}