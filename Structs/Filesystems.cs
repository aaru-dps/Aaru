// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common structures.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations and structures of common usage by filesystem
//     plugins.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.CommonTypes.Structs
{
    /// <summary>File attributes.</summary>
    [Flags]
    public enum FileAttributes : ulong
    {
        /// <summary>File has no attributes</summary>
        None = 0,
        /// <summary>File is an alias (Mac OS)</summary>
        Alias = 0x01,
        /// <summary>Indicates that the file can only be writable appended</summary>
        AppendOnly = 0x02,
        /// <summary>File is candidate for archival/backup</summary>
        Archive = 0x04,
        /// <summary>File is a block device</summary>
        BlockDevice = 0x08,
        /// <summary>File is stored on filesystem block units instead of device sectors</summary>
        BlockUnits = 0x10,
        /// <summary>Directory is a bundle or file contains a BNDL resource</summary>
        Bundle = 0x20,
        /// <summary>File is a char device</summary>
        CharDevice = 0x40,
        /// <summary>File is compressed</summary>
        Compressed = 0x80,
        /// <summary>File is compressed and should not be uncompressed on read</summary>
        CompressedRaw = 0x100,
        /// <summary>File has compression errors</summary>
        CompressionError = 0x200,
        /// <summary>Compressed file is dirty</summary>
        CompressionDirty = 0x400,
        /// <summary>File is a device</summary>
        Device = 0x800,
        /// <summary>File is a directory</summary>
        Directory = 0x1000,
        /// <summary>File is encrypted</summary>
        Encrypted = 0x2000,
        /// <summary>File is stored on disk using extents</summary>
        Extents = 0x4000,
        /// <summary>File is a FIFO</summary>
        FIFO = 0x8000,
        /// <summary>File is a normal file</summary>
        File = 0x10000,
        /// <summary>File is a Mac OS file containing desktop databases that has already been added to the desktop database</summary>
        HasBeenInited = 0x20000,
        /// <summary>File contains an icon resource / EA</summary>
        HasCustomIcon = 0x40000,
        /// <summary>File is a Mac OS extension or control panel lacking INIT resources</summary>
        HasNoINITs = 0x80000,
        /// <summary>File is hidden/invisible</summary>
        Hidden = 0x100000,
        /// <summary>File cannot be written, deleted, modified or linked to</summary>
        Immutable = 0x200000,
        /// <summary>Directory is indexed using hashed trees</summary>
        IndexedDirectory = 0x400000,
        /// <summary>File contents are stored alongside its inode (or equivalent)</summary>
        Inline = 0x800000,
        /// <summary>File contains integrity checks</summary>
        IntegrityStream = 0x1000000,
        /// <summary>File is on desktop</summary>
        IsOnDesk = 0x2000000,
        /// <summary>File changes are written to filesystem journal before being written to file itself</summary>
        Journaled = 0x4000000,
        /// <summary>Access time will not be modified</summary>
        NoAccessTime = 0x8000000,
        /// <summary>File will not be subject to copy-on-write</summary>
        NoCopyOnWrite = 0x10000000,
        /// <summary>File will not be backed up</summary>
        NoDump = 0x20000000,
        /// <summary>File contents should not be scrubbed</summary>
        NoScrub = 0x40000000,
        /// <summary>File contents should not be indexed</summary>
        NotIndexed = 0x80000000,
        /// <summary>File is offline</summary>
        Offline = 0x100000000,
        /// <summary>File is password protected, but contents are not encrypted on disk</summary>
        Password = 0x200000000,
        /// <summary>File is read-only</summary>
        ReadOnly = 0x400000000,
        /// <summary>File is a reparse point</summary>
        ReparsePoint = 0x800000000,
        /// <summary>When file is removed its content will be overwritten with zeroes</summary>
        Secured = 0x1000000000,
        /// <summary>File contents are sparse</summary>
        Sparse = 0x2000000000,
        /// <summary>File is a shadow (OS/2)</summary>
        Shadow = 0x4000000000,
        /// <summary>File is shared</summary>
        Shared = 0x8000000000,
        /// <summary>File is a stationery</summary>
        Stationery = 0x10000000000,
        /// <summary>File is a symbolic link</summary>
        Symlink = 0x20000000000,
        /// <summary>File writes are synchronously written to disk</summary>
        Sync = 0x40000000000,
        /// <summary>File belongs to the operating system</summary>
        System = 0x80000000000,
        /// <summary>If file end is a partial block its content will be merged with other files</summary>
        TailMerged = 0x100000000000,
        /// <summary>File is temporary</summary>
        Temporary = 0x200000000000,
        /// <summary>Subdirectories inside of this directory are not related and should be allocated elsewhere</summary>
        TopDirectory = 0x400000000000,
        /// <summary>If file is deleted, contents should be stored, for a possible future undeletion</summary>
        Undeletable = 0x800000000000,
        /// <summary>File is a pipe</summary>
        Pipe = 0x1000000000000,
        /// <summary>File is a socket</summary>
        Socket = 0x2000000000000
    }

    /// <summary>Information about a file entry</summary>
    public class FileEntryInfo
    {
        /// <summary>File attributes</summary>
        public FileAttributes Attributes;
        /// <summary>File length in blocks</summary>
        public long Blocks;
        /// <summary>File block size in bytes</summary>
        public long BlockSize;
        /// <summary>If file points to a device, device number. Null if the underlying filesystem does not support them.</summary>
        public ulong? DeviceNo;
        /// <summary>POSIX group ID. Null if the underlying filesystem does not support them.</summary>
        public ulong? GID;
        /// <summary>inode number for this file (or other unique identifier for the volume)</summary>
        public ulong Inode;
        /// <summary>File length in bytes</summary>
        public long Length;
        /// <summary>Number of hard links pointing to this file (. and .. entries count as hard links)</summary>
        public ulong Links;
        /// <summary>POSIX permissions/mode for this file. Null if the underlying filesystem does not support them.</summary>
        public uint? Mode;
        /// <summary>POSIX owner ID. Null if the underlying filesystem does not support them.</summary>
        public ulong? UID;
        /// <summary>File creation date in UTC. Null if the underlying filesystem does not support them.</summary>
        public DateTime? CreationTimeUtc { get; set; }
        /// <summary>File last access date in UTC. Null if the underlying filesystem does not support them.</summary>
        public DateTime? AccessTimeUtc { get; set; }
        /// <summary>File attributes change date in UTC. Null if the underlying filesystem does not support them.</summary>
        public DateTime? StatusChangeTimeUtc { get; set; }
        /// <summary>File last backup date in UTC. Null if the underlying filesystem does not support them.</summary>
        public DateTime? BackupTimeUtc { get; set; }
        /// <summary>File last modification date in UTC. Null if the underlying filesystem does not support them.</summary>
        public DateTime? LastWriteTimeUtc { get; set; }

        /// <summary>File creation date. Null if the underlying filesystem does not support them.</summary>
        public DateTime? CreationTime
        {
            get => CreationTimeUtc?.ToLocalTime();
            set => CreationTimeUtc = value?.ToUniversalTime();
        }

        /// <summary>File last access date. Null if the underlying filesystem does not support them.</summary>
        public DateTime? AccessTime
        {
            get => AccessTimeUtc?.ToLocalTime();
            set => AccessTimeUtc = value?.ToUniversalTime();
        }

        /// <summary>File attributes change date. Null if the underlying filesystem does not support them.</summary>
        public DateTime? StatusChangeTime
        {
            get => StatusChangeTimeUtc?.ToLocalTime();
            set => StatusChangeTimeUtc = value?.ToUniversalTime();
        }

        /// <summary>File last backup date. Null if the underlying filesystem does not support them.</summary>
        public DateTime? BackupTime
        {
            get => BackupTimeUtc?.ToLocalTime();
            set => BackupTimeUtc = value?.ToUniversalTime();
        }

        /// <summary>File last modification date. Null if the underlying filesystem does not support them.</summary>
        public DateTime? LastWriteTime
        {
            get => LastWriteTimeUtc?.ToLocalTime();
            set => LastWriteTimeUtc = value?.ToUniversalTime();
        }
    }

    public class FileSystemInfo
    {
        /// <summary>Blocks for this filesystem</summary>
        public ulong Blocks;
        /// <summary>Maximum length of filenames on this filesystem</summary>
        public ushort FilenameLength;
        /// <summary>Files on this filesystem</summary>
        public ulong Files;
        /// <summary>Blocks free on this filesystem</summary>
        public ulong FreeBlocks;
        /// <summary>Free inodes on this filesystem</summary>
        public ulong FreeFiles;
        /// <summary>Filesystem ID</summary>
        public FileSystemId Id;
        /// <summary>ID of plugin for this file</summary>
        public Guid PluginId;
        /// <summary>Filesystem type</summary>
        public string Type;

        public FileSystemInfo() => Id = new FileSystemId();

        public FileSystemInfo ShallowCopy() => (FileSystemInfo)MemberwiseClone();
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FileSystemId
    {
        [FieldOffset(0)]
        public bool IsInt;
        [FieldOffset(1)]
        public bool IsLong;
        [FieldOffset(2)]
        public bool IsGuid;

        [FieldOffset(3)]
        public uint Serial32;
        [FieldOffset(3)]
        public ulong Serial64;
        [FieldOffset(3)]
        public Guid uuid;
    }

    /// <summary>Errors</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Errno
    {
        /// <summary>No error happened</summary>
        NoError = 0,
        /// <summary>Access denied</summary>
        AccessDenied = -13,
        /// <summary>Busy, cannot complete</summary>
        Busy = -16,
        /// <summary>File is too large</summary>
        FileTooLarge = -27,
        /// <summary>Invalid argument</summary>
        InvalidArgument = -22,
        /// <summary>I/O error</summary>
        InOutError = -5,
        /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
        IsDirectory = -21,
        /// <summary>Name is too long</summary>
        NameTooLong = -36,
        /// <summary>There is no data available</summary>
        NoData = 61,
        /// <summary>There is no such attribute</summary>
        NoSuchExtendedAttribute = NoData,
        /// <summary>No such device</summary>
        NoSuchDevice = -19,
        /// <summary>No such file or directory</summary>
        NoSuchFile = -2,
        /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
        NotDirectory = -20,
        /// <summary>Not implemented</summary>
        NotImplemented = -38,
        /// <summary>Not supported</summary>
        NotSupported = -252,
        /// <summary>Link is severed</summary>
        SeveredLink = -67,
        /// <summary>Access denied</summary>
        EACCES = AccessDenied,
        /// <summary>Busy, cannot complete</summary>
        EBUSY = Busy,
        /// <summary>File is too large</summary>
        EFBIG = FileTooLarge,
        /// <summary>Invalid argument</summary>
        EINVAL = InvalidArgument,
        /// <summary>I/O error</summary>
        EIO = InOutError,
        /// <summary>Is a directory (e.g.: trying to Read() a dir)</summary>
        EISDIR = IsDirectory,
        /// <summary>Name is too long</summary>
        ENAMETOOLONG = NameTooLong,
        /// <summary>There is no such attribute</summary>
        ENOATTR = NoSuchExtendedAttribute,
        /// <summary>There is no data available</summary>
        ENODATA = NoData,
        /// <summary>No such device</summary>
        ENODEV = NoSuchDevice,
        /// <summary>No such file or directory</summary>
        ENOENT = NoSuchFile,
        /// <summary>Link is severed</summary>
        ENOLINK = SeveredLink,
        /// <summary>Not implemented</summary>
        ENOSYS = NotImplemented,
        /// <summary>Is not a directory (e.g.: trying to ReadDir() a file)</summary>
        ENOTDIR = NotDirectory,
        /// <summary>Not supported</summary>
        ENOTSUP = NotSupported
    }
}