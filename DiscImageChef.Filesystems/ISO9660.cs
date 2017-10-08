// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO 9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ISO 9660 filesystem and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // This is coded following ECMA-119.
    // TODO: Differentiate ISO Level 1, 2, 3 and ISO 9660:1999
    // TODO: Apple extensiones, requires XA or advance RR interpretation.
    // TODO: Needs a major rewrite
    public class ISO9660Plugin : Filesystem
    {
        //static bool alreadyLaunched;

        public ISO9660Plugin()
        {
            Name = "ISO9660 Filesystem";
            PluginUUID = new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
            CurrentEncoding = Encoding.ASCII;
        }

        public ISO9660Plugin(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "ISO9660 Filesystem";
            PluginUUID = new Guid("d812f4d3-c357-400d-90fd-3b22ef786aa8");
            if(encoding == null)
                CurrentEncoding = Encoding.ASCII;
        }

        struct DecodedVolumeDescriptor
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

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            /*            if (alreadyLaunched)
                            return false;
                        alreadyLaunched = true;*/

            byte VDType;

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.GetSectorSize() < 2048)
                return false;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End <= (16 + partition.Start))
                return false;

            // Read to Volume Descriptor
            byte[] vd_sector = imagePlugin.ReadSector(16 + partition.Start);

            int xa_off = 0;
            if(vd_sector.Length == 2336)
                xa_off = 8;

            VDType = vd_sector[0 + xa_off];
            byte[] VDMagic = new byte[5];

            // Wrong, VDs can be any order!
            if(VDType == 255) // Supposedly we are in the PVD.
                return false;

            Array.Copy(vd_sector, 0x001 + xa_off, VDMagic, 0, 5);

            DicConsole.DebugWriteLine("ISO9660 plugin", "VDMagic = {0}", CurrentEncoding.GetString(VDMagic));

            return CurrentEncoding.GetString(VDMagic) == "CD001";
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
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

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.GetSectorSize() < 2048)
                return;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End < 16)
                return;

            ulong counter = 0;

            while(true)
            {
                DicConsole.DebugWriteLine("ISO9660 plugin", "Processing VD loop no. {0}", counter);
                // Seek to Volume Descriptor
                DicConsole.DebugWriteLine("ISO9660 plugin", "Reading sector {0}", 16 + counter + partition.Start);
                byte[] vd_sector_tmp = imagePlugin.ReadSector(16 + counter + partition.Start);
                byte[] vd_sector;
                if(vd_sector_tmp.Length == 2336)
                {
                    vd_sector = new byte[2336 - 8];
                    Array.Copy(vd_sector_tmp, 8, vd_sector, 0, 2336 - 8);
                }
                else
                    vd_sector = vd_sector_tmp;

                VDType = vd_sector[0];
                DicConsole.DebugWriteLine("ISO9660 plugin", "VDType = {0}", VDType);

                if(VDType == 255) // Supposedly we are in the PVD.
                {
                    if(counter == 0)
                        return;
                    break;
                }

                Array.Copy(vd_sector, 0x001, VDMagic, 0, 5);

                if(CurrentEncoding.GetString(VDMagic) != "CD001") // Recognized, it is an ISO9660, now check for rest of data.
                {
                    if(counter == 0)
                        return;
                    break;
                }

                switch(VDType)
                {
                    case 0: // TODO
                        {
                            Bootable = true;
                            BootSpec = "Unknown";

                            // Read to boot system identifier
                            Array.Copy(vd_sector, 0x007, BootSysId, 0, 32);

                            if(CurrentEncoding.GetString(BootSysId).Substring(0, 23) == "EL TORITO SPECIFICATION")
                                BootSpec = "El Torito";

                            break;
                        }
                    case 1:
                        {
                            // Read first identifiers
                            Array.Copy(vd_sector, 0x008, VDSysId, 0, 32);
                            Array.Copy(vd_sector, 0x028, VDVolId, 0, 32);

                            // Get path table start
                            Array.Copy(vd_sector, 0x08C, VDPathTableStart, 0, 4);

                            // Read next identifiers
                            Array.Copy(vd_sector, 0x0BE, VDVolSetId, 0, 128);
                            Array.Copy(vd_sector, 0x13E, VDPubId, 0, 128);
                            Array.Copy(vd_sector, 0x1BE, VDDataPrepId, 0, 128);
                            Array.Copy(vd_sector, 0x23E, VDAppId, 0, 128);

                            // Read dates
                            Array.Copy(vd_sector, 0x32D, VCTime, 0, 17);
                            Array.Copy(vd_sector, 0x33E, VMTime, 0, 17);
                            Array.Copy(vd_sector, 0x34F, VXTime, 0, 17);
                            Array.Copy(vd_sector, 0x360, VETime, 0, 17);

                            break;
                        }
                    case 2:
                        {
                            // Check if this is Joliet
                            Array.Copy(vd_sector, 0x058, JolietMagic, 0, 3);
                            if(JolietMagic[0] == '%' && JolietMagic[1] == '/')
                            {
                                if(JolietMagic[2] == '@' || JolietMagic[2] == 'C' || JolietMagic[2] == 'E')
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

                            // Read first identifiers
                            Array.Copy(vd_sector, 0x008, JolietSysId, 0, 32);
                            Array.Copy(vd_sector, 0x028, JolietVolId, 0, 32);

                            // Read next identifiers
                            Array.Copy(vd_sector, 0x0BE, JolietVolSetId, 0, 128);
                            Array.Copy(vd_sector, 0x13E, JolietPubId, 0, 128);
                            Array.Copy(vd_sector, 0x13E, JolietDataPrepId, 0, 128);
                            Array.Copy(vd_sector, 0x13E, JolietAppId, 0, 128);

                            // Read dates
                            Array.Copy(vd_sector, 0x32D, JolietCTime, 0, 17);
                            Array.Copy(vd_sector, 0x33E, JolietMTime, 0, 17);
                            Array.Copy(vd_sector, 0x34F, JolietXTime, 0, 17);
                            Array.Copy(vd_sector, 0x360, JolietETime, 0, 17);

                            break;
                        }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();
            DecodedVolumeDescriptor decodedJolietVD = new DecodedVolumeDescriptor();

            decodedVD = DecodeVolumeDescriptor(VDSysId, VDVolId, VDVolSetId, VDPubId, VDDataPrepId, VDAppId, VCTime, VMTime, VXTime, VETime);
            if(Joliet)
                decodedJolietVD = DecodeJolietDescriptor(JolietSysId, JolietVolId, JolietVolSetId, JolietPubId, JolietDataPrepId, JolietAppId, JolietCTime, JolietMTime, JolietXTime, JolietETime);


            ulong i = (ulong)BitConverter.ToInt32(VDPathTableStart, 0);
            DicConsole.DebugWriteLine("ISO9660 plugin", "VDPathTableStart = {0} + {1} = {2}", i, partition.Start, i + partition.Start);

            // TODO: Check this
            /*
            if((i + partition.Start) < partition.End)
            {

                byte[] path_table = imagePlugin.ReadSector(i + partition.Start);
                Array.Copy(path_table, 2, RootDirectoryLocation, 0, 4);
                // Check for Rock Ridge
                byte[] root_dir = imagePlugin.ReadSector((ulong)BitConverter.ToInt32(RootDirectoryLocation, 0) + partition.Start);

                byte[] SUSPMagic = new byte[2];
                byte[] RRMagic = new byte[2];

                Array.Copy(root_dir, 0x22, SUSPMagic, 0, 2);
                if(CurrentEncoding.GetString(SUSPMagic) == "SP")
                {
                    Array.Copy(root_dir, 0x29, RRMagic, 0, 2);
                    RockRidge |= CurrentEncoding.GetString(RRMagic) == "RR";
                }
            }*/

            byte[] ipbin_sector = imagePlugin.ReadSector(0 + partition.Start);
            Decoders.Sega.CD.IPBin? SegaCD = Decoders.Sega.CD.DecodeIPBin(ipbin_sector);
            Decoders.Sega.Saturn.IPBin? Saturn = Decoders.Sega.Saturn.DecodeIPBin(ipbin_sector);
            Decoders.Sega.Dreamcast.IPBin? Dreamcast = Decoders.Sega.Dreamcast.DecodeIPBin(ipbin_sector);

            ISOMetadata.AppendFormat("ISO9660 file system").AppendLine();
            if(Joliet)
                ISOMetadata.AppendFormat("Joliet extensions present.").AppendLine();
            if(RockRidge)
                ISOMetadata.AppendFormat("Rock Ridge Interchange Protocol present.").AppendLine();
            if(Bootable)
                ISOMetadata.AppendFormat("Disc bootable following {0} specifications.", BootSpec).AppendLine();
            if(SegaCD != null)
            {
                ISOMetadata.AppendLine("This is a SegaCD / MegaCD disc.");
                ISOMetadata.AppendLine(Decoders.Sega.CD.Prettify(SegaCD));
            }
            if(Saturn != null)
            {
                ISOMetadata.AppendLine("This is a Sega Saturn disc.");
                ISOMetadata.AppendLine(Decoders.Sega.Saturn.Prettify(Saturn));
            }
            if(Dreamcast != null)
            {
                ISOMetadata.AppendLine("This is a Sega Dreamcast disc.");
                ISOMetadata.AppendLine(Decoders.Sega.Dreamcast.Prettify(Dreamcast));
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
            ISOMetadata.AppendFormat("Volume creation date: {0}", decodedVD.CreationTime).AppendLine();
            if(decodedVD.HasModificationTime)
                ISOMetadata.AppendFormat("Volume modification date: {0}", decodedVD.ModificationTime).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume has not been modified.").AppendLine();
            if(decodedVD.HasExpirationTime)
                ISOMetadata.AppendFormat("Volume expiration date: {0}", decodedVD.ExpirationTime).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume does not expire.").AppendLine();
            if(decodedVD.HasEffectiveTime)
                ISOMetadata.AppendFormat("Volume effective date: {0}", decodedVD.EffectiveTime).AppendLine();
            else
                ISOMetadata.AppendFormat("Volume has always been effective.").AppendLine();

            if(Joliet)
            {
                ISOMetadata.AppendLine("---------------------------------------");
                ISOMetadata.AppendLine("JOLIET VOLUME DESCRIPTOR INFORMATION:");
                ISOMetadata.AppendLine("---------------------------------------");
                ISOMetadata.AppendFormat("System identifier: {0}", decodedJolietVD.SystemIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Volume identifier: {0}", decodedJolietVD.VolumeIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Volume set identifier: {0}", decodedJolietVD.VolumeSetIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Publisher identifier: {0}", decodedJolietVD.PublisherIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Data preparer identifier: {0}", decodedJolietVD.DataPreparerIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Application identifier: {0}", decodedJolietVD.ApplicationIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Volume creation date: {0}", decodedJolietVD.CreationTime).AppendLine();
                if(decodedJolietVD.HasModificationTime)
                    ISOMetadata.AppendFormat("Volume modification date: {0}", decodedJolietVD.ModificationTime).AppendLine();
                else
                    ISOMetadata.AppendFormat("Volume has not been modified.").AppendLine();
                if(decodedJolietVD.HasExpirationTime)
                    ISOMetadata.AppendFormat("Volume expiration date: {0}", decodedJolietVD.ExpirationTime).AppendLine();
                else
                    ISOMetadata.AppendFormat("Volume does not expire.").AppendLine();
                if(decodedJolietVD.HasEffectiveTime)
                    ISOMetadata.AppendFormat("Volume effective date: {0}", decodedJolietVD.EffectiveTime).AppendLine();
                else
                    ISOMetadata.AppendFormat("Volume has always been effective.").AppendLine();
            }

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "ISO9660";

            if(Joliet)
            {
                xmlFSType.VolumeName = decodedJolietVD.VolumeIdentifier;

                if(decodedJolietVD.SystemIdentifier == null || decodedVD.SystemIdentifier.Length > decodedJolietVD.SystemIdentifier.Length)
                    xmlFSType.SystemIdentifier = decodedVD.SystemIdentifier;
                else
                    xmlFSType.SystemIdentifier = decodedJolietVD.SystemIdentifier;

                if(decodedJolietVD.VolumeSetIdentifier == null || decodedVD.VolumeSetIdentifier.Length > decodedJolietVD.VolumeSetIdentifier.Length)
                    xmlFSType.VolumeSetIdentifier = decodedVD.VolumeSetIdentifier;
                else
                    xmlFSType.VolumeSetIdentifier = decodedJolietVD.VolumeSetIdentifier;

                if(decodedJolietVD.PublisherIdentifier == null || decodedVD.PublisherIdentifier.Length > decodedJolietVD.PublisherIdentifier.Length)
                    xmlFSType.PublisherIdentifier = decodedVD.PublisherIdentifier;
                else
                    xmlFSType.PublisherIdentifier = decodedJolietVD.PublisherIdentifier;

                if(decodedJolietVD.DataPreparerIdentifier == null || decodedVD.DataPreparerIdentifier.Length > decodedJolietVD.DataPreparerIdentifier.Length)
                    xmlFSType.DataPreparerIdentifier = decodedVD.DataPreparerIdentifier;
                else
                    xmlFSType.DataPreparerIdentifier = decodedJolietVD.SystemIdentifier;

                if(decodedJolietVD.ApplicationIdentifier == null || decodedVD.ApplicationIdentifier.Length > decodedJolietVD.ApplicationIdentifier.Length)
                    xmlFSType.ApplicationIdentifier = decodedVD.ApplicationIdentifier;
                else
                    xmlFSType.ApplicationIdentifier = decodedJolietVD.SystemIdentifier;

                xmlFSType.CreationDate = decodedJolietVD.CreationTime;
                xmlFSType.CreationDateSpecified = true;
                if(decodedJolietVD.HasModificationTime)
                {
                    xmlFSType.ModificationDate = decodedJolietVD.ModificationTime;
                    xmlFSType.ModificationDateSpecified = true;
                }
                if(decodedJolietVD.HasExpirationTime)
                {
                    xmlFSType.ExpirationDate = decodedJolietVD.ExpirationTime;
                    xmlFSType.ExpirationDateSpecified = true;
                }
                if(decodedJolietVD.HasEffectiveTime)
                {
                    xmlFSType.EffectiveDate = decodedJolietVD.EffectiveTime;
                    xmlFSType.EffectiveDateSpecified = true;
                }
            }
            else
            {
                xmlFSType.SystemIdentifier = decodedVD.SystemIdentifier;
                xmlFSType.VolumeName = decodedVD.VolumeIdentifier;
                xmlFSType.VolumeSetIdentifier = decodedVD.VolumeSetIdentifier;
                xmlFSType.PublisherIdentifier = decodedVD.PublisherIdentifier;
                xmlFSType.DataPreparerIdentifier = decodedVD.DataPreparerIdentifier;
                xmlFSType.ApplicationIdentifier = decodedVD.ApplicationIdentifier;
                xmlFSType.CreationDate = decodedVD.CreationTime;
                xmlFSType.CreationDateSpecified = true;
                if(decodedVD.HasModificationTime)
                {
                    xmlFSType.ModificationDate = decodedVD.ModificationTime;
                    xmlFSType.ModificationDateSpecified = true;
                }
                if(decodedVD.HasExpirationTime)
                {
                    xmlFSType.ExpirationDate = decodedVD.ExpirationTime;
                    xmlFSType.ExpirationDateSpecified = true;
                }
                if(decodedVD.HasEffectiveTime)
                {
                    xmlFSType.EffectiveDate = decodedVD.EffectiveTime;
                    xmlFSType.EffectiveDateSpecified = true;
                }
            }

            xmlFSType.Bootable |= Bootable || SegaCD != null || Saturn != null || Dreamcast != null;
            xmlFSType.Clusters = (long)(partition.End - partition.Start + 1);
            xmlFSType.ClusterSize = 2048;

            information = ISOMetadata.ToString();
        }

        static DecodedVolumeDescriptor DecodeJolietDescriptor(byte[] VDSysId, byte[] VDVolId, byte[] VDVolSetId, byte[] VDPubId, byte[] VDDataPrepId, byte[] VDAppId, byte[] VCTime, byte[] VMTime, byte[] VXTime, byte[] VETime)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.BigEndianUnicode.GetString(VDSysId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeIdentifier = Encoding.BigEndianUnicode.GetString(VDVolId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeSetIdentifier = Encoding.BigEndianUnicode.GetString(VDVolSetId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.PublisherIdentifier = Encoding.BigEndianUnicode.GetString(VDPubId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.DataPreparerIdentifier = Encoding.BigEndianUnicode.GetString(VDDataPrepId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.ApplicationIdentifier = Encoding.BigEndianUnicode.GetString(VDAppId).TrimEnd().Trim(new[] { '\u0000' });
            if(VCTime[0] < 0x31 || VCTime[0] > 0x39)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(VCTime);

            if(VMTime[0] < 0x31 || VMTime[0] > 0x39)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(VMTime);
            }

            if(VXTime[0] < 0x31 || VXTime[0] > 0x39)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(VXTime);
            }

            if(VETime[0] < 0x31 || VETime[0] > 0x39)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(VETime);
            }

            return decodedVD;
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(byte[] VDSysId, byte[] VDVolId, byte[] VDVolSetId, byte[] VDPubId, byte[] VDDataPrepId, byte[] VDAppId, byte[] VCTime, byte[] VMTime, byte[] VXTime, byte[] VETime)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(VDSysId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(VDVolId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(VDVolSetId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(VDPubId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(VDDataPrepId).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.ApplicationIdentifier = Encoding.ASCII.GetString(VDAppId).TrimEnd().Trim(new[] { '\u0000' });
            if(VCTime[0] == '0' || VCTime[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(VCTime);

            if(VMTime[0] == '0' || VMTime[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(VMTime);
            }

            if(VXTime[0] == '0' || VXTime[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(VXTime);
            }

            if(VETime[0] == '0' || VETime[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(VETime);
            }

            return decodedVD;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}