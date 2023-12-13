// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using Database.Tables;
using Exchange.Model;

namespace Database.Converter;

/// <summary>
///     <para>DB Company Hilfsmethoden</para>
///     Klasse ConverterDbCompany. (C) 2022 FOTEC Forschungs- und Technologietransfer GmbH
/// </summary>
public static class ConverterDbDeviceInfo
{
    /// <summary>
    ///     Konvertieren in TableDevice
    /// </summary>
    /// <param name="d">Daten</param>
    /// <param name="t">DB Element</param>
    /// <returns></returns>
    public static void ToTableDevice(this ExDeviceInfo d, TableDevice t)
    {
        if (d == null! || t == null)
        {
            throw new NullReferenceException($"[{nameof(ConverterDbDeviceInfo)}]({nameof(ToTableDevice)}): {nameof(ExDeviceInfo)} or {nameof(TableDevice)} is null!");
        }

        t.ScreenResolution = d.ScreenResolution;
        t.RefreshToken = d.RefreshToken;
        t.LastLogin = d.LastLogin;
        t.AppVersion = d.AppVersion;
        t.CurrentAppType = d.CurrentAppType;
        t.DeviceHardwareId = d.DeviceHardwareId;
        t.Plattform = d.Plattform;
        t.DeviceIdiom = d.DeviceIdiom;
        t.OperatingSystemVersion = d.OperatingSystemVersion;
        t.DeviceType = d.DeviceType;
        t.DeviceName = d.DeviceName;
        t.Model = d.Model;
        t.Manufacturer = d.Manufacturer;
        t.DeviceToken = d.DeviceToken;
    }

    /// <summary>
    ///     Konvertieren in ExDeviceInfo
    /// </summary>
    /// <param name="t">Daten</param>
    /// <returns></returns>
    public static ExDeviceInfo ToExDeviceInfo(this TableDevice t)
    {
        if (t == null!)
        {
            throw new NullReferenceException("[ToExDeviceInfo] TableDevice is null!");
        }

        return new ExDeviceInfo
               {
                   LastDateTimeUtcOnline = t.LastDateTimeUtcOnline,
                   IsAppRunning = t.IsAppRunning,
                   ScreenResolution = string.IsNullOrEmpty(t.ScreenResolution) ? string.Empty : t.ScreenResolution,
                   RefreshToken = string.IsNullOrEmpty(t.RefreshToken) ? string.Empty : t.RefreshToken,
                   LastLogin = t.LastLogin ?? DateTime.MinValue,
                   AppVersion = t.AppVersion,
                   DeviceHardwareId = t.DeviceHardwareId,
                   Plattform = t.Plattform,
                   DeviceIdiom = t.DeviceIdiom,
                   OperatingSystemVersion = t.OperatingSystemVersion,
                   DeviceType = t.DeviceType,
                   DeviceName = t.DeviceName,
                   Model = t.Model,
                   Manufacturer = t.Manufacturer,
                   DeviceToken = t.DeviceToken
               };
    }
}