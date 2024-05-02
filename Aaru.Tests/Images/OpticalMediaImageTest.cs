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

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();


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
                Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;

                Assert.That(image,
                            Is.Not.Null,
                            string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.That(opened, Is.EqualTo(ErrorNumber.NoError), string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(image.Info.Sectors,
                                    Is.EqualTo(test.Sectors),
                                    string.Format(Localization.Sectors_0, testFile));

                        if(test.SectorSize > 0)
                        {
                            Assert.That(image.Info.SectorSize,
                                        Is.EqualTo(test.SectorSize),
                                        string.Format(Localization.Sector_size_0, testFile));
                        }

                        Assert.That(image.Info.MediaType,
                                    Is.EqualTo(test.MediaType),
                                    string.Format(Localization.Media_type_0, testFile));

                        if(image.Info.MetadataMediaType != MetadataMediaType.OpticalDisc) return;

                        Assert.That(image.Tracks,
                                    Has.Count.EqualTo(test.Tracks.Length),
                                    string.Format(Localization.Tracks_0, testFile));

                        image.Tracks.Select(t => t.Session)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Session),
                                             string.Format(Localization.Track_session_0, testFile));

                        image.Tracks.Select(t => t.StartSector)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Start),
                                             string.Format(Localization.Track_start_0, testFile));

                        image.Tracks.Select(t => t.EndSector)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.End),
                                             string.Format(Localization.Track_end_0, testFile));

                        image.Tracks.Select(t => t.Pregap)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Pregap),
                                             string.Format(Localization.Track_pregap_0, testFile));

                        var trackNo = 0;

                        var   flags           = new byte?[image.Tracks.Count];
                        ulong latestEndSector = 0;

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(currentTrack.EndSector > latestEndSector) latestEndSector = currentTrack.EndSector;

                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                            {
                                ErrorNumber errno =
                                    image.ReadSectorTag(currentTrack.Sequence,
                                                        SectorTagType.CdTrackFlags,
                                                        out byte[] tmp);

                                if(errno != ErrorNumber.NoError) continue;

                                flags[trackNo] = tmp[0];
                            }

                            trackNo++;
                        }

                        flags.Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                             string.Format(Localization.Track_flags_0, testFile));

                        Assert.That(image.Info.Sectors - 1,
                                    Is.EqualTo(latestEndSector),
                                    string.Format(Localization.Last_sector_for_tracks_is_0_but_it_is_1_for_image,
                                                  latestEndSector,
                                                  image.Info.Sectors));
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
                Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;

                Assert.That(image,
                            Is.Not.Null,
                            string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.That(opened, Is.EqualTo(ErrorNumber.NoError), string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        foreach(TrackInfoTestExpected track in test.Tracks)
                        {
                            if(track.FileSystems is null) continue;

                            ulong trackStart = track.Start + track.Pregap;

                            if(track.Number <= 1 && track.Pregap >= 150) trackStart -= 150;

                            var partition = new Partition
                            {
                                Length = track.End - trackStart + 1,
                                Start  = trackStart
                            };

                            Core.Filesystems.Identify(image, out List<string> idPlugins, partition);

                            Assert.That(idPlugins,
                                        Has.Count.EqualTo(track.FileSystems.Length),
                                        string.Format(Localization.Expected_0_filesystems_in_1_but_found_2,
                                                      track.FileSystems.Length,
                                                      testFile,
                                                      idPlugins.Count));

                            for(var i = 0; i < track.FileSystems.Length; i++)
                            {
                                PluginRegister plugins = PluginRegister.Singleton;
                                bool found = plugins.Filesystems.TryGetValue(idPlugins[i], out IFilesystem fs);

                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                // It is not the case, it changes
                                if(!found) continue;

                                Assert.That(fs,
                                            Is.Not.Null,
                                            string.Format(Localization.Could_not_instantiate_filesystem_for_0,
                                                          testFile));

                                fs.GetInformation(image, partition, null, out _, out FileSystem fsMetadata);

                                if(track.FileSystems[i].ApplicationId != null)
                                {
                                    Assert.That(fsMetadata.ApplicationIdentifier,
                                                Is.EqualTo(track.FileSystems[i].ApplicationId),
                                                string.Format(Localization.Application_ID_0, testFile));
                                }

                                Assert.That(fsMetadata.Bootable,
                                            Is.EqualTo(track.FileSystems[i].Bootable),
                                            string.Format(Localization.Bootable_0, testFile));

                                Assert.That(fsMetadata.Clusters,
                                            Is.EqualTo(track.FileSystems[i].Clusters),
                                            string.Format(Localization.Clusters_0, testFile));

                                Assert.That(fsMetadata.ClusterSize,
                                            Is.EqualTo(track.FileSystems[i].ClusterSize),
                                            string.Format(Localization.Cluster_size_0, testFile));

                                if(track.FileSystems[i].SystemId != null)
                                {
                                    Assert.That(fsMetadata.SystemIdentifier,
                                                Is.EqualTo(track.FileSystems[i].SystemId),
                                                string.Format(Localization.System_ID_0, testFile));
                                }

                                Assert.That(fsMetadata.Type,
                                            Is.EqualTo(track.FileSystems[i].Type),
                                            string.Format(Localization.Filesystem_type_0, testFile));

                                Assert.That(fsMetadata.VolumeName,
                                            Is.EqualTo(track.FileSystems[i].VolumeName),
                                            string.Format(Localization.Volume_name_0, testFile));

                                Assert.That(fsMetadata.VolumeSerial,
                                            Is.EqualTo(track.FileSystems[i].VolumeSerial),
                                            string.Format(Localization.Volume_serial_0, testFile));

                                if(fs is not IReadOnlyFilesystem rofs)
                                {
                                    if(track.FileSystems[i].Contents     != null ||
                                       track.FileSystems[i].ContentsJson != null ||
                                       File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json"))
                                    {
                                        Assert.Fail(string.Format(Localization
                                                                     .Could_not_instantiate_filesystem_for_0_track_1_filesystem_2,
                                                                  testFile,
                                                                  track.Number,
                                                                  i));
                                    }

                                    continue;
                                }

                                track.FileSystems[i].Encoding ??= Encoding.ASCII;

                                ErrorNumber ret = rofs.Mount(image,
                                                             partition,
                                                             track.FileSystems[i].Encoding,
                                                             null,
                                                             track.FileSystems[i].Namespace);

                                Assert.That(ret,
                                            Is.EqualTo(ErrorNumber.NoError),
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
                                        JsonSerializer.Deserialize<Dictionary<string, FileData>>(track.FileSystems[i]
                                               .ContentsJson,
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

                                if(track.FileSystems[i].Contents is null) continue;

                                var currentDepth = 0;

                                ReadOnlyFilesystemTest.TestDirectory(rofs,
                                                                     "/",
                                                                     track.FileSystems[i].Contents,
                                                                     testFile,
                                                                     true,
                                                                     out List<ReadOnlyFilesystemTest.NextLevel>
                                                                             currentLevel,
                                                                     currentDepth);

                                while(currentLevel.Count > 0)
                                {
                                    currentDepth++;
                                    List<ReadOnlyFilesystemTest.NextLevel> nextLevels = new();

                                    foreach(ReadOnlyFilesystemTest.NextLevel subLevel in currentLevel)
                                    {
                                        ReadOnlyFilesystemTest.TestDirectory(rofs,
                                                                             subLevel.Path,
                                                                             subLevel.Children,
                                                                             testFile,
                                                                             true,
                                                                             out List<ReadOnlyFilesystemTest.NextLevel>
                                                                                 nextLevel,
                                                                             currentDepth);

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
            Parallel.For(0L,
                         Tests.Length,
                         (i, _) =>
                         {
                             string testFile = Tests[i].TestFile;

                             bool exists = File.Exists(testFile);
                             Assert.That(exists, string.Format(Localization._0_not_found, testFile));

                             // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                             // It arrives here...
                             if(!exists) return;

                             IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                             filter.Open(testFile);

                             var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;

                             Assert.That(image,
                                         Is.Not.Null,
                                         string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                             ErrorNumber opened = image.Open(filter);

                             Assert.That(opened,
                                         Is.EqualTo(ErrorNumber.NoError),
                                         string.Format(Localization.Open_0, testFile));

                             if(opened != ErrorNumber.NoError) return;

                             Md5Context ctx;

                             if(image.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
                             {
                                 foreach(bool @long in new[]
                                         {
                                             false, true
                                         })
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
                                                             ? image.ReadSectorsLong(doneSectors,
                                                                 SECTORS_TO_READ,
                                                                 currentTrack.Sequence,
                                                                 out sector)
                                                             : image.ReadSectors(doneSectors,
                                                                 SECTORS_TO_READ,
                                                                 currentTrack.Sequence,
                                                                 out sector);

                                                 doneSectors += SECTORS_TO_READ;
                                             }
                                             else
                                             {
                                                 errno = @long
                                                             ? image.ReadSectorsLong(doneSectors,
                                                                 (uint)(sectors - doneSectors),
                                                                 currentTrack.Sequence,
                                                                 out sector)
                                                             : image.ReadSectors(doneSectors,
                                                                 (uint)(sectors - doneSectors),
                                                                 currentTrack.Sequence,
                                                                 out sector);

                                                 doneSectors += sectors - doneSectors;
                                             }

                                             Assert.That(errno, Is.EqualTo(ErrorNumber.NoError));

                                             ctx.Update(sector);
                                         }
                                     }

                                     Assert.That(ctx.End(),
                                                 Is.EqualTo(@long ? Tests[i].LongMd5 : Tests[i].Md5),
                                                 $"{(@long ? "Long hash" : "Hash")}: {testFile}");
                                 }

                                 if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel)) return;

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
                                             errno = image.ReadSectorsTag(doneSectors,
                                                                          SECTORS_TO_READ,
                                                                          currentTrack.Sequence,
                                                                          SectorTagType.CdSectorSubchannel,
                                                                          out sector);

                                             doneSectors += SECTORS_TO_READ;
                                         }
                                         else
                                         {
                                             errno = image.ReadSectorsTag(doneSectors,
                                                                          (uint)(sectors - doneSectors),
                                                                          currentTrack.Sequence,
                                                                          SectorTagType.CdSectorSubchannel,
                                                                          out sector);

                                             doneSectors += sectors - doneSectors;
                                         }

                                         Assert.That(errno, Is.EqualTo(ErrorNumber.NoError));
                                         ctx.Update(sector);
                                     }
                                 }

                                 Assert.That(ctx.End(),
                                             Is.EqualTo(Tests[i].SubchannelMd5),
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
                                         errno = image.ReadSectors(doneSectors,
                                                                   (uint)(image.Info.Sectors - doneSectors),
                                                                   out sector);

                                         doneSectors += image.Info.Sectors - doneSectors;
                                     }

                                     Assert.That(errno, Is.EqualTo(ErrorNumber.NoError));
                                     ctx.Update(sector);
                                 }

                                 Assert.That(ctx.End(),
                                             Is.EqualTo(Tests[i].Md5),
                                             string.Format(Localization.Hash_0, testFile));
                             }
                         });
        });
    }
}