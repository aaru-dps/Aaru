// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtractFiles.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'extract-files' verb.
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
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Commands
{
    // TODO: Rewrite this, has an insane amount of repeating code ;)
    static class ExtractFiles
    {
        internal static void doExtractFiles(ExtractFilesOptions options)
        {
            DicConsole.DebugWriteLine("Extract-Files command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Extract-Files command", "--input={0}", options.InputFile);
            DicConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}", options.Xattrs);
            DicConsole.DebugWriteLine("Extract-Files command", "--output={0}", options.OutputDir);

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

                if(Directory.Exists(options.OutputDir) || File.Exists(options.OutputDir))
                {
                    DicConsole.ErrorWriteLine("Destination exists, aborting.");
                    return;
                }

                Directory.CreateDirectory(options.OutputDir);

                try
                {
                    if(!_imageFormat.OpenImage(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.",
                                              _imageFormat.GetImageSize());
                    DicConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.",
                                              _imageFormat.GetSectors());
                    DicConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.",
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

                if(partitions.Count == 0) DicConsole.DebugWriteLine("Extract-Files command", "No partitions found");
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
                                            foreach(string entry in rootDir)
                                            {
                                                FileEntryInfo stat = new FileEntryInfo();
                                                string outputPath;
                                                FileStream outputFile;

                                                string volumeName;
                                                if(string.IsNullOrEmpty(fs.XmlFSType.VolumeName))
                                                    volumeName = "NO NAME";
                                                else volumeName = fs.XmlFSType.VolumeName;

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
                                                                    Directory
                                                                        .CreateDirectory(Path.Combine(options.OutputDir,
                                                                                                      fs.XmlFSType.Type,
                                                                                                      volumeName,
                                                                                                      ".xattrs",
                                                                                                      xattr));

                                                                    outputPath =
                                                                        Path.Combine(options.OutputDir,
                                                                                     fs.XmlFSType.Type, volumeName,
                                                                                     ".xattrs", xattr, entry);

                                                                    if(!File.Exists(outputPath))
                                                                    {
                                                                        outputFile =
                                                                            new FileStream(outputPath,
                                                                                           FileMode.CreateNew,
                                                                                           FileAccess.ReadWrite,
                                                                                           FileShare.None);
                                                                        outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                                                        outputFile.Close();
                                                                        FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                        try
                                                                        {
                                                                            fi.CreationTimeUtc = stat.CreationTimeUtc;
                                                                        }
                                                                        catch { }
                                                                        try
                                                                        {
                                                                            fi.LastWriteTimeUtc = stat.LastWriteTimeUtc;
                                                                        }
                                                                        catch { }
                                                                        try
                                                                        {
                                                                            fi.LastAccessTimeUtc = stat.AccessTimeUtc;
                                                                        }
                                                                        catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                        DicConsole
                                                                            .WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                                                       xattrBuf.Length, xattr, entry,
                                                                                       outputPath);
                                                                    }
                                                                    else
                                                                        DicConsole
                                                                            .ErrorWriteLine("Cannot write xattr {0} for {1}, output exists",
                                                                                            xattr, entry);
                                                                }
                                                            }
                                                        }
                                                    }

                                                    Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                           fs.XmlFSType.Type,
                                                                                           volumeName));

                                                    outputPath =
                                                        Path.Combine(options.OutputDir, fs.XmlFSType.Type, volumeName,
                                                                     entry);

                                                    if(!File.Exists(outputPath))
                                                    {
                                                        byte[] outBuf = new byte[0];

                                                        error = fs.Read(entry, 0, stat.Length, ref outBuf);

                                                        if(error == Errno.NoError)
                                                        {
                                                            outputFile =
                                                                new FileStream(outputPath, FileMode.CreateNew,
                                                                               FileAccess.ReadWrite, FileShare.None);
                                                            outputFile.Write(outBuf, 0, outBuf.Length);
                                                            outputFile.Close();
                                                            FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                            try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                            catch { }
                                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                            catch { }
                                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                            DicConsole.WriteLine("Written {0} bytes of file {1} to {2}",
                                                                                 outBuf.Length, entry, outputPath);
                                                        }
                                                        else
                                                            DicConsole.ErrorWriteLine("Error {0} reading file {1}",
                                                                                      error, entry);
                                                    }
                                                    else
                                                        DicConsole
                                                            .ErrorWriteLine("Cannot write file {0}, output exists",
                                                                            entry);
                                                }
                                                else DicConsole.ErrorWriteLine("Error reading file {0}", entry);
                                            }
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
                                    foreach(string entry in rootDir)
                                    {
                                        FileEntryInfo stat = new FileEntryInfo();
                                        string outputPath;
                                        FileStream outputFile;

                                        string volumeName;
                                        if(string.IsNullOrEmpty(fs.XmlFSType.VolumeName)) volumeName = "NO NAME";
                                        else volumeName = fs.XmlFSType.VolumeName;

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
                                                                                                   volumeName,
                                                                                                   ".xattrs", xattr));

                                                            outputPath =
                                                                Path.Combine(options.OutputDir, fs.XmlFSType.Type,
                                                                             volumeName, ".xattrs", xattr, entry);

                                                            if(!File.Exists(outputPath))
                                                            {
                                                                outputFile =
                                                                    new FileStream(outputPath, FileMode.CreateNew,
                                                                                   FileAccess.ReadWrite,
                                                                                   FileShare.None);
                                                                outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                                                outputFile.Close();
                                                                FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                                catch { }
                                                                try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                                catch { }
                                                                try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                DicConsole
                                                                    .WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                                               xattrBuf.Length, xattr, entry,
                                                                               outputPath);
                                                            }
                                                            else
                                                                DicConsole
                                                                    .ErrorWriteLine("Cannot write xattr {0} for {1}, output exists",
                                                                                    xattr, entry);
                                                        }
                                                    }
                                                }
                                            }

                                            Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFSType.Type,
                                                                                   volumeName));

                                            outputPath =
                                                Path.Combine(options.OutputDir, fs.XmlFSType.Type, volumeName, entry);

                                            if(!File.Exists(outputPath))
                                            {
                                                byte[] outBuf = new byte[0];

                                                error = fs.Read(entry, 0, stat.Length, ref outBuf);

                                                if(error == Errno.NoError)
                                                {
                                                    outputFile =
                                                        new FileStream(outputPath, FileMode.CreateNew,
                                                                       FileAccess.ReadWrite, FileShare.None);
                                                    outputFile.Write(outBuf, 0, outBuf.Length);
                                                    outputFile.Close();
                                                    FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                    try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                    catch { }
                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                    catch { }
                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                    catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                    DicConsole.WriteLine("Written {0} bytes of file {1} to {2}",
                                                                         outBuf.Length, entry, outputPath);
                                                }
                                                else
                                                    DicConsole.ErrorWriteLine("Error {0} reading file {1}", error,
                                                                              entry);
                                            }
                                            else
                                                DicConsole.ErrorWriteLine("Cannot write file {0}, output exists",
                                                                          entry);
                                        }
                                        else DicConsole.ErrorWriteLine("Error reading file {0}", entry);
                                    }
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
                                    foreach(string entry in rootDir)
                                    {
                                        FileEntryInfo stat = new FileEntryInfo();
                                        string outputPath;
                                        FileStream outputFile;

                                        string volumeName;
                                        if(string.IsNullOrEmpty(fs.XmlFSType.VolumeName)) volumeName = "NO NAME";
                                        else volumeName = fs.XmlFSType.VolumeName;

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
                                                                                                   volumeName,
                                                                                                   ".xattrs", xattr));

                                                            outputPath =
                                                                Path.Combine(options.OutputDir, fs.XmlFSType.Type,
                                                                             volumeName, ".xattrs", xattr, entry);

                                                            if(!File.Exists(outputPath))
                                                            {
                                                                outputFile =
                                                                    new FileStream(outputPath, FileMode.CreateNew,
                                                                                   FileAccess.ReadWrite,
                                                                                   FileShare.None);
                                                                outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                                                outputFile.Close();
                                                                FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                                catch { }
                                                                try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                                catch { }
                                                                try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                                DicConsole
                                                                    .WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                                               xattrBuf.Length, xattr, entry,
                                                                               outputPath);
                                                            }
                                                            else
                                                                DicConsole
                                                                    .ErrorWriteLine("Cannot write xattr {0} for {1}, output exists",
                                                                                    xattr, entry);
                                                        }
                                                    }
                                                }
                                            }

                                            Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFSType.Type,
                                                                                   volumeName));

                                            outputPath =
                                                Path.Combine(options.OutputDir, fs.XmlFSType.Type, volumeName, entry);

                                            if(!File.Exists(outputPath))
                                            {
                                                byte[] outBuf = new byte[0];

                                                error = fs.Read(entry, 0, stat.Length, ref outBuf);

                                                if(error == Errno.NoError)
                                                {
                                                    outputFile =
                                                        new FileStream(outputPath, FileMode.CreateNew,
                                                                       FileAccess.ReadWrite, FileShare.None);
                                                    outputFile.Write(outBuf, 0, outBuf.Length);
                                                    outputFile.Close();
                                                    FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                    try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                    catch { }
                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                    catch { }
                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                    catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                    DicConsole.WriteLine("Written {0} bytes of file {1} to {2}",
                                                                         outBuf.Length, entry, outputPath);
                                                }
                                                else
                                                    DicConsole.ErrorWriteLine("Error {0} reading file {1}", error,
                                                                              entry);
                                            }
                                            else
                                                DicConsole.ErrorWriteLine("Cannot write file {0}, output exists",
                                                                          entry);
                                        }
                                        else DicConsole.ErrorWriteLine("Error reading file {0}", entry);
                                    }
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
                                FileEntryInfo stat = new FileEntryInfo();
                                string outputPath;
                                FileStream outputFile;

                                string volumeName;
                                if(string.IsNullOrEmpty(fs.XmlFSType.VolumeName)) volumeName = "NO NAME";
                                else volumeName = fs.XmlFSType.VolumeName;

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
                                                                                           volumeName, ".xattrs",
                                                                                           xattr));

                                                    outputPath =
                                                        Path.Combine(options.OutputDir, fs.XmlFSType.Type, volumeName,
                                                                     ".xattrs", xattr, entry);

                                                    if(!File.Exists(outputPath))
                                                    {
                                                        outputFile =
                                                            new FileStream(outputPath, FileMode.CreateNew,
                                                                           FileAccess.ReadWrite, FileShare.None);
                                                        outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                                        outputFile.Close();
                                                        FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                        try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                                        catch { }
                                                        try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                        catch { }
                                                        try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                        catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                                        DicConsole
                                                            .WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                                       xattrBuf.Length, xattr, entry, outputPath);
                                                    }
                                                    else
                                                        DicConsole
                                                            .ErrorWriteLine("Cannot write xattr {0} for {1}, output exists",
                                                                            xattr, entry);
                                                }
                                            }
                                        }
                                    }

                                    Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFSType.Type,
                                                                           volumeName));

                                    outputPath = Path.Combine(options.OutputDir, fs.XmlFSType.Type, volumeName, entry);

                                    if(!File.Exists(outputPath))
                                    {
                                        byte[] outBuf = new byte[0];

                                        error = fs.Read(entry, 0, stat.Length, ref outBuf);

                                        if(error == Errno.NoError)
                                        {
                                            outputFile = new FileStream(outputPath, FileMode.CreateNew,
                                                                        FileAccess.ReadWrite, FileShare.None);
                                            outputFile.Write(outBuf, 0, outBuf.Length);
                                            outputFile.Close();
                                            FileInfo fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                            try { fi.CreationTimeUtc = stat.CreationTimeUtc; }
                                            catch { }
                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                            catch { }
                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                            DicConsole.WriteLine("Written {0} bytes of file {1} to {2}", outBuf.Length,
                                                                 entry, outputPath);
                                        }
                                        else DicConsole.ErrorWriteLine("Error {0} reading file {1}", error, entry);
                                    }
                                    else DicConsole.ErrorWriteLine("Cannot write file {0}, output exists", entry);
                                }
                                else DicConsole.ErrorWriteLine("Error reading file {0}", entry);
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
                DicConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);
            }

            Core.Statistics.AddCommand("extract-files");
        }
    }
}