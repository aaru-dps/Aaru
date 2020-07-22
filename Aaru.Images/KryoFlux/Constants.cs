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
//     Contains constants for KryoFlux STREAM images.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class KryoFlux
    {
        const string _hostDate  = "host_date";
        const string _hostTime  = "host_time";
        const string _kfName    = "name";
        const string _kfVersion = "version";
        const string _kfDate    = "date";
        const string _kfTime    = "time";
        const string _kfHwId    = "hwid";
        const string _kfHwRv    = "hwrv";
        const string _kfSck     = "sck";
        const string _kfIck     = "ick";
    }
}