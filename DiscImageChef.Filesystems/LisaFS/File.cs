// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
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
using System.Diagnostics;
using DiscImageChef.ImagePlugins;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using DiscImageChef.Console;
using System.IO;

namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            Int16 fileId;
            Errno error = LookupFileId(path, out fileId);
            if(error != Errno.NoError)
                return error;

            return GetAttributes(fileId, ref attributes);
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(offset < 0 || size < 0)
                return Errno.EINVAL;
            
            Int16 fileId;
            bool isDir;
            Errno error = LookupFileId(path, out fileId, out isDir);
            if(error != Errno.NoError)
                return error;

            byte[] tmp;
            if(debug)
            {
                switch(fileId)
                {
                    case FILEID_BOOT_SIGNED:
                    case FILEID_LOADER_SIGNED:
                    case (short)FILEID_MDDF:
                    case (short)FILEID_BITMAP:
                    case (short)FILEID_SRECORD:
                    case (short)FILEID_DIRECTORY:
                        error = ReadSystemFile(fileId, out tmp);
                        break;
                    default:
                        error = ReadFile(fileId, out tmp);
                        break;
                }
            }
            else
                error = ReadFile(fileId, out tmp);

            if(error != Errno.NoError)
                return error;

            if(offset >= tmp.Length)
                return Errno.EINVAL;

            if(size + offset >= tmp.Length)
                size = tmp.Length - offset;

            buf = new byte[size];
            Array.Copy(tmp, offset, buf, 0, size);
            return Errno.NoError;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            Int16 fileId;
            Errno error = LookupFileId(path, out fileId);
            if(error != Errno.NoError)
                return error;

            return Stat(fileId, out stat);
        }

        Errno GetAttributes(Int16 fileId, ref FileAttributes attributes)
        {
            if(!mounted)
                return Errno.AccessDenied;

            if(fileId <= 4)
            {
                if(!debug || fileId == 0)
                    return Errno.NoSuchFile;
                else
                {
                    attributes = new FileAttributes();
                    attributes = FileAttributes.System;
                    attributes |= FileAttributes.Hidden;

                    if(fileId == 4)
                        attributes |= FileAttributes.Directory;
                    else
                        attributes |= FileAttributes.File;

                    return Errno.NoError;
                }
            }

            ExtentFile extFile;
            Errno error = ReadExtentsFile(fileId, out extFile);

            if(error != Errno.NoError)
                return error;

            attributes = new FileAttributes();

            switch(extFile.ftype)
            {
                case FileType.Spool:
                    attributes |= FileAttributes.CharDevice;
                    break;
                case FileType.UserCat:
                case FileType.RootCat:
                    attributes |= FileAttributes.Directory;
                    break;
                case FileType.Pipe:
                    attributes |= FileAttributes.Pipe;
                    break;
                case FileType.Undefined:
                    break;
                default:
                    attributes |= FileAttributes.File;
                    // Subcatalogs use extents?
                    attributes |= FileAttributes.Extents;
                    break;
            }

            if(extFile.protect > 0)
                attributes |= FileAttributes.Immutable;
            if(extFile.locked > 0)
                attributes |= FileAttributes.ReadOnly;
            if(extFile.password_valid > 0)
                attributes |= FileAttributes.Password;

            return Errno.NoError;
        }

        Errno ReadSystemFile(Int16 fileId, out byte[] buf)
        {
            return ReadSystemFile(fileId, out buf, false);
        }

        Errno ReadSystemFile(Int16 fileId, out byte[] buf, bool tags)
        {
            buf = null;
            if(!mounted || !debug)
                return Errno.AccessDenied;

            if(fileId > 4 || fileId <= 0)
            {
                if(fileId != FILEID_BOOT_SIGNED && fileId != FILEID_LOADER_SIGNED)
                    return Errno.InvalidArgument;
            }

            if(systemFileCache.TryGetValue(fileId, out buf) && !tags)
               return Errno.NoError;

            int count = 0;

            // Should be enough to check 100 sectors?
            for(ulong i = 0; i < 100; i++)
            {
                byte[] tag = device.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                Int16 id = BigEndianBitConverter.ToInt16(tag, 0x04);

                if(id == fileId)
                    count++;
            }

            if(count == 0)
                return Errno.NoSuchFile;

            if(!tags)
                buf = new byte[count * device.GetSectorSize()];
            else
                buf = new byte[count * 12];

            // Should be enough to check 100 sectors?
            for(ulong i = 0; i < 100; i++)
            {
                byte[] tag = device.ReadSectorTag((ulong)i, SectorTagType.AppleSectorTag);
                UInt16 id = BigEndianBitConverter.ToUInt16(tag, 0x04);

                if(id == fileId)
                {
                    UInt16 pos = BigEndianBitConverter.ToUInt16(tag, 0x06);
                    byte[] sector;

                    if(!tags)
                        sector = device.ReadSector(i);
                    else
                        sector = device.ReadSectorTag(i, SectorTagType.AppleSectorTag);
                    
                    Array.Copy(sector, 0, buf, sector.Length * pos, sector.Length);
                }
            }

            if(!tags)
                systemFileCache.Add(fileId, buf);
            
            return Errno.NoError;
        }

        Errno Stat(Int16 fileId, out FileEntryInfo stat)
        {
            stat = null;

            if(!mounted)
                return Errno.AccessDenied;

            Errno error;
            ExtentFile file;

            if(fileId <= 4)
            {
                if(!debug || fileId == 0)
                    return Errno.NoSuchFile;
                else
                {
                    stat = new FileEntryInfo();
                    stat.Attributes = new FileAttributes();

                    error = GetAttributes(fileId, ref stat.Attributes);
                    if(error != Errno.NoError)
                        return error;

                    if(fileId < 0 && fileId != FILEID_BOOT_SIGNED && fileId != FILEID_LOADER_SIGNED)
                    {
                        error = ReadExtentsFile((short)(fileId * -1), out file);
                        if(error != Errno.NoError)
                            return error;

                        stat.CreationTime = DateHandlers.LisaToDateTime(file.dtc);
                        stat.AccessTime = DateHandlers.LisaToDateTime(file.dta);
                        stat.BackupTime = DateHandlers.LisaToDateTime(file.dtb);
                        stat.LastWriteTime = DateHandlers.LisaToDateTime(file.dtm);

                        stat.Inode = (ulong)fileId;
                        stat.Mode = 0x124;
                        stat.Links = 0;
                        stat.UID = 0;
                        stat.GID = 0;
                        stat.DeviceNo = 0;
                        stat.Length = mddf.datasize;
                        stat.BlockSize = mddf.datasize;
                        stat.Blocks = 1;
                    }
                    else
                    {
                        byte[] buf;
                        error = ReadSystemFile(fileId, out buf);
                        if(error != Errno.NoError)
                            return error;

                        if(fileId != 4)
                            stat.CreationTime = mddf.dtvc;
                        else
                            stat.CreationTime = mddf.dtcc;

                        stat.BackupTime = mddf.dtvb;

                        stat.Inode = (ulong)fileId;
                        stat.Mode = 0x124;
                        stat.Links = 0;
                        stat.UID = 0;
                        stat.GID = 0;
                        stat.DeviceNo = 0;
                        stat.Length = buf.Length;
                        stat.BlockSize = mddf.datasize;
                        stat.Blocks = buf.Length / mddf.datasize;
                    }

                    return Errno.NoError;
                }
            }

            stat = new FileEntryInfo();
            stat.Attributes = new FileAttributes();
            error = GetAttributes(fileId, ref stat.Attributes);
            if(error != Errno.NoError)
            {
                //DicConsole.ErrorWriteLine("Error {0} reading attributes for file {1}", error, fileId);
                return error;
            }

            error = ReadExtentsFile(fileId, out file);
            if(error != Errno.NoError)
            {
                //DicConsole.ErrorWriteLine("Error {0} reading extents for file {1}", error, fileId);
                return error;
            }

            stat.CreationTime = DateHandlers.LisaToDateTime(file.dtc);
            stat.AccessTime = DateHandlers.LisaToDateTime(file.dta);
            stat.BackupTime = DateHandlers.LisaToDateTime(file.dtb);
            stat.LastWriteTime = DateHandlers.LisaToDateTime(file.dtm);

            stat.Inode = (ulong)fileId;
            stat.Mode = 0x1B6;
            stat.Links = 1;
            stat.UID = 0;
            stat.GID = 0;
            stat.DeviceNo = 0;
            int len;
            if(!fileSizeCache.TryGetValue(fileId, out len))
                stat.Length = file.length;
            else
                stat.Length = len;
            stat.BlockSize = mddf.datasize;
            stat.Blocks = file.length;

            return Errno.NoError;
        }

        Errno ReadFile(Int16 fileId, out byte[] buf)
        {
            return ReadFile(fileId, out buf, false);
        }

        Errno ReadFile(Int16 fileId, out byte[] buf, bool tags)
        {
            buf = null;
            if(!mounted)
                return Errno.AccessDenied;

            tags &= debug;

            if(fileId <= 4)
                return Errno.InvalidArgument;

            if(!tags && fileCache.TryGetValue(fileId, out buf))
                return Errno.NoError;

            Errno error;
            ExtentFile file;

            error = ReadExtentsFile(fileId, out file);
            if(error != Errno.NoError)
                return error;

            int sectorSize;
            if(tags)
                sectorSize = 12;
            else
                sectorSize = (int)device.GetSectorSize();

            byte[] temp = new byte[file.length * sectorSize];

            int offset = 0;
            for(int i = 0; i < file.extents.Length; i++)
            {
                byte[] sector;

                if(!tags)
                    sector = device.ReadSectors((ulong)(file.extents[i].start + mddf.mddf_block), (uint)file.extents[i].length);
                else
                    sector = device.ReadSectorsTag((ulong)(file.extents[i].start + mddf.mddf_block), (uint)file.extents[i].length, SectorTagType.AppleSectorTag);

                Array.Copy(sector, 0, temp, offset, sector.Length);
                offset += sector.Length;
            }

            if(!tags)
            {
                int realSize;
                if(fileSizeCache.TryGetValue(fileId, out realSize))
                {
                    buf = new byte[realSize];
                    Array.Copy(temp, 0, buf, 0, realSize);
                }
                else
                    buf = temp;

                fileCache.Add(fileId, buf);
            }
            else
                buf = temp;

            return Errno.NoError;
        }

        Errno LookupFileId(string path, out Int16 fileId)
        {
            bool temp;
            return LookupFileId(path, out fileId, out temp);
        }

        Errno LookupFileId(string path, out Int16 fileId, out bool isDir)
        {
            fileId = 0;
            isDir = false;

            if(!mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length == 0)
            {
                fileId = (short)FILEID_DIRECTORY;
                isDir = true;
                return Errno.NoError;
            }

            // TODO: Subcatalogs
            if(pathElements.Length > 1)
                return Errno.NotImplemented;

            if(debug)
            {
                if(String.Compare(pathElements[0], "$MDDF", StringComparison.InvariantCulture) == 0)
                {
                    fileId = (short)FILEID_MDDF;
                    return Errno.NoError;
                }

                if(String.Compare(pathElements[0], "$Boot", StringComparison.InvariantCulture) == 0)
                {
                    fileId = FILEID_BOOT_SIGNED;
                    return Errno.NoError;
                }

                if(String.Compare(pathElements[0], "$Loader", StringComparison.InvariantCulture) == 0)
                {
                    fileId = FILEID_LOADER_SIGNED;
                    return Errno.NoError;
                }

                if(String.Compare(pathElements[0], "$Bitmap", StringComparison.InvariantCulture) == 0)
                {
                    fileId = (short)FILEID_BITMAP;
                    return Errno.NoError;
                }

                if(String.Compare(pathElements[0], "$S-Record", StringComparison.InvariantCulture) == 0)
                {
                    fileId = (short)FILEID_SRECORD;
                    return Errno.NoError;
                }

                if(String.Compare(pathElements[0], "$", StringComparison.InvariantCulture) == 0)
                {
                    fileId = (short)FILEID_DIRECTORY;
                    isDir = true;
                    return Errno.NoError;
                }
            }

            List<CatalogEntry> catalog;

            Errno error = ReadCatalog((short)FILEID_DIRECTORY, out catalog);
            if(error != Errno.NoError)
                return error;

            string wantedFilename = pathElements[0].Replace(':', '/');

            foreach(CatalogEntry entry in catalog)
            {
                string filename = StringHandlers.CToString(entry.filename);
                // Should they be case sensitive?
                if(String.Compare(wantedFilename, filename, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    fileId = entry.fileID;
                    isDir |= entry.fileType != 0x03;
                    return Errno.NoError;
                }
            }

            return Errno.NoSuchFile;
        }
    }
}

