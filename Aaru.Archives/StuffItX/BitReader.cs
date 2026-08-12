using System.IO;

namespace Aaru.Archives;

public sealed partial class StuffItX
{
    /// <summary>Bit-level reader for StuffIt X P2 variable-length encoding.</summary>
    sealed class BitReader
    {
        internal readonly Stream BaseStream;
        int                      _bitsLeft;
        int                      _buffer;

        public BitReader(Stream stream)
        {
            BaseStream = stream;
            _bitsLeft  = 0;
            _buffer    = 0;
        }

        /// <summary>Reads a single bit (LSB-first within each byte).</summary>
        int ReadBitLE()
        {
            if(_bitsLeft == 0)
            {
                int b = BaseStream.ReadByte();

                if(b < 0) return -1;

                _buffer   = b;
                _bitsLeft = 8;
            }

            int bit = _buffer & 1;
            _buffer >>= 1;
            _bitsLeft--;

            return bit;
        }

        /// <summary>Reads multiple bits (LSB-first accumulation).</summary>
        public int ReadBitsLE(int count)
        {
            var value = 0;

            for(var i = 0; i < count; i++)
            {
                int bit = ReadBitLE();

                if(bit < 0) return -1;

                value |= bit << i;
            }

            return value;
        }

        /// <summary>Discards any remaining bits in the current byte.</summary>
        public void FlushBits()
        {
            _bitsLeft = 0;
            _buffer   = 0;
        }

        /// <summary>
        ///     Reads a P2-encoded variable-length unsigned integer.
        ///     Format: count leading 1-bits (terminated by 0-bit), then count data bits (each 1-bit decrements counter,
        ///     0-bits are positional). Returns value - 1.
        /// </summary>
        public ulong ReadSitxP2()
        {
            var n = 1;

            while(ReadBitLE() == 1 && n < 64) n++;

            ulong value = 0;
            ulong bit   = 1;

            while(n > 0)
            {
                int b = ReadBitLE();

                // At end of stream return 0, the terminator value, so callers stop instead of looping forever
                if(b < 0) return 0;

                if(b == 1)
                {
                    n--;
                    value |= bit;
                }

                bit <<= 1;
            }

            return value - 1;
        }

        /// <summary>Reads a 32-bit big-endian integer via the bit buffer (8 bits at a time).</summary>
        public uint ReadSitxUInt32()
        {
            uint val = 0;

            for(var i = 0; i < 4; i++) val = val << 8 | (uint)ReadBitsLE(8);

            return val;
        }

        /// <summary>Reads a 64-bit big-endian integer via the bit buffer (8 bits at a time).</summary>
        public ulong ReadSitxUInt64()
        {
            ulong val = 0;

            for(var i = 0; i < 8; i++) val = val << 8 | (uint)ReadBitsLE(8);

            return val;
        }

        /// <summary>Reads a P2-length-prefixed string, then flushes remaining bits.</summary>
        public byte[] ReadSitxString()
        {
            var len = (int)ReadSitxP2();

            if(len <= 0) return [];

            var data = new byte[len];
            BaseStream.ReadExactly(data, 0, len);
            FlushBits();

            return data;
        }

        /// <summary>Reads exactly <paramref name="n" /> bytes via the bit buffer (8 bits at a time).</summary>
        public byte[] ReadSitxData(int n)
        {
            var data = new byte[n];

            for(var i = 0; i < n; i++) data[i] = (byte)ReadBitsLE(8);

            return data;
        }
    }
}