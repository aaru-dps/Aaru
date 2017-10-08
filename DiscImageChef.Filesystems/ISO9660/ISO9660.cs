// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660.cs
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems.ISO9660
{
    // This is coded following ECMA-119.
    // TODO: Differentiate ISO Level 1, 2, 3 and ISO 9660:1999
    // TODO: Apple extensiones, requires XA or advance RR interpretation.
    // TODO: Check ECMA-167
    // TODO: Check ECMA-168
    public partial class ISO9660 : Filesystem
    {
        public ISO9660()
        {
            Name = "ISO9660 Filesystem";
            PluginUUID = new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
            CurrentEncoding = Encoding.ASCII;
        }

        public ISO9660(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "ISO9660 Filesystem";
            PluginUUID = new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
            if(encoding == null)
                CurrentEncoding = Encoding.ASCII;
        }
    }
}
