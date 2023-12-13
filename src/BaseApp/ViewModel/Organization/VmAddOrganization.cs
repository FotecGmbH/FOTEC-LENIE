// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Biss.Collections;
using Biss.Interfaces;
using Exchange.Helper;
using Exchange.Model.Organization;
using Exchange.Resources;

namespace BaseApp.ViewModel.Organization
{
    /// <summary>
    ///     <para>VmAddOrganizationUser</para>
    ///     Klasse VmAddOrganizationUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddOrganization", true)]
    public class VmAddOrganization : VmEditDcListPoint<ExOrganization>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddUser.DesignInstance}"
        /// </summary>
        public static VmAddOrganization DesignInstance = new VmAddOrganization();

        /// <summary>
        ///     VmAddUser
        /// </summary>
        public VmAddOrganization() : base(ResViewAddOrganization.LblTitle, subTitle: ResViewAddOrganization.LblSubTitle)
        {
            SetViewProperties(true);
        }

        #region Properties

        /// <summary>
        ///     Alle verfügbaren Gemeinden
        /// </summary>
        public List<ExTown> Towns { get; set; } = new List<ExTown>();

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
        public VmPicker<ExTown> PickerTowns { get; set; } = new VmPicker<ExTown>(nameof(PickerTowns));

        /// <summary>
        ///     Picker enthält Elemente.
        /// </summary>
        public bool PickerTownsHasElements => PickerTowns.Any();

        #endregion

        /// <summary>
        ///     Updated die Liste der verfügbaren Regionen
        /// </summary>
        public void UpdatePickerTowns()
        {
            if (!Towns.Any())
            {
                Towns = TownHelper.GetTowns().Where(x => x.IsMainPostalCode).ToList();
            }

            var validSearchString = TownSearchText.Length >= 3;

            var towns = validSearchString
                ? Towns.Where(t =>
                    t.Name.ToUpperInvariant().Contains(TownSearchText.ToUpperInvariant(), StringComparison.InvariantCulture) ||
                    t.PostalCode.Contains(TownSearchText, StringComparison.InvariantCulture))
                : new List<ExTown>();

            PickerTowns.Clear();

            if (validSearchString)
            {
                foreach (var t in towns)
                {
                    PickerTowns.AddKey(t, t.NamePlzString);
                }
            }

            this.InvokeOnPropertyChanged(nameof(PickerTownsHasElements));
        }

        private void PickerTowns_SelectedItemChanged(object sender, SelectedItemEventArgs<VmPickerElement<ExTown>> e)
        {
            if (e.CurrentItem != null!)
            {
                var town = e.CurrentItem.Key;

                Data.PostalCode = town.PostalCode;
                Data.Name = town.Name;
            }
            else
            {
                Data.PostalCode = string.Empty;
                Data.Name = string.Empty;
            }
        }

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            if (!Dc.DcExUser.Data.IsSysAdmin)
            {
                Nav.Back();
            }

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var r = base.OnActivated(args);

            CheckBeforeSaving = async () =>
            {
                if (PickerTowns.SelectedItem == null! || string.IsNullOrWhiteSpace(Data.Name))
                {
                    await MsgBox.Show("Bitte einen Ort auswählen!").ConfigureAwait(true);
                    return false;
                }

                if (Dc.DcExOrganization.Any(x =>
                        x.Data.PostalCode == Data.PostalCode &&
                        x.Data.Name == Data.Name &&
                        x.Data != Data))
                {
                    await MsgBox.Show("Gemeinde bereits vorhanden! Bitte einen anderen Ort auswählen!").ConfigureAwait(true);
                    return false;
                }

                return true;
            };

            EntryTownSearch = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewEditUser.EntryTitleTownSearch,
                "",
                this,
                nameof(TownSearchText),
                maxChar: 50,
                showTitle: !TabletMode);

            EntryTownSearch.PropertyChanged += (sender, eventArgs) => { UpdatePickerTowns(); };

            PickerTowns.SelectedItemChanged += PickerTowns_SelectedItemChanged;

            return r;
        }

        #endregion
    }
}