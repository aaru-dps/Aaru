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
//     Writes CopyTape tape images.
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
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages.CopyTape
{
    public partial class CopyTape
    {
        FileStream               dataStream;
        ulong                    lastWrittenBlock;
        Dictionary<ulong, ulong> writtenBlockPositions;

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
                MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors
            };

            try
            {
                dataStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            imageInfo.MediaType = mediaType;

            IsWriting             = true;
            IsTape                = false;
            ErrorMessage          = null;
            lastWrittenBlock      = 0;
            writtenBlockPositions = new Dictionary<ulong, ulong>();

            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!writtenBlockPositions.TryGetValue(sectorAddress, out ulong position))
            {
                if(dataStream.Length != 0 &&
                   lastWrittenBlock  >= sectorAddress)
                {
                    ErrorMessage = "Cannot write unwritten blocks";

                    return false;
                }

                if(lastWrittenBlock + 1 != sectorAddress &&
                   sectorAddress        != 0             &&
                   lastWrittenBlock     != 0)
                {
                    ErrorMessage = "Cannot skip blocks";

                    return false;
                }
            }
            else
                dataStream.Position = (long)position;

            byte[] header = Encoding.ASCII.GetBytes($"CPTP:BLK {data.Length:D6}\n");

            writtenBlockPositions[sectorAddress] = (ulong)dataStream.Position;
            dataStream.Write(header, 0, header.Length);
            dataStream.Write(data, 0, data.Length);
            dataStream.WriteByte(0x0A);

            if(sectorAddress > lastWrittenBlock)
                lastWrittenBlock = sectorAddress;

            dataStream.Seek(0, SeekOrigin.End);

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            for(uint i = 0; i < length; i++)
            {
                bool ret = WriteSector(data, sectorAddress + i);

                if(!ret)
                    return false;
            }

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            byte[] footer = Encoding.ASCII.GetBytes("CPTP:EOT\n");

            dataStream.Write(footer, 0, footer.Length);
            dataStream.Flush();
            dataStream.Close();

            IsWriting = false;

            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            ErrorMessage = "Unsupported feature";

            return false;
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

        public bool AddFile(TapeFile file)
        {
            if(file.Partition != 0)
            {
                ErrorMessage = "Unsupported feature";

                return false;
            }

            byte[] marker = Encoding.ASCII.GetBytes("CPTP:MRK\n");

            dataStream.Write(marker, 0, marker.Length);

            return true;
        }

        public bool AddPartition(TapePartition partition)
        {
            if(partition.Number == 0)
                return true;

            ErrorMessage = "Unsupported feature";

            return false;
        }

        public bool SetTape()
        {
            IsTape = true;

            return true;
        }
    }
}