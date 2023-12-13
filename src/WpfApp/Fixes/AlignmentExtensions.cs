// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Windows;
using Xamarin.Forms;
using TextAlignment = System.Windows.TextAlignment;

namespace WpfApp.Fixes
{
    internal static class AlignmentExtensions
    {
        internal static TextAlignment ToNativeTextAlignment(this Xamarin.Forms.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    return TextAlignment.Center;
                case Xamarin.Forms.TextAlignment.End:
                    return TextAlignment.Right;
                default:
                    return TextAlignment.Left;
            }
        }

        internal static VerticalAlignment ToNativeVerticalAlignment(this Xamarin.Forms.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Xamarin.Forms.TextAlignment.Start:
                    return VerticalAlignment.Top;
                case Xamarin.Forms.TextAlignment.Center:
                    return VerticalAlignment.Center;
                case Xamarin.Forms.TextAlignment.End:
                    return VerticalAlignment.Bottom;
                default:
                    return VerticalAlignment.Top;
            }
        }

        internal static HorizontalAlignment ToNativeHorizontalAlignment(this Xamarin.Forms.TextAlignment alignment)
        {
            switch (alignment)
            {
                case Xamarin.Forms.TextAlignment.Center:
                    return HorizontalAlignment.Center;
                case Xamarin.Forms.TextAlignment.End:
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Left;
            }
        }

        internal static VerticalAlignment ToNativeVerticalAlignment(this LayoutOptions alignment)
        {
            switch (alignment.Alignment)
            {
                case LayoutAlignment.Start:
                    return VerticalAlignment.Top;
                case LayoutAlignment.Center:
                    return VerticalAlignment.Center;
                case LayoutAlignment.End:
                    return VerticalAlignment.Bottom;
                case LayoutAlignment.Fill:
                    return VerticalAlignment.Stretch;
                default:
                    return VerticalAlignment.Stretch;
            }
        }

        internal static HorizontalAlignment ToNativeHorizontalAlignment(this LayoutOptions alignment)
        {
            switch (alignment.Alignment)
            {
                case LayoutAlignment.Start:
                    return HorizontalAlignment.Left;
                case LayoutAlignment.Center:
                    return HorizontalAlignment.Center;
                case LayoutAlignment.End:
                    return HorizontalAlignment.Right;
                case LayoutAlignment.Fill:
                    return HorizontalAlignment.Stretch;
                default:
                    return HorizontalAlignment.Stretch;
            }
        }
    }
}