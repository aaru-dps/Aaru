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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Symbian
{
    void Parse(BinaryReader br, ref uint offset, ref uint currentFile, uint maxFiles, List<string> languages,
               ref int      conditionLevel)
    {
        currentFile++;

        if(currentFile > maxFiles)
            return;

        var tabulationChars = new char[conditionLevel];
        for(var i = 0; i < conditionLevel; i++)
            tabulationChars[i] = '\t';
        string tabulation = new(tabulationChars);

        AaruConsole.DebugWriteLine(MODULE_NAME, "Seeking to {0} for parsing of file {1} of {2}", offset, currentFile,
                                   maxFiles);

        br.BaseStream.Seek(offset, SeekOrigin.Begin);
        var recordType = (FileRecordType)br.ReadUInt32();

        AaruConsole.DebugWriteLine(MODULE_NAME, "Found record with type {0}", recordType);

        br.BaseStream.Seek(-sizeof(FileRecordType), SeekOrigin.Current);

        byte[]            buffer;
        ConditionalRecord conditionalRecord;

        StringBuilder conditionSb;
        Attribute?    nullAttribute;
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

                // Files that are not written to disk but shown or installed components do not have a destination name.
                if(simpleFileRecord.record.destinationNameLen > 0)
                {
                    br.BaseStream.Seek(simpleFileRecord.record.destinationNamePtr, SeekOrigin.Begin);
                    buffer                            = br.ReadBytes((int)simpleFileRecord.record.destinationNameLen);
                    decodedFileRecord.destinationName = _encoding.GetString(buffer);
                }
                else
                    decodedFileRecord.destinationName = decodedFileRecord.sourceName;

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

                if(conditionLevel > 0)
                {
                    bool wait, close;
                    switch(decodedFileRecord.type)
                    {
                        case FileType.FileText:
                            switch((FileDetails)((uint)decodedFileRecord.details & 0xFF))
                            {
                                case FileDetails.TextContinue:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecord.sourceName}\", BUTTONS_CONTINUE, ACTION_CONTINUE)");
                                    break;
                                case FileDetails.TextSkip:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecord.sourceName}\", BUTTONS_YES_NO, ACTION_SKIP)");
                                    break;
                                case FileDetails.TextAbort:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecord.sourceName}\", BUTTONS_YES_NO, ACTION_ABORT)");
                                    break;
                                case FileDetails.TextExit:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecord.sourceName}\", BUTTONS_YES_NO, ACTION_EXIT)");
                                    break;
                            }

                            break;
                        case FileType.FileRun:
                            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                            wait  = (decodedFileRecord.details & FileDetails.RunWait) != 0;
                            close = (decodedFileRecord.details & FileDetails.RunsEnd) != 0;
                            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
                            switch((FileDetails)((uint)decodedFileRecord.details & 0xFF))
                            {
                                case FileDetails.RunInstall:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL, 0)");
                                    }

                                    break;
                                case FileDetails.RunRemove:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_REMOVE, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_REMOVE, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_REMOVE, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_REMOVE, 0)");
                                    }

                                    break;
                                case FileDetails.RunBoth:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL | ON_REMOVE, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL | ON_REMOVE, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL | ON_REMOVE, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecord.sourceName}\", ON_INSTALL | ON_REMOVE, 0)");
                                    }

                                    break;
                            }

                            break;
                        case FileType.FileMime:
                            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                            wait  = (decodedFileRecord.details & FileDetails.RunWait) != 0;
                            close = (decodedFileRecord.details & FileDetails.RunsEnd) != 0;
                            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
                            if(wait && close)
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecord.sourceName}\", WAIT | CLOSE)");
                            else if(close)
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecord.sourceName}\", CLOSE)");
                            else if(wait)
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecord.sourceName}\", WAIT)");
                            else
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecord.sourceName}\", 0)");
                            break;
                    }
                }

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
                string destinationName;

                // Files that are not written to disk but shown or installed components do not have a destination name.
                if(multipleFileRecord.record.destinationNameLen > 0)
                {
                    br.BaseStream.Seek(multipleFileRecord.record.destinationNamePtr, SeekOrigin.Begin);
                    buffer          = br.ReadBytes((int)multipleFileRecord.record.destinationNameLen);
                    destinationName = _encoding.GetString(buffer);
                }
                else
                    destinationName = sourceName;

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

                if(conditionLevel > 0)
                {
                    bool wait, close;
                    switch(decodedFileRecords[0].type)
                    {
                        case FileType.File:
                            _conditions.Add(tabulation + $"InstallFileTo(\"{decodedFileRecords[0].destinationName}\")");
                            break;
                        case FileType.FileText:
                            switch((FileDetails)((uint)decodedFileRecords[0].details & 0xFF))
                            {
                                case FileDetails.TextContinue:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecords[0].sourceName}\", BUTTONS_CONTINUE, ACTION_CONTINUE)");
                                    break;
                                case FileDetails.TextSkip:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecords[0].sourceName}\", BUTTONS_YES_NO, ACTION_SKIP)");
                                    break;
                                case FileDetails.TextAbort:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecords[0].sourceName}\", BUTTONS_YES_NO, ACTION_ABORT)");
                                    break;
                                case FileDetails.TextExit:
                                    _conditions.Add(tabulation +
                                                    $"ShowText(\"{decodedFileRecords[0].sourceName}\", BUTTONS_YES_NO, ACTION_EXIT)");
                                    break;
                            }

                            break;
                        case FileType.FileRun:
                            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                            wait  = (decodedFileRecords[0].details & FileDetails.RunWait) != 0;
                            close = (decodedFileRecords[0].details & FileDetails.RunsEnd) != 0;
                            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
                            switch((FileDetails)((uint)decodedFileRecords[0].details & 0xFF))
                            {
                                case FileDetails.RunInstall:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL, 0)");
                                    }

                                    break;
                                case FileDetails.RunRemove:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_REMOVE, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_REMOVE, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_REMOVE, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_REMOVE, 0)");
                                    }

                                    break;
                                case FileDetails.RunBoth:
                                    if(wait && close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL | ON_REMOVE, WAIT | CLOSE)");
                                    }
                                    else if(close)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL | ON_REMOVE, CLOSE)");
                                    }
                                    else if(wait)
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL | ON_REMOVE, WAIT)");
                                    }
                                    else
                                    {
                                        _conditions.Add(tabulation +
                                                        $"Run(\"{decodedFileRecords[0].sourceName}\", ON_INSTALL | ON_REMOVE, 0)");
                                    }

                                    break;
                            }

                            break;
                        case FileType.FileMime:
                            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                            wait  = (decodedFileRecords[0].details & FileDetails.RunWait) != 0;
                            close = (decodedFileRecords[0].details & FileDetails.RunsEnd) != 0;
                            // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
                            if(wait && close)
                            {
                                _conditions.Add(tabulation +
                                                $"Open(\"{decodedFileRecords[0].sourceName}\", WAIT | CLOSE)");
                            }
                            else if(close)
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecords[0].sourceName}\", CLOSE)");
                            else if(wait)
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecords[0].sourceName}\", WAIT)");
                            else
                                _conditions.Add(tabulation + $"Open(\"{decodedFileRecords[0].sourceName}\", 0)");

                            break;
                    }
                }

                break;
            case FileRecordType.Options:
                OptionsLineRecord optionsLineRecord = new()
                {
                    recordType      = (FileRecordType)br.ReadUInt32(),
                    numberOfOptions = br.ReadUInt32()
                };

                optionsLineRecord.options = new OptionRecord[(int)optionsLineRecord.numberOfOptions];

                for(var i = 0; i < optionsLineRecord.numberOfOptions; i++)
                {
                    optionsLineRecord.options[i] = new OptionRecord();

                    buffer = br.ReadBytes(sizeof(uint) * languages.Count);
                    span   = buffer;
                    optionsLineRecord.options[i].lengths =
                        MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                    buffer = br.ReadBytes(sizeof(uint) * languages.Count);
                    span   = buffer;
                    optionsLineRecord.options[i].pointers =
                        MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                    optionsLineRecord.options[i].names = new Dictionary<string, string>();

                    offset = (uint)br.BaseStream.Position;

                    for(var j = 0; j < languages.Count; j++)
                    {
                        br.BaseStream.Seek(optionsLineRecord.options[i].pointers[j], SeekOrigin.Begin);
                        buffer = br.ReadBytes((int)optionsLineRecord.options[i].lengths[j]);
                        optionsLineRecord.options[i].names.Add(languages[j], _encoding.GetString(buffer));
                    }

                    br.BaseStream.Seek(offset, SeekOrigin.Begin);

                    _options.Add(optionsLineRecord.options[i]);
                }

                offset = (uint)br.BaseStream.Position;
                break;
            case FileRecordType.If:
                conditionLevel--;

                tabulationChars = new char[conditionLevel];
                for(var i = 0; i < conditionLevel; i++)
                    tabulationChars[i] = '\t';
                tabulation = new string(tabulationChars);

                conditionalRecord = new ConditionalRecord
                {
                    recordType = (FileRecordType)br.ReadUInt32(),
                    length     = br.ReadUInt32()
                };

                offset        = (uint)(br.BaseStream.Position + conditionalRecord.length);
                conditionSb   = new StringBuilder();
                nullAttribute = null;

                conditionSb.Append(tabulation + "if(");
                ParseConditionalExpression(br, offset, conditionSb, ref nullAttribute);
                conditionSb.Append(")");

                _conditions.Add(conditionSb.ToString());

                break;
            case FileRecordType.ElseIf:
                conditionLevel--;

                tabulationChars = new char[conditionLevel];
                for(var i = 0; i < conditionLevel; i++)
                    tabulationChars[i] = '\t';
                tabulation = new string(tabulationChars);

                conditionalRecord = new ConditionalRecord
                {
                    recordType = (FileRecordType)br.ReadUInt32(),
                    length     = br.ReadUInt32()
                };

                offset        = (uint)(br.BaseStream.Position + conditionalRecord.length);
                conditionSb   = new StringBuilder();
                nullAttribute = null;

                conditionSb.Append(tabulation + "else if(");
                ParseConditionalExpression(br, offset, conditionSb, ref nullAttribute);
                conditionSb.Append(")");

                _conditions.Add(conditionSb.ToString());

                break;
            case FileRecordType.Else:
                tabulationChars = new char[conditionLevel - 1];
                for(var i = 0; i < conditionLevel - 1; i++)
                    tabulationChars[i] = '\t';
                tabulation = new string(tabulationChars);

                _conditions.Add(tabulation             + "else");
                offset = (uint)(br.BaseStream.Position + Marshal.SizeOf<ConditionalEndRecord>());

                break;
            case FileRecordType.EndIf:
                conditionLevel++;
                _conditions.Add(tabulation             + "endif()" + Environment.NewLine);
                offset = (uint)(br.BaseStream.Position + Marshal.SizeOf<ConditionalEndRecord>());

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}