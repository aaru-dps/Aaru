// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiMmcMode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 2Ah from reports.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Server
{
    public static class ScsiMmcMode
    {
        /// <summary>
        ///     Takes the MODE PAGE 2Ah part of a device report and prints it as a list of values to be sequenced by ASP.NET in the
        ///     rendering
        /// </summary>
        /// <param name="mode">MODE PAGE 2Ah part of the report</param>
        /// <param name="mmcOneValue">List to put the values on</param>
        public static void Report(Modes.ModePage_2A mode, ref List<string> mmcOneValue)
        {
            if(mode.AudioPlay) mmcOneValue.Add("Drive can play audio");
            if(mode.Mode2Form1) mmcOneValue.Add("Drive can read sectors in Mode 2 Form 1 format");
            if(mode.Mode2Form2) mmcOneValue.Add("Drive can read sectors in Mode 2 Form 2 format");
            if(mode.MultiSession) mmcOneValue.Add("Drive supports multi-session discs and/or Photo-CD");

            if(mode.CDDACommand) mmcOneValue.Add("Drive can read digital audio");
            if(mode.AccurateCDDA) mmcOneValue.Add("Drive can continue from streaming loss");
            if(mode.Subchannel) mmcOneValue.Add("Drive can read uncorrected and interleaved R-W subchannels");
            if(mode.DeinterlaveSubchannel) mmcOneValue.Add("Drive can read, deinterleave and correct R-W subchannels");
            if(mode.C2Pointer) mmcOneValue.Add("Drive supports C2 pointers");
            if(mode.UPC) mmcOneValue.Add("Drive can read Media Catalogue Number");
            if(mode.ISRC) mmcOneValue.Add("Drive can read ISRC");

            switch(mode.LoadingMechanism)
            {
                case 0:
                    mmcOneValue.Add("Drive uses media caddy");
                    break;
                case 1:
                    mmcOneValue.Add("Drive uses a tray");
                    break;
                case 2:
                    mmcOneValue.Add("Drive is pop-up");
                    break;
                case 4:
                    mmcOneValue.Add("Drive is a changer with individually changeable discs");
                    break;
                case 5:
                    mmcOneValue.Add("Drive is a changer using cartridges");
                    break;
                default:
                    mmcOneValue.Add($"Drive uses unknown loading mechanism type {mode.LoadingMechanism}");
                    break;
            }

            if(mode.Lock) mmcOneValue.Add("Drive can lock media");
            if(mode.PreventJumper)
            {
                mmcOneValue.Add("Drive power ups locked");
                mmcOneValue.Add(mode.LockState
                                    ? "Drive is locked, media cannot be ejected or inserted"
                                    : "Drive is not locked, media can be ejected and inserted");
            }
            else
                mmcOneValue.Add(mode.LockState
                                    ? "Drive is locked, media cannot be ejected, but if empty, can be inserted"
                                    : "Drive is not locked, media can be ejected and inserted");

            if(mode.Eject) mmcOneValue.Add("Drive can eject media");

            if(mode.SeparateChannelMute) mmcOneValue.Add("Each channel can be muted independently");
            if(mode.SeparateChannelVolume) mmcOneValue.Add("Each channel's volume can be controlled independently");

            if(mode.SupportedVolumeLevels > 0)
                mmcOneValue.Add($"Drive supports {mode.SupportedVolumeLevels} volume levels");
            if(mode.BufferSize > 0) mmcOneValue.Add($"Drive has {mode.BufferSize} Kbyte of buffer");
            if(mode.MaximumSpeed > 0)
                mmcOneValue.Add($"Drive's maximum reading speed is {mode.MaximumSpeed} Kbyte/sec.");
            if(mode.CurrentSpeed > 0)
                mmcOneValue.Add($"Drive's current reading speed is {mode.CurrentSpeed} Kbyte/sec.");

            if(mode.ReadCDR)
            {
                mmcOneValue.Add(mode.WriteCDR ? "Drive can read and write CD-R" : "Drive can read CD-R");

                if(mode.Method2) mmcOneValue.Add("Drive supports reading CD-R packet media");
            }

            if(mode.ReadCDRW)
                mmcOneValue.Add(mode.WriteCDRW ? "Drive can read and write CD-RW" : "Drive can read CD-RW");

            if(mode.ReadDVDROM) mmcOneValue.Add("Drive can read DVD-ROM");
            if(mode.ReadDVDR)
                mmcOneValue.Add(mode.WriteDVDR ? "Drive can read and write DVD-R" : "Drive can read DVD-R");
            if(mode.ReadDVDRAM)
                mmcOneValue.Add(mode.WriteDVDRAM ? "Drive can read and write DVD-RAM" : "Drive can read DVD-RAM");

            if(mode.Composite) mmcOneValue.Add("Drive can deliver a composite audio and video data stream");
            if(mode.DigitalPort1) mmcOneValue.Add("Drive supports IEC-958 digital output on port 1");
            if(mode.DigitalPort2) mmcOneValue.Add("Drive supports IEC-958 digital output on port 2");

            if(mode.SDP) mmcOneValue.Add("Drive contains a changer that can report the exact contents of the slots");
            if(mode.CurrentWriteSpeedSelected > 0)
            {
                if(mode.RotationControlSelected == 0)
                    mmcOneValue
                       .Add($"Drive's current writing speed is {mode.CurrentWriteSpeedSelected} Kbyte/sec. in CLV mode");
                else if(mode.RotationControlSelected == 1)
                    mmcOneValue
                       .Add($"Drive's current writing speed is {mode.CurrentWriteSpeedSelected} Kbyte/sec. in pure CAV mode");
            }
            else
            {
                if(mode.MaxWriteSpeed > 0)
                    mmcOneValue.Add($"Drive's maximum writing speed is {mode.MaxWriteSpeed} Kbyte/sec.");
                if(mode.CurrentWriteSpeed > 0)
                    mmcOneValue.Add($"Drive's current writing speed is {mode.CurrentWriteSpeed} Kbyte/sec.");
            }

            if(mode.WriteSpeedPerformanceDescriptors != null)
                foreach(Modes.ModePage_2A_WriteDescriptor descriptor in
                    mode.WriteSpeedPerformanceDescriptors.Where(descriptor => descriptor.WriteSpeed > 0))
                    if(descriptor.RotationControl == 0)
                        mmcOneValue.Add($"Drive supports writing at {descriptor.WriteSpeed} Kbyte/sec. in CLV mode");
                    else if(descriptor.RotationControl == 1)
                        mmcOneValue
                           .Add($"Drive supports writing at is {descriptor.WriteSpeed} Kbyte/sec. in pure CAV mode");

            if(mode.TestWrite) mmcOneValue.Add("Drive supports test writing");

            if(mode.ReadBarcode) mmcOneValue.Add("Drive can read barcode");

            if(mode.SCC) mmcOneValue.Add("Drive can read both sides of a disc");
            if(mode.LeadInPW) mmcOneValue.Add("Drive an read raw R-W subchannel from the Lead-In");

            if(mode.CMRSupported == 1) mmcOneValue.Add("Drive supports DVD CSS and/or DVD CPPM");

            if(mode.BUF) mmcOneValue.Add("Drive supports buffer under-run free recording");

            mmcOneValue.Sort();
            mmcOneValue.Add("");
        }
    }
}