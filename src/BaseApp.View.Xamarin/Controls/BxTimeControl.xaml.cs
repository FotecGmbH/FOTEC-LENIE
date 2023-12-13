// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Dc.Core;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     Zeitcontrol
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BxTimeControl
    {
        /// <summary>
        ///     Zeit von
        /// </summary>
        public static readonly BindableProperty FromProperty = BindableProperty.Create(
            nameof(From),
            typeof(DateTime),
            typeof(BxTimeControl),
            DateTime.MinValue,
            BindingMode.TwoWay,
            propertyChanged: FromPropertyChanged
        );

        /// <summary>
        ///     Zeit bis
        /// </summary>
        public static readonly BindableProperty ToProperty = BindableProperty.Create(
            nameof(To),
            typeof(DateTime),
            typeof(BxTimeControl),
            DateTime.MaxValue,
            BindingMode.TwoWay,
            propertyChanged: ToPropertyChanged
        );

        /// <summary>
        ///     Kann das Datum geändert werden?
        /// </summary>
        public static readonly BindableProperty CanEditProperty = BindableProperty.Create(
            nameof(CanEdit),
            typeof(bool),
            typeof(BxTimeControl),
            true,
            propertyChanged: EditPropertyChanged
        );

        /// <summary>
        ///     Labels mit "Von" und "bis" anzeigen
        /// </summary>
        public static readonly BindableProperty ShowLabelsProperty = BindableProperty.Create(
            nameof(ShowLabels),
            typeof(bool),
            typeof(BxTimeControl),
            true
        );

        /// <summary>
        ///     Datum von
        /// </summary>
        public static readonly BindableProperty FromDateProperty = BindableProperty.Create(
            nameof(FromDate),
            typeof(DateTime),
            typeof(BxTimeControl),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: FromDatePropertyChanged
        );

        /// <summary>
        ///     Datum bis
        /// </summary>
        public static readonly BindableProperty ToDateProperty = BindableProperty.Create(
            nameof(ToDate),
            typeof(DateTime),
            typeof(BxTimeControl),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: ToDatePropertyChanged
        );

        /// <summary>
        ///     Zeit von
        /// </summary>
        public static readonly BindableProperty FromTimeProperty = BindableProperty.Create(
            nameof(FromTime),
            typeof(TimeSpan),
            typeof(BxTimeControl),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: FromTimePropertyChanged
        );

        /// <summary>
        ///     Zeit bis
        /// </summary>
        public static readonly BindableProperty ToTimeProperty = BindableProperty.Create(
            nameof(ToTime),
            typeof(TimeSpan),
            typeof(BxTimeControl),
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: ToTimePropertyChanged
        );

        /// <summary>
        ///     Ist einzelner Tag
        /// </summary>
        public static readonly BindableProperty IsSingleDayProperty = BindableProperty.Create(
            nameof(IsSingleDay),
            typeof(bool),
            typeof(BxTimeControl),
            false,
            BindingMode.TwoWay
        );

        /// <summary>
        ///     IdeaState
        /// </summary>
        public static readonly BindableProperty IdeaStateProperty = BindableProperty.Create(
            nameof(IdeaState),
            typeof(EnumDcListElementState?),
            typeof(BxTimeControl)
        );

        private bool _fromChanging;
        private bool _toChanging;

        /// <summary>
        ///     Zeitcontrol
        /// </summary>
        public BxTimeControl()
        {
            InitializeComponent();
            ContentGrid.BindingContext = this;
        }

        #region Properties

        /// <summary>
        ///     IdeaState
        /// </summary>
        public EnumDcListElementState? IdeaState
        {
            get => (EnumDcListElementState?) GetValue(IdeaStateProperty);
            set => SetValue(IdeaStateProperty, value);
        }

        /// <summary>
        ///     Labels mit "Von" und "bis" anzeigen
        /// </summary>
        public bool ShowLabels
        {
            get => (bool) GetValue(ShowLabelsProperty);
            set => SetValue(ShowLabelsProperty, value);
        }

        /// <summary>
        ///     Kann das Datum geändert werden?
        /// </summary>
        public bool CanEdit
        {
            get => (bool) GetValue(CanEditProperty);
            set => SetValue(CanEditProperty, value);
        }

        /// <summary>
        ///     Zeit von
        /// </summary>
        public DateTime From
        {
            get => (DateTime) GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        /// <summary>
        ///     Zeit bis
        /// </summary>
        public DateTime To
        {
            get => (DateTime) GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        /// <summary>
        ///     Datum von
        /// </summary>
        public DateTime FromDate
        {
            get => (DateTime) GetValue(FromDateProperty);
            set => SetValue(FromDateProperty, value);
        }

        /// <summary>
        ///     Datum bis
        /// </summary>
        public DateTime ToDate
        {
            get => (DateTime) GetValue(ToDateProperty);
            set => SetValue(ToDateProperty, value);
        }

        /// <summary>
        ///     Zeit von
        /// </summary>
        public TimeSpan FromTime
        {
            get => (TimeSpan) GetValue(FromTimeProperty);
            set => SetValue(FromTimeProperty, value);
        }

        /// <summary>
        ///     Zeit bis
        /// </summary>
        public TimeSpan ToTime
        {
            get => (TimeSpan) GetValue(ToTimeProperty);
            set => SetValue(ToTimeProperty, value);
        }

        /// <summary>
        ///     Ist einzelner Tag
        /// </summary>
        public bool IsSingleDay
        {
            get => (bool) GetValue(IsSingleDayProperty);
            set => SetValue(IsSingleDayProperty, value);
        }

        #endregion

        private static void EditPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                control.ReCalcIsSingleDay();
            }
        }

        private static void ToPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (newvalue is DateTime newDate)
                {
                    control._toChanging = true;

                    control.ToDate = newDate.Date;
                    control.ToTime = newDate.TimeOfDay;

                    control._toChanging = false;
                }

                control.ReCalcIsSingleDay();
            }
        }

        private static void FromPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (newvalue is DateTime newDate)
                {
                    control._fromChanging = true;

                    var shift = newDate.Subtract(control.From);

                    control.FromDate = newDate.Date;
                    control.FromTime = newDate.TimeOfDay;

                    if (control.IdeaState == EnumDcListElementState.New)
                    {
                        control.To = control.To.Add(shift);
                    }

                    control._fromChanging = false;
                }

                control.ReCalcIsSingleDay();
            }
        }

        private static void FromDatePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (!control._fromChanging)
                {
                    control.From = control.FromDate.Add(control.FromTime);
                }

                control.ReCalcIsSingleDay();
            }
        }

        private static void ToDatePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (!control._toChanging)
                {
                    control.To = control.ToDate.Add(control.ToTime);
                }

                control.ReCalcIsSingleDay();
            }
        }

        private static void FromTimePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (!control._fromChanging)
                {
                    control.From = control.FromDate.Add(control.FromTime);
                }

                control.ReCalcIsSingleDay();
            }
        }

        private static void ToTimePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is BxTimeControl control)
            {
                if (!control._toChanging)
                {
                    control.To = control.ToDate.Add(control.ToTime);
                }

                control.ReCalcIsSingleDay();
            }
        }

        private void ReCalcIsSingleDay()
        {
            if (CanEdit)
            {
                IsSingleDay = false;
            }
            else
            {
                IsSingleDay = FromDate == ToDate;
            }
        }
    }
}