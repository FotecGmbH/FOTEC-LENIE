// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Exchange.Model.Organization;

namespace Exchange.Helper
{
    /// <summary>
    ///     <para>Helper für Gemeinden</para>
    ///     Klasse TownHelper. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class TownHelper
    {
        private static List<ExTown>? _towns;

        /// <summary>
        ///     Alle Gemeinden mit PLZ auslesen
        /// </summary>
        /// <returns></returns>
        public static List<ExTown> GetTowns()
        {
            if (_towns != null)
            {
                return _towns;
            }

            var csvName = "Exchange.Resources.gemliste_nam.csv";
            var assembly = Assembly.Load(new AssemblyName("Exchange"));
            var csvStream = assembly.GetManifestResourceStream(csvName);

            var reader = new StreamReader(csvStream);
            var csvText = reader.ReadToEnd();

            reader.Dispose();

            var towns = new List<ExTown>();

            foreach (var line in csvText.Split("\n"))
            {
                var cols = line.Split(";");

                if (cols.Length != 6)
                {
                    continue;
                }

                towns.Add(new ExTown
                          {
                              TownCode = cols[0],
                              Name = cols[1],
                              PostalCode = cols[4],
                              IsMainPostalCode = true,
                          });

                var additional = cols[5];
                if (additional != null && !string.IsNullOrWhiteSpace(additional))
                {
                    var otherPostals = additional.Split(" ");

                    foreach (var otherPostal in otherPostals)
                    {
                        if (string.IsNullOrWhiteSpace(otherPostal))
                        {
                            continue;
                        }

                        towns.Add(new ExTown
                                  {
                                      TownCode = cols[0],
                                      Name = cols[1],
                                      PostalCode = otherPostal,
                                      IsMainPostalCode = false,
                                  });
                    }
                }
            }

            _towns = towns;

            return _towns;
        }
    }
}