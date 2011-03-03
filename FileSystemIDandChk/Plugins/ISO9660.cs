using System;
using System.IO;
using System.Text;
using System.Globalization;
using FileSystemIDandChk;

// This is coded following ECMA-119.
// TODO: Differentiate ISO Level 1, 2, 3 and ISO 9660:1999
// TODO: Apple extensiones, requires XA or advance RR interpretation.

namespace FileSystemIDandChk.Plugins
{
    class ISO9660Plugin : Plugin
    {
        public ISO9660Plugin(PluginBase Core)
        {
            base.Name = "ISO9660 Filesystem";
            base.PluginUUID = new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
        }

        private struct DecodedVolumeDescriptor
        {
            public string SystemIdentifier;
            public string VolumeIdentifier;
            public string VolumeSetIdentifier;
            public string PublisherIdentifier;
            public string DataPreparerIdentifier;
            public string ApplicationIdentifier;
            public DateTime CreationTime;
            public bool HasModificationTime;
            public DateTime ModificationTime;
            public bool HasExpirationTime;
            public DateTime ExpirationTime;
            public bool HasEffectiveTime;
            public DateTime EffectiveTime;
        }

        public override bool Identify(FileStream fileStream, long offset)
        {
			byte VDType;
			
			// ISO9660 Primary Volume Descriptor starts at 32768, so that's minimal size.
            if (fileStream.Length < 32768)
                return false;
			
			// Seek to Volume Descriptor
            fileStream.Seek(32768 + offset, SeekOrigin.Begin);

            VDType = (byte)fileStream.ReadByte();
			byte[] VDMagic = new byte[5];

            if (VDType == 255) // Supposedly we are in the PVD.
	            return false;

            if (fileStream.Read(VDMagic, 0, 5) != 5)
	            return false; // Something bad happened

            if (Encoding.ASCII.GetString(VDMagic) != "CD001") // Recognized, it is an ISO9660, now check for rest of data.
                return false;
			
			return true;
		}
		
		public override void GetInformation (FileStream fileStream, long offset, out string information)
		{
            information = "";
            StringBuilder ISOMetadata = new StringBuilder();
            bool Joliet = false;
            bool Bootable = false;
            bool RockRidge = false;
            byte VDType;                            // Volume Descriptor Type, should be 1 or 2.
            byte[] VDMagic = new byte[5];           // Volume Descriptor magic "CD001"
            byte[] VDSysId = new byte[32];          // System Identifier
            byte[] VDVolId = new byte[32];          // Volume Identifier
            byte[] VDVolSetId = new byte[128];      // Volume Set Identifier
            byte[] VDPubId = new byte[128];         // Publisher Identifier
            byte[] VDDataPrepId = new byte[128];    // Data Preparer Identifier
            byte[] VDAppId = new byte[128];         // Application Identifier
            byte[] VCTime = new byte[17];           // Volume Creation Date and Time
            byte[] VMTime = new byte[17];           // Volume Modification Date and Time
            byte[] VXTime = new byte[17];           // Volume Expiration Date and Time
            byte[] VETime = new byte[17];           // Volume Effective Date and Time

            byte[] JolietMagic = new byte[3];
            byte[] JolietSysId = new byte[32];          // System Identifier
            byte[] JolietVolId = new byte[32];          // Volume Identifier
            byte[] JolietVolSetId = new byte[128];      // Volume Set Identifier
            byte[] JolietPubId = new byte[128];         // Publisher Identifier
            byte[] JolietDataPrepId = new byte[128];    // Data Preparer Identifier
            byte[] JolietAppId = new byte[128];         // Application Identifier
            byte[] JolietCTime = new byte[17];           // Volume Creation Date and Time
            byte[] JolietMTime = new byte[17];           // Volume Modification Date and Time
            byte[] JolietXTime = new byte[17];           // Volume Expiration Date and Time
            byte[] JolietETime = new byte[17];           // Volume Effective Date and Time

            byte[] BootSysId = new byte[32];
            string BootSpec = "";

            byte[] VDPathTableStart = new byte[4];
            byte[] RootDirectoryLocation = new byte[4];

            fileStream.Seek(0 + offset, SeekOrigin.Begin);

            // ISO9660 Primary Volume Descriptor starts at 32768, so that's minimal size.
            if (fileStream.Length < 32768)
                return;

            int counter = 0;

            while (true)
            {
                // Seek to Volume Descriptor
                fileStream.Seek(32768+(2048*counter) + offset, SeekOrigin.Begin);

                VDType = (byte)fileStream.ReadByte();

                if (VDType == 255) // Supposedly we are in the PVD.
                {
                    if (counter == 0)
                        return;
                    break;
                }

                if (fileStream.Read(VDMagic, 0, 5) != 5)
                {
                    if (counter == 0)
                        return; // Something bad happened
                    break;
                }

                if (Encoding.ASCII.GetString(VDMagic) != "CD001") // Recognized, it is an ISO9660, now check for rest of data.
                {
                    if (counter == 0)
                        return;
                    break;
                }

                switch(VDType)
                {
                    case 0: // TODO
                        {
                            Bootable = true;
                            BootSpec = "Unknown";

                            // Seek to boot system identifier
                            fileStream.Seek(32775 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(BootSysId, 0, 32) != 32)
                                break; // Something bad happened

                            if (Encoding.ASCII.GetString(BootSysId).Substring(0, 23) == "EL TORITO SPECIFICATION")
                                BootSpec = "El Torito";

                            break;
                        }
                    case 1:
                        {
                            // Seek to first identifiers
                            fileStream.Seek(32776 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(VDSysId, 0, 32) != 32)
                                break; // Something bad happened
                            if (fileStream.Read(VDVolId, 0, 32) != 32)
                                break; // Something bad happened

                            // Get path table start
                            fileStream.Seek(32908 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(VDPathTableStart, 0, 4) != 4)
                                break; // Something bad happened

                            // Seek to next identifiers
                            fileStream.Seek(32958 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(VDVolSetId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(VDPubId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(VDDataPrepId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(VDAppId, 0, 128) != 128)
                                break; // Something bad happened

                            // Seek to dates
                            fileStream.Seek(33581 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(VCTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(VMTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(VXTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(VETime, 0, 17) != 17)
                                break; // Something bad happened

                            break;
                        }
                    case 2:
                        {
                            // Check if this is Joliet
                            fileStream.Seek(32856 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(JolietMagic, 0, 3) != 3)
                            {
                                break; // Something bad happened
                            }

                            if (JolietMagic[0] == '%' && JolietMagic[1] == '/')
                            {
                                if (JolietMagic[2] == '@' || JolietMagic[2] == 'C' || JolietMagic[2] == 'E')
                                {
                                    Joliet = true;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                                break;

                            // Seek to first identifiers
                            fileStream.Seek(32776 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(JolietSysId, 0, 32) != 32)
                                break; // Something bad happened
                            if (fileStream.Read(JolietVolId, 0, 32) != 32)
                                break; // Something bad happened

                            // Seek to next identifiers
                            fileStream.Seek(32958 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(JolietVolSetId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(JolietPubId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(JolietDataPrepId, 0, 128) != 128)
                                break; // Something bad happened
                            if (fileStream.Read(JolietAppId, 0, 128) != 128)
                                break; // Something bad happened

                            // Seek to dates
                            fileStream.Seek(33581 + (2048 * counter) + offset, SeekOrigin.Begin);

                            if (fileStream.Read(JolietCTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(JolietMTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(JolietXTime, 0, 17) != 17)
                                break; // Something bad happened
                            if (fileStream.Read(JolietETime, 0, 17) != 17)
                                break; // Something bad happened

                            break;
                        }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();
            if(!Joliet)
                decodedVD = DecodeVolumeDescriptor(VDSysId, VDVolId, VDVolSetId, VDPubId, VDDataPrepId, VDAppId, VCTime, VMTime, VXTime, VETime);
            else
                decodedVD = DecodeJolietDescriptor(JolietSysId, JolietVolId, JolietVolSetId, JolietPubId, JolietDataPrepId, JolietAppId, JolietCTime, JolietMTime, JolietXTime, JolietETime);

            int i = BitConverter.ToInt32(VDPathTableStart, 0);

            fileStream.Seek((i * 2048)+2 + offset, SeekOrigin.Begin); // Seek to first path table location field

            // Check for Rock Ridge
            if (fileStream.Read(RootDirectoryLocation, 0, 4) == 4)
            {
                fileStream.Seek((BitConverter.ToInt32(RootDirectoryLocation,0) * 2048)+34 + offset, SeekOrigin.Begin); // Seek to root directory, first entry, system use field

                byte[] SUSPMagic = new byte[2];
                byte[] RRMagic = new byte[2];

                fileStream.Read(SUSPMagic, 0, 2);
                if (Encoding.ASCII.GetString(SUSPMagic) == "SP")
                {
                    fileStream.Seek(5, SeekOrigin.Current); // Seek for rock ridge magic
                    fileStream.Read(RRMagic, 0, 2);
                    if (Encoding.ASCII.GetString(RRMagic) == "RR")
                    {
                        RockRidge = true;
                    }
                }
            }

            #region SEGA IP.BIN Read and decoding
            
            bool SegaCD = false;
            bool Saturn = false;
            bool Dreamcast = false;
            StringBuilder IPBinInformation = new StringBuilder();

            byte[] SegaHardwareID = new byte[16];
            fileStream.Seek(0 + offset, SeekOrigin.Begin); // Seek to start (again)
            fileStream.Read(SegaHardwareID, 0, 16);

            switch (Encoding.ASCII.GetString(SegaHardwareID))
            {
                case "SEGADISCSYSTEM  ":
                case "SEGADATADISC    ":
                case "SEGAOS          ":
                    {
                        SegaCD = true; // Ok, this contains SegaCD IP.BIN

                        IPBinInformation.AppendLine("--------------------------------");
                        IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
                        IPBinInformation.AppendLine("--------------------------------");

                        // Definitions following
                        byte[] volume_name = new byte[11];      // Varies
                        byte[] spare_space1 = new byte[1];         // 0x00
                        byte[] volume_version = new byte[2];       // Volume version in BCD. <100 = Prerelease.
                        byte[] volume_type = new byte[2];          // Bit 0 = 1 => CD-ROM. Rest should be 0.
                        byte[] system_name = new byte[11];      // Unknown, varies!
                        byte[] spare_space2 = new byte[1];         // 0x00
                        byte[] system_version = new byte[2];       // Should be 1
                        byte[] spare_space3 = new byte[2];         // 0x0000
                        byte[] ip_address = new byte[4];	      // Initial program address
                        byte[] ip_loadsize = new byte[4];          // Load size of initial program
                        byte[] ip_entry_address = new byte[4];     // Initial program entry address
                        byte[] ip_work_ram_size = new byte[4];     // Initial program work RAM size in bytes
                        byte[] sp_address = new byte[4];	      // System program address
                        byte[] sp_loadsize = new byte[4];          // Load size of system program
                        byte[] sp_entry_address = new byte[4];     // System program entry address
                        byte[] sp_work_ram_size = new byte[4];     // System program work RAM size in bytes
                        byte[] release_date = new byte[8];      // MMDDYYYY
                        byte[] unknown1 = new byte[7];          // Seems to be all 0x20s
                        byte[] spare_space4 = new byte[1];         // 0x00 ?
                        byte[] system_reserved = new byte[160]; // System Reserved Area
                        byte[] hardware_id = new byte[16];      // Hardware ID
                        byte[] copyright = new byte[3];         // "(C)" -- Can be the developer code directly!, if that is the code release date will be displaced
                        byte[] developer_code = new byte[5];    // "SEGA" or "T-xx"
                        byte[] unknown2 = new byte[1];             // Seems to be part of developer code, need to get a SEGA disc to check
                        byte[] release_date2 = new byte[8];     // Another release date, this with month in letters?
                        byte[] domestic_title = new byte[48];   // Domestic version of the game title
                        byte[] overseas_title = new byte[48];   // Overseas version of the game title
                        byte[] application_type = new byte[2];  // Application type
                        byte[] space_space5 = new byte[1];         // 0x20
                        byte[] product_code = new byte[13];      // Official product code
                        byte[] peripherals = new byte[16];      // Supported peripherals, see above
                        byte[] spare_space6 = new byte[16];     // 0x20
                        byte[] spare_space7 = new byte[64];     // Inside here should be modem information, but I need to get a modem-enabled game
                        byte[] region_codes = new byte[16];     // Region codes, space-filled
                        //Reading all data
                        fileStream.Read(volume_name, 0, 11);      // Varies
                        fileStream.Read(spare_space1, 0, 1);         // 0x00
                        fileStream.Read(volume_version, 0, 2);       // Volume version in BCD. <100 = Prerelease.
                        fileStream.Read(volume_type, 0, 2);          // Bit 0 = 1 => CD-ROM. Rest should be 0.
                        fileStream.Read(system_name, 0, 11);      // Unknown, varies!
                        fileStream.Read(spare_space2, 0, 1);         // 0x00
                        fileStream.Read(system_version, 0, 2);       // Should be 1
                        fileStream.Read(spare_space3, 0, 2);         // 0x0000
                        fileStream.Read(ip_address, 0, 4);	      // Initial program address
                        fileStream.Read(ip_loadsize, 0, 4);          // Load size of initial program
                        fileStream.Read(ip_entry_address, 0, 4);     // Initial program entry address
                        fileStream.Read(ip_work_ram_size, 0, 4);     // Initial program work RAM size in bytes
                        fileStream.Read(sp_address, 0, 4);	      // System program address
                        fileStream.Read(sp_loadsize, 0, 4);          // Load size of system program
                        fileStream.Read(sp_entry_address, 0, 4);     // System program entry address
                        fileStream.Read(sp_work_ram_size, 0, 4);     // System program work RAM size in bytes
                        fileStream.Read(release_date, 0, 8);      // MMDDYYYY
                        fileStream.Read(unknown1, 0, 7);          // Seems to be all 0x20s
                        fileStream.Read(spare_space4, 0, 1);         // 0x00 ?
                        fileStream.Read(system_reserved, 0, 160); // System Reserved Area
                        fileStream.Read(hardware_id, 0, 16);      // Hardware ID
                        fileStream.Read(copyright, 0, 3);         // "(C)" -- Can be the developer code directly!, if that is the code release date will be displaced
                        if (Encoding.ASCII.GetString(copyright) != "(C)")
                            fileStream.Seek(-3, SeekOrigin.Current);
                        fileStream.Read(developer_code, 0, 5);    // "SEGA" or "T-xx"
                        if (Encoding.ASCII.GetString(copyright) != "(C)")
                            fileStream.Seek(1, SeekOrigin.Current);
                        fileStream.Read(release_date2, 0, 8);     // Another release date, this with month in letters?
                        if (Encoding.ASCII.GetString(copyright) != "(C)")
                            fileStream.Seek(2, SeekOrigin.Current);
                        fileStream.Read(domestic_title, 0, 48);   // Domestic version of the game title
                        fileStream.Read(overseas_title, 0, 48);   // Overseas version of the game title
                        fileStream.Read(application_type, 0, 2);  // Application type
                        fileStream.Read(space_space5, 0, 1);         // 0x20
                        fileStream.Read(product_code, 0, 13);      // Official product code
                        fileStream.Read(peripherals, 0, 16);      // Supported peripherals, see above
                        fileStream.Read(spare_space6, 0, 16);     // 0x20
                        fileStream.Read(spare_space7, 0, 64);     // Inside here should be modem information, but I need to get a modem-enabled game
                        fileStream.Read(region_codes, 0, 16);     // Region codes, space-filled
                        // Decoding all data
                        DateTime ipbindate = new DateTime();
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(release_date), "MMddyyyy", provider);

                        switch (Encoding.ASCII.GetString(application_type))
                        {
                            case "GM":
                                IPBinInformation.AppendLine("Disc is a game.");
                                break;
                            case "AI":
                                IPBinInformation.AppendLine("Disc is an application.");
                                break;
                            default:
                                IPBinInformation.AppendLine("Disc is from unknown type.");
                                break;
                        }

                        IPBinInformation.AppendFormat("Volume name: {0}", Encoding.ASCII.GetString(volume_name)).AppendLine();
                        //IPBinInformation.AppendFormat("Volume version: {0}", Encoding.ASCII.GetString(volume_version)).AppendLine();
                        //IPBinInformation.AppendFormat("{0}", Encoding.ASCII.GetString(volume_type)).AppendLine();
                        IPBinInformation.AppendFormat("System name: {0}", Encoding.ASCII.GetString(system_name)).AppendLine();
                        //IPBinInformation.AppendFormat("System version: {0}", Encoding.ASCII.GetString(system_version)).AppendLine();
                        IPBinInformation.AppendFormat("Initial program address: 0x{0}", BitConverter.ToInt32(ip_address, 0).ToString("X")).AppendLine();
                        IPBinInformation.AppendFormat("Initial program load size: {0} bytes", BitConverter.ToInt32(ip_loadsize, 0)).AppendLine();
                        IPBinInformation.AppendFormat("Initial program entry address: 0x{0}", BitConverter.ToInt32(ip_entry_address, 0).ToString("X")).AppendLine();
                        IPBinInformation.AppendFormat("Initial program work RAM: {0} bytes", BitConverter.ToInt32(ip_work_ram_size, 0)).AppendLine();
                        IPBinInformation.AppendFormat("System program address: 0x{0}", BitConverter.ToInt32(sp_address, 0).ToString("X")).AppendLine();
                        IPBinInformation.AppendFormat("System program load size: {0} bytes", BitConverter.ToInt32(sp_loadsize, 0)).AppendLine();
                        IPBinInformation.AppendFormat("System program entry address: 0x{0}", BitConverter.ToInt32(sp_entry_address, 0).ToString("X")).AppendLine();
                        IPBinInformation.AppendFormat("System program work RAM: {0} bytes", BitConverter.ToInt32(sp_work_ram_size, 0)).AppendLine();
                        IPBinInformation.AppendFormat("Release date: {0}", ipbindate.ToString()).AppendLine();
                        IPBinInformation.AppendFormat("Release date (other format): {0}", Encoding.ASCII.GetString(release_date2)).AppendLine();
                        IPBinInformation.AppendFormat("Hardware ID: {0}", Encoding.ASCII.GetString(hardware_id)).AppendLine();
                        IPBinInformation.AppendFormat("Developer code: {0}", Encoding.ASCII.GetString(developer_code)).AppendLine();
                        IPBinInformation.AppendFormat("Domestic title: {0}", Encoding.ASCII.GetString(domestic_title)).AppendLine();
                        IPBinInformation.AppendFormat("Overseas title: {0}", Encoding.ASCII.GetString(overseas_title)).AppendLine();
                        IPBinInformation.AppendFormat("Product code: {0}", Encoding.ASCII.GetString(product_code)).AppendLine();
                        IPBinInformation.AppendFormat("Peripherals:").AppendLine();
                        foreach(byte peripheral in peripherals)
                        {
                            switch((char)peripheral)
                            {
                                case 'A':
                                    IPBinInformation.AppendLine("Game supports analog controller.");
                                    break;
                                case 'B':
                                    IPBinInformation.AppendLine("Game supports trackball.");
                                    break;
                                case 'G':
                                    IPBinInformation.AppendLine("Game supports light gun.");
                                    break;
                                case 'J':
                                    IPBinInformation.AppendLine("Game supports JoyPad.");
                                    break;
                                case 'K':
                                    IPBinInformation.AppendLine("Game supports keyboard.");
                                    break;
                                case 'M':
                                    IPBinInformation.AppendLine("Game supports mouse.");
                                    break;
                                case 'O':
                                    IPBinInformation.AppendLine("Game supports Master System's JoyPad.");
                                    break;
                                case 'P':
                                    IPBinInformation.AppendLine("Game supports printer interface.");
                                    break;
                                case 'R':
                                    IPBinInformation.AppendLine("Game supports serial (RS-232C) interface.");
                                    break;
                                case 'T':
                                    IPBinInformation.AppendLine("Game supports tablet interface.");
                                    break;
                                case 'V':
                                    IPBinInformation.AppendLine("Game supports paddle controller.");
                                    break;
                                case ' ':
                                    break;
                                default:
                                    IPBinInformation.AppendFormat("Game supports unknown peripheral {0}.", peripheral.ToString()).AppendLine();
                                    break;
                            }
                        }
                        IPBinInformation.AppendLine("Regions supported:");
                        foreach (byte region in region_codes)
                        {
                            switch ((char)region)
                            {
                                case 'J':
                                    IPBinInformation.AppendLine("Japanese NTSC.");
                                    break;
                                case 'U':
                                    IPBinInformation.AppendLine("USA NTSC.");
                                    break;
                                case 'E':
                                    IPBinInformation.AppendLine("Europe PAL.");
                                    break;
                                case ' ':
                                    break;
                                default:
                                    IPBinInformation.AppendFormat("Game supports unknown region {0}.", region.ToString()).AppendLine();
                                    break;
                            }
                        }

                        break;
                    }
                case "SEGA SEGASATURN ":
                    {
                        Saturn = true;

                        IPBinInformation.AppendLine("--------------------------------");
                        IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
                        IPBinInformation.AppendLine("--------------------------------");

                        // Definitions following
                        byte[] maker_id = new byte[16];      // "SEGA ENTERPRISES"
                        byte[] product_no = new byte[10];         // Product number
                        byte[] product_version = new byte[6];       // Product version
                        byte[] release_date = new byte[8];          // YYYYMMDD
                        byte[] saturn_media = new byte[3];      // "CD-"
                        byte[] disc_no = new byte[1];         // Disc number
                        byte[] disc_no_separator = new byte[1];       // '/'
                        byte[] disc_total_nos = new byte[1];         // Total number of discs
                        byte[] spare_space1 = new byte[2];	      // "  "
                        byte[] region_codes = new byte[10];          // Region codes, space-filled
                        byte[] spare_space2 = new byte[6];     // "  "
                        byte[] peripherals = new byte[16];     // Supported peripherals, see above
                        byte[] product_name = new byte[112];	     // Game name, space-filled
                        // Reading all data
                        fileStream.Read(maker_id, 0, 16);      // "SEGA ENTERPRISES"
                        fileStream.Read(product_no, 0, 10);         // Product number
                        fileStream.Read(product_version, 0, 6);       // Product version
                        fileStream.Read(release_date, 0, 8);          // YYYYMMDD
                        fileStream.Read(saturn_media, 0, 3);      // "CD-"
                        fileStream.Read(disc_no, 0, 1);         // Disc number
                        fileStream.Read(disc_no_separator, 0, 1);       // '/'
                        fileStream.Read(disc_total_nos, 0, 1);         // Total number of discs
                        fileStream.Read(spare_space1, 0, 2);	      // "  "
                        fileStream.Read(region_codes, 0, 10);          // Region codes, space-filled
                        fileStream.Read(spare_space2, 0, 6);     // "  "
                        fileStream.Read(peripherals, 0, 16);     // Supported peripherals, see above
                        fileStream.Read(product_name, 0, 112);	     // Game name, space-filled
                        // Decoding all data
                        DateTime ipbindate = new DateTime();
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(release_date), "yyyyMMdd", provider);
                        IPBinInformation.AppendFormat("Product name: {0}", Encoding.ASCII.GetString(product_name)).AppendLine();
                        IPBinInformation.AppendFormat("Product number: {0}", Encoding.ASCII.GetString(product_no)).AppendLine();
                        IPBinInformation.AppendFormat("Product version: {0}", Encoding.ASCII.GetString(product_version)).AppendLine();
                        IPBinInformation.AppendFormat("Release date: {0}", ipbindate.ToString()).AppendLine();
                        IPBinInformation.AppendFormat("Disc number {0} of {1}", Encoding.ASCII.GetString(disc_no), Encoding.ASCII.GetString(disc_total_nos)).AppendLine();

                        IPBinInformation.AppendFormat("Peripherals:").AppendLine();
                        foreach (byte peripheral in peripherals)
                        {
                            switch ((char)peripheral)
                            {
                                case 'A':
                                    IPBinInformation.AppendLine("Game supports analog controller.");
                                    break;
                                case 'J':
                                    IPBinInformation.AppendLine("Game supports JoyPad.");
                                    break;
                                case 'K':
                                    IPBinInformation.AppendLine("Game supports keyboard.");
                                    break;
                                case 'M':
                                    IPBinInformation.AppendLine("Game supports mouse.");
                                    break;
                                case 'S':
                                    IPBinInformation.AppendLine("Game supports analog steering controller.");
                                    break;
                                case 'T':
                                    IPBinInformation.AppendLine("Game supports multitap.");
                                    break;
                                case ' ':
                                    break;
                                default:
                                    IPBinInformation.AppendFormat("Game supports unknown peripheral {0}.", peripheral.ToString()).AppendLine();
                                    break;
                            }
                        }
                        IPBinInformation.AppendLine("Regions supported:");
                        foreach (byte region in region_codes)
                        {
                            switch ((char)region)
                            {
                                case 'J':
                                    IPBinInformation.AppendLine("Japanese NTSC.");
                                    break;
                                case 'U':
                                    IPBinInformation.AppendLine("North America NTSC.");
                                    break;
                                case 'E':
                                    IPBinInformation.AppendLine("Europe PAL.");
                                    break;
                                case 'T':
                                    IPBinInformation.AppendLine("Asia NTSC.");
                                    break;
                                case ' ':
                                    break;
                                default:
                                    IPBinInformation.AppendFormat("Game supports unknown region {0}.", region.ToString()).AppendLine();
                                    break;
                            }
                        }

                        break;
                    }
                case "SEGA SEGAKATANA ":
                    {
                        Dreamcast = true;

                        IPBinInformation.AppendLine("--------------------------------");
                        IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
                        IPBinInformation.AppendLine("--------------------------------");

                        // Declarations following
                        byte[] maker_id = new byte[16];      // "SEGA ENTERPRISES"
                        byte[] dreamcast_crc = new byte[4];         // CRC of product_no and product_version
                        byte[] spare_space1 = new byte[1];       // " "
                        byte[] dreamcast_media = new byte[6];          // "GD-ROM"
                        byte[] disc_no = new byte[1];         // Disc number
                        byte[] disc_no_separator = new byte[1];       // '/'
                        byte[] disc_total_nos = new byte[1];         // Total number of discs
                        byte[] spare_space2 = new byte[2];	      // "  "
                        byte[] region_codes = new byte[8];          // Region codes, space-filled
                        byte[] peripherals = new byte[4];     // Supported peripherals, bitwise
                        byte[] product_no = new byte[10];     // Product number
                        byte[] product_version = new byte[6];	     // Product version
                        byte[] release_date = new byte[8];     // YYYYMMDD
                        byte[] spare_space3 = new byte[8];     // "  "
                        byte[] boot_filename = new byte[12];     // Usually "1ST_READ.BIN" or "0WINCE.BIN  "
                        byte[] producer = new byte[16];     // Game producer, space-filled
                        byte[] product_name = new byte[128];     // Game name, space-filled
                        // Reading all data
                        fileStream.Read(maker_id, 0, 16);      // "SEGA ENTERPRISES"
                        fileStream.Read(dreamcast_crc, 0, 4);         // CRC of product_no and product_version
                        fileStream.Read(spare_space1, 0, 1);       // " "
                        fileStream.Read(dreamcast_media, 0, 6);          // "GD-ROM"
                        fileStream.Read(disc_no, 0, 1);         // Disc number
                        fileStream.Read(disc_no_separator, 0, 1);       // '/'
                        fileStream.Read(disc_total_nos, 0, 1);         // Total number of discs
                        fileStream.Read(spare_space2, 0, 2);	      // "  "
                        fileStream.Read(region_codes, 0, 8);          // Region codes, space-filled
                        fileStream.Read(peripherals, 0, 4);     // Supported peripherals, bitwise
                        fileStream.Read(product_no, 0, 10);     // Product number
                        fileStream.Read(product_version, 0, 6);	     // Product version
                        fileStream.Read(release_date, 0, 8);     // YYYYMMDD
                        fileStream.Read(spare_space3, 0, 8);        // "  "
                        fileStream.Read(boot_filename, 0, 12);     // Usually "1ST_READ.BIN" or "0WINCE.BIN  "
                        fileStream.Read(producer, 0, 16);     // Game producer, space-filled
                        fileStream.Read(product_name, 0, 128);     // Game name, space-filled
                        // Decoding all data
                        DateTime ipbindate = new DateTime();
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(release_date), "yyyyMMdd", provider);
                        IPBinInformation.AppendFormat("Product name: {0}", Encoding.ASCII.GetString(product_name)).AppendLine();
                        IPBinInformation.AppendFormat("Product version: {0}", Encoding.ASCII.GetString(product_version)).AppendLine();
                        IPBinInformation.AppendFormat("Producer: {0}", Encoding.ASCII.GetString(producer)).AppendLine();
                        IPBinInformation.AppendFormat("Disc media: {0}", Encoding.ASCII.GetString(dreamcast_media)).AppendLine();
                        IPBinInformation.AppendFormat("Disc number {0} of {1}", Encoding.ASCII.GetString(disc_no), Encoding.ASCII.GetString(disc_total_nos)).AppendLine();
                        IPBinInformation.AppendFormat("Release date: {0}", ipbindate.ToString()).AppendLine();
                        switch (Encoding.ASCII.GetString(boot_filename))
                        {
                            case "1ST_READ.BIN":
                                IPBinInformation.AppendLine("Disc boots natively.");
                                break;
                            case "0WINCE.BIN  ":
                                IPBinInformation.AppendLine("Disc boots using Windows CE.");
                                break;
                            default:
                                IPBinInformation.AppendFormat("Disc boots using unknown loader: {0}.", Encoding.ASCII.GetString(boot_filename)).AppendLine();
                                break;
                        }
                        IPBinInformation.AppendLine("Regions supported:");
                        foreach (byte region in region_codes)
                        {
                            switch ((char)region)
                            {
                                case 'J':
                                    IPBinInformation.AppendLine("Japanese NTSC.");
                                    break;
                                case 'U':
                                    IPBinInformation.AppendLine("North America NTSC.");
                                    break;
                                case 'E':
                                    IPBinInformation.AppendLine("Europe PAL.");
                                    break;
                                case ' ':
                                    break;
                                default:
                                    IPBinInformation.AppendFormat("Game supports unknown region {0}.", region.ToString()).AppendLine();
                                    break;
                            }
                        }

                        int iPeripherals = BitConverter.ToInt32(peripherals, 0);

                        if((iPeripherals & 0x00000010) == 0x00000010)
                            IPBinInformation.AppendLine("Game uses Windows CE.");

                        IPBinInformation.AppendFormat("Peripherals:").AppendLine();

                        if ((iPeripherals & 0x00000100) == 0x00000100)
                            IPBinInformation.AppendLine("Game supports the VGA Box.");
                        if ((iPeripherals & 0x00001000) == 0x00001000)
                            IPBinInformation.AppendLine("Game supports other expansion.");
                        if ((iPeripherals & 0x00002000) == 0x00002000)
                            IPBinInformation.AppendLine("Game supports Puru Puru pack.");
                        if ((iPeripherals & 0x00004000) == 0x00004000)
                            IPBinInformation.AppendLine("Game supports Mike Device.");
                        if ((iPeripherals & 0x00008000) == 0x00008000)
                            IPBinInformation.AppendLine("Game supports Memory Card.");
                        if ((iPeripherals & 0x00010000) == 0x00010000)
                            IPBinInformation.AppendLine("Game requires A + B + Start buttons and D-Pad.");
                        if ((iPeripherals & 0x00020000) == 0x00020000)
                            IPBinInformation.AppendLine("Game requires C button.");
                        if ((iPeripherals & 0x00040000) == 0x00040000)
                            IPBinInformation.AppendLine("Game requires D button.");
                        if ((iPeripherals & 0x00080000) == 0x00080000)
                            IPBinInformation.AppendLine("Game requires X button.");
                        if ((iPeripherals & 0x00100000) == 0x00100000)
                            IPBinInformation.AppendLine("Game requires Y button.");
                        if ((iPeripherals & 0x00200000) == 0x00200000)
                            IPBinInformation.AppendLine("Game requires Z button.");
                        if ((iPeripherals & 0x00400000) == 0x00400000)
                            IPBinInformation.AppendLine("Game requires expanded direction buttons.");
                        if ((iPeripherals & 0x00800000) == 0x00800000)
                            IPBinInformation.AppendLine("Game requires analog R trigger.");
                        if ((iPeripherals & 0x01000000) == 0x01000000)
                            IPBinInformation.AppendLine("Game requires analog L trigger.");
                        if ((iPeripherals & 0x02000000) == 0x02000000)
                            IPBinInformation.AppendLine("Game requires analog horizontal controller.");
                        if ((iPeripherals & 0x04000000) == 0x04000000)
                            IPBinInformation.AppendLine("Game requires analog vertical controller.");
                        if ((iPeripherals & 0x08000000) == 0x08000000)
                            IPBinInformation.AppendLine("Game requires expanded analog horizontal controller.");
                        if ((iPeripherals & 0x10000000) == 0x10000000)
                            IPBinInformation.AppendLine("Game requires expanded analog vertical controller.");
                        if ((iPeripherals & 0x20000000) == 0x20000000)
                            IPBinInformation.AppendLine("Game supports Gun.");
                        if ((iPeripherals & 0x40000000) == 0x40000000)
                            IPBinInformation.AppendLine("Game supports Keyboard.");
                        if ((iPeripherals & 0x80000000) == 0x80000000)
                            IPBinInformation.AppendLine("Game supports Mouse.");

                        break;
                    }
            }
            #endregion

            ISOMetadata.AppendFormat("ISO9660 file system").AppendLine();
            if(Joliet)
                ISOMetadata.AppendFormat("Joliet extensions present.").AppendLine();
            if (RockRidge)
                ISOMetadata.AppendFormat("Rock Ridge Interchange Protocol present.").AppendLine();
            if (Bootable)
                ISOMetadata.AppendFormat("Disc bootable following {0} specifications.", BootSpec).AppendLine();
            if (SegaCD)
            {
                ISOMetadata.AppendLine("This is a SegaCD / MegaCD disc.");
                ISOMetadata.AppendLine(IPBinInformation.ToString());
            }
            if (Saturn)
            {
                ISOMetadata.AppendLine("This is a Sega Saturn disc.");
                ISOMetadata.AppendLine(IPBinInformation.ToString());
            }
            if (Dreamcast)
            {
                ISOMetadata.AppendLine("This is a Sega Dreamcast disc.");
                ISOMetadata.AppendLine(IPBinInformation.ToString());
            }
            ISOMetadata.AppendLine("--------------------------------");
            ISOMetadata.AppendLine("VOLUME DESCRIPTOR INFORMATION:");
            ISOMetadata.AppendLine("--------------------------------");
            ISOMetadata.AppendFormat("System identifier: {0}", decodedVD.SystemIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume identifier: {0}", decodedVD.VolumeIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume set identifier: {0}", decodedVD.VolumeSetIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Publisher identifier: {0}", decodedVD.PublisherIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Data preparer identifier: {0}", decodedVD.DataPreparerIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Application identifier: {0}", decodedVD.ApplicationIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume creation date: {0}", decodedVD.CreationTime.ToString()).AppendLine();
            if (decodedVD.HasModificationTime)
                ISOMetadata.AppendFormat("Volume modification date: {0}", decodedVD.ModificationTime.ToString()).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume has not been modified.").AppendLine();
            if (decodedVD.HasExpirationTime)
                ISOMetadata.AppendFormat("Volume expiration date: {0}", decodedVD.ExpirationTime.ToString()).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume does not expire.").AppendLine();
            if (decodedVD.HasEffectiveTime)
                ISOMetadata.AppendFormat("Volume effective date: {0}", decodedVD.EffectiveTime.ToString()).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume has always been effective.").AppendLine();

            information = ISOMetadata.ToString();
        }

        private DecodedVolumeDescriptor DecodeJolietDescriptor(byte[] VDSysId, byte[] VDVolId, byte[] VDVolSetId, byte[] VDPubId, byte[] VDDataPrepId, byte[] VDAppId, byte[] VCTime, byte[] VMTime, byte[] VXTime, byte[] VETime)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.BigEndianUnicode.GetString(VDSysId);
            decodedVD.VolumeIdentifier = Encoding.BigEndianUnicode.GetString(VDVolId);
            decodedVD.VolumeSetIdentifier = Encoding.BigEndianUnicode.GetString(VDVolSetId);
            decodedVD.PublisherIdentifier = Encoding.BigEndianUnicode.GetString(VDPubId);
            decodedVD.DataPreparerIdentifier = Encoding.BigEndianUnicode.GetString(VDDataPrepId);
            decodedVD.ApplicationIdentifier = Encoding.BigEndianUnicode.GetString(VDAppId);
            if (VCTime[0] == '0' || VCTime[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DecodeVDDateTime(VCTime);

            if (VMTime[0] == '0' || VMTime[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DecodeVDDateTime(VMTime);
            }

            if (VXTime[0] == '0' || VXTime[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DecodeVDDateTime(VXTime);
            }

            if (VETime[0] == '0' || VETime[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DecodeVDDateTime(VETime);
            }

            return decodedVD;
        }

        private DecodedVolumeDescriptor DecodeVolumeDescriptor(byte[] VDSysId, byte[] VDVolId, byte[] VDVolSetId, byte[] VDPubId, byte[] VDDataPrepId, byte[] VDAppId, byte[] VCTime, byte[] VMTime, byte[] VXTime, byte[] VETime)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(VDSysId);
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(VDVolId);
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(VDVolSetId);
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(VDPubId);
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(VDDataPrepId);
            decodedVD.ApplicationIdentifier = Encoding.ASCII.GetString(VDAppId);
            if (VCTime[0] == '0' || VCTime[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DecodeVDDateTime(VCTime);

            if (VMTime[0] == '0' || VMTime[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DecodeVDDateTime(VMTime);
            }

            if (VXTime[0] == '0' || VXTime[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DecodeVDDateTime(VXTime);
            }

            if (VETime[0] == '0' || VETime[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DecodeVDDateTime(VETime);
            }

            return decodedVD;
        }

        private DateTime DecodeVDDateTime(byte[] VDDateTime)
        {
            int year, month, day, hour, minute, second, hundredths;
            byte[] twocharvalue = new byte[2];
            byte[] fourcharvalue = new byte[4];

            fourcharvalue[0] = VDDateTime[0];
            fourcharvalue[1] = VDDateTime[1];
            fourcharvalue[2] = VDDateTime[2];
            fourcharvalue[3] = VDDateTime[3];
            year = Convert.ToInt32(Encoding.ASCII.GetString(fourcharvalue));

            twocharvalue[0] = VDDateTime[4];
            twocharvalue[1] = VDDateTime[5];
            month = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            twocharvalue[0] = VDDateTime[6];
            twocharvalue[1] = VDDateTime[7];
            day = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            twocharvalue[0] = VDDateTime[8];
            twocharvalue[1] = VDDateTime[9];
            hour = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            twocharvalue[0] = VDDateTime[10];
            twocharvalue[1] = VDDateTime[11];
            minute = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            twocharvalue[0] = VDDateTime[12];
            twocharvalue[1] = VDDateTime[13];
            second = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            twocharvalue[0] = VDDateTime[14];
            twocharvalue[1] = VDDateTime[15];
            hundredths = Convert.ToInt32(Encoding.ASCII.GetString(twocharvalue));

            DateTime decodedDT = new DateTime(year, month, day, hour, minute, second, hundredths * 10, DateTimeKind.Unspecified);

            return decodedDT;
        }
    }
}
