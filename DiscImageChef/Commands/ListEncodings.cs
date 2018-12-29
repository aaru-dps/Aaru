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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Commands
{
    static class ListEncodings
    {
        internal static void DoList()
        {
            List<CommonEncodingInfo> encodings = Encoding
                                                .GetEncodings().Select(info => new CommonEncodingInfo
                                                 {
                                                     Name        = info.Name,
                                                     DisplayName = info.GetEncoding().EncodingName
                                                 }).ToList();
            encodings.AddRange(Claunia.Encoding.Encoding.GetEncodings()
                                      .Select(info => new CommonEncodingInfo
                                       {
                                           Name        = info.Name,
                                           DisplayName = info.DisplayName
                                       }));

            DicConsole.WriteLine("{0,-16} {1,-8}", "Name", "Description");

            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
                DicConsole.WriteLine("{0,-16} {1,-8}", info.Name, info.DisplayName);

            Core.Statistics.AddCommand("list-encodings");
        }

        struct CommonEncodingInfo
        {
            public string Name;
            public string DisplayName;
        }
    }
}