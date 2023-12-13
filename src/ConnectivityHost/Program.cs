// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Exchange;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ConnectivityHost
{
    /// <summary>
    ///     <para>Program</para>
    ///     Klasse Program. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     Main
        /// </summary>
        /// <param name="args">Args</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        ///     Host Builder erzeugen
        /// </summary>
        /// <param name="args">args</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls(AppSettings.Current().DcSignalHost)
                        .UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                });
        }
    }
}