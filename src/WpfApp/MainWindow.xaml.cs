// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using BaseApp.View.Xamarin;
using Biss.Apps.Enum;
using Biss.Apps.Map.XF.Wpf;
using Exchange;

namespace WpfApp
{
    /// <summary>
    ///     <para>MainWindow</para>
    ///     Klasse MainWindow. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        ///     MainWindow
        /// </summary>
        public MainWindow()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            Param = new object[]
#pragma warning restore CA1416 // Validate platform compatibility
                    {
                        AppSettings.Current().ProjectWorkUserFolder,
                        AppSettings.Current(),
                    };

            InitializeComponent();
            SetSize();
#pragma warning disable CA1416 // Validate platform compatibility
            _ = BissAppsFormsInit();
#pragma warning restore CA1416 // Validate platform compatibility
            _ = this.InitBissMap(AppSettings.Current());
#pragma warning disable CA1416 // Validate platform compatibility
            LoadApplication(new BissApp(Initializer));
#pragma warning restore CA1416 // Validate platform compatibility
        }

        /// <summary>
        ///     Default Breite und Höhe setzen
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void SetSize()
        {
            if (AppSettings.Current() != null!)
            {
                switch (AppSettings.Current().AppOrientationOverride)
                {
                    case EnumAppOrientation.Auto:
                    case EnumAppOrientation.ForceTablet:
                        Width = 1400;
                        Height = 900;
                        break;
                    case EnumAppOrientation.ForcePhone:
                        Width = 500;
                        Height = 900;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}