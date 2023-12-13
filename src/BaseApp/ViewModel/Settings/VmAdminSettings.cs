// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using BaseApp.Connectivity;
using Biss.Apps.Attributes;
using Biss.Apps.Base;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;
using Biss.Serialize;
using Exchange.Model;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp.ViewModel
{
    /// <summary>
    ///     <para>Einstellungen für Admins</para>
    ///     Klasse VmAdminSettings. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAdminSettings")]
    public class VmAdminSettings : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAdminSettings.DesignInstance}"
        /// </summary>
        public static VmAdminSettings DesignInstance = new VmAdminSettings();

        private bool _eventsAttached;

        /// <summary>
        ///     VmAdminSettings
        /// </summary>
        public VmAdminSettings() : base("Admin Einstellungen")
        {
            SetViewProperties();
        }

        #region Commands

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand(string.Empty, async () =>
            {
                View.BusySet();

                var res = await Dc.DcExIntros.StoreAll().ConfigureAwait(true);

                if (!res.DataOk)
                {
                    Logging.Log.LogError($"[{nameof(VmAdminSettings)}]({nameof(InitializeCommands)}): {res.ServerExceptionText}");
                    await MsgBox.Show("Änderungen konnten nicht gespeichert werden!", ResCommon.MsgTitleError).ConfigureAwait(true);
                }
                else
                {
                    CheckSaveBehavior = null;
                    GCmdHome.IsSelected = true;
                }

                View.BusyClear();
            }, glyph: Glyphs.Floppy_disk);
        }

        #endregion

        /// <summary>
        ///     ViewModel Events
        /// </summary>
        /// <param name="attach"></param>
        private void AttachDetachVmEvents(bool attach)
        {
            if (attach)
            {
                if (!_eventsAttached)
                {
                    _eventsAttached = true;
                    Dc.DcExIntros.CollectionEvent += DcExIntrosOnCollectionEvent;
                }
            }
            else
            {
                if (_eventsAttached)
                {
                    _eventsAttached = false;
                    Dc.DcExIntros.CollectionEvent -= DcExIntrosOnCollectionEvent;
                }
            }
        }

        private async Task DcExIntrosOnCollectionEvent(object sender, CollectionEventArgs<DcIntroItem> e)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                return;
            }

            View.BusySet();

            if (e.TypeOfEvent == EnumCollectionEventType.AddRequest)
            {
                Dc.DcExIntros.Add(new DcIntroItem(new ExIntroItem
                                                  {
                                                      HtmlSource = "https://land-noe.at/lenie",
                                                  }));
            }
            else if (e.TypeOfEvent == EnumCollectionEventType.DeleteRequest)
            {
                var deleteItem = e.Item;

                try
                {
                    Dc.DcExIntros.Remove(deleteItem);
                }
                catch (Exception)
                {
                    Logging.Log.LogWarning($"[{nameof(VmAdminSettings)}]({nameof(DcExIntrosOnCollectionEvent)}): Workaround - Neues Nuget");
                }
            }

            View.BusyClear();
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            var csb = new CheckSaveJsonBehavior();
            csb.SetCompareData(Dc.DcExIntros.ToJson());
            csb.CheckSaveComparer += (_, args) => { args.JsonToCompare = Dc.DcExIntros.ToJson(); };

            CheckSaveBehavior = csb;

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            if (!Dc.DcExIntros.SyncedSinceUserRegistered)
            {
                try
                {
                    Dc.DcExIntros.Sync();
                }
                catch (Exception e)
                {
                    Logging.Log.LogError($"[{nameof(VmAdminSettings)}]({nameof(OnActivated)}): {e}");
                }
            }

            return base.OnActivated(args);
        }

        /// <summary>
        ///     OnLoaded (3) für View geladen
        ///     Jedes Mal wenn View wieder sichtbar
        /// </summary>
        public override Task OnLoaded()
        {
            AttachDetachVmEvents(true);
            return base.OnLoaded();
        }

        /// <summary>
        ///     OnDisappearing (4) wenn View unsichtbar / beendet wird
        ///     Nur einmal
        /// </summary>
        public override Task OnDisappearing(IView view)
        {
            AttachDetachVmEvents(false);
            return base.OnDisappearing(view);
        }

        #endregion
    }
}