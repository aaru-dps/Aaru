// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Decode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'decode' command.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Decoders.ATA;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Spectre.Console;

namespace Aaru.Commands.Image
{
    internal sealed class DecodeCommand : Command
    {
        public DecodeCommand() : base("decode", "Decodes and pretty prints disk and/or sector tags.")
        {
            Add(new Option(new[]
                {
                    "--disk-tags", "-f"
                }, "Decode disk tags.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--length", "-l"
                }, "How many sectors to decode, or \"all\".")
                {
                    Argument = new Argument<string>(() => "all"),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--sector-tags", "-p"
                }, "Decode sector tags.")
                {
                    Argument = new Argument<bool>(() => true),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--start", "-s"
                }, "Sector to start decoding from.")
                {
                    Argument = new Argument<ulong>(() => 0),
                    Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity       = ArgumentArity.ExactlyOne,
                Description = "Media image path",
                Name        = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool verbose, bool debug, bool diskTags, string imagePath, string length,
                                 bool sectorTags, ulong startSector)
        {
            MainClass.PrintCopyright();

            if(debug)
            {
                IAnsiConsole stderrConsole = AnsiConsole.Create(new AnsiConsoleSettings
                {
                    Out = new AnsiConsoleOutput(System.Console.Error)
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

            Statistics.AddCommand("decode");

            AaruConsole.DebugWriteLine("Decode command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Decode command", "--disk-tags={0}", diskTags);
            AaruConsole.DebugWriteLine("Decode command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Decode command", "--length={0}", length);
            AaruConsole.DebugWriteLine("Decode command", "--sector-tags={0}", sectorTags);
            AaruConsole.DebugWriteLine("Decode command", "--start={0}", startSector);
            AaruConsole.DebugWriteLine("Decode command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying file filter...").IsIndeterminate();
                inputFilter = filtersList.GetFilter(imagePath);
            });

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = null;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Identifying image format...").IsIndeterminate();
                inputFormat = ImageFormat.Detect(inputFilter);
            });

            if(inputFormat == null)
            {
                AaruConsole.ErrorWriteLine("Unable to recognize image format, not decoding");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            ErrorNumber opened = ErrorNumber.NoData;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Opening image file...").IsIndeterminate();
                opened = inputFormat.Open(inputFilter);
            });

            if(opened != ErrorNumber.NoError)
            {
                AaruConsole.WriteLine("Error {opened} opening image format");

                return (int)opened;
            }

            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);
            ErrorNumber errno;

            if(diskTags)
                if(inputFormat.Info.ReadableMediaTags.Count == 0)
                    AaruConsole.WriteLine("There are no disk tags in chosen disc image.");
                else
                    foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                        switch(tag)
                        {
                            case MediaTagType.SCSI_INQUIRY:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.SCSI_INQUIRY, out byte[] inquiry);

                                if(inquiry == null)
                                    AaruConsole.WriteLine("Error {0} reading SCSI INQUIRY response from disc image",
                                                          errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]SCSI INQUIRY command response:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(Inquiry.Prettify(inquiry));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATA_IDENTIFY:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.ATA_IDENTIFY, out byte[] identify);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.
                                        WriteLine("Error {0} reading ATA IDENTIFY DEVICE response from disc image",
                                                  errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]ATA IDENTIFY DEVICE command response:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(Identify.Prettify(identify));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATAPI_IDENTIFY:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.ATAPI_IDENTIFY, out byte[] identify);

                                if(identify == null)
                                    AaruConsole.
                                        WriteLine("Error {0} reading ATA IDENTIFY PACKET DEVICE response from disc image",
                                                  errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]ATA IDENTIFY PACKET DEVICE command response:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(Identify.Prettify(identify));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_ATIP:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_ATIP, out byte[] atip);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.WriteLine("Error {0} reading CD ATIP from disc image", errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD ATIP:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(ATIP.Prettify(atip));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_FullTOC:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_FullTOC, out byte[] fullToc);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.WriteLine("Error {0} reading CD full TOC from disc image", errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD full TOC:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(FullTOC.Prettify(fullToc));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_PMA:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_PMA, out byte[] pma);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.WriteLine("Error {0} reading CD PMA from disc image", errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD PMA:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(PMA.Prettify(pma));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_SessionInfo:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_SessionInfo, out byte[] sessionInfo);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.WriteLine("Error {0} reading CD session information from disc image",
                                                          errno);
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD session information:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(Session.Prettify(sessionInfo));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TEXT:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_TEXT, out byte[] cdText);

                                if(errno != ErrorNumber.NoError)
                                    AaruConsole.WriteLine("Error reading CD-TEXT from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD-TEXT:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(CDTextOnLeadIn.Prettify(cdText));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TOC:
                            {
                                errno = inputFormat.ReadMediaTag(MediaTagType.CD_TOC, out byte[] toc);

                                if(toc == null)
                                    AaruConsole.WriteLine("Error reading CD TOC from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("[bold]CD TOC:[/]");

                                    AaruConsole.
                                        WriteLine("================================================================================");

                                    AaruConsole.WriteLine(TOC.Prettify(toc));

                                    AaruConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            default:
                                AaruConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                      tag);

                                break;
                        }

            if(sectorTags)
            {
                if(length.ToLowerInvariant() == "all") {}
                else
                {
                    if(!ulong.TryParse(length, out ulong _))
                    {
                        AaruConsole.WriteLine("Value \"{0}\" is not a valid number for length.", length);
                        AaruConsole.WriteLine("Not decoding sectors tags");

                        return 3;
                    }
                }

                if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    AaruConsole.WriteLine("There are no sector tags in chosen disc image.");
                else
                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                        switch(tag)
                        {
                            default:
                                AaruConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                      tag);

                                break;
                        }

                // TODO: Not implemented
            }

            return (int)ErrorNumber.NoError;
        }
    }
}