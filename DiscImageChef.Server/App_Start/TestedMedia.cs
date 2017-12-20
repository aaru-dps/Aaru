// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : TestedMedia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes media tests from reports.
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
using DiscImageChef.Metadata;

namespace DiscImageChef.Server.App_Start
{
    public static class TestedMedia
    {
        public static void Report(testedMediaType[] testedMedias, bool ata, ref List<string> mediaOneValue)
        {
            foreach(testedMediaType testedMedia in testedMedias)
            {
                if(!string.IsNullOrWhiteSpace(testedMedia.MediumTypeName))
                {
                    mediaOneValue.Add(string.Format("<i>Information for medium named \"{0}\"</i>",
                                                    testedMedia.MediumTypeName));
                    if(testedMedia.MediumTypeSpecified)
                        mediaOneValue.Add(string.Format("Medium type code: {0:X2}h", testedMedia.MediumType));
                }
                else if(testedMedia.MediumTypeSpecified)
                    mediaOneValue.Add(string.Format("<i>Information for medium type {0:X2}h</i>",
                                                    testedMedia.MediumType));
                else mediaOneValue.Add("<i>Information for unknown medium type</i>");

                if(testedMedia.MediaIsRecognized) mediaOneValue.Add("Drive recognizes this medium.");
                else mediaOneValue.Add("Drive does not recognize this medium.");

                if(!string.IsNullOrWhiteSpace(testedMedia.Manufacturer))
                    mediaOneValue.Add(string.Format("Medium manufactured by: {0}", testedMedia.Manufacturer));
                if(!string.IsNullOrWhiteSpace(testedMedia.Model))
                    mediaOneValue.Add(string.Format("Medium model: {0}", testedMedia.Model));
                if(testedMedia.DensitySpecified)
                    mediaOneValue.Add(string.Format("Density code: {0:X2}h", testedMedia.Density));

                if(testedMedia.BlockSizeSpecified)
                    mediaOneValue.Add(string.Format("Logical sector size: {0} bytes", testedMedia.BlockSize));
                if(testedMedia.PhysicalBlockSizeSpecified)
                    mediaOneValue.Add(string.Format("Physical sector size: {0} bytes", testedMedia.PhysicalBlockSize));
                if(testedMedia.LongBlockSizeSpecified)
                    mediaOneValue.Add(string.Format("READ LONG sector size: {0} bytes", testedMedia.LongBlockSize));

                if(testedMedia.BlocksSpecified && testedMedia.BlockSizeSpecified)
                {
                    mediaOneValue.Add(string.Format("Medium has {0} blocks of {1} bytes each", testedMedia.Blocks,
                                                    testedMedia.BlockSize));

                    if(testedMedia.Blocks * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                    {
                        mediaOneValue.Add(string.Format("Medium size: {0} bytes, {1} Tb, {2:F2} TiB",
                                                        testedMedia.Blocks * testedMedia.BlockSize,
                                                        testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000 /
                                                        1000 / 1000,
                                                        (double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 /
                                                        1024 / 1024 / 1024));
                    }
                    else if(testedMedia.Blocks * testedMedia.BlockSize / 1024 / 1024 > 1000)
                    {
                        mediaOneValue.Add(string.Format("Medium size: {0} bytes, {1} Gb, {2:F2} GiB",
                                                        testedMedia.Blocks * testedMedia.BlockSize,
                                                        testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000 /
                                                        1000,
                                                        (double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 /
                                                        1024 / 1024));
                    }
                    else
                    {
                        mediaOneValue.Add(string.Format("Medium size: {0} bytes, {1} Mb, {2:F2} MiB",
                                                        testedMedia.Blocks * testedMedia.BlockSize,
                                                        testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000,
                                                        (double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 /
                                                        1024));
                    }
                }

                if(testedMedia.CHS != null && testedMedia.CurrentCHS != null)
                {
                    int currentSectors = testedMedia.CurrentCHS.Cylinders * testedMedia.CurrentCHS.Heads *
                                         testedMedia.CurrentCHS.Sectors;
                    mediaOneValue.Add(string.Format("Cylinders: {0} max., {1} current", testedMedia.CHS.Cylinders,
                                                    testedMedia.CurrentCHS.Cylinders));
                    mediaOneValue.Add(string.Format("Heads: {0} max., {1} current", testedMedia.CHS.Heads,
                                                    testedMedia.CurrentCHS.Heads));
                    mediaOneValue.Add(string.Format("Sectors per track: {0} max., {1} current", testedMedia.CHS.Sectors,
                                                    testedMedia.CurrentCHS.Sectors));
                    mediaOneValue.Add(string.Format("Sectors addressable in CHS mode: {0} max., {1} current",
                                                    testedMedia.CHS.Cylinders * testedMedia.CHS.Heads *
                                                    testedMedia.CHS.Sectors, currentSectors));
                    mediaOneValue.Add(string.Format("Medium size in CHS mode: {0} bytes, {1} Mb, {2:F2} MiB",
                                                    (ulong)currentSectors * testedMedia.BlockSize,
                                                    (ulong)currentSectors * testedMedia.BlockSize / 1000 / 1000,
                                                    (double)((ulong)currentSectors * testedMedia.BlockSize) / 1024 /
                                                    1024));
                }
                else if(testedMedia.CHS != null)
                {
                    int currentSectors = testedMedia.CHS.Cylinders * testedMedia.CHS.Heads * testedMedia.CHS.Sectors;
                    mediaOneValue.Add(string.Format("Cylinders: {0}", testedMedia.CHS.Cylinders));
                    mediaOneValue.Add(string.Format("Heads: {0}", testedMedia.CHS.Heads));
                    mediaOneValue.Add(string.Format("Sectors per track: {0}", testedMedia.CHS.Sectors));
                    mediaOneValue.Add(string.Format("Sectors addressable in CHS mode: {0}", currentSectors));
                    mediaOneValue.Add(string.Format("Medium size in CHS mode: {0} bytes, {1} Mb, {2:F2} MiB",
                                                    (ulong)currentSectors * testedMedia.BlockSize,
                                                    (ulong)currentSectors * testedMedia.BlockSize / 1000 / 1000,
                                                    (double)((ulong)currentSectors * testedMedia.BlockSize) / 1024 /
                                                    1024));
                }

                if(testedMedia.LBASectorsSpecified)
                {
                    mediaOneValue.Add(string.Format("Sectors addressable in sectors in 28-bit LBA mode: {0}",
                                                    testedMedia.LBASectors));

                    if((ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                    {
                        mediaOneValue.Add(string.Format("Medium size in 28-bit LBA mode: {0} bytes, {1} Tb, {2:F2} TiB",
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize,
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 /
                                                        1000 / 1000 / 1000,
                                                        (double)((ulong)testedMedia.LBASectors *
                                                                 testedMedia.BlockSize) / 1024 / 1024 / 1024 / 1024));
                    }
                    else if((ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1024 / 1024 > 1000)
                    {
                        mediaOneValue.Add(string.Format("Medium size in 28-bit LBA mode: {0} bytes, {1} Gb, {2:F2} GiB",
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize,
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 /
                                                        1000 / 1000,
                                                        (double)((ulong)testedMedia.LBASectors *
                                                                 testedMedia.BlockSize) / 1024 / 1024 / 1024));
                    }
                    else
                    {
                        mediaOneValue.Add(string.Format("Medium size in 28-bit LBA mode: {0} bytes, {1} Mb, {2:F2} MiB",
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize,
                                                        (ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 /
                                                        1000,
                                                        (double)((ulong)testedMedia.LBASectors *
                                                                 testedMedia.BlockSize) / 1024 / 1024));
                    }
                }

                if(testedMedia.LBA48SectorsSpecified)
                {
                    mediaOneValue.Add(string.Format("Sectors addressable in sectors in 48-bit LBA mode: {0}",
                                                    testedMedia.LBA48Sectors));

                    if(testedMedia.LBA48Sectors * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                    {
                        mediaOneValue.Add(string.Format("Medium size in 48-bit LBA mode: {0} bytes, {1} Tb, {2:F2} TiB",
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize,
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 /
                                                        1000 / 1000 / 1000,
                                                        (double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) /
                                                        1024 / 1024 / 1024 / 1024));
                    }
                    else if(testedMedia.LBA48Sectors * testedMedia.BlockSize / 1024 / 1024 > 1000)
                    {
                        mediaOneValue.Add(string.Format("Medium size in 48-bit LBA mode: {0} bytes, {1} Gb, {2:F2} GiB",
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize,
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 /
                                                        1000 / 1000,
                                                        (double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) /
                                                        1024 / 1024 / 1024));
                    }
                    else
                    {
                        mediaOneValue.Add(string.Format("Medium size in 48-bit LBA mode: {0} bytes, {1} Mb, {2:F2} MiB",
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize,
                                                        testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 /
                                                        1000,
                                                        (double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) /
                                                        1024 / 1024));
                    }
                }

                if(testedMedia.NominalRotationRateSpecified && testedMedia.NominalRotationRate != 0x0000 &&
                   testedMedia.NominalRotationRate != 0xFFFF)
                {
                    if(testedMedia.NominalRotationRate == 0x0001) mediaOneValue.Add("Medium does not rotate.");
                    else mediaOneValue.Add(string.Format("Medium rotates at {0} rpm", testedMedia.NominalRotationRate));
                }

                if(testedMedia.BlockSizeSpecified && testedMedia.PhysicalBlockSizeSpecified &&
                   testedMedia.BlockSize != testedMedia.PhysicalBlockSize &&
                   (testedMedia.LogicalAlignment & 0x8000) == 0x0000 &&
                   (testedMedia.LogicalAlignment & 0x4000) == 0x4000)
                {
                    mediaOneValue.Add(string.Format("Logical sector starts at offset {0} from physical sector",
                                                    testedMedia.LogicalAlignment & 0x3FFF));
                }

                if(testedMedia.SupportsRead && ata)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in CHS mode with this medium");
                if(testedMedia.SupportsReadRetry)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) RETRY command in CHS mode with this medium");
                if(testedMedia.SupportsReadDma)
                    mediaOneValue.Add("Device can use the READ DMA command in CHS mode with this medium");
                if(testedMedia.SupportsReadDmaRetry)
                    mediaOneValue.Add("Device can use the READ DMA RETRY command in CHS mode with this medium");
                if(testedMedia.SupportsReadLong && ata)
                    mediaOneValue.Add("Device can use the READ LONG command in CHS mode with this medium");
                if(testedMedia.SupportsReadLongRetry)
                    mediaOneValue.Add("Device can use the READ LONG RETRY command in CHS mode with this medium");

                if(testedMedia.SupportsReadLba)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadRetryLba)
                    mediaOneValue
                        .Add("Device can use the READ SECTOR(S) RETRY command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaLba)
                    mediaOneValue.Add("Device can use the READ DMA command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaRetryLba)
                    mediaOneValue.Add("Device can use the READ DMA RETRY command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadLongLba)
                    mediaOneValue.Add("Device can use the READ LONG command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadLongRetryLba)
                    mediaOneValue.Add("Device can use the READ LONG RETRY command in 28-bit LBA mode with this medium");

                if(testedMedia.SupportsReadLba48)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in 48-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaLba48)
                    mediaOneValue.Add("Device can use the READ DMA command in 48-bit LBA mode with this medium");

                if(testedMedia.SupportsSeek)
                    mediaOneValue.Add("Device can use the SEEK command in CHS mode with this medium");
                if(testedMedia.SupportsSeekLba)
                    mediaOneValue.Add("Device can use the SEEK command in 28-bit LBA mode with this medium");

                if(testedMedia.SupportsReadCapacity)
                    mediaOneValue.Add("Device can use the READ CAPACITY (10) command with this medium");
                if(testedMedia.SupportsReadCapacity16)
                    mediaOneValue.Add("Device can use the READ CAPACITY (16) command with this medium");
                if(testedMedia.SupportsRead && !ata)
                    mediaOneValue.Add("Device can use the READ (6) command with this medium");
                if(testedMedia.SupportsRead10)
                    mediaOneValue.Add("Device can use the READ (10) command with this medium");
                if(testedMedia.SupportsRead12)
                    mediaOneValue.Add("Device can use the READ (12) command with this medium");
                if(testedMedia.SupportsRead16)
                    mediaOneValue.Add("Device can use the READ (16) command with this medium");
                if(testedMedia.SupportsReadLong && !ata)
                    mediaOneValue.Add("Device can use the READ LONG (10) command with this medium");
                if(testedMedia.SupportsReadLong16)
                    mediaOneValue.Add("Device can use the READ LONG (16) command with this medium");

                if(testedMedia.SupportsReadCd)
                    mediaOneValue.Add("Device can use the READ CD command with LBA addressing with this medium");
                if(testedMedia.SupportsReadCdMsf)
                    mediaOneValue.Add("Device can use the READ CD command with MM:SS:FF addressing with this medium");
                if(testedMedia.SupportsReadCdRaw)
                    mediaOneValue
                        .Add("Device can use the READ CD command with LBA addressing with this medium to read raw sector");
                if(testedMedia.SupportsReadCdMsfRaw)
                    mediaOneValue
                        .Add("Device can use the READ CD command with MM:SS:FF addressing with this medium read raw sector");

                if(testedMedia.SupportsHLDTSTReadRawDVD)
                    mediaOneValue.Add("Device can use the HL-DT-ST vendor READ DVD (RAW) command with this medium");
                if(testedMedia.SupportsNECReadCDDA)
                    mediaOneValue.Add("Device can use the NEC vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPioneerReadCDDA)
                    mediaOneValue.Add("Device can use the PIONEER vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPioneerReadCDDAMSF)
                    mediaOneValue.Add("Device can use the PIONEER vendor READ CD-DA MSF command with this medium");
                if(testedMedia.SupportsPlextorReadCDDA)
                    mediaOneValue.Add("Device can use the PLEXTOR vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPlextorReadRawDVD)
                    mediaOneValue.Add("Device can use the PLEXOR vendor READ DVD (RAW) command with this medium");

                if(testedMedia.CanReadAACS)
                    mediaOneValue.Add("Device can read the Advanced Access Content System from this medium");
                if(testedMedia.CanReadADIP)
                    mediaOneValue.Add("Device can read the DVD ADress-In-Pregroove from this medium");
                if(testedMedia.CanReadATIP)
                    mediaOneValue.Add("Device can read the CD Absolute-Time-In-Pregroove from this medium");
                if(testedMedia.CanReadBCA) mediaOneValue.Add("Device can read the Burst Cutting Area from this medium");
                if(testedMedia.CanReadC2Pointers)
                    mediaOneValue.Add("Device can report the C2 pointers when reading from this medium");
                if(testedMedia.CanReadCMI)
                    mediaOneValue.Add("Device can read the Copyright Management Information from this medium");
                if(testedMedia.CanReadCorrectedSubchannel)
                    mediaOneValue.Add("Device can correct subchannels when reading from this medium");
                if(testedMedia.CanReadCorrectedSubchannelWithC2)
                    mediaOneValue
                        .Add("Device can correct subchannels and report the C2 pointers when reading from this medium");
                if(testedMedia.CanReadDCB)
                    mediaOneValue.Add("Device can read the Disc Control Blocks from this medium");
                if(testedMedia.CanReadDDS)
                    mediaOneValue.Add("Device can read the Disc Definition Structure from this medium");
                if(testedMedia.CanReadDMI)
                    mediaOneValue.Add("Device can read the Disc Manufacurer Information from this medium");
                if(testedMedia.CanReadDiscInformation)
                    mediaOneValue.Add("Device can read the Disc Information from this medium");
                if(testedMedia.CanReadFullTOC)
                    mediaOneValue.Add("Device can read the Table of Contents from this medium, without processing it");
                if(testedMedia.CanReadHDCMI)
                    mediaOneValue.Add("Device can read the HD DVD Copyright Management Information from this medium");
                if(testedMedia.CanReadLayerCapacity)
                    mediaOneValue.Add("Device can read the layer capacity from this medium");
                if(testedMedia.CanReadLeadIn) mediaOneValue.Add("Device can read the Lead-In from this medium");
                if(testedMedia.CanReadLeadOut) mediaOneValue.Add("Device can read the Lead-Out from this medium");
                if(testedMedia.CanReadMediaID) mediaOneValue.Add("Device can read the Media ID from this medium");
                if(testedMedia.CanReadMediaSerial)
                    mediaOneValue.Add("Device can read the Media Serial Number from this medium");
                if(testedMedia.CanReadPAC) mediaOneValue.Add("Device can read the PAC from this medium");
                if(testedMedia.CanReadPFI)
                    mediaOneValue.Add("Device can read the Physical Format Information from this medium");
                if(testedMedia.CanReadPMA)
                    mediaOneValue.Add("Device can read the Power Management Area from this medium");
                if(testedMedia.CanReadPQSubchannel)
                    mediaOneValue.Add("Device can read the P to Q subchannels from this medium");
                if(testedMedia.CanReadPQSubchannelWithC2)
                    mediaOneValue
                        .Add("Device can read the P to Q subchannels from this medium reporting the C2 pointers");
                if(testedMedia.CanReadPRI)
                    mediaOneValue.Add("Device can read the Pre-Recorded Information from this medium");
                if(testedMedia.CanReadRWSubchannel)
                    mediaOneValue.Add("Device can read the R to W subchannels from this medium");
                if(testedMedia.CanReadRWSubchannelWithC2)
                    mediaOneValue
                        .Add("Device can read the R to W subchannels from this medium reporting the C2 pointers");
                if(testedMedia.CanReadRecordablePFI)
                    mediaOneValue.Add("Device can read the Physical Format Information from Lead-In from this medium");
                if(testedMedia.CanReadSpareAreaInformation)
                    mediaOneValue.Add("Device can read the Spare Area Information from this medium");
                if(testedMedia.CanReadTOC) mediaOneValue.Add("Device can read the Table of Contents from this medium");

                mediaOneValue.Add("");
            }
        }
    }
}