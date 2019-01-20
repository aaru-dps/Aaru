// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using Schemas;

namespace DiscImageChef.DiscImages
{
    public partial class VMware
    {
        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("adapter", out adapter_type))
                    switch(adapter_type.ToLowerInvariant())
                    {
                        case "ide":
                        case "lsilogic":
                        case "buslogic":
                            adapter_type = adapter_type.ToLowerInvariant();
                            break;
                        case "legacyesx":
                            adapter_type = "legacyESX";
                            break;
                        default:
                            ErrorMessage = $"Invalid adapter type {adapter_type}";
                            return false;
                    }
                else adapter_type = "ide";

                if(options.TryGetValue("hwversion", out string tmpValue))
                {
                    if(!uint.TryParse(tmpValue, out hwversion))
                    {
                        ErrorMessage = "Invalid value for hwversion option";
                        return false;
                    }
                }
                else hwversion = 4;

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
                adapter_type = "ide";
                hwversion    = 4;
            }

            if(sectorSize != 512)
            {
                ErrorMessage = "Unsupported sector size";
                return false;
            }

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
                // TODO: Support split
                writingStream = new FileStream(writingBaseName + "-flat.vmdk", FileMode.OpenOrCreate,
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

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            ErrorMessage = "Writing media tags is not supported.";
            return false;
        }

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

            if(sectorAddress >= imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
            writingStream.Write(data, 0, data.Length);

            ErrorMessage = "";
            return true;
        }

        // TODO: Implement sparse and split
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

            if(sectorAddress + length > imageInfo.Sectors)
            {
                ErrorMessage = "Tried to write past image size";
                return false;
            }

            writingStream.Seek((long)(sectorAddress * 512), SeekOrigin.Begin);
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

        // TODO: Implement sparse and split
        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            writingStream.Flush();
            writingStream.Close();

            if(imageInfo.Cylinders == 0)
            {
                imageInfo.Cylinders       = (uint)(imageInfo.Sectors / 16 / 63);
                imageInfo.Heads           = 16;
                imageInfo.SectorsPerTrack = 63;

                while(imageInfo.Cylinders == 0)
                {
                    imageInfo.Heads--;

                    if(imageInfo.Heads == 0)
                    {
                        imageInfo.SectorsPerTrack--;
                        imageInfo.Heads = 16;
                    }

                    imageInfo.Cylinders = (uint)(imageInfo.Sectors / imageInfo.Heads / imageInfo.SectorsPerTrack);

                    if(imageInfo.Cylinders == 0 && imageInfo.Heads == 0 && imageInfo.SectorsPerTrack == 0) break;
                }
            }

            descriptorStream.WriteLine("# Disk DescriptorFile");
            descriptorStream.WriteLine("version=1");
            descriptorStream.WriteLine($"CID={new Random().Next(int.MinValue, int.MaxValue):x8}");
            descriptorStream.WriteLine("parentCID=ffffffff");
            descriptorStream.WriteLine("createType=\"monolithicFlat\"");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine("# Extent description");
            descriptorStream.WriteLine($"RW {imageInfo.Sectors} FLAT \"{writingBaseName + "-flat.vmdk"}\" 0");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine("# The Disk Data Base");
            descriptorStream.WriteLine("#DDB");
            descriptorStream.WriteLine();
            descriptorStream.WriteLine($"ddb.virtualHWVersion = \"{hwversion}\"");
            descriptorStream.WriteLine($"ddb.geometry.cylinders = \"{imageInfo.Cylinders}\"");
            descriptorStream.WriteLine($"ddb.geometry.heads = \"{imageInfo.Heads}\"");
            descriptorStream.WriteLine($"ddb.geometry.sectors = \"{imageInfo.SectorsPerTrack}\"");
            descriptorStream.WriteLine($"ddb.adapterType = \"{adapter_type}\"");

            descriptorStream.Flush();
            descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata) => true;

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

            imageInfo.SectorsPerTrack = sectorsPerTrack;
            imageInfo.Heads           = heads;
            imageInfo.Cylinders       = cylinders;

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