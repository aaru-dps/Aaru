// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 2Ah: CD-ROM capabilities page.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI.Modes;

namespace Aaru.Decoders.SCSI
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public static partial class Modes
    {
        #region Mode Page 0x2A: CD-ROM capabilities page
        public static string PrettifyModePage_2A(byte[] pageResponse) =>
            PrettifyModePage_2A(ModePage_2A.Decode(pageResponse));

        public static string PrettifyModePage_2A(ModePage_2A modePage)
        {
            if(modePage is null)
                return null;

            ModePage_2A page = modePage;
            var         sb   = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM capabilities page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.AudioPlay)
                sb.AppendLine("\tDrive can play audio");

            if(page.Mode2Form1)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 1 format");

            if(page.Mode2Form2)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 2 format");

            if(page.MultiSession)
                sb.AppendLine("\tDrive supports multi-session discs and/or Photo-CD");

            if(page.CDDACommand)
                sb.AppendLine("\tDrive can read digital audio");

            if(page.AccurateCDDA)
                sb.AppendLine("\tDrive can continue from streaming loss");

            if(page.Subchannel)
                sb.AppendLine("\tDrive can read uncorrected and interleaved R-W subchannels");

            if(page.DeinterlaveSubchannel)
                sb.AppendLine("\tDrive can read, deinterleave and correct R-W subchannels");

            if(page.C2Pointer)
                sb.AppendLine("\tDrive supports C2 pointers");

            if(page.UPC)
                sb.AppendLine("\tDrive can read Media Catalogue Number");

            if(page.ISRC)
                sb.AppendLine("\tDrive can read ISRC");

            switch(page.LoadingMechanism)
            {
                case 0:
                    sb.AppendLine("\tDrive uses media caddy");

                    break;
                case 1:
                    sb.AppendLine("\tDrive uses a tray");

                    break;
                case 2:
                    sb.AppendLine("\tDrive is pop-up");

                    break;
                case 4:
                    sb.AppendLine("\tDrive is a changer with individually changeable discs");

                    break;
                case 5:
                    sb.AppendLine("\tDrive is a changer using cartridges");

                    break;
                default:
                    sb.AppendFormat("\tDrive uses unknown loading mechanism type {0}", page.LoadingMechanism).
                       AppendLine();

                    break;
            }

            if(page.Lock)
                sb.AppendLine("\tDrive can lock media");

            if(page.PreventJumper)
            {
                sb.AppendLine("\tDrive power ups locked");

                sb.AppendLine(page.LockState ? "\tDrive is locked, media cannot be ejected or inserted"
                                  : "\tDrive is not locked, media can be ejected and inserted");
            }
            else
                sb.AppendLine(page.LockState
                                  ? "\tDrive is locked, media cannot be ejected, but if empty, can be inserted"
                                  : "\tDrive is not locked, media can be ejected and inserted");

            if(page.Eject)
                sb.AppendLine("\tDrive can eject media");

            if(page.SeparateChannelMute)
                sb.AppendLine("\tEach channel can be muted independently");

            if(page.SeparateChannelVolume)
                sb.AppendLine("\tEach channel's volume can be controlled independently");

            if(page.SupportedVolumeLevels > 0)
                sb.AppendFormat("\tDrive supports {0} volume levels", page.SupportedVolumeLevels).AppendLine();

            if(page.BufferSize > 0)
                sb.AppendFormat("\tDrive has {0} Kbyte of buffer", page.BufferSize).AppendLine();

            if(page.MaximumSpeed > 0)
                sb.AppendFormat("\tDrive's maximum reading speed is {0} Kbyte/sec.", page.MaximumSpeed).AppendLine();

            if(page.CurrentSpeed > 0)
                sb.AppendFormat("\tDrive's current reading speed is {0} Kbyte/sec.", page.CurrentSpeed).AppendLine();

            if(page.ReadCDR)
            {
                sb.AppendLine(page.WriteCDR ? "\tDrive can read and write CD-R" : "\tDrive can read CD-R");

                if(page.Method2)
                    sb.AppendLine("\tDrive supports reading CD-R packet media");
            }

            if(page.ReadCDRW)
                sb.AppendLine(page.WriteCDRW ? "\tDrive can read and write CD-RW" : "\tDrive can read CD-RW");

            if(page.ReadDVDROM)
                sb.AppendLine("\tDrive can read DVD-ROM");

            if(page.ReadDVDR)
                sb.AppendLine(page.WriteDVDR ? "\tDrive can read and write DVD-R" : "\tDrive can read DVD-R");

            if(page.ReadDVDRAM)
                sb.AppendLine(page.WriteDVDRAM ? "\tDrive can read and write DVD-RAM" : "\tDrive can read DVD-RAM");

            if(page.Composite)
                sb.AppendLine("\tDrive can deliver a composite audio and video data stream");

            if(page.DigitalPort1)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 1");

            if(page.DigitalPort2)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 2");

            if(page.SDP)
                sb.AppendLine("\tDrive contains a changer that can report the exact contents of the slots");

            if(page.CurrentWriteSpeedSelected > 0)
            {
                if(page.RotationControlSelected == 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in CLV mode",
                                    page.CurrentWriteSpeedSelected).AppendLine();
                else if(page.RotationControlSelected == 1)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in pure CAV mode",
                                    page.CurrentWriteSpeedSelected).AppendLine();
            }
            else
            {
                if(page.MaxWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's maximum writing speed is {0} Kbyte/sec.", page.MaxWriteSpeed).
                       AppendLine();

                if(page.CurrentWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec.", page.CurrentWriteSpeed).
                       AppendLine();
            }

            if(page.WriteSpeedPerformanceDescriptors != null)
                foreach(ModePage_2A_WriteDescriptor descriptor in
                    page.WriteSpeedPerformanceDescriptors.Where(descriptor => descriptor.WriteSpeed > 0))
                    if(descriptor.RotationControl == 0)
                        sb.AppendFormat("\tDrive supports writing at {0} Kbyte/sec. in CLV mode",
                                        descriptor.WriteSpeed).AppendLine();
                    else if(descriptor.RotationControl == 1)
                        sb.AppendFormat("\tDrive supports writing at is {0} Kbyte/sec. in pure CAV mode",
                                        descriptor.WriteSpeed).AppendLine();

            if(page.TestWrite)
                sb.AppendLine("\tDrive supports test writing");

            if(page.ReadBarcode)
                sb.AppendLine("\tDrive can read barcode");

            if(page.SCC)
                sb.AppendLine("\tDrive can read both sides of a disc");

            if(page.LeadInPW)
                sb.AppendLine("\tDrive an read raw R-W subchannel from the Lead-In");

            if(page.CMRSupported == 1)
                sb.AppendLine("\tDrive supports DVD CSS and/or DVD CPPM");

            if(page.BUF)
                sb.AppendLine("\tDrive supports buffer under-run free recording");

            return sb.ToString();
        }
        #endregion Mode Page 0x2A: CD-ROM capabilities page
    }
}