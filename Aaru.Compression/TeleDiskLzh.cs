// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : TeleDiskLzh.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Compression algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decompress TeleDisk variant of LZH.
//
// --[ License ] --------------------------------------------------------------
//
//     Redistribution and use in source and binary forms, with or without modification, are permitted provided that
//     the following conditions are met:
//
//     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
//     following disclaimer.
//
//     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
//     following disclaimer in the documentation and/or other materials provided with the distribution.
//
//     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
//     products derived from this software without specific prior written permission.
//
//     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
//     WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
//     PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
//     ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
//     TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//     HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//     NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
//     POSSIBILITY OF SUCH DAMAGE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2017 Miodrag Milanovic
// Copyright © 1988 Haruhiko OKUMURA
// Copyright © 1988 Haruyasu YOSHIZAKI
// Copyright © 1988 Kenji RIKITAKE
// ****************************************************************************/

using System;
using System.IO;

namespace Aaru.Compression
{
    /*
     * Based on Japanese version 29-NOV-1988
     * LZSS coded by Haruhiko OKUMURA
     * Adaptive Huffman Coding coded by Haruyasu YOSHIZAKI
     * Edited and translated to English by Kenji RIKITAKE
     */
    /// <summary>Implements the TeleDisk version of LZH</summary>
    public class TeleDiskLzh
    {
        const int BUFSZ = 512;

        /* LZSS Parameters */

        const int N         = 4096; /* Size of string buffer */
        const int F         = 60;   /* Size of look-ahead buffer */
        const int THRESHOLD = 2;

        /* Huffman coding parameters */

        const int N_CHAR = 256 - THRESHOLD + F;
        /* character code (= 0..N_CHAR-1) */
        const int T        = (N_CHAR * 2) - 1; /* Size of table */
        const int ROOT     = T            - 1; /* root position */
        const int MAX_FREQ = 0x8000;

        /*
         * Tables for encoding/decoding upper 6 bits of
         * sliding dictionary pointer
         */

        /* decoder table */
        readonly byte[] _dCode =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05,
            0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09,
            0x09, 0x09, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0D, 0x0D, 0x0D, 0x0D, 0x0E, 0x0E, 0x0E, 0x0E, 0x0F, 0x0F, 0x0F, 0x0F, 0x10, 0x10,
            0x10, 0x10, 0x11, 0x11, 0x11, 0x11, 0x12, 0x12, 0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 0x14, 0x14, 0x14,
            0x15, 0x15, 0x15, 0x15, 0x16, 0x16, 0x16, 0x16, 0x17, 0x17, 0x17, 0x17, 0x18, 0x18, 0x19, 0x19, 0x1A, 0x1A,
            0x1B, 0x1B, 0x1C, 0x1C, 0x1D, 0x1D, 0x1E, 0x1E, 0x1F, 0x1F, 0x20, 0x20, 0x21, 0x21, 0x22, 0x22, 0x23, 0x23,
            0x24, 0x24, 0x25, 0x25, 0x26, 0x26, 0x27, 0x27, 0x28, 0x28, 0x29, 0x29, 0x2A, 0x2A, 0x2B, 0x2B, 0x2C, 0x2C,
            0x2D, 0x2D, 0x2E, 0x2E, 0x2F, 0x2F, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
            0x3C, 0x3D, 0x3E, 0x3F
        };

        readonly byte[] _dLen =
        {
            0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04,
            0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04,
            0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04,
            0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
            0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
            0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
            0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
            0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06,
            0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06,
            0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
            0x08, 0x08, 0x08, 0x08
        };
        readonly ushort[] _freq = new ushort[T + 1]; /* cumulative freq table */

        readonly Stream _inStream;

        /*
         * pointing parent nodes.
         * area [T..(T + N_CHAR - 1)] are pointers for leaves
         */
        readonly short[] _prnt = new short[T + N_CHAR];

        /* pointing children nodes (son[], son[] + 1)*/
        readonly short[] _son     = new short[T];
        readonly byte[]  _textBuf = new byte[N + F - 1];

        ushort _getbuf;
        byte   _getlen;

        Tdlzhuf _tdctl;

        /// <summary>Implements the TeleDisk LZH algorithm over the specified stream.</summary>
        /// <param name="dataStream">Stream with compressed data.</param>
        public TeleDiskLzh(Stream dataStream)
        {
            int i;
            _getbuf        = 0;
            _getlen        = 0;
            _tdctl         = new Tdlzhuf();
            _tdctl.Ibufcnt = _tdctl.Ibufndx = 0; // input buffer is empty
            _tdctl.Bufcnt  = 0;
            StartHuff();

            for(i = 0; i < N - F; i++)
                _textBuf[i] = 0x20;

            _tdctl.R  = N - F;
            _inStream = dataStream;
        }

        /* DeCompression

        split out initialization code to init_Decode()

        */

        /// <summary>Decompresses data</summary>
        /// <param name="buf">Buffer to write the decompressed data to</param>
        /// <param name="len">Number of bytes to decompress</param>
        /// <returns>Number of decompressed bytes</returns>
        public int Decode(out byte[] buf, int len) /* Decoding/Uncompressing */
        {
            short c;
            buf = new byte[len];
            int count; // was an unsigned long, seems unnecessary

            for(count = 0; count < len;)
                if(_tdctl.Bufcnt == 0)
                {
                    if((c = DecodeChar()) < 0)
                        return count; // fatal error

                    if(c < 256)
                    {
                        buf[count]           =  (byte)c;
                        _textBuf[_tdctl.R++] =  (byte)c;
                        _tdctl.R             &= N - 1;
                        count++;
                    }
                    else
                    {
                        short pos;

                        if((pos = DecodePosition()) < 0)
                            return count; // fatal error

                        _tdctl.Bufpos = (ushort)((_tdctl.R - pos - 1) & (N - 1));
                        _tdctl.Bufcnt = (ushort)(c - 255 + THRESHOLD);
                        _tdctl.Bufndx = 0;
                    }
                }
                else
                {
                    // still chars from last string
                    while(_tdctl.Bufndx < _tdctl.Bufcnt &&
                          count         < len)
                    {
                        c          = _textBuf[(_tdctl.Bufpos + _tdctl.Bufndx) & (N - 1)];
                        buf[count] = (byte)c;
                        _tdctl.Bufndx++;
                        _textBuf[_tdctl.R++] =  (byte)c;
                        _tdctl.R             &= N - 1;
                        count++;
                    }

                    // reset bufcnt after copy string from text_buf[]
                    if(_tdctl.Bufndx >= _tdctl.Bufcnt)
                        _tdctl.Bufndx = _tdctl.Bufcnt = 0;
                }

            return count; // count == len, success
        }

        long DataRead(out byte[] buf, long size)
        {
            if(size > _inStream.Length - _inStream.Position)
                size = _inStream.Length - _inStream.Position;

            buf = new byte[size];
            _inStream.Read(buf, 0, (int)size);

            return size;
        }

        int NextWord()
        {
            if(_tdctl.Ibufndx >= _tdctl.Ibufcnt)
            {
                _tdctl.Ibufndx = 0;
                _tdctl.Ibufcnt = (ushort)DataRead(out _tdctl.Inbuf, BUFSZ);

                if(_tdctl.Ibufcnt <= 0)
                    return -1;
            }

            while(_getlen <= 8)
            {
                // typically reads a word at a time
                _getbuf |= (ushort)(_tdctl.Inbuf[_tdctl.Ibufndx++] << (8 - _getlen));
                _getlen += 8;
            }

            return 0;
        }

        int GetBit() /* get one bit */
        {
            if(NextWord() < 0)
                return -1;

            short i = (short)_getbuf;
            _getbuf <<= 1;
            _getlen--;

            return i < 0 ? 1 : 0;
        }

        int GetByte() /* get a byte */
        {
            if(NextWord() != 0)
                return -1;

            ushort i = _getbuf;
            _getbuf <<= 8;
            _getlen -=  8;
            i       =   (ushort)(i >> 8);

            return i;
        }

        /* initialize freq tree */

        void StartHuff()
        {
            int i;

            for(i = 0; i < N_CHAR; i++)
            {
                _freq[i]     = 1;
                _son[i]      = (short)(i + T);
                _prnt[i + T] = (short)i;
            }

            i = 0;
            int j = N_CHAR;

            while(j <= ROOT)
            {
                _freq[j] =  (ushort)(_freq[i] + _freq[i + 1]);
                _son[j]  =  (short)i;
                _prnt[i] =  _prnt[i + 1] = (short)j;
                i        += 2;
                j++;
            }

            _freq[T]    = 0xffff;
            _prnt[ROOT] = 0;
        }

        /* reconstruct freq tree */

        void Reconst()
        {
            short i, k;

            /* halven cumulative freq for leaf nodes */
            short j = 0;

            for(i = 0; i < T; i++)
                if(_son[i] >= T)
                {
                    _freq[j] = (ushort)((_freq[i] + 1) / 2);
                    _son[j]  = _son[i];
                    j++;
                }

            /* make a tree : first, connect children nodes */
            for(i = 0, j = N_CHAR; j < T; i += 2, j++)
            {
                k = (short)(i + 1);
                ushort f = _freq[j] = (ushort)(_freq[i] + _freq[k]);

                for(k = (short)(j - 1); f < _freq[k]; k--) {}

                k++;
                ushort l = (ushort)((j - k) * 2);

                Array.ConstrainedCopy(_freq, k, _freq, k + 1, l);
                _freq[k] = f;
                Array.ConstrainedCopy(_son, k, _son, k + 1, l);
                _son[k] = i;
            }

            /* connect parent nodes */
            for(i = 0; i < T; i++)
                if((k = _son[i]) >= T)
                    _prnt[k] = i;
                else
                    _prnt[k] = _prnt[k + 1] = i;
        }

        /* update freq tree */

        void Update(int c)
        {
            if(_freq[ROOT] == MAX_FREQ)
                Reconst();

            c = _prnt[c + T];

            do
            {
                int k = ++_freq[c];

                /* swap nodes to keep the tree freq-ordered */
                int l;

                if(k <= _freq[l = c + 1])
                    continue;

                while(k > _freq[++l]) {}

                l--;
                _freq[c] = _freq[l];
                _freq[l] = (ushort)k;

                int i = _son[c];
                _prnt[i] = (short)l;

                if(i < T)
                    _prnt[i + 1] = (short)l;

                int j = _son[l];
                _son[l] = (short)i;

                _prnt[j] = (short)c;

                if(j < T)
                    _prnt[j + 1] = (short)c;

                _son[c] = (short)j;

                c = l;
            } while((c = _prnt[c]) != 0); /* do it until reaching the root */
        }

        short DecodeChar()
        {
            ushort c = (ushort)_son[ROOT];

            /*
             * start searching tree from the root to leaves.
             * choose node #(son[]) if input bit == 0
             * else choose #(son[]+1) (input bit == 1)
             */
            while(c < T)
            {
                int ret;

                if((ret = GetBit()) < 0)
                    return -1;

                c += (ushort)ret;
                c =  (ushort)_son[c];
            }

            c -= T;
            Update(c);

            return (short)c;
        }

        short DecodePosition()
        {
            short bit;

            /* decode upper 6 bits from given table */
            if((bit = (short)GetByte()) < 0)
                return -1;

            ushort i = (ushort)bit;
            ushort c = (ushort)(_dCode[i] << 6);
            ushort j = _dLen[i];

            /* input lower 6 bits directly */
            j -= 2;

            while(j-- > 0)
            {
                if((bit = (short)GetBit()) < 0)
                    return -1;

                i = (ushort)((i << 1) + bit);
            }

            return (short)(c | (i & 0x3f));
        }
        /* update when cumulative frequency */
        /* reaches to this value */

        struct Tdlzhuf
        {
            public ushort R, Bufcnt, Bufndx, Bufpos, // string buffer
                          // the following to allow block reads from input in next_word()
                          Ibufcnt, Ibufndx; // input buffer counters
            public byte[] Inbuf;            // input buffer
        }
    }
}