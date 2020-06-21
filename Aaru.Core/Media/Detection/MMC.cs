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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.Sega;
using Aaru.Devices;

// ReSharper disable JoinDeclarationAndInitializer

namespace Aaru.Core.Media.Detection
{
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

            byte[] testMark = new byte[12];
            Array.Copy(sector, 0, testMark, 0, 12);

            return syncMark.SequenceEqual(testMark) && (sector[0xF] == 0 || sector[0xF] == 1 || sector[0xF] == 2);
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
               sector0[0xF] != 2)
                return false;

            byte[] testMark = new byte[4];
            Array.Copy(sector16, 24, testMark, 0, 4);

            return cdiMark.SequenceEqual(testMark);
        }

        static bool IsVideoNowColor(byte[] videoFrame)
        {
            if(videoFrame is null ||
               videoFrame.Length < _videoNowColorFrameMarker.Length)
                return false;

            byte[] buffer = new byte[_videoNowColorFrameMarker.Length];

            for(int framePosition = 0; framePosition + buffer.Length < videoFrame.Length; framePosition++)
            {
                Array.Copy(videoFrame, framePosition, buffer, 0, buffer.Length);

                for(int ab = 9; ab < buffer.Length; ab += 10)
                    buffer[ab] = 0;

                if(!_videoNowColorFrameMarker.SequenceEqual(buffer))
                    continue;

                return true;
            }

            return false;
        }

        public static int GetVideoNowColorOffset(byte[] data)
        {
            byte[] buffer = new byte[_videoNowColorFrameMarker.Length];

            for(int framePosition = 0; framePosition + buffer.Length < data.Length; framePosition++)
            {
                Array.Copy(data, framePosition, buffer, 0, buffer.Length);

                for(int ab = 9; ab < buffer.Length; ab += 10)
                    buffer[ab] = 0;

                if(!_videoNowColorFrameMarker.SequenceEqual(buffer))
                    continue;

                return 18032 - framePosition;
            }

            return 0;
        }

        public static void DetectDiscType(ref MediaType mediaType, int sessions, FullTOC.CDFullTOC? decodedToc,
                                          Device dev, out bool hiddenTrack, out bool hiddenData,
                                          int firstTrackLastSession)
        {
            uint   startOfFirstDataTrack = uint.MaxValue;
            byte[] cmdBuf;
            bool   sense;
            byte   secondSessionFirstTrack = 0;
            byte[] sector0;
            byte[] sector1                      = null;
            byte[] ps2BootSectors               = null;
            byte[] playdia1                     = null;
            byte[] playdia2                     = null;
            byte[] firstDataSectorNotZero       = null;
            byte[] secondDataSectorNotZero      = null;
            byte[] firstTrackSecondSession      = null;
            byte[] firstTrackSecondSessionAudio = null;
            byte[] videoNowColorFrame;
            hiddenTrack = false;
            hiddenData  = false;

            if(decodedToc.HasValue)
                if(decodedToc.Value.TrackDescriptors.Any(t => t.SessionNumber == 2))
                    secondSessionFirstTrack =
                        decodedToc.Value.TrackDescriptors.Where(t => t.SessionNumber == 2).Min(t => t.POINT);

            if(mediaType == MediaType.CD ||
               mediaType == MediaType.CDROMXA)
            {
                bool hasDataTrack                  = false;
                bool hasAudioTrack                 = false;
                bool allFirstSessionTracksAreAudio = true;
                bool hasVideoTrack                 = false;

                if(decodedToc.HasValue)
                {
                    FullTOC.TrackDataDescriptor a0Track =
                        decodedToc.Value.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA0 && t.ADR == 1);

                    if(a0Track.POINT == 0xA0)
                        switch(a0Track.PSEC)
                        {
                            case 0x10:
                                mediaType = MediaType.CDI;

                                break;
                            case 0x20:
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
                            uint startAddress =
                                (uint)(((track.PHOUR * 3600 * 75) + (track.PMIN * 60 * 75) + (track.PSEC * 75) +
                                        track.PFRAME) - 150);

                            if(startAddress < startOfFirstDataTrack)
                                startOfFirstDataTrack = startAddress;

                            hasDataTrack                  =  true;
                            allFirstSessionTracksAreAudio &= track.POINT >= firstTrackLastSession;
                        }
                        else
                        {
                            hasAudioTrack = true;
                        }

                        hasVideoTrack |= track.ADR == 4;
                    }
                }

                if(mediaType != MediaType.CDI)
                {
                    if(hasDataTrack                  &&
                       hasAudioTrack                 &&
                       allFirstSessionTracksAreAudio &&
                       sessions == 2)
                        mediaType = MediaType.CDPLUS;

                    if(!hasDataTrack &&
                       hasAudioTrack &&
                       sessions == 1)
                        mediaType = MediaType.CDDA;

                    if(hasDataTrack   &&
                       !hasAudioTrack &&
                       sessions == 1)
                        mediaType = MediaType.CDROM;

                    if(hasVideoTrack &&
                       !hasDataTrack &&
                       sessions == 1)
                        mediaType = MediaType.CDV;
                }

                if((mediaType == MediaType.CD || mediaType == MediaType.CDROM) && hasDataTrack)
                {
                    foreach(FullTOC.TrackDataDescriptor track in
                        decodedToc.Value.TrackDescriptors.Where(t => t.POINT > 0 && t.POINT <= 0x99 &&
                                                                     ((TocControl)(t.CONTROL & 0x0D) ==
                                                                      TocControl.DataTrack ||
                                                                      (TocControl)(t.CONTROL & 0x0D) ==
                                                                      TocControl.DataTrackIncremental)))
                    {
                        uint startAddress =
                            (uint)(((track.PHOUR * 3600 * 75) + (track.PMIN * 60 * 75) + (track.PSEC * 75) +
                                    track.PFRAME) - 150) + 16;

                        sense = dev.ReadCd(out cmdBuf, out _, startAddress, 2352, 1, MmcSectorTypes.AllTypes, false,
                                           false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            if(cmdBuf[0]  == 0x00 &&
                               cmdBuf[1]  == 0xFF &&
                               cmdBuf[2]  == 0xFF &&
                               cmdBuf[3]  == 0xFF &&
                               cmdBuf[4]  == 0xFF &&
                               cmdBuf[5]  == 0xFF &&
                               cmdBuf[6]  == 0xFF &&
                               cmdBuf[7]  == 0xFF &&
                               cmdBuf[8]  == 0xFF &&
                               cmdBuf[9]  == 0xFF &&
                               cmdBuf[10] == 0xFF &&
                               cmdBuf[11] == 0x00 &&
                               cmdBuf[15] == 0x02)
                            {
                                mediaType = MediaType.CDROMXA;

                                break;
                            }
                        }
                    }
                }
            }

            if(secondSessionFirstTrack != 0 &&
               decodedToc.HasValue          &&
               decodedToc.Value.TrackDescriptors.Any(t => t.POINT == secondSessionFirstTrack))
            {
                FullTOC.TrackDataDescriptor secondSessionFirstTrackTrack =
                    decodedToc.Value.TrackDescriptors.First(t => t.POINT == secondSessionFirstTrack);

                uint firstSectorSecondSessionFirstTrack =
                    (uint)(((secondSessionFirstTrackTrack.PHOUR * 3600 * 75) +
                            (secondSessionFirstTrackTrack.PMIN * 60 * 75) + (secondSessionFirstTrackTrack.PSEC * 75) +
                            secondSessionFirstTrackTrack.PFRAME) - 150);

                sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack, 2352, 1,
                                   MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                {
                    firstTrackSecondSession = cmdBuf;
                }
                else
                {
                    sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack, 2352, 1,
                                       MmcSectorTypes.Cdda, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                       MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                    if(!sense &&
                       !dev.Error)
                        firstTrackSecondSession = cmdBuf;
                }

                sense = dev.ReadCd(out cmdBuf, out _, firstSectorSecondSessionFirstTrack - 1, 2352, 3,
                                   MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                if(!sense &&
                   !dev.Error)
                {
                    firstTrackSecondSessionAudio = cmdBuf;
                }
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

            for(int i = 0; i < 9; i++)
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

            if(decodedToc.HasValue)
            {
                FullTOC.TrackDataDescriptor firstTrack =
                    decodedToc.Value.TrackDescriptors.FirstOrDefault(t => t.POINT == 1);

                if(firstTrack.POINT == 1)
                {
                    uint firstTrackSector = (uint)(((firstTrack.PHOUR * 3600 * 75) + (firstTrack.PMIN * 60 * 75) +
                                                    (firstTrack.PSEC         * 75) + firstTrack.PFRAME) - 150);

                    // Check for hidden data before start of track 1
                    if(firstTrackSector > 0)
                    {
                        sense = dev.ReadCd(out sector0, out _, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true,
                                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!dev.Error &&
                           !sense)
                        {
                            hiddenTrack = true;

                            hiddenData = IsData(sector0);

                            if(hiddenData)
                            {
                                sense = dev.ReadCd(out byte[] sector16, out _, 16, 2352, 1, MmcSectorTypes.AllTypes,
                                                   false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                                   MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                                if(IsCdi(sector0, sector16))
                                {
                                    mediaType = MediaType.CDIREADY;
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
                                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            sector1 = new byte[2048];
                            Array.Copy(cmdBuf, 16, sector1, 0, 2048);
                        }

                        sense = dev.ReadCd(out cmdBuf, out _, 4200, 2352, 1, MmcSectorTypes.AllTypes, false, false,
                                           true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            playdia1 = new byte[2048];
                            Array.Copy(cmdBuf, 24, playdia1, 0, 2048);
                        }

                        sense = dev.ReadCd(out cmdBuf, out _, 4201, 2352, 1, MmcSectorTypes.AllTypes, false, false,
                                           true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                           MmcSubchannel.None, dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            playdia2 = new byte[2048];
                            Array.Copy(cmdBuf, 24, playdia2, 0, 2048);
                        }

                        if(startOfFirstDataTrack != uint.MaxValue)
                        {
                            sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack, 2352, 1,
                                               MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                               true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                firstDataSectorNotZero = new byte[2048];
                                Array.Copy(cmdBuf, 16, firstDataSectorNotZero, 0, 2048);
                            }

                            sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack + 1, 2352, 1,
                                               MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                               true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

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
                            sense = dev.ReadCd(out cmdBuf, out _, p, 2352, 1, MmcSectorTypes.AllTypes, false, false,
                                               true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
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

                            sense = dev.ReadCd(out cmdBuf, out _, 4200, 2324, 1, MmcSectorTypes.Mode2, false, false,
                                               false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                               MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                playdia1 = new byte[2048];
                                Array.Copy(cmdBuf, 0, playdia1, 0, 2048);
                            }

                            sense = dev.ReadCd(out cmdBuf, out _, 4201, 2324, 1, MmcSectorTypes.Mode2, false, false,
                                               false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                               MmcSubchannel.None, dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                playdia2 = new byte[2048];
                                Array.Copy(cmdBuf, 0, playdia2, 0, 2048);
                            }

                            if(startOfFirstDataTrack != uint.MaxValue)
                            {
                                sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack, 2324, 1,
                                                   MmcSectorTypes.Mode2, false, false, false, MmcHeaderCodes.None, true,
                                                   false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

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
                                sense = dev.ReadCd(out cmdBuf, out _, p, 2324, 1, MmcSectorTypes.Mode2, false, false,
                                                   false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                   MmcSubchannel.None, dev.Timeout, out _);

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

                                sense = dev.ReadCd(out cmdBuf, out _, 0, 2048, 1, MmcSectorTypes.Mode1, false, false,
                                                   false, MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                                   MmcSubchannel.None, dev.Timeout, out _);

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
                                                       MmcSectorTypes.Mode1, false, false, false, MmcHeaderCodes.None,
                                                       true, false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout,
                                                       out _);

                                    if(!sense &&
                                       !dev.Error)
                                        firstDataSectorNotZero = cmdBuf;

                                    sense = dev.ReadCd(out cmdBuf, out _, startOfFirstDataTrack + 1, 2048, 1,
                                                       MmcSectorTypes.Mode1, false, false, false, MmcHeaderCodes.None,
                                                       true, false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout,
                                                       out _);

                                    if(!sense &&
                                       !dev.Error)
                                        secondDataSectorNotZero = cmdBuf;
                                }
                            }
                            else
                            {
                                goto case MediaType.DVDROM;
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
                case MediaType.Unknown:
                    sense = dev.Read16(out cmdBuf, out _, 0, false, true, false, 0, 2048, 0, 1, false, dev.Timeout,
                                       out _);

                    if(!sense &&
                       !dev.Error)
                    {
                        sector0 = cmdBuf;

                        sense = dev.Read16(out cmdBuf, out _, 0, false, true, false, 1, 2048, 0, 1, false, dev.Timeout,
                                           out _);

                        if(!sense &&
                           !dev.Error)
                            sector1 = cmdBuf;

                        sense = dev.Read16(out cmdBuf, out _, 0, false, true, false, 0, 2048, 0, 12, false, dev.Timeout,
                                           out _);

                        if(!sense     &&
                           !dev.Error &&
                           cmdBuf.Length == 0x6000)
                            ps2BootSectors = cmdBuf;
                    }
                    else
                    {
                        sense = dev.Read12(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 1, false,
                                           dev.Timeout, out _);

                        if(!sense &&
                           !dev.Error)
                        {
                            sector0 = cmdBuf;

                            sense = dev.Read12(out cmdBuf, out _, 0, false, true, false, false, 1, 2048, 0, 1, false,
                                               dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                                sector1 = cmdBuf;

                            sense = dev.Read12(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 12, false,
                                               dev.Timeout, out _);

                            if(!sense     &&
                               !dev.Error &&
                               cmdBuf.Length == 0x6000)
                                ps2BootSectors = cmdBuf;
                        }
                        else
                        {
                            sense = dev.Read10(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 1,
                                               dev.Timeout, out _);

                            if(!sense &&
                               !dev.Error)
                            {
                                sector0 = cmdBuf;

                                sense = dev.Read10(out cmdBuf, out _, 0, false, true, false, false, 1, 2048, 0, 1,
                                                   dev.Timeout, out _);

                                if(!sense &&
                                   !dev.Error)
                                    sector1 = cmdBuf;

                                sense = dev.Read10(out cmdBuf, out _, 0, false, true, false, false, 0, 2048, 0, 12,
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

                    break;

                // Recordables will not be checked
                case MediaType.CDR:
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

                        return;
                    }

                    if(Saturn.DecodeIPBin(sector0).HasValue)
                    {
                        mediaType = MediaType.SATURNCD;

                        return;
                    }

                    // Are GDR detectable ???
                    if(Dreamcast.DecodeIPBin(sector0).HasValue)
                    {
                        mediaType = MediaType.GDROM;

                        return;
                    }

                    if(ps2BootSectors        != null &&
                       ps2BootSectors.Length == 0x6000)
                    {
                        // The decryption key is applied as XOR. As first byte is originally always NULL, it gives us the key :)
                        byte decryptByte = ps2BootSectors[0];

                        for(int i = 0; i < 0x6000; i++)
                            ps2BootSectors[i] ^= decryptByte;

                        string ps2BootSectorsHash = Sha256Context.Data(ps2BootSectors, out _);

                        AaruConsole.DebugWriteLine("Media-info Command", "PlayStation 2 boot sectors SHA256: {0}",
                                                   ps2BootSectorsHash);

                        if(ps2BootSectorsHash == PS2_PAL_HASH  ||
                           ps2BootSectorsHash == PS2_NTSC_HASH ||
                           ps2BootSectorsHash == PS2_JAPANESE_HASH)
                        {
                            mediaType = MediaType.PS2CD;

                            return;
                        }
                    }

                    if(sector0 != null)
                    {
                        byte[] syncBytes = new byte[7];
                        Array.Copy(sector0, 0, syncBytes, 0, 7);

                        if(_operaId.SequenceEqual(syncBytes))
                        {
                            mediaType = MediaType.ThreeDO;

                            return;
                        }

                        if(_fmTownsBootId.SequenceEqual(syncBytes))
                        {
                            mediaType = MediaType.FMTOWNS;

                            return;
                        }
                    }

                    if(playdia1 != null &&
                       playdia2 != null)
                    {
                        byte[] pd1 = new byte[_playdiaCopyright.Length];
                        byte[] pd2 = new byte[_playdiaCopyright.Length];

                        Array.Copy(playdia1, 38, pd1, 0, pd1.Length);
                        Array.Copy(playdia2, 0, pd2, 0, pd1.Length);

                        if(_playdiaCopyright.SequenceEqual(pd1) &&
                           _playdiaCopyright.SequenceEqual(pd2))
                        {
                            mediaType = MediaType.Playdia;

                            return;
                        }
                    }

                    if(secondDataSectorNotZero != null)
                    {
                        byte[] pce = new byte[_pcEngineSignature.Length];
                        Array.Copy(secondDataSectorNotZero, 32, pce, 0, pce.Length);

                        if(_pcEngineSignature.SequenceEqual(pce))
                        {
                            mediaType = MediaType.SuperCDROM2;

                            return;
                        }
                    }

                    if(firstDataSectorNotZero != null)
                    {
                        byte[] pcfx = new byte[_pcFxSignature.Length];
                        Array.Copy(firstDataSectorNotZero, 0, pcfx, 0, pcfx.Length);

                        if(_pcFxSignature.SequenceEqual(pcfx))
                        {
                            mediaType = MediaType.PCFX;

                            return;
                        }
                    }

                    if(firstTrackSecondSessionAudio != null)
                    {
                        byte[] jaguar = new byte[_atariSignature.Length];

                        for(int i = 0; i + jaguar.Length <= firstTrackSecondSessionAudio.Length; i += 2)
                        {
                            Array.Copy(firstTrackSecondSessionAudio, i, jaguar, 0, jaguar.Length);

                            if(!_atariSignature.SequenceEqual(jaguar))
                                continue;

                            mediaType = MediaType.JaguarCD;

                            break;
                        }
                    }

                    if(firstTrackSecondSession != null)
                        if(firstTrackSecondSession.Length >= 2336)
                        {
                            byte[] milcd = new byte[2048];
                            Array.Copy(firstTrackSecondSession, 24, milcd, 0, 2048);

                            if(Dreamcast.DecodeIPBin(milcd).HasValue)
                            {
                                mediaType = MediaType.MilCD;

                                return;
                            }
                        }

                    // TODO: Detect black and white VideoNow
                    // TODO: Detect VideoNow XP
                    if(IsVideoNowColor(videoNowColorFrame))
                    {
                        mediaType = MediaType.VideoNowColor;

                        return;
                    }

                    // Check if ISO9660
                    sense = dev.Read12(out byte[] isoSector, out _, 0, false, true, false, false, 16, 2048, 0, 1, false,
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
                        sense = dev.Read12(out isoSector, out _, 0, false, true, false, false, isoSectorPosition, 2048,
                                           0, 1, false, dev.Timeout, out _);

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

                    uint rootStart  = BitConverter.ToUInt32(isoSector, 158);
                    uint rootLength = BitConverter.ToUInt32(isoSector, 166);

                    if(rootStart  == 0 ||
                       rootLength == 0)
                        return;

                    rootLength /= 2048;

                    try
                    {
                        using var rootMs = new MemoryStream();

                        for(uint i = 0; i < rootLength; i++)
                        {
                            sense = dev.Read12(out isoSector, out _, 0, false, true, false, false, rootStart + i, 2048,
                                               0, 1, false, dev.Timeout, out _);

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
                        break;

                    List<string> rootEntries   = new List<string>();
                    int          rootPos       = 0;
                    uint         ngcdIplStart  = 0;
                    uint         ngcdIplLength = 0;

                    while(isoSector[rootPos]           > 0                &&
                          rootPos                      < isoSector.Length &&
                          rootPos + isoSector[rootPos] <= isoSector.Length)
                    {
                        int    nameLen = isoSector[rootPos + 32];
                        byte[] tmpName = new byte[nameLen];
                        Array.Copy(isoSector, rootPos + 33, tmpName, 0, nameLen);
                        string name = StringHandlers.CToString(tmpName).ToUpperInvariant();

                        if(name.EndsWith(";1", StringComparison.InvariantCulture))
                            name = name.Substring(0, name.Length - 2);

                        // TODO: Video CD and Super Video CD
                        rootEntries.Add(name);

                        if(name == "IPL.TXT")
                        {
                            ngcdIplStart  = BitConverter.ToUInt32(isoSector, rootPos + 2);
                            ngcdIplLength = BitConverter.ToUInt32(isoSector, rootPos + 10);
                        }

                        rootPos += isoSector[rootPos];
                    }

                    if(rootEntries.Count == 0)
                        return;

                    if(rootEntries.Contains("CD32.TM"))
                    {
                        mediaType = MediaType.CD32;

                        return;
                    }

                    if(rootEntries.Contains("CDTV.TM"))
                    {
                        mediaType = MediaType.CDTV;

                        return;
                    }

                    // "IPL.TXT" length
                    if(ngcdIplLength > 0)
                    {
                        uint ngcdSectors = ngcdIplLength / 2048;

                        if(ngcdIplLength % 2048 > 0)
                            ngcdSectors++;

                        string iplTxt = null;

                        // Read "IPL.TXT"
                        try
                        {
                            using var ngcdMs = new MemoryStream();

                            for(uint i = 0; i < ngcdSectors; i++)
                            {
                                sense = dev.Read12(out isoSector, out _, 0, false, true, false, false, ngcdIplStart + i,
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

                            bool correctNeoGeoCd = true;
                            int  lineNumber      = 0;

                            while(sr.Peek() > 0)
                            {
                                string? line = sr.ReadLine();

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
                                if(split[2].Length < 1 ||
                                   split[2].Length > 8)
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
                case MediaType.Unknown:
                    // TODO: Nuon requires reading the filesystem, searching for a file called "/NUON/NUON.RUN"
                    if(ps2BootSectors        != null &&
                       ps2BootSectors.Length == 0x6000)
                    {
                        // The decryption key is applied as XOR. As first byte is originally always NULL, it gives us the key :)
                        byte decryptByte = ps2BootSectors[0];

                        for(int i = 0; i < 0x6000; i++)
                            ps2BootSectors[i] ^= decryptByte;

                        string ps2BootSectorsHash = Sha256Context.Data(ps2BootSectors, out _);

                        AaruConsole.DebugWriteLine("Media-info Command", "PlayStation 2 boot sectors SHA256: {0}",
                                                   ps2BootSectorsHash);

                        if(ps2BootSectorsHash == PS2_PAL_HASH  ||
                           ps2BootSectorsHash == PS2_NTSC_HASH ||
                           ps2BootSectorsHash == PS2_JAPANESE_HASH)
                            mediaType = MediaType.PS2DVD;
                    }

                    if(sector1 != null)
                    {
                        byte[] tmp = new byte[_ps3Id.Length];
                        Array.Copy(sector1, 0, tmp, 0, tmp.Length);

                        if(tmp.SequenceEqual(_ps3Id))
                            switch(mediaType)
                            {
                                case MediaType.BDROM:
                                    mediaType = MediaType.PS3BD;

                                    break;
                                case MediaType.DVDROM:
                                    mediaType = MediaType.PS3DVD;

                                    break;
                            }

                        tmp = new byte[_ps4Id.Length];
                        Array.Copy(sector1, 512, tmp, 0, tmp.Length);

                        if(tmp.SequenceEqual(_ps4Id) &&
                           mediaType == MediaType.BDROM)
                            mediaType = MediaType.PS4BD;
                    }

                    // TODO: Identify discs that require reading tracks (PC-FX, PlayStation, Sega, etc)
                    break;
            }
        }
    }
}