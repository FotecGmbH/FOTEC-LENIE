// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using BaseApp.Connectivity;
using Exchange.Model.Chat;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BaseApp.View.Xamarin.Controls
{
    /// <summary>
    ///     Control für ChatDayEntry
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BxChatDayEntryControl
    {
        /// <summary>
        ///     Bindableproperty für den ChatDayEntry
        /// </summary>
        public static BindableProperty ChatDayEntryProperty = BindableProperty.Create(nameof(ChatDayEntry), typeof(DcVmUiChatDayEntry), typeof(BxChatDayEntryControl));

        /// <summary>
        ///     DevMode aktiv?
        /// </summary>
        public static BindableProperty DevModeProperty = BindableProperty.Create(nameof(DevMode), typeof(bool), typeof(BxChatDayEntryControl));

        /// <summary>
        ///     Konstruktor
        /// </summary>
        public BxChatDayEntryControl()
        {
            InitializeComponent();
        }

        #region Properties

        /// <summary>
        ///     Für Autocompletion in der view
        /// </summary>
        public static DcVmUiChatEntry DesignInstanceChatEntry { get; set; } = new DcVmUiChatEntry(new ExDcChatEntry(), new ExDcChatUser());

        /// <summary>
        ///     ChatDayEntry
        /// </summary>
        public DcVmUiChatDayEntry ChatDayEntry
        {
            get => (DcVmUiChatDayEntry) GetValue(ChatDayEntryProperty);
            set => SetValue(ChatDayEntryProperty, value);
        }

        /// <summary>
        ///     DevMode aktiv
        /// </summary>
        public bool DevMode
        {
            get => (bool) GetValue(DevModeProperty);
            set => SetValue(DevModeProperty, value);
        }

        #endregion
    }
}