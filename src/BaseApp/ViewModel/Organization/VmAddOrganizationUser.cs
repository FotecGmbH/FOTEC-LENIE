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
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.ViewModel;
using Biss.EMail;
using Exchange.Enum;
using Exchange.Model.Organization;
using Exchange.Resources;

namespace BaseApp.ViewModel.Organization
{
    /// <summary>
    ///     <para>VmAddOrganizationUser</para>
    ///     Klasse VmAddOrganizationUser. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddOrganizationUser", true)]
    public class VmAddOrganizationUser : VmEditDcListPoint<ExOrganizationUser>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddUser.DesignInstance}"
        /// </summary>
        public static VmAddOrganizationUser DesignInstance = new VmAddOrganizationUser();

        private List<string> _currentUsers = new List<string>();

        /// <summary>
        ///     VmAddUser
        /// </summary>
        public VmAddOrganizationUser() : base(ResViewAddOrganizationUser.LblTitle, subTitle: ResViewAddOrganizationUser.LblSubTitle)
        {
            SetViewProperties(true);

            PickerUserRole.AddKey(EnumUserRole.Admin, ResCommon.EnumUserRoleAdmin);
            PickerUserRole.AddKey(EnumUserRole.UserPlus, ResCommon.EnumUserRoleUserPlus);
            PickerUserRole.AddKey(EnumUserRole.User, ResCommon.EnumUserRoleUser);
        }

        #region Properties

        /// <summary>
        ///     Rolle im Unternehmen
        /// </summary>
        public VmPicker<EnumUserRole> PickerUserRole { get; private set; } = new VmPicker<EnumUserRole>(nameof(PickerUserRole));

        /// <summary>
        ///     Loginname
        /// </summary>
        public VmEntry EntryLoginName { get; private set; } = null!;

        #endregion

        /// <summary>
        ///     View wurde erzeugt und geladen - Aber noch nicht sichtbar (gerendert)
        /// </summary>
        public override async Task OnActivated(object? args = null)
        {
            if (!await CheckConnected().ConfigureAwait(true))
            {
                GCmdHome.IsSelected = true;
                return;
            }

            await base.OnActivated(args).ConfigureAwait(true);

            _currentUsers = Dc.DcExOrganizationUsers
                .Where(w =>
                    (w.Data.OrganizationId == Data.OrganizationId || w.Data.IsSuperadmin)
                    && !string.IsNullOrEmpty(w.Data.UserLoginEmail))
                .Select(s => s.Data.UserLoginEmail).ToList();

            EntryLoginName = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewAddOrganizationUser.EntryTitleEmail,
                ResViewAddOrganizationUser.EntryPlaceholderEmail,
                Data,
                nameof(ExOrganizationUser.UserLoginEmail),
                ValidateLoginName,
                showTitle: !TabletMode,
                showMaxChar: false
            );

            PickerUserRole.SelectKey(Data.UserRole);

            PickerUserRole.SelectedItemChanged += (sender, eventArgs) => { Data.UserRole = eventArgs.CurrentItem.Key; };
        }

        /// <summary>
        ///     Validierung für Loginname
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private (string hint, bool valid) ValidateLoginName(string arg)
        {
            if (Data.CanEditLoginEMail)
            {
                var r = VmEntryValidators.ValidateFuncStringEmpty(arg);
                if (!r.valid)
                {
                    return r;
                }

                if (!Validator.Check(arg))
                {
                    return (ResCommon.ValNoEmail, false);
                }

                if (_currentUsers.Any(a => string.Equals(a, arg, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return (ResViewAddOrganizationUser.ValUserDuplicate, false);
                }
            }

            return (string.Empty, true);
        }
    }
}