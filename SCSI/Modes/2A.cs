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

namespace Aaru.Decoders.SCSI;

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

        var sb = new StringBuilder();

        sb.AppendLine("SCSI CD-ROM capabilities page:");

        if(modePage.PS)
            sb.AppendLine("\tParameters can be saved");

        if(modePage.AudioPlay)
            sb.AppendLine("\tDrive can play audio");

        if(modePage.Mode2Form1)
            sb.AppendLine("\tDrive can read sectors in Mode 2 Form 1 format");

        if(modePage.Mode2Form2)
            sb.AppendLine("\tDrive can read sectors in Mode 2 Form 2 format");

        if(modePage.MultiSession)
            sb.AppendLine("\tDrive supports multi-session discs and/or Photo-CD");

        if(modePage.CDDACommand)
            sb.AppendLine("\tDrive can read digital audio");

        if(modePage.AccurateCDDA)
            sb.AppendLine("\tDrive can continue from streaming loss");

        if(modePage.Subchannel)
            sb.AppendLine("\tDrive can read uncorrected and interleaved R-W subchannels");

        if(modePage.DeinterlaveSubchannel)
            sb.AppendLine("\tDrive can read, deinterleave and correct R-W subchannels");

        if(modePage.C2Pointer)
            sb.AppendLine("\tDrive supports C2 pointers");

        if(modePage.UPC)
            sb.AppendLine("\tDrive can read Media Catalogue Number");

        if(modePage.ISRC)
            sb.AppendLine("\tDrive can read ISRC");

        switch(modePage.LoadingMechanism)
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
                sb.AppendFormat("\tDrive uses unknown loading mechanism type {0}", modePage.LoadingMechanism).
                   AppendLine();

                break;
        }

        if(modePage.Lock)
            sb.AppendLine("\tDrive can lock media");

        if(modePage.PreventJumper)
        {
            sb.AppendLine("\tDrive power ups locked");

            sb.AppendLine(modePage.LockState ? "\tDrive is locked, media cannot be ejected or inserted"
                              : "\tDrive is not locked, media can be ejected and inserted");
        }
        else
            sb.AppendLine(modePage.LockState
                              ? "\tDrive is locked, media cannot be ejected, but if empty, can be inserted"
                              : "\tDrive is not locked, media can be ejected and inserted");

        if(modePage.Eject)
            sb.AppendLine("\tDrive can eject media");

        if(modePage.SeparateChannelMute)
            sb.AppendLine("\tEach channel can be muted independently");

        if(modePage.SeparateChannelVolume)
            sb.AppendLine("\tEach channel's volume can be controlled independently");

        if(modePage.SupportedVolumeLevels > 0)
            sb.AppendFormat("\tDrive supports {0} volume levels", modePage.SupportedVolumeLevels).AppendLine();

        if(modePage.BufferSize > 0)
            sb.AppendFormat("\tDrive has {0} Kbyte of buffer", modePage.BufferSize).AppendLine();

        if(modePage.MaximumSpeed > 0)
            sb.AppendFormat("\tDrive's maximum reading speed is {0} Kbyte/sec.", modePage.MaximumSpeed).AppendLine();

        if(modePage.CurrentSpeed > 0)
            sb.AppendFormat("\tDrive's current reading speed is {0} Kbyte/sec.", modePage.CurrentSpeed).AppendLine();

        if(modePage.ReadCDR)
        {
            sb.AppendLine(modePage.WriteCDR ? "\tDrive can read and write CD-R" : "\tDrive can read CD-R");

            if(modePage.Method2)
                sb.AppendLine("\tDrive supports reading CD-R packet media");
        }

        if(modePage.ReadCDRW)
            sb.AppendLine(modePage.WriteCDRW ? "\tDrive can read and write CD-RW" : "\tDrive can read CD-RW");

        if(modePage.ReadDVDROM)
            sb.AppendLine("\tDrive can read DVD-ROM");

        if(modePage.ReadDVDR)
            sb.AppendLine(modePage.WriteDVDR ? "\tDrive can read and write DVD-R" : "\tDrive can read DVD-R");

        if(modePage.ReadDVDRAM)
            sb.AppendLine(modePage.WriteDVDRAM ? "\tDrive can read and write DVD-RAM" : "\tDrive can read DVD-RAM");

        if(modePage.Composite)
            sb.AppendLine("\tDrive can deliver a composite audio and video data stream");

        if(modePage.DigitalPort1)
            sb.AppendLine("\tDrive supports IEC-958 digital output on port 1");

        if(modePage.DigitalPort2)
            sb.AppendLine("\tDrive supports IEC-958 digital output on port 2");

        if(modePage.SDP)
            sb.AppendLine("\tDrive contains a changer that can report the exact contents of the slots");

        if(modePage.CurrentWriteSpeedSelected > 0)
            switch(modePage.RotationControlSelected)
            {
                case 0:
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in CLV mode",
                                    modePage.CurrentWriteSpeedSelected).AppendLine();

                    break;
                case 1:
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in pure CAV mode",
                                    modePage.CurrentWriteSpeedSelected).AppendLine();

                    break;
            }
        else
        {
            if(modePage.MaxWriteSpeed > 0)
                sb.AppendFormat("\tDrive's maximum writing speed is {0} Kbyte/sec.", modePage.MaxWriteSpeed).
                   AppendLine();

            if(modePage.CurrentWriteSpeed > 0)
                sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec.", modePage.CurrentWriteSpeed).
                   AppendLine();
        }

        if(modePage.WriteSpeedPerformanceDescriptors != null)
            foreach(ModePage_2A_WriteDescriptor descriptor in
                    modePage.WriteSpeedPerformanceDescriptors.Where(descriptor => descriptor.WriteSpeed > 0))
                switch(descriptor.RotationControl)
                {
                    case 0:
                        sb.AppendFormat("\tDrive supports writing at {0} Kbyte/sec. in CLV mode",
                                        descriptor.WriteSpeed).AppendLine();

                        break;
                    case 1:
                        sb.AppendFormat("\tDrive supports writing at is {0} Kbyte/sec. in pure CAV mode",
                                        descriptor.WriteSpeed).AppendLine();

                        break;
                }

        if(modePage.TestWrite)
            sb.AppendLine("\tDrive supports test writing");

        if(modePage.ReadBarcode)
            sb.AppendLine("\tDrive can read barcode");

        if(modePage.SCC)
            sb.AppendLine("\tDrive can read both sides of a disc");

        if(modePage.LeadInPW)
            sb.AppendLine("\tDrive an read raw R-W subchannel from the Lead-In");

        if(modePage.CMRSupported == 1)
            sb.AppendLine("\tDrive supports DVD CSS and/or DVD CPPM");

        if(modePage.BUF)
            sb.AppendLine("\tDrive supports buffer under-run free recording");

        return sb.ToString();
    }
    #endregion Mode Page 0x2A: CD-ROM capabilities page
}