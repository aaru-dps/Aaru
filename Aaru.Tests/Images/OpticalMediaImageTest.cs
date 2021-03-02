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
    public abstract class OpticalMediaImageTest : BlockMediaImageTest
    {
        const           uint     SECTORS_TO_READ = 256;
        public abstract string[] _longMd5S { get; }

        public abstract string[] _subchannelMd5S { get; }

        public abstract int[] _tracks { get; }

        public abstract int[][] _trackSessions { get; }

        public abstract ulong[][] _trackStarts { get; }

        public abstract ulong[][] _trackEnds { get; }

        public abstract ulong[][] _trackPregaps { get; }

        public abstract byte[][] _trackFlags { get; }

        [Test]
        public new void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(_testFiles[i]);
                    filter.Open(_testFiles[i]);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IOpticalMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {_testFiles[i]}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");

                            if(_sectorSize != null)
                                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");

                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");

                            if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                                return;

                            Assert.AreEqual(_tracks[i], image.Tracks.Count, $"Tracks: {_testFiles[i]}");

                            image.Tracks.Select(t => t.TrackSession).Should().
                                  BeEquivalentTo(_trackSessions[i], $"Track session: {_testFiles[i]}");

                            image.Tracks.Select(t => t.TrackStartSector).Should().
                                  BeEquivalentTo(_trackStarts[i], $"Track start: {_testFiles[i]}");

                            image.Tracks.Select(t => t.TrackEndSector).Should().
                                  BeEquivalentTo(_trackEnds[i], $"Track end: {_testFiles[i]}");

                            image.Tracks.Select(t => t.TrackPregap).Should().
                                  BeEquivalentTo(_trackPregaps[i], $"Track pregap: {_testFiles[i]}");

                            int trackNo = 0;

                            byte[] flags = new byte[image.Tracks.Count];

                            foreach(Track currentTrack in image.Tracks)
                            {
                                if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                    flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                         SectorTagType.CdTrackFlags)[0];

                                trackNo++;
                            }

                            flags.Should().BeEquivalentTo(_trackFlags[i], $"Track flags: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        [Test]
        public new void Hashes()
        {
            Environment.CurrentDirectory = Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                Parallel.For(0L, _testFiles.Length, (i, state) =>
                {
                    var     filtersList = new FiltersList();
                    IFilter filter      = filtersList.GetFilter(_testFiles[i]);
                    filter.Open(_testFiles[i]);

                    var image = Activator.CreateInstance(_plugin.GetType()) as IOpticalMediaImage;
                    Assert.NotNull(image, $"Could not instantiate filesystem for {_testFiles[i]}");

                    bool opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

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

                            Assert.AreEqual(@long ? _longMd5S[i] : _md5S[i], ctx.End(),
                                            $"{(@long ? "Long hash" : "Hash")}: {_testFiles[i]}");
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

                        Assert.AreEqual(_subchannelMd5S[i], ctx.End(), $"Subchannel hash: {_testFiles[i]}");
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

                        Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                    }
                });
            });
        }
    }
}