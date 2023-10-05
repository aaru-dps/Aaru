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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Aaru.CommonTypes;

public class PluginRegister
{
    static PluginRegister _instance;

    /// <summary>List of byte addressable image plugins</summary>
    public readonly SortedDictionary<string, Type> ByteAddressableImages;
    /// <summary>List of all filesystem plugins</summary>
    public readonly SortedDictionary<string, Type> Filesystems;

    /// <summary>List of floppy image plugins</summary>
    public readonly SortedDictionary<string, Type> FloppyImages;
    /// <summary>List of all media image plugins</summary>
    public readonly SortedDictionary<string, Type> MediaImages;

    /// <summary>List of read-only filesystem plugins</summary>
    public readonly SortedDictionary<string, Type> ReadOnlyFilesystems;
    /// <summary>List of writable floppy image plugins</summary>
    public readonly SortedDictionary<string, Type> WritableFloppyImages;
    /// <summary>List of writable media image plugins</summary>
    public readonly SortedDictionary<string, Type> WritableImages;
    IServiceProvider   _serviceProvider;
    IServiceCollection _services;

    PluginRegister()
    {
        Filesystems           = new SortedDictionary<string, Type>();
        ReadOnlyFilesystems   = new SortedDictionary<string, Type>();
        MediaImages           = new SortedDictionary<string, Type>();
        WritableImages        = new SortedDictionary<string, Type>();
        FloppyImages          = new SortedDictionary<string, Type>();
        WritableFloppyImages  = new SortedDictionary<string, Type>();
        ByteAddressableImages = new SortedDictionary<string, Type>();
    }

    /// <summary>List of all archive formats</summary>
    public SortedDictionary<string, IArchive> Archives
    {
        get
        {
            SortedDictionary<string, IArchive> archives = new();
            foreach(IArchive plugin in _serviceProvider.GetServices<IArchive>())
                archives.Add(plugin.Name.ToLower(), plugin);

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
                partitions.Add(plugin.Name.ToLower(), plugin);

            return partitions;
        }
    }

    /// <summary>List of filter plugins</summary>
    public SortedDictionary<string, IFilter> Filters
    {
        get
        {
            SortedDictionary<string, IFilter> filters = new();
            foreach(IFilter plugin in _serviceProvider.GetServices<IFilter>())
                filters.Add(plugin.Name.ToLower(), plugin);

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
                checksums.Add(plugin.Name.ToLower(), plugin);

            return checksums;
        }
    }

    /// <summary>Gets a singleton with all the known plugins</summary>
    public static PluginRegister Singleton
    {
        get
        {
            if(_instance != null)
                return _instance;

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

        foreach(IPluginRegister registrator in registrators)
            AddPlugins(registrator);

        _instance._serviceProvider = _instance._services.BuildServiceProvider();
    }

    /// <summary>Adds plugins to the central plugin register</summary>
    /// <param name="pluginRegister">Plugin register</param>
    void AddPlugins(IPluginRegister pluginRegister)
    {
        pluginRegister.RegisterChecksumPlugins(_services);

        foreach(Type type in pluginRegister.GetAllFilesystemPlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IFilesystem plugin && !Filesystems.ContainsKey(plugin.Name.ToLower()))
                Filesystems.Add(plugin.Name.ToLower(), type);
        }

        pluginRegister.RegisterFilterPlugins(_services);

        foreach(Type type in pluginRegister.GetAllFloppyImagePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IFloppyImage plugin &&
               !FloppyImages.ContainsKey(plugin.Name.ToLower()))
                FloppyImages.Add(plugin.Name.ToLower(), type);
        }

        foreach(Type type in pluginRegister.GetAllMediaImagePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IMediaImage plugin && !MediaImages.ContainsKey(plugin.Name.ToLower()))
                MediaImages.Add(plugin.Name.ToLower(), type);
        }

        pluginRegister.RegisterPartitionPlugins(_services);

        foreach(Type type in pluginRegister.GetAllReadOnlyFilesystemPlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IReadOnlyFilesystem plugin &&
               !ReadOnlyFilesystems.ContainsKey(plugin.Name.ToLower()))
                ReadOnlyFilesystems.Add(plugin.Name.ToLower(), type);
        }

        foreach(Type type in pluginRegister.GetAllWritableFloppyImagePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IWritableFloppyImage plugin &&
               !WritableFloppyImages.ContainsKey(plugin.Name.ToLower()))
                WritableFloppyImages.Add(plugin.Name.ToLower(), type);
        }

        foreach(Type type in pluginRegister.GetAllWritableImagePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IBaseWritableImage plugin &&
               !WritableImages.ContainsKey(plugin.Name.ToLower()))
                WritableImages.Add(plugin.Name.ToLower(), type);
        }

        pluginRegister.RegisterArchivePlugins(_services);

        foreach(Type type in pluginRegister.GetAllByteAddressablePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IByteAddressableImage plugin &&
               !ByteAddressableImages.ContainsKey(plugin.Name.ToLower()))
                ByteAddressableImages.Add(plugin.Name.ToLower(), type);
        }
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
                    if(!filter.Identify(path))
                        continue;

                    var foundFilter =
                        (IFilter)filter.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>());

                    if(foundFilter?.Open(path) == ErrorNumber.NoError)
                        return foundFilter;
                }
                else
                    noFilter = filter;
            }
            catch(IOException)
            {
                // Ignore and continue
            }
        }

        if(!noFilter?.Identify(path) == true)
            return null;

        noFilter?.Open(path);

        return noFilter;
    }
}