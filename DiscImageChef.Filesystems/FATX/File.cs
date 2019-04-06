// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle files.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.CommonTypes.Structs;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock) => throw new NotImplementedException();

        public Errno GetAttributes(string path, out FileAttributes attributes) => throw new NotImplementedException();

        public Errno Read(string path, long offset, long size, ref byte[] buf) => throw new NotImplementedException();

        public Errno Stat(string path, out FileEntryInfo stat) => throw new NotImplementedException();
    }
}