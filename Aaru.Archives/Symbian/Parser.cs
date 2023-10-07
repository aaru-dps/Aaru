// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

public sealed partial class Symbian
{
    void Parse(BinaryReader br, ref uint offset, ref uint currentFile, uint maxFiles, List<string> languages)
    {
        currentFile++;

        if(currentFile > maxFiles)
            return;

        AaruConsole.DebugWriteLine(MODULE_NAME, "Seeking to {0} for parsing of file {1} of {2}", offset, currentFile,
                                   maxFiles);

        br.BaseStream.Seek(offset, SeekOrigin.Begin);
        var recordType = (FileRecordType)br.ReadUInt32();

        AaruConsole.DebugWriteLine(MODULE_NAME, "Found record with type {0}", recordType);

        br.BaseStream.Seek(-sizeof(FileRecordType), SeekOrigin.Current);

        byte[] buffer;

        switch(recordType)
        {
            case FileRecordType.SimpleFile:
                buffer = br.ReadBytes(Marshal.SizeOf<SimpleFileRecord>());
                SimpleFileRecord simpleFileRecord = Marshal.ByteArrayToStructureLittleEndian<SimpleFileRecord>(buffer);

                offset = (uint)br.BaseStream.Position;

                // Remove the 3 fields that exist only on >= ER6
                if(!_release6)
                    offset -= sizeof(uint) * 3;

                var decodedFileRecord = new DecodedFileRecord
                {
                    type    = simpleFileRecord.record.type,
                    details = simpleFileRecord.record.details,
                    length  = simpleFileRecord.length,
                    pointer = simpleFileRecord.pointer
                };

                br.BaseStream.Seek(simpleFileRecord.record.sourceNamePtr, SeekOrigin.Begin);
                buffer                       = br.ReadBytes((int)simpleFileRecord.record.sourceNameLen);
                decodedFileRecord.sourceName = _encoding.GetString(buffer);

                br.BaseStream.Seek(simpleFileRecord.record.destinationNamePtr, SeekOrigin.Begin);
                buffer                            = br.ReadBytes((int)simpleFileRecord.record.destinationNameLen);
                decodedFileRecord.destinationName = _encoding.GetString(buffer);

                if(_release6)
                {
                    decodedFileRecord.originalLength = simpleFileRecord.originalLength;

                    br.BaseStream.Seek(simpleFileRecord.mimePtr, SeekOrigin.Begin);
                    buffer                 = br.ReadBytes((int)simpleFileRecord.mimeLen);
                    decodedFileRecord.mime = _encoding.GetString(buffer);
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, "Found file for \"{0}\" with length {1} at {2}",
                                           decodedFileRecord.destinationName, decodedFileRecord.length,
                                           decodedFileRecord.pointer);

                _files.Add(decodedFileRecord);

                break;
            case FileRecordType.MultipleLanguageFiles:
                MultipleFileRecord multipleFileRecord = new();

                // Read common file record fields
                buffer                    = br.ReadBytes(Marshal.SizeOf<BaseFileRecord>());
                multipleFileRecord.record = Marshal.ByteArrayToStructureLittleEndian<BaseFileRecord>(buffer);

                buffer = br.ReadBytes(sizeof(uint) * languages.Count);
                ReadOnlySpan<byte> span = buffer;
                multipleFileRecord.lengths = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                buffer                      = br.ReadBytes(sizeof(uint) * languages.Count);
                span                        = buffer;
                multipleFileRecord.pointers = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                if(_release6)
                {
                    buffer = br.ReadBytes(sizeof(uint) * languages.Count);
                    span   = buffer;
                    multipleFileRecord.originalLengths =
                        MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();
                    multipleFileRecord.mimeLen = br.ReadUInt32();
                    multipleFileRecord.mimePtr = br.ReadUInt32();
                }
                else
                    multipleFileRecord.originalLengths = multipleFileRecord.lengths;

                offset = (uint)br.BaseStream.Position;

                br.BaseStream.Seek(multipleFileRecord.record.sourceNamePtr, SeekOrigin.Begin);
                buffer = br.ReadBytes((int)multipleFileRecord.record.sourceNameLen);
                string sourceName = _encoding.GetString(buffer);

                br.BaseStream.Seek(multipleFileRecord.record.destinationNamePtr, SeekOrigin.Begin);
                buffer = br.ReadBytes((int)multipleFileRecord.record.destinationNameLen);
                string destinationName = _encoding.GetString(buffer);

                string mimeType = null;

                if(_release6)
                {
                    br.BaseStream.Seek(multipleFileRecord.mimePtr, SeekOrigin.Begin);
                    buffer   = br.ReadBytes((int)multipleFileRecord.mimeLen);
                    mimeType = _encoding.GetString(buffer);
                }

                var decodedFileRecords = new DecodedFileRecord[languages.Count];

                for(var i = 0; i < languages.Count; i++)
                {
                    decodedFileRecords[i].type            = multipleFileRecord.record.type;
                    decodedFileRecords[i].details         = multipleFileRecord.record.details;
                    decodedFileRecords[i].sourceName      = sourceName;
                    decodedFileRecords[i].destinationName = destinationName;
                    decodedFileRecords[i].length          = multipleFileRecord.lengths[i];
                    decodedFileRecords[i].pointer         = multipleFileRecord.pointers[i];
                    decodedFileRecords[i].originalLength  = multipleFileRecord.originalLengths[i];
                    decodedFileRecords[i].mime            = mimeType;
                    decodedFileRecords[i].language        = languages[i];
                }

                _files.AddRange(decodedFileRecords);

                break;
            case FileRecordType.Options:
                throw new NotImplementedException();
            case FileRecordType.If:
                throw new NotImplementedException();
            case FileRecordType.ElseIf:
                throw new NotImplementedException();
            case FileRecordType.Else:
                throw new NotImplementedException();
            case FileRecordType.EndIf:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}