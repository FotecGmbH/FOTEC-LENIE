// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Biss.CsBuilder.Sql;
using Biss.Email;

namespace WebExchange
{
    /// <summary>
    ///     <para>Konstanten für WebProjekte</para>
    ///     Klasse Constants. (C) 2020 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class WebConstants
    {
        /// <summary>
        ///     Aktuelle WebSettings
        /// </summary>
        public static WebSettings CurrentWebSettings = WebSettings.Current();

        /// <summary>
        ///     DB Connection String
        /// </summary>
        public static string ConnectionString = string.IsNullOrEmpty(WebSettings.Current().ConnectionString)
            ? new CsBuilderSql(WebSettings.Current().ConnectionStringDbServer, WebSettings.Current().ConnectionStringDb, WebSettings.Current().ConnectionStringUser, WebSettings.Current().ConnectionStringUserPwd, SqlCommonStandardApplicationName.EntityFramework).ToString()
            : WebSettings.Current().ConnectionString;

        /// <summary>
        ///     Biss Email mit Sendgrid Key
        /// </summary>
        public static BissEMail Email = new BissEMail(new SendGridCredentials
                                                      {
                                                          ApiKeyV3 = WebSettings.Current().SendGridApiKey,
                                                      });
    }
}