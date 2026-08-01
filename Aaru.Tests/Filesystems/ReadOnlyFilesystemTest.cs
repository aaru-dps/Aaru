using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using Directory = System.IO.Directory;
using File = System.IO.File;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystem = Aaru.CommonTypes.AaruMetadata.FileSystem;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Tests.Filesystems;

public abstract class ReadOnlyFilesystemTest : FilesystemTest
{
    const string DEEP_ENV  = "AARU_TESTS_DEEP";
    const string BUILD_ENV = "AARU_TESTS_BUILD";

    internal static readonly JsonSerializerOptions ContentsSerializerOptions = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        },
        MaxDepth                    = 1536, // More than this an we get a StackOverflowException
        WriteIndented               = false,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    protected ReadOnlyFilesystemTest() {}

    protected ReadOnlyFilesystemTest(string fileSystemType) : base(fileSystemType) {}

    [Test]
    public void Contents()
    {
        bool deep = Environment.GetEnvironmentVariable(DEEP_ENV) == "1";

        using(new AssertionScope())
        {
            foreach(FileSystemTest test in AllTests)
            {
                string testFile = Path.Combine(DataFolder, test.TestFile);

                bool exists = File.Exists(testFile);
                exists.Should().BeTrue(Localization._0_not_found, testFile);

                if(!exists) continue;

                IReadOnlyFilesystem fs = OpenFilesystem(test, testFile, true, out IMediaImage image, out _);

                if(fs is null)
                {
                    DisposeImage(image);

                    continue;
                }

                try
                {
                    TestContents(test, testFile, fs, deep);
                }
                finally
                {
                    // Dispose deterministically so image finalizers never run native close code
                    // concurrently with other tests' native reads
                    fs.Unmount();
                    DisposeImage(image);
                }
            }
        }
    }

    static void TestContents(FileSystemTest test, string testFile, IReadOnlyFilesystem fs, bool deep)
    {
        // Fast path: one traversal, one digest comparison, instead of walking multi-megabyte
        // JSON expectations. Falls back to the per-entry compare when the digest mismatches so
        // the failure message pinpoints the offending entry.
        var  verifyFileContents = true;
        bool digestChecked      = false;

        string digestPath = $"{testFile}.contents.digest.json";

        if(!deep && File.Exists(digestPath))
        {
            var expected =
                JsonSerializer.Deserialize<ContentsDigestFile>(File.ReadAllText(digestPath), InfoSerializerOptions);

            if(expected?.Version == ContentsDigest.Version)
            {
                ContentsDigestResult actual = ContentsDigest.Compute(BuildDirectory(fs, "/", 0));

                if(actual.Digest == expected.Digest && actual.TimestampDigest == expected.TimestampDigest) return;

                digestChecked = true;

                // Contents proven identical; only timestamps differ (e.g. a timezone shift the
                // per-entry compare fudges by ±1 hour), so skip re-reading file data below.
                verifyFileContents = actual.Digest != expected.Digest;

                // Even if the per-entry walk below finds nothing, a stale primary digest must not
                // pass silently — regenerate it with the Build target
                if(verifyFileContents)
                {
                    actual.Digest.Should()
                          .Be(expected.Digest,
                              "contents digest for {0} mismatched; if the per-entry differences below are intended, regenerate the sidecar files with {1}=1",
                              testFile,
                              BUILD_ENV);
                }
            }
        }

        test.Contents ??= LoadExpectedContents(test, testFile);

        if(test.Contents is null)
        {
            digestChecked.Should()
                         .BeFalse("contents digest mismatched for {0} and no contents JSON exists to diagnose it",
                                  testFile);

            return;
        }

        var currentDepth = 0;

        TestDirectory(fs,
                      "/",
                      test.Contents,
                      testFile,
                      true,
                      out List<NextLevel> currentLevel,
                      currentDepth,
                      verifyFileContents);

        while(currentLevel.Count > 0)
        {
            currentDepth++;
            List<NextLevel> nextLevels = [];

            foreach(NextLevel subLevel in currentLevel)
            {
                TestDirectory(fs,
                              subLevel.Path,
                              subLevel.Children,
                              testFile,
                              true,
                              out List<NextLevel> nextLevel,
                              currentDepth,
                              verifyFileContents);

                nextLevels.AddRange(nextLevel);
            }

            currentLevel = nextLevels;
        }
    }

    /// <summary>
    ///     Regenerates the expectation artifacts beside each image: {image}.contents.json.gz (per-file tree),
    ///     {image}.contents.digest.json (fast-path aggregate digest) and, for images without an in-code test
    ///     declaration, {image}.info.json (volume metadata). Not a test: run it manually per fixture with
    ///     AARU_TESTS_BUILD=1, e.g. AARU_TESTS_BUILD=1 dotnet test --filter FullyQualifiedName~Filesystems.FAT12
    /// </summary>
    [Test]
    public void Build()
    {
        if(Environment.GetEnvironmentVariable(BUILD_ENV) != "1")
            Assert.Ignore($"Not a test. Set {BUILD_ENV}=1 to regenerate expectation files.");

        List<string> failures = [];

        foreach(FileSystemTest test in EnumerateBuildTargets())
        {
            string testFile = Path.Combine(DataFolder, test.TestFile);

            if(!File.Exists(testFile)) continue;

            IReadOnlyFilesystem fs =
                OpenFilesystem(test, testFile, false, out IMediaImage image, out Partition partition);

            if(fs is null)
            {
                (image as IDisposable)?.Dispose();

                continue;
            }

            try
            {
                Dictionary<string, FileData> contents = BuildDirectory(fs, "/", 0);

                using(var sw = new GZipStream(new FileStream($"{testFile}.contents.json.gz", FileMode.Create),
                                              CompressionLevel.SmallestSize))
                    JsonSerializer.Serialize(sw, contents, ContentsSerializerOptions);

                File.WriteAllText($"{testFile}.contents.digest.json",
                                  JsonSerializer.Serialize(ContentsDigest.Compute(contents).ToFile(),
                                                           InfoSerializerOptions));

                // Only images declared via sidecar (or brand new ones) get their metadata refreshed on disk;
                // in-code declarations stay authoritative in the fixture source
                if(test.FromInfoJson) WriteInfoFile(test, testFile, image, partition);
            }
            catch(Exception ex)
            {
                // Keep building the remaining images and report every failing image at the end
                failures.Add($"{testFile}:{Environment.NewLine}{ex}");
            }
            finally
            {
                // Dispose deterministically so image finalizers never run native close code
                // concurrently with other tests' native reads
                fs.Unmount();
                (image as IDisposable)?.Dispose();
            }
        }

        if(failures.Count > 0)
        {
            Assert.Fail($"Building expectations failed for {failures.Count} image(s):{Environment.NewLine}{
                string.Join(Environment.NewLine + Environment.NewLine, failures)}");
        }
    }

    /// <summary>
    ///     All declared tests plus any image file in the data folder that has neither an in-code declaration nor
    ///     an .info.json sidecar yet — dropping a new image in the folder and running Build is enough to test it.
    /// </summary>
    IEnumerable<FileSystemTest> EnumerateBuildTargets()
    {
        var covered = new HashSet<string>(StringComparer.Ordinal);

        foreach(FileSystemTest test in AllTests)
        {
            covered.Add(test.TestFile);

            yield return test;
        }

        if(!Directory.Exists(DataFolder)) yield break;

        foreach(string name in Directory.GetFiles(DataFolder)
                                        .Select(Path.GetFileName)
                                        .Where(n => !n.EndsWith(".json",    StringComparison.Ordinal) &&
                                                    !n.EndsWith(".json.gz", StringComparison.Ordinal) &&
                                                    !covered.Contains(n)))
        {
            yield return new FileSystemTest
            {
                TestFile     = name,
                FromInfoJson = true
            };
        }
    }

    void WriteInfoFile(FileSystemTest test, string testFile, IMediaImage image, Partition partition)
    {
        if(Activator.CreateInstance(Plugin.GetType()) is not IFilesystem infoFs) return;

        infoFs.GetInformation(image, partition, test.Encoding, out _, out FileSystem metadata);

        test.MediaType     = image.Info.MediaType;
        test.Sectors       = image.Info.Sectors;
        test.SectorSize    = image.Info.SectorSize;
        test.ApplicationId = metadata.ApplicationIdentifier;
        test.Bootable      = metadata.Bootable;
        test.Clusters      = (long)metadata.Clusters;
        test.ClusterSize   = metadata.ClusterSize;
        test.SystemId      = metadata.SystemIdentifier;
        test.Type          = metadata.Type;
        test.VolumeName    = metadata.VolumeName;
        test.VolumeSerial  = metadata.VolumeSerial;

        File.WriteAllText($"{testFile}.info.json", JsonSerializer.Serialize(test, InfoSerializerOptions));
    }

    /// <summary>Detects, opens and mounts the filesystem under test; returns null (asserting if requested) on failure</summary>
    IReadOnlyFilesystem OpenFilesystem(FileSystemTest test, string testFile, bool asserting, out IMediaImage image,
                                       out Partition  partition)
    {
        var found = false;
        image     = null;
        partition = new Partition();

        IFilter inputFilter = PluginRegister.Singleton.GetFilter(testFile);

        if(asserting) inputFilter.Should().NotBeNull(Localization.Filter_0, testFile);

        if(inputFilter is null) return null;

        IBaseImage detected = ImageFormat.Detect(inputFilter);

        if(asserting) (detected is IMediaImage).Should().BeTrue(Localization.Image_format_0, testFile);

        if(detected is not IMediaImage mediaImage) return null;

        image = mediaImage;

        ErrorNumber opened = image.Open(inputFilter);

        if(asserting)
            opened.Should().Be(ErrorNumber.NoError, string.Format(Localization.Cannot_open_image_for_0, testFile));

        if(opened != ErrorNumber.NoError) return null;

        List<string> idPlugins;

        if(Partitions)
        {
            List<Partition> partitionsList = Core.Partitions.GetAll(image);

            if(asserting) partitionsList.Should().NotBeEmpty(Localization.No_partitions_found_for_0, testFile);

            // In reverse to skip boot partitions we're not interested in
            for(int index = partitionsList.Count - 1; index >= 0; index--)
            {
                Core.Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

                if(idPlugins.Count == 0) continue;

                if(!idPlugins.Contains(Plugin.Id.ToString())) continue;

                found     = true;
                partition = partitionsList[index];

                break;
            }
        }
        else
        {
            partition = new Partition
            {
                Name   = "Whole device",
                Length = image.Info.Sectors,
                Size   = image.Info.Sectors * image.Info.SectorSize
            };

            Core.Filesystems.Identify(image, out idPlugins, partition, true);

            if(asserting) idPlugins.Should().NotBeEmpty(Localization.No_filesystems_found_for_0, testFile);

            found = idPlugins.Contains(Plugin.Id.ToString());
        }

        if(asserting) found.Should().BeTrue(Localization.Filesystem_not_identified_for_0, testFile);

        if(!found) return null;

        object instance = Activator.CreateInstance(Plugin.GetType());

        if(asserting)
        {
            (instance is IReadOnlyFilesystem).Should()
                                             .BeTrue(Localization.Could_not_instantiate_filesystem_for_0, testFile);
        }

        if(instance is not IReadOnlyFilesystem fs) return null;

        test.Encoding ??= ResolveEncoding(test.EncodingName) ?? System.Text.Encoding.ASCII;

        ErrorNumber ret = fs.Mount(image, partition, test.Encoding, null, test.Namespace);

        if(asserting) ret.Should().Be(ErrorNumber.NoError, string.Format(Localization.Unmountable_0, testFile));

        return ret == ErrorNumber.NoError ? fs : null;
    }

    /// <summary>Loads the per-file expectations, preferring the compressed sidecar over the legacy plain JSON</summary>
    internal static Dictionary<string, FileData> LoadExpectedContents(FileSystemTest test, string testFile) =>
        LoadContentsFile(test.ContentsJson, $"{testFile}.contents.json");

    internal static Dictionary<string, FileData> LoadContentsFile(string inlineJson, string jsonPath)
    {
        if(inlineJson != null)
            return JsonSerializer.Deserialize<Dictionary<string, FileData>>(inlineJson, ContentsSerializerOptions);

        if(File.Exists($"{jsonPath}.gz"))
        {
            using var sr = new GZipStream(File.OpenRead($"{jsonPath}.gz"), CompressionMode.Decompress);

            return JsonSerializer.Deserialize<Dictionary<string, FileData>>(sr, ContentsSerializerOptions);
        }

        if(!File.Exists(jsonPath)) return null;

        using FileStream stream = File.OpenRead(jsonPath);

        return JsonSerializer.Deserialize<Dictionary<string, FileData>>(stream, ContentsSerializerOptions);
    }

    internal static Dictionary<string, FileData> BuildDirectory(IReadOnlyFilesystem fs, string path, int currentDepth)
    {
        currentDepth++;

        if(path == "/") path = "";

        Dictionary<string, FileData> children = new();
        ErrorNumber                  ret      = fs.OpenDir(path, out IDirNode node);

        if(ret != ErrorNumber.NoError) return children;

        while(fs.ReadDir(node, out string child) == ErrorNumber.NoError && child is not null)
        {
            var         childPath = $"{path}/{child}";
            ErrorNumber statError = fs.Stat(childPath, out FileEntryInfo stat);

            if(stat is null)
            {
                // Filesystem plugin bug: name the offending entry so it can be diagnosed
                throw new
                    InvalidOperationException($"Stat of \"{childPath}\" returned {statError} without information");
            }

            var data = new FileData
            {
                Info = stat
            };

            if(stat.Attributes.HasFlag(FileAttributes.Directory))
            {
                // Cannot serialize to JSON too many depth levels 🤷‍♀️
                if(currentDepth < 384) data.Children = BuildDirectory(fs, childPath, currentDepth);
            }
            else if(stat.Attributes.HasFlag(FileAttributes.Symlink))
            {
                if(fs.ReadLink(childPath, out string link) == ErrorNumber.NoError) data.LinkTarget = link;
            }
            else
                data.Md5 = BuildFile(fs, childPath, stat.Length);

            if(fs.ListXAttr(childPath, out List<string> xattrs) == ErrorNumber.NoError && xattrs.Count > 0)
                data.XattrsWithMd5 = BuildFileXattrs(fs, childPath);

            children[child] = data;
        }

        fs.CloseDir(node);

        return children;
    }

    const int READ_CHUNK = 1 << 20;

    static string BuildFile(IReadOnlyFilesystem fs, string path, long length)
    {
        // Streamed in chunks: files can be huge and a whole-file byte[] doubles as memory pressure.
        // Any unreadable remainder is hashed as zeroes, matching the old whole-buffer behavior.
        var  md5       = new Md5Context();
        var  buffer    = new byte[Math.Min(length, READ_CHUNK)];
        long remaining = length;

        ErrorNumber error = fs.OpenFile(path, out IFileNode fileNode);

        if(error == ErrorNumber.NoError)
        {
            try
            {
                while(remaining > 0)
                {
                    long want = Math.Min(remaining, buffer.Length);

                    if(fs.ReadFile(fileNode, want, buffer, out long read) != ErrorNumber.NoError || read <= 0) break;

                    md5.Update(buffer, (uint)read);
                    remaining -= read;
                }
            }
            catch(Exception ex)
            {
                // Filesystem plugin bug: name the offending file so it can be diagnosed
                throw new InvalidOperationException($"Reading \"{path}\" ({length} bytes) threw", ex);
            }

            fs.CloseFile(fileNode);
        }

        if(remaining <= 0) return md5.End();

        Array.Clear(buffer);

        while(remaining > 0)
        {
            long want = Math.Min(remaining, buffer.Length);
            md5.Update(buffer, (uint)want);
            remaining -= want;
        }

        return md5.End();
    }

    static Dictionary<string, string> BuildFileXattrs(IReadOnlyFilesystem fs, string path)
    {
        fs.ListXAttr(path, out List<string> contents);

        if(contents.Count == 0) return null;

        Dictionary<string, string> xattrs = new();

        foreach(string xattr in contents)
        {
            byte[]      buffer = [];
            ErrorNumber ret    = fs.GetXattr(path, xattr, ref buffer);

            string data = ret != ErrorNumber.NoError && ret != ErrorNumber.OutOfRange
                              ? Md5Context.Data([],     out _)
                              : Md5Context.Data(buffer, out _);

            xattrs[xattr] = data;
        }

        return xattrs;
    }

    /// <summary>Timestamps coming from the driver may be shifted by exactly one hour (timezone quirks); tolerate it</summary>
    static void FudgeTimestamps(FileEntryInfo stat, FileEntryInfo expected)
    {
        if(expected is null) return;

        if((stat.AccessTime - expected.AccessTime)?.Hours is 1 or -1) stat.AccessTime = expected.AccessTime;

        if((stat.AccessTimeUtc - expected.AccessTimeUtc)?.Hours is 1 or -1) stat.AccessTimeUtc = expected.AccessTimeUtc;

        if((stat.BackupTime - expected.BackupTime)?.Hours is 1 or -1) stat.BackupTime = expected.BackupTime;

        if((stat.BackupTimeUtc - expected.BackupTimeUtc)?.Hours is 1 or -1) stat.BackupTimeUtc = expected.BackupTimeUtc;

        if((stat.CreationTime - expected.CreationTime)?.Hours is 1 or -1) stat.CreationTime = expected.CreationTime;

        if((stat.CreationTimeUtc - expected.CreationTimeUtc)?.Hours is 1 or -1)
            stat.CreationTimeUtc = expected.CreationTimeUtc;

        if((stat.LastWriteTime - expected.LastWriteTime)?.Hours is 1 or -1) stat.LastWriteTime = expected.LastWriteTime;

        if((stat.LastWriteTimeUtc - expected.LastWriteTimeUtc)?.Hours is 1 or -1)
            stat.LastWriteTimeUtc = expected.LastWriteTimeUtc;

        if((stat.StatusChangeTime - expected.StatusChangeTime)?.Hours is 1 or -1)
            stat.StatusChangeTime = expected.StatusChangeTime;

        if((stat.StatusChangeTimeUtc - expected.StatusChangeTimeUtc)?.Hours is 1 or -1)
            stat.StatusChangeTimeUtc = expected.StatusChangeTimeUtc;
    }

    internal static void TestDirectory(IReadOnlyFilesystem fs, string path, Dictionary<string, FileData> children,
                                       string              testFile, bool testXattr, out List<NextLevel> nextLevels,
                                       int                 currentDepth, bool verifyFileContents = true)
    {
        currentDepth++;
        nextLevels = [];
        ErrorNumber ret = fs.OpenDir(path, out IDirNode node);

        // Directory is not readable, probably filled the volume, just ignore it
        if(ret == ErrorNumber.InvalidArgument) return;

        ret.Should()
           .Be(ErrorNumber.NoError,
               string.Format(Localization.Unexpected_error_0_when_reading_directory_1_of_2, ret, path, testFile));

        if(ret != ErrorNumber.NoError) return;

        // HashSet: Contains/Remove are called once per child, which is quadratic on a List for
        // directories with millions of entries
        HashSet<string> contents = [];

        while(fs.ReadDir(node, out string filename) == ErrorNumber.NoError && filename is not null)
            contents.Add(filename);

        fs.CloseDir(node);

        if(children.Count == 0 && contents.Count == 0) return;

        if(path == "/") path = "";

        List<string> expectedNotFound = [];

        foreach(KeyValuePair<string, FileData> child in children)
        {
            var childPath = $"{path}/{child.Key}";
            ret = fs.Stat(childPath, out FileEntryInfo stat);

            if(ret == ErrorNumber.NoSuchFile || ret == ErrorNumber.NoError && !contents.Contains(child.Key))
            {
                expectedNotFound.Add(child.Key);

                continue;
            }

            contents.Remove(child.Key);

            ret.Should()
               .Be(ErrorNumber.NoError,
                   string.Format(Localization.Unexpected_error_0_retrieving_stats_for_1_in_2,
                                 ret,
                                 childPath,
                                 testFile));

            FudgeTimestamps(stat, child.Value.Info);

            stat.Should()
                .BeEquivalentTo(child.Value.Info,
                                string.Format(Localization.Wrong_info_for_0_in_1, childPath, testFile));

            if(child.Value.Info.Attributes.HasFlag(FileAttributes.Directory))
            {
                ret = fs.OpenFile(childPath, out _);

                ret.Should()
                   .Be(ErrorNumber.IsDirectory,
                       string.Format(Localization.Got_wrong_data_for_directory_0_in_1, childPath, testFile));

                // Cannot serialize to JSON too many depth levels 🤷‍♀️
                if(currentDepth < 384)
                {
                    child.Value.Children.Should()
                         .NotBeNull(Localization.Contents_for_0_in_1_must_be_defined_in_unit_test_declaration,
                                    childPath,
                                    testFile);

                    if(child.Value.Children != null) nextLevels.Add(new NextLevel(childPath, child.Value.Children));
                }
            }
            else if(child.Value.Info.Attributes.HasFlag(FileAttributes.Symlink))
            {
                ret = fs.ReadLink(childPath, out string link);

                ret.Should()
                   .Be(ErrorNumber.NoError,
                       string.Format(Localization.Got_wrong_data_for_symbolic_link_0_in_1, childPath, testFile));

                link.Should()
                    .Be(child.Value.LinkTarget,
                        string.Format(Localization.Invalid_target_for_symbolic_link_0_in_1, childPath, testFile));
            }
            else if(verifyFileContents) TestFile(fs, childPath, child.Value.Md5, child.Value.Info.Length, testFile);

            if(!testXattr) continue;

            ret = fs.ListXAttr(childPath, out List<string> xattrs);

            if(ret == ErrorNumber.NotSupported)
            {
                child.Value.XattrsWithMd5.Should()
                     .BeNull(Localization.Defined_extended_attributes_for_0_in_1_are_not_supported_by_filesystem,
                             childPath,
                             testFile);

                continue;
            }

            ret.Should()
               .Be(ErrorNumber.NoError,
                   string.Format(Localization.Unexpected_error_0_when_listing_extended_attributes_for_1_in_2,
                                 ret,
                                 childPath,
                                 testFile));

            if(xattrs.Count > 0)
            {
                child.Value.XattrsWithMd5.Should()
                     .NotBeNull(Localization.Extended_attributes_for_0_in_1_must_be_defined_in_unit_test_declaration,
                                childPath,
                                testFile);
            }

            if(xattrs.Count > 0 || child.Value.XattrsWithMd5?.Count > 0)
                TestFileXattrs(fs, childPath, child.Value.XattrsWithMd5, testFile);
        }

        expectedNotFound.Should()
                        .BeEmpty(Localization.Could_not_find_the_children_of_0_in_1_2,
                                 path,
                                 testFile,
                                 string.Join(" ", expectedNotFound));

        contents.Should()
                .BeEmpty(Localization.Found_the_following_unexpected_children_of_0_in_1_2,
                         path,
                         testFile,
                         string.Join(" ", contents));
    }

    static void TestFile(IReadOnlyFilesystem fs, string path, string md5, long length, string testFile)
    {
        var         md5Ctx = new Md5Context();
        var         buffer = new byte[Math.Min(length, READ_CHUNK)];
        ErrorNumber ret    = fs.OpenFile(path, out IFileNode fileNode);

        ret.Should()
           .Be(ErrorNumber.NoError,
               string.Format(Localization.Unexpected_error_0_when_reading_1_in_2, ret, path, testFile));

        if(ret != ErrorNumber.NoError) return;

        long totalRead = 0;

        while(totalRead < length)
        {
            long want = Math.Min(length - totalRead, buffer.Length);

            ret = fs.ReadFile(fileNode, want, buffer, out long read);

            ret.Should()
               .Be(ErrorNumber.NoError,
                   string.Format(Localization.Unexpected_error_0_when_reading_1_in_2, ret, path, testFile));

            if(ret != ErrorNumber.NoError || read <= 0) break;

            md5Ctx.Update(buffer, (uint)read);
            totalRead += read;
        }

        totalRead.Should()
                 .Be(length,
                     string.Format(Localization.Got_less_bytes_0_than_expected_1_when_reading_2_in_3,
                                   totalRead,
                                   length,
                                   path,
                                   testFile));

        fs.CloseFile(fileNode);

        string data = md5Ctx.End();

        data.Should()
            .Be(md5, string.Format(Localization.Got_MD5_0_for_1_in_2_but_expected_3, data, path, testFile, md5));
    }

    static void TestFileXattrs(IReadOnlyFilesystem fs, string path, Dictionary<string, string> xattrs, string testFile)
    {
        // Nothing to test
        if(xattrs is null) return;

        fs.ListXAttr(path, out List<string> contents);

        if(xattrs.Count == 0 && contents.Count == 0) return;

        List<string> expectedNotFound = [];

        foreach(KeyValuePair<string, string> xattr in xattrs)
        {
            byte[]      buffer = [];
            ErrorNumber ret    = fs.GetXattr(path, xattr.Key, ref buffer);

            if(ret == ErrorNumber.NoSuchExtendedAttribute || !contents.Contains(xattr.Key))
            {
                expectedNotFound.Add(xattr.Key);

                continue;
            }

            contents.Remove(xattr.Key);

            // Partially read extended attribute... dunno why it happens with some Toast images
            if(ret != ErrorNumber.OutOfRange)
            {
                ret.Should()
                   .Be(ErrorNumber.NoError,
                       string.Format(Localization.Unexpected_error_0_retrieving_extended_attributes_for_1_in_2,
                                     ret,
                                     path,
                                     testFile));
            }

            string data = Md5Context.Data(buffer, out _);

            data.Should()
                .Be(xattr.Value,
                    string.Format(Localization.Got_MD5_0_for_1_of_2_in_3_but_expected_4,
                                  data,
                                  xattr.Key,
                                  path,
                                  testFile,
                                  xattr.Value));
        }

        expectedNotFound.Should()
                        .BeEmpty(Localization.Could_not_find_the_following_extended_attributes_of_0_in_1_2,
                                 path,
                                 testFile,
                                 string.Join(" ", expectedNotFound));

        contents.Should()
                .BeEmpty(Localization.Found_the_following_unexpected_extended_attributes_of_0_in_1_2,
                         path,
                         testFile,
                         string.Join(" ", contents));
    }

#region Nested type: NextLevel

    internal sealed record NextLevel(string Path, Dictionary<string, FileData> Children);

#endregion
}