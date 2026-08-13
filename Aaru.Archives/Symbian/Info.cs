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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Spectre.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Archives;

public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public bool Identify(IFilter filter)
    {
        if(filter.DataForkLength < Marshal.SizeOf<SymbianHeader>()) return false;

        Stream stream = filter.GetDataForkStream();

        var hdr = new byte[Marshal.SizeOf<SymbianHeader>()];

        stream.EnsureRead(hdr, 0, hdr.Length);

        SymbianHeader header = Marshal.ByteArrayToStructureLittleEndian<SymbianHeader>(hdr);

        if(header.uid1 == SYMBIAN9_MAGIC) return true;

        if(header.uid3 != SYMBIAN_MAGIC) return false;

        return header.uid2 is EPOC_MAGIC or EPOC6_MAGIC;
    }

    public void GetInformation(IFilter filter, Encoding encoding, out string information)
    {
        information = "";
        var    description  = new StringBuilder();
        var    languages    = new List<string>();
        var    capabilities = new Dictionary<uint, uint>();
        Stream stream       = filter.GetDataForkStream();

        if(stream.Length < Marshal.SizeOf<SymbianHeader>()) return;

        var buffer = new byte[Marshal.SizeOf<SymbianHeader>()];

        stream.Seek(0, SeekOrigin.Begin);
        stream.EnsureRead(buffer, 0, buffer.Length);
        SymbianHeader sh = Marshal.ByteArrayToStructureLittleEndian<SymbianHeader>(buffer);

        AaruLogging.Debug(MODULE_NAME, "[navy]sh.uid1[/] = [teal]{0}[/]",         sh.uid1);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.uid2[/] = [teal]{0}[/]",         sh.uid2);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.uid3[/] = [teal]{0}[/]",         sh.uid3);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.uid4[/] = [teal]{0}[/]",         sh.uid4);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.crc16[/] = [teal]{0}[/]",        sh.crc16);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.languages[/] = [teal]{0}[/]",    sh.languages);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.files[/] = [teal]{0}[/]",        sh.files);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.requisites[/] = [teal]{0}[/]",   sh.requisites);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.inst_lang[/] = [teal]{0}[/]",    sh.inst_lang);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.inst_files[/] = [teal]{0}[/]",   sh.inst_files);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.inst_drive[/] = [teal]{0}[/]",   sh.inst_drive);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.capabilities[/] = [teal]{0}[/]", sh.capabilities);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.inst_version[/] = [teal]{0}[/]", sh.inst_version);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.options[/] = [teal]{0}[/]",      sh.options);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.type[/] = [teal]{0}[/]",         sh.type);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.major[/] = [teal]{0}[/]",        sh.major);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.minor[/] = [teal]{0}[/]",        sh.minor);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.variant[/] = [teal]{0}[/]",      sh.variant);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.lang_ptr[/] = [teal]{0}[/]",     sh.lang_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.files_ptr[/] = [teal]{0}[/]",    sh.files_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.reqs_ptr[/] = [teal]{0}[/]",     sh.reqs_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.certs_ptr[/] = [teal]{0}[/]",    sh.certs_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.comp_ptr[/] = [teal]{0}[/]",     sh.comp_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.sig_ptr[/] = [teal]{0}[/]",      sh.sig_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.caps_ptr[/] = [teal]{0}[/]",     sh.caps_ptr);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.instspace[/] = [teal]{0}[/]",    sh.instspace);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.maxinsspc[/] = [teal]{0}[/]",    sh.maxinsspc);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.reserved1[/] = [teal]{0}[/]",    sh.reserved1);
        AaruLogging.Debug(MODULE_NAME, "[navy]sh.reserved2[/] = [teal]{0}[/]",    sh.reserved2);

        _release6 = false;

        if(sh.uid1 == SYMBIAN9_MAGIC)
        {
            description.AppendLine(Localization.Symbian_Installation_File);
            description.AppendLine(Localization.Symbian_9_1_or_later);
            description.AppendFormat(Localization.Application_ID_0, sh.uid3).AppendLine();
            _release6 = true;

            description.AppendLine();
            description.AppendLine(Localization.This_file_format_is_not_yet_implemented);

            information = description.ToString();

            return;
        }

        if(sh.uid3 == SYMBIAN_MAGIC)
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

        if(sh.options.HasFlag(SymbianOptions.IsUnicode))
            _encoding = Encoding.Unicode;
        else
            _encoding = encoding ?? Encoding.GetEncoding("windows-1252");

        var br = new BinaryReader(stream);

        // Go to enumerate languages
        br.BaseStream.Seek(sh.lang_ptr, SeekOrigin.Begin);
        for(var i = 0; i < sh.languages; i++) languages.Add(((LanguageCodes)br.ReadUInt16()).ToString("G"));

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
            AaruLogging.Debug(MODULE_NAME,
                              Localization.Found_component_name_for_language_0_at_1_with_a_length_of_2_bytes,
                              languages[i],
                              componentRecord.namesPointers[i],
                              componentRecord.namesLengths[i]);

            br.BaseStream.Seek(componentRecord.namesPointers[i], SeekOrigin.Begin);
            buffer                   = br.ReadBytes((int)componentRecord.namesLengths[i]);
            componentRecord.names[i] = _encoding.GetString(buffer);
        }

        // Go to capabilities (???)
        br.BaseStream.Seek(sh.caps_ptr, SeekOrigin.Begin);

        for(var i = 0; i < sh.capabilities; i++)
        {
            uint cap_Key   = br.ReadUInt32();
            uint cap_Value = br.ReadUInt32();
            capabilities[cap_Key] = cap_Value;
        }

        description.AppendFormat(Localization.UIDs_checksum_0,   sh.uid4).AppendLine();
        description.AppendFormat(Localization.Archive_options_0, sh.options).AppendLine();
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
            if(i > 0) description.Append(", ");
            description.Append($"[italic][rosybrown]{languages[i]}[/][/]");
        }

        description.AppendLine();
        description.AppendLine();

        for(var i = 0; i < languages.Count; i++)
        {
            description.AppendFormat(Localization.Component_name_for_language_with_code_0_1,
                                     languages[i],
                                     Markup.Escape(componentRecord.names[i]))
                       .AppendLine();
        }

        description.AppendLine();

        description.AppendFormat(Localization.File_contains_0_files_pointer_1, sh.files, sh.files_ptr).AppendLine();
        description.AppendFormat(Localization.File_contains_0_requisites,      sh.requisites).AppendLine();

        uint offset = sh.reqs_ptr;

        if(sh.requisites > 0)
        {
            for(var r = 0; r < sh.requisites; r++)
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                var requisiteRecord = new RequisiteRecord
                {
                    uid          = br.ReadUInt32(),
                    majorVersion = br.ReadUInt16(),
                    minorVersion = br.ReadUInt16(),
                    variant      = br.ReadUInt32()
                };

                buffer                       = br.ReadBytes(sizeof(uint) * languages.Count);
                span                         = buffer;
                requisiteRecord.namesLengths = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                buffer                        = br.ReadBytes(sizeof(uint) * languages.Count);
                span                          = buffer;
                requisiteRecord.namesPointers = MemoryMarshal.Cast<byte, uint>(span)[..languages.Count].ToArray();

                description.AppendFormat(Localization.Requisite_0, r).AppendLine();

                description.AppendFormat("\t" + Localization.Required_UID_0_version_1_2,
                                         Markup.Escape(DecodePlatformUid(requisiteRecord.uid)),
                                         requisiteRecord.majorVersion,
                                         requisiteRecord.minorVersion)
                           .AppendLine();

                description.AppendFormat("\t" + Localization.Required_variant_0, requisiteRecord.variant).AppendLine();

                offset = (uint)br.BaseStream.Position;

                for(var i = 0; i < languages.Count; i++)
                {
                    br.BaseStream.Seek(requisiteRecord.namesPointers[i], SeekOrigin.Begin);
                    buffer = br.ReadBytes((int)requisiteRecord.namesLengths[i]);

                    description.AppendFormat("\t" + Localization.Requisite_for_language_0_1,
                                             languages[i],
                                             Markup.Escape(_encoding.GetString(buffer)))
                               .AppendLine();
                }

                description.AppendLine();
            }
        }

//          description.AppendLine(Localization.Capabilities);
//          foreach(KeyValuePair<uint, uint> kvp in capabilities)
//          description.AppendFormat("{0} = {1}", kvp.Key, kvp.Value).AppendLine();

        // Set instance values
        _files      = [];
        _conditions = [];

        uint currentFile = 0;
        offset = sh.files_ptr;
        var conditionLevel = 0;
        _options = [];

        // Get only the options records
        do
        {
            Parse(br, ref offset, ref currentFile, sh.files, languages, ref conditionLevel, true);
        } while(currentFile < sh.files);

        // Get all other records
        offset         = sh.files_ptr;
        currentFile    = 0;
        conditionLevel = 0;

        do
        {
            Parse(br, ref offset, ref currentFile, sh.files, languages, ref conditionLevel, false);
        } while(currentFile < sh.files);

        description.AppendLine();

        // Files appear on .sis in the reverse order they should be processed
        _files.Reverse();

        // Conditions do as well
        _conditions.Reverse();

        if(_files.Any(static t => t.language is null))
        {
            description.AppendLine(Localization.Files_for_all_languages);

            foreach(DecodedFileRecord file in _files.Where(static t => t.language is null))
                description.AppendLine($"[green]{Markup.Escape(file.destinationName)}[/]");

            description.AppendLine();
        }

        foreach(string lang in languages)
        {
            if(_files.All(t => t.language != lang)) continue;

            description.AppendFormat(Localization.Files_for_0_language, lang).AppendLine();

            foreach(DecodedFileRecord file in _files.Where(t => t.language == lang))
                description.AppendLine($"[green]{Markup.Escape(file.destinationName)}[/]");

            description.AppendLine();
        }

        if(_options.Count > 0)
        {
            for(var i = 0; i < _options.Count; i++)
            {
                OptionRecord option = _options[i];

                description.AppendFormat(Localization.Option_0, i + 1).AppendLine();

                foreach(KeyValuePair<string, string> kvp in option.names)
                {
                    description.AppendFormat("\t" + Localization.Name_for_language_0_1,
                                             kvp.Key,
                                             Markup.Escape(kvp.Value))
                               .AppendLine();
                }
            }

            description.AppendLine();
        }

        if(_conditions.Count > 0)
        {
            description.AppendLine(Localization.Conditions);
            foreach(string condition in _conditions) description.AppendLine(Markup.Escape($"[green]{condition}[/]"));
        }

        information = description.ToString();
    }

#endregion
}