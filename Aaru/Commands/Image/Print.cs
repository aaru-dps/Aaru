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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Commands.Image;

using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;
using Spectre.Console;

sealed class PrintHexCommand : Command
{
    public PrintHexCommand() : base("print", "Prints a sector, in hexadecimal values, to the console.")
    {
        Add(new Option<ulong>(new[]
        {
            "--length", "-l"
        }, () => 1, "How many sectors to print."));

        Add(new Option<bool>(new[]
        {
            "--long-sectors", "-r"
        }, () => false, "Print sectors with tags included."));

        Add(new Option<ulong>(new[]
        {
            "--start", "-s"
        }, "Starting sector."));

        Add(new Option<ushort>(new[]
        {
            "--width", "-w"
        }, () => 32, "How many bytes to print per line."));

        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Media image path",
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

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Identifying file filter...").IsIndeterminate();
            inputFilter = filtersList.GetFilter(imagePath);
        });

        if(inputFilter == null)
        {
            AaruConsole.ErrorWriteLine("Cannot open specified file.");

            return (int)ErrorNumber.CannotOpenFile;
        }

        IBaseImage inputFormat = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask("Identifying image format...").IsIndeterminate();
            inputFormat = ImageFormat.Detect(inputFilter);
        });

        if(inputFormat == null)
        {
            AaruConsole.ErrorWriteLine("Unable to recognize image format, not verifying");

            return (int)ErrorNumber.UnrecognizedFormat;
        }

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

        if(inputFormat.Info.XmlMediaType == XmlMediaType.LinearMedia)
        {
            var byteAddressableImage = inputFormat as IByteAddressableImage;

            AaruConsole.WriteLine("[bold][italic]Start {0}[/][/]", start);

            var         data      = new byte[length];
            ErrorNumber errno     = ErrorNumber.NoError;
            var         bytesRead = 0;

            Spectre.ProgressSingleSpinner(ctx =>
            {
                ctx.AddTask("Reading data...").IsIndeterminate();

                errno = byteAddressableImage?.ReadBytesAt((long)start, data, 0, (int)length, out bytesRead) ??
                        ErrorNumber.InvalidArgument;
            });

            // TODO: Span
            if(bytesRead != (int)length)
            {
                var tmp = new byte[bytesRead];
                Array.Copy(data, 0, tmp, 0, bytesRead);
                data = tmp;
            }

            if(errno == ErrorNumber.NoError)
                AaruConsole.WriteLine(Markup.Escape(PrintHex.ByteArrayToHexArrayString(data, width, true)));
            else
                AaruConsole.ErrorWriteLine($"Error {errno} reading data from {start}.");
        }
        else
            for(ulong i = 0; i < length; i++)
            {
                if(inputFormat is not IMediaImage blockImage)
                {
                    AaruConsole.ErrorWriteLine("Cannot open image file, aborting...");

                    break;
                }

                AaruConsole.WriteLine("[bold][italic]Sector {0}[/][/]", start + i);

                if(blockImage.Info.ReadableSectorTags == null)
                {
                    AaruConsole.
                        WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");

                    longSectors = false;
                }
                else
                {
                    if(blockImage.Info.ReadableSectorTags.Count == 0)
                    {
                        AaruConsole.
                            WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");

                        longSectors = false;
                    }
                }

                byte[]      sector = Array.Empty<byte>();
                ErrorNumber errno  = ErrorNumber.NoError;

                Spectre.ProgressSingleSpinner(ctx =>
                {
                    ctx.AddTask("Reading sector...").IsIndeterminate();

                    errno = longSectors ? blockImage.ReadSectorLong(start + i, out sector)
                                : blockImage.ReadSector(start             + i, out sector);
                });

                if(errno == ErrorNumber.NoError)
                    AaruConsole.WriteLine(Markup.Escape(PrintHex.ByteArrayToHexArrayString(sector, width, true)));
                else
                    AaruConsole.ErrorWriteLine($"Error {errno} reading sector {start + i}.");
            }

        return (int)ErrorNumber.NoError;
    }
}