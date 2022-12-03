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
//     Contains constants for CDRWin cuesheets (cue/bin).
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

namespace Aaru.DiscImages;

public sealed partial class CdrWin
{
    // Type for FILE entity
    /// <summary>Data as-is in little-endian</summary>
    const string CDRWIN_DISK_TYPE_LITTLE_ENDIAN = "BINARY";
    /// <summary>Data as-is in big-endian</summary>
    const string CDRWIN_DISK_TYPE_BIG_ENDIAN = "MOTOROLA";
    /// <summary>Audio in Apple AIF file</summary>
    const string CDRWIN_DISK_TYPE_AIFF = "AIFF";
    /// <summary>Audio in Microsoft WAV file</summary>
    const string CDRWIN_DISK_TYPE_RIFF = "WAVE";
    /// <summary>Audio in MP3 file</summary>
    const string CDRWIN_DISK_TYPE_MP3 = "MP3";

    // Type for TRACK entity
    /// <summary>Audio track, 2352 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_AUDIO = "AUDIO";
    /// <summary>CD+G track, 2448 bytes/sector (audio+subchannel)</summary>
    const string CDRWIN_TRACK_TYPE_CDG = "CDG";
    /// <summary>Mode 1 track, cooked, 2048 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE1 = "MODE1/2048";
    /// <summary>Mode 1 track, raw, 2352 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE1_RAW = "MODE1/2352";
    /// <summary>Mode 2 form 1 track, cooked, 2048 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE2_FORM1 = "MODE2/2048";
    /// <summary>Mode 2 form 2 track, cooked, 2324 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE2_FORM2 = "MODE2/2324";
    /// <summary>Mode 2 formless track, cooked, 2336 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE2_FORMLESS = "MODE2/2336";
    /// <summary>Mode 2 track, raw, 2352 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_MODE2_RAW = "MODE2/2352";
    /// <summary>CD-i track, cooked, 2336 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_CDI = "CDI/2336";
    /// <summary>CD-i track, raw, 2352 bytes/sector</summary>
    const string CDRWIN_TRACK_TYPE_CDI_RAW = "CDI/2352";

    // Type for REM ORIGINAL MEDIA-TYPE entity
    /// <summary>DiskType.CD</summary>
    const string CDRWIN_DISK_TYPE_CD = "CD";
    /// <summary>DiskType.CDRW</summary>
    const string CDRWIN_DISK_TYPE_CDRW = "CD-RW";
    /// <summary>DiskType.CDMRW</summary>
    const string CDRWIN_DISK_TYPE_CDMRW = "CD-MRW";
    /// <summary>DiskType.CDMRW</summary>
    const string CDRWIN_DISK_TYPE_CDMRW2 = "CD-(MRW)";
    /// <summary>DiskType.DVDROM</summary>
    const string CDRWIN_DISK_TYPE_DVD = "DVD";
    /// <summary>DiskType.DVDPRW</summary>
    const string CDRWIN_DISK_TYPE_DVDPMRW = "DVD+MRW";
    /// <summary>DiskType.DVDPRW</summary>
    const string CDRWIN_DISK_TYPE_DVDPMRW2 = "DVD+(MRW)";
    /// <summary>DiskType.DVDPRWDL</summary>
    const string CDRWIN_DISK_TYPE_DVDPMRWDL = "DVD+MRW DL";
    /// <summary>DiskType.DVDPRWDL</summary>
    const string CDRWIN_DISK_TYPE_DVDPMRWDL2 = "DVD+(MRW) DL";
    /// <summary>DiskType.DVDPR</summary>
    const string CDRWIN_DISK_TYPE_DVDPR = "DVD+R";
    /// <summary>DiskType.DVDPRDL</summary>
    const string CDRWIN_DISK_TYPE_DVDPRDL = "DVD+R DL";
    /// <summary>DiskType.DVDPRW</summary>
    const string CDRWIN_DISK_TYPE_DVDPRW = "DVD+RW";
    /// <summary>DiskType.DVDPRWDL</summary>
    const string CDRWIN_DISK_TYPE_DVDPRWDL = "DVD+RW DL";
    /// <summary>DiskType.DVDPR</summary>
    const string CDRWIN_DISK_TYPE_DVDPVR = "DVD+VR";
    /// <summary>DiskType.DVDRAM</summary>
    const string CDRWIN_DISK_TYPE_DVDRAM = "DVD-RAM";
    /// <summary>DiskType.DVDR</summary>
    const string CDRWIN_DISK_TYPE_DVDR = "DVD-R";
    /// <summary>DiskType.DVDRDL</summary>
    const string CDRWIN_DISK_TYPE_DVDRDL = "DVD-R DL";
    /// <summary>DiskType.DVDRW</summary>
    const string CDRWIN_DISK_TYPE_DVDRW = "DVD-RW";
    /// <summary>DiskType.DVDRWDL</summary>
    const string CDRWIN_DISK_TYPE_DVDRWDL = "DVD-RW DL";
    /// <summary>DiskType.DVDR</summary>
    const string CDRWIN_DISK_TYPE_DVDVR = "DVD-VR";
    /// <summary>DiskType.DVDRW</summary>
    const string CDRWIN_DISK_TYPE_DVDRW2 = "DVDRW";
    /// <summary>DiskType.HDDVDROM</summary>
    const string CDRWIN_DISK_TYPE_HDDVD = "HD DVD";
    /// <summary>DiskType.HDDVDRAM</summary>
    const string CDRWIN_DISK_TYPE_HDDVDRAM = "HD DVD-RAM";
    /// <summary>DiskType.HDDVDR</summary>
    const string CDRWIN_DISK_TYPE_HDDVDR = "HD DVD-R";
    /// <summary>DiskType.HDDVDR</summary>
    const string CDRWIN_DISK_TYPE_HDDVDRDL = "HD DVD-R DL";
    /// <summary>DiskType.HDDVDRW</summary>
    const string CDRWIN_DISK_TYPE_HDDVDRW = "HD DVD-RW";
    /// <summary>DiskType.HDDVDRW</summary>
    const string CDRWIN_DISK_TYPE_HDDVDRWDL = "HD DVD-RW DL";
    /// <summary>DiskType.BDROM</summary>
    const string CDRWIN_DISK_TYPE_BD = "BD";
    /// <summary>DiskType.BDR</summary>
    const string CDRWIN_DISK_TYPE_BDR = "BD-R";
    /// <summary>DiskType.BDRE</summary>
    const string CDRWIN_DISK_TYPE_BDRE = "BD-RE";
    /// <summary>DiskType.BDR</summary>
    const string CDRWIN_DISK_TYPE_BDRDL = "BD-R DL";
    /// <summary>DiskType.BDRE</summary>
    const string CDRWIN_DISK_TYPE_BDREDL = "BD-RE DL";

    const string REGEX_SESSION    = @"\bREM\s+SESSION\s+(?<number>\d+).*$";
    const string REGEX_MEDIA_TYPE = @"\bREM\s+ORIGINAL MEDIA-TYPE:\s+(?<mediatype>.+)$";
    const string REGEX_LEAD_OUT   = @"\bREM\s+LEAD-OUT\s+(?<msf>[\d]+:[\d]+:[\d]+).*$";

    // Not checked
    const string REGEX_LBA        = @"\bREM MSF:\s+(?<msf>[\d]+:[\d]+:[\d]+)\s+=\s+LBA:\s+(?<lba>[\d]+)$";
    const string REGEX_DISC_ID    = @"\bDISC_ID\s+(?<diskid>[\da-f]{8})$";
    const string REGEX_BARCODE    = @"\bUPC_EAN\s+(?<barcode>[\d]{12,13})$";
    const string REGEX_COMMENT    = @"\bREM\s+(?<comment>.+)$";
    const string REGEX_CDTEXT     = @"\bCDTEXTFILE\s+(?<filename>.+)$";
    const string REGEX_MCN        = @"^\s*CATALOG\s*(?<catalog>[\x21-\x7F]{13})$";
    const string REGEX_TITLE      = @"\bTITLE\s+(?<title>.+)$";
    const string REGEX_GENRE      = @"\bGENRE\s+(?<genre>.+)$";
    const string REGEX_ARRANGER   = @"\bARRANGER\s+(?<arranger>.+)$";
    const string REGEX_COMPOSER   = @"\bCOMPOSER\s+(?<composer>.+)$";
    const string REGEX_PERFORMER  = @"\bPERFORMER\s+(?<performer>.+)$";
    const string REGEX_SONGWRITER = @"\bSONGWRITER\s+(?<songwriter>.+)$";
    const string REGEX_FILE       = @"\bFILE\s+(?<filename>.+)\s+(?<type>\S+)$";
    const string REGEX_TRACK      = @"\bTRACK\s+(?<number>\d+)\s+(?<type>\S+)$";
    const string REGEX_ISRC       = @"\bISRC\s+(?<isrc>\w{12})$";
    const string REGEX_INDEX      = @"\bINDEX\s+(?<index>\d+)\s+(?<msf>[\d]+:[\d]+:[\d]+)$";
    const string REGEX_PREGAP     = @"\bPREGAP\s+(?<msf>[\d]+:[\d]+:[\d]+)$";
    const string REGEX_POSTGAP    = @"\bPOSTGAP\s+(?<msf>[\d]+:[\d]+:[\d]+)$";
    const string REGEX_FLAGS      = @"\bFLAGS\s+(((?<dcp>DCP)|(?<quad>4CH)|(?<pre>PRE)|(?<scms>SCMS))\s*)+$";

    // Trurip extensions
    const string REGEX_APPLICATION        = @"\bREM\s+Ripping Tool:\s+(?<application>.+)$";
    const string REGEX_TRURIP_DISC_HASHES = @"\bREM\s+DISC\s+HASHES$";
    const string REGEX_TRURIP_DISC_CRC32  = @"\bREM\s+CRC32\s+:\s+(?<hash>[\da-f]{8})$";
    const string REGEX_TRURIP_DISC_MD5    = @"\bREM\s+MD5\s+:\s+(?<hash>[\da-f]{32})$";
    const string REGEX_TRURIP_DISC_SHA1   = @"\bREM\s+SHA1\s+:\s+(?<hash>[\da-f]{40})$";
    const string REGEX_TRURIP_TRACK_METHOD =
        @"\bREM\s+Gap\s+Append\s+Method:\s+(?<method>Prev|None|Next)\s+\[(?<hash>\w+)\]$";
    const string REGEX_TRURIP_TRACK_CRC32   = @"\bREM\s+(Gap|Trk)\s+(?<number>\d{2}):\s+[\da-f]{8}$";
    const string REGEX_TRURIP_TRACK_MD5     = @"\bREM\s+(Gap|Trk)\s+(?<number>\d{2}):\s+[\da-f]{32}$";
    const string REGEX_TRURIP_TRACK_SHA1    = @"\bREM\s+(Gap|Trk)\s+(?<number>\d{2}):\s+[\da-f]{40}$";
    const string REGEX_TRURIP_TRACK_UNKNOWN = @"\bREM\s+(Gap|Trk)\s+(?<number>\d{2}):\s+[\da-f]{8,}$";

    // Redump.org extensions
    const string REGEX_REDUMP_SD_AREA = @"\bREM\s+SINGLE-DENSITY\s+AREA$";
    const string REGEX_REDUMP_HD_AREA = @"\bREM\s+HIGH-DENSITY\s+AREA$";

    const string REGEX_DIC_MEDIA_TYPE      = @"\bREM\s+METADATA DIC MEDIA-TYPE:\s+(?<mediatype>.+)$";
    const string REGEX_AARU_MEDIA_TYPE     = @"\bREM\s+METADATA AARU MEDIA-TYPE:\s+(?<mediatype>.+)$";
    const string REGEX_APPLICATION_VERSION = @"\bREM\s+Ripping Tool Version:\s+(?<application>.+)$";
    const string REGEX_DUMP_EXTENT =
        @"\bREM\s+METADATA DUMP EXTENT:\s+(?<application>.+)\s+\|\s+(?<version>.+)\s+\|\s+(?<os>.+)\s+\|\s+(?<manufacturer>.+)\s+\|\s+(?<model>.+)\s+\|\s+(?<firmware>.+)\s+\|\s+(?<serial>.+)\s+\|\s+(?<start>\d+):(?<end>\d+)$";
}