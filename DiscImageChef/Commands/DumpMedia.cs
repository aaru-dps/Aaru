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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Core;
using DiscImageChef.Core.Devices.Dumping;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Devices;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using Schemas;
using DiscImageChef.Metadata;
using System.Xml.Serialization;

namespace DiscImageChef.Commands
{
    public static class DumpMedia
    {
        public static void doDumpMedia(DumpMediaOptions options)
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

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + char.ToUpper(options.DevicePath[0]) + ':';
            }

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
            {
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
            }

            if(resume != null && resume.NextBlock > resume.LastBlock && resume.BadBlocks.Count == 0)
            {
                DicConsole.WriteLine("Media already dumped correctly, not continuing...");
                return;
            }

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    ATA.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force, options.Raw, options.Persistent, options.StopOnError, ref resume);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    SecureDigital.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force, options.Raw, options.Persistent, options.StopOnError, ref resume);
                    break;
                case DeviceType.NVMe:
                    NVMe.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force, options.Raw, options.Persistent, options.StopOnError, ref resume);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    SCSI.Dump(dev, options.DevicePath, options.OutputPrefix, options.RetryPasses, options.Force, options.Raw, options.Persistent, options.StopOnError, options.SeparateSubchannel, ref resume);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            if(resume != null && options.Resume)
            {
                resume.LastWriteDate = DateTime.UtcNow;
                resume.BadBlocks.Sort();

                if(File.Exists(options.OutputPrefix + ".resume.xml"))
                    File.Delete(options.OutputPrefix + ".resume.xml");

                FileStream fs = new FileStream(options.OutputPrefix + ".resume.xml", FileMode.Create, FileAccess.ReadWrite);
                xs = new XmlSerializer(resume.GetType());
                xs.Serialize(fs, resume);
                fs.Close();
            }

            Core.Statistics.AddCommand("dump-media");
        }
    }
}