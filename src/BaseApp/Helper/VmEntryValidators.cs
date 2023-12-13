// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.EMail;
using Exchange.Helper;
using Exchange.Resources;

namespace BaseApp.Helper
{
    /// <summary>
    ///     <para>VmEntry allegemeine Prüffunktionen</para>
    ///     Klasse VmEntryValidators. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class VmEntryValidators
    {
        /// <summary>
        ///     String nach Int
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncInt(string arg)
        {
            if (!int.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine ganze Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Long
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncLong(string arg)
        {
            if (!long.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine ganze Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Long
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncLongPositive(string arg)
        {
            if (!long.TryParse(arg, out var value))
            {
                return ("Ungültig. Muss eine ganze Zahl sein!", false);
            }

            if (value < 0)
            {
                return ("Ungültig. Muss eine positive Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Int
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncByte(string arg)
        {
            if (!byte.TryParse(arg, out _))
            {
                return ("Ungültig. Muss ein Byte sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nach Double
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncDouble(string arg)
        {
            if (!double.TryParse(arg, out _))
            {
                return ("Ungültig. Muss eine Zahl sein!", false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nicht leer
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncStringEmpty(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     String nicht leer für Suche
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncStringEmptySearch(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (string.Empty, false);
            }

            if (string.IsNullOrWhiteSpace(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     Eingabevalidierung für Passwort Feld
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidatePwdFunc(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     Eingabevalidierung für Telefonnummer Feld
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncEmail(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            if (!Validator.Check(arg))
            {
                return (ResCommon.ValNoEmail, false);
            }

            if (arg.Contains(' ', StringComparison.CurrentCultureIgnoreCase))
            {
                return (ResCommon.ValNoWhitespace, false);
            }

            return (string.Empty, true);
        }

        /// <summary>
        ///     Eingabevalidierung für Telefonnummer Feld
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static (string hint, bool valid) ValidateFuncTelephone(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return (ResCommon.ValNotEmpty, false);
            }

            if (arg.Equals("+43", StringComparison.CurrentCultureIgnoreCase))
            {
                return (string.Empty, false);
            }

            if (!PhoneNumberValidator.IsValid(arg))
            {
                return (ResCommon.ValNoPhone, false);
            }

            if (arg.Contains(' ', StringComparison.CurrentCultureIgnoreCase))
            {
                return (ResCommon.ValNoWhitespace, false);
            }

            return (string.Empty, true);
        }
    }
}