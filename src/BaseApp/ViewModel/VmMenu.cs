// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Biss.AppConfiguration;
using Biss.Apps.Collections;
using Biss.Apps.ViewModel;
using Exchange;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>VmMenu</para>
    ///     Klasse VmMenu. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class VmMenu : VmProjectBase
    {
        /// <summary>
        ///     ViewModel Template
        /// </summary>
        public VmMenu() : base(string.Empty)
        {
            CurrentVmMenu = this;
        }

        #region Properties

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmMenu.DesignInstance}"
        /// </summary>
        public static VmMenu DesignInstance => new VmMenu();

        /// <summary>
        ///     Alle Menüeinträge für seitliches Menü
        /// </summary>
        public BxObservableCollection<VmCommandSelectable> CmdAllMenuCommands { get; } = new BxObservableCollection<VmCommandSelectable>();

        /// <summary>
        ///     PreviewVersion
        /// </summary>
        public bool PreviewVersion => Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease;

        #endregion

        /// <summary>
        ///     Menüeinträge aktualisieren
        /// </summary>
        public void UpdateMenu()
        {
            CmdAllMenuCommands.Clear();

            if (Dc.DeviceAndUserRegisteredLocal)
            {
                CmdAllMenuCommands.Add(GCmdHome);

                if (Dc.DcExUser.Data.IsUserPlus)
                {
                    CmdAllMenuCommands.Add(GCmdOrganization);
                    CmdAllMenuCommands.Add(GCmdReportOverview);
                }

                CmdAllMenuCommands.Add(GCmdChat);
                CmdAllMenuCommands.Add(GCmdInfos);
                CmdAllMenuCommands.Add(View.GCmdUser);

                if (Dc.DcExUser.Data.IsSysAdmin)
                {
                    CmdAllMenuCommands.Add(GCmdAdminSettings);
                    CmdAllMenuCommands.Add(GCmdSysAdmin);
                }

                if (ShowDeveloperInfos)
                {
                    CmdAllMenuCommands.Add(GCmdDeveloperInfos);
                }
            }
            else
            {
                AddDefault();
            }

            UpdateFooterButton();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            AddDefault();
        }

        /// <summary>
        ///     "Default" Einträge (wenn User noch nicht eingeloggt ist) laden
        /// </summary>
        private void AddDefault()
        {
            CmdAllMenuCommands.Add(GCmdHome);

            CmdAllMenuCommands.Add(GCmdLogin);

            if (ShowDeveloperInfos)
            {
                CmdAllMenuCommands.Add(GCmdDeveloperInfos);
            }
        }
    }
}