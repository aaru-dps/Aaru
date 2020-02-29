// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ConvertImage.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts from one media image to another.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Schemas;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Commands.Image
{
    internal class ConvertImageCommand : Command
    {
        public ConvertImageCommand() : base("convert", "Converts one image to another format.")
        {
            Add(new Option(new[]
                {
                    "--cicm-xml", "-x"
                }, "Take metadata from existing CICM XML sidecar.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option("--comments", "Image comments.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option(new[]
                {
                    "--count", "-c"
                }, "How many sectors to convert at once.")
                {
                    Argument = new Argument<int>(() => 64), Required = false
                });

            Add(new Option("--creator", "Who (person) created the image?.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--drive-manufacturer",
                           "Manufacturer of the drive used to read the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--drive-model", "Model of the drive used to read the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--drive-revision",
                           "Firmware revision of the drive used to read the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--drive-serial",
                           "Serial number of the drive used to read the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option(new[]
                {
                    "--force", "-f"
                }, "Continue conversion even if sector or media tags will be lost in the process.")
                {
                    Argument = new Argument<bool>(() => false), Required = false
                });

            Add(new Option(new[]
                           {
                               "--format", "-p"
                           },
                           "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-barcode", "Barcode of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-lastsequence",
                           "Last media of the sequence the media represented by the image corresponds to.")
            {
                Argument = new Argument<int>(() => 0), Required = false
            });

            Add(new Option("--media-manufacturer", "Manufacturer of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-model", "Model of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-partnumber", "Part number of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-sequence", "Number in sequence for the media represented by the image.")
            {
                Argument = new Argument<int>(() => 0), Required = false
            });

            Add(new Option("--media-serial", "Serial number of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option("--media-title", "Title of the media represented by the image.")
            {
                Argument = new Argument<string>(() => null), Required = false
            });

            Add(new Option(new[]
                {
                    "--options", "-O"
                }, "Comma separated name=value pairs of options to pass to output image plugin.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            Add(new Option(new[]
                {
                    "--resume-file", "-r"
                }, "Take list of dump hardware from existing resume file.")
                {
                    Argument = new Argument<string>(() => null), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Input image path", Name = "input-path"
            });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Output image path", Name = "output-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool verbose, bool debug, string cicmXml, string comments, int count, string creator,
                                 string driveFirmwareRevision, string driveManufacturer, string driveModel,
                                 string driveSerialNumber, bool force, string inputPath, int lastMediaSequence,
                                 string mediaBarcode, string mediaManufacturer, string mediaModel,
                                 string mediaPartNumber, int mediaSequence, string mediaSerialNumber, string mediaTitle,
                                 string outputPath, string outputOptions, string resumeFile, string format)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("convert-image");

            AaruConsole.DebugWriteLine("Analyze command", "--cicm-xml={0}", cicmXml);
            AaruConsole.DebugWriteLine("Analyze command", "--comments={0}", comments);
            AaruConsole.DebugWriteLine("Analyze command", "--count={0}", count);
            AaruConsole.DebugWriteLine("Analyze command", "--creator={0}", creator);
            AaruConsole.DebugWriteLine("Analyze command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Analyze command", "--drive-manufacturer={0}", driveManufacturer);
            AaruConsole.DebugWriteLine("Analyze command", "--drive-model={0}", driveModel);
            AaruConsole.DebugWriteLine("Analyze command", "--drive-revision={0}", driveFirmwareRevision);
            AaruConsole.DebugWriteLine("Analyze command", "--drive-serial={0}", driveSerialNumber);
            AaruConsole.DebugWriteLine("Analyze command", "--force={0}", force);
            AaruConsole.DebugWriteLine("Analyze command", "--format={0}", format);
            AaruConsole.DebugWriteLine("Analyze command", "--input={0}", inputPath);
            AaruConsole.DebugWriteLine("Analyze command", "--media-barcode={0}", mediaBarcode);
            AaruConsole.DebugWriteLine("Analyze command", "--media-lastsequence={0}", lastMediaSequence);
            AaruConsole.DebugWriteLine("Analyze command", "--media-manufacturer={0}", mediaManufacturer);
            AaruConsole.DebugWriteLine("Analyze command", "--media-model={0}", mediaModel);
            AaruConsole.DebugWriteLine("Analyze command", "--media-partnumber={0}", mediaPartNumber);
            AaruConsole.DebugWriteLine("Analyze command", "--media-sequence={0}", mediaSequence);
            AaruConsole.DebugWriteLine("Analyze command", "--media-serial={0}", mediaSerialNumber);
            AaruConsole.DebugWriteLine("Analyze command", "--media-title={0}", mediaTitle);
            AaruConsole.DebugWriteLine("Analyze command", "--options={0}", outputOptions);
            AaruConsole.DebugWriteLine("Analyze command", "--output={0}", outputPath);
            AaruConsole.DebugWriteLine("Analyze command", "--resume-file={0}", resumeFile);
            AaruConsole.DebugWriteLine("Analyze command", "--verbose={0}", verbose);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(outputOptions);
            AaruConsole.DebugWriteLine("Analyze command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                AaruConsole.DebugWriteLine("Analyze command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            if(count == 0)
            {
                AaruConsole.ErrorWriteLine("Need to specify more than 0 sectors to copy at once");

                return (int)ErrorNumber.InvalidArgument;
            }

            Resume           resume  = null;
            CICMMetadataType sidecar = null;
            MediaType        mediaType;

            var xs = new XmlSerializer(typeof(CICMMetadataType));

            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        var sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)xs.Deserialize(sr);
                        sr.Close();
                    }
                    catch(Exception ex)
                    {
                        AaruConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");
                        AaruConsole.DebugWriteLine("Image conversion", $"{ex}");

                        return (int)ErrorNumber.InvalidSidecar;
                    }
                else
                {
                    AaruConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");

                    return (int)ErrorNumber.FileNotFound;
                }

            xs = new XmlSerializer(typeof(Resume));

            if(resumeFile != null)
                if(File.Exists(resumeFile))
                    try
                    {
                        var sr = new StreamReader(resumeFile);
                        resume = (Resume)xs.Deserialize(sr);
                        sr.Close();
                    }
                    catch(Exception ex)
                    {
                        AaruConsole.ErrorWriteLine("Incorrect resume file, not continuing...");
                        AaruConsole.DebugWriteLine("Image conversion", $"{ex}");

                        return (int)ErrorNumber.InvalidResume;
                    }
                else
                {
                    AaruConsole.ErrorWriteLine("Could not find resume file, not continuing...");

                    return (int)ErrorNumber.FileNotFound;
                }

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(inputPath);

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            if(File.Exists(outputPath))
            {
                AaruConsole.ErrorWriteLine("Output file already exists, not continuing.");

                return (int)ErrorNumber.DestinationExists;
            }

            PluginBase  plugins     = GetPluginBase.Instance;
            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                AaruConsole.WriteLine("Input image format not identified, not proceeding with conversion.");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                AaruConsole.VerboseWriteLine("Input image format identified by {0} ({1}).", inputFormat.Name,
                                             inputFormat.Id);
            else
                AaruConsole.WriteLine("Input image format identified by {0}.", inputFormat.Name);

            try
            {
                if(!inputFormat.Open(inputFilter))
                {
                    AaruConsole.WriteLine("Unable to open image format");
                    AaruConsole.WriteLine("No error given");

                    return (int)ErrorNumber.CannotOpenFormat;
                }

                mediaType = inputFormat.Info.MediaType;

                // Obsolete types
                #pragma warning disable 612
                switch(mediaType)
                {
                    case MediaType.SQ1500:
                        mediaType = MediaType.SyJet;

                        break;
                    case MediaType.Bernoulli:
                        mediaType = MediaType.Bernoulli10;

                        break;
                    case MediaType.Bernoulli2:
                        mediaType = MediaType.BernoulliBox2_20;

                        break;
                }
                #pragma warning restore 612

                AaruConsole.DebugWriteLine("Convert-image command", "Correctly opened image file.");

                AaruConsole.DebugWriteLine("Convert-image command", "Image without headers is {0} bytes.",
                                           inputFormat.Info.ImageSize);

                AaruConsole.DebugWriteLine("Convert-image command", "Image has {0} sectors.", inputFormat.Info.Sectors);

                AaruConsole.DebugWriteLine("Convert-image command", "Image identifies media type as {0}.", mediaType);

                Statistics.AddMediaFormat(inputFormat.Format);
                Statistics.AddMedia(mediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Unable to open image format");
                AaruConsole.ErrorWriteLine("Error: {0}", ex.Message);
                AaruConsole.DebugWriteLine("Convert-image command", "Stack trace: {0}", ex.StackTrace);

                return (int)ErrorNumber.CannotOpenFormat;
            }

            List<IWritableImage> candidates = new List<IWritableImage>();

            // Try extension
            if(string.IsNullOrEmpty(format))
                candidates.AddRange(plugins.WritableImages.Values.Where(t =>
                                                                            t.KnownExtensions.
                                                                              Contains(Path.GetExtension(outputPath))));

            // Try Id
            else if(Guid.TryParse(format, out Guid outId))
                candidates.AddRange(plugins.WritableImages.Values.Where(t => t.Id.Equals(outId)));

            // Try name
            else
                candidates.AddRange(plugins.WritableImages.Values.Where(t => string.Equals(t.Name, format,
                                                                                           StringComparison.
                                                                                               InvariantCultureIgnoreCase)));

            if(candidates.Count == 0)
            {
                AaruConsole.WriteLine("No plugin supports requested extension.");

                return (int)ErrorNumber.FormatNotFound;
            }

            if(candidates.Count > 1)
            {
                AaruConsole.WriteLine("More than one plugin supports requested extension.");

                return (int)ErrorNumber.TooManyFormats;
            }

            IWritableImage outputFormat = candidates[0];

            if(verbose)
                AaruConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            else
                AaruConsole.WriteLine("Output image format: {0}.", outputFormat.Name);

            if(!outputFormat.SupportedMediaTypes.Contains(mediaType))
            {
                AaruConsole.ErrorWriteLine("Output format does not support media type, cannot continue...");

                return (int)ErrorNumber.UnsupportedMedia;
            }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(outputFormat.SupportedMediaTags.Contains(mediaTag) || force)
                    continue;

                AaruConsole.ErrorWriteLine("Converting image will lose media tag {0}, not continuing...", mediaTag);
                AaruConsole.ErrorWriteLine("If you don't care, use force option.");

                return (int)ErrorNumber.DataWillBeLost;
            }

            bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

            foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags)
            {
                if(outputFormat.SupportedSectorTags.Contains(sectorTag))
                    continue;

                if(force)
                {
                    if(sectorTag != SectorTagType.CdTrackFlags &&
                       sectorTag != SectorTagType.CdTrackIsrc  &&
                       sectorTag != SectorTagType.CdSectorSubchannel)
                        useLong = false;

                    continue;
                }

                AaruConsole.ErrorWriteLine("Converting image will lose sector tag {0}, not continuing...", sectorTag);

                AaruConsole.
                    ErrorWriteLine("If you don't care, use force option. This will skip all sector tags converting only user data.");

                return (int)ErrorNumber.DataWillBeLost;
            }

            if(!outputFormat.Create(outputPath, mediaType, parsedOptions, inputFormat.Info.Sectors,
                                    inputFormat.Info.SectorSize))
            {
                AaruConsole.ErrorWriteLine("Error {0} creating output image.", outputFormat.ErrorMessage);

                return (int)ErrorNumber.CannotCreateFormat;
            }

            var metadata = new ImageInfo
            {
                Application           = "Aaru",
                ApplicationVersion    = Version.GetVersion(),
                Comments              = comments              ?? inputFormat.Info.Comments,
                Creator               = creator               ?? inputFormat.Info.Creator,
                DriveFirmwareRevision = driveFirmwareRevision ?? inputFormat.Info.DriveFirmwareRevision,
                DriveManufacturer     = driveManufacturer     ?? inputFormat.Info.DriveManufacturer,
                DriveModel            = driveModel            ?? inputFormat.Info.DriveModel,
                DriveSerialNumber     = driveSerialNumber     ?? inputFormat.Info.DriveSerialNumber,
                LastMediaSequence     = lastMediaSequence != 0 ? lastMediaSequence : inputFormat.Info.LastMediaSequence,
                MediaBarcode          = mediaBarcode      ?? inputFormat.Info.MediaBarcode,
                MediaManufacturer     = mediaManufacturer ?? inputFormat.Info.MediaManufacturer,
                MediaModel            = mediaModel        ?? inputFormat.Info.MediaModel,
                MediaPartNumber       = mediaPartNumber   ?? inputFormat.Info.MediaPartNumber,
                MediaSequence         = mediaSequence != 0 ? mediaSequence : inputFormat.Info.MediaSequence,
                MediaSerialNumber     = mediaSerialNumber ?? inputFormat.Info.MediaSerialNumber,
                MediaTitle            = mediaTitle        ?? inputFormat.Info.MediaTitle
            };

            if(!outputFormat.SetMetadata(metadata))
            {
                AaruConsole.ErrorWrite("Error {0} setting metadata, ", outputFormat.ErrorMessage);

                if(!force)
                {
                    AaruConsole.ErrorWriteLine("not continuing...");

                    return (int)ErrorNumber.WriteError;
                }

                AaruConsole.ErrorWriteLine("continuing...");
            }

            CICMMetadataType       cicmMetadata = inputFormat.CicmMetadata;
            List<DumpHardwareType> dumpHardware = inputFormat.DumpHardware;

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(force && !outputFormat.SupportedMediaTags.Contains(mediaTag))
                    continue;

                AaruConsole.WriteLine("Converting media tag {0}", mediaTag);
                byte[] tag = inputFormat.ReadDiskTag(mediaTag);

                if(outputFormat.WriteMediaTag(tag, mediaTag))
                    continue;

                if(force)
                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, continuing...", outputFormat.ErrorMessage);
                else
                {
                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...",
                                               outputFormat.ErrorMessage);

                    return (int)ErrorNumber.WriteError;
                }
            }

            AaruConsole.WriteLine("{0} sectors to convert", inputFormat.Info.Sectors);
            ulong doneSectors = 0;

            if(inputFormat is IOpticalMediaImage inputOptical      &&
               outputFormat is IWritableOpticalImage outputOptical &&
               inputOptical.Tracks != null)
            {
                if(!outputOptical.SetTracks(inputOptical.Tracks))
                {
                    AaruConsole.ErrorWriteLine("Error {0} sending tracks list to output image.",
                                               outputFormat.ErrorMessage);

                    return (int)ErrorNumber.WriteError;
                }

                foreach(Track track in inputOptical.Tracks)
                {
                    doneSectors = 0;
                    ulong trackSectors = (track.TrackEndSector - track.TrackStartSector) + 1;

                    while(doneSectors < trackSectors)
                    {
                        byte[] sector;

                        uint sectorsToDo;

                        if(trackSectors - doneSectors >= (ulong)count)
                            sectorsToDo = (uint)count;
                        else
                            sectorsToDo = (uint)(trackSectors - doneSectors);

                        AaruConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)",
                                          doneSectors               + track.TrackStartSector,
                                          doneSectors + sectorsToDo + track.TrackStartSector,
                                          (doneSectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors,
                                          track.TrackSequence);

                        bool result;

                        if(useLong)
                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorLong(doneSectors           + track.TrackStartSector);
                                result = outputFormat.WriteSectorLong(sector, doneSectors + track.TrackStartSector);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsLong(doneSectors + track.TrackStartSector, sectorsToDo);

                                result = outputFormat.WriteSectorsLong(sector, doneSectors + track.TrackStartSector,
                                                                       sectorsToDo);
                            }
                        else
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

                        if(!result)
                            if(force)
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);
                            else
                            {
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);

                                return (int)ErrorNumber.WriteError;
                            }

                        doneSectors += sectorsToDo;
                    }
                }

                AaruConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)",
                                  inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0, inputOptical.Tracks.Count);

                AaruConsole.WriteLine();

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.OrderBy(t => t))
                {
                    if(!useLong)
                        break;

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

                    if(force && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    foreach(Track track in inputOptical.Tracks)
                    {
                        doneSectors = 0;
                        ulong  trackSectors = (track.TrackEndSector - track.TrackStartSector) + 1;
                        byte[] sector;
                        bool   result;

                        switch(tag)
                        {
                            case SectorTagType.CdTrackFlags:
                            case SectorTagType.CdTrackIsrc:
                                AaruConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag,
                                                  track.TrackSequence,
                                                  track.TrackSequence / (double)inputOptical.Tracks.Count);

                                sector = inputFormat.ReadSectorTag(track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, track.TrackStartSector, tag);

                                if(!result)
                                    if(force)
                                        AaruConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                   outputFormat.ErrorMessage);
                                    else
                                    {
                                        AaruConsole.ErrorWriteLine("Error {0} writing tag, not continuing...",
                                                                   outputFormat.ErrorMessage);

                                        return (int)ErrorNumber.WriteError;
                                    }

                                continue;
                        }

                        while(doneSectors < trackSectors)
                        {
                            uint sectorsToDo;

                            if(trackSectors - doneSectors >= (ulong)count)
                                sectorsToDo = (uint)count;
                            else
                                sectorsToDo = (uint)(trackSectors - doneSectors);

                            AaruConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
                                              doneSectors               + track.TrackStartSector,
                                              doneSectors + sectorsToDo + track.TrackStartSector,
                                              (doneSectors + track.TrackStartSector) / (double)inputFormat.Info.Sectors,
                                              track.TrackSequence, tag);

                            if(sectorsToDo == 1)
                            {
                                sector = inputFormat.ReadSectorTag(doneSectors           + track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, doneSectors + track.TrackStartSector, tag);
                            }
                            else
                            {
                                sector = inputFormat.ReadSectorsTag(doneSectors + track.TrackStartSector, sectorsToDo,
                                                                    tag);

                                result = outputFormat.WriteSectorsTag(sector, doneSectors + track.TrackStartSector,
                                                                      sectorsToDo, tag);
                            }

                            if(!result)
                                if(force)
                                    AaruConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                               outputFormat.ErrorMessage, doneSectors);
                                else
                                {
                                    AaruConsole.
                                        ErrorWriteLine("Error {0} writing tag for sector {1}, not continuing...",
                                                       outputFormat.ErrorMessage, doneSectors);

                                    return (int)ErrorNumber.WriteError;
                                }

                            doneSectors += sectorsToDo;
                        }
                    }

                    switch(tag)
                    {
                        case SectorTagType.CdTrackFlags:
                        case SectorTagType.CdTrackIsrc:
                            AaruConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag,
                                              inputOptical.Tracks.Count, 1.0);

                            break;
                        default:
                            AaruConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
                                              inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0,
                                              inputOptical.Tracks.Count, tag);

                            break;
                    }

                    AaruConsole.WriteLine();
                }
            }
            else
            {
                AaruConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads and {2} sectors per track",
                                      inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                      inputFormat.Info.SectorsPerTrack);

                if(!outputFormat.SetGeometry(inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                             inputFormat.Info.SectorsPerTrack))
                    AaruConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                               outputFormat.ErrorMessage);

                while(doneSectors < inputFormat.Info.Sectors)
                {
                    byte[] sector;

                    uint sectorsToDo;

                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                        sectorsToDo = (uint)count;
                    else
                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                    AaruConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", doneSectors,
                                      doneSectors + sectorsToDo, doneSectors / (double)inputFormat.Info.Sectors);

                    bool result;

                    if(useLong)
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorLong(doneSectors);
                            result = outputFormat.WriteSectorLong(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsLong(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                        }
                    else
                    {
                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSector(doneSectors);
                            result = outputFormat.WriteSector(sector, doneSectors);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectors(doneSectors, sectorsToDo);
                            result = outputFormat.WriteSectors(sector, doneSectors, sectorsToDo);
                        }
                    }

                    if(!result)
                        if(force)
                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                       outputFormat.ErrorMessage, doneSectors);
                        else
                        {
                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                       outputFormat.ErrorMessage, doneSectors);

                            return (int)ErrorNumber.WriteError;
                        }

                    doneSectors += sectorsToDo;
                }

                AaruConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", inputFormat.Info.Sectors,
                                  inputFormat.Info.Sectors, 1.0);

                AaruConsole.WriteLine();

                foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                {
                    if(!useLong)
                        break;

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

                    if(force && !outputFormat.SupportedSectorTags.Contains(tag))
                        continue;

                    doneSectors = 0;

                    while(doneSectors < inputFormat.Info.Sectors)
                    {
                        byte[] sector;

                        uint sectorsToDo;

                        if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                            sectorsToDo = (uint)count;
                        else
                            sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                        AaruConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)", doneSectors,
                                          doneSectors + sectorsToDo, doneSectors / (double)inputFormat.Info.Sectors,
                                          tag);

                        bool result;

                        if(sectorsToDo == 1)
                        {
                            sector = inputFormat.ReadSectorTag(doneSectors, tag);
                            result = outputFormat.WriteSectorTag(sector, doneSectors, tag);
                        }
                        else
                        {
                            sector = inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag);
                            result = outputFormat.WriteSectorsTag(sector, doneSectors, sectorsToDo, tag);
                        }

                        if(!result)
                            if(force)
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);
                            else
                            {
                                AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                           outputFormat.ErrorMessage, doneSectors);

                                return (int)ErrorNumber.WriteError;
                            }

                        doneSectors += sectorsToDo;
                    }

                    AaruConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)",
                                      inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0, tag);

                    AaruConsole.WriteLine();
                }
            }

            bool ret = false;

            if(resume       != null ||
               dumpHardware != null)
            {
                if(resume != null)
                    ret = outputFormat.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null)
                    ret = outputFormat.SetDumpHardware(dumpHardware);

                if(ret)
                    AaruConsole.WriteLine("Written dump hardware list to output image.");
            }

            ret = false;

            if(sidecar      != null ||
               cicmMetadata != null)
            {
                if(sidecar != null)
                    ret = outputFormat.SetCicmMetadata(sidecar);
                else if(cicmMetadata != null)
                    ret = outputFormat.SetCicmMetadata(cicmMetadata);

                if(ret)
                    AaruConsole.WriteLine("Written CICM XML metadata to output image.");
            }

            AaruConsole.WriteLine("Closing output image.");

            if(!outputFormat.Close())
                AaruConsole.ErrorWriteLine("Error {0} closing output image... Contents are not correct.",
                                           outputFormat.ErrorMessage);

            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Conversion done.");

            return (int)ErrorNumber.NoError;
        }
    }
}