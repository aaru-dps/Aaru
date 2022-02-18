// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     High Performance Optical File System.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems
{
    // Information from test floppy images created with OS/2 HPOFS 2.0
    // Need to get IBM document GA32-0224 -> IBM 3995 Optical Library Dataserver Products: Optical Disk Format
    /// <inheritdoc />
    /// <summary>Implements identification of IBM's High Performance Optical File System</summary>
    public sealed partial class HPOFS : IFilesystem
    {
        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "High Performance Optical File System";
        /// <inheritdoc />
        public Guid Id => new Guid("1b72dcd5-d031-4757-8a9f-8d2fb18c59e2");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";
    }
}