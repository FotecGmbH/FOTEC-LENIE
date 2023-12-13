// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaseApp.ViewModel;
using BaseApp.ViewModel.Idea;
using Biss.Apps;
using Biss.Apps.Interfaces;
using Biss.Common;
using Biss.Core.Logging.Events;
using Biss.Dc.Core;
using Biss.Log.Producer;
using Exchange;
using Exchange.Enum;
using Exchange.Resources;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     <para>Basis View Model projektspezifisch</para>
    ///     Klasse ViewModelBase. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase : VmBase
    {
        private static Stream _defaultImage = null!;
        private static VmMenu _menu = null!;
        internal static bool _showDarkTheme;

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:DesignInstanceBissEventsLoggerEventArgs}"
        /// </summary>
        public static BissEventsLoggerEventArgs DesignInstanceBissEventsLoggerEventArgs = new BissEventsLoggerEventArgs();

        /// <summary>
        ///     Logeintäge
        /// </summary>
        public static ObservableCollection<BissEventsLoggerEventArgs> LogEntries = new ObservableCollection<BissEventsLoggerEventArgs>();

        /// <summary>
        ///     Aktuelles Menü
        /// </summary>
        public static VmMenu? CurrentVmMenu = null;

        private CancellationTokenSource? _ctsLoaded;
        private bool _footerEnabled;

        /// <summary>
        ///     Basis View Model für alle ViewModel
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="args"></param>
        /// <param name="subTitle"></param>
        protected VmProjectBase(string pageTitle, object? args = null, string subTitle = "") : base(pageTitle, args, subTitle)
        {
            if (_defaultImage == null!)
            {
                _defaultImage = Images.ReadImageAsStream(EnumEmbeddedImage.Logo_png);
            }

            Appeared += (sender, eventArgs) =>
            {
                View.ViewSizeChanged += (s, a) =>
                {
                    if (ShowDeveloperInfos)
                    {
                        Logging.Log.LogTrace($"[{nameof(VmProjectBase)}]({nameof(VmProjectBase)}): View Size: W: {a.Width:00} x H: {a.Height:00}");
                    }

                    if (!TabletMode)
                    {
                        if (a.Width > a.Height)
                        {
                            View.ShowFooter = false;
                        }
                        else
                        {
                            View.ShowFooter = _footerEnabled;
                        }
                    }
                };

                Dc.ConnectionChanged += (sender2, state) => { UpdateConnectionState(state); };
            };
            if (!TabletMode)
            {
                Loaded += (sender, eventArgs) => { UpdateFooterButton(); };
            }
        }

        #region Properties

        /// <summary>
        ///     Warten auf DC-Daten welche beim "späteren" Start asynchron geladen werden
        /// </summary>
        public bool WaitForProjectDataLoadedAfterDcConnected { get; set; }

        /// <summary>
        ///     Bild
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public Stream Image => _defaultImage;
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        ///     Zugriff auf das Hauptmenü
        /// </summary>
        public static VmMenu GetVmBaseStatic
        {
            get
            {
                if (_menu == null!)
                {
                    _menu = new VmMenu();
                }

                return _menu;
            }
        }

        /// <summary>
        ///     Aktuelle Type der App falls das Projekt aus mehr als einer App besteht
        /// </summary>
        public EnumAppType CurrentAppType => (EnumAppType) CurrentAppTypeId;

        /// <summary>
        ///     Developer Infos in den Views anzeigen (bei Release nicht)
        /// </summary>
        public bool ShowDeveloperInfos => AppSettings.Current().AppConfigurationConstants != 0;

        #endregion

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override async Task OnAppearing(IView view)
        {
            await base.OnAppearing(view).ConfigureAwait(true);
            if (WaitForProjectDataLoadedAfterDcConnected)
            {
                if (Dc.ConnectionState == EnumDcConnectionState.Connected && !ProjectDataLoadedAfterDcConnected)
                {
                    _ctsLoaded = new CancellationTokenSource();
                    View.BusySet("Lade Daten ...");
                    await Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(200, _ctsLoaded.Token).ConfigureAwait(false);
                            if (ProjectDataLoadedAfterDcConnected)
                            {
                                break;
                            }
                        } while (!_ctsLoaded.IsCancellationRequested);
                    }).ConfigureAwait(true);
                    View.BusyClear();
                }
            }
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            Dc.DcExUser.DataChangedEvent -= DcExUserOnDataChangedEvent;
            return base.OnDisappearing(view);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            if (!Dc.AutoConnect)
            {
                DcStartAutoConnect();
            }

            return base.OnLoaded();
        }

        /// <summary>
        ///     View für das Projekt parametrieren
        /// </summary>
        /// <param name="modalPage"></param>
        public void SetViewProperties(bool modalPage = false)
        {
            if (TabletMode)
            {
                if (DeviceInfo.Plattform == EnumPlattform.XamarinWpf)
                {
                    View.ShowMenu = false;
                }
                else if (modalPage == false)
                {
                    View.ShowMenu = true;
                }

                View.ShowFooter = false;
                View.ShowHeader = true;
                View.ShowSubTitle = true;


                View.ShowUser = Dc.CoreConnectionInfos.UserOk;

                if (modalPage)
                {
                    View.ShowBack = true;
                    View.ShowUser = false;
                }
                else
                {
                    View.ShowUser = Dc.CoreConnectionInfos.UserOk;
                }
            }
            else
            {
                View.ShowFooter = !modalPage;
                View.ShowHeader = true;
                View.ShowBack = modalPage;
                View.ShowMenu = !modalPage;
                View.ShowUser = true;

                UpdateFooterButton();
            }

            UpdateConnectionState(Dc.ConnectionState);
            _footerEnabled = View.ShowFooter;
        }

        /// <summary>
        ///     Falls der Hardware Back Button gedrückt wurde sollte bevor die App geschlossen wird noch die ViewMain aufgemacht
        ///     werden
        /// </summary>
        public override void OpenMainPage()
        {
            _gcmdHome.IsSelected = true;
        }

        /// <summary>
        ///     Welche View soll initial getartet weren
        /// </summary>
        public static void LaunchFirstView()
        {
            _ = CManager!.InitHigh();
            _gcmdHome.IsSelected = true;

            // Erststart der App - Introscreen zeigen
            if (CurrentVmMenu != null && CurrentVmMenu.Dc.CoreConnectionInfos.DeviceId < 0)
            {
                CurrentVmMenu.Nav.ToView(typeof(VmFirstLaunch));
            }
        }

        /// <summary>
        ///     Projekt Initialisieren
        /// </summary>
        /// <param name="currentAppType">User oder Admin App</param>
        /// <param name="showDarkTheme">Zeige Dark Theme.</param>
        /// <returns></returns>
        public static Task InitializeApp(EnumAppType currentAppType = EnumAppType.User, bool showDarkTheme = true)
        {
            var bisslogConfig = new BissEventsLoggerConfiguration {LogLevel = LogLevel.Trace};
            bisslogConfig.NewLogEntry += BisslogConfigOnNewLogEntry;
#if DEBUG
            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Trace)
                .AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Trace));
#else
            Logging.Init(l => l.AddDebug().SetMinimumLevel(LogLevel.Warning).AddProvider(new BissEventsLoggerProvider(bisslogConfig)).SetMinimumLevel(LogLevel.Warning));
#endif

            CurrentAppTypeId = (int) currentAppType;
            var mainType = GetVmBaseStatic.Dc.CoreConnectionInfos.UserOk
                ? typeof(VmMainIdea)
                : typeof(VmMain);
            SetMainViewModel(mainType);
            _showDarkTheme = showDarkTheme;

            try
            {
                Logging.Log.LogTrace("Init App");
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"Init App Error: {e}");
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Crash ins Appcenter schicken
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="additionalInfos"></param>
        public static void LogCrash4Appcenter(Exception exception, string additionalInfos = "")
        {
            if (string.IsNullOrWhiteSpace(additionalInfos))
            {
                Crashes.TrackError(exception);
            }
            else
            {
                var er = ErrorAttachmentLog.AttachmentWithText(additionalInfos, "AdditionalInfo");

                Crashes.TrackError(exception, attachments: er);
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_ctsLoaded != null!)
            {
                _ctsLoaded.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Logeintrag intern merken für die Developer Infos View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void BisslogConfigOnNewLogEntry(object sender, BissEventsLoggerEventArgs e)
        {
            if (Dispatcher == null!)
            {
                return;
            }

            Dispatcher.RunOnDispatcher(() =>
            {
                LogEntries.Insert(0, e);
                if (LogEntries.Count > 100)
                {
                    try
                    {
                        LogEntries.RemoveAt(LogEntries.Count - 1);
                    }
                    catch (Exception)
                    {
                        Logging.Log.LogWarning($"[{nameof(VmProjectBase)}]({nameof(BisslogConfigOnNewLogEntry)}): Workaroud - neues NuGet");
                    }
                }
            });
        }
    }
}