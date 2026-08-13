using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class StuffItX
{
    /// <summary>Windows FILETIME epoch: January 1, 1601.</summary>
    static readonly DateTime _fileTimeEpoch = new(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Reads an element header: flags, type, attributes, and algorithm lists.</summary>
    static Element ReadElement(BitReader reader)
    {
        var element = new Element
        {
            Attribs = new long[10],
            AlgList = new long[6]
        };

        for(var i = 0; i < 10; i++) element.Attribs[i] = -1;
        for(var i = 0; i < 6; i++) element.AlgList[i]  = -1;

        element.AlgList3Extra = -1;

        element.Something = reader.ReadBitsLE(1);

        element.Type = (int)reader.ReadSitxP2();

        // Read attributes
        for(;;)
        {
            var attrType = (int)reader.ReadSitxP2();

            if(attrType == 0) break;

            ulong value = reader.ReadSitxP2();

            if(attrType is >= 1 and <= 10) element.Attribs[attrType - 1] = (long)value;
        }

        // Read algorithm list
        for(;;)
        {
            var algType = (int)reader.ReadSitxP2();

            if(algType == 0) break;

            ulong value = reader.ReadSitxP2();

            if(algType is >= 1 and <= 6) element.AlgList[algType - 1] = (long)value;

            if(algType == 4) element.AlgList3Extra = (long)reader.ReadSitxP2();
        }

        element.DataOffset = reader.BaseStream.Position;
        element.ActualSize = 0;

        return element;
    }

    /// <summary>
    ///     Scans over element data blocks (P2-encoded size + raw bytes), extracting the optional CRC.
    ///     After this, the stream position is just past all data for this element.
    /// </summary>
    void ScanElementData(BitReader reader, ref Element element)
    {
        _stream.Position = element.DataOffset;
        reader.FlushBits();

        // Skip first block sequence
        for(;;)
        {
            ulong len = reader.ReadSitxP2();

            // Also stop on lengths that wrap negative or point past end of file
            if(len == 0 || (long)len < 0 || (long)len > _stream.Length - _stream.Position) break;

            reader.FlushBits();
            _stream.Position += (long)len;
        }

        reader.FlushBits();
        ulong crcLen = reader.ReadSitxP2();


        switch(crcLen)
        {
            case 0:
                return;

            case 4:
                // CRC is read as raw bytes, not through the bit buffer
                reader.FlushBits();
                var crcBuf = new byte[4];
                _stream.ReadExactly(crcBuf, 0, 4);
                element.DataCrc = (uint)crcBuf[0] << 24 | (uint)crcBuf[1] << 16 | (uint)crcBuf[2] << 8 | crcBuf[3];
                crcLen          = reader.ReadSitxP2();

                break;
        }

        // Skip remaining data blocks
        while(crcLen != 0)
        {
            // Stop on lengths that wrap negative or point past end of file
            if((long)crcLen < 0 || (long)crcLen > _stream.Length - _stream.Position) break;

            reader.FlushBits();
            _stream.Position += (long)crcLen;
            crcLen           =  reader.ReadSitxP2();
        }
    }

    /// <summary>Parses catalog metadata from a decompressed catalog stream and populates entry attributes.</summary>
    void ParseCatalog(Stream catalogStream, List<Dictionary<string, object>> entries,
                      Dictionary<long, Dictionary<string, object>> entryDict)
    {
        var catReader = new BitReader(catalogStream);

        foreach(Dictionary<string, object> entry in entries)
        {
            for(;;)
            {
                if(catalogStream.Position >= catalogStream.Length) break;

                int key;

                try
                {
                    key = (int)catReader.ReadSitxP2();
                }
                catch(Exception ex)
                {
                    AaruLogging.Debug(MODULE_NAME, "Exception reading catalog key: {0}", ex);

                    break;
                }


                if(key == 0) break;

                try
                {
                    switch(key)
                    {
                        case 1: // Filename
                        {
                            byte[] filenameBytes = catReader.ReadSitxString();
                            string filename      = _encoding.GetString(filenameBytes);

                            // Build hierarchical path from parent
                            string parentPath = null;

                            if(entry.TryGetValue("StuffItXParent", out object parentIdObj))
                            {
                                var parentId = (long)parentIdObj;

                                if(entryDict.TryGetValue(parentId, out Dictionary<string, object> parentEntry) &&
                                   parentEntry.TryGetValue("FullPath", out object parentPathObj))
                                    parentPath = (string)parentPathObj;
                            }

                            string fullPath = parentPath != null ? parentPath + "/" + filename : filename;
                            entry["Filename"]      = filename;
                            entry["FullPath"]      = fullPath;
                            entry["DirectoryPath"] = parentPath;
                        }

                            break;

                        case 2: // Modification time (100-nanosecond intervals since 1601-01-01)
                        {
                            ulong    ticks = catReader.ReadSitxUInt64();
                            DateTime time  = _fileTimeEpoch.AddTicks((long)ticks);
                            entry["ModificationTime"] = time;
                        }

                            break;

                        case 3: // Unknown uint32
                            catReader.ReadSitxUInt32();

                            break;

                        case 4: // FinderInfo (32 bytes)
                        case 5:
                        {
                            byte[] data = catReader.ReadSitxData(32);

                            // Check for symlink marker
                            if(data.Length >= 8         &&
                               data[0]     == (byte)'s' &&
                               data[1]     == (byte)'l' &&
                               data[2]     == (byte)'n' &&
                               data[3]     == (byte)'k' &&
                               data[4]     == (byte)'r' &&
                               data[5]     == (byte)'h' &&
                               data[6]     == (byte)'a' &&
                               data[7]     == (byte)'p')
                                entry["IsLink"] = true;
                            else
                                entry["FinderInfo"] = data;
                        }

                            break;

                        case 6: // POSIX permissions
                        {
                            int  hasOwner    = catReader.ReadSitxData(1)[0];
                            uint permissions = catReader.ReadSitxUInt32();
                            entry["PosixPermissions"] = permissions;

                            if(hasOwner != 0)
                            {
                                entry["PosixUser"]  = catReader.ReadSitxUInt32();
                                entry["PosixGroup"] = catReader.ReadSitxUInt32();
                            }
                        }

                            break;

                        case 7: // Unknown P2 value
                            catReader.ReadSitxP2();

                            break;

                        case 8: // Creation time (100-nanosecond intervals since 1601-01-01)
                        {
                            ulong    ticks = catReader.ReadSitxUInt64();
                            DateTime time  = _fileTimeEpoch.AddTicks((long)ticks);
                            entry["CreationTime"] = time;
                        }

                            break;

                        case 9: // Comment
                        {
                            byte[] commentBytes = catReader.ReadSitxString();

                            if(commentBytes is { Length: > 0 }) entry["Comment"] = _encoding.GetString(commentBytes);
                        }

                            break;

                        case 10: // Unknown array of strings
                        {
                            var num = (int)catReader.ReadSitxP2();


                            for(var i = 0; i < num; i++) catReader.ReadSitxString();
                        }

                            break;

                        case 11: // Unknown string
                        case 12:
                            catReader.ReadSitxString();

                            break;
                    }
                }
                catch(Exception ex)
                {
                    AaruLogging.Debug(MODULE_NAME, "Exception reading catalog value: {0}", ex);

                    break;
                }
            }

            catReader.FlushBits();
        }
    }

    /// <summary>Decompresses an element's data blocks through its compression pipeline and returns the result.</summary>
    ErrorNumber DecompressElement(Element element, out Stream result)
    {
        result = null;

        var  compAlg   = (CompressionAlgorithm)element.AlgList[0];
        long cryptoAlg = element.AlgList[3];

        if(cryptoAlg >= 0) return ErrorNumber.NotSupported;

        _stream.Position = element.DataOffset;

        var reader = new BitReader(_stream);

        MemoryStream blockData = ReassembleBlocks(reader);

        // If no compression, return raw block data
        if(compAlg == CompressionAlgorithm.None)
        {
            result = blockData;

            return ErrorNumber.NoError;
        }

        long decompressedSize = element.ActualSize;

        var preprocessAlg = (PreprocessAlgorithm)element.AlgList[2];

        // When English preprocessing is active, the decompressor output is the English-encoded
        // intermediate data, which can be larger than actualSize (especially for binary content
        // where escape bytes expand the data). Over-allocate to avoid truncation.
        if(preprocessAlg == PreprocessAlgorithm.English) decompressedSize = element.ActualSize * 2;

        Stream decompressed;

        try
        {
            decompressed = DecompressStream(blockData, compAlg, decompressedSize);
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Exception decompressing element: {0}", ex);

            return ErrorNumber.InOutError;
        }


        if(decompressed == null) return ErrorNumber.NotSupported;

        // Apply preprocessing filter
        Stream preprocessed = ApplyPreprocessing(decompressed, preprocessAlg, element.ActualSize);

        if(preprocessed == null) return ErrorNumber.NotSupported;

        result = preprocessed;

        return ErrorNumber.NoError;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _encoding        = encoding ?? Encoding.GetEncoding("macintosh");
        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;

        // Skip first 7 bytes of "StuffIt" signature
        _stream.Position = 7;

        int encodingMarker = _stream.ReadByte();

        if(encodingMarker == '?') return ErrorNumber.NotSupported;

        var reader = new BitReader(_stream);

        // Entries and directory structures indexed by their object ID
        var allEntries  = new List<Dictionary<string, object>>();
        var entryDict   = new Dictionary<long, Dictionary<string, object>>();
        var streamForks = new Dictionary<long, List<ForkInfo>>();
        var forkedIds   = new HashSet<long>();

        while(_stream.Position < _stream.Length)
        {
            Element element = ReadElement(reader);


            switch((ElementType)element.Type)
            {
                case ElementType.End:
                    goto doneParsingElements;

                case ElementType.File:
                {
                    long objId  = element.Attribs[0];
                    long parent = element.Attribs[1];

                    var file = new Dictionary<string, object>
                    {
                        ["StuffItXID"]     = objId,
                        ["StuffItXParent"] = parent
                    };

                    allEntries.Add(file);
                    entryDict[objId] = file;
                }

                    break;

                case ElementType.Directory:
                {
                    long objId  = element.Attribs[0];
                    long parent = element.Attribs[1];

                    var dir = new Dictionary<string, object>
                    {
                        ["StuffItXID"]     = objId,
                        ["StuffItXParent"] = parent,
                        ["IsDirectory"]    = true
                    };

                    allEntries.Add(dir);
                    entryDict[objId] = dir;
                }

                    break;

                case ElementType.Fork:
                {
                    long entryId    = element.Attribs[1];
                    long streamId   = element.Attribs[2];
                    long forkIndex  = element.Attribs[3];
                    long forkLength = element.Attribs[4];

                    var forkType = (ForkType)reader.ReadSitxP2();

                    // Reject nonsensical fork indexes, a huge one would allocate that many padding slots
                    if(forkIndex is < 0 or > MAX_FORK_INDEX) return ErrorNumber.InvalidArgument;

                    forkedIds.Add(entryId);

                    if(!streamForks.TryGetValue(streamId, out List<ForkInfo> forks))
                    {
                        forks                 = [];
                        streamForks[streamId] = forks;
                    }

                    var forkInfo = new ForkInfo
                    {
                        EntryIds = [entryId],
                        Type     = forkType,
                        Length   = forkLength
                    };

                    int count = forks.Count;

                    if(forkIndex == count)
                        forks.Add(forkInfo);
                    else if(forkIndex > count)
                    {
                        // Pad with default entries for any gaps
                        for(int i = count; i < (int)forkIndex; i++)
                            forks.Add(new ForkInfo
                            {
                                EntryIds = null,
                                Type     = 0,
                                Length   = -1
                            });

                        forks.Add(forkInfo);
                    }
                    else
                    {
                        // Multiple files can reference the same fork slot
                        ForkInfo existing = forks[(int)forkIndex];

                        if(existing.EntryIds == null)
                            forks[(int)forkIndex] = forkInfo;
                        else
                        {
                            if(existing.Length != forkLength) return ErrorNumber.InvalidArgument;

                            existing.EntryIds.Add(entryId);
                            forks[(int)forkIndex] = existing;
                        }
                    }
                }

                    break;

                case ElementType.Catalog:
                {
                    ScanElementData(reader, ref element);
                    element.ActualSize = element.Attribs[4];
                    long pos = _stream.Position;

                    ErrorNumber catErr = DecompressElement(element, out Stream catalogStream);

                    if(catErr != ErrorNumber.NoError) return catErr;

                    if(catalogStream == null) return ErrorNumber.NotSupported;

                    ParseCatalog(catalogStream, allEntries, entryDict);

                    _stream.Position = pos;
                }

                    break;

                case ElementType.Data:
                {
                    long objId      = element.Attribs[0];
                    long uncompSize = element.Attribs[4];


                    ScanElementData(reader, ref element);

                    long pos = _stream.Position;

                    // Compute actual size from all forks referencing this stream
                    if(streamForks.TryGetValue(objId, out List<ForkInfo> forks))
                    {
                        element.ActualSize = 0;

                        foreach(ForkInfo fork in forks)
                        {
                            if(fork.EntryIds == null) return ErrorNumber.InvalidArgument;

                            element.ActualSize += fork.Length;
                        }
                    }

                    long compSize = pos - element.DataOffset;

                    // Emit entries that have no data streams (not referenced by any fork)
                    foreach(Dictionary<string, object> entry in allEntries)
                    {
                        if(!entry.TryGetValue("StuffItXID", out object idObj)) continue;

                        var id = (long)idObj;

                        if(forkedIds.Contains(id)) continue;

                        if(entry.ContainsKey("EmptyEmitted")) continue;

                        entry["EmptyEmitted"] = true;
                        entry["EmptyStream"]  = true;
                    }

                    // Emit entries for each fork in this data stream
                    if(forks != null)
                    {
                        long offset = 0;

                        foreach(ForkInfo fork in forks)
                        {
                            if(fork.EntryIds == null) return ErrorNumber.InvalidArgument;

                            // Only process data (type 0) and resource (type 1) forks
                            if(fork.Type != ForkType.Data && fork.Type != ForkType.Resource)
                            {
                                offset += fork.Length;

                                continue;
                            }

                            foreach(long entryId in fork.EntryIds)
                            {
                                if(!entryDict.TryGetValue(entryId, out Dictionary<string, object> entry)) continue;

                                long currCompSize = 0;

                                if(uncompSize > 0) currCompSize = fork.Length * compSize / uncompSize;

                                entry["SolidElement"]    = element;
                                entry["SolidOffset"]     = offset;
                                entry["FileSize"]        = fork.Length;
                                entry["CompressedSize"]  = currCompSize;
                                entry["IsResourceFork"]  = fork.Type == ForkType.Resource;
                                entry["HasSolidElement"] = true;
                            }

                            offset += fork.Length;
                        }
                    }

                    _stream.Position = pos;
                }

                    break;

                case ElementType.Root:
                    reader.ReadSitxP2(); // skip root value

                    break;

                case ElementType.Clue:
                {
                    long size = element.Attribs[4];

                    if(size > 0) _stream.Position += size;
                }

                    break;

                case ElementType.Boundary:
                    break;

                default:
                    if(element.Type > 10)
                        ScanElementData(reader, ref element);
                    else
                        return ErrorNumber.NotSupported;

                    break;
            }

            reader.FlushBits();
        }

    doneParsingElements:

        // Build final entry list
        _entries = [];

        foreach(Dictionary<string, object> rawEntry in allEntries)
        {
            var entry = new Entry();

            if(rawEntry.TryGetValue("Filename", out object filenameObj)) entry.Filename = (string)filenameObj;

            if(rawEntry.TryGetValue("DirectoryPath", out object dirPathObj)) entry.DirectoryPath = (string)dirPathObj;

            if(rawEntry.TryGetValue("IsDirectory", out object isDirObj)) entry.IsDirectory = (bool)isDirObj;

            if(rawEntry.TryGetValue("IsLink", out object isLinkObj)) entry.IsLink = (bool)isLinkObj;

            if(rawEntry.TryGetValue("EmptyStream", out object emptyObj)) entry.IsEmptyStream = (bool)emptyObj;

            if(rawEntry.TryGetValue("FinderInfo", out object finderObj)) entry.FinderInfo = (byte[])finderObj;

            if(rawEntry.TryGetValue("PosixPermissions", out object permObj)) entry.PosixPermissions = (uint)permObj;

            if(rawEntry.TryGetValue("PosixUser", out object uidObj))
            {
                entry.PosixUser     = (uint)uidObj;
                entry.HasPosixOwner = true;
            }

            if(rawEntry.TryGetValue("PosixGroup", out object gidObj)) entry.PosixGroup = (uint)gidObj;

            if(rawEntry.TryGetValue("CreationTime", out object cTimeObj)) entry.CreationTime = (DateTime)cTimeObj;

            if(rawEntry.TryGetValue("ModificationTime", out object mTimeObj))
                entry.ModificationTime = (DateTime)mTimeObj;

            if(rawEntry.TryGetValue("Comment", out object commentObj)) entry.Comment = (string)commentObj;

            if(rawEntry.TryGetValue("SolidElement", out object solidObj)) entry.SolidElement = (Element)solidObj;

            if(rawEntry.TryGetValue("HasSolidElement", out object hasSolidObj))
                entry.HasSolidElement = (bool)hasSolidObj;

            if(rawEntry.TryGetValue("SolidOffset", out object solidOffObj)) entry.SolidOffset = (long)solidOffObj;

            if(rawEntry.TryGetValue("FileSize", out object fileSizeObj)) entry.UncompressedSize = (long)fileSizeObj;

            if(rawEntry.TryGetValue("CompressedSize", out object compSizeObj)) entry.CompressedSize = (long)compSizeObj;

            if(rawEntry.TryGetValue("IsResourceFork", out object isResObj)) entry.IsResourceFork = (bool)isResObj;

            if(rawEntry.TryGetValue("Encrypted", out object encObj)) entry.Encrypted = (bool)encObj;

            _entries.Add(entry);
        }

        // Compute features from actual entries
        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        var hasCompression    = false;
        var hasSubdirectories = false;
        var hasDirectories    = false;
        var hasXAttrs         = false;
        var hasProtection     = false;

        for(int i = 0, count = _entries.Count; i < count; i++)
        {
            Entry entry = _entries[i];

            if(entry.IsDirectory) hasDirectories = true;

            if(entry.HasSolidElement)
            {
                var alg = (CompressionAlgorithm)entry.SolidElement.AlgList[0];

                if(alg != CompressionAlgorithm.None) hasCompression = true;
            }

            if(entry.DirectoryPath is not null) hasSubdirectories = true;

            if(entry.Encrypted) hasProtection = true;

            if(!entry.IsDirectory && (entry.FinderInfo != null || entry.Comment != null)) hasXAttrs = true;

            if(entry.IsResourceFork) hasXAttrs = true;
        }

        if(hasCompression) _features    |= ArchiveSupportedFeature.SupportsCompression;
        if(hasSubdirectories) _features |= ArchiveSupportedFeature.SupportsSubdirectories;
        if(hasDirectories) _features    |= ArchiveSupportedFeature.HasExplicitDirectories;
        if(hasXAttrs) _features         |= ArchiveSupportedFeature.SupportsXAttrs;
        if(hasProtection) _features     |= ArchiveSupportedFeature.SupportsProtection;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();

        _stream  = null;
        _entries = null;
        Opened   = false;
    }

#endregion
}