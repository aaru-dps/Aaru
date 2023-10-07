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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

public partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public bool Identify(IFilter filter)
    {
        if(filter.DataForkLength < Marshal.SizeOf<SymbianHeader>())
            return false;

        Stream stream = filter.GetDataForkStream();

        var hdr = new byte[Marshal.SizeOf<SymbianHeader>()];

        stream.EnsureRead(hdr, 0, hdr.Length);

        SymbianHeader header = Marshal.ByteArrayToStructureLittleEndian<SymbianHeader>(hdr);

        if(header.uid1 == SYMBIAN9_MAGIC)
            return true;

        if(header.uid3 != SYMBIAN_MAGIC)
            return false;

        return header.uid2 is EPOC_MAGIC or EPOC6_MAGIC;
    }

    public void GetInformation(IFilter filter, Encoding encoding, out string information)
    {
        encoding    ??= Encoding.GetEncoding("windows-1252");
        information =   "";
        var    description   = new StringBuilder();
        var    languages     = new List<string>();
        var    capabilities  = new Dictionary<uint, uint>();
        var    en_Pos        = 0;
        var    componentName = "";
        Stream stream        = filter.GetDataForkStream();

        if(stream.Length < Marshal.SizeOf<SymbianHeader>())
            return;

        var buffer = new byte[Marshal.SizeOf<SymbianHeader>()];

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, buffer.Length);
        SymbianHeader sh = Marshal.ByteArrayToStructureLittleEndian<SymbianHeader>(buffer);

        var br = new BinaryReader(stream);

        // Go to enumerate languages
        br.BaseStream.Seek(sh.lang_ptr, SeekOrigin.Begin);
        for(var i = 0; i < sh.languages; i++)
            languages.Add(((LanguageCodes)br.ReadUInt16()).ToString("G"));

        // Go to component record
        br.BaseStream.Seek(sh.comp_ptr, SeekOrigin.Begin);
        var componentRecord = new ComponentRecord
        {
            names = new string[languages.Count]
        };
        buffer = new byte[sizeof(uint) * languages.Count];

        // Read the component string lenghts
        stream.EnsureRead(buffer, 0, buffer.Length);
        ReadOnlySpan<byte> span = buffer;
        componentRecord.namesLengths = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

        // Read the component string pointers
        stream.EnsureRead(buffer, 0, buffer.Length);
        span                          = buffer;
        componentRecord.namesPointers = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

        for(var i = 0; i < sh.languages; i++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "Found component name for language {0} at {1} with a length of {2} bytes",
                                       languages[i], componentRecord.namesPointers[i], componentRecord.namesLengths[i]);

            br.BaseStream.Seek(componentRecord.namesPointers[i], SeekOrigin.Begin);
            buffer                   = br.ReadBytes((int)componentRecord.namesLengths[i]);
            componentRecord.names[i] = encoding.GetString(buffer);
        }

        // Go to capabilities (???)
        br.BaseStream.Seek(sh.caps_ptr, SeekOrigin.Begin);
        for(var i = 0; i < sh.capabilities; i++)
        {
            uint cap_Key   = br.ReadUInt32();
            uint cap_Value = br.ReadUInt32();
            capabilities.Add(cap_Key, cap_Value);
        }

        _release6 = false;

        if(sh.uid1 == SYMBIAN9_MAGIC)
        {
            description.AppendLine(Localization.Symbian_Installation_File);
            description.AppendLine(Localization.Symbian_9_1_or_later);
            description.AppendFormat(Localization.Application_ID_0, sh.uid3).AppendLine();
            _release6 = true;
        }
        else if(sh.uid3 == SYMBIAN_MAGIC)
        {
            description.AppendLine(Localization.Symbian_Installation_File);

            switch(sh.uid2)
            {
                case EPOC_MAGIC:
                    description.AppendLine(Localization.Symbian_3_or_later);
                    break;
                case EPOC6_MAGIC:
                    description.AppendLine(Localization.Symbian_6_or_later);
                    _release6 = true;
                    break;
                default:
                    description.AppendFormat(Localization.Unknown_EPOC_magic_0, sh.uid2).AppendLine();
                    break;
            }

            description.AppendFormat(Localization.Application_ID_0, sh.uid1).AppendLine();
        }

        description.AppendFormat(Localization.UIDs_checksum_0,   sh.uid4).AppendLine();
        description.AppendFormat(Localization.CRC16_of_header_0, sh.crc16).AppendLine();
        description.AppendLine();

        switch(sh.type)
        {
            case SymbianType.Application:
                description.AppendLine(Localization.SIS_contains_an_application);
                break;
        }

        description.AppendFormat(Localization.Component_version_0_1, sh.major, sh.minor).AppendLine();

        description.AppendLine();

        description.AppendFormat(Localization.File_contains_0_languages, sh.languages).AppendLine();

        for(var i = 0; i < languages.Count; i++)
        {
            if(i > 0)
                description.Append(", ");
            description.Append($"{languages[i]}");
        }

        description.AppendLine();
        description.AppendLine();

        for(var i = 0; i < languages.Count; i++)
        {
            description.AppendFormat(Localization.Component_name_for_language_with_code_0_1, languages[i],
                                     componentRecord.names[i]).
                        AppendLine();
        }

        description.AppendLine();

        description.AppendFormat(Localization.File_contains_0_files_pointer_1, sh.files, sh.files_ptr).AppendLine();
        description.AppendFormat(Localization.File_contains_0_requisites,      sh.requisites).AppendLine();

//          description.AppendLine(Localization.Capabilities);
//          foreach(KeyValuePair<uint, uint> kvp in capabilities)
//          description.AppendFormat("{0} = {1}", kvp.Key, kvp.Value).AppendLine();

        // Set instance values
        _encoding = encoding;
        _files    = new List<DecodedFileRecord>();

        uint currentFile = 0;
        uint offset      = sh.files_ptr;

        do
        {
            Parse(br, ref offset, ref currentFile, sh.files);
        } while(currentFile < sh.files);

        // Files appear on .sis in the reverse order they should be processed
        _files.Reverse();

        if(_files.Any(t => t.language is null))
        {
            description.AppendLine(Localization.Files_for_all_languages);
            foreach(DecodedFileRecord file in _files.Where(t => t.language is null))
                description.AppendLine($"{file.destinationName}");
            description.AppendLine();
        }

        foreach(string lang in languages)
        {
            if(_files.All(t => t.language != lang))
                continue;

            description.AppendFormat(Localization.Files_for_0_language, lang).AppendLine();
            foreach(DecodedFileRecord file in _files.Where(t => t.language == lang))
                description.AppendLine($"{file.destinationName}");
        }

        information = description.ToString();
    }

#endregion
}