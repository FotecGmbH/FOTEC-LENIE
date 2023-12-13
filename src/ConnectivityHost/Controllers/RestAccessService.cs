// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Biss.Apps.Base.Connectivity.Model;
using Biss.Apps.Service.Connectivity.Interfaces;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     <para>RestAccessService</para>
    ///     Klasse RestAccessService. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class RestAccessService : IRestAccessService
    {
        #region Properties

        /// <summary>
        ///     Infos über die App
        /// </summary>
        public ExPingResult? PingResult { get; set; } = null!;

        /// <summary>
        ///     Geheimer Schlüssel für den JWT Token
        /// </summary>
        public string AppSecret => "appSettings.Secret";

        /// <summary>
        ///     Gespeicherte RefreshTokens
        /// </summary>
        public ConcurrentDictionary<long, List<ExRefreshToken>> LstRefreshToken { get; } = new ConcurrentDictionary<long, List<ExRefreshToken>>();

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Benutzer abfragen (Id, RestPasswort)
        /// </summary>
        public ExAuthUser GetUser(long id, string token)
        {
            return new ExAuthUser
                   {
                       Username = "u.PhoneNumber",
                       Id = 1,
                       FirstName = "u.UserName"
                   };
        }

        /// <summary>
        ///     Benutzer abfragen (Id)
        /// </summary>
        public ExAuthUser GetUserById(long userid)
        {
            return new ExAuthUser
                   {
                       Username = "u.PhoneNumber",
                       Id = 1,
                       FirstName = "u.UserName"
                   };
        }

        /// <summary>
        ///     RefreshToken hinzufügen
        /// </summary>
        public bool AddRefreshToken(long userId, ExRefreshToken token)
        {
            return true;
        }

        /// <summary>
        ///     RefreshToken entfernen
        /// </summary>
        public bool RemoveRefreshToken(long userId, ExRefreshToken token)
        {
            return true;
        }

        #endregion
    }
}