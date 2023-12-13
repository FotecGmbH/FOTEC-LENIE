// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Log.Producer;
using Exchange.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     <para>SA Demo Zugriff</para>
    ///     Klasse SaDemoController. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [AllowAnonymous]
    public class SaDemoController : Controller
    {
        private static int _counter;


        /// <summary>
        ///     Add
        /// </summary>
        /// <returns></returns>
        [Route("api/Add/{a}/{b}")]
        [HttpGet]
        public int GetRandom(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        ///     GetDemoData
        /// </summary>
        /// <returns></returns>
        [Route("api/DemoData/")]
        [HttpPost]
        public ExDemoData DemoData([FromBody] ExDemoData request)
        {
            if (request == null!)
            {
                throw new NullReferenceException();
            }

            Logging.Log.LogTrace($"GetDemoData: Post Data: {request.Data}, {request.Counter}");
            _counter++;

            return new ExDemoData
                   {
                       Counter = _counter,
                       Data = "Daten via api/GetDemoData/"
                   };
        }
    }
}