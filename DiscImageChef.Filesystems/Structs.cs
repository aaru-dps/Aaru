// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
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
namespace DiscImageChef.Filesystems
{
    /// <summary>
    /// File attributes.
    /// </summary>
    [Flags]
    public enum FileAttributes
    {
        /// <summary>File is an alias (Mac OS)</summary>
        Alias,
        /// <summary>Indicates that the file can only be writable appended</summary>
        AppendOnly,
        /// <summary>File is candidate for archival/backup</summary>
        Archive,
        /// <summary>File is a block device</summary>
        BlockDevice,
        /// <summary>File is stored on filesystem block units instead of device sectors</summary>
        BlockUnits,
        /// <summary>Directory is a bundle or file contains a BNDL resource</summary>
        Bundle,
        /// <summary>File is a char device</summary>
        CharDevice,
        /// <summary>File is compressed</summary>
        Compressed,
        /// <summary>File is compressed and should not be uncompressed on read</summary>
        CompressedRaw,
        /// <summary>File has compression errors</summary>
        CompressionError,
        /// <summary>Compressed file is dirty</summary>
        CompressionDirty,
        /// <summary>File is a device</summary>
        Device,
        /// <summary>File is a directory</summary>
        Directory,
        /// <summary>File is encrypted</summary>
        Encrypted,
        /// <summary>File is stored on disk using extents</summary>
        Extents,
        /// <summary>File is a FIFO</summary>
        FIFO,
        /// <summary>File is a normal file</summary>
        File,
        /// <summary>File is a Mac OS file containing desktop databases that has already been added to the desktop database</summary>
        HasBeenInited,
        /// <summary>File contains an icon resource / EA</summary>
        HasCustomIcon,
        /// <summary>File is a Mac OS extension or control panel lacking INIT resources</summary>
        HasNoINITs,
        /// <summary>File is hidden/invisible</summary>
        Hidden,
        /// <summary>File cannot be written, deleted, modified or linked to</summary>
        Immutable,
        /// <summary>Directory is indexed using hashed trees</summary>
        IndexedDirectory,
        /// <summary>File contents are stored alongside its inode (or equivalent)</summary>
        Inline,
        /// <summary>File contains integrity checks</summary>
        IntegrityStream,
        /// <summary>File is on desktop</summary>
        IsOnDesk,
        /// <summary>File changes are written to filesystem journal before being written to file itself</summary>
        Journaled,
        /// <summary>Access time will not be modified</summary>
        NoAccessTime,
        /// <summary>File will not be subject to copy-on-write</summary>
        NoCopyOnWrite,
        /// <summary>File will not be backed up</summary>
        NoDump,
        /// <summary>File contents should not be scrubed</summary>
        NoScrub,
        /// <summary>File contents should not be indexed</summary>
        NotIndexed,
        /// <summary>File is offline</summary>
        Offline,
        /// <summary>File is password protected, but contents are not encrypted on disk</summary>
        Password,
        /// <summary>File is read-only</summary>
        ReadOnly,
        /// <summary>File is a reparse point</summary>
        ReparsePoint,
        /// <summary>When file is removed its content will be overwritten with zeroes</summary>
        Secured,
        /// <summary>File contents are sparse</summary>
        Sparse,
        /// <summary>File is a shadow (OS/2)</summary>
        Shadow,
        /// <summary>File is shared</summary>
        Shared,
        /// <summary>File is a stationery</summary>
        Stationery,
        /// <summary>File is a symbolic link</summary>
        Symlink,
        /// <summary>File writes are synchronously written to disk</summary>
        Sync,
        /// <summary>File belongs to the operating system</summary>
        System,
        /// <summary>If file end is a partial block its contend will be merged with other files</summary>
        TailMerged,
        /// <summary>File is temporary</summary>
        Temporary,
        /// <summary>Subdirectories inside of this directory are not related and should be allocated elsewhere</summary>
        TopDirectory,
        /// <summary>If file is deleted, contents should be stored, for a possible future undeletion</summary>
        Undeletable
    }

    /// <summary>
    /// Information about a file entry
    /// </summary>
    public struct FileEntryInfo
    {
        DateTime crtimeUtc;
        DateTime atimeUtc;
        DateTime ctimeUtc;
        DateTime btimeUtc;
        DateTime mtimeUtc;

        /// <summary>File attributes</summary>
        public FileAttributes Attributes;

        /// <summary>File creation date in UTC</summary>
        public DateTime CreationTimeUtc { get { return crtimeUtc; } set { crtimeUtc = value; } }
        /// <summary>File last access date in UTC</summary>
        public DateTime AccessTimeUtc { get { return atimeUtc; } set { atimeUtc = value; } }
        /// <summary>File attributes change date in UTC</summary>
        public DateTime StatusChangeTimeUtc { get { return ctimeUtc; } set { ctimeUtc = value; } }
        /// <summary>File last backup date in UTC</summary>
        public DateTime BackupTimeUtc { get { return btimeUtc; } set { btimeUtc = value; } }
        /// <summary>File last modification date in UTC</summary>
        public DateTime LastWriteTimeUtc { get { return mtimeUtc; } set { mtimeUtc = value; } }

        /// <summary>File creation date</summary>
        public DateTime CreationTime { get { return crtimeUtc.ToLocalTime(); } set { crtimeUtc = value.ToUniversalTime(); } }
        /// <summary>File last access date</summary>
        public DateTime AccessTime { get { return atimeUtc.ToLocalTime(); } set { atimeUtc = value.ToUniversalTime(); } }
        /// <summary>File attributes change date</summary>
        public DateTime StatusChangeTime { get { return ctimeUtc.ToLocalTime(); } set { ctimeUtc = value.ToUniversalTime(); } }
        /// <summary>File last backup date</summary>
        public DateTime BackupTime { get { return btimeUtc.ToLocalTime(); } set { btimeUtc = value.ToUniversalTime(); } }
        /// <summary>File last modification date</summary>
        public DateTime LastWriteTime { get { return mtimeUtc.ToLocalTime(); } set { mtimeUtc = value.ToUniversalTime(); } }

        /// <summary>Path of device that contains this file</summary>
        public string DevicePath;
        /// <summary>inode number for this file</summary>
        public ulong Inode;
        /// <summary>POSIX permissions/mode for this file</summary>
        public uint Mode;
        /// <summary>Number of hard links pointing to this file</summary>
        public ulong Links;
        /// <summary>POSIX owner ID</summary>
        public ulong UID;
        /// <summary>POSIX group ID</summary>
        public ulong GID;
        /// <summary>If file points to a device, device number</summary>
        public ulong DeviceNo;
        /// <summary>File length in bytes</summary>
        public long Length;
        /// <summary>File block size in bytes</summary>
        public long BlockSize;
        /// <summary>File length in blocks</summary>
        public long Blocks;
    }

    public struct FileSystemInfo
    {
        /// <summary>Filesystem type</summary>
        public string Type;
        /// <summary>ID of plugin for this file</summary>
        public Guid PluginId;
        /// <summary>Blocks for this filesystem</summary>
        public long Blocks;
        /// <summary>Blocks free on this filesystem</summary>
        public long FreeBlocks;
        /// <summary>Files on this filesystem</summary>
        public ulong Files;
        /// <summary>Free inodes on this filesystem</summary>
        public ulong FreeFiles;
        /// <summary>Maximum length of filenames on this filesystem</summary>
        public ushort FilenameLength;
        /// <summary>Filesystem ID</summary>
        public Guid Id;
    }
}