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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.Console;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Commands
{
    static class DumpMedia
    {
        internal static void DoDumpMedia(DumpMediaOptions options)
        {
            DicConsole.DebugWriteLine("Dump-Media command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Dump-Media command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Dump-Media command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Dump-Media command", "--output-prefix={0}", options.OutputPrefix);
            DicConsole.DebugWriteLine("Dump-Media command", "--raw={0}", options.Raw);
            DicConsole.DebugWriteLine("Dump-Media command", "--stop-on-error={0}", options.StopOnError);
            DicConsole.DebugWriteLine("Dump-Media command", "--force={0}", options.Force);
            DicConsole.DebugWriteLine("Dump-Media command", "--retry-passes={0}", options.RetryPasses);
            DicConsole.DebugWriteLine("Dump-Media command", "--persistent={0}", options.Persistent);
            DicConsole.DebugWriteLine("Dump-Media command", "--separate-subchannel={0}", options.SeparateSubchannel);
            DicConsole.DebugWriteLine("Dump-Media command", "--resume={0}", options.Resume);
            DicConsole.DebugWriteLine("Dump-Media command", "--lead-in={0}", options.LeadIn);
            DicConsole.DebugWriteLine("Dump-Media command", "--encoding={0}", options.EncodingName);

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

            Resume resume = null;
            XmlSerializer xs = new XmlSerializer(typeof(Resume));
            if(File.Exists(options.OutputPrefix + ".resume.xml") && options.Resume)
                try
                {
                    StreamReader sr = new StreamReader(options.OutputPrefix + ".resume.xml");
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

            DumpLog dumpLog = new DumpLog(options.OutputPrefix + ".log", dev);

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    Ata.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force,
                             options.Raw, options.Persistent, options.StopOnError, ref resume, ref dumpLog, encoding);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses,
                                       options.Force, options.Raw, options.Persistent, options.StopOnError, ref resume,
                                       ref dumpLog, encoding);
                    break;
                case DeviceType.NVMe:
                    NvMe.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force,
                              options.Raw, options.Persistent, options.StopOnError, ref resume, ref dumpLog, encoding);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    Scsi.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force,
                              options.Raw, options.Persistent, options.StopOnError, options.SeparateSubchannel,
                              ref resume, ref dumpLog, options.LeadIn, encoding);
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

                if(File.Exists(options.OutputPrefix + ".resume.xml")) File.Delete(options.OutputPrefix + ".resume.xml");

                FileStream fs = new FileStream(options.OutputPrefix + ".resume.xml", FileMode.Create,
                                               FileAccess.ReadWrite);
                xs = new XmlSerializer(resume.GetType());
                xs.Serialize(fs, resume);
                fs.Close();
            }

            dumpLog.Close();

            Core.Statistics.AddCommand("dump-media");
        }
    }
}