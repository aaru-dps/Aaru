using System;

namespace Aaru.Archives;

public sealed partial class Rar
{
#region Nested type: Entry

    struct Entry
    {
        public string            Filename;
        public long              CompressedSize;
        public long              UncompressedSize;
        public long              DataOffset;
        public DateTime          LastWriteTime;
        public DateTime          CreationTime;
        public DateTime          LastAccessTime;
        public uint              Crc32;
        public uint              Attributes;
        public byte              UnpVersion;
        public CompressionMethod Method;
        public HostOs            Os;
        public bool              IsRar5;
        public Rar5Os            Os5;
        public bool              IsDirectory;
        public bool              IsSolid;
        public bool              IsEncrypted;
        public bool              IsSplit;
        public nuint             WindowSize;
        public string            Comment;
        public bool              HasCreationTime;
        public bool              HasLastAccessTime;
    }

#endregion
}