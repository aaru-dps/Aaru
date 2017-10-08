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
using System.Runtime.InteropServices;
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PrimaryVolumeDescriptor
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            // Only used in SVDs
            public byte flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] volume_id;
            public ulong reserved2;
            public ulong volume_space_size;
            // Only used in SVDs
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] escape_sequences;
            public uint volume_set_size;
            public uint volume_sequence_number;
            public uint logical_block_size;
            public ulong path_table_size;
            public uint type_1_path_table;
            public uint opt_type_1_path_table;
            public uint type_m_path_table;
            public uint opt_type_m_path_table;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)]
            public byte[] root_directory_record;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] volume_set_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] publisher_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] preparer_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] application_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] copyright_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] abstract_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] bibliographic_file_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] creation_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] modification_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] expiration_date;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public byte[] effective_date;
            public byte file_structure_version;
            public byte reserved4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] application_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
            public byte[] reserved5;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BootRecord
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] id;
            public byte version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] system_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] boot_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1977)]
            public byte[] boot_use;
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
            bool RockRidge = false;
            byte VDType;                            // Volume Descriptor Type, should be 1 or 2.
            byte[] VDMagic = new byte[5];           // Volume Descriptor magic "CD001"

            string BootSpec = "";

            byte[] VDPathTableStart = new byte[4];
            byte[] RootDirectoryLocation = new byte[4];

            PrimaryVolumeDescriptor? pvd = null;
            PrimaryVolumeDescriptor? jolietvd = null;
            BootRecord? bvd = null;

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
                            bvd = new BootRecord();
                            IntPtr ptr = Marshal.AllocHGlobal(2048);
                            Marshal.Copy(vd_sector, 0, ptr, 2048);
                            bvd = (BootRecord)Marshal.PtrToStructure(ptr, typeof(BootRecord));
                            Marshal.FreeHGlobal(ptr);

                            BootSpec = "Unknown";

                            if(CurrentEncoding.GetString(bvd.Value.system_id).Substring(0, 23) == "EL TORITO SPECIFICATION")
                                BootSpec = "El Torito";

                            break;
                        }
                    case 1:
                        {
                            pvd = new PrimaryVolumeDescriptor();
                            IntPtr ptr = Marshal.AllocHGlobal(2048);
                            Marshal.Copy(vd_sector, 0, ptr, 2048);
                            pvd = (PrimaryVolumeDescriptor)Marshal.PtrToStructure(ptr, typeof(PrimaryVolumeDescriptor));
                            Marshal.FreeHGlobal(ptr);
                            break;
                        }
                    case 2:
                        {
                            PrimaryVolumeDescriptor svd = new PrimaryVolumeDescriptor();
                            IntPtr ptr = Marshal.AllocHGlobal(2048);
                            Marshal.Copy(vd_sector, 0, ptr, 2048);
                            svd = (PrimaryVolumeDescriptor)Marshal.PtrToStructure(ptr, typeof(PrimaryVolumeDescriptor));
                            Marshal.FreeHGlobal(ptr);

                            // Check if this is Joliet
                            if(svd.escape_sequences[0] == '%' && svd.escape_sequences[1] == '/')
                            {
                                if(svd.escape_sequences[2] == '@' || svd.escape_sequences[2] == 'C' || svd.escape_sequences[2] == 'E')
                                {
                                    jolietvd = svd;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                                DicConsole.WriteLine("ISO9660 plugin", "Found unknown supplementary volume descriptor");

                            break;
                        }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();
            DecodedVolumeDescriptor decodedJolietVD = new DecodedVolumeDescriptor();

            xmlFSType = new Schemas.FileSystemType();

            if(pvd == null)
            {
                information = "ERROR: Could not find primary volume descriptor";
                return;
            }

            decodedVD = DecodeVolumeDescriptor(pvd.Value);
            if(jolietvd != null)
                decodedJolietVD = DecodeJolietDescriptor(jolietvd.Value);


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
            if(jolietvd != null)
                ISOMetadata.AppendFormat("Joliet extensions present.").AppendLine();
            if(RockRidge)
                ISOMetadata.AppendFormat("Rock Ridge Interchange Protocol present.").AppendLine();
            if(bvd != null)
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

            if(jolietvd != null)
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

            xmlFSType.Type = "ISO9660";

            if(jolietvd != null)
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

            xmlFSType.Bootable |= bvd != null || SegaCD != null || Saturn != null || Dreamcast != null;
            xmlFSType.Clusters = (long)(partition.End - partition.Start + 1);
            xmlFSType.ClusterSize = 2048;

            information = ISOMetadata.ToString();
        }

        static DecodedVolumeDescriptor DecodeJolietDescriptor(PrimaryVolumeDescriptor jolietvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.system_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.VolumeSetIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.volume_set_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.PublisherIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.publisher_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.DataPreparerIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.preparer_id).TrimEnd().Trim(new[] { '\u0000' });
            decodedVD.ApplicationIdentifier = Encoding.BigEndianUnicode.GetString(jolietvd.application_id).TrimEnd().Trim(new[] { '\u0000' });
            if(jolietvd.creation_date[0] < 0x31 || jolietvd.creation_date[0] > 0x39)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(jolietvd.creation_date);

            if(jolietvd.modification_date[0] < 0x31 || jolietvd.modification_date[0] > 0x39)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(jolietvd.modification_date);
            }

            if(jolietvd.expiration_date[0] < 0x31 || jolietvd.expiration_date[0] > 0x39)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(jolietvd.expiration_date);
            }

            if(jolietvd.effective_date[0] < 0x31 || jolietvd.effective_date[0] > 0x39)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(jolietvd.effective_date);
            }

            return decodedVD;
        }

        static DecodedVolumeDescriptor DecodeVolumeDescriptor(PrimaryVolumeDescriptor pvd)
        {
            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();

            decodedVD.SystemIdentifier = Encoding.ASCII.GetString(pvd.system_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeIdentifier = Encoding.ASCII.GetString(pvd.volume_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.VolumeSetIdentifier = Encoding.ASCII.GetString(pvd.volume_set_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.PublisherIdentifier = Encoding.ASCII.GetString(pvd.publisher_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.DataPreparerIdentifier = Encoding.ASCII.GetString(pvd.preparer_id).TrimEnd().Trim(new[] { '\0' });
            decodedVD.ApplicationIdentifier = Encoding.ASCII.GetString(pvd.application_data).TrimEnd().Trim(new[] { '\0' });
            if(pvd.creation_date[0] == '0' || pvd.creation_date[0] == 0x00)
                decodedVD.CreationTime = DateTime.MinValue;
            else
                decodedVD.CreationTime = DateHandlers.ISO9660ToDateTime(pvd.creation_date);

            if(pvd.modification_date[0] == '0' || pvd.modification_date[0] == 0x00)
            {
                decodedVD.HasModificationTime = false;
            }
            else
            {
                decodedVD.HasModificationTime = true;
                decodedVD.ModificationTime = DateHandlers.ISO9660ToDateTime(pvd.modification_date);
            }

            if(pvd.expiration_date[0] == '0' || pvd.expiration_date[0] == 0x00)
            {
                decodedVD.HasExpirationTime = false;
            }
            else
            {
                decodedVD.HasExpirationTime = true;
                decodedVD.ExpirationTime = DateHandlers.ISO9660ToDateTime(pvd.expiration_date);
            }

            if(pvd.effective_date[0] == '0' || pvd.effective_date[0] == 0x00)
            {
                decodedVD.HasEffectiveTime = false;
            }
            else
            {
                decodedVD.HasEffectiveTime = true;
                decodedVD.EffectiveTime = DateHandlers.ISO9660ToDateTime(pvd.effective_date);
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