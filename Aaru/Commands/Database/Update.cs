// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ListDevices.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the 'media-info' verb.
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

namespace Aaru.Commands
{
    internal class UpdateCommand : Command
    {
        readonly bool _masterDbUpdate;

        public UpdateCommand(bool masterDbUpdate) : base("update", "Updates the database.")
        {
            _masterDbUpdate = masterDbUpdate;

            Add(new Option("--clear", "Clear existing master database.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Add(new Option("--clear-all", "Clear existing master and local database.")
            {
                Argument = new Argument<bool>(() => false), Required = false
            });

            Handler = CommandHandler.Create<bool, bool, bool, bool>(Invoke);
        }

        public int Invoke(bool debug, bool verbose, bool clear, bool clearAll)
        {
            if(_masterDbUpdate)
                return(int)ErrorNumber.NoError;

            MainClass.PrintCopyright();

            if(debug)
                AaruConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                AaruConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            AaruConsole.DebugWriteLine("Update command", "--debug={0}", debug);
            AaruConsole.DebugWriteLine("Update command", "--verbose={0}", verbose);

            if(clearAll)
            {
                try
                {
                    File.Delete(Aaru.Settings.Settings.LocalDbPath);

                    var ctx = DicContext.Create(Aaru.Settings.Settings.LocalDbPath);
                    ctx.Database.Migrate();
                    ctx.SaveChanges();
                }
                catch(Exception e)
                {
                    if(Debugger.IsAttached)
                        throw;

                    AaruConsole.ErrorWriteLine("Could not remove local database.");

                    return(int)ErrorNumber.CannotRemoveDatabase;
                }
            }

            if(clear || clearAll)
            {
                try
                {
                    File.Delete(Aaru.Settings.Settings.MasterDbPath);
                }
                catch(Exception e)
                {
                    if(Debugger.IsAttached)
                        throw;

                    AaruConsole.ErrorWriteLine("Could not remove master database.");

                    return(int)ErrorNumber.CannotRemoveDatabase;
                }
            }

            DoUpdate(clear || clearAll);

            return(int)ErrorNumber.NoError;
        }

        internal static void DoUpdate(bool create)
        {
            Remote.UpdateMasterDatabase(create);
            Statistics.AddCommand("update");
        }
    }
}