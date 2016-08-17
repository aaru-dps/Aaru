// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages BlindWrite 4 disc images.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using System.Linq;
using System.Text;
using DiscImageChef.Console;
using System.Runtime.InteropServices;
using System.Globalization;

namespace DiscImageChef.ImagePlugins
{
    class BlindWrite4 : ImagePlugin
    {
        #region Internal Constants
        /// <summary>"BLINDWRITE TOC FILE"</summary>
        readonly byte[] BW4_Signature = { 0x42, 0x4C, 0x49, 0x4E, 0x44, 0x57, 0x52, 0x49, 0x54, 0x45, 0x20, 0x54, 0x4F, 0x43, 0x20, 0x46, 0x49, 0x4C, 0x45 };
        #endregion Internal Constants

        #region Internal Structures
        struct BW4_Header
        {
            public byte[] signature;
            public uint unknown1;
            public ulong timestamp;
            public uint volumeIdLength;
            public byte[] volumeIdBytes;
            public uint sysIdLength;
            public byte[] sysIdBytes;
            public uint commentsLength;
            public byte[] commentsBytes;
            public uint trackDescriptors;
            public uint dataFileLength;
            public byte[] dataFileBytes;
            public uint subchannelFileLength;
            public byte[] subchannelFileBytes;
            public uint unknown2;
            public byte unknown3;
            public byte[] unknown4;

            // On memory only
            public string volumeIdentifier;
            public string systemIdentifier;
            public string comments;
            public string dataFile;
            public string subchannelFile;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BW4_TrackDescriptor
        {
            public uint filenameLen;
            public byte[] filenameBytes;
            public uint offset;
            public byte subchannel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unknown1;
            public uint unknown2;
            public byte unknown3;
            public byte session;
            public byte unknown4;
            public byte adrCtl;
            public byte unknown5;
            public BW4_TrackType trackMode;
            public byte unknown6;
            public byte point;
            public uint unknown7;
            public uint unknown8;
            public uint unknown9;
            public uint unknown10;
            public ushort unknown11;
            public uint lastSector;
            public byte unknown12;
            public int pregap;
            public int startSector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] unknown13;
            public uint titleLen;
            public byte[] titleBytes;
            public uint performerLen;
            public byte[] performerBytes;
            public uint unkStrLen1;
            public byte[] unkStrBytes1;
            public uint unkStrLen2;
            public byte[] unkStrBytes2;
            public uint unkStrLen3;
            public byte[] unkStrBytes3;
            public uint unkStrLen4;
            public byte[] unkStrBytes4;
            public uint discIdLen;
            public byte[] discIdBytes;
            public uint unkStrLen5;
            public byte[] unkStrBytes5;
            public uint unkStrLen6;
            public byte[] unkStrBytes6;
            public uint unkStrLen7;
            public byte[] unkStrBytes7;
            public uint unkStrLen8;
            public byte[] unkStrBytes8;
            public uint unkStrLen9;
            public byte[] unkStrBytes9;
            public uint unkStrLen10;
            public byte[] unkStrBytes10;
            public uint unkStrLen11;
            public byte[] unkStrBytes11;
            public uint isrcLen;
            public byte[] isrcBytes;

            // On memory only
            public string filename;
            public string title;
            public string performer;
            public string unkString1;
            public string unkString2;
            public string unkString3;
            public string unkString4;
            public string discId;
            public string unkString5;
            public string unkString6;
            public string unkString7;
            public string unkString8;
            public string unkString9;
            public string unkString10;
            public string unkString11;
            public string isrcUpc;
        }
        #endregion Internal Structures

        #region Internal enumerations
        enum BW4_TrackType : byte
        {
            Audio = 0,
            Mode1 = 1,
            Mode2 = 2
        }
        #endregion Internal enumerations


        #region Internal variables
        BW4_Header header;
        List<BW4_TrackDescriptor> bwTracks;
        List<Track> tracks;
        Dictionary<uint, ulong> offsetmap;
        List<Partition> partitions;
        List<Session> sessions;
        string dataFile, subFile;
        FileStream imageStream;
        Dictionary<uint, byte> trackFlags;
        #endregion Internal variables

        #region Public Methods
        public BlindWrite4()
        {
            Name = "BlindWrite 4";
            PluginUUID = new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
            ImageInfo = new ImageInfo();
            ImageInfo.readableSectorTags = new List<SectorTagType>();
            ImageInfo.readableMediaTags = new List<MediaTagType>();
            ImageInfo.imageHasPartitions = true;
            ImageInfo.imageHasSessions = true;
            ImageInfo.imageVersion = null;
            ImageInfo.imageApplicationVersion = null;
            ImageInfo.imageName = null;
            ImageInfo.imageCreator = null;
            ImageInfo.mediaManufacturer = null;
            ImageInfo.mediaModel = null;
            ImageInfo.mediaPartNumber = null;
            ImageInfo.mediaSequence = 0;
            ImageInfo.lastMediaSequence = 0;
            ImageInfo.driveManufacturer = null;
            ImageInfo.driveModel = null;
            ImageInfo.driveSerialNumber = null;
        }

        public override bool IdentifyImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 19)
                return false;

            byte[] signature = new byte[19];
            stream.Read(signature, 0, 19);

            stream.Close();

            return BW4_Signature.SequenceEqual(signature);
        }

        public override bool OpenImage(string imagePath)
        {
            FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            if(stream.Length < 19)
                return false;

            byte[] tmpArray = new byte[19];
            byte[] tmpUShort = new byte[2];
            byte[] tmpUInt = new byte[4];
            byte[] tmpULong = new byte[8];

            stream.Read(tmpArray, 0, 19);

            if(!BW4_Signature.SequenceEqual(tmpArray))
                return false;

            header = new BW4_Header();
            header.signature = tmpArray;

            // Seems to always be 2
            stream.Read(tmpUInt, 0, 4);
            header.unknown1 = BitConverter.ToUInt32(tmpUInt, 0);
            // Seems to be a timetamp
            stream.Read(tmpULong, 0, 8);
            header.timestamp = BitConverter.ToUInt64(tmpULong, 0);

            stream.Read(tmpUInt, 0, 4);
            header.volumeIdLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray = new byte[header.volumeIdLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.volumeIdBytes = tmpArray;
            header.volumeIdentifier = StringHandlers.CToString(header.volumeIdBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.sysIdLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray = new byte[header.sysIdLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.sysIdBytes = tmpArray;
            header.systemIdentifier = StringHandlers.CToString(header.sysIdBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.commentsLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray = new byte[header.commentsLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.commentsBytes = tmpArray;
            header.comments = StringHandlers.CToString(header.commentsBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.trackDescriptors = BitConverter.ToUInt32(tmpUInt, 0);

            stream.Read(tmpUInt, 0, 4);
            header.dataFileLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray = new byte[header.dataFileLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.dataFileBytes = tmpArray;
            header.dataFile = StringHandlers.CToString(header.dataFileBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.subchannelFileLength = BitConverter.ToUInt32(tmpUInt, 0);
            tmpArray = new byte[header.subchannelFileLength];
            stream.Read(tmpArray, 0, tmpArray.Length);
            header.subchannelFileBytes = tmpArray;
            header.subchannelFile = StringHandlers.CToString(header.subchannelFileBytes, Encoding.Default);

            stream.Read(tmpUInt, 0, 4);
            header.unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
            header.unknown3 = (byte)stream.ReadByte();
            tmpArray = new byte[header.unknown3];
            stream.Read(tmpArray, 0, header.unknown3);
            header.unknown4 = tmpArray;

            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.signature = {0}", StringHandlers.CToString(header.signature));
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown1 = {0}", header.unknown1);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.timestamp = {0}", header.timestamp);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.volumeIdLength = {0}", header.volumeIdLength);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.volumeIdentifier = {0}", header.volumeIdentifier);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.sysIdLength = {0}", header.sysIdLength);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.systemIdentifier = {0}", header.systemIdentifier);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.commentsLength = {0}", header.commentsLength);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.comments = {0}", header.comments);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.trackDescriptors = {0}", header.trackDescriptors);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.dataFileLength = {0}", header.dataFileLength);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.dataFile = {0}", header.dataFile);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.subchannelFileLength = {0}", header.subchannelFileLength);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.subchannelFile = {0}", header.subchannelFile);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown2 = {0}", header.unknown2);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown3 = {0}", header.unknown3);
            DicConsole.DebugWriteLine("BlindWrite4 plugin", "header.unknown4.Length = {0}", header.unknown4.Length);

            bwTracks = new List<BW4_TrackDescriptor>();

            for(int i = 0; i < header.trackDescriptors; i++)
            {
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "stream.Position = {0}", stream.Position);

                BW4_TrackDescriptor track = new BW4_TrackDescriptor();

                stream.Read(tmpUInt, 0, 4);
                track.filenameLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.filenameLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.filenameBytes = tmpArray;
                track.filename = StringHandlers.CToString(track.filenameBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.offset = BitConverter.ToUInt32(tmpUInt, 0);
                track.subchannel = (byte)stream.ReadByte();

                tmpArray = new byte[3];
                stream.Read(tmpArray, 0, 3);
                track.unknown1 = tmpArray;

                stream.Read(tmpUInt, 0, 4);
                track.unknown2 = BitConverter.ToUInt32(tmpUInt, 0);
                track.unknown3 = (byte)stream.ReadByte();
                track.session = (byte)stream.ReadByte();
                track.unknown4 = (byte)stream.ReadByte();
                track.adrCtl = (byte)stream.ReadByte();

                track.unknown5 = (byte)stream.ReadByte();
                track.trackMode = (BW4_TrackType)stream.ReadByte();
                track.unknown6 = (byte)stream.ReadByte();
                track.point = (byte)stream.ReadByte();

                stream.Read(tmpUInt, 0, 4);
                track.unknown7 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown8 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown9 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.unknown10 = BitConverter.ToUInt32(tmpUInt, 0);
                stream.Read(tmpUShort, 0, 2);
                track.unknown11 = BitConverter.ToUInt16(tmpUShort, 0);
                stream.Read(tmpUInt, 0, 4);
                track.lastSector = BitConverter.ToUInt32(tmpUInt, 0);
                track.unknown12 = (byte)stream.ReadByte();

                stream.Read(tmpUInt, 0, 4);
                track.pregap = BitConverter.ToInt32(tmpUInt, 0);
                stream.Read(tmpUInt, 0, 4);
                track.startSector = BitConverter.ToInt32(tmpUInt, 0);

                track.unknown13 = new uint[2];
                for(int j = 0; j < track.unknown13.Length; j++)
                {
                    stream.Read(tmpUInt, 0, 4);
                    track.unknown13[j] = BitConverter.ToUInt32(tmpUInt, 0);
                }

                stream.Read(tmpUInt, 0, 4);
                track.titleLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.titleLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.titleBytes = tmpArray;
                track.title = StringHandlers.CToString(track.titleBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.performerLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.performerLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.performerBytes = tmpArray;
                track.performer = StringHandlers.CToString(track.performerBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen1 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen1];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes1 = tmpArray;
                track.unkString1 = StringHandlers.CToString(track.unkStrBytes1, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen2 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen2];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes2 = tmpArray;
                track.unkString2 = StringHandlers.CToString(track.unkStrBytes2, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen3 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen3];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes3 = tmpArray;
                track.unkString3 = StringHandlers.CToString(track.unkStrBytes3, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen4 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen4];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes4 = tmpArray;
                track.unkString4 = StringHandlers.CToString(track.unkStrBytes4, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.discIdLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.discIdLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.discIdBytes = tmpArray;
                track.discId = StringHandlers.CToString(track.discIdBytes, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen5 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen5];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes5 = tmpArray;
                track.unkString5 = StringHandlers.CToString(track.unkStrBytes5, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen6 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen6];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes6 = tmpArray;
                track.unkString6 = StringHandlers.CToString(track.unkStrBytes6, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen7 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen7];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes7 = tmpArray;
                track.unkString7 = StringHandlers.CToString(track.unkStrBytes7, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen8 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen8];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes8 = tmpArray;
                track.unkString8 = StringHandlers.CToString(track.unkStrBytes8, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen9 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen9];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes9 = tmpArray;
                track.unkString9 = StringHandlers.CToString(track.unkStrBytes9, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen10 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen10];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes10 = tmpArray;
                track.unkString10 = StringHandlers.CToString(track.unkStrBytes10, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.unkStrLen11 = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.unkStrLen11];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.unkStrBytes11 = tmpArray;
                track.unkString11 = StringHandlers.CToString(track.unkStrBytes11, Encoding.Default);

                stream.Read(tmpUInt, 0, 4);
                track.isrcLen = BitConverter.ToUInt32(tmpUInt, 0);
                tmpArray = new byte[track.isrcLen];
                stream.Read(tmpArray, 0, tmpArray.Length);
                track.isrcBytes = tmpArray;
                track.isrcUpc = StringHandlers.CToString(track.isrcBytes, Encoding.Default);

                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.filenameLen = {0}", track.filenameLen);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.filename = {0}", track.filename);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.offset = {0}", track.offset);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.subchannel = {0}", track.subchannel);
                for(int j = 0; j < track.unknown1.Length; j++)
                    DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown1[{1}] = 0x{0:X8}", track.unknown1[j], j);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown2 = {0}", track.unknown2);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown3 = {0}", track.unknown3);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.session = {0}", track.session);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown4 = {0}", track.unknown4);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.adrCtl = {0}", track.adrCtl);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown5 = {0}", track.unknown5);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.trackMode = {0}", track.trackMode);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown6 = {0}", track.unknown6);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.point = {0}", track.point);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown7 = {0}", track.unknown7);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown8 = {0}", track.unknown8);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown9 = {0}", track.unknown9);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown10 = {0}", track.unknown10);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown11 = {0}", track.unknown11);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.lastSector = {0}", track.lastSector);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown12 = {0}", track.unknown12);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.pregap = {0}", track.pregap);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.startSector = {0}", track.startSector);
                for(int j = 0; j < track.unknown13.Length; j++)
                    DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unknown13[{1}] = 0x{0:X8}", track.unknown13[j], j);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.titleLen = {0}", track.titleLen);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.title = {0}", track.title);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.performerLen = {0}", track.performerLen);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.performer = {0}", track.performer);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen1 = {0}", track.unkStrLen1);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString1 = {0}", track.unkString1);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen2 = {0}", track.unkStrLen2);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString2 = {0}", track.unkString2);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen3 = {0}", track.unkStrLen3);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString3 = {0}", track.unkString3);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen4 = {0}", track.unkStrLen4);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString4 = {0}", track.unkString4);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.discIdLen = {0}", track.discIdLen);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.discId = {0}", track.discId);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen5 = {0}", track.unkStrLen5);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString5 = {0}", track.unkString5);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen6 = {0}", track.unkStrLen6);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString6 = {0}", track.unkString6);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen7 = {0}", track.unkStrLen7);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString7 = {0}", track.unkString7);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen8 = {0}", track.unkStrLen8);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString8 = {0}", track.unkString8);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen9 = {0}", track.unkStrLen9);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString9 = {0}", track.unkString9);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen10 = {0}", track.unkStrLen10);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString10 = {0}", track.unkString10);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkStrLen11 = {0}", track.unkStrLen11);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.unkString11 = {0}", track.unkString11);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.isrcLen = {0}", track.isrcLen);
                DicConsole.DebugWriteLine("BlindWrite4 plugin", "track.isrcUpc = {0}", track.isrcUpc);

                bwTracks.Add(track);
            }

            if(!string.IsNullOrEmpty(header.dataFile))
            {
                while(true)
                {
                    dataFile = header.dataFile;
                    if(File.Exists(dataFile))
                        break;

                    dataFile = header.dataFile.ToLower(CultureInfo.CurrentCulture);
                    if(File.Exists(dataFile))
                        break;

                    dataFile = header.dataFile.ToUpper(CultureInfo.CurrentCulture);
                    if(File.Exists(dataFile))
                        break;

                    dataFile = header.dataFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if(File.Exists(dataFile))
                        break;

                    dataFile = header.dataFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToLower(CultureInfo.CurrentCulture);
                    if(File.Exists(dataFile))
                        break;

                    dataFile = header.dataFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToUpper(CultureInfo.CurrentCulture);
                    if(File.Exists(dataFile))
                        break;

                    throw new ArgumentException(string.Format("Data file {0} not found", header.dataFile));
                }
            }
            else
                throw new ArgumentException("Unable to find data file");

            if(!string.IsNullOrEmpty(header.dataFile))
            {
                do
                {
                    subFile = header.subchannelFile;
                    if(File.Exists(subFile))
                        break;

                    subFile = header.subchannelFile.ToLower(CultureInfo.CurrentCulture);
                    if(File.Exists(subFile))
                        break;

                    subFile = header.subchannelFile.ToUpper(CultureInfo.CurrentCulture);
                    if(File.Exists(subFile))
                        break;

                    subFile = header.subchannelFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    if(File.Exists(subFile))
                        break;

                    subFile = header.subchannelFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToLower(CultureInfo.CurrentCulture);
                    if(File.Exists(subFile))
                        break;

                    subFile = header.subchannelFile.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToUpper(CultureInfo.CurrentCulture);
                    if(File.Exists(subFile))
                        break;

                    subFile = null;
                }
                while(true);
            }

            tracks = new List<Track>();
            partitions = new List<Partition>();
            offsetmap = new Dictionary<uint, ulong>();
            trackFlags = new Dictionary<uint, byte>();
            ushort maxSession = 0;
            ulong currentPos = 0;
            foreach(BW4_TrackDescriptor bwTrack in bwTracks)
            {
                if(bwTrack.point < 0xA0)
                {
                    Track track = new Track();
                    track.TrackDescription = bwTrack.title;
                    track.TrackEndSector = bwTrack.lastSector;

                    if(!string.IsNullOrEmpty(bwTrack.filename))
                    {
                        do
                        {
                            track.TrackFile = bwTrack.filename;
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = bwTrack.filename.ToLower(CultureInfo.CurrentCulture);
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = bwTrack.filename.ToUpper(CultureInfo.CurrentCulture);
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = bwTrack.filename.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = bwTrack.filename.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToLower(CultureInfo.CurrentCulture);
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = bwTrack.filename.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToUpper(CultureInfo.CurrentCulture);
                            if(File.Exists(track.TrackFile))
                                break;

                            track.TrackFile = dataFile;
                        }
                        while(true);
                    }
                    else
                        track.TrackFile = dataFile;

                    track.TrackFileOffset = bwTrack.offset;
                    if(bwTrack.pregap > 0)
                        track.TrackFileOffset += (ulong)(bwTrack.startSector - bwTrack.pregap) * 2352;
                    track.TrackFileType = "BINARY";
                    track.TrackPregap = (ulong)(bwTrack.startSector - bwTrack.pregap);
                    track.TrackRawBytesPerSector = 2352;
                    track.TrackSequence = bwTrack.point;
                    track.TrackSession = bwTrack.session;
                    if(track.TrackSession > maxSession)
                        maxSession = track.TrackSession;
                    track.TrackStartSector = (ulong)bwTrack.startSector;
                    track.TrackSubchannelFile = subFile;
                    track.TrackSubchannelOffset = track.TrackStartSector / 96;
                    if(!string.IsNullOrEmpty(track.TrackSubchannelFile) && bwTrack.subchannel > 0)
                    {
                        track.TrackSubchannelType = TrackSubchannelType.Packed;
                        if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
                            ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);
                    }
                    else
                        track.TrackSubchannelType = TrackSubchannelType.None;

                    switch(bwTrack.trackMode)
                    {
                        case BW4_TrackType.Audio:
                            track.TrackType = TrackType.Audio;
                            ImageInfo.sectorSize = 2352;
                            track.TrackBytesPerSector = 2352;
                            break;
                        case BW4_TrackType.Mode1:
                            track.TrackType = TrackType.CDMode1;
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                            if(ImageInfo.sectorSize < 2048)
                                ImageInfo.sectorSize = 2048;
                            track.TrackBytesPerSector = 2048;
                            break;
                        case BW4_TrackType.Mode2:
                            track.TrackType = TrackType.CDMode2Formless;
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                            if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                            if(ImageInfo.sectorSize < 2336)
                                ImageInfo.sectorSize = 2336;
                            track.TrackBytesPerSector = 2336;
                            break;
                        default:
                            track.TrackType = TrackType.Data;
                            track.TrackRawBytesPerSector = 2048;
                            ImageInfo.sectorSize = 2048;
                            track.TrackBytesPerSector = 2048;
                            break;
                    }

                    track.Indexes = new Dictionary<int, ulong>();
                    if(bwTrack.pregap > 0)
                        track.Indexes.Add(0, (ulong)bwTrack.pregap);
                    track.Indexes.Add(1, (ulong)bwTrack.startSector);

                    Partition partition = new Partition();
                    if(bwTrack.pregap > 0)
                        currentPos += (ulong)(bwTrack.startSector - bwTrack.pregap) * 2352;
                    partition.PartitionDescription = track.TrackDescription;
                    partition.PartitionLength = (track.TrackEndSector - track.TrackStartSector + 1) * 2352;
                    partition.PartitionSectors = track.TrackEndSector - track.TrackStartSector;
                    partition.PartitionSequence = track.TrackSequence;
                    partition.PartitionStart = currentPos;
                    partition.PartitionStartSector = track.TrackStartSector;
                    partition.PartitionType = track.TrackType.ToString();

                    partitions.Add(partition);
                    tracks.Add(track);
                    currentPos += partition.PartitionLength;

                    if(!offsetmap.ContainsKey(track.TrackSequence))
                        offsetmap.Add(track.TrackSequence, track.TrackStartSector);

                    if(!trackFlags.ContainsKey(track.TrackSequence))
                        trackFlags.Add(track.TrackSequence, (byte)(bwTrack.adrCtl & 0x0F));

                    ImageInfo.sectors += (ulong)(bwTrack.lastSector - bwTrack.startSector + 1);
                }
                else
                {
                    ImageInfo.mediaBarcode = bwTrack.isrcUpc;
                    ImageInfo.mediaSerialNumber = bwTrack.discId;
                    ImageInfo.imageName = bwTrack.title;

                    if(!string.IsNullOrEmpty(bwTrack.isrcUpc) && !ImageInfo.readableMediaTags.Contains(MediaTagType.CD_MCN))
                        ImageInfo.readableMediaTags.Add(MediaTagType.CD_MCN);
                }
            }

            sessions = new List<Session>();
            for(ushort i = 1; i <= maxSession; i++)
            {
                Session session = new Session();
                session.SessionSequence = i;
                session.StartTrack = uint.MaxValue;
                session.StartSector = uint.MaxValue;

                foreach(Track track in tracks)
                {
                    if(track.TrackSession == i)
                    {
                        if(track.TrackSequence < session.StartTrack)
                            session.StartTrack = track.TrackSequence;
                        if(track.TrackSequence > session.EndTrack)
                            session.StartTrack = track.TrackSequence;
                        if(track.TrackStartSector < session.StartSector)
                            session.StartSector = track.TrackStartSector;
                        if(track.TrackEndSector > session.EndSector)
                            session.EndSector = track.TrackEndSector;
                    }
                }

                sessions.Add(session);
            }

            ImageInfo.mediaType = MediaType.CD;

            ImageInfo.imageApplication = "BlindRead 4";

            FileInfo fi = new FileInfo(dataFile);
            ImageInfo.imageSize = (ulong)fi.Length;
            ImageInfo.imageCreationTime = fi.CreationTimeUtc;
            ImageInfo.imageLastModificationTime = fi.LastWriteTimeUtc;
            ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

            bool data = false;
            bool mode2 = false;
            bool firstaudio = false;
            bool firstdata = false;
            bool audio = false;

            foreach(Track _track in tracks)
            {
                // First track is audio
                firstaudio |= _track.TrackSequence == 1 && _track.TrackType == TrackType.Audio;

                // First track is data
                firstdata |= _track.TrackSequence == 1 && _track.TrackType != TrackType.Audio;

                // Any non first track is data
                data |= _track.TrackSequence != 1 && _track.TrackType != TrackType.Audio;

                // Any non first track is audio
                audio |= _track.TrackSequence != 1 && _track.TrackType == TrackType.Audio;

                switch(_track.TrackType)
                {
                    case TrackType.CDMode2Formless:
                        mode2 = true;
                        break;
                }
            }

            if(!data && !firstdata)
                ImageInfo.mediaType = MediaType.CDDA;
            else if(firstaudio && data && sessions.Count > 1 && mode2)
                ImageInfo.mediaType = MediaType.CDPLUS;
            else if((firstdata && audio) || mode2)
                ImageInfo.mediaType = MediaType.CDROMXA;
            else if(!audio)
                ImageInfo.mediaType = MediaType.CDROM;
            else
                ImageInfo.mediaType = MediaType.CD;

            ImageInfo.imageComments = header.comments;

            return true;
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.imageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.imageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.sectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                    {
                        if(ImageInfo.mediaSerialNumber != null)
                        {
                            return Encoding.ASCII.GetBytes(ImageInfo.mediaSerialNumber);
                        }
                        throw new FeatureNotPresentImageException("Image does not contain MCN information.");
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector + 1))
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector + 1))
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector - _track.TrackStartSector + 1))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, (_track.TrackEndSector - _track.TrackStartSector + 1)));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.TrackType)
            {
                case TrackType.CDMode1:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288;
                        break;
                    }
                case TrackType.CDMode2Formless:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.Audio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case TrackType.Data:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = new FileStream(_track.TrackFile, FileMode.Open, FileAccess.Read);
            using(BinaryReader br = new BinaryReader(imageStream))
            {
                br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
                if(sector_offset == 0 && sector_skip == 0)
                    buffer = br.ReadBytes((int)(sector_size * length));
                else
                {
                    for(int i = 0; i < length; i++)
                    {
                        byte[] sector;
                        br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                        sector = br.ReadBytes((int)sector_size);
                        br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                        Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                    }
                }
            }
            imageStream.Close();

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector - _track.TrackStartSector + 1))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, (_track.TrackEndSector - _track.TrackStartSector + 1)));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            if(_track.TrackType == TrackType.Data)
                throw new ArgumentException("Unsupported tag requested", nameof(tag));

            switch(tag)
            {
                case SectorTagType.CDSectorECC:
                case SectorTagType.CDSectorECC_P:
                case SectorTagType.CDSectorECC_Q:
                case SectorTagType.CDSectorEDC:
                case SectorTagType.CDSectorHeader:
                case SectorTagType.CDSectorSubchannel:
                case SectorTagType.CDSectorSubHeader:
                case SectorTagType.CDSectorSync:
                    break;
                case SectorTagType.CDTrackFlags:
                    byte flag;
                    if(trackFlags.TryGetValue(track, out flag))
                        return new byte[] { flag };
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(_track.TrackType)
            {
                case TrackType.CDMode1:
                    switch(tag)
                    {
                        case SectorTagType.CDSectorSync:
                            {
                                sector_offset = 0;
                                sector_size = 12;
                                sector_skip = 2340;
                                break;
                            }
                        case SectorTagType.CDSectorHeader:
                            {
                                sector_offset = 12;
                                sector_size = 4;
                                sector_skip = 2336;
                                break;
                            }
                        case SectorTagType.CDSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CDSectorECC:
                            {
                                sector_offset = 2076;
                                sector_size = 276;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorECC_P:
                            {
                                sector_offset = 2076;
                                sector_size = 172;
                                sector_skip = 104;
                                break;
                            }
                        case SectorTagType.CDSectorECC_Q:
                            {
                                sector_offset = 2248;
                                sector_size = 104;
                                sector_skip = 0;
                                break;
                            }
                        case SectorTagType.CDSectorEDC:
                            {
                                sector_offset = 2064;
                                sector_size = 4;
                                sector_skip = 284;
                                break;
                            }
                        case SectorTagType.CDSectorSubchannel:
                            throw new NotImplementedException("Subchannel interleaving not yet implemented");
                        default:
                            throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }
                    break;
                case TrackType.CDMode2Formless:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSync:
                            case SectorTagType.CDSectorHeader:
                            case SectorTagType.CDSectorECC:
                            case SectorTagType.CDSectorECC_P:
                            case SectorTagType.CDSectorECC_Q:
                                throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                            case SectorTagType.CDSectorSubHeader:
                                {
                                    sector_offset = 0;
                                    sector_size = 8;
                                    sector_skip = 2328;
                                    break;
                                }
                            case SectorTagType.CDSectorEDC:
                                {
                                    sector_offset = 2332;
                                    sector_size = 4;
                                    sector_skip = 0;
                                    break;
                                }
                            case SectorTagType.CDSectorSubchannel:
                                throw new NotImplementedException("Subchannel interleaving not yet implemented");
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                        break;
                    }
                case TrackType.Audio:
                    {
                        switch(tag)
                        {
                            case SectorTagType.CDSectorSubchannel:
                                throw new NotImplementedException("Subchannel interleaving not yet implemented");
                            default:
                                throw new ArgumentException("Unsupported tag requested", nameof(tag));
                        }
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = new FileStream(_track.TrackFile, FileMode.Open, FileAccess.Read);
            using(BinaryReader br = new BinaryReader(imageStream))
            {
                br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
                if(sector_offset == 0 && sector_skip == 0)
                    buffer = br.ReadBytes((int)(sector_size * length));
                else
                {
                    for(int i = 0; i < length; i++)
                    {
                        byte[] sector;
                        br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                        sector = br.ReadBytes((int)sector_size);
                        br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                        Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                    }
                }
            }
            imageStream.Close();

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector + 1))
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track bwTrack in tracks)
            {
                if(bwTrack.TrackSequence == track)
                {
                    _track = bwTrack;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress > (_track.TrackEndSector - _track.TrackStartSector + 1))
                throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, (_track.TrackEndSector - _track.TrackStartSector + 1)));

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.TrackType)
            {
                case TrackType.Audio:
                case TrackType.CDMode1:
                case TrackType.CDMode2Formless:
                case TrackType.Data:
                    {
                        sector_offset = 0;
                        sector_size = (uint)_track.TrackRawBytesPerSector;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = new FileStream(_track.TrackFile, FileMode.Open, FileAccess.Read);
            using(BinaryReader br = new BinaryReader(imageStream))
            {
                br.BaseStream.Seek((long)_track.TrackFileOffset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
                buffer = br.ReadBytes((int)(sector_size * length));
            }
            imageStream.Close();

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "BlindWrite 4 TOC file";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.imageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.imageApplication;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(sessions.Contains(session))
            {
                return GetSessionTracks(session.SessionSequence);
            }
            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> _tracks = new List<Track>();
            foreach(Track _track in tracks)
            {
                if(_track.TrackSession == session)
                    _tracks.Add(_track);
            }

            return _tracks;
        }
        public override List<Session> GetSessions()
        {
            return sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CDChecksums.CheckCDSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            FailingLBAs = new List<ulong>();
            UnknownLBAs = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        UnknownLBAs.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        FailingLBAs.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(UnknownLBAs.Count > 0)
                return null;
            if(FailingLBAs.Count > 0)
                return false;
            return true;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
        #endregion Public Methods

        #region Unsupported features
        public override string GetImageApplicationVersion()
        {
            return ImageInfo.imageApplicationVersion;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.imageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.imageLastModificationTime;
        }

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.mediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.lastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.driveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.driveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.driveSerialNumber;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }
        #endregion Unsupported features
    }
}

