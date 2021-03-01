using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    public abstract class FilesystemTest
    {
        readonly string _fileSystemType;

        public FilesystemTest(string fileSystemType) => _fileSystemType = fileSystemType;

        public abstract string      _dataFolder { get; }
        public abstract IFilesystem _plugin     { get; }
        public abstract bool        _partitions { get; }
        public abstract string[]    _testFiles  { get; }
        public abstract MediaType[] _mediaTypes { get; }
        public abstract ulong[]     _sectors    { get; }
        public abstract uint[]      _sectorSize { get; }

        public abstract string[] _appId        { get; }
        public abstract bool[]   _bootable     { get; }
        public abstract long[]   _clusters     { get; }
        public abstract uint[]   _clusterSize  { get; }
        public abstract string[] _oemId        { get; }
        public abstract string[] _type         { get; }
        public abstract string[] _volumeName   { get; }
        public abstract string[] _volumeSerial { get; }

        [Test]
        public void Detect()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                foreach(string testFile in _testFiles)
                {
                    var     filtersList = new FiltersList();
                    IFilter inputFilter = filtersList.GetFilter(testFile);

                    Assert.IsNotNull(inputFilter, $"Filter: {testFile}");

                    IMediaImage image = ImageFormat.Detect(inputFilter);

                    Assert.IsNotNull(image, $"Image format: {testFile}");

                    Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {testFile}");

                    List<string> idPlugins;

                    if(_partitions)
                    {
                        List<Partition> partitionsList = Core.Partitions.GetAll(image);

                        Assert.Greater(partitionsList.Count, 0, $"No partitions found for {testFile}");

                        bool found = false;

                        foreach(Partition p in partitionsList)
                        {
                            Core.Filesystems.Identify(image, out idPlugins, p, true);

                            if(idPlugins.Count == 0)
                                continue;

                            if(!idPlugins.Contains(_plugin.Id.ToString()))
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

                        Core.Filesystems.Identify(image, out idPlugins, wholePart, true);

                        Assert.Greater(idPlugins.Count, 0, $"No filesystems found for {testFile}");

                        Assert.True(idPlugins.Contains(_plugin.Id.ToString()), $"Not identified for {testFile}");
                    }
                }
            });
        }

        [Test]
        public void ImageInfo()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    string  testFile    = _testFiles[i];
                    var     filtersList = new FiltersList();
                    IFilter inputFilter = filtersList.GetFilter(testFile);

                    Assert.IsNotNull(inputFilter, $"Filter: {testFile}");

                    IMediaImage image = ImageFormat.Detect(inputFilter);

                    Assert.IsNotNull(image, $"Image format: {testFile}");

                    Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {testFile}");

                    Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                    Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                    Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                }
            });
        }

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    bool found     = false;
                    var  partition = new Partition();

                    var     filtersList = new FiltersList();
                    IFilter inputFilter = filtersList.GetFilter(_testFiles[i]);

                    Assert.IsNotNull(inputFilter, $"Filter: {_testFiles[i]}");

                    IMediaImage image = ImageFormat.Detect(inputFilter);

                    Assert.IsNotNull(image, $"Image format: {_testFiles[i]}");

                    Assert.AreEqual(true, image.Open(inputFilter), $"Cannot open image for {_testFiles[i]}");

                    List<string> idPlugins;

                    if(_partitions)
                    {
                        List<Partition> partitionsList = Core.Partitions.GetAll(image);

                        Assert.Greater(partitionsList.Count, 0, $"No partitions found for {_testFiles[i]}");

                        // In reverse to skip boot partitions we're not interested in
                        for(int index = partitionsList.Count - 1; index >= 0; index--)
                        {
                            Core.Filesystems.Identify(image, out idPlugins, partitionsList[index], true);

                            if(idPlugins.Count == 0)
                                continue;

                            if(!idPlugins.Contains(_plugin.Id.ToString()))
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

                        Assert.Greater(idPlugins.Count, 0, $"No filesystems found for {_testFiles[i]}");

                        found = idPlugins.Contains(_plugin.Id.ToString());
                    }

                    Assert.True(found, $"Filesystem not identified for {_testFiles[i]}");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It is not the case, it changes
                    if(!found)
                        continue;

                    var fs = Activator.CreateInstance(_plugin.GetType()) as IFilesystem;

                    Assert.NotNull(fs, $"Could not instantiate filesystem for {_testFiles[i]}");

                    fs.GetInformation(image, partition, out _, null);

                    if(_appId != null)
                        Assert.AreEqual(_appId[i], fs.XmlFsType.ApplicationIdentifier,
                                        $"Application ID: {_testFiles[i]}");

                    Assert.AreEqual(_bootable[i], fs.XmlFsType.Bootable, $"Bootable: {_testFiles[i]}");
                    Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, $"Clusters: {_testFiles[i]}");
                    Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, $"Cluster size: {_testFiles[i]}");

                    if(_oemId != null)
                        Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, $"System ID: {_testFiles[i]}");

                    Assert.AreEqual(_fileSystemType ?? _type[i], fs.XmlFsType.Type,
                                    $"Filesystem type: {_testFiles[i]}");

                    Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, $"Volume name: {_testFiles[i]}");
                    Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, $"Volume serial: {_testFiles[i]}");
                }
            });
        }
    }
}