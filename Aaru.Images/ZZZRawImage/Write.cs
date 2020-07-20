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
//     Writes raw image, that is, user data sector by sector copy.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public partial class ZZZRawImage
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize == 0)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

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

            basepath  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            mediaTags = new Dictionary<MediaTagType, byte[]>();

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

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

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!SupportedMediaTags.Contains(tag))
            {
                ErrorMessage = $"Tried to write unsupported media tag {tag}.";

                return false;
            }

            if(mediaTags.ContainsKey(tag))
                mediaTags.Remove(tag);

            mediaTags.Add(tag, data);

            return true;
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            writingStream.Seek((long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            writingStream.Seek((long)(sectorAddress * imageInfo.SectorSize), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

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

        public bool SetTracks(List<Track> tracks)
        {
            if(tracks.Count <= 1)
                return true;

            ErrorMessage = "This format supports only 1 track";

            return false;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            writingStream.Flush();
            writingStream.Close();
            IsWriting = false;

            foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
            {
                string suffix = readWriteSidecars.Concat(writeOnlySidecars).Where(t => t.tag == tag.Key).
                                                  Select(t => t.name).FirstOrDefault();

                if(suffix == null)
                    continue;

                var tagStream = new FileStream(basepath + suffix, FileMode.Create, FileAccess.ReadWrite,
                                               FileShare.None);

                tagStream.Write(tag.Value, 0, tag.Value.Length);
                tagStream.Close();
            }

            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;
    }
}