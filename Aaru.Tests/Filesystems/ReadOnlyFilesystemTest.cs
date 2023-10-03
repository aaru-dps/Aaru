using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Filesystems;

public abstract class ReadOnlyFilesystemTest : FilesystemTest
{
    protected ReadOnlyFilesystemTest() {}

    protected ReadOnlyFilesystemTest(string fileSystemType) : base(fileSystemType) {}

    [Test]
    public void Contents()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(FileSystemTest test in Tests)
            {
                string testFile  = test.TestFile;
                var    found     = false;
                var    partition = new Partition();

                bool exists = File.Exists(testFile);
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter inputFilter = filtersList.GetFilter(testFile);

                Assert.IsNotNull(inputFilter, string.Format(Localization.Filter_0, testFile));

                var image = ImageFormat.Detect(inputFilter) as IMediaImage;

                Assert.IsNotNull(image, string.Format(Localization.Image_format_0, testFile));

                Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter),
                                string.Format(Localization.Cannot_open_image_for_0, testFile));

                List<string> idPlugins;

                if(Partitions)
                {
                    List<Partition> partitionsList = Core.Partitions.GetAll(image);

                    Assert.Greater(partitionsList.Count, 0,
                                   string.Format(Localization.No_partitions_found_for_0, testFile));

                    // In reverse to skip boot partitions we're not interested in
                    for(int index = partitionsList.Count - 1; index >= 0; index--)
                    {
                        Core.Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

                        if(idPlugins.Count == 0)
                            continue;

                        if(!idPlugins.Contains(Plugin.Id.ToString()))
                            continue;

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

                    Assert.Greater(idPlugins.Count, 0,
                                   string.Format(Localization.No_filesystems_found_for_0, testFile));

                    found = idPlugins.Contains(Plugin.Id.ToString());
                }

                Assert.True(found, string.Format(Localization.Filesystem_not_identified_for_0, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It is not the case, it changes
                if(!found)
                    continue;

                var fs = Activator.CreateInstance(Plugin.GetType()) as IReadOnlyFilesystem;

                Assert.NotNull(fs, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                test.Encoding ??= Encoding.ASCII;

                ErrorNumber ret = fs.Mount(image, partition, test.Encoding, null, test.Namespace);

                Assert.AreEqual(ErrorNumber.NoError, ret, string.Format(Localization.Unmountable_0, testFile));

                var serializerOptions = new JsonSerializerOptions
                {
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    },
                    MaxDepth                    = 1536, // More than this an we get a StackOverflowException
                    WriteIndented               = true,
                    DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true
                };

                if(test.ContentsJson != null)
                {
                    test.Contents =
                        JsonSerializer.Deserialize<Dictionary<string, FileData>>(test.ContentsJson, serializerOptions);
                }
                else if(File.Exists($"{testFile}.contents.json"))
                {
                    var sr = new FileStream($"{testFile}.contents.json", FileMode.Open);
                    test.Contents = JsonSerializer.Deserialize<Dictionary<string, FileData>>(sr, serializerOptions);
                }

                if(test.Contents is null)
                    continue;

                var currentDepth = 0;

                TestDirectory(fs, "/", test.Contents, testFile, true, out List<NextLevel> currentLevel, currentDepth);

                while(currentLevel.Count > 0)
                {
                    currentDepth++;
                    List<NextLevel> nextLevels = new();

                    foreach(NextLevel subLevel in currentLevel)
                    {
                        TestDirectory(fs,                            subLevel.Path, subLevel.Children, testFile, true,
                                      out List<NextLevel> nextLevel, currentDepth);

                        nextLevels.AddRange(nextLevel);
                    }

                    currentLevel = nextLevels;
                }
            }
        });
    }

    [Test]
    [Ignore("Not a test, do not run")]
    public void Build()
    {
        Environment.CurrentDirectory = DataFolder;

        foreach(FileSystemTest test in Tests)
        {
            string testFile  = test.TestFile;
            var    found     = false;
            var    partition = new Partition();

            bool exists = File.Exists(testFile);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // It arrives here...
            if(!exists)
                continue;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(testFile);

            if(ImageFormat.Detect(inputFilter) is not IMediaImage image)
                continue;

            ErrorNumber opened = image.Open(inputFilter);

            if(opened != ErrorNumber.NoError)
                continue;

            List<string> idPlugins;

            if(Partitions)
            {
                List<Partition> partitionsList = Core.Partitions.GetAll(image);

                // In reverse to skip boot partitions we're not interested in
                for(int index = partitionsList.Count - 1; index >= 0; index--)
                {
                    Core.Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

                    if(idPlugins.Count == 0)
                        continue;

                    if(!idPlugins.Contains(Plugin.Id.ToString()))
                        continue;

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

                found = idPlugins.Contains(Plugin.Id.ToString());
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // It is not the case, it changes
            if(!found)
                continue;

            var fs = Activator.CreateInstance(Plugin.GetType()) as IReadOnlyFilesystem;

            test.Encoding ??= Encoding.ASCII;

            fs?.Mount(image, partition, test.Encoding, null, test.Namespace);

            Dictionary<string, FileData> contents = BuildDirectory(fs, "/", 0);

            var serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                },
                MaxDepth                    = 1536,
                WriteIndented               = true,
                DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            var sw = new FileStream($"{testFile}.contents.json", FileMode.Create);
            JsonSerializer.Serialize(sw, contents, serializerOptions);
            sw.Close();
        }
    }

    internal static Dictionary<string, FileData> BuildDirectory(IReadOnlyFilesystem fs, string path, int currentDepth)
    {
        currentDepth++;

        if(path == "/")
            path = "";

        Dictionary<string, FileData> children = new();
        ErrorNumber                  ret      = fs.OpenDir(path, out IDirNode node);

        if(ret != ErrorNumber.NoError)
            return children;

        while(fs.ReadDir(node, out string child) == ErrorNumber.NoError &&
              child is not null)
        {
            var childPath = $"{path}/{child}";
            fs.Stat(childPath, out FileEntryInfo stat);

            var data = new FileData
            {
                Info = stat
            };

            if(stat.Attributes.HasFlag(FileAttributes.Directory))
            {
                // Cannot serialize to JSON too many depth levels 🤷‍♀️
                if(currentDepth < 384)
                    data.Children = BuildDirectory(fs, childPath, currentDepth);
            }
            else if(stat.Attributes.HasFlag(FileAttributes.Symlink))
            {
                if(fs.ReadLink(childPath, out string link) == ErrorNumber.NoError)
                    data.LinkTarget = link;
            }
            else
                data.Md5 = BuildFile(fs, childPath, stat.Length);

            if(fs.ListXAttr(childPath, out List<string> xattrs) == ErrorNumber.NoError &&
               xattrs.Count                                     > 0)
                data.XattrsWithMd5 = BuildFileXattrs(fs, childPath);

            children[child] = data;
        }

        fs.CloseDir(node);

        return children;
    }

    static string BuildFile(IReadOnlyFilesystem fs, string path, long length)
    {
        var buffer = new byte[length];

        ErrorNumber error = fs.OpenFile(path, out IFileNode fileNode);

        if(error != ErrorNumber.NoError)
            return Md5Context.Data(buffer, out _);

        fs.ReadFile(fileNode, length, buffer, out _);
        fs.CloseFile(fileNode);

        return Md5Context.Data(buffer, out _);
    }

    static Dictionary<string, string> BuildFileXattrs(IReadOnlyFilesystem fs, string path)
    {
        fs.ListXAttr(path, out List<string> contents);

        if(contents.Count == 0)
            return null;

        Dictionary<string, string> xattrs = new();

        foreach(string xattr in contents)
        {
            byte[]      buffer = Array.Empty<byte>();
            ErrorNumber ret    = fs.GetXattr(path, xattr, ref buffer);
            string      data;

            data = ret != ErrorNumber.NoError && ret != ErrorNumber.OutOfRange
                       ? Md5Context.Data(Array.Empty<byte>(), out _)
                       : Md5Context.Data(buffer,              out _);

            xattrs[xattr] = data;
        }

        return xattrs;
    }

    internal static void TestDirectory(IReadOnlyFilesystem fs,       string path, Dictionary<string, FileData> children,
                                       string              testFile, bool   testXattr, out List<NextLevel> nextLevels,
                                       int                 currentDepth)
    {
        currentDepth++;
        nextLevels = new List<NextLevel>();
        ErrorNumber ret = fs.OpenDir(path, out IDirNode node);

        // Directory is not readable, probably filled the volume, just ignore it
        if(ret == ErrorNumber.InvalidArgument)
            return;

        Assert.AreEqual(ErrorNumber.NoError, ret,
                        string.Format(Localization.Unexpected_error_0_when_reading_directory_1_of_2, ret, path,
                                      testFile));

        if(ret != ErrorNumber.NoError)
            return;

        List<string> contents = new();

        while(fs.ReadDir(node, out string filename) == ErrorNumber.NoError &&
              filename is not null)
            contents.Add(filename);

        fs.CloseDir(node);

        if(children.Count == 0 &&
           contents.Count == 0)
            return;

        if(path == "/")
            path = "";

        List<string> expectedNotFound = new();

        foreach(KeyValuePair<string, FileData> child in children)
        {
            var childPath = $"{path}/{child.Key}";
            ret = fs.Stat(childPath, out FileEntryInfo stat);

            if(ret == ErrorNumber.NoSuchFile ||
               contents is null              ||
               ret == ErrorNumber.NoError && !contents.Contains(child.Key))
            {
                expectedNotFound.Add(child.Key);

                continue;
            }

            contents.Remove(child.Key);

            Assert.AreEqual(ErrorNumber.NoError, ret,
                            string.Format(Localization.Unexpected_error_0_retrieving_stats_for_1_in_2, ret, childPath,
                                          testFile));

            if(child.Value.Info is not null)
            {
                if((stat.AccessTime - child.Value.Info.AccessTime)?.Hours is 1 or -1)
                    stat.AccessTime = child.Value.Info.AccessTime;

                if((stat.AccessTimeUtc - child.Value.Info.AccessTimeUtc)?.Hours is 1 or -1)
                    stat.AccessTimeUtc = child.Value.Info.AccessTimeUtc;

                if((stat.BackupTime - child.Value.Info.BackupTime)?.Hours is 1 or -1)
                    stat.BackupTime = child.Value.Info.BackupTime;

                if((stat.BackupTimeUtc - child.Value.Info.BackupTimeUtc)?.Hours is 1 or -1)
                    stat.BackupTimeUtc = child.Value.Info.BackupTimeUtc;

                if((stat.CreationTime - child.Value.Info.CreationTime)?.Hours is 1 or -1)
                    stat.CreationTime = child.Value.Info.CreationTime;

                if((stat.CreationTimeUtc - child.Value.Info.CreationTimeUtc)?.Hours is 1 or -1)
                    stat.CreationTimeUtc = child.Value.Info.CreationTimeUtc;

                if((stat.LastWriteTime - child.Value.Info.LastWriteTime)?.Hours is 1 or -1)
                    stat.LastWriteTime = child.Value.Info.LastWriteTime;

                if((stat.LastWriteTimeUtc - child.Value.Info.LastWriteTimeUtc)?.Hours is 1 or -1)
                    stat.LastWriteTimeUtc = child.Value.Info.LastWriteTimeUtc;

                if((stat.StatusChangeTime - child.Value.Info.StatusChangeTime)?.Hours is 1 or -1)
                    stat.StatusChangeTime = child.Value.Info.StatusChangeTime;

                if((stat.StatusChangeTimeUtc - child.Value.Info.StatusChangeTimeUtc)?.Hours is 1 or -1)
                    stat.StatusChangeTimeUtc = child.Value.Info.StatusChangeTimeUtc;
            }

            stat.Should().BeEquivalentTo(child.Value.Info,
                                         string.Format(Localization.Wrong_info_for_0_in_1, childPath, testFile));

            byte[] buffer = Array.Empty<byte>();

            if(child.Value.Info.Attributes.HasFlag(FileAttributes.Directory))
            {
                ret = fs.OpenFile(childPath, out _);

                Assert.AreEqual(ErrorNumber.IsDirectory, ret,
                                string.Format(Localization.Got_wrong_data_for_directory_0_in_1, childPath, testFile));

                // Cannot serialize to JSON too many depth levels 🤷‍♀️
                if(currentDepth < 384)
                {
                    Assert.IsNotNull(child.Value.Children,
                                     string.
                                         Format(
                                             Localization.Contents_for_0_in_1_must_be_defined_in_unit_test_declaration,
                                             childPath, testFile));

                    if(child.Value.Children != null)
                    {
                        nextLevels.Add(new NextLevel(childPath, child.Value.Children));

                        //   TestDirectory(fs, childPath, child.Value.Children, testFile, testXattr);
                    }
                }
            }
            else if(child.Value.Info.Attributes.HasFlag(FileAttributes.Symlink))
            {
                ret = fs.ReadLink(childPath, out string link);

                Assert.AreEqual(ErrorNumber.NoError, ret,
                                string.Format(Localization.Got_wrong_data_for_symbolic_link_0_in_1, childPath,
                                              testFile));

                Assert.AreEqual(child.Value.LinkTarget, link,
                                string.Format(Localization.Invalid_target_for_symbolic_link_0_in_1, childPath,
                                              testFile));
            }
            else

                // This ensure the buffer does not hang for collection
                TestFile(fs, childPath, child.Value.Md5, child.Value.Info.Length, testFile);

            if(!testXattr)
                continue;

            ret = fs.ListXAttr(childPath, out List<string> xattrs);

            if(ret == ErrorNumber.NotSupported)
            {
                Assert.IsNull(child.Value.XattrsWithMd5,
                              string.
                                  Format(
                                      Localization.
                                          Defined_extended_attributes_for_0_in_1_are_not_supported_by_filesystem,
                                      childPath, testFile));

                continue;
            }

            Assert.AreEqual(ErrorNumber.NoError, ret,
                            string.Format(Localization.Unexpected_error_0_when_listing_extended_attributes_for_1_in_2,
                                          ret, childPath, testFile));

            if(xattrs.Count > 0)
            {
                Assert.IsNotNull(child.Value.XattrsWithMd5,
                                 string.
                                     Format(
                                         Localization.
                                             Extended_attributes_for_0_in_1_must_be_defined_in_unit_test_declaration,
                                         childPath, testFile));
            }

            if(xattrs.Count                     > 0 ||
               child.Value.XattrsWithMd5?.Count > 0)
                TestFileXattrs(fs, childPath, child.Value.XattrsWithMd5, testFile);
        }

        Assert.IsEmpty(expectedNotFound,
                       string.Format(Localization.Could_not_find_the_children_of_0_in_1_2, path, testFile,
                                     string.Join(" ", expectedNotFound)));

        if(contents != null)
        {
            Assert.IsEmpty(contents,
                           string.Format(Localization.Found_the_following_unexpected_children_of_0_in_1_2, path,
                                         testFile, string.Join(" ", contents)));
        }
    }

    static void TestFile(IReadOnlyFilesystem fs, string path, string md5, long length, string testFile)
    {
        var         buffer = new byte[length];
        ErrorNumber ret    = fs.OpenFile(path, out IFileNode fileNode);

        Assert.AreEqual(ErrorNumber.NoError, ret,
                        string.Format(Localization.Unexpected_error_0_when_reading_1_in_2, ret, path, testFile));

        ret = fs.ReadFile(fileNode, length, buffer, out long readBytes);

        Assert.AreEqual(ErrorNumber.NoError, ret,
                        string.Format(Localization.Unexpected_error_0_when_reading_1_in_2, ret, path, testFile));

        Assert.AreEqual(length, readBytes,
                        string.Format(Localization.Got_less_bytes_0_than_expected_1_when_reading_2_in_3, readBytes,
                                      length, path, testFile));

        fs.CloseFile(fileNode);

        string data = Md5Context.Data(buffer, out _);

        Assert.AreEqual(md5, data,
                        string.Format(Localization.Got_MD5_0_for_1_in_2_but_expected_3, data, path, testFile, md5));
    }

    static void TestFileXattrs(IReadOnlyFilesystem fs, string path, Dictionary<string, string> xattrs, string testFile)
    {
        // Nothing to test
        if(xattrs is null)
            return;

        fs.ListXAttr(path, out List<string> contents);

        if(xattrs.Count   == 0 &&
           contents.Count == 0)
            return;

        List<string> expectedNotFound = new();

        foreach(KeyValuePair<string, string> xattr in xattrs)
        {
            byte[]      buffer = Array.Empty<byte>();
            ErrorNumber ret    = fs.GetXattr(path, xattr.Key, ref buffer);

            if(ret == ErrorNumber.NoSuchExtendedAttribute ||
               !contents.Contains(xattr.Key))
            {
                expectedNotFound.Add(xattr.Key);

                continue;
            }

            contents.Remove(xattr.Key);

            // Partially read extended attribute... dunno why it happens with some Toast images
            if(ret != ErrorNumber.OutOfRange)
            {
                Assert.AreEqual(ErrorNumber.NoError, ret,
                                string.Format(Localization.Unexpected_error_0_retrieving_extended_attributes_for_1_in_2,
                                              ret, path, testFile));
            }

            string data = Md5Context.Data(buffer, out _);

            Assert.AreEqual(xattr.Value, data,
                            string.Format(Localization.Got_MD5_0_for_1_of_2_in_3_but_expected_4, data, xattr.Key, path,
                                          testFile, xattr.Value));
        }

        Assert.IsEmpty(expectedNotFound,
                       string.Format(Localization.Could_not_find_the_following_extended_attributes_of_0_in_1_2, path,
                                     testFile, string.Join(" ", expectedNotFound)));

        Assert.IsEmpty(contents,
                       string.Format(Localization.Found_the_following_unexpected_extended_attributes_of_0_in_1_2, path,
                                     testFile, string.Join(" ", contents)));
    }

#region Nested type: NextLevel

    internal sealed record NextLevel(string Path, Dictionary<string, FileData> Children);

#endregion
}