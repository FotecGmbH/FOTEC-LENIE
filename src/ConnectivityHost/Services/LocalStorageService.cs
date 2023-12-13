// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     Interface Lokaler Speicher
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        ///     Item abfragen
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        Task<T> GetItemAsync<T>(string key);

        /// <summary>
        ///     Item in den lokalen Speicher schreiben
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <param name="value">wert</param>
        /// <returns></returns>
        Task SetItemAsync<T>(string key, T value);

        /// <summary>
        ///     Item löschen
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task RemoveItemAsync(string key);
    }

    /// <summary>
    ///     Service für den Zugriff auf lokale Daten
    /// </summary>
    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="jsRuntime"></param>
        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        #region Interface Implementations

        /// <summary>
        ///     Item abfragen
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">key</param>
        /// <returns></returns>
        public async Task<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key).ConfigureAwait(true);

            if (string.IsNullOrEmpty(json))
            {
                return default!;
            }

            return JsonSerializer.Deserialize<T>(json)!;
        }

        /// <summary>
        ///     Item setzen
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Wert</param>
        /// <returns></returns>
        public async Task SetItemAsync<T>(string key, T value)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value)).ConfigureAwait(true);
        }

        /// <summary>
        ///     Item löschen
        /// </summary>
        /// <param name="key">key</param>
        /// <returns></returns>
        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key).ConfigureAwait(true);
        }

        #endregion
    }
}