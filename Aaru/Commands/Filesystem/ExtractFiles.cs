// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ExtractFiles.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'extract' command.
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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using JetBrains.Annotations;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Commands.Filesystem
{
    internal sealed class ExtractFilesCommand : Command
    {
        public ExtractFilesCommand() : base("extract", "Extracts all files in disc image.")
        {
            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to filesystem plugin.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--xattrs", "-x"
                }, "Extract extended attributes if present.")
                {
                    Argument = new Argument<bool>(() => false),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--namespace", "-n"
                }, "Namespace to use for filenames.")
                {
                    Argument = new Argument<string>(() => null),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Disc image path",
                Name        = "image-path"
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
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("extract-files");

            AaruConsole.DebugWriteLine("Extract-Files command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Extract-Files command", "--encoding={0}", encoding);
            AaruConsole.DebugWriteLine("Extract-Files command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Extract-Files command", "--options={0}", options);
            AaruConsole.DebugWriteLine("Extract-Files command", "--output={0}", outputDir);
            AaruConsole.DebugWriteLine("Extract-Files command", "--verbose={0}", verbose);
            AaruConsole.DebugWriteLine("Extract-Files command", "--xattrs={0}", xattrs);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            AaruConsole.DebugWriteLine("Extract-Files command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                AaruConsole.DebugWriteLine("Extract-Files command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            parsedOptions.Add("debug", debug.ToString());

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            Encoding encodingClass = null;

            if(encoding != null)
                try
                {
                    encodingClass = Claunia.Encoding.Encoding.GetEncoding(encoding);

                    if(verbose)
                        AaruConsole.VerboseWriteLine("Using encoding for {0}.", encodingClass.EncodingName);
                }
                catch(ArgumentException)
                {
                    AaruConsole.ErrorWriteLine("Specified encoding is not supported.");

                    return (int)ErrorNumber.EncodingUnknown;
                }

            PluginBase plugins = GetPluginBase.Instance;

            try
            {
                IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

                if(imageFormat == null)
                {
                    AaruConsole.WriteLine("Image format not identified, not proceeding with analysis.");

                    return (int)ErrorNumber.UnrecognizedFormat;
                }

                if(verbose)
                    AaruConsole.VerboseWriteLine("Image format identified by {0} ({1}).", imageFormat.Name,
                                                 imageFormat.Id);
                else
                    AaruConsole.WriteLine("Image format identified by {0}.", imageFormat.Name);

                if(outputDir == null)
                {
                    AaruConsole.WriteLine("Output directory missing.");

                    return (int)ErrorNumber.MissingArgument;
                }

                if(Directory.Exists(outputDir) ||
                   File.Exists(outputDir))
                {
                    AaruConsole.ErrorWriteLine("Destination exists, aborting.");

                    return (int)ErrorNumber.DestinationExists;
                }

                Directory.CreateDirectory(outputDir);

                try
                {
                    if(!imageFormat.Open(inputFilter))
                    {
                        AaruConsole.WriteLine("Unable to open image format");
                        AaruConsole.WriteLine("No error given");

                        return (int)ErrorNumber.CannotOpenFormat;
                    }

                    AaruConsole.DebugWriteLine("Extract-Files command", "Correctly opened image file.");

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image without headers is {0} bytes.",
                                               imageFormat.Info.ImageSize);

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image has {0} sectors.",
                                               imageFormat.Info.Sectors);

                    AaruConsole.DebugWriteLine("Extract-Files command", "Image identifies disk type as {0}.",
                                               imageFormat.Info.MediaType);

                    Statistics.AddMediaFormat(imageFormat.Format);
                    Statistics.AddMedia(imageFormat.Info.MediaType, false);
                    Statistics.AddFilter(inputFilter.Name);
                }
                catch(Exception ex)
                {
                    AaruConsole.ErrorWriteLine("Unable to open image format");
                    AaruConsole.ErrorWriteLine("Error: {0}", ex.Message);

                    return (int)ErrorNumber.CannotOpenFormat;
                }

                List<Partition> partitions = Core.Partitions.GetAll(imageFormat);
                Core.Partitions.AddSchemesToStats(partitions);

                if(partitions.Count == 0)
                {
                    AaruConsole.DebugWriteLine("Ls command", "No partitions found");

                    partitions.Add(new Partition
                    {
                        Description = "Whole device",
                        Length      = imageFormat.Info.Sectors,
                        Offset      = 0,
                        Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                        Sequence    = 1,
                        Start       = 0
                    });
                }

                AaruConsole.WriteLine("{0} partitions found.", partitions.Count);

                for(int i = 0; i < partitions.Count; i++)
                {
                    AaruConsole.WriteLine();
                    AaruConsole.WriteLine("Partition {0}:", partitions[i].Sequence);

                    AaruConsole.WriteLine("Identifying filesystem on partition");

                    Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

                    if(idPlugins.Count == 0)
                        AaruConsole.WriteLine("Filesystem not identified");
                    else
                    {
                        IReadOnlyFilesystem plugin;
                        Errno               error;

                        if(idPlugins.Count > 1)
                        {
                            AaruConsole.WriteLine($"Identified by {idPlugins.Count} plugins");

                            foreach(string pluginName in idPlugins)
                                if(plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out plugin))
                                {
                                    AaruConsole.WriteLine($"As identified by {plugin.Name}.");

                                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                         Invoke(new object[]
                                                                                    {});

                                    error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions,
                                                     @namespace);

                                    if(error == Errno.NoError)
                                    {
                                        string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                                : fs.XmlFsType.VolumeName;

                                        ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                        Statistics.AddFilesystem(fs.XmlFsType.Type);
                                    }
                                    else
                                        AaruConsole.ErrorWriteLine("Unable to mount device, error {0}",
                                                                   error.ToString());
                                }
                        }
                        else
                        {
                            plugins.ReadOnlyFilesystems.TryGetValue(idPlugins[0], out plugin);

                            if(plugin == null)
                                continue;

                            AaruConsole.WriteLine($"Identified by {plugin.Name}.");

                            var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                 Invoke(new object[]
                                                                            {});

                            error = fs.Mount(imageFormat, partitions[i], encodingClass, parsedOptions, @namespace);

                            if(error == Errno.NoError)
                            {
                                string volumeName = string.IsNullOrEmpty(fs.XmlFsType.VolumeName) ? "NO NAME"
                                                        : fs.XmlFsType.VolumeName;

                                ExtractFilesInDir("/", fs, volumeName, outputDir, xattrs);

                                Statistics.AddFilesystem(fs.XmlFsType.Type);
                            }
                            else
                                AaruConsole.ErrorWriteLine("Unable to mount device, error {0}", error.ToString());
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine($"Error reading file: {ex.Message}");
                AaruConsole.DebugWriteLine("Extract-Files command", ex.StackTrace);

                return (int)ErrorNumber.UnexpectedException;
            }

            return (int)ErrorNumber.NoError;
        }

        static void ExtractFilesInDir(string path, [NotNull] IReadOnlyFilesystem fs, string volumeName,
                                      string outputDir, bool doXattrs)
        {
            if(path.StartsWith('/'))
                path = path.Substring(1);

            Errno error = fs.ReadDir(path, out List<string> directory);

            if(error != Errno.NoError)
            {
                AaruConsole.ErrorWriteLine("Error {0} reading root directory {0}", error.ToString());

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
                        outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path, entry);

                        Directory.CreateDirectory(outputPath);

                        AaruConsole.WriteLine("Created subdirectory at {0}", outputPath);

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
                                byte[] xattrBuf = Array.Empty<byte>();
                                error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                                if(error != Errno.NoError)
                                    continue;

                                outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, ".xattrs", path,
                                                          xattr);

                                Directory.CreateDirectory(outputPath);

                                outputPath = Path.Combine(outputPath, entry);

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
                                    AaruConsole.WriteLine("Written {0} bytes of xattr {1} from file {2} to {3}",
                                                          xattrBuf.Length, xattr, entry, outputPath);
                                }
                                else
                                    AaruConsole.ErrorWriteLine("Cannot write xattr {0} for {1}, output exists", xattr,
                                                               entry);
                            }
                    }

                    outputPath = Path.Combine(outputDir, fs.XmlFsType.Type, volumeName, path);

                    Directory.CreateDirectory(outputPath);

                    outputPath = Path.Combine(outputPath, entry);

                    if(!File.Exists(outputPath))
                    {
                        byte[] outBuf = Array.Empty<byte>();

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
                            AaruConsole.WriteLine("Written {0} bytes of file {1} to {2}", outBuf.Length, entry,
                                                  outputPath);
                        }
                        else
                            AaruConsole.ErrorWriteLine("Error {0} reading file {1}", error, entry);
                    }
                    else
                        AaruConsole.ErrorWriteLine("Cannot write file {0}, output exists", entry);
                }
                else
                    AaruConsole.ErrorWriteLine("Error reading file {0}", entry);
            }
        }
    }
}