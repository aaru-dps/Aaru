// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MD5Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Wraps up .NET MD5 implementation to a Init(), Update(), Final() context.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Checksums;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Aaru.CommonTypes.Interfaces;

/// <summary>Wraps up .NET MD5 implementation to a Init(), Update(), Final() context.</summary>
public sealed class Md5Context : IChecksum
{
    readonly MD5 _provider;

    /// <summary>Initializes the MD5 hash provider</summary>
    public Md5Context() => _provider = MD5.Create();

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of buffer to hash.</param>
    public void Update(byte[] data, uint len) => _provider.TransformBlock(data, 0, (int)len, data, 0);

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    public void Update(byte[] data) => Update(data, (uint)data.Length);

    /// <inheritdoc />
    /// <summary>Returns a byte array of the hash value.</summary>
    public byte[] Final()
    {
        _provider.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        return _provider.Hash;
    }

    /// <inheritdoc />
    /// <summary>Returns a hexadecimal representation of the hash value.</summary>
    public string End()
    {
        _provider.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        var md5Output = new StringBuilder();

        foreach(byte h in _provider.Hash)
            md5Output.Append(h.ToString("x2"));

        return md5Output.ToString();
    }

    /// <summary>Gets the hash of a file</summary>
    /// <param name="filename">File path.</param>
    public static byte[] File(string filename)
    {
        var    localMd5Provider = MD5.Create();
        var    fileStream       = new FileStream(filename, FileMode.Open);
        byte[] result           = localMd5Provider.ComputeHash(fileStream);
        fileStream.Close();

        return result;
    }

    /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
    /// <param name="filename">File path.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string File(string filename, out byte[] hash)
    {
        var localMd5Provider = MD5.Create();
        var fileStream       = new FileStream(filename, FileMode.Open);
        hash = localMd5Provider.ComputeHash(fileStream);
        var md5Output = new StringBuilder();

        foreach(byte h in hash)
            md5Output.Append(h.ToString("x2"));

        fileStream.Close();

        return md5Output.ToString();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of the data buffer to hash.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string Data(byte[] data, uint len, out byte[] hash)
    {
        var localMd5Provider = MD5.Create();
        hash = localMd5Provider.ComputeHash(data, 0, (int)len);
        var md5Output = new StringBuilder();

        foreach(byte h in hash)
            md5Output.Append(h.ToString("x2"));

        return md5Output.ToString();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
}