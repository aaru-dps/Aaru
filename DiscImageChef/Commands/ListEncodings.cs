// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ListEncodings.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
using DiscImageChef.Console;
using System.Linq;

namespace DiscImageChef.Commands
{
    public static class ListEncodings
    {
        struct CommonEncodingInfo
        {
            public string Name;
            public string DisplayName;
        }

        public static void DoList(ListEncodingsOptions EncodingOptions)
        {
            List<CommonEncodingInfo> encodings = new List<CommonEncodingInfo>();

            foreach(System.Text.EncodingInfo info in System.Text.Encoding.GetEncodings())
                encodings.Add(new CommonEncodingInfo { Name = info.Name, DisplayName = info.GetEncoding().EncodingName });
            foreach(Claunia.Encoding.EncodingInfo info in Claunia.Encoding.Encoding.GetEncodings())
                encodings.Add(new CommonEncodingInfo { Name = info.Name, DisplayName = info.DisplayName });

            DicConsole.WriteLine("{0,-16} {1,-8}", "Name", "Description");

            foreach(CommonEncodingInfo info in encodings.OrderBy(t => t.DisplayName))
                    DicConsole.WriteLine("{0,-16} {1,-8}", info.Name, info.DisplayName);

            Core.Statistics.AddCommand("list-encodings");
        }
    }
}

