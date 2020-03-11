// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceDto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     DTO for syncing processed device reports in database.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using Aaru.CommonTypes.Metadata;

// ReSharper disable VirtualMemberCallInConstructor

namespace Aaru.Dto
{
    public class DeviceDto : DeviceReportV2
    {
        public DeviceDto() {}

        public DeviceDto(DeviceReportV2 report)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATAPI;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            MultiMediaCard = report.MultiMediaCard;
            PCMCIA         = report.PCMCIA;
            SCSI           = report.SCSI;
            SecureDigital  = report.SecureDigital;
            USB            = report.USB;
            Manufacturer   = report.Manufacturer;
            Model          = report.Model;
            Revision       = report.Revision;
            Type           = report.Type;
        }

        public DeviceDto(DeviceReportV2 report, int id, int optimalMultipleSectorsRead)
        {
            ATA            = report.ATA;
            ATAPI          = report.ATAPI;
            CompactFlash   = report.CompactFlash;
            FireWire       = report.FireWire;
            MultiMediaCard = report.MultiMediaCard;
            PCMCIA         = report.PCMCIA;
            SCSI           = report.SCSI;
            SecureDigital  = report.SecureDigital;
            USB            = report.USB;
            Manufacturer   = report.Manufacturer;
            Model          = report.Model;
            Revision       = report.Revision;
            Type           = report.Type;

            if(ATA != null)
            {
                ATA.Identify         = null;
                ATA.ReadCapabilities = ClearBinaries(ATA.ReadCapabilities);

                if(ATA.RemovableMedias != null)
                {
                    TestedMedia[] medias = ATA.RemovableMedias.ToArray();
                    ATA.RemovableMedias = new List<TestedMedia>();

                    foreach(TestedMedia media in medias)
                        ATA.RemovableMedias.Add(ClearBinaries(media));
                }
            }

            if(ATAPI != null)
            {
                ATAPI.Identify         = null;
                ATAPI.ReadCapabilities = ClearBinaries(ATAPI.ReadCapabilities);

                if(ATAPI.RemovableMedias != null)
                {
                    TestedMedia[] medias = ATAPI.RemovableMedias.ToArray();
                    ATAPI.RemovableMedias = new List<TestedMedia>();

                    foreach(TestedMedia media in medias)
                        ATAPI.RemovableMedias.Add(ClearBinaries(media));
                }
            }

            if(PCMCIA != null)
            {
                PCMCIA.AdditionalInformation = null;
                PCMCIA.CIS                   = null;
            }

            MultiMediaCard = null;
            SecureDigital  = null;

            if(SCSI != null)
            {
                SCSI.EVPDPages                 = null;
                SCSI.InquiryData               = null;
                SCSI.ModeSense6Data            = null;
                SCSI.ModeSense10Data           = null;
                SCSI.ModeSense6CurrentData     = null;
                SCSI.ModeSense10CurrentData    = null;
                SCSI.ModeSense6ChangeableData  = null;
                SCSI.ModeSense10ChangeableData = null;
                SCSI.ReadCapabilities          = ClearBinaries(SCSI.ReadCapabilities);

                if(SCSI.ModeSense != null)
                {
                    SCSI.ModeSense.BlockDescriptors = null;
                    SCSI.ModeSense.ModePages        = null;
                }

                if(SCSI.RemovableMedias != null)
                {
                    TestedMedia[] medias = SCSI.RemovableMedias.ToArray();
                    SCSI.RemovableMedias = new List<TestedMedia>();

                    foreach(TestedMedia media in medias)
                        SCSI.RemovableMedias.Add(ClearBinaries(media));
                }

                if(SCSI.MultiMediaDevice != null)
                {
                    SCSI.MultiMediaDevice.ModeSense2AData = null;

                    if(SCSI.MultiMediaDevice.Features != null)
                        SCSI.MultiMediaDevice.Features.BinaryData = null;

                    if(SCSI.MultiMediaDevice.TestedMedia != null)
                    {
                        TestedMedia[] medias = SCSI.MultiMediaDevice.TestedMedia.ToArray();
                        SCSI.MultiMediaDevice.TestedMedia = new List<TestedMedia>();

                        foreach(TestedMedia media in medias)
                            SCSI.MultiMediaDevice.TestedMedia.Add(ClearBinaries(media));
                    }
                }

                SCSI.SequentialDevice = null;
            }

            if(USB != null)
                USB.Descriptors = null;

            Id                         = id;
            OptimalMultipleSectorsRead = optimalMultipleSectorsRead;
        }

        public int OptimalMultipleSectorsRead { get; set; }

        public new int Id { get; set; }

        static TestedMedia ClearBinaries(TestedMedia media)
        {
            if(media is null)
                return null;

            media.AdipData                      = null;
            media.AtipData                      = null;
            media.BluBcaData                    = null;
            media.BluDdsData                    = null;
            media.BluDiData                     = null;
            media.BluPacData                    = null;
            media.BluSaiData                    = null;
            media.C2PointersData                = null;
            media.CmiData                       = null;
            media.CorrectedSubchannelData       = null;
            media.CorrectedSubchannelWithC2Data = null;
            media.DcbData                       = null;
            media.DmiData                       = null;
            media.DvdAacsData                   = null;
            media.DvdBcaData                    = null;
            media.DvdDdsData                    = null;
            media.DvdLayerData                  = null;
            media.DvdSaiData                    = null;
            media.EmbossedPfiData               = null;
            media.FullTocData                   = null;
            media.HdCmiData                     = null;
            media.HLDTSTReadRawDVDData          = null;
            media.IdentifyData                  = null;
            media.LeadInData                    = null;
            media.LeadOutData                   = null;
            media.ModeSense6Data                = null;
            media.ModeSense10Data               = null;
            media.NecReadCddaData               = null;
            media.PfiData                       = null;
            media.PioneerReadCddaData           = null;
            media.PioneerReadCddaMsfData        = null;
            media.PlextorReadCddaData           = null;
            media.PlextorReadRawDVDData         = null;
            media.PmaData                       = null;
            media.PQSubchannelData              = null;
            media.PQSubchannelWithC2Data        = null;
            media.PriData                       = null;
            media.Read6Data                     = null;
            media.Read10Data                    = null;
            media.Read12Data                    = null;
            media.Read16Data                    = null;
            media.ReadCdData                    = null;
            media.ReadCdFullData                = null;
            media.ReadCdMsfData                 = null;
            media.ReadCdMsfFullData             = null;
            media.ReadDmaData                   = null;
            media.ReadDmaLba48Data              = null;
            media.ReadDmaLbaData                = null;
            media.ReadDmaRetryData              = null;
            media.ReadLba48Data                 = null;
            media.ReadLbaData                   = null;
            media.ReadLong10Data                = null;
            media.ReadLong16Data                = null;
            media.ReadLongData                  = null;
            media.ReadLongLbaData               = null;
            media.ReadLongRetryData             = null;
            media.ReadLongRetryLbaData          = null;
            media.ReadRetryLbaData              = null;
            media.ReadSectorsData               = null;
            media.ReadSectorsRetryData          = null;
            media.RWSubchannelData              = null;
            media.RWSubchannelWithC2Data        = null;
            media.TocData                       = null;
            media.Track1PregapData              = null;

            return media;
        }
    }
}