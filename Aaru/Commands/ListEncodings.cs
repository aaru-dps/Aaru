// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListEncodings.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     List all supported character encodings.
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
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Localization;
using Spectre.Console;

namespace Aaru.Commands;

sealed class ListEncodingsCommand : Command
{
    const string MODULE_NAME = "List-Encodings command";

    public ListEncodingsCommand() : base("list-encodings", UI.List_Encodings_Command_Description) =>
        Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)) ?? throw new NullReferenceException());

    public static int Invoke(bool debug, bool verbose)
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

        Statistics.AddCommand("list-encodings");

        AaruConsole.DebugWriteLine(MODULE_NAME, "--debug={0}",   debug);
        AaruConsole.DebugWriteLine(MODULE_NAME, "--verbose={0}", verbose);

        var encodings = Encoding.GetEncodings().
                                 Select(info => new CommonEncodingInfo
                                 {
                                     Name        = info.Name,
                                     DisplayName = info.GetEncoding().EncodingName
                                 }).
                                 ToList();

        encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().
                                   Select(info => new CommonEncodingInfo
                                   {
                                       Name        = info.Name,
                                       DisplayName = info.DisplayName
                                   }));

        Table table = new();
        table.AddColumn(UI.Title_Name);
        table.AddColumn(UI.Title_Description);

        foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
            table.AddRow(info.Name, info.DisplayName);

        AnsiConsole.Write(table);

        return (int)ErrorNumber.NoError;
    }

#region Nested type: CommonEncodingInfo

    struct CommonEncodingInfo
    {
        public string Name;
        public string DisplayName;
    }

#endregion
}