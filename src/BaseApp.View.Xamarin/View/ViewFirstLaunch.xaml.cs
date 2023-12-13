// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewFirstLaunch
    {
        public ViewFirstLaunch() : this(null)
        {
        }

        public ViewFirstLaunch(object? args = null) : base(args)
        {
            InitializeComponent();

            // Workaround iOS - sonst sieht man beim ersten start der View die IndicatorView unten nicht
            ViewModel.Activated += (sender, eventArgs) =>
            {
                if (ViewModel.HasCustomView)
                {
                    IndiView.IsVisible = false;
                    IndiView.IsVisible = true;
                }
            };
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member