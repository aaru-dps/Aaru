// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Update.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'update' command.
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

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Core;
using Aaru.Database;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Aaru.Commands.Database;

internal sealed class UpdateCommand : Command
{
    readonly bool _mainDbUpdate;

    public UpdateCommand(bool mainDbUpdate) : base("update", "Updates the database.")
    {
        _mainDbUpdate = mainDbUpdate;

        Add(new Option("--clear", "Clear existing main database.")
        {
            Argument = new Argument<bool>(() => false),
            Required = false
        });

        Add(new Option("--clear-all", "Clear existing main and local database.")
        {
            Argument = new Argument<bool>(() => false),
            Required = false
        });

        Handler = CommandHandler.Create((Func<bool, bool, bool, bool, int>)Invoke);
    }

    int Invoke(bool debug, bool verbose, bool clear, bool clearAll)
    {
        if(_mainDbUpdate)
            return (int)ErrorNumber.NoError;

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

        AaruConsole.DebugWriteLine("Update command", "--debug={0}", debug);
        AaruConsole.DebugWriteLine("Update command", "--verbose={0}", verbose);

        if(clearAll)
        {
            try
            {
                File.Delete(Settings.Settings.LocalDbPath);

                var ctx = AaruContext.Create(Settings.Settings.LocalDbPath);
                ctx.Database.Migrate();
                ctx.SaveChanges();
            }
            catch(Exception)
            {
                if(Debugger.IsAttached)
                    throw;

                AaruConsole.ErrorWriteLine("Could not remove local database.");

                return (int)ErrorNumber.CannotRemoveDatabase;
            }
        }

        if(clear || clearAll)
        {
            try
            {
                File.Delete(Settings.Settings.MainDbPath);
            }
            catch(Exception)
            {
                if(Debugger.IsAttached)
                    throw;

                AaruConsole.ErrorWriteLine("Could not remove main database.");

                return (int)ErrorNumber.CannotRemoveDatabase;
            }
        }

        DoUpdate(clear || clearAll);

        return (int)ErrorNumber.NoError;
    }

    internal static void DoUpdate(bool create)
    {
        Remote.UpdateMainDatabase(create);
        Statistics.AddCommand("update");
    }
}