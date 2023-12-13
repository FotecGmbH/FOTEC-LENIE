// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Microsoft.AspNetCore.Mvc;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     Intro Controller für App Intro.
    /// </summary>
    public class IntroController : Controller
    {
        /// <summary>
        ///     Erste View des Intros.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Intro/One/")]
        public IActionResult One()
        {
            return View();
        }

        /// <summary>
        ///     Zweite View des Intros.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Intro/Two/")]
        public IActionResult Two()
        {
            return View();
        }

        /// <summary>
        ///     Dritte View des Intros.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Intro/Three/")]
        public IActionResult Three()
        {
            return View();
        }
    }
}