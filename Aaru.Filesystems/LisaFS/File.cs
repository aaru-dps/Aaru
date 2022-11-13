// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle files.
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

namespace Aaru.Filesystems.LisaFS;

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Helpers;

public sealed partial class LisaFS
{
    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        ErrorNumber error = LookupFileId(path, out short fileId, out bool isDir);

        if(error != ErrorNumber.NoError)
            return error;

        if(!isDir)
            return GetAttributes(fileId, out attributes);

        attributes = FileAttributes.Directory;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Read(string path, long offset, long size, ref byte[] buf)
    {
        if(size == 0)
        {
            buf = Array.Empty<byte>();

            return ErrorNumber.NoError;
        }

        if(offset < 0)
            return ErrorNumber.InvalidArgument;

        ErrorNumber error = LookupFileId(path, out short fileId, out _);

        if(error != ErrorNumber.NoError)
            return error;

        byte[] tmp;

        if(_debug)
            switch(fileId)
            {
                case FILEID_BOOT_SIGNED:
                case FILEID_LOADER_SIGNED:
                case (short)FILEID_MDDF:
                case (short)FILEID_BITMAP:
                case (short)FILEID_SRECORD:
                case (short)FILEID_CATALOG:
                    error = ReadSystemFile(fileId, out tmp);

                    break;
                default:
                    error = ReadFile(fileId, out tmp);

                    break;
            }
        else
            error = ReadFile(fileId, out tmp);

        if(error != ErrorNumber.NoError)
            return error;

        if(offset >= tmp.Length)
            return ErrorNumber.EINVAL;

        if(size + offset >= tmp.Length)
            size = tmp.Length - offset;

        buf = new byte[size];
        Array.Copy(tmp, offset, buf, 0, size);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;
        ErrorNumber error = LookupFileId(path, out short fileId, out bool isDir);

        if(error != ErrorNumber.NoError)
            return error;

        return isDir ? StatDir(fileId, out stat) : Stat(fileId, out stat);
    }

    ErrorNumber GetAttributes(short fileId, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(fileId < 4)
        {
            if(!_debug)
                return ErrorNumber.NoSuchFile;

            attributes =  new FileAttributes();
            attributes =  FileAttributes.System;
            attributes |= FileAttributes.Hidden;

            attributes |= FileAttributes.File;

            return ErrorNumber.NoError;
        }

        ErrorNumber error = ReadExtentsFile(fileId, out ExtentFile extFile);

        if(error != ErrorNumber.NoError)
            return error;

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
            case FileType.Undefined: break;
            default:
                attributes |= FileAttributes.File;
                attributes |= FileAttributes.Extents;

                break;
        }

        if(extFile.protect > 0)
            attributes |= FileAttributes.Immutable;

        if(extFile.locked > 0)
            attributes |= FileAttributes.ReadOnly;

        if(extFile.password_valid > 0)
            attributes |= FileAttributes.Password;

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadSystemFile(short fileId, out byte[] buf) => ReadSystemFile(fileId, out buf, false);

    ErrorNumber ReadSystemFile(short fileId, out byte[] buf, bool tags)
    {
        buf = null;
        ErrorNumber errno;

        if(!_mounted ||
           !_debug)
            return ErrorNumber.AccessDenied;

        if(fileId is > 4 or <= 0)
            if(fileId != FILEID_BOOT_SIGNED &&
               fileId != FILEID_LOADER_SIGNED)
                return ErrorNumber.InvalidArgument;

        if(_systemFileCache.TryGetValue(fileId, out buf) &&
           !tags)
            return ErrorNumber.NoError;

        var count = 0;

        if(fileId == FILEID_SRECORD)
            if(!tags)
            {
                errno = _device.ReadSectors(_mddf.mddf_block + _volumePrefix + _mddf.srec_ptr, _mddf.srec_len, out buf);

                if(errno != ErrorNumber.NoError)
                    return errno;

                _systemFileCache.Add(fileId, buf);

                return ErrorNumber.NoError;
            }
            else
            {
                errno = _device.ReadSectorsTag(_mddf.mddf_block + _volumePrefix + _mddf.srec_ptr, _mddf.srec_len,
                                               SectorTagType.AppleSectorTag, out buf);

                return errno != ErrorNumber.NoError ? errno : ErrorNumber.NoError;
            }

        LisaTag.PriamTag sysTag;

        // Should be enough to check 100 sectors?
        for(ulong i = 0; i < 100; i++)
        {
            errno = _device.ReadSectorTag(i, SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

            DecodeTag(tag, out sysTag);

            if(sysTag.FileId == fileId)
                count++;
        }

        if(count == 0)
            return ErrorNumber.NoSuchFile;

        buf = !tags ? new byte[count * _device.Info.SectorSize] : new byte[count * _devTagSize];

        // Should be enough to check 100 sectors?
        for(ulong i = 0; i < 100; i++)
        {
            errno = _device.ReadSectorTag(i, SectorTagType.AppleSectorTag, out byte[] tag);

            if(errno != ErrorNumber.NoError)
                continue;

            DecodeTag(tag, out sysTag);

            if(sysTag.FileId != fileId)
                continue;

            byte[] sector;

            errno = !tags ? _device.ReadSector(i, out sector)
                        : _device.ReadSectorTag(i, SectorTagType.AppleSectorTag, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            // Relative block for $Loader starts at $Boot block
            if(sysTag.FileId == FILEID_LOADER_SIGNED)
                sysTag.RelPage--;

            Array.Copy(sector, 0, buf, sector.Length * sysTag.RelPage, sector.Length);
        }

        if(!tags)
            _systemFileCache.Add(fileId, buf);

        return ErrorNumber.NoError;
    }

    ErrorNumber Stat(short fileId, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        ErrorNumber error;
        ExtentFile  file;

        if(fileId <= 4)
            if(!_debug ||
               fileId == 0)
                return ErrorNumber.NoSuchFile;
            else
            {
                stat = new FileEntryInfo();

                error = GetAttributes(fileId, out FileAttributes attrs);

                stat.Attributes = attrs;

                if(error != ErrorNumber.NoError)
                    return error;

                if(fileId < 0                   &&
                   fileId != FILEID_BOOT_SIGNED &&
                   fileId != FILEID_LOADER_SIGNED)
                {
                    error = ReadExtentsFile((short)(fileId * -1), out file);

                    if(error != ErrorNumber.NoError)
                        return error;

                    stat.CreationTime  = DateHandlers.LisaToDateTime(file.dtc);
                    stat.AccessTime    = DateHandlers.LisaToDateTime(file.dta);
                    stat.BackupTime    = DateHandlers.LisaToDateTime(file.dtb);
                    stat.LastWriteTime = DateHandlers.LisaToDateTime(file.dtm);

                    stat.Inode     = (ulong)fileId;
                    stat.Links     = 0;
                    stat.Length    = _mddf.datasize;
                    stat.BlockSize = _mddf.datasize;
                    stat.Blocks    = 1;
                }
                else
                {
                    error = ReadSystemFile(fileId, out byte[] buf);

                    if(error != ErrorNumber.NoError)
                        return error;

                    stat.CreationTime = fileId != 4 ? _mddf.dtvc : _mddf.dtcc;

                    stat.BackupTime = _mddf.dtvb;

                    stat.Inode     = (ulong)fileId;
                    stat.Links     = 0;
                    stat.Length    = buf.Length;
                    stat.BlockSize = _mddf.datasize;
                    stat.Blocks    = buf.Length / _mddf.datasize;
                }

                return ErrorNumber.NoError;
            }

        stat = new FileEntryInfo();

        error           = GetAttributes(fileId, out FileAttributes fileAttributes);
        stat.Attributes = fileAttributes;

        if(error != ErrorNumber.NoError)
            return error;

        error = ReadExtentsFile(fileId, out file);

        if(error != ErrorNumber.NoError)
            return error;

        stat.CreationTime  = DateHandlers.LisaToDateTime(file.dtc);
        stat.AccessTime    = DateHandlers.LisaToDateTime(file.dta);
        stat.BackupTime    = DateHandlers.LisaToDateTime(file.dtb);
        stat.LastWriteTime = DateHandlers.LisaToDateTime(file.dtm);

        stat.Inode = (ulong)fileId;
        stat.Links = 1;

        if(!_fileSizeCache.TryGetValue(fileId, out int len))
            stat.Length = _srecords[fileId].filesize;
        else
            stat.Length = len;

        stat.BlockSize = _mddf.datasize;
        stat.Blocks    = file.length;

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadFile(short fileId, out byte[] buf) => ReadFile(fileId, out buf, false);

    ErrorNumber ReadFile(short fileId, out byte[] buf, bool tags)
    {
        buf = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        tags &= _debug;

        if(fileId < 4 ||
           fileId == 4 && _mddf.fsversion != LISA_V2 && _mddf.fsversion != LISA_V1)
            return ErrorNumber.InvalidArgument;

        if(!tags &&
           _fileCache.TryGetValue(fileId, out buf))
            return ErrorNumber.NoError;

        ErrorNumber error = ReadExtentsFile(fileId, out ExtentFile file);

        if(error != ErrorNumber.NoError)
            return error;

        int sectorSize;

        if(tags)
            sectorSize = _devTagSize;
        else
            sectorSize = (int)_device.Info.SectorSize;

        var temp = new byte[file.length * sectorSize];

        var offset = 0;

        for(var i = 0; i < file.extents.Length; i++)
        {
            byte[] sector;

            ErrorNumber errno =
                !tags ? _device.ReadSectors((ulong)file.extents[i].start + _mddf.mddf_block + _volumePrefix,
                                            (uint)file.extents[i].length, out sector)
                    : _device.ReadSectorsTag((ulong)file.extents[i].start + _mddf.mddf_block + _volumePrefix,
                                             (uint)file.extents[i].length, SectorTagType.AppleSectorTag, out sector);

            if(errno != ErrorNumber.NoError)
                return errno;

            Array.Copy(sector, 0, temp, offset, sector.Length);
            offset += sector.Length;
        }

        if(!tags)
        {
            if(_fileSizeCache.TryGetValue(fileId, out int realSize))
                if(realSize > temp.Length)
                    AaruConsole.ErrorWriteLine("File {0} gets truncated.", fileId);

            buf = temp;

            _fileCache.Add(fileId, buf);
        }
        else
            buf = temp;

        return ErrorNumber.NoError;
    }

    ErrorNumber LookupFileId(string path, out short fileId, out bool isDir)
    {
        fileId = 0;
        isDir  = false;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        switch(pathElements.Length)
        {
            case 0:
                fileId = DIRID_ROOT;
                isDir  = true;

                return ErrorNumber.NoError;

            // Only V3 supports subdirectories
            case > 1 when _mddf.fsversion != LISA_V3: return ErrorNumber.NotSupported;
        }

        if(_debug && pathElements.Length == 1)
        {
            if(string.Compare(pathElements[0], "$MDDF", StringComparison.InvariantCulture) == 0)
            {
                fileId = (short)FILEID_MDDF;

                return ErrorNumber.NoError;
            }

            if(string.Compare(pathElements[0], "$Boot", StringComparison.InvariantCulture) == 0)
            {
                fileId = FILEID_BOOT_SIGNED;

                return ErrorNumber.NoError;
            }

            if(string.Compare(pathElements[0], "$Loader", StringComparison.InvariantCulture) == 0)
            {
                fileId = FILEID_LOADER_SIGNED;

                return ErrorNumber.NoError;
            }

            if(string.Compare(pathElements[0], "$Bitmap", StringComparison.InvariantCulture) == 0)
            {
                fileId = (short)FILEID_BITMAP;

                return ErrorNumber.NoError;
            }

            if(string.Compare(pathElements[0], "$S-Record", StringComparison.InvariantCulture) == 0)
            {
                fileId = (short)FILEID_SRECORD;

                return ErrorNumber.NoError;
            }

            if(string.Compare(pathElements[0], "$", StringComparison.InvariantCulture) == 0)
            {
                fileId = DIRID_ROOT;
                isDir  = true;

                return ErrorNumber.NoError;
            }
        }

        for(var lvl = 0; lvl < pathElements.Length; lvl++)
        {
            string wantedFilename = pathElements[0].Replace('-', '/');

            foreach(CatalogEntry entry in _catalogCache)
            {
                string filename = StringHandlers.CToString(entry.filename, Encoding);

                // LisaOS is case insensitive
                if(string.Compare(wantedFilename, filename, StringComparison.InvariantCultureIgnoreCase) != 0 ||
                   entry.parentID                                                                        != fileId)
                    continue;

                fileId = entry.fileID;
                isDir  = entry.fileType == 0x01;

                // Not last path element, and it's not a directory
                if(lvl != pathElements.Length - 1 &&
                   !isDir)
                    return ErrorNumber.NotDirectory;

                // Arrived last path element
                if(lvl == pathElements.Length - 1)
                    return ErrorNumber.NoError;
            }
        }

        return ErrorNumber.NoSuchFile;
    }
}