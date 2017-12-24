// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Settings.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef settings.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Xml.Serialization;
using Claunia.PropertyList;
using DiscImageChef.Interop;
using Microsoft.Win32;
using PlatformID = DiscImageChef.Interop.PlatformID;

namespace DiscImageChef.Settings
{
    /// <summary>
    ///     Settings
    /// </summary>
    public class DicSettings
    {
        /// <summary>
        ///     If set to <c>true</c>, reports will be saved locally
        /// </summary>
        public bool SaveReportsGlobally;
        /// <summary>
        ///     If set to <c>true</c>, reports will be sent to DiscImageChef.Server
        /// </summary>
        public bool ShareReports;
        /// <summary>
        ///     Statistics
        /// </summary>
        public StatsSettings Stats;
    }

    // TODO: Use this
    /// <summary>
    ///     User settings, for media dumps, completely unused
    /// </summary>
    public class UserSettings
    {
        public string Email;
        public string Name;
    }

    /// <summary>
    ///     Statistics settings
    /// </summary>
    public class StatsSettings
    {
        /// <summary>
        ///     If set to <c>true</c>, benchmark statistics will be stored
        /// </summary>
        public bool BenchmarkStats;
        /// <summary>
        ///     If set to <c>true</c>, command usage statistics will be stored
        /// </summary>
        public bool CommandStats;
        /// <summary>
        ///     If set to <c>true</c>, device statistics will be stored
        /// </summary>
        public bool DeviceStats;
        /// <summary>
        ///     If set to <c>true</c>, filesystem statistics will be stored
        /// </summary>
        public bool FilesystemStats;
        /// <summary>
        ///     If set to <c>true</c>, filters statistics will be stored
        /// </summary>
        public bool FilterStats;
        /// <summary>
        ///     If set to <c>true</c>, dump media images statistics will be stored
        /// </summary>
        public bool MediaImageStats;
        /// <summary>
        ///     If set to <c>true</c>, media scan statistics will be stored
        /// </summary>
        public bool MediaScanStats;
        /// <summary>
        ///     If set to <c>true</c>, media types statistics will be stored
        /// </summary>
        public bool MediaStats;
        /// <summary>
        ///     If set to <c>true</c>, partition schemes statistics will be stored
        /// </summary>
        public bool PartitionStats;
        /// <summary>
        ///     If set to <c>true</c>, statistics will be sent to DiscImageChef.Server
        /// </summary>
        public bool ShareStats;
        /// <summary>
        ///     If set to <c>true</c>, dump media verification statistics will be stored
        /// </summary>
        public bool VerifyStats;
    }

    /// <summary>
    ///     Manages statistics
    /// </summary>
    public static class Settings
    {
        /// <summary>
        ///     Current statistcs
        /// </summary>
        public static DicSettings Current;

        /// <summary>
        ///     Global path to save reports
        /// </summary>
        static string ReportsPath { get; set; }

        /// <summary>
        ///     Global path to save statistics
        /// </summary>
        public static string StatsPath { get; private set; }

        /// <summary>
        ///     Loads settings
        /// </summary>
        public static void LoadSettings()
        {
            Current = new DicSettings();
            PlatformID ptId = DetectOS.GetRealPlatformID();

            try
            {
                switch(ptId)
                {
                    // In case of macOS or iOS statistics and reports will be saved in ~/Library/Application Support/Claunia.com/DiscImageChef
                    case PlatformID.MacOSX:
                    case PlatformID.iOS:
                    {
                        string appSupportPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                         "Application Support", "Claunia.com");
                        if(!Directory.Exists(appSupportPath)) Directory.CreateDirectory(appSupportPath);

                        string dicPath = Path.Combine(appSupportPath, "DiscImageChef");
                        if(!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);

                        ReportsPath = Path.Combine(dicPath, "Reports");
                        if(!Directory.Exists(ReportsPath)) Directory.CreateDirectory(ReportsPath);

                        StatsPath = Path.Combine(dicPath, "Statistics");
                        if(!Directory.Exists(StatsPath)) Directory.CreateDirectory(StatsPath);
                    }
                        break;
                    // In case of Windows statistics and reports will be saved in %APPDATA%\Claunia.com\DiscImageChef
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                    case PlatformID.WindowsPhone:
                    {
                        string appSupportPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                         "Claunia.com");
                        if(!Directory.Exists(appSupportPath)) Directory.CreateDirectory(appSupportPath);

                        string dicPath = Path.Combine(appSupportPath, "DiscImageChef");
                        if(!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);

                        ReportsPath = Path.Combine(dicPath, "Reports");
                        if(!Directory.Exists(ReportsPath)) Directory.CreateDirectory(ReportsPath);

                        StatsPath = Path.Combine(dicPath, "Statistics");
                        if(!Directory.Exists(StatsPath)) Directory.CreateDirectory(StatsPath);
                    }
                        break;
                    // Otherwise, statistics and reports will be saved in ~/.claunia.com/DiscImageChef
                    default:
                    {
                        string appSupportPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                         ".claunia.com");
                        if(!Directory.Exists(appSupportPath)) Directory.CreateDirectory(appSupportPath);

                        string dicPath = Path.Combine(appSupportPath, "DiscImageChef");
                        if(!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath);

                        ReportsPath = Path.Combine(dicPath, "Reports");
                        if(!Directory.Exists(ReportsPath)) Directory.CreateDirectory(ReportsPath);

                        StatsPath = Path.Combine(dicPath, "Statistics");
                        if(!Directory.Exists(StatsPath)) Directory.CreateDirectory(StatsPath);
                    }
                        break;
                }
            }
            catch { ReportsPath = null; }

            FileStream prefsFs = null;
            StreamReader prefsSr = null;

            try
            {
                switch(ptId)
                {
                    // In case of macOS or iOS settings will be saved in ~/Library/Preferences/com.claunia.discimagechef.plist
                    case PlatformID.MacOSX:
                    case PlatformID.iOS:
                    {
                        string preferencesPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                         "Preferences");
                        string preferencesFilePath = Path.Combine(preferencesPath, "com.claunia.discimagechef.plist");

                        if(!File.Exists(preferencesFilePath))
                        {
                            SetDefaultSettings();
                            SaveSettings();
                        }

                        prefsFs = new FileStream(preferencesFilePath, FileMode.Open, FileAccess.Read);

                        NSDictionary parsedPreferences = (NSDictionary)BinaryPropertyListParser.Parse(prefsFs);
                        if(parsedPreferences != null)
                        {
                            Current.SaveReportsGlobally =
                                parsedPreferences.TryGetValue("SaveReportsGlobally", out NSObject obj) &&
                                ((NSNumber)obj).ToBool();

                            Current.ShareReports = parsedPreferences.TryGetValue("ShareReports", out obj) &&
                                                   ((NSNumber)obj).ToBool();

                            if(parsedPreferences.TryGetValue("Stats", out obj))
                            {
                                NSDictionary stats = (NSDictionary)obj;

                                if(stats != null)
                                    Current.Stats = new StatsSettings
                                    {
                                        ShareStats =
                                            stats.TryGetValue("ShareStats", out NSObject obj2) &&
                                            ((NSNumber)obj2).ToBool(),
                                        BenchmarkStats =
                                            stats.TryGetValue("BenchmarkStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        CommandStats =
                                            stats.TryGetValue("CommandStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        DeviceStats =
                                            stats.TryGetValue("DeviceStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        FilesystemStats =
                                            stats.TryGetValue("FilesystemStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        FilterStats =
                                            stats.TryGetValue("FilterStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        MediaImageStats =
                                            stats.TryGetValue("MediaImageStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        MediaScanStats =
                                            stats.TryGetValue("MediaScanStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        PartitionStats =
                                            stats.TryGetValue("PartitionStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        MediaStats =
                                            stats.TryGetValue("MediaStats", out obj2) && ((NSNumber)obj2).ToBool(),
                                        VerifyStats =
                                            stats.TryGetValue("VerifyStats", out obj2) && ((NSNumber)obj2).ToBool()
                                    };
                            }
                            else Current.Stats = null;

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
                    // In case of Windows settings will be saved in the registry: HKLM/SOFTWARE/Claunia.com/DiscImageChef
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                    case PlatformID.WindowsPhone:
                    {
                        RegistryKey parentKey = Registry.CurrentUser.OpenSubKey("SOFTWARE")?.OpenSubKey("Claunia.com");
                        if(parentKey == null)
                        {
                            SetDefaultSettings();
                            SaveSettings();
                            return;
                        }

                        RegistryKey key = parentKey.OpenSubKey("DiscImageChef");
                        if(key == null)
                        {
                            SetDefaultSettings();
                            SaveSettings();
                            return;
                        }

                        Current.SaveReportsGlobally = Convert.ToBoolean(key.GetValue("SaveReportsGlobally"));
                        Current.ShareReports = Convert.ToBoolean(key.GetValue("ShareReports"));

                        bool stats = Convert.ToBoolean(key.GetValue("Statistics"));
                        if(stats)
                            Current.Stats = new StatsSettings
                            {
                                ShareStats = Convert.ToBoolean(key.GetValue("ShareStats")),
                                BenchmarkStats = Convert.ToBoolean(key.GetValue("BenchmarkStats")),
                                CommandStats = Convert.ToBoolean(key.GetValue("CommandStats")),
                                DeviceStats = Convert.ToBoolean(key.GetValue("DeviceStats")),
                                FilesystemStats = Convert.ToBoolean(key.GetValue("FilesystemStats")),
                                FilterStats = Convert.ToBoolean(key.GetValue("FilterStats")),
                                MediaImageStats = Convert.ToBoolean(key.GetValue("MediaImageStats")),
                                MediaScanStats = Convert.ToBoolean(key.GetValue("MediaScanStats")),
                                PartitionStats = Convert.ToBoolean(key.GetValue("PartitionStats")),
                                MediaStats = Convert.ToBoolean(key.GetValue("MediaStats")),
                                VerifyStats = Convert.ToBoolean(key.GetValue("VerifyStats"))
                            };
                    }

                        break;
                    // Otherwise, settings will be saved in ~/.config/DiscImageChef.xml
                    default:
                    {
                        string configPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                        string settingsPath = Path.Combine(configPath, "DiscImageChef.xml");

                        if(!Directory.Exists(configPath))
                        {
                            SetDefaultSettings();
                            SaveSettings();
                            return;
                        }

                        XmlSerializer xs = new XmlSerializer(Current.GetType());
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

        public static void SaveSettings()
        {
            try
            {
                PlatformID ptId = DetectOS.GetRealPlatformID();

                switch(ptId)
                {
                    // In case of macOS or iOS settings will be saved in ~/Library/Preferences/com.claunia.discimagechef.plist
                    case PlatformID.MacOSX:
                    case PlatformID.iOS:
                    {
                        NSDictionary root = new NSDictionary
                        {
                            {"SaveReportsGlobally", Current.SaveReportsGlobally},
                            {"ShareReports", Current.ShareReports}
                        };
                        if(Current.Stats != null)
                        {
                            NSDictionary stats = new NSDictionary
                            {
                                {"ShareStats", Current.Stats.ShareStats},
                                {"BenchmarkStats", Current.Stats.BenchmarkStats},
                                {"CommandStats", Current.Stats.CommandStats},
                                {"DeviceStats", Current.Stats.DeviceStats},
                                {"FilesystemStats", Current.Stats.FilesystemStats},
                                {"FilterStats", Current.Stats.FilterStats},
                                {"MediaImageStats", Current.Stats.MediaImageStats},
                                {"MediaScanStats", Current.Stats.MediaScanStats},
                                {"PartitionStats", Current.Stats.PartitionStats},
                                {"MediaStats", Current.Stats.MediaStats},
                                {"VerifyStats", Current.Stats.VerifyStats}
                            };
                            root.Add("Stats", stats);
                        }

                        string preferencesPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library",
                                         "Preferences");
                        string preferencesFilePath = Path.Combine(preferencesPath, "com.claunia.discimagechef.plist");

                        FileStream fs = new FileStream(preferencesFilePath, FileMode.Create);
                        BinaryPropertyListWriter.Write(fs, root);
                        fs.Close();
                    }
                        break;
                    // In case of Windows settings will be saved in the registry: HKLM/SOFTWARE/Claunia.com/DiscImageChef
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                    case PlatformID.WindowsPhone:
                    {
                        RegistryKey parentKey =
                            Registry.CurrentUser.OpenSubKey("SOFTWARE", true)?.CreateSubKey("Claunia.com");
                        RegistryKey key = parentKey?.CreateSubKey("DiscImageChef");

                        if(key != null)
                        {
                            key.SetValue("SaveReportsGlobally", Current.SaveReportsGlobally);
                            key.SetValue("ShareReports", Current.ShareReports);

                            if(Current.Stats != null)
                            {
                                key.SetValue("Statistics", true);
                                key.SetValue("ShareStats", Current.Stats.ShareStats);
                                key.SetValue("BenchmarkStats", Current.Stats.BenchmarkStats);
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
                                key.DeleteValue("BenchmarkStats", false);
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
                    // Otherwise, settings will be saved in ~/.config/DiscImageChef.xml
                    default:
                    {
                        string configPath =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                        string settingsPath = Path.Combine(configPath, "DiscImageChef.xml");

                        if(!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);

                        FileStream fs = new FileStream(settingsPath, FileMode.Create);
                        XmlSerializer xs = new XmlSerializer(Current.GetType());
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

        /// <summary>
        ///     Sets default settings as all statistics, share everything
        /// </summary>
        static void SetDefaultSettings()
        {
            Current = new DicSettings
            {
                SaveReportsGlobally = true,
                ShareReports = true,
                Stats = new StatsSettings
                {
                    BenchmarkStats = true,
                    CommandStats = true,
                    DeviceStats = true,
                    FilesystemStats = true,
                    MediaImageStats = true,
                    MediaScanStats = true,
                    FilterStats = true,
                    MediaStats = true,
                    PartitionStats = true,
                    ShareStats = true,
                    VerifyStats = true
                }
            };
        }
    }
}