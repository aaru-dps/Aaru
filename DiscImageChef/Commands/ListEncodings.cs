// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListEncodings.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Verbs.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;

namespace DiscImageChef.Commands
{
    internal class ListEncodingsCommand : Command
    {
        public ListEncodingsCommand() : base("list-encodings", "Lists all supported text encodings and code pages.") =>
            Handler = CommandHandler.Create(GetType().GetMethod(nameof(Invoke)));

        public static int Invoke(bool debug, bool verbose)
        {
            MainClass.PrintCopyright();

            if(debug)
                DicConsole.DebugWriteLineEvent += System.Console.Error.WriteLine;

            if(verbose)
                DicConsole.VerboseWriteLineEvent += System.Console.WriteLine;

            Statistics.AddCommand("list-encodings");

            DicConsole.DebugWriteLine("List-Encodings command", "--debug={0}", debug);
            DicConsole.DebugWriteLine("List-Encodings command", "--verbose={0}", verbose);

            List<CommonEncodingInfo> encodings = Encoding.GetEncodings().Select(info => new CommonEncodingInfo
            {
                Name = info.Name, DisplayName = info.GetEncoding().EncodingName
            }).ToList();

            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings().Select(info => new CommonEncodingInfo
            {
                Name = info.Name, DisplayName = info.DisplayName
            }));

            DicConsole.WriteLine("{0,-16} {1,-8}", "Name", "Description");

            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
                DicConsole.WriteLine("{0,-16} {1,-8}", info.Name, info.DisplayName);

            return(int)ErrorNumber.NoError;
        }

        struct CommonEncodingInfo
        {
            public string Name;
            public string DisplayName;
        }
    }
}