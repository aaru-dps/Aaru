// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Filesystems;

// Information from Inside Macintosh Volume II
public sealed partial class AppleMFS
{
    /// <inheritdoc />
    public ErrorNumber MapBlock(string path, long fileBlock, out long deviceBlock)
    {
        deviceBlock = new long();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        if(fileBlock > entry.flPyLen / _volMdb.drAlBlkSiz)
            return ErrorNumber.InvalidArgument;

        uint nextBlock = entry.flStBlk;
        long relBlock  = 0;

        while(true)
        {
            if(relBlock == fileBlock)
            {
                deviceBlock = ((nextBlock - 2) * _sectorsPerBlock) + _volMdb.drAlBlSt + (long)_partitionStart;

                return ErrorNumber.NoError;
            }

            if(_blockMap[nextBlock] == BMAP_FREE ||
               _blockMap[nextBlock] == BMAP_LAST)
                break;

            nextBlock = _blockMap[nextBlock];
            relBlock++;
        }

        return ErrorNumber.InOutError;
    }

    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsAlias))
            attributes |= FileAttributes.Alias;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasBundle))
            attributes |= FileAttributes.Bundle;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasBeenInited))
            attributes |= FileAttributes.HasBeenInited;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasCustomIcon))
            attributes |= FileAttributes.HasCustomIcon;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kHasNoINITs))
            attributes |= FileAttributes.HasNoINITs;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsInvisible))
            attributes |= FileAttributes.Hidden;

        if(entry.flFlags.HasFlag(FileFlags.Locked))
            attributes |= FileAttributes.Immutable;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsOnDesk))
            attributes |= FileAttributes.IsOnDesk;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsShared))
            attributes |= FileAttributes.Shared;

        if(entry.flUsrWds.fdFlags.HasFlag(AppleCommon.FinderFlags.kIsStationery))
            attributes |= FileAttributes.Stationery;

        if(!attributes.HasFlag(FileAttributes.Alias)  &&
           !attributes.HasFlag(FileAttributes.Bundle) &&
           !attributes.HasFlag(FileAttributes.Stationery))
            attributes |= FileAttributes.File;

        attributes |= FileAttributes.BlockUnits;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        byte[]      file;
        ErrorNumber error = ErrorNumber.NoError;

        switch(_debug)
        {
            case true when string.Compare(path, "$", StringComparison.InvariantCulture) == 0:
                file = _directoryBlocks;

                break;
            case true when string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 && _bootBlocks != null:
                file = _bootBlocks;

                break;
            case true when string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0:
                file = _blockMapBytes;

                break;
            case true when string.Compare(path, "$MDB", StringComparison.InvariantCulture) == 0:
                file = _mdbBlocks;

                break;
            default:
                error = ReadFile(path, out file, false, false);

                break;
        }

        if(error != ErrorNumber.NoError)
            return error;

        node = new AppleMfsFileNode
        {
            Path   = path,
            Length = file.Length,
            Offset = 0,
            _cache = file
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not AppleMfsFileNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode._cache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(buffer is null ||
           buffer.Length < length)
            return ErrorNumber.InvalidArgument;

        if(node is not AppleMfsFileNode mynode)
            return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length)
            read = mynode.Length - mynode.Offset;

        Array.Copy(mynode._cache, mynode.Offset, buffer, 0, read);

        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        if(_debug)
            if(string.Compare(path, "$", StringComparison.InvariantCulture)       == 0 ||
               string.Compare(path, "$Boot", StringComparison.InvariantCulture)   == 0 ||
               string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0 ||
               string.Compare(path, "$MDB", StringComparison.InvariantCulture)    == 0)
            {
                stat = new FileEntryInfo
                {
                    BlockSize  = _device.Info.SectorSize,
                    Inode      = 0,
                    Links      = 1,
                    Attributes = FileAttributes.System
                };

                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                {
                    stat.Blocks = (_directoryBlocks.Length / stat.BlockSize) +
                                  (_directoryBlocks.Length % stat.BlockSize);

                    stat.Length = _directoryBlocks.Length;
                }
                else if(string.Compare(path, "$Bitmap", StringComparison.InvariantCulture) == 0)
                {
                    stat.Blocks = (_blockMapBytes.Length / stat.BlockSize) + (_blockMapBytes.Length % stat.BlockSize);

                    stat.Length = _blockMapBytes.Length;
                }
                else if(string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0 &&
                        _bootBlocks                                                      != null)
                {
                    stat.Blocks = (_bootBlocks.Length / stat.BlockSize) + (_bootBlocks.Length % stat.BlockSize);
                    stat.Length = _bootBlocks.Length;
                }
                else if(string.Compare(path, "$MDB", StringComparison.InvariantCulture) == 0)
                {
                    stat.Blocks = (_mdbBlocks.Length / stat.BlockSize) + (_mdbBlocks.Length % stat.BlockSize);
                    stat.Length = _mdbBlocks.Length;
                }
                else
                    return ErrorNumber.InvalidArgument;

                return ErrorNumber.NoError;
            }

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        ErrorNumber error = GetAttributes(path, out FileAttributes attr);

        if(error != ErrorNumber.NoError)
            return error;

        stat = new FileEntryInfo
        {
            Attributes    = attr,
            Blocks        = entry.flPyLen / _volMdb.drAlBlkSiz,
            BlockSize     = _volMdb.drAlBlkSiz,
            CreationTime  = DateHandlers.MacToDateTime(entry.flCrDat),
            Inode         = entry.flFlNum,
            LastWriteTime = DateHandlers.MacToDateTime(entry.flMdDat),
            Length        = entry.flLgLen,
            Links         = 1
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        return ErrorNumber.NotImplemented;
    }

    ErrorNumber ReadFile(string path, out byte[] buf, bool resourceFork, bool tags)
    {
        buf = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        path = pathElements[0];

        if(!_filenameToId.TryGetValue(path.ToLowerInvariant(), out uint fileId))
            return ErrorNumber.NoSuchFile;

        if(!_idToEntry.TryGetValue(fileId, out FileEntry entry))
            return ErrorNumber.NoSuchFile;

        uint nextBlock;

        if(resourceFork)
        {
            if(entry.flRPyLen == 0)
            {
                buf = Array.Empty<byte>();

                return ErrorNumber.NoError;
            }

            nextBlock = entry.flRStBlk;
        }
        else
        {
            if(entry.flPyLen == 0)
            {
                buf = Array.Empty<byte>();

                return ErrorNumber.NoError;
            }

            nextBlock = entry.flStBlk;
        }

        var ms = new MemoryStream();

        do
        {
            byte[] sectors;

            ErrorNumber errno =
                tags
                    ? _device.
                        ReadSectorsTag((ulong)((nextBlock - 2) * _sectorsPerBlock) + _volMdb.drAlBlSt + _partitionStart,
                                       (uint)_sectorsPerBlock, SectorTagType.AppleSectorTag, out sectors)
                    : _device.
                        ReadSectors((ulong)((nextBlock - 2) * _sectorsPerBlock) + _volMdb.drAlBlSt + _partitionStart,
                                    (uint)_sectorsPerBlock, out sectors);

            if(errno != ErrorNumber.NoError)
                return errno;

            ms.Write(sectors, 0, sectors.Length);

            if(_blockMap[nextBlock] == BMAP_FREE)
            {
                AaruConsole.ErrorWriteLine(Localization.File_truncated_at_block_0, nextBlock);

                break;
            }

            nextBlock = _blockMap[nextBlock];
        } while(nextBlock > BMAP_LAST);

        if(tags)
            buf = ms.ToArray();
        else
        {
            if(resourceFork)
                if(ms.Length < entry.flRLgLen)
                    buf = ms.ToArray();
                else
                {
                    buf = new byte[entry.flRLgLen];
                    Array.Copy(ms.ToArray(), 0, buf, 0, buf.Length);
                }
            else
            {
                if(ms.Length < entry.flLgLen)
                    buf = ms.ToArray();
                else
                {
                    buf = new byte[entry.flLgLen];
                    Array.Copy(ms.ToArray(), 0, buf, 0, buf.Length);
                }
            }
        }

        return ErrorNumber.NoError;
    }
}