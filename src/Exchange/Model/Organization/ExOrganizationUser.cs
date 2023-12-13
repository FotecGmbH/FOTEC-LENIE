// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Interfaces;
using Exchange.Enum;
using Exchange.Resources;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.Organization
{
    /// <summary>
    ///     <para>Benuter der Organisation</para>
    ///     Klasse ExCompanyUser. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExOrganizationUser : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Id der Organisation
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        ///     OrganizationPostalCode
        /// </summary>
        public string OrganizationPostalCode { get; set; } = string.Empty;

        /// <summary>
        ///     Id des User
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///     Loginname des User
        /// </summary>
        public string UserLoginEmail { get; set; } = string.Empty;

        /// <summary>
        ///     Telefonnummer des Users
        /// </summary>
        public string UserPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        ///     User hat seinen Account erfolgreich aktiviert
        /// </summary>
        public bool LoginDoneByUser { get; set; }

        /// <summary>
        ///     Rolle des Users in der Firma
        /// </summary>
        public EnumUserRole UserRole { get; set; }

        /// <summary>
        ///     Rechte des Users bei der Firma
        /// </summary>
        public EnumUserRight UserRight { get; set; }

        /// <summary>
        ///     Name des Users für das UI
        /// </summary>
        public string Fullname { get; set; } = string.Empty;

        /// <summary>
        ///     Name für Ui
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(Fullname))]
        public string FullNameUi => string.IsNullOrEmpty(Fullname.Trim()) ? ResCommon.LblUnknown : Fullname;

        /// <summary>
        ///     Systemadmins
        /// </summary>
        public bool IsSuperadmin { get; set; }

        /// <summary>
        ///     Ist gesperrt
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        ///     Bild des Users wenn vorhanden
        /// </summary>
        public string UserImageLink { get; set; } = string.Empty;

        /// <summary>
        ///     Bild (Link) vorhanden
        /// </summary>
        public bool HasImage => !string.IsNullOrEmpty(UserImageLink);

        /// <summary>
        ///     Userrolle Ui
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(IsSuperadmin), nameof(UserRole))]
        public string UserRoleUi
        {
            get
            {
                if (IsSuperadmin)
                {
                    return ResCommon.EnumUserRoleSysAdmin;
                }

                switch (UserRole)
                {
                    case EnumUserRole.User:
                        return ResCommon.EnumUserRoleUser;
                    case EnumUserRole.UserPlus:
                        return ResCommon.EnumUserRoleUserPlus;
                    case EnumUserRole.Admin:
                        return ResCommon.EnumUserRoleAdmin;
                    default:
                        return ResCommon.LblUnknown;
                }
            }
        }

        /// <summary>
        ///     Kann im Ui das "UserRight" verändert werden?
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(UserRight))]
        public bool CanEditUserRight => UserRole == EnumUserRole.User;

        /// <summary>
        ///     Kann der Loginname (noch) verändert werden?
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(UserId))]
        public bool CanEditLoginEMail => UserId <= 0;

        #endregion

        #region Interface Implementations

#pragma warning disable CS0067
#pragma warning disable CS0414
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged = null!;

#pragma warning restore CS0067
#pragma warning restore CS0414

        #endregion
    }
}