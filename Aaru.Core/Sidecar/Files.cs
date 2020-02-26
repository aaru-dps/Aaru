using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.Core
{
    public partial class Sidecar
    {
        FilesystemContentsType Files(IReadOnlyFilesystem filesystem)
        {
            FilesystemContentsType contents = new FilesystemContentsType();

            Errno ret = filesystem.ReadDir("/", out List<string> dirents);

            if(ret != Errno.NoError) return null;

            List<DirectoryType>    directories = new List<DirectoryType>();
            List<ContentsFileType> files       = new List<ContentsFileType>();

            foreach(string dirent in dirents)
            {
                ret = filesystem.Stat(dirent, out FileEntryInfo stat);

                if(ret != Errno.NoError)
                {
                    DicConsole.DebugWriteLine("Create-Sidecar command", "Cannot stat {0}", dirent);
                    continue;
                }

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    directories.Add(SidecarDirectory(filesystem, "", dirent, stat));
                    continue;
                }

                files.Add(SidecarFile(filesystem, "", dirent, stat));
            }

            if(files.Count       > 0) contents.File      = files.OrderBy(f => f.name).ToArray();
            if(directories.Count > 0) contents.Directory = directories.OrderBy(d => d.name).ToArray();

            return contents;
        }

        DirectoryType SidecarDirectory(IReadOnlyFilesystem filesystem, string path, string filename, FileEntryInfo stat)
        {
            DirectoryType directory = new DirectoryType();

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

            if(ret != Errno.NoError) return null;

            List<DirectoryType>    directories = new List<DirectoryType>();
            List<ContentsFileType> files       = new List<ContentsFileType>();

            foreach(string dirent in dirents)
            {
                ret = filesystem.Stat(path + "/" + filename + "/" + dirent, out FileEntryInfo entryStat);

                if(ret != Errno.NoError)
                {
                    DicConsole.DebugWriteLine("Create-Sidecar command", "Cannot stat {0}", dirent);
                    continue;
                }

                if(entryStat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    directories.Add(SidecarDirectory(filesystem, path + "/" + filename, dirent, entryStat));
                    continue;
                }

                files.Add(SidecarFile(filesystem, path + "/" + filename, dirent, entryStat));
            }

            if(files.Count       > 0) directory.File      = files.OrderBy(f => f.name).ToArray();
            if(directories.Count > 0) directory.Directory = directories.OrderBy(d => d.name).ToArray();

            return directory;
        }

        ContentsFileType SidecarFile(IReadOnlyFilesystem filesystem, string path, string filename, FileEntryInfo stat)
        {
            ContentsFileType file          = new ContentsFileType();
            Checksum         fileChkWorker = new Checksum();

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

            byte[] data = new byte[0];
            if(stat.Length > 0)
            {
                long position = 0;
                UpdateStatus($"Hashing file {path}/{filename}...");
                InitProgress2();
                while(position < stat.Length - 1048576)
                {
                    if(aborted) return file;

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
            else file.Checksums = emptyChecksums;

            Errno ret = filesystem.ListXAttr(path + "/" + filename, out List<string> xattrs);

            if(ret != Errno.NoError) return file;

            List<ExtendedAttributeType> xattrTypes = new List<ExtendedAttributeType>();

            foreach(string xattr in xattrs)
            {
                ret = filesystem.GetXattr(path + "/" + filename, xattr, ref data);

                if(ret != Errno.NoError) continue;

                Checksum xattrChkWorker = new Checksum();
                xattrChkWorker.Update(data);

                xattrTypes.Add(new ExtendedAttributeType
                {
                    Checksums = xattrChkWorker.End().ToArray(),
                    length    = (ulong)data.Length,
                    name      = xattr
                });
            }

            if(xattrTypes.Count > 0) file.ExtendedAttributes = xattrTypes.OrderBy(x => x.name).ToArray();

            return file;
        }
    }
}