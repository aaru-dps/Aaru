/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : SpamSumContext.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Checksums.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements the SpamSum fuzzy hashing algorithm.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2015 Claunia.com
****************************************************************************/
//  Based on ssdeep
//  Copyright (C) 2002 Andrew Tridgell <tridge@samba.org>
//  Copyright (C) 2006 ManTech International Corporation
//  Copyright (C) 2013 Helmut Grohne <helmut@subdivi.de>
//
//  Earlier versions of this code were named fuzzy.c and can be found at:
//      http://www.samba.org/ftp/unpacked/junkcode/spamsum/
//      http://ssdeep.sf.net/

using System;
using System.Text;

namespace DiscImageChef.Checksums
{
    /// <summary>
    /// Provides a UNIX similar API to calculate Fuzzy Hash (SpamSum).
    /// </summary>
    public class SpamSumContext
    {
        const UInt32 ROLLING_WINDOW = 7;
        const UInt32 MIN_BLOCKSIZE = 3;
        const UInt32 HASH_PRIME = 0x01000193;
        const UInt32 HASH_INIT = 0x28021967;
        const UInt32 NUM_BLOCKHASHES = 31;
        const UInt32 SPAMSUM_LENGTH = 64;
        const UInt32 FUZZY_MAX_RESULT = (2 * SPAMSUM_LENGTH + 20);
        //"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        readonly byte[] b64 =
        {0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50,
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5A, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66,
            0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E,
            0x6F, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76,
            0x77, 0x78, 0x79, 0x7A, 0x30, 0x31, 0x32, 0x33,
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x2B, 0x2F
        };

        struct roll_state
        {
            public byte[] window;
            // ROLLING_WINDOW
            public UInt32 h1;
            public UInt32 h2;
            public UInt32 h3;
            public UInt32 n;
        }

        /* A blockhash contains a signature state for a specific (implicit) blocksize.
         * The blocksize is given by SSDEEP_BS(index). The h and halfh members are the
         * FNV hashes, where halfh stops to be reset after digest is SPAMSUM_LENGTH/2
         * long. The halfh hash is needed be able to truncate digest for the second
         * output hash to stay compatible with ssdeep output. */
        struct blockhash_context
        {
            public UInt32 h;
            public UInt32 halfh;
            public byte[] digest;
            // SPAMSUM_LENGTH
            public byte halfdigest;
            public UInt32 dlen;
        }

        struct fuzzy_state
        {
            public UInt32 bhstart;
            public UInt32 bhend;
            public blockhash_context[] bh;
            //NUM_BLOCKHASHES
            public UInt64 total_size;
            public roll_state roll;
        }

        fuzzy_state self;

        void roll_init()
        {
            self.roll = new roll_state();
            self.roll.window = new byte[ROLLING_WINDOW];
        }

        /// <summary>
        /// Initializes the SpamSum structures
        /// </summary>
        public void Init()
        {
            self = new fuzzy_state();
            self.bh = new blockhash_context[NUM_BLOCKHASHES];
            for (int i = 0; i < NUM_BLOCKHASHES; i++)
                self.bh[i].digest = new byte[SPAMSUM_LENGTH];

            self.bhstart = 0;
            self.bhend = 1;
            self.bh[0].h = HASH_INIT;
            self.bh[0].halfh = HASH_INIT;
            self.bh[0].digest[0] = 0;
            self.bh[0].halfdigest = 0;
            self.bh[0].dlen = 0;
            self.total_size = 0;
            roll_init();
        }

        /*
         * a rolling hash, based on the Adler checksum. By using a rolling hash
         * we can perform auto resynchronisation after inserts/deletes

         * internally, h1 is the sum of the bytes in the window and h2
         * is the sum of the bytes times the index

         * h3 is a shift/xor based rolling hash, and is mostly needed to ensure that
         * we can cope with large blocksize values
         */
        void roll_hash(byte c)
        {
            self.roll.h2 -= self.roll.h1;
            self.roll.h2 += ROLLING_WINDOW * (UInt32)c;

            self.roll.h1 += (UInt32)c;
            self.roll.h1 -= (UInt32)self.roll.window[self.roll.n % ROLLING_WINDOW];

            self.roll.window[self.roll.n % ROLLING_WINDOW] = c;
            self.roll.n++;

            /* The original spamsum AND'ed this value with 0xFFFFFFFF which
             * in theory should have no effect. This AND has been removed
             * for performance (jk) */
            self.roll.h3 <<= 5;
            self.roll.h3 ^= c;
        }

        UInt32 roll_sum()
        {
            return self.roll.h1 + self.roll.h2 + self.roll.h3;
        }

        /* A simple non-rolling hash, based on the FNV hash. */
        static UInt32 sum_hash(byte c, UInt32 h)
        {
            return (h * HASH_PRIME) ^ c;
        }

        static UInt32 SSDEEP_BS(UInt32 index)
        {
            return (MIN_BLOCKSIZE << (int)index);
        }

        void fuzzy_try_fork_blockhash()
        {
            uint obh, nbh;

            if (self.bhend >= NUM_BLOCKHASHES)
                return;

            if (self.bhend == 0) // assert
                throw new Exception("Assertion failed");

            obh = self.bhend - 1;
            nbh = self.bhend;
            self.bh[nbh].h = self.bh[obh].h;
            self.bh[nbh].halfh = self.bh[obh].halfh;
            self.bh[nbh].digest[0] = 0;
            self.bh[nbh].halfdigest = 0;
            self.bh[nbh].dlen = 0;
            ++self.bhend;
        }

        void fuzzy_try_reduce_blockhash()
        {
            if (self.bhstart >= self.bhend)
                throw new Exception("Assertion failed");

            if (self.bhend - self.bhstart < 2)
                /* Need at least two working hashes. */
                return;
            if ((UInt64)SSDEEP_BS(self.bhstart) * SPAMSUM_LENGTH >=
                self.total_size)
                /* Initial blocksize estimate would select this or a smaller
                 * blocksize. */
                return;
            if (self.bh[self.bhstart + 1].dlen < SPAMSUM_LENGTH / 2)
                /* Estimate adjustment would select this blocksize. */
                return;
            /* At this point we are clearly no longer interested in the
             * start_blocksize. Get rid of it. */
            ++self.bhstart;
        }

        void fuzzy_engine_step(byte c)
        {
            UInt64 h;
            UInt32 i;
            /* At each character we update the rolling hash and the normal hashes.
             * When the rolling hash hits a reset value then we emit a normal hash
             * as a element of the signature and reset the normal hash. */
            roll_hash(c);
            h = roll_sum();

            for (i = self.bhstart; i < self.bhend; ++i)
            {
                self.bh[i].h = sum_hash(c, self.bh[i].h);
                self.bh[i].halfh = sum_hash(c, self.bh[i].halfh);
            }

            for (i = self.bhstart; i < self.bhend; ++i)
            {
                /* With growing blocksize almost no runs fail the next test. */
                if (h % SSDEEP_BS(i) != SSDEEP_BS(i) - 1)
                    /* Once this condition is false for one bs, it is
                     * automatically false for all further bs. I.e. if
                     * h === -1 (mod 2*bs) then h === -1 (mod bs). */
                    break;
                /* We have hit a reset point. We now emit hashes which are
                 * based on all characters in the piece of the message between
                 * the last reset point and this one */
                if (0 == self.bh[i].dlen)
                {
                    /* Can only happen 30 times. */
                    /* First step for this blocksize. Clone next. */
                    fuzzy_try_fork_blockhash();
                }
                self.bh[i].digest[self.bh[i].dlen] = b64[self.bh[i].h % 64];
                self.bh[i].halfdigest = b64[self.bh[i].halfh % 64];
                if (self.bh[i].dlen < SPAMSUM_LENGTH - 1)
                {
                    /* We can have a problem with the tail overflowing. The
                     * easiest way to cope with this is to only reset the
                     * normal hash if we have room for more characters in
                     * our signature. This has the effect of combining the
                     * last few pieces of the message into a single piece
                     * */
                    self.bh[i].digest[++(self.bh[i].dlen)] = 0;
                    self.bh[i].h = HASH_INIT;
                    if (self.bh[i].dlen < SPAMSUM_LENGTH / 2)
                    {
                        self.bh[i].halfh = HASH_INIT;
                        self.bh[i].halfdigest = 0;
                    }
                }
                else
                    fuzzy_try_reduce_blockhash();
            }
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            self.total_size += len;
            for (int i = 0; i < len; i++)
                fuzzy_engine_step(data[i]);
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        // CLAUNIA: Flags seems to never be used in ssdeep, so I just removed it for code simplicity
        UInt32 fuzzy_digest(out byte[] result)
        {
            StringBuilder sb = new StringBuilder();
            UInt32 bi = self.bhstart;
            UInt32 h = roll_sum();
            int i, result_off;
            int remain = (int)(FUZZY_MAX_RESULT - 1); /* Exclude terminating '\0'. */
            result = new byte[FUZZY_MAX_RESULT];
            /* Verify that our elimination was not overeager. */
            if (!(bi == 0 || (UInt64)SSDEEP_BS(bi) / 2 * SPAMSUM_LENGTH < self.total_size))
                throw new Exception("Assertion failed");

            result_off = 0;

            /* Initial blocksize guess. */
            while ((UInt64)SSDEEP_BS(bi) * SPAMSUM_LENGTH < self.total_size)
            {
                ++bi;
                if (bi >= NUM_BLOCKHASHES)
                {
                    throw new OverflowException("The input exceeds data types.");
                }
            }
            /* Adapt blocksize guess to actual digest length. */
            while (bi >= self.bhend)
                --bi;
            while (bi > self.bhstart && self.bh[bi].dlen < SPAMSUM_LENGTH / 2)
                --bi;
            if ((bi > 0 && self.bh[bi].dlen < SPAMSUM_LENGTH / 2))
                throw new Exception("Assertion failed");

            sb.AppendFormat("{0}:", SSDEEP_BS(bi));
            i = Encoding.ASCII.GetBytes(sb.ToString()).Length;
            if (i <= 0)
                /* Maybe snprintf has set errno here? */
                throw new OverflowException("The input exceeds data types.");
            if (i >= remain)
                throw new Exception("Assertion failed");
            remain -= i;

            Array.Copy(Encoding.ASCII.GetBytes(sb.ToString()), 0, result, 0, i);

            result_off += i;

            i = (int)self.bh[bi].dlen;
            if (i > remain)
                throw new Exception("Assertion failed");

            Array.Copy(self.bh[bi].digest, 0, result, result_off, i);
            result_off += i;
            remain -= i;
            if (h != 0)
            {
                if (remain <= 0)
                    throw new Exception("Assertion failed");
                result[result_off] = b64[self.bh[bi].h % 64];
                if (i < 3 ||
                    result[result_off] != result[result_off - 1] ||
                    result[result_off] != result[result_off - 2] ||
                    result[result_off] != result[result_off - 3])
                {
                    ++result_off;
                    --remain;
                }
            }
            else if (self.bh[bi].digest[i] != 0)
            {
                if (remain <= 0)
                    throw new Exception("Assertion failed");
                result[result_off] = self.bh[bi].digest[i];
                if (i < 3 ||
                    result[result_off] != result[result_off - 1] ||
                    result[result_off] != result[result_off - 2] ||
                    result[result_off] != result[result_off - 3])
                {
                    ++result_off;
                    --remain;
                }
            }
            if (remain <= 0)
                throw new Exception("Assertion failed");
            result[result_off++] = 0x3A; // ':'
            --remain;
            if (bi < self.bhend - 1)
            {
                ++bi;
                i = (int)self.bh[bi].dlen;
                if (i > remain)
                    throw new Exception("Assertion failed");
                Array.Copy(self.bh[bi].digest, 0, result, result_off, i);
                result_off += i;
                remain -= i;

                if (h != 0)
                {
                    if (remain <= 0)
                        throw new Exception("Assertion failed");
                    h = self.bh[bi].halfh;
                    result[result_off] = b64[h % 64];
                    if (i < 3 ||
                        result[result_off] != result[result_off - 1] ||
                        result[result_off] != result[result_off - 2] ||
                        result[result_off] != result[result_off - 3])
                    {
                        ++result_off;
                        --remain;
                    }
                }
                else
                {
                    i = self.bh[bi].halfdigest;
                    if (i != 0)
                    {
                        if (remain <= 0)
                            throw new Exception("Assertion failed");
                        result[result_off] = (byte)i;
                        if (i < 3 ||
                            result[result_off] != result[result_off - 1] ||
                            result[result_off] != result[result_off - 2] ||
                            result[result_off] != result[result_off - 3])
                        {
                            ++result_off;
                            --remain;
                        }
                    }
                }
            }
            else if (h != 0)
            {
                if (self.bh[bi].dlen != 0)
                    throw new Exception("Assertion failed");
                if (remain <= 0)
                    throw new Exception("Assertion failed");
                result[result_off++] = b64[self.bh[bi].h % 64];
                /* No need to bother with FUZZY_FLAG_ELIMSEQ, because this
                 * digest has length 1. */
                --remain;
            }
            result[result_off] = 0;
            return 0;
        }

        /// <summary>
        /// Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            // SpamSum does not have a binary representation, or so it seems
            throw new NotImplementedException("SpamSum does not have a binary representation.");
        }

        /// <summary>
        /// Returns a base64 representation of the hash value.
        /// </summary>
        public string End()
        {
            byte[] result;
            fuzzy_digest(out result);

            return CToString(result);
        }

        /// <summary>
        /// Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            // SpamSum does not have a binary representation, or so it seems
            throw new NotImplementedException("SpamSum does not have a binary representation.");
        }

        /// <summary>
        /// Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            // SpamSum does not have a binary representation, or so it seems
            throw new NotImplementedException("Not yet implemented.");
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">null</param>
        /// <returns>Base64 representation of SpamSum $blocksize:$hash:$hash</returns>
        public string Data(byte[] data, uint len, out byte[] hash)
        {
            SpamSumContext fuzzyContext = new SpamSumContext();
            fuzzyContext.Init();

            fuzzyContext.Update(data, len);
            hash = null;

            byte[] result;
            fuzzy_digest(out result);

            return CToString(result);
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">null</param>
        /// <returns>Base64 representation of SpamSum $blocksize:$hash:$hash</returns>
        public string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }

        // Converts an ASCII null-terminated string to .NET string
        private string CToString(byte[] CString)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < CString.Length; i++)
            {
                if (CString[i] == 0)
                    break;

                sb.Append(Encoding.ASCII.GetString(CString, i, 1));
            }

            return sb.ToString();
        }
    }
}

