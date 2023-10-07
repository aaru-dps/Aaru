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
using System.IO;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Archives;

public partial class Symbian
{
    void Parse(BinaryReader br, ref uint offset, ref uint currentFile, uint maxFiles)
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
                throw new NotImplementedException();
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