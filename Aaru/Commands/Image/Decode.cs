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
// Copyright © 2011-2023 Natalia Portillo
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
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("decode");

            AaruConsole.DebugWriteLine("Decode command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Decode command", "--disk-tags={0}", diskTags);
            AaruConsole.DebugWriteLine("Decode command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("Decode command", "--length={0}", length);
            AaruConsole.DebugWriteLine("Decode command", "--sector-tags={0}", sectorTags);
            AaruConsole.DebugWriteLine("Decode command", "--start={0}", startSector);
            AaruConsole.DebugWriteLine("Decode command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                AaruConsole.ErrorWriteLine("Cannot open specified file.");

                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                AaruConsole.ErrorWriteLine("Unable to recognize image format, not decoding");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            if(diskTags)
                if(inputFormat.Info.ReadableMediaTags.Count == 0)
                    AaruConsole.WriteLine("There are no disk tags in chosen disc image.");
                else
                    foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                        switch(tag)
                        {
                            case MediaTagType.SCSI_INQUIRY:
                            {
                                byte[] inquiry = inputFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);

                                if(inquiry == null)
                                    AaruConsole.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("SCSI INQUIRY command response:");

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
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

                                if(identify == null)
                                    AaruConsole.WriteLine("Error reading ATA IDENTIFY DEVICE response from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("ATA IDENTIFY DEVICE command response:");

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
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

                                if(identify == null)
                                    AaruConsole.
                                        WriteLine("Error reading ATA IDENTIFY PACKET DEVICE response from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("ATA IDENTIFY PACKET DEVICE command response:");

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
                                byte[] atip = inputFormat.ReadDiskTag(MediaTagType.CD_ATIP);

                                if(atip == null)
                                    AaruConsole.WriteLine("Error reading CD ATIP from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD ATIP:");

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
                                byte[] fullToc = inputFormat.ReadDiskTag(MediaTagType.CD_FullTOC);

                                if(fullToc == null)
                                    AaruConsole.WriteLine("Error reading CD full TOC from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD full TOC:");

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
                                byte[] pma = inputFormat.ReadDiskTag(MediaTagType.CD_PMA);

                                if(pma == null)
                                    AaruConsole.WriteLine("Error reading CD PMA from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD PMA:");

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
                                byte[] sessionInfo = inputFormat.ReadDiskTag(MediaTagType.CD_SessionInfo);

                                if(sessionInfo == null)
                                    AaruConsole.WriteLine("Error reading CD session information from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD session information:");

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
                                byte[] cdText = inputFormat.ReadDiskTag(MediaTagType.CD_TEXT);

                                if(cdText == null)
                                    AaruConsole.WriteLine("Error reading CD-TEXT from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD-TEXT:");

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
                                byte[] toc = inputFormat.ReadDiskTag(MediaTagType.CD_TOC);

                                if(toc == null)
                                    AaruConsole.WriteLine("Error reading CD TOC from disc image");
                                else
                                {
                                    AaruConsole.WriteLine("CD TOC:");

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