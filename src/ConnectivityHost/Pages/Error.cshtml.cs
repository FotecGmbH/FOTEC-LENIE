// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     Error Model
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        #region Properties

        /// <summary>
        ///     Request ID
        /// </summary>
        public string RequestId { get; set; } = null!;

        /// <summary>
        ///     Show ID
        /// </summary>

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        #endregion

        /// <summary>
        ///     HTTPGET
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}