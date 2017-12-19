// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the ISO9660 filesystem and shows information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
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
            if(imagePlugin.GetSectorSize() < 2048) return false;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End <= (16 + partition.Start)) return false;

            // Read to Volume Descriptor
            byte[] vd_sector = imagePlugin.ReadSector(16 + partition.Start);

            int xa_off = 0;
            if(vd_sector.Length == 2336) xa_off = 8;

            VDType = vd_sector[0 + xa_off];
            byte[] VDMagic = new byte[5];
            byte[] HSMagic = new byte[5];

            // This indicates the end of a volume descriptor. HighSierra here would have 16 so no problem
            if(VDType == 255) return false;

            Array.Copy(vd_sector, 0x001 + xa_off, VDMagic, 0, 5);
            Array.Copy(vd_sector, 0x009 + xa_off, HSMagic, 0, 5);

            DicConsole.DebugWriteLine("ISO9660 plugin", "VDMagic = {0}", CurrentEncoding.GetString(VDMagic));
            DicConsole.DebugWriteLine("ISO9660 plugin", "HSMagic = {0}", CurrentEncoding.GetString(HSMagic));

            return CurrentEncoding.GetString(VDMagic) == IsoMagic ||
                   CurrentEncoding.GetString(HSMagic) == HighSierraMagic ||
                   CurrentEncoding.GetString(VDMagic) == CdiMagic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            StringBuilder ISOMetadata = new StringBuilder();
            byte VDType; // Volume Descriptor Type, should be 1 or 2.
            byte[] VDMagic = new byte[5]; // Volume Descriptor magic "CD001"
            byte[] HSMagic = new byte[5]; // Volume Descriptor magic "CDROM"

            string BootSpec = "";

            byte[] VDPathTableStart = new byte[4];
            byte[] RootDirectoryLocation = new byte[4];

            PrimaryVolumeDescriptor? pvd = null;
            PrimaryVolumeDescriptor? jolietvd = null;
            BootRecord? bvd = null;
            HighSierraPrimaryVolumeDescriptor? hsvd = null;
            FileStructureVolumeDescriptor? fsvd = null;
            ElToritoBootRecord? torito = null;

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.GetSectorSize() < 2048) return;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End < 16) return;

            ulong counter = 0;

            byte[] vd_sector = imagePlugin.ReadSector(16 + counter + partition.Start);
            int xa_off = vd_sector.Length == 2336 ? 8 : 0;
            Array.Copy(vd_sector, 0x009 + xa_off, HSMagic, 0, 5);
            bool HighSierra = CurrentEncoding.GetString(HSMagic) == HighSierraMagic;
            int hs_off = 0;
            if(HighSierra) hs_off = 8;
            bool CDi = false;

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
                    if(counter == 0) return;

                    break;
                }

                Array.Copy(vd_sector, 0x001, VDMagic, 0, 5);
                Array.Copy(vd_sector, 0x009, HSMagic, 0, 5);

                if(CurrentEncoding.GetString(VDMagic) != IsoMagic &&
                   CurrentEncoding.GetString(HSMagic) != HighSierraMagic &&
                   CurrentEncoding.GetString(VDMagic) != CdiMagic
                ) // Recognized, it is an ISO9660, now check for rest of data.
                {
                    if(counter == 0) return;

                    break;
                }

                CDi |= CurrentEncoding.GetString(VDMagic) == CdiMagic;

                switch(VDType)
                {
                    case 0:
                    {
                        bvd = new BootRecord();
                        IntPtr ptr = Marshal.AllocHGlobal(2048);
                        Marshal.Copy(vd_sector, hs_off, ptr, 2048 - hs_off);
                        bvd = (BootRecord)Marshal.PtrToStructure(ptr, typeof(BootRecord));
                        Marshal.FreeHGlobal(ptr);

                        BootSpec = "Unknown";

                        if(CurrentEncoding.GetString(bvd.Value.system_id).Substring(0, 23) == "EL TORITO SPECIFICATION")
                        {
                            BootSpec = "El Torito";
                            torito = new ElToritoBootRecord();
                            ptr = Marshal.AllocHGlobal(2048);
                            Marshal.Copy(vd_sector, hs_off, ptr, 2048 - hs_off);
                            torito = (ElToritoBootRecord)Marshal.PtrToStructure(ptr, typeof(ElToritoBootRecord));
                            Marshal.FreeHGlobal(ptr);
                        }

                        break;
                    }
                    case 1:
                    {
                        if(HighSierra)
                        {
                            hsvd = new HighSierraPrimaryVolumeDescriptor();
                            IntPtr ptr = Marshal.AllocHGlobal(2048);
                            Marshal.Copy(vd_sector, 0, ptr, 2048);
                            hsvd =
                                (HighSierraPrimaryVolumeDescriptor)
                                Marshal.PtrToStructure(ptr, typeof(HighSierraPrimaryVolumeDescriptor));
                            Marshal.FreeHGlobal(ptr);
                        }
                        else if(CDi)
                            fsvd =
                                BigEndianMarshal
                                    .ByteArrayToStructureBigEndian<FileStructureVolumeDescriptor>(vd_sector);
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
                            if(svd.escape_sequences[2] == '@' || svd.escape_sequences[2] == 'C' ||
                               svd.escape_sequences[2] == 'E') { jolietvd = svd; }
                            else { break; }
                        }
                        else DicConsole.WriteLine("ISO9660 plugin", "Found unknown supplementary volume descriptor");

                        break;
                    }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVD = new DecodedVolumeDescriptor();
            DecodedVolumeDescriptor decodedJolietVD = new DecodedVolumeDescriptor();

            xmlFSType = new Schemas.FileSystemType();

            if(pvd == null && hsvd == null && fsvd == null)
            {
                information = "ERROR: Could not find primary volume descriptor";
                return;
            }

            if(HighSierra) decodedVD = DecodeVolumeDescriptor(hsvd.Value);
            else if(CDi) decodedVD = DecodeVolumeDescriptor(fsvd.Value);
            else decodedVD = DecodeVolumeDescriptor(pvd.Value);

            if(jolietvd != null) decodedJolietVD = DecodeJolietDescriptor(jolietvd.Value);

            uint rootLocation = 0;
            uint rootSize = 0;

            // No need to read root on CD-i, as extensions are not supported...
            if(!CDi)
            {
                rootLocation = HighSierra
                                   ? hsvd.Value.root_directory_record.extent
                                   : pvd.Value.root_directory_record.extent;

                if(HighSierra)
                {
                    rootSize = hsvd.Value.root_directory_record.size / hsvd.Value.logical_block_size;
                    if(hsvd.Value.root_directory_record.size % hsvd.Value.logical_block_size > 0) rootSize++;
                }
                else
                {
                    rootSize = pvd.Value.root_directory_record.size / pvd.Value.logical_block_size;
                    if(pvd.Value.root_directory_record.size % pvd.Value.logical_block_size > 0) rootSize++;
                }
            }

            byte[] root_dir = imagePlugin.ReadSectors(rootLocation + partition.Start, rootSize);
            int rootOff = 0;
            bool XA = false;
            bool Apple = false;
            bool SUSP = false;
            bool RRIP = false;
            bool ziso = false;
            bool Amiga = false;
            bool AAIP = false;
            List<ContinuationArea> contareas = new List<ContinuationArea>();
            List<byte[]> refareas = new List<byte[]>();
            StringBuilder suspInformation = new StringBuilder();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            // Walk thru root directory to see system area extensions in use
            while(rootOff + Marshal.SizeOf(typeof(DirectoryRecord)) < root_dir.Length && !CDi)
            {
                DirectoryRecord record = new DirectoryRecord();
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(record));
                Marshal.Copy(root_dir, rootOff, ptr, Marshal.SizeOf(record));
                record = (DirectoryRecord)Marshal.PtrToStructure(ptr, typeof(DirectoryRecord));
                Marshal.FreeHGlobal(ptr);

                int sa_off = Marshal.SizeOf(record) + record.name_len;
                sa_off += sa_off % 2;
                int sa_len = record.length - sa_off;

                if(sa_len > 0 && rootOff + sa_off + sa_len <= root_dir.Length)
                {
                    byte[] sa = new byte[sa_len];
                    Array.Copy(root_dir, rootOff + sa_off, sa, 0, sa_len);
                    sa_off = 0;

                    while(sa_off < sa_len)
                    {
                        bool noneFound = true;

                        if(Marshal.SizeOf(typeof(CdromXa)) + sa_off <= sa_len)
                        {
                            CdromXa xa = BigEndianMarshal.ByteArrayToStructureBigEndian<CdromXa>(sa);
                            if(xa.signature == XaMagic)
                            {
                                XA = true;
                                sa_off += Marshal.SizeOf(typeof(CdromXa));
                                noneFound = false;
                            }
                        }

                        if(sa_off + 2 >= sa_len) break;

                        ushort nextSignature = BigEndianBitConverter.ToUInt16(sa, sa_off);

                        // Easy, contains size field
                        if(nextSignature == AppleMagic)
                        {
                            Apple = true;
                            sa_off += sa[sa_off + 2];
                            noneFound = false;
                        }

                        // Not easy, contains size field
                        if(nextSignature == AppleMagicOld)
                        {
                            Apple = true;
                            AppleOldId apple_id = (AppleOldId)sa[sa_off + 2];
                            noneFound = false;

                            switch(apple_id)
                            {
                                case AppleOldId.ProDOS:
                                    sa_off += Marshal.SizeOf(typeof(AppleProDOSOldSystemUse));
                                    break;
                                case AppleOldId.TypeCreator:
                                case AppleOldId.TypeCreatorBundle:
                                    sa_off += Marshal.SizeOf(typeof(AppleHFSTypeCreatorSystemUse));
                                    break;
                                case AppleOldId.TypeCreatorIcon:
                                case AppleOldId.TypeCreatorIconBundle:
                                    sa_off += Marshal.SizeOf(typeof(AppleHFSIconSystemUse));
                                    break;
                                case AppleOldId.HFS:
                                    sa_off += Marshal.SizeOf(typeof(AppleHFSOldSystemUse));
                                    break;
                            }
                        }

                        // IEEE-P1281 aka SUSP 1.12
                        if(nextSignature == SUSP_Indicator)
                        {
                            SUSP = true;
                            sa_off += sa[sa_off + 2];
                            noneFound = false;

                            while(sa_off + 2 < sa_len)
                            {
                                nextSignature = BigEndianBitConverter.ToUInt16(sa, sa_off);

                                if(nextSignature == AppleMagic)
                                {
                                    // Can collide with AAIP
                                    if(sa[sa_off + 3] == 1 && sa[sa_off + 2] == 7) Apple = true;
                                    else Apple |= sa[sa_off + 3] != 1;
                                }

                                if(nextSignature == SUSP_Continuation && sa_off + sa[sa_off + 2] <= sa_len)
                                {
                                    byte[] ce = new byte[sa[sa_off + 2]];
                                    Array.Copy(sa, sa_off, ce, 0, ce.Length);
                                    ContinuationArea ca =
                                        BigEndianMarshal.ByteArrayToStructureBigEndian<ContinuationArea>(ce);
                                    contareas.Add(ca);
                                }

                                if(nextSignature == SUSP_Reference && sa_off + sa[sa_off + 2] <= sa_len)
                                {
                                    byte[] er = new byte[sa[sa_off + 2]];
                                    Array.Copy(sa, sa_off, er, 0, er.Length);
                                    refareas.Add(er);
                                }

                                RRIP |= nextSignature == RRIP_Magic || nextSignature == RRIP_PosixAttributes ||
                                        nextSignature == RRIP_PosixDevNo || nextSignature == RRIP_Symlink ||
                                        nextSignature == RRIP_Name || nextSignature == RRIP_Childlink ||
                                        nextSignature == RRIP_Parentlink || nextSignature == RRIP_RelocatedDir ||
                                        nextSignature == RRIP_Timestamps || nextSignature == RRIP_Sparse;

                                ziso |= nextSignature == ziso_Magic;
                                Amiga |= nextSignature == Amiga_Magic;
                                AAIP |= nextSignature == AAIP_Magic ||
                                        (nextSignature == AAIP_OldMagic && sa[sa_off + 3] == 1 && sa[sa_off + 2] >= 9);

                                sa_off += sa[sa_off + 2];

                                if(nextSignature == SUSP_Terminator) break;
                            }
                        }

                        if(noneFound) break;
                    }
                }

                rootOff += record.length;

                if(record.length == 0) break;
            }

            foreach(ContinuationArea ca in contareas)
            {
                uint ca_len = (ca.ca_length_be + ca.offset_be) /
                              (HighSierra ? hsvd.Value.logical_block_size : pvd.Value.logical_block_size);
                if((ca.ca_length_be + ca.offset_be) %
                   (HighSierra ? hsvd.Value.logical_block_size : pvd.Value.logical_block_size) > 0) ca_len++;

                byte[] ca_sectors = imagePlugin.ReadSectors(ca.block_be, ca_len);
                byte[] ca_data = new byte[ca.ca_length_be];
                Array.Copy(ca_sectors, ca.offset_be, ca_data, 0, ca.ca_length_be);
                int ca_off = 0;

                while(ca_off < ca.ca_length_be)
                {
                    ushort nextSignature = BigEndianBitConverter.ToUInt16(ca_data, ca_off);

                    // Apple never said to include its extensions inside a continuation area, but just in case
                    if(nextSignature == AppleMagic)
                    {
                        // Can collide with AAIP
                        if(ca_data[ca_off + 3] == 1 && ca_data[ca_off + 2] == 7) Apple = true;
                        else Apple |= ca_data[ca_off + 3] != 1;
                    }

                    if(nextSignature == SUSP_Reference && ca_off + ca_data[ca_off + 2] <= ca.ca_length_be)
                    {
                        byte[] er = new byte[ca_data[ca_off + 2]];
                        Array.Copy(ca_data, ca_off, er, 0, er.Length);
                        refareas.Add(er);
                    }

                    RRIP |= nextSignature == RRIP_Magic || nextSignature == RRIP_PosixAttributes ||
                            nextSignature == RRIP_PosixDevNo || nextSignature == RRIP_Symlink ||
                            nextSignature == RRIP_Name || nextSignature == RRIP_Childlink ||
                            nextSignature == RRIP_Parentlink || nextSignature == RRIP_RelocatedDir ||
                            nextSignature == RRIP_Timestamps || nextSignature == RRIP_Sparse;

                    ziso |= nextSignature == ziso_Magic;
                    Amiga |= nextSignature == Amiga_Magic;
                    AAIP |= nextSignature == AAIP_Magic ||
                            (nextSignature == AAIP_OldMagic && ca_data[ca_off + 3] == 1 && ca_data[ca_off + 2] >= 9);

                    ca_off += ca_data[ca_off + 2];
                }
            }

            if(refareas.Count > 0)
            {
                suspInformation.AppendLine("----------------------------------------");
                suspInformation.AppendLine("SYSTEM USE SHARING PROTOCOL INFORMATION:");
                suspInformation.AppendLine("----------------------------------------");

                counter = 1;
                foreach(byte[] erb in refareas)
                {
                    ReferenceArea er = BigEndianMarshal.ByteArrayToStructureBigEndian<ReferenceArea>(erb);
                    string ext_id = CurrentEncoding.GetString(erb, Marshal.SizeOf(er), er.id_len);
                    string ext_des = CurrentEncoding.GetString(erb, Marshal.SizeOf(er) + er.id_len, er.des_len);
                    string ext_src =
                        CurrentEncoding.GetString(erb, Marshal.SizeOf(er) + er.id_len + er.des_len, er.src_len);
                    suspInformation.AppendFormat("Extension: {0}", counter).AppendLine();
                    suspInformation.AppendFormat("\tID: {0}, version {1}", ext_id, er.ext_ver).AppendLine();
                    suspInformation.AppendFormat("\tDescription: {0}", ext_des).AppendLine();
                    suspInformation.AppendFormat("\tSource: {0}", ext_src).AppendLine();
                    counter++;
                }
            }

            byte[] ipbin_sector = imagePlugin.ReadSector(0 + partition.Start);
            Decoders.Sega.CD.IPBin? SegaCD = Decoders.Sega.CD.DecodeIPBin(ipbin_sector);
            Decoders.Sega.Saturn.IPBin? Saturn = Decoders.Sega.Saturn.DecodeIPBin(ipbin_sector);
            Decoders.Sega.Dreamcast.IPBin? Dreamcast = Decoders.Sega.Dreamcast.DecodeIPBin(ipbin_sector);

            string fsFormat;
            if(HighSierra) fsFormat = "High Sierra Format";
            else if(CDi) fsFormat = "CD-i";
            else fsFormat = "ISO9660";

            ISOMetadata.AppendFormat("{0} file system", fsFormat).AppendLine();
            if(XA) ISOMetadata.AppendLine("CD-ROM XA extensions present.");
            if(Apple) ISOMetadata.AppendLine("Apple extensions present.");
            if(jolietvd != null) ISOMetadata.AppendLine("Joliet extensions present.");
            if(SUSP) ISOMetadata.AppendLine("System Use Sharing Protocol present.");
            if(RRIP) ISOMetadata.AppendLine("Rock Ridge Interchange Protocol present.");
            if(AAIP) ISOMetadata.AppendLine("Arbitrary Attribute Interchange Protocol present.");
            if(ziso) ISOMetadata.AppendLine("zisofs compression present.");
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
            ISOMetadata.AppendFormat("{0}------------------------------", CDi ? "---------------" : "").AppendLine();
            ISOMetadata.AppendFormat("{0}VOLUME DESCRIPTOR INFORMATION:", CDi ? "FILE STRUCTURE " : "").AppendLine();
            ISOMetadata.AppendFormat("{0}------------------------------", CDi ? "---------------" : "").AppendLine();
            ISOMetadata.AppendFormat("System identifier: {0}", decodedVD.SystemIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume identifier: {0}", decodedVD.VolumeIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume set identifier: {0}", decodedVD.VolumeSetIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Publisher identifier: {0}", decodedVD.PublisherIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Data preparer identifier: {0}", decodedVD.DataPreparerIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Application identifier: {0}", decodedVD.ApplicationIdentifier).AppendLine();
            ISOMetadata.AppendFormat("Volume creation date: {0}", decodedVD.CreationTime).AppendLine();
            if(decodedVD.HasModificationTime)
                ISOMetadata.AppendFormat("Volume modification date: {0}", decodedVD.ModificationTime).AppendLine();
            else ISOMetadata.AppendFormat("Volume has not been modified.").AppendLine();
            if(decodedVD.HasExpirationTime)
                ISOMetadata.AppendFormat("Volume expiration date: {0}", decodedVD.ExpirationTime).AppendLine();
            else ISOMetadata.AppendFormat("Volume does not expire.").AppendLine();
            if(decodedVD.HasEffectiveTime)
                ISOMetadata.AppendFormat("Volume effective date: {0}", decodedVD.EffectiveTime).AppendLine();
            else ISOMetadata.AppendFormat("Volume has always been effective.").AppendLine();
            ISOMetadata.AppendFormat("Volume has {0} blocks of {1} bytes each", decodedVD.Blocks, decodedVD.BlockSize)
                       .AppendLine();

            if(jolietvd != null)
            {
                ISOMetadata.AppendLine("-------------------------------------");
                ISOMetadata.AppendLine("JOLIET VOLUME DESCRIPTOR INFORMATION:");
                ISOMetadata.AppendLine("-------------------------------------");
                ISOMetadata.AppendFormat("System identifier: {0}", decodedJolietVD.SystemIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Volume identifier: {0}", decodedJolietVD.VolumeIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Volume set identifier: {0}", decodedJolietVD.VolumeSetIdentifier)
                           .AppendLine();
                ISOMetadata.AppendFormat("Publisher identifier: {0}", decodedJolietVD.PublisherIdentifier).AppendLine();
                ISOMetadata.AppendFormat("Data preparer identifier: {0}", decodedJolietVD.DataPreparerIdentifier)
                           .AppendLine();
                ISOMetadata.AppendFormat("Application identifier: {0}", decodedJolietVD.ApplicationIdentifier)
                           .AppendLine();
                ISOMetadata.AppendFormat("Volume creation date: {0}", decodedJolietVD.CreationTime).AppendLine();
                if(decodedJolietVD.HasModificationTime)
                    ISOMetadata.AppendFormat("Volume modification date: {0}", decodedJolietVD.ModificationTime)
                               .AppendLine();
                else ISOMetadata.AppendFormat("Volume has not been modified.").AppendLine();
                if(decodedJolietVD.HasExpirationTime)
                    ISOMetadata.AppendFormat("Volume expiration date: {0}", decodedJolietVD.ExpirationTime)
                               .AppendLine();
                else ISOMetadata.AppendFormat("Volume does not expire.").AppendLine();
                if(decodedJolietVD.HasEffectiveTime)
                    ISOMetadata.AppendFormat("Volume effective date: {0}", decodedJolietVD.EffectiveTime).AppendLine();
                else ISOMetadata.AppendFormat("Volume has always been effective.").AppendLine();
            }

            if(torito != null)
            {
                vd_sector = imagePlugin.ReadSector(torito.Value.catalog_sector + partition.Start);
                Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
                sha1Ctx.Init();
                byte[] boot_image;

                int torito_off = 0;

                if(vd_sector[torito_off] != 1) goto exit_torito;

                ElToritoValidationEntry valentry = new ElToritoValidationEntry();
                IntPtr ptr = Marshal.AllocHGlobal(ElToritoEntrySize);
                Marshal.Copy(vd_sector, torito_off, ptr, ElToritoEntrySize);
                valentry = (ElToritoValidationEntry)Marshal.PtrToStructure(ptr, typeof(ElToritoValidationEntry));
                Marshal.FreeHGlobal(ptr);

                if(valentry.signature != ElToritoMagic) goto exit_torito;

                torito_off += ElToritoEntrySize;

                ElToritoInitialEntry initial_entry = new ElToritoInitialEntry();
                ptr = Marshal.AllocHGlobal(ElToritoEntrySize);
                Marshal.Copy(vd_sector, torito_off, ptr, ElToritoEntrySize);
                initial_entry = (ElToritoInitialEntry)Marshal.PtrToStructure(ptr, typeof(ElToritoInitialEntry));
                Marshal.FreeHGlobal(ptr);
                initial_entry.boot_type = (ElToritoEmulation)((byte)initial_entry.boot_type & 0xF);

                boot_image =
                    imagePlugin.ReadSectors(initial_entry.load_rba + partition.Start, initial_entry.sector_count);

                ISOMetadata.AppendLine("----------------------");
                ISOMetadata.AppendLine("EL TORITO INFORMATION:");
                ISOMetadata.AppendLine("----------------------");

                ISOMetadata.AppendLine("Initial entry:");
                ISOMetadata.AppendFormat("\tDeveloper ID: {0}", CurrentEncoding.GetString(valentry.developer_id))
                           .AppendLine();
                if(initial_entry.bootable == ElToritoIndicator.Bootable)
                {
                    ISOMetadata.AppendFormat("\tBootable on {0}", valentry.platform_id).AppendLine();
                    ISOMetadata.AppendFormat("\tBootable image starts at sector {0} and runs for {1} sectors",
                                             initial_entry.load_rba, initial_entry.sector_count).AppendLine();
                    if(valentry.platform_id == ElToritoPlatform.x86)
                        ISOMetadata.AppendFormat("\tBootable image will be loaded at segment {0:X4}h",
                                                 initial_entry.load_seg == 0 ? 0x7C0 : initial_entry.load_seg)
                                   .AppendLine();
                    else
                        ISOMetadata.AppendFormat("\tBootable image will be loaded at 0x{0:X8}",
                                                 (uint)initial_entry.load_seg * 10).AppendLine();
                    switch(initial_entry.boot_type)
                    {
                        case ElToritoEmulation.None:
                            ISOMetadata.AppendLine("\tImage uses no emulation");
                            break;
                        case ElToritoEmulation.Md2hd:
                            ISOMetadata.AppendLine("\tImage emulates a 5.25\" high-density (MD2HD, 1.2Mb) floppy");
                            break;
                        case ElToritoEmulation.Mf2hd:
                            ISOMetadata.AppendLine("\tImage emulates a 3.5\" high-density (MF2HD, 1.44Mb) floppy");
                            break;
                        case ElToritoEmulation.Mf2ed:
                            ISOMetadata.AppendLine("\tImage emulates a 3.5\" extra-density (MF2ED, 2.88Mb) floppy");
                            break;
                        default:
                            ISOMetadata.AppendFormat("\tImage uses unknown emulation type {0}",
                                                     (byte)initial_entry.boot_type).AppendLine();
                            break;
                    }

                    ISOMetadata.AppendFormat("\tSystem type: 0x{0:X2}", initial_entry.system_type).AppendLine();
                    ISOMetadata.AppendFormat("\tBootable image's SHA1: {0}", sha1Ctx.Data(boot_image, out byte[] hash))
                               .AppendLine();
                }
                else ISOMetadata.AppendLine("\tNot bootable");

                torito_off += ElToritoEntrySize;

                int section_counter = 2;

                while(torito_off < vd_sector.Length && (vd_sector[torito_off] == (byte)ElToritoIndicator.Header ||
                                                        vd_sector[torito_off] == (byte)ElToritoIndicator.LastHeader))
                {
                    ElToritoSectionHeaderEntry section_header = new ElToritoSectionHeaderEntry();
                    ptr = Marshal.AllocHGlobal(ElToritoEntrySize);
                    Marshal.Copy(vd_sector, torito_off, ptr, ElToritoEntrySize);
                    section_header =
                        (ElToritoSectionHeaderEntry)Marshal.PtrToStructure(ptr, typeof(ElToritoSectionHeaderEntry));
                    Marshal.FreeHGlobal(ptr);
                    torito_off += ElToritoEntrySize;

                    ISOMetadata.AppendFormat("Boot section {0}:", section_counter);
                    ISOMetadata.AppendFormat("\tSection ID: {0}", CurrentEncoding.GetString(section_header.identifier))
                               .AppendLine();

                    for(int entry_counter = 1; entry_counter <= section_header.entries && torito_off < vd_sector.Length;
                        entry_counter++)
                    {
                        ElToritoSectionEntry section_entry = new ElToritoSectionEntry();
                        ptr = Marshal.AllocHGlobal(ElToritoEntrySize);
                        Marshal.Copy(vd_sector, torito_off, ptr, ElToritoEntrySize);
                        section_entry = (ElToritoSectionEntry)Marshal.PtrToStructure(ptr, typeof(ElToritoSectionEntry));
                        Marshal.FreeHGlobal(ptr);
                        torito_off += ElToritoEntrySize;

                        ISOMetadata.AppendFormat("\tEntry {0}:", entry_counter);
                        if(section_entry.bootable == ElToritoIndicator.Bootable)
                        {
                            boot_image =
                                imagePlugin.ReadSectors(section_entry.load_rba + partition.Start,
                                                        section_entry.sector_count);
                            ISOMetadata.AppendFormat("\t\tBootable on {0}", section_header.platform_id).AppendLine();
                            ISOMetadata.AppendFormat("\t\tBootable image starts at sector {0} and runs for {1} sectors",
                                                     section_entry.load_rba, section_entry.sector_count).AppendLine();
                            if(valentry.platform_id == ElToritoPlatform.x86)
                                ISOMetadata.AppendFormat("\t\tBootable image will be loaded at segment {0:X4}h",
                                                         section_entry.load_seg == 0 ? 0x7C0 : section_entry.load_seg)
                                           .AppendLine();
                            else
                                ISOMetadata.AppendFormat("\t\tBootable image will be loaded at 0x{0:X8}",
                                                         (uint)section_entry.load_seg * 10).AppendLine();
                            switch((ElToritoEmulation)((byte)section_entry.boot_type & 0xF))
                            {
                                case ElToritoEmulation.None:
                                    ISOMetadata.AppendLine("\t\tImage uses no emulation");
                                    break;
                                case ElToritoEmulation.Md2hd:
                                    ISOMetadata
                                        .AppendLine("\t\tImage emulates a 5.25\" high-density (MD2HD, 1.2Mb) floppy");
                                    break;
                                case ElToritoEmulation.Mf2hd:
                                    ISOMetadata
                                        .AppendLine("\t\tImage emulates a 3.5\" high-density (MF2HD, 1.44Mb) floppy");
                                    break;
                                case ElToritoEmulation.Mf2ed:
                                    ISOMetadata
                                        .AppendLine("\t\tImage emulates a 3.5\" extra-density (MF2ED, 2.88Mb) floppy");
                                    break;
                                default:
                                    ISOMetadata.AppendFormat("\t\tImage uses unknown emulation type {0}",
                                                             (byte)initial_entry.boot_type).AppendLine();
                                    break;
                            }

                            ISOMetadata.AppendFormat("\t\tSelection criteria type: {0}",
                                                     section_entry.selection_criteria_type).AppendLine();
                            ISOMetadata.AppendFormat("\t\tSystem type: 0x{0:X2}", section_entry.system_type)
                                       .AppendLine();
                            ISOMetadata.AppendFormat("\t\tBootable image's SHA1: {0}",
                                                     sha1Ctx.Data(boot_image, out byte[] hash)).AppendLine();
                        }
                        else ISOMetadata.AppendLine("\t\tNot bootable");

                        ElToritoFlags flags = (ElToritoFlags)((byte)section_entry.boot_type & 0xF0);
                        if(flags.HasFlag(ElToritoFlags.ATAPI))
                            ISOMetadata.AppendLine("\t\tImage contains ATAPI drivers");
                        if(flags.HasFlag(ElToritoFlags.SCSI)) ISOMetadata.AppendLine("\t\tImage contains SCSI drivers");

                        if(flags.HasFlag(ElToritoFlags.Continued))
                        {
                            while(true && torito_off < vd_sector.Length)
                            {
                                ElToritoSectionEntryExtension section_extension = new ElToritoSectionEntryExtension();
                                ptr = Marshal.AllocHGlobal(ElToritoEntrySize);
                                Marshal.Copy(vd_sector, torito_off, ptr, ElToritoEntrySize);
                                section_extension =
                                    (ElToritoSectionEntryExtension)
                                    Marshal.PtrToStructure(ptr, typeof(ElToritoSectionEntryExtension));
                                Marshal.FreeHGlobal(ptr);
                                torito_off += ElToritoEntrySize;

                                if(!section_extension.extension_flags.HasFlag(ElToritoFlags.Continued)) break;
                            }
                        }
                    }

                    if(section_header.header_id == ElToritoIndicator.LastHeader) break;
                }
            }

            exit_torito:
            if(refareas.Count > 0) ISOMetadata.Append(suspInformation.ToString());

            xmlFSType.Type = fsFormat;

            if(jolietvd != null)
            {
                xmlFSType.VolumeName = decodedJolietVD.VolumeIdentifier;

                if(decodedJolietVD.SystemIdentifier == null ||
                   decodedVD.SystemIdentifier.Length > decodedJolietVD.SystemIdentifier.Length)
                    xmlFSType.SystemIdentifier = decodedVD.SystemIdentifier;
                else xmlFSType.SystemIdentifier = decodedJolietVD.SystemIdentifier;

                if(decodedJolietVD.VolumeSetIdentifier == null || decodedVD.VolumeSetIdentifier.Length >
                   decodedJolietVD.VolumeSetIdentifier.Length)
                    xmlFSType.VolumeSetIdentifier = decodedVD.VolumeSetIdentifier;
                else xmlFSType.VolumeSetIdentifier = decodedJolietVD.VolumeSetIdentifier;

                if(decodedJolietVD.PublisherIdentifier == null || decodedVD.PublisherIdentifier.Length >
                   decodedJolietVD.PublisherIdentifier.Length)
                    xmlFSType.PublisherIdentifier = decodedVD.PublisherIdentifier;
                else xmlFSType.PublisherIdentifier = decodedJolietVD.PublisherIdentifier;

                if(decodedJolietVD.DataPreparerIdentifier == null || decodedVD.DataPreparerIdentifier.Length >
                   decodedJolietVD.DataPreparerIdentifier.Length)
                    xmlFSType.DataPreparerIdentifier = decodedVD.DataPreparerIdentifier;
                else xmlFSType.DataPreparerIdentifier = decodedJolietVD.SystemIdentifier;

                if(decodedJolietVD.ApplicationIdentifier == null || decodedVD.ApplicationIdentifier.Length >
                   decodedJolietVD.ApplicationIdentifier.Length)
                    xmlFSType.ApplicationIdentifier = decodedVD.ApplicationIdentifier;
                else xmlFSType.ApplicationIdentifier = decodedJolietVD.SystemIdentifier;

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