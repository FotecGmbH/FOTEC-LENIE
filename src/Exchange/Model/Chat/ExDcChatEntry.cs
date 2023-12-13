// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using Biss.Dc.Core;
using Biss.Dc.Core.DcChat;
using Biss.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;

namespace Exchange.Model.Chat
{
    /// <summary>
    ///     <para>ExDcChatEntry - Einzelner Chat Eintrag</para>
    ///     Klasse ExDcChatEntry. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ExDcChatEntry : IBissModel, IDcChatEntry, IComparable<ExDcChatEntry>, IComparable
    {
        #region Properties

        /// <summary>
        ///     Eindeutige Id des Eintrags
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Id des dazugeörigen Chat
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        ///     Verfasser der Nachricht
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        ///     Verfasungszeitpunkt
        /// </summary>
        public DateTime TimeStampUtc { get; set; }

        /// <summary>
        ///     Lokale Zeit
        /// </summary>
        [JsonIgnore]
        [DependsOn(nameof(TimeStampUtc))]
        public DateTime TimeStamp => TimeStampUtc.ToLocalTime();

        /// <summary>
        ///     Text-Eintrag
        /// </summary>
        public string Message { get; set; } = string.Empty;

        #endregion

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return ((IDcChangeTracking) this).DebugInfos();
        }

        #region Interface Implementations

        /// <inheritdoc />
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This instance precedes <paramref name="obj" /> in the sort order.
        ///     Zero
        ///     This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///     Greater than zero
        ///     This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ExDcChatEntry other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ExDcChatEntry)}");
        }

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This instance precedes <paramref name="other" /> in the sort order.
        ///     Zero
        ///     This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///     Greater than zero
        ///     This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(ExDcChatEntry? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return TimeStampUtc.CompareTo(other.TimeStampUtc);
        }

        #endregion

        /// <summary>
        ///     Version der Daten (aus Db)
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] DataVersion { get; set; } = null!;

        /// <summary>
        ///     Ist archiviert.
        /// </summary>
        public bool IsArchived { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}