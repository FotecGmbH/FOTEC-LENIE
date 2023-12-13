// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.IO;
using System.Reflection;

// ReSharper disable InconsistentNaming
namespace Exchange.Resources
{
    /// <summary>
    ///     Bilder in den Resourcen
    /// </summary>
#pragma warning disable CS1591
    public enum EnumEmbeddedImage
    {
        Logo_png,
        DefaultUserImage_png,
        Pin_png,
        DefaultUserImageRed_png,
        DefaultUserImageYellow_png,
        dihost_png,
        FOTECtransp_png,
        noe_png,
        NoeRegional_png,
        Year100_png,
        Year100transp_png,
        LENIE_png,
        LENIE2_png,
        LENIEIcon_png,
        background_jpg,
        Logo1_png,
        Logo2_png,
        Logo3_png,
        backgroundInfo_jpg,
        Idea_png,
        HdDNetzwerk_png,
        EcoDigitalEU_png
    }
#pragma warning restore CS1591

    /// <summary>
    ///     <para>Bilder laden (Projektweit)</para>
    ///     Klasse Images. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public static class Images
    {
        /// <summary>
        ///     Bild als lokalen Stream Laden
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public static Stream ReadImageAsStream(EnumEmbeddedImage imageName)
        {
#pragma warning disable CA1307 // Specify StringComparison for clarity
            var image = $"Exchange.Resources.Images.{imageName.ToString().Replace("_", ".")}";
#pragma warning restore CA1307 // Specify StringComparison for clarity
            var assembly = Assembly.Load(new AssemblyName("Exchange"));
            var imageStream = assembly.GetManifestResourceStream(image);
            return imageStream;
        }
    }
}