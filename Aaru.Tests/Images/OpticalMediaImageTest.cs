using System;
using System.Linq;
using System.Threading.Tasks;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
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
                    string  testFile    = test.TestFile;
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(testFile);
                    filter.Open(testFile);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IOpticalMediaImage;
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

                            if((test.SectorSize > 0) != null)
                                Assert.AreEqual(test.SectorSize, image.Info.SectorSize, $"Sector size: {testFile}");

                            Assert.AreEqual(test.MediaType, image.Info.MediaType, $"Media type: {testFile}");

                            if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                                return;

                            Assert.AreEqual(test.Tracks, image.Tracks.Count, $"Tracks: {testFile}");

                            image.Tracks.Select(t => t.TrackSession).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Session), $"Track session: {testFile}");

                            image.Tracks.Select(t => t.TrackStartSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Start), $"Track start: {testFile}");

                            image.Tracks.Select(t => t.TrackEndSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.End), $"Track end: {testFile}");

                            image.Tracks.Select(t => t.TrackPregap).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Pregap), $"Track pregap: {testFile}");

                            int trackNo = 0;

                            byte[] flags = new byte[image.Tracks.Count];

                            foreach(Track currentTrack in image.Tracks)
                            {
                                if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                    flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                         SectorTagType.CdTrackFlags)[0];

                                trackNo++;
                            }

                            flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags), $"Track flags: {testFile}");
                        });
                    }
                }
            });
        }

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = Environment.CurrentDirectory = DataFolder;

            Assert.Multiple(() =>
            {
                Parallel.For(0L, Tests.Length, (i, state) =>
                {
                    string  testFile    = Tests[i].TestFile;
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(testFile);
                    filter.Open(testFile);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IOpticalMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {testFile}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {testFile}");

                    if(!opened)
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
                                ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                                ulong doneSectors = 0;

                                while(doneSectors < sectors)
                                {
                                    byte[] sector;

                                    if(sectors - doneSectors >= SECTORS_TO_READ)
                                    {
                                        sector =
                                            @long ? image.ReadSectorsLong(doneSectors, SECTORS_TO_READ,
                                                                          currentTrack.TrackSequence)
                                                : image.ReadSectors(doneSectors, SECTORS_TO_READ,
                                                                    currentTrack.TrackSequence);

                                        doneSectors += SECTORS_TO_READ;
                                    }
                                    else
                                    {
                                        sector =
                                            @long ? image.ReadSectorsLong(doneSectors, (uint)(sectors - doneSectors),
                                                                          currentTrack.TrackSequence)
                                                : image.ReadSectors(doneSectors, (uint)(sectors - doneSectors),
                                                                    currentTrack.TrackSequence);

                                        doneSectors += sectors - doneSectors;
                                    }

                                    ctx.Update(sector);
                                }
                            }

                            Assert.AreEqual(@long ? Tests[i].LongMD5 : Tests[i].MD5, ctx.End(),
                                            $"{(@long ? "Long hash" : "Hash")}: {testFile}");
                        }

                        if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            return;

                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors     = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            ulong doneSectors = 0;

                            while(doneSectors < sectors)
                            {
                                byte[] sector;

                                if(sectors - doneSectors >= SECTORS_TO_READ)
                                {
                                    sector = image.ReadSectorsTag(doneSectors, SECTORS_TO_READ,
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                    doneSectors += SECTORS_TO_READ;
                                }
                                else
                                {
                                    sector = image.ReadSectorsTag(doneSectors, (uint)(sectors - doneSectors),
                                                                  currentTrack.TrackSequence,
                                                                  SectorTagType.CdSectorSubchannel);

                                    doneSectors += sectors - doneSectors;
                                }

                                ctx.Update(sector);
                            }
                        }

                        Assert.AreEqual(Tests[i].SubchannelMD5, ctx.End(), $"Subchannel hash: {testFile}");
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

                        Assert.AreEqual(Tests[i].MD5, ctx.End(), $"Hash: {testFile}");
                    }
                });
            });
        }
    }
}