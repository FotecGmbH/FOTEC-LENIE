// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

namespace Exchange.Enum
{
    /// <summary>
    ///     <para>Dc Commands zum Server</para>
    ///     Klasse EnumDcCommonCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDcCommonCommands
    {
        /// <summary>
        ///     Datei löschen
        /// </summary>
        FileDelete,

        /// <summary>
        ///     Bestätigng E-Mail erneut senden
        /// </summary>
        ResendAccessEMail,

        /// <summary>
        ///     Passwort zurücksetzen starten
        /// </summary>
        ResetPassword,

        /// <summary>
        ///     Testpush senden
        /// </summary>
        SendTestPush,

        /// <summary>
        ///     Allgemeiner (Warn)hinweis
        /// </summary>
        SendCommonMsg,

        /// <summary>
        ///     Like/Dislike für Idee
        /// </summary>
        LikeIdea,

        /// <summary>
        ///     BestätigungsSMS neu schicken
        /// </summary>
        ResendConfirmationSms,

        /// <summary>
        ///     Bestätigung SMS
        /// </summary>
        ConfirmSms,
    }

    /// <summary>
    ///     <para>Dc Commands zum Client</para>
    ///     Klasse EnumDcCommonCommands. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public enum EnumDcCommonCommandsClient
    {
        /// <summary>
        ///     Allgemeine Meldung
        /// </summary>
        CommonMsg,

        /// <summary>
        ///     Dc Liste neu laden
        /// </summary>
        ReloadDcList
    }
}