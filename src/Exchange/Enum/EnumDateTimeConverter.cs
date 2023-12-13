// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Konvertieren von DateTime</para>
    ///     Klasse EnumDateTimeConverter. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDateTimeConverter
    {
        /// <summary>
        ///     Gesamtes Datum mit Zeit
        /// </summary>
        DateAndTime,

        /// <summary>
        ///     Nur Datum
        /// </summary>
        DateOnly,

        /// <summary>
        ///     Nur Zeit
        /// </summary>
        TimeOnly,

        /// <summary>
        ///     Seit ...
        /// </summary>
        TimeDiffToNow,
    }
}