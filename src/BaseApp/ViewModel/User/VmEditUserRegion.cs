// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Collections;
using Biss.Apps.Enum;
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
    ///     <para>VmEditUserRegion</para>
    ///     Klasse VmEditUserRegion. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewEditUserRegion")]
    public class VmEditUserRegion : VmProjectBase
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUserRegion.DesignInstance}"
        /// </summary>
        public static VmEditUserRegion DesignInstance = new VmEditUserRegion();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUserRegion.DesignInstanceUserPermission}"
        /// </summary>
        public static ExUserPermission DesignInstanceUserPermission = new ExUserPermission();

        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmEditUserRegion.DesignInstanceUserPermission}"
        /// </summary>
        public static ExTown DesignInstanceExTown = new ExTown();

        private readonly AsyncAutoResetEvent townLock = new AsyncAutoResetEvent(true);

        /// <summary>
        ///     VmEditUserRegion
        /// </summary>
        public VmEditUserRegion() : base(ResViewEditUserRegion.LblTitle, subTitle: ResViewEditUserRegion.LblSubTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Alle verfügbaren Gemeinden
        /// </summary>
        public static List<ExTown> Towns { get; set; } = new List<ExTown>();

        /// <summary>
        ///     Der zu bearbeitende User
        /// </summary>
        public DcDataPoint<ExUser> UiUser { get; set; } = null!;

        /// <summary>
        ///     Suche Gemeinden
        /// </summary>
        public VmEntry EntryTownSearch { get; set; } = null!;

        /// <summary>
        ///     Suchtext Gemeinden
        /// </summary>
        public string TownSearchText { get; set; } = string.Empty;

        /// <summary>
        ///     Gemeindeauswahl für Gemeinde
        /// </summary>
        public BxObservableCollection<ExTown> PickerTowns { get; set; } = new BxObservableCollection<ExTown> {SelectionMode = EnumSelectionMode.Single, ShowSelectionStatus = false};

        /// <summary>
        ///     Regionen die bereits hinzugefügt sind.
        /// </summary>
        public ObservableCollection<ExUserPermission> UiAddedTowns { get; set; } = new ObservableCollection<ExUserPermission>();

        /// <summary>
        ///     Picker enthält Elemente.
        /// </summary>
        [DependsOn(nameof(PickerTowns))]
        public bool PickerTownsHasElements => PickerTowns.Any();

        #endregion

        /// <summary>
        ///     Updated die Liste der verfügbaren Regionen
        /// </summary>
        public async void UpdatePickerTowns()
        {
            await townLock.WaitOne().ConfigureAwait(true);

            if (!Towns.Any())
            {
                Towns = TownHelper.GetTowns().Where(x => x.IsMainPostalCode).ToList();
            }

            var validSearchString = TownSearchText.Length > 0;

            var towns = validSearchString
                ? Towns.Where(t =>
                    t.Name.ToUpperInvariant().StartsWith(TownSearchText.ToUpperInvariant(), StringComparison.InvariantCulture) ||
                    t.PostalCode.StartsWith(TownSearchText, StringComparison.InvariantCulture))
                : new List<ExTown>();

            Dispatcher!.RunOnDispatcher(() =>
            {
                PickerTowns.Clear();

                if (validSearchString)
                {
                    foreach (var t in towns.Where(x =>
                                 UiUser.Data.Permissions
                                     .All(p => p.Town != null &&
                                               (p.Town.Name != x.Name || p.Town.PostalCode != x.PostalCode))))
                    {
                        PickerTowns.Add(t);
                    }
                }

                this.InvokeOnPropertyChanged(nameof(PickerTownsHasElements));

                townLock.Set();
            });
        }

        /// <summary>
        ///     Updated die Liste der hinzugefügten Regionen
        /// </summary>
        public async Task UpdateAddedTowns()
        {
            await UiUser.WaitDataFromServerAsync().ConfigureAwait(true);

            UiAddedTowns.Clear();

            foreach (var t in UiUser.Data.Permissions)
            {
                UiAddedTowns.Add(t);
            }

            this.InvokeOnPropertyChanged(nameof(UiAddedTowns));
        }

        #region Overrides

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            View.BusySet();

            if (args is DcDataPoint<ExUser> u)
            {
                UiUser = u;
            }
            else
            {
                UiUser = Dc.DcExUser;
            }


            PickerTowns.SelectedItemChanged += PickerTowns_SelectedItemChanged;

            EntryTownSearch = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleTownSearch,
                ResViewEditUser.EntryPlaceholderTownSearch,
                this,
                nameof(TownSearchText),
                maxChar: 50,
                showTitle: false);
            EntryTownSearch.AutoValidateData = true;

            PropertyChanged += (sender, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(TownSearchText) && EntryTownSearch.DataValid)
                {
                    _ = Task.Run(UpdatePickerTowns);
                }
            };

            await UpdateAddedTowns().ConfigureAwait(true);
            await base.OnActivated(args).ConfigureAwait(false);

            _ = Task.Run(() =>
            {
                if (!Towns.Any())
                {
                    Towns = TownHelper.GetTowns().Where(x => x.IsMainPostalCode).ToList();
                }
            });

            View.BusyClear();
        }

        private void PickerTowns_SelectedItemChanged(object sender, SelectedItemEventArgs<ExTown> e)
        {
            PickerTowns.SelectedItem = null!;
            CmdAddRegion.CanExecute(e.CurrentItem);
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Region hinzufügen
        /// </summary>
        public VmCommand CmdAddRegion { get; set; } = null!;

        /// <summary>
        ///     Region entfernen
        /// </summary>
        public VmCommand CmdDeleteRegion { get; set; } = null!;

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            CmdAddRegion = new VmCommand(string.Empty, async arg =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                var maxCount = 5;

                if (UiUser.Data.Permissions.Count >= maxCount)
                {
                    await MsgBox.Show(string.Format(ResViewEditUserRegion.MsgErrorMaxTowns, maxCount), ResViewEditUserRegion.MsgErrorMaxTownsTitle).ConfigureAwait(false);
                    return;
                }

                if (arg is ExTown town)
                {
                    if (!Dc.DcExOrganization.Any(x => x.Data.Name == town.Name && x.Data.PostalCode == town.PostalCode))
                    {
                        // Add Organization
                        Dc.DcExOrganization.Add(new DcListDataPoint<ExOrganization>(new ExOrganization
                                                                                    {
                                                                                        PostalCode = town.PostalCode,
                                                                                        Name = town.Name,
                                                                                        OrganizationType = EnumOrganizationTypes.PublicOrganization,
                                                                                    }));
                        var res = await Dc.DcExOrganization.StoreAll().ConfigureAwait(true);
                        if (!res.DataOk)
                        {
                            await MsgBox.Show("Speicherfehler! (Organization) " + res.ServerExceptionText).ConfigureAwait(true);
                            return;
                        }
                    }


                    var comp = Dc.DcExOrganization.First(x => x.Data.Name == town.Name && x.Data.PostalCode == town.PostalCode);
                    UiUser.Data.Permissions.Add(new ExUserPermission {IsMainCompany = false, Town = comp.Data, CompanyId = comp.Index, UserRole = EnumUserRole.User});
                    await UiUser.StoreData().ConfigureAwait(true);
                    UpdatePickerTowns();
                    await UpdateAddedTowns().ConfigureAwait(true);

                    try
                    {
                        await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"[{nameof(VmEditUserRegion)}]({nameof(InitializeCommands)}): {e}");
                    }
                }
            });

            CmdDeleteRegion = new VmCommand(string.Empty, async p =>
            {
                if (!await CheckConnected().ConfigureAwait(true))
                {
                    return;
                }

                if (p is ExUserPermission {Town: { }} permission)
                {
                    if (permission.IsMainCompany)
                    {
                        await MsgBox.Show(ResViewEditUserRegion.MsgErrorTextDefaultRegionRemoved, ResViewEditUserRegion.MsgErrorCaptionDefaultRegionRemoved).ConfigureAwait(true);
                    }
                    else
                    {
                        UiUser.Data.Permissions.Remove(permission);
                        await UiUser.StoreData().ConfigureAwait(true);
                        await UpdateAddedTowns().ConfigureAwait(true);
                        UpdatePickerTowns();

                        try
                        {
                            await Dc.DcExIdeas.Sync().ConfigureAwait(true);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.LogError($"[{nameof(VmEditUserRegion)}]({nameof(InitializeCommands)}): {e}");
                        }
                    }
                }
            });
        }

        #endregion
    }
}