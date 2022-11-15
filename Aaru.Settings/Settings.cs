// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Settings.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru settings.
//
// --[ Description ] ----------------------------------------------------------
//
//     Stores and retrieves settings.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2021 Rebecca Wallander
// ****************************************************************************/

namespace Aaru.Settings;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Aaru.CommonTypes.Interop;
using Claunia.PropertyList;
using Microsoft.Win32;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

/// <summary>Settings</summary>
public class DicSettings
{
    /// <summary>
    ///     Level for GDPR compliance checking. Every time a new feature may share user information this level should go
    ///     up, and the user asked to opt-in.
    /// </summary>
    public const ulong GDPR_LEVEL = 1;
    /// <summary>If set to <c>true</c>, enables the ability to decrypt encrypted data</summary>
    public bool EnableDecryption;
    /// <summary>Set of GDPR compliance, if lower than <see cref="GDPR_LEVEL" />, ask user for compliance.</summary>
    public ulong GdprCompliance;

    /// <summary>If set to <c>true</c>, reports will be saved locally</summary>
    public bool SaveReportsGlobally;
    /// <summary>If set to <c>true</c>, reports will be sent to Aaru.Server</summary>
    public bool ShareReports;
    /// <summary>Statistics</summary>
    public StatsSettings Stats;
}

// TODO: Use this
/// <summary>User settings, for media dumps, completely unused</summary>
public class UserSettings
{
    /// <summary>User email</summary>
    public string Email;
    /// <summary>User name or nick</summary>
    public string Name;
}

/// <summary>Statistics settings</summary>
public class StatsSettings
{
    /// <summary>If set to <c>true</c>, benchmark statistics will be stored</summary>
    public bool BenchmarkStats = false;
    /// <summary>If set to <c>true</c>, command usage statistics will be stored</summary>
    public bool CommandStats;
    /// <summary>If set to <c>true</c>, device statistics will be stored</summary>
    public bool DeviceStats;
    /// <summary>If set to <c>true</c>, filesystem statistics will be stored</summary>
    public bool FilesystemStats;
    /// <summary>If set to <c>true</c>, filters statistics will be stored</summary>
    public bool FilterStats;
    /// <summary>If set to <c>true</c>, dump media images statistics will be stored</summary>
    public bool MediaImageStats;
    /// <summary>If set to <c>true</c>, media scan statistics will be stored</summary>
    public bool MediaScanStats;
    /// <summary>If set to <c>true</c>, media types statistics will be stored</summary>
    public bool MediaStats;
    /// <summary>If set to <c>true</c>, partition schemes statistics will be stored</summary>
    public bool PartitionStats;
    /// <summary>If set to <c>true</c>, statistics will be sent to Aaru.Server</summary>
    public bool ShareStats;
    /// <summary>If set to <c>true</c>, dump media verification statistics will be stored</summary>
    public bool VerifyStats;
}

/// <summary>Manages statistics</summary>
public static class Settings
{
    const string XDG_DATA_HOME            = "XDG_DATA_HOME";
    const string XDG_CONFIG_HOME          = "XDG_CONFIG_HOME";
    const string XDG_DATA_HOME_RESOLVED   = ".local/share";
    const string XDG_CONFIG_HOME_RESOLVED = ".config";
    /// <summary>Current statistics</summary>
    public static DicSettings Current;

    /// <summary>Global path to save reports</summary>
    static string ReportsPath { get; set; }

    /// <summary>Global path to save statistics</summary>
    public static string StatsPath { get; private set; }

    /// <summary>Local database path</summary>
    public static string LocalDbPath { get; private set; }
    /// <summary>Main database path</summary>
    public static string MainDbPath { get; private set; }

    /// <summary>Loads settings</summary>
    public static void LoadSettings()
    {
        Current = new DicSettings();
        PlatformID ptId     = DetectOS.GetRealPlatformID();
        string     homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        LocalDbPath = "local.db";
        var oldMainDbPath = "master.db";
        MainDbPath = "main.db";

        try
        {
            switch(ptId)
            {
                // In case of macOS or iOS statistics and reports will be saved in ~/Library/Application Support/Claunia.com/Aaru
                case PlatformID.MacOSX:
                case PlatformID.iOS:
                {
                    string appSupportPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                     "Application Support", "Claunia.com");

                    if(!Directory.Exists(appSupportPath))
                        Directory.CreateDirectory(appSupportPath);

                    string dicPath  = Path.Combine(appSupportPath, "DiscImageChef");
                    string aaruPath = Path.Combine(appSupportPath, "Aaru");

                    if(Directory.Exists(dicPath) &&
                       !Directory.Exists(aaruPath))
                        Directory.Move(dicPath, aaruPath);

                    if(!Directory.Exists(aaruPath))
                        Directory.CreateDirectory(aaruPath);

                    LocalDbPath   = Path.Combine(aaruPath, LocalDbPath);
                    MainDbPath    = Path.Combine(aaruPath, MainDbPath);
                    oldMainDbPath = Path.Combine(aaruPath, oldMainDbPath);

                    ReportsPath = Path.Combine(aaruPath, "Reports");

                    if(!Directory.Exists(ReportsPath))
                        Directory.CreateDirectory(ReportsPath);

                    StatsPath = Path.Combine(aaruPath, "Statistics");

                    if(!Directory.Exists(StatsPath))
                        Directory.CreateDirectory(StatsPath);
                }

                    break;

                // In case of Windows statistics and reports will be saved in %APPDATA%\Claunia.com\Aaru
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.WindowsPhone:
                {
                    string appSupportPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                     "Claunia.com");

                    if(!Directory.Exists(appSupportPath))
                        Directory.CreateDirectory(appSupportPath);

                    string dicPath  = Path.Combine(appSupportPath, "DiscImageChef");
                    string aaruPath = Path.Combine(appSupportPath, "Aaru");

                    if(Directory.Exists(dicPath) &&
                       !Directory.Exists(aaruPath))
                        Directory.Move(dicPath, aaruPath);

                    if(!Directory.Exists(aaruPath))
                        Directory.CreateDirectory(aaruPath);

                    LocalDbPath   = Path.Combine(aaruPath, LocalDbPath);
                    MainDbPath    = Path.Combine(aaruPath, MainDbPath);
                    oldMainDbPath = Path.Combine(aaruPath, oldMainDbPath);

                    ReportsPath = Path.Combine(aaruPath, "Reports");

                    if(!Directory.Exists(ReportsPath))
                        Directory.CreateDirectory(ReportsPath);

                    StatsPath = Path.Combine(aaruPath, "Statistics");

                    if(!Directory.Exists(StatsPath))
                        Directory.CreateDirectory(StatsPath);
                }

                    break;

                // Otherwise, statistics and reports will be saved in ~/.claunia.com/Aaru
                default:
                {
                    string xdgDataPath =
                        Path.Combine(homePath,
                                     Environment.GetEnvironmentVariable(XDG_DATA_HOME) ?? XDG_DATA_HOME_RESOLVED);

                    string oldDicPath = Path.Combine(homePath, ".claunia.com", "DiscImageChef");
                    string dicPath    = Path.Combine(xdgDataPath, "DiscImageChef");
                    string aaruPath   = Path.Combine(xdgDataPath, "Aaru");

                    if(Directory.Exists(oldDicPath) &&
                       !Directory.Exists(aaruPath))
                    {
                        Directory.Move(oldDicPath, aaruPath);
                        Directory.Delete(Path.Combine(homePath, ".claunia.com"));
                    }

                    if(Directory.Exists(dicPath) &&
                       !Directory.Exists(aaruPath))
                        Directory.Move(dicPath, aaruPath);

                    if(!Directory.Exists(aaruPath))
                        Directory.CreateDirectory(aaruPath);

                    LocalDbPath   = Path.Combine(aaruPath, LocalDbPath);
                    MainDbPath    = Path.Combine(aaruPath, MainDbPath);
                    oldMainDbPath = Path.Combine(aaruPath, oldMainDbPath);

                    ReportsPath = Path.Combine(aaruPath, "Reports");

                    if(!Directory.Exists(ReportsPath))
                        Directory.CreateDirectory(ReportsPath);

                    StatsPath = Path.Combine(aaruPath, "Statistics");

                    if(!Directory.Exists(StatsPath))
                        Directory.CreateDirectory(StatsPath);
                }

                    break;
            }

            if(File.Exists(oldMainDbPath))
                File.Move(oldMainDbPath, MainDbPath);
        }
        catch
        {
            ReportsPath = null;
        }

        FileStream   prefsFs = null;
        StreamReader prefsSr = null;

        try
        {
            switch(ptId)
            {
                // In case of macOS or iOS settings will be saved in ~/Library/Preferences/com.claunia.aaru.plist
                case PlatformID.MacOSX:
                case PlatformID.iOS:
                {
                    string preferencesPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                     "Preferences");

                    string dicPreferencesFilePath = Path.Combine(preferencesPath, "com.claunia.discimagechef.plist");

                    string preferencesFilePath = Path.Combine(preferencesPath, "com.claunia.aaru.plist");

                    if(File.Exists(dicPreferencesFilePath))
                        File.Move(dicPreferencesFilePath, preferencesFilePath);

                    if(!File.Exists(preferencesFilePath))
                    {
                        SetDefaultSettings();
                        SaveSettings();
                    }

                    prefsFs = new FileStream(preferencesFilePath, FileMode.Open, FileAccess.Read);

                    var parsedPreferences = (NSDictionary)BinaryPropertyListParser.Parse(prefsFs);

                    if(parsedPreferences != null)
                    {
                        Current.SaveReportsGlobally =
                            parsedPreferences.TryGetValue("SaveReportsGlobally", out NSObject obj) &&
                            ((NSNumber)obj).ToBool();

                        Current.ShareReports = parsedPreferences.TryGetValue("ShareReports", out obj) &&
                                               ((NSNumber)obj).ToBool();

                        Current.EnableDecryption = parsedPreferences.TryGetValue("EnableDecryption", out obj) &&
                                                   ((NSNumber)obj).ToBool();

                        if(parsedPreferences.TryGetValue("Stats", out obj))
                        {
                            var stats = (NSDictionary)obj;

                            if(stats != null)
                                Current.Stats = new StatsSettings
                                {
                                    ShareStats = stats.TryGetValue("ShareStats", out NSObject obj2) &&
                                                 ((NSNumber)obj2).ToBool(),
                                    CommandStats = stats.TryGetValue("CommandStats", out obj2) &&
                                                   ((NSNumber)obj2).ToBool(),
                                    DeviceStats = stats.TryGetValue("DeviceStats", out obj2) &&
                                                  ((NSNumber)obj2).ToBool(),
                                    FilesystemStats = stats.TryGetValue("FilesystemStats", out obj2) &&
                                                      ((NSNumber)obj2).ToBool(),
                                    FilterStats = stats.TryGetValue("FilterStats", out obj2) &&
                                                  ((NSNumber)obj2).ToBool(),
                                    MediaImageStats = stats.TryGetValue("MediaImageStats", out obj2) &&
                                                      ((NSNumber)obj2).ToBool(),
                                    MediaScanStats = stats.TryGetValue("MediaScanStats", out obj2) &&
                                                     ((NSNumber)obj2).ToBool(),
                                    PartitionStats = stats.TryGetValue("PartitionStats", out obj2) &&
                                                     ((NSNumber)obj2).ToBool(),
                                    MediaStats = stats.TryGetValue("MediaStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                    VerifyStats = stats.TryGetValue("VerifyStats", out obj2) &&
                                                  ((NSNumber)obj2).ToBool()
                                };
                        }
                        else
                            Current.Stats = null;

                        Current.GdprCompliance = parsedPreferences.TryGetValue("GdprCompliance", out obj)
                                                     ? (ulong)((NSNumber)obj).ToLong() : 0;

                        prefsFs.Close();
                    }
                    else
                    {
                        prefsFs.Close();

                        SetDefaultSettings();
                        SaveSettings();
                    }
                }

                    break;
            #if !NETSTANDARD2_0

                // In case of Windows settings will be saved in the registry: HKLM/SOFTWARE/Claunia.com/Aaru
                case PlatformID.Win32NT when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.Win32S when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.Win32Windows when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.WinCE when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.WindowsPhone when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                {
                    RegistryKey parentKey = Registry.CurrentUser.OpenSubKey("SOFTWARE")?.OpenSubKey("Claunia.com");

                    if(parentKey == null)
                    {
                        SetDefaultSettings();
                        SaveSettings();

                        return;
                    }

                    RegistryKey dicKey = parentKey.OpenSubKey("DiscImageChef");
                    RegistryKey key    = parentKey.OpenSubKey("Aaru");
                    bool        stats;

                    if(dicKey != null &&
                       key    == null)
                    {
                        Current.SaveReportsGlobally = Convert.ToBoolean(dicKey.GetValue("SaveReportsGlobally"));
                        Current.ShareReports        = Convert.ToBoolean(dicKey.GetValue("ShareReports"));
                        Current.GdprCompliance      = Convert.ToUInt64(dicKey.GetValue("GdprCompliance"));
                        Current.EnableDecryption    = Convert.ToBoolean(dicKey.GetValue("EnableDecryption"));

                        stats = Convert.ToBoolean(dicKey.GetValue("Statistics"));

                        if(stats)
                            Current.Stats = new StatsSettings
                            {
                                ShareStats      = Convert.ToBoolean(dicKey.GetValue("ShareStats")),
                                CommandStats    = Convert.ToBoolean(dicKey.GetValue("CommandStats")),
                                DeviceStats     = Convert.ToBoolean(dicKey.GetValue("DeviceStats")),
                                FilesystemStats = Convert.ToBoolean(dicKey.GetValue("FilesystemStats")),
                                FilterStats     = Convert.ToBoolean(dicKey.GetValue("FilterStats")),
                                MediaImageStats = Convert.ToBoolean(dicKey.GetValue("MediaImageStats")),
                                MediaScanStats  = Convert.ToBoolean(dicKey.GetValue("MediaScanStats")),
                                PartitionStats  = Convert.ToBoolean(dicKey.GetValue("PartitionStats")),
                                MediaStats      = Convert.ToBoolean(dicKey.GetValue("MediaStats")),
                                VerifyStats     = Convert.ToBoolean(dicKey.GetValue("VerifyStats"))
                            };

                        SaveSettings();

                        parentKey.DeleteSubKeyTree("DiscImageChef");

                        return;
                    }

                    if(key == null)
                    {
                        SetDefaultSettings();
                        SaveSettings();

                        return;
                    }

                    Current.SaveReportsGlobally = Convert.ToBoolean(key.GetValue("SaveReportsGlobally"));
                    Current.ShareReports        = Convert.ToBoolean(key.GetValue("ShareReports"));
                    Current.GdprCompliance      = Convert.ToUInt64(key.GetValue("GdprCompliance"));
                    Current.EnableDecryption    = Convert.ToBoolean(key.GetValue("EnableDecryption"));

                    stats = Convert.ToBoolean(key.GetValue("Statistics"));

                    if(stats)
                        Current.Stats = new StatsSettings
                        {
                            ShareStats      = Convert.ToBoolean(key.GetValue("ShareStats")),
                            CommandStats    = Convert.ToBoolean(key.GetValue("CommandStats")),
                            DeviceStats     = Convert.ToBoolean(key.GetValue("DeviceStats")),
                            FilesystemStats = Convert.ToBoolean(key.GetValue("FilesystemStats")),
                            FilterStats     = Convert.ToBoolean(key.GetValue("FilterStats")),
                            MediaImageStats = Convert.ToBoolean(key.GetValue("MediaImageStats")),
                            MediaScanStats  = Convert.ToBoolean(key.GetValue("MediaScanStats")),
                            PartitionStats  = Convert.ToBoolean(key.GetValue("PartitionStats")),
                            MediaStats      = Convert.ToBoolean(key.GetValue("MediaStats")),
                            VerifyStats     = Convert.ToBoolean(key.GetValue("VerifyStats"))
                        };
                }

                    break;
            #endif

                // Otherwise, settings will be saved in ~/.config/Aaru.xml
                default:
                {
                    string oldConfigPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

                    string oldSettingsPath = Path.Combine(oldConfigPath, "DiscImageChef.xml");

                    string xdgConfigPath =
                        Path.Combine(homePath,
                                     Environment.GetEnvironmentVariable(XDG_CONFIG_HOME) ?? XDG_CONFIG_HOME_RESOLVED);

                    string dicSettingsPath = Path.Combine(xdgConfigPath, "DiscImageChef.xml");
                    string settingsPath    = Path.Combine(xdgConfigPath, "Aaru.xml");

                    if(File.Exists(oldSettingsPath) &&
                       !File.Exists(settingsPath))
                    {
                        if(!Directory.Exists(xdgConfigPath))
                            Directory.CreateDirectory(xdgConfigPath);

                        File.Move(oldSettingsPath, settingsPath);
                    }

                    if(File.Exists(dicSettingsPath) &&
                       !File.Exists(settingsPath))
                        File.Move(dicSettingsPath, settingsPath);

                    if(!File.Exists(settingsPath))
                    {
                        SetDefaultSettings();
                        SaveSettings();

                        return;
                    }

                    var xs = new XmlSerializer(Current.GetType());
                    prefsSr = new StreamReader(settingsPath);
                    Current = (DicSettings)xs.Deserialize(prefsSr);
                }

                    break;
            }
        }
        catch
        {
            prefsFs?.Close();
            prefsSr?.Close();
            SetDefaultSettings();
            SaveSettings();
        }
    }

    /// <summary>Saves current settings</summary>
    public static void SaveSettings()
    {
        try
        {
            PlatformID ptId = DetectOS.GetRealPlatformID();

            switch(ptId)
            {
                // In case of macOS or iOS settings will be saved in ~/Library/Preferences/com.claunia.aaru.plist
                case PlatformID.MacOSX:
                case PlatformID.iOS:
                {
                    var root = new NSDictionary
                    {
                        {
                            "SaveReportsGlobally", Current.SaveReportsGlobally
                        },
                        {
                            "ShareReports", Current.ShareReports
                        },
                        {
                            "GdprCompliance", Current.GdprCompliance
                        },
                        {
                            "EnableDecryption", Current.EnableDecryption
                        }
                    };

                    if(Current.Stats != null)
                    {
                        var stats = new NSDictionary
                        {
                            {
                                "ShareStats", Current.Stats.ShareStats
                            },
                            {
                                "CommandStats", Current.Stats.CommandStats
                            },
                            {
                                "DeviceStats", Current.Stats.DeviceStats
                            },
                            {
                                "FilesystemStats", Current.Stats.FilesystemStats
                            },
                            {
                                "FilterStats", Current.Stats.FilterStats
                            },
                            {
                                "MediaImageStats", Current.Stats.MediaImageStats
                            },
                            {
                                "MediaScanStats", Current.Stats.MediaScanStats
                            },
                            {
                                "PartitionStats", Current.Stats.PartitionStats
                            },
                            {
                                "MediaStats", Current.Stats.MediaStats
                            },
                            {
                                "VerifyStats", Current.Stats.VerifyStats
                            }
                        };

                        root.Add("Stats", stats);
                    }

                    string preferencesPath =
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                     "Preferences");

                    string preferencesFilePath = Path.Combine(preferencesPath, "com.claunia.aaru.plist");

                    var fs = new FileStream(preferencesFilePath, FileMode.Create);
                    BinaryPropertyListWriter.Write(fs, root);
                    fs.Close();
                }

                    break;
            #if !NETSTANDARD2_0

                // In case of Windows settings will be saved in the registry: HKLM/SOFTWARE/Claunia.com/Aaru
                case PlatformID.Win32NT when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.Win32S when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.Win32Windows when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.WinCE when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                case PlatformID.WindowsPhone when RuntimeInformation.IsOSPlatform(OSPlatform.Windows):
                {
                    RegistryKey parentKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true)?.
                                                     CreateSubKey("Claunia.com");

                    RegistryKey key = parentKey?.CreateSubKey("Aaru");

                    if(key != null)
                    {
                        key.SetValue("SaveReportsGlobally", Current.SaveReportsGlobally);
                        key.SetValue("ShareReports", Current.ShareReports);
                        key.SetValue("GdprCompliance", Current.GdprCompliance);
                        key.SetValue("EnableDecryption", Current.EnableDecryption);

                        if(Current.Stats != null)
                        {
                            key.SetValue("Statistics", true);
                            key.SetValue("ShareStats", Current.Stats.ShareStats);
                            key.SetValue("CommandStats", Current.Stats.CommandStats);
                            key.SetValue("DeviceStats", Current.Stats.DeviceStats);
                            key.SetValue("FilesystemStats", Current.Stats.FilesystemStats);
                            key.SetValue("FilterStats", Current.Stats.FilterStats);
                            key.SetValue("MediaImageStats", Current.Stats.MediaImageStats);
                            key.SetValue("MediaScanStats", Current.Stats.MediaScanStats);
                            key.SetValue("PartitionStats", Current.Stats.PartitionStats);
                            key.SetValue("MediaStats", Current.Stats.MediaStats);
                            key.SetValue("VerifyStats", Current.Stats.VerifyStats);
                        }
                        else
                        {
                            key.SetValue("Statistics", true);
                            key.DeleteValue("ShareStats", false);
                            key.DeleteValue("CommandStats", false);
                            key.DeleteValue("DeviceStats", false);
                            key.DeleteValue("FilesystemStats", false);
                            key.DeleteValue("MediaImageStats", false);
                            key.DeleteValue("MediaScanStats", false);
                            key.DeleteValue("PartitionStats", false);
                            key.DeleteValue("MediaStats", false);
                            key.DeleteValue("VerifyStats", false);
                        }
                    }
                }

                    break;
            #endif

                // Otherwise, settings will be saved in ~/.config/Aaru.xml
                default:
                {
                    string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                    string xdgConfigPath =
                        Path.Combine(homePath,
                                     Environment.GetEnvironmentVariable(XDG_CONFIG_HOME) ?? XDG_CONFIG_HOME_RESOLVED);

                    string settingsPath = Path.Combine(xdgConfigPath, "Aaru.xml");

                    if(!Directory.Exists(xdgConfigPath))
                        Directory.CreateDirectory(xdgConfigPath);

                    var fs = new FileStream(settingsPath, FileMode.Create);
                    var xs = new XmlSerializer(Current.GetType());
                    xs.Serialize(fs, Current);
                    fs.Close();
                }

                    break;
            }
        }
        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
        catch
        {
            // ignored
        }
        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
    }

    /// <summary>Sets default settings as all statistics, share everything</summary>
    static void SetDefaultSettings() => Current = new DicSettings
    {
        SaveReportsGlobally = true,
        ShareReports        = true,
        GdprCompliance      = 0,
        EnableDecryption    = true,
        Stats = new StatsSettings
        {
            CommandStats    = true,
            DeviceStats     = true,
            FilesystemStats = true,
            MediaImageStats = true,
            MediaScanStats  = true,
            FilterStats     = true,
            MediaStats      = true,
            PartitionStats  = true,
            ShareStats      = true,
            VerifyStats     = true
        }
    };
}