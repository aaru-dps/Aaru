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

namespace Aaru.Archives;

public partial class Symbian
{
#region IArchive Members

    /// <inheritdoc />
    public bool Identify(Stream stream) => throw new NotImplementedException();

#endregion

    /*
        public override bool Identify(FileStream fileStream, long offset)
        {
            uint uid1, uid2, uid3;
            BinaryReader br = new BinaryReader(fileStream);

            br.BaseStream.Seek(0 + offset, SeekOrigin.Begin);

            uid1 = br.Readuint();
            uid2 = br.Readuint();
            uid3 = br.Readuint();

            if(uid1 == Symbian9Magic)
                return true;
            else if(uid3 == SymbianMagic)
            {
                if(uid2 == EPOCMagic || uid2 == EPOC6Magic)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public override void GetInformation (FileStream fileStream, long offset, out string information)
        {
            information = "";
            StringBuilder description = new StringBuilder();
            List<string> languages = new List<string>();
            Dictionary<uint, uint> capabilities = new Dictionary<uint, uint>();
            int ENpos = 0;
            uint comp_len;
            uint comp_name_ptr;
            byte[] ComponentName_b;
            string ComponentName = "";

            SymbianHeader sh = new SymbianHeader();
            BinaryReader br = new BinaryReader(fileStream);

            br.BaseStream.Seek(0 + offset, SeekOrigin.Begin);

            sh.uid1 = br.Readuint();
            sh.uid2 = br.Readuint();
            sh.uid3 = br.Readuint();
            sh.uid4 = br.Readuint();
            sh.crc16 = br.Readushort();
            sh.languages = br.Readushort();
            sh.files = br.Readushort();
            sh.requisites = br.Readushort();
            sh.inst_lang = br.Readushort();
            sh.inst_files = br.Readushort();
            sh.inst_drive = br.Readushort();
            sh.capabilities = br.Readushort();
            sh.inst_version = br.Readuint();
            sh.options = br.Readushort();
            sh.type = br.Readushort();
            sh.major = br.Readushort();
            sh.minor = br.Readushort();
            sh.variant = br.Readuint();
            sh.lang_ptr = br.Readuint();
            sh.files_ptr = br.Readuint();
            sh.reqs_ptr = br.Readuint();
            sh.certs_ptr = br.Readuint();
            sh.comp_ptr = br.Readuint();
            sh.sig_ptr = br.Readuint();
            sh.caps_ptr = br.Readuint();
            sh.instspace = br.Readuint();
            sh.maxinsspc = br.Readuint();
            sh.reserved1 = br.Readulong();
            sh.reserved2 = br.Readulong();

            // Go to enumerate languages
            br.BaseStream.Seek(sh.lang_ptr + offset, SeekOrigin.Begin);
            for(int i = 0; i < sh.languages; i++)
            {
                ushort language = br.Readushort();
                if(language == 0x0001)
                    ENpos = i;
                languages.Add(((LanguageCodes)language).ToString("G"));
            }

            // Go to component record
            br.BaseStream.Seek(sh.comp_ptr + offset, SeekOrigin.Begin);
            for(int i = 0; i < sh.languages; i++)
            {
                comp_len = br.Readuint();
                comp_name_ptr = br.Readuint();
                if(i == ENpos)
                {
                    br.BaseStream.Seek(comp_name_ptr + offset, SeekOrigin.Begin);
                    ComponentName_b = new byte[comp_len];
                    ComponentName_b = br.ReadBytes((int)comp_len);
                    ComponentName = Encoding.ASCII.GetString(ComponentName_b);
                    break;
                }
            }

            // Go to capabilities (???)
            br.BaseStream.Seek(sh.caps_ptr + offset, SeekOrigin.Begin);
            for(int i = 0; i < sh.capabilities; i++)
            {
                uint cap_key = br.Readuint();
                uint cap_value = br.Readuint();
                capabilities.Add(cap_key, cap_value);
            }

            if(sh.uid1 == Symbian9Magic)
            {
                description.AppendLine("Symbian Installation File");
                description.AppendLine("SymbianOS 9.1 or later");
                description.AppendFormat("Application ID: 0x{0:X8}", sh.uid3).AppendLine();
                description.AppendFormat("UIDs checksum: 0x{0:X8}", sh.uid4).AppendLine();
            }
            else if(sh.uid3 == SymbianMagic)
            {
                description.AppendLine("Symbian Installation File");

                if(sh.uid2 == EPOCMagic)
                    description.AppendLine("SymbianOS 3 or later");
                else if (sh.uid2 == EPOC6Magic)
                    description.AppendLine("SymbianOS 6 or later");
                else
                    description.AppendFormat("Unknown EPOC magic 0x{0:X8}", sh.uid2).AppendLine();

                description.AppendFormat("Application ID: 0x{0:X8}", sh.uid1).AppendLine();
                description.AppendFormat("UIDs checksum: 0x{0:X8}", sh.uid4).AppendLine();
                description.AppendFormat("CRC16 of header: 0x{0:X4}", sh.crc16).AppendLine();
                description.AppendLine();

                switch(sh.type)
                {
                    case SISApp:
                        description.AppendLine("SIS contains an application");
                        break;
                }

                description.AppendFormat("Component: {0} v{1}.{2}", ComponentName, sh.major, sh.minor).AppendLine();

                description.AppendFormat("File contains {0} languages:", sh.languages).AppendLine();
                for(int i = 0; i < languages.Count; i++)
                {
                    if(i>0)
                        description.Append(", ");
                    description.AppendFormat("{0}", languages[i]);
                }
                description.AppendLine();

                description.AppendFormat("File contains {0} files (pointer: {1})", sh.files, sh.files_ptr).AppendLine();
                description.AppendFormat("File contains {0} requisites", sh.requisites).AppendLine();
//				description.AppendLine("Capabilities:");
//				foreach(KeyValuePair<uint, uint> kvp in capabilities)
//					description.AppendFormat("{0} = {1}", kvp.Key, kvp.Value).AppendLine();
            }

            information = description.ToString();
        }


*/
}