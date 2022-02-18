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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.DiscImages
{
    public sealed partial class DriDiskCopy
    {
        /// <inheritdoc />
        public ErrorNumber Open(IFilter imageFilter)
        {
            Stream stream = imageFilter.GetDataForkStream();

            if((stream.Length - Marshal.SizeOf<Footer>()) % 512 != 0)
                return ErrorNumber.InvalidArgument;

            byte[] buffer = new byte[Marshal.SizeOf<Footer>()];
            stream.Seek(-buffer.Length, SeekOrigin.End);
            stream.Read(buffer, 0, buffer.Length);

            _footer = Marshal.ByteArrayToStructureLittleEndian<Footer>(buffer);

            string sig = StringHandlers.CToString(_footer.signature);

            var   regexSignature = new Regex(REGEX_DRI);
            Match matchSignature = regexSignature.Match(sig);

            if(!matchSignature.Success)
                return ErrorNumber.InvalidArgument;

            if(_footer.bpb.sptrack * _footer.bpb.cylinders * _footer.bpb.heads != _footer.bpb.sectors)
                return ErrorNumber.InvalidArgument;

            if((_footer.bpb.sectors * _footer.bpb.bps) + Marshal.SizeOf<Footer>() != stream.Length)
                return ErrorNumber.InvalidArgument;

            _imageInfo.Cylinders          = _footer.bpb.cylinders;
            _imageInfo.Heads              = _footer.bpb.heads;
            _imageInfo.SectorsPerTrack    = _footer.bpb.sptrack;
            _imageInfo.Sectors            = _footer.bpb.sectors;
            _imageInfo.SectorSize         = _footer.bpb.bps;
            _imageInfo.ApplicationVersion = matchSignature.Groups["version"].Value;

            _driImageFilter = imageFilter;

            _imageInfo.ImageSize            = (ulong)(stream.Length - Marshal.SizeOf<Footer>());
            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

            AaruConsole.DebugWriteLine("DRI DiskCopy plugin", "Image application = {0} version {1}",
                                       _imageInfo.Application, _imageInfo.ApplicationVersion);

            // Correct some incorrect data in images of NEC 2HD disks
            if(_imageInfo.Cylinders       == 77  &&
               _imageInfo.Heads           == 2   &&
               _imageInfo.SectorsPerTrack == 16  &&
               _imageInfo.SectorSize      == 512 &&
               (_footer.bpb._driveCode == DriveCode.md2hd || _footer.bpb._driveCode == DriveCode.mf2hd))
            {
                _imageInfo.SectorsPerTrack = 8;
                _imageInfo.SectorSize      = 1024;
            }

            _imageInfo.MediaType = Geometry.GetMediaType(((ushort)_imageInfo.Cylinders, (byte)_imageInfo.Heads,
                                                          (ushort)_imageInfo.SectorsPerTrack, _imageInfo.SectorSize,
                                                          MediaEncoding.MFM, false));

            switch(_imageInfo.MediaType)
            {
                case MediaType.NEC_525_HD when _footer.bpb._driveCode == DriveCode.mf2hd ||
                                               _footer.bpb._driveCode == DriveCode.mf2ed:
                    _imageInfo.MediaType = MediaType.NEC_35_HD_8;

                    break;
                case MediaType.DOS_525_HD when _footer.bpb._driveCode == DriveCode.mf2hd ||
                                               _footer.bpb._driveCode == DriveCode.mf2ed:
                    _imageInfo.MediaType = MediaType.NEC_35_HD_15;

                    break;
                case MediaType.RX50 when _footer.bpb._driveCode == DriveCode.md2dd ||
                                         _footer.bpb._driveCode == DriveCode.md2hd:
                    _imageInfo.MediaType = MediaType.ATARI_35_SS_DD;

                    break;
            }

            _imageInfo.XmlMediaType = XmlMediaType.BlockMedia;

            AaruConsole.VerboseWriteLine("Digital Research DiskCopy image contains a disk of type {0}",
                                         _imageInfo.MediaType);

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) =>
            ReadSectors(sectorAddress, 1, out buffer);

        /// <inheritdoc />
        public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
        {
            buffer = null;

            if(sectorAddress > _imageInfo.Sectors - 1)
                return ErrorNumber.OutOfRange;

            if(sectorAddress + length > _imageInfo.Sectors)
                return ErrorNumber.OutOfRange;

            buffer = new byte[length * _imageInfo.SectorSize];

            Stream stream = _driImageFilter.GetDataForkStream();
            stream.Seek((long)(sectorAddress    * _imageInfo.SectorSize), SeekOrigin.Begin);
            stream.Read(buffer, 0, (int)(length * _imageInfo.SectorSize));

            return ErrorNumber.NoError;
        }
    }
}