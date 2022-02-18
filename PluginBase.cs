// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PluginBase.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Class to hold all installed plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.CommonTypes;

/// <summary>Contain all plugins (filesystem, partition and image)</summary>
public class PluginBase
{
    /// <summary>List of all archive formats</summary>
    public readonly SortedDictionary<string, IArchive> Archives;
    /// <summary>List of byte addressable image plugins</summary>
    public readonly SortedDictionary<string, IByteAddressableImage> ByteAddressableImages;
    /// <summary>List of checksum plugins</summary>
    public readonly List<IChecksum> Checksums;
    /// <summary>List of filter plugins</summary>
    public readonly SortedDictionary<string, IFilter> Filters;
    /// <summary>List of floppy image plugins</summary>
    public readonly SortedDictionary<string, IFloppyImage> FloppyImages;
    /// <summary>List of all media image plugins</summary>
    public readonly SortedDictionary<string, IMediaImage> ImagePluginsList;
    /// <summary>List of all partition plugins</summary>
    public readonly SortedDictionary<string, IPartition> PartPluginsList;
    /// <summary>List of all filesystem plugins</summary>
    public readonly SortedDictionary<string, IFilesystem> PluginsList;
    /// <summary>List of read-only filesystem plugins</summary>
    public readonly SortedDictionary<string, IReadOnlyFilesystem> ReadOnlyFilesystems;
    /// <summary>List of writable floppy image plugins</summary>
    public readonly SortedDictionary<string, IWritableFloppyImage> WritableFloppyImages;
    /// <summary>List of writable media image plugins</summary>
    public readonly SortedDictionary<string, IBaseWritableImage> WritableImages;

    /// <summary>Initializes the plugins lists</summary>
    public PluginBase()
    {
        PluginsList           = new SortedDictionary<string, IFilesystem>();
        ReadOnlyFilesystems   = new SortedDictionary<string, IReadOnlyFilesystem>();
        PartPluginsList       = new SortedDictionary<string, IPartition>();
        ImagePluginsList      = new SortedDictionary<string, IMediaImage>();
        WritableImages        = new SortedDictionary<string, IBaseWritableImage>();
        Checksums             = new List<IChecksum>();
        Filters               = new SortedDictionary<string, IFilter>();
        FloppyImages          = new SortedDictionary<string, IFloppyImage>();
        WritableFloppyImages  = new SortedDictionary<string, IWritableFloppyImage>();
        Archives              = new SortedDictionary<string, IArchive>();
        ByteAddressableImages = new SortedDictionary<string, IByteAddressableImage>();
    }

    /// <summary>Adds plugins to the central plugin register</summary>
    /// <param name="pluginRegister">Plugin register</param>
    public void AddPlugins(IPluginRegister pluginRegister)
    {
        foreach(Type type in pluginRegister.GetAllChecksumPlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IChecksum plugin)
                Checksums.Add(plugin);

        foreach(Type type in pluginRegister.GetAllFilesystemPlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IFilesystem plugin &&
               !PluginsList.ContainsKey(plugin.Name.ToLower()))
                PluginsList.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllFilterPlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IFilter plugin &&
               !Filters.ContainsKey(plugin.Name.ToLower()))
                Filters.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllFloppyImagePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IFloppyImage plugin &&
               !FloppyImages.ContainsKey(plugin.Name.ToLower()))
                FloppyImages.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllMediaImagePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IMediaImage plugin &&
               !ImagePluginsList.ContainsKey(plugin.Name.ToLower()))
                ImagePluginsList.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllPartitionPlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IPartition plugin &&
               !PartPluginsList.ContainsKey(plugin.Name.ToLower()))
                PartPluginsList.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllReadOnlyFilesystemPlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IReadOnlyFilesystem plugin &&
               !ReadOnlyFilesystems.ContainsKey(plugin.Name.ToLower()))
                ReadOnlyFilesystems.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllWritableFloppyImagePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IWritableFloppyImage plugin &&
               !WritableFloppyImages.ContainsKey(plugin.Name.ToLower()))
                WritableFloppyImages.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllWritableImagePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IBaseWritableImage plugin &&
               !WritableImages.ContainsKey(plugin.Name.ToLower()))
                WritableImages.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllArchivePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IArchive plugin &&
               !Archives.ContainsKey(plugin.Name.ToLower()))
                Archives.Add(plugin.Name.ToLower(), plugin);

        foreach(Type type in pluginRegister.GetAllByteAddressablePlugins() ?? Enumerable.Empty<Type>())
            if(type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                {}) is IByteAddressableImage plugin &&
               !ByteAddressableImages.ContainsKey(plugin.Name.ToLower()))
                ByteAddressableImages.Add(plugin.Name.ToLower(), plugin);
    }
}