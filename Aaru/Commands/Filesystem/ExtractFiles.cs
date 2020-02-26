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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using FileAttributes = DiscImageChef.CommonTypes.Structs.FileAttributes;

namespace DiscImageChef.Commands.Filesystem
{
    internal class ExtractFilesCommand : Command
    {
        public ExtractFilesCommand() : base("extract", "Extracts all files in disc image.")
        {
            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to filesystem plugin.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--xattrs", "-x"
                }, "Extract extended attributes if present.")
                {
                    Argument = new Argument<bool>(() => false), Required = false
                });

            Add(new Option(new[]
                {
                    "--namespace", "-n"
                }, "Namespace to use for filenames.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Disc image path", Name = "image-path"
            });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Directory where extracted files will be created. Will abort if it exists",
                Name        = "output-dir"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string encoding, bool xattrs, string imagePath,
                                 string @namespace, string outputDir, string options)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("extract-files");

            DicConsole.DebugWriteLine("Extract-Files command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Extract-Files command", "--encoding={0}", encoding);
            DicConsole.DebugWriteLine("Extract-Files command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Extract-Files command", "--options={0}", options);
            DicConsole.DebugWriteLine("Extract-Files command", "--output={0}", outputDir);
            DicConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", verbose);
            DicConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}", xattrs);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            DicConsole.DebugWriteLine("Extract-Files command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Extract-Files command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            parsedOptions.Add("debug", debug.ToString());

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            Encoding encodingClass = null;

            if(encoding != null)
                try
                {
                    encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                    if(verbose)
                        DicConsole.VerboseWriteLine("Using encoding for {0}.", encodingClass.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");

                    return(int)ErrorNumber.EncodingUnknown;
                }

            PluginBase plugins = GetPluginBase.Instance;

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    DicConsole.WriteLine("Image format not identified, not proceeding with analysis.");

                    return(int)ErrorNumber.UnrecognizedFormat;
                }

                if(verbose)
                    DicConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                imageFormat.Id);
                else
                    DicConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                if(outputDir == null)
                {
                    DicConsole.WriteLine("Output directory missing.");

                    return(int)ErrorNumber.MissingArgument;
                }

                if(Directory.Exists(outputDir) ||
                   File.Exists(outputDir))
                {
                    DicConsole.ErrorWriteLine("Destination exists, aborting.");

                    return(int)ErrorNumber.DestinationExists;
                }

                Directory.CreateDirectory(outputDir);

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        DicConsole.WriteLine("Unable to open image format");
                        DicConsole.WriteLine("No error given");

                        return(int)ErrorNumber.CannotOpenFormat;
                    }

                    DicConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");

                    DicConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.",
                                              imageFormat.Info.ImageSize);

                    DicConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.",
                                              imageFormat.Info.Sectors);

                    DicConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.",
                                              imageFormat.Info.MediaType);

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    DicConsole.ErrorWriteLine("Unable to open image format");
                    DicConsole.ErrorWriteLine("Error: {0}", ex.Message);

                    return(int)ErrorNumber.CannotOpenFormat;
                }

                List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                Core.Partitions.AddSchemesToStats(partitions);

                List<string>        idPlugins;
                IReadOnlyFilesystem plugin;
                Errno               error;

                if(partitions.Count == 0)
                    DicConsole.DebugWriteLine("Extract-Files command", "No partitions found");
                else
                {
                    DicConsole.WriteLine("{0} partitions found.", partitions.Count);

                    for(int i = 0; i < partitions.Count; i++)
                    {
                        DicConsole.WriteLine();
                        DicConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                        DicConsole.WriteLine("Identifying filesystem on partition");

                        Core.Filesystems.Identify(imageFormat, out idPlugins, partitions[i]);

                        if(idPlugins.Count == 0)
                            DicConsole.WriteLine("Filesystem not identified");
                        else if(idPlugins.Count > 1)
                        {
                            DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    DicConsole.WriteLine($"As identified by {plugin.Name}.");

                                    var fs = (IReadOnlyFilesystem)plugin.
                                                                  GetType().GetConstructor(Type.EmptyTypes)?.
                                                                  Invoke(new object[]
                                                                             { });

                                    error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions,
                                                     @namespace);

                                    if(error == Errno.NoError)
                                    {
                                        string volumeName =
                                            string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                : fs.XmlFsType.VolumeName;

                                        ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                        Statistics.AddFilesystem(fs.XmlFsType.Type);
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

                            var fs = (IReadOnlyFilesystem)plugin.
                                                          GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                                { });

                            error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }

                var wholePart = new Partition
                {
                    Name = "Whole device", Length = imageFormat.Info.Sectors,
                    Size = imageFormat.Info.Sectors * imageFormat.Info.SectorSize
                };

                Core.Filesystems.Identify(imageFormat, out idPlugins, wholePart);

                if(idPlugins.Count == 0)
                    DicConsole.WriteLine("Filesystem not identified");
                else if(idPlugins.Count > 1)
                {
                    DicConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                    foreach(string pluginName in idPlugins)
                        if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                        {
                            DicConsole.WriteLine($"As identified by {plugin.Name}.");

                            var fs = (IReadOnlyFilesystem)plugin.
                                                          GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                                { });

                            error = fs.Mount(imageFormat, wholePart, encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                }
                else
                {
                    plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);
                    DicConsole.WriteLine($"Identified by {plugin.Name}.");

                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                                                                                                               { });

                    error = fs.Mount(imageFormat, wholePart, encodingClass, parsedOptions, @namespace);

                    if(error == Errno.NoError)
                    {
                        string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                : fs.XmlFsType.VolumeName;

                        ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                        Statistics.AddFilesystem(fs.XmlFsType.Type);
                    }
                    else
                        DicConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                }
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                DicConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);

                return(int)ErrorNumber.UnexpectedException;
            }

            return(int)ErrorNumber.NoError;
        }

        static void ExtractFilesInDir(string path, IReadOnlyFilesystem fs, string volumeName, string outputDir,
                                      bool doXattrs)
        {
            if(path.StartsWith("/"))
                path = path.Substring(1);

            Errno error = fs.ReadDir(path, out List<string> directory);

            if(error != Errno.NoError)
            {
                DicConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

                return;
            }

            foreach(string entry in directory)
            {
                error = fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                if(error == Errno.NoError)
                {
                    string outputPath;

                    if(stat.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        Directory.CreateDirectory(Path.Combine(outputDir, fs.XmlFsType.Type, volumeName));

                        outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path, entry);

                        Directory.CreateDirectory(outputPath);

                        DicConsole.WriteLine("Created subdirectory at {0}", outputPath);

                        ExtractFilesInDir(path + "/" + entry, fs, volumeName, outputDir, doXattrs);

                        var di = new DirectoryInfo(outputPath);

                        #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        try
                        {
                            if(stat.CreationTimeUtc.HasValue)
                                di.CreationTimeUtc = stat.CreationTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.LastWriteTimeUtc.HasValue)
                                di.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }

                        try
                        {
                            if(stat.AccessTimeUtc.HasValue)
                                di.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                        }
                        catch
                        {
                            // ignored
                        }
                        #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

                        continue;
                    }

                    FileStream outputFile;

                    if(doXattrs)
                    {
                        error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                        if(error == Errno.NoError)
                            foreach(string xattr in xattrs)
                            {
                                byte[] xattrBuf = new byte[0];
                                error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                                if(error != Errno.NoError)
                                    continue;

                                Directory.CreateDirectory(Path.Combine(outputDir, fs.XmlFsType.Type, volumeName,
                                                                       ".xattrs", xattr));

                                outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, ".xattrs", xattr,
                                                          path, entry);

                                if(!File.Exists(outputPath))
                                {
                                    outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                                FileShare.None);

                                    outputFile.Write(xattrBuf, 0, xattrBuf.Length);
                                    outputFile.Close();
                                    var fi = new FileInfo(outputPath);
                                    #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    try
                                    {
                                        if(stat.CreationTimeUtc.HasValue)
                                            fi.CreationTimeUtc = stat.CreationTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }

                                    try
                                    {
                                        if(stat.LastWriteTimeUtc.HasValue)
                                            fi.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }

                                    try
                                    {
                                        if(stat.AccessTimeUtc.HasValue)
                                            fi.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                                    }
                                    catch
                                    {
                                        // ignored
                                    }
                                    #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                                    DicConsole.WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                         xattrBuf.Length, xattr, entry, outputPath);
                                }
                                else
                                    DicConsole.ErrorWriteLine("Cannot write xattr {0} for {1}, output exists", xattr,
                                                              entry);
                            }
                    }

                    Directory.CreateDirectory(Path.Combine(outputDir, fs.XmlFsType.Type, volumeName));

                    outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path, entry);

                    if(!File.Exists(outputPath))
                    {
                        byte[] outBuf = new byte[0];

                        error = fs.Read(path + "/" + entry, 0, stat.Length, ref outBuf);

                        if(error == Errno.NoError)
                        {
                            outputFile = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite,
                                                        FileShare.None);

                            outputFile.Write(outBuf, 0, outBuf.Length);
                            outputFile.Close();
                            var fi = new FileInfo(outputPath);
                            #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                            try
                            {
                                if(stat.CreationTimeUtc.HasValue)
                                    fi.CreationTimeUtc = stat.CreationTimeUtc.Value;
                            }
                            catch
                            {
                                // ignored
                            }

                            try
                            {
                                if(stat.LastWriteTimeUtc.HasValue)
                                    fi.LastWriteTimeUtc = stat.LastWriteTimeUtc.Value;
                            }
                            catch
                            {
                                // ignored
                            }

                            try
                            {
                                if(stat.AccessTimeUtc.HasValue)
                                    fi.LastAccessTimeUtc = stat.AccessTimeUtc.Value;
                            }
                            catch
                            {
                                // ignored
                            }
                            #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                            DicConsole.WriteLine("Written {0} bytes of file {1} to {2}", outBuf.Length, entry,
                                                 outputPath);
                        }
                        else
                            DicConsole.ErrorWriteLine("Error {0} reading file {1}", error, entry);
                    }
                    else
                        DicConsole.ErrorWriteLine("Cannot write file {0}, output exists", entry);
                }
                else
                    DicConsole.ErrorWriteLine("Error reading file {0}", entry);
            }
        }
    }
}