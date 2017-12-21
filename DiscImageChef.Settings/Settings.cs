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
    public class DicSettings
    {
        public StatsSettings Stats;
        public bool SaveReportsGlobally;
        public bool ShareReports;
    }

    public class UserSettings
    {
        public string Name;
        public string Email;
    }

    public class StatsSettings
    {
        public bool ShareStats;
        public bool BenchmarkStats;
        public bool CommandStats;
        public bool DeviceStats;
        public bool FilesystemStats;
        public bool FilterStats;
        public bool MediaImageStats;
        public bool MediaScanStats;
        public bool PartitionStats;
        public bool MediaStats;
        public bool VerifyStats;
    }

    public static class Settings
    {
        public static DicSettings Current;

        static string ReportsPath { get; set; }

        public static string StatsPath { get; private set; }

        public static void LoadSettings()
        {
            Current = new DicSettings();
            PlatformID ptID = DetectOS.GetRealPlatformID();

            try
            {
                switch(ptID)
                {
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
                switch(ptID)
                {
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
                            NSObject obj;

                            if(parsedPreferences.TryGetValue("SaveReportsGlobally", out obj)) Current.SaveReportsGlobally = ((NSNumber)obj).ToBool();
                            else Current.SaveReportsGlobally = false;

                            if(parsedPreferences.TryGetValue("ShareReports", out obj)) Current.ShareReports = ((NSNumber)obj).ToBool();
                            else Current.ShareReports = false;

                            NSDictionary stats;
                            if(parsedPreferences.TryGetValue("Stats", out obj))
                            {
                                stats = (NSDictionary)obj;

                                if(stats != null)
                                {
                                    NSObject obj2;
                                    Current.Stats = new StatsSettings();

                                    if(stats.TryGetValue("ShareStats", out obj2)) Current.Stats.ShareStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.ShareStats = false;

                                    if(stats.TryGetValue("BenchmarkStats", out obj2)) Current.Stats.BenchmarkStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.BenchmarkStats = false;

                                    if(stats.TryGetValue("CommandStats", out obj2)) Current.Stats.CommandStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.CommandStats = false;

                                    if(stats.TryGetValue("DeviceStats", out obj2)) Current.Stats.DeviceStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.DeviceStats = false;

                                    if(stats.TryGetValue("FilesystemStats", out obj2)) Current.Stats.FilesystemStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.FilesystemStats = false;

                                    if(stats.TryGetValue("FilterStats", out obj2)) Current.Stats.FilterStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.FilterStats = false;

                                    if(stats.TryGetValue("MediaImageStats", out obj2)) Current.Stats.MediaImageStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.MediaImageStats = false;

                                    if(stats.TryGetValue("MediaScanStats", out obj2)) Current.Stats.MediaScanStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.MediaScanStats = false;

                                    if(stats.TryGetValue("PartitionStats", out obj2)) Current.Stats.PartitionStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.PartitionStats = false;

                                    if(stats.TryGetValue("MediaStats", out obj2)) Current.Stats.MediaStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.MediaStats = false;

                                    if(stats.TryGetValue("VerifyStats", out obj2)) Current.Stats.VerifyStats = ((NSNumber)obj2).ToBool();
                                    else Current.Stats.VerifyStats = false;
                                }
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
                        {
                            Current.Stats = new StatsSettings();
                            Current.Stats.ShareStats = Convert.ToBoolean(key.GetValue("ShareStats"));
                            Current.Stats.BenchmarkStats = Convert.ToBoolean(key.GetValue("BenchmarkStats"));
                            Current.Stats.CommandStats = Convert.ToBoolean(key.GetValue("CommandStats"));
                            Current.Stats.DeviceStats = Convert.ToBoolean(key.GetValue("DeviceStats"));
                            Current.Stats.FilesystemStats = Convert.ToBoolean(key.GetValue("FilesystemStats"));
                            Current.Stats.FilterStats = Convert.ToBoolean(key.GetValue("FilterStats"));
                            Current.Stats.MediaImageStats = Convert.ToBoolean(key.GetValue("MediaImageStats"));
                            Current.Stats.MediaScanStats = Convert.ToBoolean(key.GetValue("MediaScanStats"));
                            Current.Stats.PartitionStats = Convert.ToBoolean(key.GetValue("PartitionStats"));
                            Current.Stats.MediaStats = Convert.ToBoolean(key.GetValue("MediaStats"));
                            Current.Stats.VerifyStats = Convert.ToBoolean(key.GetValue("VerifyStats"));
                        }
                    }

                        break;
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
                if(prefsFs != null) prefsFs.Close();
                if(prefsSr != null) prefsSr.Close();
                SetDefaultSettings();
                SaveSettings();
            }
        }

        public static void SaveSettings()
        {
            try
            {
                PlatformID ptID = DetectOS.GetRealPlatformID();

                switch(ptID)
                {
                    case PlatformID.MacOSX:
                    case PlatformID.iOS:
                    {
                        NSDictionary root = new NSDictionary();
                        root.Add("SaveReportsGlobally", Current.SaveReportsGlobally);
                        root.Add("ShareReports", Current.ShareReports);
                        if(Current.Stats != null)
                        {
                            NSDictionary stats = new NSDictionary();
                            stats.Add("ShareStats", Current.Stats.ShareStats);
                            stats.Add("BenchmarkStats", Current.Stats.BenchmarkStats);
                            stats.Add("CommandStats", Current.Stats.CommandStats);
                            stats.Add("DeviceStats", Current.Stats.DeviceStats);
                            stats.Add("FilesystemStats", Current.Stats.FilesystemStats);
                            stats.Add("FilterStats", Current.Stats.FilterStats);
                            stats.Add("MediaImageStats", Current.Stats.MediaImageStats);
                            stats.Add("MediaScanStats", Current.Stats.MediaScanStats);
                            stats.Add("PartitionStats", Current.Stats.PartitionStats);
                            stats.Add("MediaStats", Current.Stats.MediaStats);
                            stats.Add("VerifyStats", Current.Stats.VerifyStats);
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
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body 
        }

        public static void SetDefaultSettings()
        {
            Current = new DicSettings();
            Current.SaveReportsGlobally = true;
            Current.ShareReports = true;
            Current.Stats = new StatsSettings();
            Current.Stats.BenchmarkStats = true;
            Current.Stats.CommandStats = true;
            Current.Stats.DeviceStats = true;
            Current.Stats.FilesystemStats = true;
            Current.Stats.MediaImageStats = true;
            Current.Stats.MediaScanStats = true;
            Current.Stats.FilterStats = true;
            Current.Stats.MediaStats = true;
            Current.Stats.PartitionStats = true;
            Current.Stats.ShareStats = true;
            Current.Stats.VerifyStats = true;
        }
    }
}