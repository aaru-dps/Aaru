// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ls.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'ls' verb.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Commands
{
    static class Ls
    {
        internal static void doLs(LsOptions options)
        {
            DicConsole.DebugWriteLine("Ls command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Ls command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Ls command", "--input={0}", options.InputFile);

            FiltersList filtersList = new FiltersList();
            Filter inputFilter = filtersList.GetFilter(options.InputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            Encoding encoding = null;

            if(options.EncodingName != null)
            {
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(options.EncodingName);
                    if(options.Verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    encoding = null;
                    return;
                }
            }

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins(encoding);

            List<string> id_plugins;
            Filesystem _plugin;
            ImagePlugin _imageFormat;
            Errno error;

            try
            {
                _imageFormat = ImageFormat.Detect(inputFilter);

                if(_imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                    return;
                }
                else
                {
                    if(options.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", _imageFormat.Name,
                                                    _imageFormat.PluginUUID);
                    else DicConsole.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                }

                try
                {
                    if(!_imageFormat.OpenImage(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Ls command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Ls command", "Image without headers is {0} bytes.",
                                              _imageFormat.GetImageSize());
                    DicConsole.DebugWriteLine("Ls command", "Image has {0} sectors.", _imageFormat.GetSectors());
                    DicConsole.DebugWriteLine("Ls command", "Image identifies disk type as {0}.",
                                              _imageFormat.GetMediaType());

                    Core.Statistics.AddMediaFormat(_imageFormat.GetImageFormat());
                    Core.Statistics.AddMedia(_imageFormat.ImageInfo.mediaType, false);
                    Core.Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                List<Partition> partitions = Partitions.GetAll(_imageFormat);
                Partitions.AddSchemesToStats(partitions);

                if(partitions.Count == 0) DicConsole.DebugWriteLine("Ls command", "No partitions found");
                else
                {
                    DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                    for(int i = 0; i < partitions.Count; i++)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                        DicConsole.WriteLine("Identifying filesystem on partition");

                        Core.Filesystems.Identify(_imageFormat, out id_plugins, partitions[i]);
                        if(id_plugins.Count == 0) DicConsole.WriteLine("Filesystem not identified");
                        else if(id_plugins.Count > 1)
                        {
                            DicConsole.WriteLine(string.Format("Identified by {0} plugins", id_plugins.Count));

                            foreach(string plugin_name in id_plugins)
                            {
                                if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                                {
                                    DicConsole.WriteLine(string.Format("As identified by {0}.", _plugin.Name));
                                    Filesystem fs = (Filesystem)_plugin
                                        .GetType().GetConstructor(new Type[]
                                        {
                                            typeof(ImagePlugin), typeof(Partition), typeof(System.Text.Encoding)
                                        }).Invoke(new object[] {_imageFormat, partitions[i], null});

                                    error = fs.Mount(options.Debug);
                                    if(error == Errno.NoError)
                                    {
                                        List<string> rootDir = new List<string>();
                                        error = fs.ReadDir("/", ref rootDir);
                                        if(error == Errno.NoError)
                                        {
                                            foreach(string entry in rootDir) DicConsole.WriteLine("{0}", entry);
                                        }
                                        else
                                            DicConsole.ErrorWriteLine("Error {0} reading root directory {0}",
                                                                      error.ToString());

                                        Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                                    }
                                    else
                                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}",
                                                                  error.ToString());
                                }
                            }
                        }
                        else
                        {
                            plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                            DicConsole.WriteLine(string.Format("Identified by {0}.", _plugin.Name));
                            Filesystem fs = (Filesystem)_plugin
                                .GetType().GetConstructor(new Type[]
                                {
                                    typeof(ImagePlugin), typeof(Partition), typeof(System.Text.Encoding)
                                }).Invoke(new object[] {_imageFormat, partitions[i], null});
                            error = fs.Mount(options.Debug);
                            if(error == Errno.NoError)
                            {
                                List<string> rootDir = new List<string>();
                                error = fs.ReadDir("/", ref rootDir);
                                if(error == Errno.NoError)
                                {
                                    foreach(string entry in rootDir) DicConsole.WriteLine("{0}", entry);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                            }
                            else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }

                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = _imageFormat.GetSectors(),
                    Size = _imageFormat.GetSectors() * _imageFormat.GetSectorSize()
                };

                Core.Filesystems.Identify(_imageFormat, out id_plugins, wholePart);
                if(id_plugins.Count == 0) DicConsole.WriteLine("Filesystem not identified");
                else if(id_plugins.Count > 1)
                {
                    DicConsole.WriteLine(string.Format("Identified by {0} plugins", id_plugins.Count));

                    foreach(string plugin_name in id_plugins)
                    {
                        if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                        {
                            DicConsole.WriteLine(string.Format("As identified by {0}.", _plugin.Name));
                            Filesystem fs = (Filesystem)_plugin
                                .GetType().GetConstructor(new Type[]
                                {
                                    typeof(ImagePlugin), typeof(Partition), typeof(System.Text.Encoding)
                                }).Invoke(new object[] {_imageFormat, wholePart, null});
                            error = fs.Mount(options.Debug);
                            if(error == Errno.NoError)
                            {
                                List<string> rootDir = new List<string>();
                                error = fs.ReadDir("/", ref rootDir);
                                if(error == Errno.NoError)
                                {
                                    foreach(string entry in rootDir) DicConsole.WriteLine("{0}", entry);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                            }
                            else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }
                else
                {
                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                    DicConsole.WriteLine(string.Format("Identified by {0}.", _plugin.Name));
                    Filesystem fs = (Filesystem)_plugin
                        .GetType().GetConstructor(new Type[]
                        {
                            typeof(ImagePlugin), typeof(Partition), typeof(System.Text.Encoding)
                        }).Invoke(new object[] {_imageFormat, wholePart, null});
                    error = fs.Mount(options.Debug);
                    if(error == Errno.NoError)
                    {
                        List<string> rootDir = new List<string>();
                        error = fs.ReadDir("/", ref rootDir);
                        if(error == Errno.NoError)
                        {
                            foreach(string entry in rootDir)
                            {
                                if(options.Long)
                                {
                                    FileEntryInfo stat = new FileEntryInfo();
                                    List<string> xattrs = new List<string>();

                                    error = fs.Stat(entry, ref stat);
                                    if(error == Errno.NoError)
                                    {
                                        DicConsole.WriteLine("{0}\t{1}\t{2} bytes\t{3}", stat.CreationTimeUtc,
                                                             stat.Inode, stat.Length, entry);

                                        error = fs.ListXAttr(entry, ref xattrs);
                                        if(error == Errno.NoError)
                                        {
                                            foreach(string xattr in xattrs)
                                            {
                                                byte[] xattrBuf = new byte[0];
                                                error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                if(error == Errno.NoError)
                                                {
                                                    DicConsole.WriteLine("\t\t{0}\t{1} bytes", xattr, xattrBuf.Length);
                                                }
                                            }
                                        }
                                    }
                                    else DicConsole.WriteLine("{0}", entry);
                                }
                                else DicConsole.WriteLine("{0}", entry);
                            }
                        }
                        else DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                        Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                    }
                    else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine(string.Format("Error reading file: {0}", ex.Message));
                DicConsole.DebugWriteLine("Ls command", ex.StackTrace);
            }

            Core.Statistics.AddCommand("ls");
        }
    }
}