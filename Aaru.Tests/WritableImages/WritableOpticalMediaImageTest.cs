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
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Devices;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.WritableImages;

public abstract class WritableOpticalMediaImageTest : BaseWritableMediaImageTest
{
    const           uint                       SECTORS_TO_READ = 256;
    public abstract OpticalImageTestExpected[] Tests { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var image = Activator.CreateInstance(InputPlugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_filesystem_for_0, testFile));

                ErrorNumber opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors,
                                        image.Info.Sectors,
                                        string.Format(Localization.Sectors_0, testFile));

                        if(test.SectorSize > 0)
                        {
                            Assert.AreEqual(test.SectorSize,
                                            image.Info.SectorSize,
                                            string.Format(Localization.Sector_size_0, testFile));
                        }

                        Assert.AreEqual(test.MediaType,
                                        image.Info.MediaType,
                                        string.Format(Localization.Media_type_0, testFile));

                        if(image.Info.MetadataMediaType != MetadataMediaType.OpticalDisc) return;

                        Assert.AreEqual(test.Tracks.Length,
                                        image.Tracks.Count,
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
                                errno = image.ReadSectorTag(currentTrack.Sequence,
                                                            SectorTagType.CdTrackFlags,
                                                            out byte[] tmp);

                                if(errno == ErrorNumber.NoError) flags[trackNo] = tmp[0];
                            }

                            trackNo++;
                        }

                        flags.Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                             string.Format(Localization.Track_flags_0, testFile));

                        Assert.AreEqual(latestEndSector,
                                        image.Info.Sectors - 1,
                                        string.Format(Localization.Last_sector_for_tracks_is_0_but_it_is_1_for_image,
                                                      latestEndSector,
                                                      image.Info.Sectors));
                    });
                }
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
                Assert.True(exists, string.Format(Localization._0_not_found, testFile));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // It arrives here...
                if(!exists) continue;

                IFilter filter = PluginRegister.Singleton.GetFilter(testFile);
                filter.Open(testFile);

                var inputFormat = Activator.CreateInstance(InputPlugin.GetType()) as IOpticalMediaImage;

                Assert.NotNull(inputFormat,
                               string.Format(Localization.Could_not_instantiate_input_plugin_for_0, testFile));

                ErrorNumber opened = inputFormat.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                string outputPath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.{OutputExtension}");

                var outputFormat = Activator.CreateInstance(OutputPlugin.GetType()) as IWritableOpticalImage;

                Assert.NotNull(outputFormat,
                               string.Format(Localization.Could_not_instantiate_output_plugin_for_0, testFile));

                Assert.IsTrue(outputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType),
                              string.Format(Localization.Trying_to_convert_unsupported_media_type_0_for_1,
                                            inputFormat.Info.MediaType,
                                            testFile));

                bool useLong = inputFormat.Info.ReadableSectorTags.Except(new[]
                                           {
                                               SectorTagType.CdTrackFlags
                                           })
                                          .Any();

                // TODO: Can be done with LINQ only
                foreach(SectorTagType _ in inputFormat.Info.ReadableSectorTags
                                                      .Where(sectorTag =>
                                                                 !outputFormat.SupportedSectorTags.Contains(sectorTag))
                                                      .Where(sectorTag =>
                                                                 sectorTag != SectorTagType.CdTrackFlags &&
                                                                 sectorTag != SectorTagType.CdTrackIsrc  &&
                                                                 sectorTag != SectorTagType.CdSectorSubchannel))
                    useLong = false;

                Assert.IsTrue(outputFormat.Create(outputPath,
                                                  inputFormat.Info.MediaType,
                                                  new Dictionary<string, string>(),
                                                  inputFormat.Info.Sectors,
                                                  inputFormat.Info.SectorSize),
                              string.Format(Localization.Error_0_creating_output_image, outputFormat.ErrorMessage));

                foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                            outputFormat.SupportedMediaTags.Contains(mediaTag)))
                {
                    if(inputFormat.ReadMediaTag(mediaTag, out byte[] buffer) == ErrorNumber.NoError)
                        outputFormat.WriteMediaTag(buffer, mediaTag);
                }

                Assert.IsTrue(outputFormat.SetTracks(inputFormat.Tracks),
                              string.Format(Localization.Error_0_sending_tracks_list_to_output_image,
                                            outputFormat.ErrorMessage));

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
                                        : inputFormat.ReadSectorsLong(doneSectors + track.StartSector,
                                                                      sectorsToDo,
                                                                      out sector);

                            if(errno == ErrorNumber.NoError)
                            {
                                result = sectorsToDo == 1
                                             ? outputFormat.WriteSectorLong(sector, doneSectors + track.StartSector)
                                             : outputFormat.WriteSectorsLong(sector,
                                                                             doneSectors + track.StartSector,
                                                                             sectorsToDo);
                            }
                            else
                                result = false;

                            if(!result && sector.Length % 2352 != 0) useNotLong = true;
                        }

                        if(!useLong || useNotLong)
                        {
                            errno = sectorsToDo == 1
                                        ? inputFormat.ReadSector(doneSectors + track.StartSector, out sector)
                                        : inputFormat.ReadSectors(doneSectors + track.StartSector,
                                                                  sectorsToDo,
                                                                  out sector);

                            Assert.AreEqual(ErrorNumber.NoError, errno);

                            result = sectorsToDo == 1
                                         ? outputFormat.WriteSector(sector, doneSectors + track.StartSector)
                                         : outputFormat.WriteSectors(sector,
                                                                     doneSectors + track.StartSector,
                                                                     sectorsToDo);
                        }

                        Assert.IsTrue(result,
                                      string.Format(Localization.Error_0_writing_sector_1,
                                                    outputFormat.ErrorMessage,
                                                    doneSectors + track.StartSector));

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

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags
                                                        .Where(t => t == SectorTagType.CdTrackIsrc)
                                                        .OrderBy(t => t))
                {
                    foreach(Track track in tracks)
                    {
                        errno = inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                        if(errno != ErrorNumber.NoError) continue;

                        isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                    }
                }

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags
                                                        .Where(t => t == SectorTagType.CdTrackFlags)
                                                        .OrderBy(t => t))
                {
                    foreach(Track track in tracks)
                    {
                        errno = inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] flags);

                        if(errno != ErrorNumber.NoError) continue;

                        trackFlags[(byte)track.Sequence] = flags[0];
                    }
                }

                for(ulong s = 0; s < inputFormat.Info.Sectors; s++)
                {
                    if(s > int.MaxValue) break;

                    subchannelExtents.Add((int)s);
                }

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t)
                                                        .TakeWhile(_ => useLong))
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

                    if(!outputFormat.SupportedSectorTags.Contains(tag)) continue;

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

                                Assert.AreEqual(ErrorNumber.NoError,
                                                errno,
                                                string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                                result = outputFormat.WriteSectorTag(sector, track.Sequence, tag);

                                Assert.IsTrue(result,
                                              string.Format(Localization.Error_0_writing_tag_not_continuing,
                                                            outputFormat.ErrorMessage));

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

                                Assert.AreEqual(ErrorNumber.NoError,
                                                errno,
                                                string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                                if(tag == SectorTagType.CdSectorSubchannel)
                                {
                                    bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                        MmcSubchannel.Raw,
                                        sector,
                                        doneSectors + track.StartSector,
                                        1,
                                        null,
                                        isrcs,
                                        (byte)track.Sequence,
                                        ref mcn,
                                        tracks,
                                        subchannelExtents,
                                        true,
                                        outputFormat,
                                        true,
                                        true,
                                        null,
                                        null,
                                        smallestPregapLbaPerTrack,
                                        false,
                                        out _);

                                    if(indexesChanged) outputFormat.SetTracks(tracks.ToList());

                                    result = true;
                                }
                                else
                                    result = outputFormat.WriteSectorTag(sector, doneSectors + track.StartSector, tag);
                            }
                            else
                            {
                                errno = inputFormat.ReadSectorsTag(doneSectors + track.StartSector,
                                                                   sectorsToDo,
                                                                   tag,
                                                                   out sector);

                                Assert.AreEqual(ErrorNumber.NoError,
                                                errno,
                                                string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                                if(tag == SectorTagType.CdSectorSubchannel)
                                {
                                    bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                        MmcSubchannel.Raw,
                                        sector,
                                        doneSectors + track.StartSector,
                                        sectorsToDo,
                                        null,
                                        isrcs,
                                        (byte)track.Sequence,
                                        ref mcn,
                                        tracks,
                                        subchannelExtents,
                                        true,
                                        outputFormat,
                                        true,
                                        true,
                                        null,
                                        null,
                                        smallestPregapLbaPerTrack,
                                        false,
                                        out _);

                                    if(indexesChanged) outputFormat.SetTracks(tracks.ToList());

                                    result = true;
                                }
                                else
                                {
                                    result = outputFormat.WriteSectorsTag(sector,
                                                                          doneSectors + track.StartSector,
                                                                          sectorsToDo,
                                                                          tag);
                                }
                            }

                            Assert.IsTrue(result,
                                          string.Format(Localization.Error_0_writing_tag_for_sector_1_not_continuing,
                                                        outputFormat.ErrorMessage,
                                                        doneSectors + track.StartSector));

                            doneSectors += sectorsToDo;
                        }
                    }
                }

                if(isrcs.Count > 0)
                {
                    foreach(KeyValuePair<byte, string> isrc in isrcs)
                    {
                        outputFormat.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value),
                                                    isrc.Key,
                                                    SectorTagType.CdTrackIsrc);
                    }
                }

                if(trackFlags.Count > 0)
                {
                    foreach((byte track, byte flags) in trackFlags)
                    {
                        outputFormat.WriteSectorTag(new[]
                                                    {
                                                        flags
                                                    },
                                                    track,
                                                    SectorTagType.CdTrackFlags);
                    }
                }

                if(mcn != null) outputFormat.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

                // TODO: Progress
                if(inputFormat.Info.MediaType is MediaType.CD
                                              or MediaType.CDDA
                                              or MediaType.CDG
                                              or MediaType.CDEG
                                              or MediaType.CDI
                                              or MediaType.CDROM
                                              or MediaType.CDROMXA
                                              or MediaType.CDPLUS
                                              or MediaType.CDMO
                                              or MediaType.CDR
                                              or MediaType.CDRW
                                              or MediaType.CDMRW
                                              or MediaType.VCD
                                              or MediaType.SVCD
                                              or MediaType.PCD
                                              or MediaType.DTSCD
                                              or MediaType.CDMIDI
                                              or MediaType.CDV
                                              or MediaType.CDIREADY
                                              or MediaType.FMTOWNS
                                              or MediaType.PS1CD
                                              or MediaType.PS2CD
                                              or MediaType.MEGACD
                                              or MediaType.SATURNCD
                                              or MediaType.GDROM
                                              or MediaType.GDR
                                              or MediaType.MilCD
                                              or MediaType.SuperCDROM2
                                              or MediaType.JaguarCD
                                              or MediaType.ThreeDO
                                              or MediaType.PCFX
                                              or MediaType.NeoGeoCD
                                              or MediaType.CDTV
                                              or MediaType.CD32
                                              or MediaType.Playdia
                                              or MediaType.Pippin
                                              or MediaType.VideoNow
                                              or MediaType.VideoNowColor
                                              or MediaType.VideoNowXp
                                              or MediaType.CVD)
                {
                    CompactDisc.GenerateSubchannels(subchannelExtents,
                                                    tracks,
                                                    trackFlags,
                                                    inputFormat.Info.Sectors,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    null,
                                                    outputFormat);
                }

                Assert.IsTrue(outputFormat.Close(),
                              string.Format(Localization.Error_0_closing_output_image_Contents_are_not_correct,
                                            outputFormat.ErrorMessage));

                filter = PluginRegister.Singleton.GetFilter(outputPath);
                filter.Open(outputPath);

                string tmpFolder = Path.GetDirectoryName(outputPath);
                Environment.CurrentDirectory = tmpFolder;

                var image = Activator.CreateInstance(OutputPlugin.GetType()) as IOpticalMediaImage;
                Assert.NotNull(image, string.Format(Localization.Could_not_instantiate_output_plugin_for_0, testFile));

                opened = image.Open(filter);
                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_created_0, testFile));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(test.Sectors,
                                        image.Info.Sectors,
                                        string.Format(Localization.Sectors_output_0, testFile));

                        if(test.SectorSize > 0)
                        {
                            Assert.AreEqual(test.SectorSize,
                                            image.Info.SectorSize,
                                            string.Format(Localization.Sector_size_output_0, testFile));
                        }

                        Assert.AreEqual(test.Tracks.Length,
                                        image.Tracks.Count,
                                        string.Format(Localization.Tracks_output_0, testFile));

                        image.Tracks.Select(t => t.Session)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Session),
                                             string.Format(Localization.Track_session_output_0, testFile));

                        image.Tracks.Select(t => t.StartSector)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Start),
                                             string.Format(Localization.Track_start_output_0, testFile));

                        image.Tracks.Select(t => t.EndSector)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.End),
                                             string.Format(Localization.Track_end_output_0, testFile));

                        image.Tracks.Select(t => t.Pregap)
                             .Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Pregap),
                                             string.Format(Localization.Track_pregap_output_0, testFile));

                        var trackNo = 0;

                        var   flags           = new byte?[image.Tracks.Count];
                        ulong latestEndSector = 0;

                        foreach(Track currentTrack in image.Tracks)
                        {
                            if(currentTrack.EndSector > latestEndSector) latestEndSector = currentTrack.EndSector;

                            if(image.Info.ReadableSectorTags.Contains(SectorTagType.CdTrackFlags))
                            {
                                errno = image.ReadSectorTag(currentTrack.Sequence,
                                                            SectorTagType.CdTrackFlags,
                                                            out byte[] tmp);

                                if(errno == ErrorNumber.NoError) flags[trackNo] = tmp[0];
                            }

                            trackNo++;
                        }

                        flags.Should()
                             .BeEquivalentTo(test.Tracks.Select(s => s.Flags),
                                             string.Format(Localization.Track_flags_output_0, testFile));

                        Assert.AreEqual(latestEndSector,
                                        image.Info.Sectors - 1,
                                        string.Format(Localization
                                                         .Last_sector_for_tracks_is_0_but_it_is_1_for_image_output,
                                                      latestEndSector,
                                                      image.Info.Sectors));
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
                        ulong sectors = currentTrack.EndSector - currentTrack.StartSector + 1;
                        doneSectors = 0;

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

                            Assert.AreEqual(ErrorNumber.NoError, errno);

                            ctx.Update(sector);
                        }
                    }

                    Assert.AreEqual(@long ? test.LongMd5 : test.Md5,
                                    ctx.End(),
                                    string.Format("{0}: {1}",
                                                  @long ? Localization.Long_hash_output : Localization.Hash_output,
                                                  testFile));
                }

                if(!image.Info.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel)) return;

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

                        Assert.AreEqual(ErrorNumber.NoError, errno);

                        ctx.Update(sector);
                    }
                }

                Assert.AreEqual(test.SubchannelMd5,
                                ctx.End(),
                                string.Format(Localization.Subchannel_hash_output_0, testFile));
            }
        });
    }
}