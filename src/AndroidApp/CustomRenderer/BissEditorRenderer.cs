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

[assembly: ExportRenderer(typeof(Editor), typeof(BissEditorRenderer))]

namespace AndroidApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissEditorRenderer : EditorRenderer
    {
        public BissEditorRenderer(Context context) : base(context)
        {
        }

        public static float DpToPixels(Context context, float valueInDp)
        {
            var metrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }
    }
}