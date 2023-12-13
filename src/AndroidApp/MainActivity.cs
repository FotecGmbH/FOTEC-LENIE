// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Android.App;
using Android.Content.PM;
using Android.OS;
using BaseApp.View.Xamarin;
using Biss.Apps.Droid;
using Biss.Apps.Droid.Toast.Options;
using Biss.Apps.Map.Droid;
using Biss.Apps.Push.Droid;
using Exchange;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace AndroidApp
{
    /// <summary>
    ///     <para>MainActivity - Wird durch SplashActivity aufgerufen</para>
    ///     Klasse MainActivity. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Activity(Label = "AndroidApp",
        Icon = "@drawable/icon",
        Theme = "@style/MainTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.LayoutDirection,
        LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : BissAppsFormsAppCompatActivity
    {
        public MainActivity()
        {
            Param = new object[]
                    {
                        AppSettings.Current().ProjectWorkUserFolder,
                        this,
                        null,
                        new PlatformOptions {SmallIconDrawable = Resource.Drawable.ic_notification},
                        AppSettings.Current(),
                    };
        }

        protected override void OnCreate(Bundle bundle)
        {
            this.BissUsePush(AppSettings.Current());
            base.OnCreate(bundle);
            this.InitBissMap(bundle);
            LoadApplication(new BissApp(Initializer));
            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>()
                .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }
    }
}