// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads SuperCardPro flux images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public partial class SuperCardPro
    {
        public bool Open(IFilter imageFilter)
        {
            Header    = new ScpHeader();
            scpStream = imageFilter.GetDataForkStream();
            scpStream.Seek(0, SeekOrigin.Begin);

            if(scpStream.Length < Marshal.SizeOf<ScpHeader>())
                return false;

            byte[] hdr = new byte[Marshal.SizeOf<ScpHeader>()];
            scpStream.Read(hdr, 0, Marshal.SizeOf<ScpHeader>());

            Header = Marshal.ByteArrayToStructureLittleEndian<ScpHeader>(hdr);

            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.signature = \"{0}\"",
                                       StringHandlers.CToString(Header.signature));

            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.version = {0}.{1}", (Header.version & 0xF0) >> 4,
                                       Header.version & 0xF);

            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.type = {0}", Header.type);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.revolutions = {0}", Header.revolutions);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.start = {0}", Header.start);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.end = {0}", Header.end);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.flags = {0}", Header.flags);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.bitCellEncoding = {0}", Header.bitCellEncoding);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.heads = {0}", Header.heads);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.reserved = {0}", Header.reserved);
            AaruConsole.DebugWriteLine("SuperCardPro plugin", "header.checksum = 0x{0:X8}", Header.checksum);

            if(!scpSignature.SequenceEqual(Header.signature))
                return false;

            ScpTracks = new Dictionary<byte, TrackHeader>();

            for(byte t = Header.start; t <= Header.end; t++)
            {
                if(t >= Header.offsets.Length)
                    break;

                scpStream.Position = Header.offsets[t];

                var trk = new TrackHeader
                {
                    Signature = new byte[3],
                    Entries   = new TrackEntry[Header.revolutions]
                };

                scpStream.Read(trk.Signature, 0, trk.Signature.Length);
                trk.TrackNumber = (byte)scpStream.ReadByte();

                if(!trk.Signature.SequenceEqual(trkSignature))
                {
                    AaruConsole.DebugWriteLine("SuperCardPro plugin",
                                               "Track header at {0} contains incorrect signature.", Header.offsets[t]);

                    continue;
                }

                if(trk.TrackNumber != t)
                {
                    AaruConsole.DebugWriteLine("SuperCardPro plugin", "Track number at {0} should be {1} but is {2}.",
                                               Header.offsets[t], t, trk.TrackNumber);

                    continue;
                }

                AaruConsole.DebugWriteLine("SuperCardPro plugin", "Found track {0} at {1}.", t, Header.offsets[t]);

                for(byte r = 0; r < Header.revolutions; r++)
                {
                    byte[] rev = new byte[Marshal.SizeOf<TrackEntry>()];
                    scpStream.Read(rev, 0, Marshal.SizeOf<TrackEntry>());

                    trk.Entries[r] = Marshal.ByteArrayToStructureLittleEndian<TrackEntry>(rev);

                    // De-relative offsets
                    trk.Entries[r].dataOffset += Header.offsets[t];
                }

                ScpTracks.Add(t, trk);
            }

            if(Header.flags.HasFlag(ScpFlags.HasFooter))
            {
                long position = scpStream.Position;
                scpStream.Seek(-4, SeekOrigin.End);

                while(scpStream.Position >= position)
                {
                    byte[] footerSig = new byte[4];
                    scpStream.Read(footerSig, 0, 4);
                    uint footerMagic = BitConverter.ToUInt32(footerSig, 0);

                    if(footerMagic == FOOTER_SIGNATURE)
                    {
                        scpStream.Seek(-Marshal.SizeOf<ScpFooter>(), SeekOrigin.Current);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "Found footer at {0}", scpStream.Position);

                        byte[] ftr = new byte[Marshal.SizeOf<ScpFooter>()];
                        scpStream.Read(ftr, 0, Marshal.SizeOf<ScpFooter>());

                        ScpFooter footer = Marshal.ByteArrayToStructureLittleEndian<ScpFooter>(ftr);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.manufacturerOffset = 0x{0:X8}",
                                                   footer.manufacturerOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.modelOffset = 0x{0:X8}",
                                                   footer.modelOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.serialOffset = 0x{0:X8}",
                                                   footer.serialOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.creatorOffset = 0x{0:X8}",
                                                   footer.creatorOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationOffset = 0x{0:X8}",
                                                   footer.applicationOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.commentsOffset = 0x{0:X8}",
                                                   footer.commentsOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.creationTime = {0}",
                                                   footer.creationTime);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.modificationTime = {0}",
                                                   footer.modificationTime);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.applicationVersion = {0}.{1}",
                                                   (footer.applicationVersion & 0xF0) >> 4,
                                                   footer.applicationVersion & 0xF);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.hardwareVersion = {0}.{1}",
                                                   (footer.hardwareVersion & 0xF0) >> 4, footer.hardwareVersion & 0xF);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.firmwareVersion = {0}.{1}",
                                                   (footer.firmwareVersion & 0xF0) >> 4, footer.firmwareVersion & 0xF);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.imageVersion = {0}.{1}",
                                                   (footer.imageVersion & 0xF0) >> 4, footer.imageVersion & 0xF);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "footer.signature = \"{0}\"",
                                                   StringHandlers.CToString(BitConverter.GetBytes(footer.signature)));

                        imageInfo.DriveManufacturer = ReadPStringUtf8(scpStream, footer.manufacturerOffset);
                        imageInfo.DriveModel        = ReadPStringUtf8(scpStream, footer.modelOffset);
                        imageInfo.DriveSerialNumber = ReadPStringUtf8(scpStream, footer.serialOffset);
                        imageInfo.Creator           = ReadPStringUtf8(scpStream, footer.creatorOffset);
                        imageInfo.Application       = ReadPStringUtf8(scpStream, footer.applicationOffset);
                        imageInfo.Comments          = ReadPStringUtf8(scpStream, footer.commentsOffset);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveManufacturer = \"{0}\"",
                                                   imageInfo.DriveManufacturer);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveModel = \"{0}\"",
                                                   imageInfo.DriveModel);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.driveSerialNumber = \"{0}\"",
                                                   imageInfo.DriveSerialNumber);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreator = \"{0}\"",
                                                   imageInfo.Creator);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageApplication = \"{0}\"",
                                                   imageInfo.Application);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageComments = \"{0}\"",
                                                   imageInfo.Comments);

                        imageInfo.CreationTime = footer.creationTime != 0
                                                     ? DateHandlers.UnixToDateTime(footer.creationTime)
                                                     : imageFilter.GetCreationTime();

                        imageInfo.LastModificationTime = footer.modificationTime != 0
                                                             ? DateHandlers.UnixToDateTime(footer.modificationTime)
                                                             : imageFilter.GetLastWriteTime();

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageCreationTime = {0}",
                                                   imageInfo.CreationTime);

                        AaruConsole.DebugWriteLine("SuperCardPro plugin", "ImageInfo.imageLastModificationTime = {0}",
                                                   imageInfo.LastModificationTime);

                        imageInfo.ApplicationVersion =
                            $"{(footer.applicationVersion & 0xF0) >> 4}.{footer.applicationVersion & 0xF}";

                        imageInfo.DriveFirmwareRevision =
                            $"{(footer.firmwareVersion & 0xF0) >> 4}.{footer.firmwareVersion & 0xF}";

                        imageInfo.Version = $"{(footer.imageVersion & 0xF0) >> 4}.{footer.imageVersion & 0xF}";

                        break;
                    }

                    scpStream.Seek(-8, SeekOrigin.Current);
                }
            }
            else
            {
                imageInfo.Application          = "SuperCardPro";
                imageInfo.ApplicationVersion   = $"{(Header.version & 0xF0) >> 4}.{Header.version & 0xF}";
                imageInfo.CreationTime         = imageFilter.GetCreationTime();
                imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
                imageInfo.Version              = "1.5";
            }

            throw new NotImplementedException("Flux decoding is not yet implemented.");
        }

        public byte[] ReadDiskTag(MediaTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        public byte[] ReadSectors(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        public byte[] ReadSectorLong(ulong sectorAddress) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length) =>
            throw new NotImplementedException("Flux decoding is not yet implemented.");
    }
}