using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpCompress.Compressors.Xz;
// ReSharper disable UnusedMember.Global

namespace Aaru.Compression;

public static partial class XZ
{
    public enum CheckType : uint
    {
        None   = 0,
        Crc32  = 1,
        Crc64  = 4,
        Sha256 = 10
    }

    /// <summary>
    ///     AARU_EXPORT int32_t AARU_CALL AARU_xz_decode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t
    ///     *dst_buffer, size_t *dst_size)
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the compressed data</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the decompressed data to</param>
    /// <param name="dst_size">Size of the destination buffer, total bytes written on finish</param>
    /// <returns>Error code or 0 on success</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_xz_decode_buffer(byte[]    src_buffer, nuint src_size, byte[] dst_buffer,
                                                     ref nuint dst_size);


    /// <summary>
    ///     AARU_EXPORT int32_t AARU_CALL AARU_xz_encode_buffer(const uint8_t *src_buffer, size_t src_size, uint8_t
    ///     *dst_buffer, size_t *dst_size, uint32_t preset, uint32_t checkType)
    /// </summary>
    /// <param name="src_buffer">Buffer that contains the data to compress</param>
    /// <param name="src_size">Size of the source buffer</param>
    /// <param name="dst_buffer">Buffer to write the compressed data to</param>
    /// <param name="dst_size">Size of the destination buffer, total bytes written on finish</param>
    /// <param name="level">Compression level</param>
    /// <param name="checkType">Checksum to use</param>
    /// <returns>Error code or 0 on success</returns>
    [LibraryImport("libAaru.Compression.Native", SetLastError = true)]
    private static partial int AARU_xz_encode_buffer(byte[]    src_buffer, nuint src_size, byte[] dst_buffer,
                                                     ref nuint dst_size,   uint  level,    CheckType checkType);

    /// <summary>Decodes a buffer compressed with XZ</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    public static int DecodeBuffer(byte[] source, byte[] destination)
    {
        if(Native.IsSupported)
        {
            var dstSize = (nuint)destination.Length;
            int res     = AARU_xz_decode_buffer(source, (nuint)source.Length, destination, ref dstSize);

            if(res == 0) return (int)dstSize;
        }

        // Managed fallback using SharpCompress
        return DecodeBufferManaged(source, destination);
    }

    /// <summary>Decodes a buffer compressed with XZ using managed code</summary>
    /// <param name="source">Encoded buffer</param>
    /// <param name="destination">Buffer where to write the decoded data</param>
    /// <returns>The number of decoded bytes, or -1 on error</returns>
    static int DecodeBufferManaged(byte[] source, byte[] destination)
    {
        try
        {
            using var inputStream  = new MemoryStream(source);
            using var xzStream     = new XZStream(inputStream);
            using var outputStream = new MemoryStream();

            xzStream.CopyTo(outputStream);

            byte[] result = outputStream.ToArray();

            if(result.Length > destination.Length) return -1;

            Array.Copy(result, destination, result.Length);

            return result.Length;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>Compresses a buffer using XZ</summary>
    /// <param name="source">Data to compress</param>
    /// <param name="destination">Buffer to store the compressed data</param>
    /// <param name="level">Compression level (0-9)</param>
    /// <param name="checkType">Checksum type to use for integrity verification</param>
    /// <returns>The size of the compressed data, or -1 on error</returns>
    public static int EncodeBuffer(byte[] source, byte[] destination, uint level, CheckType checkType)
    {
        if(!Native.IsSupported) return -1;

        var dstSize = (nuint)destination.Length;
        int res     = AARU_xz_encode_buffer(source, (nuint)source.Length, destination, ref dstSize, level, checkType);

        if(res != 0) return -1;

        return (int)dstSize;
    }
}