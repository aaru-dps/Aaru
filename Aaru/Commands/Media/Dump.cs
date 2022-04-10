// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dump.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'dump' command.
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
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2020-2022 Rebecca Wallander
// ****************************************************************************/

namespace Aaru.Commands.Media;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Devices.Dumping;
using Aaru.Core.Logging;
using Aaru.Devices;
using Schemas;
using Spectre.Console;

// TODO: Add raw dumping
sealed class DumpMediaCommand : Command
{
    static ProgressTask _progressTask1;
    static ProgressTask _progressTask2;

    public DumpMediaCommand() : base("dump", "Dumps the media inserted on a device to a media image.")
    {
        Add(new Option<string>(new[]
        {
            "--cicm-xml", "-x"
        }, () => null, "Take metadata from existing CICM XML sidecar."));

        Add(new Option<string>(new[]
        {
            "--encoding", "-e"
        }, () => null, "Name of character encoding to use."));

        Add(new Option<bool>("--first-pregap", () => false,
                             "Try to read first track pregap. Only applicable to CD/DDCD/GD."));

        Add(new Option<bool>("--fix-offset", () => true, "Fix audio tracks offset. Only applicable to CD/GD."));

        Add(new Option<bool>(new[]
        {
            "--force", "-f"
        }, () => false, "Continue dump whatever happens."));

        Add(new Option<string>(new[]
                               {
                                   "--format", "-t"
                               }, () => null,
                               "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension."));

        Add(new Option<bool>("--metadata", () => true, "Enables creating CICM XML sidecar."));

        Add(new Option<bool>("--trim", () => true, "Enables trimming errored from skipped sectors."));

        Add(new Option<string>(new[]
        {
            "--options", "-O"
        }, () => null, "Comma separated name=value pairs of options to pass to output image plugin."));

        Add(new Option<bool>("--persistent", () => false, "Try to recover partial or incorrect data."));

        Add(new Option<bool>(new[]
        {
            "--resume", "-r"
        }, () => true, "Create/use resume mapfile."));

        Add(new Option<ushort>(new[]
        {
            "--retry-passes", "-p"
        }, () => 5, "How many retry passes to do."));

        Add(new Option<uint>(new[]
        {
            "--skip", "-k"
        }, () => 512, "When an unreadable sector is found skip this many sectors."));

        Add(new Option<bool>(new[]
        {
            "--stop-on-error", "-s"
        }, () => false, "Stop media dump on first error."));

        Add(new Option<string>("--subchannel", () => "any",
                               "Subchannel to dump. Only applicable to CD/GD. Values: any, rw, rw-or-pq, pq, none."));

        Add(new Option<byte>("--speed", () => 0, "Speed to dump. Only applicable to optical drives, 0 for maximum."));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Device path",
            Name        = "device-path"
        });

        AddArgument(new Argument<string>
        {
            Arity = ArgumentArity.ExactlyOne,
            Description =
                "Output image path. If filename starts with # and exists, it will be read as a list of output images, its extension will be used to detect the image output format, each media will be ejected and confirmation for the next one will be asked.",
            Name = "output-path"
        });

        Add(new Option<bool>(new[]
        {
            "--private"
        }, () => false, "Do not store paths and serial numbers in log or metadata."));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel-position"
        }, () => true, "Store subchannel according to the sector they describe."));

        Add(new Option<bool>(new[]
        {
            "--retry-subchannel"
        }, () => true, "Retry subchannel. Implies fixing subchannel position."));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel"
        }, () => false, "Try to fix subchannel. Implies fixing subchannel position."));

        Add(new Option<bool>(new[]
        {
            "--fix-subchannel-crc"
        }, () => false, "If subchannel looks OK but CRC fails, rewrite it. Implies fixing subchannel."));

        Add(new Option<bool>(new[]
        {
            "--generate-subchannels"
        }, () => false, "Generates missing subchannels (they don't count as dumped in resume file)."));

        Add(new Option<bool>(new[]
        {
            "--skip-cdiready-hole"
        }, () => true, "Skip the hole between data and audio in a CD-i Ready disc."));

        Add(new Option<bool>(new[]
        {
            "--eject"
        }, () => false, "Eject media after dump finishes."));

        Add(new Option<uint>(new[]
        {
            "--max-blocks"
        }, () => 64, "Maximum number of blocks to read at once."));

        Add(new Option<bool>(new[]
        {
            "--use-buffered-reads"
        }, () => true, "For MMC/SD, use OS buffered reads if CMD23 is not supported."));

        Add(new Option<bool>(new[]
        {
            "--store-encrypted"
        }, () => true, "Store encrypted data as is."));

        Add(new Option<bool>(new[]
        {
            "--title-keys"
        }, () => true, "Try to read the title keys from CSS encrypted DVDs (very slow)."));

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string cicmXml, string devicePath, bool resume, string encoding,
                             bool firstPregap, bool fixOffset, bool force, bool metadata, bool trim, string outputPath,
                             string options, bool persistent, ushort retryPasses, uint skip, byte speed,
                             bool stopOnError, string format, string subchannel, bool @private,
                             bool fixSubchannelPosition, bool retrySubchannel, bool fixSubchannel,
                             bool fixSubchannelCrc, bool generateSubchannels, bool skipCdiReadyHole, bool eject,
                             uint maxBlocks, bool useBufferedReads, bool storeEncrypted, bool titleKeys)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };

        fixSubchannel         |= fixSubchannelCrc;
        fixSubchannelPosition |= retrySubchannel || fixSubchannel;

        if(maxBlocks == 0)
            maxBlocks = 64;

        Statistics.AddCommand("dump-media");

        AaruConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}", cicmXml);
        AaruConsole.DebugWriteLine("Dump-Media command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Dump-Media command", "--device={0}", devicePath);
        AaruConsole.DebugWriteLine("Dump-Media command", "--encoding={0}", encoding);
        AaruConsole.DebugWriteLine("Dump-Media command", "--first-pregap={0}", firstPregap);
        AaruConsole.DebugWriteLine("Dump-Media command", "--fix-offset={0}", fixOffset);
        AaruConsole.DebugWriteLine("Dump-Media command", "--force={0}", force);
        AaruConsole.DebugWriteLine("Dump-Media command", "--format={0}", format);
        AaruConsole.DebugWriteLine("Dump-Media command", "--metadata={0}", metadata);
        AaruConsole.DebugWriteLine("Dump-Media command", "--options={0}", options);
        AaruConsole.DebugWriteLine("Dump-Media command", "--output={0}", outputPath);
        AaruConsole.DebugWriteLine("Dump-Media command", "--persistent={0}", persistent);
        AaruConsole.DebugWriteLine("Dump-Media command", "--resume={0}", resume);
        AaruConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}", retryPasses);
        AaruConsole.DebugWriteLine("Dump-Media command", "--skip={0}", skip);
        AaruConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", stopOnError);
        AaruConsole.DebugWriteLine("Dump-Media command", "--trim={0}", trim);
        AaruConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", verbose);
        AaruConsole.DebugWriteLine("Dump-Media command", "--subchannel={0}", subchannel);
        AaruConsole.DebugWriteLine("Dump-Media command", "--private={0}", @private);
        AaruConsole.DebugWriteLine("Dump-Media command", "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruConsole.DebugWriteLine("Dump-Media command", "--retry-subchannel={0}", retrySubchannel);
        AaruConsole.DebugWriteLine("Dump-Media command", "--fix-subchannel={0}", fixSubchannel);
        AaruConsole.DebugWriteLine("Dump-Media command", "--fix-subchannel-crc={0}", fixSubchannelCrc);
        AaruConsole.DebugWriteLine("Dump-Media command", "--generate-subchannels={0}", generateSubchannels);
        AaruConsole.DebugWriteLine("Dump-Media command", "--skip-cdiready-hole={0}", skipCdiReadyHole);
        AaruConsole.DebugWriteLine("Dump-Media command", "--eject={0}", eject);
        AaruConsole.DebugWriteLine("Dump-Media command", "--max-blocks={0}", maxBlocks);
        AaruConsole.DebugWriteLine("Dump-Media command", "--use-buffered-reads={0}", useBufferedReads);
        AaruConsole.DebugWriteLine("Dump-Media command", "--store-encrypted={0}", storeEncrypted);
        AaruConsole.DebugWriteLine("Dump-Media command", "--title-keys={0}", titleKeys);

        // TODO: Disabled temporarily
        //AaruConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           raw);

        Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
        AaruConsole.DebugWriteLine("Dump-Media command", "Parsed options:");

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

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

        DumpSubchannel wantedSubchannel = DumpSubchannel.Any;

        switch(subchannel?.ToLowerInvariant())
        {
            case "any":
            case null:
                wantedSubchannel = DumpSubchannel.Any;

                break;
            case "rw":
                wantedSubchannel = DumpSubchannel.Rw;

                break;
            case "rw-or-pq":
                wantedSubchannel = DumpSubchannel.RwOrPq;

                break;
            case "pq":
                wantedSubchannel = DumpSubchannel.Pq;

                break;
            case "none":
                wantedSubchannel = DumpSubchannel.None;

                break;
            default:
                AaruConsole.WriteLine("Incorrect subchannel type \"{0}\" requested.", subchannel);

                break;
        }

        string filename = Path.GetFileNameWithoutExtension(outputPath);

        bool isResponse = filename.StartsWith("#", StringComparison.OrdinalIgnoreCase) &&
                          File.Exists(Path.Combine(Path.GetDirectoryName(outputPath),
                                                   Path.GetFileNameWithoutExtension(outputPath)));

        TextReader resReader;

        if(isResponse)
            resReader = new StreamReader(Path.Combine(Path.GetDirectoryName(outputPath),
                                                      Path.GetFileNameWithoutExtension(outputPath)));
        else
            resReader = new StringReader(Path.GetFileNameWithoutExtension(outputPath));

        if(isResponse)
            eject = true;

        PluginBase               plugins    = GetPluginBase.Instance;
        List<IBaseWritableImage> candidates = new();
        string                   extension  = Path.GetExtension(outputPath);

        // Try extension
        if(string.IsNullOrEmpty(format))
            candidates.AddRange(plugins.WritableImages.Values.Where(t => t.KnownExtensions.Contains(extension)));

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
            AaruConsole.WriteLine("No plugin supports requested extension.");

            return (int)ErrorNumber.FormatNotFound;
        }

        if(candidates.Count > 1)
        {
            AaruConsole.WriteLine("More than one plugin supports requested extension.");

            return (int)ErrorNumber.TooManyFormats;
        }

        while(true)
        {
            string responseLine = resReader.ReadLine();

            if(responseLine is null)
                break;

            if(responseLine.Any(c => c < 0x20))
            {
                AaruConsole.ErrorWriteLine("Invalid characters found in list of files, exiting...");

                return (int)ErrorNumber.InvalidArgument;
            }

            if(isResponse)
            {
                AaruConsole.WriteLine("Please insert media with title {0} and press any key to continue...",
                                      responseLine);

                Console.ReadKey();
                Thread.Sleep(1000);
            }

            responseLine = responseLine.Replace('/', '／');

            // Replace Windows forbidden filename characters with Japanese equivalents that are visually the same, but bigger.
            if(DetectOS.IsWindows)
                responseLine = responseLine.Replace('<', '\uFF1C').Replace('>', '\uFF1E').Replace(':', '\uFF1A').
                                            Replace('"', '\u2033').Replace('\\', '＼').Replace('|', '｜').
                                            Replace('?', '？').Replace('*', '＊');

            if(devicePath.Length == 2   &&
               devicePath[1]     == ':' &&
               devicePath[0]     != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device      dev      = null;
            ErrorNumber devErrno = ErrorNumber.NoError;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening device...").IsIndeterminate();
                dev = Device.Create(devicePath, out devErrno);
            });

            switch(dev)
            {
                case null:
                {
                    AaruConsole.ErrorWriteLine($"Could not open device, error {devErrno}.");

                    if(isResponse)
                        continue;

                    return (int)devErrno;
                }
                case Devices.Remote.Device remoteDev:
                    Statistics.AddRemote(remoteDev.RemoteApplication, remoteDev.RemoteVersion,
                                         remoteDev.RemoteOperatingSystem, remoteDev.RemoteOperatingSystemVersion,
                                         remoteDev.RemoteArchitecture);

                    break;
            }

            if(dev.Error)
            {
                AaruConsole.ErrorWriteLine(Error.Print(dev.LastError));

                if(isResponse)
                    continue;

                return (int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(outputPath), responseLine);

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
                    AaruConsole.ErrorWriteLine("Incorrect resume file, not continuing...");

                    if(isResponse)
                        continue;

                    return (int)ErrorNumber.InvalidResume;
                }

            if(resumeClass                 != null                                               &&
               resumeClass.NextBlock       > resumeClass.LastBlock                               &&
               resumeClass.BadBlocks.Count == 0                                                  &&
               !resumeClass.Tape                                                                 &&
               (resumeClass.BadSubchannels is null   || resumeClass.BadSubchannels.Count   == 0) &&
               (resumeClass.MissingTitleKeys is null || resumeClass.MissingTitleKeys.Count == 0))
            {
                AaruConsole.WriteLine("Media already dumped correctly, not continuing...");

                if(isResponse)
                    continue;

                return (int)ErrorNumber.AlreadyDumped;
            }

            CICMMetadataType sidecar   = null;
            var              sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        var sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        AaruConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");

                        if(isResponse)
                            continue;

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                else
                {
                    AaruConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");

                    if(isResponse)
                        continue;

                    return (int)ErrorNumber.NoSuchFile;
                }

            plugins    = GetPluginBase.Instance;
            candidates = new List<IBaseWritableImage>();

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

            IBaseWritableImage outputFormat = candidates[0];

            var dumpLog = new DumpLog(outputPrefix + ".log", dev, @private);

            if(verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
                AaruConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);
                AaruConsole.WriteLine("Output image format: {0}.", outputFormat.Name);
            }

            var errorLog = new ErrorLog(outputPrefix + ".error.log");

            var dumper = new Dump(resume, dev, devicePath, outputFormat, retryPasses, force, false, persistent,
                                  stopOnError, resumeClass, dumpLog, encodingClass, outputPrefix,
                                  outputPrefix + extension, parsedOptions, sidecar, skip, metadata, trim, firstPregap,
                                  fixOffset, debug, wantedSubchannel, speed, @private, fixSubchannelPosition,
                                  retrySubchannel, fixSubchannel, fixSubchannelCrc, skipCdiReadyHole, errorLog,
                                  generateSubchannels, maxBlocks, useBufferedReads, storeEncrypted, titleKeys);

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            dumper.UpdateStatus += text =>
                            {
                                AaruConsole.WriteLine(Markup.Escape(text));
                            };

                            dumper.ErrorMessage += text =>
                            {
                                AaruConsole.ErrorWriteLine($"[red]{Markup.Escape(text)}[/]");
                            };

                            dumper.StoppingErrorMessage += text =>
                            {
                                AaruConsole.ErrorWriteLine($"[red]{Markup.Escape(text)}[/]");
                            };

                            dumper.UpdateProgress += (text, current, maximum) =>
                            {
                                _progressTask1             ??= ctx.AddTask("Progress");
                                _progressTask1.Description =   Markup.Escape(text);
                                _progressTask1.Value       =   current;
                                _progressTask1.MaxValue    =   maximum;
                            };

                            dumper.PulseProgress += text =>
                            {
                                if(_progressTask1 is null)
                                    ctx.AddTask(Markup.Escape(text)).IsIndeterminate();
                                else
                                {
                                    _progressTask1.Description     = Markup.Escape(text);
                                    _progressTask1.IsIndeterminate = true;
                                }
                            };

                            dumper.InitProgress += () =>
                            {
                                _progressTask1 = ctx.AddTask("Progress");
                            };

                            dumper.EndProgress += () =>
                            {
                                _progressTask1?.StopTask();
                                _progressTask1 = null;
                            };

                            dumper.InitProgress2 += () =>
                            {
                                _progressTask2 = ctx.AddTask("Progress");
                            };

                            dumper.EndProgress2 += () =>
                            {
                                _progressTask2?.StopTask();
                                _progressTask2 = null;
                            };

                            dumper.UpdateProgress2 += (text, current, maximum) =>
                            {
                                _progressTask2             ??= ctx.AddTask("Progress");
                                _progressTask2.Description =   Markup.Escape(text);
                                _progressTask2.Value       =   current;
                                _progressTask2.MaxValue    =   maximum;
                            };

                            Console.CancelKeyPress += (_, e) =>
                            {
                                e.Cancel = true;
                                dumper.Abort();
                            };

                            dumper.Start();
                        });

            if(eject && dev.IsRemovable)
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Ejecting media...").IsIndeterminate();

                    switch(dev.Type)
                    {
                        case DeviceType.ATA:
                            dev.DoorUnlock(out _, dev.Timeout, out _);
                            dev.MediaEject(out _, dev.Timeout, out _);

                            break;
                        case DeviceType.ATAPI:
                        case DeviceType.SCSI:
                            switch(dev.ScsiType)
                            {
                                case PeripheralDeviceTypes.DirectAccess:
                                case PeripheralDeviceTypes.SimplifiedDevice:
                                case PeripheralDeviceTypes.SCSIZonedBlockDevice:
                                case PeripheralDeviceTypes.WriteOnceDevice:
                                case PeripheralDeviceTypes.OpticalDevice:
                                case PeripheralDeviceTypes.OCRWDevice:
                                    dev.SpcAllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.EjectTray(out _, dev.Timeout, out _);

                                    break;
                                case PeripheralDeviceTypes.MultiMediaDevice:
                                    dev.AllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.EjectTray(out _, dev.Timeout, out _);

                                    break;
                                case PeripheralDeviceTypes.SequentialAccess:
                                    dev.SpcAllowMediumRemoval(out _, dev.Timeout, out _);
                                    dev.LoadUnload(out _, true, false, false, false, false, dev.Timeout, out _);

                                    break;
                            }

                            break;
                    }
                });

            dev.Close();
        }

        return (int)ErrorNumber.NoError;
    }
}