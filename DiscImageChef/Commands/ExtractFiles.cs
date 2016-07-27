// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtractFiles.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$

// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Filesystems;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using System.IO;

namespace DiscImageChef.Commands
{
    public class ExtractFiles
    {
        public static void doExtractFiles(ExtractFilesOptions options)
        {
            DicConsole.DebugWriteLine("Extract-Files command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Extract-Files command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}", options.Xattrs);
            DicConsole.DebugWriteLine("Extract-Files command", "--output={0}", options.OutputDir);

            if(!System.IO.File.Exists(options.InputFile))
            {
                DicConsole.ErrorWriteLine("Specified file does not exist.");
                return;
            }

            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            List<string> id_plugins;
            Filesystem _plugin;
            ImagePlugin _imageFormat;
            Errno error;

            try
            {
                _imageFormat = ImageFormat.Detect(options.InputFile);

                if(_imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                    return;
                }
                else
                {
                    if(options.Verbose)
                        DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", _imageFormat.Name, _imageFormat.PluginUUID);
                    else
                        DicConsole.WriteLine("Image format identified by {0}.", _imageFormat.Name);
                }

                if(Directory.Exists(options.OutputDir) || File.Exists(options.OutputDir))
                {
                    DicConsole.ErrorWriteLine("Destination exists, aborting.");
                    return;
                }

                Directory.CreateDirectory(options.OutputDir);

                try
                {
                    if(!_imageFormat.OpenImage(options.InputFile))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.", _imageFormat.GetImageSize());
                    DicConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.", _imageFormat.GetSectors());
                    DicConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.", _imageFormat.GetMediaType());

                    Core.Statistics.AddMediaFormat(_imageFormat.GetImageFormat());
                    Core.Statistics.AddMedia(_imageFormat.ImageInfo.mediaType, false);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                List<CommonTypes.Partition> partitions = new List<CommonTypes.Partition>();
                string partition_scheme = "";

                // TODO: Solve possibility of multiple partition schemes (CUE + MBR, MBR + RDB, CUE + APM, etc)
                foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                {
                    List<CommonTypes.Partition> _partitions;
                    if(_partplugin.GetInformation(_imageFormat, out _partitions))
                    {
                        partition_scheme = _partplugin.Name;
                        partitions.AddRange(_partitions);
                        Core.Statistics.AddPartition(_partplugin.Name);
                    }
                }

                if(_imageFormat.ImageHasPartitions())
                {
                    partition_scheme = _imageFormat.GetImageFormat();
                    partitions.AddRange(_imageFormat.GetPartitions());
                }

                if(partition_scheme == "")
                    DicConsole.DebugWriteLine("Extract-Files command", "No partitions found");
                else
                {
                    DicConsole.WriteLine("Partition scheme identified as {0}", partition_scheme);
                    DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                    for(int i = 0; i < partitions.Count; i++)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Partition {0}:", partitions[i].PartitionSequence);

                        DicConsole.WriteLine("Identifying filesystem on partition");

                        IdentifyFilesystems(_imageFormat, out id_plugins, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors);
                        if(id_plugins.Count == 0)
                            DicConsole.WriteLine("Filesystem not identified");
                        else if(id_plugins.Count > 1)
                        {
                            DicConsole.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));

                            foreach(string plugin_name in id_plugins)
                            {
                                if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                                {
                                    DicConsole.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                                    Filesystem fs = (Filesystem)_plugin.GetType().GetConstructor(new Type[] { typeof(ImagePlugin), typeof(ulong), typeof(ulong) }).Invoke(new object[] { _imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors });

                                    error = fs.Mount(options.Debug);
                                    if(error == Errno.NoError)
                                    {
                                        List<string> rootDir = new List<string>();
                                        error = fs.ReadDir("/", ref rootDir);
                                        if(error == Errno.NoError)
                                        {
                                            foreach(string entry in rootDir)
                                                DicConsole.WriteLine("{0}", entry);
                                        }
                                        else
                                            DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                        Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                                    }
                                    else
                                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                                }
                            }
                        }
                        else
                        {
                            plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                            DicConsole.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                            Filesystem fs = (Filesystem)_plugin.GetType().GetConstructor(new Type[] { typeof(ImagePlugin), typeof(ulong), typeof(ulong) }).Invoke(new object[] { _imageFormat, partitions[i].PartitionStartSector, partitions[i].PartitionStartSector + partitions[i].PartitionSectors });
                            error = fs.Mount(options.Debug);
                            if(error == Errno.NoError)
                            {
                                List<string> rootDir = new List<string>();
                                error = fs.ReadDir("/", ref rootDir);
                                if(error == Errno.NoError)
                                {
                                    foreach(string entry in rootDir)
                                        DicConsole.WriteLine("{0}", entry);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }

                IdentifyFilesystems(_imageFormat, out id_plugins, 0, _imageFormat.GetSectors() - 1);
                if(id_plugins.Count == 0)
                    DicConsole.WriteLine("Filesystem not identified");
                else if(id_plugins.Count > 1)
                {
                    DicConsole.WriteLine(String.Format("Identified by {0} plugins", id_plugins.Count));

                    foreach(string plugin_name in id_plugins)
                    {
                        if(plugins.PluginsList.TryGetValue(plugin_name, out _plugin))
                        {
                            DicConsole.WriteLine(String.Format("As identified by {0}.", _plugin.Name));
                            Filesystem fs = (Filesystem)_plugin.GetType().GetConstructor(new Type[] { typeof(ImagePlugin), typeof(ulong), typeof(ulong) }).Invoke(new object[] { _imageFormat, (ulong)0, (ulong)(_imageFormat.GetSectors() - 1) });
                            error = fs.Mount(options.Debug);
                            if(error == Errno.NoError)
                            {
                                List<string> rootDir = new List<string>();
                                error = fs.ReadDir("/", ref rootDir);
                                if(error == Errno.NoError)
                                {
                                    foreach(string entry in rootDir)
                                        DicConsole.WriteLine("{0}", entry);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }
                else
                {
                    plugins.PluginsList.TryGetValue(id_plugins[0], out _plugin);
                    DicConsole.WriteLine(String.Format("Identified by {0}.", _plugin.Name));
                    Filesystem fs = (Filesystem)_plugin.GetType().GetConstructor(new Type[] { typeof(ImagePlugin), typeof(ulong), typeof(ulong) }).Invoke(new object[] { _imageFormat, (ulong)0, (ulong)(_imageFormat.GetSectors() - 1) });
                    error = fs.Mount(options.Debug);
                    if(error == Errno.NoError)
                    {
                        List<string> rootDir = new List<string>();
                        error = fs.ReadDir("/", ref rootDir);
                        if(error == Errno.NoError)
                        {
                            foreach(string entry in rootDir)
                            {
                                FileEntryInfo stat = new FileEntryInfo();
                                string outputPath;
                                FileStream outputFile;

                                error = fs.Stat(entry, ref stat);
                                if(error == Errno.NoError)
                                {
                                    if(options.Xattrs)
                                    {
                                        List<string> xattrs = new List<string>();

                                        error = fs.ListXAttr(entry, ref xattrs);
                                        if(error == Errno.NoError)
                                        {
                                            foreach(string xattr in xattrs)
                                            {
                                                byte[] xattrBuf = new byte[0];
                                                error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                if(error == Errno.NoError)
                                                {
                                                    Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                           fs.XmlFSType.Type,
                                                                                           fs.XmlFSType.VolumeName,
                                                                                           ".xattrs",
                                                                                           xattr));

                                                    outputPath = Path.Combine(options.OutputDir,
                                                                                    fs.XmlFSType.Type,
                                                                                    fs.XmlFSType.VolumeName,
                                                                                    ".xattrs",
                                                                                    xattr,
                                                                                     entry);

                                                    if(!File.Exists(outputPath))
                                                    {
                                                        outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                                                               FileShare.None);
                                                        outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                                        outputFile.Close();
                                                        System.IO.FileInfo fi = new System.IO.FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                        try { fi.CreationTimeUtc = stat.CreationTimeUtc; } catch { }
                                                        try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; } catch { }
                                                        try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; } catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                        DicConsole.WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                                             xattrBuf.Length, xattr, entry, outputPath);
                                                    }
                                                    else
                                                        DicConsole.ErrorWriteLine("Cannot write xattr {0} for {1}, output exists",
                                                                             xattr, entry);

                                                }
                                            }
                                        }
                                    }

                                    Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                           fs.XmlFSType.Type,
                                                                           fs.XmlFSType.VolumeName));

                                    outputPath = Path.Combine(options.OutputDir,
                                                                    fs.XmlFSType.Type,
                                                                    fs.XmlFSType.VolumeName,
                                                                     entry);

                                    if(!File.Exists(outputPath))
                                    {
                                        byte[] outBuf = new byte[0];

                                        error = fs.Read(entry, 0, stat.Length, ref outBuf);

                                        if(error == Errno.NoError)
                                        {
                                            outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                                        FileShare.None);
                                            outputFile.Write(outBuf, 0, outBuf.Length);
                                            outputFile.Close();
                                            System.IO.FileInfo fi = new System.IO.FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                            try { fi.CreationTimeUtc = stat.CreationTimeUtc; } catch { }
                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; } catch { }
                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; } catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                            DicConsole.WriteLine("Written {0} bytes of file {1} to {2}",
                                                                 outBuf.Length, entry, outputPath);
                                        }
                                        else
                                            DicConsole.ErrorWriteLine("Error {0} reading file {1}", error, entry);
                                    }
                                    else
                                        DicConsole.ErrorWriteLine("Cannot write file {1}, output exists", entry);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Error reading file {0}", entry);
                            }
                        }
                        else
                            DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                        Core.Statistics.AddFilesystem(fs.XmlFSType.Type);
                    }
                    else
                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine(String.Format("Error reading file: {0}", ex.Message));
                DicConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);
            }

            Core.Statistics.AddCommand("ls");
        }

        static void IdentifyFilesystems(ImagePlugin imagePlugin, out List<string> id_plugins, ulong partitionStart, ulong partitionEnd)
        {
            id_plugins = new List<string>();
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();

            foreach(Filesystem _plugin in plugins.PluginsList.Values)
            {
                if(_plugin.Identify(imagePlugin, partitionStart, partitionEnd))
                    id_plugins.Add(_plugin.Name.ToLower());
            }
        }
    }
}

