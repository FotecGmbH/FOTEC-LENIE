// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace BaseApp.View.Xamarin.View
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewMain
    {
        public ViewMain()
        {
            InitializeComponent();
            Init();
        }

        public ViewMain(object args = null!) : base(args)
        {
            InitializeComponent();
            Init();
        }

        public void ClearStackPanelAndNavigateFrist(INavigation navigation)
        {
            if (navigation == null!)
            {
                return;
            }

            var existingPages = navigation.NavigationStack.ToList();
            foreach (var t in existingPages)
            {
                navigation.RemovePage(t);
            }
        }

        private void Init()
        {
            if (Navigation.NavigationStack.Count > 0)
            {
                ClearStackPanelAndNavigateFrist(Navigation);
            }
        }
    }
}