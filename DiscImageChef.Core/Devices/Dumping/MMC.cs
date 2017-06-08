// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
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
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using Schemas;

namespace DiscImageChef.Core.Devices.Dumping
{
    internal static class MMC
    {
        internal static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError, ref CICMMetadataType sidecar, ref MediaType dskType, bool separateSubchannel, ref Metadata.Resume resume)
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

            sidecar.OpticalDisc = new OpticalDiscType[1];
            sidecar.OpticalDisc[0] = new OpticalDiscType();

            sense = dev.GetConfiguration(out cmdBuf, out senseBuf, 0, MmcGetConfigurationRt.Current, dev.Timeout, out duration);
            if(!sense)
            {
                Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(cmdBuf);
                currentProfile = ftr.CurrentProfile;

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
                CompactDisc.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar, ref dskType, separateSubchannel, ref resume);
                return;
            }

            Reader scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
            blocks = scsiReader.GetDeviceBlocks();

            #region Nintendo
            if(dskType == MediaType.Unknown && blocks > 0)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    Decoders.DVD.PFI.PhysicalFormatInformation? nintendoPfi = Decoders.DVD.PFI.Decode(cmdBuf);
                    if(nintendoPfi != null)
                    {
                        if(nintendoPfi.Value.DiskCategory == Decoders.DVD.DiskCategory.Nintendo &&
                            nintendoPfi.Value.PartVersion == 15)
                        {
                            throw new NotImplementedException("Dumping Nintendo GameCube or Wii discs is not yet implemented.");
                        }
                    }
                }
            }
            #endregion Nintendo

            #region All DVD and HD DVD types
            if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDPR ||
                dskType == MediaType.DVDPRDL || dskType == MediaType.DVDPRW ||
                dskType == MediaType.DVDPRWDL || dskType == MediaType.DVDR ||
                dskType == MediaType.DVDRAM || dskType == MediaType.DVDRDL ||
                dskType == MediaType.DVDROM || dskType == MediaType.DVDRW ||
                dskType == MediaType.DVDRWDL || dskType == MediaType.HDDVDR ||
                dskType == MediaType.HDDVDRAM || dskType == MediaType.HDDVDRDL ||
                dskType == MediaType.HDDVDROM || dskType == MediaType.HDDVDRW ||
                dskType == MediaType.HDDVDRWDL)
            {

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.DVD.PFI.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].PFI = new DumpType();
                        sidecar.OpticalDisc[0].PFI.Image = outputPrefix + ".pfi.bin";
                        sidecar.OpticalDisc[0].PFI.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].PFI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFI.Image, tmpBuf);

                        Decoders.DVD.PFI.PhysicalFormatInformation decPfi = Decoders.DVD.PFI.Decode(cmdBuf).Value;
                        DicConsole.WriteLine("PFI:\n{0}", Decoders.DVD.PFI.Prettify(decPfi));

                        // False book types
                        if(dskType == MediaType.DVDROM)
                        {
                            switch(decPfi.DiskCategory)
                            {
                                case Decoders.DVD.DiskCategory.DVDPR:
                                    dskType = MediaType.DVDPR;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDPRDL:
                                    dskType = MediaType.DVDPRDL;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDPRW:
                                    dskType = MediaType.DVDPRW;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDPRWDL:
                                    dskType = MediaType.DVDPRWDL;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDR:
                                    if(decPfi.PartVersion == 6)
                                        dskType = MediaType.DVDRDL;
                                    else
                                        dskType = MediaType.DVDR;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDRAM:
                                    dskType = MediaType.DVDRAM;
                                    break;
                                default:
                                    dskType = MediaType.DVDROM;
                                    break;
                                case Decoders.DVD.DiskCategory.DVDRW:
                                    if(decPfi.PartVersion == 3)
                                        dskType = MediaType.DVDRWDL;
                                    else
                                        dskType = MediaType.DVDRW;
                                    break;
                                case Decoders.DVD.DiskCategory.HDDVDR:
                                    dskType = MediaType.HDDVDR;
                                    break;
                                case Decoders.DVD.DiskCategory.HDDVDRAM:
                                    dskType = MediaType.HDDVDRAM;
                                    break;
                                case Decoders.DVD.DiskCategory.HDDVDROM:
                                    dskType = MediaType.HDDVDROM;
                                    break;
                                case Decoders.DVD.DiskCategory.HDDVDRW:
                                    dskType = MediaType.HDDVDRW;
                                    break;
                                case Decoders.DVD.DiskCategory.Nintendo:
                                    if(decPfi.DiscSize == Decoders.DVD.DVDSize.Eighty)
                                        dskType = MediaType.GOD;
                                    else
                                        dskType = MediaType.WOD;
                                    break;
                                case Decoders.DVD.DiskCategory.UMD:
                                    dskType = MediaType.UMD;
                                    break;
                            }
                        }
                    }
                }
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.Xbox.DMI.IsXbox(cmdBuf) || Decoders.Xbox.DMI.IsXbox360(cmdBuf))
                    {
                        if(Decoders.Xbox.DMI.IsXbox(cmdBuf))
                            dskType = MediaType.XGD;
                        else if(Decoders.Xbox.DMI.IsXbox360(cmdBuf))
                        {
                            dskType = MediaType.XGD2;

                            // All XGD3 all have the same number of blocks
                            if(blocks == 25063 || // Locked (or non compatible drive)
                               blocks == 4229664 || // Xtreme unlock
                               blocks == 4246304) // Wxripper unlock
                                dskType = MediaType.XGD3;
                        }

                        byte[] inqBuf;
                        sense = dev.ScsiInquiry(out inqBuf, out senseBuf);

                        if(sense || !Decoders.SCSI.Inquiry.Decode(inqBuf).HasValue ||
                           (Decoders.SCSI.Inquiry.Decode(inqBuf).HasValue && !Decoders.SCSI.Inquiry.Decode(inqBuf).Value.KreonPresent))
                            throw new NotImplementedException("Dumping Xbox Game Discs requires a drive with Kreon firmware.");

                        if(dumpRaw && !force)
                        {
                            DicConsole.ErrorWriteLine("Not continuing. If you want to continue reading cooked data when raw is not available use the force option.");
                            // TODO: Exit more gracefully
                            return;
                        }

                        isXbox = true;
                    }

                    if(cmdBuf.Length == 2052)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].DMI = new DumpType();
                        sidecar.OpticalDisc[0].DMI.Image = outputPrefix + ".dmi.bin";
                        sidecar.OpticalDisc[0].DMI.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].DMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DMI.Image, tmpBuf);
                    }
                }
            }
            #endregion All DVD and HD DVD types

            #region DVD-ROM
            if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].CMI = new DumpType();
                        sidecar.OpticalDisc[0].CMI.Image = outputPrefix + ".cmi.bin";
                        sidecar.OpticalDisc[0].CMI.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].CMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);

                        Decoders.DVD.CSS_CPRM.LeadInCopyright cpy = Decoders.DVD.CSS_CPRM.DecodeLeadInCopyright(cmdBuf).Value;
                        if(cpy.CopyrightType != Decoders.DVD.CopyrightType.NoProtection)
                            sidecar.OpticalDisc[0].CopyProtection = cpy.CopyrightType.ToString();
                    }
                }
            }
            #endregion DVD-ROM

            #region DVD-ROM and HD DVD-ROM
            if(dskType == MediaType.DVDDownload || dskType == MediaType.DVDROM ||
                dskType == MediaType.HDDVDROM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].BCA = new DumpType();
                    sidecar.OpticalDisc[0].BCA.Image = outputPrefix + ".bca.bin";
                    sidecar.OpticalDisc[0].BCA.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                }
            }
            #endregion DVD-ROM and HD DVD-ROM

            #region DVD-RAM and HD DVD-RAM
            if(dskType == MediaType.DVDRAM || dskType == MediaType.HDDVDRAM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_DDS, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.DVD.DDS.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].DDS = new DumpType();
                        sidecar.OpticalDisc[0].DDS.Image = outputPrefix + ".dds.bin";
                        sidecar.OpticalDisc[0].DDS.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].DDS.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                    }
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_SpareAreaInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.DVD.Spare.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].SAI = new DumpType();
                        sidecar.OpticalDisc[0].SAI.Image = outputPrefix + ".sai.bin";
                        sidecar.OpticalDisc[0].SAI.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].SAI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                    }
                }
            }
            #endregion DVD-RAM and HD DVD-RAM

            #region DVD-R and DVD-RW
            if(dskType == MediaType.DVDR || dskType == MediaType.DVDRW)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].PRI = new DumpType();
                    sidecar.OpticalDisc[0].PRI.Image = outputPrefix + ".pri.bin";
                    sidecar.OpticalDisc[0].PRI.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].PRI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                }
            }
            #endregion DVD-R and DVD-RW

            #region DVD-R, DVD-RW and HD DVD-R
            if(dskType == MediaType.DVDR || dskType == MediaType.DVDRW || dskType == MediaType.HDDVDR)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_MediaIdentifier, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].MediaID = new DumpType();
                    sidecar.OpticalDisc[0].MediaID.Image = outputPrefix + ".mid.bin";
                    sidecar.OpticalDisc[0].MediaID.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].MediaID.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].MediaID.Image, tmpBuf);
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_PhysicalInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].PFIR = new DumpType();
                    sidecar.OpticalDisc[0].PFIR.Image = outputPrefix + ".pfir.bin";
                    sidecar.OpticalDisc[0].PFIR.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].PFIR.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PFIR.Image, tmpBuf);
                }
            }
            #endregion DVD-R, DVD-RW and HD DVD-R

            #region All DVD+
            if(dskType == MediaType.DVDPR || dskType == MediaType.DVDPRDL ||
                dskType == MediaType.DVDPRW || dskType == MediaType.DVDPRWDL)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.ADIP, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].ADIP = new DumpType();
                    sidecar.OpticalDisc[0].ADIP.Image = outputPrefix + ".adip.bin";
                    sidecar.OpticalDisc[0].ADIP.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].ADIP.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].ADIP.Image, tmpBuf);
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DCB, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].DCB = new DumpType();
                    sidecar.OpticalDisc[0].DCB.Image = outputPrefix + ".dcb.bin";
                    sidecar.OpticalDisc[0].DCB.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].DCB.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DCB.Image, tmpBuf);
                }
            }
            #endregion All DVD+

            #region HD DVD-ROM
            if(dskType == MediaType.HDDVDROM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVD_CopyrightInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].CMI = new DumpType();
                    sidecar.OpticalDisc[0].CMI.Image = outputPrefix + ".cmi.bin";
                    sidecar.OpticalDisc[0].CMI.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].CMI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].CMI.Image, tmpBuf);
                }
            }
            #endregion HD DVD-ROM

            #region All Blu-ray
            if(dskType == MediaType.BDR || dskType == MediaType.BDRE || dskType == MediaType.BDROM ||
                dskType == MediaType.BDRXL || dskType == MediaType.BDREXL)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    if(Decoders.Bluray.DI.Decode(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        sidecar.OpticalDisc[0].DI = new DumpType();
                        sidecar.OpticalDisc[0].DI.Image = outputPrefix + ".di.bin";
                        sidecar.OpticalDisc[0].DI.Size = tmpBuf.Length;
                        sidecar.OpticalDisc[0].DI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                        DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DI.Image, tmpBuf);
                    }
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.PAC, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].PAC = new DumpType();
                    sidecar.OpticalDisc[0].PAC.Image = outputPrefix + ".pac.bin";
                    sidecar.OpticalDisc[0].PAC.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].PAC.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PAC.Image, tmpBuf);
                }
            }
            #endregion All Blu-ray


            #region BD-ROM only
            if(dskType == MediaType.BDROM)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_BurstCuttingArea, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].BCA = new DumpType();
                    sidecar.OpticalDisc[0].BCA.Image = outputPrefix + ".bca.bin";
                    sidecar.OpticalDisc[0].BCA.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].BCA.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].BCA.Image, tmpBuf);
                }
            }
            #endregion BD-ROM only

            #region Writable Blu-ray only
            if(dskType == MediaType.BDR || dskType == MediaType.BDRE ||
                dskType == MediaType.BDRXL || dskType == MediaType.BDREXL)
            {
                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_DDS, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].DDS = new DumpType();
                    sidecar.OpticalDisc[0].DDS.Image = outputPrefix + ".dds.bin";
                    sidecar.OpticalDisc[0].DDS.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].DDS.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].DDS.Image, tmpBuf);
                }

                sense = dev.ReadDiscStructure(out cmdBuf, out senseBuf, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_SpareAreaInformation, 0, dev.Timeout, out duration);
                if(!sense)
                {
                    tmpBuf = new byte[cmdBuf.Length - 4];
                    Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                    sidecar.OpticalDisc[0].SAI = new DumpType();
                    sidecar.OpticalDisc[0].SAI.Image = outputPrefix + ".sai.bin";
                    sidecar.OpticalDisc[0].SAI.Size = tmpBuf.Length;
                    sidecar.OpticalDisc[0].SAI.Checksums = Checksum.GetChecksums(tmpBuf).ToArray();
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].SAI.Image, tmpBuf);
                }
            }
            #endregion Writable Blu-ray only

            if(isXbox)
            {
                XGD.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar, ref dskType, ref resume);
                return;
            }

            SBC.Dump(dev, devicePath, outputPrefix, retryPasses, force, dumpRaw, persistent, stopOnError, ref sidecar, ref dskType, true, ref resume);
        }
    }
}
