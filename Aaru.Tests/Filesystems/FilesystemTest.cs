namespace Aaru.Tests.Filesystems;

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using NUnit.Framework;

public abstract class FilesystemTest
{
    readonly string _fileSystemType;

    public FilesystemTest() => _fileSystemType = null;

    public FilesystemTest(string fileSystemType) => _fileSystemType = fileSystemType;

    public abstract string      DataFolder { get; }
    public abstract IFilesystem Plugin     { get; }
    public abstract bool        Partitions { get; }

    public abstract FileSystemTest[] Tests { get; }

    [Test]
    public void Detect()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(FileSystemTest test in Tests)
            {
                string testFile = test.TestFile;

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

                    var found = false;

                    foreach(Partition p in partitionsList)
                    {
                        Filesystems.Identify(image, out idPlugins, p, true);

                        if(idPlugins.Count == 0)
                            continue;

                        if(!idPlugins.Contains(Plugin.Id.ToString()))
                            continue;

                        found = true;

                        break;
                    }

                    Assert.True(found, $"Filesystem not identified for {testFile}");
                }
                else
                {
                    var wholePart = new Partition
                    {
                        Name   = "Whole device",
                        Length = image.Info.Sectors,
                        Size   = image.Info.Sectors * image.Info.SectorSize
                    };

                    Filesystems.Identify(image, out idPlugins, wholePart, true);

                    Assert.Greater(idPlugins.Count, 0, $"No filesystems found for {testFile}");

                    Assert.True(idPlugins.Contains(Plugin.Id.ToString()), $"Not identified for {testFile}");
                }
            }
        });
    }

    [Test]
    public void ImageInfo()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(FileSystemTest test in Tests)
            {
                string testFile = test.TestFile;

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

                Assert.AreEqual(test.MediaType, image.Info.MediaType, testFile);
                Assert.AreEqual(test.Sectors, image.Info.Sectors, testFile);
                Assert.AreEqual(test.SectorSize, image.Info.SectorSize, testFile);
            }
        });
    }

    [Test]
    public void Info()
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

                var fs = Activator.CreateInstance(Plugin.GetType()) as IFilesystem;

                Assert.NotNull(fs, $"Could not instantiate filesystem for {testFile}");

                fs.GetInformation(image, partition, out _, null);

                if(test.ApplicationId != null)
                    Assert.AreEqual(test.ApplicationId, fs.XmlFsType.ApplicationIdentifier,
                                    $"Application ID: {testFile}");

                Assert.AreEqual(test.Bootable, fs.XmlFsType.Bootable, $"Bootable: {testFile}");
                Assert.AreEqual(test.Clusters, fs.XmlFsType.Clusters, $"Clusters: {testFile}");
                Assert.AreEqual(test.ClusterSize, fs.XmlFsType.ClusterSize, $"Cluster size: {testFile}");

                if(test.SystemId != null)
                    Assert.AreEqual(test.SystemId, fs.XmlFsType.SystemIdentifier, $"System ID: {testFile}");

                Assert.AreEqual(_fileSystemType ?? test.Type, fs.XmlFsType.Type, $"Filesystem type: {testFile}");

                Assert.AreEqual(test.VolumeName, fs.XmlFsType.VolumeName, $"Volume name: {testFile}");
                Assert.AreEqual(test.VolumeSerial, fs.XmlFsType.VolumeSerial, $"Volume serial: {testFile}");
            }
        });
    }
}