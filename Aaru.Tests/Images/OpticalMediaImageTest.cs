using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        using(new AssertionScope())
        {
            foreach(OpticalImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                exists.Should().BeTrue(Localization._0_not_found, testFile);

                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                object instance = Activator.CreateInstance(Plugin.GetType());

                (instance is IOpticalMediaImage).Should()
                                                .BeTrue(Localization.Could_not_instantiate_filesystem_for_0, testFile);

                if(instance is not IOpticalMediaImage image) continue;

                ErrorNumber opened = image.Open(filter);
                opened.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    image.Info.Sectors.Should().Be(test.Sectors, string.Format(Localization.Sectors_0, testFile));

                    if(test.SectorSize > 0)
                    {
                        image.Info.SectorSize.Should()
                             .Be(test.SectorSize, string.Format(Localization.Sector_size_0, testFile));
                    }

                    image.Info.MediaType.Should()
                         .Be(test.MediaType, string.Format(Localization.Media_type_0, testFile));

                    if(image.Info.MetadataMediaType != MetadataMediaType.OpticalDisc) return;

                    image.Tracks.Should().HaveCount(test.Tracks.Length, string.Format(Localization.Tracks_0, testFile));

                    image.Tracks.Select(static t => t.Session)
                         .Should()
                         .BeEquivalentTo(test.Tracks.Select(static s => s.Session),
                                         string.Format(Localization.Track_session_0, testFile));

                    image.Tracks.Select(static t => t.StartSector)
                         .Should()
                         .BeEquivalentTo(test.Tracks.Select(static s => s.Start),
                                         string.Format(Localization.Track_start_0, testFile));

                    image.Tracks.Select(static t => t.EndSector)
                         .Should()
                         .BeEquivalentTo(test.Tracks.Select(static s => s.End),
                                         string.Format(Localization.Track_end_0, testFile));

                    image.Tracks.Select(static t => t.Pregap)
                         .Should()
                         .BeEquivalentTo(test.Tracks.Select(static s => s.Pregap),
                                         string.Format(Localization.Track_pregap_0, testFile));

                    var trackNo = 0;

                    var   flags           = new byte?[image.Tracks.Count];
                    ulong latestEndSector = 0;

                    foreach(Track currentTrack in image.Tracks)
                    {
                        if(currentTrack.EndSector > latestEndSector) latestEndSector = currentTrack.EndSector;

                        if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                        {
                            ErrorNumber errno = image.ReadSectorTag(currentTrack.Sequence,
                                                                    false,
                                                                    SectorTagType.CdTrackFlags,
                                                                    out byte[] tmp);

                            if(errno != ErrorNumber.NoError) continue;

                            flags[trackNo] = tmp[0];
                        }

                        trackNo++;
                    }

                    flags.Should()
                         .BeEquivalentTo(test.Tracks.Select(static s => s.Flags),
                                         string.Format(Localization.Track_flags_0, testFile));

                    (image.Info.Sectors - 1).Should()
                                            .Be(latestEndSector,
                                                string.Format(Localization
                                                                 .Last_sector_for_tracks_is_0_but_it_is_1_for_image,
                                                              latestEndSector,
                                                              image.Info.Sectors));
                }
            }
        }
    }

    [Test]
    public void Contents()
    {
        Environment.CurrentDirectory = DataFolder;

        using(new AssertionScope())
        {
            foreach(OpticalImageTestExpected test in Tests)
            {
                string testFile = test.TestFile;

                bool exists = File.Exists(testFile);
                exists.Should().BeTrue(Localization._0_not_found, testFile);

                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                object instance = Activator.CreateInstance(Plugin.GetType());

                (instance is IOpticalMediaImage).Should()
                                                .BeTrue(Localization.Could_not_instantiate_filesystem_for_0, testFile);

                if(instance is not IOpticalMediaImage image) continue;

                ErrorNumber opened = image.Open(filter);
                opened.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
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

                        idPlugins.Should()
                                 .HaveCount(track.FileSystems.Length,
                                            string.Format(Localization.Expected_0_filesystems_in_1_but_found_2,
                                                          track.FileSystems.Length,
                                                          testFile,
                                                          idPlugins.Count));

                        for(var i = 0; i < track.FileSystems.Length; i++)
                        {
                            PluginRegister plugins = PluginRegister.Singleton;
                            bool           found   = plugins.Filesystems.TryGetValue(idPlugins[i], out IFilesystem fs);

                            if(!found) continue;

                            fs.Should().NotBeNull(Localization.Could_not_instantiate_filesystem_for_0, testFile);

                            fs.GetInformation(image, partition, null, out _, out FileSystem fsMetadata);

                            if(track.FileSystems[i].ApplicationId != null)
                            {
                                fsMetadata.ApplicationIdentifier.Should()
                                          .Be(track.FileSystems[i].ApplicationId,
                                              string.Format(Localization.Application_ID_0, testFile));
                            }

                            fsMetadata.Bootable.Should()
                                      .Be(track.FileSystems[i].Bootable,
                                          string.Format(Localization.Bootable_0, testFile));

                            fsMetadata.Clusters.Should()
                                      .Be((ulong)track.FileSystems[i].Clusters,
                                          string.Format(Localization.Clusters_0, testFile));

                            fsMetadata.ClusterSize.Should()
                                      .Be(track.FileSystems[i].ClusterSize,
                                          string.Format(Localization.Cluster_size_0, testFile));

                            if(track.FileSystems[i].SystemId != null)
                            {
                                fsMetadata.SystemIdentifier.Should()
                                          .Be(track.FileSystems[i].SystemId,
                                              string.Format(Localization.System_ID_0, testFile));
                            }

                            fsMetadata.Type.Should()
                                      .Be(track.FileSystems[i].Type,
                                          string.Format(Localization.Filesystem_type_0, testFile));

                            fsMetadata.VolumeName.Should()
                                      .Be(track.FileSystems[i].VolumeName,
                                          string.Format(Localization.Volume_name_0, testFile));

                            fsMetadata.VolumeSerial.Should()
                                      .Be(track.FileSystems[i].VolumeSerial,
                                          string.Format(Localization.Volume_serial_0, testFile));

                            if(fs is not IReadOnlyFilesystem rofs)
                            {
                                (track.FileSystems[i].Contents     == null                                   &&
                                 track.FileSystems[i].ContentsJson == null                                   &&
                                 !File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json") &&
                                 !File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json.gz"))
                                   .Should()
                                   .BeTrue(Localization.Could_not_instantiate_filesystem_for_0_track_1_filesystem_2,
                                           testFile,
                                           track.Number,
                                           i);


                                continue;
                            }

                            track.FileSystems[i].Encoding ??= Encoding.ASCII;

                            ErrorNumber ret = rofs.Mount(image,
                                                         partition,
                                                         track.FileSystems[i].Encoding,
                                                         null,
                                                         track.FileSystems[i].Namespace);

                            ret.Should().Be(ErrorNumber.NoError, string.Format(Localization.Unmountable_0, testFile));

                            if(ret != ErrorNumber.NoError) continue;

                            string sidecarBase = $"{testFile}.track{track.Number}.filesystem{i}";

                            if(Environment.GetEnvironmentVariable("AARU_TESTS_BUILD") == "1")
                            {
                                Dictionary<string, FileData> built =
                                    ReadOnlyFilesystemTest.BuildDirectory(rofs, "/", 0);

                                using(var sw = new GZipStream(new FileStream($"{sidecarBase}.contents.json.gz",
                                                                             FileMode.Create),
                                                              CompressionLevel.SmallestSize))
                                {
                                    JsonSerializer.Serialize(sw,
                                                             built,
                                                             ReadOnlyFilesystemTest.ContentsSerializerOptions);
                                }

                                File.WriteAllText($"{sidecarBase}.contents.digest.json",
                                                  JsonSerializer.Serialize(ContentsDigest.Compute(built).ToFile(),
                                                                           FilesystemTest.InfoSerializerOptions));

                                continue;
                            }

                            // Fast path: compare one aggregate digest instead of walking the JSON expectations
                            var    verifyFileContents = true;
                            string digestPath         = $"{sidecarBase}.contents.digest.json";

                            if(Environment.GetEnvironmentVariable("AARU_TESTS_DEEP") != "1" && File.Exists(digestPath))
                            {
                                var expectedDigest =
                                    JsonSerializer.Deserialize<ContentsDigestFile>(File.ReadAllText(digestPath),
                                        FilesystemTest.InfoSerializerOptions);

                                if(expectedDigest?.Version == ContentsDigest.Version)
                                {
                                    ContentsDigestResult actual =
                                        ContentsDigest.Compute(ReadOnlyFilesystemTest.BuildDirectory(rofs, "/", 0));

                                    if(actual.Digest          == expectedDigest.Digest &&
                                       actual.TimestampDigest == expectedDigest.TimestampDigest)
                                        continue;

                                    verifyFileContents = actual.Digest != expectedDigest.Digest;

                                    // A stale primary digest must not pass silently even when the
                                    // per-entry walk below finds nothing
                                    if(verifyFileContents)
                                    {
                                        actual.Digest.Should()
                                              .Be(expectedDigest.Digest,
                                                  "contents digest for {0} mismatched; if the per-entry differences below are intended, regenerate the sidecar files with AARU_TESTS_BUILD=1",
                                                  testFile);
                                    }
                                }
                            }

                            track.FileSystems[i].Contents ??=
                                ReadOnlyFilesystemTest.LoadContentsFile(track.FileSystems[i].ContentsJson,
                                                                        $"{sidecarBase}.contents.json");

                            if(track.FileSystems[i].Contents is null) continue;

                            var currentDepth = 0;

                            ReadOnlyFilesystemTest.TestDirectory(rofs,
                                                                 "/",
                                                                 track.FileSystems[i].Contents,
                                                                 testFile,
                                                                 true,
                                                                 out List<ReadOnlyFilesystemTest.NextLevel>
                                                                         currentLevel,
                                                                 currentDepth,
                                                                 verifyFileContents);

                            while(currentLevel.Count > 0)
                            {
                                currentDepth++;
                                List<ReadOnlyFilesystemTest.NextLevel> nextLevels = [];

                                foreach(ReadOnlyFilesystemTest.NextLevel subLevel in currentLevel)
                                {
                                    ReadOnlyFilesystemTest.TestDirectory(rofs,
                                                                         subLevel.Path,
                                                                         subLevel.Children,
                                                                         testFile,
                                                                         true,
                                                                         out List<ReadOnlyFilesystemTest.NextLevel>
                                                                                 nextLevel,
                                                                         currentDepth,
                                                                         verifyFileContents);

                                    nextLevels.AddRange(nextLevel);
                                }

                                currentLevel = nextLevels;
                            }
                        }
                    }
                }
            }
        }
    }

    [Test]
    public void Hashes()
    {
        Environment.CurrentDirectory = Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

        using(new AssertionScope())
        {
            Parallel.For(0L,
                         Tests.Length,
                         (i, _) =>
                         {
                             string testFile = Tests[i].TestFile;

                             bool exists = File.Exists(testFile);
                             exists.Should().BeTrue(Localization._0_not_found, testFile);

                             if(!exists) return;

                             IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                             filter.Open(testFile);

                             object instance = Activator.CreateInstance(Plugin.GetType());

                             (instance is IOpticalMediaImage).Should()
                                                             .BeTrue(Localization
                                                                        .Could_not_instantiate_filesystem_for_0,
                                                                     testFile);

                             if(instance is not IOpticalMediaImage image) return;

                             ErrorNumber opened = image.Open(filter);

                             opened.Should().Be(ErrorNumber.NoError, string.Format(Localization.Open_0, testFile));

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
                                                                 out sector,
                                                                 out SectorStatus[] _)
                                                             : image.ReadSectors(doneSectors,
                                                                 SECTORS_TO_READ,
                                                                 currentTrack.Sequence,
                                                                 out sector,
                                                                 out SectorStatus[] _);

                                                 doneSectors += SECTORS_TO_READ;
                                             }
                                             else
                                             {
                                                 errno = @long
                                                             ? image.ReadSectorsLong(doneSectors,
                                                                 (uint)(sectors - doneSectors),
                                                                 currentTrack.Sequence,
                                                                 out sector,
                                                                 out SectorStatus[] _)
                                                             : image.ReadSectors(doneSectors,
                                                                 (uint)(sectors - doneSectors),
                                                                 currentTrack.Sequence,
                                                                 out sector,
                                                                 out SectorStatus[] _);

                                                 doneSectors += sectors - doneSectors;
                                             }

                                             errno.Should().Be(ErrorNumber.NoError);

                                             ctx.Update(sector);
                                         }
                                     }

                                     ctx.End()
                                        .Should()
                                        .Be(@long ? Tests[i].LongMd5 : Tests[i].Md5,
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

                                         errno.Should().Be(ErrorNumber.NoError);
                                         ctx.Update(sector);
                                     }
                                 }

                                 ctx.End()
                                    .Should()
                                    .Be(Tests[i].SubchannelMd5,
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
                                         errno = image.ReadSectors(doneSectors,
                                                                   false,
                                                                   SECTORS_TO_READ,
                                                                   out sector,
                                                                   out SectorStatus[] _);

                                         doneSectors += SECTORS_TO_READ;
                                     }
                                     else
                                     {
                                         errno = image.ReadSectors(doneSectors,
                                                                   false,
                                                                   (uint)(image.Info.Sectors - doneSectors),
                                                                   out sector,
                                                                   out SectorStatus[] _);

                                         doneSectors += image.Info.Sectors - doneSectors;
                                     }

                                     errno.Should().Be(ErrorNumber.NoError);
                                     ctx.Update(sector);
                                 }

                                 ctx.End().Should().Be(Tests[i].Md5, string.Format(Localization.Hash_0, testFile));
                             }
                         });
        }
    }
}