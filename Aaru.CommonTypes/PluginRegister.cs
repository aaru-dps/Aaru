// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PluginRegister.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets lists of all known plugins.
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
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Aaru.CommonTypes;

public class PluginRegister
{
    static PluginRegister _instance;


    IServiceProvider   _serviceProvider;
    IServiceCollection _services;

    PluginRegister() {}

    /// <summary>List of byte addressable image plugins</summary>
    public SortedDictionary<string, IByteAddressableImage> ByteAddressableImages
    {
        get
        {
            SortedDictionary<string, IByteAddressableImage> byteAddressableImages = new();

            foreach(IByteAddressableImage plugin in _serviceProvider.GetServices<IByteAddressableImage>())
                byteAddressableImages[plugin.Name.ToLower()] = plugin;

            return byteAddressableImages;
        }
    }

    /// <summary>List of writable media image plugins</summary>
    public SortedDictionary<string, IBaseWritableImage> WritableImages
    {
        get
        {
            SortedDictionary<string, IBaseWritableImage> mediaImages = new();

            foreach(IBaseWritableImage plugin in _serviceProvider.GetServices<IBaseWritableImage>())
                mediaImages[plugin.Name.ToLower()] = plugin;

            return mediaImages;
        }
    }

    /// <summary>List of writable floppy image plugins</summary>
    public SortedDictionary<string, IWritableFloppyImage> WritableFloppyImages
    {
        get
        {
            SortedDictionary<string, IWritableFloppyImage> floppyImages = new();

            foreach(IWritableFloppyImage plugin in _serviceProvider.GetServices<IWritableFloppyImage>())
                floppyImages[plugin.Name.ToLower()] = plugin;

            return floppyImages;
        }
    }

    /// <summary>List of floppy image plugins</summary>
    public SortedDictionary<string, IFloppyImage> FloppyImages
    {
        get
        {
            SortedDictionary<string, IFloppyImage> floppyImages = new();

            foreach(IFloppyImage plugin in _serviceProvider.GetServices<IFloppyImage>())
                floppyImages[plugin.Name.ToLower()] = plugin;

            return floppyImages;
        }
    }

    /// <summary>List of all media image plugins</summary>
    public SortedDictionary<string, IMediaImage> MediaImages
    {
        get
        {
            SortedDictionary<string, IMediaImage> mediaImages = new();

            foreach(IMediaImage plugin in _serviceProvider.GetServices<IMediaImage>())
                mediaImages[plugin.Name.ToLower()] = plugin;

            return mediaImages;
        }
    }

    /// <summary>List of read-only filesystem plugins</summary>
    public SortedDictionary<string, IReadOnlyFilesystem> ReadOnlyFilesystems
    {
        get
        {
            SortedDictionary<string, IReadOnlyFilesystem> readOnlyFilesystems = new();

            foreach(IReadOnlyFilesystem plugin in _serviceProvider.GetServices<IReadOnlyFilesystem>())
                readOnlyFilesystems[plugin.Name.ToLower()] = plugin;

            return readOnlyFilesystems;
        }
    }

    /// <summary>List of all filesystem plugins</summary>
    public SortedDictionary<string, IFilesystem> Filesystems
    {
        get
        {
            SortedDictionary<string, IFilesystem> filesystems = new();

            foreach(IFilesystem plugin in _serviceProvider.GetServices<IFilesystem>())
                filesystems[plugin.Name.ToLower()] = plugin;

            return filesystems;
        }
    }

    /// <summary>List of all archive formats</summary>
    public SortedDictionary<string, IArchive> Archives
    {
        get
        {
            SortedDictionary<string, IArchive> archives = new();

            foreach(IArchive plugin in _serviceProvider.GetServices<IArchive>())
                archives[plugin.Name.ToLower()] = plugin;

            return archives;
        }
    }

    /// <summary>List of all partition plugins</summary>
    public SortedDictionary<string, IPartition> Partitions
    {
        get
        {
            SortedDictionary<string, IPartition> partitions = new();

            foreach(IPartition plugin in _serviceProvider.GetServices<IPartition>())
                partitions[plugin.Name.ToLower()] = plugin;

            return partitions;
        }
    }

    /// <summary>List of filter plugins</summary>
    public SortedDictionary<string, IFilter> Filters
    {
        get
        {
            SortedDictionary<string, IFilter> filters = new();

            foreach(IFilter plugin in _serviceProvider.GetServices<IFilter>()) filters[plugin.Name.ToLower()] = plugin;

            return filters;
        }
    }

    /// <summary>List of checksum plugins</summary>
    public SortedDictionary<string, IChecksum> Checksums
    {
        get
        {
            SortedDictionary<string, IChecksum> checksums = new();

            foreach(IChecksum plugin in _serviceProvider.GetServices<IChecksum>())
                checksums[plugin.Name.ToLower()] = plugin;

            return checksums;
        }
    }

    /// <summary>Gets a singleton with all the known plugins</summary>
    public static PluginRegister Singleton
    {
        get
        {
            if(_instance != null) return _instance;

            _instance = new PluginRegister
            {
                _services = new ServiceCollection()
            };

            _instance._serviceProvider = _instance._services.BuildServiceProvider();

            return _instance;
        }
    }


    /// <summary>
    ///     Replaces registered plugins list of this instance with the new ones provided by the providen registrators.
    /// </summary>
    /// <param name="registrators">List of plugin registrators as obtained from the assemblies that implement them.</param>
    public void InitPlugins(IEnumerable<IPluginRegister> registrators)
    {
        _services = new ServiceCollection();

        foreach(IPluginRegister registrator in registrators) AddPlugins(registrator);

        _instance._serviceProvider = _instance._services.BuildServiceProvider();
    }

    /// <summary>Adds plugins to the central plugin register</summary>
    /// <param name="pluginRegister">Plugin register</param>
    void AddPlugins(IPluginRegister pluginRegister)
    {
        pluginRegister.RegisterChecksumPlugins(_services);
        pluginRegister.RegisterFilesystemPlugins(_services);
        pluginRegister.RegisterFilterPlugins(_services);
        pluginRegister.RegisterReadOnlyFilesystemPlugins(_services);
        pluginRegister.RegisterFloppyImagePlugins(_services);
        pluginRegister.RegisterMediaImagePlugins(_services);
        pluginRegister.RegisterPartitionPlugins(_services);
        pluginRegister.RegisterWritableFloppyImagePlugins(_services);
        pluginRegister.RegisterWritableImagePlugins(_services);
        pluginRegister.RegisterArchivePlugins(_services);
        pluginRegister.RegisterByteAddressablePlugins(_services);
    }

    /// <summary>Gets the filter that allows to read the specified path</summary>
    /// <param name="path">Path</param>
    /// <returns>The filter that allows reading the specified path</returns>
    public IFilter GetFilter(string path)
    {
        IFilter noFilter = null;

        foreach(IFilter filter in Filters.Values)
        {
            try
            {
                if(filter.Id != new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
                {
                    if(!filter.Identify(path)) continue;

                    var foundFilter =
                        (IFilter)filter.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>());

                    if(foundFilter?.Open(path) == ErrorNumber.NoError) return foundFilter;
                }
                else
                    noFilter = filter;
            }
            catch(IOException)
            {
                // Ignore and continue
            }
        }

        if(!noFilter?.Identify(path) == true) return null;

        noFilter?.Open(path);

        return noFilter;
    }
}