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

using System.CommandLine;
using System.CommandLine.Invocation;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Core;
using Aaru.Helpers;

namespace Aaru.Commands.Image
{
    internal sealed class PrintHexCommand : Command
    {
        public PrintHexCommand() : base("print", "Prints a sector, in hexadecimal values, to the console.")
        {
            Add(new Option(new[]
                {
                    "--length", "-l"
                }, "How many sectors to print.")
                {
                    Argument = new Argument<ulong>(() => 1),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--long-sectors", "-r"
                }, "Print sectors with tags included.")
                {
                    Argument = new Argument<bool>(() => false),
                    Required = false
                });

            Add(new Option(new[]
                {
                    "--start", "-s"
                }, "Starting sector.")
                {
                    Argument = new Argument<ulong>(),
                    Required = true
                });

            Add(new Option(new[]
                {
                    "--width", "-w"
                }, "How many bytes to print per line.")
                {
                    Argument = new Argument<ushort>(() => 32),
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

        public static int Invoke(bool debug, bool verbose, string imagePath, ulong length, bool longSectors,
                                 ulong start, ushort width)
        {
            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("print-hex");

            AaruConsole.DebugWriteLine("PrintHex command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("PrintHex command", "--input={0}", imagePath);
            AaruConsole.DebugWriteLine("PrintHex command", "--length={0}", length);
            AaruConsole.DebugWriteLine("PrintHex command", "--long-sectors={0}", longSectors);
            AaruConsole.DebugWriteLine("PrintHex command", "--start={0}", start);
            AaruConsole.DebugWriteLine("PrintHex command", "--verbose={0}", verbose);
            AaruConsole.DebugWriteLine("PrintHex command", "--width={0}", width);

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
                AaruConsole.ErrorWriteLine("Unable to recognize image format, not verifying");

                return (int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);

            for(ulong i = 0; i < length; i++)
            {
                AaruConsole.WriteLine("Sector {0}", start + i);

                if(inputFormat.Info.ReadableSectorTags == null)
                {
                    AaruConsole.
                        WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");

                    longSectors = false;
                }
                else
                {
                    if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    {
                        AaruConsole.
                            WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");

                        longSectors = false;
                    }
                }

                byte[] sector = longSectors ? inputFormat.ReadSectorLong(start + i) : inputFormat.ReadSector(start + i);

                PrintHex.PrintHexArray(sector, width);
            }

            return (int)ErrorNumber.NoError;
        }
    }
}