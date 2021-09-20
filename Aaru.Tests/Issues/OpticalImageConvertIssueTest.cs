using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Devices;
using NUnit.Framework;
using Schemas;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Tests.Issues
{
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

        [Test]
        public void Convert()
        {
            Environment.CurrentDirectory = DataFolder;

            Resume           resume  = null;
            CICMMetadataType sidecar = null;
            ErrorNumber      errno;

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(InputPath);

            Assert.IsNotNull(inputFilter, "Cannot open specified file.");

            string outputPath = Path.Combine(Path.GetTempPath(), SuggestedOutputFilename);

            Assert.IsFalse(File.Exists(outputPath), "Output file already exists, not continuing.");

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            Assert.IsNotNull(inputFormat, "Input image format not identified, not proceeding with conversion.");

            Assert.AreEqual(ErrorNumber.NoError, inputFormat.Open(inputFilter), "Unable to open image format");

            Assert.IsTrue(OutputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType),
                          "Output format does not support media type, cannot continue...");

            if(inputFormat.Info.ReadableSectorTags.Count == 0)
            {
                Assert.IsFalse(UseLong, "Input image does not support long sectors.");
            }

            var inputOptical  = inputFormat as IOpticalMediaImage;
            var outputOptical = OutputFormat as IWritableOpticalImage;

            Assert.IsNotNull(inputOptical, "Could not treat existing image as optical disc.");
            Assert.IsNotNull(outputOptical, "Could not treat new image as optical disc.");
            Assert.IsNotNull(inputOptical.Tracks, "Existing image contains no tracks.");

            Assert.IsTrue(outputOptical.Create(outputPath, inputFormat.Info.MediaType, ParsedOptions, inputFormat.Info.Sectors, inputFormat.Info.SectorSize),
                          $"Error {outputOptical.ErrorMessage} creating output image.");

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

            Assert.IsTrue(outputOptical.SetMetadata(metadata),
                          $"Error {outputOptical.ErrorMessage} setting metadata, ");

            CICMMetadataType       cicmMetadata = inputFormat.CicmMetadata;
            List<DumpHardwareType> dumpHardware = inputFormat.DumpHardware;

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                outputOptical.SupportedMediaTags.Contains(mediaTag)))
            {
                AaruConsole.WriteLine("Converting media tag {0}", mediaTag);
                errno = inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

                Assert.AreEqual(ErrorNumber.NoError, errno);
                Assert.IsTrue(outputOptical.WriteMediaTag(tag, mediaTag));
            }

            AaruConsole.WriteLine("{0} sectors to convert", inputFormat.Info.Sectors);
            ulong doneSectors;

            Assert.IsTrue(outputOptical.SetTracks(inputOptical.Tracks),
                          $"Error {outputOptical.ErrorMessage} sending tracks list to output image.");

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

                    bool useNotLong = false;
                    bool result     = false;

                    if(UseLong)
                    {
                        errno = sectorsToDo == 1 ? inputFormat.ReadSectorLong(doneSectors + track.StartSector, out sector) : inputFormat.ReadSectorsLong(doneSectors + track.StartSector, sectorsToDo, out sector);

                        if(errno == ErrorNumber.NoError)
                            result = sectorsToDo == 1
                                         ? outputOptical.WriteSectorLong(sector, doneSectors + track.StartSector)
                                         : outputOptical.WriteSectorsLong(sector, doneSectors + track.StartSector,
                                                                          sectorsToDo);
                        else
                            result = false;

                        if(!result &&
                           sector.Length % 2352 != 0)
                            useNotLong = true;
                    }

                    if(!UseLong || useNotLong)
                    {
                        errno = sectorsToDo == 1 ? inputFormat.ReadSector(doneSectors + track.StartSector, out sector)
                                    : inputFormat.ReadSectors(doneSectors + track.StartSector, sectorsToDo, out sector);

                        Assert.AreEqual(ErrorNumber.NoError, errno);

                        result = sectorsToDo == 1 ? outputOptical.WriteSector(sector, doneSectors + track.StartSector)
                                     : outputOptical.WriteSectors(sector, doneSectors + track.StartSector, sectorsToDo);
                    }

                    Assert.IsTrue(result,
                                  $"Error {outputOptical.ErrorMessage} writing sector {doneSectors + track.StartSector}, not continuing...");

                    doneSectors += sectorsToDo;
                }
            }

            Dictionary<byte, string> isrcs                     = new();
            Dictionary<byte, byte>   trackFlags                = new();
            string                   mcn                       = null;
            HashSet<int>             subchannelExtents         = new();
            Dictionary<byte, int>    smallestPregapLbaPerTrack = new();
            Track[]                  tracks                    = new Track[inputOptical.Tracks.Count];

            for(int i = 0; i < tracks.Length; i++)
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
                    byte[] isrc = inputFormat.ReadSectorTag(track.Sequence, tag);

                    if(isrc is null)
                        continue;

                    isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                }
            }

            foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.
                                                     Where(t => t == SectorTagType.CdTrackFlags).OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    byte[] flags = inputFormat.ReadSectorTag(track.Sequence, tag);

                    if(flags is null)
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

            foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t).TakeWhile(tag => UseLong))
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
                            sector = inputFormat.ReadSectorTag(track.Sequence, tag);
                            result = outputOptical.WriteSectorTag(sector, track.Sequence, tag);

                            Assert.IsTrue(result, $"Error {outputOptical.ErrorMessage} writing tag, not continuing...");

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
                            sector = inputFormat.ReadSectorTag(doneSectors + track.StartSector, tag);

                            if(tag == SectorTagType.CdSectorSubchannel)
                            {
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                    MmcSubchannel.Raw, sector, doneSectors + track.StartSector, 1, null, isrcs,
                                    (byte)track.Sequence, ref mcn, tracks, subchannelExtents, true, outputOptical,
                                    true, true, null, null, smallestPregapLbaPerTrack, false);

                                if(indexesChanged)
                                    outputOptical.SetTracks(tracks.ToList());

                                result = true;
                            }
                            else
                                result = outputOptical.WriteSectorTag(sector, doneSectors + track.StartSector, tag);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsTag(doneSectors + track.StartSector, sectorsToDo, tag);

                            if(tag == SectorTagType.CdSectorSubchannel)
                            {
                                bool indexesChanged = CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                    MmcSubchannel.Raw, sector, doneSectors + track.StartSector, sectorsToDo, null,
                                    isrcs, (byte)track.Sequence, ref mcn, tracks, subchannelExtents, true,
                                    outputOptical, true, true, null, null, smallestPregapLbaPerTrack, false);

                                if(indexesChanged)
                                    outputOptical.SetTracks(tracks.ToList());

                                result = true;
                            }
                            else
                                result = outputOptical.WriteSectorsTag(sector, doneSectors + track.StartSector,
                                                                       sectorsToDo, tag);
                        }

                        Assert.IsTrue(result,
                                      $"Error {outputOptical.ErrorMessage} writing tag for sector {doneSectors + track.StartSector}, not continuing...");

                        doneSectors += sectorsToDo;
                    }
                }
            }

            if(isrcs.Count > 0)
                foreach(KeyValuePair<byte, string> isrc in isrcs)
                    outputOptical.WriteSectorTag(Encoding.UTF8.GetBytes(isrc.Value), isrc.Key,
                                                 SectorTagType.CdTrackIsrc);

            if(trackFlags.Count > 0)
                foreach((byte track, byte flags) in trackFlags)
                    outputOptical.WriteSectorTag(new[]
                    {
                        flags
                    }, track, SectorTagType.CdTrackFlags);

            if(mcn != null)
                outputOptical.WriteMediaTag(Encoding.UTF8.GetBytes(mcn), MediaTagType.CD_MCN);

            if(resume       != null ||
               dumpHardware != null)
            {
                if(resume != null)
                    outputOptical.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null)
                    outputOptical.SetDumpHardware(dumpHardware);
            }

            if(sidecar      != null ||
               cicmMetadata != null)
            {
                if(sidecar != null)
                    outputOptical.SetCicmMetadata(sidecar);
                else if(cicmMetadata != null)
                    outputOptical.SetCicmMetadata(cicmMetadata);
            }

            Assert.True(outputOptical.Close(),
                        $"Error {outputOptical.ErrorMessage} closing output image... Contents are not correct.");

            // Some images will never generate the same
            if(Md5 != null)
            {
                string md5 = Md5Context.File(outputPath, out _);

                Assert.AreEqual(Md5, md5, "Hashes are different");
            }

            File.Delete(outputPath);
        }
    }
}