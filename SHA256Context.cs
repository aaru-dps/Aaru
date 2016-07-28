// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SHA256Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Wraps up .NET SHA256 implementation to a Init(), Update(), Final() context.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace DiscImageChef.Checksums
{
    /// <summary>
    /// Provides a UNIX similar API to .NET SHA256.
    /// </summary>
    public class SHA256Context
    {
        SHA256 _sha256Provider;

        /// <summary>
        /// Initializes the SHA256 hash provider
        /// </summary>
        public void Init()
        {
            _sha256Provider = SHA256.Create();
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            _sha256Provider.TransformBlock(data, 0, (int)len, data, 0);
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        /// Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            _sha256Provider.TransformFinalBlock(new byte[0], 0, 0);
            return _sha256Provider.Hash;
        }

        /// <summary>
        /// Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            _sha256Provider.TransformFinalBlock(new byte[0], 0, 0);
            StringBuilder sha256Output = new StringBuilder();

            for(int i = 0; i < _sha256Provider.Hash.Length; i++)
            {
                sha256Output.Append(_sha256Provider.Hash[i].ToString("x2"));
            }

            return sha256Output.ToString();
        }

        /// <summary>
        /// Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public byte[] File(string filename)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            return _sha256Provider.ComputeHash(fileStream);
        }

        /// <summary>
        /// Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            hash = _sha256Provider.ComputeHash(fileStream);
            StringBuilder sha256Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++)
            {
                sha256Output.Append(hash[i].ToString("x2"));
            }

            return sha256Output.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public string Data(byte[] data, uint len, out byte[] hash)
        {
            hash = _sha256Provider.ComputeHash(data, 0, (int)len);
            StringBuilder sha256Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++)
            {
                sha256Output.Append(hash[i].ToString("x2"));
            }

            return sha256Output.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }
}

