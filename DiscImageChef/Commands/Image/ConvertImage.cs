// /***************************************************************************
// The Disc Image Chef
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Schemas;
using ImageInfo = DiscImageChef.CommonTypes.Structs.ImageInfo;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Commands.Image
{
    internal class ConvertImageCommand : Command
    {
        public ConvertImageCommand() : base("convert-image", "Converts one image to another format.")
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
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("convert-image");

            DicConsole.DebugWriteLine("Analyze command", "--cicm-xml={0}", cicmXml);
            DicConsole.DebugWriteLine("Analyze command", "--comments={0}", comments);
            DicConsole.DebugWriteLine("Analyze command", "--count={0}", count);
            DicConsole.DebugWriteLine("Analyze command", "--creator={0}", creator);
            DicConsole.DebugWriteLine("Analyze command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Analyze command", "--drive-manufacturer={0}", driveManufacturer);
            DicConsole.DebugWriteLine("Analyze command", "--drive-model={0}", driveModel);
            DicConsole.DebugWriteLine("Analyze command", "--drive-revision={0}", driveFirmwareRevision);
            DicConsole.DebugWriteLine("Analyze command", "--drive-serial={0}", driveSerialNumber);
            DicConsole.DebugWriteLine("Analyze command", "--force={0}", force);
            DicConsole.DebugWriteLine("Analyze command", "--format={0}", format);
            DicConsole.DebugWriteLine("Analyze command", "--input={0}", inputPath);
            DicConsole.DebugWriteLine("Analyze command", "--media-barcode={0}", mediaBarcode);
            DicConsole.DebugWriteLine("Analyze command", "--media-lastsequence={0}", lastMediaSequence);
            DicConsole.DebugWriteLine("Analyze command", "--media-manufacturer={0}", mediaManufacturer);
            DicConsole.DebugWriteLine("Analyze command", "--media-model={0}", mediaModel);
            DicConsole.DebugWriteLine("Analyze command", "--media-partnumber={0}", mediaPartNumber);
            DicConsole.DebugWriteLine("Analyze command", "--media-sequence={0}", mediaSequence);
            DicConsole.DebugWriteLine("Analyze command", "--media-serial={0}", mediaSerialNumber);
            DicConsole.DebugWriteLine("Analyze command", "--media-title={0}", mediaTitle);
            DicConsole.DebugWriteLine("Analyze command", "--options={0}", outputOptions);
            DicConsole.DebugWriteLine("Analyze command", "--output={0}", outputPath);
            DicConsole.DebugWriteLine("Analyze command", "--resume-file={0}", resumeFile);
            DicConsole.DebugWriteLine("Analyze command", "--verbose={0}", verbose);

            Dictionary<string, string> parsedOptions = Core.Options.Parse(outputOptions);
            DicConsole.DebugWriteLine("Analyze command", "Parsed options:");

            foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
                DicConsole.DebugWriteLine("Analyze command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

            if(count == 0)
            {
                DicConsole.ErrorWriteLine("Need to specify more than 0 sectors to copy at once");

                return(int)ErrorNumber.InvalidArgument;
            }

            Resume           resume  = null;
            CICMMetadataType sidecar = null;

            var xs = new XmlSerializer(typeof(CICMMetadataType));

            if(cicmXml != null)
                if(File.Exists(cicmXml))
                    try
                    {
                        var sr = new StreamReader(cicmXml);
                        sidecar = (CICMMetadataType)xs.Deserialize(sr);
                        sr.Close();
                    }
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect metadata sidecar file, not continuing...");

                        return(int)ErrorNumber.InvalidSidecar;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find metadata sidecar, not continuing...");

                    return(int)ErrorNumber.FileNotFound;
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
                    catch
                    {
                        DicConsole.ErrorWriteLine("Incorrect resume file, not continuing...");

                        return(int)ErrorNumber.InvalidResume;
                    }
                else
                {
                    DicConsole.ErrorWriteLine("Could not find resume file, not continuing...");

                    return(int)ErrorNumber.FileNotFound;
                }

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(inputPath);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            if(File.Exists(outputPath))
            {
                DicConsole.ErrorWriteLine("Output file already exists, not continuing.");

                return(int)ErrorNumber.DestinationExists;
            }

            PluginBase  plugins     = GetPluginBase.Instance;
            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.WriteLine("Input image format not identified, not proceeding with conversion.");

                return(int)ErrorNumber.UnrecognizedFormat;
            }

            if(verbose)
                DicConsole.VerboseWriteLine("Input image format identified by {0} ({1}).", inputFormat.Name,
                                            inputFormat.Id);
            else
                DicConsole.WriteLine("Input image format identified by {0}.", inputFormat.Name);

            try
            {
                if(!inputFormat.Open(inputFilter))
                {
                    DicConsole.WriteLine("Unable to open image format");
                    DicConsole.WriteLine("No error given");

                    return(int)ErrorNumber.CannotOpenFormat;
                }

                DicConsole.DebugWriteLine("Convert-image command", "Correctly opened image file.");

                DicConsole.DebugWriteLine("Convert-image command", "Image without headers is {0} bytes.",
                                          inputFormat.Info.ImageSize);

                DicConsole.DebugWriteLine("Convert-image command", "Image has {0} sectors.", inputFormat.Info.Sectors);

                DicConsole.DebugWriteLine("Convert-image command", "Image identifies media type as {0}.",
                                          inputFormat.Info.MediaType);

                Statistics.AddMediaFormat(inputFormat.Format);
                Statistics.AddMedia(inputFormat.Info.MediaType, false);
                Statistics.AddFilter(inputFilter.Name);
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Unable to open image format");
                DicConsole.ErrorWriteLine("Error: {0}", ex.Message);
                DicConsole.DebugWriteLine("Convert-image command", "Stack trace: {0}", ex.StackTrace);

                return(int)ErrorNumber.CannotOpenFormat;
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
                DicConsole.WriteLine("No plugin supports requested extension.");

                return(int)ErrorNumber.FormatNotFound;
            }

            if(candidates.Count > 1)
            {
                DicConsole.WriteLine("More than one plugin supports requested extension.");

                return(int)ErrorNumber.TooManyFormats;
            }

            IWritableImage outputFormat = candidates[0];

            if(verbose)
                DicConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
            else
                DicConsole.WriteLine("Output image format: {0}.", outputFormat.Name);

            if(!outputFormat.SupportedMediaTypes.Contains(inputFormat.Info.MediaType))
            {
                DicConsole.ErrorWriteLine("Output format does not support media type, cannot continue...");

                return(int)ErrorNumber.UnsupportedMedia;
            }

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(outputFormat.SupportedMediaTags.Contains(mediaTag) || force)
                    continue;

                DicConsole.ErrorWriteLine("Converting image will lose media tag {0}, not continuing...", mediaTag);
                DicConsole.ErrorWriteLine("If you don't care, use force option.");

                return(int)ErrorNumber.DataWillBeLost;
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

                DicConsole.ErrorWriteLine("Converting image will lose sector tag {0}, not continuing...", sectorTag);

                DicConsole.
                    ErrorWriteLine("If you don't care, use force option. This will skip all sector tags converting only user data.");

                return(int)ErrorNumber.DataWillBeLost;
            }

            if(!outputFormat.Create(outputPath, inputFormat.Info.MediaType, parsedOptions, inputFormat.Info.Sectors,
                                    inputFormat.Info.SectorSize))
            {
                DicConsole.ErrorWriteLine("Error {0} creating output image.", outputFormat.ErrorMessage);

                return(int)ErrorNumber.CannotCreateFormat;
            }

            var metadata = new ImageInfo
            {
                Application           = "DiscImageChef",
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
                DicConsole.ErrorWrite("Error {0} setting metadata, ", outputFormat.ErrorMessage);

                if(!force)
                {
                    DicConsole.ErrorWriteLine("not continuing...");

                    return(int)ErrorNumber.WriteError;
                }

                DicConsole.ErrorWriteLine("continuing...");
            }

            CICMMetadataType       cicmMetadata = inputFormat.CicmMetadata;
            List<DumpHardwareType> dumpHardware = inputFormat.DumpHardware;

            foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags)
            {
                if(force && !outputFormat.SupportedMediaTags.Contains(mediaTag))
                    continue;

                DicConsole.WriteLine("Converting media tag {0}", mediaTag);
                byte[] tag = inputFormat.ReadDiskTag(mediaTag);

                if(outputFormat.WriteMediaTag(tag, mediaTag))
                    continue;

                if(force)
                    DicConsole.ErrorWriteLine("Error {0} writing media tag, continuing...", outputFormat.ErrorMessage);
                else
                {
                    DicConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...",
                                              outputFormat.ErrorMessage);

                    return(int)ErrorNumber.WriteError;
                }
            }

            DicConsole.WriteLine("{0} sectors to convert", inputFormat.Info.Sectors);
            ulong doneSectors = 0;

            if(inputFormat is IOpticalMediaImage inputOptical      &&
               outputFormat is IWritableOpticalImage outputOptical &&
               inputOptical.Tracks != null)
            {
                if(!outputOptical.SetTracks(inputOptical.Tracks))
                {
                    DicConsole.ErrorWriteLine("Error {0} sending tracks list to output image.",
                                              outputFormat.ErrorMessage);

                    return(int)ErrorNumber.WriteError;
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

                        DicConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)",
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
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);
                            else
                            {
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);

                                return(int)ErrorNumber.WriteError;
                            }

                        doneSectors += sectorsToDo;
                    }
                }

                DicConsole.Write("\rConverting sectors {0} to {1} in track {3} ({2:P2} done)", inputFormat.Info.Sectors,
                                 inputFormat.Info.Sectors, 1.0, inputOptical.Tracks.Count);

                DicConsole.WriteLine();

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
                                DicConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag,
                                                 track.TrackSequence,
                                                 track.TrackSequence / (double)inputOptical.Tracks.Count);

                                sector = inputFormat.ReadSectorTag(track.TrackStartSector, tag);
                                result = outputFormat.WriteSectorTag(sector, track.TrackStartSector, tag);

                                if(!result)
                                    if(force)
                                        DicConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                  outputFormat.ErrorMessage);
                                    else
                                    {
                                        DicConsole.ErrorWriteLine("Error {0} writing tag, not continuing...",
                                                                  outputFormat.ErrorMessage);

                                        return(int)ErrorNumber.WriteError;
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

                            DicConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
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
                                    DicConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                              outputFormat.ErrorMessage, doneSectors);
                                else
                                {
                                    DicConsole.ErrorWriteLine("Error {0} writing tag for sector {1}, not continuing...",
                                                              outputFormat.ErrorMessage, doneSectors);

                                    return(int)ErrorNumber.WriteError;
                                }

                            doneSectors += sectorsToDo;
                        }
                    }

                    switch(tag)
                    {
                        case SectorTagType.CdTrackFlags:
                        case SectorTagType.CdTrackIsrc:
                            DicConsole.Write("\rConverting tag {0} in track {1} ({2:P2} done).", tag,
                                             inputOptical.Tracks.Count, 1.0);

                            break;
                        default:
                            DicConsole.Write("\rConverting tag {4} for sectors {0} to {1} in track {3} ({2:P2} done)",
                                             inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0,
                                             inputOptical.Tracks.Count, tag);

                            break;
                    }

                    DicConsole.WriteLine();
                }
            }
            else
            {
                DicConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads and {2} sectors per track",
                                     inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                     inputFormat.Info.SectorsPerTrack);

                if(!outputFormat.SetGeometry(inputFormat.Info.Cylinders, inputFormat.Info.Heads,
                                             inputFormat.Info.SectorsPerTrack))
                    DicConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                              outputFormat.ErrorMessage);

                while(doneSectors < inputFormat.Info.Sectors)
                {
                    byte[] sector;

                    uint sectorsToDo;

                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                        sectorsToDo = (uint)count;
                    else
                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                    DicConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", doneSectors,
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
                            DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                      outputFormat.ErrorMessage, doneSectors);
                        else
                        {
                            DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                      outputFormat.ErrorMessage, doneSectors);

                            return(int)ErrorNumber.WriteError;
                        }

                    doneSectors += sectorsToDo;
                }

                DicConsole.Write("\rConverting sectors {0} to {1} ({2:P2} done)", inputFormat.Info.Sectors,
                                 inputFormat.Info.Sectors, 1.0);

                DicConsole.WriteLine();

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

                        DicConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)", doneSectors,
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
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);
                            else
                            {
                                DicConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                          outputFormat.ErrorMessage, doneSectors);

                                return(int)ErrorNumber.WriteError;
                            }

                        doneSectors += sectorsToDo;
                    }

                    DicConsole.Write("\rConverting tag {2} for sectors {0} to {1} ({2:P2} done)",
                                     inputFormat.Info.Sectors, inputFormat.Info.Sectors, 1.0, tag);

                    DicConsole.WriteLine();
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
                    DicConsole.WriteLine("Written dump hardware list to output image.");
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
                    DicConsole.WriteLine("Written CICM XML metadata to output image.");
            }

            DicConsole.WriteLine("Closing output image.");

            if(!outputFormat.Close())
                DicConsole.ErrorWriteLine("Error {0} closing output image... Contents are not correct.",
                                          outputFormat.ErrorMessage);

            DicConsole.WriteLine();
            DicConsole.WriteLine("Conversion done.");

            return(int)ErrorNumber.NoError;
        }
    }
}