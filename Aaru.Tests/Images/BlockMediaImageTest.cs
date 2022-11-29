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

namespace Aaru.Tests.Images;

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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors,
                                        string.Format(Localization.Sectors_0, testFile));

                        Assert.AreEqual(test.SectorSize, image.Info.SectorSize,
                                        string.Format(Localization.Sector_size_0, testFile));

                        Assert.AreEqual(test.MediaType, image.Info.MediaType,
                                        string.Format(Localization.Media_type_0, testFile));
                    });
            }
        });
    }

    [Test]
    public void Hashes()
    {
        Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

        Assert.Multiple(() =>
        {
            foreach(BlockImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                ulong doneSectors = 0;
                var   ctx         = new Md5Context();

                while(doneSectors < image.Info.Sectors)
                {
                    byte[] sector;

                    if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        errno       =  image.ReadSectors(doneSectors, SECTORS_TO_READ, out sector);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        errno = image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors), out sector);

                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    Assert.AreEqual(ErrorNumber.NoError, errno);
                    ctx.Update(sector);
                }

                Assert.AreEqual(test.Md5, ctx.End(), string.Format(Localization.Hash_0, testFile));
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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                List<Partition> partitions = Core.Partitions.GetAll(image);

                if(partitions.Count == 0)
                    partitions.Add(new Partition
                    {
                        Description = "Whole device",
                        Length      = image.Info.Sectors,
                        Offset      = 0,
                        Size        = image.Info.SectorSize * image.Info.Sectors,
                        Sequence    = 1,
                        Start       = 0
                    });

                Assert.AreEqual(test.Partitions.Length, partitions.Count,
                                string.Format(Localization.Expected_0_partitions_in_1_but_found_2,
                                              test.Partitions.Length, testFile, partitions.Count));

                using(new AssertionScope())
                    Assert.Multiple(() =>
                    {
                        for(int i = 0; i < test.Partitions.Length; i++)
                        {
                            BlockPartitionVolumes expectedPartition = test.Partitions[i];
                            Partition             foundPartition    = partitions[i];

                            Assert.AreEqual(expectedPartition.Start, foundPartition.Start,
                                            string.
                                                Format(Localization.Expected_partition_0_to_start_at_sector_1_but_found_it_starts_at_2_in_3,
                                                       i, expectedPartition.Start, foundPartition.Start, testFile));

                            Assert.AreEqual(expectedPartition.Length, foundPartition.Length,
                                            string.
                                                Format(Localization.Expected_partition_0_to_have_1_sectors_but_found_it_has_2_sectors_in_3,
                                                       i, expectedPartition.Length, foundPartition.Length, testFile));

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

                            VolumeData[] expectedData = serializer.Deserialize<VolumeData[]>(new JsonTextReader(sr));

                            Assert.NotNull(expectedData);

                            Core.Filesystems.Identify(image, out List<string> idPlugins, partitions[i]);

                            if(expectedData.Length != idPlugins.Count)
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

                                    Assert.IsNotNull(plugin, Localization.Could_not_instantiate_filesystem_plugin);

                                    var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                         Invoke(new object[]
                                                                             {});

                                    Assert.IsNotNull(fs, string.Format(Localization.Could_not_instantiate_filesystem_0, pluginName));

                                    Errno error = fs.Mount(image, partitions[i], null, null, null);

                                    Assert.AreEqual(ErrorNumber.NoError, error,
                                                    string.Format(Localization.Could_not_mount_0_in_partition_1, pluginName, i));

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

                            if(idPlugins.Count == 0)
                                continue;

                            Assert.AreEqual(expectedData.Length, idPlugins.Count,
                                            $"Expected {expectedData.Length} filesystems identified in partition {i
                                            } but found {idPlugins.Count} in {testFile}");

                            for(int j = 0; j < idPlugins.Count; j++)
                            {
                                string pluginName = idPlugins[j];

                                if(!plugins.ReadOnlyFilesystems.TryGetValue(pluginName, out IReadOnlyFilesystem plugin))
                                    continue;

                                Assert.IsNotNull(plugin, Localization.Could_not_instantiate_filesystem_plugin);

                                var fs = (IReadOnlyFilesystem)plugin.GetType().GetConstructor(Type.EmptyTypes)?.
                                                                     Invoke(Array.Empty<object>());

                                Assert.IsNotNull(fs, $"Could not instantiate filesystem {pluginName} in {testFile}");

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
        });
    }
}