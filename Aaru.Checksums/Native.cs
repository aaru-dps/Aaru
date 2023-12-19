// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Native.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Checks that Aaru.Checksums.Native library is available and usable.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Checksums;

/// <summary>Handles native implementations of compression algorithms</summary>
public static class Native
{
    static bool _checked;
    static bool _supported;

    /// <summary>Set to return native as never supported</summary>
    public static bool ForceManaged { get; set; }

    /// <summary>
    ///     If set to <c>true</c> the native library was found and loaded correctly and its reported version is
    ///     compatible.
    /// </summary>
    public static bool IsSupported
    {
        get
        {
            if(ForceManaged)
                return false;

            if(_checked)
                return _supported;

            ulong version;
            _checked = true;

            try
            {
                version = get_acn_version();
            }
            catch
            {
                _supported = false;

                return false;
            }

            // TODO: Check version compatibility
            _supported = version >= 0x06000000;

            return _supported;
        }
    }

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern ulong get_acn_version();
}