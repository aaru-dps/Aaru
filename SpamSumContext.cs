// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SpamSumContext.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the SpamSum fuzzy hashing algorithm.
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

//  Based on ssdeep
//  Copyright (C) 2002 Andrew Tridgell <tridge@samba.org>
//  Copyright (C) 2006 ManTech International Corporation
//  Copyright (C) 2013 Helmut Grohne <helmut@subdivi.de>
//
//  Earlier versions of this code were named fuzzy.c and can be found at:
//      http://www.samba.org/ftp/unpacked/junkcode/spamsum/
//      http://ssdeep.sf.net/

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Checksums;

/// <inheritdoc />
/// <summary>Implements the SpamSum fuzzy hashing algorithm.</summary>
public sealed class SpamSumContext : IChecksum
{
    const uint ROLLING_WINDOW   = 7;
    const uint MIN_BLOCKSIZE    = 3;
    const uint HASH_PRIME       = 0x01000193;
    const uint HASH_INIT        = 0x28021967;
    const uint NUM_BLOCKHASHES  = 31;
    const uint SPAMSUM_LENGTH   = 64;
    const uint FUZZY_MAX_RESULT = (2 * SPAMSUM_LENGTH) + 20;

    //"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    readonly byte[] _b64 =
    {
        0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50, 0x51, 0x52,
        0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A,
        0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x30, 0x31,
        0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x2B, 0x2F
    };

    FuzzyState _self;

    /// <summary>Initializes the SpamSum structures</summary>
    public SpamSumContext()
    {
        _self = new FuzzyState
        {
            Bh = new BlockhashContext[NUM_BLOCKHASHES]
        };

        for(int i = 0; i < NUM_BLOCKHASHES; i++)
            _self.Bh[i].Digest = new byte[SPAMSUM_LENGTH];

        _self.Bhstart          = 0;
        _self.Bhend            = 1;
        _self.Bh[0].H          = HASH_INIT;
        _self.Bh[0].Halfh      = HASH_INIT;
        _self.Bh[0].Digest[0]  = 0;
        _self.Bh[0].Halfdigest = 0;
        _self.Bh[0].Dlen       = 0;
        _self.TotalSize        = 0;
        roll_init();
    }

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of buffer to hash.</param>
    public void Update(byte[] data, uint len)
    {
        _self.TotalSize += len;

        for(int i = 0; i < len; i++)
            fuzzy_engine_step(data[i]);
    }

    /// <inheritdoc />
    /// <summary>Updates the hash with data.</summary>
    /// <param name="data">Data buffer.</param>
    public void Update(byte[] data) => Update(data, (uint)data.Length);

    /// <inheritdoc />
    /// <summary>Returns a byte array of the hash value.</summary>
    public byte[] Final() =>
        throw new NotImplementedException(Localization.SpamSum_does_not_have_a_binary_representation);

    /// <inheritdoc />
    /// <summary>Returns a base64 representation of the hash value.</summary>
    public string End()
    {
        FuzzyDigest(out byte[] result);

        return CToString(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void roll_init() => _self.Roll = new RollState
    {
        Window = new byte[ROLLING_WINDOW]
    };

    /*
     * a rolling hash, based on the Adler checksum. By using a rolling hash
     * we can perform auto resynchronisation after inserts/deletes

     * internally, h1 is the sum of the bytes in the window and h2
     * is the sum of the bytes times the index

     * h3 is a shift/xor based rolling hash, and is mostly needed to ensure that
     * we can cope with large blocksize values
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void roll_hash(byte c)
    {
        _self.Roll.H2 -= _self.Roll.H1;
        _self.Roll.H2 += ROLLING_WINDOW * c;

        _self.Roll.H1 += c;
        _self.Roll.H1 -= _self.Roll.Window[_self.Roll.N % ROLLING_WINDOW];

        _self.Roll.Window[_self.Roll.N % ROLLING_WINDOW] = c;
        _self.Roll.N++;

        /* The original spamsum AND'ed this value with 0xFFFFFFFF which
         * in theory should have no effect. This AND has been removed
         * for performance (jk) */
        _self.Roll.H3 <<= 5;
        _self.Roll.H3 ^=  c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    uint roll_sum() => _self.Roll.H1 + _self.Roll.H2 + _self.Roll.H3;

    /* A simple non-rolling hash, based on the FNV hash. */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint sum_hash(byte c, uint h) => (h * HASH_PRIME) ^ c;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static uint SSDEEP_BS(uint index) => MIN_BLOCKSIZE << (int)index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void fuzzy_try_fork_blockhash()
    {
        switch(_self.Bhend)
        {
            case >= NUM_BLOCKHASHES: return;

            // assert
            case 0: throw new Exception(Localization.Assertion_failed);
        }

        uint obh = _self.Bhend - 1;
        uint nbh = _self.Bhend;
        _self.Bh[nbh].H          = _self.Bh[obh].H;
        _self.Bh[nbh].Halfh      = _self.Bh[obh].Halfh;
        _self.Bh[nbh].Digest[0]  = 0;
        _self.Bh[nbh].Halfdigest = 0;
        _self.Bh[nbh].Dlen       = 0;
        ++_self.Bhend;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void fuzzy_try_reduce_blockhash()
    {
        if(_self.Bhstart >= _self.Bhend)
            throw new Exception(Localization.Assertion_failed);

        if(_self.Bhend - _self.Bhstart < 2)
            /* Need at least two working hashes. */
            return;

        if((ulong)SSDEEP_BS(_self.Bhstart) * SPAMSUM_LENGTH >= _self.TotalSize)
            /* Initial blocksize estimate would select this or a smaller
             * blocksize. */
            return;

        if(_self.Bh[_self.Bhstart + 1].Dlen < SPAMSUM_LENGTH / 2)
            /* Estimate adjustment would select this blocksize. */
            return;

        /* At this point we are clearly no longer interested in the
         * start_blocksize. Get rid of it. */
        ++_self.Bhstart;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void fuzzy_engine_step(byte c)
    {
        uint i;
        /* At each character we update the rolling hash and the normal hashes.
         * When the rolling hash hits a reset value then we emit a normal hash
         * as a element of the signature and reset the normal hash. */
        roll_hash(c);
        ulong h = roll_sum();

        for(i = _self.Bhstart; i < _self.Bhend; ++i)
        {
            _self.Bh[i].H     = sum_hash(c, _self.Bh[i].H);
            _self.Bh[i].Halfh = sum_hash(c, _self.Bh[i].Halfh);
        }

        for(i = _self.Bhstart; i < _self.Bhend; ++i)
        {
            /* With growing blocksize almost no runs fail the next test. */
            if(h % SSDEEP_BS(i) != SSDEEP_BS(i) - 1)
                /* Once this condition is false for one bs, it is
                 * automatically false for all further bs. I.e. if
                 * h === -1 (mod 2*bs) then h === -1 (mod bs). */
                break;

            /* We have hit a reset point. We now emit hashes which are
             * based on all characters in the piece of the message between
             * the last reset point and this one */
            if(0 == _self.Bh[i].Dlen)
                fuzzy_try_fork_blockhash();

            _self.Bh[i].Digest[_self.Bh[i].Dlen] = _b64[_self.Bh[i].H     % 64];
            _self.Bh[i].Halfdigest               = _b64[_self.Bh[i].Halfh % 64];

            if(_self.Bh[i].Dlen < SPAMSUM_LENGTH - 1)
            {
                /* We can have a problem with the tail overflowing. The
                 * easiest way to cope with this is to only reset the
                 * normal hash if we have room for more characters in
                 * our signature. This has the effect of combining the
                 * last few pieces of the message into a single piece
                 * */
                _self.Bh[i].Digest[++_self.Bh[i].Dlen] = 0;
                _self.Bh[i].H                          = HASH_INIT;

                if(_self.Bh[i].Dlen >= SPAMSUM_LENGTH / 2)
                    continue;

                _self.Bh[i].Halfh      = HASH_INIT;
                _self.Bh[i].Halfdigest = 0;
            }
            else
                fuzzy_try_reduce_blockhash();
        }
    }

    // CLAUNIA: Flags seems to never be used in ssdeep, so I just removed it for code simplicity
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void FuzzyDigest(out byte[] result)
    {
        var  sb     = new StringBuilder();
        uint bi     = _self.Bhstart;
        uint h      = roll_sum();
        int  remain = (int)(FUZZY_MAX_RESULT - 1); /* Exclude terminating '\0'. */
        result = new byte[FUZZY_MAX_RESULT];

        /* Verify that our elimination was not overeager. */
        if(!(bi == 0 || (ulong)SSDEEP_BS(bi) / 2 * SPAMSUM_LENGTH < _self.TotalSize))
            throw new Exception(Localization.Assertion_failed);

        /* Initial blocksize guess. */
        while((ulong)SSDEEP_BS(bi) * SPAMSUM_LENGTH < _self.TotalSize)
        {
            ++bi;

            if(bi >= NUM_BLOCKHASHES)
                throw new OverflowException(Localization.The_input_exceeds_data_types);
        }

        /* Adapt blocksize guess to actual digest length. */
        while(bi >= _self.Bhend)
            --bi;

        while(bi                > _self.Bhstart &&
              _self.Bh[bi].Dlen < SPAMSUM_LENGTH / 2)
            --bi;

        if(bi                > 0 &&
           _self.Bh[bi].Dlen < SPAMSUM_LENGTH / 2)
            throw new Exception(Localization.Assertion_failed);

        sb.AppendFormat("{0}:", SSDEEP_BS(bi));
        int i = Encoding.ASCII.GetBytes(sb.ToString()).Length;

        if(i <= 0)
            /* Maybe snprintf has set errno here? */
            throw new OverflowException(Localization.The_input_exceeds_data_types);

        if(i >= remain)
            throw new Exception(Localization.Assertion_failed);

        remain -= i;

        Array.Copy(Encoding.ASCII.GetBytes(sb.ToString()), 0, result, 0, i);

        int resultOff = i;

        i = (int)_self.Bh[bi].Dlen;

        if(i > remain)
            throw new Exception(Localization.Assertion_failed);

        Array.Copy(_self.Bh[bi].Digest, 0, result, resultOff, i);
        resultOff += i;
        remain    -= i;

        if(h != 0)
        {
            if(remain <= 0)
                throw new Exception(Localization.Assertion_failed);

            result[resultOff] = _b64[_self.Bh[bi].H % 64];

            if(i                 < 3                      ||
               result[resultOff] != result[resultOff - 1] ||
               result[resultOff] != result[resultOff - 2] ||
               result[resultOff] != result[resultOff - 3])
            {
                ++resultOff;
                --remain;
            }
        }
        else if(_self.Bh[bi].Digest[i] != 0)
        {
            if(remain <= 0)
                throw new Exception(Localization.Assertion_failed);

            result[resultOff] = _self.Bh[bi].Digest[i];

            if(i                 < 3                      ||
               result[resultOff] != result[resultOff - 1] ||
               result[resultOff] != result[resultOff - 2] ||
               result[resultOff] != result[resultOff - 3])
            {
                ++resultOff;
                --remain;
            }
        }

        if(remain <= 0)
            throw new Exception(Localization.Assertion_failed);

        result[resultOff++] = 0x3A; // ':'
        --remain;

        if(bi < _self.Bhend - 1)
        {
            ++bi;
            i = (int)_self.Bh[bi].Dlen;

            if(i > remain)
                throw new Exception(Localization.Assertion_failed);

            Array.Copy(_self.Bh[bi].Digest, 0, result, resultOff, i);
            resultOff += i;
            remain    -= i;

            if(h != 0)
            {
                if(remain <= 0)
                    throw new Exception(Localization.Assertion_failed);

                h                 = _self.Bh[bi].Halfh;
                result[resultOff] = _b64[h % 64];

                if(i                 < 3                      ||
                   result[resultOff] != result[resultOff - 1] ||
                   result[resultOff] != result[resultOff - 2] ||
                   result[resultOff] != result[resultOff - 3])
                {
                    ++resultOff;
                    --remain;
                }
            }
            else
            {
                i = _self.Bh[bi].Halfdigest;

                if(i != 0)
                {
                    if(remain <= 0)
                        throw new Exception(Localization.Assertion_failed);

                    result[resultOff] = (byte)i;

                    if(i                 < 3                      ||
                       result[resultOff] != result[resultOff - 1] ||
                       result[resultOff] != result[resultOff - 2] ||
                       result[resultOff] != result[resultOff - 3])
                    {
                        ++resultOff;
                        --remain;
                    }
                }
            }
        }
        else if(h != 0)
        {
            if(_self.Bh[bi].Dlen != 0)
                throw new Exception(Localization.Assertion_failed);

            if(remain <= 0)
                throw new Exception(Localization.Assertion_failed);

            result[resultOff++] = _b64[_self.Bh[bi].H % 64];
            /* No need to bother with FUZZY_FLAG_ELIMSEQ, because this
             * digest has length 1. */
            --remain;
        }

        result[resultOff] = 0;
    }

    /// <summary>Gets the hash of a file</summary>
    /// <param name="filename">File path.</param>
    public static byte[] File(string filename) =>
        throw new NotImplementedException(Localization.SpamSum_does_not_have_a_binary_representation);

    /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
    /// <param name="filename">File path.</param>
    /// <param name="hash">Byte array of the hash value.</param>
    public static string File(string filename, out byte[] hash) =>
        throw new NotImplementedException(Localization.Not_yet_implemented);

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="len">Length of the data buffer to hash.</param>
    /// <param name="hash">null</param>
    /// <returns>Base64 representation of SpamSum $blocksize:$hash:$hash</returns>
    public static string Data(byte[] data, uint len, out byte[] hash)
    {
        var fuzzyContext = new SpamSumContext();

        fuzzyContext.Update(data, len);

        hash = null;

        return fuzzyContext.End();
    }

    /// <summary>Gets the hash of the specified data buffer.</summary>
    /// <param name="data">Data buffer.</param>
    /// <param name="hash">null</param>
    /// <returns>Base64 representation of SpamSum $blocksize:$hash:$hash</returns>
    public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);

    // Converts an ASCII null-terminated string to .NET string
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string CToString(byte[] cString)
    {
        int count = 0;

        // ReSharper disable once LoopCanBeConvertedToQuery
        // LINQ is six times slower
        foreach(byte c in cString)
        {
            if(c == 0)
                break;

            count++;
        }

        return Encoding.ASCII.GetString(cString, 0, count);
    }

    struct RollState
    {
        public byte[] Window;

        // ROLLING_WINDOW
        public uint H1;
        public uint H2;
        public uint H3;
        public uint N;
    }

    /* A blockhash contains a signature state for a specific (implicit) blocksize.
     * The blocksize is given by SSDEEP_BS(index). The h and halfh members are the
     * FNV hashes, where halfh stops to be reset after digest is SPAMSUM_LENGTH/2
     * long. The halfh hash is needed be able to truncate digest for the second
     * output hash to stay compatible with ssdeep output. */
    struct BlockhashContext
    {
        public uint   H;
        public uint   Halfh;
        public byte[] Digest;

        // SPAMSUM_LENGTH
        public byte Halfdigest;
        public uint Dlen;
    }

    struct FuzzyState
    {
        public uint               Bhstart;
        public uint               Bhend;
        public BlockhashContext[] Bh;

        //NUM_BLOCKHASHES
        public ulong     TotalSize;
        public RollState Roll;
    }
}