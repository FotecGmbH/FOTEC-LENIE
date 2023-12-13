// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Label = Xamarin.Forms.Label;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using NativeSize = Windows.Foundation.Size;
#else
using System.Windows.Controls;
using System.Windows.Documents;
using NativeSize = System.Windows.Size;
#endif

#if WINDOWS_UWP
namespace Xamarin.Forms.Platform.UWP
#else
namespace WpfApp.Fixes
#endif
{
    internal static class FixTextBlockExtensions
    {
        public static double FindDefaultLineHeight(this TextBlock control, Inline inline)
        {
            control.Inlines.Add(inline);

            control.Measure(new NativeSize(double.PositiveInfinity, double.PositiveInfinity));

            var height = control.DesiredSize.Height;

            control.Inlines.Remove(inline);
            control = null!;

            return height;
        }

        public static void RecalculateSpanPositions(this TextBlock control, Label element, IList<double> inlineHeights)
        {
            if (element.FormattedText?.Spans == null
                || element.FormattedText.Spans.Count == 0)
            {
                return;
            }

            var labelWidth = control.ActualWidth;

            if (labelWidth <= 0 || control.Height <= 0)
            {
                return;
            }

            for (var i = 0; i < element.FormattedText.Spans.Count; i++)
            {
                var span = element.FormattedText.Spans[i];

                var inline = control.Inlines.ElementAt(i);
                var rect = inline.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                var endRect = inline.ContentEnd.GetCharacterRect(LogicalDirection.Forward);

                var defaultLineHeight = inlineHeights[i];
                if (defaultLineHeight == 0)
                {
                    defaultLineHeight = 19.5;
                }

                var yaxis = rect.Top;
                var lineHeights = new List<double>();
                while (yaxis < endRect.Bottom)
                {
                    double lineHeight;
                    if (Math.Abs(yaxis - rect.Top) == 0) // First Line
                    {
                        lineHeight = rect.Bottom - rect.Top;
                    }
                    else if (Math.Abs(yaxis - endRect.Top) > 0) // Middle Line(s)
                    {
                        lineHeight = defaultLineHeight;
                    }
                    else // Bottom Line
                    {
                        lineHeight = endRect.Bottom - endRect.Top;
                    }

                    lineHeights.Add(lineHeight);
                    yaxis += lineHeight;
                }

                ((ISpatialElement) span).Region = Region.FromLines(lineHeights.ToArray(), labelWidth, rect.X, endRect.X + endRect.Width, rect.Top).Inflate(10);
            }
        }
    }
}