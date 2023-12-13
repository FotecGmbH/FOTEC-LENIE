// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Linq;
using System.Threading.Tasks;
using Biss.EMail;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Exchange.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectivityHost.Controllers
{
    /// <summary>
    ///     Controller für diverse Testmethoden
    /// </summary>
    [AllowAnonymous]
    public class TestController : Controller
    {
        /// <summary>
        ///     Razor Engine
        /// </summary>
        private readonly ICustomRazorEngine _razorEngine;

        /// <summary>
        ///     Controller für Testmethoden
        /// </summary>
        /// <param name="razorEngine"></param>
        public TestController(ICustomRazorEngine razorEngine)
        {
            _razorEngine = razorEngine;
        }

        /// <summary>
        ///     Alle Emails testen
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/TestAllMails/{email}")]
        [HttpGet]
        public async Task<bool> TestAllMails(string email)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.AsNoTracking()
                .FirstOrDefault(x => x.LoginName == email);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.LoginName) && Validator.Check(user.LoginName))
            {
                UserAccountEmailService ems = new(_razorEngine);
                await ems.SendValidationMail(user, 0, db).ConfigureAwait(true);
                await ems.SendValidationMailWithoutDevice(user, db, "YOUR_PASSWORD").ConfigureAwait(true);
                await ems.SendPasswordResetMail(user, db).ConfigureAwait(true);
                await ems.SendPasswordResetConfirmationMail(user).ConfigureAwait(true);
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && PhoneNumberValidator.IsValid(user.PhoneNumber))
            {
                await SmsHelper.SendValidationSms(user, db).ConfigureAwait(true);
            }

            return true;
        }

        /// <summary>
        ///     Mail für User hat sich registriert
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/TestRegistrationMail/{email}")]
        [HttpGet]
        public async Task<bool> TestRegistrationMail(string email)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.AsNoTracking()
                .FirstOrDefault(x => x.LoginName == email);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.LoginName) && Validator.Check(user.LoginName))
            {
                var ems = new UserAccountEmailService(_razorEngine);
                await ems.SendValidationMail(user, 0, db).ConfigureAwait(true);
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && PhoneNumberValidator.IsValid(user.PhoneNumber))
            {
                await SmsHelper.SendValidationSms(user, db).ConfigureAwait(true);
            }

            return true;
        }

        /// <summary>
        ///     Mail für User vom Admin angelegt
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/TestRegistrationMailWithoutDevice/{email}")]
        [HttpGet]
        public async Task<bool> TestRegistrationMailWithoutDevice(string email)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.AsNoTracking()
                .FirstOrDefault(x => x.LoginName == email);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.LoginName) && Validator.Check(user.LoginName))
            {
                UserAccountEmailService ems = new(_razorEngine);
                await ems.SendValidationMailWithoutDevice(user, db, "YOUR_PASSWORD").ConfigureAwait(true);
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && PhoneNumberValidator.IsValid(user.PhoneNumber))
            {
                await SmsHelper.SendValidationSms(user, db).ConfigureAwait(true);
            }

            return true;
        }

        /// <summary>
        ///     Email für "Wollen Sie Passwort ändern"
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/TestPasswordResetMail/{email}")]
        [HttpGet]
        public async Task<bool> TestPasswordResetMail(string email)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.AsNoTracking()
                .FirstOrDefault(x => x.LoginName == email);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.LoginName) && Validator.Check(user.LoginName))
            {
                UserAccountEmailService ems = new(_razorEngine);
                await ems.SendPasswordResetMail(user, db).ConfigureAwait(true);
            }

            return true;
        }

        /// <summary>
        ///     Email für "Passwort geändert"
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("api/TestPasswordChangeMail/{email}")]
        [HttpGet]
        public async Task<bool> TestPasswordChangeMail(string email)
        {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            var user = db.TblUsers.AsNoTracking()
                .FirstOrDefault(x => x.LoginName == email);

            if (user == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(user.LoginName) && Validator.Check(user.LoginName))
            {
                UserAccountEmailService ems = new(_razorEngine);
                await ems.SendPasswordResetConfirmationMail(user).ConfigureAwait(true);
            }

            return true;
        }
    }
}