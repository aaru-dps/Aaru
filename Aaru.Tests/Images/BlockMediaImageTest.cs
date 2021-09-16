using System;
using System.Collections.Generic;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Tests.Filesystems;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    public abstract class BlockMediaImageTest : BaseMediaImageTest
    {
        // How many sectors to read at once
        const           uint                     SECTORS_TO_READ = 256;
        public abstract BlockImageTestExpected[] Tests { get; }

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = DataFolder;

            Assert.Multiple(() =>
            {
                foreach(BlockImageTestExpected test in Tests)
                {
                    string testFile = test.TestFile;

                    bool exists = File.Exists(testFile);
                    Assert.True(exists, $"{testFile} not found");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It arrives here...
                    if(!exists)
                        continue;

                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(testFile);
                    filter.Open(testFile);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {testFile}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(test.Sectors, image.Info.Sectors, $"Sectors: {testFile}");
                            Assert.AreEqual(test.SectorSize, image.Info.SectorSize, $"Sector size: {testFile}");
                            Assert.AreEqual(test.MediaType, image.Info.MediaType, $"Media type: {testFile}");
                        });
                    }
                }
            });
        }

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = DataFolder;

            Assert.Multiple(() =>
            {
                foreach(BlockImageTestExpected test in Tests)
                {
                    string testFile = test.TestFile;

                    bool exists = File.Exists(testFile);
                    Assert.True(exists, $"{testFile} not found");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It arrives here...
                    if(!exists)
                        continue;

                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(testFile);
                    filter.Open(testFile);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {testFile}");

                    if(!opened)
                        continue;

                    ulong doneSectors = 0;
                    var   ctx         = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                            doneSectors += SECTORS_TO_READ;
                        }
                        else
                        {
                            sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        ctx.Update(sector);
                    }

                    Assert.AreEqual(test.MD5, ctx.End(), $"Hash: {testFile}");
                }
            });
        }

        [Test]
        public void Contents()
        {
            Environment.CurrentDirectory = DataFolder;
            PluginBase plugins = GetPluginBase.Instance;

            Assert.Multiple(() =>
            {
                foreach(BlockImageTestExpected test in Tests)
                {
                    if(test.Partitions is null)
                        continue;

                    string testFile = test.TestFile;

                    bool exists = File.Exists(testFile);
                    Assert.True(exists, $"{testFile} not found");

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // It arrives here...
                    if(!exists)
                        continue;

                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(testFile);
                    filter.Open(testFile);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {testFile}");

                    if(!opened)
                        continue;

                    List<Partition> partitions = Core.Partitions.GetAll(image);

                    if(partitions.Count == 0)
                    {
                        partitions.Add(new Partition
                        {
                            Description = "Whole device",
                            Length      = image.Info.Sectors,
                            Offset      = 0,
                            Size        = image.Info.SectorSize * image.Info.Sectors,
                            Sequence    = 1,
                            Start       = 0
                        });
                    }

                    Assert.AreEqual(test.Partitions.Length, partitions.Count,
                                    $"Expected {test.Partitions.Length} partitions in {testFile} but found {partitions.Count}");

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            for(int i = 0; i < test.Partitions.Length; i++)
                            {
                                BlockPartitionVolumes expectedPartition = test.Partitions[i];
                                Partition             foundPartition    = partitions[i];

                                Assert.AreEqual(expectedPartition.Start, foundPartition.Start,
                                                $"Expected partition {i} to start at sector {expectedPartition.Start} but found it starts at {foundPartition.Start} in {testFile}");

                                Assert.AreEqual(expectedPartition.Length, foundPartition.Length,
                                                $"Expected partition {i} to have {expectedPartition.Length} sectors but found it has {foundPartition.Length} sectors in {testFile}");

                                string expectedDataFilename = $"{testFile}.contents.partition{i}.json";

                                if(!File.Exists(expectedDataFilename))
                                    continue;

                                var serializer = new JsonSerializer
                                {
                                    Formatting        = Formatting.Indented,
                                    MaxDepth          = 16384,
                                    NullValueHandling = NullValueHandling.Ignore
                                };

                                serializer.Converters.Add(new StringEnumConverter());

                                var sr = new StreamReader(expectedDataFilename);

                                VolumeData[] expectedData =
                                    serializer.Deserialize<VolumeData[]>(new JsonTextReader(sr));

                                Assert.NotNull(expectedData);

                                Core.Filesystems.Identify(image, out List<string> idPlugins, partitions[i]);

                                if(expectedData.Length != idPlugins.Count)
                                {
                                    continue;

                                    // Uncomment to generate JSON file
                                    /*
                                    expectedData = new VolumeData[idPlugins.Count];

                                    for(int j = 0; j < idPlugins.Count; j++)
                                    {
                                        string pluginName = idPlugins[j];

                                        if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName,
                                               out IReadOnlyFilesystem plugin))
                                            continue;

                                        Assert.IsNotNull(plugin, "Could not instantiate filesystem plugin");

                                        var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                             Invoke(new object[]
                                                                                 {});

                                        Assert.IsNotNull(fs, $"Could not instantiate filesystem {pluginName}");

                                        Errno error = fs.Mount(image, partitions[i], null, null, null);

                                        Assert.AreEqual(ErrorNumber.NoError, error,
                                                        $"Could not mount {pluginName} in partition {i}.");

                                        if(error != ErrorNumber.NoError)
                                            continue;

                                        expectedData[j] = new VolumeData
                                        {
                                            Files = ReadOnlyFilesystemTest.BuildDirectory(fs, "/")
                                        };
                                    }

                                    var sw = new StreamWriter(expectedDataFilename);
                                    serializer.Serialize(sw, expectedData);
                                    sw.Close();
                                    */
                                }

                                if(idPlugins.Count == 0)
                                    continue;

                                Assert.AreEqual(expectedData.Length, idPlugins.Count,
                                                $"Expected {expectedData.Length} filesystems identified in partition {i} but found {idPlugins.Count} in {testFile}");

                                for(int j = 0; j < idPlugins.Count; j++)
                                {
                                    string pluginName = idPlugins[j];

                                    if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName,
                                                                                    out IReadOnlyFilesystem plugin))
                                        continue;

                                    Assert.IsNotNull(plugin, "Could not instantiate filesystem plugin");

                                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                         Invoke(new object[]
                                                                                    {});

                                    Assert.IsNotNull(fs,
                                                     $"Could not instantiate filesystem {pluginName} in {testFile}");

                                    ErrorNumber error = fs.Mount(image, partitions[i], null, null, null);

                                    Assert.AreEqual(ErrorNumber.NoError, error,
                                                    $"Could not mount {pluginName} in partition {i} in {testFile}.");

                                    if(error != ErrorNumber.NoError)
                                        continue;

                                    VolumeData volumeData = expectedData[j];

                                    ReadOnlyFilesystemTest.TestDirectory(fs, "/", volumeData.Files, testFile, false);
                                }
                            }
                        });
                    }
                }
            });
        }
    }
}