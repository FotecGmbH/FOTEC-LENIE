// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Threading.Tasks;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using Xamarin.Essentials;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewChat
    {
        public ViewChat() : this(null)
        {
        }

        public ViewChat(object? args = null) : base(args)
        {
            InitializeComponent();
            ViewModel.ScrollTo = ScrollToBottom;
        }

        /// <summary>
        ///     When overridden, allows application developers to customize behavior immediately prior to the
        ///     <see cref="T:Xamarin.Forms.Page" /> becoming visible.
        /// </summary>
        /// <remarks>To be added.</remarks>
        protected override void OnAppearing()
        {
            LayoutChanged += ViewChat_LayoutChanged;
            base.OnAppearing();
        }


        /// <summary>
        ///     When overridden, allows the application developer to customize behavior as the
        ///     <see cref="T:Xamarin.Forms.Page" /> disappears.
        /// </summary>
        /// <remarks>To be added.</remarks>
        protected override void OnDisappearing()
        {
            LayoutChanged -= ViewChat_LayoutChanged;
            base.OnDisappearing();
        }

        private async void ViewChat_LayoutChanged(object sender, EventArgs e)
        {
            await ScrollToBottom().ConfigureAwait(true);
        }

        private async Task ScrollToBottom()
        {
            try
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await Task.Delay(200).ConfigureAwait(true);
                }

                var maxy = ChatScrollView.ContentSize.Height - ChatScrollView.Height;
                // +1000 Tested Android & iOS
                await ChatScrollView.ScrollToAsync(0, maxy + 1000, false).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"[{nameof(ViewChat)}]({nameof(ScrollToBottom)}): {e}");
                VmProjectBase.LogCrash4Appcenter(e, $"Sizes: SV-{ChatScrollView.Height} SVC-{ChatScrollView.Content.Height}");
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member