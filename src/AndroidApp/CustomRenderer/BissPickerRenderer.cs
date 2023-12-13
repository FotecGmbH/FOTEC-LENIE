// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using AndroidApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Picker), typeof(BissPickerRenderer))]

namespace AndroidApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissPickerRenderer : PickerRenderer
    {
        public BissPickerRenderer(Context context) : base(context)
        {
        }

        public static float DpToPixels(Context context, float valueInDp)
        {
            var metrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                var view = Element;


                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Control.BackgroundTintList = ColorStateList.ValueOf(view.TextColor.ToAndroid());
                }
                else
#pragma warning disable CS0618 // Type or member is obsolete
                {
                    Control.Background.SetColorFilter(view.TextColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
            {
                var picker = (Picker) sender;
                Control.SetTextColor(picker.TextColor.ToAndroid());
            }
        }
    }
}