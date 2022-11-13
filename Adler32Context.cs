// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Adler32Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements an Adler-32 algorithm.
//
// --[ License ] --------------------------------------------------------------
//
//     This software is provided 'as-is', without any express or implied
//     warranty.  In no event will the authors be held liable for any damages
//     arising from the use of this software.
//
//     Permission is granted to anyone to use this software for any purpose,
//     including commercial applications, and to alter it and redistribute it
//     freely, subject to the following restrictions:
//
//  1. The origin of this software must not be misrepresented; you must not
//     claim that you wrote the original software. If you use this software
//     in a product, an acknowledgment in the product documentation would be
//     appreciated but is not required.
//
//  2. Altered source versions must be plainly marked as such, and must not be
//     misrepresented as being the original software.
//  3. This notice may not be removed or altered from any source distribution.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// Copyright (C) 1995-2011 Mark Adler
// Copyright (C) Jean-loup Gailly
// ****************************************************************************/

namespace Aaru.Checksums;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using Aaru.Checksums.Adler32;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Ssse3 = System.Runtime.Intrinsics.X86.Ssse3;

/// <inheritdoc />
/// <summary>Implements the Adler-32 algorithm</summary>
public sealed class Adler32Context : IChecksum
{
    internal const ushort ADLER_MODULE = 65521;
    internal const uint   NMAX         = 5552;
    readonly       IntPtr _nativeContext;
    readonly       bool   _useNative;
    ushort                _sum1, _sum2;

    /// <summary>Initializes the Adler-32 sums</summary>
    public Adler32Context()
    {
        _sum1 = 1;
        _sum2 = 0;

        if(!Native.IsSupported)
            return;

        _nativeContext = adler32_init();
        _useNative     = _nativeContext != IntPtr.Zero;
    }

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of buffer to hash.</param>
    public void Update(byte[] data, uint len) => Step(ref _sum1, ref _sum2, data, len, _useNative, _nativeContext);

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    public void Update(byte[] data) => Update(data, (uint)data.Length);

    /// <inheritdoc />
    /// <summary>Returns a byte array of the hash value.</summary>
    public byte[] Final()
    {
        var finalSum = (uint)((_sum2 << 16) | _sum1);

        if(!_useNative)
            return BigEndianBitConverter.GetBytes(finalSum);

        adler32_final(_nativeContext, ref finalSum);
        adler32_free(_nativeContext);

        return BigEndianBitConverter.GetBytes(finalSum);
    }

    /// <inheritdoc />
    /// <summary>Returns a hexadecimal representation of the hash value.</summary>
    public string End()
    {
        var finalSum = (uint)((_sum2 << 16) | _sum1);

        if(_useNative)
        {
            adler32_final(_nativeContext, ref finalSum);
            adler32_free(_nativeContext);
        }

        var adlerOutput = new StringBuilder();

        for(var i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
            adlerOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

        return adlerOutput.ToString();
    }

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern IntPtr adler32_init();

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int adler32_update(IntPtr ctx, byte[] data, uint len);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern int adler32_final(IntPtr ctx, ref uint checksum);

    [DllImport("libAaru.Checksums.Native", SetLastError = true)]
    static extern void adler32_free(IntPtr ctx);

    static void Step(ref ushort preSum1, ref ushort preSum2, byte[] data, uint len, bool useNative,
                     IntPtr nativeContext)
    {
        if(useNative)
        {
            adler32_update(nativeContext, data, len);

            return;
        }

        if(Ssse3.IsSupported)
        {
            Adler32.Ssse3.Step(ref preSum1, ref preSum2, data, len);

            return;
        }

        if(AdvSimd.IsSupported)
        {
            Neon.Step(ref preSum1, ref preSum2, data, len);

            return;
        }

        uint sum1    = preSum1;
        uint sum2    = preSum2;
        var  dataOff = 0;

        switch(len)
        {
            /* in case user likes doing a byte at a time, keep it fast */
            case 1:
            {
                sum1 += data[dataOff];

                if(sum1 >= ADLER_MODULE)
                    sum1 -= ADLER_MODULE;

                sum2 += sum1;

                if(sum2 >= ADLER_MODULE)
                    sum2 -= ADLER_MODULE;

                preSum1 = (ushort)(sum1 & 0xFFFF);
                preSum2 = (ushort)(sum2 & 0xFFFF);

                return;
            }
            /* in case short lengths are provided, keep it somewhat fast */
            case < 16:
            {
                while(len-- > 0)
                {
                    sum1 += data[dataOff++];
                    sum2 += sum1;
                }

                if(sum1 >= ADLER_MODULE)
                    sum1 -= ADLER_MODULE;

                sum2    %= ADLER_MODULE; /* only added so many ADLER_MODULE's */
                preSum1 =  (ushort)(sum1 & 0xFFFF);
                preSum2 =  (ushort)(sum2 & 0xFFFF);

                return;
            }
        }

        /* do length NMAX blocks -- requires just one modulo operation */
        while(len >= NMAX)
        {
            len -= NMAX;
            uint n = NMAX / 16;

            do
            {
                sum1 += data[dataOff];
                sum2 += sum1;
                sum1 += data[dataOff + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 4];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 2 + 1];
                sum2 += sum1;

                /* 16 sums unrolled */
                dataOff += 16;
            } while(--n != 0);

            sum1 %= ADLER_MODULE;
            sum2 %= ADLER_MODULE;
        }

        /* do remaining bytes (less than NMAX, still just one modulo) */
        if(len != 0)
        {
            /* avoid modulos if none remaining */
            while(len >= 16)
            {
                len  -= 16;
                sum1 += data[dataOff];
                sum2 += sum1;
                sum1 += data[dataOff + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 4];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 4 + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 2 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 1];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 2];
                sum2 += sum1;
                sum1 += data[dataOff + 8 + 4 + 2 + 1];
                sum2 += sum1;

                dataOff += 16;
            }

            while(len-- != 0)
            {
                sum1 += data[dataOff++];
                sum2 += sum1;
            }

            sum1 %= ADLER_MODULE;
            sum2 %= ADLER_MODULE;
        }

        preSum1 = (ushort)(sum1 & 0xFFFF);
        preSum2 = (ushort)(sum2 & 0xFFFF);
    }

    /// <summary>Gets the hash of a file</summary>
    /// <param name="filename">File path.</param>
    public static byte[] File(string filename)
    {
        File(filename, out byte[] hash);

        return hash;
    }

    /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
    /// <param name="filename">File path.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string File(string filename, out byte[] hash)
    {
        bool   useNative     = Native.IsSupported;
        IntPtr nativeContext = IntPtr.Zero;

        if(useNative)
        {
            nativeContext = adler32_init();

            if(nativeContext == IntPtr.Zero)
                useNative = false;
        }

        var fileStream = new FileStream(filename, FileMode.Open);

        ushort localSum1 = 1;
        ushort localSum2 = 0;

        var buffer = new byte[65536];
        int read   = fileStream.Read(buffer, 0, 65536);

        while(read > 0)
        {
            Step(ref localSum1, ref localSum2, buffer, (uint)read, useNative, nativeContext);
            read = fileStream.Read(buffer, 0, 65536);
        }

        var finalSum = (uint)((localSum2 << 16) | localSum1);

        if(useNative)
        {
            adler32_final(nativeContext, ref finalSum);
            adler32_free(nativeContext);
        }

        hash = BigEndianBitConverter.GetBytes(finalSum);

        var adlerOutput = new StringBuilder();

        foreach(byte h in hash)
            adlerOutput.Append(h.ToString("x2"));

        fileStream.Close();

        return adlerOutput.ToString();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of the data buffer to hash.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string Data(byte[] data, uint len, out byte[] hash)
    {
        bool   useNative     = Native.IsSupported;
        IntPtr nativeContext = IntPtr.Zero;

        if(useNative)
        {
            nativeContext = adler32_init();

            if(nativeContext == IntPtr.Zero)
                useNative = false;
        }

        ushort localSum1 = 1;
        ushort localSum2 = 0;

        Step(ref localSum1, ref localSum2, data, len, useNative, nativeContext);

        var finalSum = (uint)((localSum2 << 16) | localSum1);

        if(useNative)
        {
            adler32_final(nativeContext, ref finalSum);
            adler32_free(nativeContext);
        }

        hash = BigEndianBitConverter.GetBytes(finalSum);

        var adlerOutput = new StringBuilder();

        foreach(byte h in hash)
            adlerOutput.Append(h.ToString("x2"));

        return adlerOutput.ToString();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
}