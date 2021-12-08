using System;
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Schemas;

namespace Aaru.DiscImages.ByteAddressable;

public class NES :IByteAddressableImage
{
    /// <inheritdoc />
    public string                 Author       { get; }
    /// <inheritdoc />
    public CICMMetadataType       CicmMetadata { get; }
    /// <inheritdoc />
    public List<DumpHardwareType> DumpHardware { get; }
    /// <inheritdoc />
    public string                 Format       { get; }
    /// <inheritdoc />
    public Guid                   Id           { get; }
    /// <inheritdoc />
    public ImageInfo              Info         { get; }
    /// <inheritdoc />
    public string                 Name         { get; }

    /// <inheritdoc />
    public bool Identify(IFilter imageFilter) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter) => throw new NotImplementedException();

    /// <inheritdoc />
    public string                                                                     ErrorMessage        { get; }
    /// <inheritdoc />
    public bool                                                                       IsWriting           { get; }
    /// <inheritdoc />
    public IEnumerable<string>                                                        KnownExtensions     { get; }
    /// <inheritdoc />
    public IEnumerable<MediaTagType>                                                  SupportedMediaTags  { get; }
    /// <inheritdoc />
    public IEnumerable<MediaType>                                                     SupportedMediaTypes { get; }
    /// <inheritdoc />
    public IEnumerable<(string name, Type type, string description, object @default)> SupportedOptions    { get; }
    /// <inheritdoc />
    public IEnumerable<SectorTagType>                                                 SupportedSectorTags { get; }

    /// <inheritdoc />
    public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors, uint sectorSize) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool Close() => throw new NotImplementedException();

    /// <inheritdoc />
    public bool SetCicmMetadata(CICMMetadataType metadata) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool SetDumpHardware(List<DumpHardwareType> dumpHardware) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool SetMetadata(ImageInfo metadata) => throw new NotImplementedException();

    /// <inheritdoc />
    public long Position { get; set; }

    /// <inheritdoc />
    public ErrorNumber Create(string path, MediaType mediaType, Dictionary<string, string> options, long maximumSize) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber GetHeader(out byte[] header) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber GetMappings(out LinearMemoryMap mappings) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadByte(out byte b, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadByteAt(long position, out byte b, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadBytes(byte[] buffer, int offset, int bytesToRead, out int bytesRead, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber ReadBytesAt(long position, byte[] buffer, int offset, int bytesToRead, out int bytesRead,
                                   bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber SetHeader(byte[] header) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber SetMappings(LinearMemoryMap mappings) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber WriteByte(byte b, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber WriteByteAt(long position, byte b, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber WriteBytes(byte[] buffer, int offset, int bytesToWrite, out int bytesWritten, bool advance = true) => throw new NotImplementedException();

    /// <inheritdoc />
    public ErrorNumber WriteBytesAt(long position, byte[] buffer, int offset, int bytesToWrite, out int bytesWritten,
                                    bool advance = true) => throw new NotImplementedException();
}