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
//     Reads interleaved Apple ][ disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages
{
    public sealed partial class AppleDos
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] tmp = new byte[imageFilter.GetDataForkLength()];
            stream.Read(tmp, 0, tmp.Length);

            _extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();

            if((_extension == ".d13" || _extension == ".do") &&
               tmp.Length == 116480)
            {
                _dos32         = true;
                _deinterleaved = tmp;
            }
            else
            {
                bool isDos = tmp[0x11001] == 17 && tmp[0x11002] < 16 && tmp[0x11027] <= 122 && tmp[0x11034] == 35 &&
                             tmp[0x11035] == 16 && tmp[0x11036] == 0 && tmp[0x11037] == 1;

                _deinterleaved = new byte[tmp.Length];

                int[] offsets = _extension == ".do"
                                    ? isDos
                                          ? _deinterleave
                                          : _interleave
                                    : isDos
                                        ? _interleave
                                        : _deinterleave;

                for(int t = 0; t < 35; t++)
                {
                    for(int s = 0; s < 16; s++)
                        Array.Copy(tmp, (t * 16 * 256) + (s * 256), _deinterleaved, (t * 16 * 256) + (offsets[s] * 256),
                                   256);
                }
            }

            _imageInfo.SectorSize           = 256;
            _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
            _imageInfo.CreationTime         = imageFilter.GetCreationTime();
            _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
            _imageInfo.MediaTitle           = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
            _imageInfo.Sectors              = _dos32 ? 455u : 560u;
            _imageInfo.MediaType            = _dos32 ? MediaType.Apple32SS : MediaType.Apple33SS;
            _imageInfo.XmlMediaType         = XmlMediaType.BlockMedia;
            _imageInfo.Cylinders            = 35;
            _imageInfo.Heads                = 1;
            _imageInfo.SectorsPerTrack      = _dos32 ? 13u : 16u;

            return true;
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            if(sectorAddress > _imageInfo.Sectors - 1)
                throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

            if(sectorAddress + length > _imageInfo.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

            byte[] buffer = new byte[length * _imageInfo.SectorSize];

            Array.Copy(_deinterleaved, (int)(sectorAddress * _imageInfo.SectorSize), buffer, 0, buffer.Length);

            return buffer;
        }
    }
}