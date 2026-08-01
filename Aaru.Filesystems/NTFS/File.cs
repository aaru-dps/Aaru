// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class NTFS
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        uint mftRecordNumber;

        if(normalizedPath == "/")
            mftRecordNumber = (uint)SystemFileNumber.Root;
        else
        {
            ErrorNumber resolveErrno = ResolvePathToMftRecord(normalizedPath, out mftRecordNumber);

            if(resolveErrno != ErrorNumber.NoError) return resolveErrno;
        }

        ErrorNumber errno = ReadMftRecord(mftRecordNumber, out byte[] recordData);

        if(errno != ErrorNumber.NoError) return errno;

        MftRecord header = ParseMftRecordHeader(recordData);

        if(header.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} has invalid magic", mftRecordNumber);

            return ErrorNumber.InvalidArgument;
        }

        if(!header.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} is not in use", mftRecordNumber);

            return ErrorNumber.NoSuchFile;
        }

        stat = new FileEntryInfo
        {
            Inode     = mftRecordNumber,
            Links     = header.link_count,
            BlockSize = _bytesPerCluster,
            Attributes = header.flags.HasFlag(MftRecordFlags.IsDirectory)
                             ? FileAttributes.Directory
                             : FileAttributes.File
        };

        // Walk attributes (including extension records via $ATTRIBUTE_LIST)
        ErrorNumber findErrno = FindAllAttributes(recordData, header, mftRecordNumber, out List<FoundAttribute> attrs);

        if(findErrno != ErrorNumber.NoError) return findErrno;

        FileAttributeFlags ntfsAttributes    = 0;
        var                foundStdInfo      = false;
        var                foundFileName     = false;
        long               dataSize          = 0;
        long               dataAllocatedSize = 0;
        var                foundData         = false;
        var                isDataEncrypted   = false;
        var                isDeduplicated    = false;
        FileNameAttribute  bestFileName      = default;
        FileNameNamespace  bestNamespace     = FileNameNamespace.Dos;
        uint?              lxUid             = null;
        uint?              lxGid             = null;
        uint?              lxMod             = null;
        ulong?             lxDev             = null;
        DateTime?          lxAtime           = null;
        DateTime?          lxMtime           = null;
        DateTime?          lxCtime           = null;

        foreach(FoundAttribute attr in attrs)
        {
            var attrType   = (AttributeType)BitConverter.ToUInt32(attr.RecordData, attr.Offset);
            var attrLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 4);

            if(attrLength == 0) continue;

            byte nonResident = attr.RecordData[attr.Offset + 8];

            switch(attrType)
            {
                case AttributeType.StandardInformation when nonResident == 0:
                {
                    var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
                    var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

                    int valueStart = attr.Offset + valueOffset;

                    if(valueStart + valueLength <= attr.RecordData.Length)
                    {
                        if(valueLength >= Marshal.SizeOf<StandardInformationV3>())
                        {
                            StandardInformationV3 stdInfo =
                                Marshal.ByteArrayToStructureLittleEndian<StandardInformationV3>(attr.RecordData,
                                    valueStart,
                                    Marshal.SizeOf<StandardInformationV3>());

                            ntfsAttributes = stdInfo.file_attributes;

                            SetTimestamps(stat,
                                          stdInfo.creation_time,
                                          stdInfo.last_data_change_time,
                                          stdInfo.last_mft_change_time,
                                          stdInfo.last_access_time);

                            stat.UID = stdInfo.owner_id;
                        }
                        else if(valueLength >= (uint)Marshal.SizeOf<StandardInformationV1>())
                        {
                            StandardInformationV1 stdInfo =
                                Marshal.ByteArrayToStructureLittleEndian<StandardInformationV1>(attr.RecordData,
                                    valueStart,
                                    Marshal.SizeOf<StandardInformationV1>());

                            ntfsAttributes = stdInfo.file_attributes;

                            SetTimestamps(stat,
                                          stdInfo.creation_time,
                                          stdInfo.last_data_change_time,
                                          stdInfo.last_mft_change_time,
                                          stdInfo.last_access_time);
                        }

                        foundStdInfo = true;
                    }

                    break;
                }
                case AttributeType.FileName when nonResident == 0:
                {
                    var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
                    var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

                    int valueStart = attr.Offset + valueOffset;

                    if(valueStart + Marshal.SizeOf<FileNameAttribute>() <= attr.RecordData.Length)
                    {
                        FileNameAttribute fnAttr =
                            Marshal.ByteArrayToStructureLittleEndian<FileNameAttribute>(attr.RecordData,
                                valueStart,
                                Marshal.SizeOf<FileNameAttribute>());

                        // Prefer Win32 or Win32AndDos over Posix over Dos
                        if(!foundFileName                                                                    ||
                           fnAttr.file_name_type is FileNameNamespace.Win32 or FileNameNamespace.Win32AndDos ||
                           fnAttr.file_name_type == FileNameNamespace.Posix && bestNamespace == FileNameNamespace.Dos)
                        {
                            bestFileName  = fnAttr;
                            bestNamespace = fnAttr.file_name_type;
                            foundFileName = true;
                        }
                    }

                    break;
                }
                case AttributeType.Data:
                {
                    // Only process the unnamed (default) $DATA attribute
                    byte nameLength = attr.RecordData[attr.Offset + 9];

                    if(nameLength == 0 && !foundData)
                    {
                        // Check attribute flags for encryption (EFS)
                        var dataAttrFlags = (AttributeFlags)BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x0C);

                        if(dataAttrFlags.HasFlag(AttributeFlags.Encrypted)) isDataEncrypted = true;

                        if(nonResident == 0)
                        {
                            // Resident $DATA
                            var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);
                            dataSize          = valueLength;
                            dataAllocatedSize = valueLength;
                        }
                        else
                        {
                            // Non-resident $DATA — use first extent for size info
                            NonResidentAttributeRecord nrAttr =
                                Marshal.ByteArrayToStructureLittleEndian<NonResidentAttributeRecord>(attr.RecordData,
                                    attr.Offset,
                                    Marshal.SizeOf<NonResidentAttributeRecord>());

                            dataSize          = (long)nrAttr.data_size;
                            dataAllocatedSize = (long)nrAttr.allocated_size;
                        }

                        foundData = true;
                    }

                    break;
                }
                case AttributeType.Ea:
                {
                    ErrorNumber eaErrno = ReadEaAttributeData(attr.RecordData,
                                                              attr.Offset,
                                                              nonResident,
                                                              out byte[] eaData,
                                                              out int eaLength);

                    if(eaErrno == ErrorNumber.NoError && eaLength > 0)
                    {
                        ParseWslEas(eaData,
                                    0,
                                    eaLength,
                                    ref lxUid,
                                    ref lxGid,
                                    ref lxMod,
                                    ref lxDev,
                                    ref lxAtime,
                                    ref lxMtime,
                                    ref lxCtime);
                    }

                    break;
                }
            }
        }

        // Set file size from $DATA or $FILE_NAME
        if(foundData)
        {
            stat.Length = dataSize;
            stat.Blocks = (dataAllocatedSize + _bytesPerCluster - 1) / _bytesPerCluster;
        }
        else if(foundFileName)
        {
            stat.Length = (long)bestFileName.data_size;
            stat.Blocks = (long)((bestFileName.allocated_size + _bytesPerCluster - 1) / _bytesPerCluster);
        }

        // Map NTFS FileAttributeFlags to Aaru FileAttributes
        if(ntfsAttributes.HasFlag(FileAttributeFlags.ReadOnly)) stat.Attributes     |= FileAttributes.ReadOnly;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Hidden)) stat.Attributes       |= FileAttributes.Hidden;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.System)) stat.Attributes       |= FileAttributes.System;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Archive)) stat.Attributes      |= FileAttributes.Archive;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Device)) stat.Attributes       |= FileAttributes.Device;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Temporary)) stat.Attributes    |= FileAttributes.Temporary;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.SparseFile)) stat.Attributes   |= FileAttributes.Sparse;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.ReparsePoint)) stat.Attributes |= FileAttributes.ReparsePoint;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Compressed)) stat.Attributes   |= FileAttributes.Compressed;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Offline)) stat.Attributes      |= FileAttributes.Offline;
        if(ntfsAttributes.HasFlag(FileAttributeFlags.Encrypted)) stat.Attributes    |= FileAttributes.Encrypted;

        if(ntfsAttributes.HasFlag(FileAttributeFlags.NotContentIndexed)) stat.Attributes |= FileAttributes.NotIndexed;

        // If $DATA attribute is EFS-encrypted, ensure the Encrypted flag is set
        if(isDataEncrypted) stat.Attributes |= FileAttributes.Encrypted;

        // If file is deduplicated, set Sparse attribute (dedup files are stored as sparse + reparse)
        if(isDeduplicated) stat.Attributes |= FileAttributes.Sparse;

        // Synthesize POSIX mode from NTFS attributes
        // NTFS doesn't store POSIX mode natively, but we can approximate:
        //   directories get 0755, files get 0644, read-only files get 0444
        uint mode;

        if(header.flags.HasFlag(MftRecordFlags.IsDirectory))
        {
            // S_IFDIR | rwxr-xr-x
            mode = 0x4000 | 0755;
        }
        else if(ntfsAttributes.HasFlag(FileAttributeFlags.ReparsePoint))
        {
            // Read the reparse tag from $REPARSE_POINT attribute to determine the actual file type
            uint reparseTag = 0;

            ErrorNumber rpErrno = FindAttributes(recordData,
                                                 header,
                                                 mftRecordNumber,
                                                 AttributeType.ReparsePoint,
                                                 null,
                                                 out List<FoundAttribute> rpResults);

            if(rpErrno == ErrorNumber.NoError && rpResults.Count > 0)
            {
                FoundAttribute rpAttr   = rpResults[0];
                byte           rpNonRes = rpAttr.RecordData[rpAttr.Offset + 8];

                if(rpNonRes == 0)
                {
                    var rpValueOffset = BitConverter.ToUInt16(rpAttr.RecordData, rpAttr.Offset + 0x14);
                    int rpValueStart  = rpAttr.Offset + rpValueOffset;

                    if(rpValueStart + 4 <= rpAttr.RecordData.Length)
                        reparseTag = BitConverter.ToUInt32(rpAttr.RecordData, rpValueStart);
                }
            }

            switch(reparseTag)
            {
                case IO_REPARSE_TAG_LX_FIFO:
                    // S_IFIFO | rw-rw-rw-
                    mode            =  0x1000 | 0666;
                    stat.Attributes |= FileAttributes.FIFO;

                    break;
                case IO_REPARSE_TAG_LX_CHR:
                    // S_IFCHR | rw-rw-rw-
                    mode            =  0x2000 | 0666;
                    stat.Attributes |= FileAttributes.CharDevice;

                    break;
                case IO_REPARSE_TAG_LX_BLK:
                    // S_IFBLK | rw-rw-rw-
                    mode            =  0x6000 | 0660;
                    stat.Attributes |= FileAttributes.BlockDevice;

                    break;
                case IO_REPARSE_TAG_AF_UNIX:
                    // S_IFSOCK | rwxrwxrwx
                    mode            =  0xC000 | 0777;
                    stat.Attributes |= FileAttributes.Socket;

                    break;
                case IO_REPARSE_TAG_DEDUP:
                    // Deduplicated file — treat as regular file
                    isDeduplicated = true;

                    if(ntfsAttributes.HasFlag(FileAttributeFlags.ReadOnly))
                        mode = 0x8000 | 0444; // S_IFREG | r--r--r--
                    else
                        mode = 0x8000 | 0644; // S_IFREG | rw-r--r--

                    break;
                default:
                    // S_IFLNK | rwxrwxrwx (symlinks, mount points, and unknown reparse points)
                    mode            =  0xA000 | 0777;
                    stat.Attributes |= FileAttributes.Symlink;

                    break;
            }
        }
        else if(ntfsAttributes.HasFlag(FileAttributeFlags.Device))
        {
            // S_IFCHR | rw-rw-rw-
            mode = 0x2000 | 0666;
        }
        else
        {
            // S_IFREG
            mode = 0x8000;

            if(ntfsAttributes.HasFlag(FileAttributeFlags.ReadOnly))
                mode |= 0444; // r--r--r--
            else
                mode |= 0644; // rw-r--r--
        }

        stat.Mode = mode;

        // Override with WSL EA values if present
        if(lxUid.HasValue) stat.UID = lxUid.Value;

        if(lxGid.HasValue) stat.GID = lxGid.Value;

        if(lxMod.HasValue) stat.Mode = lxMod.Value;

        if(lxDev.HasValue) stat.DeviceNo = lxDev.Value;

        if(lxAtime.HasValue) stat.AccessTimeUtc = lxAtime.Value;

        if(lxMtime.HasValue) stat.LastWriteTimeUtc = lxMtime.Value;

        if(lxCtime.HasValue) stat.StatusChangeTimeUtc = lxCtime.Value;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory is not a file
        if(normalizedPath == "/") return ErrorNumber.IsDirectory;

        ErrorNumber resolveErrno = ResolvePathToMftRecord(normalizedPath, out uint mftRecordNumber);

        if(resolveErrno != ErrorNumber.NoError) return resolveErrno;

        ErrorNumber errno = ReadMftRecord(mftRecordNumber, out byte[] recordData);

        if(errno != ErrorNumber.NoError) return errno;

        MftRecord header = ParseMftRecordHeader(recordData);

        if(header.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} has invalid magic", mftRecordNumber);

            return ErrorNumber.InvalidArgument;
        }

        if(!header.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} is not in use", mftRecordNumber);

            return ErrorNumber.NoSuchFile;
        }

        // Reject directories
        if(header.flags.HasFlag(MftRecordFlags.IsDirectory)) return ErrorNumber.IsDirectory;

        // Find the unnamed $DATA attribute across base + extension records
        ErrorNumber findErrno = FindAttributes(recordData,
                                               header,
                                               mftRecordNumber,
                                               AttributeType.Data,
                                               null,
                                               out List<FoundAttribute> dataAttrs);

        if(findErrno != ErrorNumber.NoError) return findErrno;

        if(dataAttrs.Count == 0) return ErrorNumber.NoSuchFile;

        // Check first extent — if resident, data is inline
        FoundAttribute firstAttr   = dataAttrs[0];
        byte           nonResident = firstAttr.RecordData[firstAttr.Offset + 8];

        if(nonResident == 0)
        {
            // Resident $DATA — small file stored in MFT record
            var valueOffset = BitConverter.ToUInt16(firstAttr.RecordData, firstAttr.Offset + 0x14);
            var valueLength = BitConverter.ToUInt32(firstAttr.RecordData, firstAttr.Offset + 0x10);

            int valueStart = firstAttr.Offset + valueOffset;

            var residentData = new byte[valueLength];

            if(valueStart + valueLength <= firstAttr.RecordData.Length)
                Array.Copy(firstAttr.RecordData, valueStart, residentData, 0, valueLength);

            node = new NtfsFileNode
            {
                Path         = normalizedPath,
                Length       = valueLength,
                Offset       = 0,
                IsResident   = true,
                ResidentData = residentData
            };

            return ErrorNumber.NoError;
        }

        // Non-resident $DATA — assemble data runs from all extents
        ErrorNumber asmErrno = AssembleNonResidentRuns(mftRecordNumber,
                                                       AttributeType.Data,
                                                       null,
                                                       out List<(long offset, long length)> dataRuns,
                                                       out long dataSize,
                                                       out _,
                                                       out AttributeFlags dataFlags,
                                                       out byte compUnit);

        if(asmErrno != ErrorNumber.NoError) return asmErrno;

        bool isCompressed            = compUnit != 0 && dataFlags.HasFlag(AttributeFlags.Compressed);
        int  compressionUnitClusters = isCompressed ? 1 << compUnit : 0;

        // Check for WOF (Windows Overlay Filter) external compression via $REPARSE_POINT
        ErrorNumber rpFindErrno = FindAttributes(recordData,
                                                 header,
                                                 mftRecordNumber,
                                                 AttributeType.ReparsePoint,
                                                 null,
                                                 out List<FoundAttribute> rpAttrs);

        if(rpFindErrno == ErrorNumber.NoError && rpAttrs.Count > 0)
        {
            FoundAttribute rpAttr        = rpAttrs[0];
            byte           rpNonResident = rpAttr.RecordData[rpAttr.Offset + 8];

            if(rpNonResident == 0)
            {
                var rpValueOffset = BitConverter.ToUInt16(rpAttr.RecordData, rpAttr.Offset + 0x14);
                var rpValueLength = BitConverter.ToUInt32(rpAttr.RecordData, rpAttr.Offset + 0x10);

                int rpValueStart      = rpAttr.Offset + rpValueOffset;
                int reparseHeaderSize = Marshal.SizeOf<ReparsePointAttribute>();

                if(rpValueStart + reparseHeaderSize <= rpAttr.RecordData.Length && rpValueLength >= reparseHeaderSize)
                {
                    ReparsePointAttribute reparseHeader =
                        Marshal.ByteArrayToStructureLittleEndian<ReparsePointAttribute>(rpAttr.RecordData,
                            rpValueStart,
                            reparseHeaderSize);

                    if(reparseHeader.reparse_tag == ReparseTag.Wof)
                    {
                        // Parse WOF reparse buffer: WofVersion(4) + WofProvider(4) + ProviderVer(4) + Algorithm(4)
                        int wofDataStart = rpValueStart + reparseHeaderSize;

                        if(wofDataStart + 16 <= rpAttr.RecordData.Length)
                        {
                            var wofVersion  = BitConverter.ToUInt32(rpAttr.RecordData, wofDataStart);
                            var wofProvider = BitConverter.ToUInt32(rpAttr.RecordData, wofDataStart + 4);
                            var providerVer = BitConverter.ToUInt32(rpAttr.RecordData, wofDataStart + 8);
                            var algorithm   = BitConverter.ToUInt32(rpAttr.RecordData, wofDataStart + 12);

                            if(wofVersion  == WOF_CURRENT_VERSION &&
                               wofProvider == WOF_PROVIDER_SYSTEM &&
                               providerVer == WOF_PROVIDER_CURRENT_VERSION)
                            {
                                int frameSize = algorithm switch
                                                {
                                                    WOF_COMPRESSION_XPRESS4K  => 0x1000, // 4 KiB
                                                    WOF_COMPRESSION_LZX32K    => 0x8000, // 32 KiB
                                                    WOF_COMPRESSION_XPRESS8K  => 0x2000, // 8 KiB
                                                    WOF_COMPRESSION_XPRESS16K => 0x4000, // 16 KiB
                                                    _                         => 0
                                                };

                                if(frameSize > 0)
                                {
                                    // Read the WofCompressedData named stream
                                    ErrorNumber wofFindErrno =
                                        FindAttributes(recordData,
                                                       header,
                                                       mftRecordNumber,
                                                       AttributeType.Data,
                                                       WOF_COMPRESSED_DATA_STREAM,
                                                       out List<FoundAttribute> wofAttrs);

                                    if(wofFindErrno == ErrorNumber.NoError && wofAttrs.Count > 0)
                                    {
                                        FoundAttribute wofAttr        = wofAttrs[0];
                                        byte           wofNonResident = wofAttr.RecordData[wofAttr.Offset + 8];

                                        if(wofNonResident == 0)
                                        {
                                            // Resident WofCompressedData
                                            var wofValOff = BitConverter.ToUInt16(wofAttr.RecordData,
                                                wofAttr.Offset + 0x14);

                                            var wofValLen = BitConverter.ToUInt32(wofAttr.RecordData,
                                                wofAttr.Offset + 0x10);

                                            int wofValStart = wofAttr.Offset + wofValOff;
                                            var wofResident = new byte[wofValLen];

                                            if(wofValStart + wofValLen <= wofAttr.RecordData.Length)
                                                Array.Copy(wofAttr.RecordData, wofValStart, wofResident, 0, wofValLen);

                                            node = new NtfsFileNode
                                            {
                                                Path            = normalizedPath,
                                                Length          = dataSize,
                                                Offset          = 0,
                                                IsResident      = false,
                                                DataRuns        = dataRuns,
                                                IsWofCompressed = true,
                                                WofAlgorithm    = algorithm,
                                                WofFrameSize    = frameSize,
                                                WofIsResident   = true,
                                                WofResidentData = wofResident,
                                                WofDataSize     = wofValLen
                                            };

                                            return ErrorNumber.NoError;
                                        }

                                        // Non-resident WofCompressedData
                                        ErrorNumber wofAsmErrno =
                                            AssembleNonResidentRuns(mftRecordNumber,
                                                                    AttributeType.Data,
                                                                    WOF_COMPRESSED_DATA_STREAM,
                                                                    out List<(long offset, long length)> wofRuns,
                                                                    out long wofSize,
                                                                    out _,
                                                                    out _,
                                                                    out _);

                                        if(wofAsmErrno == ErrorNumber.NoError)
                                        {
                                            node = new NtfsFileNode
                                            {
                                                Path            = normalizedPath,
                                                Length          = dataSize,
                                                Offset          = 0,
                                                IsResident      = false,
                                                DataRuns        = dataRuns,
                                                IsWofCompressed = true,
                                                WofAlgorithm    = algorithm,
                                                WofFrameSize    = frameSize,
                                                WofDataRuns     = wofRuns,
                                                WofDataSize     = wofSize
                                            };

                                            return ErrorNumber.NoError;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        node = new NtfsFileNode
        {
            Path                    = normalizedPath,
            Length                  = dataSize,
            Offset                  = 0,
            IsResident              = false,
            DataRuns                = dataRuns,
            IsCompressed            = isCompressed,
            CompressionUnitClusters = compressionUnitClusters
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(node is not NtfsFileNode mynode) return ErrorNumber.InvalidArgument;

        mynode.DataRuns                 = null;
        mynode.ResidentData             = null;
        mynode.CachedCluster            = null;
        mynode.CachedClusterOffset      = -1;
        mynode.CachedCompressionUnit    = null;
        mynode.CachedCompressionUnitVcn = -1;
        mynode.WofDataRuns              = null;
        mynode.WofResidentData          = null;
        mynode.CachedWofFrame           = null;
        mynode.CachedWofFrameIndex      = -1;
        mynode.Offset                   = -1;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not NtfsFileNode mynode) return ErrorNumber.InvalidArgument;

        if(mynode.Offset < 0) return ErrorNumber.InvalidArgument;

        if(length <= 0) return ErrorNumber.NoError;

        // Clamp to remaining file size
        long remaining = mynode.Length - mynode.Offset;

        if(remaining <= 0) return ErrorNumber.NoError;

        if(length > remaining) length = remaining;

        // Clamp to buffer size
        if(length > buffer.Length) length = buffer.Length;

        if(mynode.IsResident)
        {
            // Resident data — direct copy
            Array.Copy(mynode.ResidentData, mynode.Offset, buffer, 0, length);
            mynode.Offset += length;
            read          =  length;

            return ErrorNumber.NoError;
        }

        // Non-resident data — read from data runs
        if(mynode.IsWofCompressed) return ReadWofFile(mynode, length, buffer, out read);

        if(mynode.IsCompressed) return ReadCompressedFile(mynode, length, buffer, out read);

        return ReadUncompressedFile(mynode, length, buffer, out read);
    }

    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Normalize path
        string normalizedPath = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if(!normalizedPath.StartsWith("/", StringComparison.Ordinal)) normalizedPath = "/" + normalizedPath;

        // Root directory cannot be a symlink
        if(normalizedPath == "/") return ErrorNumber.InvalidArgument;

        ErrorNumber resolveErrno = ResolvePathToMftRecord(normalizedPath, out uint mftRecordNumber);

        if(resolveErrno != ErrorNumber.NoError) return resolveErrno;

        ErrorNumber errno = ReadMftRecord(mftRecordNumber, out byte[] recordData);

        if(errno != ErrorNumber.NoError) return errno;

        MftRecord header = ParseMftRecordHeader(recordData);

        if(header.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} has invalid magic", mftRecordNumber);

            return ErrorNumber.InvalidArgument;
        }

        if(!header.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "MFT record {0} is not in use", mftRecordNumber);

            return ErrorNumber.NoSuchFile;
        }

        // Find $REPARSE_POINT attribute across base + extension records
        ErrorNumber findErrno = FindAttributes(recordData,
                                               header,
                                               mftRecordNumber,
                                               AttributeType.ReparsePoint,
                                               null,
                                               out List<FoundAttribute> reparseAttrs);

        if(findErrno != ErrorNumber.NoError) return findErrno;

        if(reparseAttrs.Count == 0) return ErrorNumber.InvalidArgument;

        FoundAttribute reparseAttr = reparseAttrs[0];
        byte           nonResident = reparseAttr.RecordData[reparseAttr.Offset + 8];

        if(nonResident != 0)
        {
            // Reparse points should always be resident
            AaruLogging.Debug(MODULE_NAME, "Non-resident $REPARSE_POINT in MFT record {0}", mftRecordNumber);

            return ErrorNumber.InvalidArgument;
        }

        byte[] rd      = reparseAttr.RecordData;
        int    attrOff = reparseAttr.Offset;

        var valueOffset = BitConverter.ToUInt16(rd, attrOff + 0x14);
        var valueLength = BitConverter.ToUInt32(rd, attrOff + 0x10);

        int valueStart        = attrOff + valueOffset;
        int reparseHeaderSize = Marshal.SizeOf<ReparsePointAttribute>();

        if(valueStart + reparseHeaderSize > rd.Length || valueLength < reparseHeaderSize)
            return ErrorNumber.InvalidArgument;

        ReparsePointAttribute reparseHeader =
            Marshal.ByteArrayToStructureLittleEndian<ReparsePointAttribute>(rd, valueStart, reparseHeaderSize);

        int dataStart = valueStart + reparseHeaderSize;

        switch(reparseHeader.reparse_tag)
        {
            case ReparseTag.Symlink:
            {
                if(dataStart + 12 > rd.Length) return ErrorNumber.InvalidArgument;

                var printNameOffset = BitConverter.ToUInt16(rd, dataStart + 4);
                var printNameLength = BitConverter.ToUInt16(rd, dataStart + 6);

                int pathBufferStart = dataStart       + 12;
                int printNameStart  = pathBufferStart + printNameOffset;

                if(printNameLength > 0 && printNameStart + printNameLength <= rd.Length)
                    dest = Encoding.Unicode.GetString(rd, printNameStart, printNameLength);
                else
                {
                    var substituteNameOffset = BitConverter.ToUInt16(rd, dataStart);
                    var substituteNameLength = BitConverter.ToUInt16(rd, dataStart + 2);

                    int substituteNameStart = pathBufferStart + substituteNameOffset;

                    if(substituteNameLength == 0 || substituteNameStart + substituteNameLength > rd.Length)
                        return ErrorNumber.InvalidArgument;

                    dest = Encoding.Unicode.GetString(rd, substituteNameStart, substituteNameLength);

                    if(dest.StartsWith(@"\??\", StringComparison.Ordinal)) dest = dest[4..];
                }

                dest = dest.Replace('\\', '/');

                return ErrorNumber.NoError;
            }
            case ReparseTag.MountPoint:
            {
                if(dataStart + 8 > rd.Length) return ErrorNumber.InvalidArgument;

                var printNameOffset = BitConverter.ToUInt16(rd, dataStart + 4);
                var printNameLength = BitConverter.ToUInt16(rd, dataStart + 6);

                int pathBufferStart = dataStart       + 8;
                int printNameStart  = pathBufferStart + printNameOffset;

                if(printNameLength > 0 && printNameStart + printNameLength <= rd.Length)
                    dest = Encoding.Unicode.GetString(rd, printNameStart, printNameLength);
                else
                {
                    var substituteNameOffset = BitConverter.ToUInt16(rd, dataStart);
                    var substituteNameLength = BitConverter.ToUInt16(rd, dataStart + 2);

                    int substituteNameStart = pathBufferStart + substituteNameOffset;

                    if(substituteNameLength == 0 || substituteNameStart + substituteNameLength > rd.Length)
                        return ErrorNumber.InvalidArgument;

                    dest = Encoding.Unicode.GetString(rd, substituteNameStart, substituteNameLength);

                    if(dest.StartsWith(@"\??\", StringComparison.Ordinal)) dest = dest[4..];
                }

                dest = dest.Replace('\\', '/');

                return ErrorNumber.NoError;
            }
            case ReparseTag.LxSymlink:
            {
                if(dataStart + 4 > rd.Length) return ErrorNumber.InvalidArgument;

                int targetStart  = dataStart                         + 4;
                int targetLength = reparseHeader.reparse_data_length - 4;

                if(targetLength <= 0 || targetStart + targetLength > rd.Length) return ErrorNumber.InvalidArgument;

                dest = Encoding.UTF8.GetString(rd, targetStart, targetLength);

                return ErrorNumber.NoError;
            }
            default:
                AaruLogging.Debug(MODULE_NAME,
                                  "Unsupported reparse tag 0x{0:X8} in MFT record {1}",
                                  (uint)reparseHeader.reparse_tag,
                                  mftRecordNumber);

                return ErrorNumber.NotSupported;
        }
    }

    /// <summary>Reads uncompressed non-resident file data from data runs with single-cluster caching.</summary>
    /// <param name="mynode">The file node containing data runs and read state.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="read">Number of bytes actually read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadUncompressedFile(NtfsFileNode mynode, long length, byte[] buffer, out long read)
    {
        read = 0;
        long bytesRead = 0;

        while(bytesRead < length)
        {
            long fileOffset      = mynode.Offset;
            long clusterIndex    = fileOffset / _bytesPerCluster;
            long offsetInCluster = fileOffset % _bytesPerCluster;

            // Translate logical cluster to physical cluster via data runs
            long physicalCluster = -1;
            long runStartVcn     = 0;

            foreach((long clusterOffset, long clusterLength) in mynode.DataRuns)
            {
                if(clusterIndex >= runStartVcn && clusterIndex < runStartVcn + clusterLength)
                {
                    // Sparse run (offset 0 with no offset bytes stored as 0)
                    if(clusterOffset == 0 && clusterLength > 0)
                    {
                        physicalCluster = 0; // Will be treated as sparse below

                        break;
                    }

                    physicalCluster = clusterOffset + (clusterIndex - runStartVcn);

                    break;
                }

                runStartVcn += clusterLength;
            }

            // Beyond the end of data runs
            if(physicalCluster < 0) break;

            long bytesToCopy = Math.Min(length - bytesRead, _bytesPerCluster - offsetInCluster);

            if(physicalCluster == 0)
            {
                // Sparse cluster — fill with zeros
                Array.Clear(buffer, (int)bytesRead, (int)bytesToCopy);
            }
            else if(mynode.CachedClusterOffset == physicalCluster && mynode.CachedCluster != null)
            {
                // Cache hit — copy from cached cluster
                Array.Copy(mynode.CachedCluster, offsetInCluster, buffer, bytesRead, bytesToCopy);
            }
            else
            {
                // Read cluster from disk
                ulong sectorStart = (ulong)physicalCluster * _sectorsPerCluster;

                ErrorNumber errno = _image.ReadSectors(_partition.Start + sectorStart,
                                                       false,
                                                       _sectorsPerCluster,
                                                       out byte[] clusterData,
                                                       out _);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "Error reading cluster {0}: {1}", physicalCluster, errno);

                    break;
                }

                // Cache this cluster
                mynode.CachedCluster       = clusterData;
                mynode.CachedClusterOffset = physicalCluster;

                Array.Copy(clusterData, offsetInCluster, buffer, bytesRead, bytesToCopy);
            }

            bytesRead     += bytesToCopy;
            mynode.Offset += bytesToCopy;
        }

        read = bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads compressed non-resident file data by decompressing LZNT1 compression units on demand with caching.
    /// </summary>
    /// <param name="mynode">The file node containing data runs and read state.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="read">Number of bytes actually read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadCompressedFile(NtfsFileNode mynode, long length, byte[] buffer, out long read)
    {
        read = 0;
        long bytesRead               = 0;
        int  compressionUnitClusters = mynode.CompressionUnitClusters;
        long compressionUnitBytes    = compressionUnitClusters * _bytesPerCluster;

        while(bytesRead < length)
        {
            long fileOffset = mynode.Offset;

            // Determine which compression unit this offset falls in
            long compressionUnitIndex = fileOffset           / compressionUnitBytes;
            long offsetInUnit         = fileOffset           % compressionUnitBytes;
            long compressionUnitVcn   = compressionUnitIndex * compressionUnitClusters;

            // Check cache first
            if(mynode.CachedCompressionUnitVcn != compressionUnitVcn || mynode.CachedCompressionUnit == null)
            {
                // Need to decompress this compression unit
                ErrorNumber decompressErrno =
                    DecompressCompressionUnit(mynode, compressionUnitVcn, compressionUnitClusters, out byte[] unitData);

                if(decompressErrno != ErrorNumber.NoError) break;

                mynode.CachedCompressionUnit    = unitData;
                mynode.CachedCompressionUnitVcn = compressionUnitVcn;
            }

            long bytesToCopy = Math.Min(length - bytesRead, compressionUnitBytes - offsetInUnit);

            // Clamp to what the cached unit actually contains
            if(offsetInUnit + bytesToCopy > mynode.CachedCompressionUnit.Length)
                bytesToCopy = mynode.CachedCompressionUnit.Length - offsetInUnit;

            if(bytesToCopy <= 0) break;

            Array.Copy(mynode.CachedCompressionUnit, offsetInUnit, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            mynode.Offset += bytesToCopy;
        }

        read = bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and decompresses a single compression unit from the data runs.</summary>
    /// <param name="mynode">The file node containing data runs.</param>
    /// <param name="unitStartVcn">VCN of the first cluster in the compression unit.</param>
    /// <param name="compressionUnitClusters">Number of clusters in a compression unit.</param>
    /// <param name="unitData">Output decompressed data for the compression unit.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber DecompressCompressionUnit(NtfsFileNode mynode, long unitStartVcn, int compressionUnitClusters,
                                          out byte[]   unitData)
    {
        unitData = null;
        long unitEndVcn = unitStartVcn + compressionUnitClusters;

        // Collect physical cluster mappings for this compression unit
        // A compression unit may span multiple data runs, and sparse runs within indicate
        // that the remaining clusters in the unit are compressed (or entirely sparse)
        List<(long physicalCluster, long count)> physicalRuns  = [];
        long                                     totalClusters = 0;
        var                                      hasSparseRun  = false;

        long runStartVcn = 0;

        foreach((long clusterOffset, long clusterLength) in mynode.DataRuns)
        {
            long runEndVcn = runStartVcn + clusterLength;

            // Skip runs entirely before or after this compression unit
            if(runEndVcn <= unitStartVcn)
            {
                runStartVcn = runEndVcn;

                continue;
            }

            if(runStartVcn >= unitEndVcn) break;

            // Clamp run to the compression unit boundaries
            long overlapStart = Math.Max(runStartVcn, unitStartVcn);
            long overlapEnd   = Math.Min(runEndVcn, unitEndVcn);
            long overlapCount = overlapEnd - overlapStart;

            if(clusterOffset == 0)
            {
                // Sparse run
                hasSparseRun  =  true;
                totalClusters += overlapCount;
            }
            else
            {
                long physStart = clusterOffset + (overlapStart - runStartVcn);
                physicalRuns.Add((physStart, overlapCount));
                totalClusters += overlapCount;
            }

            runStartVcn = runEndVcn;
        }

        int unitBytes = compressionUnitClusters * (int)_bytesPerCluster;

        // If the entire compression unit is sparse, return zeros
        if(physicalRuns.Count == 0)
        {
            unitData = new byte[unitBytes];

            return ErrorNumber.NoError;
        }

        // Count physical (non-sparse) clusters
        long physicalCount = 0;

        foreach((long _, long count) in physicalRuns) physicalCount += count;

        // If the physical clusters fill the entire compression unit, data is uncompressed
        if(physicalCount >= compressionUnitClusters && !hasSparseRun)
        {
            unitData = new byte[unitBytes];
            var dstOffset = 0;

            foreach((long physicalCluster, long count) in physicalRuns)
            {
                ulong sectorStart = (ulong)physicalCluster * _sectorsPerCluster;
                var   sectorCount = (uint)(count * _sectorsPerCluster);

                ErrorNumber errno = _image.ReadSectors(_partition.Start + sectorStart,
                                                       false,
                                                       sectorCount,
                                                       out byte[] clusterData,
                                                       out _);

                if(errno != ErrorNumber.NoError)
                {
                    AaruLogging.Debug(MODULE_NAME, "Error reading clusters at LCN {0}: {1}", physicalCluster, errno);

                    return errno;
                }

                int copyLen = Math.Min(clusterData.Length, unitBytes - dstOffset);
                Array.Copy(clusterData, 0, unitData, dstOffset, copyLen);
                dstOffset += copyLen;
            }

            return ErrorNumber.NoError;
        }

        // Compressed: read the physical clusters and decompress
        var compressedBytes  = (int)(physicalCount * _bytesPerCluster);
        var compressedData   = new byte[compressedBytes];
        var compressedOffset = 0;

        foreach((long physicalCluster, long count) in physicalRuns)
        {
            ulong sectorStart = (ulong)physicalCluster * _sectorsPerCluster;
            var   sectorCount = (uint)(count * _sectorsPerCluster);

            ErrorNumber errno = _image.ReadSectors(_partition.Start + sectorStart,
                                                   false,
                                                   sectorCount,
                                                   out byte[] clusterData,
                                                   out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "Error reading compressed clusters at LCN {0}: {1}",
                                  physicalCluster,
                                  errno);

                return errno;
            }

            int copyLen = Math.Min(clusterData.Length, compressedBytes - compressedOffset);
            Array.Copy(clusterData, 0, compressedData, compressedOffset, copyLen);
            compressedOffset += copyLen;
        }

        unitData = DecompressLznt1(compressedData, unitBytes);

        if(unitData == null)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "LZNT1 decompression failed for compression unit starting at VCN {0}",
                              unitStartVcn);

            return ErrorNumber.InOutError;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Reads WOF (Windows Overlay Filter) externally compressed file data by decompressing individual frames
    ///     using Xpress or LZX algorithms with single-frame caching.
    /// </summary>
    /// <param name="mynode">The file node containing WOF compressed data information.</param>
    /// <param name="length">Number of bytes to read.</param>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="read">Number of bytes actually read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadWofFile(NtfsFileNode mynode, long length, byte[] buffer, out long read)
    {
        read = 0;
        long bytesRead = 0;
        int  frameSize = mynode.WofFrameSize;

        // Total number of frames (0-based index of last frame)
        long lastFrameIndex = mynode.Length > 0 ? (mynode.Length - 1) / frameSize : 0;

        // Offset table size: one entry per frame except the first
        // For files < 4 GiB, entries are 4 bytes; otherwise 8 bytes
        int  bytesPerOffset  = mynode.Length < 0x100000000L ? 4 : 8;
        long offsetTableSize = lastFrameIndex * bytesPerOffset;

        while(bytesRead < length)
        {
            long fileOffset    = mynode.Offset;
            long frameIndex    = fileOffset / frameSize;
            long offsetInFrame = fileOffset % frameSize;

            // Check cache first
            if(mynode.CachedWofFrameIndex != frameIndex || mynode.CachedWofFrame == null)
            {
                // Need to decompress this frame
                ErrorNumber frameErrno = DecompressWofFrame(mynode,
                                                            frameIndex,
                                                            lastFrameIndex,
                                                            bytesPerOffset,
                                                            offsetTableSize,
                                                            out byte[] frameData);

                if(frameErrno != ErrorNumber.NoError) break;

                mynode.CachedWofFrame      = frameData;
                mynode.CachedWofFrameIndex = frameIndex;
            }

            long bytesToCopy = Math.Min(length - bytesRead, mynode.CachedWofFrame.Length - offsetInFrame);

            if(bytesToCopy <= 0) break;

            Array.Copy(mynode.CachedWofFrame, offsetInFrame, buffer, bytesRead, bytesToCopy);

            bytesRead     += bytesToCopy;
            mynode.Offset += bytesToCopy;
        }

        read = bytesRead;

        return ErrorNumber.NoError;
    }

    /// <summary>Decompresses a single WOF frame from the WofCompressedData stream.</summary>
    /// <param name="mynode">The file node containing WOF compressed data information.</param>
    /// <param name="frameIndex">Zero-based index of the frame to decompress.</param>
    /// <param name="lastFrameIndex">Zero-based index of the last frame in the file.</param>
    /// <param name="bytesPerOffset">Size of each offset table entry (4 or 8 bytes).</param>
    /// <param name="offsetTableSize">Total size of the offset table in bytes.</param>
    /// <param name="frameData">Output decompressed frame data.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber DecompressWofFrame(NtfsFileNode mynode, long frameIndex, long lastFrameIndex, int bytesPerOffset,
                                   long         offsetTableSize, out byte[] frameData)
    {
        frameData = null;

        // Determine the compressed frame boundaries from the offset table.
        // The offset table has (lastFrameIndex) entries — one for each frame except the first.
        // Entry[N-1] gives the cumulative end offset of frame N relative to the data area.
        // Frame 0 starts at offset 0 from the data area (no entry needed).
        long frameStart;
        long frameEnd;

        if(mynode.WofIsResident)
        {
            // Read offset table from resident data
            ErrorNumber offsetErrno = ReadWofOffsetTableResident(mynode.WofResidentData,
                                                                 frameIndex,
                                                                 lastFrameIndex,
                                                                 bytesPerOffset,
                                                                 offsetTableSize,
                                                                 out frameStart,
                                                                 out frameEnd);

            if(offsetErrno != ErrorNumber.NoError) return offsetErrno;

            // Read compressed frame data from resident stream
            var srcStart = (int)(offsetTableSize + frameStart);
            var srcLen   = (int)(frameEnd        - frameStart);

            if(srcStart + srcLen > mynode.WofResidentData.Length)
            {
                AaruLogging.Debug(MODULE_NAME, "WOF resident compressed frame {0} exceeds stream bounds", frameIndex);

                return ErrorNumber.InOutError;
            }

            var compressedFrame = new byte[srcLen];
            Array.Copy(mynode.WofResidentData, srcStart, compressedFrame, 0, srcLen);

            // Determine uncompressed size for this frame
            int uncompressedSize = frameIndex == lastFrameIndex
                                       ? (int)(1 + (mynode.Length - 1) % mynode.WofFrameSize)
                                       : mynode.WofFrameSize;

            // If compressed data is same size or larger, it's stored uncompressed
            if(srcLen >= uncompressedSize)
            {
                frameData = new byte[uncompressedSize];
                Array.Copy(compressedFrame, 0, frameData, 0, uncompressedSize);

                return ErrorNumber.NoError;
            }

            frameData = DecompressWofFrameData(compressedFrame, uncompressedSize, mynode.WofAlgorithm);

            if(frameData == null)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "WOF decompression failed for frame {0} (algorithm {1})",
                                  frameIndex,
                                  mynode.WofAlgorithm);

                return ErrorNumber.InOutError;
            }

            return ErrorNumber.NoError;
        }

        // Non-resident WofCompressedData — need to read from data runs
        // First read the offset table entry to find where the compressed frame lives
        ErrorNumber nrOffsetErrno = ReadWofOffsetTableNonResident(mynode,
                                                                  frameIndex,
                                                                  lastFrameIndex,
                                                                  bytesPerOffset,
                                                                  offsetTableSize,
                                                                  out frameStart,
                                                                  out frameEnd);

        if(nrOffsetErrno != ErrorNumber.NoError) return nrOffsetErrno;

        // Read compressed frame from non-resident stream
        long compressedOffset = offsetTableSize + frameStart;
        var  compressedSize   = (int)(frameEnd - frameStart);

        var compressedBuf = new byte[compressedSize];

        ErrorNumber readErrno = ReadFromDataRuns(mynode.WofDataRuns,
                                                 mynode.WofDataSize,
                                                 compressedOffset,
                                                 compressedBuf,
                                                 compressedSize);

        if(readErrno != ErrorNumber.NoError) return readErrno;

        // Determine uncompressed size for this frame
        int uncSize = frameIndex == lastFrameIndex
                          ? (int)(1 + (mynode.Length - 1) % mynode.WofFrameSize)
                          : mynode.WofFrameSize;

        // If compressed data is same size or larger, it's stored uncompressed
        if(compressedSize >= uncSize)
        {
            frameData = new byte[uncSize];
            Array.Copy(compressedBuf, 0, frameData, 0, uncSize);

            return ErrorNumber.NoError;
        }

        frameData = DecompressWofFrameData(compressedBuf, uncSize, mynode.WofAlgorithm);

        if(frameData == null)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "WOF decompression failed for frame {0} (algorithm {1})",
                              frameIndex,
                              mynode.WofAlgorithm);

            return ErrorNumber.InOutError;
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads WOF frame offset boundaries from a resident WofCompressedData stream.</summary>
    /// <param name="data">The complete resident WofCompressedData stream.</param>
    /// <param name="frameIndex">Zero-based frame index.</param>
    /// <param name="lastFrameIndex">Zero-based index of the last frame.</param>
    /// <param name="bytesPerOffset">Size of each offset table entry (4 or 8).</param>
    /// <param name="offsetTableSize">Total size of the offset table in bytes.</param>
    /// <param name="frameStart">Output: byte offset where compressed frame starts (relative to data area).</param>
    /// <param name="frameEnd">Output: byte offset where compressed frame ends (relative to data area).</param>
    /// <returns>Error number indicating success or failure.</returns>
    static ErrorNumber ReadWofOffsetTableResident(byte[] data, long frameIndex, long lastFrameIndex, int bytesPerOffset,
                                                  long   offsetTableSize, out long frameStart, out long frameEnd)
    {
        frameStart = 0;
        frameEnd   = 0;

        if(frameIndex == 0)
        {
            // First frame starts at offset 0 from the data area
            frameStart = 0;

            if(lastFrameIndex == 0)
            {
                // Only one frame — compressed data extends to end of stream
                frameEnd = data.Length - offsetTableSize;
            }
            else
            {
                // Read offset[0] to get end of first frame
                if(bytesPerOffset == 4)
                    frameEnd = BitConverter.ToUInt32(data, 0);
                else
                    frameEnd = (long)BitConverter.ToUInt64(data, 0);
            }
        }
        else
        {
            // Read offset[frameIndex-1] for start and offset[frameIndex] for end
            var prevPos = (int)((frameIndex - 1) * bytesPerOffset);

            if(bytesPerOffset == 4)
                frameStart = BitConverter.ToUInt32(data, prevPos);
            else
                frameStart = (long)BitConverter.ToUInt64(data, prevPos);

            if(frameIndex == lastFrameIndex)
            {
                // Last frame — compressed data extends to end of stream
                frameEnd = data.Length - offsetTableSize;
            }
            else
            {
                var curPos = (int)(frameIndex * bytesPerOffset);

                if(bytesPerOffset == 4)
                    frameEnd = BitConverter.ToUInt32(data, curPos);
                else
                    frameEnd = (long)BitConverter.ToUInt64(data, curPos);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads WOF frame offset boundaries from a non-resident WofCompressedData stream.</summary>
    /// <param name="mynode">File node with WOF data runs.</param>
    /// <param name="frameIndex">Zero-based frame index.</param>
    /// <param name="lastFrameIndex">Zero-based index of the last frame.</param>
    /// <param name="bytesPerOffset">Size of each offset table entry (4 or 8).</param>
    /// <param name="offsetTableSize">Total size of the offset table in bytes.</param>
    /// <param name="frameStart">Output: byte offset where compressed frame starts (relative to data area).</param>
    /// <param name="frameEnd">Output: byte offset where compressed frame ends (relative to data area).</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadWofOffsetTableNonResident(NtfsFileNode mynode,         long frameIndex,      long lastFrameIndex,
                                              int          bytesPerOffset, long offsetTableSize, out long frameStart,
                                              out long     frameEnd)
    {
        frameStart = 0;
        frameEnd   = 0;

        if(frameIndex == 0)
        {
            frameStart = 0;

            if(lastFrameIndex == 0)
                frameEnd = mynode.WofDataSize - offsetTableSize;
            else
            {
                // Read offset[0]
                var offsetBuf = new byte[bytesPerOffset];

                ErrorNumber errno =
                    ReadFromDataRuns(mynode.WofDataRuns, mynode.WofDataSize, 0, offsetBuf, bytesPerOffset);

                if(errno != ErrorNumber.NoError) return errno;

                frameEnd = bytesPerOffset == 4
                               ? BitConverter.ToUInt32(offsetBuf, 0)
                               : (long)BitConverter.ToUInt64(offsetBuf, 0);
            }
        }
        else
        {
            // Read both offset[frameIndex-1] and offset[frameIndex] (if not last frame)
            long prevOff     = (frameIndex - 1) * bytesPerOffset;
            int  bytesToRead = frameIndex == lastFrameIndex ? bytesPerOffset : bytesPerOffset * 2;
            var  offsetBuf   = new byte[bytesToRead];

            ErrorNumber errno =
                ReadFromDataRuns(mynode.WofDataRuns, mynode.WofDataSize, prevOff, offsetBuf, bytesToRead);

            if(errno != ErrorNumber.NoError) return errno;

            frameStart = bytesPerOffset == 4
                             ? BitConverter.ToUInt32(offsetBuf, 0)
                             : (long)BitConverter.ToUInt64(offsetBuf, 0);

            if(frameIndex == lastFrameIndex)
                frameEnd = mynode.WofDataSize - offsetTableSize;
            else
            {
                frameEnd = bytesPerOffset == 4
                               ? BitConverter.ToUInt32(offsetBuf, bytesPerOffset)
                               : (long)BitConverter.ToUInt64(offsetBuf, bytesPerOffset);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <summary>Reads bytes from a non-resident data stream at a given byte offset.</summary>
    /// <param name="dataRuns">The data run list for the stream.</param>
    /// <param name="dataSize">Total logical size of the stream.</param>
    /// <param name="offset">Byte offset within the stream to start reading.</param>
    /// <param name="buffer">Destination buffer.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ReadFromDataRuns(List<(long clusterOffset, long clusterLength)> dataRuns, long dataSize, long offset,
                                 byte[]                                         buffer,   int  count)
    {
        if(offset + count > dataSize) return ErrorNumber.InvalidArgument;

        var  bytesRead    = 0;
        long runStartByte = 0;

        foreach((long clusterOffset, long clusterLength) in dataRuns)
        {
            long runBytes   = clusterLength * _bytesPerCluster;
            long runEndByte = runStartByte + runBytes;

            if(offset + bytesRead >= runEndByte)
            {
                runStartByte = runEndByte;

                continue;
            }

            if(offset + bytesRead < runStartByte)
            {
                runStartByte = runEndByte;

                continue;
            }

            long offsetInRun    = offset + bytesRead - runStartByte;
            long availableBytes = runBytes           - offsetInRun;
            var  toRead         = (int)Math.Min(count - bytesRead, availableBytes);

            if(clusterOffset == 0)
            {
                // Sparse run — fill with zeros
                Array.Clear(buffer, bytesRead, toRead);
                bytesRead += toRead;
            }
            else
            {
                long clusterInRun    = offsetInRun / _bytesPerCluster;
                long offsetInCluster = offsetInRun % _bytesPerCluster;
                long physicalCluster = clusterOffset + clusterInRun;

                while(toRead > 0)
                {
                    ulong sectorStart = (ulong)physicalCluster * _sectorsPerCluster;

                    ErrorNumber errno = _image.ReadSectors(_partition.Start + sectorStart,
                                                           false,
                                                           _sectorsPerCluster,
                                                           out byte[] clusterData,
                                                           out _);

                    if(errno != ErrorNumber.NoError) return errno;

                    var copyLen = (int)Math.Min(toRead, _bytesPerCluster - offsetInCluster);
                    Array.Copy(clusterData, offsetInCluster, buffer, bytesRead, copyLen);

                    bytesRead += copyLen;
                    toRead    -= copyLen;
                    physicalCluster++;
                    offsetInCluster = 0;
                }
            }

            runStartByte = runEndByte;

            if(bytesRead >= count) break;
        }

        return bytesRead >= count ? ErrorNumber.NoError : ErrorNumber.InOutError;
    }

    /// <summary>Decompresses a WOF compressed frame using the appropriate algorithm.</summary>
    /// <param name="compressedData">The compressed frame data.</param>
    /// <param name="uncompressedSize">Expected uncompressed size of the frame.</param>
    /// <param name="algorithm">WOF compression algorithm identifier.</param>
    /// <returns>The decompressed data, or <c>null</c> if decompression fails.</returns>
    static byte[] DecompressWofFrameData(byte[] compressedData, int uncompressedSize, uint algorithm)
    {
        return algorithm switch
               {
                   WOF_COMPRESSION_XPRESS4K  => DecompressXpress(compressedData, uncompressedSize),
                   WOF_COMPRESSION_XPRESS8K  => DecompressXpress(compressedData, uncompressedSize),
                   WOF_COMPRESSION_XPRESS16K => DecompressXpress(compressedData, uncompressedSize),
                   WOF_COMPRESSION_LZX32K    => DecompressLzx(compressedData, uncompressedSize),
                   _                         => null
               };
    }

    /// <summary>Sets timestamps on a <see cref="FileEntryInfo" /> from NTFS FILETIME values.</summary>
    /// <param name="info">The file entry info to populate.</param>
    /// <param name="creationTime">NTFS creation time (FILETIME).</param>
    /// <param name="lastWriteTime">NTFS last data modification time (FILETIME).</param>
    /// <param name="lastMftChangeTime">NTFS last MFT record change time (FILETIME).</param>
    /// <param name="lastAccessTime">NTFS last access time (FILETIME).</param>
    static void SetTimestamps(FileEntryInfo info, long creationTime, long lastWriteTime, long lastMftChangeTime,
                              long          lastAccessTime)
    {
        if(creationTime > 0) info.CreationTimeUtc = DateTime.FromFileTimeUtc(creationTime);

        if(lastWriteTime > 0) info.LastWriteTimeUtc = DateTime.FromFileTimeUtc(lastWriteTime);

        if(lastMftChangeTime > 0) info.StatusChangeTimeUtc = DateTime.FromFileTimeUtc(lastMftChangeTime);

        if(lastAccessTime > 0) info.AccessTimeUtc = DateTime.FromFileTimeUtc(lastAccessTime);
    }

    /// <summary>Resolves a normalized path to its MFT record number by traversing the directory tree.</summary>
    /// <param name="normalizedPath">Absolute path starting with '/'.</param>
    /// <param name="mftRecordNumber">Output MFT record number.</param>
    /// <returns>Error number indicating success or failure.</returns>
    ErrorNumber ResolvePathToMftRecord(string normalizedPath, out uint mftRecordNumber)
    {
        mftRecordNumber = 0;

        string cutPath = normalizedPath[1..]; // Remove leading '/'

        string[] pieces = cutPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        if(pieces.Length == 0) return ErrorNumber.NoSuchFile;

        // Start from root directory
        Dictionary<string, ulong> currentDirectory = _rootDirectoryCache;

        for(var p = 0; p < pieces.Length; p++)
        {
            string component = pieces[p];

            if(!currentDirectory.TryGetValue(component, out ulong mftRef)) return ErrorNumber.NoSuchFile;

            var recordNum = (uint)(mftRef & 0x0000FFFFFFFFFFFF);

            // If this is the last component, return it
            if(p == pieces.Length - 1)
            {
                mftRecordNumber = recordNum;

                return ErrorNumber.NoError;
            }

            // Not the last component — must be a directory
            ErrorNumber errno = GetDirectoryEntries(recordNum, out Dictionary<string, ulong> dirEntries);

            if(errno != ErrorNumber.NoError) return errno;

            currentDirectory = dirEntries;
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <summary>Parses WSL metadata EAs ($LXUID, $LXGID, $LXMOD, $LXDEV) from EA attribute data.</summary>
    /// <param name="data">Buffer containing the EA data.</param>
    /// <param name="start">Start offset of the EA data in the buffer.</param>
    /// <param name="length">Total length of the EA data.</param>
    /// <param name="lxUid">Output UID if $LXUID is found.</param>
    /// <param name="lxGid">Output GID if $LXGID is found.</param>
    /// <param name="lxMod">Output POSIX mode if $LXMOD is found.</param>
    /// <param name="lxDev">Output device number if $LXDEV is found.</param>
    /// <param name="lxAtime">Output last access time if $LXATTRB is found.</param>
    /// <param name="lxMtime">Output last modification time if $LXATTRB is found.</param>
    /// <param name="lxCtime">Output last status change time if $LXATTRB is found.</param>
    static void ParseWslEas(byte[]     data,  int start, int length, ref uint? lxUid, ref uint? lxGid, ref uint? lxMod,
                            ref ulong? lxDev, ref DateTime? lxAtime, ref DateTime? lxMtime, ref DateTime? lxCtime)
    {
        int pos          = start;
        int end          = start + length;
        int eaHeaderSize = Marshal.SizeOf<EaAttribute>();
        int lxAttrbSize  = Marshal.SizeOf<LxAttrb>();

        // First pass: parse $LXATTRB (WSL1 combined metadata) as base values
        int firstPos = pos;

        while(firstPos + eaHeaderSize <= end)
        {
            EaAttribute ea = Marshal.ByteArrayToStructureLittleEndian<EaAttribute>(data, firstPos, eaHeaderSize);

            int nameStart = firstPos + eaHeaderSize;

            if(nameStart + ea.ea_name_length > end) break;

            string eaName = Encoding.ASCII.GetString(data, nameStart, ea.ea_name_length);

            if(eaName == EA_LXATTRB)
            {
                int valueStart = nameStart + ea.ea_name_length + 1;

                if(ea.ea_value_length >= lxAttrbSize && valueStart + lxAttrbSize <= end)
                {
                    LxAttrb lxAttrb = Marshal.ByteArrayToStructureLittleEndian<LxAttrb>(data, valueStart, lxAttrbSize);

                    lxUid = lxAttrb.st_uid;
                    lxGid = lxAttrb.st_gid;
                    lxMod = lxAttrb.st_mode;

                    if(lxAttrb.st_rdev != 0)
                    {
                        uint major = lxAttrb.st_rdev >> 8 & 0xFF;
                        uint minor = lxAttrb.st_rdev      & 0xFF;
                        lxDev = (ulong)major << 32 | minor;
                    }

                    if(lxAttrb.st_atime != 0)
                    {
                        lxAtime = DateTimeOffset.FromUnixTimeSeconds(lxAttrb.st_atime)
                                                .UtcDateTime.AddTicks(lxAttrb.st_atime_nsec / 100);
                    }

                    if(lxAttrb.st_mtime != 0)
                    {
                        lxMtime = DateTimeOffset.FromUnixTimeSeconds(lxAttrb.st_mtime)
                                                .UtcDateTime.AddTicks(lxAttrb.st_mtime_nsec / 100);
                    }

                    if(lxAttrb.st_ctime != 0)
                    {
                        lxCtime = DateTimeOffset.FromUnixTimeSeconds(lxAttrb.st_ctime)
                                                .UtcDateTime.AddTicks(lxAttrb.st_ctime_nsec / 100);
                    }
                }

                break;
            }

            if(ea.next_entry_offset == 0) break;

            firstPos += (int)ea.next_entry_offset;
        }

        // Second pass: parse individual WSL2 EAs (these override $LXATTRB values)
        while(pos + eaHeaderSize <= end)
        {
            EaAttribute ea = Marshal.ByteArrayToStructureLittleEndian<EaAttribute>(data, pos, eaHeaderSize);

            int nameStart = pos + eaHeaderSize;

            if(nameStart + ea.ea_name_length > end) break;

            string eaName = Encoding.ASCII.GetString(data, nameStart, ea.ea_name_length);

            // EA value follows the name + NUL terminator
            int valueStart = nameStart + ea.ea_name_length + 1;

            if(eaName == EA_LXUID && ea.ea_value_length >= 4 && valueStart + 4 <= end)
                lxUid = BitConverter.ToUInt32(data, valueStart);
            else if(eaName == EA_LXGID && ea.ea_value_length >= 4 && valueStart + 4 <= end)
                lxGid = BitConverter.ToUInt32(data, valueStart);
            else if(eaName == EA_LXMOD && ea.ea_value_length >= 4 && valueStart + 4 <= end)
                lxMod = BitConverter.ToUInt32(data, valueStart);
            else if(eaName == EA_LXDEV && ea.ea_value_length >= 8 && valueStart + 8 <= end)
            {
                var major = BitConverter.ToUInt32(data, valueStart);
                var minor = BitConverter.ToUInt32(data, valueStart + 4);
                lxDev = (ulong)major << 32 | minor;
            }

            if(ea.next_entry_offset == 0) break;

            pos += (int)ea.next_entry_offset;
        }
    }
}