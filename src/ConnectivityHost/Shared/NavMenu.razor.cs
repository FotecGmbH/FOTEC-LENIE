// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Biss.Apps.Collections;
using Biss.Apps.ViewModel;
using Biss.Interfaces;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ConnectivityHost.Shared
{
    /// <summary>
    ///     Nav Menu.
    /// </summary>
    public partial class NavMenu
    {
        /// <summary>
        ///     Attach Property / CollectionChanged
        /// </summary>
        /// <param name="attach"></param>
        /// <returns></returns>
        public async Task AttachDetachEvents(bool attach)
        {
            if (attach)
            {
                VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.CollectionChanged += CmdAllMenuCommandsOnCollectionChanged;
                foreach (var command in VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands)
                {
                    command.PropertyChanged += CommandOnPropertyChanged;
                    command.Selected += CommandOnSelected;
                }
            }
            else
            {
                VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.CollectionChanged -= CmdAllMenuCommandsOnCollectionChanged;
                foreach (var command in VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands)
                {
                    command.PropertyChanged -= CommandOnPropertyChanged;
                    command.Selected -= CommandOnSelected;
                }
            }

            Logging.Log.LogInfo($"[{nameof(NavMenu)}]({nameof(AttachDetachEvents)}): {attach}");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        private async void CommandOnSelected(object? sender, BissSelectableEventArgs e)
        {
            Logging.Log.LogInfo($"[{nameof(NavMenu)}]({nameof(CommandOnSelected)}): Menu SC");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        private async void CommandOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Logging.Log.LogInfo($"[{nameof(NavMenu)}]({nameof(CommandOnPropertyChanged)}): Menu PC - {e.PropertyName}");

            if (e.PropertyName == nameof(VmCommandSelectable.IsSelected) ||
                e.PropertyName == nameof(VmCommandSelectable.IsVisible))
            {
                await InvokeStateHasChangedAsync().ConfigureAwait(true);
            }
        }

        private async void CmdAllMenuCommandsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Logging.Log.LogInfo($"[{nameof(NavMenu)}]({nameof(CmdAllMenuCommandsOnCollectionChanged)}): Menu CC");

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is VmCommandSelectable command)
                    {
                        command.PropertyChanged -= CommandOnPropertyChanged;
                        command.Selected -= CommandOnSelected;
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (sender is BxObservableCollection<VmCommandSelectable> cmds)
                {
                    foreach (var command in cmds.AllItems)
                    {
                        command.PropertyChanged -= CommandOnPropertyChanged;
                        command.Selected -= CommandOnSelected;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is VmCommandSelectable command)
                    {
                        command.PropertyChanged += CommandOnPropertyChanged;
                        command.Selected += CommandOnSelected;
                    }
                }
            }

            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///     Navigation
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="args">args</param>
        private async Task Navigate(VmCommandSelectable cmd, MouseEventArgs args)
        {
            VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem = cmd;
            await JsRuntime.InvokeVoidAsync("collapseNavbar").ConfigureAwait(true);
        }

        /// <summary>
        ///     Men�eintrag ist aktiv oder inaktiv
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private string IsActive(VmCommandSelectable cmd)
        {
            if (VmProjectBase.GetVmBaseStatic == null! || VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem == null)
            {
                return "";
            }

            if (cmd == VmProjectBase.GetVmBaseStatic.CmdAllMenuCommands.SelectedItem)
            {
                return "active";
            }

            return "";
        }
    }
}