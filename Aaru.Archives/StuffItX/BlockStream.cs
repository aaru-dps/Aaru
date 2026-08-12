using System.IO;

namespace Aaru.Archives;

public sealed partial class StuffItX
{
    /// <summary>
    ///     Reassembles P2-encoded blocks from a StuffIt X data element into a contiguous byte stream.
    ///     Block format: repeated { P2-encoded size, size raw bytes }. Size == 0 terminates the sequence.
    ///     After each P2 size read, remaining bits are flushed before reading raw block data.
    /// </summary>
    static MemoryStream ReassembleBlocks(BitReader reader)
    {
        var ms = new MemoryStream();

        for(;;)
        {
            ulong blockSize = reader.ReadSitxP2();

            if(blockSize == 0) break;

            // Reject block sizes that wrap negative or exceed the remaining stream
            if((long)blockSize < 0 || (long)blockSize > reader.BaseStream.Length - reader.BaseStream.Position) break;

            reader.FlushBits();

            var block = new byte[blockSize];
            reader.BaseStream.ReadExactly(block, 0, (int)blockSize);

            ms.Write(block, 0, (int)blockSize);
        }

        ms.Position = 0;

        return ms;
    }
}