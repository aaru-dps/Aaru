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
using System.Collections.Generic;
using System.Text;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.Bluray;
using DiscImageChef.Decoders.DVD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using DiscImageChef.DiscImages;
using DiscImageChef.Metadata;
using Schemas;
using DDS = DiscImageChef.Decoders.DVD.DDS;
using DMI = DiscImageChef.Decoders.Xbox.DMI;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using Spare = DiscImageChef.Decoders.DVD.Spare;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implement dumping optical discs from MultiMedia devices
    /// </summary>
    static class Mmc
    {
        /// <summary>
        ///     Dumps an optical disc
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="outputPrefix">Prefix for output data files</param>
        /// <param name="outputPlugin">Plugin for output file</param>
        /// <param name="retryPasses">How many times to retry</param>
        /// <param name="force">Force to continue dump whenever possible</param>
        /// <param name="dumpRaw">Dump raw/long sectors</param>
        /// <param name="persistent">Store whatever data the drive returned on error</param>
        /// <param name="stopOnError">Stop dump on first error</param>
        /// <param name="resume">Information for dump resuming</param>
        /// <param name="dumpLog">Dump logger</param>
        /// <param name="encoding">Encoding to use when analyzing dump</param>
        /// <param name="dskType">Disc type as detected in MMC layer</param>
        /// <param name="dumpLeadIn">Try to read and dump as much Lead-in as possible</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="formatOptions">Formats to pass to output file plugin</param>
        /// <exception cref="NotImplementedException">If trying to dump GOD or WOD, or XGDs without a Kreon drive</exception>
        internal static void Dump(Device        dev, string devicePath, IWritableImage outputPlugin, ushort retryPasses,
                                  bool          force, bool dumpRaw, bool              persistent, bool     stopOnError,
                                  ref MediaType dskType,
                                  ref
                                      Resume resume, ref DumpLog dumpLog, bool dumpLeadIn,
                                  Encoding   encoding,
                                  string
                                      outputPrefix, string outputPath, Dictionary<string, string> formatOptions,
                                  CICMMetadataType
                                      preSidecar, uint skip, bool nometadata)
        {
            bool   sense;
            ulong  blocks;
            byte[] tmpBuf;
            bool   compactDisc = true;
            bool   isXbox      = false;

            // TODO: Log not only what is it reading, but if it was read correctly or not.

            sense = dev.GetConfiguration(out byte[] cmdBuf, out _, 0, MmcGetConfigurationRt.Current, dev.Timeout,
                                         out _);
            if(!sense)
            {
                Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);
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
                CompactDisc.Dump(dev, devicePath, outputPlugin, retryPasses, force, dumpRaw, persistent, stopOnError,
                                 ref dskType, ref resume, ref dumpLog, dumpLeadIn, encoding, outputPrefix, outputPath,
                                 formatOptions, preSidecar, skip, nometadata);
                return;
            }

            Reader scsiReader = new Reader(dev, dev.Timeout, null, dumpRaw);
            blocks            = scsiReader.GetDeviceBlocks();
            dumpLog.WriteLine("Device reports disc has {0} blocks", blocks);
            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>();

            #region Nintendo
            switch(dskType)
            {
                case MediaType.Unknown when blocks > 0:
                    dumpLog.WriteLine("Reading Physical Format Information");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        PFI.PhysicalFormatInformation? nintendoPfi = PFI.Decode(cmdBuf);
                        if(nintendoPfi                        != null)
                            if(nintendoPfi.Value.DiskCategory == DiskCategory.Nintendo &&
                               nintendoPfi.Value.PartVersion  == 15)
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
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);
                    if(!sense)
                        if(PFI.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length                - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVD_PFI, tmpBuf);

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
                                        dskType = decPfi.PartVersion == 6 ? MediaType.DVDRDL : MediaType.DVDR;
                                        break;
                                    case DiskCategory.DVDRAM:
                                        dskType = MediaType.DVDRAM;
                                        break;
                                    default:
                                        dskType = MediaType.DVDROM;
                                        break;
                                    case DiskCategory.DVDRW:
                                        dskType = decPfi.PartVersion == 3 ? MediaType.DVDRWDL : MediaType.DVDRW;
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
                                        dskType = decPfi.DiscSize == DVDSize.Eighty ? MediaType.GOD : MediaType.WOD;
                                        break;
                                    case DiskCategory.UMD:
                                        dskType = MediaType.UMD;
                                        break;
                                }
                        }

                    dumpLog.WriteLine("Reading Disc Manufacturing Information");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                                  out _);
                    if(!sense)
                    {
                        if(DMI.IsXbox(cmdBuf) || DMI.IsXbox360(cmdBuf))
                        {
                            if(DMI.IsXbox(cmdBuf)) dskType = MediaType.XGD;
                            else if(DMI.IsXbox360(cmdBuf))
                            {
                                dskType = MediaType.XGD2;

                                // All XGD3 all have the same number of blocks
                                if(blocks == 25063   || // Locked (or non compatible drive)
                                   blocks == 4229664 || // Xtreme unlock
                                   blocks == 4246304)   // Wxripper unlock
                                    dskType = MediaType.XGD3;
                            }

                            sense = dev.ScsiInquiry(out byte[] inqBuf, out _);

                            if(sense || !Inquiry.Decode(inqBuf).HasValue || Inquiry.Decode(inqBuf).HasValue &&
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

                        if(cmdBuf.Length == 2052)
                        {
                            tmpBuf = new byte[cmdBuf.Length                - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVD_DMI, tmpBuf);
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
                sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                              MmcDiscStructureFormat.CopyrightInformation, 0, dev.Timeout, out _);
                if(!sense)
                    if(CSS_CPRM.DecodeLeadInCopyright(cmdBuf).HasValue)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_CMI, tmpBuf);
                    }
            }
            #endregion DVD-ROM

            switch(dskType)
            {
                #region DVD-ROM and HD DVD-ROM
                case MediaType.DVDDownload:
                case MediaType.DVDROM:
                case MediaType.HDDVDROM:
                    dumpLog.WriteLine("Reading Burst Cutting Area.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.BurstCuttingArea, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_BCA, tmpBuf);
                    }

                    break;
                #endregion DVD-ROM and HD DVD-ROM

                #region DVD-RAM and HD DVD-RAM
                case MediaType.DVDRAM:
                case MediaType.HDDVDRAM:
                    dumpLog.WriteLine("Reading Disc Description Structure.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramDds, 0, dev.Timeout, out _);
                    if(!sense)
                        if(DDS.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length                - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVDRAM_DDS, tmpBuf);
                        }

                    dumpLog.WriteLine("Reading Spare Area Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdramSpareAreaInformation, 0, dev.Timeout,
                                                  out _);
                    if(!sense)
                        if(Spare.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length                - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.DVDRAM_SpareArea, tmpBuf);
                        }

                    break;
                #endregion DVD-RAM and HD DVD-RAM

                #region DVD-R and DVD-RW
                case MediaType.DVDR:
                case MediaType.DVDRW:
                    dumpLog.WriteLine("Reading Pre-Recorded Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PreRecordedInfo, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDR_PreRecordedInfo, tmpBuf);
                    }

                    break;
                #endregion DVD-R and DVD-RW
            }

            switch(dskType)
            {
                #region DVD-R, DVD-RW and HD DVD-R
                case MediaType.DVDR:
                case MediaType.DVDRW:
                case MediaType.HDDVDR:
                    dumpLog.WriteLine("Reading Media Identifier.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrMediaIdentifier, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDR_MediaIdentifier, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Recordable Physical Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DvdrPhysicalInformation, 0, dev.Timeout,
                                                  out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVDR_PFI, tmpBuf);
                    }

                    break;
                #endregion DVD-R, DVD-RW and HD DVD-R

                #region All DVD+
                case MediaType.DVDPR:
                case MediaType.DVDPRDL:
                case MediaType.DVDPRW:
                case MediaType.DVDPRWDL:
                    dumpLog.WriteLine("Reading ADdress In Pregroove.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.Adip, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DVD_ADIP, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Disc Control Blocks.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.Dcb, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.DCB, tmpBuf);
                    }

                    break;
                #endregion All DVD+

                #region HD DVD-ROM
                case MediaType.HDDVDROM:
                    dumpLog.WriteLine("Reading Lead-in Copyright Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.HddvdCopyrightInformation, 0, dev.Timeout,
                                                  out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.HDDVD_CPI, tmpBuf);
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
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out _);
                    if(!sense)
                        if(DI.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length                - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.BD_DI, tmpBuf);
                        }

                    // TODO: PAC
                    /*
                    dumpLog.WriteLine("Reading PAC.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.Pac, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.PAC, tmpBuf);
                    }*/
                    break;
                #endregion All Blu-ray
            }

            switch(dskType)
            {
                #region BD-ROM only
                case MediaType.BDROM:
                    dumpLog.WriteLine("Reading Burst Cutting Area.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdBurstCuttingArea, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.BD_BCA, tmpBuf);
                    }

                    break;
                #endregion BD-ROM only

                #region Writable Blu-ray only
                case MediaType.BDR:
                case MediaType.BDRE:
                case MediaType.BDRXL:
                case MediaType.BDREXL:
                    dumpLog.WriteLine("Reading Disc Definition Structure.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdDds, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.BD_DDS, tmpBuf);
                    }

                    dumpLog.WriteLine("Reading Spare Area Information.");
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.BdSpareAreaInformation, 0, dev.Timeout, out _);
                    if(!sense)
                    {
                        tmpBuf = new byte[cmdBuf.Length                - 4];
                        Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                        mediaTags.Add(MediaTagType.BD_SpareArea, tmpBuf);
                    }

                    break;
                #endregion Writable Blu-ray only
            }

            if(isXbox)
            {
                Xgd.Dump(dev, devicePath, outputPlugin, retryPasses, force, dumpRaw, persistent, stopOnError, mediaTags,
                         ref dskType, ref resume, ref dumpLog, encoding, outputPrefix, outputPath, formatOptions,
                         preSidecar, skip, nometadata);
                return;
            }

            Sbc.Dump(dev, devicePath, outputPlugin, retryPasses, force, dumpRaw, persistent, stopOnError, mediaTags,
                     ref dskType, true, ref resume, ref dumpLog, encoding, outputPrefix, outputPath, formatOptions,
                     preSidecar, skip, nometadata);
        }

        internal static void AddMediaTagToSidecar(string                             outputPath,
                                                  KeyValuePair<MediaTagType, byte[]> tag,
                                                  ref CICMMetadataType               sidecar)
        {
            switch(tag.Key)
            {
                case MediaTagType.DVD_PFI:
                    sidecar.OpticalDisc[0].PFI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVD_DMI:
                    sidecar.OpticalDisc[0].DMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVD_CMI:
                case MediaTagType.HDDVD_CPI:
                    sidecar.OpticalDisc[0].CMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };

                    CSS_CPRM.LeadInCopyright cpy = CSS_CPRM.DecodeLeadInCopyright(tag.Value).Value;
                    if(cpy.CopyrightType != CopyrightType.NoProtection)
                        sidecar.OpticalDisc[0].CopyProtection = cpy.CopyrightType.ToString();

                    break;
                case MediaTagType.DVD_BCA:
                case MediaTagType.BD_BCA:
                    sidecar.OpticalDisc[0].BCA = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.BD_DDS:
                case MediaTagType.DVDRAM_DDS:
                    sidecar.OpticalDisc[0].DDS = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVDRAM_SpareArea:
                case MediaTagType.BD_SpareArea:
                    sidecar.OpticalDisc[0].SAI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVDR_PreRecordedInfo:
                    sidecar.OpticalDisc[0].PRI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVD_MediaIdentifier:
                    sidecar.OpticalDisc[0].MediaID = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVDR_PFI:
                    sidecar.OpticalDisc[0].PFIR = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DVD_ADIP:
                    sidecar.OpticalDisc[0].ADIP = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.DCB:
                    sidecar.OpticalDisc[0].DCB = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.BD_DI:
                    sidecar.OpticalDisc[0].DI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.Xbox_SecuritySector:
                    if(sidecar.OpticalDisc[0].Xbox == null) sidecar.OpticalDisc[0].Xbox = new XboxType();

                    sidecar.OpticalDisc[0].Xbox.SecuritySectors = new[]
                    {
                        new XboxSecuritySectorsType
                        {
                            RequestNumber   = 0,
                            RequestVersion  = 1,
                            SecuritySectors = new DumpType
                            {
                                Image     = outputPath,
                                Size      = tag.Value.Length,
                                Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                            }
                        }
                    };

                    break;
                case MediaTagType.Xbox_PFI:
                    if(sidecar.OpticalDisc[0].Xbox == null) sidecar.OpticalDisc[0].Xbox = new XboxType();

                    sidecar.OpticalDisc[0].Xbox.PFI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.Xbox_DMI:
                    if(sidecar.OpticalDisc[0].Xbox == null) sidecar.OpticalDisc[0].Xbox = new XboxType();

                    sidecar.OpticalDisc[0].Xbox.DMI = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.CD_FullTOC:
                    sidecar.OpticalDisc[0].TOC = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.CD_ATIP:
                    sidecar.OpticalDisc[0].ATIP = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.CD_PMA:
                    sidecar.OpticalDisc[0].PMA = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.CD_TEXT:
                    sidecar.OpticalDisc[0].LeadInCdText = new DumpType
                    {
                        Image     = outputPath,
                        Size      = tag.Value.Length,
                        Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                    };
                    break;
                case MediaTagType.CD_LeadIn:
                    sidecar.OpticalDisc[0].LeadIn = new[]
                    {
                        new BorderType
                        {
                            Image     = outputPath,
                            Size      = tag.Value.Length,
                            Checksums = Checksum.GetChecksums(tag.Value).ToArray()
                        }
                    };
                    break;
            }
        }
    }
}