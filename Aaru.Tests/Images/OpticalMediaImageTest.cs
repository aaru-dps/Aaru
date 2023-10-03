using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Core;
using Aaru.Tests.Filesystems;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using File = System.IO.File;
using Partition = Aaru.CommonTypes.Partition;
using Track = Aaru.CommonTypes.Structs.Track;

namespace Aaru.Tests.Images;

public abstract class OpticalMediaImageTest : BaseMediaImageTest
{
    const           uint                       SECTORS_TO_READ = 256;
    public abstract OpticalImageTestExpected[] Tests { get; }

    [Test]
    public void Info()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(OpticalImageTestExpected test in Tests)
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

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors,
                                        string.Format(Localization.Sectors_0, testFile));

                        if(test.SectorSize > 0)
                        {
                            Assert.AreEqual(test.SectorSize, image.Info.SectorSize,
                                            string.Format(Localization.Sector_size_0, testFile));
                        }

                        Assert.AreEqual(test.MediaType, image.Info.MediaType,
                                        string.Format(Localization.Media_type_0, testFile));

                        if(image.Info.MetadataMediaType != MetadataMediaType.OpticalDisc)
                            return;

                        Assert.AreEqual(test.Tracks.Length, image.Tracks.Count,
                                        string.Format(Localization.Tracks_0, testFile));

                        image.Tracks.Select(t => t.Session).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Session),
                                             string.Format(Localization.Track_session_0, testFile));

                        image.Tracks.Select(t => t.StartSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Start),
                                             string.Format(Localization.Track_start_0, testFile));

                        image.Tracks.Select(t => t.EndSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.End),
                                             string.Format(Localization.Track_end_0, testFile));

                        image.Tracks.Select(t => t.Pregap).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Pregap),
                                             string.Format(Localization.Track_pregap_0, testFile));

                        var trackNo = 0;

                        var   flags           = new byte?[image.Tracks.Count];
                        ulong latestEndSector = 0;

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(currentTrack.EndSector > latestEndSector)
                                latestEndSector = currentTrack.EndSector;

                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                            {
                                ErrorNumber errno =
                                    image.ReadSectorTag(currentTrack.Sequence, SectorTagType.CdTrackFlags,
                                                        out byte[] tmp);

                                if(errno != ErrorNumber.NoError)
                                    continue;

                                flags[trackNo] = tmp[0];
                            }

                            trackNo++;
                        }

                        flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                                      string.Format(Localization.Track_flags_0, testFile));

                        Assert.AreEqual(latestEndSector, image.Info.Sectors - 1,
                                        string.Format(Localization.Last_sector_for_tracks_is_0_but_it_is_1_for_image,
                                                      latestEndSector, image.Info.Sectors));
                    });
                }
            }
        });
    }

    [Test]
    public void Contents()
    {
        Environment.CurrentDirectory = DataFolder;

        Assert.Multiple(() =>
        {
            foreach(OpticalImageTestExpected test in Tests)
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

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        foreach(TrackInfoTestExpected track in test.Tracks)
                        {
                            if(track.FileSystems is null)
                                continue;

                            ulong trackStart = track.Start + track.Pregap;

                            if(track.Number <= 1 &&
                               track.Pregap >= 150)
                                trackStart -= 150;

                            var partition = new Partition
                            {
                                Length = track.End - trackStart + 1,
                                Start  = trackStart
                            };

                            Core.Filesystems.Identify(image, out List<string> idPlugins, partition);

                            Assert.AreEqual(track.FileSystems.Length, idPlugins.Count,
                                            string.Format(Localization.Expected_0_filesystems_in_1_but_found_2,
                                                          track.FileSystems.Length, testFile, idPlugins.Count));

                            for(var i = 0; i < track.FileSystems.Length; i++)
                            {
                                PluginBase plugins = PluginBase.Singleton;
                                bool       found   = plugins.Filesystems.TryGetValue(idPlugins[i], out Type pluginType);

                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                // It is not the case, it changes
                                if(!found)
                                    continue;

                                var fs = Activator.CreateInstance(pluginType) as IFilesystem;

                                Assert.NotNull(fs,
                                               string.Format(Localization.Could_not_instantiate_filesystem_for_0,
                                                             testFile));

                                fs.GetInformation(image, partition, null, out _, out FileSystem fsMetadata);

                                if(track.FileSystems[i].ApplicationId != null)
                                {
                                    Assert.AreEqual(track.FileSystems[i].ApplicationId,
                                                    fsMetadata.ApplicationIdentifier,
                                                    string.Format(Localization.Application_ID_0, testFile));
                                }

                                Assert.AreEqual(track.FileSystems[i].Bootable, fsMetadata.Bootable,
                                                string.Format(Localization.Bootable_0, testFile));

                                Assert.AreEqual(track.FileSystems[i].Clusters, fsMetadata.Clusters,
                                                string.Format(Localization.Clusters_0, testFile));

                                Assert.AreEqual(track.FileSystems[i].ClusterSize, fsMetadata.ClusterSize,
                                                string.Format(Localization.Cluster_size_0, testFile));

                                if(track.FileSystems[i].SystemId != null)
                                {
                                    Assert.AreEqual(track.FileSystems[i].SystemId, fsMetadata.SystemIdentifier,
                                                    string.Format(Localization.System_ID_0, testFile));
                                }

                                Assert.AreEqual(track.FileSystems[i].Type, fsMetadata.Type,
                                                string.Format(Localization.Filesystem_type_0, testFile));

                                Assert.AreEqual(track.FileSystems[i].VolumeName, fsMetadata.VolumeName,
                                                string.Format(Localization.Volume_name_0, testFile));

                                Assert.AreEqual(track.FileSystems[i].VolumeSerial, fsMetadata.VolumeSerial,
                                                string.Format(Localization.Volume_serial_0, testFile));

                                if(Activator.CreateInstance(pluginType) is not IReadOnlyFilesystem rofs)
                                {
                                    if(track.FileSystems[i].Contents     != null ||
                                       track.FileSystems[i].ContentsJson != null ||
                                       File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json"))
                                    {
                                        Assert.NotNull(null,
                                                       string.
                                                           Format(
                                                               Localization.
                                                                   Could_not_instantiate_filesystem_for_0_track_1_filesystem_2,
                                                               testFile, track.Number, i));
                                    }

                                    continue;
                                }

                                track.FileSystems[i].Encoding ??= Encoding.ASCII;

                                ErrorNumber ret = rofs.Mount(image, partition, track.FileSystems[i].Encoding, null,
                                                             track.FileSystems[i].Namespace);

                                Assert.AreEqual(ErrorNumber.NoError, ret,
                                                string.Format(Localization.Unmountable_0, testFile));

                                var serializerOptions = new JsonSerializerOptions
                                {
                                    Converters =
                                    {
                                        new JsonStringEnumConverter()
                                    },
                                    MaxDepth = 1536, // More than this an we get a StackOverflowException
                                    WriteIndented = true,
                                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                    PropertyNameCaseInsensitive = true
                                };

                                if(track.FileSystems[i].ContentsJson != null)
                                {
                                    track.FileSystems[i].Contents =
                                        JsonSerializer.
                                            Deserialize<Dictionary<string, FileData>>(track.FileSystems[i].ContentsJson,
                                                serializerOptions);
                                }
                                else if(File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json"))
                                {
                                    var sr =
                                        new FileStream($"{testFile}.track{track.Number}.filesystem{i}.contents.json",
                                                       FileMode.Open);

                                    track.FileSystems[i].Contents =
                                        JsonSerializer.Deserialize<Dictionary<string, FileData>>(sr, serializerOptions);
                                }

                                if(track.FileSystems[i].Contents is null)
                                    continue;

                                var currentDepth = 0;

                                ReadOnlyFilesystemTest.TestDirectory(rofs, "/", track.FileSystems[i].Contents, testFile,
                                                                     true,
                                                                     out List<ReadOnlyFilesystemTest.NextLevel>
                                                                             currentLevel, currentDepth);

                                while(currentLevel.Count > 0)
                                {
                                    currentDepth++;
                                    List<ReadOnlyFilesystemTest.NextLevel> nextLevels = new();

                                    foreach(ReadOnlyFilesystemTest.NextLevel subLevel in currentLevel)
                                    {
                                        ReadOnlyFilesystemTest.TestDirectory(rofs, subLevel.Path, subLevel.Children,
                                                                             testFile, true,
                                                                             out List<ReadOnlyFilesystemTest.NextLevel>
                                                                                 nextLevel, currentDepth);

                                        nextLevels.AddRange(nextLevel);
                                    }

                                    currentLevel = nextLevels;
                                }

                                // Uncomment to generate JSON file
                                /*  var contents = ReadOnlyFilesystemTest.BuildDirectory(rofs, "/", 0);

                                    var sw = new FileStream($"{testFile}.track{track.Number}.filesystem{i}.contents.json", FileMode.Create);
                                    JsonSerializer.Serialize(sw, contents, serializerOptions);
                                    sw.Close();*/
                            }
                        }
                    });
                }
            }
        });
    }

    [Test]
    public void Hashes()
    {
        Environment.CurrentDirectory = Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

        Assert.Multiple(() =>
        {
            Parallel.For(0L, Tests.Length, (i, _) =>
            {
                string testFile = Tests[i].TestFile;

                bool exists = File.Exists(testFile);
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    return;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError)
                    return;

                Md5Context ctx;

                if(image.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
                {
                    foreach(bool @long in new[] { false, true })
                    {
                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors     = currentTrack.EndSector - currentTrack.StartSector + 1;
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= SECTORS_TO_READ)
                                {
                                    errno = @long
                                                ? image.ReadSectorsLong(doneSectors, SECTORS_TO_READ,
                                                                        currentTrack.Sequence, out sector)
                                                : image.ReadSectors(doneSectors, SECTORS_TO_READ, currentTrack.Sequence,
                                                                    out sector);

                                    doneSectors += SECTORS_TO_READ;
                                }
                                else
                                {
                                    errno = @long
                                                ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                        currentTrack.Sequence, out sector)
                                                : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                    currentTrack.Sequence, out sector);

                                    doneSectors += sectors - doneSectors;
                                }

                                Assert.AreEqual(ErrorNumber.NoError, errno);

                                ctx.Update(sector);
                            }
                        }

                        Assert.AreEqual(@long ? Tests[i].LongMd5 : Tests[i].Md5, ctx.End(),
                                        $"{(@long ? "Long hash" : "Hash")}: {testFile}");
                    }

                    if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        return;

                    ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors     = currentTrack.EndSector - currentTrack.StartSector + 1;
                        ulong doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                errno = image.ReadSectorsTag(doneSectors, SECTORS_TO_READ, currentTrack.Sequence,
                                                             SectorTagType.CdSectorSubchannel, out sector);

                                doneSectors += SECTORS_TO_READ;
                            }
                            else
                            {
                                errno = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                             currentTrack.Sequence, SectorTagType.CdSectorSubchannel,
                                                             out sector);

                                doneSectors += sectors - doneSectors;
                            }

                            Assert.AreEqual(ErrorNumber.NoError, errno);
                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(Tests[i].SubchannelMd5, ctx.End(),
                                    string.Format(Localization.Subchannel_hash_0, testFile));
                }
                else
                {
                    ctx = new Md5Context();
                    ulong doneSectors = 0;

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
                            errno = image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors),
                                                      out sector);

                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        Assert.AreEqual(ErrorNumber.NoError, errno);
                        ctx.Update(sector);
                    }

                    Assert.AreEqual(Tests[i].Md5, ctx.End(), string.Format(Localization.Hash_0, testFile));
                }
            });
        });
    }
}