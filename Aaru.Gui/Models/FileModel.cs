using System;
using Aaru.CommonTypes.Structs;

namespace Aaru.Gui.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Size => $"{Stat.Length}";
        public string CreationTime =>
            Stat.CreationTime == default(DateTime) ? "" : $"{Stat.CreationTime:G}";
        public string LastAccessTime => Stat.AccessTime == default(DateTime) ? "" : $"{Stat.AccessTime:G}";
        public string ChangedTime =>
            Stat.StatusChangeTime == default(DateTime) ? "" : $"{Stat.StatusChangeTime:G}";
        public string LastBackupTime => Stat.BackupTime == default(DateTime) ? "" : $"{Stat.BackupTime:G}";
        public string LastWriteTime =>
            Stat.LastWriteTime == default(DateTime) ? "" : $"{Stat.LastWriteTime:G}";
        public string        Attributes => $"{Stat.Attributes}";
        public string        Gid        => $"{Stat.GID}";
        public string        Uid        => $"{Stat.UID}";
        public string        Inode      => $"{Stat.Inode}";
        public string        Links      => $"{Stat.Links}";
        public string        Mode       => $"{Stat.Mode}";
        public FileEntryInfo Stat       { get; set; }
    }
}