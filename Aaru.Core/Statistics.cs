// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles usage statistics.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Database;
using Aaru.Database.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Device = Aaru.Devices.Device;
using MediaType = Aaru.CommonTypes.MediaType;
using OperatingSystem = Aaru.Database.Models.OperatingSystem;
using Version = Aaru.Database.Models.Version;

namespace Aaru.Core;

/// <summary>Handles anonymous usage statistics</summary>
public static class Statistics
{
    const string MODULE_NAME = "Stats";
    /// <summary>Statistics file semaphore</summary>
    static bool _submitStatsLock;

    /// <summary>Loads saved statistics from disk</summary>
    public static void LoadStats()
    {
        try
        {
            using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            if(Settings.Settings.Current.Stats == null)
                return;

            ctx.OperatingSystems.Add(new OperatingSystem
            {
                Name         = DetectOS.GetRealPlatformID().ToString(),
                Synchronized = false,
                Version      = DetectOS.GetVersion(),
                Count        = 1
            });

            ctx.Versions.Add(new Version
            {
                Name         = CommonTypes.Interop.Version.GetVersion(),
                Synchronized = false,
                Count        = 1
            });

            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Saves statistics to disk</summary>
    public static void SaveStats()
    {
        try
        {
            using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }

        if(Settings.Settings.Current.Stats is { ShareStats: true })
            SubmitStats();
    }

    /// <summary>Submits statistics to Aaru.Server</summary>
    static void SubmitStats()
    {
        var submitThread = new Thread(() =>
        {
            using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            try
            {
                if(_submitStatsLock)
                    return;

                _submitStatsLock = true;

                if(ctx.Commands.Any(c => !c.Synchronized)            ||
                   ctx.Filesystems.Any(c => !c.Synchronized)         ||
                   ctx.Filters.Any(c => !c.Synchronized)             ||
                   ctx.MediaFormats.Any(c => !c.Synchronized)        ||
                   ctx.Partitions.Any(c => !c.Synchronized)          ||
                   ctx.Medias.Any(c => !c.Synchronized)              ||
                   ctx.SeenDevices.Any(c => !c.Synchronized)         ||
                   ctx.OperatingSystems.Any(c => !c.Synchronized)    ||
                   ctx.Versions.Any(c => !c.Synchronized)            ||
                   ctx.RemoteApplications.Any(c => !c.Synchronized)  ||
                   ctx.RemoteArchitectures.Any(c => !c.Synchronized) ||
                   ctx.RemoteOperatingSystems.Any(c => !c.Synchronized))
                {
                    var dto = new StatsDto();

                    if(ctx.Commands.Any(c => !c.Synchronized))
                    {
                        dto.Commands = new List<NameValueStats>();

                        foreach(string nvs in ctx.Commands.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            dto.Commands.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Commands.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.Filesystems.Any(c => !c.Synchronized))
                    {
                        dto.Filesystems = new List<NameValueStats>();

                        foreach(string nvs in ctx.Filesystems.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            dto.Filesystems.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Filesystems.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.Filters.Any(c => !c.Synchronized))
                    {
                        dto.Filters = new List<NameValueStats>();

                        foreach(string nvs in ctx.Filters.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            dto.Filters.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Filters.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.MediaFormats.Any(c => !c.Synchronized))
                    {
                        dto.MediaFormats = new List<NameValueStats>();

                        foreach(string nvs in ctx.MediaFormats.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            dto.MediaFormats.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.MediaFormats.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.Partitions.Any(c => !c.Synchronized))
                    {
                        dto.Partitions = new List<NameValueStats>();

                        foreach(string nvs in ctx.Partitions.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            dto.Partitions.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Partitions.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.Versions.Any(c => !c.Synchronized))
                    {
                        dto.Versions = new List<NameValueStats>();

                        foreach(string nvs in ctx.Versions.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            dto.Versions.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Versions.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.Medias.Any(c => !c.Synchronized))
                    {
                        dto.Medias = new List<MediaStats>();

                        foreach(string media in ctx.Medias.Where(c => !c.Synchronized).Select(c => c.Type).Distinct())
                        {
                            if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && c.Real))
                            {
                                dto.Medias.Add(new MediaStats
                                {
                                    real      = true,
                                    MediaType = media,
                                    Value     = ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && c.Real)
                                });
                            }

                            if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && !c.Real))
                            {
                                dto.Medias.Add(new MediaStats
                                {
                                    real      = false,
                                    MediaType = media,
                                    Value     = ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && !c.Real)
                                });
                            }
                        }
                    }

                    if(ctx.SeenDevices.Any(c => !c.Synchronized))
                    {
                        dto.Devices = new List<DeviceStats>();

                        foreach(DeviceStat device in ctx.SeenDevices.Where(c => !c.Synchronized))
                        {
                            dto.Devices.Add(new DeviceStats
                            {
                                Bus                   = device.Bus,
                                Manufacturer          = device.Manufacturer,
                                ManufacturerSpecified = device.Manufacturer is not null,
                                Model                 = device.Model,
                                Revision              = device.Revision
                            });
                        }
                    }

                    if(ctx.OperatingSystems.Any(c => !c.Synchronized))
                    {
                        dto.OperatingSystems = new List<OsStats>();

                        foreach(string osName in ctx.OperatingSystems.Where(c => !c.Synchronized).
                                                     Select(c => c.Name).
                                                     Distinct())
                        {
                            foreach(string osVersion in ctx.OperatingSystems.
                                                            Where(c => !c.Synchronized && c.Name == osName).
                                                            Select(c => c.Version).
                                                            Distinct())
                            {
                                dto.OperatingSystems.Add(new OsStats
                                {
                                    name    = osName,
                                    version = osVersion,
                                    Value = ctx.OperatingSystems.LongCount(c => !c.Synchronized    &&
                                                                               c.Name    == osName &&
                                                                               c.Version == osVersion)
                                });
                            }
                        }
                    }

                    if(ctx.RemoteApplications.Any(c => !c.Synchronized))
                    {
                        dto.RemoteApplications = new List<OsStats>();

                        foreach(string remoteAppName in ctx.RemoteApplications.Where(c => !c.Synchronized).
                                                            Select(c => c.Name).
                                                            Distinct())
                        {
                            foreach(string remoteAppVersion in ctx.RemoteApplications.
                                                                   Where(c => !c.Synchronized &&
                                                                              c.Name == remoteAppName).
                                                                   Select(c => c.Version).
                                                                   Distinct())
                            {
                                dto.RemoteApplications.Add(new OsStats
                                {
                                    name    = remoteAppName,
                                    version = remoteAppVersion,
                                    Value = ctx.RemoteApplications.LongCount(c => !c.Synchronized           &&
                                                                                 c.Name    == remoteAppName &&
                                                                                 c.Version == remoteAppVersion)
                                });
                            }
                        }
                    }

                    if(ctx.RemoteArchitectures.Any(c => !c.Synchronized))
                    {
                        dto.RemoteArchitectures = new List<NameValueStats>();

                        foreach(string nvs in ctx.RemoteArchitectures.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            dto.RemoteArchitectures.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.RemoteArchitectures.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                        }
                    }

                    if(ctx.RemoteOperatingSystems.Any(c => !c.Synchronized))
                    {
                        dto.RemoteOperatingSystems = new List<OsStats>();

                        foreach(string remoteOsName in ctx.RemoteOperatingSystems.Where(c => !c.Synchronized).
                                                           Select(c => c.Name).
                                                           Distinct())
                        {
                            foreach(string remoteOsVersion in ctx.RemoteOperatingSystems.
                                                                  Where(c => !c.Synchronized && c.Name == remoteOsName).
                                                                  Select(c => c.Version).
                                                                  Distinct())
                            {
                                dto.RemoteOperatingSystems.Add(new OsStats
                                {
                                    name    = remoteOsName,
                                    version = remoteOsVersion,
                                    Value = ctx.RemoteOperatingSystems.LongCount(c => !c.Synchronized &&
                                        c.Name    == remoteOsName                                     &&
                                        c.Version == remoteOsVersion)
                                });
                            }
                        }
                    }

                #if DEBUG
                    System.Console.WriteLine(Localization.Core.Uploading_statistics);
                #else
                            Aaru.Console.AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Uploading_statistics);
                #endif
                    string json = JsonSerializer.Serialize(dto, typeof(StatsDto), StatsDtoContext.Default);

                    byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                    var    request   = WebRequest.Create("https://www.aaru.app/api/uploadstatsv2");

                    ((HttpWebRequest)request).UserAgent = $"Aaru {typeof(Version).Assembly.GetName().Version}";

                    request.Method        = "POST";
                    request.ContentLength = jsonBytes.Length;
                    request.ContentType   = "application/json";
                    Stream reqStream = request.GetRequestStream();
                    reqStream.Write(jsonBytes, 0, jsonBytes.Length);

                    //jsonStream.CopyTo(reqStream);
                    reqStream.Close();
                    WebResponse response = request.GetResponse();

                    if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
                        return;

                    Stream data   = response.GetResponseStream();
                    var    reader = new StreamReader(data ?? throw new InvalidOperationException());

                    string result = reader.ReadToEnd();
                    data.Close();
                    response.Close();

                    if(result != "ok")
                        return;

                    if(ctx.Commands.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.Commands.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            Command existing = ctx.Commands.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                               new Command
                                               {
                                                   Name         = nvs,
                                                   Synchronized = true
                                               };

                            existing.Count += (ulong)ctx.Commands.LongCount(c => !c.Synchronized && c.Name == nvs);
                            ctx.Commands.Update(existing);
                            ctx.Commands.RemoveRange(ctx.Commands.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.Filesystems.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.Filesystems.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            Filesystem existing =
                                ctx.Filesystems.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                new Filesystem
                                {
                                    Name         = nvs,
                                    Synchronized = true
                                };

                            existing.Count += (ulong)ctx.Filesystems.LongCount(c => !c.Synchronized && c.Name == nvs);

                            ctx.Filesystems.Update(existing);

                            ctx.Filesystems.RemoveRange(ctx.Filesystems.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.Filters.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.Filters.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            Filter existing = ctx.Filters.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                              new Filter
                                              {
                                                  Name         = nvs,
                                                  Synchronized = true
                                              };

                            existing.Count += (ulong)ctx.Filters.LongCount(c => !c.Synchronized && c.Name == nvs);
                            ctx.Filters.Update(existing);
                            ctx.Filters.RemoveRange(ctx.Filters.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.MediaFormats.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.MediaFormats.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            MediaFormat existing =
                                ctx.MediaFormats.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                new MediaFormat
                                {
                                    Name         = nvs,
                                    Synchronized = true
                                };

                            existing.Count += (ulong)ctx.MediaFormats.LongCount(c => !c.Synchronized && c.Name == nvs);

                            ctx.MediaFormats.Update(existing);

                            ctx.MediaFormats.RemoveRange(ctx.MediaFormats.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.Partitions.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.Partitions.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            Partition existing = ctx.Partitions.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                                 new Partition
                                                 {
                                                     Name         = nvs,
                                                     Synchronized = true
                                                 };

                            existing.Count += (ulong)ctx.Partitions.LongCount(c => !c.Synchronized && c.Name == nvs);

                            ctx.Partitions.Update(existing);
                            ctx.Partitions.RemoveRange(ctx.Partitions.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.Versions.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.Versions.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                        {
                            Version existing = ctx.Versions.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                               new Version
                                               {
                                                   Name         = nvs,
                                                   Synchronized = true
                                               };

                            existing.Count += (ulong)ctx.Versions.LongCount(c => !c.Synchronized && c.Name == nvs);
                            ctx.Versions.Update(existing);
                            ctx.Versions.RemoveRange(ctx.Versions.Where(c => !c.Synchronized && c.Name == nvs));
                        }
                    }

                    if(ctx.Medias.Any(c => !c.Synchronized))
                    {
                        foreach(string media in ctx.Medias.Where(c => !c.Synchronized).Select(c => c.Type).Distinct())
                        {
                            if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && c.Real))
                            {
                                Database.Models.Media existing =
                                    ctx.Medias.FirstOrDefault(c => c.Synchronized && c.Type == media && c.Real) ??
                                    new Database.Models.Media
                                    {
                                        Synchronized = true,
                                        Type         = media,
                                        Real         = true
                                    };

                                existing.Count +=
                                    (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && c.Real);

                                ctx.Medias.Update(existing);

                                ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized &&
                                                                             c.Type == media &&
                                                                             c.Real));
                            }

                            if(!ctx.Medias.Any(c => !c.Synchronized && c.Type == media && !c.Real))
                                continue;

                            {
                                Database.Models.Media existing =
                                    ctx.Medias.FirstOrDefault(c => c.Synchronized && c.Type == media && !c.Real) ??
                                    new Database.Models.Media
                                    {
                                        Synchronized = true,
                                        Type         = media,
                                        Real         = false
                                    };

                                existing.Count +=
                                    (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && !c.Real);

                                ctx.Medias.Update(existing);

                                ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized &&
                                                                             c.Type == media &&
                                                                             !c.Real));
                            }
                        }
                    }

                    if(ctx.SeenDevices.Any(c => !c.Synchronized))
                    {
                        foreach(DeviceStat device in ctx.SeenDevices.Where(c => !c.Synchronized))
                        {
                            device.Synchronized = true;
                            ctx.Update(device);
                        }
                    }

                    if(ctx.OperatingSystems.Any(c => !c.Synchronized))
                    {
                        foreach(string osName in ctx.OperatingSystems.Where(c => !c.Synchronized).
                                                     Select(c => c.Name).
                                                     Distinct())
                        {
                            foreach(string osVersion in ctx.OperatingSystems.
                                                            Where(c => !c.Synchronized && c.Name == osName).
                                                            Select(c => c.Version).
                                                            Distinct())
                            {
                                OperatingSystem existing =
                                    ctx.OperatingSystems.FirstOrDefault(c => c.Synchronized      &&
                                                                             c.Name    == osName &&
                                                                             c.Version == osVersion) ??
                                    new OperatingSystem
                                    {
                                        Synchronized = true,
                                        Version      = osVersion,
                                        Name         = osName
                                    };

                                existing.Count +=
                                    (ulong)ctx.OperatingSystems.LongCount(c => !c.Synchronized     &&
                                                                               c.Name    == osName &&
                                                                               c.Version == osVersion);

                                ctx.OperatingSystems.Update(existing);

                                ctx.OperatingSystems.RemoveRange(ctx.OperatingSystems.Where(c => !c.Synchronized &&
                                                                     c.Name    == osName                         &&
                                                                     c.Version == osVersion));
                            }
                        }
                    }

                    if(ctx.RemoteApplications.Any(c => !c.Synchronized))
                    {
                        foreach(string remoteAppName in ctx.RemoteApplications.Where(c => !c.Synchronized).
                                                            Select(c => c.Name).
                                                            Distinct())
                        {
                            foreach(string remoteAppVersion in ctx.RemoteApplications.
                                                                   Where(c => !c.Synchronized &&
                                                                              c.Name == remoteAppName).
                                                                   Select(c => c.Version).
                                                                   Distinct())
                            {
                                RemoteApplication existing =
                                    ctx.RemoteApplications.FirstOrDefault(c => c.Synchronized             &&
                                                                               c.Name    == remoteAppName &&
                                                                               c.Version == remoteAppVersion) ??
                                    new RemoteApplication
                                    {
                                        Synchronized = true,
                                        Version      = remoteAppVersion,
                                        Name         = remoteAppName
                                    };

                                existing.Count +=
                                    (ulong)ctx.RemoteApplications.LongCount(c => !c.Synchronized           &&
                                                                                c.Name    == remoteAppName &&
                                                                                c.Version == remoteAppVersion);

                                ctx.RemoteApplications.Update(existing);

                                ctx.RemoteApplications.RemoveRange(ctx.RemoteApplications.Where(c => !c.Synchronized &&
                                                                       c.Name    == remoteAppName                    &&
                                                                       c.Version == remoteAppVersion));
                            }
                        }
                    }

                    if(ctx.RemoteArchitectures.Any(c => !c.Synchronized))
                    {
                        foreach(string nvs in ctx.RemoteArchitectures.Where(c => !c.Synchronized).
                                                  Select(c => c.Name).
                                                  Distinct())
                        {
                            RemoteArchitecture existing =
                                ctx.RemoteArchitectures.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                new RemoteArchitecture
                                {
                                    Name         = nvs,
                                    Synchronized = true
                                };

                            existing.Count +=
                                (ulong)ctx.RemoteArchitectures.LongCount(c => !c.Synchronized && c.Name == nvs);

                            ctx.RemoteArchitectures.Update(existing);

                            ctx.RemoteArchitectures.RemoveRange(ctx.RemoteArchitectures.Where(c => !c.Synchronized &&
                                                                    c.Name == nvs));
                        }
                    }

                    foreach(string remoteOsName in ctx.RemoteOperatingSystems.Where(c => !c.Synchronized).
                                                       Select(c => c.Name).
                                                       Distinct())
                    {
                        foreach(string remoteOsVersion in ctx.RemoteOperatingSystems.
                                                              Where(c => !c.Synchronized && c.Name == remoteOsName).
                                                              Select(c => c.Version).
                                                              Distinct())
                        {
                            RemoteOperatingSystem existing =
                                ctx.RemoteOperatingSystems.FirstOrDefault(c => c.Synchronized            &&
                                                                               c.Name    == remoteOsName &&
                                                                               c.Version == remoteOsVersion) ??
                                new RemoteOperatingSystem
                                {
                                    Synchronized = true,
                                    Version      = remoteOsVersion,
                                    Name         = remoteOsName
                                };

                            existing.Count +=
                                (ulong)ctx.RemoteOperatingSystems.LongCount(c => !c.Synchronized          &&
                                                                                c.Name    == remoteOsName &&
                                                                                c.Version == remoteOsVersion);

                            ctx.RemoteOperatingSystems.Update(existing);

                            ctx.RemoteOperatingSystems.RemoveRange(ctx.RemoteOperatingSystems.Where(c =>
                                                                       !c.Synchronized           &&
                                                                       c.Name    == remoteOsName &&
                                                                       c.Version == remoteOsVersion));
                        }
                    }

                    ctx.SaveChanges();
                }
            }
            catch(WebException)
            {
                // Can't connect to the server, do nothing
            }
            catch(DbUpdateConcurrencyException)
            {
                // Ignore db concurrency errors
            }

            // ReSharper disable once RedundantCatchClause
            catch
            {
            #if DEBUG
                _submitStatsLock = false;

                if(Debugger.IsAttached)
                    throw;
            #endif
            }

            _submitStatsLock = false;
        });

        submitThread.Start();
    }

    /// <summary>Adds the execution of a command to statistics</summary>
    /// <param name="command">Command</param>
    public static void AddCommand(string command)
    {
        if(string.IsNullOrWhiteSpace(command))
            return;

        if(Settings.Settings.Current.Stats is not { DeviceStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.Commands.Add(new Command
        {
            Name         = command,
            Synchronized = false,
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new filesystem to statistics</summary>
    /// <param name="filesystem">Filesystem name</param>
    public static void AddFilesystem(string filesystem)
    {
        if(string.IsNullOrWhiteSpace(filesystem))
            return;

        if(Settings.Settings.Current.Stats is not { FilesystemStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.Filesystems.Add(new Filesystem
        {
            Name         = filesystem,
            Synchronized = false,
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new partition scheme to statistics</summary>
    /// <param name="partition">Partition scheme name</param>
    internal static void AddPartition(string partition)
    {
        if(string.IsNullOrWhiteSpace(partition))
            return;

        if(Settings.Settings.Current.Stats is not { PartitionStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.Partitions.Add(new Partition
        {
            Name         = partition,
            Synchronized = false,
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new filter to statistics</summary>
    /// <param name="filter">Filter name</param>
    public static void AddFilter(string filter)
    {
        if(string.IsNullOrWhiteSpace(filter))
            return;

        if(Settings.Settings.Current.Stats is not { FilterStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.Filters.Add(new Filter
        {
            Name         = filter,
            Synchronized = false,
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Ads a new media image to statistics</summary>
    /// <param name="format">Media image name</param>
    public static void AddMediaFormat(string format)
    {
        if(string.IsNullOrWhiteSpace(format))
            return;

        if(Settings.Settings.Current.Stats is not { MediaImageStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.MediaFormats.Add(new MediaFormat
        {
            Name         = format,
            Synchronized = false,
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new device to statistics</summary>
    /// <param name="dev">Device</param>
    public static void AddDevice(Device dev)
    {
        if(Settings.Settings.Current.Stats is not { DeviceStats: true })
            return;

        string deviceBus;

        if(dev.IsUsb)
            deviceBus = "USB";
        else if(dev.IsFireWire)
            deviceBus = "FireWire";
        else
            deviceBus = dev.Type.ToString();

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        if(ctx.SeenDevices.Any(d => d.Manufacturer == dev.Manufacturer     &&
                                    d.Model        == dev.Model            &&
                                    d.Revision     == dev.FirmwareRevision &&
                                    d.Bus          == deviceBus))
            return;

        ctx.SeenDevices.Add(new DeviceStat
        {
            Bus          = deviceBus,
            Manufacturer = dev.Manufacturer,
            Model        = dev.Model,
            Revision     = dev.FirmwareRevision,
            Synchronized = false
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new media type to statistics</summary>
    /// <param name="type">Media type</param>
    /// <param name="real">Set if media was found on a real device, otherwise found on a media image</param>
    public static void AddMedia(MediaType type, bool real)
    {
        if(Settings.Settings.Current.Stats is not { MediaStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.Medias.Add(new Database.Models.Media
        {
            Real         = real,
            Synchronized = false,
            Type         = type.ToString(),
            Count        = 1
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }

    /// <summary>Adds a new remote to statistics</summary>
    public static void AddRemote(string serverApplication, string serverVersion, string serverOperatingSystem,
                                 string serverOperatingSystemVersion, string serverArchitecture)
    {
        if(Settings.Settings.Current.Stats is not { MediaStats: true })
            return;

        using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        ctx.RemoteApplications.Add(new RemoteApplication
        {
            Count        = 1,
            Name         = serverApplication,
            Synchronized = false,
            Version      = serverVersion
        });

        ctx.RemoteArchitectures.Add(new RemoteArchitecture
        {
            Count        = 1,
            Name         = serverArchitecture,
            Synchronized = false
        });

        ctx.RemoteOperatingSystems.Add(new RemoteOperatingSystem
        {
            Count        = 1,
            Name         = serverOperatingSystem,
            Synchronized = false,
            Version      = serverOperatingSystemVersion
        });

        try
        {
            ctx.SaveChanges();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.DebugWriteLine(MODULE_NAME, "{0}", ex);
        }
    }
}