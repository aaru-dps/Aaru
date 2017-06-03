// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ScsiMmc.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Metadata;
using System.Collections.Generic;
namespace DiscImageChef.Server.App_Start
{
    public static class ScsiMmc
    {
        public static void Report(mmcType mmc, ref List<string> mmcOneValue, ref testedMediaType[] testedMedia)
        {
            testedMedia = mmc.TestedMedia;

            if(mmc.ModeSense2A != null)
            {
                if(mmc.ModeSense2A.PlaysAudio)
                    mmcOneValue.Add("Drive can play audio");
                if(mmc.ModeSense2A.ReadsMode2Form1)
                    mmcOneValue.Add("Drive can read sectors in Mode 2 Form 1 format");
                if(mmc.ModeSense2A.ReadsMode2Form2)
                    mmcOneValue.Add("Drive can read sectors in Mode 2 Form 2 format");
                if(mmc.ModeSense2A.SupportsMultiSession)
                    mmcOneValue.Add("Drive supports multi-session discs and/or Photo-CD");

                if(mmc.ModeSense2A.CDDACommand)
                    mmcOneValue.Add("Drive can read digital audio");
                if(mmc.ModeSense2A.AccurateCDDA)
                    mmcOneValue.Add("Drive can continue from streaming loss");
                if(mmc.ModeSense2A.ReadsSubchannel)
                    mmcOneValue.Add("Drive can read uncorrected and interleaved R-W subchannels");
                if(mmc.ModeSense2A.ReadsDeinterlavedSubchannel)
                    mmcOneValue.Add("Drive can read, deinterleave and correct R-W subchannels");
                if(mmc.ModeSense2A.ReturnsC2Pointers)
                    mmcOneValue.Add("Drive supports C2 pointers");
                if(mmc.ModeSense2A.ReadsUPC)
                    mmcOneValue.Add("Drive can read Media Catalogue Number");
                if(mmc.ModeSense2A.ReadsISRC)
                    mmcOneValue.Add("Drive can read ISRC");

                switch(mmc.ModeSense2A.LoadingMechanismType)
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
                        mmcOneValue.Add(string.Format("Drive uses unknown loading mechanism type {0}", mmc.ModeSense2A.LoadingMechanismType));
                        break;
                }

                if(mmc.ModeSense2A.CanLockMedia)
                    mmcOneValue.Add("Drive can lock media");
                if(mmc.ModeSense2A.PreventJumperStatus)
                {
                    mmcOneValue.Add("Drive power ups locked");
                    if(mmc.ModeSense2A.LockStatus)
                        mmcOneValue.Add("Drive is locked, media cannot be ejected or inserted");
                    else
                        mmcOneValue.Add("Drive is not locked, media can be ejected and inserted");
                }
                else
                {
                    if(mmc.ModeSense2A.LockStatus)
                        mmcOneValue.Add("Drive is locked, media cannot be ejected, but if empty, can be inserted");
                    else
                        mmcOneValue.Add("Drive is not locked, media can be ejected and inserted");
                }
                if(mmc.ModeSense2A.CanEject)
                    mmcOneValue.Add("Drive can eject media");

                if(mmc.ModeSense2A.SeparateChannelMute)
                    mmcOneValue.Add("Each channel can be muted independently");
                if(mmc.ModeSense2A.SeparateChannelVolume)
                    mmcOneValue.Add("Each channel's volume can be controlled independently");

                if(mmc.ModeSense2A.SupportedVolumeLevels > 0)
                    mmcOneValue.Add(string.Format("Drive supports {0} volume levels", mmc.ModeSense2A.SupportedVolumeLevels));
                if(mmc.ModeSense2A.BufferSize > 0)
                    mmcOneValue.Add(string.Format("Drive has {0} Kbyte of buffer", mmc.ModeSense2A.BufferSize));
                if(mmc.ModeSense2A.MaximumSpeed > 0)
                    mmcOneValue.Add(string.Format("Drive's maximum reading speed is {0} Kbyte/sec.", mmc.ModeSense2A.MaximumSpeed));
                if(mmc.ModeSense2A.CurrentSpeed > 0)
                    mmcOneValue.Add(string.Format("Drive's current reading speed is {0} Kbyte/sec.", mmc.ModeSense2A.CurrentSpeed));

                if(mmc.ModeSense2A.ReadsCDR)
                {
                    if(mmc.ModeSense2A.WritesCDR)
                        mmcOneValue.Add("Drive can read and write CD-R");
                    else
                        mmcOneValue.Add("Drive can read CD-R");

                    if(mmc.ModeSense2A.ReadsPacketCDR)
                        mmcOneValue.Add("Drive supports reading CD-R packet media");
                }

                if(mmc.ModeSense2A.ReadsCDRW)
                {
                    if(mmc.ModeSense2A.WritesCDRW)
                        mmcOneValue.Add("Drive can read and write CD-RW");
                    else
                        mmcOneValue.Add("Drive can read CD-RW");
                }

                if(mmc.ModeSense2A.ReadsDVDROM)
                    mmcOneValue.Add("Drive can read DVD-ROM");
                if(mmc.ModeSense2A.ReadsDVDR)
                {
                    if(mmc.ModeSense2A.WritesDVDR)
                        mmcOneValue.Add("Drive can read and write DVD-R");
                    else
                        mmcOneValue.Add("Drive can read DVD-R");
                }
                if(mmc.ModeSense2A.ReadsDVDRAM)
                {
                    if(mmc.ModeSense2A.WritesDVDRAM)
                        mmcOneValue.Add("Drive can read and write DVD-RAM");
                    else
                        mmcOneValue.Add("Drive can read DVD-RAM");
                }

                if(mmc.ModeSense2A.CompositeAudioVideo)
                    mmcOneValue.Add("Drive can deliver a composite audio and video data stream");
                if(mmc.ModeSense2A.DigitalPort1)
                    mmcOneValue.Add("Drive supports IEC-958 digital output on port 1");
                if(mmc.ModeSense2A.DigitalPort2)
                    mmcOneValue.Add("Drive supports IEC-958 digital output on port 2");

                if(mmc.ModeSense2A.DeterministicSlotChanger)
                    mmcOneValue.Add("Drive contains a changer that can report the exact contents of the slots");
                if(mmc.ModeSense2A.CurrentWriteSpeedSelected > 0)
                {
                    if(mmc.ModeSense2A.RotationControlSelected == 0)
                        mmcOneValue.Add(string.Format("Drive's current writing speed is {0} Kbyte/sec. in CLV mode", mmc.ModeSense2A.CurrentWriteSpeedSelected));
                    else if(mmc.ModeSense2A.RotationControlSelected == 1)
                        mmcOneValue.Add(string.Format("Drive's current writing speed is {0} Kbyte/sec. in pure CAV mode", mmc.ModeSense2A.CurrentWriteSpeedSelected));
                }
                else
                {
                    if(mmc.ModeSense2A.MaximumWriteSpeed > 0)
                        mmcOneValue.Add(string.Format("Drive's maximum writing speed is {0} Kbyte/sec.", mmc.ModeSense2A.MaximumWriteSpeed));
                    if(mmc.ModeSense2A.CurrentWriteSpeed > 0)
                        mmcOneValue.Add(string.Format("Drive's current writing speed is {0} Kbyte/sec.", mmc.ModeSense2A.CurrentWriteSpeed));
                }

                if(mmc.ModeSense2A.WriteSpeedPerformanceDescriptors != null)
                {
                    foreach(Decoders.SCSI.Modes.ModePage_2A_WriteDescriptor descriptor in mmc.ModeSense2A.WriteSpeedPerformanceDescriptors)
                    {
                        if(descriptor.WriteSpeed > 0)
                        {
                            if(descriptor.RotationControl == 0)
                                mmcOneValue.Add(string.Format("Drive supports writing at {0} Kbyte/sec. in CLV mode", descriptor.WriteSpeed));
                            else if(descriptor.RotationControl == 1)
                                mmcOneValue.Add(string.Format("Drive supports writing at is {0} Kbyte/sec. in pure CAV mode", descriptor.WriteSpeed));
                        }
                    }
                }

                if(mmc.ModeSense2A.TestWrite)
                    mmcOneValue.Add("Drive supports test writing");

                if(mmc.ModeSense2A.ReadsBarcode)
                    mmcOneValue.Add("Drive can read barcode");

                if(mmc.ModeSense2A.ReadsBothSides)
                    mmcOneValue.Add("Drive can read both sides of a disc");
                if(mmc.ModeSense2A.LeadInPW)
                    mmcOneValue.Add("Drive an read raw R-W subchannel from the Lead-In");

                if(mmc.ModeSense2A.CSSandCPPMSupported)
                    mmcOneValue.Add("Drive supports DVD CSS and/or DVD CPPM");

                if(mmc.ModeSense2A.BufferUnderRunProtection)
                    mmcOneValue.Add("Drive supports buffer under-run free recording");
            }

            if(mmc.Features != null)
            {

                    sb.AppendLine("MMC Core Feature:");
                    sb.Append("\tDrive uses ");
                    switch(ftr.PhysicalInterfaceStandard)
                    {
                        case PhysicalInterfaces.Unspecified:
                            sb.AppendLine("an unspecified physical interface");
                            break;
                        case PhysicalInterfaces.SCSI:
                            sb.AppendLine("SCSI interface");
                            break;
                        case PhysicalInterfaces.ATAPI:
                            sb.AppendLine("ATAPI interface");
                            break;
                        case PhysicalInterfaces.IEEE1394:
                            sb.AppendLine("IEEE-1394 interface");
                            break;
                        case PhysicalInterfaces.IEEE1394A:
                            sb.AppendLine("IEEE-1394A interface");
                            break;
                        case PhysicalInterfaces.FC:
                            sb.AppendLine("Fibre Channel interface");
                            break;
                        case PhysicalInterfaces.IEEE1394B:
                            sb.AppendLine("IEEE-1394B interface");
                            break;
                        case PhysicalInterfaces.SerialATAPI:
                            sb.AppendLine("Serial ATAPI interface");
                            break;
                        case PhysicalInterfaces.USB:
                            sb.AppendLine("USB interface");
                            break;
                        case PhysicalInterfaces.Vendor:
                            sb.AppendLine("a vendor unique interface");
                            break;
                        default:
                            sb.AppendFormat("an unknown interface with code {0}", (uint)ftr.PhysicalInterfaceStandard).AppendLine();
                            break;
                    }

                    if(ftr.DBE)
                        mmcOneValue.Add("Drive supports Device Busy events");
                    if(ftr.INQ2)
                        mmcOneValue.Add("Drive supports EVPD, Page Code and 16-bit Allocation Length as described in SPC-3");

                    if(ftr.Async)
                        mmcOneValue.Add("Drive supports polling and asynchronous GET EVENT STATUS NOTIFICATION");
                    else
                        mmcOneValue.Add("Drive supports only polling GET EVENT STATUS NOTIFICATION");

                    if(ftr.OCEvent)
                        mmcOneValue.Add("Drive supports operational change request / notification class events");


                    switch(ftr.LoadingMechanismType)
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
                            sb.AppendFormat("\tDrive uses unknown loading mechanism type {0}", ftr.LoadingMechanismType).AppendLine();
                            break;
                    }

                    if(ftr.Lock)
                        mmcOneValue.Add("Drive can lock media");
                    if(ftr.PreventJumper)
                        mmcOneValue.Add("Drive power ups locked");
                    if(ftr.Eject)
                        mmcOneValue.Add("Drive can eject media");
                    if(ftr.Load)
                        mmcOneValue.Add("Drive can load media");
                    if(ftr.DBML)
                        mmcOneValue.Add("Drive reports Device Busy Class events during medium loading/unloading");

                    
                    if(ftr.DWP)
                        mmcOneValue.Add("Drive supports reading/writing the Disc Write Protect PAC on BD-R/-RE media");
                    if(ftr.WDCB)
                        mmcOneValue.Add("Drive supports writing the Write Inhibit DCB on DVD+RW media");
                    if(ftr.SPWP)
                        mmcOneValue.Add("Drive supports set/release of PWP status");
                    if(ftr.SSWPP)
                        mmcOneValue.Add("Drive supports the SWPP bit of the Timeout and Protect mode page");

                    

                    if(ftr.PP)
                        mmcOneValue.Add("Drive shall report Read/Write Error Recovery mode page");
                    if(ftr.LogicalBlockSize > 0)
                        sb.AppendFormat("\t{0} bytes per logical block", ftr.LogicalBlockSize).AppendLine();
                    if(ftr.Blocking > 1)
                        sb.AppendFormat("\t{0} logical blocks per media readable unit", ftr.Blocking).AppendLine();

                    
                    return !feature.HasValue ? null : "Drive claims capability to read all CD formats according to OSTA Multi-Read Specification\n";
                    


                    if(ftr.DAP)
                        mmcOneValue.Add("Drive supports the DAP bit in the READ CD and READ CD MSF commands");
                    if(ftr.C2)
                        mmcOneValue.Add("Drive supports C2 Error Pointers");
                    if(ftr.CDText)
                        mmcOneValue.Add("Drive can return CD-Text from Lead-In");

                    

                    mmcOneValue.Add("Drive can read DVD media");

                    if(ftr.DualR)
                        mmcOneValue.Add("Drive can read DVD-R DL from all recording modes");
                    if(ftr.DualRW)
                        mmcOneValue.Add("Drive can read DVD-RW DL from all recording modes");
                    if(ftr.MULTI110)
                        mmcOneValue.Add("Drive conforms to DVD Multi Drive Read-only Specifications");


                    if(ftr.PP)
                        mmcOneValue.Add("Drive shall report Read/Write Error Recovery mode page");
                    if(ftr.LogicalBlockSize > 0)
                        sb.AppendFormat("\t{0} bytes per logical block", ftr.LogicalBlockSize).AppendLine();
                    if(ftr.Blocking > 1)
                        sb.AppendFormat("\t{0} logical blocks per media writable unit", ftr.Blocking).AppendLine();
                    if(ftr.LastLBA > 0)
                        sb.AppendFormat("\tLast adressable logical block is {0}", ftr.LastLBA).AppendLine();


                    if(ftr.DataTypeSupported > 0)
                    {
                        sb.Append("\tDrive supports data block types:");
                        if((ftr.DataTypeSupported & 0x0001) == 0x0001)
                            sb.Append(" 0");
                        if((ftr.DataTypeSupported & 0x0002) == 0x0002)
                            sb.Append(" 1");
                        if((ftr.DataTypeSupported & 0x0004) == 0x0004)
                            sb.Append(" 2");
                        if((ftr.DataTypeSupported & 0x0008) == 0x0008)
                            sb.Append(" 3");
                        if((ftr.DataTypeSupported & 0x0010) == 0x0010)
                            sb.Append(" 4");
                        if((ftr.DataTypeSupported & 0x0020) == 0x0020)
                            sb.Append(" 5");
                        if((ftr.DataTypeSupported & 0x0040) == 0x0040)
                            sb.Append(" 6");
                        if((ftr.DataTypeSupported & 0x0080) == 0x0080)
                            sb.Append(" 7");
                        if((ftr.DataTypeSupported & 0x0100) == 0x0100)
                            sb.Append(" 8");
                        if((ftr.DataTypeSupported & 0x0200) == 0x0200)
                            sb.Append(" 9");
                        if((ftr.DataTypeSupported & 0x0400) == 0x0400)
                            sb.Append(" 10");
                        if((ftr.DataTypeSupported & 0x0800) == 0x0800)
                            sb.Append(" 11");
                        if((ftr.DataTypeSupported & 0x1000) == 0x1000)
                            sb.Append(" 12");
                        if((ftr.DataTypeSupported & 0x2000) == 0x2000)
                            sb.Append(" 13");
                        if((ftr.DataTypeSupported & 0x4000) == 0x4000)
                            sb.Append(" 14");
                        if((ftr.DataTypeSupported & 0x8000) == 0x8000)
                            sb.Append(" 15");
                        sb.AppendLine();
                    }

                    if(ftr.TRIO)
                        mmcOneValue.Add("Drive claims support to report Track Resources Information");
                    if(ftr.ARSV)
                        mmcOneValue.Add("Drive supports address mode reservation on the RESERVE TRACK command");
                    if(ftr.BUF)
                        mmcOneValue.Add("Drive is capable of zero loss linking");

                    
                    mmcOneValue.Add("Drive can format media into logical blocks");

                    if(ftr.RENoSA)
                        mmcOneValue.Add("Drive can format BD-RE with no spares allocated");
                    if(ftr.Expand)
                        mmcOneValue.Add("Drive can expand the spare area on a formatted BD-RE disc");
                    if(ftr.QCert)
                        mmcOneValue.Add("Drive can format BD-RE discs with quick certification");
                    if(ftr.Cert)
                        mmcOneValue.Add("Drive can format BD-RE discs with full certification");
                    if(ftr.FRF)
                        mmcOneValue.Add("Drive can fast re-format BD-RE discs");
                    if(ftr.RRM)
                        mmcOneValue.Add("Drive can format BD-R discs with RRM format");


                    sb.AppendLine("MMC Hardware Defect Management:");
                    mmcOneValue.Add("Drive shall be able to provide a defect-free contiguous address space");
                    if(ftr.SSA)
                        mmcOneValue.Add("Drive can return Spare Area Information");



                    if(ftr.PP)
                        mmcOneValue.Add("Drive shall report Read/Write Error Recovery mode page");
                    if(ftr.LogicalBlockSize > 0)
                        sb.AppendFormat("\t{0} bytes per logical block", ftr.LogicalBlockSize).AppendLine();
                    if(ftr.Blocking > 1)
                        sb.AppendFormat("\t{0} logical blocks per media writable unit", ftr.Blocking).AppendLine();

                    
                    return !feature.HasValue ? null : "Drive shall have the ability to overwrite logical blocks only in fixed sets at a time\n";
                    

                    sb.Append("Drive can write High-Speed CD-RW");
                    

                    if(ftr.Write && ftr.DVDPRead && ftr.DVDPWrite)
                        sb.Append("Drive can read and write CD-MRW and DVD+MRW");
                    else if(ftr.DVDPRead && ftr.DVDPWrite)
                        sb.Append("Drive can read and write DVD+MRW");
                    else if(ftr.Write && ftr.DVDPRead)
                        sb.Append("Drive and read DVD+MRW and read and write CD-MRW");
                    else if(ftr.Write)
                        sb.Append("Drive can read and write CD-MRW");
                    else if(ftr.DVDPRead)
                        sb.Append("Drive can read CD-MRW and DVD+MRW");
                    else
                        sb.Append("Drive can read CD-MRW");


                    if(ftr.DRTDM)
                        mmcOneValue.Add("Drive supports DRT-DM mode");
                    else
                        mmcOneValue.Add("Drive supports Persistent-DM mode");

                    if(ftr.DBICacheZones > 0)
                        sb.AppendFormat("\tDrive has {0} DBI cache zones", ftr.DBICacheZones).AppendLine();
                    if(ftr.Entries > 0)
                        sb.AppendFormat("\tDrive has {0} DBI entries", ftr.Entries).AppendLine();

                    

                    if(ftr.Write)
                    {
                        sb.Append("Drive can read and write DVD+RW");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                        if(ftr.CloseOnly)
                            mmcOneValue.Add("Drive supports only the read compatibility stop");
                        else
                            mmcOneValue.Add("Drive supports both forms of background format stopping");
                        if(ftr.QuickStart)
                            mmcOneValue.Add("Drive can do a quick start formatting");
                    }
                    else
                    {
                        sb.Append("Drive can read DVD+RW");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }

                    if(ftr.Write)
                    {
                        sb.Append("Drive can read and write DVD+R");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }
                    else
                    {
                        sb.Append("Drive can read DVD+R");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }


                    if(ftr.Blank)
                        mmcOneValue.Add("Drive supports the BLANK command");
                    if(ftr.Intermediate)
                        mmcOneValue.Add("Drive supports writing on an intermediate state session and quick formatting");
                    if(ftr.DSDR)
                        mmcOneValue.Add("Drive can read Defect Status data recorded on the medium");
                    if(ftr.DSDG)
                        mmcOneValue.Add("Drive can generate Defect Status data during formatting");



                    sb.AppendLine("Drive can write CDs in Track at Once Mode:");

                    if(ftr.RWSubchannel)
                    {
                        mmcOneValue.Add("Drive can write user provided data in the R-W subchannels");
                        if(ftr.RWRaw)
                            mmcOneValue.Add("Drive accepts RAW R-W subchannel data");
                        if(ftr.RWPack)
                            mmcOneValue.Add("Drive accepts Packed R-W subchannel data");

                    }

                    if(ftr.CDRW)
                        mmcOneValue.Add("Drive can overwrite a TAO track with another in CD-RWs");
                    if(ftr.TestWrite)
                        mmcOneValue.Add("Drive can do a test writing");
                    if(ftr.BUF)
                        mmcOneValue.Add("Drive supports zero loss linking");

                    if(ftr.DataTypeSupported > 0)
                    {
                        sb.Append("\tDrive supports data block types:");
                        if((ftr.DataTypeSupported & 0x0001) == 0x0001)
                            sb.Append(" 0");
                        if((ftr.DataTypeSupported & 0x0002) == 0x0002)
                            sb.Append(" 1");
                        if((ftr.DataTypeSupported & 0x0004) == 0x0004)
                            sb.Append(" 2");
                        if((ftr.DataTypeSupported & 0x0008) == 0x0008)
                            sb.Append(" 3");
                        if((ftr.DataTypeSupported & 0x0010) == 0x0010)
                            sb.Append(" 4");
                        if((ftr.DataTypeSupported & 0x0020) == 0x0020)
                            sb.Append(" 5");
                        if((ftr.DataTypeSupported & 0x0040) == 0x0040)
                            sb.Append(" 6");
                        if((ftr.DataTypeSupported & 0x0080) == 0x0080)
                            sb.Append(" 7");
                        if((ftr.DataTypeSupported & 0x0100) == 0x0100)
                            sb.Append(" 8");
                        if((ftr.DataTypeSupported & 0x0200) == 0x0200)
                            sb.Append(" 9");
                        if((ftr.DataTypeSupported & 0x0400) == 0x0400)
                            sb.Append(" 10");
                        if((ftr.DataTypeSupported & 0x0800) == 0x0800)
                            sb.Append(" 11");
                        if((ftr.DataTypeSupported & 0x1000) == 0x1000)
                            sb.Append(" 12");
                        if((ftr.DataTypeSupported & 0x2000) == 0x2000)
                            sb.Append(" 13");
                        if((ftr.DataTypeSupported & 0x4000) == 0x4000)
                            sb.Append(" 14");
                        if((ftr.DataTypeSupported & 0x8000) == 0x8000)
                            sb.Append(" 15");
                        sb.AppendLine();

                    if(ftr.SAO && !ftr.RAW)
                        sb.AppendLine("Drive can write CDs in Session at Once Mode:");
                    else if(!ftr.SAO && ftr.RAW)
                        sb.AppendLine("Drive can write CDs in raw Mode:");
                    else
                        sb.AppendLine("Drive can write CDs in Session at Once and in Raw Modes:");

                    if(ftr.RAW && ftr.RAWMS)
                        mmcOneValue.Add("Drive can write multi-session CDs in raw mode");

                    if(ftr.RW)
                        mmcOneValue.Add("Drive can write user provided data in the R-W subchannels");

                    if(ftr.CDRW)
                        mmcOneValue.Add("Drive can write CD-RWs");
                    if(ftr.TestWrite)
                        mmcOneValue.Add("Drive can do a test writing");
                    if(ftr.BUF)
                        mmcOneValue.Add("Drive supports zero loss linking");

                    if(ftr.MaxCueSheet > 0)
                        sb.AppendFormat("\tDrive supports a maximum of {0} bytes in a single cue sheet", ftr.MaxCueSheet).AppendLine();

                    if(ftr.DVDRW && ftr.RDL)
                        sb.AppendLine("Drive supports writing DVD-R, DVD-RW and DVD-R DL");
                    else if(ftr.RDL)
                        sb.AppendLine("Drive supports writing DVD-R and DVD-R DL");
                    else if(ftr.DVDRW)
                        sb.AppendLine("Drive supports writing DVD-R and DVD-RW");
                    else
                        sb.AppendLine("Drive supports writing DVD-R");

                    if(ftr.TestWrite)
                        mmcOneValue.Add("Drive can do a test writing");
                    if(ftr.BUF)
                        mmcOneValue.Add("Drive supports zero loss linking");

                    return !feature.HasValue ? null : "Drive can read DDCDs\n";

                    sb.AppendLine("Drive supports writing DDCD-R");

                    if(ftr.TestWrite)
                        mmcOneValue.Add("Drive can do a test writing");


                    sb.AppendLine("Drive supports writing DDCD-RW");

                    if(ftr.Blank)
                        mmcOneValue.Add("Drive supports the BLANK command");
                    if(ftr.Intermediate)
                        mmcOneValue.Add("Drive supports quick formatting");


                    if(ftr.LinkSizes != null)
                    {
                        foreach(byte link in ftr.LinkSizes)
                            sb.AppendFormat("\tCurrent media has a {0} bytes link available", link).AppendLine();
                    }

                    
                    return !feature.HasValue ? null : "Drive can stop a long immediate operation\n";

                    sb.AppendLine("Drive can write CD-RW");
                    if(ftr.SubtypeSupport > 0)
                    {
                        sb.Append("\tDrive supports CD-RW subtypes");
                        if((ftr.SubtypeSupport & 0x01) == 0x01)
                            sb.Append(" 0");
                        if((ftr.SubtypeSupport & 0x02) == 0x02)
                            sb.Append(" 1");
                        if((ftr.SubtypeSupport & 0x04) == 0x04)
                            sb.Append(" 2");
                        if((ftr.SubtypeSupport & 0x08) == 0x08)
                            sb.Append(" 3");
                        if((ftr.SubtypeSupport & 0x10) == 0x10)
                            sb.Append(" 4");
                        if((ftr.SubtypeSupport & 0x20) == 0x20)
                            sb.Append(" 5");
                        if((ftr.SubtypeSupport & 0x40) == 0x40)
                            sb.Append(" 6");
                        if((ftr.SubtypeSupport & 0x80) == 0x80)
                            sb.Append(" 7");
                        sb.AppendLine();
                    }
                    return !feature.HasValue ? null : "Drive can write BD-R on Pseudo-OVerwrite SRM mode\n";

                    if(ftr.Write)
                    {
                        sb.Append("Drive can read and write DVD+RW DL");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                        if(ftr.CloseOnly)
                            mmcOneValue.Add("Drive supports only the read compatibility stop");
                        else
                            mmcOneValue.Add("Drive supports both forms of background format stopping");
                        if(ftr.QuickStart)
                            mmcOneValue.Add("Drive can do a quick start formatting");
                    }
                    else
                    {
                        sb.Append("Drive can read DVD+RW DL");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }


                    if(ftr.Write)
                    {
                        sb.Append("Drive can read and write DVD+R DL");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }
                    else
                    {
                        sb.Append("Drive can read DVD+R DL");
                        if(ftr.Current)
                            sb.AppendLine(" (current)");
                        else
                            sb.AppendLine();
                    }



                    if(ftr.OldROM)
                        mmcOneValue.Add("Drive can read BD-ROM pre-1.0");
                    if(ftr.ROM)
                        mmcOneValue.Add("Drive can read BD-ROM Ver.1");
                    if(ftr.OldR)
                        mmcOneValue.Add("Drive can read BD-R pre-1.0");
                    if(ftr.R)
                        mmcOneValue.Add("Drive can read BD-R Ver.1");
                    if(ftr.OldRE)
                        mmcOneValue.Add("Drive can read BD-RE pre-1.0");
                    if(ftr.RE1)
                        mmcOneValue.Add("Drive can read BD-RE Ver.1");
                    if(ftr.RE2)
                        mmcOneValue.Add("Drive can read BD-RE Ver.2");

                    if(ftr.BCA)
                        mmcOneValue.Add("Drive can read BD's Burst Cutting Area");


                    if(ftr.OldR)
                        mmcOneValue.Add("Drive can write BD-R pre-1.0");
                    if(ftr.R)
                        mmcOneValue.Add("Drive can write BD-R Ver.1");
                    if(ftr.OldRE)
                        mmcOneValue.Add("Drive can write BD-RE pre-1.0");
                    if(ftr.RE1)
                        mmcOneValue.Add("Drive can write BD-RE Ver.1");
                    if(ftr.RE2)
                        mmcOneValue.Add("Drive can write BD-RE Ver.2");

                    if(ftr.SVNR)
                        mmcOneValue.Add("Drive supports write without verify requirement");

                    return !feature.HasValue ? null : "Drive is able to detect and report defective writable unit and behave accordinly\n";
                    if(ftr.HDDVDR && ftr.HDDVDRAM)
                        sb.Append("Drive can read HD DVD-ROM, HD DVD-RW, HD DVD-R and HD DVD-RAM");
                    else if(ftr.HDDVDR)
                        sb.Append("Drive can read HD DVD-ROM, HD DVD-RW and HD DVD-R");
                    else if(ftr.HDDVDRAM)
                        sb.Append("Drive can read HD DVD-ROM, HD DVD-RW and HD DVD-RAM");
                    else
                        sb.Append("Drive can read HD DVD-ROM and HD DVD-RW");

                    if(ftr.HDDVDR && ftr.HDDVDRAM)
                        sb.Append("Drive can write HD DVD-RW, HD DVD-R and HD DVD-RAM");
                    else if(ftr.HDDVDR)
                        sb.Append("Drive can write HD DVD-RW and HD DVD-R");
                    else if(ftr.HDDVDRAM)
                        sb.Append("Drive can write HD DVD-RW and HD DVD-RAM");
                    else
                        sb.Append("Drive can write HD DVD-RW");


                    sb.Append("Drive is able to access Hybrid discs");

                    if(ftr.RI)
                        mmcOneValue.Add("Drive is able to maintain the online format layer through reset and power cycling");

                    return !feature.HasValue ? null : "Drive is able to perform host and drive directed power management\n";

                    sb.AppendLine("Drive supports S.M.A.R.T.");
                    if(ftr.PP)
                        mmcOneValue.Add("Drive supports the Informational Exceptions Control mode page 1Ch");


                    sb.AppendLine("MMC Embedded Changer:");

                    if(ftr.SCC)
                        mmcOneValue.Add("Drive can change disc side");
                    if(ftr.SDP)
                        mmcOneValue.Add("Drive is able to report slots contents after a reset or change");

                    sb.AppendFormat("\tDrive has {0} slots", ftr.HighestSlotNumber + 1).AppendLine();


                    sb.AppendLine("Drive has an analogue audio output");

                    if(ftr.Scan)
                        mmcOneValue.Add("Drive supports the SCAN command");
                    if(ftr.SCM)
                        mmcOneValue.Add("Drive is able to mute channels separately");
                    if(ftr.SV)
                        mmcOneValue.Add("Drive supports separate volume per channel");

                    sb.AppendFormat("\tDrive has {0} volume levels", ftr.VolumeLevels + 1).AppendLine();


                    sb.AppendLine("Drive supports Microcode Upgrade");
                    if(ftr.M5)
                        sb.AppendLine("Drive supports validating the 5-bit Mode of the READ BUFFER and WRITE BUFFER commands");


                    sb.AppendLine("Drive supports Timeout & Protect mode page 1Dh");

                    if(ftr.Group3)
                    {
                        mmcOneValue.Add("Drive supports the Group3 in Timeout & Protect mode page 1Dh");
                        if(ftr.UnitLength > 0)
                            sb.AppendFormat("\tDrive has {0} increase of Group 3 time unit", ftr.UnitLength).AppendLine();
                    }


                    sb.AppendFormat("Drive supports DVD CSS/CPPM version {0}", ftr.CSSVersion);
                    if(ftr.Current)
                        sb.AppendLine(" and current disc is encrypted");
                    else
                        sb.AppendLine();


                    sb.AppendLine("MMC Real Time Streaming:");

                    if(ftr.SMP)
                        mmcOneValue.Add("Drive supports Set Minimum Performance with the SET STREAMING command");
                    if(ftr.RBCB)
                        mmcOneValue.Add("Drive supports the block bit in the READ BUFFER CAPACITY command");
                    if(ftr.SCS)
                        mmcOneValue.Add("Drive supports the SET CD SPEED command");
                    if(ftr.MP2A)
                        mmcOneValue.Add("Drive supports the Write Speed Performance Descriptor Blocks in the MMC mode page 2Ah");
                    if(ftr.WSPD)
                        mmcOneValue.Add("Drive supports the Write Speed data of GET PERFORMANCE and the WRC field of SET STREAMING");
                    if(ftr.SW)
                        mmcOneValue.Add("Drive supports stream recording");

                    return !feature.HasValue ? null : "Drive is to read media serial number\n";

                    if(ftr.DCBs != null)
                    {
                        foreach(uint DCB in ftr.DCBs)
                            sb.AppendFormat("Drive supports DCB {0:X8}h", DCB).AppendLine();
                    }



                    sb.AppendFormat("Drive supports DVD CPRM version {0}", ftr.CPRMVersion);
                    if(ftr.Current)
                        sb.AppendLine(" and current disc is or can be encrypted");
                    else
                        sb.AppendLine();


                    string syear, smonth, sday, shour, sminute, ssecond;
                    byte[] temp;

                    temp = new byte[4];
                    temp[0] = (byte)((ftr.Century & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Century & 0xFF);
                    temp[2] = (byte)((ftr.Year & 0xFF00) >> 8);
                    temp[3] = (byte)(ftr.Year & 0xFF);
                    syear = Encoding.ASCII.GetString(temp);
                    temp = new byte[2];
                    temp[0] = (byte)((ftr.Month & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Month & 0xFF);
                    smonth = Encoding.ASCII.GetString(temp);
                    temp = new byte[2];
                    temp[0] = (byte)((ftr.Day & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Day & 0xFF);
                    sday = Encoding.ASCII.GetString(temp);
                    temp = new byte[2];
                    temp[0] = (byte)((ftr.Hour & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Hour & 0xFF);
                    shour = Encoding.ASCII.GetString(temp);
                    temp = new byte[2];
                    temp[0] = (byte)((ftr.Minute & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Minute & 0xFF);
                    sminute = Encoding.ASCII.GetString(temp);
                    temp = new byte[2];
                    temp[0] = (byte)((ftr.Second & 0xFF00) >> 8);
                    temp[1] = (byte)(ftr.Second & 0xFF);
                    ssecond = Encoding.ASCII.GetString(temp);

                    try
                    {
                        DateTime fwDate = new DateTime(int.Parse(syear), int.Parse(smonth),
                                              int.Parse(sday), int.Parse(shour), int.Parse(sminute),
                                              int.Parse(ssecond), DateTimeKind.Utc);

                        sb.AppendFormat("Drive firmware is dated {0}", fwDate).AppendLine();
                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                    {
                    }


                    sb.AppendFormat("Drive supports AACS version {0}", ftr.AACSVersion);
                    if(ftr.Current)
                        sb.AppendLine(" and current disc is encrypted");
                    else
                        sb.AppendLine();

                    if(ftr.RDC)
                        mmcOneValue.Add("Drive supports reading the Drive Certificate");
                    if(ftr.RMC)
                        mmcOneValue.Add("Drive supports reading Media Key Block of CPRM");
                    if(ftr.WBE)
                        mmcOneValue.Add("Drive supports writing with bus encryption");
                    if(ftr.BEC)
                        mmcOneValue.Add("Drive supports bus encryption");
                    if(ftr.BNG)
                    {
                        mmcOneValue.Add("Drive supports generating the binding nonce");
                        if(ftr.BindNonceBlocks > 0)
                            sb.AppendFormat("\t{0} media blocks are required for the binding nonce", ftr.BindNonceBlocks).AppendLine();
                    }
                    if(ftr.AGIDs > 0)
                        sb.AppendFormat("\tDrive supports {0} AGIDs concurrently", ftr.AGIDs).AppendLine();

                        

                    sb.Append("Drive supports DVD-Download");
                    if(ftr.Current)
                        sb.AppendLine(" (current)");
                    else
                        sb.AppendLine();

                    if(ftr.MaxScrambleExtent > 0)
                        sb.AppendFormat("\tMaximum {0} scranble extent information entries", ftr.MaxScrambleExtent).AppendLine();


                    if(ftr.Current)
                        sb.AppendLine("Drive and currently inserted media support VCPS");
                    else
                        sb.AppendLine("Drive supports VCPS");

                    

                    if(ftr.Current)
                        sb.AppendLine("Drive and currently inserted media support SecurDisc");
                    else
                        sb.AppendLine("Drive supports SecurDisc");

                    

                    sb.AppendLine("Drive supports the Trusted Computing Group Optical Security Subsystem Class");

                    if(ftr.Current)
                        mmcOneValue.Add("Current media is initialized with TCG OSSC");
                    if(ftr.PSAU)
                        mmcOneValue.Add("Drive supports PSA updates on write-once media");
                    if(ftr.LOSPB)
                        mmcOneValue.Add("Drive supports linked OSPBs");
                    if(ftr.ME)
                        mmcOneValue.Add("Drive will only record on the OSSC Disc Format");

                    if(ftr.Profiles != null)
                    {
                        for(int i = 0; i < ftr.Profiles.Length; i++)
                            sb.AppendFormat("\tProfile {0}: {1}", i, ftr.Profiles[i]).AppendLine();
                    }
            }
        }
    }
}
