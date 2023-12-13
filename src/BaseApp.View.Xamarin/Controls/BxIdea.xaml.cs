// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Biss.Apps.ViewModel;
using Biss.Apps.XF.Controls;
using Biss.Dc.Client;
using Biss.Log.Producer;
using Exchange.Model.Idea;
using Microsoft.Extensions.Logging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     Idee Item von DC für UI
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BxIdea
    {
        /// <summary>
        ///     Idee Item von DC für UI
        /// </summary>
        public static readonly BindableProperty DcIdeaProperty = BindableProperty.Create(
            nameof(DcIdea),
            typeof(DcListDataPoint<ExIdea>),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime, propertyChanged: DcIdeaChanged
        );


        /// <summary>
        ///     Kalender öffner
        /// </summary>
        public static readonly BindableProperty CmdCalendarProperty = BindableProperty.Create(
            nameof(CmdCalendar),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Liken
        /// </summary>
        public static readonly BindableProperty CmdLikeProperty = BindableProperty.Create(
            nameof(CmdLike),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Chatten
        /// </summary>
        public static readonly BindableProperty CmdChatUserProperty = BindableProperty.Create(
            nameof(CmdChatUser),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Chatten
        /// </summary>
        public static readonly BindableProperty CmdChatIdeaProperty = BindableProperty.Create(
            nameof(CmdChatIdea),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Karte
        /// </summary>
        public static readonly BindableProperty CmdMapProperty = BindableProperty.Create(
            nameof(CmdMap),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Editieren
        /// </summary>
        public static readonly BindableProperty CmdEditProperty = BindableProperty.Create(
            nameof(CmdEdit),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Löschen
        /// </summary>
        public static readonly BindableProperty CmdDeleteProperty = BindableProperty.Create(
            nameof(CmdDelete),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Anzeigen
        /// </summary>
        public static readonly BindableProperty CmdShowProperty = BindableProperty.Create(
            nameof(CmdShow),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Reporten
        /// </summary>
        public static readonly BindableProperty CmdAddReportProperty = BindableProperty.Create(
            nameof(CmdAddReport),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Command für Reporten
        /// </summary>
        public static readonly BindableProperty CmdShowReportsProperty = BindableProperty.Create(
            nameof(CmdShowReports),
            typeof(VmCommand),
            typeof(BxIdea),
            defaultBindingMode: BindingMode.OneTime
        );

        /// <summary>
        ///     Idee
        /// </summary>
        public BxIdea()
        {
            InitializeComponent();
            ContentGrid.BindingContext = this;
        }

        #region Properties

        /// <summary>
        ///     Kalender öffner
        /// </summary>
        public VmCommand CmdCalendar
        {
            get => (VmCommand) GetValue(CmdCalendarProperty);
            set => SetValue(CmdCalendarProperty, value);
        }

        /// <summary>
        ///     Idee Item von DC
        /// </summary>
        public DcListDataPoint<ExIdea> DcIdea
        {
            get => (DcListDataPoint<ExIdea>) GetValue(DcIdeaProperty);
            set => SetValue(DcIdeaProperty, value);
        }

        /// <summary>
        ///     Command für Liken
        /// </summary>
        public VmCommand CmdLike
        {
            get => (VmCommand) GetValue(CmdLikeProperty);
            set => SetValue(CmdLikeProperty, value);
        }

        /// <summary>
        ///     Command für Chatten
        /// </summary>
        public VmCommand CmdChatUser
        {
            get => (VmCommand) GetValue(CmdChatUserProperty);
            set => SetValue(CmdChatUserProperty, value);
        }

        /// <summary>
        ///     Command für Chatten
        /// </summary>
        public VmCommand CmdChatIdea
        {
            get => (VmCommand) GetValue(CmdChatIdeaProperty);
            set => SetValue(CmdChatIdeaProperty, value);
        }

        /// <summary>
        ///     Command für Karte
        /// </summary>
        public VmCommand CmdMap
        {
            get => (VmCommand) GetValue(CmdMapProperty);
            set => SetValue(CmdMapProperty, value);
        }

        /// <summary>
        ///     Command für Editieren
        /// </summary>
        public VmCommand CmdEdit
        {
            get => (VmCommand) GetValue(CmdEditProperty);
            set => SetValue(CmdEditProperty, value);
        }

        /// <summary>
        ///     Command für Löschen
        /// </summary>
        public VmCommand CmdDelete
        {
            get => (VmCommand) GetValue(CmdDeleteProperty);
            set => SetValue(CmdDeleteProperty, value);
        }

        /// <summary>
        ///     Command für Anzeigen
        /// </summary>
        public VmCommand CmdShow
        {
            get => (VmCommand) GetValue(CmdShowProperty);
            set => SetValue(CmdShowProperty, value);
        }

        /// <summary>
        ///     Command zum Hinzufügen eines Reports
        /// </summary>
        public VmCommand CmdAddReport
        {
            get => (VmCommand) GetValue(CmdAddReportProperty);
            set => SetValue(CmdAddReportProperty, value);
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

        /// <summary>Invalidates the current layout.</summary>
        /// <remarks>Calling this method will invalidate the measure and triggers a new layout cycle.</remarks>
        protected override void InvalidateLayout()
        {
            if (DcIdea == null! || DcIdea.Data == null!)
            {
                return;
            }


            Logging.Log.LogTrace($"[{nameof(BxIdea)}]({nameof(InvalidateLayout)}): Idea Layout");
            base.InvalidateLayout();
        }

        private static void DcIdeaChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (oldvalue == null! && (newvalue is DcListDataPoint<ExIdea>) && (bindable is BxIdea ideacontrol))
            {
                ideacontrol.InitializeControl();
            }
        }

        private void InitializeControl()
        {
            if (DcIdea != null!)
            {
                if (DcIdea.Data.CanSeeReports)
                {
                    var see = new SeeReports();
                    see.SetValue(SeeReports.DcIdeaProperty, DcIdea);
                    see.SetValue(SeeReports.CmdShowReportsProperty, CmdShowReports);
                    StatusGrid.Children.Add(see);
                    Grid.SetColumn(see, 4);
                    Grid.SetRow(see, 0);
                    Grid.SetRowSpan(see, 4);
                }

                if (DcIdea.Data.IsHelping)
                {
                    var supportGrid = new Grid {HorizontalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(0, 12), RowDefinitions = new RowDefinitionCollection {new RowDefinition {Height = GridLength.Auto}, new RowDefinition {Height = GridLength.Auto}}};
                    var supportLabel = new Label {Text = "Du unterstützt diese Idee!", FontAttributes = FontAttributes.Bold};
                    Application.Current.Resources.TryGetValue("ColorAccentLight", out var light);
                    Application.Current.Resources.TryGetValue("ColorAccent", out var dark);
                    supportLabel.SetAppThemeColor(Label.TextColorProperty, (Color) light, (Color) dark);
                    supportGrid.Children.Add(supportLabel);
                    Grid.SetRow(supportLabel, 0);
                    ContentGrid.Children.Add(supportGrid);
                    Grid.SetRow(supportGrid, 7);
                }

                InitializeButtons();
            }
        }

        private void InitializeButtons()
        {
            if (DcIdea.Data.CanEdit)
            {
                var btnEdit = new BxGlyphButton
                              {
                                  Command = CmdEdit,
                                  CommandParameter = DcIdea,
                                  Glyph = CmdEdit.Glyph
                              };

                var btnDelete = new BxGlyphButton
                                {
                                    Command = CmdDelete,
                                    CommandParameter = DcIdea,
                                    Glyph = CmdDelete.Glyph
                                };

                ButtonStack.Children.Add(btnEdit);
                ButtonStack.Children.Add(btnDelete);
            }

            var btnChat = new BxGlyphButton
                          {
                              Command = CmdChatIdea,
                              CommandParameter = DcIdea,
                              Glyph = CmdChatIdea.Glyph
                          };
            ButtonStack.Children.Add(btnChat);

            if (!DcIdea.Data.IsMine)
            {
                var btnReport = new BxGlyphButton
                                {
                                    Command = CmdAddReport,
                                    CommandParameter = DcIdea,
                                    Glyph = CmdAddReport.Glyph
                                };
                ButtonStack.Children.Add(btnReport);
            }
        }
    }
}