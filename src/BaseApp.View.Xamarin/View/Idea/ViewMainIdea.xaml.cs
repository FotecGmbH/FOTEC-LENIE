// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Biss.Log.Producer;
using Microsoft.Extensions.Logging;
using PropertyChanged;
using Xamarin.Forms;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public enum BissScreenSize
    {
        Large,
        Medium,
        Small
    }


    public partial class ViewMainIdea
    {
        private BissScreenSize _screenSize = BissScreenSize.Large;

        public ViewMainIdea() : this(null)
        {
        }

        public ViewMainIdea(object? args = null) : base(args)
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;
            OnSizeChanged(this, EventArgs.Empty);
        }


        [SuppressPropertyChangedWarnings]
        private void OnSizeChanged(object sender, EventArgs e)
        {
            Logging.Log.LogTrace($"[{nameof(ViewMainIdea)}]({nameof(OnSizeChanged)}): Size Change started");
            ViewModel.View.BusySet("Lade Ansicht");

            try
            {
                var width = Width;
                if (width < 650)
                {
                    if (_screenSize == BissScreenSize.Small)
                    {
                        return;
                    }

                    Grid.SetColumn(InnerGrid, 0);
                    Grid.SetColumnSpan(InnerGrid, 3);
                    InnerGrid.Margin = new Thickness(0, 0);
                    ColumnMid.Width = GridLength.Star;

                    _screenSize = BissScreenSize.Small;
                }
                else if (width < 810)
                {
                    if (_screenSize == BissScreenSize.Medium)
                    {
                        return;
                    }

                    Grid.SetColumn(InnerGrid, 0);
                    Grid.SetColumnSpan(InnerGrid, 3);
                    InnerGrid.Margin = new Thickness(80, 0);
                    ColumnMid.Width = 650;

                    _screenSize = BissScreenSize.Medium;
                }
                else
                {
                    if (_screenSize == BissScreenSize.Large)
                    {
                        return;
                    }

                    Grid.SetColumn(InnerGrid, 1);
                    Grid.SetColumnSpan(InnerGrid, 1);
                    InnerGrid.Margin = new Thickness(0, 0);
                    ColumnMid.Width = 650;

                    _screenSize = BissScreenSize.Large;
                }
            }
            catch (Exception ex)
            {
                Logging.Log.LogError($"[{nameof(ViewMainIdea)}]({nameof(OnSizeChanged)}): {ex}");
            }
            finally
            {
                ViewModel.View.BusyClear(true);
                Logging.Log.LogTrace($"[{nameof(ViewMainIdea)}]({nameof(OnSizeChanged)}): Size Change finished");
            }
        }

        private void VisualElement_OnSizeChanged(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.WPF)
            {
                ForceLayout();
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member