// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using BaseApp.ViewModel;
using BaseApp.ViewModel.Chat;
using BaseApp.ViewModel.Idea;
using BaseApp.ViewModel.Organization;
using BaseApp.ViewModel.Reports;
using BaseApp.ViewModel.Settings;
using BaseApp.ViewModel.User;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     <para>Commands für alle Views und das Menü</para>
    ///     Klasse VmProjectBaseCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public abstract partial class VmProjectBase
    {
        private static VmCommandSelectable _gcmdMore = null!;
        private static VmCommandSelectable _gcmdHome = null!;
        private static VmCommandSelectable _gcmdLogin = null!;
        private static VmCommandSelectable _gcmdNotSelectable = null!;
        private static VmCommandSelectable _gcmdSettings = null!;
        private static VmCommandSelectable _gcmdDeveloperInfos = null!;
        private static VmCommandSelectable _gcmdInfos = null!;
        private static VmCommandSelectable _gcmdOrganization = null!;
        private static VmCommandSelectable _gcmdOpenLink = null!;
        private static VmCommandSelectable _gcmdSysAdmin = null!;
        private static VmCommandSelectable _gcmdChat = null!;
        private static VmCommandSelectable _gcmdFutureWishes = null!;
        private static VmCommandSelectable _gcmdReportOverview = null!;
        private static VmCommandSelectable _gcmdAdminSettings = null!;

        /// <summary>
        ///     Projektbeogene, globale VmCommands(Selectable) initialisieren
        /// </summary>
        protected override bool InitializeProjectBaseCommands()
        {
            _gcmdMore = new VmCommandSelectable(ResCommon.CmdMore, () =>
            {
                GCmdShowMenu.Execute(null!);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(250).ConfigureAwait(false);
                    _gcmdMore.IsSelected = false;
                }).ConfigureAwait(false);
            }, ResCommon.CmdMoreToolTip, Glyphs.Navigation_menu_horizontal);


            VmViewProperties.SetGcmdUserCommand(new VmCommandSelectable(ResCommon.CmdUser, () =>
            {
                if (!Dc.DeviceAndUserRegisteredLocal)
                {
                    _gcmdLogin.Execute(null!);
                }
                else
                {
                    Nav.ToView(typeof(VmUser), showMenu: true, cachePage: false);
                }
            }, glyph: Glyphs.Single_man));

            VmViewProperties.SetInfoBannerCommand(new VmCommandSelectable(string.Empty, async () => { await MsgBox.Show(ResCommon.MsgHeaderInfos, ResCommon.MsgTitleHeaderInfos).ConfigureAwait(true); }));

            _gcmdHome = new VmCommandSelectable(ResCommon.CmdHome, () =>
            {
                var mainType = GetVmBaseStatic.Dc.CoreConnectionInfos.UserOk
                    ? typeof(VmMainIdea)
                    : typeof(VmMain);
                Nav.ToView(mainType, showMenu: true, cachePage: true);
            }, glyph: Glyphs.House_chimney_2);

            _gcmdNotSelectable = new VmCommandSelectable("!Selectable", async () => { await MsgBox.Show(ResCommon.MsgInfo, ResCommon.MsgTitleInfo).ConfigureAwait(true); }, glyph: Glyphs.Server_lock, canEnableIsSelect: false, autoCloseMenu: true);

            _gcmdLogin = new VmCommandSelectable(ResCommon.CmdLogin, () => { Nav.ToView(typeof(VmLogin), showMenu: true, cachePage: false); }, glyph: Glyphs.Single_man);

            _gcmdSettings = new VmCommandSelectable(ResCommon.CmdSettings, () => { Nav.ToView(typeof(VmSettings), showMenu: true, cachePage: true); }, glyph: Glyphs.Cog);

            _gcmdDeveloperInfos = new VmCommandSelectable("DEV Infos", () => { Nav.ToView(typeof(VmDeveloperInfos), showMenu: true, cachePage: true); }, glyph: Glyphs.Computer_bug);

            _gcmdInfos = new VmCommandSelectable(ResCommon.CmdInfo, () =>
            {
                Nav.ToView(typeof(VmMain), showMenu: true, cachePage: true);
                //Nav.ToView(typeof(VmInfo), showMenu: true, cachePage: true);
            }, glyph: Glyphs.Information_circle);

            _gcmdOrganization = new VmCommandSelectable(ResCommon.CmdOrganization, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    _gcmdHome.IsSelected = true;
                    return;
                }

                View.BusySet(String.Empty, 50);
                Nav.ToView(typeof(VmOrganization), showMenu: true, cachePage: true);
                View.BusyClear();
            }, glyph: Glyphs.Real_estate_search_house);

            _gcmdOpenLink = new VmCommandSelectable(string.Empty, async l =>
            {
                if (l == null!)
                {
                    Logging.Log.LogWarning($"[{nameof(VmProjectBase)}]({nameof(InitializeProjectBaseCommands)}): Link is NULL!");
                    return;
                }

                if (l is string link)
                {
                    await Open.Browser(link).ConfigureAwait(false);
                }
                else
                {
                    Logging.Log.LogError($"[{nameof(VmProjectBase)}]({nameof(InitializeProjectBaseCommands)}): Link is no string. Link is {l.GetType()}");
                }
            });

            _gcmdSysAdmin = new VmCommandSelectable("System Admin", () => { Nav.ToView(typeof(VmSysAdmin), showMenu: true, cachePage: false); }, glyph: Glyphs.Lock_unlock);

            _gcmdChat = new VmCommandSelectable(ResCommon.CmdChat, async () =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    _gcmdHome.IsSelected = true;
                    return;
                }

                Nav.ToView(typeof(VmChatOverview), showMenu: true, cachePage: false);
            }, glyph: Glyphs.Conversation_chat_1);

            _gcmdFutureWishes = new VmCommandSelectable(ResCommon.CmdFutureDevelopment, () => { Nav.ToView(typeof(VmFutureWishes), showMenu: true, cachePage: true); }, glyph: Glyphs.Cloud_star);

            _gcmdReportOverview = new VmCommandSelectable(ResCommon.CmdReportOverview, () => { Nav.ToView(typeof(VmReportsOverview), showMenu: true, cachePage: true); }, glyph: Glyphs.Flag_warning);

            _gcmdAdminSettings = new VmCommandSelectable(ResCommon.CmdAdminSettings, () => { Nav.ToView(typeof(VmAdminSettings), showMenu: true, cachePage: true); }, glyph: Glyphs.Cloud_settings);

            return true;
        }

#pragma warning disable 1591

        public VmCommandSelectable GCmdMore => _gcmdMore;
        public VmCommandSelectable GCmdHome => _gcmdHome;
        public VmCommandSelectable GCmdLogin => _gcmdLogin;
        public VmCommandSelectable GCmdNotSelectable => _gcmdNotSelectable;
        public VmCommandSelectable GCmdSettings => _gcmdSettings;
        public VmCommandSelectable GCmdDeveloperInfos => _gcmdDeveloperInfos;
        public VmCommandSelectable GCmdInfos => _gcmdInfos;
        public VmCommandSelectable GCmdOrganization => _gcmdOrganization;
        public VmCommandSelectable GCmdOpenLink => _gcmdOpenLink;
        public VmCommandSelectable GCmdSysAdmin => _gcmdSysAdmin;
        public VmCommandSelectable GCmdChat => _gcmdChat;
        public VmCommandSelectable GCmdFutureWishes => _gcmdFutureWishes;
        public VmCommandSelectable GCmdReportOverview => _gcmdReportOverview;
        public VmCommandSelectable GCmdAdminSettings => _gcmdAdminSettings;

        public VmCommand GCmdHeaderCommon { get; set; } = null!;

        /// <summary>
        ///     "Default" Button im Footer im Phone Mode
        /// </summary>
        public void UpdateFooterButton()
        {
            if (!TabletMode)
            {
                if (Dc.DeviceAndUserRegisteredLocal)
                {
                    View.GCmdFooter1 = View.GCmdUser;
                }
                else
                {
                    View.GCmdFooter1 = _gcmdLogin;
                }

                if (Dc.DcExUser.Data.IsUserPlus)
                {
                    View.GCmdFooter2 = _gcmdOrganization;
                }
                else if (Dc.DeviceAndUserRegisteredLocal)
                {
                    View.GCmdFooter2 = _gcmdInfos;
                }
                else
                {
                    View.GCmdFooter2 = null!;
                }

                View.GCmdFooter3 = _gcmdHome;

                if (Dc.DeviceAndUserRegisteredLocal)
                {
                    View.GCmdFooter4 = _gcmdChat;
                }
                else
                {
                    View.GCmdFooter4 = null!;
                }

                View.GCmdFooter5 = _gcmdMore;
            }
        }


#pragma warning restore 1591
    }
}