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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class ScsiMmcMode
    {
        public static void Report(mmcModeType mode, ref List<string> mmcOneValue)
        {
            if(mode.PlaysAudio) mmcOneValue.Add("Drive can play audio");
            if(mode.ReadsMode2Form1) mmcOneValue.Add("Drive can read sectors in Mode 2 Form 1 format");
            if(mode.ReadsMode2Form2) mmcOneValue.Add("Drive can read sectors in Mode 2 Form 2 format");
            if(mode.SupportsMultiSession) mmcOneValue.Add("Drive supports multi-session discs and/or Photo-CD");

            if(mode.CDDACommand) mmcOneValue.Add("Drive can read digital audio");
            if(mode.AccurateCDDA) mmcOneValue.Add("Drive can continue from streaming loss");
            if(mode.ReadsSubchannel) mmcOneValue.Add("Drive can read uncorrected and interleaved R-W subchannels");
            if(mode.ReadsDeinterlavedSubchannel)
                mmcOneValue.Add("Drive can read, deinterleave and correct R-W subchannels");
            if(mode.ReturnsC2Pointers) mmcOneValue.Add("Drive supports C2 pointers");
            if(mode.ReadsUPC) mmcOneValue.Add("Drive can read Media Catalogue Number");
            if(mode.ReadsISRC) mmcOneValue.Add("Drive can read ISRC");

            switch(mode.LoadingMechanismType)
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
                    mmcOneValue.Add($"Drive uses unknown loading mechanism type {mode.LoadingMechanismType}");
                    break;
            }

            if(mode.CanLockMedia) mmcOneValue.Add("Drive can lock media");
            if(mode.PreventJumperStatus)
            {
                mmcOneValue.Add("Drive power ups locked");
                if(mode.LockStatus) mmcOneValue.Add("Drive is locked, media cannot be ejected or inserted");
                else mmcOneValue.Add("Drive is not locked, media can be ejected and inserted");
            }
            else
            {
                if(mode.LockStatus)
                    mmcOneValue.Add("Drive is locked, media cannot be ejected, but if empty, can be inserted");
                else mmcOneValue.Add("Drive is not locked, media can be ejected and inserted");
            }
            if(mode.CanEject) mmcOneValue.Add("Drive can eject media");

            if(mode.SeparateChannelMute) mmcOneValue.Add("Each channel can be muted independently");
            if(mode.SeparateChannelVolume) mmcOneValue.Add("Each channel's volume can be controlled independently");

            if(mode.SupportedVolumeLevels > 0)
                mmcOneValue.Add($"Drive supports {mode.SupportedVolumeLevels} volume levels");
            if(mode.BufferSize > 0) mmcOneValue.Add($"Drive has {mode.BufferSize} Kbyte of buffer");
            if(mode.MaximumSpeed > 0)
                mmcOneValue.Add($"Drive's maximum reading speed is {mode.MaximumSpeed} Kbyte/sec.");
            if(mode.CurrentSpeed > 0)
                mmcOneValue.Add($"Drive's current reading speed is {mode.CurrentSpeed} Kbyte/sec.");

            if(mode.ReadsCDR)
            {
                if(mode.WritesCDR) mmcOneValue.Add("Drive can read and write CD-R");
                else mmcOneValue.Add("Drive can read CD-R");

                if(mode.ReadsPacketCDR) mmcOneValue.Add("Drive supports reading CD-R packet media");
            }

            if(mode.ReadsCDRW)
                if(mode.WritesCDRW) mmcOneValue.Add("Drive can read and write CD-RW");
                else mmcOneValue.Add("Drive can read CD-RW");

            if(mode.ReadsDVDROM) mmcOneValue.Add("Drive can read DVD-ROM");
            if(mode.ReadsDVDR)
                if(mode.WritesDVDR) mmcOneValue.Add("Drive can read and write DVD-R");
                else mmcOneValue.Add("Drive can read DVD-R");
            if(mode.ReadsDVDRAM)
                if(mode.WritesDVDRAM) mmcOneValue.Add("Drive can read and write DVD-RAM");
                else mmcOneValue.Add("Drive can read DVD-RAM");

            if(mode.CompositeAudioVideo) mmcOneValue.Add("Drive can deliver a composite audio and video data stream");
            if(mode.DigitalPort1) mmcOneValue.Add("Drive supports IEC-958 digital output on port 1");
            if(mode.DigitalPort2) mmcOneValue.Add("Drive supports IEC-958 digital output on port 2");

            if(mode.DeterministicSlotChanger)
                mmcOneValue.Add("Drive contains a changer that can report the exact contents of the slots");
            if(mode.CurrentWriteSpeedSelected > 0)
            {
                if(mode.RotationControlSelected == 0)
                    mmcOneValue.Add($"Drive's current writing speed is {mode.CurrentWriteSpeedSelected} Kbyte/sec. in CLV mode");
                else if(mode.RotationControlSelected == 1)
                    mmcOneValue.Add($"Drive's current writing speed is {mode.CurrentWriteSpeedSelected} Kbyte/sec. in pure CAV mode");
            }
            else
            {
                if(mode.MaximumWriteSpeed > 0)
                    mmcOneValue.Add($"Drive's maximum writing speed is {mode.MaximumWriteSpeed} Kbyte/sec.");
                if(mode.CurrentWriteSpeed > 0)
                    mmcOneValue.Add($"Drive's current writing speed is {mode.CurrentWriteSpeed} Kbyte/sec.");
            }

            if(mode.WriteSpeedPerformanceDescriptors != null)
                foreach(Modes.ModePage_2A_WriteDescriptor descriptor in mode.WriteSpeedPerformanceDescriptors.Where(descriptor => descriptor.WriteSpeed > 0)) if(descriptor.RotationControl == 0)
                        mmcOneValue.Add($"Drive supports writing at {descriptor.WriteSpeed} Kbyte/sec. in CLV mode");
                    else if(descriptor.RotationControl == 1)
                        mmcOneValue
                            .Add($"Drive supports writing at is {descriptor.WriteSpeed} Kbyte/sec. in pure CAV mode");

            if(mode.TestWrite) mmcOneValue.Add("Drive supports test writing");

            if(mode.ReadsBarcode) mmcOneValue.Add("Drive can read barcode");

            if(mode.ReadsBothSides) mmcOneValue.Add("Drive can read both sides of a disc");
            if(mode.LeadInPW) mmcOneValue.Add("Drive an read raw R-W subchannel from the Lead-In");

            if(mode.CSSandCPPMSupported) mmcOneValue.Add("Drive supports DVD CSS and/or DVD CPPM");

            if(mode.BufferUnderRunProtection) mmcOneValue.Add("Drive supports buffer under-run free recording");

            mmcOneValue.Sort();
            mmcOneValue.Add("");
        }
    }
}