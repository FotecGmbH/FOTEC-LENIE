// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System.Reflection;
using System.Runtime.InteropServices;
using Android;
using Android.App;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LENIE")]
[assembly: ComVisible(false)]
[assembly: UsesFeature("android.hardware.wifi", Required = false)]
[assembly: UsesPermission(Manifest.Permission.Flashlight)]
[assembly: UsesPermission(Manifest.Permission.Camera)]

[assembly: AssemblyVersion("1.4.1.0")]
[assembly: AssemblyFileVersion("1.4.1.0")]
[assembly: AssemblyProduct("LENIE")]
[assembly: AssemblyDescription("LENIE Leben in Niederösterreich")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DIH-Ost")]
[assembly: AssemblyCopyright("(C) 2009-2023 FOTEC Forschungs- und Technologietransfer GmbH")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyTrademark("(C) 2009-2023 FOTEC Forschungs- und Technologietransfer GmbH")]