// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.ComponentModel;
using Biss.Interfaces;
using Exchange.Enum;
using Exchange.Model.Organization;

namespace Exchange.Model.User
{
    /// <summary>
    ///     <para>Rechte in einer Firma falls der User nicht Superadmin (in TableUser = Admin = True) ist</para>
    ///     Klasse ExUserPremission. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExUserPermission : IBissModel
    {
        #region Properties

        /// <summary>
        ///     Db id.
        /// </summary>
        public long DbId { get; set; }

        /// <summary>
        ///     Recht für die Firma mit der Id
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        ///     Die Gemeinde für die der User Rechte hat
        /// </summary>
        public ExOrganization? Town { get; set; }

        /// <summary>
        ///     Hauptwohnsitz für User
        /// </summary>
        public bool IsMainCompany { get; set; }

        /// <summary>
        ///     Rolle des Users in der Firma
        /// </summary>
        public EnumUserRole UserRole { get; set; }

        /// <summary>
        ///     Rechte des Users bei der Firma
        /// </summary>
        public EnumUserRight UserRight { get; set; }

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