// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using PropertyChanged;
using Xamarin.Forms;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewShowReports
    {
        private StackOrientation _stackOrientation = StackOrientation.Horizontal;

        public ViewShowReports() : this(null)
        {
        }

        public ViewShowReports(object? args = null) : base(args)
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();
            OnSizeChanged(this, EventArgs.Empty);
        }

        #region Properties

        /// <summary>
        ///     Ausrichtung von Username und Begründung
        /// </summary>
        public StackOrientation StackOrientation
        {
            get => _stackOrientation;
            set
            {
                _stackOrientation = value;

                if (_stackOrientation == StackOrientation.Horizontal)
                {
                    ColSpanUserLabel = 1;
                    RowSpanUserLabel = 2;

                    RowReasonLabel = 0;
                    RowSpanReasonLabel = 2;

                    ColReasonLabel = 1;
                    ColSpanReasonLabel = 1;
                }
                else if (_stackOrientation == StackOrientation.Vertical)
                {
                    ColSpanUserLabel = 2;
                    RowSpanUserLabel = 1;

                    RowReasonLabel = 1;
                    RowSpanReasonLabel = 1;

                    ColReasonLabel = 0;
                    ColSpanReasonLabel = 2;
                }
            }
        }

        /// <summary>
        ///     User Label Column Span
        /// </summary>
        public int ColSpanUserLabel { get; set; }

        /// <summary>
        ///     User Label Row Span
        /// </summary>
        public int RowSpanUserLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowReasonLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColReasonLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowSpanReasonLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColSpanReasonLabel { get; set; }

        #endregion

        [SuppressPropertyChangedWarnings]
        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width < 700 && StackOrientation != StackOrientation.Vertical)
            {
                StackOrientation = StackOrientation.Vertical;
            }
            else if (Width >= 700 && StackOrientation != StackOrientation.Horizontal)
            {
                StackOrientation = StackOrientation.Horizontal;
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member