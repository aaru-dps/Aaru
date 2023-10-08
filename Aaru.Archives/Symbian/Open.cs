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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Archives;

public sealed partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter filter, Encoding encoding)
    {
        // Already opened!
        if(Opened)
            return ErrorNumber.InvalidArgument;

        var languages = new List<string>();
        _stream = filter.GetDataForkStream();

        if(_stream.Length < Marshal.SizeOf<SymbianHeader>())
            return ErrorNumber.InvalidArgument;

        var buffer = new byte[Marshal.SizeOf<SymbianHeader>()];

        _stream.Seek(0, SeekOrigin.Begin);
        _stream.EnsureRead(buffer, 0, buffer.Length);
        SymbianHeader sh = Marshal.ByteArrayToStructureLittleEndian<SymbianHeader>(buffer);

        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.uid1 = {0}",         sh.uid1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.uid2 = {0}",         sh.uid2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.uid3 = {0}",         sh.uid3);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.uid4 = {0}",         sh.uid4);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.crc16 = {0}",        sh.crc16);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.languages = {0}",    sh.languages);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.files = {0}",        sh.files);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.requisites = {0}",   sh.requisites);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.inst_lang = {0}",    sh.inst_lang);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.inst_files = {0}",   sh.inst_files);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.inst_drive = {0}",   sh.inst_drive);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.capabilities = {0}", sh.capabilities);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.inst_version = {0}", sh.inst_version);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.options = {0}",      sh.options);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.type = {0}",         sh.type);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.major = {0}",        sh.major);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.minor = {0}",        sh.minor);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.variant = {0}",      sh.variant);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.lang_ptr = {0}",     sh.lang_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.files_ptr = {0}",    sh.files_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.reqs_ptr = {0}",     sh.reqs_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.certs_ptr = {0}",    sh.certs_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.comp_ptr = {0}",     sh.comp_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.sig_ptr = {0}",      sh.sig_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.caps_ptr = {0}",     sh.caps_ptr);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.instspace = {0}",    sh.instspace);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.maxinsspc = {0}",    sh.maxinsspc);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.reserved1 = {0}",    sh.reserved1);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sh.reserved2 = {0}",    sh.reserved2);

        _release6 = false;

        if(sh.uid1 == SYMBIAN9_MAGIC)
        {
            AaruConsole.ErrorWriteLine("Symbian Installation Files from Symbian OS 9 or later are not yet supported.");
            return ErrorNumber.NotSupported;
        }

        if(sh.uid3 == SYMBIAN_MAGIC)
        {
            switch(sh.uid2)
            {
                case EPOC_MAGIC:
                    break;
                case EPOC6_MAGIC:
                    _release6 = true;
                    break;
            }
        }

        if(sh.options.HasFlag(SymbianOptions.IsUnicode))
            _encoding = Encoding.Unicode;
        else
            _encoding = encoding ?? Encoding.GetEncoding("windows-1252");

        var br = new BinaryReader(_stream);

        // Go to enumerate languages
        br.BaseStream.Seek(sh.lang_ptr, SeekOrigin.Begin);
        for(var i = 0; i < sh.languages; i++)
            languages.Add(((LanguageCodes)br.ReadUInt16()).ToString("G"));

        _files = new List<DecodedFileRecord>();

        uint currentFile    = 0;
        uint offset         = sh.files_ptr;
        var  conditionLevel = 0;

        do
        {
            Parse(br, ref offset, ref currentFile, sh.files, languages, ref conditionLevel);
        } while(currentFile < sh.files);

        // Files appear on .sis in the reverse order they should be processed
        _files.Reverse();

        List<DecodedFileRecord> filesWithFixedFilenames = new();

        foreach(DecodedFileRecord f in _files)
        {
            DecodedFileRecord file = f;

            if(file.destinationName.Length > 3 && file.destinationName[1] == ':' && file.destinationName[2] == '\\')
                file.destinationName = file.destinationName[3..];

            file.destinationName = file.destinationName.Replace('\\', '/');

            if(file.language != null)
                file.destinationName = $"{file.language}/{file.destinationName}";

            filesWithFixedFilenames.Add(file);
        }

        _files = filesWithFixedFilenames;

        _features = ArchiveSupportedFeature.SupportsFilenames | ArchiveSupportedFeature.SupportsSubdirectories;

        if(_release6 && !sh.options.HasFlag(SymbianOptions.NoCompress))
        {
            _features   |= ArchiveSupportedFeature.SupportsCompression;
            _compressed =  true;
        }

        if(_files.Any(t => t.mime is not null))
            _features |= ArchiveSupportedFeature.SupportsXAttrs;

        Opened = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public void Close()
    {
        // Already closed
        if(!Opened)
            return;

        _stream?.Close();

        _stream = null;
        Opened  = false;
    }

#endregion
}