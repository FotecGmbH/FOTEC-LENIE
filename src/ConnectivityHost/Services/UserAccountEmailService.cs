// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Biss.AppConfiguration;
using Biss.Apps.Base;
using Biss.Log.Producer;
using ConnectivityHost.Controllers;
using Database;
using Database.Tables;
using Exchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     <para>Service für Benutzer-Accounts</para>
    ///     Klasse UserAccountEmailService. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class UserAccountEmailService
    {
        private readonly ICustomRazorEngine _razorEngine;

        /// <summary>
        ///     Konstruktor
        /// </summary>
        /// <param name="razorEngine">RazorEngine</param>
        public UserAccountEmailService(ICustomRazorEngine razorEngine)
        {
            _razorEngine = razorEngine;
        }

        /// <summary>
        ///     Sendet Bestätigungemail nach Registrierung
        /// </summary>
        /// <param name="user"></param>
        /// <param name="deviceId">device Id von dem die Anforderung gekommen ist</param>
        /// <param name="db">Datenbank</param>
        /// <returns></returns>
        public async Task<bool> SendValidationMail(TableUser user, long deviceId, Db db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Notwendige Parameter werden gesetzt
            var bem = WebConstants.Email;
            var subject = "Bitte bestätige deine Registrierung";
            var receiverBetaInfo = "";
            List<string> ccReceifer = new();
            var receiver = user.LoginName;

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            _ = await db.SaveChangesAsync().ConfigureAwait(true);

            var url = $"{AppSettings.Current().SaApiHost}{nameof(WebLinksController.UserValidateEMail)}/{deviceId}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"SendCheckEMail: {url}");

            // Für Beta Versionen
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                receiverBetaInfo = $"To: {receiver}, CC: ";
                foreach (var cc in ccReceifer)
                {
                    receiverBetaInfo += $"{cc}; ";
                }

                ccReceifer = new();
            }

            var user4Mail = new EMailModelUser
                            {
                                Firstname = user.FirstName,
                                Lastname = user.LastName,
                                Link = url
                            };

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailVerification.cshtml", user4Mail).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Sendet Bestätigungemail nach Benutzeranlage wenn der User von einem anderen User angelegt wurde
        /// </summary>
        /// <param name="user"></param>
        /// <param name="db">Datenbank</param>
        /// <param name="password">Das generierte Passwort</param>
        /// <param name="route">Route (Methode des Controllers)</param>
        /// <returns></returns>
        public async Task<bool> SendValidationMailWithoutDevice(TableUser user, Db db, string password, string route = "UserValidateEMail")
        {
            if (db == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): {nameof(db)} is null");
            }

            if (user == null)
            {
                throw new ArgumentNullException($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): {nameof(user)} is null");
            }

            // Notwendige Parameter werden gesetzt
            var bem = WebConstants.Email;
            var subject = "Bitte bestätige deine Registrierung";
            var receiverBetaInfo = "";
            List<string> ccReceifer = new();
            var receiver = user.LoginName;

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            _ = await db.SaveChangesAsync().ConfigureAwait(true);

            var url = $"{AppSettings.Current().SaApiHost}{route}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"[{nameof(UserAccountEmailService)}]({nameof(SendValidationMailWithoutDevice)}): url: {url}");

            // Für Beta Versionen
            if (Constants.AppConfiguration.CurrentBuildType != EnumCurrentBuildType.CustomerRelease)
            {
                receiverBetaInfo = $"To: {receiver}, CC: ";
                foreach (var cc in ccReceifer)
                {
                    receiverBetaInfo += $"{cc}; ";
                }

                ccReceifer = new();
            }

            var user4Mail = new EMailModelUser
                            {
                                Firstname = user.FirstName,
                                Lastname = user.LastName,
                                Username = user.LoginName,
                                Link = url,
                                Password = password
                            };

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailVerification.cshtml", user4Mail).ConfigureAwait(true);

            // Email wird gesendet
            var sendResult = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);
            return sendResult;
        }

        /// <summary>
        ///     Passwort zurücksetzen
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <param name="db">Datenbank</param>
        /// <returns></returns>
        public async Task<bool> SendPasswordResetMail(TableUser user, Db db)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            // Nötige Daten werden gesetzt
            var subject = "Setze dein Passwort zurück!";
            var receiverBetaInfo = "";
            var ccReceifer = new List<string>();

            Logging.Log.LogInfo($"UserStartResetPassword {user.Id}");

            user.ConfirmationToken = Guid.NewGuid().ToString("N");
            await db.SaveChangesAsync().ConfigureAwait(true);

            // PW Reset Url wird generiert
            var url = $"{AppSettings.Current().SaApiHost}{nameof(WebLinksController.UserResetPassword)}/{user.Id}/{user.ConfirmationToken}";
            Logging.Log.LogInfo($"Send StartResetPassword: {url}");
            var bem = WebConstants.Email;

            // Für Darstellung nötige Daten werden zusammen gesammelt
            var emailModel = new EMailModelUser {Firstname = user.FirstName, Lastname = user.LastName, Link = url};

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailPasswordReset.cshtml", emailModel).ConfigureAwait(true);

            // Senden der Email
            var result = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);

            return result;
        }

        /// <summary>
        ///     Erfolgsmeldung nach dem Zurücksetzen des Passwortes versenden
        /// </summary>
        /// <param name="user">Benutzer</param>
        /// <returns></returns>
        public async Task<bool> SendPasswordResetConfirmationMail(TableUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Nötige Daten werden gesetzt
            var subject = "Passwort wurde geändert!";
            var receiverBetaInfo = "";
            var ccReceifer = new List<string>();

            Logging.Log.LogInfo($"UserStartResetPassword {user.Id}");

            var newPwd = AppCrypt.GeneratePassword(5);

            //Passwort wird in der Datenbankgeändert und gespeichert
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var data = await db.TblUsers.FirstOrDefaultAsync(u => u.Id == user.Id).ConfigureAwait(true);

            data!.PasswordHash = AppCrypt.CumputeHash(newPwd);
            data.ConfirmationToken = string.Empty;

            // data.RefreshToken = string.Empty;
            // data.JwtToken = string.Empty;

            try
            {
                await db.SaveChangesAsync().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"UserResetPassword: {e}");
                return false;
            }

            Logging.Log.LogInfo("Send UserResetPassword");

            // Benötigte Daten für die Darstellung werden zusammengesammelt 
            var bem = WebConstants.Email;

            // Für Darstellung nötige Daten werden zusammen gesammelt
            var emailModel = new EMailModelUser {Firstname = user.FirstName, Lastname = user.LastName, Password = newPwd};

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailPasswordChanged.cshtml", emailModel).ConfigureAwait(true);

            // Email wird gesendet
            var result = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);

            return result;
        }

        /// <summary>
        ///     Mails für gemeldete Idee
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userIds"></param>
        /// <param name="reportUserId"></param>
        /// <param name="idea"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public async Task<bool> SendReportMail(Db db, List<long> userIds, long reportUserId, TableIdea idea, TableIdeaReport report)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            var users = db.TblUsers.Where(x => userIds.Contains(x.Id));
            var reportUser = db.TblUsers.FirstOrDefault(x => reportUserId == x.Id);
            var res = users.Any() && reportUser != null;

            if (res)
            {
                foreach (var user in users)
                {
                    res = await SendReportMail(user, reportUser!, idea, report).ConfigureAwait(true) && res;
                }
            }

            return res;
        }

        /// <summary>
        ///     Mail für gemeldete Idee
        /// </summary>
        /// <param name="user"></param>
        /// <param name="reportUser"></param>
        /// <param name="idea"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public async Task<bool> SendReportMail(TableUser user, TableUser reportUser, TableIdea idea, TableIdeaReport report)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (reportUser == null)
            {
                throw new ArgumentNullException(nameof(reportUser));
            }

            if (idea == null)
            {
                throw new ArgumentNullException(nameof(idea));
            }

            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            // Nötige Daten werden gesetzt
            var subject = "Eine Idee wurde gemeldet!";
            var receiverBetaInfo = "";
            var ccReceifer = new List<string>();

            // Benötigte Daten für die Darstellung werden zusammengesammelt 
            var bem = WebConstants.Email;

            // Für Darstellung nötige Daten werden zusammen gesammelt
            var emailModel = new EMailModelReportIdea
                             {
                                 IdeaTitle = idea.Title,
                                 Reason = report.Reason,
                                 ReportUserFirstName = reportUser.FirstName,
                                 ReportUserLastName = reportUser.LastName,
                                 ReportUserName = reportUser.LoginName,
                                 ReportUserPhone = reportUser.PhoneNumber,
                             };

            // HTML wird gerendert
            var htmlRendered = await _razorEngine
                .RazorViewToHtmlAsync("Views/EMail/EmailReportIdea.cshtml", emailModel).ConfigureAwait(true);

            // Email wird gesendet
            var result = await bem.SendHtmlEMail(WebSettings.Current().SendEMailAs, new List<string> {user.LoginName}, subject, htmlRendered + receiverBetaInfo, WebSettings.Current().SendEMailAsDisplayName, ccReceifer).ConfigureAwait(true);

            return result;
        }
    }
}