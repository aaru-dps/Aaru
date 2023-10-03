// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'info' command.
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
// Copyright © 2021-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands.Archive;

sealed class ArchiveInfoCommand : Command
{
    const string MODULE_NAME = "Analyze command";

    public ArchiveInfoCommand() : base("info", UI.Archive_Info_Command_Description)
    {
        AddArgument(new Argument<string>
        {
            Arity       = ArgumentArity.ExactlyOne,
            Description = "Archive file path",
            Name        = "archive-path"
        });

        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));
    }

    public static int Invoke(bool debug, bool verbose, string imagePath)
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
        {
            AaruConsole.WriteEvent += (format, objects) =>
                                      {
                                          if(objects is null)
                                              AnsiConsole.Markup(format);
                                          else
                                              AnsiConsole.Markup(format, objects);
                                      };
        }

        Statistics.AddCommand("archive-info");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--input={0}",   imagePath);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

        /* TODO: This is just a stub for now */

        return (int)ErrorNumber.NoError;
    }
}