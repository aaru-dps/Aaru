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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using Mono.Options;
using Schemas;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;

namespace DiscImageChef.Commands
{
    class DumpMediaCommand : Command
    {
        string cicmXml;
        string devicePath;
        bool   doResume = true;
        string encodingName;
        bool   firstTrackPregap;
        bool   force;
        bool   noMetadata;
        bool   noTrim;
        string outputFile;
        string outputOptions;
        bool   persistent;
        // TODO: Disabled temporarily
        bool   raw         = false;
        ushort retryPasses = 5;
        bool   showHelp;
        int    skip = 512;
        bool   stopOnError;
        string wanteOutputFormat;

        public DumpMediaCommand() : base("dump-media", "Dumps the media inserted on a device to a media image.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] devicepath outputimage",
                "",
                Help,
                {"cicm-xml|x=", "Take metadata from existing CICM XML sidecar.", s => cicmXml = s},
                {"encoding|e=", "Name of character encoding to use.", s => encodingName       = s}
            };

            if(DetectOS.GetRealPlatformID() != PlatformID.FreeBSD)
                Options.Add("first-pregap", "Try to read first track pregap. Only applicable to CD/DDCD/GD.",
                            b => firstTrackPregap = b != null);

            Options.Add("force|f", "Continue dump whatever happens.", b => force = b != null);
            Options.Add("format|t=",
                        "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.",
                        s => wanteOutputFormat = s);
            Options.Add("no-metadata", "Disables creating CICM XML sidecar.",             b => noMetadata = b != null);
            Options.Add("no-trim",     "Disables trimming errored from skipped sectors.", b => noTrim     = b != null);
            Options.Add("options|O=", "Comma separated name=value pairs of options to pass to output image plugin.",
                        s => outputOptions = s);
            Options.Add("persistent", "Try to recover partial or incorrect data.", b => persistent = b != null);
            /* TODO: Disabled temporarily
            Options.Add("raw|r", "Dump sectors with tags included. For optical media, dump scrambled sectors.", (b) => raw = b != null);*/
            Options.Add("resume|r", "Create/use resume mapfile.",
                        b => doResume = b != null);
            Options.Add("retry-passes|p=", "How many retry passes to do.",
                        (ushort us) => retryPasses = us);
            Options.Add("skip|k=", "When an unreadable sector is found skip this many sectors.",
                        (int i) => skip = i);
            Options.Add("stop-on-error|s", "Stop media dump on first error.",
                        b => stopOnError = b != null);
            Options.Add("help|h|?", "Show this message and exit.",
                        v => showHelp = v != null);
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
            Statistics.AddCommand("dump-media");

            if(extra.Count > 2)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count <= 1)
            {
                DicConsole.ErrorWriteLine("Missing paths.");
                return (int)ErrorNumber.MissingArgument;
            }

            devicePath = extra[0];
            outputFile = extra[1];

            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}",     cicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}",        MainClass.Debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}",       devicePath);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}",     encodingName);
            DicConsole.DebugWriteLine("Dump-Media command", "--first-pregap={0}", firstTrackPregap);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",        force);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",        force);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}",       wanteOutputFormat);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}",  noMetadata);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}",      Options);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}",       outputFile);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}",   persistent);
            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           raw);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}",        doResume);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}",  retryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}",          skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", stopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}",       MainClass.Verbose);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(outputOptions);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            Encoding encoding = null;

            if(encodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(encodingName);
                    if(MainClass.Verbose) DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");
                    return (int)ErrorNumber.EncodingUnknown;
                }

            if(devicePath.Length == 2 && devicePath[1] == ':' && devicePath[0] != '/' && char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            Device dev = new Device(devicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return (int)ErrorNumber.CannotOpenDevice;
            }

            Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(outputFile),
                                               Path.GetFileNameWithoutExtension(outputFile));

            Resume        resume = null;
            XmlSerializer xs     = new XmlSerializer(typeof(Resume));
            if(File.Exists(outputPrefix + ".resume.xml") && doResume)
                try
                {
                    StreamReader sr = new StreamReader(outputPrefix + ".resume.xml");
                    resume = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                    return (int)ErrorNumber.InvalidResume;
                }

            if(resume != null && resume.NextBlock > resume.LastBlock && resume.BadBlocks.Count == 0 && !resume.Tape)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");
                return (int)ErrorNumber.AlreadyDumped;
            }

            CICMMetadataType sidecar   = null;
            XmlSerializer    sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        StreamReader sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");
                        return (int)ErrorNumber.InvalidSidecar;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");
                    return (int)ErrorNumber.FileNotFound;
                }

            PluginBase           plugins    = GetPluginBase.Instance;
            List<IWritableImage> candidates = new List<IWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(wanteOutputFormat))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions
                                                                             .Contains(Path.GetExtension(outputFile))));
            // Try Id
            else if(Guid.TryParse(wanteOutputFormat, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));
            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, wanteOutputFormat,
                                                                                           StringComparison
                                                                                              .InvariantCultureIgnoreCase)));

            if(candidates.Count == 0)
            {
                DicConsole.WriteLine("No plugin supports requested extension.");
                return (int)ErrorNumber.FormatNotFound;
            }

            if(candidates.Count > 1)
            {
                DicConsole.WriteLine("More than one plugin supports requested extension.");
                return (int)ErrorNumber.TooManyFormats;
            }

            IWritableImage outputFormat = candidates[0];

            DumpLog dumpLog = new DumpLog(outputPrefix + ".log", dev);

            if(MainClass.Verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);
                DicConsole.WriteLine("Output image format: {0}.", outputFormat.Name);
            }

            Dump dumper = new Dump(doResume, dev, devicePath, outputFormat, retryPasses, force, false, persistent,
                                   stopOnError, resume, dumpLog, encoding, outputPrefix, outputFile, parsedOptions,
                                   sidecar, (uint)skip, noMetadata, noTrim, firstTrackPregap);
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
            return (int)ErrorNumber.NoError;
        }
    }
}