// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PrintHex.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'printhex' verb.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class PrintHexCommand : Command
    {
        string inputFile;
        ulong  length = 1;
        bool   longSectors;
        bool   showHelp;
        ulong? startSector;
        ushort widthBytes = 32;

        public PrintHexCommand() : base("printhex", "Prints a sector, in hexadecimal values, to the console.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name} [OPTIONS] imagefile",
                "",
                Help,
                {"length|l=", "How many sectors to print.", (ulong ul) => length             = ul},
                {"long-sectors|r", "Print sectors with tags included.", b => longSectors     = b != null},
                {"start|s=", "Name of character encoding to use.", (ulong ul) => startSector = ul},
                {"width|w=", "How many bytes to print per line.", (ushort us) => widthBytes  = us},
                {"help|h|?", "Show this message and exit.", v => showHelp                    = v != null}
            };
        }

        public override int Invoke(IEnumerable<string> arguments)
        {
            List<string> extra = Options.Parse(arguments);

            if(showHelp)
            {
                Options.WriteOptionDescriptions(CommandSet.Out);
                return (int)ErrorNumber.HelpRequested;
            }

            MainClass.PrintCopyright();
            if(MainClass.Debug) DicConsole.DebugWriteLineEvent     += System.Console.Error.WriteLine;
            if(MainClass.Verbose) DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;
            Statistics.AddCommand("print-hex");

            if(extra.Count > 1)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            if(extra.Count == 0)
            {
                DicConsole.ErrorWriteLine("Missing input image.");
                return (int)ErrorNumber.MissingArgument;
            }

            if(startSector is null)
            {
                DicConsole.ErrorWriteLine("Missing starting sector.");
                return (int)ErrorNumber.MissingArgument;
            }

            inputFile = extra[0];

            DicConsole.DebugWriteLine("PrintHex command", "--debug={0}",        MainClass.Debug);
            DicConsole.DebugWriteLine("PrintHex command", "--input={0}",        inputFile);
            DicConsole.DebugWriteLine("PrintHex command", "--length={0}",       length);
            DicConsole.DebugWriteLine("PrintHex command", "--long-sectors={0}", longSectors);
            DicConsole.DebugWriteLine("PrintHex command", "--start={0}",        startSector);
            DicConsole.DebugWriteLine("PrintHex command", "--verbose={0}",      MainClass.Verbose);
            DicConsole.DebugWriteLine("PrintHex command", "--WidthBytes={0}",   widthBytes);

            FiltersList filtersList = new FiltersList();
            IFilter     inputFilter = filtersList.GetFilter(inputFile);

            if(inputFilter == null)
            {
                DicConsole.ErrorWriteLine("Cannot open specified file.");
                return (int)ErrorNumber.CannotOpenFile;
            }

            IMediaImage inputFormat = ImageFormat.Detect(inputFilter);

            if(inputFormat == null)
            {
                DicConsole.ErrorWriteLine("Unable to recognize image format, not verifying");
                return (int)ErrorNumber.UnrecognizedFormat;
            }

            inputFormat.Open(inputFilter);

            for(ulong i = 0; i < length; i++)
            {
                DicConsole.WriteLine("Sector {0}", startSector + i);

                if(inputFormat.Info.ReadableSectorTags == null)
                {
                    DicConsole
                       .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                    longSectors = false;
                }
                else
                {
                    if(inputFormat.Info.ReadableSectorTags.Count == 0)
                    {
                        DicConsole
                           .WriteLine("Requested sectors with tags, unsupported by underlying image format, printing only user data.");
                        longSectors = false;
                    }
                }

                byte[] sector = longSectors
                                    ? inputFormat.ReadSectorLong(startSector.Value + i)
                                    : inputFormat.ReadSector(startSector.Value     + i);

                PrintHex.PrintHexArray(sector, widthBytes);
            }

            return (int)ErrorNumber.NoError;
        }
    }
}