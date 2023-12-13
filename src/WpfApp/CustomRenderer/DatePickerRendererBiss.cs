// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;
using Application = System.Windows.Application;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using WDatePicker = System.Windows.Controls.DatePicker;

namespace WpfApp.CustomRenderer
{
    /// <inheritdoc />
    public class DatePickerRenderer2 : ViewRenderer<DatePicker, WDatePicker>
    {
        bool _isDisposed;

        /// <inheritdoc />
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.OldElement != null)
            {
                if (Control != null)
                {
                    Control.GotKeyboardFocus -= OnGotKeyboardFocus;
                    Control.SelectedDateChanged -= OnNativeSelectedDateChanged;
                }
            }

            if (e.NewElement != null)
            {
                SetNativeControl(new WDatePicker());
                //Control.GotKeyboardFocus += OnGotKeyboardFocus;
                Control!.SelectedDateChanged += OnNativeSelectedDateChanged;

                // Update control properties 
                UpdateDate();
                UpdateMinimumDate();
                UpdateMaximumDate();
                UpdateTextColor();
                UpdateFontSize();
                UpdateFontFamily();
                UpdateFontAttributes();
            }

            base.OnElementChanged(e);
        }

        /// <inheritdoc />
        protected override void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == DatePicker.DateProperty.PropertyName)
            {
                UpdateDate();
            }
            else if (e.PropertyName == DatePicker.MaximumDateProperty.PropertyName)
            {
                UpdateMaximumDate();
            }
            else if (e.PropertyName == DatePicker.MinimumDateProperty.PropertyName)
            {
                UpdateMinimumDate();
            }
            else if (e.PropertyName == DatePicker.TextColorProperty.PropertyName)
            {
                UpdateTextColor();
            }
            else if (e.PropertyName == DatePicker.FontSizeProperty.PropertyName)
            {
                UpdateFontSize();
            }
            else if (e.PropertyName == DatePicker.FontFamilyProperty.PropertyName)
            {
                UpdateFontFamily();
            }
            else if (e.PropertyName == DatePicker.FontAttributesProperty.PropertyName)
            {
                UpdateFontAttributes();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (Control != null)
                {
                    Control.SelectedDateChanged -= OnNativeSelectedDateChanged;
                }
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }

        void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!Control.IsDropDownOpen)
            {
                Control.IsDropDownOpen = true;
            }
        }

        void UpdateDate()
        {
            Control.SelectedDate = Element.Date;
        }

        void UpdateMaximumDate()
        {
            Control.DisplayDateEnd = Element.MaximumDate;
        }

        void UpdateMinimumDate()
        {
            Control.DisplayDateStart = Element.MinimumDate;
        }

        void UpdateTextColor()
        {
            Control.UpdateDependencyColor(System.Windows.Controls.Control.ForegroundProperty, Element.TextColor);
        }

        void UpdateFontFamily()
        {
            if (!string.IsNullOrEmpty(Element.FontFamily))
            {
                Control.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), Element.FontFamily);
            }
            else
            {
                Control.FontFamily = (FontFamily) Application.Current.Resources["FontFamilyNormal"];
            }
        }

        void UpdateFontSize()
        {
            Control.FontSize = Element.FontSize;
        }

        void UpdateFontAttributes()
        {
            Control.ApplyFontAttributes(Element.FontAttributes);
        }

        void OnNativeSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (Control.SelectedDate.HasValue)
            {
                ((IElementController) Element).SetValueFromRenderer(DatePicker.DateProperty, Control.SelectedDate.Value);
            }
        }
    }
}