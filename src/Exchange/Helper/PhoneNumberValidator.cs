// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

namespace Exchange.Helper
{
    /// <summary>
    ///     <para>Validieren von Telefonnummern</para>
    ///     Klasse PhoneNumberValidator. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class PhoneNumberValidator
    {
        /// <summary>
        ///     Validieren, ob die Telefonnummer gültig ist
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static bool IsValid(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            if (phoneNumber.Contains(' ', StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!phoneNumber.StartsWith('+'))
            {
                return false;
            }

            return true;
        }
    }
}