// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Print.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'print' command.
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

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Image;

sealed class PrintHexCommand : Command
{
    public PrintHexCommand() : base("print", UI.Image_Print_Command_Description)
    {
        Add(new Option<ulong>(new[]
        {
            "--length", "-l"
        }, () => 1, UI.How_many_sectors_to_print));

        Add(new Option<bool>(new[]
        {
            "--long-sectors", "-r"
        }, () => false, UI.Print_sectors_with_tags_included));

        Add(new Option<ulong>(new[]
        {
            "--start", "-s"
        }, UI.Starting_sector));

        Add(new Option<ushort>(new[]
        {
            "--width", "-w"
        }, () => 32, UI.How_many_bytes_to_print_per_line));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = UI.Media_image_path,
            Name        = "image-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string imagePath, ulong length, bool longSectors, ulong start,
                             ushort width)
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

        Statistics.AddCommand("print-hex");

        AaruConsole.DebugWriteLine("PrintHex command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("PrintHex command", "--input={0}", imagePath);
        AaruConsole.DebugWriteLine("PrintHex command", "--length={0}", length);
        AaruConsole.DebugWriteLine("PrintHex command", "--long-sectors={0}", longSectors);
        AaruConsole.DebugWriteLine("PrintHex command", "--start={0}", start);
        AaruConsole.DebugWriteLine("PrintHex command", "--verbose={0}", verbose);
        AaruConsole.DebugWriteLine("PrintHex command", "--width={0}", width);

        var     filtersList = new FiltersList();
        IFilter inputFilter = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_file_filter).IsIndeterminate();
            inputFilter = filtersList.GetFilter(imagePath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine(UI.Cannot_open_specified_file);

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Identifying_image_format).IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat == null)
        {
            AaruConsole.ErrorWriteLine(UI.Unable_to_recognize_image_format_not_printing);

            return (int)ErrorNumber.UnrecognizedFormat;
        }

        ErrorNumber opened = ErrorNumber.NoData;

        Core.Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(UI.Invoke_Opening_image_file).IsIndeterminate();
            opened = inputFormat.Open(inputFilter);
        });

        if(opened != ErrorNumber.NoError)
        {
            AaruConsole.WriteLine(UI.Unable_to_open_image_format);
            AaruConsole.WriteLine(Localization.Core.Error_0, opened);

            return (int)opened;
        }

        if(inputFormat.Info.XmlMediaType == XmlMediaType.LinearMedia)
        {
            var byteAddressableImage = inputFormat as IByteAddressableImage;

            AaruConsole.WriteLine($"[bold][italic]{string.Format(UI.Start_0_as_in_sector_start, start)}[/][/]");

            byte[]      data      = new byte[length];
            ErrorNumber errno     = ErrorNumber.NoError;
            int         bytesRead = 0;

            Core.Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask(UI.Reading_data).IsIndeterminate();

                errno = byteAddressableImage?.ReadBytesAt((long)start, data, 0, (int)length, out bytesRead) ??
                        ErrorNumber.InvalidArgument;
            });

            // TODO: Span
            if(bytesRead != (int)length)
            {
                byte[] tmp = new byte[bytesRead];
                Array.Copy(data, 0, tmp, 0, bytesRead);
                data = tmp;
            }

            if(errno == ErrorNumber.NoError)
                AaruConsole.WriteLine(Markup.Escape(PrintHex.ByteArrayToHexArrayString(data, width, true)));
            else
                AaruConsole.ErrorWriteLine(string.Format(UI.Error_0_reading_data_from_1, errno, start));
        }
        else
            for(ulong i = 0; i < length; i++)
            {
                if(inputFormat is not IMediaImage blockImage)
                {
                    AaruConsole.ErrorWriteLine(UI.Cannot_open_image_file_aborting);

                    break;
                }

                AaruConsole.WriteLine($"[bold][italic]{string.Format(UI.Sector_0_as_in_sector_number, start)}[/][/]" +
                                      i);

                if(blockImage.Info.ReadableSectorTags == null)
                {
                    AaruConsole.WriteLine(UI.Requested_sectors_tags_unsupported_by_image_format_printing_user_data);

                    longSectors = false;
                }
                else
                {
                    if(blockImage.Info.ReadableSectorTags.Count == 0)
                    {
                        AaruConsole.WriteLine(UI.Requested_sectors_tags_unsupported_by_image_format_printing_user_data);

                        longSectors = false;
                    }
                }

                byte[]      sector = Array.Empty<byte>();
                ErrorNumber errno  = ErrorNumber.NoError;

                Core.Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask(UI.Reading_sector).IsIndeterminate();

                    errno = longSectors ? blockImage.ReadSectorLong(start + i, out sector)
                                : blockImage.ReadSector(start             + i, out sector);
                });

                if(errno == ErrorNumber.NoError)
                    AaruConsole.WriteLine(Markup.Escape(PrintHex.ByteArrayToHexArrayString(sector, width, true)));
                else
                    AaruConsole.ErrorWriteLine(string.Format(UI.Error_0_reading_sector_1, errno, start + i));
            }

        return (int)ErrorNumber.NoError;
    }
}