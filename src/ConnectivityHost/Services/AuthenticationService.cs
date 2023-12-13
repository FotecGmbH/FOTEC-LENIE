// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Apps.Base;
using Database;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     Authentication Service - Interface
    /// </summary>
    public interface IAuthenticationService
    {
        #region Properties

        /// <summary>
        ///     Benutzer
        /// </summary>
        static AuthUser User { get; } = null!;

        #endregion

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="password">Passwort</param>
        /// <returns></returns>
        Task Login(string username, string password);

        /// <summary>
        ///     Logout
        /// </summary>
        /// <returns></returns>
        Task Logout();
    }

    /// <summary>
    ///     Authentication Service
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly NavigationManager _navigationManager;

        /// <summary>
        ///     CTOR
        /// </summary>
        /// <param name="navigationManager">Nav Manager</param>
        /// <param name="localStorageService">Lokaler Speicher</param>
        public AuthenticationService(
            NavigationManager navigationManager,
            ILocalStorageService localStorageService
        )
        {
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;

            _ = Task.Run(async () => await Initialize().ConfigureAwait(true));
        }

        #region Properties

        /// <summary>
        ///     Benutzer
        /// </summary>
        public static AuthUser User { get; private set; } = null!;

        #endregion

        #region Interface Implementations

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            //Userdaten aus lokalem Speicher auslesen
            User = await _localStorageService.GetItemAsync<AuthUser>("user").ConfigureAwait(true);
        }

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="username">Benutzername</param>
        /// <param name="password">Passwort</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Login(string username, string password)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var pwdHash = AppCrypt.CumputeHash(password);

            var user = await db.TblUsers.FirstOrDefaultAsync(a => a.IsAdmin == true && a.LoginName == username && a.PasswordHash == pwdHash).ConfigureAwait(true);
            if (user is null)
            {
                throw new Exception();
            }

            var u = new AuthUser
                    {
                        Id = user.Id,
                        Token = user.JwtToken,
                        Username = user.LoginName
                    };

            User = u;

            await _localStorageService.SetItemAsync("user", u).ConfigureAwait(true);
        }

        /// <summary>
        ///     Logout
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            User = null!;
            await _localStorageService.RemoveItemAsync("user").ConfigureAwait(true);
            _navigationManager.NavigateTo("login");
        }

        #endregion
    }

    /// <summary>
    ///     Benutzer
    /// </summary>
    public class AuthUser
    {
        #region Properties

        /// <summary>
        ///     ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Benutzername
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        ///     Token
        /// </summary>
        public string Token { get; set; } = null!;

        #endregion
    }
}