// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Globalization;
using Exchange.Enum;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>Converter für DateTimes - abweichung zu jetzt</para>
    ///     Klasse ConverterDateTimeForUi. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterDateTimeForUi : IValueConverter
    {
        #region Interface Implementations

        /// <summary>
        ///     Konvertiert ein Objekt für XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das UI</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder null</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime checkDate)
            {
                if (parameter is EnumDateTimeConverter format)
                {
                    switch (format)
                    {
                        case EnumDateTimeConverter.DateAndTime:
                            return $"{checkDate.ToShortDateString()} {checkDate.ToShortTimeString()}";
                        case EnumDateTimeConverter.DateOnly:
                            return $"{checkDate.ToShortDateString()}";
                        case EnumDateTimeConverter.TimeOnly:
                            return $"{checkDate.ToShortTimeString()}";
                        case EnumDateTimeConverter.TimeDiffToNow:
                        {
                            var difference = DateTime.UtcNow - checkDate;

                            if (difference.TotalMinutes < 1)
                            {
                                return "gerade eben";
                            }

                            if (difference.TotalMinutes < 2)
                            {
                                return "vor einer Minute";
                            }

                            if (difference.TotalHours < 1)
                            {
                                return $"vor {difference.Minutes} Minuten";
                            }

                            if (difference.TotalHours < 2)
                            {
                                return "vor einer Stunde";
                            }

                            if (difference.TotalDays < 1)
                            {
                                return $"vor {difference.Hours} Stunden";
                            }

                            if (difference.TotalDays < 2)
                            {
                                return "vor einem Tag";
                            }

                            if (difference.TotalDays < 14)
                            {
                                return $"vor {difference.Days} Tagen";
                            }

                            if (difference.TotalDays < 56)
                            {
                                return $"vor {difference.Days / 7} Wochen";
                            }

                            return $"seit {checkDate.ToShortDateString()}";
                        }
                    }
                }

                return $"{checkDate.ToShortDateString()} {checkDate.ToShortTimeString()}";
            }

            return null!;
        }

        /// <summary>
        ///     Konvertiert ein Objekt von XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das Datenobjekt</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder UnsetValue</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }

        #endregion
    }
}