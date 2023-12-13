// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Status einer Nachricht im Chat</para>
    ///     Klasse EnumChatRecipientState. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumChatRecipientState
    {
        /// <summary>
        ///     Wird aktuell verschickt
        /// </summary>
        Sending = 0,

        /// <summary>
        ///     Versendet
        /// </summary>
        Sent = 1,

        /// <summary>
        ///     Empfangen
        /// </summary>
        Received = 2,

        /// <summary>
        ///     Gelesen
        /// </summary>
        Read = 3,
    }
}