using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Devices;
using NUnit.Framework;
using File = System.IO.File;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using Track = Aaru.CommonTypes.Structs.Track;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Tests.Issues;

/// <summary>
///     This tests the conversion of an input image (autodetected) to an output image and checks that the resulting
///     image has the same hash as it should
/// </summary>

// TODO: The algorithm should be in Core, not copied in 3 places
public abstract class OpticalImageConvertIssueTest
{
    public const    int                        SECTORS_TO_READ = 256;
    public abstract Dictionary<string, string> ParsedOptions           { get; }
    public abstract string                     DataFolder              { get; }
    public abstract string                     InputPath               { get; }
    public abstract string                     SuggestedOutputFilename { get; }
    public abstract IWritableImage             OutputFormat            { get; }
    public abstract string                     Md5                     { get; }
    public abstract bool                       UseLong                 { get; }

    [OneTimeSetUp]
    public void InitTest() => PluginBase.Init();

    [Test]
    public void Convert()
    {
        Environment.CurrentDirectory = DataFolder;

        Resume      resume  = null;
        Metadata    sidecar = null;
        ErrorNumber errno;

        IFilter inputFilter = PluginRegister.Singleton.GetFilter(InputPath);

        Assert.IsNotNull(inputFilter, Localization.Cannot_open_specified_file);

        string outputPath = Path.Combine(Path.GetTempPath(), SuggestedOutputFilename);

        Assert.IsFalse(File.Exists(outputPath), Localization.Output_file_already_exists_not_continuing);

        var inputFormat = ImageFormat.Detect(inputFilter) as IMediaImage;

        Assert.IsNotNull(inputFormat, Localization.Input_image_format_not_identified_not_proceeding_with_conversion);

        Assert.AreEqual(ErrorNumber.NoError, inputFormat.Open(inputFilter), Localization.Unable_to_open_image_format);

        Assert.IsTrue(OutputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType),
                      Localization.Output_format_does_not_support_media_type_cannot_continue);

        if(inputFormat.Info.ReadableSectorTags.Count == 0)
            Assert.IsFalse(UseLong, Localization.Input_image_does_not_support_long_sectors);

        var inputOptical  = inputFormat as IOpticalMediaImage;
        var outputOptical = OutputFormat as IWritableOpticalImage;

        Assert.IsNotNull(inputOptical,        Localization.Could_not_treat_existing_image_as_optical_disc);
        Assert.IsNotNull(outputOptical,       Localization.Could_not_treat_new_image_as_optical_disc);
        Assert.IsNotNull(inputOptical.Tracks, Localization.Existing_image_contains_no_tracks);

        Assert.IsTrue(outputOptical.Create(outputPath, inputFormat.Info.MediaType, ParsedOptions, inputFormat.Info.Sectors, inputFormat.Info.SectorSize),
                      string.Format(Localization.Error_0_creating_output_image, outputOptical.ErrorMessage));

        var metadata = new ImageInfo
        {
            Application           = "Aaru",
            ApplicationVersion    = Version.GetVersion(),
            Comments              = inputFormat.Info.Comments,
            Creator               = inputFormat.Info.Creator,
            DriveFirmwareRevision = inputFormat.Info.DriveFirmwareRevision,
            DriveManufacturer     = inputFormat.Info.DriveManufacturer,
            DriveModel            = inputFormat.Info.DriveModel,
            DriveSerialNumber     = inputFormat.Info.DriveSerialNumber,
            LastMediaSequence     = inputFormat.Info.LastMediaSequence,
            MediaBarcode          = inputFormat.Info.MediaBarcode,
            MediaManufacturer     = inputFormat.Info.MediaManufacturer,
            MediaModel            = inputFormat.Info.MediaModel,
            MediaPartNumber       = inputFormat.Info.MediaPartNumber,
            MediaSequence         = inputFormat.Info.MediaSequence,
            MediaSerialNumber     = inputFormat.Info.MediaSerialNumber,
            MediaTitle            = inputFormat.Info.MediaTitle
        };

        Assert.IsTrue(outputOptical.SetImageInfo(metadata),
                      string.Format(Localization.Error_0_setting_metadata, outputOptical.ErrorMessage));

        Metadata           aaruMetadata = inputFormat.AaruMetadata;
        List<DumpHardware> dumpHardware = inputFormat.DumpHardware;

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
            outputOptical.SupportedMediaTags.Contains(mediaTag)))
        {
            AaruConsole.WriteLine(Localization.Converting_media_tag_0, mediaTag);
            errno = inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

            Assert.AreEqual(ErrorNumber.NoError, errno);
            Assert.IsTrue(outputOptical.WriteMediaTag(tag, mediaTag));
        }

        AaruConsole.WriteLine(Localization._0_sectors_to_convert, inputFormat.Info.Sectors);
        ulong doneSectors;

        Assert.IsTrue(outputOptical.SetTracks(inputOptical.Tracks),
                      string.Format(Localization.Error_0_sending_tracks_list_to_output_image,
                                    outputOptical.ErrorMessage));

        foreach(Track track in inputOptical.Tracks)
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

                if(UseLong)
                {
                    errno = sectorsToDo == 1
                                ? inputFormat.ReadSectorLong(doneSectors  + track.StartSector, out sector)
                                : inputFormat.ReadSectorsLong(doneSectors + track.StartSector, sectorsToDo, out sector);

                    if(errno == ErrorNumber.NoError)
                    {
                        result = sectorsToDo == 1
                                     ? outputOptical.WriteSectorLong(sector, doneSectors + track.StartSector)
                                     : outputOptical.WriteSectorsLong(sector, doneSectors + track.StartSector,
                                                                      sectorsToDo);
                    }
                    else
                        result = true;

                    if(!result && sector.Length % 2352 != 0)
                        useNotLong = true;
                }

                if(!UseLong || useNotLong)
                {
                    errno = sectorsToDo == 1
                                ? inputFormat.ReadSector(doneSectors  + track.StartSector, out sector)
                                : inputFormat.ReadSectors(doneSectors + track.StartSector, sectorsToDo, out sector);

                    Assert.AreEqual(ErrorNumber.NoError, errno);

                    result = sectorsToDo == 1
                                 ? outputOptical.WriteSector(sector, doneSectors  + track.StartSector)
                                 : outputOptical.WriteSectors(sector, doneSectors + track.StartSector, sectorsToDo);
                }

                Assert.IsTrue(result,
                              string.Format(Localization.Error_0_writing_sector_1_not_continuing,
                                            outputOptical.ErrorMessage, doneSectors + track.StartSector));

                doneSectors += sectorsToDo;
            }
        }

        Dictionary<byte, string> isrcs                     = new();
        Dictionary<byte, byte>   trackFlags                = new();
        string                   mcn                       = null;
        HashSet<int>             subchannelExtents         = new();
        Dictionary<byte, int>    smallestPregapLbaPerTrack = new();
        var                      tracks                    = new Track[inputOptical.Tracks.Count];

        for(var i = 0; i < tracks.Length; i++)
        {
            tracks[i] = new Track
            {
                Indexes           = new Dictionary<ushort, int>(),
                Description       = inputOptical.Tracks[i].Description,
                EndSector         = inputOptical.Tracks[i].EndSector,
                StartSector       = inputOptical.Tracks[i].StartSector,
                Pregap            = inputOptical.Tracks[i].Pregap,
                Sequence          = inputOptical.Tracks[i].Sequence,
                Session           = inputOptical.Tracks[i].Session,
                BytesPerSector    = inputOptical.Tracks[i].BytesPerSector,
                RawBytesPerSector = inputOptical.Tracks[i].RawBytesPerSector,
                Type              = inputOptical.Tracks[i].Type,
                SubchannelType    = inputOptical.Tracks[i].SubchannelType
            };

            foreach(KeyValuePair<ushort, int> idx in inputOptical.Tracks[i].Indexes)
                tracks[i].Indexes[idx.Key] = idx.Value;
        }

        foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.Where(t => t == SectorTagType.CdTrackIsrc).
                                                 OrderBy(t => t))
        {
            foreach(Track track in tracks)
            {
                errno = inputFormat.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                if(errno != ErrorNumber.NoError)
                    continue;

                isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
            }
        }

        foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.Where(t => t == SectorTagType.CdTrackFlags).
                                                 OrderBy(t => t))
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

        foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t).TakeWhile(_ => UseLong))
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

            if(!outputOptical.SupportedSectorTags.Contains(tag))
                continue;

            foreach(Track track in inputOptical.Tracks)
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

                        if(errno == ErrorNumber.NoData)
                            continue;

                        Assert.AreEqual(ErrorNumber.NoError, errno,
                                        string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                        result = outputOptical.WriteSectorTag(sector, track.Sequence, tag);

                        Assert.IsTrue(result,
                                      string.Format(Localization.Error_0_writing_tag_not_continuing,
                                                    outputOptical.ErrorMessage));

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
                                        string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                        if(tag == SectorTagType.CdSectorSubchannel)
                        {
                            bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                MmcSubchannel.Raw, sector, doneSectors + track.StartSector, 1, null, isrcs,
                                (byte)track.Sequence, ref mcn, tracks, subchannelExtents, true, outputOptical,
                                true, true, null, null, smallestPregapLbaPerTrack, false, out _);

                            if(indexesChanged)
                                outputOptical.SetTracks(tracks.ToList());

                            result = true;
                        }
                        else
                            result = outputOptical.WriteSectorTag(sector, doneSectors + track.StartSector, tag);
                    }
                    else
                    {
                        errno = inputFormat.ReadSectorsTag(doneSectors + track.StartSector, sectorsToDo, tag,
                                                           out sector);

                        Assert.AreEqual(ErrorNumber.NoError, errno,
                                        string.Format(Localization.Error_0_reading_tag_not_continuing, errno));

                        if(tag == SectorTagType.CdSectorSubchannel)
                        {
                            bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                MmcSubchannel.Raw, sector, doneSectors + track.StartSector, sectorsToDo, null,
                                isrcs, (byte)track.Sequence, ref mcn, tracks, subchannelExtents, true,
                                outputOptical, true, true, null, null, smallestPregapLbaPerTrack, false, out _);

                            if(indexesChanged)
                                outputOptical.SetTracks(tracks.ToList());

                            result = true;
                        }
                        else
                        {
                            result = outputOptical.WriteSectorsTag(sector, doneSectors + track.StartSector, sectorsToDo,
                                                                   tag);
                        }
                    }

                    Assert.IsTrue(result,
                                  string.Format(Localization.Error_0_writing_tag_for_sector_1_not_continuing,
                                                outputOptical.ErrorMessage, doneSectors + track.StartSector));

                    doneSectors += sectorsToDo;
                }
            }
        }

        if(isrcs.Count > 0)
        {
            foreach(KeyValuePair<byte, string> isrc in isrcs)
                outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value), isrc.Key, SectorTagType.CdTrackIsrc);
        }

        if(trackFlags.Count > 0)
        {
            foreach((byte track, byte flags) in trackFlags)
            {
                outputOptical.WriteSectorTag(new[]
                {
                    flags
                }, track, SectorTagType.CdTrackFlags);
            }
        }

        if(mcn != null)
            outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

        if(resume != null || dumpHardware != null)
        {
            if(resume != null)
                outputOptical.SetDumpHardware(resume.Tries);
            else if(dumpHardware != null)
                outputOptical.SetDumpHardware(dumpHardware);
        }

        if(sidecar != null || aaruMetadata != null)
        {
            if(sidecar != null)
                outputOptical.SetMetadata(sidecar);
            else if(aaruMetadata != null)
                outputOptical.SetMetadata(aaruMetadata);
        }

        Assert.True(outputOptical.Close(),
                    string.Format(Localization.Error_0_closing_output_image_Contents_are_not_correct,
                                  outputOptical.ErrorMessage));

        // Some images will never generate the same
        if(Md5 != null)
        {
            string md5 = Md5Context.File(outputPath, out _);

            Assert.AreEqual(Md5, md5, Localization.Hashes_are_different);
        }

        File.Delete(outputPath);
    }
}