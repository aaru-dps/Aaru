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
    // TODO: Add raw dumping
    internal class DumpMediaCommand : Command
    {
        string _cicmXml;
        string _devicePath;
        bool   _doResume = true;
        string _encodingName;
        bool   _firstTrackPregap;
        bool   _force;
        bool   _noMetadata;
        bool   _noTrim;
        string _outputFile;
        string _outputOptions;
        bool   _persistent;
        ushort _retryPasses = 5;
        bool   _showHelp;
        int    _skip = 512;
        bool   _stopOnError;
        string _wantedOutputFormat;

        public DumpMediaCommand() : base("dump-media", "Dumps the media inserted on a device to a media image.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}", "", $"usage: DiscImageChef {Name} [OPTIONS] device_path output_image",
                "", Help,
                {
                    "cicm-xml|x=", "Take metadata from existing CICM XML sidecar.", s => _cicmXml = s
                },
                {
                    "encoding|e=", "Name of character encoding to use.", s => _encodingName = s
                }
            };

            if(DetectOS.GetRealPlatformID() != PlatformID.FreeBSD)
                Options.Add("first-pregap", "Try to read first track pregap. Only applicable to CD/DDCD/GD.",
                            b => _firstTrackPregap = b != null);

            Options.Add("force|f", "Continue dump whatever happens.", b => _force = b != null);

            Options.Add("format|t=",
                        "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.",
                        s => _wantedOutputFormat = s);

            Options.Add("no-metadata", "Disables creating CICM XML sidecar.", b => _noMetadata     = b != null);
            Options.Add("no-trim", "Disables trimming errored from skipped sectors.", b => _noTrim = b != null);

            Options.Add("options|O=", "Comma separated name=value pairs of options to pass to output image plugin.",
                        s => _outputOptions = s);

            Options.Add("persistent", "Try to recover partial or incorrect data.", b => _persistent = b != null);

            Options.Add("resume|r", "Create/use resume mapfile.", b => _doResume = b != null);

            Options.Add("retry-passes|p=", "How many retry passes to do.", (ushort us) => _retryPasses            = us);
            Options.Add("skip|k=", "When an unreadable sector is found skip this many sectors.", (int i) => _skip = i);

            Options.Add("stop-on-error|s", "Stop media dump on first error.", b => _stopOnError = b != null);

            Options.Add("help|h|?", "Show this message and exit.", v => _showHelp = v != null);

            /* TODO: Disabled temporarily
            Options.Add("raw|r", "Dump sectors with tags included. For optical media, dump scrambled sectors.", (b) => raw = b != null);*/
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(_showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);

                return(int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();

            if(MainClass.Debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(MainClass.Verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("dump-media");

            if(extra.Count > 2)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");

                return(int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count <= 1)
            {
                DicConsole.ErrorWriteLine("Missing paths.");

                return(int)ErrorNumber.MissingArgument;
            }

            _devicePath = extra[0];
            _outputFile = extra[1];

            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}", _cicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}", MainClass.Debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}", _devicePath);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}", _encodingName);
            DicConsole.DebugWriteLine("Dump-Media command", "--first-pregap={0}", _firstTrackPregap);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}", _force);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}", _force);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}", _wantedOutputFormat);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}", _noMetadata);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}", Options);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}", _outputFile);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}", _persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}", _doResume);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}", _retryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}", _skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", _stopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", MainClass.Verbose);

            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           raw);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(_outputOptions);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            Encoding encoding = null;

            if(_encodingName != null)
                try
                {
                    encoding = Claunia.Encoding.Encoding.GetEncoding(_encodingName);

                    if(MainClass.Verbose)
                        DicConsole.VerboseWriteLine("Using encoding for {0}.", encoding.EncodingName);
                }
                catch(ArgumentException)
                {
                    DicConsole.ErrorWriteLine("Specified encoding is not supported.");

                    return(int)ErrorNumber.EncodingUnknown;
                }

            if(_devicePath.Length == 2   &&
               _devicePath[1]     == ':' &&
               _devicePath[0]     != '/' &&
               char.IsLetter(_devicePath[0]))
                _devicePath = "\\\\.\\" + char.ToUpper(_devicePath[0]) + ':';

            Device dev;

            try
            {
                dev = new Device(_devicePath);

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

            string outputPrefix = Path.Combine(Path.GetDirectoryName(_outputFile),
                                               Path.GetFileNameWithoutExtension(_outputFile));

            Resume resume = null;
            var    xs     = new XmlSerializer(typeof(Resume));

            if(File.Exists(outputPrefix + ".resume.xml") && _doResume)
                try
                {
                    var sr = new StreamReader(outputPrefix + ".resume.xml");
                    resume = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");

                    return(int)ErrorNumber.InvalidResume;
                }

            if(resume                 != null            &&
               resume.NextBlock       > resume.LastBlock &&
               resume.BadBlocks.Count == 0               &&
               !resume.Tape)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");

                return(int)ErrorNumber.AlreadyDumped;
            }

            CICMMetadataType sidecar   = null;
            var              sidecarXs = new XmlSerializer(typeof(CICMMetadataType));

            if(_cicmXml != null)
                if(File.Exists(_cicmXml))
                {
                    try
                    {
                        var sr = new StreamReader(_cicmXml);
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
            if(string.IsNullOrEmpty(_wantedOutputFormat))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions.
                                                                              Contains(Path.
                                                                                           GetExtension(_outputFile))));

            // Try Id
            else if(Guid.TryParse(_wantedOutputFormat, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));

            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, _wantedOutputFormat,
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

            var dumper = new Dump(_doResume, dev, _devicePath, outputFormat, _retryPasses, _force, false, _persistent,
                                  _stopOnError, resume, dumpLog, encoding, outputPrefix, _outputFile, parsedOptions,
                                  sidecar, (uint)_skip, _noMetadata, _noTrim, _firstTrackPregap);

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