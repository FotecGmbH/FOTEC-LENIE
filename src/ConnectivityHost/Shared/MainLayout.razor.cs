// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.ComponentModel;
using System.Threading.Tasks;
using Biss.Apps.ViewModel;
using Biss.Log.Producer;

namespace ConnectivityHost.Shared
{
    /// <summary>
    ///     Main Layout.
    /// </summary>
    public partial class MainLayout
    {
        #region Properties

        /// <summary>
        ///     Nav Menu für View Binding
        /// </summary>
        private NavMenu? MenuProject
        {
            get => NavMenu as NavMenu;
        }

        #endregion

        /// <inheritdoc />
        public override async Task AttachDetachEvents(bool attach)
        {
            if (ViewModel != null)
            {
                if (attach)
                {
                    ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
                    ViewModel.View.PropertyChanged += ViewOnPropertyChanged;

                    if (ViewModel.View.CmdSaveHeader != null)
                    {
                        ViewModel.View.CmdSaveHeader.PropertyChanged += CmdSaveHeaderOnPropertyChanged;
                    }
                }
                else
                {
                    ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                    ViewModel.View.PropertyChanged -= ViewOnPropertyChanged;

                    if (ViewModel.View.CmdSaveHeader != null)
                    {
                        ViewModel.View.CmdSaveHeader.PropertyChanged -= CmdSaveHeaderOnPropertyChanged;
                    }
                }
            }

            if (MenuProject != null)
            {
                await MenuProject.AttachDetachEvents(attach).ConfigureAwait(true);
            }

            await base.AttachDetachEvents(attach).ConfigureAwait(true);

            Logging.Log.LogInfo($"[{nameof(MainLayout)}]({nameof(AttachDetachEvents)}): {attach}");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        private async void ViewOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VmViewProperties.CmdSaveHeader))
            {
                if (ViewModel?.View.CmdSaveHeader != null)
                {
                    ViewModel.View.CmdSaveHeader.PropertyChanged += CmdSaveHeaderOnPropertyChanged;
                }
            }

            Logging.Log.LogInfo($"[{nameof(MainLayout)}]({nameof(ViewOnPropertyChanged)}): VM.View - {e.PropertyName}");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        private async void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsLoaded))
            {
                return;
            }

            Logging.Log.LogInfo($"[{nameof(MainLayout)}]({nameof(ViewModelOnPropertyChanged)}): VM.PC - {e.PropertyName}");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        ///     Property Changed des Header Commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CmdSaveHeaderOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Logging.Log.LogInfo($"[{nameof(MainLayout)}]({nameof(CmdSaveHeaderOnPropertyChanged)}): SaveHeader - {e.PropertyName}");
            await InvokeStateHasChangedAsync().ConfigureAwait(true);
        }
    }
}