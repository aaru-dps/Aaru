// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Decode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'decode' verb.
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
    internal class DecodeCommand : Command
    {
        public DecodeCommand() : base("decode", "Decodes and pretty prints disk and/or sector tags.")
        {
            Add(new Option(new[]
                {
                    "--disk-tags", "-f"
                }, "Decode disk tags.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--length", "-l"
                }, "How many sectors to decode, or \"all\".")
                {
                    Argument = new Argument<string>(() => "all"), Required = false
                });

            Add(new Option(new[]
                {
                    "--sector-tags", "-p"
                }, "Decode sector tags.")
                {
                    Argument = new Argument<bool>(() => true), Required = false
                });

            Add(new Option(new[]
                {
                    "--start", "-s"
                }, "Sector to start decoding from.")
                {
                    Argument = new Argument<ulong>(() => 0), Required = false
                });

            AddArgument(new Argument<string>
            {
                Arity = ArgumentArity.ExactlyOne, Description = "Media image path", Name = "image-path"
            });

            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
        }

        public static int Invoke(bool verbose, bool debug, bool diskTags, string imagePath, string length,
                                 bool sectorTags, ulong startSector)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("decode");

            DicConsole.DebugWriteLine("Decode command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Decode command", "--disk-tags={0}", diskTags);
            DicConsole.DebugWriteLine("Decode command", "--input={0}", imagePath);
            DicConsole.DebugWriteLine("Decode command", "--length={0}", length);
            DicConsole.DebugWriteLine("Decode command", "--sector-tags={0}", sectorTags);
            DicConsole.DebugWriteLine("Decode command", "--start={0}", startSector);
            DicConsole.DebugWriteLine("Decode command", "--verbose={0}", verbose);

            var     filtersList = new FiltersList();
            IFilter inputFilter = filtersList.GetFilter(imagePath);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");

                return(int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not decoding");

                return(int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);
            Statistics.AddMediaFormat(inputFormat.Format);
            Statistics.AddMedia(inputFormat.Info.MediaType, false);
            Statistics.AddFilter(inputFilter.Name);

            if(diskTags)
                if(inputFormat.Info.ReadableMediaTags.Count == 0)
                    DicConsole.WriteLine("There are no disk tags in chosen disc image.");
                else
                    foreach(MediaTagType tag in inputFormat.Info.ReadableMediaTags)
                        switch(tag)
                        {
                            case MediaTagType.SCSI_INQUIRY:
                            {
                                byte[] inquiry = inputFormat.ReadDiskTag(MediaTagType.SCSI_INQUIRY);

                                if(inquiry == null)
                                    DicConsole.WriteLine("Error reading SCSI INQUIRY response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("SCSI INQUIRY command response:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(Inquiry.Prettify(inquiry));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATA_IDENTIFY:
                            {
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATA_IDENTIFY);

                                if(identify == null)
                                    DicConsole.WriteLine("Error reading ATA IDENTIFY DEVICE response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("ATA IDENTIFY DEVICE command response:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(Identify.Prettify(identify));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.ATAPI_IDENTIFY:
                            {
                                byte[] identify = inputFormat.ReadDiskTag(MediaTagType.ATAPI_IDENTIFY);

                                if(identify == null)
                                    DicConsole.
                                        WriteLine("Error reading ATA IDENTIFY PACKET DEVICE response from disc image");
                                else
                                {
                                    DicConsole.WriteLine("ATA IDENTIFY PACKET DEVICE command response:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(Identify.Prettify(identify));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_ATIP:
                            {
                                byte[] atip = inputFormat.ReadDiskTag(MediaTagType.CD_ATIP);

                                if(atip == null)
                                    DicConsole.WriteLine("Error reading CD ATIP from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD ATIP:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(ATIP.Prettify(atip));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_FullTOC:
                            {
                                byte[] fullToc = inputFormat.ReadDiskTag(MediaTagType.CD_FullTOC);

                                if(fullToc == null)
                                    DicConsole.WriteLine("Error reading CD full TOC from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD full TOC:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(FullTOC.Prettify(fullToc));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_PMA:
                            {
                                byte[] pma = inputFormat.ReadDiskTag(MediaTagType.CD_PMA);

                                if(pma == null)
                                    DicConsole.WriteLine("Error reading CD PMA from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD PMA:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(PMA.Prettify(pma));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_SessionInfo:
                            {
                                byte[] sessionInfo = inputFormat.ReadDiskTag(MediaTagType.CD_SessionInfo);

                                if(sessionInfo == null)
                                    DicConsole.WriteLine("Error reading CD session information from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD session information:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(Session.Prettify(sessionInfo));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TEXT:
                            {
                                byte[] cdText = inputFormat.ReadDiskTag(MediaTagType.CD_TEXT);

                                if(cdText == null)
                                    DicConsole.WriteLine("Error reading CD-TEXT from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD-TEXT:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(CDTextOnLeadIn.Prettify(cdText));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            case MediaTagType.CD_TOC:
                            {
                                byte[] toc = inputFormat.ReadDiskTag(MediaTagType.CD_TOC);

                                if(toc == null)
                                    DicConsole.WriteLine("Error reading CD TOC from disc image");
                                else
                                {
                                    DicConsole.WriteLine("CD TOC:");

                                    DicConsole.
                                        WriteLine("================================================================================");

                                    DicConsole.WriteLine(TOC.Prettify(toc));

                                    DicConsole.
                                        WriteLine("================================================================================");
                                }

                                break;
                            }
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                     tag);

                                break;
                        }

            if(sectorTags)
            {
                if(length.ToLowerInvariant() == "all") { }
                else
                {
                    if(!ulong.TryParse(length, out ulong _))
                    {
                        DicConsole.WriteLine("Value \"{0}\" is not a valid number for length.", length);
                        DicConsole.WriteLine("Not decoding sectors tags");

                        return 3;
                    }
                }

                if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    DicConsole.WriteLine("There are no sector tags in chosen disc image.");
                else
                    foreach(SectorTagType tag in inputFormat.Info.ReadableSectorTags)
                        switch(tag)
                        {
                            default:
                                DicConsole.WriteLine("Decoder for disk tag type \"{0}\" not yet implemented, sorry.",
                                                     tag);

                                break;
                        }

                // TODO: Not implemented
            }

            return(int)ErrorNumber.NoError;
        }
    }
}