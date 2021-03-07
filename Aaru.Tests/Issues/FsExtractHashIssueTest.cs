using System;
using System.Collections.Generic;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using FluentAssertions;
using NUnit.Framework;

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
        protected abstract FsExtractHashData          ExpectedData     { get; }

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

            Assert.AreEqual(ExpectedData.Partitions.Length, partitions.Count,
                            $"Excepted {ExpectedData.Partitions.Length} partitions but found {partitions.Count}");

            for(int i = 0; i < partitions.Count; i++)
            {
                Core.Filesystems.Identify(imageFormat, out List<string> idPlugins, partitions[i]);

                if(idPlugins.Count == 0)
                {
                    Assert.IsNull(ExpectedData.Partitions[i],
                                  $"Expected no filesystems identified in partition {i} but found {idPlugins.Count}");

                    continue;
                }

                if(ExpectedData.Partitions[i].Volumes is null)
                    continue;

                Assert.AreEqual(ExpectedData.Partitions[i].Volumes.Length, idPlugins.Count,
                                $"Expected {ExpectedData.Partitions[i].Volumes.Length} filesystems identified in partition {i} but found {idPlugins.Count}");

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

                    Assert.AreEqual(ExpectedData.Partitions[i].Volumes[j].VolumeName, fs.XmlFsType.VolumeName,
                                    $"Excepted volume name \"{ExpectedData.Partitions[i].Volumes[j].VolumeName}\" for filesystem {j} in partition {i} but found \"{fs.XmlFsType.VolumeName}\"");

                    VolumeData volumeData = ExpectedData.Partitions[i].Volumes[j];

                    ExtractFilesInDir("/", fs, Xattrs, volumeData);

                    volumeData.Directories.Should().BeEmpty("Expected directories not found:", volumeData.Directories);
                    volumeData.FilesWithMd5.Should().BeEmpty("Expected files not found:", volumeData.FilesWithMd5.Keys);
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

                if(doXattrs)
                {
                    // TODO: Hash this
                    error = fs.ListXAttr(path + "/" + entry, out List<string> xattrs);

                    Assert.AreEqual(Errno.NoError, error,
                                    $"Error {error} getting extended attributes for entry {path + "/" + entry}");

                    if(error == Errno.NoError)
                        foreach(string xattr in xattrs)
                        {
                            byte[] xattrBuf = new byte[0];
                            error = fs.GetXattr(path + "/" + entry, xattr, ref xattrBuf);

                            Assert.AreEqual(Errno.NoError, error,
                                            $"Error {error} reading extended attributes for entry {path + "/" + entry}");
                        }
                }

                string md5;

                if(string.IsNullOrWhiteSpace(path))
                {
                    Assert.IsTrue(volumeData.FilesWithMd5.TryGetValue(entry, out md5),
                                  $"Found unexpected file {entry}");

                    volumeData.FilesWithMd5.Remove(entry);
                }
                else
                {
                    Assert.IsTrue(volumeData.FilesWithMd5.TryGetValue(path + "/" + entry, out md5),
                                  $"Found unexpected file {path + "/" + entry}");

                    volumeData.FilesWithMd5.Remove(path + "/" + entry);
                }

                byte[] outBuf = new byte[0];

                error = fs.Read(path + "/" + entry, 0, stat.Length, ref outBuf);

                Assert.AreEqual(Errno.NoError, error, $"Error {error} reading file {path + "/" + entry}");

                string calculatedMd5 = Md5Context.Data(outBuf, out _);

                Assert.AreEqual(md5, calculatedMd5, $"Invalid checksum for file {path + "/" + entry}");
            }
        }
    }
}