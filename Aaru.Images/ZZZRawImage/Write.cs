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
// Copyright © 2011-2022 Natalia Portillo
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
    public sealed partial class ZZZRawImage
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(sectorSize == 0)
            {
                ErrorMessage = "Unsupported sector size";

                return false;
            }

            _extension = Path.GetExtension(path)?.ToLower();

            switch(_extension)
            {
                case ".1kn" when sectorSize  != 1024:
                case ".2kn" when sectorSize  != 2048:
                case ".4kn" when sectorSize  != 4096:
                case ".8kn" when sectorSize  != 8192:
                case ".16kn" when sectorSize != 16384:
                case ".32kn" when sectorSize != 32768:
                case ".64kn" when sectorSize != 65536:
                case ".512" when sectorSize  != 515:
                case ".512e" when sectorSize != 512:
                case ".128" when sectorSize  != 128:
                case ".256" when sectorSize  != 256:
                case ".iso" when sectorSize  != 2048:
                    ErrorMessage = "The specified sector size does not correspond with the requested image extension.";

                    return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupported media format {mediaType}";

                return false;
            }

            _imageInfo = new ImageInfo
            {
                MediaType  = mediaType,
                SectorSize = sectorSize,
                Sectors    = sectors
            };

            try
            {
                _writingStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";

                return false;
            }

            _basePath  = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));
            _mediaTags = new Dictionary<MediaTagType, byte[]>();

            IsWriting    = true;
            ErrorMessage = null;

            return true;
        }

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack) => true;

        /// <inheritdoc />
        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            ErrorMessage = "Unsupported feature";

            return false;
        }

        /// <inheritdoc />
        public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => false;

        /// <inheritdoc />
        public bool SetCicmMetadata(CICMMetadataType metadata) => false;

        /// <inheritdoc />
        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!SupportedMediaTags.Contains(tag))
            {
                ErrorMessage = $"Tried to write unsupported media tag {tag}.";

                return false;
            }

            if(_mediaTags.ContainsKey(tag))
                _mediaTags.Remove(tag);

            _mediaTags.Add(tag, data);

            return true;
        }

        /// <inheritdoc />
        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != _imageInfo.SectorSize)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            _writingStream.Seek((long)(sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % _imageInfo.SectorSize != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            _writingStream.Seek((long)(sectorAddress * _imageInfo.SectorSize), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            ErrorMessage = "Writing sectors with tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool SetTracks(List<Track> tracks)
        {
            if(tracks.Count <= 1)
                return true;

            ErrorMessage = "This format supports only 1 track";

            return false;
        }

        /// <inheritdoc />
        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";

                return false;
            }

            _writingStream.Flush();
            _writingStream.Close();
            IsWriting = false;

            foreach(KeyValuePair<MediaTagType, byte[]> tag in _mediaTags)
            {
                string suffix = _readWriteSidecars.Concat(_writeOnlySidecars).Where(t => t.tag == tag.Key).
                                                   Select(t => t.name).FirstOrDefault();

                if(suffix == null)
                    continue;

                var tagStream = new FileStream(_basePath + suffix, FileMode.Create, FileAccess.ReadWrite,
                                               FileShare.None);

                tagStream.Write(tag.Value, 0, tag.Value.Length);
                tagStream.Close();
            }

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata) => true;
    }
}