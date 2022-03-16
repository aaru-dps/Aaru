// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Convert.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Commands.Image;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Core.Media;
using Aaru.Devices;
using Schemas;
using Spectre.Console;
using ImageInfo = Aaru.CommonTypes.Structs.ImageInfo;
using MediaType = Aaru.CommonTypes.MediaType;
using Version = Aaru.CommonTypes.Interop.Version;

sealed class ConvertImageCommand : Command
{
    public ConvertImageCommand() : base("convert", "Converts one image to another format.")
    {
        Add(new Option(new[]
            {
                "--cicm-xml", "-x"
            }, "Take metadata from existing CICM XML sidecar.")
            {
                Argument = new Argument<string>(() => null),
                Required = false
            });

        Add(new Option("--comments", "Image comments.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option(new[]
            {
                "--count", "-c"
            }, "How many sectors to convert at once.")
            {
                Argument = new Argument<int>(() => 64),
                Required = false
            });

        Add(new Option("--creator", "Who (person) created the image?.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--drive-manufacturer",
                       "Manufacturer of the drive used to read the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--drive-model", "Model of the drive used to read the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--drive-revision",
                       "Firmware revision of the drive used to read the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--drive-serial", "Serial number of the drive used to read the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option(new[]
            {
                "--force", "-f"
            }, "Continue conversion even if sector or media tags will be lost in the process.")
            {
                Argument = new Argument<bool>(() => false),
                Required = false
            });

        Add(new Option(new[]
                       {
                           "--format", "-p"
                       },
                       "Format of the output image, as plugin name or plugin id. If not present, will try to detect it from output image extension.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-barcode", "Barcode of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-lastsequence",
                       "Last media of the sequence the media represented by the image corresponds to.")
        {
            Argument = new Argument<int>(() => 0),
            Required = false
        });

        Add(new Option("--media-manufacturer", "Manufacturer of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-model", "Model of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-partnumber", "Part number of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-sequence", "Number in sequence for the media represented by the image.")
        {
            Argument = new Argument<int>(() => 0),
            Required = false
        });

        Add(new Option("--media-serial", "Serial number of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option("--media-title", "Title of the media represented by the image.")
        {
            Argument = new Argument<string>(() => null),
            Required = false
        });

        Add(new Option(new[]
            {
                "--options", "-O"
            }, "Comma separated name=value pairs of options to pass to output image plugin.")
            {
                Argument = new Argument<string>(() => null),
                Required = false
            });

        Add(new Option(new[]
            {
                "--resume-file", "-r"
            }, "Take list of dump hardware from existing resume file.")
            {
                Argument = new Argument<string>(() => null),
                Required = false
            });

        Add(new Option(new[]
            {
                "--geometry", "-g"
            }, "Force geometry, only supported in not tape block media. Specify as C/H/S.")
            {
                Argument = new Argument<string>(() => null),
                Required = false
            });

        Add(new Option(new[]
            {
                "--fix-subchannel-position"
            }, "Store subchannel according to the sector they describe.")
            {
                Argument = new Argument<bool>(() => true),
                Required = false
            });

        Add(new Option(new[]
            {
                "--fix-subchannel"
            }, "Try to fix subchannel. Implies fixing subchannel position.")
            {
                Argument = new Argument<bool>(() => false),
                Required = false
            });

        Add(new Option(new[]
            {
                "--fix-subchannel-crc"
            }, "If subchannel looks OK but CRC fails, rewrite it. Implies fixing subchannel.")
            {
                Argument = new Argument<bool>(() => false),
                Required = false
            });

        Add(new Option(new[]
            {
                "--generate-subchannels"
            }, "Generates missing subchannels.")
            {
                Argument = new Argument<bool>(() => false),
                Required = false
            });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Input image path",
            Name        = "input-path"
        });

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Output image path",
            Name        = "output-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool verbose, bool debug, string cicmXml, string comments, int count, string creator,
                             string driveFirmwareRevision, string driveManufacturer, string driveModel,
                             string driveSerialNumber, bool force, string inputPath, int lastMediaSequence,
                             string mediaBarcode, string mediaManufacturer, string mediaModel, string mediaPartNumber,
                             int mediaSequence, string mediaSerialNumber, string mediaTitle, string outputPath,
                             string options, string resumeFile, string format, string geometry,
                             bool fixSubchannelPosition, bool fixSubchannel, bool fixSubchannelCrc,
                             bool generateSubchannels)
    {
        MainClass.PrintCopyright();

        if(debug)
        {
            IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(Console.Error)
            });

            AaruConsole.DebugWriteLineEvent += (format, objects) =>
            {
                if(objects is null)
                    stderrConsole.MarkupLine(format);
                else
                    stderrConsole.MarkupLine(format, objects);
            };
        }

        if(verbose)
            AaruConsole.WriteEvent += (format, objects) =>
            {
                if(objects is null)
                    AnsiConsole.Markup(format);
                else
                    AnsiConsole.Markup(format, objects);
            };

        if(fixSubchannelCrc)
            fixSubchannel = true;

        if(fixSubchannel)
            fixSubchannelPosition = true;

        Statistics.AddCommand("convert-image");

        AaruConsole.DebugWriteLine("Image convert command", "--cicm-xml={0}", cicmXml);
        AaruConsole.DebugWriteLine("Image convert command", "--comments={0}", comments);
        AaruConsole.DebugWriteLine("Image convert command", "--count={0}", count);
        AaruConsole.DebugWriteLine("Image convert command", "--creator={0}", creator);
        AaruConsole.DebugWriteLine("Image convert command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Image convert command", "--drive-manufacturer={0}", driveManufacturer);
        AaruConsole.DebugWriteLine("Image convert command", "--drive-model={0}", driveModel);
        AaruConsole.DebugWriteLine("Image convert command", "--drive-revision={0}", driveFirmwareRevision);
        AaruConsole.DebugWriteLine("Image convert command", "--drive-serial={0}", driveSerialNumber);
        AaruConsole.DebugWriteLine("Image convert command", "--force={0}", force);
        AaruConsole.DebugWriteLine("Image convert command", "--format={0}", format);
        AaruConsole.DebugWriteLine("Image convert command", "--geometry={0}", geometry);
        AaruConsole.DebugWriteLine("Image convert command", "--input={0}", inputPath);
        AaruConsole.DebugWriteLine("Image convert command", "--media-barcode={0}", mediaBarcode);
        AaruConsole.DebugWriteLine("Image convert command", "--media-lastsequence={0}", lastMediaSequence);
        AaruConsole.DebugWriteLine("Image convert command", "--media-manufacturer={0}", mediaManufacturer);
        AaruConsole.DebugWriteLine("Image convert command", "--media-model={0}", mediaModel);
        AaruConsole.DebugWriteLine("Image convert command", "--media-partnumber={0}", mediaPartNumber);
        AaruConsole.DebugWriteLine("Image convert command", "--media-sequence={0}", mediaSequence);
        AaruConsole.DebugWriteLine("Image convert command", "--media-serial={0}", mediaSerialNumber);
        AaruConsole.DebugWriteLine("Image convert command", "--media-title={0}", mediaTitle);
        AaruConsole.DebugWriteLine("Image convert command", "--options={0}", options);
        AaruConsole.DebugWriteLine("Image convert command", "--output={0}", outputPath);
        AaruConsole.DebugWriteLine("Image convert command", "--resume-file={0}", resumeFile);
        AaruConsole.DebugWriteLine("Image convert command", "--verbose={0}", verbose);
        AaruConsole.DebugWriteLine("Image convert command", "--fix-subchannel-position={0}", fixSubchannelPosition);
        AaruConsole.DebugWriteLine("Image convert command", "--fix-subchannel={0}", fixSubchannel);
        AaruConsole.DebugWriteLine("Image convert command", "--fix-subchannel-crc={0}", fixSubchannelCrc);
        AaruConsole.DebugWriteLine("Image convert command", "--generate-subchannels={0}", generateSubchannels);

        Dictionary<string, string> parsedOptions = Core.Options.Parse(options);
        AaruConsole.DebugWriteLine("Image convert command", "Parsed options:");

        foreach(KeyValuePair<string, string> parsedOption in parsedOptions)
            AaruConsole.DebugWriteLine("Image convert command", "{0} = {1}", parsedOption.Key, parsedOption.Value);

        if(count == 0)
        {
            AaruConsole.ErrorWriteLine("Need to specify more than 0 sectors to copy at once");

            return (int)ErrorNumber.InvalidArgument;
        }

        (uint cylinders, uint heads, uint sectors)? geometryValues = null;

        if(geometry != null)
        {
            string[] geometryPieces = geometry.Split('/');

            if(geometryPieces.Length == 0)
                geometryPieces = geometry.Split('-');

            if(geometryPieces.Length != 3)
            {
                AaruConsole.ErrorWriteLine("Invalid geometry specified");

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[0], out uint cylinders) ||
               cylinders == 0)
            {
                AaruConsole.ErrorWriteLine("Invalid number of cylinders specified");

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[1], out uint heads) ||
               heads == 0)
            {
                AaruConsole.ErrorWriteLine("Invalid number of heads specified");

                return (int)ErrorNumber.InvalidArgument;
            }

            if(!uint.TryParse(geometryPieces[2], out uint spt) ||
               spt == 0)
            {
                AaruConsole.ErrorWriteLine("Invalid sectors per track specified");

                return (int)ErrorNumber.InvalidArgument;
            }

            geometryValues = (cylinders, heads, spt);
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

                return (int)ErrorNumber.NoSuchFile;
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

                return (int)ErrorNumber.NoSuchFile;
            }

        var     filtersList = new FiltersList();
        IFilter inputFilter = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Identifying file filter...").IsIndeterminate();
            inputFilter = filtersList.GetFilter(inputPath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine("Cannot open specified file.");

            return (int)ErrorNumber.CannotOpenFile;
        }

        if(File.Exists(outputPath))
        {
            AaruConsole.ErrorWriteLine("Output file already exists, not continuing.");

            return (int)ErrorNumber.FileExists;
        }

        PluginBase  plugins     = GetPluginBase.Instance;
        IMediaImage inputFormat = null;
        IBaseImage  baseImage   = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Identifying image format...").IsIndeterminate();
            baseImage   = ImageFormat.Detect(inputFilter);
            inputFormat = baseImage as IMediaImage;
        });

        if(inputFormat == null)
        {
            AaruConsole.WriteLine("Input image format not identified, not proceeding with conversion.");

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        // TODO: Implement
        if(inputFormat == null)
        {
            AaruConsole.WriteLine("Command not yet supported for this image type.");

            return (int)ErrorNumber.InvalidArgument;
        }

        if(verbose)
            AaruConsole.VerboseWriteLine("Input image format identified by {0} ({1}).", inputFormat.Name,
                                         inputFormat.Id);
        else
            AaruConsole.WriteLine("Input image format identified by {0}.", inputFormat.Name);

        try
        {
            ErrorNumber opened = ErrorNumber.NoData;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening image file...").IsIndeterminate();
                opened = inputFormat.Open(inputFilter);
            });

            if(opened != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine("Unable to open image format");
                AaruConsole.WriteLine("Error {0}", opened);

                return (int)opened;
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

        List<IBaseWritableImage> candidates = new();

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

        IBaseWritableImage outputFormat = candidates[0];

        if(verbose)
            AaruConsole.VerboseWriteLine("Output image format: {0} ({1}).", outputFormat.Name, outputFormat.Id);
        else
            AaruConsole.WriteLine("Output image format: {0}.", outputFormat.Name);

        if(!outputFormat.SupportedMediaTypes.Contains(mediaType))
        {
            AaruConsole.ErrorWriteLine("Output format does not support media type, cannot continue...");

            return (int)ErrorNumber.UnsupportedMedia;
        }

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                    !outputFormat.SupportedMediaTags.Contains(mediaTag) && !force))
        {
            AaruConsole.ErrorWriteLine("Converting image will lose media tag {0}, not continuing...", mediaTag);
            AaruConsole.ErrorWriteLine("If you don't care, use force option.");

            return (int)ErrorNumber.DataWillBeLost;
        }

        bool useLong = inputFormat.Info.ReadableSectorTags.Count != 0;

        foreach(SectorTagType sectorTag in inputFormat.Info.ReadableSectorTags.Where(sectorTag =>
                    !outputFormat.SupportedSectorTags.Contains(sectorTag)))
        {
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

        var inputTape  = inputFormat as ITapeImage;
        var outputTape = outputFormat as IWritableTapeImage;

        if(inputTape?.IsTape == true &&
           outputTape is null)
        {
            AaruConsole.
                ErrorWriteLine("Input format contains a tape image and is not supported by output format, not continuing...");

            return (int)ErrorNumber.UnsupportedMedia;
        }

        var ret = false;

        if(inputTape?.IsTape == true &&
           outputTape        != null)
        {
            ret = outputTape.SetTape();

            // Cannot set image to tape mode
            if(!ret)
            {
                AaruConsole.ErrorWriteLine("Error setting output image in tape mode, not continuing...");
                AaruConsole.ErrorWriteLine(outputFormat.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }
        }

        if((outputFormat as IWritableOpticalImage)?.OpticalCapabilities.HasFlag(OpticalImageCapabilities.
                                                                                    CanStoreSessions) != true &&
           (inputFormat as IOpticalMediaImage)?.Sessions?.Count > 1)
        {
            // TODO: Disabled until 6.0
            /*if(!_force)
            {*/
            AaruConsole.
                ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, not continuing...");

            return (int)ErrorNumber.UnsupportedMedia;
            /*}

            AaruConsole.ErrorWriteLine("Output format does not support sessions, this will end in a loss of data, continuing...");*/
        }

        var created = false;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Opening image file...").IsIndeterminate();

            created = outputFormat.Create(outputPath, mediaType, parsedOptions, inputFormat.Info.Sectors,
                                          inputFormat.Info.SectorSize);
        });

        if(!created)
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

        foreach(MediaTagType mediaTag in inputFormat.Info.ReadableMediaTags.Where(mediaTag =>
                    !force || outputFormat.SupportedMediaTags.Contains(mediaTag)))
        {
            ErrorNumber ErrorNumber = ErrorNumber.NoError;

            AnsiConsole.Progress().AutoClear(false).HideCompleted(false).
                        Columns(new TaskDescriptionColumn(), new SpinnerColumn()).Start(ctx =>
                        {
                            ctx.AddTask($"Converting media tag {Markup.Escape(mediaTag.ToString())}");
                            ErrorNumber errno = inputFormat.ReadMediaTag(mediaTag, out byte[] tag);

                            if(errno != ErrorNumber.NoError)
                            {
                                if(force)
                                    AaruConsole.ErrorWriteLine("Error {0} reading media tag, continuing...", errno);
                                else
                                {
                                    AaruConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...", errno);

                                    ErrorNumber = errno;
                                }

                                return;
                            }

                            if((outputFormat as IWritableImage)?.WriteMediaTag(tag, mediaTag) == true)
                                return;

                            if(force)
                                AaruConsole.ErrorWriteLine("Error {0} writing media tag, continuing...",
                                                           outputFormat.ErrorMessage);
                            else
                            {
                                AaruConsole.ErrorWriteLine("Error {0} writing media tag, not continuing...",
                                                           outputFormat.ErrorMessage);

                                ErrorNumber = ErrorNumber.WriteError;
                            }
                        });

            if(ErrorNumber != ErrorNumber.NoError)
                return (int)ErrorNumber;
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
                                           outputOptical.ErrorMessage);

                return (int)ErrorNumber.WriteError;
            }

            ErrorNumber errno = ErrorNumber.NoError;

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask discTask = ctx.AddTask("Converting disc...");
                            discTask.MaxValue = inputOptical.Tracks.Count;

                            foreach(Track track in inputOptical.Tracks)
                            {
                                discTask.Description =
                                    $"Converting sectors in track {discTask.Value + 1} of {discTask.MaxValue}";

                                doneSectors = 0;
                                ulong trackSectors = track.EndSector - track.StartSector + 1;

                                ProgressTask trackTask = ctx.AddTask("Converting track");
                                trackTask.MaxValue = trackSectors;

                                while(doneSectors < trackSectors)
                                {
                                    byte[] sector;

                                    uint sectorsToDo;

                                    if(trackSectors - doneSectors >= (ulong)count)
                                        sectorsToDo = (uint)count;
                                    else
                                        sectorsToDo = (uint)(trackSectors - doneSectors);

                                    trackTask.Description =
                                        $"Converting sectors {doneSectors + track.StartSector} to {doneSectors + sectorsToDo + track.StartSector} in track {track.Sequence}";

                                    var useNotLong = false;
                                    var result     = false;

                                    if(useLong)
                                    {
                                        errno = sectorsToDo == 1
                                                    ? inputOptical.ReadSectorLong(doneSectors + track.StartSector,
                                                        out sector)
                                                    : inputOptical.ReadSectorsLong(doneSectors + track.StartSector,
                                                        sectorsToDo, out sector);

                                        if(errno == ErrorNumber.NoError)
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSectorLong(sector,
                                                             doneSectors + track.StartSector)
                                                         : outputOptical.WriteSectorsLong(sector,
                                                             doneSectors + track.StartSector, sectorsToDo);
                                        else
                                        {
                                            result = true;

                                            if(force)
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} reading sector {1}, continuing...", errno,
                                                                   doneSectors + track.StartSector);
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} reading sector {1}, not continuing...",
                                                                   errno, doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }

                                        if(!result &&
                                           sector.Length % 2352 != 0)
                                        {
                                            if(!force)
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine("Input image is not returning raw sectors, use force if you want to continue...");

                                                errno = ErrorNumber.InOutError;

                                                return;
                                            }

                                            useNotLong = true;
                                        }
                                    }

                                    if(!useLong || useNotLong)
                                    {
                                        errno = sectorsToDo == 1
                                                    ? inputOptical.ReadSector(doneSectors + track.StartSector,
                                                                              out sector)
                                                    : inputOptical.ReadSectors(doneSectors + track.StartSector,
                                                                               sectorsToDo, out sector);

                                        if(errno == ErrorNumber.NoError)
                                            result = sectorsToDo == 1
                                                         ? outputOptical.WriteSector(sector,
                                                             doneSectors + track.StartSector)
                                                         : outputOptical.WriteSectors(sector,
                                                             doneSectors + track.StartSector, sectorsToDo);
                                        else
                                        {
                                            result = true;

                                            if(force)
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} reading sector {1}, continuing...", errno,
                                                                   doneSectors + track.StartSector);
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} reading sector {1}, not continuing...",
                                                                   errno, doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }
                                        }
                                    }

                                    if(!result)
                                        if(force)
                                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                                       outputOptical.ErrorMessage,
                                                                       doneSectors + track.StartSector);
                                        else
                                        {
                                            AaruConsole.
                                                ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                               outputOptical.ErrorMessage,
                                                               doneSectors + track.StartSector);

                                            errno = ErrorNumber.WriteError;

                                            return;
                                        }

                                    doneSectors     += sectorsToDo;
                                    trackTask.Value += sectorsToDo;
                                }

                                trackTask.StopTask();
                                discTask.Increment(1);
                            }
                        });

            if(errno != ErrorNumber.NoError)
                return (int)errno;

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

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackIsrc).OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, tag, out byte[] isrc);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    isrcs[(byte)track.Sequence] = Encoding.UTF8.GetString(isrc);
                }
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.
                                                      Where(t => t == SectorTagType.CdTrackFlags).OrderBy(t => t))
            {
                foreach(Track track in tracks)
                {
                    errno = inputOptical.ReadSectorTag(track.Sequence, tag, out byte[] flags);

                    if(errno != ErrorNumber.NoError)
                        continue;

                    trackFlags[(byte)track.Sequence] = flags[0];
                }
            }

            for(ulong s = 0; s < inputOptical.Info.Sectors; s++)
            {
                if(s > int.MaxValue)
                    break;

                subchannelExtents.Add((int)s);
            }

            foreach(SectorTagType tag in inputOptical.Info.ReadableSectorTags.OrderBy(t => t).TakeWhile(_ => useLong))
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

                if(force && !outputOptical.SupportedSectorTags.Contains(tag))
                    continue;

                errno = ErrorNumber.NoError;

                AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                            Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                            Start(ctx =>
                            {
                                ProgressTask discTask = ctx.AddTask("Converting disc...");
                                discTask.MaxValue = inputOptical.Tracks.Count;

                                foreach(Track track in inputOptical.Tracks)
                                {
                                    discTask.Description =
                                        $"Converting tags in track {discTask.Value + 1} of {discTask.MaxValue}";

                                    doneSectors = 0;
                                    ulong  trackSectors = track.EndSector - track.StartSector + 1;
                                    byte[] sector;
                                    bool   result;

                                    switch(tag)
                                    {
                                        case SectorTagType.CdTrackFlags:
                                        case SectorTagType.CdTrackIsrc:
                                            errno = inputOptical.ReadSectorTag(track.Sequence, tag, out sector);

                                            if(errno == ErrorNumber.NoData)
                                            {
                                                errno = ErrorNumber.NoError;

                                                continue;
                                            }

                                            if(errno == ErrorNumber.NoError)
                                                result = outputOptical.WriteSectorTag(sector, track.Sequence, tag);
                                            else
                                            {
                                                if(force)
                                                {
                                                    AaruConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                               outputOptical.ErrorMessage);

                                                    continue;
                                                }

                                                AaruConsole.ErrorWriteLine("Error {0} writing tag, not continuing...",
                                                                           outputOptical.ErrorMessage);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }

                                            if(!result)
                                                if(force)
                                                    AaruConsole.ErrorWriteLine("Error {0} writing tag, continuing...",
                                                                               outputOptical.ErrorMessage);
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine("Error {0} writing tag, not continuing...",
                                                                       outputOptical.ErrorMessage);

                                                    errno = ErrorNumber.WriteError;

                                                    return;
                                                }

                                            continue;
                                    }

                                    ProgressTask trackTask = ctx.AddTask("Converting track");
                                    trackTask.MaxValue = trackSectors;

                                    while(doneSectors < trackSectors)
                                    {
                                        uint sectorsToDo;

                                        if(trackSectors - doneSectors >= (ulong)count)
                                            sectorsToDo = (uint)count;
                                        else
                                            sectorsToDo = (uint)(trackSectors - doneSectors);

                                        trackTask.Description =
                                            string.Format("Converting tag {3} for sectors {0} to {1} in track {2}",
                                                          doneSectors + track.StartSector,
                                                          doneSectors + sectorsToDo + track.StartSector, track.Sequence,
                                                          tag);

                                        if(sectorsToDo == 1)
                                        {
                                            errno = inputOptical.ReadSectorTag(doneSectors + track.StartSector, tag,
                                                                               out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw, sector,
                                                            doneSectors + track.StartSector, 1, null, isrcs,
                                                            (byte)track.Sequence, ref mcn, tracks,
                                                            subchannelExtents, fixSubchannelPosition,
                                                            outputOptical, fixSubchannel, fixSubchannelCrc, null,
                                                            null, smallestPregapLbaPerTrack, false);

                                                    if(indexesChanged)
                                                        outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                    result =
                                                        outputOptical.WriteSectorTag(sector,
                                                            doneSectors + track.StartSector, tag);
                                            }
                                            else
                                            {
                                                result = true;

                                                if(force)
                                                    AaruConsole.
                                                        ErrorWriteLine("Error {0} reading tag for sector {1}, continuing...",
                                                                       errno, doneSectors + track.StartSector);
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine("Error {0} reading tag for sector {1}, not continuing...",
                                                                       errno, doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            errno = inputOptical.ReadSectorsTag(doneSectors + track.StartSector,
                                                                                    sectorsToDo, tag, out sector);

                                            if(errno == ErrorNumber.NoError)
                                            {
                                                if(tag == SectorTagType.CdSectorSubchannel)
                                                {
                                                    bool indexesChanged =
                                                        CompactDisc.WriteSubchannelToImage(MmcSubchannel.Raw,
                                                            MmcSubchannel.Raw, sector,
                                                            doneSectors + track.StartSector, sectorsToDo, null,
                                                            isrcs, (byte)track.Sequence, ref mcn, tracks,
                                                            subchannelExtents, fixSubchannelPosition,
                                                            outputOptical, fixSubchannel, fixSubchannelCrc, null,
                                                            null, smallestPregapLbaPerTrack, false);

                                                    if(indexesChanged)
                                                        outputOptical.SetTracks(tracks.ToList());

                                                    result = true;
                                                }
                                                else
                                                    result =
                                                        outputOptical.WriteSectorsTag(sector,
                                                            doneSectors + track.StartSector, sectorsToDo, tag);
                                            }
                                            else
                                            {
                                                result = true;

                                                if(force)
                                                    AaruConsole.
                                                        ErrorWriteLine("Error {0} reading tag for sector {1}, continuing...",
                                                                       errno, doneSectors + track.StartSector);
                                                else
                                                {
                                                    AaruConsole.
                                                        ErrorWriteLine("Error {0} reading tag for sector {1}, not continuing...",
                                                                       errno, doneSectors + track.StartSector);

                                                    return;
                                                }
                                            }
                                        }

                                        if(!result)
                                            if(force)
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} writing tag for sector {1}, continuing...",
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector);
                                            else
                                            {
                                                AaruConsole.
                                                    ErrorWriteLine("Error {0} writing tag for sector {1}, not continuing...",
                                                                   outputOptical.ErrorMessage,
                                                                   doneSectors + track.StartSector);

                                                errno = ErrorNumber.WriteError;

                                                return;
                                            }

                                        doneSectors     += sectorsToDo;
                                        trackTask.Value += sectorsToDo;
                                    }

                                    trackTask.StopTask();
                                    discTask.Increment(1);
                                }
                            });

                if(errno != ErrorNumber.NoError &&
                   !force)
                    return (int)errno;
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

            // TODO: Progress
            if(inputOptical.Info.MediaType is MediaType.CD or MediaType.CDDA or MediaType.CDG or MediaType.CDEG
                                           or MediaType.CDI or MediaType.CDROM or MediaType.CDROMXA or MediaType.CDPLUS
                                           or MediaType.CDMO or MediaType.CDR or MediaType.CDRW or MediaType.CDMRW
                                           or MediaType.VCD or MediaType.SVCD or MediaType.PCD or MediaType.DTSCD
                                           or MediaType.CDMIDI or MediaType.CDV or MediaType.CDIREADY
                                           or MediaType.FMTOWNS or MediaType.PS1CD or MediaType.PS2CD
                                           or MediaType.MEGACD or MediaType.SATURNCD or MediaType.GDROM or MediaType.GDR
                                           or MediaType.MilCD or MediaType.SuperCDROM2 or MediaType.JaguarCD
                                           or MediaType.ThreeDO or MediaType.PCFX or MediaType.NeoGeoCD
                                           or MediaType.CDTV or MediaType.CD32 or MediaType.Playdia or MediaType.Pippin
                                           or MediaType.VideoNow or MediaType.VideoNowColor or MediaType.VideoNowXp
                                           or MediaType.CVD && generateSubchannels)
                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Generating subchannels...").IsIndeterminate();

                    CompactDisc.GenerateSubchannels(subchannelExtents, tracks, trackFlags, inputOptical.Info.Sectors,
                                                    null, null, null, null, null, outputOptical);
                });
        }
        else
        {
            var outputMedia = outputFormat as IWritableImage;

            if(inputTape  == null ||
               outputTape == null ||
               !inputTape.IsTape)
            {
                (uint cylinders, uint heads, uint sectors) chs =
                    geometryValues != null
                        ? (geometryValues.Value.cylinders, geometryValues.Value.heads, geometryValues.Value.sectors)
                        : (inputFormat.Info.Cylinders, inputFormat.Info.Heads, inputFormat.Info.SectorsPerTrack);

                AaruConsole.WriteLine("Setting geometry to {0} cylinders, {1} heads and {2} sectors per track",
                                      chs.cylinders, chs.heads, chs.sectors);

                if(!outputMedia.SetGeometry(chs.cylinders, chs.heads, chs.sectors))
                    AaruConsole.ErrorWriteLine("Error {0} setting geometry, image may be incorrect, continuing...",
                                               outputMedia.ErrorMessage);
            }

            ErrorNumber errno = ErrorNumber.NoError;

            AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
                        Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn()).
                        Start(ctx =>
                        {
                            ProgressTask mediaTask = ctx.AddTask("Converting media");
                            mediaTask.MaxValue = inputFormat.Info.Sectors;

                            while(doneSectors < inputFormat.Info.Sectors)
                            {
                                byte[] sector;

                                uint sectorsToDo;

                                if(inputTape?.IsTape == true)
                                    sectorsToDo = 1;
                                else if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                                    sectorsToDo = (uint)count;
                                else
                                    sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                mediaTask.Description =
                                    $"Converting sectors {doneSectors} to {doneSectors + sectorsToDo}";

                                bool result;

                                if(useLong)
                                {
                                    errno = sectorsToDo == 1 ? inputFormat.ReadSectorLong(doneSectors, out sector)
                                                : inputFormat.ReadSectorsLong(doneSectors, sectorsToDo, out sector);

                                    if(errno == ErrorNumber.NoError)
                                        result = sectorsToDo == 1 ? outputMedia.WriteSectorLong(sector, doneSectors)
                                                     : outputMedia.WriteSectorsLong(sector, doneSectors, sectorsToDo);
                                    else
                                    {
                                        result = true;

                                        if(force)
                                            AaruConsole.ErrorWriteLine("Error {0} reading sector {1}, continuing...",
                                                                       errno, doneSectors);
                                        else
                                        {
                                            AaruConsole.
                                                ErrorWriteLine("Error {0} reading sector {1}, not continuing...", errno,
                                                               doneSectors);

                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    errno = sectorsToDo == 1 ? inputFormat.ReadSector(doneSectors, out sector)
                                                : inputFormat.ReadSectors(doneSectors, sectorsToDo, out sector);

                                    if(errno == ErrorNumber.NoError)
                                        result = sectorsToDo == 1 ? outputMedia.WriteSector(sector, doneSectors)
                                                     : outputMedia.WriteSectors(sector, doneSectors, sectorsToDo);
                                    else
                                    {
                                        result = true;

                                        if(force)
                                            AaruConsole.ErrorWriteLine("Error {0} reading sector {1}, continuing...",
                                                                       errno, doneSectors);
                                        else
                                        {
                                            AaruConsole.
                                                ErrorWriteLine("Error {0} reading sector {1}, not continuing...", errno,
                                                               doneSectors);

                                            return;
                                        }
                                    }
                                }

                                if(!result)
                                    if(force)
                                        AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                                   outputMedia.ErrorMessage, doneSectors);
                                    else
                                    {
                                        AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                                   outputMedia.ErrorMessage, doneSectors);

                                        errno = ErrorNumber.WriteError;

                                        return;
                                    }

                                doneSectors     += sectorsToDo;
                                mediaTask.Value += sectorsToDo;
                            }

                            mediaTask.StopTask();

                            foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags.TakeWhile(_ => useLong))
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

                                if(force && !outputMedia.SupportedSectorTags.Contains(tag))
                                    continue;

                                doneSectors = 0;

                                ProgressTask tagsTask = ctx.AddTask("Converting tags");
                                tagsTask.MaxValue = inputFormat.Info.Sectors;

                                while(doneSectors < inputFormat.Info.Sectors)
                                {
                                    byte[] sector;

                                    uint sectorsToDo;

                                    if(inputFormat.Info.Sectors - doneSectors >= (ulong)count)
                                        sectorsToDo = (uint)count;
                                    else
                                        sectorsToDo = (uint)(inputFormat.Info.Sectors - doneSectors);

                                    tagsTask.Description =
                                        string.Format("Converting tag {2} for sectors {0} to {1}", doneSectors,
                                                      doneSectors + sectorsToDo, tag);

                                    bool result;

                                    errno = sectorsToDo == 1 ? inputFormat.ReadSectorTag(doneSectors, tag, out sector)
                                                : inputFormat.ReadSectorsTag(doneSectors, sectorsToDo, tag, out sector);

                                    if(errno == ErrorNumber.NoError)
                                        result = sectorsToDo == 1 ? outputMedia.WriteSectorTag(sector, doneSectors, tag)
                                                     : outputMedia.WriteSectorsTag(sector, doneSectors, sectorsToDo,
                                                         tag);
                                    else
                                    {
                                        result = true;

                                        if(force)
                                            AaruConsole.ErrorWriteLine("Error {0} reading sector {1}, continuing...",
                                                                       errno, doneSectors);
                                        else
                                        {
                                            AaruConsole.
                                                ErrorWriteLine("Error {0} reading sector {1}, not continuing...", errno,
                                                               doneSectors);

                                            return;
                                        }
                                    }

                                    if(!result)
                                        if(force)
                                            AaruConsole.ErrorWriteLine("Error {0} writing sector {1}, continuing...",
                                                                       outputMedia.ErrorMessage, doneSectors);
                                        else
                                        {
                                            AaruConsole.
                                                ErrorWriteLine("Error {0} writing sector {1}, not continuing...",
                                                               outputMedia.ErrorMessage, doneSectors);

                                            errno = ErrorNumber.WriteError;

                                            return;
                                        }

                                    doneSectors    += sectorsToDo;
                                    tagsTask.Value += sectorsToDo;
                                }

                                tagsTask.StopTask();
                            }

                            if(inputTape  == null ||
                               outputTape == null ||
                               !inputTape.IsTape)
                                return;

                            ProgressTask filesTask = ctx.AddTask("Converting files");
                            filesTask.MaxValue = inputTape.Files.Count;

                            foreach(TapeFile tapeFile in inputTape.Files)
                            {
                                filesTask.Description =
                                    $"Converting file {tapeFile.File} of partition {tapeFile.Partition}...";

                                outputTape.AddFile(tapeFile);
                                filesTask.Increment(1);
                            }

                            filesTask.StopTask();

                            ProgressTask partitionTask = ctx.AddTask("Converting files");
                            partitionTask.MaxValue = inputTape.TapePartitions.Count;

                            foreach(TapePartition tapePartition in inputTape.TapePartitions)
                            {
                                partitionTask.Description = $"Converting tape partition {tapePartition.Number}...";

                                outputTape.AddPartition(tapePartition);
                            }

                            partitionTask.StopTask();
                        });

            if(errno != ErrorNumber.NoError)
                return (int)errno;
        }

        if(resume       != null ||
           dumpHardware != null)
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Writing dump hardware list...").IsIndeterminate();

                if(resume != null)
                    ret = outputFormat.SetDumpHardware(resume.Tries);
                else if(dumpHardware != null)
                    ret = outputFormat.SetDumpHardware(dumpHardware);
            });

            if(ret)
                AaruConsole.WriteLine("Written dump hardware list to output image.");
        }

        ret = false;

        if(sidecar      != null ||
           cicmMetadata != null)
        {
            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Writing metadata...").IsIndeterminate();

                if(sidecar != null)
                    ret = outputFormat.SetCicmMetadata(sidecar);
                else if(cicmMetadata != null)
                    ret = outputFormat.SetCicmMetadata(cicmMetadata);
            });

            if(ret)
                AaruConsole.WriteLine("Written CICM XML metadata to output image.");
        }

        var closed = false;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Closing output image...").IsIndeterminate();
            closed = outputFormat.Close();
        });

        if(!closed)
        {
            AaruConsole.ErrorWriteLine("Error {0} closing output image... Contents are not correct.",
                                       outputFormat.ErrorMessage);

            return (int)ErrorNumber.WriteError;
        }

        AaruConsole.WriteLine("Conversion done.");

        return (int)ErrorNumber.NoError;
    }
}