using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Media;
using Aaru.Devices;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.WritableImages
{
    public abstract class WritableOpticalMediaImageTest : BaseWritableMediaImageTest
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

                    var image = Activator.CreateInstance(InputPlugin.GetType()) as IOpticalMediaImage;
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

                            if(test.SectorSize > 0)
                                Assert.AreEqual(test.SectorSize, image.Info.SectorSize, $"Sector size: {testFile}");

                            Assert.AreEqual(test.MediaType, image.Info.MediaType, $"Media type: {testFile}");

                            if(image.Info.XmlMediaType != XmlMediaType.OpticalDisc)
                                return;

                            Assert.AreEqual(test.Tracks.Length, image.Tracks.Count, $"Tracks: {testFile}");

                            image.Tracks.Select(t => t.TrackSession).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Session), $"Track session: {testFile}");

                            image.Tracks.Select(t => t.TrackStartSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Start), $"Track start: {testFile}");

                            image.Tracks.Select(t => t.TrackEndSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.End), $"Track end: {testFile}");

                            image.Tracks.Select(t => t.TrackPregap).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Pregap), $"Track pregap: {testFile}");

                            int trackNo = 0;

                            byte?[] flags           = new byte?[image.Tracks.Count];
                            ulong   latestEndSector = 0;

                            foreach(Track currentTrack in image.Tracks)
                            {
                                if(currentTrack.TrackEndSector > latestEndSector)
                                    latestEndSector = currentTrack.TrackEndSector;

                                if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                    flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                         SectorTagType.CdTrackFlags)[0];

                                trackNo++;
                            }

                            flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags), $"Track flags: {testFile}");

                            Assert.AreEqual(latestEndSector, image.Info.Sectors - 1,
                                            $"Last sector for tracks is {latestEndSector}, but it is {image.Info.Sectors} for image");
                        });
                    }
                }
            });
        }

        [Test]
        public void Convert()
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

                    var inputFormat = Activator.CreateInstance(InputPlugin.GetType()) as IOpticalMediaImage;
                    Assert.NotNull(inputFormat, $"Could not instantiate input plugin for {testFile}");

                    bool opened = inputFormat.Open(filter);
                    Assert.AreEqual(true, opened, $"Open: {testFile}");

                    if(!opened)
                        continue;

                    string outputPath =
                        Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{OutputExtension}");

                    var outputFormat = Activator.CreateInstance(OutputPlugin.GetType()) as IWritableOpticalImage;
                    Assert.NotNull(outputFormat, $"Could not instantiate output plugin for {testFile}");

                    Assert.IsTrue(outputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType),
                                  $"Trying to convert unsupported media type {inputFormat.Info.MediaType} for {testFile}");

                    bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

                    foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
                        !outputFormat.SupportedSectorTags.Contains(sectorTag)))
                    {
                        if(sectorTag != SectorTagType.CdTrackFlags &&
                           sectorTag != SectorTagType.CdTrackIsrc  &&
                           sectorTag != SectorTagType.CdSectorSubchannel)
                            useLong = false;
                    }

                    Assert.IsTrue(outputFormat.Create(outputPath, inputFormat.Info.MediaType, new Dictionary<string, string>(), inputFormat.Info.Sectors, inputFormat.Info.SectorSize),
                                  $"Error {outputFormat.ErrorMessage} creating output image.");

                    foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                        outputFormat.SupportedMediaTags.Contains(mediaTag)))
                        outputFormat.WriteMediaTag(inputFormat.ReadDiskTag(mediaTag), mediaTag);

                    Assert.IsTrue(outputFormat.SetTracks(inputFormat.Tracks),
                                  $"Error {outputFormat.ErrorMessage} sending tracks list to output image.");

                    ulong doneSectors;

                    foreach(Track track in inputFormat.Tracks)
                    {
                        doneSectors = 0;
                        ulong trackSectors = track.TrackEndSector - track.TrackStartSector + 1;

                        while(doneSectors < trackSectors)
                        {
                            byte[] sector;

                            uint sectorsToDo;

                            if(trackSectors - doneSectors >= SECTORS_TO_READ)
                                sectorsToDo = SECTORS_TO_READ;
                            else
                                sectorsToDo = (uint)(trackSectors - doneSectors);

                            bool useNotLong = false;
                            bool result     = false;

                            if(useLong)
                            {
                                if(sectorsToDo == 1)
                                {
                                    sector = inputFormat.ReadSectorLong(doneSectors           + track.TrackStartSector);
                                    result = outputFormat.WriteSectorLong(sector, doneSectors + track.TrackStartSector);
                                }
                                else
                                {
                                    sector = inputFormat.ReadSectorsLong(doneSectors + track.TrackStartSector,
                                                                         sectorsToDo);

                                    result = outputFormat.WriteSectorsLong(sector, doneSectors + track.TrackStartSector,
                                                                           sectorsToDo);
                                }

                                if(!result &&
                                   sector.Length % 2352 != 0)
                                    useNotLong = true;
                            }

                            if(!useLong || useNotLong)
                            {
                                if(sectorsToDo == 1)
                                {
                                    sector = inputFormat.ReadSector(doneSectors           + track.TrackStartSector);
                                    result = outputFormat.WriteSector(sector, doneSectors + track.TrackStartSector);
                                }
                                else
                                {
                                    sector = inputFormat.ReadSectors(doneSectors + track.TrackStartSector, sectorsToDo);

                                    result = outputFormat.WriteSectors(sector, doneSectors + track.TrackStartSector,
                                                                       sectorsToDo);
                                }
                            }

                            Assert.IsTrue(result,
                                          $"Error {outputFormat.ErrorMessage} writing sector {doneSectors + track.TrackStartSector}...");

                            doneSectors += sectorsToDo;
                        }
                    }

                    Dictionary<byte, string> isrcs                     = new Dictionary<byte, string>();
                    Dictionary<byte, byte>   trackFlags                = new Dictionary<byte, byte>();
                    string                   mcn                       = null;
                    HashSet<int>             subchannelExtents         = new HashSet<int>();
                    Dictionary<byte, int>    smallestPregapLbaPerTrack = new Dictionary<byte, int>();
                    Track[]                  tracks                    = new Track[inputFormat.Tracks.Count];

                    for(int i = 0; i < tracks.Length; i++)
                    {
                        tracks[i] = new Track
                        {
                            Indexes                = new Dictionary<ushort, int>(),
                            TrackDescription       = inputFormat.Tracks[i].TrackDescription,
                            TrackEndSector         = inputFormat.Tracks[i].TrackEndSector,
                            TrackStartSector       = inputFormat.Tracks[i].TrackStartSector,
                            TrackPregap            = inputFormat.Tracks[i].TrackPregap,
                            TrackSequence          = inputFormat.Tracks[i].TrackSequence,
                            TrackSession           = inputFormat.Tracks[i].TrackSession,
                            TrackBytesPerSector    = inputFormat.Tracks[i].TrackBytesPerSector,
                            TrackRawBytesPerSector = inputFormat.Tracks[i].TrackRawBytesPerSector,
                            TrackType              = inputFormat.Tracks[i].TrackType,
                            TrackSubchannelType    = inputFormat.Tracks[i].TrackSubchannelType
                        };

                        foreach(KeyValuePair<ushort, int> idx in inputFormat.Tracks[i].Indexes)
                            tracks[i].Indexes[idx.Key] = idx.Value;
                    }

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.
                                                             Where(t => t == SectorTagType.CdTrackIsrc).OrderBy(t => t))
                    {
                        foreach(Track track in tracks)
                        {
                            byte[] isrc = inputFormat.ReadSectorTag(track.TrackSequence, tag);

                            if(isrc is null)
                                continue;

                            isrcs[(byte)track.TrackSequence] = Encoding.UTF8.GetString(isrc);
                        }
                    }

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.
                                                             Where(t => t == SectorTagType.CdTrackFlags).
                                                             OrderBy(t => t))
                    {
                        foreach(Track track in tracks)
                        {
                            byte[] flags = inputFormat.ReadSectorTag(track.TrackSequence, tag);

                            if(flags is null)
                                continue;

                            trackFlags[(byte)track.TrackSequence] = flags[0];
                        }
                    }

                    for(ulong s = 0; s < inputFormat.Info.Sectors; s++)
                    {
                        if(s > int.MaxValue)
                            break;

                        subchannelExtents.Add((int)s);
                    }

                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t).
                                                             TakeWhile(tag => useLong))
                    {
                        switch(tag)
                        {
                            case SectorTagType.AppleSectorTag:
                            case SectorTagType.CdSectorSync:
                            case SectorTagType.CdSectorHeader:
                            case SectorTagType.CdSectorSubHeader:
                            case SectorTagType.CdSectorEdc:
                            case SectorTagType.CdSectorEccP:
                            case SectorTagType.CdSectorEccQ:
                            case SectorTagType.CdSectorEcc:
                                // This tags are inline in long sector
                                continue;
                        }

                        if(!outputFormat.SupportedSectorTags.Contains(tag))
                            continue;

                        foreach(Track track in inputFormat.Tracks)
                        {
                            doneSectors = 0;
                            ulong  trackSectors = track.TrackEndSector - track.TrackStartSector + 1;
                            byte[] sector;
                            bool   result;

                            switch(tag)
                            {
                                case SectorTagType.CdTrackFlags:
                                case SectorTagType.CdTrackIsrc:
                                    sector = inputFormat.ReadSectorTag(track.TrackSequence, tag);
                                    result = outputFormat.WriteSectorTag(sector, track.TrackSequence, tag);

                                    Assert.IsTrue(result,
                                                  $"Error {outputFormat.ErrorMessage} writing tag, not continuing...");

                                    continue;
                            }

                            while(doneSectors < trackSectors)
                            {
                                uint sectorsToDo;

                                if(trackSectors - doneSectors >= SECTORS_TO_READ)
                                    sectorsToDo = SECTORS_TO_READ;
                                else
                                    sectorsToDo = (uint)(trackSectors - doneSectors);

                                if(sectorsToDo == 1)
                                {
                                    sector = inputFormat.ReadSectorTag(doneSectors + track.TrackStartSector, tag);

                                    if(tag == SectorTagType.CdSectorSubchannel)
                                    {
                                        bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                            MmcSubchannel.Raw, sector, doneSectors + track.TrackStartSector, 1,
                                            null, isrcs, (byte)track.TrackSequence, ref mcn, tracks,
                                            subchannelExtents, true, outputFormat, true, true, null, null,
                                            smallestPregapLbaPerTrack, false, out _);

                                        if(indexesChanged)
                                            outputFormat.SetTracks(tracks.ToList());

                                        result = true;
                                    }
                                    else
                                        result =
                                            outputFormat.WriteSectorTag(sector, doneSectors + track.TrackStartSector,
                                                                        tag);
                                }
                                else
                                {
                                    sector = inputFormat.ReadSectorsTag(doneSectors + track.TrackStartSector,
                                                                        sectorsToDo, tag);

                                    if(tag == SectorTagType.CdSectorSubchannel)
                                    {
                                        bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                            MmcSubchannel.Raw, sector, doneSectors + track.TrackStartSector,
                                            sectorsToDo, null, isrcs, (byte)track.TrackSequence, ref mcn, tracks,
                                            subchannelExtents, true, outputFormat, true, true, null, null,
                                            smallestPregapLbaPerTrack, false, out _);

                                        if(indexesChanged)
                                            outputFormat.SetTracks(tracks.ToList());

                                        result = true;
                                    }
                                    else
                                        result =
                                            outputFormat.WriteSectorsTag(sector, doneSectors + track.TrackStartSector,
                                                                         sectorsToDo, tag);
                                }

                                Assert.IsTrue(result,
                                              $"Error {outputFormat.ErrorMessage} writing tag for sector {doneSectors + track.TrackStartSector}, not continuing...");

                                doneSectors += sectorsToDo;
                            }
                        }
                    }

                    if(isrcs.Count > 0)
                        foreach(KeyValuePair<byte, string> isrc in isrcs)
                            outputFormat.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value), isrc.Key,
                                                        SectorTagType.CdTrackIsrc);

                    if(trackFlags.Count > 0)
                        foreach((byte track, byte flags) in trackFlags)
                            outputFormat.WriteSectorTag(new[]
                            {
                                flags
                            }, track, SectorTagType.CdTrackFlags);

                    if(mcn != null)
                        outputFormat.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

                    // TODO: Progress
                    if(inputFormat.Info.MediaType == MediaType.CD            ||
                       inputFormat.Info.MediaType == MediaType.CDDA          ||
                       inputFormat.Info.MediaType == MediaType.CDG           ||
                       inputFormat.Info.MediaType == MediaType.CDEG          ||
                       inputFormat.Info.MediaType == MediaType.CDI           ||
                       inputFormat.Info.MediaType == MediaType.CDROM         ||
                       inputFormat.Info.MediaType == MediaType.CDROMXA       ||
                       inputFormat.Info.MediaType == MediaType.CDPLUS        ||
                       inputFormat.Info.MediaType == MediaType.CDMO          ||
                       inputFormat.Info.MediaType == MediaType.CDR           ||
                       inputFormat.Info.MediaType == MediaType.CDRW          ||
                       inputFormat.Info.MediaType == MediaType.CDMRW         ||
                       inputFormat.Info.MediaType == MediaType.VCD           ||
                       inputFormat.Info.MediaType == MediaType.SVCD          ||
                       inputFormat.Info.MediaType == MediaType.PCD           ||
                       inputFormat.Info.MediaType == MediaType.DTSCD         ||
                       inputFormat.Info.MediaType == MediaType.CDMIDI        ||
                       inputFormat.Info.MediaType == MediaType.CDV           ||
                       inputFormat.Info.MediaType == MediaType.CDIREADY      ||
                       inputFormat.Info.MediaType == MediaType.FMTOWNS       ||
                       inputFormat.Info.MediaType == MediaType.PS1CD         ||
                       inputFormat.Info.MediaType == MediaType.PS2CD         ||
                       inputFormat.Info.MediaType == MediaType.MEGACD        ||
                       inputFormat.Info.MediaType == MediaType.SATURNCD      ||
                       inputFormat.Info.MediaType == MediaType.GDROM         ||
                       inputFormat.Info.MediaType == MediaType.GDR           ||
                       inputFormat.Info.MediaType == MediaType.MilCD         ||
                       inputFormat.Info.MediaType == MediaType.SuperCDROM2   ||
                       inputFormat.Info.MediaType == MediaType.JaguarCD      ||
                       inputFormat.Info.MediaType == MediaType.ThreeDO       ||
                       inputFormat.Info.MediaType == MediaType.PCFX          ||
                       inputFormat.Info.MediaType == MediaType.NeoGeoCD      ||
                       inputFormat.Info.MediaType == MediaType.CDTV          ||
                       inputFormat.Info.MediaType == MediaType.CD32          ||
                       inputFormat.Info.MediaType == MediaType.Playdia       ||
                       inputFormat.Info.MediaType == MediaType.Pippin        ||
                       inputFormat.Info.MediaType == MediaType.VideoNow      ||
                       inputFormat.Info.MediaType == MediaType.VideoNowColor ||
                       inputFormat.Info.MediaType == MediaType.VideoNowXp    ||
                       inputFormat.Info.MediaType == MediaType.CVD)
                        CompactDisc.GenerateSubchannels(subchannelExtents, tracks, trackFlags, inputFormat.Info.Sectors,
                                                        null, null, null, null, null, outputFormat);

                    Assert.IsTrue(outputFormat.Close(),
                                  $"Error {outputFormat.ErrorMessage} closing output image... Contents are not correct.");

                    filtersList = new FiltersList();
                    filter      = filtersList.GetFilter(outputPath);
                    filter.Open(outputPath);

                    string tmpFolder = Path.GetDirectoryName(outputPath);
                    Environment.CurrentDirectory = tmpFolder;

                    var image = Activator.CreateInstance(OutputPlugin.GetType()) as IOpticalMediaImage;
                    Assert.NotNull(image, $"Could not instantiate output plugin for {testFile}");

                    opened = image.Open(filter);
                    Assert.AreEqual(true, opened, $"Open created: {testFile}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(test.Sectors, image.Info.Sectors, $"Sectors (output): {testFile}");

                            if(test.SectorSize > 0)
                                Assert.AreEqual(test.SectorSize, image.Info.SectorSize,
                                                $"Sector size (output): {testFile}");

                            Assert.AreEqual(test.Tracks.Length, image.Tracks.Count, $"Tracks (output): {testFile}");

                            image.Tracks.Select(t => t.TrackSession).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Session),
                                                 $"Track session (output): {testFile}");

                            image.Tracks.Select(t => t.TrackStartSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Start), $"Track start (output): {testFile}");

                            image.Tracks.Select(t => t.TrackEndSector).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.End), $"Track end (output): {testFile}");

                            image.Tracks.Select(t => t.TrackPregap).Should().
                                  BeEquivalentTo(test.Tracks.Select(s => s.Pregap),
                                                 $"Track pregap (output): {testFile}");

                            int trackNo = 0;

                            byte?[] flags           = new byte?[image.Tracks.Count];
                            ulong   latestEndSector = 0;

                            foreach(Track currentTrack in image.Tracks)
                            {
                                if(currentTrack.TrackEndSector > latestEndSector)
                                    latestEndSector = currentTrack.TrackEndSector;

                                if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                                    flags[trackNo] = image.ReadSectorTag(currentTrack.TrackSequence,
                                                                         SectorTagType.CdTrackFlags)[0];

                                trackNo++;
                            }

                            flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                                          $"Track flags (output): {testFile}");

                            Assert.AreEqual(latestEndSector, image.Info.Sectors - 1,
                                            $"Last sector for tracks is {latestEndSector}, but it is {image.Info.Sectors} for image (output)");
                        });
                    }

                    Md5Context ctx;

                    foreach(bool @long in new[]
                    {
                        /*false,*/ true
                    })
                    {
                        ctx = new Md5Context();

                        foreach(Track currentTrack in image.Tracks)
                        {
                            ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                            doneSectors = 0;

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

                        Assert.AreEqual(@long ? test.LongMD5 : test.MD5, ctx.End(),
                                        $"{(@long ? "Long hash (output)" : "Hash (output)")}: {testFile}");
                    }

                    if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        return;

                    ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors = currentTrack.TrackEndSector - currentTrack.TrackStartSector + 1;
                        doneSectors = 0;

                        while(doneSectors < sectors)
                        {
                            byte[] sector;

                            if(sectors - doneSectors >= SECTORS_TO_READ)
                            {
                                sector = image.ReadSectorsTag(doneSectors, SECTORS_TO_READ, currentTrack.TrackSequence,
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

                    Assert.AreEqual(test.SubchannelMD5, ctx.End(), $"Subchannel hash (output): {testFile}");
                }
            });
        }
    }
}