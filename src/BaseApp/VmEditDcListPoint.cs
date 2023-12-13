// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Biss.AppConfiguration;
using Biss.Apps.Connectivity.Dc;
using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Biss.Dc.Core;
using Biss.Interfaces;
using Biss.Log.Producer;
using Biss.ObjectEx;
using Exchange;
using Exchange.Enum;
using Exchange.Resources;
using Microsoft.Extensions.Logging;

namespace BaseApp
{
    /// <summary>
    ///     <para>Bearbeiten eines DC Listenpunkts</para>
    ///     Klasse VmEditDcListPoint. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    /// <typeparam name="T2">Der Typ eines DcListDataPoint</typeparam>
    public abstract class VmEditDcListPoint<T2> : VmProjectBase
        where T2 : IBissModel
    {
        private readonly bool _autoHideCmdSaveHeader;
        private readonly bool _debugSaveCheck;
        private bool _allowServerOverrideDataOriginal;

        /// <summary>
        ///     Soll automatisch im DC gespeichert werden
        /// </summary>
        protected bool _autoStoreDc;

        private bool _backIsPressed;
        private VmCommand? _commandSaveBackup;
        private List<INotifyPropertyChanged>? _iNotifyPropertyChangedList;
        private bool _isNavigatedToNavToViewWithResult;
        private bool _isNewElement;
        private bool _lastCheckSaveCommandResult;

        /// <summary>
        ///     Entries auf der Seite
        /// </summary>
        protected List<VmEntry> _pageVmEntries = new List<VmEntry>();

        /// <summary>
        ///     Bearbeiten eines DC Listenpunkts
        /// </summary>
        /// <param name="pageTitle"></param>
        /// <param name="args"></param>
        /// <param name="subTitle"></param>
        /// <param name="autoStoreDc">Beim speichern (versuchen) automatisch die Daten am Connectivity-Host zu sichern</param>
        /// <param name="autoHideCmdSaveHeader">Sichtbarkeit des Speichern Button</param>
        /// <param name="debugSaveCheck">Debuginfos zum Sichern Button</param>
        protected VmEditDcListPoint(string pageTitle, object? args = null, string subTitle = "", bool autoStoreDc = true, bool autoHideCmdSaveHeader = false, bool debugSaveCheck = false) : base(pageTitle, args, subTitle)
        {
            SetViewProperties(true);

            _autoStoreDc = autoStoreDc;
            _autoHideCmdSaveHeader = autoHideCmdSaveHeader;
            _debugSaveCheck = debugSaveCheck;
            ViewResult = EnumVmEditResult.NotModified;

            Appeared += OnAppeared;
            Loaded += OnLoaded;
            Disappeared += OnDisappeared;
        }

        #region Properties

        /// <summary>
        ///     Listendatenpunkt
        /// </summary>
        public DcListDataPoint<T2> DcListDataPoint { get; private set; } = null!;

        /// <summary>
        ///     Daten zum Bearbeiten
        /// </summary>
        public T2 Data { get; set; } = default!;

        /// <summary>
        ///     Back überschreiben
        /// </summary>
        public new VmCommand GCmdBack { get; set; } = null!;

        /// <summary>
        ///     Wird aufgerufen bevor gesichert wird. Bei False wird nicht gesichert.
        /// </summary>
        public Func<Task<bool>>? CheckBeforeSaving { get; set; } = null!;

        /// <summary>
        ///     Property-Namen in dieser Liste lösen _kein_ View.CmdSaveHeader?.CanExecute(); aus
        /// </summary>
        public List<string> AnyINotifyPropertyChangedFilter { get; } = new List<string>();


        /// <summary>
        ///     NavToViewWithResult Flag
        ///     Muss in der abgeleiteten Klasse gestzt werden bevor navigiert wird (danach wieder zurücksetzen)
        /// </summary>
        public bool IsNavigatedToNavToViewWithResult
        {
            get => _isNavigatedToNavToViewWithResult;
            set
            {
                if (value)
                {
                    _commandSaveBackup = View.CmdSaveHeader;
                }
                else
                {
                    View.CmdSaveHeader = _commandSaveBackup;
                    View.CmdSaveHeader?.CanExecute();
                }

                _isNavigatedToNavToViewWithResult = value;
            }
        }

        /// <summary>
        ///     Übersschreibbar - kann Speichern gedrückt werden, bei Null Automatisch berechnen
        /// </summary>
        public bool? CheckSaveOverride { get; set; }

        #endregion

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args is DcListDataPoint<T2> p)
            {
                DcListDataPoint = p;
                Data = DcListDataPoint.Data;

                if (DcListDataPoint.State != EnumDcListElementState.New)
                {
                    CheckSaveBehavior = new CheckSaveDcBehavior<T2>(DcListDataPoint);
                }
                else
                {
                    _isNewElement = true;
                }

                _allowServerOverrideDataOriginal = DcListDataPoint.AllowServerOverrideData;
                DcListDataPoint.AllowServerOverrideData = false;
            }
            else
            {
                Logging.Log.LogError($"[VmEditDcListPoint]({nameof(OnActivated)}): Invalid Type!");
                throw new ArgumentException($"[VmEditDcListPoint]({nameof(OnActivated)}): Invalid Type!");
            }

            return base.OnActivated(args);
        }

        /// <summary>
        ///     Commands Initialisieren (aufruf im Kostruktor von VmBase)
        /// </summary>
        protected override void InitializeCommands()
        {
            View.CmdSaveHeader = new VmCommand(ResCommon.CmdSave, async () =>
            {
                View.BusySet(ResCommon.CmdSave);

                // Solange in Speichern Methode -> Speichern nicht klickbar machen
                // disabled auch die ViewResult Berechnung
                var cso = CheckSaveOverride;
                CheckSaveOverride = false;

                if (_pageVmEntries.Any(p => !p.DataValid))
                {
                    Logging.Log.LogWarning($"[VmEditDcListPoint]({nameof(InitializeCommands)}): {string.Join(",", _pageVmEntries.Where(x => !x.DataValid).Select(x => x.Title))}");
                    CheckSaveOverride = cso;
                    View.BusyClear();
                    await MsgBox.Show(ResCommon.MsgCannotSave, ResCommon.MsgTitleValidationError).ConfigureAwait(false);
                    return;
                }

                if (DcListDataPoint.State != EnumDcListElementState.New)
                {
                    DcListDataPoint.State = EnumDcListElementState.Modified;
                }

                if (_autoStoreDc)
                {
                    if (CheckBeforeSaving != null)
                    {
                        var check = await CheckBeforeSaving().ConfigureAwait(true);
                        if (!check)
                        {
                            CheckSaveOverride = cso;
                            Logging.Log.LogInfo($"[VmEditDcListPoint]({nameof(InitializeCommands)}): StoreData canceled by BeforeSave!");
                            View.BusyClear();
                            return;
                        }
                    }

                    var r = await DcListDataPoint.StoreData(true).ConfigureAwait(true);
                    if (!r.DataOk)
                    {
                        Logging.Log.LogWarning($"[VmEditDcListPoint]({nameof(InitializeCommands)}): {r.ErrorType}-{r.ServerExceptionText}");

                        var msg = ResCommon.MsgCannotSave;
                        if (Constants.AppConfiguration.CurrentBuildType == EnumCurrentBuildType.Developer)
                        {
                            msg += $"\r\n{r.ErrorType}\r\n{r.ServerExceptionText}";
                        }

                        CheckSaveOverride = cso;
                        View.BusyClear();
                        await MsgBox.Show(msg, ResCommon.MsgTitleServerError).ConfigureAwait(false);
                        return;
                    }

                    ViewResult = EnumVmEditResult.ModifiedAndStored;
                }
                else
                {
                    ViewResult = EnumVmEditResult.Modified;
                }

                CheckSaveBehavior = null;
                await Nav.Back().ConfigureAwait(true);
                CheckSaveOverride = cso;
                View.BusyClear();
            }, CanExecuteSaveCommand, glyph: Glyphs.Floppy_disk);
        }

        /// <summary>
        ///     OnAppeared
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAppeared(object sender, EventArgs e)
        {
            GCmdBack = new VmCommand(base.GCmdBack.DisplayName, () =>
            {
                View.BusySet(string.Empty);

                _iNotifyPropertyChangedList?.ForEach(i => i.PropertyChanged -= OnAnyINotifyPropertyChangedInView);
                _backIsPressed = true;
                OnBackButtonPressed(true);
                _iNotifyPropertyChangedList?.ForEach(i => i.PropertyChanged += OnAnyINotifyPropertyChangedInView);

                View.BusyClear();
            }, glyph: base.GCmdBack.Glyph);
        }

        /// <summary>
        ///     OnDisappeared
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisappeared(object sender, EventArgs e)
        {
            if (IsNavigatedToNavToViewWithResult)
            {
                return;
            }

            Disappeared -= OnDisappeared;
            _iNotifyPropertyChangedList?.ForEach(i => i.PropertyChanged -= OnAnyINotifyPropertyChangedInView);

            if (CheckSaveBehavior == null)
            {
                if (!_isNewElement)
                {
                    DcListDataPoint.EndEdit();
                }
            }
            else
            {
                DcListDataPoint.EndEdit(true);
            }

            if (View.CmdSaveHeader != null)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = "Visible false -> Disap";
                }

                View.CmdSaveHeader.IsVisible = false;
            }

            //if (_debugSaveCheck)
            //    PageTitle = "Set Null -> Disap";
            //View.CmdSaveHeader = null;

            DcListDataPoint.AllowServerOverrideData = _allowServerOverrideDataOriginal;
        }

        /// <summary>
        ///     OnLoaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, EventArgs e)
        {
            if (_iNotifyPropertyChangedList == null)
            {
                _iNotifyPropertyChangedList = Data.GetAllInstancesWithType<INotifyPropertyChanged>();
            }

            _iNotifyPropertyChangedList.ForEach(i => i.PropertyChanged += OnAnyINotifyPropertyChangedInView);
            _pageVmEntries = this.GetAllInstancesWithType<VmEntry>();
            //Loaded -= OnLoaded;
            View.CmdSaveHeader?.CanExecute();
        }

        /// <summary>
        ///     INotify Property Changed für die aktuelle View richtig behandeln
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAnyINotifyPropertyChangedInView(object sender, PropertyChangedEventArgs e)
        {
            if (AnyINotifyPropertyChangedFilter.Contains(e.PropertyName))
            {
                Logging.Log.LogTrace($"[VmEditDcListPoint]({nameof(OnAnyINotifyPropertyChangedInView)}): {e.PropertyName} - depraved");
                return;
            }

            Logging.Log.LogTrace($"[VmEditDcListPoint]({nameof(OnAnyINotifyPropertyChangedInView)}): {e.PropertyName}");

            if (_backIsPressed)
            {
                _backIsPressed = false;
            }

            View.CmdSaveHeader?.CanExecute();
        }

        /// <summary>
        ///     Sollte gesichert werden (können)?
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSaveCommand()
        {
            if (CheckSaveOverride.HasValue)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = $"CanExecute - Override: {CheckSaveOverride.Value}";
                }

                return CheckSaveOverride.Value;
            }

            if (_isNewElement)
            {
                if (View.CmdSaveHeader != null && _autoHideCmdSaveHeader)
                {
                    if (_debugSaveCheck)
                    {
                        PageTitle = "CanExecute - new Element visi true";
                    }

                    View.CmdSaveHeader.IsVisible = true;
                }

                if (_debugSaveCheck)
                {
                    PageTitle = "CanExecute - new Element true";
                }

                return true;
            }

            if (!IsLoaded)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = "CanExecute - View Loaded = False";
                }

                _lastCheckSaveCommandResult = false;
                return false;
            }

            if (_backIsPressed)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = $"CanExecute - Back Pressed: {_lastCheckSaveCommandResult}";
                }

                return _lastCheckSaveCommandResult;
            }

            if (CheckSaveBehavior is null)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = "CanExecute - Check Save is Null -> true";
                }

                _lastCheckSaveCommandResult = true;
                return true;
            }

            var r = CheckSaveBehavior!.Check();

            if (View.CmdSaveHeader != null && _autoHideCmdSaveHeader)
            {
                if (_debugSaveCheck)
                {
                    PageTitle = $"CanExecute - CheckSavebehavior visi: {r}";
                }

                View.CmdSaveHeader.IsVisible = r;
            }

            _lastCheckSaveCommandResult = r;

            ViewResult = r ? EnumVmEditResult.Modified : EnumVmEditResult.NotModified;

            if (_debugSaveCheck)
            {
                PageTitle = $"CanExecute - CheckSavebehavior: {r}";
            }

            return r;
        }
    }
}