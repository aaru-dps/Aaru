namespace Aaru.Tests.Filesystems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

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
                var    found     = false;
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

                var image = ImageFormat.Detect(inputFilter) as IMediaImage;

                Assert.IsNotNull(image, $"Image format: {testFile}");

                Assert.AreEqual(ErrorNumber.NoError, image.Open(inputFilter), $"Cannot open image for {testFile}");

                List<string> idPlugins;

                if(Partitions)
                {
                    List<Partition> partitionsList = Core.Partitions.GetAll(image);

                    Assert.Greater(partitionsList.Count, 0, $"No partitions found for {testFile}");

                    // In reverse to skip boot partitions we're not interested in
                    for(int index = partitionsList.Count - 1; index >= 0; index--)
                    {
                        Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

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

                    Filesystems.Identify(image, out idPlugins, partition, true);

                    Assert.Greater(idPlugins.Count, 0, $"No filesystems found for {testFile}");

                    found = idPlugins.Contains(Plugin.Id.ToString());
                }

                Assert.True(found, $"Filesystem not identified for {testFile}");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It is not the case, it changes
                if(!found)
                    continue;

                var fs = Activator.CreateInstance(Plugin.GetType()) as IReadOnlyFilesystem;

                Assert.NotNull(fs, $"Could not instantiate filesystem for {testFile}");

                test.Encoding ??= Encoding.ASCII;

                ErrorNumber ret = fs.Mount(image, partition, test.Encoding, null, test.Namespace);

                Assert.AreEqual(ErrorNumber.NoError, ret, $"Unmountable: {testFile}");

                var serializer = new JsonSerializer
                {
                    Formatting        = Formatting.Indented,
                    MaxDepth          = 16384,
                    NullValueHandling = NullValueHandling.Ignore
                };

                serializer.Converters.Add(new StringEnumConverter());

                if(test.ContentsJson != null)
                    test.Contents =
                        serializer.
                            Deserialize<
                                Dictionary<string, FileData>>(new JsonTextReader(new StringReader(test.ContentsJson)));
                else if(File.Exists($"{testFile}.contents.json"))
                {
                    var sr = new StreamReader($"{testFile}.contents.json");
                    test.Contents = serializer.Deserialize<Dictionary<string, FileData>>(new JsonTextReader(sr));
                }

                if(test.Contents is null)
                    continue;

                TestDirectory(fs, "/", test.Contents, testFile, true);
            }
        });
    }

    [Test, Ignore("Not a test, do not run")]
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

            var         filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(testFile);

            if(ImageFormat.Detect(inputFilter) is not IMediaImage image)
                continue;

            ErrorNumber opened      = image.Open(inputFilter);

            if(opened != ErrorNumber.NoError)
                continue;

            List<string> idPlugins;

            if(Partitions)
            {
                List<Partition> partitionsList = Core.Partitions.GetAll(image);

                // In reverse to skip boot partitions we're not interested in
                for(int index = partitionsList.Count - 1; index >= 0; index--)
                {
                    Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

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

                Filesystems.Identify(image, out idPlugins, partition, true);

                found = idPlugins.Contains(Plugin.Id.ToString());
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // It is not the case, it changes
            if(!found)
                continue;

            var fs = Activator.CreateInstance(Plugin.GetType()) as IReadOnlyFilesystem;

            test.Encoding ??= Encoding.ASCII;

            fs?.Mount(image, partition, test.Encoding, null, test.Namespace);

            Dictionary<string, FileData> contents = BuildDirectory(fs, "/");

            var serializer = new JsonSerializer
            {
                Formatting        = Formatting.Indented,
                MaxDepth          = 16384,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializer.Converters.Add(new StringEnumConverter());

            var sw = new StreamWriter($"{testFile}.contents.json");
            serializer.Serialize(sw, contents);
            sw.Close();
        }
    }

    internal static Dictionary<string, FileData> BuildDirectory(IReadOnlyFilesystem fs, string path)
    {
        if(path == "/")
            path = "";

        Dictionary<string, FileData> children = new();
        fs.ReadDir(path, out List<string> contents);

        if(contents is null)
            return children;

        foreach(string child in contents)
        {
            var childPath = $"{path}/{child}";
            fs.Stat(childPath, out FileEntryInfo stat);

            var data = new FileData
            {
                Info = stat
            };

            if(stat.Attributes.HasFlag(FileAttributes.Directory))
                data.Children = BuildDirectory(fs, childPath);
            else if(stat.Attributes.HasFlag(FileAttributes.Symlink))
            {
                if(fs.ReadLink(childPath, out string link) == ErrorNumber.NoError)
                    data.LinkTarget = link;
            }
            else
                data.Md5 = BuildFile(fs, childPath, stat.Length);

            children[child] = data;
        }

        return children;
    }

    static string BuildFile(IReadOnlyFilesystem fs, string path, long length)
    {
        var buffer = new byte[length];
        fs.Read(path, 0, length, ref buffer);

        return Md5Context.Data(buffer, out _);
    }

    internal static void TestDirectory(IReadOnlyFilesystem fs, string path, Dictionary<string, FileData> children,
                                       string testFile, bool testXattr)
    {
        ErrorNumber ret = fs.ReadDir(path, out List<string> contents);

        Assert.AreEqual(ErrorNumber.NoError, ret,
                        $"Unexpected error {ret} when reading directory \"{path}\" of {testFile}.");

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
                            $"Unexpected error {ret} retrieving stats for \"{childPath}\" in {testFile}");

            stat.Should().BeEquivalentTo(child.Value.Info, $"Wrong info for \"{childPath}\" in {testFile}");

            byte[] buffer = Array.Empty<byte>();

            if(child.Value.Info.Attributes.HasFlag(FileAttributes.Directory))
            {
                ret = fs.Read(childPath, 0, 1, ref buffer);

                Assert.AreEqual(ErrorNumber.IsDirectory, ret,
                                $"Got wrong data for directory \"{childPath}\" in {testFile}");

                Assert.IsNotNull(child.Value.Children,
                                 $"Contents for \"{childPath}\" in {testFile} must be defined in unit test declaration!");

                if(child.Value.Children != null)
                    TestDirectory(fs, childPath, child.Value.Children, testFile, testXattr);
            }
            else if(child.Value.Info.Attributes.HasFlag(FileAttributes.Symlink))
            {
                ret = fs.ReadLink(childPath, out string link);

                Assert.AreEqual(ErrorNumber.NoError, ret,
                                $"Got wrong data for symbolic link \"{childPath}\" in {testFile}");

                Assert.AreEqual(child.Value.LinkTarget, link,
                                $"Invalid target for symbolic link \"{childPath}\" in {testFile}");
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
                              $"Defined extended attributes for \"{childPath}\" in {testFile} are not supported by filesystem.");

                continue;
            }

            Assert.AreEqual(ErrorNumber.NoError, ret,
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

        if(contents != null)
            Assert.IsEmpty(contents,
                           $"Found the following unexpected children of \"{path}\" in {testFile}: {string.Join(" ", contents)}");
    }

    static void TestFile(IReadOnlyFilesystem fs, string path, string md5, long length, string testFile)
    {
        var         buffer = new byte[length];
        ErrorNumber ret    = fs.Read(path, 0, length, ref buffer);

        Assert.AreEqual(ErrorNumber.NoError, ret, $"Unexpected error {ret} when reading \"{path}\" in {testFile}");

        string data = Md5Context.Data(buffer, out _);

        Assert.AreEqual(md5, data, $"Got MD5 {data} for \"{path}\" in {testFile} but expected {md5}");
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

            Assert.AreEqual(ErrorNumber.NoError, ret,
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