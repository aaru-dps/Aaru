// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Drive.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Drive access for Data Position Measurement, timed with our own clock.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dpm;

/// <summary>Kind of optical medium, which sets the DPM timing parameters</summary>
public enum DpmMediumKind
{
    /// <summary>Compact Disc</summary>
    Cd,
    /// <summary>DVD</summary>
    Dvd,
    /// <summary>Blu-ray</summary>
    Bd
}

/// <summary>Geometry of the medium in the drive, as needed to measure DPM</summary>
public struct DpmMedium
{
    /// <summary>Kind of medium</summary>
    public DpmMediumKind Kind;
    /// <summary>Current MMC profile, or -1 if unknown</summary>
    public int Profile;
    /// <summary>Last readable sector, <see cref="uint.MaxValue" /> if unknown</summary>
    public uint LastLba;
    /// <summary>Last sector of every layer but the last</summary>
    public uint[] LayerEnds;
    /// <summary>Whether the layers use opposite track path</summary>
    public bool OppositeTrackPath;
    /// <summary>Nominal channel bit length in metres (DVD and BD), 0 if unknown</summary>
    public double ChannelBit;
}

/// <summary>Drive operations needed to measure DPM</summary>
public interface IDpmDrive
{
    /// <summary>Monotonic time in nanoseconds</summary>
    long Now { get; }

    /// <summary>Reads one sector with Force Unit Access, timing it</summary>
    /// <param name="lba">Sector address</param>
    /// <param name="elapsed">Nanoseconds the command took</param>
    /// <param name="sincePrevious">
    ///     Nanoseconds since the previous command finished, including any time the host took before sending this one,
    ///     while the disc kept turning, or <paramref name="elapsed" /> if there was no previous command
    /// </param>
    /// <returns><c>true</c> if the sector was read, <c>false</c> otherwise</returns>
    bool TimedRead(uint lba, out long elapsed, out long sincePrevious);

    /// <summary>Reads several sectors without timing them, to flush the drive cache</summary>
    /// <param name="lba">First sector</param>
    /// <param name="blocks">Number of sectors</param>
    void ReadUntimed(uint lba, ushort blocks);

    /// <summary>Sets the drive read speed</summary>
    /// <param name="kbps">Speed in kB/s, 0xFFFF is the drive maximum</param>
    /// <returns><c>true</c> if the drive accepted it</returns>
    bool SetSpeed(ushort kbps);

    /// <summary>Disables the drive read cache with the caching mode page, if the drive supports it</summary>
    /// <returns><c>true</c> if the read cache was disabled</returns>
    bool DisableReadCache();

    /// <summary>Restores the caching mode page changed by <see cref="DisableReadCache" /></summary>
    void RestoreReadCache();

    /// <summary>Identifies the medium in the drive</summary>
    /// <param name="medium">Medium geometry</param>
    /// <returns><c>true</c> if the medium can be measured</returns>
    bool DetectMedium(out DpmMedium medium);
}

/// <summary>DPM drive operations on a real device</summary>
/// <remarks>
///     Commands are timed here, with a <see cref="Stopwatch" /> around the command and nothing else, because the
///     duration devices report has a resolution of a millisecond and DPM needs better than a tenth of it.
/// </remarks>
public sealed class DeviceDpmDrive : IDpmDrive
{
    const uint   TIMEOUT          = 20;
    const ushort DECOY_MAX_BLOCKS = 64;
    /// <summary>Physical sector number of LBA 0 on DVD</summary>
    const uint DVD_PSN_BASE = 0x030000;

    readonly byte[] _cdb = new byte[12];
    readonly Device _dev;
    /// <summary>First and last sector of every audio track, timed with READ CD as READ(12) can't read them</summary>
    (uint first, uint last)[] _audio = [];
    byte[] _audioBuffer = new byte[2352];
    byte[] _cachePage;
    byte[] _decoyBuffer = new byte[DECOY_MAX_BLOCKS * 2352];
    byte[] _sectorBuffer = new byte[2048];
    /// <summary>Timestamp when the previous command finished, 0 if none</summary>
    long _previousEnd;

    /// <summary>Creates DPM drive operations on a device</summary>
    /// <param name="dev">Device</param>
    public DeviceDpmDrive(Device dev) => _dev = dev;

    /// <inheritdoc />
    public long Now => (long)(Stopwatch.GetTimestamp() * (1e9 / Stopwatch.Frequency));

    /// <inheritdoc />
    public bool TimedRead(uint lba, out long elapsed, out long sincePrevious)
    {
        bool   audio  = IsAudio(lba);
        byte[] buffer = audio ? _audioBuffer : _sectorBuffer;

        if(audio)
            FillReadCd(lba, 1);
        else
            FillRead12(lba, 1, true);

        long start = Stopwatch.GetTimestamp();
        int  error = _dev.SendScsiCommand(_cdb, ref buffer, TIMEOUT, ScsiDirection.In, out _, out bool sense);
        long end   = Stopwatch.GetTimestamp();

        elapsed       = (long)((end - start) * (1e9 / Stopwatch.Frequency));
        sincePrevious = _previousEnd == 0 ? elapsed : (long)((end - _previousEnd) * (1e9 / Stopwatch.Frequency));
        _previousEnd  = end;

        return error == 0 && !sense;
    }

    /// <inheritdoc />
    public void ReadUntimed(uint lba, ushort blocks)
    {
        if(blocks > DECOY_MAX_BLOCKS) blocks = DECOY_MAX_BLOCKS;

        bool audio = IsAudio(lba);

        if(audio)
            FillReadCd(lba, blocks);
        else
            FillRead12(lba, blocks, false);

        int length = blocks * (audio ? 2352 : 2048);

        if(_decoyBuffer.Length != length) _decoyBuffer = new byte[length];

        _dev.SendScsiCommand(_cdb, ref _decoyBuffer, TIMEOUT, ScsiDirection.In, out _, out _);
        _previousEnd = Stopwatch.GetTimestamp();
    }

    /// <inheritdoc />
    public bool SetSpeed(ushort kbps)
    {
        bool ok = !_dev.SetCdSpeed(out _, RotationalControl.ClvAndImpureCav, kbps, 0xFFFF, TIMEOUT, out _);

        // The head may have moved, so the next timed read starts afresh
        _previousEnd = 0;

        return ok;
    }

    /// <inheritdoc />
    public bool DisableReadCache()
    {
        if(_dev.ModeSense10(out byte[] buffer, out _, false, ScsiModeSensePageControl.Current, 0x08, TIMEOUT, out _) ||
           buffer is null)
            return false;

        if(buffer.Length < 8) return false;

        int pageOffset = 8 + (buffer[6] << 8 | buffer[7]);

        if(pageOffset + 2 >= buffer.Length || (buffer[pageOffset] & 0x3F) != 0x08) return false;

        // Mode data length is reserved on MODE SELECT, and so is the PS bit
        buffer[0]          =  0;
        buffer[1]          =  0;
        buffer[pageOffset] &= 0x7F;

        var original = (byte[])buffer.Clone();

        buffer[pageOffset + 2] |= 0x01;

        if(_dev.ModeSelect10(buffer, out _, true, false, TIMEOUT, out _)) return false;

        _cachePage = original;

        return true;
    }

    /// <inheritdoc />
    public void RestoreReadCache()
    {
        if(_cachePage is null) return;

        _dev.ModeSelect10(_cachePage, out _, true, false, TIMEOUT, out _);
        _cachePage = null;
    }

    /// <inheritdoc />
    public bool DetectMedium(out DpmMedium medium)
    {
        medium = new DpmMedium
        {
            Kind      = DpmMediumKind.Cd,
            Profile   = -1,
            LastLba   = uint.MaxValue,
            LayerEnds = []
        };

        if(!_dev.GetConfiguration(out byte[] buffer, out _, 0, MmcGetConfigurationRt.Single, TIMEOUT, out _) &&
           buffer?.Length >= 8)
            medium.Profile = buffer[6] << 8 | buffer[7];

        if(!_dev.ReadCapacity(out buffer, out _, TIMEOUT, out _) && buffer?.Length >= 4)
            medium.LastLba = BinaryPrimitives.ReadUInt32BigEndian(buffer);

        switch(medium.Profile)
        {
            // HD DVD is not supported
            case >= 0x50:
                return false;
            case >= 0x40:
                medium.Kind              = DpmMediumKind.Bd;
                medium.OppositeTrackPath = true;

                return DetectBluray(ref medium);
            case >= 0x10:
                medium.Kind = DpmMediumKind.Dvd;

                return DetectDvd(ref medium);
            default:
                DetectAudioTracks();

                return true;
        }
    }

    /// <summary>Reads the layer layout and density of a DVD from its physical format information</summary>
    bool DetectDvd(ref DpmMedium medium)
    {
        if(_dev.ReadDiscStructure(out byte[] buffer,
                                  out _,
                                  MmcDiscStructureMediaType.Dvd,
                                  0,
                                  0,
                                  MmcDiscStructureFormat.PhysicalInformation,
                                  0,
                                  TIMEOUT,
                                  out _) ||
           buffer is not { Length: >= 20 })
            return false;

        ReadOnlySpan<byte> pfi = buffer.AsSpan(4);

        int layers = (pfi[2] >> 5 & 3) + 1;
        medium.OppositeTrackPath = (pfi[2] >> 4 & 1) == 1;

        // Linear density: 0 = 0.267 um/bit (single layer), 1 = 0.293 um/bit (dual layer). A channel bit is half.
        medium.ChannelBit = (pfi[3] >> 4 == 1 ? 0.293e-6 : 0.267e-6) / 2.0;

        if(layers >= 2) medium.LayerEnds = [BinaryPrimitives.ReadUInt32BigEndian(pfi[12..]) - DVD_PSN_BASE];

        return true;
    }

    /// <summary>Reads the layer layout and density of a Blu-ray from its disc information</summary>
    /// <remarks>
    ///     Each layer's data zone is given in address units. Their size in sectors is not reliable across BD-ROM,
    ///     BD-R and BD-RE, so the layers are scaled to the capacity the drive reports. Without disc information
    ///     the layers are taken as the same size.
    /// </remarks>
    bool DetectBluray(ref DpmMedium medium)
    {
        // 74.5 nm channel bit, the 25 GB per layer density, until the disc says otherwise
        medium.ChannelBit = 74.5e-9;

        if(_dev.ReadDiscStructure(out byte[] buffer,
                                  out _,
                                  MmcDiscStructureMediaType.Bd,
                                  0,
                                  0,
                                  MmcDiscStructureFormat.DiscInformation,
                                  0,
                                  TIMEOUT,
                                  out _) ||
           buffer is null)
            return true;

        Decoders.Bluray.DI.DiscInformation? di = Decoders.Bluray.DI.Decode(buffer);

        if(di?.Units is not { Length: > 0 } units || medium.LastLba == uint.MaxValue) return true;

        int layers = units[0].Layers;

        if(layers <= 1) return true;

        var    sizes = new double[layers];
        double total = 0;

        for(int i = 0; i < layers; i++)
        {
            Decoders.Bluray.DI.DiscInformationUnits unit = Array.Find(units, u => u.Layer == i);

            sizes[i] = unit.LastAun > unit.FirstAun ? unit.LastAun - unit.FirstAun + 1.0 : 0;
            total    += sizes[i];
        }

        double cumulative = 0;
        var    ends       = new uint[layers - 1];

        for(int i = 0; i < layers - 1; i++)
        {
            cumulative += total > 0 && Array.TrueForAll(sizes, s => s > 0) ? sizes[i] / total : 1.0 / layers;

            // Layers end on a whole cluster
            ulong end = (ulong)Math.Round(cumulative * (medium.LastLba + 1.0) / 32.0) * 32;
            ends[i] = (uint)end - 1;
        }

        medium.LayerEnds = ends;

        return true;
    }

    /// <summary>Finds the audio tracks of a CD from its table of contents</summary>
    void DetectAudioTracks()
    {
        _audio = [];

        if(_dev.ReadToc(out byte[] buffer, out _, false, 0, TIMEOUT, out _) || buffer is null) return;

        Decoders.CD.TOC.CDTOC? toc = Decoders.CD.TOC.Decode(buffer);

        if(toc?.TrackDescriptors is not { Length: > 0 } descriptors) return;

        Decoders.CD.TOC.CDTOCTrackDataDescriptor[] tracks =
            descriptors.OrderBy(static t => t.TrackStartAddress).ToArray();

        List<(uint first, uint last)> audio = [];

        for(int i = 0; i < tracks.Length - 1; i++)
        {
            // The lead-out is the last descriptor; tracks with the data bit clear are audio
            if(tracks[i].TrackNumber == 0xAA || (tracks[i].CONTROL & 0x04) != 0) continue;

            audio.Add((tracks[i].TrackStartAddress, tracks[i + 1].TrackStartAddress - 1));
        }

        _audio = audio.ToArray();
    }

    /// <summary>Whether a sector is in an audio track</summary>
    bool IsAudio(uint lba)
    {
        foreach((uint first, uint last) in _audio)
            if(lba >= first && lba <= last)
                return true;

        return false;
    }

    /// <summary>Fills a READ CD CDB asking for any sector type and only the main channel user data</summary>
    void FillReadCd(uint lba, ushort blocks)
    {
        Array.Clear(_cdb);
        _cdb[0] = 0xBE;
        BinaryPrimitives.WriteUInt32BigEndian(_cdb.AsSpan(2), lba);
        _cdb[6] = (byte)(blocks >> 16);
        _cdb[7] = (byte)(blocks >> 8);
        _cdb[8] = (byte)blocks;

        // User data only: no sync, header, EDC/ECC, C2 or subchannel
        _cdb[9] = 0x10;
    }

    /// <summary>Fills the READ(12) CDB</summary>
    void FillRead12(uint lba, ushort blocks, bool fua)
    {
        Array.Clear(_cdb);
        _cdb[0] = 0xA8;
        _cdb[1] = (byte)(fua ? 0x08 : 0x00);
        BinaryPrimitives.WriteUInt32BigEndian(_cdb.AsSpan(2), lba);
        BinaryPrimitives.WriteUInt32BigEndian(_cdb.AsSpan(6), blocks);
    }
}