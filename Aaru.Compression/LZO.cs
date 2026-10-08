using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Aaru.Compression;

public sealed partial class LZO
{
    /// <summary>
    ///     LZO algorithm types. The LZO1X algorithm is the most commonly used
    /// </summary>
    public enum Algorithm
    {
        /// <summary>LZO1 algorithm</summary>
        LZO1 = 0,
        /// <summary>LZO1A algorithm</summary>
        LZO1A = 1,
        /// <summary>LZO1B algorithm (supports compression levels 1-9, 99, 999)</summary>
        LZO1B = 2,
        /// <summary>LZO1C algorithm (supports compression levels 1-9, 99, 999)</summary>
        LZO1C = 3,
        /// <summary>LZO1F algorithm (supports compression level 999)</summary>
        LZO1F = 4,
        /// <summary>LZO1X algorithm (supports compression levels 11, 12, 15, 999) - most common</summary>
        LZO1X = 5,
        /// <summary>LZO1Y algorithm (supports compression level 999)</summary>
        LZO1Y = 6,
        /// <summary>LZO1Z algorithm (only 999 compression level)</summary>
        LZO1Z = 7,
        /// <summary>LZO2A algorithm (only 999 compression level)</summary>
        LZO2A = 8
    }

    /// <summary>
    ///     AARU_EXPORT int32_t AARU_CALL AARU_lzo_decode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t
    ///     *dst_buffer, size_t *dst_size, int32_t algorithm);
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the compressed data</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the decompressed data to</param>
    /// <param name="dst_size">Size of the destination buffer, total bytes written on finish</param>
    /// <param name="algorithm">Which algorithm to use</param>
    /// <returns>Error code or 0 on success</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzo_decode_buffer(byte[]    src_buffer, nuint     src_size, byte[] dst_buffer,
                                                      ref nuint dst_size,   Algorithm algorithm);


    /// <summary>
    ///     AARU_EXPORT int32_t AARU_CALL AARU_lzo_encode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t
    ///     *dst_buffer, size_t *dst_size, int32_t algorithm, int32_t compression_level);
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the data to compress</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the compressed data to</param>
    /// <param name="dst_size">Size of the destination buffer, total bytes written on finish</param>
    /// <param name="compression_level">Compression level</param>
    /// <param name="algorithm">Which algorithm to use</param>
    /// <returns>Error code or 0 on success</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_lzo_encode_buffer(byte[]    src_buffer, nuint     src_size, byte[] dst_buffer,
                                                      ref nuint dst_size,   Algorithm algorithm, int compression_level);

    /// <summary>Decodes a buffer compressed with LZO</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <param name="algorithm">LZO algorithm variant to use for decompression</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination, Algorithm algorithm)
    {
        if(!Native.IsSupported) return -1;

        var dstSize = (nuint)destination.Length;
        int res     = AARU_lzo_decode_buffer(source, (nuint)source.Length, destination, ref dstSize, algorithm);

        if(res != 0) return -1;

        return (int)dstSize;
    }

    /// <summary>Compresses a buffer using LZO</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <param name="algorithm">LZO algorithm variant to use for compression</param>
    /// <param name="compressionLevel">Compression level (valid values depend on the algorithm)</param>
    /// <returns>The size of the compressed data, or -1 on error</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, Algorithm algorithm, int compressionLevel)
    {
        if(!Native.IsSupported) return -1;

        var dstSize = (nuint)destination.Length;

        int res = AARU_lzo_encode_buffer(source,
                                         (nuint)source.Length,
                                         destination,
                                         ref dstSize,
                                         algorithm,
                                         compressionLevel);

        if(res != 0) return -1;

        return (int)dstSize;
    }
}