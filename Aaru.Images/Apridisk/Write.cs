// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Write.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes Apridisk disk images.
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.DiscImages
{
    public partial class Apridisk
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";

                return false;
            }

            imageInfo = new ImageInfo
            {
                MediaType  = mediaType,
                SectorSize = sectorSize,
                Sectors    = sectors
            };

            try
            {
                writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";

            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

            if(cylinder >= sectorsData.Length)
            {
                ErrorMessage = "Sector address not found";

                return false;
            }

            if(head >= sectorsData[cylinder].Length)
            {
                ErrorMessage = "Sector address not found";

                return false;
            }

            if(sector > sectorsData[cylinder][head].Length)
            {
                ErrorMessage = "Sector address not found";

                return false;
            }

            sectorsData[cylinder][head][sector] = data;

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            for(uint i = 0; i < length; i++)
            {
                (ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

                if(cylinder >= sectorsData.Length)
                {
                    ErrorMessage = "Sector address not found";

                    return false;
                }

                if(head >= sectorsData[cylinder].Length)
                {
                    ErrorMessage = "Sector address not found";

                    return false;
                }

                if(sector > sectorsData[cylinder][head].Length)
                {
                    ErrorMessage = "Sector address not found";

                    return false;
                }

                sectorsData[cylinder][head][sector] = data;
            }

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        // TODO: Try if apridisk software supports finding other chunks, to extend metadata support
        public bool Close()
        {
            writingStream.Seek(0, SeekOrigin.Begin);
            writingStream.Write(signature, 0, signature.Length);

            byte[] hdr = new byte[Marshal.SizeOf<ApridiskRecord>()];

            for(ushort c = 0; c < imageInfo.Cylinders; c++)
            {
                for(byte h = 0; h < imageInfo.Heads; h++)
                {
                    for(byte s = 0; s < imageInfo.SectorsPerTrack; s++)
                    {
                        if(sectorsData[c][h][s]        == null ||
                           sectorsData[c][h][s].Length == 0)
                            continue;

                        var record = new ApridiskRecord
                        {
                            type        = RecordType.Sector,
                            compression = CompressType.Uncompresed,
                            headerSize  = (ushort)Marshal.SizeOf<ApridiskRecord>(),
                            dataSize    = (uint)sectorsData[c][h][s].Length,
                            head        = h,
                            sector      = s,
                            cylinder    = c
                        };

                        MemoryMarshal.Write(hdr, ref record);

                        writingStream.Write(hdr, 0, hdr.Length);
                        writingStream.Write(sectorsData[c][h][s], 0, sectorsData[c][h][s].Length);
                    }
                }
            }

            if(!string.IsNullOrEmpty(imageInfo.Creator))
            {
                byte[] creatorBytes = Encoding.UTF8.GetBytes(imageInfo.Creator);

                var creatorRecord = new ApridiskRecord
                {
                    type        = RecordType.Creator,
                    compression = CompressType.Uncompresed,
                    headerSize  = (ushort)Marshal.SizeOf<ApridiskRecord>(),
                    dataSize    = (uint)creatorBytes.Length + 1,
                    head        = 0,
                    sector      = 0,
                    cylinder    = 0
                };

                MemoryMarshal.Write(hdr, ref creatorRecord);

                writingStream.Write(hdr, 0, hdr.Length);
                writingStream.Write(creatorBytes, 0, creatorBytes.Length);
                writingStream.WriteByte(0); // Termination
            }

            if(!string.IsNullOrEmpty(imageInfo.Comments))
            {
                byte[] commentBytes = Encoding.UTF8.GetBytes(imageInfo.Comments);

                var commentRecord = new ApridiskRecord
                {
                    type        = RecordType.Comment,
                    compression = CompressType.Uncompresed,
                    headerSize  = (ushort)Marshal.SizeOf<ApridiskRecord>(),
                    dataSize    = (uint)commentBytes.Length + 1,
                    head        = 0,
                    sector      = 0,
                    cylinder    = 0
                };

                MemoryMarshal.Write(hdr, ref commentRecord);

                writingStream.Write(hdr, 0, hdr.Length);
                writingStream.Write(commentBytes, 0, commentBytes.Length);
                writingStream.WriteByte(0); // Termination
            }

            writingStream.Flush();
            writingStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            imageInfo.Comments = metadata.Comments;
            imageInfo.Creator  = metadata.Creator;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > ushort.MaxValue)
            {
                ErrorMessage = "Too many cylinders";

                return false;
            }

            if(heads > byte.MaxValue)
            {
                ErrorMessage = "Too many heads";

                return false;
            }

            if(sectorsPerTrack > byte.MaxValue)
            {
                ErrorMessage = "Too many sectors per track";

                return false;
            }

            sectorsData = new byte[cylinders][][][];

            for(ushort c = 0; c < cylinders; c++)
            {
                sectorsData[c] = new byte[heads][][];

                for(byte h = 0; h < heads; h++)
                    sectorsData[c][h] = new byte[sectorsPerTrack][];
            }

            imageInfo.Cylinders       = cylinders;
            imageInfo.Heads           = heads;
            imageInfo.SectorsPerTrack = sectorsPerTrack;

            return true;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        public bool SetCicmMetadata(CICMMetadataType metadata) => false;
    }
}