// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UCSDPascal.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the U.C.S.D. Pascal filesystem plugin.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin : IReadOnlyFilesystem
    {
        IMediaImage device;
        byte[] bootBlocks;
        byte[] catalogBlocks;
        Encoding currentEncoding;
        bool debug;
        List<PascalFileEntry> fileEntries;
        bool mounted;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        PascalVolumeEntry mountedVolEntry;

        public string Name => "U.C.S.D. Pascal filesystem";
        public Guid Id => new Guid("B0AC2CB5-72AA-473A-9200-270B5A2C2D53");
        public Encoding Encoding => currentEncoding;

        public Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotSupported;
        }

        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotSupported;
        }

        public Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotSupported;
        }
    }
}