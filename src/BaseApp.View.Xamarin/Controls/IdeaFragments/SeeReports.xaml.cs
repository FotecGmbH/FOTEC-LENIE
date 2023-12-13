// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Biss.Apps.ViewModel;
using Biss.Dc.Client;
using Exchange.Model.Idea;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     See Reports Control.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SeeReports
    {
        /// <summary>
        ///     Idee Item von DC für UI
        /// </summary>
        public static readonly BindableProperty DcIdeaProperty = BindableProperty.Create(
            nameof(DcIdea),
            typeof(DcListDataPoint<ExIdea>),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.TwoWay
        );

        /// <summary>
        ///     Command für Reporten
        /// </summary>
        public static readonly BindableProperty CmdShowReportsProperty = BindableProperty.Create(
            nameof(CmdShowReports),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.TwoWay
        );

        /// <summary>
        ///     See Reports.
        /// </summary>
        public SeeReports()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        ///     Idee Item von DC
        /// </summary>
        public DcListDataPoint<ExIdea> DcIdea
        {
            get => (DcListDataPoint<ExIdea>) GetValue(DcIdeaProperty);
            set => SetValue(DcIdeaProperty, value);
        }

        /// <summary>
        ///     Command zum Anzeigen der Reports
        /// </summary>
        public VmCommand CmdShowReports
        {
            get => (VmCommand) GetValue(CmdShowReportsProperty);
            set => SetValue(CmdShowReportsProperty, value);
        }

        #endregion
    }
}