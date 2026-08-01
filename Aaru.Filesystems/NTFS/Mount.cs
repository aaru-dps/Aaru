// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
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
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class NTFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _image     = imagePlugin;
        _partition = partition;
        _encoding  = encoding ?? Encoding.Unicode;

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        // Read boot sector (sector 0)
        AaruLogging.Debug(MODULE_NAME, "Reading boot sector at sector {0}", _partition.Start);

        ErrorNumber errno = _image.ReadSector(_partition.Start, false, out byte[] bootSector, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading boot sector: {0}", errno);

            return errno;
        }

        _bpb = Marshal.ByteArrayToStructureLittleEndian<BiosParameterBlock>(bootSector);

        // Validate OEM name
        string oemName = StringHandlers.CToString(_bpb.oem_name);

        if(oemName != "NTFS    ")
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid OEM name: \"{0}\" (expected \"NTFS    \")", oemName);

            return ErrorNumber.InvalidArgument;
        }

        // Validate boot sector signature
        if(_bpb.signature2 != 0xAA55)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid boot sector signature: 0x{0:X4} (expected 0xAA55)",
                              _bpb.signature2);

            return ErrorNumber.InvalidArgument;
        }

        // Calculate sizes
        _bytesPerSector = _bpb.bps;

        // When spc is negative, it encodes the log2 of bytes per cluster (large cluster support, up to 2MB)
        if(_bpb.spc < 0)
        {
            _bytesPerCluster   = (uint)(1 << -_bpb.spc);
            _sectorsPerCluster = _bytesPerCluster / _bytesPerSector;
        }
        else
        {
            _sectorsPerCluster = (uint)_bpb.spc;
            _bytesPerCluster   = _bytesPerSector * _sectorsPerCluster;
        }

        if(_bpb.mft_rc_clusters > 0)
            _mftRecordSize = (uint)(_bpb.mft_rc_clusters * _bytesPerCluster);
        else
            _mftRecordSize = (uint)(1 << -_bpb.mft_rc_clusters);

        if(_bpb.index_blk_cts > 0)
            _indexBlockSize = (uint)(_bpb.index_blk_cts * _bytesPerCluster);
        else
            _indexBlockSize = (uint)(1 << -_bpb.index_blk_cts);

        AaruLogging.Debug(MODULE_NAME, "Bytes per sector: {0}",         _bytesPerSector);
        AaruLogging.Debug(MODULE_NAME, "Sectors per cluster: {0}",      _sectorsPerCluster);
        AaruLogging.Debug(MODULE_NAME, "Bytes per cluster: {0}",        _bytesPerCluster);
        AaruLogging.Debug(MODULE_NAME, "MFT record size: {0}",          _mftRecordSize);
        AaruLogging.Debug(MODULE_NAME, "Index block size: {0}",         _indexBlockSize);
        AaruLogging.Debug(MODULE_NAME, "MFT starts at cluster {0}",     _bpb.mft_lsn);
        AaruLogging.Debug(MODULE_NAME, "MFTMirr starts at cluster {0}", _bpb.mftmirror_lsn);

        // Read MFT record #0 ($MFT) to validate the MFT is readable
        errno = ReadMftRecord((uint)SystemFileNumber.Mft, out byte[] mftRecord0);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading $MFT record: {0}", errno);

            return errno;
        }

        MftRecord mftHeader = ParseMftRecordHeader(mftRecord0);

        if(mftHeader.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid $MFT magic: 0x{0:X8} (expected 0x{1:X8})",
                              (uint)mftHeader.magic,
                              (uint)NtfsRecordMagic.File);

            return ErrorNumber.InvalidArgument;
        }

        if(!mftHeader.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "$MFT record is not marked as in use");

            return ErrorNumber.InvalidArgument;
        }

        // Parse $MFT's own $DATA attribute data runs for fragmented MFT support
        _mftDataRuns = ParseMftDataRuns(mftRecord0, mftHeader);

        if(_mftDataRuns == null || _mftDataRuns.Count == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not parse $MFT data runs, MFT access will be limited");

            _mftDataRuns = null;
        }

        // Validate $MFTMirr: compare the first 4 system records against their mirror copies
        ValidateMftMirror();

        // Read MFT record #3 ($Volume) to get volume name and version info
        errno = ReadMftRecord((uint)SystemFileNumber.Volume, out byte[] volumeRecord);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading $Volume record: {0}", errno);

            return errno;
        }

        MftRecord volHeader = ParseMftRecordHeader(volumeRecord);

        if(volHeader.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid $Volume magic: 0x{0:X8} (expected 0x{1:X8})",
                              (uint)volHeader.magic,
                              (uint)NtfsRecordMagic.File);

            return ErrorNumber.InvalidArgument;
        }

        if(!volHeader.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "$Volume record is not marked as in use");

            return ErrorNumber.InvalidArgument;
        }

        // Parse $Volume attributes to get volume name and version
        var  volumeName = "";
        byte majorVer   = 0;
        byte minorVer   = 0;
        var  isDirty    = false;

        ParseVolumeRecord(volumeRecord, volHeader, out volumeName, out majorVer, out minorVer, out isDirty);

        AaruLogging.Debug(MODULE_NAME, "Volume name: \"{0}\"",  volumeName);
        AaruLogging.Debug(MODULE_NAME, "NTFS version: {0}.{1}", majorVer, minorVer);
        AaruLogging.Debug(MODULE_NAME, "Volume dirty: {0}",     isDirty);

        _ntfsMajorVersion = majorVer;
        _ntfsMinorVersion = minorVer;
        _ntfsVersion      = $"{majorVer}.{minorVer}";

        // Load $AttrDef (MFT record #4) for attribute type definitions
        _attributeDefinitions = new Dictionary<AttributeType, AttrDef>();
        LoadAttributeDefinitions();

        // Read MFT record #5 (root directory)
        errno = ReadMftRecord((uint)SystemFileNumber.Root, out byte[] rootRecord);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory record: {0}", errno);

            return errno;
        }

        MftRecord rootHeader = ParseMftRecordHeader(rootRecord);

        if(rootHeader.magic != NtfsRecordMagic.File)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid root directory magic: 0x{0:X8} (expected 0x{1:X8})",
                              (uint)rootHeader.magic,
                              (uint)NtfsRecordMagic.File);

            return ErrorNumber.InvalidArgument;
        }

        if(!rootHeader.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "Root directory record is not marked as in use");

            return ErrorNumber.InvalidArgument;
        }

        // Validate the root record is a directory
        if(!rootHeader.flags.HasFlag(MftRecordFlags.IsDirectory))
        {
            AaruLogging.Debug(MODULE_NAME, "Root MFT record is not a directory");

            return ErrorNumber.InvalidArgument;
        }

        if(!rootHeader.flags.HasFlag(MftRecordFlags.InUse))
        {
            AaruLogging.Debug(MODULE_NAME, "Root MFT record is not in use");

            return ErrorNumber.InvalidArgument;
        }

        // Parse $Secure (MFT record #9) to load centralized security descriptors (NTFS 3.0+)
        _securityDescriptors = new Dictionary<uint, byte[]>();

        if(_ntfsMajorVersion >= 3) LoadSecureDescriptors();

        // Initialize caches
        _rootDirectoryCache = new Dictionary<string, ulong>();
        _directoryCache.Clear();

        // Cache root directory entries from the $INDEX_ROOT attribute
        errno = CacheRootDirectory(rootRecord, rootHeader);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error caching root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory contains {0} entries", _rootDirectoryCache.Count);

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            ClusterSize  = _bytesPerCluster,
            Clusters     = (ulong)_bpb.sectors / _sectorsPerCluster,
            VolumeSerial = $"{_bpb.serial_no:X16}",
            VolumeName   = volumeName,
            Dirty        = isDirty
        };

        // Parse $Bitmap (MFT #6) to count free clusters
        ulong totalClusters = (ulong)_bpb.sectors / _sectorsPerCluster;
        ulong freeClusters  = CountFreeClusters(totalClusters);

        // Parse $MFT's $BITMAP attribute to count in-use MFT records
        (ulong totalFiles, ulong freeFiles) = CountMftRecords();

        // Build filesystem info for StatFs
        _statfs = new FileSystemInfo
        {
            Blocks         = totalClusters,
            FilenameLength = 255,
            Files          = totalFiles,
            FreeBlocks     = freeClusters,
            FreeFiles      = freeFiles,
            Id = new FileSystemId
            {
                IsLong   = true,
                Serial64 = _bpb.serial_no
            },
            PluginId = Id,
            Type     = FS_TYPE
        };

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _rootDirectoryCache?.Clear();
        _directoryCache.Clear();
        _securityDescriptors?.Clear();
        _attributeDefinitions?.Clear();
        _ntfsMajorVersion = 0;
        _ntfsMinorVersion = 0;
        _mounted          = false;

        return ErrorNumber.NoError;
    }

    /// <summary>Parses the $Volume MFT record to extract volume name, version, and dirty flag.</summary>
    /// <param name="recordData">Raw MFT record data after USA fixup.</param>
    /// <param name="header">Parsed MFT record header.</param>
    /// <param name="volumeName">Output volume name string.</param>
    /// <param name="majorVer">Output NTFS major version.</param>
    /// <param name="minorVer">Output NTFS minor version.</param>
    /// <param name="isDirty">Output dirty flag.</param>
    void ParseVolumeRecord(byte[]   recordData, in  MftRecord header, out string volumeName, out byte majorVer,
                           out byte minorVer,   out bool      isDirty)
    {
        volumeName = "";
        majorVer   = 0;
        minorVer   = 0;
        isDirty    = false;

        int offset = header.attrs_offset;

        while(offset + 4 <= recordData.Length)
        {
            var attrType = (AttributeType)BitConverter.ToUInt32(recordData, offset);

            if(attrType == AttributeType.End || attrType == AttributeType.Unused) break;

            var attrLength = BitConverter.ToUInt32(recordData, offset + 4);

            if(attrLength == 0 || offset + attrLength > recordData.Length) break;

            byte nonResident = recordData[offset + 8];

            if(nonResident == 0) // Resident attribute
            {
                var valueLength = BitConverter.ToUInt32(recordData, offset + 0x10);
                var valueOffset = BitConverter.ToUInt16(recordData, offset + 0x14);

                int valueStart = offset + valueOffset;

                if(valueStart + valueLength > recordData.Length)
                {
                    offset += (int)attrLength;

                    continue;
                }

                switch(attrType)
                {
                    case AttributeType.VolumeName:
                        if(valueLength > 0)
                            volumeName = Encoding.Unicode.GetString(recordData, valueStart, (int)valueLength);

                        break;

                    case AttributeType.VolumeInformation:
                        if(valueLength >= 12)
                        {
                            VolumeInformation volInfo =
                                Marshal.ByteArrayToStructureLittleEndian<VolumeInformation>(recordData,
                                    valueStart,
                                    (int)valueLength);

                            majorVer = volInfo.major_ver;
                            minorVer = volInfo.minor_ver;
                            isDirty  = volInfo.flags.HasFlag(VolumeFlags.IsDirty);
                        }

                        break;
                }
            }

            offset += (int)attrLength;
        }
    }

    /// <summary>
    ///     Loads security descriptors from the $Secure system file's $SDS named data stream.
    ///     Populates <see cref="_securityDescriptors" /> with a mapping from security_id to raw descriptor bytes.
    /// </summary>
    void LoadSecureDescriptors()
    {
        ErrorNumber errno = ReadMftRecord((uint)SystemFileNumber.Secure, out byte[] secureRecord);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading $Secure MFT record: {0}", errno);

            return;
        }

        MftRecord secureHeader = ParseMftRecordHeader(secureRecord);

        if(secureHeader.magic != NtfsRecordMagic.File) return;

        if(!secureHeader.flags.HasFlag(MftRecordFlags.InUse)) return;

        // Read the $SDS stream data (may be non-resident)
        byte[] sdsData = null;

        // Try assembling non-resident data runs for $SDS
        ErrorNumber runErrno = AssembleNonResidentRuns((uint)SystemFileNumber.Secure,
                                                       AttributeType.Data,
                                                       "$SDS",
                                                       out List<(long offset, long length)> allDataRuns,
                                                       out long sdsDataSize,
                                                       out _,
                                                       out _,
                                                       out _);

        if(runErrno == ErrorNumber.NoError && allDataRuns.Count > 0 && sdsDataSize > 0)
        {
            byte[] readBuf = Array.Empty<byte>();
            errno = ReadNonResidentData(allDataRuns, sdsDataSize, ref readBuf);

            if(errno == ErrorNumber.NoError) sdsData = readBuf;
        }
        else
        {
            // Resident $SDS (unlikely but handle it)
            ErrorNumber findErrno = FindAttributes(secureRecord,
                                                   secureHeader,
                                                   (uint)SystemFileNumber.Secure,
                                                   AttributeType.Data,
                                                   "$SDS",
                                                   out List<FoundAttribute> sdsResults);

            if(findErrno == ErrorNumber.NoError && sdsResults.Count > 0)
            {
                FoundAttribute attr   = sdsResults[0];
                byte           nonRes = attr.RecordData[attr.Offset + 8];

                if(nonRes == 0)
                {
                    var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
                    var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

                    int valueStart = attr.Offset + valueOffset;

                    if(valueStart + valueLength <= attr.RecordData.Length && valueLength > 0)
                    {
                        sdsData = new byte[valueLength];
                        Array.Copy(attr.RecordData, valueStart, sdsData, 0, valueLength);
                    }
                }
            }
        }

        if(sdsData == null || sdsData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read $Secure::$SDS data stream");

            return;
        }

        // Parse SDS entries: each has a 20-byte SdsEntryHeader followed by the security descriptor
        int sdsHeaderSize = Marshal.SizeOf<SdsEntryHeader>();
        var pos           = 0;

        while(pos + sdsHeaderSize <= sdsData.Length)
        {
            SdsEntryHeader entryHeader =
                Marshal.ByteArrayToStructureLittleEndian<SdsEntryHeader>(sdsData, pos, sdsHeaderSize);

            // End marker or invalid entry
            if(entryHeader.size < sdsHeaderSize || entryHeader.security_id == 0) break;

            // Validate bounds
            if(pos + entryHeader.size > sdsData.Length) break;

            int sdSize = (int)entryHeader.size - sdsHeaderSize;

            if(sdSize > 0 && !_securityDescriptors.ContainsKey(entryHeader.security_id))
            {
                var sd = new byte[sdSize];
                Array.Copy(sdsData, pos + sdsHeaderSize, sd, 0, sdSize);
                _securityDescriptors[entryHeader.security_id] = sd;
            }

            // Entries are 16-byte aligned
            var totalSize = (int)(entryHeader.size + 15 & ~15u);

            if(totalSize == 0) break;

            pos += totalSize;
        }

        AaruLogging.Debug(MODULE_NAME, "Loaded {0} security descriptors from $Secure", _securityDescriptors.Count);
    }

    /// <summary>
    ///     Reads the volume $Bitmap (MFT record #6) and counts how many clusters are free.
    /// </summary>
    /// <param name="totalClusters">Total number of clusters on the volume.</param>
    /// <returns>Number of free clusters, or 0 if the bitmap cannot be read.</returns>
    ulong CountFreeClusters(ulong totalClusters)
    {
        // Try non-resident first (volume bitmap is almost always non-resident)
        ErrorNumber runErrno = AssembleNonResidentRuns((uint)SystemFileNumber.Bitmap,
                                                       AttributeType.Data,
                                                       null,
                                                       out List<(long offset, long length)> dataRuns,
                                                       out long dataSize,
                                                       out _,
                                                       out _,
                                                       out _);

        byte[] bitmapData = null;

        if(runErrno == ErrorNumber.NoError && dataRuns.Count > 0 && dataSize > 0)
        {
            byte[]      readBuf = Array.Empty<byte>();
            ErrorNumber errno   = ReadNonResidentData(dataRuns, dataSize, ref readBuf);

            if(errno == ErrorNumber.NoError) bitmapData = readBuf;
        }
        else
        {
            // Resident fallback (unlikely for volume bitmap)
            ErrorNumber errno = ReadMftRecord((uint)SystemFileNumber.Bitmap, out byte[] bitmapRecord);

            if(errno != ErrorNumber.NoError) return 0;

            MftRecord bitmapHeader = ParseMftRecordHeader(bitmapRecord);

            if(bitmapHeader.magic != NtfsRecordMagic.File) return 0;

            if(!bitmapHeader.flags.HasFlag(MftRecordFlags.InUse)) return 0;

            ErrorNumber findErrno = FindAttributes(bitmapRecord,
                                                   bitmapHeader,
                                                   (uint)SystemFileNumber.Bitmap,
                                                   AttributeType.Data,
                                                   null,
                                                   out List<FoundAttribute> results);

            if(findErrno != ErrorNumber.NoError || results.Count == 0) return 0;

            FoundAttribute attr   = results[0];
            byte           nonRes = attr.RecordData[attr.Offset + 8];

            if(nonRes != 0) return 0;

            var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
            var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

            int valueStart = attr.Offset + valueOffset;

            if(valueStart + valueLength > attr.RecordData.Length || valueLength == 0) return 0;

            bitmapData = new byte[valueLength];
            Array.Copy(attr.RecordData, valueStart, bitmapData, 0, valueLength);
        }

        if(bitmapData == null || bitmapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read $Bitmap data");

            return 0;
        }

        ulong freeClusters = CountZeroBits(bitmapData, totalClusters);

        AaruLogging.Debug(MODULE_NAME,
                          "Volume bitmap: {0} total clusters, {1} free clusters",
                          totalClusters,
                          freeClusters);

        return freeClusters;
    }

    /// <summary>
    ///     Reads the $MFT's own $BITMAP attribute and counts in-use and free MFT records.
    /// </summary>
    /// <returns>Tuple of (total MFT records, free MFT records), or (0, 0) on failure.</returns>
    (ulong totalFiles, ulong freeFiles) CountMftRecords()
    {
        // The MFT bitmap is the $BITMAP attribute of the $MFT file (record #0)
        ErrorNumber runErrno = AssembleNonResidentRuns((uint)SystemFileNumber.Mft,
                                                       AttributeType.Bitmap,
                                                       null,
                                                       out List<(long offset, long length)> dataRuns,
                                                       out long dataSize,
                                                       out _,
                                                       out _,
                                                       out _);

        byte[] mftBitmapData = null;

        if(runErrno == ErrorNumber.NoError && dataRuns.Count > 0 && dataSize > 0)
        {
            byte[]      readBuf = Array.Empty<byte>();
            ErrorNumber errno   = ReadNonResidentData(dataRuns, dataSize, ref readBuf);

            if(errno == ErrorNumber.NoError) mftBitmapData = readBuf;
        }
        else
        {
            // Resident fallback (possible for very small volumes)
            ErrorNumber errno = ReadMftRecord((uint)SystemFileNumber.Mft, out byte[] mftRecord);

            if(errno != ErrorNumber.NoError) return (0, 0);

            MftRecord mftHeader = ParseMftRecordHeader(mftRecord);

            if(mftHeader.magic != NtfsRecordMagic.File) return (0, 0);

            if(!mftHeader.flags.HasFlag(MftRecordFlags.InUse)) return (0, 0);

            ErrorNumber findErrno = FindAttributes(mftRecord,
                                                   mftHeader,
                                                   (uint)SystemFileNumber.Mft,
                                                   AttributeType.Bitmap,
                                                   null,
                                                   out List<FoundAttribute> results);

            if(findErrno != ErrorNumber.NoError || results.Count == 0) return (0, 0);

            FoundAttribute attr   = results[0];
            byte           nonRes = attr.RecordData[attr.Offset + 8];

            if(nonRes != 0) return (0, 0);

            var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
            var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

            int valueStart = attr.Offset + valueOffset;

            if(valueStart + valueLength > attr.RecordData.Length || valueLength == 0) return (0, 0);

            mftBitmapData = new byte[valueLength];
            Array.Copy(attr.RecordData, valueStart, mftBitmapData, 0, valueLength);
        }

        if(mftBitmapData == null || mftBitmapData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read $MFT bitmap");

            return (0, 0);
        }

        // Each bit = one MFT record. Total records = bitmap bits.
        ulong totalRecords = (ulong)mftBitmapData.Length * 8;
        ulong usedRecords  = CountOneBits(mftBitmapData, totalRecords);
        ulong freeRecords  = totalRecords - usedRecords;

        AaruLogging.Debug(MODULE_NAME,
                          "MFT bitmap: {0} total records, {1} in use, {2} free",
                          totalRecords,
                          usedRecords,
                          freeRecords);

        return (totalRecords, freeRecords);
    }

    /// <summary>Counts the number of zero (free) bits in a bitmap, up to <paramref name="maxBits" />.</summary>
    /// <param name="bitmap">Raw bitmap data.</param>
    /// <param name="maxBits">Maximum number of bits to count (prevents counting padding bits).</param>
    /// <returns>Number of zero bits.</returns>
    static ulong CountZeroBits(byte[] bitmap, ulong maxBits)
    {
        ulong zeroBits = 0;
        ulong bitIndex = 0;

        for(var i = 0; i < bitmap.Length && bitIndex < maxBits; i++)
        {
            byte b = bitmap[i];

            // Fast path: whole byte
            if(bitIndex + 8 <= maxBits)
            {
                // Count zero bits = 8 - popcount
                zeroBits += (ulong)(8 - PopCount(b));
                bitIndex += 8;
            }
            else
            {
                // Partial byte at end
                for(var bit = 0; bit < 8 && bitIndex < maxBits; bit++)
                {
                    if((b & 1 << bit) == 0) zeroBits++;

                    bitIndex++;
                }
            }
        }

        return zeroBits;
    }

    /// <summary>Counts the number of set (in-use) bits in a bitmap, up to <paramref name="maxBits" />.</summary>
    /// <param name="bitmap">Raw bitmap data.</param>
    /// <param name="maxBits">Maximum number of bits to count.</param>
    /// <returns>Number of set bits.</returns>
    static ulong CountOneBits(byte[] bitmap, ulong maxBits)
    {
        ulong oneBits  = 0;
        ulong bitIndex = 0;

        for(var i = 0; i < bitmap.Length && bitIndex < maxBits; i++)
        {
            byte b = bitmap[i];

            if(bitIndex + 8 <= maxBits)
            {
                oneBits  += (ulong)PopCount(b);
                bitIndex += 8;
            }
            else
            {
                for(var bit = 0; bit < 8 && bitIndex < maxBits; bit++)
                {
                    if((b & 1 << bit) != 0) oneBits++;

                    bitIndex++;
                }
            }
        }

        return oneBits;
    }

    /// <summary>Returns the number of set bits in a byte (population count).</summary>
    /// <param name="b">Input byte.</param>
    /// <returns>Number of set bits (0-8).</returns>
    static int PopCount(byte b)
    {
        int v = b;
        v -= v >> 1 & 0x55;
        v =  (v & 0x33) + (v >> 2 & 0x33);

        return v + (v >> 4) & 0x0F;
    }

    /// <summary>
    ///     Validates the $MFTMirr by reading the mirrored copies of the first system MFT records
    ///     and comparing them against the primary MFT records. Logs warnings for any mismatches.
    /// </summary>
    void ValidateMftMirror()
    {
        // $MFTMirr contains copies of the first 4 MFT records on NTFS 3.0+
        // (records 0–3: $MFT, $MFTMirr, $LogFile, $Volume)
        const int MIRROR_RECORD_COUNT = 4;

        long mirrorStartByte = _bpb.mftmirror_lsn * _bytesPerCluster;

        // Read all mirror records at once
        uint totalMirrorSectors = (_mftRecordSize * MIRROR_RECORD_COUNT + _bytesPerSector - 1) / _bytesPerSector;

        ErrorNumber errno = _image.ReadSectors(_partition.Start + (ulong)(mirrorStartByte / _bytesPerSector),
                                               false,
                                               totalMirrorSectors,
                                               out byte[] mirrorData,
                                               out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Error reading $MFTMirr data at cluster {0}: {1}",
                              _bpb.mftmirror_lsn,
                              errno);

            return;
        }

        var mirrorMismatches = 0;

        for(uint i = 0; i < MIRROR_RECORD_COUNT; i++)
        {
            var mirrorOffset = (int)(i * _mftRecordSize);

            if(mirrorOffset + _mftRecordSize > mirrorData.Length) break;

            // Extract the mirror record
            var mirrorRecord = new byte[_mftRecordSize];
            Array.Copy(mirrorData, mirrorOffset, mirrorRecord, 0, _mftRecordSize);

            // Apply USA fixup to mirror record
            ErrorNumber fixupErrno = ApplyUsaFixup(mirrorRecord);

            if(fixupErrno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "$MFTMirr record {0}: USA fixup failed ({1})", i, fixupErrno);
                mirrorMismatches++;

                continue;
            }

            // Validate magic
            MftRecord mirrorHeader = ParseMftRecordHeader(mirrorRecord);

            if(mirrorHeader.magic != NtfsRecordMagic.File)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "$MFTMirr record {0}: invalid magic 0x{1:X8}",
                                  i,
                                  (uint)mirrorHeader.magic);

                mirrorMismatches++;

                continue;
            }

            if(!mirrorHeader.flags.HasFlag(MftRecordFlags.InUse))
            {
                AaruLogging.Debug(MODULE_NAME, "$MFTMirr record {0}: not marked as in use", i);

                mirrorMismatches++;

                continue;
            }

            // Read the corresponding primary MFT record
            errno = ReadMftRecord(i, out byte[] primaryRecord);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "$MFTMirr record {0}: could not read primary MFT record ({1})",
                                  i,
                                  errno);

                mirrorMismatches++;

                continue;
            }

            // Compare the records byte-for-byte
            bool match = primaryRecord.Length == mirrorRecord.Length;

            if(match)
            {
                for(var b = 0; b < primaryRecord.Length; b++)
                {
                    if(primaryRecord[b] != mirrorRecord[b])
                    {
                        match = false;

                        break;
                    }
                }
            }

            if(!match)
            {
                AaruLogging.Debug(MODULE_NAME,
                                  "$MFTMirr record {0}: does not match primary MFT record (volume may be dirty)",
                                  i);

                mirrorMismatches++;
            }
            else
                AaruLogging.Debug(MODULE_NAME, "$MFTMirr record {0}: matches primary MFT", i);
        }

        if(mirrorMismatches > 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "$MFTMirr validation: {0} of {1} records do not match primary MFT",
                              mirrorMismatches,
                              MIRROR_RECORD_COUNT);
        }
        else
        {
            AaruLogging.Debug(MODULE_NAME,
                              "$MFTMirr validation: all {0} records match primary MFT",
                              MIRROR_RECORD_COUNT);
        }
    }

    /// <summary>
    ///     Loads the attribute definition table from the $AttrDef system file (MFT record #4).
    ///     Populates <see cref="_attributeDefinitions" /> with a mapping from attribute type to its definition.
    /// </summary>
    void LoadAttributeDefinitions()
    {
        // Read the $AttrDef unnamed $DATA attribute
        ErrorNumber runErrno = AssembleNonResidentRuns((uint)SystemFileNumber.AttrDef,
                                                       AttributeType.Data,
                                                       null,
                                                       out List<(long offset, long length)> dataRuns,
                                                       out long dataSize,
                                                       out _,
                                                       out _,
                                                       out _);

        byte[] attrDefData = null;

        if(runErrno == ErrorNumber.NoError && dataRuns.Count > 0 && dataSize > 0)
        {
            byte[]      readBuf = Array.Empty<byte>();
            ErrorNumber errno   = ReadNonResidentData(dataRuns, dataSize, ref readBuf);

            if(errno == ErrorNumber.NoError) attrDefData = readBuf;
        }
        else
        {
            // Try resident fallback
            ErrorNumber errno = ReadMftRecord((uint)SystemFileNumber.AttrDef, out byte[] attrDefRecord);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error reading $AttrDef MFT record: {0}", errno);

                return;
            }

            MftRecord attrDefHeader = ParseMftRecordHeader(attrDefRecord);

            if(attrDefHeader.magic != NtfsRecordMagic.File) return;

            if(!attrDefHeader.flags.HasFlag(MftRecordFlags.InUse)) return;

            ErrorNumber findErrno = FindAttributes(attrDefRecord,
                                                   attrDefHeader,
                                                   (uint)SystemFileNumber.AttrDef,
                                                   AttributeType.Data,
                                                   null,
                                                   out List<FoundAttribute> results);

            if(findErrno == ErrorNumber.NoError && results.Count > 0)
            {
                FoundAttribute attr   = results[0];
                byte           nonRes = attr.RecordData[attr.Offset + 8];

                if(nonRes == 0)
                {
                    var valueOffset = BitConverter.ToUInt16(attr.RecordData, attr.Offset + 0x14);
                    var valueLength = BitConverter.ToUInt32(attr.RecordData, attr.Offset + 0x10);

                    int valueStart = attr.Offset + valueOffset;

                    if(valueStart + valueLength <= attr.RecordData.Length && valueLength > 0)
                    {
                        attrDefData = new byte[valueLength];
                        Array.Copy(attr.RecordData, valueStart, attrDefData, 0, valueLength);
                    }
                }
            }
        }

        if(attrDefData == null || attrDefData.Length == 0)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read $AttrDef data");

            return;
        }

        // Parse entries: each AttrDef is 160 bytes (0xA0)
        int entrySize = Marshal.SizeOf<AttrDef>();
        var pos       = 0;

        while(pos + entrySize <= attrDefData.Length)
        {
            AttrDef entry = Marshal.ByteArrayToStructureLittleEndian<AttrDef>(attrDefData, pos, entrySize);

            // End of table: type == 0
            if(entry.type == 0) break;

            if(!_attributeDefinitions.ContainsKey(entry.type)) _attributeDefinitions[entry.type] = entry;

            pos += entrySize;
        }

        AaruLogging.Debug(MODULE_NAME, "Loaded {0} attribute definitions from $AttrDef", _attributeDefinitions.Count);
    }
}