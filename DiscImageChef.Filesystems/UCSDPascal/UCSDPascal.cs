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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin : Filesystem
    {
        bool mounted;
        bool debug;
        readonly ImagePlugin device;

        PascalVolumeEntry mountedVolEntry;
        List<PascalFileEntry> fileEntries;
        byte[] bootBlocks;
        byte[] catalogBlocks;

        public PascalPlugin()
        {
            Name = "U.C.S.D. Pascal filesystem";
            PluginUUID = new Guid("B0AC2CB5-72AA-473A-9200-270B5A2C2D53");
            CurrentEncoding = new Claunia.Encoding.LisaRoman();
        }

        public PascalPlugin(Encoding encoding)
        {
            Name = "U.C.S.D. Pascal filesystem";
            PluginUUID = new Guid("B0AC2CB5-72AA-473A-9200-270B5A2C2D53");
            // TODO: Until Apple ][ encoding is implemented
            CurrentEncoding = new Claunia.Encoding.LisaRoman();
        }

        public PascalPlugin(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            device = imagePlugin;
            Name = "U.C.S.D. Pascal filesystem";
            PluginUUID = new Guid("B0AC2CB5-72AA-473A-9200-270B5A2C2D53");
            // TODO: Until Apple ][ encoding is implemented
            CurrentEncoding = new Claunia.Encoding.LisaRoman();
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotSupported;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotSupported;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotSupported;
        }
    }
}