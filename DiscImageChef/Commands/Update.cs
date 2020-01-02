// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.CommandLine;
using System.CommandLine.Invocation;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    internal class UpdateCommand : Command
    {
        readonly bool _masterDbUpdate;

        public UpdateCommand(bool masterDbUpdate) : base("update", "Updates the database.")
        {
            _masterDbUpdate = masterDbUpdate;

            Handler = CommandHandler.Create<bool, bool>(Invoke);
        }

        public int Invoke(bool debug, bool verbose)
        {
            if(_masterDbUpdate)
                return(int)ErrorNumber.NoError;

            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            DicConsole.DebugWriteLine("Update command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("Update command", "--verbose={0}", verbose);

            DoUpdate(false);

            return(int)ErrorNumber.NoError;
        }

        internal static void DoUpdate(bool create)
        {
            Remote.UpdateMasterDatabase(create);
            Statistics.AddCommand("update");
        }
    }
}