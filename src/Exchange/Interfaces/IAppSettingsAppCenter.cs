// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

namespace Exchange.Interfaces
{
    /// <summary>
    ///     <para>Appcenter für Crashes</para>
    ///     Interface IAppSettingsAppCenter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAppSettingsAppCenter
    {
        #region Properties

        /// <summary>
        ///     Appcenter Secret Android
        /// </summary>
        string DroidAppcenter { get; }

        /// <summary>
        ///     Appcenter Secret iOS
        /// </summary>
        string IosAppcenter { get; }

        /// <summary>
        ///     Appcenter Secret WPF
        /// </summary>
        string WpfAppcenter { get; }

        #endregion
    }
}