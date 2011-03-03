using System;

namespace FileSystemIDandChk
{
    static class Swapping
    {
        public static byte[] SwapTenBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[9];
            destination[1] = source[8];
            destination[2] = source[7];
            destination[3] = source[6];
            destination[4] = source[5];
            destination[5] = source[4];
            destination[6] = source[3];
            destination[7] = source[2];
            destination[8] = source[1];
            destination[9] = source[0];

            return destination;
        }

        public static byte[] SwapEightBytes(byte[] source)
        {
            byte[] destination = new byte[8];

            destination[0] = source[7];
            destination[1] = source[6];
            destination[2] = source[5];
            destination[3] = source[4];
            destination[4] = source[3];
            destination[5] = source[2];
            destination[6] = source[1];
            destination[7] = source[0];

            return destination;
        }

        public static byte[] SwapFourBytes(byte[] source)
        {
            byte[] destination = new byte[4];

            destination[0] = source[3];
            destination[1] = source[2];
            destination[2] = source[1];
            destination[3] = source[0];

            return destination;
        }

        public static byte[] SwapTwoBytes(byte[] source)
        {
            byte[] destination = new byte[2];

            destination[0] = source[1];
            destination[1] = source[0];

            return destination;
        }
    }
}
