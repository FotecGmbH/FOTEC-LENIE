﻿// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using IOsApp.CustomRenderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


//https://www.c-sharpcorner.com/article/xamarin-forms-custom-entry/
[assembly: ExportRenderer(typeof(Entry), typeof(BissEntryRenderer))]

namespace IOsApp.CustomRenderer
{
    /// <summary>
    ///     <para>BissEntryRenderer</para>
    ///     Klasse BissEntryRenderer. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class BissEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                var view = Element;

                //Control.LeftView = new UIView(new CGRect(0f, 0f, 9f, 20f));  
                //Control.LeftViewMode = UITextFieldViewMode.Always;  

                //Control.KeyboardAppearance = UIKeyboardAppearance.Dark;  
                //Control.ReturnKeyType = UIReturnKeyType.Done;  
                //// Radius for the curves  
                //Control.Layer.CornerRadius = Convert.ToSingle(8);  
                //// Thickness of the Border Color  
                //Control.Layer.BorderColor = view.BackgroundColor.ToCGColor();  
                //// Thickness of the Border Width  
                //Control.Layer.BorderWidth = 1;  
                //Control.ClipsToBounds = true;  

                Control.ReturnKeyType = UIReturnKeyType.Done;
                // Radius for the curves  
                Control.Layer.CornerRadius = Convert.ToSingle(4);
                // Thickness of the Border Color  
                Control.Layer.BorderColor = view.TextColor.ToCGColor();
                // Thickness of the Border Width  
                Control.Layer.BorderWidth = 1;
                Control.ClipsToBounds = true;
            }
        }
    }
}