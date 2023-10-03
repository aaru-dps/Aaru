// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extensions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides class extensions.
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

using System.IO;

namespace Aaru.Helpers;

public static class Extensions
{
    /// <summary>
    ///     When overridden in a derived class, reads a sequence of bytes from the current stream and advances the
    ///     position within the stream by the number of bytes read.<br /> Guarantees the whole count of bytes is read or EOF is
    ///     found
    /// </summary>
    /// <param name="s">Stream to extend</param>
    /// <param name="buffer">
    ///     An array of bytes. When this method returns, the buffer contains the specified byte array with the
    ///     values between <see cref="offset" /> and (<see cref="offset" /> + <see cref="count" /> - 1) replaced by the bytes
    ///     read from the current source.
    /// </param>
    /// <param name="offset">
    ///     The zero-based byte offset in <see cref="buffer" /> at which to begin storing the data read from
    ///     the current stream.
    /// </param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <returns>
    ///     The total number of bytes read into the buffer. This can be less than the number of bytes requested if the end
    ///     of the stream has been reached.
    /// </returns>
    public static int EnsureRead(this Stream s, byte[] buffer, int offset, int count)
    {
        var pos = 0;
        int read;

        do
        {
            read =  s.Read(buffer, pos + offset, count - pos);
            pos  += read;
        } while(read > 0);

        return pos;
    }
}