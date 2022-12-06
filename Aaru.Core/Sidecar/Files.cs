// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Files.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates sidecar information of files contained in supported read-only
//     filesystems.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Schemas;

namespace Aaru.Core
{
    public sealed partial class Sidecar
    {
        FilesystemContentsType Files(IReadOnlyFilesystem filesystem)
        {
            var contents = new FilesystemContentsType();

            Errno ret = filesystem.ReadDir("/", out List<string> dirents);

            if(ret != Errno.NoError)
                return null;

            List<DirectoryType>    directories = new List<DirectoryType>();
            List<ContentsFileType> files       = new List<ContentsFileType>();

            foreach(string dirent in dirents)
            {
                ret = filesystem.Stat(dirent, out FileEntryInfo stat);

                if(ret != Errno.NoError)
                {
                    AaruConsole.DebugWriteLine("Create-Sidecar command", "Cannot stat {0}", dirent);

                    continue;
                }

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    directories.Add(SidecarDirectory(filesystem, "", dirent, stat));

                    continue;
                }

                files.Add(SidecarFile(filesystem, "", dirent, stat));
            }

            if(files.Count > 0)
                contents.File = files.OrderBy(f => f.name).ToArray();

            if(directories.Count > 0)
                contents.Directory = directories.OrderBy(d => d.name).ToArray();

            return contents;
        }

        DirectoryType SidecarDirectory(IReadOnlyFilesystem filesystem, string path, string filename, FileEntryInfo stat)
        {
            var directory = new DirectoryType();

            if(stat.AccessTimeUtc.HasValue)
            {
                directory.accessTime          = stat.AccessTimeUtc.Value;
                directory.accessTimeSpecified = true;
            }

            directory.attributes = (ulong)stat.Attributes;

            if(stat.BackupTimeUtc.HasValue)
            {
                directory.backupTime          = stat.BackupTimeUtc.Value;
                directory.backupTimeSpecified = true;
            }

            if(stat.CreationTimeUtc.HasValue)
            {
                directory.creationTime          = stat.CreationTimeUtc.Value;
                directory.creationTimeSpecified = true;
            }

            if(stat.DeviceNo.HasValue)
            {
                directory.deviceNumber          = stat.DeviceNo.Value;
                directory.deviceNumberSpecified = true;
            }

            directory.inode = stat.Inode;

            if(stat.LastWriteTimeUtc.HasValue)
            {
                directory.lastWriteTime          = stat.LastWriteTimeUtc.Value;
                directory.lastWriteTimeSpecified = true;
            }

            directory.links = stat.Links;
            directory.name  = filename;

            if(stat.GID.HasValue)
            {
                directory.posixGroupId          = stat.GID.Value;
                directory.posixGroupIdSpecified = true;
            }

            if(stat.Mode.HasValue)
            {
                directory.posixMode          = stat.Mode.Value;
                directory.posixModeSpecified = true;
            }

            if(stat.UID.HasValue)
            {
                directory.posixUserId          = stat.UID.Value;
                directory.posixUserIdSpecified = true;
            }

            if(stat.StatusChangeTimeUtc.HasValue)
            {
                directory.statusChangeTime          = stat.StatusChangeTimeUtc.Value;
                directory.statusChangeTimeSpecified = true;
            }

            Errno ret = filesystem.ReadDir(path + "/" + filename, out List<string> dirents);

            if(ret != Errno.NoError)
                return null;

            List<DirectoryType>    directories = new List<DirectoryType>();
            List<ContentsFileType> files       = new List<ContentsFileType>();

            foreach(string dirent in dirents)
            {
                ret = filesystem.Stat(path + "/" + filename + "/" + dirent, out FileEntryInfo entryStat);

                if(ret != Errno.NoError)
                {
                    AaruConsole.DebugWriteLine("Create-Sidecar command", "Cannot stat {0}", dirent);

                    continue;
                }

                if(entryStat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    directories.Add(SidecarDirectory(filesystem, path + "/" + filename, dirent, entryStat));

                    continue;
                }

                files.Add(SidecarFile(filesystem, path + "/" + filename, dirent, entryStat));
            }

            if(files.Count > 0)
                directory.File = files.OrderBy(f => f.name).ToArray();

            if(directories.Count > 0)
                directory.Directory = directories.OrderBy(d => d.name).ToArray();

            return directory;
        }

        ContentsFileType SidecarFile(IReadOnlyFilesystem filesystem, string path, string filename, FileEntryInfo stat)
        {
            var file          = new ContentsFileType();
            var fileChkWorker = new Checksum();

            if(stat.AccessTimeUtc.HasValue)
            {
                file.accessTime          = stat.AccessTimeUtc.Value;
                file.accessTimeSpecified = true;
            }

            file.attributes = (ulong)stat.Attributes;

            if(stat.BackupTimeUtc.HasValue)
            {
                file.backupTime          = stat.BackupTimeUtc.Value;
                file.backupTimeSpecified = true;
            }

            if(stat.CreationTimeUtc.HasValue)
            {
                file.creationTime          = stat.CreationTimeUtc.Value;
                file.creationTimeSpecified = true;
            }

            if(stat.DeviceNo.HasValue)
            {
                file.deviceNumber          = stat.DeviceNo.Value;
                file.deviceNumberSpecified = true;
            }

            file.inode = stat.Inode;

            if(stat.LastWriteTimeUtc.HasValue)
            {
                file.lastWriteTime          = stat.LastWriteTimeUtc.Value;
                file.lastWriteTimeSpecified = true;
            }

            file.length = (ulong)stat.Length;
            file.links  = stat.Links;
            file.name   = filename;

            if(stat.GID.HasValue)
            {
                file.posixGroupId          = stat.GID.Value;
                file.posixGroupIdSpecified = true;
            }

            if(stat.Mode.HasValue)
            {
                file.posixMode          = stat.Mode.Value;
                file.posixModeSpecified = true;
            }

            if(stat.UID.HasValue)
            {
                file.posixUserId          = stat.UID.Value;
                file.posixUserIdSpecified = true;
            }

            if(stat.StatusChangeTimeUtc.HasValue)
            {
                file.statusChangeTime          = stat.StatusChangeTimeUtc.Value;
                file.statusChangeTimeSpecified = true;
            }

            byte[] data = Array.Empty<byte>();

            if(stat.Length > 0)
            {
                long position = 0;
                UpdateStatus($"Hashing file {path}/{filename}...");
                InitProgress2();

                while(position < stat.Length - 1048576)
                {
                    if(_aborted)
                        return file;

                    data = new byte[1048576];
                    filesystem.Read(path + "/" + filename, position, 1048576, ref data);

                    UpdateProgress2("Hashing file byte {0} of {1}", position, stat.Length);

                    fileChkWorker.Update(data);

                    position += 1048576;
                }

                data = new byte[stat.Length - position];
                filesystem.Read(path + "/" + filename, position, stat.Length - position, ref data);

                UpdateProgress2("Hashing file byte {0} of {1}", position, stat.Length);

                fileChkWorker.Update(data);

                EndProgress();

                file.Checksums = fileChkWorker.End().ToArray();
            }
            else
                file.Checksums = _emptyChecksums;

            Errno ret = filesystem.ListXAttr(path + "/" + filename, out List<string> xattrs);

            if(ret != Errno.NoError)
                return file;

            List<ExtendedAttributeType> xattrTypes = new List<ExtendedAttributeType>();

            foreach(string xattr in xattrs)
            {
                ret = filesystem.GetXattr(path + "/" + filename, xattr, ref data);

                if(ret != Errno.NoError)
                    continue;

                var xattrChkWorker = new Checksum();
                xattrChkWorker.Update(data);

                xattrTypes.Add(new ExtendedAttributeType
                {
                    Checksums = xattrChkWorker.End().ToArray(),
                    length    = (ulong)data.Length,
                    name      = xattr
                });
            }

            if(xattrTypes.Count > 0)
                file.ExtendedAttributes = xattrTypes.OrderBy(x => x.name).ToArray();

            return file;
        }
    }
}