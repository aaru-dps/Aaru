using System;
using System.Collections.Generic;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;

namespace Aaru.Tests.Filesystems
{
    public abstract class ReadOnlyFilesystemTest : FilesystemTest
    {
        public ReadOnlyFilesystemTest() {}

        public ReadOnlyFilesystemTest(string fileSystemType) : base(fileSystemType) {}

        [Test]
        public void Contents()
        {
            Environment.CurrentDirectory = DataFolder;

            Assert.Multiple(() =>
            {
                foreach(FileSystemTest test in Tests)
                {
                    string testFile  = test.TestFile;
                    bool   found     = false;
                    var    partition = new Partition();

                    bool exists = File.Exists(testFile);
                    Assert.True(exists, $"{testFile} not found");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It arrives here...
                    if(!exists)
                        continue;

                    var     filtersList = new FiltersList();
                    IFilter inputFilter = filtersList.GetFilter(testFile);

                    Assert.IsNotNull(inputFilter, $"Filter: {testFile}");

                    IMediaImage image = ImageFormat.Detect(inputFilter);

                    Assert.IsNotNull(image, $"Image format: {testFile}");

                    Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {testFile}");

                    List<string> idPlugins;

                    if(Partitions)
                    {
                        List<Partition> partitionsList = Core.Partitions.GetAll(image);

                        Assert.Greater(partitionsList.Count, 0, $"No partitions found for {testFile}");

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

                        Assert.Greater(idPlugins.Count, 0, $"No filesystems found for {testFile}");

                        found = idPlugins.Contains(Plugin.Id.ToString());
                    }

                    Assert.True(found, $"Filesystem not identified for {testFile}");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It is not the case, it changes
                    if(!found)
                        continue;

                    if(test.Contents is null)
                        continue;

                    var fs = Activator.CreateInstance(Plugin.GetType()) as IReadOnlyFilesystem;

                    Assert.NotNull(fs, $"Could not instantiate filesystem for {testFile}");

                    Errno ret = fs.Mount(image, partition, test.Encoding, null, test.Namespace);

                    Assert.AreEqual(Errno.NoError, ret, $"Unmountable: {testFile}");

                    ret = fs.StatFs(out FileSystemInfo stat);

                    Assert.AreEqual(Errno.NoError, ret, $"Unexpected error retrieving filesystem stats for {testFile}");

                    stat.Should().BeEquivalentTo(test.Info, $"Incorrect filesystem stats for {testFile}");

                    TestDirectory(fs, "/", test.Contents, testFile);
                }
            });
        }

        void TestDirectory(IReadOnlyFilesystem fs, string path, Dictionary<string, FileData> children, string testFile)
        {
            Errno ret = fs.ReadDir(path, out List<string> contents);

            Assert.AreEqual(Errno.NoError, ret,
                            $"Unexpected error {ret} when reading directory \"{path}\" of {testFile}.");

            if(children.Count == 0 &&
               contents.Count == 0)
                return;

            if(path == "/")
                path = "";

            List<string> expectedNotFound = new List<string>();

            foreach(KeyValuePair<string, FileData> child in children)
            {
                string childPath = $"{path}/{child.Key}";
                ret = fs.Stat(childPath, out FileEntryInfo stat);

                if(ret == Errno.NoSuchFile ||
                   !contents.Contains(child.Key))
                {
                    expectedNotFound.Add(child.Key);

                    continue;
                }

                contents.Remove(child.Key);

                Assert.AreEqual(Errno.NoError, ret,
                                $"Unexpected error {ret} retrieving stats for \"{childPath}\" in {testFile}");

                stat.Should().BeEquivalentTo(child.Value.Info, $"Wrong info for \"{childPath}\" in {testFile}");

                byte[] buffer = new byte[0];

                if(child.Value.Info.Attributes.HasFlag(FileAttributes.Directory))
                {
                    ret = fs.Read(childPath, 0, 1, ref buffer);

                    Assert.AreEqual(Errno.IsDirectory, ret,
                                    $"Got wrong data for directory \"{childPath}\" in {testFile}");

                    Assert.IsNotNull(child.Value.Children,
                                     $"Contents for \"{childPath}\" in {testFile} must be defined in unit test declaration!");

                    if(child.Value.Children != null)
                        TestDirectory(fs, childPath, child.Value.Children, testFile);
                }
                else if(child.Value.Info.Attributes.HasFlag(FileAttributes.Symlink))
                {
                    ret = fs.ReadLink(childPath, out string link);

                    Assert.AreEqual(Errno.NoError, ret,
                                    $"Got wrong data for symbolic link \"{childPath}\" in {testFile}");

                    Assert.AreEqual(child.Value.LinkTarget, link,
                                    $"Invalid target for symbolic link \"{childPath}\" in {testFile}");
                }
                else

                    // This ensure the buffer does not hang for collection
                    TestFile(fs, childPath, child.Value.MD5, child.Value.Info.Length, testFile);

                ret = fs.ListXAttr(childPath, out List<string> xattrs);

                if(ret == Errno.NotSupported)
                {
                    Assert.IsNull(child.Value.XattrsWithMd5,
                                  $"Defined extended attributes for \"{childPath}\" in {testFile} are not supported by filesystem.");

                    continue;
                }

                Assert.AreEqual(Errno.NoError, ret,
                                $"Unexpected error {ret} when listing extended attributes for \"{childPath}\" in {testFile}");

                if(xattrs.Count > 0)
                    Assert.IsNotNull(child.Value.XattrsWithMd5,
                                     $"Extended attributes for \"{childPath}\" in {testFile} must be defined in unit test declaration!");

                if(xattrs.Count                     > 0 ||
                   child.Value.XattrsWithMd5?.Count > 0)
                    TestFileXattrs(fs, childPath, child.Value.XattrsWithMd5, testFile);
            }

            Assert.IsEmpty(expectedNotFound,
                           $"Could not find the children of \"{path}\" in {testFile}: {string.Join(" ", expectedNotFound)}");

            Assert.IsEmpty(contents,
                           $"Found the following unexpected children of \"{path}\" in {testFile}: {string.Join(" ", contents)}");
        }

        void TestFile(IReadOnlyFilesystem fs, string path, string md5, long length, string testFile)
        {
            byte[] buffer = new byte[length];
            Errno  ret    = fs.Read(path, 0, length, ref buffer);

            Assert.AreEqual(Errno.NoError, ret, $"Unexpected error {ret} when reading \"{path}\" in {testFile}");

            string data = Md5Context.Data(buffer, out _);

            Assert.AreEqual(md5, data, $"Got MD5 {data} for \"{path}\" in {testFile} but expected {md5}");
        }

        void TestFileXattrs(IReadOnlyFilesystem fs, string path, Dictionary<string, string> xattrs, string testFile)
        {
            fs.ListXAttr(path, out List<string> contents);

            if(xattrs.Count   == 0 &&
               contents.Count == 0)
                return;

            List<string> expectedNotFound = new List<string>();

            foreach(KeyValuePair<string, string> xattr in xattrs)
            {
                byte[] buffer = new byte[0];
                Errno  ret    = fs.GetXattr(path, xattr.Key, ref buffer);

                if(ret == Errno.NoSuchExtendedAttribute ||
                   !contents.Contains(xattr.Key))
                {
                    expectedNotFound.Add(xattr.Key);

                    continue;
                }

                contents.Remove(xattr.Key);

                Assert.AreEqual(Errno.NoError, ret,
                                $"Unexpected error {ret} retrieving extended attributes for \"{path}\" in {testFile}");

                string data = Md5Context.Data(buffer, out _);

                Assert.AreEqual(xattr.Value, data,
                                $"Got MD5 {data} for {xattr.Key} of \"{path}\" in {testFile} but expected {xattr.Value}");
            }

            Assert.IsEmpty(expectedNotFound,
                           $"Could not find the following extended attributes of \"{path}\" in {testFile}: {string.Join(" ", expectedNotFound)}");

            Assert.IsEmpty(contents,
                           $"Found the following unexpected extended attributes of \"{path}\" in {testFile}: {string.Join(" ", contents)}");
        }
    }
}