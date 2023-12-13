// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Windows;
using WpfApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

[assembly: ExportRenderer(typeof(Picker), typeof(BissWpfPickerRenderer))]

namespace WpfApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissWpfSwitchRenderer</para>
    ///     Klasse BissWpfSwitchRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissWpfPickerRenderer : PickerRenderer
    {
        /// <summary>
        ///     OnElementChanged
        /// </summary>
        /// <param name="e"></param>
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (e != null! && e.NewElement != null)
            {
                var view = e.NewElement;
                Control.Resources.Add(SystemColors.WindowBrushKey, view.BackgroundColor.ToBrush());
            }
        }
    }
}