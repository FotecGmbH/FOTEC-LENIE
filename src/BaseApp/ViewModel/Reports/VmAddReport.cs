// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using BaseApp.Helper;
using Biss.Apps.Attributes;
using Biss.Apps.Enum;
using Biss.Apps.Interfaces;
using Biss.Apps.ViewModel;
using Exchange.Model.Report;
using Exchange.Resources;

namespace BaseApp.ViewModel.Reports
{
    /// <summary>
    ///     <para>VmAddReport</para>
    ///     Klasse VmAddReport. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [ViewName("ViewAddReport")]
    public class VmAddReport : VmEditDcListPoint<ExReport>
    {
        /// <summary>
        ///     Design Instanz für XAML d:DataContext="{x:Static viewmodels:VmAddReport.DesignInstance}"
        /// </summary>
        public static VmAddReport DesignInstance = new VmAddReport();

        /// <summary>
        ///     VmAddReport
        /// </summary>
        public VmAddReport() : base(ResViewAddReport.LblTitle, subTitle: ResViewAddReport.LblSubTitle)
        {
            SetViewProperties(true);
            View.ShowUser = false;
        }

        #region Properties

        /// <summary>
        ///     Entry Grund für die Meldung
        /// </summary>
        public VmEntry EntryReason { get; set; } = null!;

        #endregion

        #region Overrides

        /// <summary>
        ///     OnAppearing (1) für View geladen noch nicht sichtbar
        ///     Wird Mal wenn View wieder sichtbar ausgeführt
        ///     Unbedingt beim überschreiben auch base. aufrufen!
        /// </summary>
        public override Task OnAppearing(IView view)
        {
            if (View.CmdSaveHeader != null)
            {
                View.CmdSaveHeader.DisplayName = ResViewAddReport.CmdSave;
                View.CmdSaveHeader.Glyph = Glyphs.Send_email;
            }

            return base.OnAppearing(view);
        }

        /// <summary>
        ///     OnActivated (2) für View geladen noch nicht sichtbar
        ///     Nur einmal
        /// </summary>
        public override Task OnActivated(object? args = null)
        {
            var r = base.OnActivated(args);

            EntryReason = new VmEntry(EnumVmEntryBehavior.StopTyping,
                ResViewAddReport.EntryReasonTitle,
                ResViewAddReport.EntryReasonPlaceholder,
                Data,
                nameof(Data.Reason),
                VmEntryValidators.ValidateFuncStringEmpty,
                showTitle: true,
                showMaxChar: false
            );

            return r;
        }

        #endregion
    }
}