// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Extent.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            // TODO: Not really important.
            return Errno.NotImplemented;
        }

        /// <summary>
        /// Searches the disk for an extents file (or gets it from cache)
        /// </summary>
        /// <returns>Error.</returns>
        /// <param name="fileId">File identifier.</param>
        /// <param name="file">Extents file.</param>
        Errno ReadExtentsFile(Int16 fileId, out ExtentFile file)
        {
            file = new ExtentFile();

            if(!mounted)
                return Errno.AccessDenied;

            if(fileId < 5)
                return Errno.InvalidArgument;

            if(extentCache.TryGetValue(fileId, out file))
                return Errno.NoError;

            // If the file is found but not its extents file we should suppose it's a directory
            bool fileFound = false;

            for(ulong i = 0; i < device.GetSectors(); i++)
            {
                byte[] tag = device.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                Int16 foundid = BigEndianBitConverter.ToInt16(tag, 0x04);

                if(foundid == fileId)
                    fileFound = true;

                if(foundid == ((short)(-1 * fileId)))
                {
                    byte[] sector = device.ReadSector((ulong)i);

                    if(sector[0] >= 32 || sector[0] == 0)
                        return Errno.InvalidArgument;

                    file.filenameLen = sector[0];
                    file.filename = new byte[file.filenameLen];
                    Array.Copy(sector, 0x01, file.filename, 0, file.filenameLen);
                    file.timestamp = BigEndianBitConverter.ToUInt32(sector, 0x20);
                    file.unknown1 = new byte[3];
                    Array.Copy(sector, 0x24, file.unknown1, 0, 3);
                    file.serial = new byte[3];
                    Array.Copy(sector, 0x27, file.serial, 0, 3);
                    file.unknown2 = BigEndianBitConverter.ToUInt32(sector, 0x2A);
                    file.dtc = BigEndianBitConverter.ToUInt32(sector, 0x2E);
                    file.dta = BigEndianBitConverter.ToUInt32(sector, 0x32);
                    file.dtm = BigEndianBitConverter.ToUInt32(sector, 0x36);
                    file.dtb = BigEndianBitConverter.ToUInt32(sector, 0x3A);
                    file.dts = BigEndianBitConverter.ToUInt32(sector, 0x3E);
                    file.unknown3 = new byte[32];
                    Array.Copy(sector, 0x42, file.unknown3, 0, 32);
                    file.flags = sector[0x62];
                    file.password = new byte[8];
                    Array.Copy(sector, 0x63, file.password, 0, 8);
                    file.unknown4 = new byte[21];
                    Array.Copy(sector, 0x6B, file.unknown4, 0, 21);
                    file.length = BigEndianBitConverter.ToInt32(sector, 0x80);
                    file.unknown5 = BigEndianBitConverter.ToInt32(sector, 0x84);
                    file.unknown6 = BigEndianBitConverter.ToInt16(sector, 0x17E);
                    file.LisaInfo = new byte[128];
                    Array.Copy(sector, 0x180, file.LisaInfo, 0, 128);

                    int extentsCount = 0;

                    for(int j = 0; j < 41; j++)
                    {
                        if(BigEndianBitConverter.ToInt16(sector, 0x88 + j * 6 + 4) == 0)
                            break;

                        extentsCount++;
                    }

                    file.extents = new Extent[extentsCount];

                    for(int j = 0; j < extentsCount; j++)
                    {
                        file.extents[j] = new Extent();
                        file.extents[j].start = BigEndianBitConverter.ToInt32(sector, 0x88 + j * 6);
                        file.extents[j].length = BigEndianBitConverter.ToInt16(sector, 0x88 + j * 6 + 4);
                    }

                    extentCache.Add(fileId, file);
                    return Errno.NoError;
                }
            }

            return fileFound ? Errno.IsDirectory : Errno.NoSuchFile;
        }
    }
}

