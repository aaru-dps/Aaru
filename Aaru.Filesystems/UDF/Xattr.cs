// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

public sealed partial class UDF
{
    // Extended Attribute Type constants per ECMA-167
    const uint EA_TYPE_CHARSET_INFO    = 1;
    const uint EA_TYPE_ALTERNATE_PERMS = 3;
    const uint EA_TYPE_FILE_TIMES      = 5;
    const uint EA_TYPE_INFO_TIMES      = 6;
    const uint EA_TYPE_DEVICE_SPEC     = 12;
    const uint EA_TYPE_IMPLEMENTATION  = 2048;
    const uint EA_TYPE_APPLICATION     = 65536;

    /// <inheritdoc />
    public ErrorNumber ListXAttr(string path, out List<string> xattrs)
    {
        xattrs = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Get file entry buffer and parse it
        ErrorNumber errno = GetFileEntryBuffer(path, out byte[] feBuffer, out ushort partitionReferenceNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ParseFileEntryInfo(feBuffer, out UdfFileEntryInfo fileEntryInfo);

        if(errno != ErrorNumber.NoError) return errno;

        xattrs = [];

        // Handle extended attributes
        if(fileEntryInfo.LengthOfExtendedAttributes > 0) ListExtendedAttributes(feBuffer, fileEntryInfo, xattrs);

        // Handle named streams (UDF 2.00+)
        if(fileEntryInfo.IsExtended && fileEntryInfo.StreamDirectoryICB.extentLength > 0)
        {
            errno = ReadNamedStreams(fileEntryInfo.StreamDirectoryICB, out List<UdfNamedStream> streams);

            if(errno == ErrorNumber.NoError)
            {
                foreach(UdfNamedStream stream in streams)
                {
                    // Skip streams that are decoded separately or represent metadata
                    if(stream.Name == STREAM_BACKUP) continue;

                    if(stream.Name == STREAM_OS2_EA)
                    {
                        // OS/2 EA stream - enumerate individual EAs
                        ListOs2EaFromStream(stream, xattrs);

                        continue;
                    }

                    if(!string.IsNullOrEmpty(stream.XattrName) && !xattrs.Contains(stream.XattrName))
                        xattrs.Add(stream.XattrName);
                }
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetXattr(string path, string xattr, ref byte[] buf)
    {
        buf = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        // Get file entry buffer and parse it
        ErrorNumber errno = GetFileEntryBuffer(path, out byte[] feBuffer, out ushort partitionReferenceNumber);

        if(errno != ErrorNumber.NoError) return errno;

        errno = ParseFileEntryInfo(feBuffer, out UdfFileEntryInfo fileEntryInfo);

        if(errno != ErrorNumber.NoError) return errno;

        // First try extended attributes
        if(fileEntryInfo.LengthOfExtendedAttributes > 0)
        {
            ErrorNumber eaResult = GetXattrFromExtendedAttributes(feBuffer, fileEntryInfo, xattr, ref buf);

            if(eaResult == ErrorNumber.NoError) return ErrorNumber.NoError;
        }

        // Then try named streams (UDF 2.00+)
        if(fileEntryInfo.IsExtended && fileEntryInfo.StreamDirectoryICB.extentLength > 0)
        {
            ErrorNumber streamResult = GetXattrFromNamedStreams(fileEntryInfo, xattr, ref buf);

            if(streamResult == ErrorNumber.NoError) return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>
    ///     Lists extended attributes from the file entry buffer
    /// </summary>
    void ListExtendedAttributes(byte[] feBuffer, UdfFileEntryInfo fileEntryInfo, List<string> xattrs)
    {
        // Parse extended attributes
        int fixedSize = fileEntryInfo.IsExtended ? 216 : 176;
        int eaOffset  = fixedSize;
        int eaEnd     = fixedSize + (int)fileEntryInfo.LengthOfExtendedAttributes;

        // First, check for Extended Attribute Header Descriptor
        if(eaEnd - eaOffset >= 24)
        {
            var tagId = (TagIdentifier)BitConverter.ToUInt16(feBuffer, eaOffset);

            if(tagId == TagIdentifier.ExtendedAttributeHeaderDescriptor) eaOffset += 24; // Skip the header descriptor
        }

        while(eaOffset + 12 <= eaEnd) // Minimum EA header is 12 bytes
        {
            GenericExtendedAttributeHeader eaHeader =
                Marshal.ByteArrayToStructureLittleEndian<GenericExtendedAttributeHeader>(feBuffer, eaOffset, 12);

            if(eaHeader.attributeLength == 0) break;

            // Skip EAs that are handled specially (e.g., file times are parsed in Stat)
            if(IsEaHandledSpecially(eaHeader.attributeType))
            {
                eaOffset += (int)eaHeader.attributeLength;

                continue;
            }

            // Special handling for OS/2 EAs - they contain multiple FEA entries
            if(eaHeader.attributeType == EA_TYPE_IMPLEMENTATION)
            {
                int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>();

                if(eaOffset + headerSize <= feBuffer.Length)
                {
                    ImplementationUseExtendedAttribute iuea =
                        Marshal.ByteArrayToStructureLittleEndian<ImplementationUseExtendedAttribute>(feBuffer,
                            eaOffset,
                            headerSize);

                    if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os2_Ea))
                    {
                        // Enumerate all FEA entries
                        List<string> os2Xattrs = GetOs2EaNames(feBuffer, eaOffset, iuea);

                        foreach(string os2Xattr in os2Xattrs)
                            if(!xattrs.Contains(os2Xattr))
                                xattrs.Add(os2Xattr);

                        eaOffset += (int)eaHeader.attributeLength;

                        continue;
                    }
                }
            }

            string xattrName = GetXAttrNameForEa(feBuffer, eaOffset, eaHeader);

            if(!string.IsNullOrEmpty(xattrName) && !xattrs.Contains(xattrName)) xattrs.Add(xattrName);

            eaOffset += (int)eaHeader.attributeLength;
        }
    }

    /// <summary>
    ///     Enumerates OS/2 EAs from a named stream
    /// </summary>
    void ListOs2EaFromStream(UdfNamedStream stream, List<string> xattrs)
    {
        // Read the stream data using partition-aware read
        ErrorNumber errno = ReadSectorFromPartition(stream.Icb.extentLocation.logicalBlockNumber,
                                                    stream.Icb.extentLocation.partitionReferenceNumber,
                                                    _partitionStartingLocation,
                                                    out byte[] streamBuffer);

        if(errno != ErrorNumber.NoError) return;

        if(ParseFileEntryInfo(streamBuffer, out UdfFileEntryInfo streamInfo) != ErrorNumber.NoError) return;

        var adType = (byte)((ushort)streamInfo.IcbTag.flags & 0x07);

        if(ReadFileDataFromInfo(streamInfo,
                                streamBuffer,
                                adType,
                                stream.Icb.extentLocation.partitionReferenceNumber,
                                out byte[] streamData) !=
           ErrorNumber.NoError)
            return;

        // Parse FEA entries
        var offset  = 0;
        int feaSize = System.Runtime.InteropServices.Marshal.SizeOf<Fea>();

        while(offset + feaSize <= streamData.Length)
        {
            Fea fea = Marshal.ByteArrayToStructureLittleEndian<Fea>(streamData, offset, feaSize);

            if(fea.lengthOfName == 0) break;

            if(offset + feaSize + fea.lengthOfName > streamData.Length) break;

            string eaName = Encoding.ASCII.GetString(streamData, offset + feaSize, fea.lengthOfName);

            if(!string.IsNullOrEmpty(eaName))
            {
                var xattrName = $"com.ibm.os2.{eaName}";

                if(!xattrs.Contains(xattrName)) xattrs.Add(xattrName);
            }

            offset += feaSize + fea.lengthOfName + fea.lengthOfValue;
        }
    }

    /// <summary>
    ///     Gets xattr data from extended attributes
    /// </summary>
    ErrorNumber GetXattrFromExtendedAttributes(byte[]     feBuffer, UdfFileEntryInfo fileEntryInfo, string xattr,
                                               ref byte[] buf)
    {
        int fixedSize = fileEntryInfo.IsExtended ? 216 : 176;
        int eaOffset  = fixedSize;
        int eaEnd     = fixedSize + (int)fileEntryInfo.LengthOfExtendedAttributes;

        // First, check for Extended Attribute Header Descriptor
        if(eaEnd - eaOffset >= 24)
        {
            var tagId = (TagIdentifier)BitConverter.ToUInt16(feBuffer, eaOffset);

            if(tagId == TagIdentifier.ExtendedAttributeHeaderDescriptor) eaOffset += 24;
        }

        while(eaOffset + 12 <= eaEnd)
        {
            GenericExtendedAttributeHeader eaHeader =
                Marshal.ByteArrayToStructureLittleEndian<GenericExtendedAttributeHeader>(feBuffer, eaOffset, 12);

            if(eaHeader.attributeLength == 0) break;

            // Skip EAs that are handled specially (e.g., file times are parsed in Stat)
            if(IsEaHandledSpecially(eaHeader.attributeType))
            {
                eaOffset += (int)eaHeader.attributeLength;

                continue;
            }

            // Special handling for OS/2 EAs
            if(eaHeader.attributeType == EA_TYPE_IMPLEMENTATION &&
               xattr.StartsWith("com.ibm.os2.", StringComparison.Ordinal))
            {
                int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>();

                if(eaOffset + headerSize <= feBuffer.Length)
                {
                    ImplementationUseExtendedAttribute iuea =
                        Marshal.ByteArrayToStructureLittleEndian<ImplementationUseExtendedAttribute>(feBuffer,
                            eaOffset,
                            headerSize);

                    if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os2_Ea))
                    {
                        string os2EaName = xattr["com.ibm.os2.".Length..];
                        buf = GetOs2EaData(feBuffer, eaOffset, iuea, os2EaName);

                        return buf != null ? ErrorNumber.NoError : ErrorNumber.NoSuchExtendedAttribute;
                    }
                }
            }

            string xattrName = GetXAttrNameForEa(feBuffer, eaOffset, eaHeader);

            if(xattrName == xattr)
            {
                buf = GetXAttrData(feBuffer, eaOffset, eaHeader);

                return buf != null ? ErrorNumber.NoError : ErrorNumber.NoSuchExtendedAttribute;
            }

            eaOffset += (int)eaHeader.attributeLength;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>
    ///     Gets xattr data from named streams
    /// </summary>
    ErrorNumber GetXattrFromNamedStreams(UdfFileEntryInfo fileEntryInfo, string xattr, ref byte[] buf)
    {
        ErrorNumber errno = ReadNamedStreams(fileEntryInfo.StreamDirectoryICB, out List<UdfNamedStream> streams);

        if(errno != ErrorNumber.NoError) return errno;

        foreach(UdfNamedStream stream in streams)
        {
            // Check if this stream matches the requested xattr
            bool matches = stream.XattrName == xattr || stream.Name == xattr;

            // Special handling for OS/2 EA stream
            if(stream.Name == STREAM_OS2_EA && xattr.StartsWith("com.ibm.os2.", StringComparison.Ordinal))
            {
                string os2EaName = xattr["com.ibm.os2.".Length..];
                buf = GetOs2EaDataFromStream(stream, os2EaName);

                return buf != null ? ErrorNumber.NoError : ErrorNumber.NoSuchExtendedAttribute;
            }

            if(!matches) continue;

            // Read the stream data using partition-aware read
            ErrorNumber streamErr = ReadSectorFromPartition(stream.Icb.extentLocation.logicalBlockNumber,
                                                            stream.Icb.extentLocation.partitionReferenceNumber,
                                                            _partitionStartingLocation,
                                                            out byte[] streamBuffer);

            if(streamErr != ErrorNumber.NoError) continue;

            if(ParseFileEntryInfo(streamBuffer, out UdfFileEntryInfo streamInfo) != ErrorNumber.NoError) continue;

            var adType = (byte)((ushort)streamInfo.IcbTag.flags & 0x07);

            if(ReadFileDataFromInfo(streamInfo,
                                    streamBuffer,
                                    adType,
                                    stream.Icb.extentLocation.partitionReferenceNumber,
                                    out buf) ==
               ErrorNumber.NoError)
                return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchExtendedAttribute;
    }

    /// <summary>
    ///     Gets OS/2 EA data from a named stream
    /// </summary>
    byte[] GetOs2EaDataFromStream(UdfNamedStream stream, string eaName)
    {
        // Read the stream data using partition-aware read
        ErrorNumber errno = ReadSectorFromPartition(stream.Icb.extentLocation.logicalBlockNumber,
                                                    stream.Icb.extentLocation.partitionReferenceNumber,
                                                    _partitionStartingLocation,
                                                    out byte[] streamBuffer);

        if(errno != ErrorNumber.NoError) return null;

        if(ParseFileEntryInfo(streamBuffer, out UdfFileEntryInfo streamInfo) != ErrorNumber.NoError) return null;

        var adType = (byte)((ushort)streamInfo.IcbTag.flags & 0x07);

        if(ReadFileDataFromInfo(streamInfo,
                                streamBuffer,
                                adType,
                                stream.Icb.extentLocation.partitionReferenceNumber,
                                out byte[] streamData) !=
           ErrorNumber.NoError)
            return null;

        // Parse FEA entries to find the requested EA
        var offset  = 0;
        int feaSize = System.Runtime.InteropServices.Marshal.SizeOf<Fea>();

        while(offset + feaSize <= streamData.Length)
        {
            Fea fea = Marshal.ByteArrayToStructureLittleEndian<Fea>(streamData, offset, feaSize);

            if(fea.lengthOfName == 0) break;

            if(offset + feaSize + fea.lengthOfName > streamData.Length) break;

            string currentEaName = Encoding.ASCII.GetString(streamData, offset + feaSize, fea.lengthOfName);

            if(currentEaName.Equals(eaName, StringComparison.OrdinalIgnoreCase))
            {
                int valueOffset = offset + feaSize + fea.lengthOfName;

                if(valueOffset + fea.lengthOfValue > streamData.Length) return null;

                var value = new byte[fea.lengthOfValue];
                Array.Copy(streamData, valueOffset, value, 0, fea.lengthOfValue);

                return value;
            }

            offset += feaSize + fea.lengthOfName + fea.lengthOfValue;
        }

        return null;
    }

    /// <summary>
    ///     Gets the extended attribute name for a given EA based on its type.
    ///     Maps ECMA-167 attribute types to appropriate namespace prefixes
    ///     (ch.ecma.*, com.apple.*, com.ibm.os2.*, org.osta.udf.*).
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <param name="eaHeader">The generic EA header</param>
    /// <returns>The xattr name, or null if the EA should be ignored</returns>
    string GetXAttrNameForEa(byte[] feBuffer, int eaOffset, GenericExtendedAttributeHeader eaHeader)
    {
        switch(eaHeader.attributeType)
        {
            case EA_TYPE_CHARSET_INFO:
                return Xattrs.XATTR_ECMA_CHARSET_INFO;

            case EA_TYPE_ALTERNATE_PERMS:
                return Xattrs.XATTR_ECMA_ALTERNATE_PERMISSIONS;

            case EA_TYPE_FILE_TIMES:
                return Xattrs.XATTR_ECMA_FILE_TIMES;

            case EA_TYPE_INFO_TIMES:
                return Xattrs.XATTR_ECMA_INFO_TIMES;

            case EA_TYPE_DEVICE_SPEC:
                return Xattrs.XATTR_ECMA_DEVICE_SPECIFICATION;

            case EA_TYPE_IMPLEMENTATION:
                return GetImplementationUseEaName(feBuffer, eaOffset);

            case EA_TYPE_APPLICATION:
                return GetApplicationUseEaName(feBuffer, eaOffset);

            default:
                return $"org.osta.udf.ea_type_{eaHeader.attributeType}";
        }
    }

    /// <summary>
    ///     Gets the xattr name for an Implementation Use Extended Attribute.
    ///     Identifies known UDF implementation EAs like Mac ResourceFork, FinderInfo,
    ///     OS/2 EAs, DVD CGMS info, etc. and maps them to appropriate names.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <returns>The xattr name, or null if the EA should be ignored or handled separately</returns>
    string GetImplementationUseEaName(byte[] feBuffer, int eaOffset)
    {
        // Implementation Use EA has EntityIdentifier at offset 12 (after the 12-byte common header)
        if(eaOffset + 44 > feBuffer.Length) return "org.osta.udf.implementation_use";

        ImplementationUseExtendedAttribute iuea =
            Marshal.ByteArrayToStructureLittleEndian<ImplementationUseExtendedAttribute>(feBuffer,
                eaOffset,
                System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>());

        if(iuea.implementationIdentifier.identifier == null) return "org.osta.udf.implementation_use";

        // Check for known identifiers
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_ResourceFork))
            return Xattrs.XATTR_APPLE_RESOURCE_FORK;

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_FinderInfo))
            return Xattrs.XATTR_APPLE_FINDER_INFO;

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_UniqueId))
            return null; // MacUniqueIDTable is internal UDF structure, should be ignored

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_VolumeInfo))
            return Xattrs.XATTR_APPLE_FINDER_INFO; // MacVolumeInfo contains Finder info

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os2_Ea))
            return null; // OS/2 EAs are handled separately in ListXAttr

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os2_Ea_Len))
            return null; // OS/2 EALength should be ignored

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _dvd_Cgms)) return Xattrs.XATTR_UDF_DVD_CGMS;

        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _udf_Free_Ea))
            return Xattrs.XATTR_UDF_FREE_EA_SPACE;

        // UDF 2.01: OS/400 DirInfo
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os400_DirInfo))
            return Xattrs.XATTR_IBM_OS400_DIR_INFO;

        // Unknown implementation use EA
        string identifier = StringHandlers.CToString(iuea.implementationIdentifier.identifier, Encoding.ASCII)?.Trim();

        return string.IsNullOrEmpty(identifier)
                   ? "org.osta.udf.implementation_use"
                   : $"org.osta.udf.{identifier.Replace("*", "").Replace(" ", "_").ToLowerInvariant()}";
    }

    /// <summary>
    ///     Gets the names for all OS/2 Extended Attributes stored as FEA (Full EA) entries.
    ///     OS/2 EAs are stored as multiple FEA records within a single Implementation Use EA,
    ///     so this method enumerates all of them and returns their names prefixed with "com.ibm.os2.".
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the Implementation Use EA within the buffer</param>
    /// <param name="iuea">The Implementation Use EA header</param>
    /// <returns>List of xattr names for all FEA entries</returns>
    List<string> GetOs2EaNames(byte[] feBuffer, int eaOffset, ImplementationUseExtendedAttribute iuea)
    {
        var names = new List<string>();

        // OS/2 EA data follows the Implementation Use EA header
        // Format: 2-byte header checksum followed by FEA entries

        int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>();
        int dataOffset = eaOffset   + headerSize;
        int dataEnd    = dataOffset + (int)iuea.implementationUseLength;

        if(dataOffset + 2 > feBuffer.Length) return names;

        // Skip 2-byte header checksum
        dataOffset += 2;

        int feaSize = System.Runtime.InteropServices.Marshal.SizeOf<Fea>();

        // Parse all FEA entries
        while(dataOffset + feaSize <= dataEnd && dataOffset + feaSize <= feBuffer.Length)
        {
            Fea fea = Marshal.ByteArrayToStructureLittleEndian<Fea>(feBuffer, dataOffset, feaSize);

            if(fea.lengthOfName == 0) break;

            if(dataOffset + feaSize + fea.lengthOfName > feBuffer.Length) break;

            string eaName = Encoding.ASCII.GetString(feBuffer, dataOffset + feaSize, fea.lengthOfName);

            if(!string.IsNullOrEmpty(eaName)) names.Add($"com.ibm.os2.{eaName}");

            // Move to next FEA entry: header + name + value
            dataOffset += feaSize + fea.lengthOfName + fea.lengthOfValue;
        }

        return names;
    }

    /// <summary>
    ///     Gets the data for a specific OS/2 Extended Attribute by name.
    ///     Searches through the FEA entries within the OS/2 EA to find the one with the matching name.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the Implementation Use EA within the buffer</param>
    /// <param name="iuea">The Implementation Use EA header</param>
    /// <param name="eaName">The OS/2 EA name to search for (without the "com.ibm.os2." prefix)</param>
    /// <returns>The EA value data, or null if not found</returns>
    byte[] GetOs2EaData(byte[] feBuffer, int eaOffset, ImplementationUseExtendedAttribute iuea, string eaName)
    {
        // OS/2 EA data follows the Implementation Use EA header
        // Format: 2-byte header checksum followed by FEA entries

        int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>();
        int dataOffset = eaOffset   + headerSize;
        int dataEnd    = dataOffset + (int)iuea.implementationUseLength;

        if(dataOffset + 2 > feBuffer.Length) return null;

        // Skip 2-byte header checksum
        dataOffset += 2;

        int feaSize = System.Runtime.InteropServices.Marshal.SizeOf<Fea>();

        // Parse all FEA entries to find the requested one
        while(dataOffset + feaSize <= dataEnd && dataOffset + feaSize <= feBuffer.Length)
        {
            Fea fea = Marshal.ByteArrayToStructureLittleEndian<Fea>(feBuffer, dataOffset, feaSize);

            if(fea.lengthOfName == 0) break;

            if(dataOffset + feaSize + fea.lengthOfName > feBuffer.Length) break;

            string currentEaName = Encoding.ASCII.GetString(feBuffer, dataOffset + feaSize, fea.lengthOfName);

            if(currentEaName.Equals(eaName, StringComparison.OrdinalIgnoreCase))
            {
                // Found it - extract the value
                int valueOffset = dataOffset + feaSize + fea.lengthOfName;

                if(valueOffset + fea.lengthOfValue > feBuffer.Length) return null;

                var value = new byte[fea.lengthOfValue];
                Array.Copy(feBuffer, valueOffset, value, 0, fea.lengthOfValue);

                return value;
            }

            // Move to next FEA entry: header + name + value
            dataOffset += feaSize + fea.lengthOfName + fea.lengthOfValue;
        }

        return null;
    }

    /// <summary>
    ///     Gets the xattr name for an Application Use Extended Attribute.
    ///     Application Use EAs are generic EAs identified by an application identifier,
    ///     which is converted to an xattr name with the "org.osta.udf.app." prefix.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <returns>The xattr name for this Application Use EA</returns>
    string GetApplicationUseEaName(byte[] feBuffer, int eaOffset)
    {
        if(eaOffset + 44 > feBuffer.Length) return "org.osta.udf.application_use";

        ApplicationUseExtendedAttribute auea =
            Marshal.ByteArrayToStructureLittleEndian<ApplicationUseExtendedAttribute>(feBuffer,
                eaOffset,
                System.Runtime.InteropServices.Marshal.SizeOf<ApplicationUseExtendedAttribute>());

        if(auea.applicationIdentifier.identifier == null) return "org.osta.udf.application_use";

        string identifier = StringHandlers.CToString(auea.applicationIdentifier.identifier, Encoding.ASCII)?.Trim();

        return string.IsNullOrEmpty(identifier)
                   ? "org.osta.udf.application_use"
                   : $"org.osta.udf.app.{identifier.Replace("*", "").Replace(" ", "_").ToLowerInvariant()}";
    }

    /// <summary>
    ///     Gets the data for an extended attribute based on its type.
    ///     Routes to specialized handlers for Implementation Use and Application Use EAs,
    ///     and returns raw data for standard ECMA-167 EA types.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <param name="eaHeader">The generic EA header</param>
    /// <returns>The EA data, or null if extraction fails</returns>
    byte[] GetXAttrData(byte[] feBuffer, int eaOffset, GenericExtendedAttributeHeader eaHeader)
    {
        switch(eaHeader.attributeType)
        {
            case EA_TYPE_CHARSET_INFO:
            case EA_TYPE_ALTERNATE_PERMS:
            case EA_TYPE_FILE_TIMES:
            case EA_TYPE_INFO_TIMES:
            case EA_TYPE_DEVICE_SPEC:
            {
                // Return the data portion after the 12-byte header
                int dataOffset = eaOffset                      + 12;
                int dataLength = (int)eaHeader.attributeLength - 12;

                if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

                var data = new byte[dataLength];
                Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

                return data;
            }

            case EA_TYPE_IMPLEMENTATION:
                return GetImplementationUseEaData(feBuffer, eaOffset, eaHeader);

            case EA_TYPE_APPLICATION:
                return GetApplicationUseEaData(feBuffer, eaOffset, eaHeader);

            default:
            {
                // Unknown EA type, return raw data after header
                int dataOffset = eaOffset                      + 12;
                int dataLength = (int)eaHeader.attributeLength - 12;

                if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

                var data = new byte[dataLength];
                Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

                return data;
            }
        }
    }

    /// <summary>
    ///     Gets the data for an Implementation Use Extended Attribute.
    ///     Handles special cases for Mac EAs (strips headers from FinderInfo and ResourceFork),
    ///     and returns the implementation use data for other types.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <param name="eaHeader">The generic EA header</param>
    /// <returns>The EA data with UDF-specific headers stripped, or null if extraction fails</returns>
    byte[] GetImplementationUseEaData(byte[] feBuffer, int eaOffset, GenericExtendedAttributeHeader eaHeader)
    {
        int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ImplementationUseExtendedAttribute>();

        if(eaOffset + headerSize > feBuffer.Length) return null;

        ImplementationUseExtendedAttribute iuea =
            Marshal.ByteArrayToStructureLittleEndian<ImplementationUseExtendedAttribute>(feBuffer,
                eaOffset,
                headerSize);

        // OS/2 EAs are handled separately via GetOs2EaData
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os2_Ea)) return null;

        int dataOffset = eaOffset + headerSize;
        var dataLength = (int)iuea.implementationUseLength;

        // For MacVolumeInfo, return only the volumeFinderInformation (32 bytes at offset 26)
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_VolumeInfo))
        {
            int macVolumeInfoSize = System.Runtime.InteropServices.Marshal.SizeOf<MacVolumeInfo>();

            if(dataOffset + macVolumeInfoSize <= feBuffer.Length)
            {
                MacVolumeInfo macVolumeInfo =
                    Marshal.ByteArrayToStructureLittleEndian<MacVolumeInfo>(feBuffer, dataOffset, macVolumeInfoSize);

                return macVolumeInfo.volumeFinderInformation;
            }

            return null;
        }

        // For MacFinderInfo, skip the 2-byte header checksum and 2-byte padding
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_FinderInfo))
        {
            // Skip 2-byte header checksum + 2-byte padding
            dataOffset += 4;
            dataLength -= 4;

            if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

            var data = new byte[dataLength];
            Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

            return data;
        }

        // For MacResourceFork, skip the 2-byte header checksum
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _mac_ResourceFork))
        {
            // Skip 2-byte header checksum
            dataOffset += 2;
            dataLength -= 2;

            if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

            var data = new byte[dataLength];
            Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

            return data;
        }

        // For OS/400 DirInfo, skip the 2-byte header checksum and 2-byte reserved padding
        if(CompareIdentifier(iuea.implementationIdentifier.identifier, _os400_DirInfo))
        {
            // Skip 2-byte header checksum + 2-byte reserved
            dataOffset += 4;
            dataLength -= 4;

            if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

            var data = new byte[dataLength];
            Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

            return data;
        }

        if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

        var rawData = new byte[dataLength];
        Array.Copy(feBuffer, dataOffset, rawData, 0, dataLength);

        return rawData;
    }

    /// <summary>
    ///     Gets the data for an Application Use Extended Attribute.
    ///     Returns the application use data portion after the EA header.
    /// </summary>
    /// <param name="feBuffer">The buffer containing the FileEntry sector</param>
    /// <param name="eaOffset">Offset to the extended attribute within the buffer</param>
    /// <param name="eaHeader">The generic EA header</param>
    /// <returns>The application use data, or null if extraction fails</returns>
    byte[] GetApplicationUseEaData(byte[] feBuffer, int eaOffset, GenericExtendedAttributeHeader eaHeader)
    {
        int headerSize = System.Runtime.InteropServices.Marshal.SizeOf<ApplicationUseExtendedAttribute>();

        if(eaOffset + headerSize > feBuffer.Length) return null;

        ApplicationUseExtendedAttribute auea =
            Marshal.ByteArrayToStructureLittleEndian<ApplicationUseExtendedAttribute>(feBuffer, eaOffset, headerSize);

        int dataOffset = eaOffset + headerSize;
        var dataLength = (int)auea.applicationUseLength;

        if(dataOffset + dataLength > feBuffer.Length || dataLength <= 0) return null;

        var data = new byte[dataLength];
        Array.Copy(feBuffer, dataOffset, data, 0, dataLength);

        return data;
    }

    /// <summary>
    ///     Gets the raw sector buffer containing the FileEntry for a given path.
    ///     This buffer includes the FileEntry structure followed by extended attributes
    ///     and allocation descriptors.
    /// </summary>
    /// <param name="path">Path to the file or directory</param>
    /// <param name="feBuffer">The raw sector buffer if found</param>
    /// <returns>Error number</returns>
    ErrorNumber GetFileEntryBuffer(string path, out byte[] feBuffer, out ushort partitionReferenceNumber)
    {
        feBuffer                 = null;
        partitionReferenceNumber = 0;

        // Root directory - use partition-aware read
        if(string.IsNullOrWhiteSpace(path) || path == "/")
        {
            partitionReferenceNumber = _rootDirectoryIcb.extentLocation.partitionReferenceNumber;

            return ReadSectorFromPartition(_rootDirectoryIcb.extentLocation.logicalBlockNumber,
                                           _rootDirectoryIcb.extentLocation.partitionReferenceNumber,
                                           _partitionStartingLocation,
                                           out feBuffer);
        }

        string   cutPath    = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;
        string[] pieces     = cutPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string   parentPath = pieces.Length > 1 ? string.Join("/", pieces[..^1]) : "";
        string   fileName   = pieces[^1];

        ErrorNumber errno = GetDirectoryEntries(parentPath, out Dictionary<string, UdfDirectoryEntry> parentEntries);

        if(errno != ErrorNumber.NoError) return errno;

        if(!TryGetEntry(parentEntries, fileName, out UdfDirectoryEntry entry)) return ErrorNumber.NoSuchFile;

        partitionReferenceNumber = entry.Icb.extentLocation.partitionReferenceNumber;

        return ReadSectorFromPartition(entry.Icb.extentLocation.logicalBlockNumber,
                                       entry.Icb.extentLocation.partitionReferenceNumber,
                                       _partitionStartingLocation,
                                       out feBuffer);
    }

    /// <summary>
    ///     Compares an identifier byte array with a pattern for UDF entity identification.
    ///     Used to match Implementation Identifier and Application Identifier fields
    ///     against known UDF identifiers like "*UDF Mac ResourceFork".
    /// </summary>
    /// <param name="identifier">The identifier bytes to check</param>
    /// <param name="pattern">The expected pattern to match</param>
    /// <returns>True if the identifier starts with the pattern bytes</returns>
    static bool CompareIdentifier(byte[] identifier, byte[] pattern)
    {
        if(identifier == null || pattern == null) return false;

        int compareLength = Math.Min(identifier.Length, pattern.Length);

        for(var i = 0; i < compareLength; i++)
            if(identifier[i] != pattern[i])
                return false;

        return true;
    }
}