// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Partitions;

namespace DiscImageChef.CommonTypes
{
    /// <summary>
    ///     Contain all plugins (filesystem, partition and image)
    /// </summary>
    public class PluginBase
    {
        /// <summary>
        ///     List of all media image plugins
        /// </summary>
        public readonly SortedDictionary<string, IMediaImage> ImagePluginsList;
        /// <summary>
        ///     List of all partition plugins
        /// </summary>
        public readonly SortedDictionary<string, IPartition> PartPluginsList;
        /// <summary>
        ///     List of all filesystem plugins
        /// </summary>
        public readonly SortedDictionary<string, IFilesystem> PluginsList;
        /// <summary>
        ///     List of read-only filesystem plugins
        /// </summary>
        public readonly SortedDictionary<string, IReadOnlyFilesystem> ReadOnlyFilesystems;
        /// <summary>
        ///     List of writable media image plugins
        /// </summary>
        public readonly SortedDictionary<string, IWritableImage> WritableImages;

        /// <summary>
        ///     Initializes the plugins lists
        /// </summary>
        public PluginBase()
        {
            PluginsList         = new SortedDictionary<string, IFilesystem>();
            ReadOnlyFilesystems = new SortedDictionary<string, IReadOnlyFilesystem>();
            PartPluginsList     = new SortedDictionary<string, IPartition>();
            ImagePluginsList    = new SortedDictionary<string, IMediaImage>();
            WritableImages      = new SortedDictionary<string, IWritableImage>();

            // We need to manually load assemblies :(
            AppDomain.CurrentDomain.Load("DiscImageChef.DiscImages");
            AppDomain.CurrentDomain.Load("DiscImageChef.Filesystems");
            AppDomain.CurrentDomain.Load("DiscImageChef.Partitions");

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies)
            {
                foreach(Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMediaImage)))
                                             .Where(t => t.IsClass))
                    try
                    {
                        IMediaImage plugin =
                            (IMediaImage)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                        RegisterImagePlugin(plugin);
                    }
                    catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

                foreach(Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IPartition)))
                                             .Where(t => t.IsClass))
                    try
                    {
                        IPartition plugin = (IPartition)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                        RegisterPartPlugin(plugin);
                    }
                    catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

                foreach(Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IFilesystem)))
                                             .Where(t => t.IsClass))
                    try
                    {
                        IFilesystem plugin =
                            (IFilesystem)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                        RegisterPlugin(plugin);
                    }
                    catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

                foreach(Type type in assembly
                                    .GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IReadOnlyFilesystem)))
                                    .Where(t => t.IsClass))
                    try
                    {
                        IReadOnlyFilesystem plugin =
                            (IReadOnlyFilesystem)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                        RegisterReadOnlyFilesystem(plugin);
                    }
                    catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }

                foreach(Type type in assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IWritableImage)))
                                             .Where(t => t.IsClass))
                    try
                    {
                        IWritableImage plugin =
                            (IWritableImage)type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                        RegisterWritableMedia(plugin);
                    }
                    catch(Exception exception) { DicConsole.ErrorWriteLine("Exception {0}", exception); }
            }
        }

        void RegisterImagePlugin(IMediaImage plugin)
        {
            if(!ImagePluginsList.ContainsKey(plugin.Name.ToLower()))
                ImagePluginsList.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterPlugin(IFilesystem plugin)
        {
            if(!PluginsList.ContainsKey(plugin.Name.ToLower())) PluginsList.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterReadOnlyFilesystem(IReadOnlyFilesystem plugin)
        {
            if(!ReadOnlyFilesystems.ContainsKey(plugin.Name.ToLower()))
                ReadOnlyFilesystems.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterWritableMedia(IWritableImage plugin)
        {
            if(!WritableImages.ContainsKey(plugin.Name.ToLower())) WritableImages.Add(plugin.Name.ToLower(), plugin);
        }

        void RegisterPartPlugin(IPartition partplugin)
        {
            if(!PartPluginsList.ContainsKey(partplugin.Name.ToLower()))
                PartPluginsList.Add(partplugin.Name.ToLower(), partplugin);
        }
    }
}