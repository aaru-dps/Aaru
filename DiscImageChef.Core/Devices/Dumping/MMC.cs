// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps media from SCSI MultiMedia devices.
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
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;
using Schemas;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Core.Devices.Dumping
{
    static class Mmc
    {
        internal static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force,
                                  bool dumpRaw, bool persistent, bool stopOnError, ref CICMMetadataType sidecar,
                                  ref MediaType dskType, bool separateSubchannel, ref Resume resume,
                                  ref DumpLog dumpLog, bool dumpLeadIn, Encoding encoding)
        {
            byte[] cmdBuf = null;
            byte[] senseBuf = null;
            bool sense = false;
            double duration;
            ulong blocks = 0;
            byte[] tmpBuf;
            bool compactDisc = true;
            ushort currentProfile = 0x0001;
            bool isXbox = false;
            Alcohol120 alcohol = new Alcohol120(outputPrefix);

            sidecar.OpticalDisc = new OpticalDiscType[1];
            sidecar.OpticalDisc[0] = new OpticalDiscType();

            // TODO: Log not only what is it reading, but if it was read correctly or not.

            sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout,
                                         out duration);
            if(!sense)
            {
                Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);
                currentProfile = ftr.CurrentProfile;
                dumpLog.WriteLine("Device reports current profile is 0x{0:X4}", ftr.CurrentProfile);

                switch(ftr.CurrentProfile)
                {
                    case 0x0001:
                        dskType = MediaType.GENERIC_HDD;
                        goto default;
                    case 0x0005:
                        dskType = MediaType.CDMO;
                        break;
                    case 0x0008:
                        dskType = MediaType.CD;
                        break;
                    case 0x0009:
                        dskType = MediaType.CDR;
                        break;
                    case 0x000A:
                        dskType = MediaType.CDRW;
                        break;
                    case 0x0010:
                        dskType = MediaType.DVDROM;
                        goto default;
                    case 0x0011:
                        dskType = MediaType.DVDR;
                        goto default;
                    case 0x0012:
                        dskType = MediaType.DVDRAM;
                        goto default;
                    case 0x0013:
                    case 0x0014:
                        dskType = MediaType.DVDRW;
                        goto default;
                    case 0x0015:
                    case 0x0016:
                        dskType = MediaType.DVDRDL;
                        goto default;
                    case 0x0017:
                        dskType = MediaType.DVDRWDL;
                        goto default;
                    case 0x0018:
                        dskType = MediaType.DVDDownload;
                        goto default;
                    case 0x001A:
                        dskType = MediaType.DVDPRW;
                        goto default;
                    case 0x001B:
                        dskType = MediaType.DVDPR;
                        goto default;
                    case 0x0020:
                        dskType = MediaType.DDCD;
                        goto default;
                    case 0x0021:
                        dskType = MediaType.DDCDR;
                        goto default;
                    case 0x0022:
                        dskType = MediaType.DDCDRW;
                        goto default;
                    case 0x002A:
                        dskType = MediaType.DVDPRWDL;
                        goto default;
                    case 0x002B:
                        dskType = MediaType.DVDPRDL;
                        goto default;
                    case 0x0040:
                        dskType = MediaType.BDROM;
                        goto default;
                    case 0x0041:
                    case 0x0042:
                        dskType = MediaType.BDR;
                        goto default;
                    case 0x0043:
                        dskType = MediaType.BDRE;
                        goto default;
                    case 0x0050:
                        dskType = MediaType.HDDVDROM;
                        goto default;
                    case 0x0051:
                        dskType = MediaType.HDDVDR;
                        goto default;
                    case 0x0052:
                        dskType = MediaType.HDDVDRAM;
                        goto default;
                    case 0x0053:
                        dskType = MediaType.HDDVDRW;
                        goto default;
                    case 0x0058:
                        dskType = MediaType.HDDVDRDL;
                        goto default;
                    case 0x005A:
                        dskType = MediaType.HDDVDRWDL;
                        goto default;
                    default:
                        compactDisc = false;
                        break;
                }
            }

            if(compactDisc)
            {
                CompactDisc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError,
                                 ref sidecar, ref dskType, separateSubchannel, ref resume, ref dumpLog, alcohol,
                                 dumpLeadIn);
                return;
            }

            Reader scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
            blocks = scsiReader.GetDeviceBlocks();
            dumpLog.WriteLine("Device reports disc has {0} blocks", blocks);

            #region Nintendo
            switch(dskType) {
                case MediaType.Unknown when blocks > 0:
                    dumpLog.WriteLine("Reading Physical Format Information");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf);
                        if(nintendoPfi != null)
                            if(nintendoPfi.Value.DiskCategory == DiskCategory.Nintendo &&
                               nintendoPfi.Value.PartVersion == 15)
                            {
                                dumpLog.WriteLine("Dumping Nintendo GameCube or Wii discs is not yet implemented.");
                                throw new
                                    NotImplementedException("Dumping Nintendo GameCube or Wii discs is not yet implemented.");
                            }
                    }

                    break;
                case MediaType.DVDDownload:
                case MediaType.DVDPR:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                case MediaType.DVDR:
                case MediaType.DVDRAM:
                case MediaType.DVDRDL:
                case MediaType.DVDROM:
                case MediaType.DVDRW:
                case MediaType.DVDRWDL:
                case MediaType.HDDVDR:
                case MediaType.HDDVDRAM:
                case MediaType.HDDVDRDL:
                case MediaType.HDDVDROM:
                case MediaType.HDDVDRW:
                case MediaType.HDDVDRWDL:
                    dumpLog.WriteLine("Reading Physical Format Information");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        alcohol.AddPfi(cmdBuf);
                        if(PFI.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].PFI = new DumpType
                            {
                                Image = outputPrefix + ".pfi.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFI.Image, tmpBuf);

                            PFI.PhysicalFormatInformation decPfi = PFI.Decode(cmdBuf).Value;
                            DicConsole.WriteLine("PFI:\n{0}", PFI.Prettify(decPfi));

                            // False book types
                            if(dskType == MediaType.DVDROM)
                                switch(decPfi.DiskCategory)
                                {
                                    case DiskCategory.DVDPR:
                                        dskType = MediaType.DVDPR;
                                        break;
                                    case DiskCategory.DVDPRDL:
                                        dskType = MediaType.DVDPRDL;
                                        break;
                                    case DiskCategory.DVDPRW:
                                        dskType = MediaType.DVDPRW;
                                        break;
                                    case DiskCategory.DVDPRWDL:
                                        dskType = MediaType.DVDPRWDL;
                                        break;
                                    case DiskCategory.DVDR:
                                        if(decPfi.PartVersion == 6) dskType = MediaType.DVDRDL;
                                        else dskType = MediaType.DVDR;
                                        break;
                                    case DiskCategory.DVDRAM:
                                        dskType = MediaType.DVDRAM;
                                        break;
                                    default:
                                        dskType = MediaType.DVDROM;
                                        break;
                                    case DiskCategory.DVDRW:
                                        if(decPfi.PartVersion == 3) dskType = MediaType.DVDRWDL;
                                        else dskType = MediaType.DVDRW;
                                        break;
                                    case DiskCategory.HDDVDR:
                                        dskType = MediaType.HDDVDR;
                                        break;
                                    case DiskCategory.HDDVDRAM:
                                        dskType = MediaType.HDDVDRAM;
                                        break;
                                    case DiskCategory.HDDVDROM:
                                        dskType = MediaType.HDDVDROM;
                                        break;
                                    case DiskCategory.HDDVDRW:
                                        dskType = MediaType.HDDVDRW;
                                        break;
                                    case DiskCategory.Nintendo:
                                        if(decPfi.DiscSize == DVDSize.Eighty) dskType = MediaType.GOD;
                                        else dskType = MediaType.WOD;
                                        break;
                                    case DiskCategory.UMD:
                                        dskType = MediaType.UMD;
                                        break;
                                }
                        }
                    }

                    dumpLog.WriteLine("Reading Disc Manufacturing Information");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                    {
                        if(DMI.IsXbox(cmdBuf) || DMI.IsXbox360(cmdBuf))
                        {
                            if(DMI.IsXbox(cmdBuf)) dskType = MediaType.XGD;
                            else if(DMI.IsXbox360(cmdBuf))
                            {
                                dskType = MediaType.XGD2;

                                // All XGD3 all have the same number of blocks
                                if(blocks == 25063 || // Locked (or non compatible drive)
                                   blocks == 4229664 || // Xtreme unlock
                                   blocks == 4246304) // Wxripper unlock
                                    dskType = MediaType.XGD3;
                            }

                            sense = dev.ScsiInquiry(out byte[] inqBuf, out senseBuf);

                            if(sense || !Inquiry.Decode(inqBuf).HasValue ||
                               Inquiry.Decode(inqBuf).HasValue &&
                               !Inquiry.Decode(inqBuf).Value.KreonPresent)
                            {
                                dumpLog.WriteLine("Dumping Xbox Game Discs requires a drive with Kreon firmware.");
                                throw new
                                    NotImplementedException("Dumping Xbox Game Discs requires a drive with Kreon firmware.");
                            }

                            if(dumpRaw && !force)
                            {
                                DicConsole
                                    .ErrorWriteLine("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                                // TODO: Exit more gracefully
                                return;
                            }

                            isXbox = true;
                        }

                        alcohol.AddDmi(cmdBuf);

                        if(cmdBuf.Length == 2052)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].DMI = new DumpType
                            {
                                Image = outputPrefix + ".dmi.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DMI.Image, tmpBuf);
                        }
                    }

                    break;
            }
            #endregion Nintendo

            #region All DVD and HD DVD types
            #endregion All DVD and HD DVD types

            #region DVD-ROM
            if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM)
            {
                dumpLog.WriteLine("Reading Lead-in Copyright Information.");
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout,
                                              out duration);
                if(!sense)
                    if(CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].CMI = new DumpType
                        {
                            Image = outputPrefix + ".cmi.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);

                        CSS_CPRM.LeadInCopyright cpy =
                            CSS_CPRM.DecodeLeadInCopyright(cmdBuf).Value;
                        if(cpy.CopyrightType != CopyrightType.NoProtection)
                            sidecar.OpticalDisc[0].CopyProtection = cpy.CopyrightType.ToString();
                    }
            }
            #endregion DVD-ROM

            switch(dskType) {
                #region DVD-ROM and HD DVD-ROM
                case MediaType.DVDDownload:
                case MediaType.DVDROM:
                case MediaType.HDDVDROM:
                    dumpLog.WriteLine("Reading Burst Cutting Area.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        alcohol.AddBca(tmpBuf);
                        sidecar.OpticalDisc[0].BCA = new DumpType
                        {
                            Image = outputPrefix + ".bca.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                    }
                    break;
                #endregion DVD-ROM and HD DVD-ROM
                #region DVD-RAM and HD DVD-RAM
                case MediaType.DVDRAM:
                case MediaType.HDDVDRAM:
                    dumpLog.WriteLine("Reading Disc Description Structure.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramDds, 0, dev.Timeout, out duration);
                    if(!sense)
                        if(DDS.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].DDS = new DumpType
                            {
                                Image = outputPrefix + ".dds.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                        }

                    dumpLog.WriteLine("Reading Spare Area Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                        if(Spare.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].SAI = new DumpType
                            {
                                Image = outputPrefix + ".sai.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                        }
                    break;
                #endregion DVD-RAM and HD DVD-RAM
                #region DVD-R and DVD-RW
                case MediaType.DVDR:
                case MediaType.DVDRW:
                    dumpLog.WriteLine("Reading Pre-Recorded Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].PRI = new DumpType
                        {
                            Image = outputPrefix + ".pri.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                    }
                    break;
                #endregion DVD-R and DVD-RW
            }

            switch(dskType) {
                #region DVD-R, DVD-RW and HD DVD-R
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.HDDVDR:
                    dumpLog.WriteLine("Reading Media Identifier.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrMediaIdentifier, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].MediaID = new DumpType
                        {
                            Image = outputPrefix + ".mid.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].MediaID.Image, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Recordable Physical Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrPhysicalInformation, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].PFIR = new DumpType
                        {
                            Image = outputPrefix + ".pfir.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFIR.Image, tmpBuf);
                    }
                    break;
                #endregion DVD-R, DVD-RW and HD DVD-R
                #region All DVD+
                case MediaType.DVDPR:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                    dumpLog.WriteLine("Reading ADdress In Pregroove.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.Adip, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].ADIP = new DumpType
                        {
                            Image = outputPrefix + ".adip.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].ADIP.Image, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Disc Control Blocks.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.Dcb, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].DCB = new DumpType
                        {
                            Image = outputPrefix + ".dcb.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DCB.Image, tmpBuf);
                    }
                    break;
                #endregion All DVD+
                #region HD DVD-ROM
                case MediaType.HDDVDROM:
                    dumpLog.WriteLine("Reading Lead-in Copyright Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdCopyrightInformation, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].CMI = new DumpType
                        {
                            Image = outputPrefix + ".cmi.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);
                    }
                    break;
                #endregion HD DVD-ROM
                #region All Blu-ray
                case MediaType.BDR:
                case MediaType.BDRE:
                case MediaType.BDROM:
                case MediaType.BDRXL:
                case MediaType.BDREXL:
                    dumpLog.WriteLine("Reading Disc Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out duration);
                    if(!sense)
                        if(DI.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].DI = new DumpType
                            {
                                Image = outputPrefix + ".di.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DI.Image, tmpBuf);
                        }

                    dumpLog.WriteLine("Reading PAC.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.Pac, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].PAC = new DumpType
                        {
                            Image = outputPrefix + ".pac.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PAC.Image, tmpBuf);
                    }
                    break;
                #endregion All Blu-ray
            }

            switch(dskType) {
                #region BD-ROM only
                case MediaType.BDROM:
                    dumpLog.WriteLine("Reading Burst Cutting Area.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdBurstCuttingArea, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        alcohol.AddBca(tmpBuf);
                        sidecar.OpticalDisc[0].BCA = new DumpType
                        {
                            Image = outputPrefix + ".bca.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                    }
                    break;
                #endregion BD-ROM only
                #region Writable Blu-ray only
                case MediaType.BDR:
                case MediaType.BDRE:
                case MediaType.BDRXL:
                case MediaType.BDREXL:
                    dumpLog.WriteLine("Reading Disc Definition Structure.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdDds, 0, dev.Timeout, out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].DDS = new DumpType
                        {
                            Image = outputPrefix + ".dds.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Spare Area Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdSpareAreaInformation, 0, dev.Timeout,
                                                  out duration);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].SAI = new DumpType
                        {
                            Image = outputPrefix + ".sai.bin",
                            Size = tmpBuf.Length,
                            Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                        };
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                    }
                    break;
                #endregion Writable Blu-ray only
            }

            if(isXbox)
            {
                Xgd.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError,
                         ref sidecar, ref dskType, ref resume, ref dumpLog, encoding);
                return;
            }

            Sbc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar,
                     ref dskType, true, ref resume, ref dumpLog, encoding, alcohol);
        }
    }
}