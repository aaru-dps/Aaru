// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CRC16Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements a CRC16 algorithm.
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums;

/// <inheritdoc />
/// <summary>Implements a CRC16 algorithm</summary>
public class Crc16Context : IChecksum
{
    readonly ushort     _finalSeed;
    readonly bool       _inverse;
    readonly IntPtr     _nativeContext;
    readonly ushort[][] _table;
    readonly bool       _useCcitt;
    readonly bool       _useIbm;
    readonly bool       _useNative;
    ushort              _hashInt;

    /// <summary>Initializes the CRC16 table with a custom polynomial and seed</summary>
    public Crc16Context(ushort polynomial, ushort seed, ushort[][] table, bool inverse)
    {
        _hashInt   = seed;
        _finalSeed = seed;
        _inverse   = inverse;

        _useNative = Native.IsSupported;

        _useCcitt = polynomial == CRC16CcittContext.CRC16_CCITT_POLY &&
                    seed       == CRC16CcittContext.CRC16_CCITT_SEED &&
                    inverse;

        _useIbm = polynomial == CRC16IbmContext.CRC16_IBM_POLY && seed == CRC16IbmContext.CRC16_IBM_SEED && !inverse;

        if(_useCcitt && _useNative)
        {
            _nativeContext = crc16_ccitt_init();
            _useNative     = _nativeContext != IntPtr.Zero;
        }
        else if(_useIbm && _useNative)
        {
            _nativeContext = crc16_init();
            _useNative     = _nativeContext != IntPtr.Zero;
        }
        else
            _useNative = false;

        if(!_useNative)
            _table = table ?? GenerateTable(polynomial, inverse);
    }

#region IChecksum Members

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of buffer to hash.</param>
    public void Update(byte[] data, uint len)
    {
        switch(_useNative)
        {
            case true when _useCcitt:
                crc16_ccitt_update(_nativeContext, data, len);

                break;
            case true when _useIbm:
                crc16_update(_nativeContext, data, len);

                break;
            default:
            {
                if(_inverse)
                    StepInverse(ref _hashInt, _table, data, len);
                else
                    Step(ref _hashInt, _table, data, len);

                break;
            }
        }
    }

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    public void Update(byte[] data) => Update(data, (uint)data.Length);

    /// <inheritdoc />
    /// <summary>Returns a byte array of the hash value.</summary>
    public byte[] Final()
    {
        ushort crc = 0;

        switch(_useNative)
        {
            case true when _useCcitt:
                crc16_ccitt_final(_nativeContext, ref crc);
                crc16_ccitt_free(_nativeContext);

                break;
            case true when _useIbm:
                crc16_final(_nativeContext, ref crc);
                crc16_free(_nativeContext);

                break;
            default:
            {
                if(_inverse)
                    crc = (ushort)~(_hashInt ^ _finalSeed);
                else
                    crc = (ushort)(_hashInt ^ _finalSeed);

                break;
            }
        }

        return BigEndianBitConverter.GetBytes(crc);
    }

    /// <inheritdoc />
    /// <summary>Returns a hexadecimal representation of the hash value.</summary>
    public string End()
    {
        var    crc16Output = new StringBuilder();
        ushort final       = 0;

        switch(_useNative)
        {
            case true when _useCcitt:
                crc16_ccitt_final(_nativeContext, ref final);
                crc16_ccitt_free(_nativeContext);

                break;
            case true when _useIbm:
                crc16_final(_nativeContext, ref final);
                crc16_free(_nativeContext);

                break;
            default:
            {
                if(_inverse)
                    final = (ushort)~(_hashInt ^ _finalSeed);
                else
                    final = (ushort)(_hashInt ^ _finalSeed);

                break;
            }
        }

        byte[] finalBytes = BigEndianBitConverter.GetBytes(final);

        foreach(byte t in finalBytes)
            crc16Output.Append(t.ToString("x2"));

        return crc16Output.ToString();
    }

#endregion

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern IntPtr crc16_init();

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int crc16_update(IntPtr ctx, byte[] data, uint len);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int crc16_final(IntPtr ctx, ref ushort crc);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern void crc16_free(IntPtr ctx);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern IntPtr crc16_ccitt_init();

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int crc16_ccitt_update(IntPtr ctx, byte[] data, uint len);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int crc16_ccitt_final(IntPtr ctx, ref ushort crc);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern void crc16_ccitt_free(IntPtr ctx);

    static void Step(ref ushort previousCrc, ushort[][] table, byte[] data, uint len)
    {
        // Unroll according to Intel slicing by uint8_t
        // http://www.intel.com/technology/comms/perfnet/download/CRC_generators.pdf
        // http://sourceforge.net/projects/slicing-by-8/

        var       currentPos  = 0;
        const int unroll      = 4;
        const int bytesAtOnce = 8 * unroll;

        ushort crc = previousCrc;

        while(len >= bytesAtOnce)
        {
            int unrolling;

            for(unrolling = 0; unrolling < unroll; unrolling++)
            {
                // TODO: What trick is Microsoft doing here that's faster than arithmetic conversion
                uint one = BitConverter.ToUInt32(data, currentPos) ^ crc;
                currentPos += 4;
                var two = BitConverter.ToUInt32(data, currentPos);
                currentPos += 4;

                crc = (ushort)(table[0][two >> 24 & 0xFF] ^
                               table[1][two >> 16 & 0xFF] ^
                               table[2][two >> 8  & 0xFF] ^
                               table[3][two       & 0xFF] ^
                               table[4][one >> 24 & 0xFF] ^
                               table[5][one >> 16 & 0xFF] ^
                               table[6][one >> 8  & 0xFF] ^
                               table[7][one       & 0xFF]);
            }

            len -= bytesAtOnce;
        }

        while(len-- != 0)
            crc = (ushort)(crc >> 8 ^ table[0][crc & 0xFF ^ data[currentPos++]]);

        previousCrc = crc;
    }

    static void StepInverse(ref ushort previousCrc, ushort[][] table, byte[] data, uint len)
    {
        // Unroll according to Intel slicing by uint8_t
        // http://www.intel.com/technology/comms/perfnet/download/CRC_generators.pdf
        // http://sourceforge.net/projects/slicing-by-8/

        var       currentPos  = 0;
        const int unroll      = 4;
        const int bytesAtOnce = 8 * unroll;

        ushort crc = previousCrc;

        while(len >= bytesAtOnce)
        {
            int unrolling;

            for(unrolling = 0; unrolling < unroll; unrolling++)
            {
                crc = (ushort)(table[7][data[currentPos + 0] ^ crc >> 8]   ^
                               table[6][data[currentPos + 1] ^ crc & 0xFF] ^
                               table[5][data[currentPos + 2]]              ^
                               table[4][data[currentPos + 3]]              ^
                               table[3][data[currentPos + 4]]              ^
                               table[2][data[currentPos + 5]]              ^
                               table[1][data[currentPos + 6]]              ^
                               table[0][data[currentPos + 7]]);

                currentPos += 8;
            }

            len -= bytesAtOnce;
        }

        while(len-- != 0)
            crc = (ushort)(crc << 8 ^ table[0][crc >> 8 ^ data[currentPos++]]);

        previousCrc = crc;
    }

    static ushort[][] GenerateTable(ushort polynomial, bool inverseTable)
    {
        var table = new ushort[8][];

        for(var i = 0; i < 8; i++)
            table[i] = new ushort[256];

        if(!inverseTable)
        {
            for(uint i = 0; i < 256; i++)
            {
                uint entry = i;

                for(var j = 0; j < 8; j++)
                {
                    if((entry & 1) == 1)
                        entry = entry >> 1 ^ polynomial;
                    else
                        entry >>= 1;
                }

                table[0][i] = (ushort)entry;
            }
        }
        else
        {
            for(uint i = 0; i < 256; i++)
            {
                uint entry = i << 8;

                for(uint j = 0; j < 8; j++)
                {
                    if((entry & 0x8000) > 0)
                        entry = entry << 1 ^ polynomial;
                    else
                        entry <<= 1;

                    table[0][i] = (ushort)entry;
                }
            }
        }

        for(var slice = 1; slice < 8; slice++)
        for(var i = 0; i < 256; i++)
        {
            if(inverseTable)
                table[slice][i] = (ushort)(table[slice - 1][i] << 8 ^ table[0][table[slice - 1][i] >> 8]);
            else
                table[slice][i] = (ushort)(table[slice - 1][i] >> 8 ^ table[0][table[slice - 1][i] & 0xFF]);
        }

        return table;
    }

    /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
    /// <param name="filename">File path.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    /// <param name="polynomial">CRC polynomial</param>
    /// <param name="seed">CRC seed</param>
    /// <param name="table">CRC lookup table</param>
    /// <param name="inverse">Is CRC inverted?</param>
    public static string File(string filename, out byte[] hash, ushort polynomial, ushort seed, ushort[][] table,
                              bool   inverse)
    {
        bool useNative = Native.IsSupported;

        bool useCcitt = polynomial == CRC16CcittContext.CRC16_CCITT_POLY &&
                        seed       == CRC16CcittContext.CRC16_CCITT_SEED &&
                        inverse;

        bool useIbm = polynomial == CRC16IbmContext.CRC16_IBM_POLY &&
                      seed       == CRC16IbmContext.CRC16_IBM_SEED &&
                      !inverse;

        IntPtr nativeContext = IntPtr.Zero;

        var fileStream = new FileStream(filename, FileMode.Open);

        ushort localHashInt = seed;

        switch(useNative)
        {
            case true when useCcitt:
                nativeContext = crc16_ccitt_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
            case true when useIbm:
                nativeContext = crc16_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
        }

        ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

        var buffer = new byte[65536];
        int read   = fileStream.EnsureRead(buffer, 0, 65536);

        while(read > 0)
        {
            switch(useNative)
            {
                case true when useCcitt:
                    crc16_ccitt_update(nativeContext, buffer, (uint)read);

                    break;
                case true when useIbm:
                    crc16_update(nativeContext, buffer, (uint)read);

                    break;
                default:
                {
                    if(inverse)
                        StepInverse(ref localHashInt, localTable, buffer, (uint)read);
                    else
                        Step(ref localHashInt, localTable, buffer, (uint)read);

                    break;
                }
            }

            read = fileStream.EnsureRead(buffer, 0, 65536);
        }

        localHashInt ^= seed;

        switch(useNative)
        {
            case true when useCcitt:
                crc16_ccitt_final(nativeContext, ref localHashInt);
                crc16_ccitt_free(nativeContext);

                break;
            case true when useIbm:
                crc16_final(nativeContext, ref localHashInt);
                crc16_free(nativeContext);

                break;
            default:
            {
                if(inverse)
                    localHashInt = (ushort)~localHashInt;

                break;
            }
        }

        hash = BigEndianBitConverter.GetBytes(localHashInt);

        var crc16Output = new StringBuilder();

        foreach(byte h in hash)
            crc16Output.Append(h.ToString("x2"));

        fileStream.Close();

        return crc16Output.ToString();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of the data buffer to hash.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    /// <param name="polynomial">CRC polynomial</param>
    /// <param name="seed">CRC seed</param>
    /// <param name="table">CRC lookup table</param>
    /// <param name="inverse">Is CRC inverted?</param>
    public static string Data(byte[] data, uint len, out byte[] hash, ushort polynomial, ushort seed, ushort[][] table,
                              bool   inverse)
    {
        bool useNative = Native.IsSupported;

        bool useCcitt = polynomial == CRC16CcittContext.CRC16_CCITT_POLY &&
                        seed       == CRC16CcittContext.CRC16_CCITT_SEED &&
                        inverse;

        bool useIbm = polynomial == CRC16IbmContext.CRC16_IBM_POLY &&
                      seed       == CRC16IbmContext.CRC16_IBM_SEED &&
                      !inverse;

        IntPtr nativeContext = IntPtr.Zero;

        ushort localHashInt = seed;

        switch(useNative)
        {
            case true when useCcitt:
                nativeContext = crc16_ccitt_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
            case true when useIbm:
                nativeContext = crc16_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
        }

        ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

        switch(useNative)
        {
            case true when useCcitt:
                crc16_ccitt_update(nativeContext, data, len);

                break;
            case true when useIbm:
                crc16_update(nativeContext, data, len);

                break;
            default:
            {
                if(inverse)
                    StepInverse(ref localHashInt, localTable, data, len);
                else
                    Step(ref localHashInt, localTable, data, len);

                break;
            }
        }

        localHashInt ^= seed;

        switch(useNative)
        {
            case true when useCcitt:
                crc16_ccitt_final(nativeContext, ref localHashInt);
                crc16_ccitt_free(nativeContext);

                break;
            case true when useIbm:
                crc16_final(nativeContext, ref localHashInt);
                crc16_free(nativeContext);

                break;
            default:
            {
                if(inverse)
                    localHashInt = (ushort)~localHashInt;

                break;
            }
        }

        hash = BigEndianBitConverter.GetBytes(localHashInt);

        var crc16Output = new StringBuilder();

        foreach(byte h in hash)
            crc16Output.Append(h.ToString("x2"));

        return crc16Output.ToString();
    }

    /// <summary>Calculates the CRC16 of the specified buffer with the specified parameters</summary>
    /// <param name="buffer">Buffer</param>
    /// <param name="polynomial">Polynomial</param>
    /// <param name="seed">Seed</param>
    /// <param name="table">Pre-generated lookup table</param>
    /// <param name="inverse">Inverse CRC</param>
    /// <returns>CRC16</returns>
    public static ushort Calculate(byte[] buffer, ushort polynomial, ushort seed, ushort[][] table, bool inverse)
    {
        bool useNative = Native.IsSupported;

        bool useCcitt = polynomial == CRC16CcittContext.CRC16_CCITT_POLY &&
                        seed       == CRC16CcittContext.CRC16_CCITT_SEED &&
                        inverse;

        bool useIbm = polynomial == CRC16IbmContext.CRC16_IBM_POLY &&
                      seed       == CRC16IbmContext.CRC16_IBM_SEED &&
                      !inverse;

        IntPtr nativeContext = IntPtr.Zero;

        ushort localHashInt = seed;

        switch(useNative)
        {
            case true when useCcitt:
                nativeContext = crc16_ccitt_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
            case true when useIbm:
                nativeContext = crc16_init();
                useNative     = nativeContext != IntPtr.Zero;

                break;
        }

        ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

        switch(useNative)
        {
            case true when useCcitt:
                crc16_ccitt_update(nativeContext, buffer, (uint)buffer.Length);

                break;
            case true when useIbm:
                crc16_update(nativeContext, buffer, (uint)buffer.Length);

                break;
            default:
            {
                if(inverse)
                    StepInverse(ref localHashInt, localTable, buffer, (uint)buffer.Length);
                else
                    Step(ref localHashInt, localTable, buffer, (uint)buffer.Length);

                break;
            }
        }

        localHashInt ^= seed;

        switch(useNative)
        {
            case true when useCcitt:
                crc16_ccitt_final(nativeContext, ref localHashInt);
                crc16_ccitt_free(nativeContext);

                break;
            case true when useIbm:
                crc16_final(nativeContext, ref localHashInt);
                crc16_free(nativeContext);

                break;
            default:
            {
                if(inverse)
                    localHashInt = (ushort)~localHashInt;

                break;
            }
        }

        return localHashInt;
    }
}