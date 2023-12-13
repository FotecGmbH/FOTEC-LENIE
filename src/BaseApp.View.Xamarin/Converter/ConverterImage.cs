// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Biss.Apps.Model;
using Exchange.Resources;
using Xamarin.Forms;

namespace BaseApp.View.Xamarin.Converter
{
    /// <summary>
    ///     <para>Url to ImageSource converter.</para>
    ///     Klasse UrlToImageSourceConverter. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class ConverterImage : IValueConverter
    {
        #region Interface Implementations

        /// <summary>
        ///     Konvertiert ein Objekt für XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das UI</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">EnumEmbeddedImage</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder null</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            #region Exfile

            if (value is ExFile file)
            {
                if (file.Bytes != null! && file.Bytes.Any())
                {
                    return ImageSource.FromStream(() => new MemoryStream(file.Bytes));
                }

                if (!string.IsNullOrWhiteSpace(file.DownloadLink))
                {
                    if (file.DownloadLink.ToUpperInvariant().StartsWith("HTTP"))
                    {
                        return ImageSource.FromUri(new Uri(file.DownloadLink));
                    }

                    return ImageSource.FromFile(file.DownloadLink);
                }
            }

            #endregion

            #region byte[] und Stream

            if (value is byte[] bytes && bytes.Any())
            {
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }

            if (value is Stream stream && stream.Length > 0)
            {
                return ImageSource.FromStream(() => stream);
            }

            #endregion

            #region URL (string)

            if (value is string url)
            {
#pragma warning disable CA1508 // Avoid dead conditional code
                if (!string.IsNullOrWhiteSpace(url))
#pragma warning restore CA1508 // Avoid dead conditional code
                {
                    if (url.ToUpperInvariant().StartsWith("HTTP"))
                    {
                        return ImageSource.FromUri(new Uri(url));
                    }

                    return ImageSource.FromFile(url);
                }
            }

            #endregion

            #region EnumEmbeddedImage

            if (parameter != null!)
            {
                if (parameter is EnumEmbeddedImage embedded)
                {
                    return ImageSource.FromStream(() => Images.ReadImageAsStream(embedded));
                }

                if (parameter is string defaultImage && defaultImage.Equals("DefaultUserImage", StringComparison.Ordinal))
                {
                    if (VmProjectBase.GetVmBaseStatic.Dc.CoreConnectionInfos.UserOk &&
                        VmProjectBase.GetVmBaseStatic.Dc.DcExUser.Data.HasImage)
                    {
                        var imgLink = VmProjectBase.GetVmBaseStatic.Dc.DcExUser.Data.UserImageLink;

                        if (imgLink.ToUpperInvariant().StartsWith("HTTP"))
                        {
                            return ImageSource.FromUri(new Uri(imgLink));
                        }

                        return ImageSource.FromFile(imgLink);
                    }

                    return ImageSource.FromStream(() => Images.ReadImageAsStream(EnumEmbeddedImage.DefaultUserImageYellow_png));
                }

                return null!;
            }

            #endregion


            return null!;
        }

        /// <summary>
        ///     Konvertiert ein Objekt von XAML
        /// </summary>
        /// <param name="value">Wert zum konvertieren für das Datenobjekt</param>
        /// <param name="targetType">Zieltyp des Werts</param>
        /// <param name="parameter">Zusätzlicher Parameter aus XAML</param>
        /// <param name="culture">Aktuelle Kultur</param>
        /// <returns>Konvertierter Wert oder UnsetValue</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }

        #endregion
    }
}