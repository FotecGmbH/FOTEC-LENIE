// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Globalization;
using BaseApp.Connectivity;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>Ist der lokale ChatUser</para>
    ///     Klasse ConverterIsLocalChatUser. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterIsLocalChatUser : IValueConverter
    {
        private DcProjectBase _dc = null!;

        #region Interface Implementations

        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <summary>
        ///     Implement this method to convert <paramref name="value" /> to <paramref name="targetType" /> by using
        ///     <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_dc == null!)
            {
                _dc = VmProjectBase.GetVmBaseStatic.Dc;
            }


            if (!(value is long id) || !_dc.UserRegisteredOnline)
            {
                return false;
            }

            return id == _dc.DcExUser.Data.Id;
        }

        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <summary>
        ///     Implement this method to convert <paramref name="value" /> back from <paramref name="targetType" /> by using
        ///     <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    /// <summary>
    ///     <para>Ist der lokale ChatUser</para>
    ///     Klasse ConverterIsLocalChatUser. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterIsRemoteChatUser : IValueConverter
    {
        private DcProjectBase _dc = null!;

        #region Interface Implementations

        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <summary>
        ///     Implement this method to convert <paramref name="value" /> to <paramref name="targetType" /> by using
        ///     <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_dc == null!)
            {
                _dc = VmProjectBase.GetVmBaseStatic.Dc;
            }


            if (!(value is long id) || !_dc.UserRegisteredOnline)
            {
                return false;
            }

            return id != _dc.DcExUser.Data.Id;
        }

        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type to which to convert the value.</param>
        /// <param name="parameter">A parameter to use during the conversion.</param>
        /// <param name="culture">The culture to use during the conversion.</param>
        /// <summary>
        ///     Implement this method to convert <paramref name="value" /> back from <paramref name="targetType" /> by using
        ///     <paramref name="parameter" /> and <paramref name="culture" />.
        /// </summary>
        /// <returns>To be added.</returns>
        /// <remarks>To be added.</remarks>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}