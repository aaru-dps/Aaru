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
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    // TODO: Rewrite this, has an insane amount of repeating code ;)
    static class ExtractFiles
    {
        internal static void DoExtractFiles(ExtractFilesOptions options)
        {
            DicConsole.DebugWriteLine("Extract-Files command", "--debug={0}",   options.Debug);
            DicConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Extract-Files command", "--input={0}",   options.InputFile);
            DicConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}",  options.Xattrs);
            DicConsole.DebugWriteLine("Extract-Files command", "--output={0}",  options.OutputDir);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(options.InputFile);

            Dictionary<string, string> parsedOptions = Options.Parse(options.Options);
            DicConsole.DebugWriteLine("Extract-Files command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Extract-Files command", "{0} = {1}", parsedOption.Key, parsedOption.Value);
            parsedOptions.Add("debug", options.Debug.ToString());

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return;
            }

            Encoding encoding = null;

            if(options.EncodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(options.EncodingName);
                    if(options.Verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return;
                }

            PluginBase plugins = GetPluginBase.Instance;

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");
                    return;
                }

                if(options.Verbose)
                    DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                imageFormat.Id);
                else DicConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                if(Directory.Exists(options.OutputDir) || File.Exists(options.OutputDir))
                {
                    DicConsole.ErrorWriteLine("Destination exists, aborting.");
                    return;
                }

                Directory.CreateDirectory(options.OutputDir);

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");
                        return;
                    }

                    DicConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");
                    DicConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.",
                                              imageFormat.Info.ImageSize);
                    DicConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.",
                                              imageFormat.Info.Sectors);
                    DicConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.",
                                              imageFormat.Info.MediaType);

                    Core.Statistics.AddMediaFormat(imageFormat.Format);
                    Core.Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Core.Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                    return;
                }

                List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                Core.Partitions.AddSchemesToStats(partitions);

                List<string>        idPlugins;
                IReadOnlyFilesystem plugin;
                Errno               error;
                if(partitions.Count == 0) DicConsole.DebugWriteLine("Extract-Files command", "No partitions found");
                else
                {
                    DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                    for(int i = 0; i < partitions.Count; i++)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                        DicConsole.WriteLine("Identifying filesystem on partition");

                        Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);
                        if(idPlugins.Count      == 0) DicConsole.WriteLine("Filesystem not identified");
                        else if(idPlugins.Count > 1)
                        {
                            DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    DicConsole.WriteLine($"As identified by {plugin.Name}.");
                                    IReadOnlyFilesystem fs = (IReadOnlyFilesystem)plugin
                                                                                 .GetType()
                                                                                 .GetConstructor(Type.EmptyTypes)
                                                                                ?.Invoke(new object[] { });

                                    error = fs.Mount(imageFormat, partitions[i], encoding, parsedOptions);
                                    if(error == Errno.NoError)
                                    {
                                        error = fs.ReadDir("/", out List<string> rootDir);
                                        if(error == Errno.NoError)
                                            foreach(string entry in rootDir)
                                            {
                                                string volumeName =
                                                    string.IsNullOrEmpty(fs.XmlFsType.VolumeName)
                                                        ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                                error = fs.Stat(entry, out FileEntryInfo stat);
                                                if(error == Errno.NoError)
                                                {
                                                    string     outputPath;
                                                    FileStream outputFile;
                                                    if(options.Xattrs)
                                                    {
                                                        error = fs.ListXAttr(entry, out List<string> xattrs);
                                                        if(error == Errno.NoError)
                                                            foreach(string xattr in xattrs)
                                                            {
                                                                byte[] xattrBuf = new byte[0];
                                                                error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                                if(error != Errno.NoError) continue;

                                                                Directory
                                                                   .CreateDirectory(Path.Combine(options.OutputDir,
                                                                                                 fs.XmlFsType.Type,
                                                                                                 volumeName, ".xattrs",
                                                                                                 xattr));

                                                                outputPath =
                                                                    Path.Combine(options.OutputDir, fs.XmlFsType.Type,
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
                                                                    catch
                                                                    {
                                                                        // ignored
                                                                    }

                                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                                    catch
                                                                    {
                                                                        // ignored
                                                                    }

                                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                                    catch
                                                                    {
                                                                        // ignored
                                                                    }
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

                                                    Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                           fs.XmlFsType.Type,
                                                                                           volumeName));

                                                    outputPath =
                                                        Path.Combine(options.OutputDir, fs.XmlFsType.Type, volumeName,
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
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }
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
                                        else
                                            DicConsole.ErrorWriteLine("Error {0} reading root directory {0}",
                                                                      error.ToString());

                                        Core.Statistics.AddFilesystem(fs.XmlFsType.Type);
                                    }
                                    else
                                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}",
                                                                  error.ToString());
                                }
                        }
                        else
                        {
                            plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);
                            DicConsole.WriteLine($"Identified by {plugin.Name}.");
                            IReadOnlyFilesystem fs = (IReadOnlyFilesystem)plugin
                                                                         .GetType().GetConstructor(Type.EmptyTypes)
                                                                        ?.Invoke(new object[] { });
                            error = fs.Mount(imageFormat, partitions[i], encoding, parsedOptions);
                            if(error == Errno.NoError)
                            {
                                error = fs.ReadDir("/", out List<string> rootDir);
                                if(error == Errno.NoError)
                                    foreach(string entry in rootDir)
                                    {
                                        string volumeName =
                                            string.IsNullOrEmpty(fs.XmlFsType.VolumeName)
                                                ? "NO NAME"
                                                : fs.XmlFsType.VolumeName;

                                        error = fs.Stat(entry, out FileEntryInfo stat);
                                        if(error == Errno.NoError)
                                        {
                                            FileStream outputFile;
                                            string     outputPath;
                                            if(options.Xattrs)
                                            {
                                                error = fs.ListXAttr(entry, out List<string> xattrs);
                                                if(error == Errno.NoError)
                                                    foreach(string xattr in xattrs)
                                                    {
                                                        byte[] xattrBuf = new byte[0];
                                                        error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                        if(error != Errno.NoError) continue;

                                                        Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                               fs.XmlFsType.Type,
                                                                                               volumeName, ".xattrs",
                                                                                               xattr));

                                                        outputPath =
                                                            Path.Combine(options.OutputDir, fs.XmlFsType.Type,
                                                                         volumeName, ".xattrs", xattr, entry);

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
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }
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

                                            Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFsType.Type,
                                                                                   volumeName));

                                            outputPath =
                                                Path.Combine(options.OutputDir, fs.XmlFsType.Type, volumeName, entry);

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
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }
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
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }

                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = imageFormat.Info.Sectors,
                    Size   = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                };

                Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);
                if(idPlugins.Count      == 0) DicConsole.WriteLine("Filesystem not identified");
                else if(idPlugins.Count > 1)
                {
                    DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                    foreach(string pluginName in idPlugins)
                        if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                        {
                            DicConsole.WriteLine($"As identified by {plugin.Name}.");
                            IReadOnlyFilesystem fs = (IReadOnlyFilesystem)plugin
                                                                         .GetType().GetConstructor(Type.EmptyTypes)
                                                                        ?.Invoke(new object[] { });
                            error = fs.Mount(imageFormat, wholePart, encoding, parsedOptions);
                            if(error == Errno.NoError)
                            {
                                error = fs.ReadDir("/", out List<string> rootDir);
                                if(error == Errno.NoError)
                                    foreach(string entry in rootDir)
                                    {
                                        string volumeName =
                                            string.IsNullOrEmpty(fs.XmlFsType.VolumeName)
                                                ? "NO NAME"
                                                : fs.XmlFsType.VolumeName;

                                        error = fs.Stat(entry, out FileEntryInfo stat);
                                        if(error == Errno.NoError)
                                        {
                                            FileStream outputFile;
                                            string     outputPath;
                                            if(options.Xattrs)
                                            {
                                                error = fs.ListXAttr(entry, out List<string> xattrs);
                                                if(error == Errno.NoError)
                                                    foreach(string xattr in xattrs)
                                                    {
                                                        byte[] xattrBuf = new byte[0];
                                                        error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                        if(error != Errno.NoError) continue;

                                                        Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                               fs.XmlFsType.Type,
                                                                                               volumeName, ".xattrs",
                                                                                               xattr));

                                                        outputPath =
                                                            Path.Combine(options.OutputDir, fs.XmlFsType.Type,
                                                                         volumeName, ".xattrs", xattr, entry);

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
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }

                                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                            catch
                                                            {
                                                                // ignored
                                                            }
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

                                            Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFsType.Type,
                                                                                   volumeName));

                                            outputPath =
                                                Path.Combine(options.OutputDir, fs.XmlFsType.Type, volumeName, entry);

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
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }
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
                                else
                                    DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                                Core.Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                }
                else
                {
                    plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);
                    DicConsole.WriteLine($"Identified by {plugin.Name}.");
                    IReadOnlyFilesystem fs =
                        (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
                    error = fs.Mount(imageFormat, wholePart, encoding, parsedOptions);
                    if(error == Errno.NoError)
                    {
                        error = fs.ReadDir("/", out List<string> rootDir);
                        if(error == Errno.NoError)
                            foreach(string entry in rootDir)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName)
                                                        ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                error = fs.Stat(entry, out FileEntryInfo stat);
                                if(error == Errno.NoError)
                                {
                                    string     outputPath;
                                    FileStream outputFile;
                                    if(options.Xattrs)
                                    {
                                        error = fs.ListXAttr(entry, out List<string> xattrs);
                                        if(error == Errno.NoError)
                                            foreach(string xattr in xattrs)
                                            {
                                                byte[] xattrBuf = new byte[0];
                                                error = fs.GetXattr(entry, xattr, ref xattrBuf);
                                                if(error != Errno.NoError) continue;

                                                Directory.CreateDirectory(Path.Combine(options.OutputDir,
                                                                                       fs.XmlFsType.Type, volumeName,
                                                                                       ".xattrs", xattr));

                                                outputPath =
                                                    Path.Combine(options.OutputDir, fs.XmlFsType.Type, volumeName,
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
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }

                                                    try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                                    catch
                                                    {
                                                        // ignored
                                                    }
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

                                    Directory.CreateDirectory(Path.Combine(options.OutputDir, fs.XmlFsType.Type,
                                                                           volumeName));

                                    outputPath = Path.Combine(options.OutputDir, fs.XmlFsType.Type, volumeName, entry);

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
                                            catch
                                            {
                                                // ignored
                                            }

                                            try { fi.LastWriteTimeUtc = stat.LastWriteTimeUtc; }
                                            catch
                                            {
                                                // ignored
                                            }

                                            try { fi.LastAccessTimeUtc = stat.AccessTimeUtc; }
                                            catch
                                            {
                                                // ignored
                                            }
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
                        else DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                        Core.Statistics.AddFilesystem(fs.XmlFsType.Type);
                    }
                    else DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);
            }

            Core.Statistics.AddCommand("extract-files");
        }
    }
}