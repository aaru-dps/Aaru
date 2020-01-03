// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DumpMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'dump-media' verb.
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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using Schemas;

namespace DiscImageChef.Commands.Media
{
    // TODO: Add raw dumping
    internal class DumpMediaCommand : Command
    {
        public DumpMediaCommand() : base("dump", "Dumps the media inserted on a device to a media image.")
        {
            Add(new Option(new[]
                {
                    "--cicm-xml", "-x"
                }, "Take metadata from existing CICM XML sidecar.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--encoding", "-e"
                }, "Name of character encoding to use.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option("--first-pregap", "Try to read first track pregap. Only applicable to CD/DDCD/GD.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Add(new Option("--fix-offset", "Fix audio tracks offset. Only applicable to CD/GD.")
            {
                Argument = new Argument<bool>(() => true), Required = false
            });

            Add(new Option(new[]
                {
                    "--force", "-f"
                }, "Continue dump whatever happens.")
                {
                    Argument = new Argument<bool>(() => false), Required = false
                });

            Add(new Option(new[]
                           {
                               "--format", "-t"
                           },
                           "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--no-metadata", "Disables creating CICM XML sidecar.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Add(new Option("--no-trim", "Disables trimming errored from skipped sectors.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to output image plugin.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option("--persistent", "Try to recover partial or incorrect data.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Add(new Option(new[]
                {
                    "--resume", "-r"
                }, "Create/use resume mapfile.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--retry-passes", "-p"
                }, "How many retry passes to do.")
                {
                    Argument = new Argument<ushort>(() => 5), Required = false
                });

            Add(new Option(new[]
                {
                    "--skip", "-k"
                }, "When an unreadable sector is found skip this many sectors.")
                {
                    Argument = new Argument<uint>(() => 512), Required = false
                });

            Add(new Option(new[]
                {
                    "--stop-on-error", "-s"
                }, "Stop media dump on first error.")
                {
                    Argument = new Argument<bool>(() => false), Required = false
                });

            Add(new Option("--subchannel",
                           "Subchannel to dump. Only applicable to CD/GD. Values: any, rw, rw-or-pq, pq, none.")
            {
                Argument = new Argument<string>(() => "any"), Required = false
            });

            Add(new Option("--speed", "Speed to dump. Only applicable to optical drives, 0 for maximum.")
            {
                Argument = new Argument<byte>(() => 0), Required = false
            });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Device path", Name = "device-path"
            });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Output image path", Name = "output-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool debug, bool verbose, string cicmXml, string devicePath, bool resume,
                                 string encoding, bool firstPregap, bool fixOffset, bool force, bool noMetadata,
                                 bool noTrim, string outputPath, string options, bool persistent, ushort retryPasses,
                                 uint skip, byte speed, bool stopOnError, string format, string subchannel)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("dump-media");

            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}", cicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}", devicePath);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}", encoding);
            DicConsole.DebugWriteLine("Dump-Media command", "--first-pregap={0}", firstPregap);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}", force);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}", force);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}", format);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}", noMetadata);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}", options);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}", outputPath);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}", persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}", resume);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}", retryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}", skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", stopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", verbose);
            DicConsole.DebugWriteLine("Dump-Media command", "--subchannel={0}", subchannel);

            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           raw);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

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

            DumpSubchannel wantedSubchannel = DumpSubchannel.Any;

            switch(subchannel?.ToLowerInvariant())
            {
                case"any":
                case null:
                    wantedSubchannel = DumpSubchannel.Any;

                    break;
                case"rw":
                    wantedSubchannel = DumpSubchannel.Rw;

                    break;
                case"rw-or-pq":
                    wantedSubchannel = DumpSubchannel.RwOrPq;

                    break;
                case"pq":
                    wantedSubchannel = DumpSubchannel.Pq;

                    break;
                case"none":
                    wantedSubchannel = DumpSubchannel.None;

                    break;
                default:
                    DicConsole.WriteLine("Incorrect subchannel type \"{0}\" requested.", subchannel);

                    break;
            }

            if(devicePath.Length == 2   &&
               devicePath[1]     == ':' &&
               devicePath[0]     != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Devices.Device dev;

            try
            {
                dev = new Devices.Device(devicePath);

                if(dev.IsRemote)
                    Statistics.AddRemote(dev.RemoteApplication, dev.RemoteVersion, dev.RemoteOperatingSystem,
                                         dev.RemoteOperatingSystemVersion, dev.RemoteArchitecture);

                if(dev.Error)
                {
                    DicConsole.ErrorWriteLine(Error.Print(dev.LastError));

                    return(int)ErrorNumber.CannotOpenDevice;
                }
            }
            catch(DeviceException e)
            {
                DicConsole.ErrorWriteLine(e.Message ?? Error.Print(e.LastError));

                return(int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(outputPath),
                                               Path.GetFileNameWithoutExtension(outputPath));

            Resume resumeClass = null;
            var    xs          = new XmlSerializer(typeof(Resume));

            if(File.Exists(outputPrefix + ".resume.xml") && resume)
                try
                {
                    var sr = new StreamReader(outputPrefix + ".resume.xml");
                    resumeClass = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");

                    return(int)ErrorNumber.InvalidResume;
                }

            if(resumeClass                 != null                 &&
               resumeClass.NextBlock       > resumeClass.LastBlock &&
               resumeClass.BadBlocks.Count == 0                    &&
               !resumeClass.Tape)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");

                return(int)ErrorNumber.AlreadyDumped;
            }

            CICMMetadataType sidecar   = null;
            var              sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            if(cicmXml != null)
                if(File.Exists(cicmXml))
                {
                    try
                    {
                        var sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");

                        return(int)ErrorNumber.InvalidSidecar;
                    }
                }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");

                    return(int)ErrorNumber.FileNotFound;
                }

            PluginBase           plugins    = GetPluginBase.Instance;
            List<IWritableImage> candidates = new List<IWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(format))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions.
                                                                              Contains(Path.GetExtension(outputPath))));

            // Try Id
            else if(Guid.TryParse(format, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));

            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, format,
                                                                                           StringComparison.
                                                                                               InvariantCultureIgnoreCase)));

            if(candidates.Count == 0)
            {
                DicConsole.WriteLine("No plugin supports requested extension.");

                return(int)ErrorNumber.FormatNotFound;
            }

            if(candidates.Count > 1)
            {
                DicConsole.WriteLine("More than one plugin supports requested extension.");

                return(int)ErrorNumber.TooManyFormats;
            }

            IWritableImage outputFormat = candidates[0];

            var dumpLog = new DumpLog(outputPrefix + ".log", dev);

            if(verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);
                DicConsole.WriteLine("Output image format: {0}.", outputFormat.Name);
            }

            var dumper = new Dump(resume, dev, devicePath, outputFormat, retryPasses, force, false, persistent,
                                  stopOnError, resumeClass, dumpLog, encodingClass, outputPrefix, outputPath,
                                  parsedOptions, sidecar, skip, noMetadata, noTrim, firstPregap, fixOffset, debug,
                                  wantedSubchannel, speed);

            dumper.UpdateStatus         += Progress.UpdateStatus;
            dumper.ErrorMessage         += Progress.ErrorMessage;
            dumper.StoppingErrorMessage += Progress.ErrorMessage;
            dumper.UpdateProgress       += Progress.UpdateProgress;
            dumper.PulseProgress        += Progress.PulseProgress;
            dumper.InitProgress         += Progress.InitProgress;
            dumper.EndProgress          += Progress.EndProgress;
            dumper.InitProgress2        += Progress.InitProgress2;
            dumper.EndProgress2         += Progress.EndProgress2;
            dumper.UpdateProgress2      += Progress.UpdateProgress2;

            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                dumper.Abort();
            };

            dumper.Start();

            dev.Close();

            return(int)ErrorNumber.NoError;
        }
    }
}