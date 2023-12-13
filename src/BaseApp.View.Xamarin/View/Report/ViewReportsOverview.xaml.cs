// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
using System;
using PropertyChanged;

namespace BaseApp.View.Xamarin.View
{
    public partial class ViewReportsOverview
    {
        private BissScreenSize _screenSize = BissScreenSize.Large;

        public ViewReportsOverview() : this(null)
        {
        }

        public ViewReportsOverview(object? args = null) : base(args)
        {
            SizeChanged += OnSizeChanged;
            InitializeComponent();
            OnSizeChanged(this, EventArgs.Empty);
        }

        #region Properties

        /// <summary>
        ///     User Label Column Span
        /// </summary>
        public int ColSpanUserLabel { get; set; } = 1;

        /// <summary>
        ///     User Label Row Span
        /// </summary>
        public int RowSpanUserLabel { get; set; } = 1;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowIdeaLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColIdeaLabel { get; set; } = 2;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowSpanIdeaLabel { get; set; } = 1;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColSpanIdeaLabel { get; set; } = 1;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowReasonLabel { get; set; }

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColReasonLabel { get; set; } = 3;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int RowSpanReasonLabel { get; set; } = 1;

        /// <summary>
        ///     Reason Label
        /// </summary>
        public int ColSpanReasonLabel { get; set; } = 1;

        #endregion

        [SuppressPropertyChangedWarnings]
        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (Width < 650 && _screenSize != BissScreenSize.Small)
            {
                ColSpanUserLabel = 3;
                RowSpanUserLabel = 1;

                RowIdeaLabel = 1;
                RowSpanIdeaLabel = 1;

                ColIdeaLabel = 1;
                ColSpanIdeaLabel = 3;

                RowReasonLabel = 2;
                RowSpanReasonLabel = 1;

                ColReasonLabel = 1;
                ColSpanReasonLabel = 3;

                _screenSize = BissScreenSize.Small;
            }
            else if (Width >= 650 && Width < 850 && _screenSize != BissScreenSize.Medium)
            {
                ColSpanUserLabel = 2;
                RowSpanUserLabel = 1;

                RowIdeaLabel = 1;
                RowSpanIdeaLabel = 1;

                ColIdeaLabel = 1;
                ColSpanIdeaLabel = 2;

                RowReasonLabel = 0;
                RowSpanReasonLabel = 2;

                ColReasonLabel = 3;
                ColSpanReasonLabel = 1;

                _screenSize = BissScreenSize.Medium;
            }
            else if (Width >= 850 && _screenSize != BissScreenSize.Large)
            {
                ColSpanUserLabel = 1;
                RowSpanUserLabel = 1;

                RowIdeaLabel = 0;
                RowSpanIdeaLabel = 1;

                ColIdeaLabel = 2;
                ColSpanIdeaLabel = 1;

                RowReasonLabel = 0;
                RowSpanReasonLabel = 1;

                ColReasonLabel = 3;
                ColSpanReasonLabel = 1;

                _screenSize = BissScreenSize.Large;
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member