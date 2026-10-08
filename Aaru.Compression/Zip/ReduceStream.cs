using System;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable LocalizableElement

namespace Aaru.Compression.Zip;

public partial class ReduceStream : Stream
{
    readonly byte[] _decoded;
    readonly long   _length;
    long            _position;

    public ReduceStream(Stream compressedStream, long decompressedLength, int compressionFactor)
    {
        if(compressedStream == null) throw new ArgumentNullException(nameof(compressedStream));
        if(!compressedStream.CanRead) throw new ArgumentException("Stream must be readable", nameof(compressedStream));
        if(decompressedLength < 0) throw new ArgumentOutOfRangeException(nameof(decompressedLength));

        // Read full compressed data into memory
        compressedStream.Position = 0;
        var inBuf = new byte[compressedStream.Length];
        compressedStream.ReadExactly(inBuf, 0, inBuf.Length);

        // Allocate output buffer
        _decoded = new byte[decompressedLength];
        var outLen = (nuint)decompressedLength;

        // Call native decompressor
        int err = AARU_zip_reduce_decode_buffer(inBuf, (nuint)inBuf.Length, _decoded, ref outLen, compressionFactor);

        if(err != 0) throw new InvalidOperationException("Reduce decompression failed");

        // Adjust actual length in case it differs
        _length   = (long)outLen;
        _position = 0;
    }

    public override bool CanRead  => true;
    public override bool CanSeek  => true;
    public override bool CanWrite => false;
    public override long Length   => _length;

    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    [LibraryImport("libAaru.Compression.Native")]
    public static partial int AARU_zip_reduce_decode_buffer(byte[] src_buffer, nuint src_size, byte[] dst_buffer,
                                                            ref nuint dst_size, int comp_factor);

    public override void Flush()
    {
        // no-op
    }

    /// <summary>
    ///     Reads up to <paramref name="count" /> bytes from the decompressed buffer
    ///     into <paramref name="buffer" />, starting at <paramref name="offset" />.
    /// </summary>
    public override int Read(byte[] buffer, int offset, int count)
    {
        if(buffer         == null) throw new ArgumentNullException(nameof(buffer));
        if(offset         < 0) throw new ArgumentOutOfRangeException(nameof(offset));
        if(count          < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if(offset + count > buffer.Length) throw new ArgumentException("offset+count exceeds buffer length");

        long remaining = _length - _position;

        if(remaining <= 0) return 0;

        var toRead = (int)Math.Min(count, remaining);
        Array.Copy(_decoded, _position, buffer, offset, toRead);
        _position += toRead;

        return toRead;
    }

    /// <summary>
    ///     Sets the current position within the decompressed buffer.
    /// </summary>
    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPos = origin switch
                      {
                          SeekOrigin.Begin   => offset,
                          SeekOrigin.Current => _position + offset,
                          SeekOrigin.End     => _length   + offset,
                          _                  => throw new ArgumentException("Invalid SeekOrigin", nameof(origin))
                      };

        if(newPos < 0 || newPos > _length) throw new IOException("Attempt to seek outside the buffer");

        _position = newPos;

        return _position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("Cannot resize decompressed buffer");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Stream is read-only");
    }
}