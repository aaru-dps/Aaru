// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleSony.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Apple/Sony floppy structures.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DiscImageChef.Decoders.Floppy
{
    // Information from:
    // Inside Macintosh, Volume II, ISBN 0-201-17732-3

    /// <summary>
    /// Methods and structures for Apple Sony GCR floppy decoding
    /// </summary>
    public static class AppleSony
    {
        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy track
        /// </summary>
        public class RawTrack
        {
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 36 bytes
            /// </summary>
            public byte[] gap;
            public RawSector[] sectors;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector
        /// </summary>
        public class RawSector
        {
            /// <summary>
            /// Address field
            /// </summary>
            public RawAddressField addressField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 6 bytes
            /// </summary>
            public byte[] innerGap;
            /// <summary>
            /// Data field
            /// </summary>
            public RawDataField dataField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, unknown size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector address field
        /// </summary>
        public class RawAddressField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0x96
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] prologue;
            /// <summary>
            /// Encoded (decodedTrack &amp; 0x3F)
            /// </summary>
            public byte track;
            /// <summary>
            /// Encoded sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Encoded side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Disk format
            /// </summary>
            public AppleEncodedFormat format;
            /// <summary>
            /// Checksum
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] epilogue;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector data field
        /// </summary>
        public class RawDataField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0xAD
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] prologue;
            /// <summary>
            /// Spare, usually <see cref="RawAddressField.sector"/>
            /// </summary>
            public byte spare;
            /// <summary>
            /// Encoded data bytes.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 698)] public byte[] data;
            /// <summary>
            /// Checksum
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] epilogue;
        }

        public static byte[] DecodeSector(RawSector sector)
        {
            if(sector.addressField.prologue[0] != 0xD5 || sector.addressField.prologue[1] != 0xAA ||
               sector.addressField.prologue[2] != 0x96) return null;

            uint ck1, ck2, ck3;
            byte carry;
            byte w1, w2, w3, w4;
            byte[] bf1 = new byte[175];
            byte[] bf2 = new byte[175];
            byte[] bf3 = new byte[175];
            byte[] nib_data = sector.dataField.data;
            MemoryStream ms = new MemoryStream();

            int j = 0;
            w3 = 0;
            for(int i = 0; i <= 174; i++)
            {
                w4 = nib_data[j++];
                w1 = nib_data[j++];
                w2 = nib_data[j++];

                if(i != 174) w3 = nib_data[j++];

                bf1[i] = (byte)(((w1 & 0x3F) | ((w4 << 2) & 0xC0)) & 0x0F);
                bf2[i] = (byte)(((w2 & 0x3F) | ((w4 << 4) & 0xC0)) & 0x0F);
                bf3[i] = (byte)(((w3 & 0x3F) | ((w4 << 6) & 0xC0)) & 0x0F);
            }

            j = 0;
            ck1 = 0;
            ck2 = 0;
            ck3 = 0;
            while(true)
            {
                ck1 = (ck1 & 0xFF) << 1;
                if((ck1 & 0x0100) > 0) ck1++;

                carry = (byte)((bf1[j] ^ ck1) & 0xFF);
                ck3 += carry;
                if((ck1 & 0x0100) > 0)
                {
                    ck3++;
                    ck1 &= 0xFF;
                }
                ms.WriteByte(carry);

                carry = (byte)((bf2[j] ^ ck3) & 0xFF);
                ck2 += carry;
                if(ck3 > 0xFF)
                {
                    ck2++;
                    ck3 &= 0xFF;
                }
                ms.WriteByte(carry);

                if(ms.Length == 524) break;

                carry = (byte)((bf3[j] ^ ck2) & 0xFF);
                ck1 += carry;
                if(ck2 > 0xFF)
                {
                    ck1++;
                    ck2 &= 0xFF;
                }
                ms.WriteByte(carry);
                j++;
            }

            return ms.ToArray();

            // Not Apple Sony GCR?
        }

        public static RawSector MarshalSector(byte[] data, int offset = 0)
        {
            int temp;
            return MarshalSector(data, out temp, offset);
        }

        public static RawSector MarshalSector(byte[] data, out int endOffset, int offset = 0)
        {
            endOffset = offset;

            // Not an Apple ][ GCR sector
            if(data == null || data.Length < 363) return null;

            RawSector sector;
            int position = offset;
            MemoryStream gaps;
            bool onSync;
            int syncCount;

            try
            {
                while(position < data.Length)
                {
                    // Prologue found
                    if(data[position] == 0xD5 && data[position + 1] == 0xAA && data[position + 2] == 0x96)
                    {
                        // Epilogue not in correct position
                        if(data[position + 8] != 0xDE || data[position + 9] != 0xAA) return null;

                        sector = new RawSector();
                        sector.addressField = new RawAddressField();
                        sector.addressField.prologue = new byte[3];
                        sector.addressField.prologue[0] = data[position];
                        sector.addressField.prologue[1] = data[position + 1];
                        sector.addressField.prologue[2] = data[position + 2];
                        sector.addressField.track = data[position + 3];
                        sector.addressField.sector = data[position + 4];
                        sector.addressField.side = data[position + 5];
                        sector.addressField.format = (AppleEncodedFormat)data[position + 6];
                        sector.addressField.checksum = data[position + 7];
                        sector.addressField.epilogue = new byte[2];
                        sector.addressField.epilogue[0] = data[position + 8];
                        sector.addressField.epilogue[1] = data[position + 9];

                        position += 10;
                        syncCount = 0;
                        onSync = false;
                        gaps = new MemoryStream();

                        while(data[position] == 0xFF)
                        {
                            gaps.WriteByte(data[position]);
                            syncCount++;
                            onSync = syncCount >= 5;
                            position++;
                        }

                        // Lost sync
                        if(!onSync) return null;

                        // Prologue not found
                        if(data[position] != 0xDE || data[position + 1] != 0xAA || data[position + 2] != 0xAD)
                            return null;

                        sector.innerGap = gaps.ToArray();
                        sector.dataField = new RawDataField();
                        sector.dataField.prologue = new byte[3];
                        sector.dataField.prologue[0] = data[position];
                        sector.dataField.prologue[1] = data[position + 1];
                        sector.dataField.prologue[2] = data[position + 2];
                        sector.dataField.spare = data[position + 3];
                        position += 4;

                        gaps = new MemoryStream();
                        // Read data until epilogue is found
                        while(data[position + 4] != 0xD5 || data[position + 5] != 0xAA)
                        {
                            gaps.WriteByte(data[position]);
                            position++;

                            // No space left for epilogue
                            if(position + 7 > data.Length) return null;
                        }

                        sector.dataField.data = gaps.ToArray();
                        sector.dataField.checksum = new byte[4];
                        sector.dataField.checksum[0] = data[position];
                        sector.dataField.checksum[1] = data[position + 2];
                        sector.dataField.checksum[2] = data[position + 3];
                        sector.dataField.checksum[3] = data[position + 4];
                        sector.dataField.epilogue = new byte[2];
                        sector.dataField.epilogue[0] = data[position + 5];
                        sector.dataField.epilogue[1] = data[position + 6];

                        position += 7;
                        gaps = new MemoryStream();
                        // Read gap, if any
                        while(position < data.Length && data[position] == 0xFF)
                        {
                            gaps.WriteByte(data[position]);
                            position++;
                        }

                        // Reduces last sector gap so doesn't eat next tracks's gap
                        if(gaps.Length > 5)
                        {
                            gaps.SetLength(gaps.Length / 2);
                            position -= (int)gaps.Length;
                        }

                        sector.gap = gaps.ToArray();
                        // Return current position to be able to read separate sectors
                        endOffset = position;
                        return sector;
                    }

                    if(data[position] == 0xFF) position++;
                    // Found data that is not sync or a prologue
                    else return null;
                }
            }
            catch(IndexOutOfRangeException) { return null; }

            return null;
        }

        public static byte[] MarshalAddressField(RawAddressField addressField)
        {
            if(addressField == null) return null;

            MemoryStream raw = new MemoryStream();
            raw.Write(addressField.prologue, 0, addressField.prologue.Length);
            raw.WriteByte(addressField.track);
            raw.WriteByte(addressField.sector);
            raw.WriteByte(addressField.side);
            raw.WriteByte((byte)addressField.format);
            raw.WriteByte(addressField.checksum);

            return raw.ToArray();
        }

        public static byte[] MarshalSector(RawSector sector)
        {
            if(sector == null) return null;

            MemoryStream raw = new MemoryStream();
            raw.Write(sector.addressField.prologue, 0, sector.addressField.prologue.Length);
            raw.WriteByte(sector.addressField.track);
            raw.WriteByte(sector.addressField.sector);
            raw.WriteByte(sector.addressField.side);
            raw.WriteByte((byte)sector.addressField.format);
            raw.WriteByte(sector.addressField.checksum);
            raw.Write(sector.innerGap, 0, sector.innerGap.Length);
            raw.Write(sector.dataField.prologue, 0, sector.dataField.prologue.Length);
            raw.WriteByte(sector.dataField.spare);
            raw.Write(sector.dataField.data, 0, sector.dataField.data.Length);
            raw.Write(sector.dataField.checksum, 0, sector.dataField.checksum.Length);
            raw.Write(sector.dataField.epilogue, 0, sector.dataField.epilogue.Length);
            raw.Write(sector.gap, 0, sector.gap.Length);

            return raw.ToArray();
        }

        public static RawTrack MarshalTrack(byte[] data, int offset = 0)
        {
            int temp;
            return MarshalTrack(data, out temp, offset);
        }

        public static RawTrack MarshalTrack(byte[] data, out int endOffset, int offset = 0)
        {
            int position = offset;
            bool firstSector = true;
            bool onSync = false;
            MemoryStream gaps = new MemoryStream();
            int count = 0;
            List<RawSector> sectors = new List<RawSector>();
            byte trackNumber = 0;
            byte sideNumber = 0;
            endOffset = offset;

            while(position < data.Length && data[position] == 0xFF)
            {
                gaps.WriteByte(data[position]);
                count++;
                position++;
                onSync = count >= 5;
            }

            if(position >= data.Length) return null;

            if(!onSync) return null;

            while(position < data.Length)
            {
                int oldPosition = position;
                RawSector sector = MarshalSector(data, out position, position);
                if(sector == null) break;

                if(firstSector)
                {
                    trackNumber = sector.addressField.track;
                    sideNumber = sector.addressField.side;
                    firstSector = false;
                }

                if(sector.addressField.track != trackNumber || sector.addressField.side != sideNumber)
                {
                    position = oldPosition;
                    break;
                }

                sectors.Add(sector);
            }

            if(sectors.Count == 0) return null;

            RawTrack track = new RawTrack();
            track.gap = gaps.ToArray();
            track.sectors = sectors.ToArray();
            endOffset = position;
            return track;
        }

        public static byte[] MarshalTrack(RawTrack track)
        {
            if(track == null) return null;

            MemoryStream raw = new MemoryStream();
            raw.Write(track.gap, 0, track.gap.Length);
            foreach(byte[] rawSector in track.sectors.Select(sector => MarshalSector(sector)))
            { raw.Write(rawSector, 0, rawSector.Length); }

            return raw.ToArray();
        }

        public static List<RawTrack> MarshalDisk(byte[] data, int offset = 0)
        {
            int temp;
            return MarshalDisk(data, out temp, offset);
        }

        public static List<RawTrack> MarshalDisk(byte[] data, out int endOffset, int offset = 0)
        {
            endOffset = offset;
            List<RawTrack> tracks = new List<RawTrack>();
            int position = offset;

            RawTrack track = MarshalTrack(data, out position, position);
            while(track != null)
            {
                tracks.Add(track);
                track = MarshalTrack(data, out position, position);
            }

            if(tracks.Count == 0) return null;

            endOffset = position;
            return tracks;
        }

        public static byte[] MarshalDisk(List<RawTrack> disk)
        {
            return MarshalDisk(disk.ToArray());
        }

        public static byte[] MarshalDisk(RawTrack[] disk)
        {
            if(disk == null) return null;

            MemoryStream raw = new MemoryStream();
            foreach(byte[] rawTrack in disk.Select(track => MarshalTrack(track))) { raw.Write(rawTrack, 0, rawTrack.Length); }

            return raw.ToArray();
        }

        public static bool IsAppleSonyGCR(byte[] data)
        {
            int position;
            RawSector sector = MarshalSector(data, out position, 0);

            return sector != null && position != 0;
        }
    }
}