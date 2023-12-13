// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using Android.Content;
using Android.Util;
using AndroidApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SearchBar), typeof(BissSearchBarRenderer))]

namespace AndroidApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissSearchBarRenderer : SearchBarRenderer
    {
        public BissSearchBarRenderer(Context context) : base(context)
        {
        }

        public static float DpToPixels(Context context, float valueInDp)
        {
            var metrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }


        protected override void OnElementChanged(ElementChangedEventArgs<SearchBar> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                //var view = Element;
                //var gradientBackground = new GradientDrawable();
                //gradientBackground.SetStroke(1, view.BackgroundColor.ToAndroid());
                //gradientBackground.SetCornerRadius(
                //    DpToPixels(_context, Convert.ToSingle(8)));
                //gradientBackground.SetShape(ShapeType.Rectangle);
                //gradientBackground.SetColor(view.BackgroundColor.ToAndroid());
                //Control.SetBackground(gradientBackground);
                //SetBackgroundColor(Color.Transparent.ToAndroid());

                //var plateId = Resources.GetIdentifier("android:id/search_plate", null, null);
                //var plate = Control.FindViewById(plateId);
                //plate.SetBackgroundColor(Android.Graphics.Color.Transparent);

                //// Set padding for the internal text from border  
                //Control.SetPadding(
                //    (int)DpToPixels(_context, Convert.ToSingle(12)), Control.PaddingTop,
                //    (int)DpToPixels(_context, Convert.ToSingle(12)), Control.PaddingBottom);
            }
        }
    }
}