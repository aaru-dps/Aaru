namespace Aaru.Tests.Images;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core;
using Aaru.Tests.Filesystems;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

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
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors, $"Sectors: {testFile}");

                        if(test.SectorSize > 0)
                            Assert.AreEqual(test.SectorSize, image.Info.SectorSize, $"Sector size: {testFile}");

                        Assert.AreEqual(test.MediaType, image.Info.MediaType, $"Media type: {testFile}");

                        if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                            return;

                        Assert.AreEqual(test.Tracks.Length, image.Tracks.Count, $"Tracks: {testFile}");

                        image.Tracks.Select(t => t.Session).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Session), $"Track session: {testFile}");

                        image.Tracks.Select(t => t.StartSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Start), $"Track start: {testFile}");

                        image.Tracks.Select(t => t.EndSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.End), $"Track end: {testFile}");

                        image.Tracks.Select(t => t.Pregap).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Pregap), $"Track pregap: {testFile}");

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

                        flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags), $"Track flags: {testFile}");

                        Assert.AreEqual(latestEndSector, image.Info.Sectors - 1,
                                        $"Last sector for tracks is {latestEndSector}, but it is {image.Info.Sectors} for image");
                    });
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
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    continue;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                    Assert.Multiple(() =>
                    {
                        foreach(TrackInfoTestExpected track in test.Tracks)
                        {
                            if(track.FileSystems is null)
                                continue;

                            ulong trackStart = track.Start + track.Pregap;

                            if(track.Number == 1 &&
                               track.Pregap >= 150)
                                trackStart -= 150;

                            var partition = new Partition
                            {
                                Length = track.End - trackStart + 1,
                                Start  = trackStart
                            };

                            Filesystems.Identify(image, out List<string> idPlugins, partition);

                            Assert.AreEqual(track.FileSystems.Length, idPlugins.Count,
                                            $"Expected {track.FileSystems.Length} filesystems in {testFile} but found {idPlugins.Count}");

                            for(var i = 0; i < track.FileSystems.Length; i++)
                            {
                                PluginBase plugins = GetPluginBase.Instance;
                                bool found = plugins.PluginsList.TryGetValue(idPlugins[i], out IFilesystem plugin);

                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                // It is not the case, it changes
                                if(!found)
                                    continue;

                                var fs = Activator.CreateInstance(plugin.GetType()) as IFilesystem;

                                Assert.NotNull(fs, $"Could not instantiate filesystem for {testFile}");

                                fs.GetInformation(image, partition, out _, null);

                                if(track.FileSystems[i].ApplicationId != null)
                                    Assert.AreEqual(track.FileSystems[i].ApplicationId,
                                                    fs.XmlFsType.ApplicationIdentifier, $"Application ID: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].Bootable, fs.XmlFsType.Bootable,
                                                $"Bootable: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].Clusters, fs.XmlFsType.Clusters,
                                                $"Clusters: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].ClusterSize, fs.XmlFsType.ClusterSize,
                                                $"Cluster size: {testFile}");

                                if(track.FileSystems[i].SystemId != null)
                                    Assert.AreEqual(track.FileSystems[i].SystemId, fs.XmlFsType.SystemIdentifier,
                                                    $"System ID: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].Type, fs.XmlFsType.Type,
                                                $"Filesystem type: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].VolumeName, fs.XmlFsType.VolumeName,
                                                $"Volume name: {testFile}");

                                Assert.AreEqual(track.FileSystems[i].VolumeSerial, fs.XmlFsType.VolumeSerial,
                                                $"Volume serial: {testFile}");

                                var rofs = Activator.CreateInstance(plugin.GetType()) as IReadOnlyFilesystem;

                                if(rofs == null)
                                {
                                    if(track.FileSystems[i].Contents     != null ||
                                       track.FileSystems[i].ContentsJson != null ||
                                       File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json"))
                                        Assert.NotNull(rofs,
                                                       $"Could not instantiate filesystem for {testFile}, track {track.Number}, filesystem {i}");

                                    continue;
                                }

                                track.FileSystems[i].Encoding ??= Encoding.ASCII;

                                ErrorNumber ret = rofs.Mount(image, partition, track.FileSystems[i].Encoding, null,
                                                             track.FileSystems[i].Namespace);

                                Assert.AreEqual(ErrorNumber.NoError, ret, $"Unmountable: {testFile}");

                                var serializer = new JsonSerializer
                                {
                                    Formatting        = Formatting.Indented,
                                    MaxDepth          = 16384,
                                    NullValueHandling = NullValueHandling.Ignore
                                };

                                serializer.Converters.Add(new StringEnumConverter());

                                if(track.FileSystems[i].ContentsJson != null)
                                    track.FileSystems[i].Contents =
                                        serializer.
                                            Deserialize<
                                                Dictionary<string, FileData>>(new JsonTextReader(new StringReader(track.
                                                                                  FileSystems[i].
                                                                                  ContentsJson)));
                                else if(File.Exists($"{testFile}.track{track.Number}.filesystem{i}.contents.json"))
                                {
                                    var sr =
                                        new StreamReader($"{testFile}.track{track.Number}.filesystem{i}.contents.json");

                                    track.FileSystems[i].Contents =
                                        serializer.Deserialize<Dictionary<string, FileData>>(new JsonTextReader(sr));
                                }

                                if(track.FileSystems[i].Contents is null)
                                    continue;

                                ReadOnlyFilesystemTest.TestDirectory(rofs, "/", track.FileSystems[i].Contents, testFile,
                                                                     false);

                                // Uncomment to generate JSON file
                                /*    var contents = ReadOnlyFilesystemTest.BuildDirectory(rofs, "/");

                                    var sw = new StreamWriter($"{testFile}.track{track.Number}.filesystem{i}.contents.json");
                                    serializer.Serialize(sw, contents);
                                    sw.Close();*/
                            }
                        }
                    });
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
            Parallel.For(0L, Tests.Length, (i, state) =>
            {
                string testFile = Tests[i].TestFile;

                bool exists = File.Exists(testFile);
                Assert.True(exists, $"{testFile} not found");

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists)
                    return;

                var     filtersList = new FiltersList();
                IFilter filter      = filtersList.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(Plugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    return;

                Md5Context ctx;

                if(image.Info.XmlMediaType == XmlMediaType.OpticalDisc)
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
                                    errno = @long ? image.ReadSectorsLong(doneSectors, SECTORS_TO_READ,
                                                                          currentTrack.Sequence, out sector)
                                                : image.ReadSectors(doneSectors, SECTORS_TO_READ, currentTrack.Sequence,
                                                                    out sector);

                                    doneSectors += SECTORS_TO_READ;
                                }
                                else
                                {
                                    errno = @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
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

                    Assert.AreEqual(Tests[i].SubchannelMd5, ctx.End(), $"Subchannel hash: {testFile}");
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

                    Assert.AreEqual(Tests[i].Md5, ctx.End(), $"Hash: {testFile}");
                }
            });
        });
    }
}