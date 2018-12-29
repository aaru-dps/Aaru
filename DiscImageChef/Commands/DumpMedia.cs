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
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using Schemas;
using Ata = DiscImageChef.Core.Devices.Dumping.Ata;
using Scsi = DiscImageChef.Core.Devices.Dumping.Scsi;

namespace DiscImageChef.Commands
{
    static class DumpMedia
    {
        internal static void DoDumpMedia(DumpMediaOptions options)
        {
            // TODO: Be able to cancel hashing
            Sidecar.InitProgressEvent    += Progress.InitProgress;
            Sidecar.UpdateProgressEvent  += Progress.UpdateProgress;
            Sidecar.EndProgressEvent     += Progress.EndProgress;
            Sidecar.InitProgressEvent2   += Progress.InitProgress2;
            Sidecar.UpdateProgressEvent2 += Progress.UpdateProgress2;
            Sidecar.EndProgressEvent2    += Progress.EndProgress2;
            Sidecar.UpdateStatusEvent    += Progress.UpdateStatus;

            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}",   options.Debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}",  options.DevicePath);
            // TODO: Disabled temporarily
            //DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}",           options.Raw);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", options.StopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",         options.Force);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}",  options.RetryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}",    options.Persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}",        options.Resume);
            DicConsole.DebugWriteLine("Dump-Media command", "--lead-in={0}",       options.LeadIn);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}",      options.EncodingName);
            DicConsole.DebugWriteLine("Dump-Media command", "--output={0}",        options.OutputFile);
            DicConsole.DebugWriteLine("Dump-Media command", "--format={0}",        options.OutputFormat);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}",         options.Force);
            DicConsole.DebugWriteLine("Dump-Media command", "--options={0}",       options.Options);
            DicConsole.DebugWriteLine("Dump-Media command", "--cicm-xml={0}",      options.CicmXml);
            DicConsole.DebugWriteLine("Dump-Media command", "--skip={0}",          options.Skip);
            DicConsole.DebugWriteLine("Dump-Media command", "--no-metadata={0}",   options.NoMetadata);

            Dictionary<string, string> parsedOptions = Options.Parse(options.Options);
            DicConsole.DebugWriteLine("Dump-Media command", "Parsed options:");
            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Dump-Media command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

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

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' && options.DevicePath[0] != '/' &&
               char.IsLetter(options.DevicePath[0]))
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            string outputPrefix = Path.Combine(Path.GetDirectoryName(options.OutputFile),
                                               Path.GetFileNameWithoutExtension(options.OutputFile));

            Resume        resume = null;
            XmlSerializer xs     = new XmlSerializer(typeof(Resume));
            if(File.Exists(outputPrefix + ".resume.xml") && options.Resume)
                try
                {
                    StreamReader sr = new StreamReader(outputPrefix + ".resume.xml");
                    resume = (Resume)xs.Deserialize(sr);
                    sr.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                    return;
                }

            if(resume != null && resume.NextBlock > resume.LastBlock && resume.BadBlocks.Count == 0)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");
                return;
            }

            CICMMetadataType sidecar   = null;
            XmlSerializer    sidecarXs = new XmlSerializer(typeof(CICMMetadataType));
            if(options.CicmXml != null)
                if(File.Exists(options.CicmXml))
                    try
                    {
                        StreamReader sr = new StreamReader(options.CicmXml);
                        sidecar = (CICMMetadataType)sidecarXs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");
                        return;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");
                    return;
                }

            PluginBase           plugins    = GetPluginBase.Instance;
            List<IWritableImage> candidates = new List<IWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(options.OutputFormat))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions
                                                                             .Contains(Path.GetExtension(options
                                                                                                            .OutputFile))));
            // Try Id
            else if(Guid.TryParse(options.OutputFormat, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));
            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, options.OutputFormat,
                                                                                           StringComparison
                                                                                              .InvariantCultureIgnoreCase)));

            if(candidates.Count == 0)
            {
                DicConsole.WriteLine("No plugin supports requested extension.");
                return;
            }

            if(candidates.Count > 1)
            {
                DicConsole.WriteLine("More than one plugin supports requested extension.");
                return;
            }

            IWritableImage outputFormat = candidates[0];

            DumpLog dumpLog = new DumpLog(outputPrefix + ".log", dev);

            if(options.Verbose)
            {
                dumpLog.WriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            }
            else
            {
                dumpLog.WriteLine("Output image format: {0}.", outputFormat.Name);
                DicConsole.WriteLine("Output image format: {0}.", outputFormat.Name);
            }

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Dump(dev, options.DevicePath, outputFormat, options.RetryPasses, options.Force,
                             false, /*options.Raw,*/
                             options.Persistent, options.StopOnError, ref resume, ref dumpLog, encoding, outputPrefix,
                             options.OutputFile, parsedOptions, sidecar, (uint)options.Skip, options.NoMetadata,
                             options.NoTrim);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Dump(dev, options.DevicePath, outputFormat, options.RetryPasses, options.Force,
                                       false, /*options.Raw,*/ options.Persistent, options.StopOnError, ref resume,
                                       ref dumpLog, encoding, outputPrefix, options.OutputFile, parsedOptions, sidecar,
                                       (uint)options.Skip, options.NoMetadata, options.NoTrim);
                    break;
                case DeviceType.NVMe:
                    NvMe.Dump(dev, options.DevicePath, outputFormat, options.RetryPasses, options.Force,
                              false, /*options.Raw,*/
                              options.Persistent, options.StopOnError, ref resume, ref dumpLog, encoding, outputPrefix,
                              options.OutputFile, parsedOptions, sidecar, (uint)options.Skip, options.NoMetadata,
                              options.NoTrim);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    Scsi.Dump(dev, options.DevicePath, outputFormat, options.RetryPasses, options.Force,
                              false, /*options.Raw,*/
                              options.Persistent, options.StopOnError, ref resume, ref dumpLog, options.LeadIn,
                              encoding, outputPrefix, options.OutputFile, parsedOptions, sidecar, (uint)options.Skip,
                              options.NoMetadata, options.NoTrim);
                    break;
                default:
                    dumpLog.WriteLine("Unknown device type.");
                    dumpLog.Close();
                    throw new NotSupportedException("Unknown device type.");
            }

            if(resume != null && options.Resume)
            {
                resume.LastWriteDate = DateTime.UtcNow;
                resume.BadBlocks.Sort();

                if(File.Exists(outputPrefix + ".resume.xml")) File.Delete(outputPrefix + ".resume.xml");

                FileStream fs = new FileStream(outputPrefix + ".resume.xml", FileMode.Create, FileAccess.ReadWrite);
                xs = new XmlSerializer(resume.GetType());
                xs.Serialize(fs, resume);
                fs.Close();
            }

            dumpLog.Close();

            Core.Statistics.AddCommand("dump-media");

            dev.Close();
        }
    }
}