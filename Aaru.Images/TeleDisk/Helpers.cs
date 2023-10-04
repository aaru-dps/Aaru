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
//     Contains helpers for Sydex TeleDisk disk images.
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

using System;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages;

public sealed partial class TeleDisk
{
    (ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
    {
        var cylinder = (ushort)(lba                            / (_imageInfo.Heads * _imageInfo.SectorsPerTrack));
        var head     = (byte)(lba / _imageInfo.SectorsPerTrack % _imageInfo.Heads);
        var sector   = (byte)(lba % _imageInfo.SectorsPerTrack + 1);

        return (cylinder, head, sector);
    }

    static ushort TeleDiskCrc(ushort crc, byte[] buffer)
    {
        var counter = 0;

        while(counter < buffer.Length)
        {
            crc ^= (ushort)((buffer[counter] & 0xFF) << 8);

            for(var i = 0; i < 8; i++)
            {
                if((crc & 0x8000) > 0)
                    crc = (ushort)(crc << 1 ^ TELE_DISK_CRC_POLY);
                else
                    crc = (ushort)(crc << 1);
            }

            counter++;
        }

        return crc;
    }

    static ErrorNumber DecodeTeleDiskData(byte       sectorSize, byte encodingType, byte[] encodedData,
                                          out byte[] decodedData)
    {
        decodedData = null;

        switch(sectorSize)
        {
            case SECTOR_SIZE_128:
                decodedData = new byte[128];

                break;
            case SECTOR_SIZE_256:
                decodedData = new byte[256];

                break;
            case SECTOR_SIZE_512:
                decodedData = new byte[512];

                break;
            case SECTOR_SIZE_1K:
                decodedData = new byte[1024];

                break;
            case SECTOR_SIZE_2K:
                decodedData = new byte[2048];

                break;
            case SECTOR_SIZE_4K:
                decodedData = new byte[4096];

                break;
            case SECTOR_SIZE_8K:
                decodedData = new byte[8192];

                break;
            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.Sector_size_0_is_incorrect, sectorSize));

                return ErrorNumber.InvalidArgument;
        }

        switch(encodingType)
        {
            case DATA_BLOCK_COPY:
                Array.Copy(encodedData, decodedData, decodedData.Length);

                break;
            case DATA_BLOCK_PATTERN:
            {
                var ins  = 0;
                var outs = 0;

                while(ins < encodedData.Length)
                {
                    var repeatValue = new byte[2];

                    var repeatNumber = BitConverter.ToUInt16(encodedData, ins);
                    Array.Copy(encodedData, ins + 2, repeatValue, 0, 2);
                    var decodedPiece = new byte[repeatNumber * 2];
                    ArrayHelpers.ArrayFill(decodedPiece, repeatValue);
                    Array.Copy(decodedPiece, 0, decodedData, outs, decodedPiece.Length);
                    ins  += 4;
                    outs += decodedPiece.Length;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Block_pattern_decoder_Input_data_size_0_bytes,
                                           encodedData.Length);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Block_pattern_decoder_Processed_input_0_bytes,
                                           ins);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Block_pattern_decoder_Output_data_size_0_bytes,
                                           decodedData.Length);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Block_pattern_decoder_Processed_Output_0_bytes,
                                           outs);

                break;
            }
            case DATA_BLOCK_RLE:
            {
                var ins  = 0;
                var outs = 0;

                while(ins < encodedData.Length)
                {
                    byte length;

                    byte encoding = encodedData[ins];

                    if(encoding == 0x00)
                    {
                        length = encodedData[ins + 1];
                        Array.Copy(encodedData, ins + 2, decodedData, outs, length);
                        ins  += 2 + length;
                        outs += length;
                    }
                    else
                    {
                        length = (byte)(encoding * 2);
                        byte run  = encodedData[ins + 1];
                        var  part = new byte[length];
                        Array.Copy(encodedData, ins + 2, part, 0, length);
                        var piece = new byte[length * run];
                        ArrayHelpers.ArrayFill(piece, part);
                        Array.Copy(piece, 0, decodedData, outs, piece.Length);
                        ins  += 2 + length;
                        outs += piece.Length;
                    }
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.RLE_decoder_Input_data_size_0_bytes,
                                           encodedData.Length);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.RLE_decoder_Processed_input_0_bytes, ins);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.RLE_decoder_Output_data_size_0_bytes,
                                           decodedData.Length);

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.RLE_decoder_Processed_Output_0_bytes, outs);

                break;
            }
            default:
                AaruConsole.ErrorWriteLine(string.Format(Localization.Data_encoding_0_is_incorrect, encodingType));

                return ErrorNumber.InvalidArgument;
        }

        return ErrorNumber.NoError;
    }

    MediaType DecodeTeleDiskDiskType()
    {
        switch(_header.DriveType)
        {
            case DRIVE_TYPE_525_DD:
            case DRIVE_TYPE_525_HD_DD_DISK:
            case DRIVE_TYPE_525_HD:
            {
                switch(_totalDiskSize)
                {
                    case 143360:
                        return _imageInfo.SectorSize == 256 ? MediaType.MetaFloppy_Mod_I : MediaType.Unknown;
                    case 163840:
                    {
                        // Acorn disk uses 256 bytes/sector
                        return _imageInfo.SectorSize == 256 ? MediaType.ACORN_525_SS_DD_40 : MediaType.DOS_525_SS_DD_8;

                        // DOS disks use 512 bytes/sector
                    }
                    case 184320:
                    {
                        // Atari disk uses 256 bytes/sector
                        return _imageInfo.SectorSize == 256 ? MediaType.ATARI_525_DD : MediaType.DOS_525_SS_DD_9;

                        // DOS disks use 512 bytes/sector
                    }
                    case 315392:
                        return _imageInfo.SectorSize == 256 ? MediaType.MetaFloppy_Mod_II : MediaType.Unknown;
                    case 327680:
                    {
                        // Acorn disk uses 256 bytes/sector
                        return _imageInfo.SectorSize == 256 ? MediaType.ACORN_525_SS_DD_80 : MediaType.DOS_525_DS_DD_8;

                        // DOS disks use 512 bytes/sector
                    }
                    case 368640:
                        return MediaType.DOS_525_DS_DD_9;
                    case 1228800:
                        return MediaType.DOS_525_HD;
                    case 102400:
                        return MediaType.ACORN_525_SS_SD_40;
                    case 204800:
                        return MediaType.ACORN_525_SS_SD_80;
                    case 655360:
                        return MediaType.ACORN_525_DS_DD;
                    case 92160:
                        return MediaType.ATARI_525_SD;
                    case 133120:
                        return MediaType.ATARI_525_ED;
                    case 1310720:
                        return MediaType.NEC_525_HD;
                    case 1261568:
                        return MediaType.SHARP_525;
                    case 839680:
                        return MediaType.FDFORMAT_525_DD;
                    case 1304320:
                        return MediaType.ECMA_99_8;
                    case 1223424:
                        return MediaType.ECMA_99_15;
                    case 1061632:
                        return MediaType.ECMA_99_26;
                    case 80384:
                        return MediaType.ECMA_66;
                    case 325632:
                        return MediaType.ECMA_70;
                    case 653312:
                        return MediaType.ECMA_78;
                    case 737280:
                        return MediaType.ECMA_78_2;
                    default:
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "Unknown 5,25\" disk with {0} bytes", _totalDiskSize);

                        return MediaType.Unknown;
                    }
                }
            }
            case DRIVE_TYPE_35_DD:
            case DRIVE_TYPE_35_ED:
            case DRIVE_TYPE_35_HD:
            {
                switch(_totalDiskSize)
                {
                    case 322560:
                        return MediaType.Apricot_35;
                    case 327680:
                        return MediaType.DOS_35_SS_DD_8;
                    case 368640:
                        return MediaType.DOS_35_SS_DD_9;
                    case 655360:
                        return MediaType.DOS_35_DS_DD_8;
                    case 737280:
                        return MediaType.DOS_35_DS_DD_9;
                    case 1474560:
                        return MediaType.DOS_35_HD;
                    case 2949120:
                        return MediaType.DOS_35_ED;
                    case 1720320:
                        return MediaType.DMF;
                    case 1763328:
                        return MediaType.DMF_82;
                    case 1884160: // Irreal size, seen as BIOS with TSR, 23 sectors/track
                    case 1860608: // Real data size, sum of all sectors
                        return MediaType.XDF_35;
                    case 819200:
                        return MediaType.CBM_35_DD;
                    case 901120:
                        return MediaType.CBM_AMIGA_35_DD;
                    case 1802240:
                        return MediaType.CBM_AMIGA_35_HD;
                    case 1310720:
                        return MediaType.NEC_35_HD_8;
                    case 1228800:
                        return MediaType.NEC_35_HD_15;
                    case 1261568:
                        return MediaType.SHARP_35;
                    default:
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "Unknown 3,5\" disk with {0} bytes", _totalDiskSize);

                        return MediaType.Unknown;
                    }
                }
            }
            case DRIVE_TYPE_8_INCH:
            {
                switch(_totalDiskSize)
                {
                    case 81664:
                        return MediaType.IBM23FD;
                    case 242944:
                        return MediaType.IBM33FD_128;
                    case 287488:
                        return MediaType.IBM33FD_256;
                    case 306432:
                        return MediaType.IBM33FD_512;
                    case 499200:
                        return MediaType.IBM43FD_128;
                    case 574976:
                        return MediaType.IBM43FD_256;
                    case 995072:
                        return MediaType.IBM53FD_256;
                    case 1146624:
                        return MediaType.IBM53FD_512;
                    case 1222400:
                        return MediaType.IBM53FD_1024;
                    case 256256:
                        // Same size, with same disk geometry, for DEC RX01, NEC and ECMA, return ECMA
                        return MediaType.ECMA_54;
                    case 512512:
                    {
                        // DEC disk uses 256 bytes/sector
                        return _imageInfo.SectorSize == 256 ? MediaType.RX02 : MediaType.ECMA_59;

                        // ECMA disks use 128 bytes/sector
                    }
                    case 1261568:
                        return MediaType.NEC_8_DD;
                    case 1255168:
                        return MediaType.ECMA_69_8;
                    case 1177344:
                        return MediaType.ECMA_69_15;
                    case 1021696:
                        return MediaType.ECMA_69_26;
                    default:
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, "Unknown 8\" disk with {0} bytes", _totalDiskSize);

                        return MediaType.Unknown;
                    }
                }
            }
            default:
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "Unknown drive type {1} with {0} bytes", _totalDiskSize,
                                           _header.DriveType);

                return MediaType.Unknown;
            }
        }
    }
}