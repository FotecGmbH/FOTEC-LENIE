// (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 08.03.2022 11:37
// Entwickler      Benjamin Moser (BMo)
// Projekt         BISS.Apps.Core

using System.ComponentModel;
using IOsApp.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(BissPickerRenderer))]

namespace IOsApp.CustomRenderer
{
    public class BissPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (Control == null || e.NewElement == null)
                return;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
            {
                var picker = (Picker) sender;
                Control.TextColor = picker.TextColor.ToUIColor();
            }
        }
    }
}