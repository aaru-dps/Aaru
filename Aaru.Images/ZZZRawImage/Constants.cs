// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for raw image, that is, user data sector by sector copy.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages;

public sealed partial class ZZZRawImage
{
    readonly byte[] _cdSync =
    {
        0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
    };
    readonly (MediaTagType tag, string name)[] _readWriteSidecars =
    {
        (MediaTagType.ATA_IDENTIFY, ".identify.bin"), (MediaTagType.BD_DI, ".di.bin"),
        (MediaTagType.CD_ATIP, ".atip.bin"), (MediaTagType.CD_FullTOC, ".toc.bin"),
        (MediaTagType.CD_FirstTrackPregap, ".leadin.bin"), (MediaTagType.CD_PMA, ".pma.bin"),
        (MediaTagType.CD_TEXT, ".cdtext.bin"), (MediaTagType.DCB, ".dcb.bin"), (MediaTagType.DVD_ADIP, ".adip.bin"),
        (MediaTagType.DVD_BCA, ".bca.bin"), (MediaTagType.DVD_CMI, ".cmi.bin"), (MediaTagType.DVD_DMI, ".dmi.bin"),
        (MediaTagType.DVD_MediaIdentifier, ".mid.bin"), (MediaTagType.DVD_PFI, ".pfi.bin"),
        (MediaTagType.DVDRAM_DDS, ".dds.bin"), (MediaTagType.DVDRAM_SpareArea, ".sai.bin"),
        (MediaTagType.DVDR_PFI, ".pfir.bin"), (MediaTagType.DVDR_PreRecordedInfo, ".pri.bin"),
        (MediaTagType.Floppy_LeadOut, ".leadout.bin"), (MediaTagType.HDDVD_CPI, ".cpi.bin"),
        (MediaTagType.MMC_ExtendedCSD, ".ecsd.bin"), (MediaTagType.PCMCIA_CIS, ".cis.bin"),
        (MediaTagType.SCSI_INQUIRY, ".inquiry.bin"), (MediaTagType.SCSI_MODEPAGE_2A, ".modepage2a.bin"),
        (MediaTagType.SCSI_MODESENSE_10, ".modesense10.bin"), (MediaTagType.SCSI_MODESENSE_6, ".modesense.bin"),
        (MediaTagType.SD_CID, ".cid.bin"), (MediaTagType.SD_CSD, ".csd.bin"), (MediaTagType.SD_OCR, ".ocr.bin"),
        (MediaTagType.SD_SCR, ".scr.bin"), (MediaTagType.USB_Descriptors, ".usbdescriptors.bin"),
        (MediaTagType.Xbox_DMI, ".xboxdmi.bin"), (MediaTagType.Xbox_PFI, ".xboxpfi.bin"),
        (MediaTagType.Xbox_SecuritySector, ".ss.bin")
    };

    readonly (MediaTagType tag, string name)[] _writeOnlySidecars =
    {
        (MediaTagType.ATAPI_IDENTIFY, ".identify.bin"), (MediaTagType.BD_BCA, ".bca.bin"),
        (MediaTagType.BD_DDS, ".dds.bin"), (MediaTagType.BD_DI, ".di.bin"), (MediaTagType.BD_SpareArea, ".sai.bin"),
        (MediaTagType.CD_LeadOut, ".leadout.bin"), (MediaTagType.MMC_CID, ".cid.bin"),
        (MediaTagType.MMC_CSD, ".csd.bin"), (MediaTagType.MMC_OCR, ".ocr.bin")
    };
}