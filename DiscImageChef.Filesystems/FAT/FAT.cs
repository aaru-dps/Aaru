// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft FAT filesystem and shows information.
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
using System.Text;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;

namespace DiscImageChef.Filesystems.FAT
{
    // TODO: Differentiate between Atari and X68k FAT, as this one uses a standard BPB.
    // X68K uses cdate/adate from direntry for extending filename
    public partial class FAT : IFilesystem
    {
        public FileSystemType XmlFsType { get; private set; }

        public Encoding Encoding { get; private set; }
        public string   Name     => "Microsoft File Allocation Table";
        public Guid     Id       => new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
        public string   Author   => "Natalia Portillo";
    }
}