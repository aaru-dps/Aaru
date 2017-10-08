// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660 : Filesystem
    {
        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
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
            byte[] HSMagic = new byte[5];

            // This indicates the end of a volume descriptor. HighSierra here would have 16 so no problem
            if(VDType == 255)
                return false;

            Array.Copy(vd_sector, 0x001 + xa_off, VDMagic, 0, 5);
            Array.Copy(vd_sector, 0x009 + xa_off, HSMagic, 0, 5);

            DicConsole.DebugWriteLine("ISO9660 plugin", "VDMagic = {0}", CurrentEncoding.GetString(VDMagic));
            DicConsole.DebugWriteLine("ISO9660 plugin", "HSMagic = {0}", CurrentEncoding.GetString(HSMagic));

            return CurrentEncoding.GetString(VDMagic) == IsoMagic || CurrentEncoding.GetString(HSMagic) == HighSierraMagic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            StringBuilder ISOMetadata = new StringBuilder();
            bool RockRidge = false;
            byte VDType;                            // Volume Descriptor Type, should be 1 or 2.
            byte[] VDMagic = new byte[5];           // Volume Descriptor magic "CD001"
            byte[] HSMagic = new byte[5];           // Volume Descriptor magic "CDROM"

            string BootSpec = "";

            byte[] VDPathTableStart = new byte[4];
            byte[] RootDirectoryLocation = new byte[4];

            PrimaryVolumeDescriptor? pvd = null;
            PrimaryVolumeDescriptor? jolietvd = null;
            BootRecord? bvd = null;
            HighSierraPrimaryVolumeDescriptor? hsvd = null;

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.GetSectorSize() < 2048)
                return;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End < 16)
                return;

            ulong counter = 0;

            byte[] vd_sector = imagePlugin.ReadSector(16 + counter + partition.Start);
            int xa_off = imagePlugin.GetSectorSize() == 2336 ? 8 : 0;
            Array.Copy(vd_sector, 0x009 + xa_off, HSMagic, 0, 5);
            bool HighSierra = CurrentEncoding.GetString(HSMagic) == HighSierraMagic;
            int hs_off = 0;
            if(HighSierra)
                hs_off = 8;

            while(true)
            {
                DicConsole.DebugWriteLine("ISO9660 plugin", "Processing VD loop no. {0}", counter);
                // Seek to Volume Descriptor
                DicConsole.DebugWriteLine("ISO9660 plugin", "Reading sector {0}", 16 + counter + partition.Start);
                byte[] vd_sector_tmp = imagePlugin.ReadSector(16 + counter + partition.Start);
                vd_sector = new byte[vd_sector_tmp.Length - xa_off];
                Array.Copy(vd_sector_tmp, xa_off, vd_sector, 0, vd_sector.Length);

                VDType = vd_sector[0 + hs_off];
                DicConsole.DebugWriteLine("ISO9660 plugin", "VDType = {0}", VDType);

                if(VDType == 255) // Supposedly we are in the PVD.
                {
                    if(counter == 0)
                        return;
                    break;
                }

                Array.Copy(vd_sector, 0x001, VDMagic, 0, 5);
                Array.Copy(vd_sector, 0x009, HSMagic, 0, 5);

                if(CurrentEncoding.GetString(VDMagic) != IsoMagic && CurrentEncoding.GetString(HSMagic) != HighSierraMagic) // Recognized, it is an ISO9660, now check for rest of data.
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
                            Marshal.Copy(vd_sector, hs_off, ptr, 2048 - hs_off);
                            bvd = (BootRecord)Marshal.PtrToStructure(ptr, typeof(BootRecord));
                            Marshal.FreeHGlobal(ptr);

                            BootSpec = "Unknown";

                            if(CurrentEncoding.GetString(bvd.Value.system_id).Substring(0, 23) == "EL TORITO SPECIFICATION")
                                BootSpec = "El Torito";

                            break;
                        }
                    case 1:
                        {
                            if(HighSierra)
                            {
                                hsvd = new HighSierraPrimaryVolumeDescriptor();
                                IntPtr ptr = Marshal.AllocHGlobal(2048);
                                Marshal.Copy(vd_sector, 0, ptr, 2048);
                                hsvd = (HighSierraPrimaryVolumeDescriptor)Marshal.PtrToStructure(ptr, typeof(HighSierraPrimaryVolumeDescriptor));
                                Marshal.FreeHGlobal(ptr);
                            }
                            else
                            {
                                pvd = new PrimaryVolumeDescriptor();
                                IntPtr ptr = Marshal.AllocHGlobal(2048);
                                Marshal.Copy(vd_sector, 0, ptr, 2048);
                                pvd = (PrimaryVolumeDescriptor)Marshal.PtrToStructure(ptr, typeof(PrimaryVolumeDescriptor));
                                Marshal.FreeHGlobal(ptr);
                            }
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

            if(pvd == null && hsvd == null)
            {
                information = "ERROR: Could not find primary volume descriptor";
                return;
            }

            if(HighSierra)
                decodedVD = DecodeVolumeDescriptor(hsvd.Value);
            else
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

            ISOMetadata.AppendFormat("{0} file system", HighSierra ? "High Sierra Format" : "ISO9660").AppendLine();
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
            ISOMetadata.AppendFormat("Volume has {0} blocks of {1} bytes each", decodedVD.Blocks, decodedVD.BlockSize).AppendLine();

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

            xmlFSType.Type = HighSierra ? "High Sierra Format" : "ISO9660";

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
            xmlFSType.Clusters = decodedVD.Blocks;
            xmlFSType.ClusterSize = decodedVD.BlockSize;

            information = ISOMetadata.ToString();
        }
    }
}
