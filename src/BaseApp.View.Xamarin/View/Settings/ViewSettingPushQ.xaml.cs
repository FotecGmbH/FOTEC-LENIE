// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Xamarin.Forms;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewSettingPushQ
    {
        public ViewSettingPushQ() : this(null)
        {
        }

        public ViewSettingPushQ(object? args = null) : base(args)
        {
            InitializeComponent();
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            await SwitchPushEnabled.TranslateTo(-5, 0, 100).ConfigureAwait(true);
            await SwitchPushEnabled.TranslateTo(15, 0, 100).ConfigureAwait(true);
            await SwitchPushEnabled.TranslateTo(0, 0, 100).ConfigureAwait(true);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member