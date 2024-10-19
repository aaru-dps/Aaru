// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru Remote.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constants for the Aaru Remote protocol.
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

namespace Aaru.Devices.Remote;

/// <summary>AaruRemote protocol constants</summary>
public class Consts
{
    /// <summary>Primary unique packet identifier</summary>
    public const uint REMOTE_ID = 0x52434944; // "DICR"
    /// <summary>Secondary unique packet identifier</summary>
    public const uint PACKET_ID = 0x544B4350; // "PCKT"
    /// <summary>Default packet version</summary>
    public const int PACKET_VERSION = 1;
    /// <summary>Maximum supported protocol version</summary>
    public const int MAX_PROTOCOL = 2;
}