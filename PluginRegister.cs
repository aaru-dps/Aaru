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
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Aaru.CommonTypes;

public class PluginRegister
{
    static PluginRegister _instance;
    /// <summary>List of all archive formats</summary>
    public readonly SortedDictionary<string, Type> Archives;
    /// <summary>List of byte addressable image plugins</summary>
    public readonly SortedDictionary<string, Type> ByteAddressableImages;
    /// <summary>List of all filesystem plugins</summary>
    public readonly SortedDictionary<string, Type> Filesystems;
    /// <summary>List of filter plugins</summary>
    public readonly SortedDictionary<string, Type> Filters;
    /// <summary>List of floppy image plugins</summary>
    public readonly SortedDictionary<string, Type> FloppyImages;
    /// <summary>List of all media image plugins</summary>
    public readonly SortedDictionary<string, Type> MediaImages;
    /// <summary>List of all partition plugins</summary>
    public readonly SortedDictionary<string, Type> Partitions;
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
        Filters               = new SortedDictionary<string, Type>();
        Filesystems           = new SortedDictionary<string, Type>();
        ReadOnlyFilesystems   = new SortedDictionary<string, Type>();
        Partitions            = new SortedDictionary<string, Type>();
        MediaImages           = new SortedDictionary<string, Type>();
        WritableImages        = new SortedDictionary<string, Type>();
        FloppyImages          = new SortedDictionary<string, Type>();
        WritableFloppyImages  = new SortedDictionary<string, Type>();
        Archives              = new SortedDictionary<string, Type>();
        ByteAddressableImages = new SortedDictionary<string, Type>();
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

        foreach(Type type in pluginRegister.GetAllFilterPlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IFilter plugin && !Filters.ContainsKey(plugin.Name.ToLower()))
                Filters.Add(plugin.Name.ToLower(), type);
        }

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

        foreach(Type type in pluginRegister.GetAllPartitionPlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IPartition plugin && !Partitions.ContainsKey(plugin.Name.ToLower()))
                Partitions.Add(plugin.Name.ToLower(), type);
        }

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

        foreach(Type type in pluginRegister.GetAllArchivePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IArchive plugin && !Archives.ContainsKey(plugin.Name.ToLower()))
                Archives.Add(plugin.Name.ToLower(), type);
        }

        foreach(Type type in pluginRegister.GetAllByteAddressablePlugins() ?? Enumerable.Empty<Type>())
        {
            if(Activator.CreateInstance(type) is IByteAddressableImage plugin &&
               !ByteAddressableImages.ContainsKey(plugin.Name.ToLower()))
                ByteAddressableImages.Add(plugin.Name.ToLower(), type);
        }
    }
}