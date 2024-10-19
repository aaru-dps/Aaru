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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

            if(Settings.Settings.Current.Stats == null) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Saves statistics to disk</summary>
    public static async Task SaveStatsAsync()
    {
        try
        {
            await using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

            await ctx.SaveChangesAsync();
        }
        catch(SqliteException ex)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Exception_while_trying_to_save_statistics);
            AaruConsole.WriteException(ex);
        }

        if(Settings.Settings.Current.Stats is { ShareStats: true }) await SubmitStatsAsync();
    }

    /// <summary>Submits statistics to Aaru.Server</summary>
    static async Task SubmitStatsAsync()
    {
        await using var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);

        try
        {
            if(_submitStatsLock) return;

            _submitStatsLock = true;
            var dto = new StatsDto();

            AddStats(ctx.Commands, out List<NameValueStats> nameValueStats);

            if(nameValueStats?.Count > 0) dto.Commands = nameValueStats;

            AddStats(ctx.Filesystems, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.Filesystems = nameValueStats;

            AddStats(ctx.Filters, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.Filters = nameValueStats;

            AddStats(ctx.MediaFormats, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.MediaFormats = nameValueStats;

            AddStats(ctx.Partitions, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.Partitions = nameValueStats;

            AddStats(ctx.Versions, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.Versions = nameValueStats;
            dto.Versions = [];

            if(ctx.Medias.Any(c => !c.Synchronized))
            {
                dto.Medias = [];

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
                dto.Devices = [];

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

            AddOperatingSystem(ctx.OperatingSystems, out List<OsStats> osStats);
            if(nameValueStats?.Count > 0) dto.OperatingSystems = osStats;
            dto.OperatingSystems = [];

            AddOperatingSystem(ctx.RemoteApplications, out osStats);
            if(nameValueStats?.Count > 0) dto.RemoteApplications = osStats;
            dto.RemoteApplications = [];

            AddStats(ctx.RemoteArchitectures, out nameValueStats);

            if(nameValueStats?.Count > 0) dto.RemoteArchitectures = nameValueStats;
            dto.RemoteArchitectures = [];

            AddOperatingSystem(ctx.RemoteOperatingSystems, out osStats);
            if(nameValueStats?.Count > 0) dto.RemoteOperatingSystems = osStats;
            dto.RemoteOperatingSystems = [];

#if DEBUG
            System.Console.WriteLine(Localization.Core.Uploading_statistics);
#else
                Aaru.Console.AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Uploading_statistics);
#endif
            using StringContent jsonContent =
                new(JsonSerializer.Serialize(dto, typeof(StatsDto), StatsDtoContext.Default),
                    Encoding.UTF8,
                    "application/json");

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://www.aaru.app");
            client.DefaultRequestHeaders.Add("User-Agent", $"Aaru {typeof(Version).Assembly.GetName().Version}");

            using HttpResponseMessage response = await client.PostAsync("/api/uploadstatsv2", jsonContent);

            if(response.StatusCode != HttpStatusCode.OK) return;

            string result = await response.Content.ReadAsStringAsync();

            if(result != "ok") return;

            await UpdateStatsAsync(ctx.Commands);
            await UpdateStatsAsync(ctx.Filesystems);
            await UpdateStatsAsync(ctx.Filters);
            await UpdateStatsAsync(ctx.MediaFormats);
            await UpdateStatsAsync(ctx.Partitions);
            await UpdateStatsAsync(ctx.Versions);

            if(ctx.Medias.Any(c => !c.Synchronized))
            {
                foreach(string media in ctx.Medias.Where(c => !c.Synchronized).Select(c => c.Type).Distinct())
                {
                    if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && c.Real))
                    {
                        Database.Models.Media existing =
                            await ctx.Medias.FirstOrDefaultAsync(c => c.Synchronized && c.Type == media && c.Real) ??
                            new Database.Models.Media
                            {
                                Synchronized = true,
                                Type         = media,
                                Real         = true
                            };

                        existing.Count +=
                            (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && c.Real);

                        ctx.Medias.Update(existing);

                        ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized && c.Type == media && c.Real));
                    }

                    if(!ctx.Medias.Any(c => !c.Synchronized && c.Type == media && !c.Real)) continue;

                    {
                        Database.Models.Media existing =
                            await ctx.Medias.FirstOrDefaultAsync(c => c.Synchronized && c.Type == media && !c.Real) ??
                            new Database.Models.Media
                            {
                                Synchronized = true,
                                Type         = media,
                                Real         = false
                            };

                        existing.Count +=
                            (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && !c.Real);

                        ctx.Medias.Update(existing);

                        ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized && c.Type == media && !c.Real));
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

            await UpdateOperatingSystemAsync(ctx.OperatingSystems);
            await UpdateOperatingSystemAsync(ctx.RemoteApplications);
            await UpdateStatsAsync(ctx.RemoteArchitectures);
            await UpdateOperatingSystemAsync(ctx.RemoteOperatingSystems);

            await ctx.SaveChangesAsync();
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

            if(Debugger.IsAttached) throw;
#endif
        }

        _submitStatsLock = false;
    }

    static async Task UpdateOperatingSystemAsync<T>(DbSet<T> source) where T : BaseOperatingSystem, new()
    {
        if(!source.Any(c => !c.Synchronized)) return;

        foreach(string name in source.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
        {
            foreach(string version in source.Where(c => !c.Synchronized && c.Name == name)
                                            .Select(c => c.Version)
                                            .Distinct())
            {
                T existing =
                    await source.FirstOrDefaultAsync(c => c.Synchronized && c.Name == name && c.Version == version) ??
                    new T
                    {
                        Synchronized = true,
                        Version      = version,
                        Name         = name
                    };

                existing.Count +=
                    (ulong)source.LongCount(c => !c.Synchronized && c.Name == name && c.Version == version);

                source.Update(existing);

                source.RemoveRange(source.Where(c => !c.Synchronized && c.Name == name && c.Version == version));
            }
        }
    }

    static async Task UpdateStatsAsync<T>(DbSet<T> source) where T : NameCountModel, new()
    {
        if(!source.Any(c => !c.Synchronized)) return;

        foreach(string nvs in source.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
        {
            T existing = await source.FirstOrDefaultAsync(c => c.Synchronized && c.Name == nvs) ??
                         new T
                         {
                             Name         = nvs,
                             Synchronized = true
                         };

            existing.Count += (ulong)source.LongCount(c => !c.Synchronized && c.Name == nvs);
            source.Update(existing);
            source.RemoveRange(source.Where(c => !c.Synchronized && c.Name == nvs));
        }
    }

    static void AddOperatingSystem(IQueryable<BaseOperatingSystem> source, out List<OsStats> destination)
    {
        destination = [];

        foreach(string remoteOsName in source.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
        {
            foreach(string remoteOsVersion in source.Where(c => !c.Synchronized && c.Name == remoteOsName)
                                                    .Select(c => c.Version)
                                                    .Distinct())
            {
                destination.Add(new OsStats
                {
                    name    = remoteOsName,
                    version = remoteOsVersion,
                    Value = source.LongCount(c => !c.Synchronized           &&
                                                  c.Name    == remoteOsName &&
                                                  c.Version == remoteOsVersion)
                });
            }
        }
    }

    static void AddStats(IQueryable<NameCountModel> source, out List<NameValueStats> destination)
    {
        destination = [];

        if(!source.Any(c => !c.Synchronized)) return;

        foreach(string nvs in source.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
        {
            destination.Add(new NameValueStats
            {
                name  = nvs,
                Value = source.LongCount(c => !c.Synchronized && c.Name == nvs)
            });
        }
    }

    /// <summary>Adds the execution of a command to statistics</summary>
    /// <param name="command">Command</param>
    public static void AddCommand(string command)
    {
        if(string.IsNullOrWhiteSpace(command)) return;

        if(Settings.Settings.Current.Stats is not { DeviceStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new filesystem to statistics</summary>
    /// <param name="filesystem">Filesystem name</param>
    public static void AddFilesystem(string filesystem)
    {
        if(string.IsNullOrWhiteSpace(filesystem)) return;

        if(Settings.Settings.Current.Stats is not { FilesystemStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new partition scheme to statistics</summary>
    /// <param name="partition">Partition scheme name</param>
    internal static void AddPartition(string partition)
    {
        if(string.IsNullOrWhiteSpace(partition)) return;

        if(Settings.Settings.Current.Stats is not { PartitionStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new filter to statistics</summary>
    /// <param name="filter">Filter name</param>
    public static void AddFilter(string filter)
    {
        if(string.IsNullOrWhiteSpace(filter)) return;

        if(Settings.Settings.Current.Stats is not { FilterStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Ads a new media image to statistics</summary>
    /// <param name="format">Media image name</param>
    public static void AddMediaFormat(string format)
    {
        if(string.IsNullOrWhiteSpace(format)) return;

        if(Settings.Settings.Current.Stats is not { MediaImageStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new device to statistics</summary>
    /// <param name="dev">Device</param>
    public static void AddDevice(Device dev)
    {
        if(Settings.Settings.Current.Stats is not { DeviceStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new media type to statistics</summary>
    /// <param name="type">Media type</param>
    /// <param name="real">Set if media was found on a real device, otherwise found on a media image</param>
    public static void AddMedia(MediaType type, bool real)
    {
        if(Settings.Settings.Current.Stats is not { MediaStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }

    /// <summary>Adds a new remote to statistics</summary>
    public static void AddRemote(string serverApplication, string serverVersion, string serverOperatingSystem,
                                 string serverOperatingSystemVersion, string serverArchitecture)
    {
        if(Settings.Settings.Current.Stats is not { MediaStats: true }) return;

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
            AaruConsole.WriteException(ex);
        }
    }
}