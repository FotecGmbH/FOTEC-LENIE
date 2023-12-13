// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Android.App;
using Android.Runtime;
using Biss.Apps.Push.Droid;

namespace AndroidApp
{
    /// <inheritdoc />
#if DEBUG
    [Application(Debuggable = true)]
#else
	[Application(Debuggable = false)]
#endif
    public class MainApplication : BissDroidApplication
    {
        /// <inheritdoc />
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
            : base(handle, transer, true)
        {
        }
    }
}