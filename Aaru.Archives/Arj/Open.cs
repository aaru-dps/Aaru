using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Compression;
using Aaru.Logging;

namespace Aaru.Archives;

public sealed partial class Arj
{
    List<Entry> _entries;

    /// <summary>
    ///     Reads an ARJ header block (signature + size + data + CRC + extended headers). Returns false if end of archive
    ///     or invalid header.
    /// </summary>
    bool ReadHeader(out byte[] headerData, out List<ExtHeaderBlock> extHeaders)
    {
        headerData = null;
        extHeaders = null;

        if(_stream.Position + 4 > _stream.Length) return false;

        // Read 2-byte signature
        var sigBytes = new byte[2];
        _stream.ReadExactly(sigBytes, 0, 2);
        var sig = BitConverter.ToUInt16(sigBytes, 0);

        if(sig != HEADER_ID) return false;

        // Read 2-byte basic header size
        var sizeBytes = new byte[2];
        _stream.ReadExactly(sizeBytes, 0, 2);
        var basicHdrSize = BitConverter.ToUInt16(sizeBytes, 0);

        // Size of 0 means end of archive
        if(basicHdrSize == 0) return false;

        if(basicHdrSize > HEADER_SIZE_MAX) return false;

        // Read header data
        if(_stream.Position + basicHdrSize + 4 > _stream.Length) return false;

        headerData = new byte[basicHdrSize];
        _stream.ReadExactly(headerData, 0, basicHdrSize);

        // Read and verify CRC32
        var crcBytes = new byte[4];
        _stream.ReadExactly(crcBytes, 0, 4);
        var  storedCrc   = BitConverter.ToUInt32(crcBytes, 0);
        uint computedCrc = ComputeCrc(CRC_MASK, headerData) ^ CRC_MASK;

        if(storedCrc != computedCrc)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "[red]Header CRC mismatch: stored=0x{0:X8}, computed=0x{1:X8}[/]",
                              storedCrc,
                              computedCrc);

            return false;
        }

        // Read extended headers
        extHeaders = [];

        while(_stream.Position + 2 <= _stream.Length)
        {
            var extSizeBytes = new byte[2];
            _stream.ReadExactly(extSizeBytes, 0, 2);
            var extSize = BitConverter.ToUInt16(extSizeBytes, 0);

            if(extSize == 0) break;

            if(_stream.Position + extSize + 4 > _stream.Length) break;

            var extData = new byte[extSize];
            _stream.ReadExactly(extData, 0, extSize);

            // Read extended header CRC
            var extCrcBytes = new byte[4];
            _stream.ReadExactly(extCrcBytes, 0, 4);
            var  extStoredCrc   = BitConverter.ToUInt32(extCrcBytes, 0);
            uint extComputedCrc = ComputeCrc(CRC_MASK, extData) ^ CRC_MASK;

            if(extStoredCrc != extComputedCrc)
            {
                AaruLogging.Debug(MODULE_NAME, "[red]Extended header CRC mismatch[/]");

                continue;
            }

            // First byte is the tag, second byte is the continuation flag
            if(extData.Length >= 2)
            {
                byte tag  = extData[0];
                var  data = new byte[extData.Length - 2];

                if(data.Length > 0) Array.Copy(extData, 2, data, 0, data.Length);

                // Find existing block with same tag and append, or create new
                var appended = false;

                for(var i = 0; i < extHeaders.Count; i++)
                {
                    if(extHeaders[i].Tag != tag) continue;

                    // Append data to existing block
                    var combined = new byte[extHeaders[i].Data.Length + data.Length];
                    Array.Copy(extHeaders[i].Data, 0, combined, 0,                         extHeaders[i].Data.Length);
                    Array.Copy(data,               0, combined, extHeaders[i].Data.Length, data.Length);

                    extHeaders[i] = new ExtHeaderBlock
                    {
                        Tag  = tag,
                        Data = combined
                    };

                    appended = true;

                    break;
                }

                if(!appended)
                    extHeaders.Add(new ExtHeaderBlock
                    {
                        Tag  = tag,
                        Data = data
                    });
            }
        }

        return true;
    }

    /// <summary>Decompress an EA extended header block.</summary>
    static byte[] DecompressEaBlock(byte[] rawEaData)
    {
        if(rawEaData is null || rawEaData.Length < 4) return null;

        byte eaMethod  = rawEaData[0];
        int  eaOrigLen = rawEaData[1] | rawEaData[2] << 8;

        // Bytes 3-6 would be CRC, but we skip validation here
        int compDataOffset = eaMethod == 0 ? 3 : 7;

        if(compDataOffset > rawEaData.Length) return null;

        int compDataLen = rawEaData.Length - compDataOffset;

        if(eaMethod == 0)
        {
            // Stored EA data
            var result = new byte[compDataLen];
            Array.Copy(rawEaData, compDataOffset, result, 0, compDataLen);

            return result;
        }

        if(eaMethod > 4 || eaOrigLen <= 0) return null;

        try
        {
            var compStream   = new MemoryStream(rawEaData, compDataOffset, compDataLen);
            var arjStream    = new ArjStream(compStream, eaOrigLen, eaMethod);
            var decompressed = new byte[eaOrigLen];
            var totalRead    = 0;

            while(totalRead < eaOrigLen)
            {
                int bytesRead = arjStream.Read(decompressed, totalRead, eaOrigLen - totalRead);

                if(bytesRead <= 0) break;

                totalRead += bytesRead;
            }

            return decompressed;
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Exception decompressing extended attributes: {0}", ex);

            return null;
        }
    }

    /// <summary>Extract a null-terminated string from a byte array at the given offset.</summary>
    static string ExtractNullTerminatedString(byte[] data, int offset, Encoding encoding)
    {
        if(offset >= data.Length) return "";

        int end = Array.IndexOf(data, (byte)0, offset);

        if(end < 0) end = data.Length;

        int length = end - offset;

        return length <= 0 ? "" : encoding.GetString(data, offset, length);
    }

    /// <summary>Get the length including the null terminator of a null-terminated string.</summary>
    static int GetNullTerminatedLength(byte[] data, int offset)
    {
        if(offset >= data.Length) return 0;

        int end = Array.IndexOf(data, (byte)0, offset);

        if(end < 0) return data.Length - offset;

        return end - offset;
    }

    /// <summary>Normalize path separators to forward slash.</summary>
    static string NormalizePath(string path) => path?.Replace('\\', '/');

    struct ExtHeaderBlock
    {
        public byte   Tag;
        public byte[] Data;
    }

#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        if(filter.DataForkLength < MIN_HEADER_SIZE) return ErrorNumber.InvalidArgument;

        _stream          = filter.GetDataForkStream();
        _stream.Position = 0;
        _encoding        = encoding ?? Encoding.GetEncoding(437);
        _entries         = [];

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.HasEntryTimestamp;

        // Read and skip the main archive header
        if(!ReadHeader(out _, out _))
        {
            AaruLogging.Debug(MODULE_NAME, "[red]Invalid main archive header[/]");

            return ErrorNumber.InvalidArgument;
        }

        // Read file entry headers
        while(_stream.Position < _stream.Length)
        {
            if(!ReadHeader(out byte[] headerData, out List<ExtHeaderBlock> extHeaders)) break;

            if(headerData is null || headerData.Length < FIRST_HDR_SIZE) break;

            // Parse fixed header fields
            var  pos          = 0;
            byte firstHdrSize = headerData[pos];
            byte arjxNbr      = headerData[pos + 2];
            byte hostOs       = headerData[pos + 3];
            byte arjFlags     = headerData[pos + 4];
            byte method       = headerData[pos + 5];
            byte fileType     = headerData[pos + 6];
            pos += 8;
            var  timestamp    = BitConverter.ToUInt32(headerData, pos);
            pos += 4;
            var compSize = BitConverter.ToUInt32(headerData, pos);
            pos += 4;
            var origSize = BitConverter.ToUInt32(headerData, pos);
            pos += 4;
            var fileCrc = BitConverter.ToUInt32(headerData, pos);
            pos += 4;
            var fileMode = BitConverter.ToUInt16(headerData, pos + 2);
            pos += 4;

            uint accessTime   = 0;
            uint creationTime = 0;

            // Do not trust first_hdr_size alone, the header actually read can be shorter
            if(firstHdrSize >= FIRST_HDR_SIZE && headerData.Length >= pos + 2)
                pos += 2; // ext_flags, chapter number

            if(firstHdrSize >= FIRST_HDR_SIZE_V && headerData.Length >= pos + 4)
                pos += 4; // prot_blocks, arjprot_id, reserved

            if(firstHdrSize >= R9_HDR_SIZE && headerData.Length >= pos + 16)
            {
                pos          += 4; // resume_position
                accessTime   =  BitConverter.ToUInt32(headerData, pos);
                creationTime =  BitConverter.ToUInt32(headerData, pos + 4);
            }

            // Extract filename (null-terminated at offset firstHdrSize)
            string filename = ExtractNullTerminatedString(headerData, firstHdrSize, _encoding);

            // Extract comment (null-terminated after filename)
            int    commentOffset = firstHdrSize + GetNullTerminatedLength(headerData, firstHdrSize) + 1;
            string comment       = null;

            if(commentOffset < headerData.Length)
            {
                string commentStr = ExtractNullTerminatedString(headerData, commentOffset, _encoding);

                if(!string.IsNullOrEmpty(commentStr)) comment = commentStr;
            }

            AaruLogging.Debug(MODULE_NAME, "[navy]filename[/] = [green]\"{0}\"[/]", filename);
            AaruLogging.Debug(MODULE_NAME, "[navy]method[/] = [teal]{0}[/]",        method);
            AaruLogging.Debug(MODULE_NAME, "[navy]compSize[/] = [teal]{0}[/]",      compSize);
            AaruLogging.Debug(MODULE_NAME, "[navy]origSize[/] = [teal]{0}[/]",      origSize);
            AaruLogging.Debug(MODULE_NAME, "[navy]hostOs[/] = [teal]{0}[/]",        (HostOs)hostOs);
            AaruLogging.Debug(MODULE_NAME, "[navy]fileType[/] = [teal]{0}[/]",      (FileType)fileType);
            AaruLogging.Debug(MODULE_NAME, "[navy]arjFlags[/] = [teal]0x{0:X2}[/]", arjFlags);
            AaruLogging.Debug(MODULE_NAME, "[navy]arjxNbr[/] = [teal]{0}[/]",       arjxNbr);

            // Process extended attributes from extended headers
            byte[] eaData = null;

            if(extHeaders is { Count: > 0 })
            {
                foreach(ExtHeaderBlock eh in extHeaders)
                {
                    if(eh.Tag != EA_TAG || eh.Data is null || eh.Data.Length < 4) continue;

                    eaData = DecompressEaBlock(eh.Data);
                }
            }

            HostOs parsedHostOs = hostOs <= 11 ? (HostOs)hostOs : HostOs.MsDos;

            var entry = new Entry
            {
                Method             = method <= 4 ? (Method)method : Method.Stored,
                Filename           = NormalizePath(filename),
                CompressedSize     = compSize,
                UncompressedSize   = origSize,
                DataOffset         = _stream.Position,
                LastWriteTime      = TimestampToDateTime(timestamp, parsedHostOs),
                HostOs             = parsedHostOs,
                FileCrc            = fileCrc,
                FileMode           = fileMode,
                ArjFlags           = arjFlags,
                ArjxNbr            = arjxNbr,
                Comment            = comment,
                ExtendedAttributes = eaData,
                IsDirectory        = (FileType)fileType == FileType.Directory
            };

            if(accessTime != 0) entry.LastAccessTime = TimestampToDateTime(accessTime, parsedHostOs);

            if(creationTime != 0) entry.CreationTime = TimestampToDateTime(creationTime, parsedHostOs);

            // Update features
            if(entry.Method != Method.Stored && entry.CompressedSize > 0)
                _features |= ArchiveSupportedFeature.SupportsCompression;

            if(entry.IsDirectory)
            {
                _features |= ArchiveSupportedFeature.HasExplicitDirectories |
                             ArchiveSupportedFeature.SupportsSubdirectories;
            }

            if(entry.Comment is not null || entry.ExtendedAttributes is not null)
                _features |= ArchiveSupportedFeature.SupportsXAttrs;

            if(entry.Filename.Contains('/')) _features |= ArchiveSupportedFeature.SupportsSubdirectories;

            _entries.Add(entry);

            // Skip past compressed data
            _stream.Position = entry.DataOffset + entry.CompressedSize;
        }

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        if(!Opened) return;

        _stream?.Close();
        _entries?.Clear();

        _stream = null;
        Opened  = false;
    }

#endregion
}