// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.Linq;
using BaseApp.Connectivity;
using BaseApp.ViewModel.Chat;
using BaseApp.ViewModel.Idea;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.Push;
using Biss.Apps.Toast;
using Biss.Apps.Toast.Options;
using Biss.Common;
using Biss.Log.Producer;
using Exchange;
using Microsoft.Extensions.Logging;
using PropertyChanged;

namespace BaseApp
{
    /// <summary>
    ///     <para>Gemeinsame Methoden für Push</para>
    ///     Klasse VmProjectBaseConnectivity. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        private static readonly AsyncAutoResetEvent LaunchCheck = new AsyncAutoResetEvent(true);

        #region Properties

        /// <summary>
        ///     Zugriff auf Push
        /// </summary>
        public static BcPush Push => PushExtension.BcPush();

        #endregion

        /// <summary>
        ///     Pushes abonnieren
        /// </summary>
        public static void SubNotifications()
        {
            PushExtension.BcPush().PushReceived += OnPushReceived;
            PushExtension.BcPush().PushTokenUpdated += OnPushTokenUpdated;
            PushExtension.BcPush().LaunchOptionsChanged += OnLaunchOptionsChanged;

            Toast.NotificationClicked += ToastOnNotificationClicked;
            ToastClick += OnToastCommandClick;

            if (CurrentVmMenu != null)
            {
                CurrentVmMenu.CheckLaunch();
            }
        }

        /// <summary>
        ///     Pushes abmelden.
        /// </summary>
        public static void UnsubNotifications()
        {
            PushExtension.BcPush().PushReceived -= OnPushReceived;
            PushExtension.BcPush().PushTokenUpdated -= OnPushTokenUpdated;
            PushExtension.BcPush().LaunchOptionsChanged -= OnLaunchOptionsChanged;

            Toast.NotificationClicked -= ToastOnNotificationClicked;
            ToastClick -= OnToastCommandClick;
        }

        /// <summary>
        ///     Checken der Launchsettings
        /// </summary>
        public async void CheckLaunch()
        {
            await LaunchCheck.WaitOne().ConfigureAwait(true);

            try
            {
                if (Push.LaunchOptions.Any())
                {
                    // Get Event from Notification
                    foreach (var launchOption in Push.LaunchOptions)
                    {
                        var didNav = CheckCustomArgs(launchOption);
                        if (didNav)
                        {
                            return;
                        }
                    }

                    Push.ClearLaunchOptions();
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
            }
            finally
            {
                LaunchCheck.Set();
            }
        }

        private static void OnToastCommandClick(object sender, string e)
        {
            // TODO Button in Toast gedrückt
        }

        private static void ToastOnNotificationClicked(object sender, NotificationEventArgs e)
        {
            Logging.Log.LogInfo($"[{nameof(VmProjectBase)}]({nameof(ToastOnNotificationClicked)}): Toast clicked - {e}");
            CheckCustomArgs(e.NotificationOptions);
        }

        private static bool CheckCustomArgs(BissNotificationOptions options)
        {
            if (options.CustomArgs.ContainsKey(Constants.DeepLinkChat))
            {
                options.CustomArgs.TryGetValue(Constants.DeepLinkChat, out var chatId);

                if (long.TryParse(chatId, out var id))
                {
                    Dispatcher!.RunOnDispatcher(() =>
                    {
                        VmChatOverview.SetFilterForChat(id);
                        if (CurrentVmMenu != null!)
                        {
                            CurrentVmMenu.GCmdChat.IsSelected = true;
                            CurrentVmMenu.Nav.ToView(typeof(VmChatOverview));
                        }
                    });

                    Push.ClearLaunchOptions();

                    return true;
                }
            }
            else if (options.CustomArgs.ContainsKey(Constants.DeepLinkIdea))
            {
                options.CustomArgs.TryGetValue(Constants.DeepLinkIdea, out var ideaId);

                if (long.TryParse(ideaId, out var id))
                {
                    Dispatcher!.RunOnDispatcher(() =>
                    {
                        VmMainIdea.SetFilterForIdea(id);

                        if (CurrentVmMenu != null && !CurrentVmMenu.GCmdHome.IsSelected)
                        {
                            CurrentVmMenu.GCmdHome.IsSelected = true;
                        }
                        else
                        {
                            VmMainIdea.CheckLaunchIdea();
                        }
                    });

                    Push.ClearLaunchOptions();

                    return true;
                }
            }

            return false;
        }

        [SuppressPropertyChangedWarnings]
        private static void OnLaunchOptionsChanged(object sender, CollectionChangeEventArgs e)
        {
            if (CurrentVmMenu != null)
            {
                CurrentVmMenu.CheckLaunch();
            }
        }

        /// <summary>
        ///     Push wurde empfangen während die App aktiv ist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPushReceived(object sender, BissNotificationOptions e)
        {
            Toast.ShowAsync(e);
        }

        /// <summary>
        ///     Push Token aktualisieren
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static async void OnPushTokenUpdated(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PushExtension.BcPush().Token))
            {
                var dc = BcDataConnectorExtensions.BcDataConnector(null!)?.GetDc<DcProjectBase>();

                if (dc == null!)
                {
                    return;
                }

                dc.DcExDeviceInfo.Data.DeviceToken = PushExtension.BcPush().Token;

                var storeRes = await dc.DcExDeviceInfo.StoreData().ConfigureAwait(false);
                if (!storeRes.DataOk)
                {
                    Logging.Log.LogError($"[DC] DeviceTokenUpdateError Error({storeRes.ErrorType}): {storeRes.ServerExceptionText}");
                }
                else
                {
                    Logging.Log.LogTrace("[DC] DeviceToken Updated");
                }
            }
        }
    }
}