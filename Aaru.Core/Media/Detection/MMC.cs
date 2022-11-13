// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects media types in MultiMediaCommand devices
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Media.Detection;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.Console;
using Aaru.Decoders.Bluray;
using Aaru.Decoders.CD;
using Aaru.Decoders.DVD;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Decoders.Sega;
using Aaru.Devices;
using Aaru.Helpers;
using DMI = Aaru.Decoders.Xbox.DMI;

/// <summary>Detects media type for MMC class devices</summary>
public static class MMC
{
    /// <summary>SHA256 of PlayStation 2 boot sectors, seen in PAL discs</summary>
    const string PS2_PAL_HASH = "5d04ff236613e1d8adcf9c201874acd6f6deed1e04306558b86f91cfb626f39d";

    /// <summary>SHA256 of PlayStation 2 boot sectors, seen in Japanese, American, Malaysian and Korean discs</summary>
    const string PS2_NTSC_HASH = "0bada1426e2c0351b872ef2a9ad2e5a0ac3918f4c53aa53329cb2911a8e16c23";

    /// <summary>SHA256 of PlayStation 2 boot sectors, seen in Japanese discs</summary>
    const string PS2_JAPANESE_HASH = "b82bffb809070d61fe050b7e1545df53d8f3cc648257cdff7502bc0ba6b38870";

    static readonly byte[] _ps3Id =
    {
        0x50, 0x6C, 0x61, 0x79, 0x53, 0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x33, 0x00, 0x00, 0x00, 0x00
    };

    static readonly byte[] _ps4Id =
    {
        0x50, 0x6C, 0x61, 0x79, 0x53, 0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x34, 0x00, 0x00, 0x00, 0x00
    };

    static readonly byte[] _ps5Id =
    {
        0x50, 0x6C, 0x61, 0x79, 0x53, 0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x35, 0x00, 0x00, 0x00, 0x00
    };

    static readonly byte[] _operaId =
    {
        0x01, 0x5A, 0x5A, 0x5A, 0x5A, 0x5A, 0x01
    };

    // Only present on bootable CDs, but those make more than 99% of all available
    static readonly byte[] _fmTownsBootId =
    {
        0x49, 0x50, 0x4C, 0x34, 0xEB, 0x55, 0x06
    };

    /// <summary>Present on first two seconds of second track, says "COPYRIGHT BANDAI"</summary>
    static readonly byte[] _playdiaCopyright =
    {
        0x43, 0x4F, 0x50, 0x59, 0x52, 0x49, 0x47, 0x48, 0x54, 0x20, 0x42, 0x41, 0x4E, 0x44, 0x41, 0x49
    };

    static readonly byte[] _pcEngineSignature =
    {
        0x50, 0x43, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x43, 0x44, 0x2D, 0x52, 0x4F, 0x4D, 0x20, 0x53,
        0x59, 0x53, 0x54, 0x45, 0x4D
    };

    static readonly byte[] _pcFxSignature =
    {
        0x50, 0x43, 0x2D, 0x46, 0x58, 0x3A, 0x48, 0x75, 0x5F, 0x43, 0x44, 0x2D, 0x52, 0x4F, 0x4D
    };

    static readonly byte[] _atariSignature =
    {
        0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41,
        0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52,
        0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41,
        0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x49, 0x52, 0x54, 0x41, 0x52, 0x41, 0x20, 0x49, 0x50, 0x41,
        0x52, 0x50, 0x56, 0x4F, 0x44, 0x45, 0x44, 0x20, 0x54, 0x41, 0x20, 0x41, 0x45, 0x48, 0x44, 0x41, 0x52, 0x45,
        0x41, 0x20, 0x52, 0x54
    };

    /// <summary>This is some kind of header. Every 10 bytes there's an audio byte.</summary>
    static readonly byte[] _videoNowColorFrameMarker =
    {
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3,
        0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00,
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3,
        0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00,
        0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3,
        0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81,
        0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7, 0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x81, 0xE3, 0xE3, 0xC7,
        0xC7, 0x81, 0x81, 0xE3, 0xC7, 0x00, 0x00, 0x00, 0x02, 0x01, 0x04, 0x02, 0x06, 0x03, 0xFF, 0x00, 0x08, 0x04,
        0x0A, 0x05, 0x0C, 0x06, 0x0E, 0x07, 0xFF, 0x00, 0x11, 0x08, 0x13, 0x09, 0x15, 0x0A, 0x17, 0x0B, 0xFF, 0x00,
        0x19, 0x0C, 0x1B, 0x0D, 0x1D, 0x0E, 0x1F, 0x0F, 0xFF, 0x00, 0x00, 0x28, 0x02, 0x29, 0x04, 0x2A, 0x06, 0x2B,
        0xFF, 0x00, 0x08, 0x2C, 0x0A, 0x2D, 0x0C, 0x2E, 0x0E, 0x2F, 0xFF, 0x00, 0x11, 0x30, 0x13, 0x31, 0x15, 0x32,
        0x17, 0x33, 0xFF, 0x00, 0x19, 0x34, 0x1B, 0x35, 0x1D, 0x36, 0x1F, 0x37, 0xFF, 0x00, 0x00, 0x38, 0x02, 0x39,
        0x04, 0x3A, 0x06, 0x3B, 0xFF, 0x00, 0x08, 0x3C, 0x0A, 0x3D, 0x0C, 0x3E, 0x0E, 0x3F, 0xFF, 0x00, 0x11, 0x40,
        0x13, 0x41, 0x15, 0x42, 0x17, 0x43, 0xFF, 0x00, 0x19, 0x44, 0x1B, 0x45, 0x1D, 0x46, 0x1F, 0x47, 0xFF, 0x00,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0x00
    };

    static bool IsData(byte[] sector)
    {
        if(sector?.Length != 2352)
            return false;

        byte[] syncMark =
        {
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
        };

        var testMark = new byte[12];
        Array.Copy(sector, 0, testMark, 0, 12);

        return syncMark.SequenceEqual(testMark) && (sector[0xF] == 0 || sector[0xF] == 1 || sector[0xF] == 2);
    }

    static bool IsScrambledData(byte[] sector, int wantedLba, out int offset)
    {
        offset = 0;

        if(sector?.Length != 2352)
            return false;

        byte[] syncMark =
        {
            0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00
        };

        var testMark = new byte[12];

        for(var i = 0; i <= 2336; i++)
        {
            Array.Copy(sector, i, testMark, 0, 12);

            if(!syncMark.SequenceEqual(testMark) ||
               sector[i + 0xF] != 0x60 && sector[i + 0xF] != 0x61 && sector[i + 0xF] != 0x62)
                continue;

            // De-scramble M and S
            int minute = sector[i + 12] ^ 0x01;
            int second = sector[i + 13] ^ 0x80;
            int frame  = sector[i + 14];

            // Convert to binary
            minute = minute / 16 * 10 + (minute & 0x0F);
            second = second / 16 * 10 + (second & 0x0F);
            frame  = frame  / 16 * 10 + (frame  & 0x0F);

            // Calculate the first found LBA
            int lba = minute * 60 * 75 + second * 75 + frame - 150;

            // Calculate the difference between the found LBA and the requested one
            int diff = wantedLba - lba;

            offset = i + 2352 * diff;

            return true;
        }

        return false;
    }

    static byte[] DescrambleAndFixOffset(byte[] sector, int offsetBytes, int sectorsForOffset)
    {
        var descrambled = new byte[2352];

        int offsetFix = offsetBytes < 0 ? 2352 * sectorsForOffset + offsetBytes : offsetBytes;

        Array.Copy(sector, offsetFix, descrambled, 0, 2352);

        return Sector.Scramble(descrambled);
    }

    /// <summary>Checks if the media corresponds to CD-i.</summary>
    /// <param name="sector0">Contents of LBA 0, with all headers.</param>
    /// <param name="sector16">Contents of LBA 0, with all headers.</param>
    /// <returns><c>true</c> if it corresponds to a CD-i, <c>false</c>otherwise.</returns>
    static bool IsCdi(byte[] sector0, byte[] sector16)
    {
        if(sector16?.Length != 2352)
            return false;

        byte[] cdiMark =
        {
            0x01, 0x43, 0x44, 0x2D
        };

        bool isData = IsData(sector0);

        if(!isData ||
           sector0[0xF] != 2 && sector0[0xF] != 1)
            return false;

        var testMark = new byte[4];
        Array.Copy(sector16, 24, testMark, 0, 4);

        return cdiMark.SequenceEqual(testMark);
    }

    static bool IsVideoNowColor(byte[] videoFrame)
    {
        if(videoFrame is null ||
           videoFrame.Length < _videoNowColorFrameMarker.Length)
            return false;

        var buffer = new byte[_videoNowColorFrameMarker.Length];

        for(var framePosition = 0; framePosition + buffer.Length < videoFrame.Length; framePosition++)
        {
            Array.Copy(videoFrame, framePosition, buffer, 0, buffer.Length);

            for(var ab = 9; ab < buffer.Length; ab += 10)
                buffer[ab] = 0;

            if(!_videoNowColorFrameMarker.SequenceEqual(buffer))
                continue;

            return true;
        }

        return false;
    }

    internal static int GetVideoNowColorOffset(byte[] data)
    {
        var buffer = new byte[_videoNowColorFrameMarker.Length];

        for(var framePosition = 0; framePosition + buffer.Length < data.Length; framePosition++)
        {
            Array.Copy(data, framePosition, buffer, 0, buffer.Length);

            for(var ab = 9; ab < buffer.Length; ab += 10)
                buffer[ab] = 0;

            if(!_videoNowColorFrameMarker.SequenceEqual(buffer))
                continue;

            return 18032 - framePosition;
        }

        return 0;
    }

    internal static void DetectDiscType(ref MediaType mediaType, int sessions, FullTOC.CDFullTOC? decodedToc,
                                        Device dev, out bool hiddenTrack, out bool hiddenData,
                                        int firstTrackLastSession, ulong blocks)
    {
        uint                startOfFirstDataTrack = uint.MaxValue;
        DI.DiscInformation? blurayDi              = null;
        byte[]              cmdBuf;
        bool                sense;
        byte                secondSessionFirstTrack = 0;
        byte[]              sector0;
        byte[]              sector1                      = null;
        byte[]              ps2BootSectors               = null;
        byte[]              playdia1                     = null;
        byte[]              playdia2                     = null;
        byte[]              firstDataSectorNotZero       = null;
        byte[]              secondDataSectorNotZero      = null;
        byte[]              firstTrackSecondSession      = null;
        byte[]              firstTrackSecondSessionAudio = null;
        byte[]              videoNowColorFrame;
        hiddenTrack = false;
        hiddenData  = false;

        sense = dev.GetConfiguration(out cmdBuf, out _, 0, MmcGetConfigurationRt.Current, dev.Timeout, out _);

        if(!sense)
        {
            Features.SeparatedFeatures ftr = Features.Separate(cmdBuf);

            AaruConsole.DebugWriteLine("Media-Info command", "GET CONFIGURATION current profile is {0:X4}h",
                                       ftr.CurrentProfile);

            switch(ftr.CurrentProfile)
            {
                case 0x0001:
                    mediaType = MediaType.GENERIC_HDD;

                    break;
                case 0x0005:
                    mediaType = MediaType.CDMO;

                    break;
                case 0x0008:
                    mediaType = MediaType.CD;

                    break;
                case 0x0009:
                    mediaType = MediaType.CDR;

                    break;
                case 0x000A:
                    mediaType = MediaType.CDRW;

                    break;
                case 0x0010:
                    mediaType = MediaType.DVDROM;

                    break;
                case 0x0011:
                    mediaType = MediaType.DVDR;

                    break;
                case 0x0012:
                    mediaType = MediaType.DVDRAM;

                    break;
                case 0x0013:
                case 0x0014:
                    mediaType = MediaType.DVDRW;

                    break;
                case 0x0015:
                case 0x0016:
                    mediaType = MediaType.DVDRDL;

                    break;
                case 0x0017:
                    mediaType = MediaType.DVDRWDL;

                    break;
                case 0x0018:
                    mediaType = MediaType.DVDDownload;

                    break;
                case 0x001A:
                    mediaType = MediaType.DVDPRW;

                    break;
                case 0x001B:
                    mediaType = MediaType.DVDPR;

                    break;
                case 0x0020:
                    mediaType = MediaType.DDCD;

                    break;
                case 0x0021:
                    mediaType = MediaType.DDCDR;

                    break;
                case 0x0022:
                    mediaType = MediaType.DDCDRW;

                    break;
                case 0x002A:
                    mediaType = MediaType.DVDPRWDL;

                    break;
                case 0x002B:
                    mediaType = MediaType.DVDPRDL;

                    break;
                case 0x0040:
                    mediaType = MediaType.BDROM;

                    break;
                case 0x0041:
                case 0x0042:
                    mediaType = MediaType.BDR;

                    break;
                case 0x0043:
                    mediaType = MediaType.BDRE;

                    break;
                case 0x0050:
                    mediaType = MediaType.HDDVDROM;

                    break;
                case 0x0051:
                    mediaType = MediaType.HDDVDR;

                    break;
                case 0x0052:
                    mediaType = MediaType.HDDVDRAM;

                    break;
                case 0x0053:
                    mediaType = MediaType.HDDVDRW;

                    break;
                case 0x0058:
                    mediaType = MediaType.HDDVDRDL;

                    break;
                case 0x005A:
                    mediaType = MediaType.HDDVDRWDL;

                    break;
            }
        }

        if(decodedToc?.TrackDescriptors.Any(t => t.SessionNumber == 2) == true)
            secondSessionFirstTrack = decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == 2).
                                                 Min(t => t.POINT);

        if(mediaType is MediaType.CD or MediaType.CDROMXA or MediaType.CDI)
        {
            sense = dev.ReadAtip(out cmdBuf, out _, dev.Timeout, out _);

            if(!sense)
            {
                ATIP.CDATIP atip = ATIP.Decode(cmdBuf);

                if(atip != null)

                    // Only CD-R and CD-RW have ATIP
                    mediaType = atip.DiscType ? MediaType.CDRW : MediaType.CDR;
            }
        }

        if(mediaType is MediaType.CD or MediaType.CDROMXA)
        {
            var hasDataTrack                  = false;
            var hasAudioTrack                 = false;
            var allFirstSessionTracksAreAudio = true;
            var hasVideoTrack                 = false;

            if(decodedToc.HasValue)
            {
                FullTOC.TrackDataDescriptor a0Track =
                    decodedToc.Value.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA0 && t.ADR == 1);

                if(a0Track.POINT == 0xA0)
                    switch(a0Track.PSEC)
                    {
                        case 0x10:
                            AaruConsole.DebugWriteLine("Media detection", "TOC says disc type is CD-i.");
                            mediaType = MediaType.CDI;

                            break;
                        case 0x20:
                            AaruConsole.DebugWriteLine("Media detection", "TOC says disc type is CD-ROM XA.");
                            mediaType = MediaType.CDROMXA;

                            break;
                    }

                foreach(FullTOC.TrackDataDescriptor track in
                        decodedToc.Value.TrackDescriptors.Where(t => t.POINT > 0 && t.POINT <= 0x99))
                {
                    if(track.TNO == 1 &&
                       ((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                        (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental))
                        allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;

                    if((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                       (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                    {
                        var startAddress = (uint)(track.PHOUR * 3600 * 75 + track.PMIN * 60 * 75 + track.PSEC * 75 +
                                                  track.PFRAME - 150);

                        if(startAddress < startOfFirstDataTrack)
                            startOfFirstDataTrack = startAddress;

                        hasDataTrack                  =  true;
                        allFirstSessionTracksAreAudio &= track.POINT >= firstTrackLastSession;
                    }
                    else
                        hasAudioTrack = true;

                    hasVideoTrack |= track.ADR == 4;
                }
            }

            if(mediaType != MediaType.CDI)
            {
                switch(hasDataTrack)
                {
                    case true when hasAudioTrack && allFirstSessionTracksAreAudio && sessions == 2:
                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Disc has audio and data tracks, two sessions, and all data tracks are in second session, setting as CD+.");

                        mediaType = MediaType.CDPLUS;

                        break;
                    case false when hasAudioTrack && sessions == 1:
                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Disc has only audio tracks in a single session, setting as CD Digital Audio.");

                        mediaType = MediaType.CDDA;

                        break;
                }

                if(hasDataTrack   &&
                   !hasAudioTrack &&
                   sessions == 1)
                {
                    AaruConsole.DebugWriteLine("Media detection",
                                               "Disc has only data tracks in a single session, setting as CD-ROM.");

                    mediaType = MediaType.CDROM;
                }

                if(hasVideoTrack &&
                   !hasDataTrack &&
                   sessions == 1)
                {
                    AaruConsole.DebugWriteLine("Media detection",
                                               "Disc has video tracks in a single session, setting as CD Video.");

                    mediaType = MediaType.CDV;
                }
            }

            if(mediaType is MediaType.CD or MediaType.CDROM && hasDataTrack)
                foreach(uint startAddress in decodedToc.Value.TrackDescriptors.
                                                        Where(t => t.POINT > 0 && t.POINT <= 0x99 &&
                                                                   ((TocControl)(t.CONTROL & 0x0D) ==
                                                                    TocControl.DataTrack ||
                                                                    (TocControl)(t.CONTROL & 0x0D) ==
                                                                    TocControl.DataTrackIncremental)).
                                                        Select(track => (uint)(track.PHOUR * 3600 * 75 +
                                                                               track.PMIN  * 60 * 75 + track.PSEC * 75 +
                                                                               track.PFRAME - 150) + 16))
                {
                    sense = dev.ReadCd(out cmdBuf, out _, startAddress, 2352, 1, MmcSectorTypes.AllTypes, false, false,
                                       true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                       MmcSubchannel.None, dev.Timeout, out _);

                    if(sense || dev.Error)
                        continue;

                    if(cmdBuf[0]  != 0x00 ||
                       cmdBuf[1]  != 0xFF ||
                       cmdBuf[2]  != 0xFF ||
                       cmdBuf[3]  != 0xFF ||
                       cmdBuf[4]  != 0xFF ||
                       cmdBuf[5]  != 0xFF ||
                       cmdBuf[6]  != 0xFF ||
                       cmdBuf[7]  != 0xFF ||
                       cmdBuf[8]  != 0xFF ||
                       cmdBuf[9]  != 0xFF ||
                       cmdBuf[10] != 0xFF ||
                       cmdBuf[11] != 0x00 ||
                       cmdBuf[15] != 0x02)
                        continue;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Disc has a mode 2 data track, setting as CD-ROM XA.");

                    mediaType = MediaType.CDROMXA;

                    break;
                }
        }

        if(secondSessionFirstTrack                                                   != 0 &&
           decodedToc?.TrackDescriptors.Any(t => t.POINT == secondSessionFirstTrack) == true)
        {
            FullTOC.TrackDataDescriptor secondSessionFirstTrackTrack =
                decodedToc.Value.TrackDescriptors.First(t => t.POINT == secondSessionFirstTrack);

            var firstSectorSecondSessionFirstTrack = (uint)(secondSessionFirstTrackTrack.PHOUR * 3600 * 75 +
                                                            secondSessionFirstTrackTrack.PMIN  * 60   * 75 +
                                                            secondSessionFirstTrackTrack.PSEC  * 75        +
                                                            secondSessionFirstTrackTrack.PFRAME - 150);

            sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack, 2352, 1, MmcSectorTypes.AllTypes,
                               false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                               MmcSubchannel.None, dev.Timeout, out _);

            if(!sense &&
               !dev.Error)
                firstTrackSecondSession = cmdBuf;
            else
            {
                sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack, 2352, 1, MmcSectorTypes.Cdda,
                                   false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                   MmcSubchannel.None, dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                    firstTrackSecondSession = cmdBuf;
            }

            sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack - 1, 2352, 3,
                               MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

            if(!sense &&
               !dev.Error)
                firstTrackSecondSessionAudio = cmdBuf;
            else
            {
                sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack - 1, 2352, 3,
                                   MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true, false,
                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                    firstTrackSecondSessionAudio = cmdBuf;
            }
        }

        videoNowColorFrame = new byte[9 * 2352];

        for(var i = 0; i < 9; i++)
        {
            sense = dev.ReadCd(out cmdBuf, out _, (uint)i, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                               dev.Timeout, out _);

            if(sense || dev.Error)
            {
                sense = dev.ReadCd(out cmdBuf, out _, (uint)i, 2352, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                   dev.Timeout, out _);

                if(sense || !dev.Error)
                {
                    videoNowColorFrame = null;

                    break;
                }
            }

            Array.Copy(cmdBuf, 0, videoNowColorFrame, i * 2352, 2352);
        }

        FullTOC.TrackDataDescriptor? firstTrack =
            decodedToc?.TrackDescriptors.FirstOrDefault(t => t.POINT ==
                                                             decodedToc.Value.TrackDescriptors.Min(m => m.POINT));

        if(firstTrack?.POINT is >= 1 and < 0xA0)
        {
            var firstTrackSector = (uint)(firstTrack.Value.PHOUR * 3600 * 75 + firstTrack.Value.PMIN * 60 * 75 +
                                          firstTrack.Value.PSEC  * 75        + firstTrack.Value.PFRAME - 150);

            // Check for hidden data before start of track 1
            if(firstTrackSector > 0)
            {
                sense = dev.ReadCd(out sector0, out _, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                   dev.Timeout, out _);

                if(!dev.Error &&
                   !sense)
                {
                    hiddenTrack = true;

                    hiddenData = IsData(sector0);

                    if(hiddenData)
                    {
                        sense = dev.ReadCd(out byte[] sector16, out _, 16, 2352, 1, MmcSectorTypes.AllTypes, false,
                                           false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           IsCdi(sector0, sector16))
                        {
                            mediaType = MediaType.CDIREADY;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Disc has a hidden CD-i track in track 1's pregap, setting as CD-i Ready.");

                            return;
                        }
                    }
                    else
                    {
                        hiddenData = IsScrambledData(sector0, 0, out int combinedOffset);

                        if(hiddenData)
                        {
                            int sectorsForOffset = combinedOffset / 2352;

                            if(sectorsForOffset < 0)
                                sectorsForOffset *= -1;

                            if(combinedOffset % 2352 != 0)
                                sectorsForOffset++;

                            var lba0  = 0;
                            var lba16 = 16;

                            if(combinedOffset < 0)
                            {
                                lba0  -= sectorsForOffset;
                                lba16 -= sectorsForOffset;
                            }

                            sense = dev.ReadCd(out sector0, out _, (uint)lba0, 2352, (uint)sectorsForOffset + 1,
                                               MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                               true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            // Drive does not support reading negative sectors?
                            if(sense && lba0 < 0)
                            {
                                dev.ReadCd(out sector0, out _, 0, 2352, 2, MmcSectorTypes.AllTypes, false, false, true,
                                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                                sector0 = DescrambleAndFixOffset(sector0, combinedOffset, sectorsForOffset);
                            }
                            else
                                sector0 = DescrambleAndFixOffset(sector0, combinedOffset, sectorsForOffset);

                            dev.ReadCd(out byte[] sector16, out _, (uint)lba16, 2352, (uint)sectorsForOffset + 1,
                                       MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                       true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            sector16 = DescrambleAndFixOffset(sector16, combinedOffset, sectorsForOffset);

                            if(IsCdi(sector0, sector16))
                            {
                                mediaType = MediaType.CDIREADY;

                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Disc has a hidden CD-i track in track 1's pregap, setting as CD-i Ready.");

                                return;
                            }
                        }
                    }
                }
            }
        }

        sector0 = null;

        switch(mediaType)
        {
            case MediaType.CD:
            case MediaType.CDDA:
            case MediaType.CDPLUS:
            case MediaType.CDROM:
            case MediaType.CDROMXA:
            {
                sense = dev.ReadCd(out cmdBuf, out _, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                   dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                {
                    sector0 = new byte[2048];
                    Array.Copy(cmdBuf, 16, sector0, 0, 2048);

                    sense = dev.ReadCd(out cmdBuf, out _, 1, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                       MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        sector1 = new byte[2048];
                        Array.Copy(cmdBuf, 16, sector1, 0, 2048);
                    }

                    sense = dev.ReadCd(out cmdBuf, out _, 4200, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                       MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        playdia1 = new byte[2048];
                        Array.Copy(cmdBuf, 24, playdia1, 0, 2048);
                    }

                    sense = dev.ReadCd(out cmdBuf, out _, 4201, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                       MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        playdia2 = new byte[2048];
                        Array.Copy(cmdBuf, 24, playdia2, 0, 2048);
                    }

                    if(startOfFirstDataTrack != uint.MaxValue)
                    {
                        sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack, 2352, 1, MmcSectorTypes.AllTypes,
                                           false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                           MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            firstDataSectorNotZero = new byte[2048];
                            Array.Copy(cmdBuf, 16, firstDataSectorNotZero, 0, 2048);
                        }

                        sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack + 1, 2352, 1,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                           true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            secondDataSectorNotZero = new byte[2048];
                            Array.Copy(cmdBuf, 16, secondDataSectorNotZero, 0, 2048);
                        }
                    }

                    var ps2Ms = new MemoryStream();

                    for(uint p = 0; p < 12; p++)
                    {
                        sense = dev.ReadCd(out cmdBuf, out _, p, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(sense || dev.Error)
                            break;

                        ps2Ms.Write(cmdBuf, cmdBuf[0x0F] == 0x02 ? 24 : 16, 2048);
                    }

                    if(ps2Ms.Length == 0x6000)
                        ps2BootSectors = ps2Ms.ToArray();
                }
                else
                {
                    sense = dev.ReadCd(out cmdBuf, out _, 0, 2324, 1, MmcSectorTypes.Mode2, false, false, false,
                                       MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        sector0 = new byte[2048];
                        Array.Copy(cmdBuf, 0, sector0, 0, 2048);

                        sense = dev.ReadCd(out cmdBuf, out _, 1, 2324, 1, MmcSectorTypes.Mode2, false, false, false,
                                           MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            sector1 = new byte[2048];
                            Array.Copy(cmdBuf, 1, sector0, 0, 2048);
                        }

                        sense = dev.ReadCd(out cmdBuf, out _, 4200, 2324, 1, MmcSectorTypes.Mode2, false, false, false,
                                           MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            playdia1 = new byte[2048];
                            Array.Copy(cmdBuf, 0, playdia1, 0, 2048);
                        }

                        sense = dev.ReadCd(out cmdBuf, out _, 4201, 2324, 1, MmcSectorTypes.Mode2, false, false, false,
                                           MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            playdia2 = new byte[2048];
                            Array.Copy(cmdBuf, 0, playdia2, 0, 2048);
                        }

                        if(startOfFirstDataTrack != uint.MaxValue)
                        {
                            sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack, 2324, 1, MmcSectorTypes.Mode2,
                                               false, false, false, MmcHeaderCodes.None, true, false,
                                               MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                firstDataSectorNotZero = new byte[2048];
                                Array.Copy(cmdBuf, 0, firstDataSectorNotZero, 0, 2048);
                            }

                            sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack + 1, 2324, 1,
                                               MmcSectorTypes.Mode2, false, false, false, MmcHeaderCodes.None, true,
                                               false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                secondDataSectorNotZero = new byte[2048];
                                Array.Copy(cmdBuf, 0, secondDataSectorNotZero, 0, 2048);
                            }
                        }

                        var ps2Ms = new MemoryStream();

                        for(uint p = 0; p < 12; p++)
                        {
                            sense = dev.ReadCd(out cmdBuf, out _, p, 2324, 1, MmcSectorTypes.Mode2, false, false, false,
                                               MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                               dev.Timeout, out _);

                            if(sense || dev.Error)
                                break;

                            ps2Ms.Write(cmdBuf, 0, 2048);
                        }

                        if(ps2Ms.Length == 0x6000)
                            ps2BootSectors = ps2Ms.ToArray();
                    }
                    else
                    {
                        sense = dev.ReadCd(out cmdBuf, out _, 0, 2048, 1, MmcSectorTypes.Mode1, false, false, false,
                                           MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            sector0 = cmdBuf;

                            sense = dev.ReadCd(out cmdBuf, out _, 0, 2048, 1, MmcSectorTypes.Mode1, false, false, false,
                                               MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None,
                                               dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                                sector1 = cmdBuf;

                            sense = dev.ReadCd(out cmdBuf, out _, 0, 2048, 12, MmcSectorTypes.Mode1, false, false,
                                               false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                               MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                                ps2BootSectors = cmdBuf;

                            if(startOfFirstDataTrack != uint.MaxValue)
                            {
                                sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack, 2048, 1,
                                                   MmcSectorTypes.Mode1, false, false, false, MmcHeaderCodes.None, true,
                                                   false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                                if(!sense &&
                                   !dev.Error)
                                    firstDataSectorNotZero = cmdBuf;

                                sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack + 1, 2048, 1,
                                                   MmcSectorTypes.Mode1, false, false, false, MmcHeaderCodes.None, true,
                                                   false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                                if(!sense &&
                                   !dev.Error)
                                    secondDataSectorNotZero = cmdBuf;
                            }
                        }
                        else
                            goto case MediaType.DVDROM;
                    }
                }

                break;
            }

            // TODO: Check for CD-i Ready
            case MediaType.CDI: break;
            case MediaType.DVDROM:
            case MediaType.HDDVDROM:
            case MediaType.BDROM:
            case MediaType.UHDBD:
            case MediaType.Unknown:
                if(mediaType is MediaType.BDROM or MediaType.UHDBD)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Bd, 0, 0,
                                                  MmcDiscStructureFormat.DiscInformation, 0, dev.Timeout, out _);

                    if(!sense)
                        blurayDi = DI.Decode(cmdBuf);
                }

                sense = dev.Read16(out cmdBuf, out _, 0, false, false, false, 0, 2048, 0, 1, false, dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                {
                    sector0 = cmdBuf;

                    sense = dev.Read16(out cmdBuf, out _, 0, false, false, false, 1, 2048, 0, 1, false, dev.Timeout,
                                       out _);

                    if(!sense &&
                       !dev.Error)
                        sector1 = cmdBuf;

                    sense = dev.Read16(out cmdBuf, out _, 0, false, false, false, 0, 2048, 0, 12, false, dev.Timeout,
                                       out _);

                    if(!sense     &&
                       !dev.Error &&
                       cmdBuf.Length == 0x6000)
                        ps2BootSectors = cmdBuf;
                }
                else
                {
                    sense = dev.Read12(out cmdBuf, out _, 0, false, false, false, false, 0, 2048, 0, 1, false,
                                       dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        sector0 = cmdBuf;

                        sense = dev.Read12(out cmdBuf, out _, 0, false, false, false, false, 1, 2048, 0, 1, false,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                            sector1 = cmdBuf;

                        sense = dev.Read12(out cmdBuf, out _, 0, false, false, false, false, 0, 2048, 0, 12, false,
                                           dev.Timeout, out _);

                        if(!sense     &&
                           !dev.Error &&
                           cmdBuf.Length == 0x6000)
                            ps2BootSectors = cmdBuf;
                    }
                    else
                    {
                        sense = dev.Read10(out cmdBuf, out _, 0, false, false, false, false, 0, 2048, 0, 1, dev.Timeout,
                                           out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            sector0 = cmdBuf;

                            sense = dev.Read10(out cmdBuf, out _, 0, false, false, false, false, 1, 2048, 0, 1,
                                               dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                                sector1 = cmdBuf;

                            sense = dev.Read10(out cmdBuf, out _, 0, false, false, false, false, 0, 2048, 0, 12,
                                               dev.Timeout, out _);

                            if(!sense     &&
                               !dev.Error &&
                               cmdBuf.Length == 0x6000)
                                ps2BootSectors = cmdBuf;
                        }
                        else
                        {
                            sense = dev.Read6(out cmdBuf, out _, 0, 2048, 1, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                sector0 = cmdBuf;

                                sense = dev.Read6(out cmdBuf, out _, 1, 2048, 1, dev.Timeout, out _);

                                if(!sense &&
                                   !dev.Error)
                                    sector1 = cmdBuf;

                                sense = dev.Read6(out cmdBuf, out _, 0, 2048, 12, dev.Timeout, out _);

                                if(!sense     &&
                                   !dev.Error &&
                                   cmdBuf.Length == 0x6000)
                                    ps2BootSectors = cmdBuf;
                            }
                        }
                    }
                }

                if(mediaType == MediaType.DVDROM)
                {
                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.PhysicalInformation, 0, dev.Timeout, out _);

                    if(!sense)
                    {
                        PFI.PhysicalFormatInformation? pfi = PFI.Decode(cmdBuf, mediaType);

                        if(pfi != null)
                            mediaType = pfi.Value.DiskCategory switch
                                        {
                                            DiskCategory.DVDPR    => MediaType.DVDPR,
                                            DiskCategory.DVDPRDL  => MediaType.DVDPRDL,
                                            DiskCategory.DVDPRW   => MediaType.DVDPRW,
                                            DiskCategory.DVDPRWDL => MediaType.DVDPRWDL,
                                            DiskCategory.DVDR => pfi.Value.PartVersion >= 6 ? MediaType.DVDRDL
                                                                     : MediaType.DVDR,
                                            DiskCategory.DVDRAM => MediaType.DVDRAM,
                                            DiskCategory.DVDRW => pfi.Value.PartVersion >= 15 ? MediaType.DVDRWDL
                                                                      : MediaType.DVDRW,
                                            DiskCategory.HDDVDR   => MediaType.HDDVDR,
                                            DiskCategory.HDDVDRAM => MediaType.HDDVDRAM,
                                            DiskCategory.HDDVDROM => MediaType.HDDVDROM,
                                            DiskCategory.HDDVDRW  => MediaType.HDDVDRW,
                                            DiskCategory.Nintendo => pfi.Value.DiscSize == DVDSize.Eighty
                                                                         ? MediaType.GOD : MediaType.WOD,
                                            DiskCategory.UMD => MediaType.UMD,
                                            _                => mediaType
                                        };
                    }

                    sense = dev.ReadDiscStructure(out cmdBuf, out _, MmcDiscStructureMediaType.Dvd, 0, 0,
                                                  MmcDiscStructureFormat.DiscManufacturingInformation, 0, dev.Timeout,
                                                  out _);

                    if(!sense)
                    {
                        if(DMI.IsXbox(cmdBuf))
                        {
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found Xbox DMI, setting disc type to Xbox Game Disc (XGD).");

                            mediaType = MediaType.XGD;

                            return;
                        }

                        if(DMI.IsXbox360(cmdBuf))
                        {
                            // All XGD3 all have the same number of blocks
                            if(blocks is 25063 or 4229664 or 4246304) // Wxripper unlock
                            {
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Xbox 360 DMI with {0} blocks, setting disc type to Xbox 360 Game Disc 3 (XGD3).");

                                mediaType = MediaType.XGD3;

                                return;
                            }

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found Xbox 360 DMI with {0} blocks, setting disc type to Xbox 360 Game Disc 2 (XGD2).");

                            mediaType = MediaType.XGD2;

                            return;
                        }
                    }
                }

                break;

            // Recordables will be checked for PhotoCD only
            case MediaType.CDR:
                // Check if ISO9660
                sense = dev.Read12(out byte[] isoSector, out _, 0, false, false, false, false, 16, 2048, 0, 1, false,
                                   dev.Timeout, out _);

                // Sector 16 reads, and contains "CD001" magic?
                if(sense                ||
                   isoSector[1] != 0x43 ||
                   isoSector[2] != 0x44 ||
                   isoSector[3] != 0x30 ||
                   isoSector[4] != 0x30 ||
                   isoSector[5] != 0x31)
                    return;

                // From sectors 16 to 31
                uint isoSectorPosition = 16;

                while(isoSectorPosition < 32)
                {
                    sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, isoSectorPosition, 2048, 0,
                                       1, false, dev.Timeout, out _);

                    // If sector cannot be read, break here
                    if(sense)
                        break;

                    // If sector does not contain "CD001" magic, break
                    if(isoSector[1] != 0x43 ||
                       isoSector[2] != 0x44 ||
                       isoSector[3] != 0x30 ||
                       isoSector[4] != 0x30 ||
                       isoSector[5] != 0x31)
                        break;

                    // If it is PVD or end of descriptor chain, break
                    if(isoSector[0] == 1 ||
                       isoSector[0] == 255)
                        break;

                    isoSectorPosition++;
                }

                // If it's not an ISO9660 PVD, return
                if(isoSector[0] != 1    ||
                   isoSector[1] != 0x43 ||
                   isoSector[2] != 0x44 ||
                   isoSector[3] != 0x30 ||
                   isoSector[4] != 0x30 ||
                   isoSector[5] != 0x31)
                    return;

                var rootStart  = BitConverter.ToUInt32(isoSector, 158);
                var rootLength = BitConverter.ToUInt32(isoSector, 166);

                if(rootStart  == 0 ||
                   rootLength == 0)
                    return;

                rootLength /= 2048;

                try
                {
                    using var rootMs = new MemoryStream();

                    for(uint i = 0; i < rootLength; i++)
                    {
                        sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, rootStart + i, 2048, 0,
                                           1, false, dev.Timeout, out _);

                        if(sense)
                            break;

                        rootMs.Write(isoSector, 0, 2048);
                    }

                    isoSector = rootMs.ToArray();
                }
                catch
                {
                    return;
                }

                if(isoSector.Length < 2048)
                    return;

                var  rootPos   = 0;
                uint pcdStart  = 0;
                uint pcdLength = 0;

                while(isoSector[rootPos]           > 0                &&
                      rootPos                      < isoSector.Length &&
                      rootPos + isoSector[rootPos] <= isoSector.Length)
                {
                    int nameLen = isoSector[rootPos + 32];
                    var tmpName = new byte[nameLen];
                    Array.Copy(isoSector, rootPos + 33, tmpName, 0, nameLen);
                    string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                    if(name.EndsWith(";1", StringComparison.InvariantCulture))
                        name = name.Substring(0, name.Length - 2);

                    if(name                             == "PHOTO_CD" &&
                       (isoSector[rootPos + 25] & 0x02) == 0x02)
                    {
                        pcdStart  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                        pcdLength = BitConverter.ToUInt32(isoSector, rootPos + 10) / 2048;
                    }

                    rootPos += isoSector[rootPos];
                }

                if(pcdLength > 0)
                {
                    try
                    {
                        using var pcdMs = new MemoryStream();

                        for(uint i = 0; i < pcdLength; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, pcdStart + i, 2048,
                                               0, 1, false, dev.Timeout, out _);

                            if(sense)
                                break;

                            pcdMs.Write(isoSector, 0, 2048);
                        }

                        isoSector = pcdMs.ToArray();
                    }
                    catch
                    {
                        return;
                    }

                    if(isoSector.Length < 2048)
                        return;

                    for(var pi = 0; pi < pcdLength; pi++)
                    {
                        int  pcdPos  = pi * 2048;
                        uint infoPos = 0;

                        while(isoSector[pcdPos]          > 0                &&
                              pcdPos                     < isoSector.Length &&
                              pcdPos + isoSector[pcdPos] <= isoSector.Length)
                        {
                            int nameLen = isoSector[pcdPos + 32];
                            var tmpName = new byte[nameLen];
                            Array.Copy(isoSector, pcdPos + 33, tmpName, 0, nameLen);
                            string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                            if(name.EndsWith(";1", StringComparison.InvariantCulture))
                                name = name.Substring(0, name.Length - 2);

                            if(name == "INFO.PCD")
                            {
                                infoPos = BitConverter.ToUInt32(isoSector, pcdPos + 2);

                                break;
                            }

                            pcdPos += isoSector[pcdPos];
                        }

                        if(infoPos > 0)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, infoPos, 2048, 0, 1,
                                               false, dev.Timeout, out _);

                            if(sense)
                                break;

                            var systemId = new byte[8];
                            Array.Copy(isoSector, 0, systemId, 0, 8);

                            string id = StringHandlers.CToString(systemId).TrimEnd();

                            switch(id)
                            {
                                case "PHOTO_CD":
                                    mediaType = MediaType.PCD;

                                    AaruConsole.DebugWriteLine("Media detection",
                                                               "Found Photo CD description file, setting disc type to Photo CD.");

                                    return;
                            }
                        }
                    }
                }

                break;

            // Other recordables will not be checked
            case MediaType.CDRW:
            case MediaType.CDMRW:
            case MediaType.DDCDR:
            case MediaType.DDCDRW:
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
            case MediaType.HDDVDRAM:
            case MediaType.HDDVDR:
            case MediaType.HDDVDRW:
            case MediaType.HDDVDRDL:
            case MediaType.HDDVDRWDL:
            case MediaType.BDR:
            case MediaType.BDRE:
            case MediaType.BDRXL:
            case MediaType.BDREXL: return;
        }

        if(sector0 == null)
            return;

        switch(mediaType)
        {
            case MediaType.CD:
            case MediaType.CDDA:
            case MediaType.CDPLUS:
            case MediaType.CDROM:
            case MediaType.CDROMXA:
                // TODO: Pippin requires interpreting Apple Partition Map, reading HFS and checking for Pippin signatures
            {
                if(CD.DecodeIPBin(sector0).HasValue)
                {
                    mediaType = MediaType.MEGACD;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found Mega/Sega CD IP.BIN, setting disc type to Mega CD.");

                    return;
                }

                if(Saturn.DecodeIPBin(sector0).HasValue)
                {
                    mediaType = MediaType.SATURNCD;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found Sega Saturn IP.BIN, setting disc type to Saturn CD.");

                    return;
                }

                // Are GDR detectable ???
                if(Dreamcast.DecodeIPBin(sector0).HasValue)
                {
                    mediaType = MediaType.GDROM;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found Sega Dreamcast IP.BIN, setting disc type to GD-ROM.");

                    return;
                }

                if(ps2BootSectors is { Length: 0x6000 })
                {
                    // The decryption key is applied as XOR. As first byte is originally always NULL, it gives us the key :)
                    byte decryptByte = ps2BootSectors[0];

                    for(var i = 0; i < 0x6000; i++)
                        ps2BootSectors[i] ^= decryptByte;

                    string ps2BootSectorsHash = Sha256Context.Data(ps2BootSectors, out _);

                    AaruConsole.DebugWriteLine("Media-info Command", "PlayStation 2 boot sectors SHA256: {0}",
                                               ps2BootSectorsHash);

                    if(ps2BootSectorsHash is PS2_PAL_HASH or PS2_NTSC_HASH or PS2_JAPANESE_HASH)
                    {
                        mediaType = MediaType.PS2CD;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Sony PlayStation 2 boot sectors, setting disc type to PS2 CD.");

                        goto hasPs2CdBoot;
                    }
                }

                if(sector0 != null)
                {
                    var syncBytes = new byte[7];
                    Array.Copy(sector0, 0, syncBytes, 0, 7);

                    if(_operaId.SequenceEqual(syncBytes))
                    {
                        mediaType = MediaType.ThreeDO;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Opera filesystem, setting disc type to 3DO.");

                        return;
                    }

                    if(_fmTownsBootId.SequenceEqual(syncBytes))
                    {
                        mediaType = MediaType.FMTOWNS;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found FM-Towns boot, setting disc type to FM-Towns.");

                        return;
                    }
                }

                if(playdia1 != null &&
                   playdia2 != null)
                {
                    var pd1 = new byte[_playdiaCopyright.Length];
                    var pd2 = new byte[_playdiaCopyright.Length];

                    Array.Copy(playdia1, 38, pd1, 0, pd1.Length);
                    Array.Copy(playdia2, 0, pd2, 0, pd1.Length);

                    if(_playdiaCopyright.SequenceEqual(pd1) &&
                       _playdiaCopyright.SequenceEqual(pd2))
                    {
                        mediaType = MediaType.Playdia;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Playdia copyright, setting disc type to Playdia.");

                        return;
                    }
                }

                if(secondDataSectorNotZero != null)
                {
                    var pce = new byte[_pcEngineSignature.Length];
                    Array.Copy(secondDataSectorNotZero, 32, pce, 0, pce.Length);

                    if(_pcEngineSignature.SequenceEqual(pce))
                    {
                        mediaType = MediaType.SuperCDROM2;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found PC-Engine CD signature, setting disc type to Super CD-ROM².");

                        return;
                    }
                }

                if(firstDataSectorNotZero != null)
                {
                    var pcfx = new byte[_pcFxSignature.Length];
                    Array.Copy(firstDataSectorNotZero, 0, pcfx, 0, pcfx.Length);

                    if(_pcFxSignature.SequenceEqual(pcfx))
                    {
                        mediaType = MediaType.PCFX;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found PC-FX copyright, setting disc type to PC-FX.");

                        return;
                    }
                }

                if(firstTrackSecondSessionAudio != null)
                {
                    var jaguar = new byte[_atariSignature.Length];

                    for(var i = 0; i + jaguar.Length <= firstTrackSecondSessionAudio.Length; i += 2)
                    {
                        Array.Copy(firstTrackSecondSessionAudio, i, jaguar, 0, jaguar.Length);

                        if(!_atariSignature.SequenceEqual(jaguar))
                            continue;

                        mediaType = MediaType.JaguarCD;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Atari signature, setting disc type to Jaguar CD.");

                        break;
                    }
                }

                if(firstTrackSecondSession?.Length >= 2336)
                {
                    var milcd = new byte[2048];
                    Array.Copy(firstTrackSecondSession, 24, milcd, 0, 2048);

                    if(Dreamcast.DecodeIPBin(milcd).HasValue)
                    {
                        mediaType = MediaType.MilCD;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Sega Dreamcast IP.BIN on second session, setting disc type to MilCD.");

                        return;
                    }
                }

                // TODO: Detect black and white VideoNow
                // TODO: Detect VideoNow XP
                if(IsVideoNowColor(videoNowColorFrame))
                {
                    mediaType = MediaType.VideoNowColor;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found VideoNow! Color frame, setting disc type to VideoNow Color.");

                    return;
                }

                // Check CD+G, CD+EG and CD+MIDI
                if(mediaType == MediaType.CDDA)
                {
                    sense = dev.ReadCd(out byte[] subBuf, out _, 150, 96, 8, MmcSectorTypes.Cdda, false, false, false,
                                       MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                       dev.Timeout, out _);

                    if(!sense)
                    {
                        var cdg    = false;
                        var cdeg   = false;
                        var cdmidi = false;

                        for(var i = 0; i < 8; i++)
                        {
                            var tmpSub = new byte[96];
                            Array.Copy(subBuf, i * 96, tmpSub, 0, 96);
                            DetectRwPackets(tmpSub, out bool cdgPacket, out bool cdegPacket, out bool cdmidiPacket);

                            if(cdgPacket)
                                cdg = true;

                            if(cdegPacket)
                                cdeg = true;

                            if(cdmidiPacket)
                                cdmidi = true;
                        }

                        if(cdeg)
                        {
                            mediaType = MediaType.CDEG;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found enhanced graphics RW packet, setting disc type to CD+EG.");

                            return;
                        }

                        if(cdg)
                        {
                            mediaType = MediaType.CDG;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found graphics RW packet, setting disc type to CD+G.");

                            return;
                        }

                        if(cdmidi)
                        {
                            mediaType = MediaType.CDMIDI;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found MIDI RW packet, setting disc type to CD+MIDI.");

                            return;
                        }
                    }
                }

                // If it has a PS2 boot area it can still be PS1, so check for SYSTEM.CNF below
            hasPs2CdBoot:

                // Check if ISO9660
                sense = dev.Read12(out byte[] isoSector, out _, 0, false, false, false, false, 16, 2048, 0, 1, false,
                                   dev.Timeout, out _);

                // Sector 16 reads, and contains "CD001" magic?
                if(sense                ||
                   isoSector[1] != 0x43 ||
                   isoSector[2] != 0x44 ||
                   isoSector[3] != 0x30 ||
                   isoSector[4] != 0x30 ||
                   isoSector[5] != 0x31)
                    return;

                // From sectors 16 to 31
                uint isoSectorPosition = 16;

                while(isoSectorPosition < 32)
                {
                    sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, isoSectorPosition, 2048, 0,
                                       1, false, dev.Timeout, out _);

                    // If sector cannot be read, break here
                    if(sense)
                        break;

                    // If sector does not contain "CD001" magic, break
                    if(isoSector[1] != 0x43 ||
                       isoSector[2] != 0x44 ||
                       isoSector[3] != 0x30 ||
                       isoSector[4] != 0x30 ||
                       isoSector[5] != 0x31)
                        break;

                    // If it is PVD or end of descriptor chain, break
                    if(isoSector[0] == 1 ||
                       isoSector[0] == 255)
                        break;

                    isoSectorPosition++;
                }

                // If it's not an ISO9660 PVD, return
                if(isoSector[0] != 1    ||
                   isoSector[1] != 0x43 ||
                   isoSector[2] != 0x44 ||
                   isoSector[3] != 0x30 ||
                   isoSector[4] != 0x30 ||
                   isoSector[5] != 0x31)
                    return;

                var rootStart  = BitConverter.ToUInt32(isoSector, 158);
                var rootLength = BitConverter.ToUInt32(isoSector, 166);

                if(rootStart  == 0 ||
                   rootLength == 0)
                    return;

                rootLength /= 2048;

                try
                {
                    using var rootMs = new MemoryStream();

                    for(uint i = 0; i < rootLength; i++)
                    {
                        sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, rootStart + i, 2048, 0,
                                           1, false, dev.Timeout, out _);

                        if(sense)
                            break;

                        rootMs.Write(isoSector, 0, 2048);
                    }

                    isoSector = rootMs.ToArray();
                }
                catch
                {
                    return;
                }

                if(isoSector.Length < 2048)
                    return;

                var  rootEntries   = new List<string>();
                uint ngcdIplStart  = 0;
                uint ngcdIplLength = 0;
                uint vcdStart      = 0;
                uint vcdLength     = 0;
                uint pcdStart      = 0;
                uint pcdLength     = 0;
                uint ps1Start      = 0;
                uint ps1Length     = 0;

                for(var ri = 0; ri < rootLength; ri++)
                {
                    int rootPos = ri * 2048;

                    while(rootPos                      < isoSector.Length &&
                          isoSector[rootPos]           > 0                &&
                          rootPos + isoSector[rootPos] <= isoSector.Length)
                    {
                        int nameLen = isoSector[rootPos + 32];
                        var tmpName = new byte[nameLen];
                        Array.Copy(isoSector, rootPos + 33, tmpName, 0, nameLen);
                        string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                        if(name.EndsWith(";1", StringComparison.InvariantCulture))
                            name = name.Substring(0, name.Length - 2);

                        rootEntries.Add(name);

                        switch(name)
                        {
                            case "IPL.TXT":
                                ngcdIplStart  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                                ngcdIplLength = BitConverter.ToUInt32(isoSector, rootPos + 10);

                                break;

                            case "VCD" when (isoSector[rootPos   + 25] & 0x02) == 0x02:
                            case "SVCD" when (isoSector[rootPos  + 25] & 0x02) == 0x02:
                            case "HQVCD" when (isoSector[rootPos + 25] & 0x02) == 0x02:
                                vcdStart  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                                vcdLength = BitConverter.ToUInt32(isoSector, rootPos + 10) / 2048;

                                break;
                            case "PHOTO_CD" when (isoSector[rootPos + 25] & 0x02) == 0x02:
                                pcdStart  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                                pcdLength = BitConverter.ToUInt32(isoSector, rootPos + 10) / 2048;

                                break;
                            case "SYSTEM.CNF":
                                ps1Start  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                                ps1Length = BitConverter.ToUInt32(isoSector, rootPos + 10);

                                break;
                        }

                        rootPos += isoSector[rootPos];
                    }
                }

                if(rootEntries.Count == 0)
                    return;

                if(rootEntries.Contains("CD32.TM"))
                {
                    mediaType = MediaType.CD32;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found CD32.TM file in root, setting disc type to Amiga CD32.");

                    return;
                }

                if(rootEntries.Contains("CDTV.TM"))
                {
                    mediaType = MediaType.CDTV;

                    AaruConsole.DebugWriteLine("Media detection",
                                               "Found CDTV.TM file in root, setting disc type to Commodore CDTV.");

                    return;
                }

                // "IPL.TXT" length
                if(ngcdIplLength > 0)
                {
                    uint ngcdSectors = ngcdIplLength / 2048;

                    if(ngcdIplLength % 2048 > 0)
                        ngcdSectors++;

                    string iplTxt;

                    // Read "IPL.TXT"
                    try
                    {
                        using var ngcdMs = new MemoryStream();

                        for(uint i = 0; i < ngcdSectors; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, ngcdIplStart + i,
                                               2048, 0, 1, false, dev.Timeout, out _);

                            if(sense)
                                break;

                            ngcdMs.Write(isoSector, 0, 2048);
                        }

                        isoSector = ngcdMs.ToArray();
                        iplTxt    = Encoding.ASCII.GetString(isoSector);
                    }
                    catch
                    {
                        iplTxt = null;
                    }

                    // Check "IPL.TXT" lines
                    if(iplTxt != null)
                    {
                        using var sr = new StringReader(iplTxt);

                        var correctNeoGeoCd = true;
                        var lineNumber      = 0;

                        while(sr.Peek() > 0)
                        {
                            string line = sr.ReadLine();

                            // End of file
                            if(line is null ||
                               line.Length == 0)
                            {
                                if(lineNumber == 0)
                                    correctNeoGeoCd = false;

                                break;
                            }

                            // Split line by comma
                            string[] split = line.Split(',');

                            // Empty line
                            if(split.Length == 0)
                                continue;

                            // More than 3 entries
                            if(split.Length != 3)
                            {
                                if(line[0] < 0x20)
                                    break;

                                correctNeoGeoCd = false;

                                break;
                            }

                            // Split filename
                            string[] split2 = split[0].Split('.');

                            // Filename must have two parts, name and extension
                            if(split2.Length != 2)
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            // Name must be smaller or equal to 8 characters
                            if(split2[0].Length > 8)
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            // Extension must be smaller or equal to 8 characters
                            if(split2[1].Length > 3)
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            // Second part must be a single digit
                            if(split[1].Length != 1 ||
                               !byte.TryParse(split[1], out _))
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            // Third part must be bigger or equal to 1 and smaller or equal to 8
                            if(split[2].Length is < 1 or > 8)
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            try
                            {
                                _ = Convert.ToUInt32(split[2], 16);
                            }
                            catch
                            {
                                correctNeoGeoCd = false;

                                break;
                            }

                            lineNumber++;
                        }

                        if(correctNeoGeoCd)
                        {
                            mediaType = MediaType.NeoGeoCD;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found correct IPL.TXT file in root, setting disc type to Neo Geo CD.");

                            return;
                        }
                    }
                }

                if(vcdLength > 0)
                {
                    try
                    {
                        using var vcdMs = new MemoryStream();

                        for(uint i = 0; i < vcdLength; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, vcdStart + i, 2048,
                                               0, 1, false, dev.Timeout, out _);

                            if(sense)
                                break;

                            vcdMs.Write(isoSector, 0, 2048);
                        }

                        isoSector = vcdMs.ToArray();
                    }
                    catch
                    {
                        return;
                    }

                    if(isoSector.Length < 2048)
                        return;

                    uint infoPos = 0;

                    for(var vi = 0; vi < vcdLength; vi++)
                    {
                        int vcdPos = vi * 2048;

                        while(vcdPos                     < isoSector.Length &&
                              isoSector[vcdPos]          > 0                &&
                              vcdPos + isoSector[vcdPos] <= isoSector.Length)
                        {
                            int nameLen = isoSector[vcdPos + 32];
                            var tmpName = new byte[nameLen];
                            Array.Copy(isoSector, vcdPos + 33, tmpName, 0, nameLen);
                            string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                            if(name.EndsWith(";1", StringComparison.InvariantCulture))
                                name = name.Substring(0, name.Length - 2);

                            if(name is "INFO.VCD" or "INFO.SVD")
                            {
                                infoPos = BitConverter.ToUInt32(isoSector, vcdPos + 2);

                                break;
                            }

                            vcdPos += isoSector[vcdPos];
                        }
                    }

                    if(infoPos > 0)
                    {
                        sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, infoPos, 2048, 0, 1,
                                           false, dev.Timeout, out _);

                        if(sense)
                            break;

                        var systemId = new byte[8];
                        Array.Copy(isoSector, 0, systemId, 0, 8);

                        string id = StringHandlers.CToString(systemId).TrimEnd();

                        switch(id)
                        {
                            case "VIDEO_CD":
                                mediaType = MediaType.VCD;

                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Video CD description file, setting disc type to Video CD.");

                                return;
                            case "SUPERVCD":
                                mediaType = MediaType.SVCD;

                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Super Video CD description file, setting disc type to Super Video CD.");

                                break;
                            case "HQ-VCD":
                                mediaType = MediaType.CVD;

                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found China Video Disc description file, setting disc type to China Video Disc.");

                                break;
                        }
                    }
                }

                if(pcdLength > 0)
                {
                    try
                    {
                        using var pcdMs = new MemoryStream();

                        for(uint i = 0; i < pcdLength; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, pcdStart + i, 2048,
                                               0, 1, false, dev.Timeout, out _);

                            if(sense)
                                break;

                            pcdMs.Write(isoSector, 0, 2048);
                        }

                        isoSector = pcdMs.ToArray();
                    }
                    catch
                    {
                        return;
                    }

                    if(isoSector.Length < 2048)
                        return;

                    uint infoPos = 0;

                    for(var pi = 0; pi < pcdLength; pi++)
                    {
                        int pcdPos = pi * 2048;

                        while(pcdPos                     < isoSector.Length &&
                              isoSector[pcdPos]          > 0                &&
                              pcdPos + isoSector[pcdPos] <= isoSector.Length)
                        {
                            int nameLen = isoSector[pcdPos + 32];
                            var tmpName = new byte[nameLen];
                            Array.Copy(isoSector, pcdPos + 33, tmpName, 0, nameLen);
                            string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                            if(name.EndsWith(";1", StringComparison.InvariantCulture))
                                name = name.Substring(0, name.Length - 2);

                            if(name == "INFO.PCD")
                            {
                                infoPos = BitConverter.ToUInt32(isoSector, pcdPos + 2);

                                break;
                            }

                            pcdPos += isoSector[pcdPos];
                        }
                    }

                    if(infoPos > 0)
                    {
                        sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, infoPos, 2048, 0, 1,
                                           false, dev.Timeout, out _);

                        if(sense)
                            break;

                        var systemId = new byte[8];
                        Array.Copy(isoSector, 0, systemId, 0, 8);

                        string id = StringHandlers.CToString(systemId).TrimEnd();

                        switch(id)
                        {
                            case "PHOTO_CD":
                                mediaType = MediaType.PCD;

                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Photo CD description file, setting disc type to Photo CD.");

                                return;
                        }
                    }
                }

                // "SYSTEM.CNF" length
                if(ps1Length > 0)
                {
                    uint ps1Sectors = ps1Length / 2048;

                    if(ps1Length % 2048 > 0)
                        ps1Sectors++;

                    string ps1Txt;

                    // Read "SYSTEM.CNF"
                    try
                    {
                        using var ps1Ms = new MemoryStream();

                        for(uint i = 0; i < ps1Sectors; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, false, false, false, ps1Start + i, 2048,
                                               0, 1, false, dev.Timeout, out _);

                            if(sense)
                                break;

                            ps1Ms.Write(isoSector, 0, 2048);
                        }

                        isoSector = ps1Ms.ToArray();
                        ps1Txt    = Encoding.ASCII.GetString(isoSector);
                    }
                    catch
                    {
                        ps1Txt = null;
                    }

                    // Check "SYSTEM.CNF" lines
                    if(ps1Txt != null)
                    {
                        using var sr = new StringReader(ps1Txt);

                        string ps1BootFile = null;
                        string ps2BootFile = null;

                        while(sr.Peek() > 0)
                        {
                            string line = sr.ReadLine();

                            // End of file
                            if(line is null ||
                               line.Length == 0)
                                break;

                            line = line.Replace(" ", "");

                            if(line.StartsWith("BOOT=cdrom:", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ps1BootFile = line.Substring(11);

                                if(ps1BootFile.StartsWith('\\'))
                                    ps1BootFile = ps1BootFile.Substring(1);

                                if(ps1BootFile.EndsWith(";1", StringComparison.InvariantCultureIgnoreCase))
                                    ps1BootFile = ps1BootFile.Substring(0, ps1BootFile.Length - 2);

                                break;
                            }

                            if(line.StartsWith("BOOT2=cdrom0:", StringComparison.InvariantCultureIgnoreCase))
                            {
                                ps2BootFile = line.Substring(13);

                                if(ps2BootFile.StartsWith('\\'))
                                    ps2BootFile = ps2BootFile.Substring(1);

                                if(ps2BootFile.EndsWith(";1", StringComparison.InvariantCultureIgnoreCase))
                                    ps2BootFile = ps2BootFile.Substring(0, ps2BootFile.Length - 2);

                                break;
                            }
                        }

                        if(ps1BootFile != null &&
                           rootEntries.Contains(ps1BootFile.ToUpperInvariant()))
                        {
                            mediaType = MediaType.PS1CD;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found correct SYSTEM.CNF file in root pointing to existing file in root, setting disc type to PlayStation CD.");
                        }

                        if(ps2BootFile != null &&
                           rootEntries.Contains(ps2BootFile.ToUpperInvariant()))
                        {
                            mediaType = MediaType.PS2CD;

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Found correct SYSTEM.CNF file in root pointing to existing file in root, setting disc type to PlayStation 2 CD.");
                        }
                    }
                }

                break;
            }

            // TODO: Check for CD-i Ready
            case MediaType.CDI: break;
            case MediaType.DVDROM:
            case MediaType.HDDVDROM:
            case MediaType.BDROM:
            case MediaType.UHDBD:
            case MediaType.Unknown:
                // TODO: Nuon requires reading the filesystem, searching for a file called "/NUON/NUON.RUN"
                if(ps2BootSectors is { Length: 0x6000 })
                {
                    // The decryption key is applied as XOR. As first byte is originally always NULL, it gives us the key :)
                    byte decryptByte = ps2BootSectors[0];

                    for(var i = 0; i < 0x6000; i++)
                        ps2BootSectors[i] ^= decryptByte;

                    string ps2BootSectorsHash = Sha256Context.Data(ps2BootSectors, out _);

                    AaruConsole.DebugWriteLine("Media-info Command", "PlayStation 2 boot sectors SHA256: {0}",
                                               ps2BootSectorsHash);

                    if(ps2BootSectorsHash is PS2_PAL_HASH or PS2_NTSC_HASH or PS2_JAPANESE_HASH)
                    {
                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Sony PlayStation 2 boot sectors, setting disc type to PS2 DVD.");

                        mediaType = MediaType.PS2DVD;
                    }
                }

                if(sector1 != null)
                {
                    var tmp = new byte[_ps3Id.Length];
                    Array.Copy(sector1, 0, tmp, 0, tmp.Length);

                    if(tmp.SequenceEqual(_ps3Id))
                        switch(mediaType)
                        {
                            case MediaType.BDROM:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Sony PlayStation 3 boot sectors, setting disc type to PS3 Blu-ray.");

                                mediaType = MediaType.PS3BD;

                                break;
                            case MediaType.DVDROM:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "Found Sony PlayStation 3 boot sectors, setting disc type to PS3 DVD.");

                                mediaType = MediaType.PS3DVD;

                                break;
                        }

                    tmp = new byte[_ps4Id.Length];
                    Array.Copy(sector1, 512, tmp, 0, tmp.Length);

                    if(tmp.SequenceEqual(_ps4Id) &&
                       mediaType == MediaType.BDROM)
                    {
                        mediaType = MediaType.PS4BD;

                        AaruConsole.DebugWriteLine("Media detection",
                                                   "Found Sony PlayStation 4 boot sectors, setting disc type to PS4 Blu-ray.");
                    }
                }

                if(blurayDi                              != null &&
                   blurayDi?.Units?.Length               > 0     &&
                   blurayDi?.Units[0].DiscTypeIdentifier != null)
                {
                    string blurayType = StringHandlers.CToString(blurayDi?.Units[0].DiscTypeIdentifier);

                    switch(blurayType)
                    {
                        case "XG4":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Blu-ray type set to \"XG4\", setting disc type to Xbox One Disc (XGD4).");

                            mediaType = MediaType.XGD4;

                            break;

                        // TODO: PS5
                        case "BDU":
                            if(sector1 != null)
                            {
                                var tmp = new byte[_ps5Id.Length];
                                Array.Copy(sector1, 1024, tmp, 0, tmp.Length);

                                if(tmp.SequenceEqual(_ps5Id))
                                {
                                    mediaType = MediaType.PS5BD;

                                    AaruConsole.DebugWriteLine("Media detection",
                                                               "Found Sony PlayStation 5 boot sectors, setting disc type to PS5 Ultra HD Blu-ray.");

                                    break;
                                }
                            }

                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Blu-ray type set to \"BDU\", setting disc type to Ultra HD Blu-ray.");

                            mediaType = MediaType.UHDBD;

                            break;
                    }
                }

                break;
        }
    }

    static void DetectRwPackets(byte[] subchannel, out bool cdgPacket, out bool cdegPacket, out bool cdmidiPacket)
    {
        cdgPacket    = false;
        cdegPacket   = false;
        cdmidiPacket = false;

        var cdSubRwPack1 = new byte[24];
        var cdSubRwPack2 = new byte[24];
        var cdSubRwPack3 = new byte[24];
        var cdSubRwPack4 = new byte[24];

        var i = 0;

        for(var j = 0; j < 24; j++)
            cdSubRwPack1[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack2[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack3[j] = (byte)(subchannel[i++] & 0x3F);

        for(var j = 0; j < 24; j++)
            cdSubRwPack4[j] = (byte)(subchannel[i++] & 0x3F);

        switch(cdSubRwPack1[0])
        {
            case 0x08:
            case 0x09:
                cdgPacket = true;

                break;
            case 0x0A:
                cdegPacket = true;

                break;
            case 0x38:
                cdmidiPacket = true;

                break;
        }

        switch(cdSubRwPack2[0])
        {
            case 0x08:
            case 0x09:
                cdgPacket = true;

                break;
            case 0x0A:
                cdegPacket = true;

                break;
            case 0x38:
                cdmidiPacket = true;

                break;
        }

        switch(cdSubRwPack3[0])
        {
            case 0x08:
            case 0x09:
                cdgPacket = true;

                break;
            case 0x0A:
                cdegPacket = true;

                break;
            case 0x38:
                cdmidiPacket = true;

                break;
        }

        switch(cdSubRwPack4[0])
        {
            case 0x08:
            case 0x09:
                cdgPacket = true;

                break;
            case 0x0A:
                cdegPacket = true;

                break;
            case 0x38:
                cdmidiPacket = true;

                break;
        }
    }
}