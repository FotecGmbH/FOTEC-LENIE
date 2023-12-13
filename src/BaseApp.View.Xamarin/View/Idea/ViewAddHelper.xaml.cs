// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable once CheckNamespace
namespace BaseApp.View.Xamarin.View
{
    public partial class ViewAddHelper
    {
        public ViewAddHelper() : this(null)
        {
        }

        public ViewAddHelper(object? args = null) : base(args)
        {
            InitializeComponent();
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member