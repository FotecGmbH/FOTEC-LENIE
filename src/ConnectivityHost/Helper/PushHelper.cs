// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biss.Apps.Service.Push;
using Exchange;
using Exchange.Resources;

namespace ConnectivityHost.Helper
{
    /// <summary>
    ///     <para>Helper für Push Notifications</para>
    ///     Klasse PushHelper. (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class PushHelper
    {
        /// <summary>
        ///     Senden des Likes.
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="ideaId"></param>
        /// <returns></returns>
        public static Task<PushResult> SendIdeaLike(List<string> devices, long? ideaId)
        {
            var commonInfo = new Dictionary<string, string>();

            if (ideaId.HasValue)
            {
                commonInfo.Add(Constants.DeepLinkIdea, ideaId.Value.ToString());
            }

            return PushService.Instance.SendMessageToDevicesWithData(ResWebCommon.PushIdeaLikeTitle, ResWebCommon.PushIdeaLikeMessage, commonInfo, devices);
        }

        /// <summary>
        ///     Senden der Idee.
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="ideaId"></param>
        /// <returns></returns>
        public static Task<PushResult> SendSupportForIdea(List<string> devices, long? ideaId)
        {
            var commonInfo = new Dictionary<string, string>();

            if (ideaId.HasValue)
            {
                commonInfo.Add(Constants.DeepLinkIdea, ideaId.Value.ToString());
            }

            return PushService.Instance.SendMessageToDevicesWithData(ResWebCommon.PushNewSupportTitle, ResWebCommon.PushNewSupportMessage, commonInfo, devices);
        }

        /// <summary>
        ///     Senden einer neuen Idee.
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="ideaId"></param>
        /// <returns></returns>
        public static Task<PushResult> SendNewIdea(List<string> devices, long? ideaId)
        {
            var commonInfo = new Dictionary<string, string>();

            if (ideaId.HasValue)
            {
                commonInfo.Add(Constants.DeepLinkIdea, ideaId.Value.ToString());
            }

            return PushService.Instance.SendMessageToDevicesWithData(ResWebCommon.PushNewIdeaTitle, ResWebCommon.PushNewIdeaMessage, commonInfo, devices);
        }

        /// <summary>
        ///     Senden eines Reports für die idee.
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="ideaId"></param>
        /// <returns></returns>
        public static Task<PushResult> SendReportForIdea(List<string> devices, long? ideaId)
        {
            var commonInfo = new Dictionary<string, string>();

            if (ideaId.HasValue)
            {
                commonInfo.Add(Constants.DeepLinkIdea, ideaId.Value.ToString());
            }

            return PushService.Instance.SendMessageToDevicesWithData(ResWebCommon.PushNewReportTitle, ResWebCommon.PushNewReportMessage, commonInfo, devices);
        }

        /// <summary>
        ///     Senden der Chat Nachricht.
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="chatId"></param>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static Task<PushResult> SendChatMessage(List<string> devices, long? chatId, string message, string title)
        {
            var commonInfo = new Dictionary<string, string>();

            if (chatId.HasValue)
            {
                commonInfo.Add(Constants.DeepLinkChat, chatId.Value.ToString());
            }

            return PushService.Instance.SendMessageToDevicesWithData(title, message, commonInfo, devices);
        }

        /// <summary>
        ///     Aussenden der Testnachricht.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Task<PushResult> SendTestMessage(string device)
        {
            return PushService.Instance.SendMessageToDevice(
                "Testnachricht",
                $"Testnachricht gesendet um {DateTime.Now.ToShortTimeString()} an Device: {device}",
                device);
        }

        /// <summary>
        ///     10 Minuten Push Nacht.
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        public static Task<PushResult> Send10MinPush(List<string> devices)
        {
            return PushService.Instance.SendMessageToDevices("10 Minuten", "10 Minuten Push Nachricht", devices);
        }
    }
}