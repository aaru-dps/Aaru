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
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Console;
using DiscImageChef.Core;
using Mono.Options;

namespace DiscImageChef.Commands
{
    class ListEncodingsCommand : Command
    {
        bool showHelp;

        public ListEncodingsCommand() : base("list-encodings", "Lists all supported text encodings and code pages.")
        {
            Options = new OptionSet
            {
                $"{MainClass.AssemblyTitle} {MainClass.AssemblyVersion?.InformationalVersion}",
                $"{MainClass.AssemblyCopyright}",
                "",
                $"usage: DiscImageChef {Name}",
                "",
                Help,
                {"help|h|?", "Show this message and exit.", v => showHelp = v != null}
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

            if(extra.Count > 0)
            {
                DicConsole.ErrorWriteLine("Too many arguments.");
                return (int)ErrorNumber.UnexpectedArgumentCount;
            }

            DicConsole.DebugWriteLine("List-Encodings command", "--debug={0}",   MainClass.Debug);
            DicConsole.DebugWriteLine("List-Encodings command", "--verbose={0}", MainClass.Verbose);

            List<CommonEncodingInfo> encodings = Encoding
                                                .GetEncodings().Select(info => new CommonEncodingInfo
                                                 {
                                                     Name = info.Name,
                                                     DisplayName =
                                                         info.GetEncoding().EncodingName
                                                 }).ToList();
            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings()
                                      .Select(info => new CommonEncodingInfo
                                       {
                                           Name = info.Name, DisplayName = info.DisplayName
                                       }));

            DicConsole.WriteLine("{0,-16} {1,-8}", "Name", "Description");

            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
                DicConsole.WriteLine("{0,-16} {1,-8}", info.Name, info.DisplayName);

            Statistics.AddCommand("list-encodings");
            return (int)ErrorNumber.NoError;
        }

        struct CommonEncodingInfo
        {
            public string Name;
            public string DisplayName;
        }
    }
}