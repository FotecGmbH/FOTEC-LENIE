// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Apps.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     Bild Editier Button.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BxImgEditButton
    {
        /// <summary>
        ///     Command
        /// </summary>
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            nameof(Command),
            typeof(VmCommand),
            typeof(BxImgEditButton)
        );

        /// <summary>
        ///     Glyph
        /// </summary>
        public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
            nameof(Glyph),
            typeof(string),
            typeof(BxImgEditButton)
        );

        /// <summary>
        ///     Bild Editier Button.
        /// </summary>
        public BxImgEditButton()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        ///     Command
        /// </summary>
        public VmCommand Command
        {
            get => (VmCommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        ///     Glyph
        /// </summary>
        public string Glyph
        {
            get => (string) GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        #endregion
    }
}