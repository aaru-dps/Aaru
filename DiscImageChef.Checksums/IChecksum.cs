// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IChecksum.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides an interface for implementing checksums and hashes.
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

namespace DiscImageChef.Checksums
{
    public interface IChecksum
    {
        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        void Update(byte[] data, uint len);

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        void Update(byte[] data);

        /// <summary>
        ///     Returns a byte array of the hash value.
        /// </summary>
        byte[] Final();

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        string End();
    }
}