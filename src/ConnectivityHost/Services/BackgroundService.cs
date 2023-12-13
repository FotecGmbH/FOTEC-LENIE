// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Biss.Dc.Server;
using Biss.Dc.Transport.Server.SignalR;
using Biss.Log.Producer;
using ConnectivityHost.DataConnector;
using ConnectivityHost.Helper;
using Database;
using Database.Converter;
using Exchange.Enum;
using Exchange.Resources;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebExchange;

namespace ConnectivityHost.Services
{
    /// <summary>
    ///     <para>Hintergrundservice</para>
    ///     Klasse BackgroundService. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BackgroundService<T> : IHostedService, IDisposable
    {
        private readonly IDcConnections _clientConnection;
        private readonly ServerRemoteCalls _dc;

        // ReSharper disable once NotAccessedField.Local
        private readonly IHubContext<DcCoreHub<T>> _hubContext;
        private readonly ICustomRazorEngine _razorEngine;
        private readonly DateTime _startDateTime = DateTime.UtcNow;
        private int _counter10Min;
        private bool _disposedValue;

        /// <summary>
        ///     Timer
        /// </summary>
        private Timer _timer = null!;

        /// <summary>
        ///     Konstruktor, Services werden injected
        /// </summary>
        /// <param name="clientConnection"></param>
        /// <param name="hubcontext"></param>
        /// <param name="serviceScopeFactory"></param>
        public BackgroundService(IDcConnections clientConnection, IHubContext<DcCoreHub<T>> hubcontext, IServiceScopeFactory serviceScopeFactory)
        {
            _clientConnection = clientConnection;
            _hubContext = hubcontext;

            if (serviceScopeFactory == null!)
            {
                throw new ArgumentException("[BackgroundService] ServiceScopeFactory is NULL!");
            }

            var scope = serviceScopeFactory.CreateScope();
            var s = scope.ServiceProvider.GetService<IServerRemoteCalls>();
            if (scope == null! || s == null!)
            {
                throw new ArgumentException("[BackgroundService] Scope is NULL!");
            }

            _dc = (ServerRemoteCalls) s;
            _dc.SetClientConnection(_clientConnection);

            _razorEngine = _dc.RazorEngine;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Dispose();
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        ///     Führt arbeiten im angegebenen Interval durch
        ///     (Für eventhandler kann async void verwendet werden)
        /// </summary>
        /// <param name="state"></param>
        private async void DoWork(object state)
        {
            try
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await using var db = new Db();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

                if (!await db.TblDevices.AnyAsync().ConfigureAwait(false))
                {
                    Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): No Devices in db.");
                    return;
                }

                var cntDevices = db.TblDevices.Count();

                if (!DcConnected())
                {
                    Logging.Log.LogWarning("[BackgroundService] DC is not connected!");
                }

                var timeDiff = DateTime.UtcNow - _startDateTime;
                Logging.Log.LogInfo($"[BackgroundService] Running time: {timeDiff:g}");
                Logging.Log.LogInfo($"[BackgroundService] Devices in Db: {cntDevices}");
                Logging.Log.LogInfo($"[BackgroundService] Devices online: {_clientConnection.GetClients().Count}");

                if (timeDiff.Hours == 0 && timeDiff.Minutes == 0)
                {
                    #region Idea Archive

                    var archcount = 0;
                    var openIdeas = db.TblIdeas.Where(x => !x.Archived);
                    foreach (var idea in openIdeas)
                    {
                        var archived = idea.CheckArchived();

                        if (archived)
                        {
                            archcount++;
                        }
                    }

                    await db.SaveChangesAsync().ConfigureAwait(true);

                    Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): {archcount} Ideen archiviert.");

                    #endregion

                    if ((timeDiff.Days % 7) == 0)
                    {
                        #region Weekly Report

                        var receiverString = db.TblSettings.FirstOrDefault(x => x.Key == EnumDbSettings.ReportReceivers);
                        var receivers = receiverString?.Value.Split(';');

                        if (receivers != null && receivers.Any())
                        {
                            var report = await WeeklyReportHelper.GetWeeklyReport(DateTime.Today.AddDays(-7), DateTime.Today, db).ConfigureAwait(true);

                            var title = ResWebCommon.WeeklyReportTitle;
                            var html = await _razorEngine
                                .RazorViewToHtmlAsync("Views/EMail/EmailWeeklyReport.cshtml", report).ConfigureAwait(true);

                            var sendRes = await WebConstants.Email.SendHtmlEMail(
                                    WebSettings.Current().SendEMailAs,
                                    receivers.ToList(),
                                    title,
                                    html,
                                    WebSettings.Current().SendEMailAsDisplayName)
                                .ConfigureAwait(true);

                            Logging.Log.LogInfo($"[{nameof(BackgroundService)}]({nameof(DoWork)}): " +
                                                $"Wöchentlicher Report ({sendRes}) versendet");
                        }

                        #endregion
                    }
                }

                var users = await db.TblUsers
                    .Where(u => u.Locked == false)
                    .Include(i => i.TblDevices)
                    .ToListAsync().ConfigureAwait(true);

                _counter10Min++;

                foreach (var user in users)
                {
                    if (user == null!)
                    {
                        continue;
                    }

                    if (_counter10Min >= 10)
                    {
                        if (user.Setting10MinPush)
                        {
                            var tokens = user.TblDevices.Where(d => !string.IsNullOrWhiteSpace(d.DeviceToken)).Select(d => d.DeviceToken).ToList();
                            if (tokens.Count == 0)
                            {
                                continue;
                            }

                            var fail = 0;
                            var success = 0;
                            var result = await PushHelper.Send10MinPush(tokens).ConfigureAwait(true);
                            fail = result.FailureCount;
                            success = result.FailureCount;
                            Logging.Log.LogInfo($"10 Minuten Push: {success} erfolgreich, {fail} nicht erfolgreich!");
                        }
                    }
                }

                if (_counter10Min >= 10)
                {
                    _counter10Min = 0;
                }
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
            }
        }

        /// <summary>
        ///     DC ist bereit
        /// </summary>
        /// <returns></returns>
        private bool DcConnected()
        {
            if (_dc != null! && _dc.ClientConnection != null!)
            {
                return true;
            }

            return false;
        }

        #region Interface Implementations

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Start
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            Logging.Log.LogInfo("[BackgroundService] Timed Hosted Service running.");
            _timer = new Timer(DoWork!, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Stop
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken stoppingToken)
        {
            Logging.Log.LogInfo("[BackgroundService] Timed Hosted Service is stopping.");
            _timer.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        #endregion
    }
}