using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Tests.Issues
{
    /// <summary>This will extract (and discard data) all files in all filesystems detected in an image.</summary>
    public abstract class FsExtractHashIssueTest
    {
        protected abstract string                     DataFolder       { get; }
        protected abstract string                     TestFile         { get; }
        protected abstract Dictionary<string, string> ParsedOptions    { get; }
        protected abstract bool                       Debug            { get; }
        protected abstract bool                       Xattrs           { get; }
        protected abstract string                     Encoding         { get; }
        protected abstract bool                       ExpectPartitions { get; }
        protected abstract string                     Namespace        { get; }

        [Test]
        public void Test()
        {
            Environment.CurrentDirectory = DataFolder;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(TestFile);

            Dictionary<string, string> options = ParsedOptions;
            options["debug"] = Debug.ToString();

            Assert.IsNotNull(inputFilter, "Cannot open specified file.");

            Encoding encodingClass = null;

            if(Encoding != null)
                encodingClass = Claunia.Encoding.Encoding.GetEncoding(Encoding);

            PluginBase plugins = GetPluginBase.Instance;

            IMediaImage imageFormat = ImageFormat.Detect(inputFilter);

            Assert.NotNull(imageFormat, "Image format not identified, not proceeding with analysis.");

            Assert.True(imageFormat.Open(inputFilter), "Unable to open image format");

            List<Partition> partitions = Core.Partitions.GetAll(imageFormat);

            if(partitions.Count == 0)
            {
                Assert.IsFalse(ExpectPartitions, "No partitions found");

                partitions.Add(new Partition
                {
                    Description = "Whole device",
                    Length      = imageFormat.Info.Sectors,
                    Offset      = 0,
                    Size        = imageFormat.Info.SectorSize * imageFormat.Info.Sectors,
                    Sequence    = 1,
                    Start       = 0
                });
            }

            bool filesystemFound = false;

            Assert.True(File.Exists($"{TestFile}.unittest.json"));

            var serializer = new JsonSerializer
            {
                Formatting        = Formatting.Indented,
                MaxDepth          = 16384,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializer.Converters.Add(new StringEnumConverter());

            var               sr           = new StreamReader($"{TestFile}.unittest.json");
            FsExtractHashData expectedData = serializer.Deserialize<FsExtractHashData>(new JsonTextReader(sr));

            Assert.NotNull(expectedData);

            Assert.AreEqual(expectedData.Partitions.Length, partitions.Count,
                            $"Excepted {expectedData.Partitions.Length} partitions but found {partitions.Count}");

            for(int i = 0; i < partitions.Count; i++)
            {
                Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

                if(idPlugins.Count == 0)
                {
                    Assert.IsNull(expectedData.Partitions[i],
                                  $"Expected no filesystems identified in partition {i} but found {idPlugins.Count}");

                    continue;
                }

                if(expectedData.Partitions[i].Volumes is null)
                    continue;

                Assert.AreEqual(expectedData.Partitions[i].Volumes.Length, idPlugins.Count,
                                $"Expected {expectedData.Partitions[i].Volumes.Length} filesystems identified in partition {i} but found {idPlugins.Count}");

                for(int j = 0; j < idPlugins.Count; j++)
                {
                    string pluginName = idPlugins[j];

                    if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem plugin))
                        continue;

                    Assert.IsNotNull(plugin, "Could not instantiate filesystem plugin");

                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.Invoke(new object[]
                        {});

                    Assert.IsNotNull(fs, $"Could not instantiate filesystem {pluginName}");

                    filesystemFound = true;

                    Errno error = fs.Mount(imageFormat, partitions[i], encodingClass, options, Namespace);

                    Assert.AreEqual(Errno.NoError, error, $"Could not mount {pluginName} in partition {i}.");

                    Assert.AreEqual(expectedData.Partitions[i].Volumes[j].VolumeName, fs.XmlFsType.VolumeName,
                                    $"Excepted volume name \"{expectedData.Partitions[i].Volumes[j].VolumeName}\" for filesystem {j} in partition {i} but found \"{fs.XmlFsType.VolumeName}\"");

                    VolumeData volumeData = expectedData.Partitions[i].Volumes[j];

                    ExtractFilesInDir("/", fs, Xattrs, volumeData);

                    volumeData.Directories.Should().BeEmpty("Expected directories not found:", volumeData.Directories);
                    volumeData.Files.Should().BeEmpty("Expected files not found:", volumeData.Files.Keys);
                }
            }

            Assert.IsTrue(filesystemFound, "No filesystems found.");
        }

        static void ExtractFilesInDir(string path, IReadOnlyFilesystem fs, bool doXattrs, VolumeData volumeData)
        {
            if(path.StartsWith('/'))
                path = path[1..];

            Errno error = fs.ReadDir(path, out List<string> directory);

            Assert.AreEqual(Errno.NoError, error,
                            string.Format("Error {0} reading root directory {0}", error.ToString()));

            foreach(string entry in directory)
            {
                error = fs.Stat(path + "/" + entry, out FileEntryInfo stat);

                Assert.AreEqual(Errno.NoError, error, $"Error getting stat for entry {entry}");

                if(stat.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if(string.IsNullOrWhiteSpace(path))
                    {
                        Assert.True(volumeData.Directories.Contains(entry), $"Found unexpected directory {entry}");
                        volumeData.Directories.Remove(entry);
                    }
                    else
                    {
                        Assert.True(volumeData.Directories.Contains(path + "/" + entry),
                                    $"Found unexpected directory {path + "/" + entry}");

                        volumeData.Directories.Remove(path + "/" + entry);
                    }

                    ExtractFilesInDir(path + "/" + entry, fs, doXattrs, volumeData);

                    continue;
                }

                FileData fileData;

                if(string.IsNullOrWhiteSpace(path))
                {
                    Assert.IsTrue(volumeData.Files.TryGetValue(entry, out fileData), $"Found unexpected file {entry}");

                    volumeData.Files.Remove(entry);
                }
                else
                {
                    Assert.IsTrue(volumeData.Files.TryGetValue(path + "/" + entry, out fileData),
                                  $"Found unexpected file {path + "/" + entry}");

                    volumeData.Files.Remove(path + "/" + entry);
                }

                if(doXattrs)
                {
                    error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                    Assert.AreEqual(Errno.NoError, error,
                                    $"Error {error} getting extended attributes for entry {path + "/" + entry}");

                    Dictionary<string, string> expectedXattrs = fileData.XattrsWithMd5;

                    if(error == Errno.NoError)
                        foreach(string xattr in xattrs)
                        {
                            Assert.IsTrue(expectedXattrs.TryGetValue(xattr, out string expectedXattrMd5),
                                          $"Found unexpected extended attribute {xattr} in file {entry}");

                            expectedXattrs.Remove(xattr);

                            byte[] xattrBuf = Array.Empty<byte>();
                            error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                            Assert.AreEqual(Errno.NoError, error,
                                            $"Error {error} reading extended attributes for entry {path + "/" + entry}");

                            string xattrMd5 = Md5Context.Data(xattrBuf, out _);

                            Assert.AreEqual(expectedXattrMd5, xattrMd5,
                                            $"Invalid checksum for xattr {xattr} for file {path + "/" + entry}");
                        }

                    expectedXattrs.Should().
                                   BeEmpty($"Expected extended attributes not found for file {path + "/" + entry}:",
                                           expectedXattrs);
                }

                byte[] outBuf = Array.Empty<byte>();

                error = fs.Read(path + "/" + entry, 0, stat.Length, ref outBuf);

                Assert.AreEqual(Errno.NoError, error, $"Error {error} reading file {path + "/" + entry}");

                string calculatedMd5 = Md5Context.Data(outBuf, out _);

                Assert.AreEqual(fileData.MD5, calculatedMd5, $"Invalid checksum for file {path + "/" + entry}");
            }
        }
    }
}