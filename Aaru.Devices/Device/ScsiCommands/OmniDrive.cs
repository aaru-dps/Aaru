// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : OmniDrive.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//                  Natalia Portillo <clania@claunia.com>
//
// Component      : OmniDrive firmware vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for OmniDrive firmware. OmniDrive is custom
//     firmware that adds SCSI command 0xC0 to read raw DVD sectors (2064 bytes)
//     directly by LBA.
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
// Copyright © 2011-2026 Rebecca Wallander
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Decoders.DVD;
using BlurayDataFrame = Aaru.Decoders.Bluray.DataFrame;
using Aaru.Logging;
using NintendoSector = Aaru.Decoders.Nintendo.Sector;

namespace Aaru.Devices;

public partial class Device
{
    const int OMNIDRIVE_BD_DATA_FRAME_SIZE  = BlurayDataFrame.Size;
    const int OMNIDRIVE_BD_TRANSFER_PER_LBA = OMNIDRIVE_BD_DATA_FRAME_SIZE + BlurayDataFrame.UserControlDataSize;

    readonly Sector         _dvdSectorDecoder      = new();
    readonly NintendoSector _nintendoSectorDecoder = new();

    /// <summary>
    ///     Encodes byte 1 of the OmniDrive READ CDB to match redumper's <c>CDB12_ReadOmniDrive</c>
    ///     (<c>scsi/mmc.ixx</c>): <c>disc_type</c> :2 (LSB), <c>raw_addressing</c> :1, <c>fua</c> :1, <c>descramble</c> :1,
    ///     reserved :3.
    /// </summary>
    /// <param name="discType">0 = CD, 1 = DVD, 2 = BD (redumper <c>OmniDrive_DiscType</c>).</param>
    /// <param name="rawAddressing">Set to <c>true</c> to use raw addressing.</param>
    /// <param name="fua">Set to <c>true</c> if the command should use FUA.</param>
    /// <param name="descramble">Set to <c>true</c> if the data should be descrambled by the device.</param>
    static byte EncodeOmniDriveReadCdb1(OmniDriveDiscType discType, bool rawAddressing, bool fua, bool descramble)
    {
        int d = (byte)discType & 3;
        int r = rawAddressing ? 1 : 0;
        int f = fua ? 1 : 0;
        int s = descramble ? 1 : 0;

        return (byte)(d | r << 2 | f << 3 | s << 4);
    }

    static void FillOmniDriveReadCdb(Span<byte> cdb, uint lba, uint transferLength, byte cdbByte1)
    {
        cdb.Clear();
        cdb[0]  = (byte)ScsiCommands.ReadOmniDrive;
        cdb[1]  = cdbByte1;
        cdb[2]  = (byte)(lba >> 24            & 0xFF);
        cdb[3]  = (byte)(lba >> 16            & 0xFF);
        cdb[4]  = (byte)(lba >> 8             & 0xFF);
        cdb[5]  = (byte)(lba                  & 0xFF);
        cdb[6]  = (byte)(transferLength >> 24 & 0xFF);
        cdb[7]  = (byte)(transferLength >> 16 & 0xFF);
        cdb[8]  = (byte)(transferLength >> 8  & 0xFF);
        cdb[9]  = (byte)(transferLength       & 0xFF);
        cdb[10] = 0; // subchannels=NONE, c2=0
        cdb[11] = 0; // control
    }

    /// <summary>
    ///     Checks if the drive has OmniDrive firmware by inspecting INQUIRY Reserved5 (bytes 74+) for "OmniDrive",
    ///     matching redumper's is_omnidrive_firmware behaviour.
    /// </summary>
    /// <returns><c>true</c> if Reserved5 starts with "OmniDrive" and has at least 11 bytes (8 + 3 for version).</returns>
    public bool IsOmniDriveFirmware(out byte major, out byte minor, out byte revision)
    {
        major    = 0;
        minor    = 0;
        revision = 0;

        bool sense = ScsiInquiry(out byte[] buffer, out _, Timeout, out _);

        if(sense || buffer == null) return false;

        Inquiry? inquiry = Inquiry.Decode(buffer);

        if(inquiry?.Reserved5 is not { Length: >= 12 }) return false;

        byte[]             reserved5 = inquiry.Value.Reserved5;
        ReadOnlySpan<byte> omnidrive = "OmniDrive"u8;

        if(!omnidrive.SequenceEqual(reserved5.AsSpan(0, omnidrive.Length))) return false;

        major    = reserved5[9];
        minor    = reserved5[10];
        revision = reserved5[11];

        return true;
    }

    /// <summary>Reads raw DVD sectors (2064 bytes) directly by LBA on OmniDrive firmware.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the raw DataFrame response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Start block address (LBA).</param>
    /// <param name="transferLength">Number of 2064-byte sectors to read.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="fua">Set to <c>true</c> if the command should use FUA.</param>
    /// <param name="descramble">Set to <c>true</c> if the data should be descrambled by the device.</param>
    /// <param name="rawAddresing">Set to <c>true</c> to use raw addressing.</param>
    public bool OmniDriveReadRawDvd(out byte[] buffer,            out ReadOnlySpan<byte> senseBuffer, uint lba,
                                    uint       transferLength,    uint timeout, out double duration, bool fua = false,
                                    bool       descramble = true, bool rawAddresing = false)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..12];
        buffer = new byte[2064 * transferLength];

        FillOmniDriveReadCdb(cdb,
                             lba,
                             transferLength,
                             EncodeOmniDriveReadCdb1(OmniDriveDiscType.Dvd, rawAddresing, fua, descramble));

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        if(!Sector.CheckIed(buffer, transferLength)) return true;

        if(descramble && !Sector.CheckEdc(buffer, transferLength)) return true;

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "OmniDrive READ RAW DVD took {0} ms", duration);

        return sense;
    }

    /// <summary>Reads raw Blu-ray sectors (2052-byte DataFrame) directly by LBA on OmniDrive firmware.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the raw Blu-ray DataFrame response will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Start block address (LBA).</param>
    /// <param name="transferLength">Number of 2052-byte sectors to read.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="fua">Set to <c>true</c> if the command should use FUA.</param>
    /// <param name="descramble">Set to <c>true</c> if the data should be descrambled by the device.</param>
    public bool OmniDriveReadRawBd(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint lba, uint transferLength,
                                   uint       timeout, out double duration, bool fua = false, bool descramble = true)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb          = CdbBuffer[..12];
        byte[]     deviceBuffer = new byte[OMNIDRIVE_BD_TRANSFER_PER_LBA * transferLength];
        buffer = new byte[OMNIDRIVE_BD_DATA_FRAME_SIZE * transferLength];

        FillOmniDriveReadCdb(cdb,
                             lba,
                             transferLength,
                             EncodeOmniDriveReadCdb1(OmniDriveDiscType.Bd, false, fua, descramble));

        LastError = SendScsiCommand(cdb, ref deviceBuffer, timeout, ScsiDirection.In, out duration, out bool sense);

        if(sense)
        {
            Error = LastError != 0;

            return true;
        }

        for(uint i = 0; i < transferLength; i++)
        {
            int deviceOffset = (int)(i * OMNIDRIVE_BD_TRANSFER_PER_LBA);
            int dataOffset   = (int)(i * OMNIDRIVE_BD_DATA_FRAME_SIZE);
            Array.Copy(deviceBuffer, deviceOffset, buffer, dataOffset, OMNIDRIVE_BD_DATA_FRAME_SIZE);

            if(descramble &&
               !Decoders.Bluray.Sector.CheckEdc(buffer.AsSpan(dataOffset, OMNIDRIVE_BD_DATA_FRAME_SIZE).ToArray()))
                return true;
        }

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "OmniDrive READ RAW BD took {0} ms", duration);

        return false;
    }

    /// <summary>
    ///     Reads Nintendo GameCube/Wii DVD sectors (2064 bytes) on OmniDrive. The drive returns DVD-layer data with
    ///     drive-side DVD descramble off; this method applies per-sector Nintendo XOR descramble and returns the result.
    ///     LBAs 0–15 use key 0; LBAs ≥ 16 use <paramref name="derivedDiscKey" /> (from LBA 0 CPR_MAI), or 0 if not yet
    ///     derived.
    /// </summary>
    /// <param name="derivedDiscKey">Disc key from LBA 0 (0–15); <c>null</c> until the host has read and derived it.</param>
    /// <param name="negativeAddressing">
    ///     True when caller is reading wrapped negative LBAs (lead-in); these sectors use standard DVD descramble.
    /// </param>
    /// <param name="regularDataEndExclusive">
    ///     Exclusive upper bound of regular user-data LBAs. Sectors at or above this are leadout and use standard DVD
    ///     descramble.
    /// </param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the descrambled sector data will be stored.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Start block address (LBA).</param>
    /// <param name="transferLength">Number of 2064-byte sectors to read.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="fua">Set to <c>true</c> if the command should use FUA.</param>
    /// <param name="descramble">Set to <c>true</c> if the data should be descrambled by the host.</param>
    public bool OmniDriveReadNintendoDvd(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint lba,
                                         uint       transferLength, uint timeout, out double duration, bool fua = false,
                                         bool       descramble         = true, byte? derivedDiscKey = null,
                                         bool       negativeAddressing = false, ulong regularDataEndExclusive = 0)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..12];
        buffer = new byte[2064 * transferLength];

        FillOmniDriveReadCdb(cdb,
                             lba,
                             transferLength,
                             EncodeOmniDriveReadCdb1(OmniDriveDiscType.Dvd, false, fua, false));

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        if(!Sector.CheckIed(buffer, transferLength)) return true;

        if(descramble)
        {
            const int  sectorBytes   = 2064;
            var        outBuf        = new byte[sectorBytes * transferLength];
            const uint maxRegularLba = 0x00FFFFFF;

            for(uint i = 0; i < transferLength; i++)
            {
                var slice = new byte[sectorBytes];
                Array.Copy(buffer, i * sectorBytes, slice, 0, sectorBytes);

                uint absLba = lba + i;
                bool wrappedNegative = negativeAddressing || absLba > maxRegularLba;
                bool leadout = !wrappedNegative && regularDataEndExclusive > 0 && absLba >= regularDataEndExclusive;

                ErrorNumber errno;
                byte[]      descrambled;

                if(wrappedNegative || leadout)
                    errno = _dvdSectorDecoder.Scramble(slice, out descrambled);
                else
                {
                    byte key = absLba < 16 ? (byte)0 : derivedDiscKey ?? 0;
                    errno = _nintendoSectorDecoder.Scramble(slice, key, out descrambled);
                }

                if(errno != ErrorNumber.NoError)
                {
                    LastError = (int)errno;
                    Error     = true;

                    return true;
                }

                Array.Copy(descrambled, 0, outBuf, i * sectorBytes, sectorBytes);
            }

            buffer = outBuf;
        }

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "OmniDrive READ NINTENDO DVD took {0} ms", duration);

        return sense;
    }

    public bool OmniDriveReadCd(out byte[] buffer,  out ReadOnlySpan<byte> senseBuffer, uint lba, uint transferLength,
                                uint       timeout, out double duration, bool fua = false, bool rawAddressing = false)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..12];
        buffer = new byte[2448 * transferLength];

        cdb.Clear();
        cdb[0]  = (byte)ScsiCommands.ReadOmniDrive;
        cdb[1]  = EncodeOmniDriveReadCdb1(OmniDriveDiscType.Cd, rawAddressing, fua, false);
        cdb[2]  = (byte)(lba >> 24            & 0xFF);
        cdb[3]  = (byte)(lba >> 16            & 0xFF);
        cdb[4]  = (byte)(lba >> 8             & 0xFF);
        cdb[5]  = (byte)(lba                  & 0xFF);
        cdb[6]  = (byte)(transferLength >> 24 & 0xFF);
        cdb[7]  = (byte)(transferLength >> 16 & 0xFF);
        cdb[8]  = (byte)(transferLength >> 8  & 0xFF);
        cdb[9]  = (byte)(transferLength       & 0xFF);
        cdb[10] = 1; // subchannels=RW
        cdb[11] = 0; // control

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "OmniDrive READ CD took {0} ms", duration);

        return sense;
    }

    public bool OmniDriveReadCdWithC2(out byte[] buffer,         out ReadOnlySpan<byte> senseBuffer, uint lba,
                                      uint       transferLength, uint timeout, out double duration, bool fua = false,
                                      bool       rawAddressing = false)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..12];
        buffer = new byte[2742 * transferLength];

        cdb.Clear();
        cdb[0]  = (byte)ScsiCommands.ReadOmniDrive;
        cdb[1]  = EncodeOmniDriveReadCdb1(OmniDriveDiscType.Cd, rawAddressing, fua, false);
        cdb[2]  = (byte)(lba >> 24            & 0xFF);
        cdb[3]  = (byte)(lba >> 16            & 0xFF);
        cdb[4]  = (byte)(lba >> 8             & 0xFF);
        cdb[5]  = (byte)(lba                  & 0xFF);
        cdb[6]  = (byte)(transferLength >> 24 & 0xFF);
        cdb[7]  = (byte)(transferLength >> 16 & 0xFF);
        cdb[8]  = (byte)(transferLength >> 8  & 0xFF);
        cdb[9]  = (byte)(transferLength       & 0xFF);
        cdb[10] = 4 + 1; // subchannels=RW, C2
        cdb[11] = 0;     // control

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, "OmniDrive READ CD took {0} ms", duration);

        return sense;
    }

    enum OmniDriveDiscType
    {
        Cd  = 0,
        Dvd = 1,
        Bd  = 2
    }
}