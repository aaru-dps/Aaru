/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Symbian.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Symbian installer (.sis) packages and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.IO;
using System.Text;
using DiscImageChef;
using System.Collections.Generic;

// Information from http://www.thoukydides.webspace.virginmedia.com/software/psifs/sis.html
// TODO: Implement support for disc images
/*

namespace DiscImageChef.Plugins
{
	class SymbianIS : Plugin
	{
		// Magics
		private const UInt32 SymbianMagic  = 0x10000419;
		private const UInt32 EPOCMagic     = 0x1000006D;
		private const UInt32 EPOC6Magic    = 0x10003A12;
		private const UInt32 Symbian9Magic = 0x10201A7A;
		
		// Options
		private const UInt16 IsUnicode       = 0x0001;
		private const UInt16 IsDistributable = 0x0002;
		private const UInt16 NoCompress      = 0x0008;
		private const UInt16 ShutdownApps    = 0x0010;
		
		// Types
		private const UInt16 SISApp     = 0x0000; // Application
		private const UInt16 SISSystem  = 0x0001; // System component (library)
		private const UInt16 SISOption  = 0x0002; // Optional component
		private const UInt16 SISConfig  = 0x0003; // Configures an application
		private const UInt16 SISPatch   = 0x0004; // Patch
		private const UInt16 SISUpgrade = 0x0005; // Upgrade
		
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
		
		public SymbianIS(PluginBase Core)
        {
            base.Name = "Symbian Installation File Plugin";
            base.PluginUUID = new Guid("0ec84ec7-eae6-4196-83fe-943b3fe48dbd");
        }
		
		public override bool Identify(FileStream fileStream, long offset)
		{
			UInt32 uid1, uid2, uid3;
			BinaryReader br = new BinaryReader(fileStream);
			
            br.BaseStream.Seek(0 + offset, SeekOrigin.Begin);

			uid1 = br.ReadUInt32();
			uid2 = br.ReadUInt32();
			uid3 = br.ReadUInt32();
			
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
			Dictionary<UInt32, UInt32> capabilities = new Dictionary<UInt32, UInt32>();
			int ENpos = 0;
			UInt32 comp_len;
			UInt32 comp_name_ptr;
			byte[] ComponentName_b;
			string ComponentName = "";
			
			SymbianHeader sh = new SymbianHeader();
			BinaryReader br = new BinaryReader(fileStream);
			
            br.BaseStream.Seek(0 + offset, SeekOrigin.Begin);
			
			sh.uid1 = br.ReadUInt32();
			sh.uid2 = br.ReadUInt32();
			sh.uid3 = br.ReadUInt32();
			sh.uid4 = br.ReadUInt32();
			sh.crc16 = br.ReadUInt16();
			sh.languages = br.ReadUInt16();
			sh.files = br.ReadUInt16();
			sh.requisites = br.ReadUInt16();
			sh.inst_lang = br.ReadUInt16();
			sh.inst_files = br.ReadUInt16();
			sh.inst_drive = br.ReadUInt16();
			sh.capabilities = br.ReadUInt16();
			sh.inst_version = br.ReadUInt32();
			sh.options = br.ReadUInt16();
			sh.type = br.ReadUInt16();
			sh.major = br.ReadUInt16();
			sh.minor = br.ReadUInt16();
			sh.variant = br.ReadUInt32();
			sh.lang_ptr = br.ReadUInt32();
			sh.files_ptr = br.ReadUInt32();
			sh.reqs_ptr = br.ReadUInt32();
			sh.certs_ptr = br.ReadUInt32();
			sh.comp_ptr = br.ReadUInt32();
			sh.sig_ptr = br.ReadUInt32();
			sh.caps_ptr = br.ReadUInt32();
			sh.instspace = br.ReadUInt32();
			sh.maxinsspc = br.ReadUInt32();
			sh.reserved1 = br.ReadUInt64();
			sh.reserved2 = br.ReadUInt64();
			
			// Go to enumerate languages
			br.BaseStream.Seek(sh.lang_ptr + offset, SeekOrigin.Begin);
			for(int i = 0; i < sh.languages; i++)
			{
				UInt16 language = br.ReadUInt16();
				if(language == 0x0001)
					ENpos = i;
				languages.Add(((LanguageCodes)language).ToString("G"));
			}
			
			// Go to component record
			br.BaseStream.Seek(sh.comp_ptr + offset, SeekOrigin.Begin);
			for(int i = 0; i < sh.languages; i++)
			{
				comp_len = br.ReadUInt32();
				comp_name_ptr = br.ReadUInt32();
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
				UInt32 cap_key = br.ReadUInt32();
				UInt32 cap_value = br.ReadUInt32();
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
//				foreach(KeyValuePair<UInt32, UInt32> kvp in capabilities)
//					description.AppendFormat("{0} = {1}", kvp.Key, kvp.Value).AppendLine();
			}

            information = description.ToString();
		}
		
		private struct SymbianHeader
		{
			public UInt32 uid1;	        // Application UID before SymbianOS 9, magic after
			public UInt32 uid2;	        // EPOC release magic before SOS 9, NULLs after
			public UInt32 uid3;	        // Application UID after SOS 9, magic before
			public UInt32 uid4;	        // Checksum of UIDs 1 to 3
			public UInt16 crc16;        // CRC16 of all header
			public UInt16 languages;    // Number of languages
			public UInt16 files;        // Number of files
			public UInt16 requisites;   // Number of requisites
			public UInt16 inst_lang;    // Installed language (only residual SIS)
			public UInt16 inst_files;   // Installed files (only residual SIS)
			public UInt16 inst_drive;   // Installed drive (only residual SIS), NULL or 0x0021
			public UInt16 capabilities; // Number of capabilities
			public UInt32 inst_version; // Version of Symbian Installer required
			public UInt16 options;      // Option flags
			public UInt16 type;         // Type
			public UInt16 major;        // Major version of application
			public UInt16 minor;        // Minor version of application
			public UInt32 variant;      // Variant when SIS is a prerequisite for other SISs
			public UInt32 lang_ptr;     // Pointer to language records
			public UInt32 files_ptr;    // Pointer to file records
			public UInt32 reqs_ptr;     // Pointer to requisite records
			public UInt32 certs_ptr;    // Pointer to certificate records
			public UInt32 comp_ptr;     // Pointer to component name record
			// From EPOC Release 6
			public UInt32 sig_ptr;      // Pointer to signature record
			public UInt32 caps_ptr;     // Pointer to capability records
			public UInt32 instspace;    // Installed space (only residual SIS)
			public UInt32 maxinsspc;    // Space required
			public UInt64 reserved1;    // Reserved
			public UInt64 reserved2;    // Reserved
		}
	}
}

*/