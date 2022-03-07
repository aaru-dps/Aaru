// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for Aaru Format disk images.
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

namespace Aaru.DiscImages;

using System;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.SecureDigital;
using Aaru.Helpers;

public sealed partial class AaruFormat
{
    /// <summary>Checks for media tags that may contain metadata and sets it up if not already set</summary>
    void SetMetadataFromTags()
    {
        // Search for SecureDigital CID
        if(_mediaTags.TryGetValue(MediaTagType.SD_CID, out byte[] sdCid))
        {
            CID decoded = Decoders.DecodeCID(sdCid);

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                _imageInfo.DriveManufacturer = VendorString.Prettify(decoded.Manufacturer);

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                _imageInfo.DriveModel = decoded.ProductName;

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                _imageInfo.DriveFirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                _imageInfo.DriveSerialNumber = $"{decoded.ProductSerialNumber}";
        }

        // Search for MultiMediaCard CID
        if(_mediaTags.TryGetValue(MediaTagType.MMC_CID, out byte[] mmcCid))
        {
            Aaru.Decoders.MMC.CID decoded = Aaru.Decoders.MMC.Decoders.DecodeCID(mmcCid);

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                _imageInfo.DriveManufacturer = Aaru.Decoders.MMC.VendorString.Prettify(decoded.Manufacturer);

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                _imageInfo.DriveModel = decoded.ProductName;

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                _imageInfo.DriveFirmwareRevision =
                    $"{(decoded.ProductRevision & 0xF0) >> 4:X2}.{decoded.ProductRevision & 0x0F:X2}";

            if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
                _imageInfo.DriveSerialNumber = $"{decoded.ProductSerialNumber}";
        }

        // Search for SCSI INQUIRY
        if(_mediaTags.TryGetValue(MediaTagType.SCSI_INQUIRY, out byte[] scsiInquiry))
        {
            Inquiry? nullableInquiry = Inquiry.Decode(scsiInquiry);

            if(nullableInquiry.HasValue)
            {
                Inquiry inquiry = nullableInquiry.Value;

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                    _imageInfo.DriveManufacturer = StringHandlers.CToString(inquiry.VendorIdentification)?.Trim();

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                    _imageInfo.DriveModel = StringHandlers.CToString(inquiry.ProductIdentification)?.Trim();

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
                    _imageInfo.DriveFirmwareRevision = StringHandlers.CToString(inquiry.ProductRevisionLevel)?.Trim();
            }
        }

        // Search for ATA or ATAPI IDENTIFY
        if(!_mediaTags.TryGetValue(MediaTagType.ATA_IDENTIFY, out byte[] ataIdentify) &&
           !_mediaTags.TryGetValue(MediaTagType.ATAPI_IDENTIFY, out ataIdentify))
            return;

        Identify.IdentifyDevice? nullableIdentify = CommonTypes.Structs.Devices.ATA.Identify.Decode(ataIdentify);

        if(!nullableIdentify.HasValue)
            return;

        Identify.IdentifyDevice identify = nullableIdentify.Value;

        string[] separated = identify.Model.Split(' ');

        if(separated.Length == 1)
            if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                _imageInfo.DriveModel = separated[0];
            else
            {
                if(string.IsNullOrWhiteSpace(_imageInfo.DriveManufacturer))
                    _imageInfo.DriveManufacturer = separated[0];

                if(string.IsNullOrWhiteSpace(_imageInfo.DriveModel))
                    _imageInfo.DriveModel = separated[^1];
            }

        if(string.IsNullOrWhiteSpace(_imageInfo.DriveFirmwareRevision))
            _imageInfo.DriveFirmwareRevision = identify.FirmwareRevision;

        if(string.IsNullOrWhiteSpace(_imageInfo.DriveSerialNumber))
            _imageInfo.DriveSerialNumber = identify.SerialNumber;
    }

    // Get the CICM XML media type from Aaru media type
    static XmlMediaType GetXmlMediaType(MediaType type)
    {
        switch(type)
        {
            case MediaType.CD:
            case MediaType.CDDA:
            case MediaType.CDG:
            case MediaType.CDEG:
            case MediaType.CDI:
            case MediaType.CDIREADY:
            case MediaType.CDROM:
            case MediaType.CDROMXA:
            case MediaType.CDPLUS:
            case MediaType.CDMO:
            case MediaType.CDR:
            case MediaType.CDRW:
            case MediaType.CDMRW:
            case MediaType.VCD:
            case MediaType.SVCD:
            case MediaType.PCD:
            case MediaType.SACD:
            case MediaType.DDCD:
            case MediaType.DDCDR:
            case MediaType.DDCDRW:
            case MediaType.DTSCD:
            case MediaType.CDMIDI:
            case MediaType.CDV:
            case MediaType.DVDROM:
            case MediaType.DVDR:
            case MediaType.DVDRW:
            case MediaType.DVDPR:
            case MediaType.DVDPRW:
            case MediaType.DVDPRWDL:
            case MediaType.DVDRDL:
            case MediaType.DVDPRDL:
            case MediaType.DVDRAM:
            case MediaType.DVDRWDL:
            case MediaType.DVDDownload:
            case MediaType.HDDVDROM:
            case MediaType.HDDVDRAM:
            case MediaType.HDDVDR:
            case MediaType.HDDVDRW:
            case MediaType.HDDVDRDL:
            case MediaType.HDDVDRWDL:
            case MediaType.BDROM:
            case MediaType.UHDBD:
            case MediaType.BDR:
            case MediaType.BDRE:
            case MediaType.BDRXL:
            case MediaType.BDREXL:
            case MediaType.EVD:
            case MediaType.FVD:
            case MediaType.HVD:
            case MediaType.CBHD:
            case MediaType.HDVMD:
            case MediaType.VCDHD:
            case MediaType.SVOD:
            case MediaType.FDDVD:
            case MediaType.LD:
            case MediaType.LDROM:
            case MediaType.CRVdisc:
            case MediaType.LDROM2:
            case MediaType.LVROM:
            case MediaType.MegaLD:
            case MediaType.PS1CD:
            case MediaType.PS2CD:
            case MediaType.PS2DVD:
            case MediaType.PS3DVD:
            case MediaType.PS3BD:
            case MediaType.PS4BD:
            case MediaType.PS5BD:
            case MediaType.UMD:
            case MediaType.XGD:
            case MediaType.XGD2:
            case MediaType.XGD3:
            case MediaType.XGD4:
            case MediaType.MEGACD:
            case MediaType.SATURNCD:
            case MediaType.GDROM:
            case MediaType.GDR:
            case MediaType.SuperCDROM2:
            case MediaType.JaguarCD:
            case MediaType.ThreeDO:
            case MediaType.PCFX:
            case MediaType.NeoGeoCD:
            case MediaType.GOD:
            case MediaType.WOD:
            case MediaType.WUOD:
            case MediaType.CDTV:
            case MediaType.CD32:
            case MediaType.Nuon:
            case MediaType.Playdia:
            case MediaType.Pippin:
            case MediaType.FMTOWNS:
            case MediaType.MilCD:
            case MediaType.VideoNow:
            case MediaType.VideoNowColor:
            case MediaType.VideoNowXp:
            case MediaType.CVD: return XmlMediaType.OpticalDisc;
            default: return XmlMediaType.BlockMedia;
        }
    }

    // Gets a DDT entry
    ulong GetDdtEntry(ulong sectorAddress)
    {
        if(_inMemoryDdt)
            return _userDataDdt[sectorAddress];

        if(_ddtEntryCache.TryGetValue(sectorAddress, out ulong entry))
            return entry;

        long oldPosition = _imageStream.Position;
        _imageStream.Position =  _outMemoryDdtPosition + Marshal.SizeOf<DdtHeader>();
        _imageStream.Position += (long)(sectorAddress * sizeof(ulong));
        var temp = new byte[sizeof(ulong)];
        _imageStream.Read(temp, 0, sizeof(ulong));
        _imageStream.Position = oldPosition;
        entry                 = BitConverter.ToUInt64(temp, 0);

        if(_ddtEntryCache.Count >= MAX_DDT_ENTRY_CACHE)
            _ddtEntryCache.Clear();

        _ddtEntryCache.Add(sectorAddress, entry);

        return entry;
    }

    // Sets a DDT entry
    void SetDdtEntry(ulong sectorAddress, ulong pointer)
    {
        if(_inMemoryDdt)
        {
            if(IsTape)
                _tapeDdt[sectorAddress] = pointer;
            else
                _userDataDdt[sectorAddress] = pointer;

            return;
        }

        long oldPosition = _imageStream.Position;
        _imageStream.Position =  _outMemoryDdtPosition + Marshal.SizeOf<DdtHeader>();
        _imageStream.Position += (long)(sectorAddress * sizeof(ulong));
        _imageStream.Write(BitConverter.GetBytes(pointer), 0, sizeof(ulong));
        _imageStream.Position = oldPosition;
    }

    // Converts between image data type and Aaru media tag type
    static MediaTagType GetMediaTagTypeForDataType(DataType type)
    {
        switch(type)
        {
            case DataType.CompactDiscPartialToc:            return MediaTagType.CD_TOC;
            case DataType.CompactDiscSessionInfo:           return MediaTagType.CD_SessionInfo;
            case DataType.CompactDiscToc:                   return MediaTagType.CD_FullTOC;
            case DataType.CompactDiscPma:                   return MediaTagType.CD_PMA;
            case DataType.CompactDiscAtip:                  return MediaTagType.CD_ATIP;
            case DataType.CompactDiscLeadInCdText:          return MediaTagType.CD_TEXT;
            case DataType.DvdPfi:                           return MediaTagType.DVD_PFI;
            case DataType.DvdLeadInCmi:                     return MediaTagType.DVD_CMI;
            case DataType.DvdDiscKey:                       return MediaTagType.DVD_DiscKey;
            case DataType.DvdBca:                           return MediaTagType.DVD_BCA;
            case DataType.DvdDmi:                           return MediaTagType.DVD_DMI;
            case DataType.DvdMediaIdentifier:               return MediaTagType.DVD_MediaIdentifier;
            case DataType.DvdMediaKeyBlock:                 return MediaTagType.DVD_MKB;
            case DataType.DvdRamDds:                        return MediaTagType.DVDRAM_DDS;
            case DataType.DvdRamMediumStatus:               return MediaTagType.DVDRAM_MediumStatus;
            case DataType.DvdRamSpareArea:                  return MediaTagType.DVDRAM_SpareArea;
            case DataType.DvdRRmd:                          return MediaTagType.DVDR_RMD;
            case DataType.DvdRPrerecordedInfo:              return MediaTagType.DVDR_PreRecordedInfo;
            case DataType.DvdRMediaIdentifier:              return MediaTagType.DVDR_MediaIdentifier;
            case DataType.DvdRPfi:                          return MediaTagType.DVDR_PFI;
            case DataType.DvdAdip:                          return MediaTagType.DVD_ADIP;
            case DataType.HdDvdCpi:                         return MediaTagType.HDDVD_CPI;
            case DataType.HdDvdMediumStatus:                return MediaTagType.HDDVD_MediumStatus;
            case DataType.DvdDlLayerCapacity:               return MediaTagType.DVDDL_LayerCapacity;
            case DataType.DvdDlMiddleZoneAddress:           return MediaTagType.DVDDL_MiddleZoneAddress;
            case DataType.DvdDlJumpIntervalSize:            return MediaTagType.DVDDL_JumpIntervalSize;
            case DataType.DvdDlManualLayerJumpLba:          return MediaTagType.DVDDL_ManualLayerJumpLBA;
            case DataType.BlurayDi:                         return MediaTagType.BD_DI;
            case DataType.BlurayBca:                        return MediaTagType.BD_BCA;
            case DataType.BlurayDds:                        return MediaTagType.BD_DDS;
            case DataType.BlurayCartridgeStatus:            return MediaTagType.BD_CartridgeStatus;
            case DataType.BluraySpareArea:                  return MediaTagType.BD_SpareArea;
            case DataType.AacsVolumeIdentifier:             return MediaTagType.AACS_VolumeIdentifier;
            case DataType.AacsSerialNumber:                 return MediaTagType.AACS_SerialNumber;
            case DataType.AacsMediaIdentifier:              return MediaTagType.AACS_MediaIdentifier;
            case DataType.AacsMediaKeyBlock:                return MediaTagType.AACS_MKB;
            case DataType.AacsDataKeys:                     return MediaTagType.AACS_DataKeys;
            case DataType.AacsLbaExtents:                   return MediaTagType.AACS_LBAExtents;
            case DataType.CprmMediaKeyBlock:                return MediaTagType.AACS_CPRM_MKB;
            case DataType.HybridRecognizedLayers:           return MediaTagType.Hybrid_RecognizedLayers;
            case DataType.ScsiMmcWriteProtection:           return MediaTagType.MMC_WriteProtection;
            case DataType.ScsiMmcDiscInformation:           return MediaTagType.MMC_DiscInformation;
            case DataType.ScsiMmcTrackResourcesInformation: return MediaTagType.MMC_TrackResourcesInformation;
            case DataType.ScsiMmcPowResourcesInformation:   return MediaTagType.MMC_POWResourcesInformation;
            case DataType.ScsiInquiry:                      return MediaTagType.SCSI_INQUIRY;
            case DataType.ScsiModePage2A:                   return MediaTagType.SCSI_MODEPAGE_2A;
            case DataType.AtaIdentify:                      return MediaTagType.ATA_IDENTIFY;
            case DataType.AtapiIdentify:                    return MediaTagType.ATAPI_IDENTIFY;
            case DataType.PcmciaCis:                        return MediaTagType.PCMCIA_CIS;
            case DataType.SecureDigitalCid:                 return MediaTagType.SD_CID;
            case DataType.SecureDigitalCsd:                 return MediaTagType.SD_CSD;
            case DataType.SecureDigitalScr:                 return MediaTagType.SD_SCR;
            case DataType.SecureDigitalOcr:                 return MediaTagType.SD_OCR;
            case DataType.MultiMediaCardCid:                return MediaTagType.MMC_CID;
            case DataType.MultiMediaCardCsd:                return MediaTagType.MMC_CSD;
            case DataType.MultiMediaCardOcr:                return MediaTagType.MMC_OCR;
            case DataType.MultiMediaCardExtendedCsd:        return MediaTagType.MMC_ExtendedCSD;
            case DataType.XboxSecuritySector:               return MediaTagType.Xbox_SecuritySector;
            case DataType.FloppyLeadOut:                    return MediaTagType.Floppy_LeadOut;
            case DataType.DvdDiscControlBlock:              return MediaTagType.DCB;
            case DataType.CompactDiscFirstTrackPregap:      return MediaTagType.CD_FirstTrackPregap;
            case DataType.CompactDiscLeadOut:               return MediaTagType.CD_LeadOut;
            case DataType.ScsiModeSense6:                   return MediaTagType.SCSI_MODESENSE_6;
            case DataType.ScsiModeSense10:                  return MediaTagType.SCSI_MODESENSE_10;
            case DataType.UsbDescriptors:                   return MediaTagType.USB_Descriptors;
            case DataType.XboxDmi:                          return MediaTagType.Xbox_DMI;
            case DataType.XboxPfi:                          return MediaTagType.Xbox_PFI;
            case DataType.CompactDiscMediaCatalogueNumber:  return MediaTagType.CD_MCN;
            case DataType.CompactDiscLeadIn:                return MediaTagType.CD_LeadIn;
            case DataType.DvdDiscKeyDecrypted:              return MediaTagType.DVD_DiscKey_Decrypted;
            default:                                        throw new ArgumentOutOfRangeException();
        }
    }

    // Converts between Aaru media tag type and image data type
    static DataType GetDataTypeForMediaTag(MediaTagType tag)
    {
        switch(tag)
        {
            case MediaTagType.CD_TOC: return DataType.CompactDiscPartialToc;
            case MediaTagType.CD_SessionInfo: return DataType.CompactDiscSessionInfo;
            case MediaTagType.CD_FullTOC: return DataType.CompactDiscToc;
            case MediaTagType.CD_PMA: return DataType.CompactDiscPma;
            case MediaTagType.CD_ATIP: return DataType.CompactDiscAtip;
            case MediaTagType.CD_TEXT: return DataType.CompactDiscLeadInCdText;
            case MediaTagType.DVD_PFI: return DataType.DvdPfi;
            case MediaTagType.DVD_CMI: return DataType.DvdLeadInCmi;
            case MediaTagType.DVD_DiscKey: return DataType.DvdDiscKey;
            case MediaTagType.DVD_BCA: return DataType.DvdBca;
            case MediaTagType.DVD_DMI: return DataType.DvdDmi;
            case MediaTagType.DVD_MediaIdentifier: return DataType.DvdMediaIdentifier;
            case MediaTagType.DVD_MKB: return DataType.DvdMediaKeyBlock;
            case MediaTagType.DVDRAM_DDS: return DataType.DvdRamDds;
            case MediaTagType.DVDRAM_MediumStatus: return DataType.DvdRamMediumStatus;
            case MediaTagType.DVDRAM_SpareArea: return DataType.DvdRamSpareArea;
            case MediaTagType.DVDR_RMD: return DataType.DvdRRmd;
            case MediaTagType.DVDR_PreRecordedInfo: return DataType.DvdRPrerecordedInfo;
            case MediaTagType.DVDR_MediaIdentifier: return DataType.DvdRMediaIdentifier;
            case MediaTagType.DVDR_PFI: return DataType.DvdRPfi;
            case MediaTagType.DVD_ADIP: return DataType.DvdAdip;
            case MediaTagType.HDDVD_CPI: return DataType.HdDvdCpi;
            case MediaTagType.HDDVD_MediumStatus: return DataType.HdDvdMediumStatus;
            case MediaTagType.DVDDL_LayerCapacity: return DataType.DvdDlLayerCapacity;
            case MediaTagType.DVDDL_MiddleZoneAddress: return DataType.DvdDlMiddleZoneAddress;
            case MediaTagType.DVDDL_JumpIntervalSize: return DataType.DvdDlJumpIntervalSize;
            case MediaTagType.DVDDL_ManualLayerJumpLBA: return DataType.DvdDlManualLayerJumpLba;
            case MediaTagType.BD_DI: return DataType.BlurayDi;
            case MediaTagType.BD_BCA: return DataType.BlurayBca;
            case MediaTagType.BD_DDS: return DataType.BlurayDds;
            case MediaTagType.BD_CartridgeStatus: return DataType.BlurayCartridgeStatus;
            case MediaTagType.BD_SpareArea: return DataType.BluraySpareArea;
            case MediaTagType.AACS_VolumeIdentifier: return DataType.AacsVolumeIdentifier;
            case MediaTagType.AACS_SerialNumber: return DataType.AacsSerialNumber;
            case MediaTagType.AACS_MediaIdentifier: return DataType.AacsMediaIdentifier;
            case MediaTagType.AACS_MKB: return DataType.AacsMediaKeyBlock;
            case MediaTagType.AACS_DataKeys: return DataType.AacsDataKeys;
            case MediaTagType.AACS_LBAExtents: return DataType.AacsLbaExtents;
            case MediaTagType.AACS_CPRM_MKB: return DataType.CprmMediaKeyBlock;
            case MediaTagType.Hybrid_RecognizedLayers: return DataType.HybridRecognizedLayers;
            case MediaTagType.MMC_WriteProtection: return DataType.ScsiMmcWriteProtection;
            case MediaTagType.MMC_DiscInformation: return DataType.ScsiMmcDiscInformation;
            case MediaTagType.MMC_TrackResourcesInformation: return DataType.ScsiMmcTrackResourcesInformation;
            case MediaTagType.MMC_POWResourcesInformation: return DataType.ScsiMmcPowResourcesInformation;
            case MediaTagType.SCSI_INQUIRY: return DataType.ScsiInquiry;
            case MediaTagType.SCSI_MODEPAGE_2A: return DataType.ScsiModePage2A;
            case MediaTagType.ATA_IDENTIFY: return DataType.AtaIdentify;
            case MediaTagType.ATAPI_IDENTIFY: return DataType.AtapiIdentify;
            case MediaTagType.PCMCIA_CIS: return DataType.PcmciaCis;
            case MediaTagType.SD_CID: return DataType.SecureDigitalCid;
            case MediaTagType.SD_CSD: return DataType.SecureDigitalCsd;
            case MediaTagType.SD_SCR: return DataType.SecureDigitalScr;
            case MediaTagType.SD_OCR: return DataType.SecureDigitalOcr;
            case MediaTagType.MMC_CID: return DataType.MultiMediaCardCid;
            case MediaTagType.MMC_CSD: return DataType.MultiMediaCardCsd;
            case MediaTagType.MMC_OCR: return DataType.MultiMediaCardOcr;
            case MediaTagType.MMC_ExtendedCSD: return DataType.MultiMediaCardExtendedCsd;
            case MediaTagType.Xbox_SecuritySector: return DataType.XboxSecuritySector;
            case MediaTagType.Floppy_LeadOut: return DataType.FloppyLeadOut;
            case MediaTagType.DCB: return DataType.DvdDiscControlBlock;
            case MediaTagType.CD_FirstTrackPregap: return DataType.CompactDiscFirstTrackPregap;
            case MediaTagType.CD_LeadOut: return DataType.CompactDiscLeadOut;
            case MediaTagType.SCSI_MODESENSE_6: return DataType.ScsiModeSense6;
            case MediaTagType.SCSI_MODESENSE_10: return DataType.ScsiModeSense10;
            case MediaTagType.USB_Descriptors: return DataType.UsbDescriptors;
            case MediaTagType.Xbox_DMI: return DataType.XboxDmi;
            case MediaTagType.Xbox_PFI: return DataType.XboxPfi;
            case MediaTagType.CD_MCN: return DataType.CompactDiscMediaCatalogueNumber;
            case MediaTagType.CD_LeadIn: return DataType.CompactDiscLeadIn;
            case MediaTagType.DVD_DiscKey_Decrypted: return DataType.DvdDiscKeyDecrypted;
            default: throw new ArgumentOutOfRangeException(nameof(tag), tag, null);
        }
    }
}