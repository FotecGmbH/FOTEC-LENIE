﻿// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using Biss.Apps.Interfaces;
using Biss.Apps.XF.Navigation.Base;
using Xamarin.Forms.Xaml;

// ReSharper disable RedundantExtendsListEntry
namespace BaseApp.View.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewMenu : BaseMenu, IMasterPage
    {
        public ViewMenu()
        {
            InitializeComponent();
            BindingContext = ViewModel;
        }

        #region Properties

        /// <summary>
        ///     ViewModel
        /// </summary>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public VmProjectBase ViewModel => VmProjectBase.GetVmBaseStatic;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        #endregion

        public override BaseMenu GetNewInstance()
        {
            return new ViewMenu();
        }
    }
}