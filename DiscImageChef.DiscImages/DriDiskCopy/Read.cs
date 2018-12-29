// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads Digital Research's DISKCOPY disk images.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class DriDiskCopy
    {
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if((stream.Length - Marshal.SizeOf(typeof(DriFooter))) % 512 != 0) return false;

            byte[] buffer = new byte[Marshal.SizeOf(typeof(DriFooter))];
            stream.Seek(-buffer.Length, SeekOrigin.End);
            stream.Read(buffer, 0, buffer.Length);

            footer = new DriFooter();
            IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
            footer = (DriFooter)Marshal.PtrToStructure(ftrPtr, typeof(DriFooter));
            Marshal.FreeHGlobal(ftrPtr);

            string sig = StringHandlers.CToString(footer.signature);

            Regex regexSignature = new Regex(REGEX_DRI);
            Match matchSignature = regexSignature.Match(sig);

            if(!matchSignature.Success) return false;

            if(footer.bpb.sptrack * footer.bpb.cylinders * footer.bpb.heads != footer.bpb.sectors) return false;

            if(footer.bpb.sectors * footer.bpb.bps + Marshal.SizeOf(footer) != stream.Length) return false;

            imageInfo.Cylinders          = footer.bpb.cylinders;
            imageInfo.Heads              = footer.bpb.heads;
            imageInfo.SectorsPerTrack    = footer.bpb.sptrack;
            imageInfo.Sectors            = footer.bpb.sectors;
            imageInfo.SectorSize         = footer.bpb.bps;
            imageInfo.ApplicationVersion = matchSignature.Groups["version"].Value;

            driImageFilter = imageFilter;

            imageInfo.ImageSize            = (ulong)(stream.Length - Marshal.SizeOf(footer));
            imageInfo.CreationTime         = imageFilter.GetCreationTime();
            imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

            DicConsole.DebugWriteLine("DRI DiskCopy plugin", "Image application = {0} version {1}",
                                      imageInfo.Application, imageInfo.ApplicationVersion);

            // Correct some incorrect data in images of NEC 2HD disks
            if(imageInfo.Cylinders  == 77  && imageInfo.Heads == 2 && imageInfo.SectorsPerTrack == 16 &&
               imageInfo.SectorSize == 512 && (footer.bpb.driveCode == DriDriveCodes.md2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2hd))
            {
                imageInfo.SectorsPerTrack = 8;
                imageInfo.SectorSize      = 1024;
            }

            imageInfo.MediaType = Geometry.GetMediaType(((ushort)imageInfo.Cylinders, (byte)imageInfo.Heads,
                                                         (ushort)imageInfo.SectorsPerTrack, imageInfo.SectorSize,
                                                         MediaEncoding.MFM, false));

            switch(imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when footer.bpb.driveCode == DriDriveCodes.mf2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_8;
                    break;
                case MediaType.DOS_525_HD when footer.bpb.driveCode == DriDriveCodes.mf2hd ||
                                               footer.bpb.driveCode == DriDriveCodes.mf2ed:
                    imageInfo.MediaType = MediaType.NEC_35_HD_15;
                    break;
                case MediaType.RX50 when footer.bpb.driveCode == DriDriveCodes.md2dd ||
                                         footer.bpb.driveCode == DriDriveCodes.md2hd:
                    imageInfo.MediaType = MediaType.ATARI_35_SS_DD;
                    break;
            }

            imageInfo.XmlMediaType = XmlMediaType.BlockMedia;
            DicConsole.VerboseWriteLine("Digital Research DiskCopy image contains a disk of type {0}",
                                        imageInfo.MediaType);

            return true;
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * imageInfo.SectorSize];

            Stream stream = driImageFilter.GetDataForkStream();
            stream.Seek((long)(sectorAddress    * imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * imageInfo.SectorSize));

            return buffer;
        }
    }
}