// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Basic Lisa Utility disk images.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Images;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class Blu
{
#region IWritableImage Members

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter)
    {
        Stream stream = imageFilter.GetDataForkStream();
        stream.Seek(0, SeekOrigin.Begin);

        if(stream.Length < 0x200) return false;

        var header = new byte[0x17];
        stream.EnsureRead(header, 0, 0x17);

        var tmpHdr = new BluHeader
        {
            DeviceName = new byte[0x0D]
        };

        Array.Copy(header, 0, tmpHdr.DeviceName, 0, 0x0D);
        tmpHdr.DeviceType    = BigEndianBitConverter.ToUInt32(header, 0x0C) & 0x00FFFFFF;
        tmpHdr.DeviceBlocks  = BigEndianBitConverter.ToUInt32(header, 0x11) & 0x00FFFFFF;
        tmpHdr.BytesPerBlock = BigEndianBitConverter.ToUInt16(header, 0x15);

        for(var i = 0; i < 0xD; i++)
        {
            if(tmpHdr.DeviceName[i] < 0x20) return false;
        }

        return (tmpHdr.BytesPerBlock & 0xFE00) == 0x200;
    }

#endregion
}