// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace AndroidApp
{
    /// <summary>
    ///     <para>SplashActivity - Erste Aktivität bei Android</para>
    ///     Klasse SplashActivity. (C) 2017 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    [Activity(MainLauncher = true, Theme = "@style/LaunchTheme", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait, LaunchMode = LaunchMode.SingleTop)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var openMainActivity = new Intent(BaseContext, typeof(MainActivity));
            openMainActivity.SetFlags(ActivityFlags.ReorderToFront);
            if (bundle != null)
            {
                openMainActivity.PutExtras(bundle);
            }

            StartActivityIfNeeded(openMainActivity, 0);
            Finish();
        }
    }
}