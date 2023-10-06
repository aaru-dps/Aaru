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

// Information from http://www.thoukydides.webspace.virginmedia.com/software/psifs/sis.html
// TODO: Implement support for disc images
/*

namespace Aaru.Plugins
{
    class SymbianIS : Plugin
    {
        // Magics
        private const uint SymbianMagic  = 0x10000419;
        private const uint EPOCMagic     = 0x1000006D;
        private const uint EPOC6Magic    = 0x10003A12;
        private const uint Symbian9Magic = 0x10201A7A;

        // Options
        private const ushort IsUnicode       = 0x0001;
        private const ushort IsDistributable = 0x0002;
        private const ushort NoCompress      = 0x0008;
        private const ushort ShutdownApps    = 0x0010;

        // Types
        private const ushort SISApp     = 0x0000; // Application
        private const ushort SISSystem  = 0x0001; // System component (library)
        private const ushort SISOption  = 0x0002; // Optional component
        private const ushort SISConfig  = 0x0003; // Configures an application
        private const ushort SISPatch   = 0x0004; // Patch
        private const ushort SISUpgrade = 0x0005; // Upgrade

        private enum LanguageCodes
        {
            Test,
            EN,
            FR,
            GE,
            SP,
            IT,
            SW,
            DA,
            NO,
            FI,
            AM,
            SF,
            SG,
            PO,
            TU,
            IC,
            RU,
            HU,
            DU,
            BL,
            AU,
            BF,
            AS,
            NZ,
            IF,
            CS,
            SK,
            PL,
            SL,
            TC,
            HK,
            ZH,
            JA,
            TH,
            AF,
            SQ,
            AH,
            AR,
            HY,
            TL,
            BE,
            BN,
            BG,
            MY,
            CA,
            HR,
            CE,
            IE,
            ZA,
            ET,
            FA,
            CF,
            GD,
            KA,
            EL,
            CG,
            GU,
            HE,
            HI,
            IN,
            GA,
            SZ,
            KN,
            KK,
            KM,
            KO,
            LO,
            LV,
            LT,
            MK,
            MS,
            ML,
            MR,
            MO,
            MN,
            NN,
            BP,
            PA,
            RO,
            SR,
            SI,
            SO,
            OS,
            LS,
            SH,
            FS,
            TA,
            TE,
            BO,
            TI,
            CT,
            TK,
            UK,
            UR,
            VI,
            CY,
            ZU
        };

        public SymbianIS()
        {
            base.Name = "Symbian Installation File Plugin";
            base.PluginUUID = new Guid("0ec84ec7-eae6-4196-83fe-943b3fe48dbd");
        }

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

        private struct SymbianHeader
        {
            public uint uid1;	        // Application UID before SymbianOS 9, magic after
            public uint uid2;	        // EPOC release magic before SOS 9, NULLs after
            public uint uid3;	        // Application UID after SOS 9, magic before
            public uint uid4;	        // Checksum of UIDs 1 to 3
            public ushort crc16;        // CRC16 of all header
            public ushort languages;    // Number of languages
            public ushort files;        // Number of files
            public ushort requisites;   // Number of requisites
            public ushort inst_lang;    // Installed language (only residual SIS)
            public ushort inst_files;   // Installed files (only residual SIS)
            public ushort inst_drive;   // Installed drive (only residual SIS), NULL or 0x0021
            public ushort capabilities; // Number of capabilities
            public uint inst_version; // Version of Symbian Installer required
            public ushort options;      // Option flags
            public ushort type;         // Type
            public ushort major;        // Major version of application
            public ushort minor;        // Minor version of application
            public uint variant;      // Variant when SIS is a prerequisite for other SISs
            public uint lang_ptr;     // Pointer to language records
            public uint files_ptr;    // Pointer to file records
            public uint reqs_ptr;     // Pointer to requisite records
            public uint certs_ptr;    // Pointer to certificate records
            public uint comp_ptr;     // Pointer to component name record
            // From EPOC Release 6
            public uint sig_ptr;      // Pointer to signature record
            public uint caps_ptr;     // Pointer to capability records
            public uint instspace;    // Installed space (only residual SIS)
            public uint maxinsspc;    // Space required
            public ulong reserved1;    // Reserved
            public ulong reserved2;    // Reserved
        }
    }
}

*/

