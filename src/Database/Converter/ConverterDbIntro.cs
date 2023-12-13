// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model;

namespace Database.Converter
{
    /// <summary>
    ///     <para>Intro</para>
    ///     Klasse ConverterDbIntro. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class ConverterDbIntro
    {
        /// <summary>
        ///     Intro
        /// </summary>
        /// <param name="dbIntro"></param>
        /// <returns></returns>
        public static ExIntroItem ToExIntro(this TableIntro dbIntro)
        {
            if (dbIntro == null)
            {
                throw new ArgumentNullException(nameof(dbIntro));
            }

            return new ExIntroItem
                   {
                       HtmlSource = dbIntro.Weblink,
                   };
        }
    }
}