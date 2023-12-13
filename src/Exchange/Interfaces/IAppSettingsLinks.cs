// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Apps.Interfaces;

namespace Exchange.Interfaces
{
    /// <summary>
    ///     <para>Links des Projekts</para>
    ///     Interface IAppSettingsLinks. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public interface IAppSettingsLinks : IAppSettingsBase
    {
        #region Properties

        /// <summary>
        ///     App im Playstore
        /// </summary>
        string DroidLink { get; }

        /// <summary>
        ///     App im Appstore
        /// </summary>
        string IosLink { get; }

        /// <summary>
        ///     App im Windows Store
        /// </summary>
        string WindowsLink { get; }

        /// <summary>
        ///     deployte BlazorApp
        /// </summary>
        string BlazorLink { get; }

        /// <summary>
        ///     Link zum Appcenter iOS
        /// </summary>
        string IosTelemetryLink { get; }

        /// <summary>
        ///     Link zum Appcenter Android
        /// </summary>
        string DroidTelemetryLink { get; }

        /// <summary>
        ///     Link zu Application Insights
        /// </summary>
        string BlazorTelemetryLink { get; }

        /// <summary>
        ///     Link zu Portal.azure
        /// </summary>
        string AzureResourceLink { get; }

        /// <summary>
        ///     Link zu Fotec DevOps
        /// </summary>
        string DevOpsLink { get; }

        #endregion
    }
}