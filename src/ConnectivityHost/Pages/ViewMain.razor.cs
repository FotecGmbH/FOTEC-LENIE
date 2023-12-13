// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Biss.Core.Logging.Events;
using ConnectivityHost.BaseApp;
using Microsoft.Extensions.Logging;
using Radzen.Blazor;

namespace ConnectivityHost.Pages
{
    /// <summary>
    ///     View Main.
    /// </summary>
    public partial class ViewMain
    {
        private RadzenDataGrid<BissEventsLoggerEventArgs>? _logGrid;

        /// <summary>
        ///     Entspricht VM OnLoaded, wird einmal pro View aufgerufen
        /// </summary>
        /// <returns></returns>
        protected override async Task OnViewLoaded()
        {
            VmProjectBase.LogEntries.CollectionChanged += LogEntriesOnCollectionChanged;

            if (ViewModel != null!)
            {
                ViewModel.DcConnection = DcConnections;


                await base.OnViewLoaded().ConfigureAwait(true);

                await ViewModel.CalculateData().ConfigureAwait(true);
            }
        }

        /// <summary>
        ///     Entspricht VM OnDisappearing, wird einmal pro View aufgerufen
        /// </summary>
        /// <returns></returns>
        protected override Task OnViewDisappearing()
        {
            VmProjectBase.LogEntries.CollectionChanged -= LogEntriesOnCollectionChanged;
            return base.OnViewDisappearing();
        }

        private void LogEntriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvokeAsync(async () =>
            {
                if (_logGrid != null)
                {
                    await _logGrid.Reload().ConfigureAwait(true);
                }
            });
        }

        /// <summary>
        ///     Css Klasse für Logausgabe.
        /// </summary>
        /// <param name="loglevel"></param>
        /// <returns></returns>
        private string GetCssClassForLogLevel(LogLevel loglevel)
        {
            if (loglevel == LogLevel.Information)
            {
                return "prompt-i";
            }

            if (loglevel == LogLevel.Error)
            {
                return "prompt-e";
            }

            if (loglevel == LogLevel.Warning)
            {
                return "prompt-w";
            }

            return "prompt-t";
        }
    }
}