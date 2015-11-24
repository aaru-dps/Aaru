// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaInfo.cs
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
using DiscImageChef.Console;
using System.IO;
using DiscImageChef.Devices;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Commands
{
    public static class MediaInfo
    {
        public static void doMediaInfo(MediaInfoSubOptions options)
        {
            DicConsole.DebugWriteLine("Device-Info command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Info command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Info command", "--device={0}", options.DevicePath);
            DicConsole.DebugWriteLine("Device-Info command", "--output-prefix={0}", options.OutputPrefix);

            if (options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if (dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            switch (dev.Type)
            {
                case DeviceType.ATA:
                    doATAMediaInfo(options.OutputPrefix, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    doSDMediaInfo(options.OutputPrefix, dev);
                    break;
                case DeviceType.NVMe:
                    doNVMeMediaInfo(options.OutputPrefix, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    doSCSIMediaInfo(options.OutputPrefix, dev);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }
        }

        static void doATAMediaInfo(string outputPrefix, Device dev)
        {
            throw new NotImplementedException("ATA devices not yet supported.");
        }

        static void doNVMeMediaInfo(string outputPrefix, Device dev)
        {
            throw new NotImplementedException("NVMe devices not yet supported.");
        }

        static void doSDMediaInfo(string outputPrefix, Device dev)
        {
            throw new NotImplementedException("MMC/SD devices not yet supported.");
        }

        static void doSCSIMediaInfo(string outputPrefix, Device dev)
        {
            byte[] cmdBuf;
            byte[] senseBuf;
            bool sense;
            double duration;
            DiskType dskType = DiskType.Unknown;

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.DirectAccess ||
               dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice ||
               dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.OCRWDevice ||
               dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.OpticalDevice ||
               dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SimplifiedDevice ||
               dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.WriteOnceDevice)
            {
                ulong blocks = 0;
                uint blockSize = 0;

                sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (!sense)
                {
                    doWriteFile(outputPrefix, "_readcapacity.bin", "SCSI READ CAPACITY", cmdBuf);
                    blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + (cmdBuf[3]));
                    blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
                }

                if (sense || blocks == 0xFFFFFFFF)
                {
                    sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, dev.Timeout, out duration);

                    if (sense && blocks == 0)
                    {
                        // Not all MMC devices support READ CAPACITY, as they have READ TOC
                        if (dev.SCSIType != DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                        {
                            DicConsole.ErrorWriteLine("Unable to get media capacity");
                            DicConsole.ErrorWriteLine("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        }
                    }

                    if (!sense)
                    {
                        doWriteFile(outputPrefix, "_readcapacity16.bin", "SCSI READ CAPACITY(16)", cmdBuf);
                        byte[] temp = new byte[8];

                        Array.Copy(cmdBuf, 0, temp, 0, 8);
                        Array.Reverse(temp);
                        blocks = BitConverter.ToUInt64(temp, 0);
                        blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
                    }
                }

                if (blocks != 0 && blockSize != 0)
                {
                    blocks++;
                    DicConsole.WriteLine("Device has {0} blocks of {1} bytes/each. (for a total of {2} bytes)",
                        blocks, blockSize, blocks * (ulong)blockSize);
                }
            }

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                throw new NotImplementedException("SCSI Streaming Devices not yet implemented");

            if (dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
            {
                sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ GET CONFIGURATION:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_getconfiguration_current.bin", "SCSI GET CONFIGURATION", cmdBuf);

                    Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(cmdBuf);

                    DicConsole.DebugWriteLine("Media-Info command", "GET CONFIGURATION current profile is {0:X4}h", ftr.CurrentProfile);

                    switch (ftr.CurrentProfile)
                    {
                        case 0x0001:
                            dskType = DiskType.GENERIC_HDD;
                            break;
                        case 0x0005:
                            dskType = DiskType.CDMO;
                            break;
                        case 0x0008:
                            dskType = DiskType.CDROM;
                            break;
                        case 0x0009:
                            dskType = DiskType.CDR;
                            break;
                        case 0x000A:
                            dskType = DiskType.CDRW;
                            break;
                        case 0x0010:
                            dskType = DiskType.DVDROM;
                            break;
                        case 0x0011:
                            dskType = DiskType.DVDR;
                            break;
                        case 0x0012:
                            dskType = DiskType.DVDRAM;
                            break;
                        case 0x0013:
                        case 0x0014:
                            dskType = DiskType.DVDRW;
                            break;
                        case 0x0015:
                        case 0x0016:
                            dskType = DiskType.DVDRDL;
                            break;
                        case 0x0017:
                            dskType = DiskType.DVDRWDL;
                            break;
                        case 0x0018:
                            dskType = DiskType.DVDDownload;
                            break;
                        case 0x001A:
                            dskType = DiskType.DVDPRW;
                            break;
                        case 0x001B:
                            dskType = DiskType.DVDPR;
                            break;
                        case 0x0020:
                            dskType = DiskType.DDCD;
                            break;
                        case 0x0021:
                            dskType = DiskType.DDCDR;
                            break;
                        case 0x0022:
                            dskType = DiskType.DDCDRW;
                            break;
                        case 0x002A:
                            dskType = DiskType.DVDPRWDL;
                            break;
                        case 0x002B:
                            dskType = DiskType.DVDPRDL;
                            break;
                        case 0x0040:
                            dskType = DiskType.BDROM;
                            break;
                        case 0x0041:
                        case 0x0042:
                            dskType = DiskType.BDR;
                            break;
                        case 0x0043:
                            dskType = DiskType.BDRE;
                            break;
                        case 0x0050:
                            dskType = DiskType.HDDVDROM;
                            break;
                        case 0x0051:
                            dskType = DiskType.HDDVDR;
                            break;
                        case 0x0052:
                            dskType = DiskType.HDDVDRAM;
                            break;
                        case 0x0053:
                            dskType = DiskType.HDDVDRW;
                            break;
                        case 0x0058:
                            dskType = DiskType.HDDVDRDL;
                            break;
                        case 0x005A:
                            dskType = DiskType.HDDVDRWDL;
                            break;
                    }
                }

                DicConsole.WriteLine("{0}", dskType);

                sense = dev.ReadTocPmaAtip(out cmdBuf, out senseBuf, false, 0, 0, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: TOC\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    DicConsole.WriteLine("TOC:\n{0}", Decoders.CD.TOC.Prettify(cmdBuf));
                    doWriteFile(outputPrefix, "_toc.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                }

                sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.DiscInformation, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ DISC INFORMATION 000b\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_readdiscinformation_000b.bin", "SCSI READ DISC INFORMATION", cmdBuf);
                }

                sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.TrackResources, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC INFORMATION 001b\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscinformation_001b.bin", "SCSI READ DISC INFORMATION", cmdBuf);

                sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.POWResources, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC INFORMATION 010b\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscinformation_010b.bin", "SCSI READ DISC INFORMATION", cmdBuf);

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSVolId, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS Volume ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacsvolid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaSerial, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS Media Serial Number\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacssn.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMediaId, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacsmediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKB, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacsmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSLBAExtents, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS LBA Extents\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacslbaextents.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSMKBCPRM, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS CPRM MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacscprmmkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.RecognizedFormatLayers, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Recognized Format Layers\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_formatlayers.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.WriteProtectionStatus, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Write Protection Status\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_writeprotection.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CapabilityList, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Capability List\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_capabilitylist.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.AACSDataKeys, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: AACS Data Keys\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_aacsdatakeys.bin", "SCSI READ DISC STRUCTURE", cmdBuf);





                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: PFI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_pfi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: CMI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_cmi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscKey, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Disc Key\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_disckey.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: BCA\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_bca.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DMI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_dmi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.SectorCopyrightInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Sector CMI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_sectorcmi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaIdentifier, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_mediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MediaKeyBlock, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: MKB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_mkb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_DDS, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DDS\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdram_dds.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_MediumStatus, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Medium Status\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdram_status.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_SpareAreaInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: SAI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdram_spare.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_RecordingType, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Recording Type\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdram_type.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.LastBorderOutRMD, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Last-Out Border RMD\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_lastrmd.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.SpecifiedRMD, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Specified RMD\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_rmd.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Pre-Recorded Info\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_pri.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_MediaIdentifier, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DVD-R Media ID\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdr_mediaid.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_PhysicalInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DVD-R PFI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdr_pfi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.ADIP, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: ADIP\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_adip.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVD_CopyrightInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: HDDVD CMI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_hddvd_cmi.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVD_AACS, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DVD AACS\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_aacs.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVDR_MediumStatus, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: HDDVD-R Medium Status\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_hddvdr_status.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVDR_LastRMD, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Last RMD\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_hddvdr_lastrmd.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_LayerCapacity, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Layer Capacity\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvdr_layercap.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.MiddleZoneStart, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Middle Zone Start\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_mzs.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.JumpIntervalSize, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Jump Interval Size\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_jis.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.ManualLayerJumpStartLBA, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Manual Layer Jump Start LBA\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_manuallj.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.RemapAnchorPoint, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Remap Anchor Point\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_remapanchor.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DCB, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DCB\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_dvd_dcb.bin", "SCSI READ DISC STRUCTURE", cmdBuf);




                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DI\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_di.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_BurstCuttingArea, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: BCA\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_bca.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_DDS, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: DDS\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_dds.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.CartridgeStatus, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Cartridge Status\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_cartstatus.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_SpareAreaInformation, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Spare Area Information\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_spare.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.RawDFL, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: Raw DFL\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_dfl.bin", "SCSI READ DISC STRUCTURE", cmdBuf);
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.PAC, 0, dev.Timeout, out duration);
                if(sense)
                    DicConsole.ErrorWriteLine("READ DISC STRUCTURE: PAC\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                    doWriteFile(outputPrefix, "_readdiscstructure_bd_pac.bin", "SCSI READ DISC STRUCTURE", cmdBuf);


                sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: Session info\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_session.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                    DicConsole.WriteLine("Session information:\n{0}", Decoders.CD.Session.Prettify(cmdBuf));
                }
                sense = dev.ReadRawToc(out cmdBuf, out senseBuf, 1, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: Raw TOC\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_rawtoc.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                    DicConsole.WriteLine("Raw TOC:\n{0}", Decoders.CD.FullTOC.Prettify(cmdBuf));
                }
                sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: PMA\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_pma.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                    DicConsole.WriteLine("PMA:\n{0}", Decoders.CD.PMA.Prettify(cmdBuf));
                }
                sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: ATIP\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_atip.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                    DicConsole.WriteLine("ATIP:\n{0}", Decoders.CD.ATIP.Prettify(cmdBuf));
                }
                
                sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                if (sense)
                    DicConsole.ErrorWriteLine("READ TOC/PMA/ATIP: CD-TEXT\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                else
                {
                    doWriteFile(outputPrefix, "_cdtext.bin", "SCSI READ TOC/PMA/ATIP", cmdBuf);
                    DicConsole.WriteLine("CD-TEXT on Lead-In:\n{0}", Decoders.CD.CDTextOnLeadIn.Prettify(cmdBuf));
                }

            }
        }

        static void doWriteFile(string outputPrefix, string outputSuffix, string whatWriting, byte[] data)
        {
            if(!string.IsNullOrEmpty(outputPrefix))
            {
                if (!File.Exists(outputPrefix + outputSuffix))
                {
                    try
                    {
                        DicConsole.DebugWriteLine("Device-Info command", "Writing " + whatWriting + " to {0}{1}", outputPrefix, outputSuffix);
                        FileStream outputFs = new FileStream(outputPrefix + outputSuffix, FileMode.CreateNew);
                        outputFs.Write(data, 0, data.Length);
                        outputFs.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Unable to write file {0}{1}", outputPrefix, outputSuffix);
                    }
                }
                else
                        DicConsole.ErrorWriteLine("Not overwriting file {0}{1}", outputPrefix, outputSuffix);
            }
        }
    }
}

