// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apple.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple extensions to ISO 9660.
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
// Copyright © 2011-2026 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Text;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    /// <summary>
    ///     Start of the protocol identifier of the Apple extensions to ISO 9660, recorded in the system identifier
    ///     (GS/OS Reference, appendix C)
    /// </summary>
    const string APPLE_PROTOCOL_IDENTIFIER = "APPLE COMPUTER, INC., TYPE: ";

    /// <summary>Decodes the protocol identifier of the Apple extensions to ISO 9660</summary>
    /// <param name="systemId">System identifier of the primary volume descriptor</param>
    /// <param name="version">Version of the Apple extensions</param>
    /// <param name="proDosNames">Set if the ProDOS filenames were transformed to ISO 9660 names</param>
    /// <returns><c>true</c> if the volume declares the Apple extensions</returns>
    static bool DecodeAppleProtocol(byte[] systemId, out byte version, out bool proDosNames)
    {
        version     = 0;
        proDosNames = false;

        if(systemId is not { Length: >= 32 }) return false;

        if(Encoding.ASCII.GetString(systemId, 0, APPLE_PROTOCOL_IDENTIFIER.Length) != APPLE_PROTOCOL_IDENTIFIER)
            return false;

        // Four flag bytes, each one 0011 followed by four bits of flags so they are valid a-characters
        for(int i = APPLE_PROTOCOL_IDENTIFIER.Length; i < APPLE_PROTOCOL_IDENTIFIER.Length + 4; i++)
            if((systemId[i] & 0xF0) != 0x30)
                return false;

        proDosNames = (systemId[APPLE_PROTOCOL_IDENTIFIER.Length] & 1) != 0;
        version     = (byte)(systemId[APPLE_PROTOCOL_IDENTIFIER.Length + 3] & 0x0F);

        return true;
    }
}