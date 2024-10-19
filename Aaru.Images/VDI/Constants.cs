// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for VirtualBox disk images.
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

namespace Aaru.Images;

public sealed partial class Vdi
{
    const uint VDI_MAGIC = 0xBEDA107F;
    const uint VDI_EMPTY = 0xFFFFFFFF;

    const string ORACLE_VDI      = "<<< Oracle VM VirtualBox Disk Image >>>\n";
    const string QEMUVDI         = "<<< QEMU VM Virtual Disk Image >>>\n";
    const string SUN_OLD_VDI     = "<<< Sun xVM VirtualBox Disk Image >>>\n";
    const string SUN_VDI         = "<<< Sun VirtualBox Disk Image >>>\n";
    const string INNOTEK_VDI     = "<<< innotek VirtualBox Disk Image >>>\n";
    const string INNOTEK_OLD_VDI = "<<< InnoTek VirtualBox Disk Image >>>\n";
    const string DIC_VDI         = "<<< DiscImageChef VirtualBox Disk Image >>>\n";
    const string DIC_AARU        = "<<< Aaru VirtualBox Disk Image >>>\n";

    const uint MAX_CACHE_SIZE     = 16777216;
    const uint MAX_CACHED_SECTORS = MAX_CACHE_SIZE / 512;
    const uint DEFAULT_BLOCK_SIZE = 1048576;
}