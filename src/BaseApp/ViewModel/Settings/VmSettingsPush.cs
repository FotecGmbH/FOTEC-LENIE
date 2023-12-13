// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Push;
using Biss.Apps.ViewModel;
using Biss.Common;
using Biss.Interfaces;
using Exchange.Enum;
using Exchange.Resources;

namespace BaseApp.ViewModel.Settings
{
    #region Hilfsklassen

    /// <summary>
    ///     Selectable Enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumSelectable<T> : IBissModel where T : Enum
    {
        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="enumToExtend"></param>
        public EnumSelectable(T enumToExtend)
        {
            Enum = enumToExtend;
        }

        #region Properties

        /// <summary>
        ///     Der Enum
        /// </summary>
        public T Enum { get; }


        /// <summary>
        ///     Ist das Enum selektiert
        /// </summary>
        public bool Selected { get; set; }

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0414
        public event PropertyChangedEventHandler PropertyChanged = null!;
#pragma warning restore CS0414
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        #endregion
    }

    #endregion

    /// <summary>
    ///     <para>Push Settings der App</para>
    ///     Klasse VMPush. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewSettingPush")]
    public class VmSettingsPush : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmDeveloperInfos.DesignInstanceExUserDevice}"
        /// </summary>
        public static EnumSelectable<EnumPushTopics> DesignInstanceEnumPushTopic = new EnumSelectable<EnumPushTopics>(EnumPushTopics.TestPushA);

        private bool _setting10MinutePushEnabled;
        private bool _settingMailChat;
        private bool _settingMailMeeting;
        private bool _settingMailReport;
        private bool _settingPushChat;
        private bool _settingPushComment;
        private bool _settingPushIdea;
        private bool _settingPushLike;
        private bool _settingPushMeeting;
        private bool _settingPushReport;
        private bool _settingPushSupport;

        /// <summary>
        ///     VMPush
        /// </summary>
        public VmSettingsPush() : base(ResViewSettingsPush.LblTitle, subTitle: ResViewSettingsPush.LblSubTitle)
        {
            SetViewProperties(true);
            _setting10MinutePushEnabled = Dc.DcExUser.Data.Setting10MinPush;
            _settingPushMeeting = Dc.DcExUser.Data.NotificationPushMeeting;
            _settingPushChat = Dc.DcExUser.Data.NotificationPushChat;
            _settingPushLike = Dc.DcExUser.Data.NotificationPushLike;
            _settingPushSupport = Dc.DcExUser.Data.NotificationPushSupport;
            _settingPushComment = Dc.DcExUser.Data.NotificationPushComment;
            _settingPushReport = Dc.DcExUser.Data.NotificationPushReport;
            _settingPushIdea = Dc.DcExUser.Data.NotificationPushIdea;
            _settingMailMeeting = Dc.DcExUser.Data.NotificationMailMeeting;
            _settingMailChat = Dc.DcExUser.Data.NotificationMailChat;
            _settingMailReport = Dc.DcExUser.Data.NotificationMailReport;
        }

        #region Properties

        /// <summary>
        ///     Design Instanz.
        /// </summary>
        public static VmSettingsPush DesignInstance => new VmSettingsPush();

        /// <summary>
        ///     Werden die Pushsettings auf der Plattform unterstützt
        /// </summary>
        public bool PushSupported => DeviceInfo.Plattform == EnumPlattform.XamarinIos || DeviceInfo.Plattform == EnumPlattform.XamarinAndroid;

        /// <summary>
        ///     Ist Push enabled.
        /// </summary>
        public bool PushEnabled { get; set; }

        /// <summary>
        ///     10 Minuten Push
        /// </summary>
        public bool Setting10MinutePushEnabled
        {
            get => _setting10MinutePushEnabled;
            set => Update10MinutePush(value);
        }

        /// <summary>
        ///     Push bei Termin
        /// </summary>
        public bool SettingPushMeeting
        {
            get => _settingPushMeeting;
            set => UpdatePushMeeting(value);
        }

        /// <summary>
        ///     Push bei Chat
        /// </summary>
        public bool SettingPushChat
        {
            get => _settingPushChat;
            set => UpdatePushChat(value);
        }

        /// <summary>
        ///     Push bei Kommentar
        /// </summary>
        public bool SettingPushComment
        {
            get => _settingPushComment;
            set => UpdatePushComment(value);
        }

        /// <summary>
        ///     Push bei Like
        /// </summary>
        public bool SettingPushLike
        {
            get => _settingPushLike;
            set => UpdatePushLike(value);
        }

        /// <summary>
        ///     Push bei Unterstützung
        /// </summary>
        public bool SettingPushSupport
        {
            get => _settingPushSupport;
            set => UpdatePushSupport(value);
        }

        /// <summary>
        ///     Push bei Meldung
        /// </summary>
        public bool SettingPushReport
        {
            get => _settingPushReport;
            set => UpdatePushReport(value);
        }

        /// <summary>
        ///     Push bei neuer Idee
        /// </summary>
        public bool SettingPushIdea
        {
            get => _settingPushIdea;
            set => UpdatePushIdea(value);
        }

        /// <summary>
        ///     Email bei Termin
        /// </summary>
        public bool SettingMailMeeting
        {
            get => _settingMailMeeting;
            set => UpdateMailMeeting(value);
        }

        /// <summary>
        ///     Email bei Chat
        /// </summary>
        public bool SettingMailChat
        {
            get => _settingMailChat;
            set => UpdateMailChat(value);
        }

        /// <summary>
        ///     Email bei Report
        /// </summary>
        public bool SettingMailReport
        {
            get => _settingMailReport;
            set => UpdateMailReport(value);
        }

        /// <summary>
        ///     Settings öffnen
        /// </summary>
        public VmCommand CmdOpenSettings { get; private set; } = null!;

        /// <summary>
        ///     Alle Benachrichtigungen an
        /// </summary>
        public VmCommand CmdAllOn { get; private set; } = null!;

        /// <summary>
        ///     Alle Benachrichtigungen aus
        /// </summary>
        public VmCommand CmdAllOff { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     Wird aufgerufen sobald die View initialisiert wurde
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnActivated(object? args = null)
        {
            if (PushSupported)
            {
                Push.PushStateChanged += Push_PushStateChanged;
                PushEnabled = await Push.CheckPushEnabled().ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdOpenSettings = new VmCommand(ResViewSettingsPush.CmdOpenSettings, () => { Push.OpenSettings(); }, () => PushSupported, glyph: Glyphs.Cog);

            CmdAllOn = new VmCommand(ResViewSettingsPush.CmdAllOn, async () =>
            {
                Dc.DcExUser.Data.Setting10MinPush = false; // true;

                Dc.DcExUser.Data.NotificationPushMeeting = false; // true;
                Dc.DcExUser.Data.NotificationPushChat = true;
                Dc.DcExUser.Data.NotificationPushLike = true;
                Dc.DcExUser.Data.NotificationPushSupport = true;
                Dc.DcExUser.Data.NotificationPushReport = Dc.DcExUser.Data.IsUserPlus; // nur setzen, wenn irgendwo user plus oder admin
                Dc.DcExUser.Data.NotificationPushIdea = true;
                Dc.DcExUser.Data.NotificationPushComment = false; // true;
                Dc.DcExUser.Data.NotificationMailMeeting = false; // true;
                Dc.DcExUser.Data.NotificationMailChat = true;
                Dc.DcExUser.Data.NotificationMailReport = true;

                if (!(await UpdateValues().ConfigureAwait(true)))
                {
                    Dc.DcExUser.Update();
                }

                _setting10MinutePushEnabled = Dc.DcExUser.Data.Setting10MinPush;
                _settingPushMeeting = Dc.DcExUser.Data.NotificationPushMeeting;
                _settingPushChat = Dc.DcExUser.Data.NotificationPushChat;
                _settingPushLike = Dc.DcExUser.Data.NotificationPushLike;
                _settingPushSupport = Dc.DcExUser.Data.NotificationPushSupport;
                _settingPushReport = Dc.DcExUser.Data.NotificationPushReport;
                _settingPushIdea = Dc.DcExUser.Data.NotificationPushIdea;
                _settingPushComment = Dc.DcExUser.Data.NotificationPushComment;
                _settingMailMeeting = Dc.DcExUser.Data.NotificationMailMeeting;
                _settingMailChat = Dc.DcExUser.Data.NotificationMailChat;
                _settingMailReport = Dc.DcExUser.Data.NotificationMailReport;

                this.InvokeOnPropertyChanged(nameof(Setting10MinutePushEnabled));
                this.InvokeOnPropertyChanged(nameof(SettingPushMeeting));
                this.InvokeOnPropertyChanged(nameof(SettingPushChat));
                this.InvokeOnPropertyChanged(nameof(SettingPushLike));
                this.InvokeOnPropertyChanged(nameof(SettingPushSupport));
                this.InvokeOnPropertyChanged(nameof(SettingPushReport));
                this.InvokeOnPropertyChanged(nameof(SettingPushIdea));
                this.InvokeOnPropertyChanged(nameof(SettingPushComment));
                this.InvokeOnPropertyChanged(nameof(SettingMailMeeting));
                this.InvokeOnPropertyChanged(nameof(SettingMailChat));
                this.InvokeOnPropertyChanged(nameof(SettingMailReport));
            });

            CmdAllOff = new VmCommand(ResViewSettingsPush.CmdAllOff, async () =>
            {
                Dc.DcExUser.Data.Setting10MinPush = false;
                Dc.DcExUser.Data.NotificationPushMeeting = false;
                Dc.DcExUser.Data.NotificationPushChat = false;
                Dc.DcExUser.Data.NotificationPushLike = false;
                Dc.DcExUser.Data.NotificationPushSupport = false;
                Dc.DcExUser.Data.NotificationPushReport = false;
                Dc.DcExUser.Data.NotificationPushIdea = false;
                Dc.DcExUser.Data.NotificationPushComment = false;
                Dc.DcExUser.Data.NotificationMailMeeting = false;
                Dc.DcExUser.Data.NotificationMailChat = false;
                Dc.DcExUser.Data.NotificationMailReport = false;

                if (!(await UpdateValues().ConfigureAwait(true)))
                {
                    Dc.DcExUser.Update();
                }

                _setting10MinutePushEnabled = Dc.DcExUser.Data.Setting10MinPush;
                _settingPushMeeting = Dc.DcExUser.Data.NotificationPushMeeting;
                _settingPushChat = Dc.DcExUser.Data.NotificationPushChat;
                _settingPushLike = Dc.DcExUser.Data.NotificationPushLike;
                _settingPushSupport = Dc.DcExUser.Data.NotificationPushSupport;
                _settingPushReport = Dc.DcExUser.Data.NotificationPushReport;
                _settingPushIdea = Dc.DcExUser.Data.NotificationPushIdea;
                _settingPushComment = Dc.DcExUser.Data.NotificationPushComment;
                _settingMailMeeting = Dc.DcExUser.Data.NotificationMailMeeting;
                _settingMailChat = Dc.DcExUser.Data.NotificationMailChat;
                _settingMailReport = Dc.DcExUser.Data.NotificationMailReport;

                this.InvokeOnPropertyChanged(nameof(Setting10MinutePushEnabled));
                this.InvokeOnPropertyChanged(nameof(SettingPushMeeting));
                this.InvokeOnPropertyChanged(nameof(SettingPushChat));
                this.InvokeOnPropertyChanged(nameof(SettingPushLike));
                this.InvokeOnPropertyChanged(nameof(SettingPushSupport));
                this.InvokeOnPropertyChanged(nameof(SettingPushReport));
                this.InvokeOnPropertyChanged(nameof(SettingPushIdea));
                this.InvokeOnPropertyChanged(nameof(SettingPushComment));
                this.InvokeOnPropertyChanged(nameof(SettingMailMeeting));
                this.InvokeOnPropertyChanged(nameof(SettingMailChat));
                this.InvokeOnPropertyChanged(nameof(SettingMailReport));
            });
        }

        private void Push_PushStateChanged(object sender, PushStateChangedEventArgs e)
        {
            PushEnabled = e.PushEnabled;
        }

        private async Task<bool> UpdateValues()
        {
            var store = await Dc.DcExUser.StoreData().ConfigureAwait(true);
            if (!store.DataOk)
            {
                await MsgBox.Show(ResViewSettingsPush.MsgSettingsNotSaved, ResViewSettingsPush.MsgTitleSettingsNotSaved).ConfigureAwait(true);
            }

            return store.DataOk;
        }

        #region Update Push

        private async void Update10MinutePush(bool value)
        {
            Dc.DcExUser.Data.Setting10MinPush = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.Setting10MinPush = !value;
            }
            else
            {
                _setting10MinutePushEnabled = value;
            }

            this.InvokeOnPropertyChanged(nameof(Setting10MinutePushEnabled));
        }

        private async void UpdatePushMeeting(bool value)
        {
            Dc.DcExUser.Data.NotificationPushMeeting = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushMeeting = !value;
            }
            else
            {
                _settingPushMeeting = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushMeeting));
        }

        private async void UpdatePushChat(bool value)
        {
            Dc.DcExUser.Data.NotificationPushChat = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushChat = !value;
            }
            else
            {
                _settingPushChat = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushChat));
        }

        private async void UpdatePushComment(bool value)
        {
            Dc.DcExUser.Data.NotificationPushComment = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushComment = !value;
            }
            else
            {
                _settingPushComment = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushComment));
        }

        private async void UpdatePushLike(bool value)
        {
            Dc.DcExUser.Data.NotificationPushLike = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushLike = !value;
            }
            else
            {
                _settingPushLike = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushLike));
        }

        private async void UpdatePushSupport(bool value)
        {
            Dc.DcExUser.Data.NotificationPushSupport = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushSupport = !value;
            }
            else
            {
                _settingPushSupport = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushSupport));
        }

        private async void UpdatePushReport(bool value)
        {
            Dc.DcExUser.Data.NotificationPushReport = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushReport = !value;
            }
            else
            {
                _settingPushReport = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushReport));
        }

        private async void UpdatePushIdea(bool value)
        {
            Dc.DcExUser.Data.NotificationPushIdea = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationPushIdea = !value;
            }
            else
            {
                _settingPushIdea = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingPushIdea));
        }

        #endregion

        #region Update Mail

        private async void UpdateMailMeeting(bool value)
        {
            Dc.DcExUser.Data.NotificationMailMeeting = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationMailMeeting = !value;
            }
            else
            {
                _settingMailMeeting = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingMailMeeting));
        }

        private async void UpdateMailChat(bool value)
        {
            Dc.DcExUser.Data.NotificationMailChat = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationMailChat = !value;
            }
            else
            {
                _settingMailChat = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingMailChat));
        }

        private async void UpdateMailReport(bool value)
        {
            Dc.DcExUser.Data.NotificationMailReport = value;

            if (!(await UpdateValues().ConfigureAwait(true)))
            {
                Dc.DcExUser.Data.NotificationMailReport = !value;
            }
            else
            {
                _settingMailReport = value;
            }

            this.InvokeOnPropertyChanged(nameof(SettingMailReport));
        }

        #endregion
    }
}