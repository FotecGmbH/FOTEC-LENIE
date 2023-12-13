// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Diagnostics.CodeAnalysis;
using Biss.AppConfiguration;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;

namespace Exchange
{
    /// <summary>
    ///     <para>Konstanten für alle Projekte</para>
    ///     Klasse Constants. (C) 2020 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static class Constants
    {
        /// <summary>
        ///     Titel für die Apps
        /// </summary>
        public static string MainTitle = "Template";

        /// <summary>
        ///     Aktuelle App Settings für verschiedene Versionen (Release, Beta, Dev)
        /// </summary>
        public static AppConfigurationConstants AppConfiguration = new AppConfigurationConstants(AppSettings.Current().AppConfigurationConstants);

        static Constants()
        {
            Logging.Init(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
        }

        #region Properties

        /// <summary>
        ///     Deeplink für Idee
        /// </summary>
        public static string DeepLinkIdea => "IdeaId";

        /// <summary>
        ///     Deeplink für Chat
        /// </summary>
        public static string DeepLinkChat => "ChatId";

        #endregion
    }
}