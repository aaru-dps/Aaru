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

namespace DiscImageChef.Server.App_Start
{
    public static class TestedMedia
    {
        /// <summary>
        ///     Takes the tested media from a device report and prints it as a list of values
        /// </summary>
        /// <param name="ata"><c>true</c> if device report is from an ATA device</param>
        /// <param name="mediaOneValue">List to put values on</param>
        /// <param name="testedMedias">List of tested media</param>
        public static void Report(List<CommonTypes.Metadata.TestedMedia> testedMedias, ref List<string> mediaOneValue)
        {
            foreach(CommonTypes.Metadata.TestedMedia testedMedia in testedMedias)
            {
                if(!string.IsNullOrWhiteSpace(testedMedia.MediumTypeName))
                {
                    mediaOneValue.Add($"<i>Information for medium named \"{testedMedia.MediumTypeName}\"</i>");
                    if(testedMedia.MediumType != null)
                        mediaOneValue.Add($"Medium type code: {testedMedia.MediumType:X2}h");
                }
                else if(testedMedia.MediumType != null)
                    mediaOneValue.Add($"<i>Information for medium type {testedMedia.MediumType:X2}h</i>");
                else mediaOneValue.Add("<i>Information for unknown medium type</i>");

                mediaOneValue.Add(testedMedia.MediaIsRecognized
                                      ? "Drive recognizes this medium."
                                      : "Drive does not recognize this medium.");

                if(!string.IsNullOrWhiteSpace(testedMedia.Manufacturer))
                    mediaOneValue.Add($"Medium manufactured by: {testedMedia.Manufacturer}");
                if(!string.IsNullOrWhiteSpace(testedMedia.Model))
                    mediaOneValue.Add($"Medium model: {testedMedia.Model}");
                if(testedMedia.Density != null) mediaOneValue.Add($"Density code: {testedMedia.Density:X2}h");

                if(testedMedia.BlockSize != null)
                    mediaOneValue.Add($"Logical sector size: {testedMedia.BlockSize} bytes");
                if(testedMedia.PhysicalBlockSize != null)
                    mediaOneValue.Add($"Physical sector size: {testedMedia.PhysicalBlockSize} bytes");
                if(testedMedia.LongBlockSize != null)
                    mediaOneValue.Add($"READ LONG sector size: {testedMedia.LongBlockSize} bytes");

                if(testedMedia.Blocks != null && testedMedia.BlockSize != null)
                {
                    mediaOneValue.Add($"Medium has {testedMedia.Blocks} blocks of {testedMedia.BlockSize} bytes each");

                    if(testedMedia.Blocks * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                        mediaOneValue
                           .Add($"Medium size: {testedMedia.Blocks * testedMedia.BlockSize} bytes, {testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000 / 1000 / 1000} Tb, {(double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                    else if(testedMedia.Blocks * testedMedia.BlockSize / 1024 / 1024 > 1000)
                        mediaOneValue
                           .Add($"Medium size: {testedMedia.Blocks * testedMedia.BlockSize} bytes, {testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000 / 1000} Gb, {(double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 / 1024 / 1024:F2} GiB");
                    else
                        mediaOneValue
                           .Add($"Medium size: {testedMedia.Blocks * testedMedia.BlockSize} bytes, {testedMedia.Blocks * testedMedia.BlockSize / 1000 / 1000} Mb, {(double)(testedMedia.Blocks * testedMedia.BlockSize) / 1024 / 1024:F2} MiB");
                }

                if(testedMedia.CHS != null && testedMedia.CurrentCHS != null)
                {
                    int currentSectors = testedMedia.CurrentCHS.Cylinders * testedMedia.CurrentCHS.Heads *
                                         testedMedia.CurrentCHS.Sectors;
                    mediaOneValue
                       .Add($"Cylinders: {testedMedia.CHS.Cylinders} max., {testedMedia.CurrentCHS.Cylinders} current");
                    mediaOneValue.Add($"Heads: {testedMedia.CHS.Heads} max., {testedMedia.CurrentCHS.Heads} current");
                    mediaOneValue
                       .Add($"Sectors per track: {testedMedia.CHS.Sectors} max., {testedMedia.CurrentCHS.Sectors} current");
                    mediaOneValue
                       .Add($"Sectors addressable in CHS mode: {testedMedia.CHS.Cylinders * testedMedia.CHS.Heads * testedMedia.CHS.Sectors} max., {currentSectors} current");
                    mediaOneValue
                       .Add($"Medium size in CHS mode: {(ulong)currentSectors * testedMedia.BlockSize} bytes, {(ulong)currentSectors * testedMedia.BlockSize / 1000 / 1000} Mb, {(double)((ulong)currentSectors * testedMedia.BlockSize) / 1024 / 1024:F2} MiB");
                }
                else if(testedMedia.CHS != null)
                {
                    int currentSectors = testedMedia.CHS.Cylinders * testedMedia.CHS.Heads * testedMedia.CHS.Sectors;
                    mediaOneValue.Add($"Cylinders: {testedMedia.CHS.Cylinders}");
                    mediaOneValue.Add($"Heads: {testedMedia.CHS.Heads}");
                    mediaOneValue.Add($"Sectors per track: {testedMedia.CHS.Sectors}");
                    mediaOneValue.Add($"Sectors addressable in CHS mode: {currentSectors}");
                    mediaOneValue
                       .Add($"Medium size in CHS mode: {(ulong)currentSectors * testedMedia.BlockSize} bytes, {(ulong)currentSectors * testedMedia.BlockSize / 1000 / 1000} Mb, {(double)((ulong)currentSectors * testedMedia.BlockSize) / 1024 / 1024:F2} MiB");
                }

                if(testedMedia.LBASectors != null)
                {
                    mediaOneValue.Add($"Sectors addressable in sectors in 28-bit LBA mode: {testedMedia.LBASectors}");

                    if((ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                        mediaOneValue
                           .Add($"Medium size in 28-bit LBA mode: {(ulong)testedMedia.LBASectors * testedMedia.BlockSize} bytes, {(ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 / 1000 / 1000 / 1000} Tb, {(double)((ulong)testedMedia.LBASectors * testedMedia.BlockSize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                    else if((ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1024 / 1024 > 1000)
                        mediaOneValue
                           .Add($"Medium size in 28-bit LBA mode: {(ulong)testedMedia.LBASectors * testedMedia.BlockSize} bytes, {(ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 / 1000 / 1000} Gb, {(double)((ulong)testedMedia.LBASectors * testedMedia.BlockSize) / 1024 / 1024 / 1024:F2} GiB");
                    else
                        mediaOneValue
                           .Add($"Medium size in 28-bit LBA mode: {(ulong)testedMedia.LBASectors * testedMedia.BlockSize} bytes, {(ulong)testedMedia.LBASectors * testedMedia.BlockSize / 1000 / 1000} Mb, {(double)((ulong)testedMedia.LBASectors * testedMedia.BlockSize) / 1024 / 1024:F2} MiB");
                }

                if(testedMedia.LBA48Sectors != null)
                {
                    mediaOneValue.Add($"Sectors addressable in sectors in 48-bit LBA mode: {testedMedia.LBA48Sectors}");

                    if(testedMedia.LBA48Sectors * testedMedia.BlockSize / 1024 / 1024 > 1000000)
                        mediaOneValue
                           .Add($"Medium size in 48-bit LBA mode: {testedMedia.LBA48Sectors * testedMedia.BlockSize} bytes, {testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 / 1000 / 1000 / 1000} Tb, {(double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                    else if(testedMedia.LBA48Sectors * testedMedia.BlockSize / 1024 / 1024 > 1000)
                        mediaOneValue
                           .Add($"Medium size in 48-bit LBA mode: {testedMedia.LBA48Sectors * testedMedia.BlockSize} bytes, {testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 / 1000 / 1000} Gb, {(double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) / 1024 / 1024 / 1024:F2} GiB");
                    else
                        mediaOneValue
                           .Add($"Medium size in 48-bit LBA mode: {testedMedia.LBA48Sectors * testedMedia.BlockSize} bytes, {testedMedia.LBA48Sectors * testedMedia.BlockSize / 1000 / 1000} Mb, {(double)(testedMedia.LBA48Sectors * testedMedia.BlockSize) / 1024 / 1024:F2} MiB");
                }

                if(testedMedia.NominalRotationRate != null && testedMedia.NominalRotationRate != 0x0000 &&
                   testedMedia.NominalRotationRate != 0xFFFF)
                    mediaOneValue.Add(testedMedia.NominalRotationRate == 0x0001
                                          ? "Medium does not rotate."
                                          : $"Medium rotates at {testedMedia.NominalRotationRate} rpm");

                if(testedMedia.BlockSize                   != null                                &&
                   testedMedia.PhysicalBlockSize           != null                                &&
                   testedMedia.BlockSize.Value             != testedMedia.PhysicalBlockSize.Value &&
                   (testedMedia.LogicalAlignment & 0x8000) == 0x0000                              &&
                   (testedMedia.LogicalAlignment & 0x4000) == 0x4000)
                    mediaOneValue
                       .Add($"Logical sector starts at offset {testedMedia.LogicalAlignment & 0x3FFF} from physical sector");

                if(testedMedia.SupportsReadSectors == true)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in CHS mode with this medium");
                if(testedMedia.SupportsReadRetry == true)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) RETRY command in CHS mode with this medium");
                if(testedMedia.SupportsReadDma == true)
                    mediaOneValue.Add("Device can use the READ DMA command in CHS mode with this medium");
                if(testedMedia.SupportsReadDmaRetry == true)
                    mediaOneValue.Add("Device can use the READ DMA RETRY command in CHS mode with this medium");
                if(testedMedia.SupportsReadLong == true)
                    mediaOneValue.Add("Device can use the READ LONG command in CHS mode with this medium");
                if(testedMedia.SupportsReadLongRetry == true)
                    mediaOneValue.Add("Device can use the READ LONG RETRY command in CHS mode with this medium");

                if(testedMedia.SupportsReadLba == true)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadRetryLba == true)
                    mediaOneValue
                       .Add("Device can use the READ SECTOR(S) RETRY command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaLba == true)
                    mediaOneValue.Add("Device can use the READ DMA command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaRetryLba == true)
                    mediaOneValue.Add("Device can use the READ DMA RETRY command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadLongLba == true)
                    mediaOneValue.Add("Device can use the READ LONG command in 28-bit LBA mode with this medium");
                if(testedMedia.SupportsReadLongRetryLba == true)
                    mediaOneValue.Add("Device can use the READ LONG RETRY command in 28-bit LBA mode with this medium");

                if(testedMedia.SupportsReadLba48 == true)
                    mediaOneValue.Add("Device can use the READ SECTOR(S) command in 48-bit LBA mode with this medium");
                if(testedMedia.SupportsReadDmaLba48 == true)
                    mediaOneValue.Add("Device can use the READ DMA command in 48-bit LBA mode with this medium");

                if(testedMedia.SupportsSeek == true)
                    mediaOneValue.Add("Device can use the SEEK command in CHS mode with this medium");
                if(testedMedia.SupportsSeekLba == true)
                    mediaOneValue.Add("Device can use the SEEK command in 28-bit LBA mode with this medium");

                if(testedMedia.SupportsReadCapacity == true)
                    mediaOneValue.Add("Device can use the READ CAPACITY (10) command with this medium");
                if(testedMedia.SupportsReadCapacity16 == true)
                    mediaOneValue.Add("Device can use the READ CAPACITY (16) command with this medium");
                if(testedMedia.SupportsRead6 == true)
                    mediaOneValue.Add("Device can use the READ (6) command with this medium");
                if(testedMedia.SupportsRead10 == true)
                    mediaOneValue.Add("Device can use the READ (10) command with this medium");
                if(testedMedia.SupportsRead12 == true)
                    mediaOneValue.Add("Device can use the READ (12) command with this medium");
                if(testedMedia.SupportsRead16 == true)
                    mediaOneValue.Add("Device can use the READ (16) command with this medium");
                if(testedMedia.SupportsReadLong == true)
                    mediaOneValue.Add("Device can use the READ LONG (10) command with this medium");
                if(testedMedia.SupportsReadLong16 == true)
                    mediaOneValue.Add("Device can use the READ LONG (16) command with this medium");

                if(testedMedia.SupportsReadCd == true)
                    mediaOneValue.Add("Device can use the READ CD command with LBA addressing with this medium");
                if(testedMedia.SupportsReadCdMsf == true)
                    mediaOneValue.Add("Device can use the READ CD command with MM:SS:FF addressing with this medium");
                if(testedMedia.SupportsReadCdRaw == true)
                    mediaOneValue
                       .Add("Device can use the READ CD command with LBA addressing with this medium to read raw sector");
                if(testedMedia.SupportsReadCdMsfRaw == true)
                    mediaOneValue
                       .Add("Device can use the READ CD command with MM:SS:FF addressing with this medium read raw sector");

                if(testedMedia.SupportsHLDTSTReadRawDVD == true)
                    mediaOneValue.Add("Device can use the HL-DT-ST vendor READ DVD (RAW) command with this medium");
                if(testedMedia.SupportsNECReadCDDA == true)
                    mediaOneValue.Add("Device can use the NEC vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPioneerReadCDDA == true)
                    mediaOneValue.Add("Device can use the PIONEER vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPioneerReadCDDAMSF == true)
                    mediaOneValue.Add("Device can use the PIONEER vendor READ CD-DA MSF command with this medium");
                if(testedMedia.SupportsPlextorReadCDDA == true)
                    mediaOneValue.Add("Device can use the PLEXTOR vendor READ CD-DA command with this medium");
                if(testedMedia.SupportsPlextorReadRawDVD == true)
                    mediaOneValue.Add("Device can use the PLEXOR vendor READ DVD (RAW) command with this medium");

                if(testedMedia.CanReadAACS == true)
                    mediaOneValue.Add("Device can read the Advanced Access Content System from this medium");
                if(testedMedia.CanReadADIP == true)
                    mediaOneValue.Add("Device can read the DVD ADress-In-Pregroove from this medium");
                if(testedMedia.CanReadATIP == true)
                    mediaOneValue.Add("Device can read the CD Absolute-Time-In-Pregroove from this medium");
                if(testedMedia.CanReadBCA == true)
                    mediaOneValue.Add("Device can read the Burst Cutting Area from this medium");
                if(testedMedia.CanReadC2Pointers == true)
                    mediaOneValue.Add("Device can report the C2 pointers when reading from this medium");
                if(testedMedia.CanReadCMI == true)
                    mediaOneValue.Add("Device can read the Copyright Management Information from this medium");
                if(testedMedia.CanReadCorrectedSubchannel == true)
                    mediaOneValue.Add("Device can correct subchannels when reading from this medium");
                if(testedMedia.CanReadCorrectedSubchannelWithC2 == true)
                    mediaOneValue
                       .Add("Device can correct subchannels and report the C2 pointers when reading from this medium");
                if(testedMedia.CanReadDCB == true)
                    mediaOneValue.Add("Device can read the Disc Control Blocks from this medium");
                if(testedMedia.CanReadDDS == true)
                    mediaOneValue.Add("Device can read the Disc Definition Structure from this medium");
                if(testedMedia.CanReadDMI == true)
                    mediaOneValue.Add("Device can read the Disc Manufacurer Information from this medium");
                if(testedMedia.CanReadDiscInformation == true)
                    mediaOneValue.Add("Device can read the Disc Information from this medium");
                if(testedMedia.CanReadFullTOC == true)
                    mediaOneValue.Add("Device can read the Table of Contents from this medium, without processing it");
                if(testedMedia.CanReadHDCMI == true)
                    mediaOneValue.Add("Device can read the HD DVD Copyright Management Information from this medium");
                if(testedMedia.CanReadLayerCapacity == true)
                    mediaOneValue.Add("Device can read the layer capacity from this medium");
                if(testedMedia.CanReadFirstTrackPreGap == true)
                    mediaOneValue.Add("Device can read the first track's pregap data");
                if(testedMedia.CanReadLeadIn == true) mediaOneValue.Add("Device can read the Lead-In from this medium");
                if(testedMedia.CanReadLeadOut == true)
                    mediaOneValue.Add("Device can read the Lead-Out from this medium");
                if(testedMedia.CanReadMediaID == true)
                    mediaOneValue.Add("Device can read the Media ID from this medium");
                if(testedMedia.CanReadMediaSerial == true)
                    mediaOneValue.Add("Device can read the Media Serial Number from this medium");
                if(testedMedia.CanReadPAC == true) mediaOneValue.Add("Device can read the PAC from this medium");
                if(testedMedia.CanReadPFI == true)
                    mediaOneValue.Add("Device can read the Physical Format Information from this medium");
                if(testedMedia.CanReadPMA == true)
                    mediaOneValue.Add("Device can read the Power Management Area from this medium");
                if(testedMedia.CanReadPQSubchannel == true)
                    mediaOneValue.Add("Device can read the P to Q subchannels from this medium");
                if(testedMedia.CanReadPQSubchannelWithC2 == true)
                    mediaOneValue
                       .Add("Device can read the P to Q subchannels from this medium reporting the C2 pointers");
                if(testedMedia.CanReadPRI == true)
                    mediaOneValue.Add("Device can read the Pre-Recorded Information from this medium");
                if(testedMedia.CanReadRWSubchannel == true)
                    mediaOneValue.Add("Device can read the R to W subchannels from this medium");
                if(testedMedia.CanReadRWSubchannelWithC2 == true)
                    mediaOneValue
                       .Add("Device can read the R to W subchannels from this medium reporting the C2 pointers");
                if(testedMedia.CanReadRecordablePFI == true)
                    mediaOneValue.Add("Device can read the Physical Format Information from Lead-In from this medium");
                if(testedMedia.CanReadSpareAreaInformation == true)
                    mediaOneValue.Add("Device can read the Spare Area Information from this medium");
                if(testedMedia.CanReadTOC == true)
                    mediaOneValue.Add("Device can read the Table of Contents from this medium");

                mediaOneValue.Add("");
            }
        }
    }
}