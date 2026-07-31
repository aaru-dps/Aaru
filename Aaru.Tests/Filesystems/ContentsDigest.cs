using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Aaru.CommonTypes.Structs;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Filesystems;

/// <summary>Result of computing the aggregate contents digest of one filesystem</summary>
public sealed class ContentsDigestResult
{
    public long   Bytes;
    public string Digest;
    public long   Entries;
    public string TimestampDigest;

    public ContentsDigestFile ToFile() => new()
    {
        Version         = ContentsDigest.Version,
        Digest          = Digest,
        TimestampDigest = TimestampDigest,
        Entries         = Entries,
        Bytes           = Bytes
    };
}

/// <summary>
///     Computes a single aggregate SHA-256 digest over a filesystem contents tree (as built by
///     <see cref="ReadOnlyFilesystemTest.BuildDirectory" />), so the fast test path can compare one hash instead of
///     walking multi-megabyte JSON expectations. Timestamps go into a separate digest because drivers may report them
///     shifted by timezone (the per-entry compare fudges ±1 hour, which an exact hash cannot).
/// </summary>
public static class ContentsDigest
{
    /// <summary>Bump when the canonical record format changes (e.g. a FileEntryInfo field is added)</summary>
    public const int Version = 1;

    public static ContentsDigestResult Compute(Dictionary<string, FileData> tree)
    {
        using var primary    = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var timestamps = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        var result = new ContentsDigestResult();

        Append(primary,    $"V{Version}\n");
        Append(timestamps, $"V{Version}\n");

        Walk(tree, "", primary, timestamps, result);

        result.Digest          = Convert.ToHexString(primary.GetHashAndReset()).ToLowerInvariant();
        result.TimestampDigest = Convert.ToHexString(timestamps.GetHashAndReset()).ToLowerInvariant();

        return result;
    }

    static void Walk(Dictionary<string, FileData> children,   string               path, IncrementalHash primary,
                     IncrementalHash              timestamps, ContentsDigestResult result)
    {
        foreach(string name in children.Keys.OrderBy(static n => n, StringComparer.Ordinal))
        {
            FileData child     = children[name];
            var      childPath = $"{path}/{name}";

            AppendEntry(primary, childPath, child);
            AppendTimestamps(timestamps, childPath, child.Info);

            result.Entries++;

            if(child.Info?.Attributes.HasFlag(FileAttributes.Directory) != true)
                result.Bytes += child.Info?.Length ?? 0;

            if(child.Children != null) Walk(child.Children, childPath, primary, timestamps, result);
        }
    }

    static void AppendEntry(IncrementalHash hash, string path, FileData data)
    {
        FileEntryInfo info = data.Info;

        string kind = info?.Attributes.HasFlag(FileAttributes.Directory) == true
                          ? "D"
                          : info?.Attributes.HasFlag(FileAttributes.Symlink) == true
                              ? "L"
                              : "F";

        // Every FileEntryInfo field except timestamps; adding a field here requires bumping Version
        var sb = new StringBuilder();
        sb.Append(path).Append('\0');
        sb.Append(kind).Append('\0');
        sb.Append((ulong?)info?.Attributes).Append('\0');
        sb.Append(info?.Length).Append('\0');
        sb.Append(info?.Links).Append('\0');
        sb.Append(info?.UID).Append('\0');
        sb.Append(info?.GID).Append('\0');
        sb.Append(info?.Inode).Append('\0');
        sb.Append(info?.Mode).Append('\0');
        sb.Append(info?.Blocks).Append('\0');
        sb.Append(info?.BlockSize).Append('\0');
        sb.Append(info?.DeviceNo).Append('\0');
        sb.Append(data.LinkTarget).Append('\0');
        sb.Append(data.Md5).Append('\0');

        if(data.XattrsWithMd5 != null)
        {
            foreach(string xattr in data.XattrsWithMd5.Keys.OrderBy(static n => n, StringComparer.Ordinal))
                sb.Append(xattr).Append('=').Append(data.XattrsWithMd5[xattr]).Append('\n');
        }

        sb.Append('\0').Append('\n');

        Append(hash, sb.ToString());
    }

    static void AppendTimestamps(IncrementalHash hash, string path, FileEntryInfo info)
    {
        var sb = new StringBuilder();
        sb.Append(path).Append('\0');
        sb.Append(info?.CreationTimeUtc?.Ticks).Append('\0');
        sb.Append(info?.AccessTimeUtc?.Ticks).Append('\0');
        sb.Append(info?.StatusChangeTimeUtc?.Ticks).Append('\0');
        sb.Append(info?.BackupTimeUtc?.Ticks).Append('\0');
        sb.Append(info?.LastWriteTimeUtc?.Ticks).Append('\0').Append('\n');

        Append(hash, sb.ToString());
    }

    static void Append(IncrementalHash hash, string text) => hash.AppendData(Encoding.UTF8.GetBytes(text));
}