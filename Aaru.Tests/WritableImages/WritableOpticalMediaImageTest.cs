namespace Aaru.Tests.WritableImages;

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

public abstract class WritableOpticalMediaImageTest : BaseWritableMediaImageTest
{
    const           uint                       SECTORS_TO_READ = 256;
    public abstract OpticalImageTestExpected[] Tests { get; }

    [Test]
    public void Info()
    {
        Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

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
                                errno = image.ReadSectorTag(currentTrack.Sequence, SectorTagType.CdTrackFlags,
                                                            out byte[] tmp);

                                if(errno != ErrorNumber.NoError)
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
    public void Convert()
    {
        Environment.CurrentDirectory = DataFolder;
        ErrorNumber errno;

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

                ErrorNumber opened = inputFormat.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                string outputPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{OutputExtension}");

                var outputFormat = Activator.CreateInstance(OutputPlugin.GetType()) as IWritableOpticalImage;
                Assert.NotNull(outputFormat, $"Could not instantiate output plugin for {testFile}");

                Assert.IsTrue(outputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType),
                              $"Trying to convert unsupported media type {inputFormat.Info.MediaType} for {testFile}");

                bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

                foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
                            !outputFormat.SupportedSectorTags.Contains(sectorTag)))
                    if(sectorTag != SectorTagType.CdTrackFlags &&
                       sectorTag != SectorTagType.CdTrackIsrc  &&
                       sectorTag != SectorTagType.CdSectorSubchannel)
                        useLong = false;

                Assert.IsTrue(outputFormat.Create(outputPath, inputFormat.Info.MediaType, new Dictionary<string, string>(), inputFormat.Info.Sectors, inputFormat.Info.SectorSize),
                              $"Error {outputFormat.ErrorMessage} creating output image.");

                foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                            outputFormat.SupportedMediaTags.Contains(mediaTag)))
                    if(inputFormat.ReadMediaTag(mediaTag, out byte[] buffer) == ErrorNumber.NoError)
                        outputFormat.WriteMediaTag(buffer, mediaTag);

                Assert.IsTrue(outputFormat.SetTracks(inputFormat.Tracks),
                              $"Error {outputFormat.ErrorMessage} sending tracks list to output image.");

                ulong doneSectors;

                foreach(Track track in inputFormat.Tracks)
                {
                    doneSectors = 0;
                    ulong trackSectors = track.EndSector - track.StartSector + 1;

                    while(doneSectors < trackSectors)
                    {
                        byte[] sector;

                        uint sectorsToDo;

                        if(trackSectors - doneSectors >= SECTORS_TO_READ)
                            sectorsToDo = SECTORS_TO_READ;
                        else
                            sectorsToDo = (uint)(trackSectors - doneSectors);

                        var useNotLong = false;
                        var result     = false;

                        if(useLong)
                        {
                            errno = sectorsToDo == 1
                                        ? inputFormat.ReadSectorLong(doneSectors + track.StartSector, out sector)
                                        : inputFormat.ReadSectorsLong(doneSectors + track.StartSector, sectorsToDo,
                                                                      out sector);

                            if(errno == ErrorNumber.NoError)
                                result = sectorsToDo == 1
                                             ? outputFormat.WriteSectorLong(sector, doneSectors + track.StartSector)
                                             : outputFormat.WriteSectorsLong(sector, doneSectors + track.StartSector,
                                                                             sectorsToDo);
                            else
                                result = false;

                            if(!result &&
                               sector.Length % 2352 != 0)
                                useNotLong = true;
                        }

                        if(!useLong || useNotLong)
                        {
                            errno = sectorsToDo == 1
                                        ? inputFormat.ReadSector(doneSectors + track.StartSector, out sector)
                                        : inputFormat.ReadSectors(doneSectors + track.StartSector, sectorsToDo,
                                                                  out sector);

                            Assert.AreEqual(ErrorNumber.NoError, errno);

                            result = sectorsToDo == 1
                                         ? outputFormat.WriteSector(sector, doneSectors + track.StartSector)
                                         : outputFormat.WriteSectors(sector, doneSectors + track.StartSector,
                                                                     sectorsToDo);
                        }

                        Assert.IsTrue(result,
                                      $"Error {outputFormat.ErrorMessage} writing sector {doneSectors + track.StartSector}...");

                        doneSectors += sectorsToDo;
                    }
                }

                Dictionary<byte, string> isrcs                     = new();
                Dictionary<byte, byte>   trackFlags                = new();
                string                   mcn                       = null;
                HashSet<int>             subchannelExtents         = new();
                Dictionary<byte, int>    smallestPregapLbaPerTrack = new();
                var                      tracks                    = new Track[inputFormat.Tracks.Count];

                for(var i = 0; i < tracks.Length; i++)
                {
                    tracks[i] = new Track
                    {
                        Indexes           = new Dictionary<ushort, int>(),
                        Description       = inputFormat.Tracks[i].Description,
                        EndSector         = inputFormat.Tracks[i].EndSector,
                        StartSector       = inputFormat.Tracks[i].StartSector,
                        Pregap            = inputFormat.Tracks[i].Pregap,
                        Sequence          = inputFormat.Tracks[i].Sequence,
                        Session           = inputFormat.Tracks[i].Session,
                        BytesPerSector    = inputFormat.Tracks[i].BytesPerSector,
                        RawBytesPerSector = inputFormat.Tracks[i].RawBytesPerSector,
                        Type              = inputFormat.Tracks[i].Type,
                        SubchannelType    = inputFormat.Tracks[i].SubchannelType
                    };

                    foreach(KeyValuePair<ushort, int> idx in inputFormat.Tracks[i].Indexes)
                        tracks[i].Indexes[idx.Key] = idx.Value;
                }

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.
                                                         Where(t => t == SectorTagType.CdTrackIsrc).OrderBy(t => t))
                {
                    foreach(Track track in tracks)
                    {
                        errno = inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                        if(errno != ErrorNumber.NoError)
                            continue;

                        isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                    }
                }

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.
                                                         Where(t => t == SectorTagType.CdTrackFlags).OrderBy(t => t))
                {
                    foreach(Track track in tracks)
                    {
                        errno = inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] flags);

                        if(errno != ErrorNumber.NoError)
                            continue;

                        trackFlags[(byte)track.Sequence] = flags[0];
                    }
                }

                for(ulong s = 0; s < inputFormat.Info.Sectors; s++)
                {
                    if(s > int.MaxValue)
                        break;

                    subchannelExtents.Add((int)s);
                }

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t).
                                                         TakeWhile(_ => useLong))
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
                        ulong  trackSectors = track.EndSector - track.StartSector + 1;
                        byte[] sector;
                        bool   result;

                        switch(tag)
                        {
                            case SectorTagType.CdTrackFlags:
                            case SectorTagType.CdTrackIsrc:
                                errno = inputFormat.ReadSectorTag(track.Sequence, tag, out sector);

                                Assert.AreEqual(ErrorNumber.NoError, errno,
                                                $"Error {errno} reading tag, not continuing...");

                                result = outputFormat.WriteSectorTag(sector, track.Sequence, tag);

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
                                errno = inputFormat.ReadSectorTag(doneSectors + track.StartSector, tag, out sector);

                                Assert.AreEqual(ErrorNumber.NoError, errno,
                                                $"Error {errno} reading tag, not continuing...");

                                if(tag == SectorTagType.CdSectorSubchannel)
                                {
                                    bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                        MmcSubchannel.Raw, sector, doneSectors + track.StartSector, 1, null, isrcs,
                                        (byte)track.Sequence, ref mcn, tracks, subchannelExtents, true,
                                        outputFormat, true, true, null, null, smallestPregapLbaPerTrack, false,
                                        out _);

                                    if(indexesChanged)
                                        outputFormat.SetTracks(tracks.ToList());

                                    result = true;
                                }
                                else
                                    result = outputFormat.WriteSectorTag(sector, doneSectors + track.StartSector, tag);
                            }
                            else
                            {
                                errno = inputFormat.ReadSectorsTag(doneSectors + track.StartSector, sectorsToDo, tag,
                                                                   out sector);

                                Assert.AreEqual(ErrorNumber.NoError, errno,
                                                $"Error {errno} reading tag, not continuing...");

                                if(tag == SectorTagType.CdSectorSubchannel)
                                {
                                    bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                        MmcSubchannel.Raw, sector, doneSectors + track.StartSector, sectorsToDo,
                                        null, isrcs, (byte)track.Sequence, ref mcn, tracks, subchannelExtents,
                                        true, outputFormat, true, true, null, null, smallestPregapLbaPerTrack,
                                        false, out _);

                                    if(indexesChanged)
                                        outputFormat.SetTracks(tracks.ToList());

                                    result = true;
                                }
                                else
                                    result = outputFormat.WriteSectorsTag(sector, doneSectors + track.StartSector,
                                                                          sectorsToDo, tag);
                            }

                            Assert.IsTrue(result,
                                          $"Error {outputFormat.ErrorMessage} writing tag for sector {doneSectors + track.StartSector}, not continuing...");

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
                if(inputFormat.Info.MediaType is MediaType.CD or MediaType.CDDA or MediaType.CDG or MediaType.CDEG
                                              or MediaType.CDI or MediaType.CDROM or MediaType.CDROMXA
                                              or MediaType.CDPLUS or MediaType.CDMO or MediaType.CDR or MediaType.CDRW
                                              or MediaType.CDMRW or MediaType.VCD or MediaType.SVCD or MediaType.PCD
                                              or MediaType.DTSCD or MediaType.CDMIDI or MediaType.CDV
                                              or MediaType.CDIREADY or MediaType.FMTOWNS or MediaType.PS1CD
                                              or MediaType.PS2CD or MediaType.MEGACD or MediaType.SATURNCD
                                              or MediaType.GDROM or MediaType.GDR or MediaType.MilCD
                                              or MediaType.SuperCDROM2 or MediaType.JaguarCD or MediaType.ThreeDO
                                              or MediaType.PCFX or MediaType.NeoGeoCD or MediaType.CDTV
                                              or MediaType.CD32 or MediaType.Playdia or MediaType.Pippin
                                              or MediaType.VideoNow or MediaType.VideoNowColor or MediaType.VideoNowXp
                                              or MediaType.CVD)
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
                Assert.AreEqual(ErrorNumber.NoError, opened, $"Open created: {testFile}");

                if(opened != ErrorNumber.NoError)
                    continue;

                using(new AssertionScope())
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors, image.Info.Sectors, $"Sectors (output): {testFile}");

                        if(test.SectorSize > 0)
                            Assert.AreEqual(test.SectorSize, image.Info.SectorSize,
                                            $"Sector size (output): {testFile}");

                        Assert.AreEqual(test.Tracks.Length, image.Tracks.Count, $"Tracks (output): {testFile}");

                        image.Tracks.Select(t => t.Session).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Session), $"Track session (output): {testFile}");

                        image.Tracks.Select(t => t.StartSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Start), $"Track start (output): {testFile}");

                        image.Tracks.Select(t => t.EndSector).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.End), $"Track end (output): {testFile}");

                        image.Tracks.Select(t => t.Pregap).Should().
                              BeEquivalentTo(test.Tracks.Select(s => s.Pregap), $"Track pregap (output): {testFile}");

                        var trackNo = 0;

                        var   flags           = new byte?[image.Tracks.Count];
                        ulong latestEndSector = 0;

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(currentTrack.EndSector > latestEndSector)
                                latestEndSector = currentTrack.EndSector;

                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                            {
                                errno = image.ReadSectorTag(currentTrack.Sequence, SectorTagType.CdTrackFlags,
                                                            out byte[] tmp);

                                if(errno == ErrorNumber.NoError)
                                    flags[trackNo] = tmp[0];
                            }

                            trackNo++;
                        }

                        flags.Should().BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                                      $"Track flags (output): {testFile}");

                        Assert.AreEqual(latestEndSector, image.Info.Sectors - 1,
                                        $"Last sector for tracks is {latestEndSector}, but it is {image.Info.Sectors} for image (output)");
                    });

                Md5Context ctx;

                foreach(bool @long in new[]
                        {
                            /*false,*/ true
                        })
                {
                    ctx = new Md5Context();

                    foreach(Track currentTrack in image.Tracks)
                    {
                        ulong sectors = currentTrack.EndSector - currentTrack.StartSector + 1;
                        doneSectors = 0;

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

                    Assert.AreEqual(@long ? test.LongMd5 : test.Md5, ctx.End(),
                                    $"{(@long ? "Long hash (output)" : "Hash (output)")}: {testFile}");
                }

                if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    return;

                ctx = new Md5Context();

                foreach(Track currentTrack in image.Tracks)
                {
                    ulong sectors = currentTrack.EndSector - currentTrack.StartSector + 1;
                    doneSectors = 0;

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

                Assert.AreEqual(test.SubchannelMd5, ctx.End(), $"Subchannel hash (output): {testFile}");
            }
        });
    }
}