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
//     Writes VMware disk images.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages
{
    public sealed partial class VMware
    {
        /// <inheritdoc />
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("adapter", out _adapterType))
                    switch(_adapterType.ToLowerInvariant())
                    {
                        case "ide":
                        case "lsilogic":
                        case "buslogic":
                            _adapterType = _adapterType.ToLowerInvariant();

                            break;
                        case "legacyesx":
                            _adapterType = "legacyESX";

                            break;
                        default:
                            ErrorMessage = $"Invalid adapter type {_adapterType}";

                            return false;
                    }
                else
                    _adapterType = "ide";

                if(options.TryGetValue("hwversion", out string tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out _hwversion))
                    {
                        ErrorMessage = "Invalid value for hwversion option";

                        return false;
                    }
                }
                else
                    _hwversion = 4;

                if(options.TryGetValue("split", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out bool tmpBool))
                    {
                        ErrorMessage = "Invalid value for split option";

                        return false;
                    }

                    if(tmpBool)
                    {
                        ErrorMessage = "Splitted images not yet implemented";

                        return false;
                    }
                }

                if(options.TryGetValue("sparse", out tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out bool tmpBool))
                    {
                        ErrorMessage = "Invalid value for sparse option";

                        return false;
                    }

                    if(tmpBool)
                    {
                        ErrorMessage = "Sparse images not yet implemented";

                        return false;
                    }
                }
            }
            else
            {
                _adapterType = "ide";
                _hwversion   = 4;
            }

            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";

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
                _writingBaseName =
                    Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path));

                _descriptorStream = new StreamWriter(path, false, Encoding.ASCII);

                // TODO: Support split
                _writingStream = new FileStream(_writingBaseName + "-flat.vmdk", FileMode.OpenOrCreate,
                                                FileAccess.ReadWrite, FileShare.None);
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

        /// <inheritdoc />
        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";

            return false;
        }

        /// <inheritdoc />
        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length != 512)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress >= _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            _writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
            _writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";

            return true;
        }

        // TODO: Implement sparse and split
        /// <inheritdoc />
        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";

                return false;
            }

            if(data.Length % 512 != 0)
            {
                ErrorMessage = "Incorrect data size";

                return false;
            }

            if(sectorAddress + length > _imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";

                return false;
            }

            _writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
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

        // TODO: Implement sparse and split
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

            if(_imageInfo.Cylinders == 0)
            {
                _imageInfo.Cylinders       = (uint)(_imageInfo.Sectors / 16 / 63);
                _imageInfo.Heads           = 16;
                _imageInfo.SectorsPerTrack = 63;

                while(_imageInfo.Cylinders == 0)
                {
                    _imageInfo.Heads--;

                    if(_imageInfo.Heads == 0)
                    {
                        _imageInfo.SectorsPerTrack--;
                        _imageInfo.Heads = 16;
                    }

                    _imageInfo.Cylinders = (uint)(_imageInfo.Sectors / _imageInfo.Heads / _imageInfo.SectorsPerTrack);

                    if(_imageInfo.Cylinders       == 0 &&
                       _imageInfo.Heads           == 0 &&
                       _imageInfo.SectorsPerTrack == 0)
                        break;
                }
            }

            _descriptorStream.WriteLine("# Disk DescriptorFile");
            _descriptorStream.WriteLine("version=1");
            _descriptorStream.WriteLine($"CID={new Random().Next(int.MinValue, int.MaxValue):x8}");
            _descriptorStream.WriteLine("parentCID=ffffffff");
            _descriptorStream.WriteLine("createType=\"monolithicFlat\"");
            _descriptorStream.WriteLine();
            _descriptorStream.WriteLine("# Extent description");
            _descriptorStream.WriteLine($"RW {_imageInfo.Sectors} FLAT \"{_writingBaseName + "-flat.vmdk"}\" 0");
            _descriptorStream.WriteLine();
            _descriptorStream.WriteLine("# The Disk Data Base");
            _descriptorStream.WriteLine("#DDB");
            _descriptorStream.WriteLine();
            _descriptorStream.WriteLine($"ddb.virtualHWVersion = \"{_hwversion}\"");
            _descriptorStream.WriteLine($"ddb.geometry.cylinders = \"{_imageInfo.Cylinders}\"");
            _descriptorStream.WriteLine($"ddb.geometry.heads = \"{_imageInfo.Heads}\"");
            _descriptorStream.WriteLine($"ddb.geometry.sectors = \"{_imageInfo.SectorsPerTrack}\"");
            _descriptorStream.WriteLine($"ddb.adapterType = \"{_adapterType}\"");

            _descriptorStream.Flush();
            _descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";

            return true;
        }

        /// <inheritdoc />
        public bool SetMetadata(ImageInfo metadata) => true;

        /// <inheritdoc />
        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            if(cylinders > ushort.MaxValue)
            {
                ErrorMessage = "Too many cylinders.";

                return false;
            }

            if(heads > byte.MaxValue)
            {
                ErrorMessage = "Too many heads.";

                return false;
            }

            if(sectorsPerTrack > byte.MaxValue)
            {
                ErrorMessage = "Too many sectors per track.";

                return false;
            }

            _imageInfo.SectorsPerTrack = sectorsPerTrack;
            _imageInfo.Heads           = heads;
            _imageInfo.Cylinders       = cylinders;

            return true;
        }

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
    }
}