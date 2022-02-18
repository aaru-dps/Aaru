// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.Sega;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.Info.SectorSize < 2048)
                return false;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End <= 16 + partition.Start)
                return false;

            // Read to Volume Descriptor
            ErrorNumber errno = imagePlugin.ReadSector(16 + partition.Start, out byte[] vdSector);

            if(errno != ErrorNumber.NoError)
                return false;

            int xaOff = 0;

            if(vdSector.Length == 2336)
                xaOff = 8;

            byte   vdType  = vdSector[0 + xaOff];
            byte[] vdMagic = new byte[5];
            byte[] hsMagic = new byte[5];

            // This indicates the end of a volume descriptor. HighSierra here would have 16 so no problem
            if(vdType == 255)
                return false;

            Array.Copy(vdSector, 0x001 + xaOff, vdMagic, 0, 5);
            Array.Copy(vdSector, 0x009 + xaOff, hsMagic, 0, 5);

            AaruConsole.DebugWriteLine("ISO9660 plugin", "VDMagic = {0}", Encoding.ASCII.GetString(vdMagic));
            AaruConsole.DebugWriteLine("ISO9660 plugin", "HSMagic = {0}", Encoding.ASCII.GetString(hsMagic));

            return Encoding.ASCII.GetString(vdMagic) == ISO_MAGIC         ||
                   Encoding.ASCII.GetString(hsMagic) == HIGH_SIERRA_MAGIC ||
                   Encoding.ASCII.GetString(vdMagic) == CDI_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.ASCII;
            information = "";
            var    isoMetadata = new StringBuilder();
            byte[] vdMagic     = new byte[5]; // Volume Descriptor magic "CD001"
            byte[] hsMagic     = new byte[5]; // Volume Descriptor magic "CDROM"

            string bootSpec = "";

            PrimaryVolumeDescriptor?           pvd      = null;
            PrimaryVolumeDescriptor?           jolietvd = null;
            BootRecord?                        bvd      = null;
            HighSierraPrimaryVolumeDescriptor? hsvd     = null;
            FileStructureVolumeDescriptor?     fsvd     = null;
            ElToritoBootRecord?                torito   = null;

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.Info.SectorSize < 2048)
                return;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End < 16)
                return;

            ulong counter = 0;

            ErrorNumber errno = imagePlugin.ReadSector(16 + counter + partition.Start, out byte[] vdSector);

            if(errno != ErrorNumber.NoError)
                return;

            int xaOff = vdSector.Length == 2336 ? 8 : 0;
            Array.Copy(vdSector, 0x009 + xaOff, hsMagic, 0, 5);
            bool highSierraInfo = Encoding.GetString(hsMagic) == HIGH_SIERRA_MAGIC;
            int  hsOff          = 0;

            if(highSierraInfo)
                hsOff = 8;

            bool cdiInfo = false;
            bool evd     = false;
            bool vpd     = false;

            while(true)
            {
                AaruConsole.DebugWriteLine("ISO9660 plugin", "Processing VD loop no. {0}", counter);

                // Seek to Volume Descriptor
                AaruConsole.DebugWriteLine("ISO9660 plugin", "Reading sector {0}", 16 + counter + partition.Start);
                errno = imagePlugin.ReadSector(16 + counter + partition.Start, out byte[] vdSectorTmp);

                if(errno != ErrorNumber.NoError)
                    return;

                vdSector = new byte[vdSectorTmp.Length - xaOff];
                Array.Copy(vdSectorTmp, xaOff, vdSector, 0, vdSector.Length);

                byte vdType = vdSector[0 + hsOff]; // Volume Descriptor Type, should be 1 or 2.
                AaruConsole.DebugWriteLine("ISO9660 plugin", "VDType = {0}", vdType);

                if(vdType == 255) // Supposedly we are in the PVD.
                {
                    if(counter == 0)
                        return;

                    break;
                }

                Array.Copy(vdSector, 0x001, vdMagic, 0, 5);
                Array.Copy(vdSector, 0x009, hsMagic, 0, 5);

                if(Encoding.GetString(vdMagic) != ISO_MAGIC         &&
                   Encoding.GetString(hsMagic) != HIGH_SIERRA_MAGIC &&
                   Encoding.GetString(vdMagic) !=
                   CDI_MAGIC) // Recognized, it is an ISO9660, now check for rest of data.
                {
                    if(counter == 0)
                        return;

                    break;
                }

                cdiInfo |= Encoding.GetString(vdMagic) == CDI_MAGIC;

                switch(vdType)
                {
                    case 0:
                    {
                        bvd = Marshal.ByteArrayToStructureLittleEndian<BootRecord>(vdSector, hsOff, 2048 - hsOff);

                        bootSpec = "Unknown";

                        if(Encoding.GetString(bvd.Value.system_id).Substring(0, 23) == "EL TORITO SPECIFICATION")
                        {
                            bootSpec = "El Torito";

                            torito =
                                Marshal.ByteArrayToStructureLittleEndian<ElToritoBootRecord>(vdSector, hsOff,
                                    2048 - hsOff);
                        }

                        break;
                    }

                    case 1:
                    {
                        if(highSierraInfo)
                            hsvd = Marshal.
                                ByteArrayToStructureLittleEndian<HighSierraPrimaryVolumeDescriptor>(vdSector);
                        else if(cdiInfo)
                            fsvd = Marshal.ByteArrayToStructureBigEndian<FileStructureVolumeDescriptor>(vdSector);
                        else
                            pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(vdSector);

                        break;
                    }

                    case 2:
                    {
                        PrimaryVolumeDescriptor svd =
                            Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(vdSector);

                        // Check if this is Joliet
                        if(svd.version == 1)
                        {
                            if(svd.escape_sequences[0] == '%' &&
                               svd.escape_sequences[1] == '/')
                                if(svd.escape_sequences[2] == '@' ||
                                   svd.escape_sequences[2] == 'C' ||
                                   svd.escape_sequences[2] == 'E')
                                    jolietvd = svd;
                                else
                                    AaruConsole.WriteLine("ISO9660 plugin",
                                                          "Found unknown supplementary volume descriptor");
                        }
                        else
                            evd = true;

                        break;
                    }

                    case 3:
                    {
                        vpd = true;

                        break;
                    }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVd;
            var                     decodedJolietVd = new DecodedVolumeDescriptor();

            XmlFsType = new FileSystemType();

            if(pvd  == null &&
               hsvd == null &&
               fsvd == null)
            {
                information = "ERROR: Could not find primary volume descriptor";

                return;
            }

            if(highSierraInfo)
                decodedVd = DecodeVolumeDescriptor(hsvd.Value);
            else if(cdiInfo)
                decodedVd = DecodeVolumeDescriptor(fsvd.Value);
            else
                decodedVd = DecodeVolumeDescriptor(pvd.Value);

            if(jolietvd != null)
                decodedJolietVd = DecodeJolietDescriptor(jolietvd.Value);

            uint rootLocation = 0;
            uint rootSize     = 0;

            // No need to read root on CD-i, as extensions are not supported...
            if(!cdiInfo)
            {
                rootLocation = highSierraInfo ? hsvd.Value.root_directory_record.extent
                                   : pvd.Value.root_directory_record.extent;

                if(highSierraInfo)
                {
                    rootSize = hsvd.Value.root_directory_record.size / hsvd.Value.logical_block_size;

                    if(hsvd.Value.root_directory_record.size % hsvd.Value.logical_block_size > 0)
                        rootSize++;
                }
                else
                {
                    rootSize = pvd.Value.root_directory_record.size / pvd.Value.logical_block_size;

                    if(pvd.Value.root_directory_record.size % pvd.Value.logical_block_size > 0)
                        rootSize++;
                }
            }

            byte[]                 rootDir         = Array.Empty<byte>();
            int                    rootOff         = 0;
            bool                   xaExtensions    = false;
            bool                   apple           = false;
            bool                   susp            = false;
            bool                   rrip            = false;
            bool                   ziso            = false;
            bool                   amiga           = false;
            bool                   aaip            = false;
            List<ContinuationArea> contareas       = new();
            List<byte[]>           refareas        = new();
            var                    suspInformation = new StringBuilder();

            if(rootLocation + rootSize < imagePlugin.Info.Sectors)
            {
                errno = imagePlugin.ReadSectors(rootLocation, rootSize, out rootDir);

                if(errno != ErrorNumber.NoError)
                    return;
            }

            // Walk thru root directory to see system area extensions in use
            while(rootOff + Marshal.SizeOf<DirectoryRecord>() < rootDir.Length &&
                  !cdiInfo)
            {
                DirectoryRecord record =
                    Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(rootDir, rootOff,
                                                                              Marshal.SizeOf<DirectoryRecord>());

                int saOff = Marshal.SizeOf<DirectoryRecord>() + record.name_len;
                saOff += saOff % 2;
                int saLen = record.length - saOff;

                if(saLen                   > 0 &&
                   rootOff + saOff + saLen <= rootDir.Length)
                {
                    byte[] sa = new byte[saLen];
                    Array.Copy(rootDir, rootOff + saOff, sa, 0, saLen);
                    saOff = 0;

                    while(saOff < saLen)
                    {
                        bool noneFound = true;

                        if(Marshal.SizeOf<CdromXa>() + saOff <= saLen)
                        {
                            CdromXa xa = Marshal.ByteArrayToStructureBigEndian<CdromXa>(sa);

                            if(xa.signature == XA_MAGIC)
                            {
                                xaExtensions =  true;
                                saOff        += Marshal.SizeOf<CdromXa>();
                                noneFound    =  false;
                            }
                        }

                        if(saOff + 2 >= saLen)
                            break;

                        ushort nextSignature = BigEndianBitConverter.ToUInt16(sa, saOff);

                        switch(nextSignature)
                        {
                            // Easy, contains size field
                            case APPLE_MAGIC:
                                apple     =  true;
                                saOff     += sa[saOff + 2];
                                noneFound =  false;

                                break;

                            // Not easy, contains size field
                            case APPLE_MAGIC_OLD:
                                apple = true;
                                var appleId = (AppleOldId)sa[saOff + 2];
                                noneFound = false;

                                switch(appleId)
                                {
                                    case AppleOldId.ProDOS:
                                        saOff += Marshal.SizeOf<AppleProDOSOldSystemUse>();

                                        break;
                                    case AppleOldId.TypeCreator:
                                    case AppleOldId.TypeCreatorBundle:
                                        saOff += Marshal.SizeOf<AppleHFSTypeCreatorSystemUse>();

                                        break;
                                    case AppleOldId.TypeCreatorIcon:
                                    case AppleOldId.TypeCreatorIconBundle:
                                        saOff += Marshal.SizeOf<AppleHFSIconSystemUse>();

                                        break;
                                    case AppleOldId.HFS:
                                        saOff += Marshal.SizeOf<AppleHFSOldSystemUse>();

                                        break;
                                }

                                break;

                            // IEEE-P1281 aka SUSP 1.12
                            case SUSP_INDICATOR:
                                susp      =  true;
                                saOff     += sa[saOff + 2];
                                noneFound =  false;

                                while(saOff + 2 < saLen)
                                {
                                    nextSignature = BigEndianBitConverter.ToUInt16(sa, saOff);

                                    switch(nextSignature)
                                    {
                                        case APPLE_MAGIC:
                                            if(sa[saOff + 3] == 1 &&
                                               sa[saOff + 2] == 7)
                                                apple = true;
                                            else
                                                apple |= sa[saOff + 3] != 1;

                                            break;
                                        case SUSP_CONTINUATION when saOff + sa[saOff + 2] <= saLen:
                                            byte[] ce = new byte[sa[saOff + 2]];
                                            Array.Copy(sa, saOff, ce, 0, ce.Length);

                                            ContinuationArea ca =
                                                Marshal.ByteArrayToStructureBigEndian<ContinuationArea>(ce);

                                            contareas.Add(ca);

                                            break;
                                        case SUSP_REFERENCE when saOff + sa[saOff + 2] <= saLen:
                                            byte[] er = new byte[sa[saOff + 2]];
                                            Array.Copy(sa, saOff, er, 0, er.Length);
                                            refareas.Add(er);

                                            break;
                                    }

                                    rrip |= nextSignature == RRIP_MAGIC || nextSignature == RRIP_POSIX_ATTRIBUTES ||
                                            nextSignature == RRIP_POSIX_DEV_NO || nextSignature == RRIP_SYMLINK ||
                                            nextSignature == RRIP_NAME || nextSignature == RRIP_CHILDLINK ||
                                            nextSignature == RRIP_PARENTLINK || nextSignature == RRIP_RELOCATED_DIR ||
                                            nextSignature == RRIP_TIMESTAMPS || nextSignature == RRIP_SPARSE;

                                    ziso  |= nextSignature == ZISO_MAGIC;
                                    amiga |= nextSignature == AMIGA_MAGIC;

                                    aaip |= nextSignature == AAIP_MAGIC || (nextSignature == AAIP_MAGIC_OLD &&
                                                                            sa[saOff + 3] == 1 && sa[saOff + 2] >= 9);

                                    saOff += sa[saOff + 2];

                                    if(nextSignature == SUSP_TERMINATOR)
                                        break;
                                }

                                break;
                        }

                        if(noneFound)
                            break;
                    }
                }

                rootOff += record.length;

                if(record.length == 0)
                    break;
            }

            foreach(ContinuationArea ca in contareas)
            {
                uint caLen = (ca.ca_length_be + ca.offset_be) /
                             (highSierraInfo ? hsvd.Value.logical_block_size : pvd.Value.logical_block_size);

                if((ca.ca_length_be + ca.offset_be) %
                   (highSierraInfo ? hsvd.Value.logical_block_size : pvd.Value.logical_block_size) > 0)
                    caLen++;

                errno = imagePlugin.ReadSectors(ca.block_be, caLen, out byte[] caSectors);

                if(errno != ErrorNumber.NoError)
                    return;

                byte[] caData = new byte[ca.ca_length_be];
                Array.Copy(caSectors, ca.offset_be, caData, 0, ca.ca_length_be);
                int caOff = 0;

                while(caOff < ca.ca_length_be)
                {
                    ushort nextSignature = BigEndianBitConverter.ToUInt16(caData, caOff);

                    switch(nextSignature)
                    {
                        // Apple never said to include its extensions inside a continuation area, but just in case
                        case APPLE_MAGIC:
                            if(caData[caOff + 3] == 1 &&
                               caData[caOff + 2] == 7)
                                apple = true;
                            else
                                apple |= caData[caOff + 3] != 1;

                            break;
                        case SUSP_REFERENCE when caOff + caData[caOff + 2] <= ca.ca_length_be:
                            byte[] er = new byte[caData[caOff + 2]];
                            Array.Copy(caData, caOff, er, 0, er.Length);
                            refareas.Add(er);

                            break;
                    }

                    rrip |= nextSignature == RRIP_MAGIC        || nextSignature == RRIP_POSIX_ATTRIBUTES ||
                            nextSignature == RRIP_POSIX_DEV_NO || nextSignature == RRIP_SYMLINK          ||
                            nextSignature == RRIP_NAME         || nextSignature == RRIP_CHILDLINK        ||
                            nextSignature == RRIP_PARENTLINK   || nextSignature == RRIP_RELOCATED_DIR    ||
                            nextSignature == RRIP_TIMESTAMPS   || nextSignature == RRIP_SPARSE;

                    ziso  |= nextSignature == ZISO_MAGIC;
                    amiga |= nextSignature == AMIGA_MAGIC;

                    aaip |= nextSignature == AAIP_MAGIC || (nextSignature == AAIP_MAGIC_OLD && caData[caOff + 3] == 1 &&
                                                            caData[caOff                                    + 2] >= 9);

                    caOff += caData[caOff + 2];
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
                    ReferenceArea er    = Marshal.ByteArrayToStructureBigEndian<ReferenceArea>(erb);
                    string        extId = Encoding.GetString(erb, Marshal.SizeOf<ReferenceArea>(), er.id_len);

                    string extDes = Encoding.GetString(erb, Marshal.SizeOf<ReferenceArea>() + er.id_len, er.des_len);

                    string extSrc = Encoding.GetString(erb, Marshal.SizeOf<ReferenceArea>() + er.id_len + er.des_len,
                                                       er.src_len);

                    suspInformation.AppendFormat("Extension: {0}", counter).AppendLine();
                    suspInformation.AppendFormat("\tID: {0}, version {1}", extId, er.ext_ver).AppendLine();
                    suspInformation.AppendFormat("\tDescription: {0}", extDes).AppendLine();
                    suspInformation.AppendFormat("\tSource: {0}", extSrc).AppendLine();
                    counter++;
                }
            }

            errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] ipbinSector);

            if(errno != ErrorNumber.NoError)
                return;

            CD.IPBin?        segaCd    = CD.DecodeIPBin(ipbinSector);
            Saturn.IPBin?    saturn    = Saturn.DecodeIPBin(ipbinSector);
            Dreamcast.IPBin? dreamcast = Dreamcast.DecodeIPBin(ipbinSector);

            string fsFormat;

            if(highSierraInfo)
                fsFormat = "High Sierra Format";
            else if(cdiInfo)
                fsFormat = "CD-i";
            else
                fsFormat = "ISO9660";

            isoMetadata.AppendFormat("{0} file system", fsFormat).AppendLine();

            if(xaExtensions)
                isoMetadata.AppendLine("CD-ROM XA extensions present.");

            if(amiga)
                isoMetadata.AppendLine("Amiga extensions present.");

            if(apple)
                isoMetadata.AppendLine("Apple extensions present.");

            if(jolietvd != null)
                isoMetadata.AppendLine("Joliet extensions present.");

            if(susp)
                isoMetadata.AppendLine("System Use Sharing Protocol present.");

            if(rrip)
                isoMetadata.AppendLine("Rock Ridge Interchange Protocol present.");

            if(aaip)
                isoMetadata.AppendLine("Arbitrary Attribute Interchange Protocol present.");

            if(ziso)
                isoMetadata.AppendLine("zisofs compression present.");

            if(evd)
                isoMetadata.AppendLine("Contains Enhanved Volume Descriptor.");

            if(vpd)
                isoMetadata.AppendLine("Contains Volume Partition Descriptor.");

            if(bvd != null)
                isoMetadata.AppendFormat("Disc bootable following {0} specifications.", bootSpec).AppendLine();

            if(segaCd != null)
            {
                isoMetadata.AppendLine("This is a SegaCD / MegaCD disc.");
                isoMetadata.AppendLine(CD.Prettify(segaCd));
            }

            if(saturn != null)
            {
                isoMetadata.AppendLine("This is a Sega Saturn disc.");
                isoMetadata.AppendLine(Saturn.Prettify(saturn));
            }

            if(dreamcast != null)
            {
                isoMetadata.AppendLine("This is a Sega Dreamcast disc.");
                isoMetadata.AppendLine(Dreamcast.Prettify(dreamcast));
            }

            isoMetadata.AppendFormat("{0}------------------------------", cdiInfo ? "---------------" : "").
                        AppendLine();

            isoMetadata.AppendFormat("{0}VOLUME DESCRIPTOR INFORMATION:", cdiInfo ? "FILE STRUCTURE " : "").
                        AppendLine();

            isoMetadata.AppendFormat("{0}------------------------------", cdiInfo ? "---------------" : "").
                        AppendLine();

            isoMetadata.AppendFormat("System identifier: {0}", decodedVd.SystemIdentifier).AppendLine();
            isoMetadata.AppendFormat("Volume identifier: {0}", decodedVd.VolumeIdentifier).AppendLine();
            isoMetadata.AppendFormat("Volume set identifier: {0}", decodedVd.VolumeSetIdentifier).AppendLine();
            isoMetadata.AppendFormat("Publisher identifier: {0}", decodedVd.PublisherIdentifier).AppendLine();
            isoMetadata.AppendFormat("Data preparer identifier: {0}", decodedVd.DataPreparerIdentifier).AppendLine();
            isoMetadata.AppendFormat("Application identifier: {0}", decodedVd.ApplicationIdentifier).AppendLine();
            isoMetadata.AppendFormat("Volume creation date: {0}", decodedVd.CreationTime).AppendLine();

            if(decodedVd.HasModificationTime)
                isoMetadata.AppendFormat("Volume modification date: {0}", decodedVd.ModificationTime).AppendLine();
            else
                isoMetadata.AppendFormat("Volume has not been modified.").AppendLine();

            if(decodedVd.HasExpirationTime)
                isoMetadata.AppendFormat("Volume expiration date: {0}", decodedVd.ExpirationTime).AppendLine();
            else
                isoMetadata.AppendFormat("Volume does not expire.").AppendLine();

            if(decodedVd.HasEffectiveTime)
                isoMetadata.AppendFormat("Volume effective date: {0}", decodedVd.EffectiveTime).AppendLine();
            else
                isoMetadata.AppendFormat("Volume has always been effective.").AppendLine();

            isoMetadata.AppendFormat("Volume has {0} blocks of {1} bytes each", decodedVd.Blocks, decodedVd.BlockSize).
                        AppendLine();

            if(jolietvd != null)
            {
                isoMetadata.AppendLine("-------------------------------------");
                isoMetadata.AppendLine("JOLIET VOLUME DESCRIPTOR INFORMATION:");
                isoMetadata.AppendLine("-------------------------------------");
                isoMetadata.AppendFormat("System identifier: {0}", decodedJolietVd.SystemIdentifier).AppendLine();
                isoMetadata.AppendFormat("Volume identifier: {0}", decodedJolietVd.VolumeIdentifier).AppendLine();

                isoMetadata.AppendFormat("Volume set identifier: {0}", decodedJolietVd.VolumeSetIdentifier).
                            AppendLine();

                isoMetadata.AppendFormat("Publisher identifier: {0}", decodedJolietVd.PublisherIdentifier).AppendLine();

                isoMetadata.AppendFormat("Data preparer identifier: {0}", decodedJolietVd.DataPreparerIdentifier).
                            AppendLine();

                isoMetadata.AppendFormat("Application identifier: {0}", decodedJolietVd.ApplicationIdentifier).
                            AppendLine();

                isoMetadata.AppendFormat("Volume creation date: {0}", decodedJolietVd.CreationTime).AppendLine();

                if(decodedJolietVd.HasModificationTime)
                    isoMetadata.AppendFormat("Volume modification date: {0}", decodedJolietVd.ModificationTime).
                                AppendLine();
                else
                    isoMetadata.AppendFormat("Volume has not been modified.").AppendLine();

                if(decodedJolietVd.HasExpirationTime)
                    isoMetadata.AppendFormat("Volume expiration date: {0}", decodedJolietVd.ExpirationTime).
                                AppendLine();
                else
                    isoMetadata.AppendFormat("Volume does not expire.").AppendLine();

                if(decodedJolietVd.HasEffectiveTime)
                    isoMetadata.AppendFormat("Volume effective date: {0}", decodedJolietVd.EffectiveTime).AppendLine();
                else
                    isoMetadata.AppendFormat("Volume has always been effective.").AppendLine();
            }

            if(torito != null)
            {
                errno = imagePlugin.ReadSector(torito.Value.catalog_sector + partition.Start, out vdSector);

                if(errno != ErrorNumber.NoError)
                    return;

                int toritoOff = 0;

                if(vdSector[toritoOff] != 1)
                    goto exit_torito;

                ElToritoValidationEntry valentry =
                    Marshal.ByteArrayToStructureLittleEndian<ElToritoValidationEntry>(vdSector, toritoOff,
                        EL_TORITO_ENTRY_SIZE);

                if(valentry.signature != EL_TORITO_MAGIC)
                    goto exit_torito;

                toritoOff = EL_TORITO_ENTRY_SIZE;

                ElToritoInitialEntry initialEntry =
                    Marshal.ByteArrayToStructureLittleEndian<ElToritoInitialEntry>(vdSector, toritoOff,
                        EL_TORITO_ENTRY_SIZE);

                initialEntry.boot_type = (ElToritoEmulation)((byte)initialEntry.boot_type & 0xF);

                AaruConsole.DebugWriteLine("DEBUG (ISO9660 plugin)", "initialEntry.load_rba = {0}",
                                           initialEntry.load_rba);

                AaruConsole.DebugWriteLine("DEBUG (ISO9660 plugin)", "initialEntry.sector_count = {0}",
                                           initialEntry.sector_count);

                byte[] bootImage = null;

                if(initialEntry.load_rba + partition.Start + initialEntry.sector_count - 1 <= partition.End)
                    imagePlugin.ReadSectors(initialEntry.load_rba + partition.Start, initialEntry.sector_count,
                                            out bootImage);

                isoMetadata.AppendLine("----------------------");
                isoMetadata.AppendLine("EL TORITO INFORMATION:");
                isoMetadata.AppendLine("----------------------");

                isoMetadata.AppendLine("Initial entry:");
                isoMetadata.AppendFormat("\tDeveloper ID: {0}", Encoding.GetString(valentry.developer_id)).AppendLine();

                if(initialEntry.bootable == ElToritoIndicator.Bootable)
                {
                    isoMetadata.AppendFormat("\tBootable on {0}", valentry.platform_id).AppendLine();

                    isoMetadata.AppendFormat("\tBootable image starts at sector {0} and runs for {1} sectors",
                                             initialEntry.load_rba, initialEntry.sector_count).AppendLine();

                    if(valentry.platform_id == ElToritoPlatform.x86)
                        isoMetadata.AppendFormat("\tBootable image will be loaded at segment {0:X4}h",
                                                 initialEntry.load_seg == 0 ? 0x7C0 : initialEntry.load_seg).
                                    AppendLine();
                    else
                        isoMetadata.AppendFormat("\tBootable image will be loaded at 0x{0:X8}",
                                                 (uint)initialEntry.load_seg * 10).AppendLine();

                    switch(initialEntry.boot_type)
                    {
                        case ElToritoEmulation.None:
                            isoMetadata.AppendLine("\tImage uses no emulation");

                            break;
                        case ElToritoEmulation.Md2Hd:
                            isoMetadata.AppendLine("\tImage emulates a 5.25\" high-density (MD2HD, 1.2Mb) floppy");

                            break;
                        case ElToritoEmulation.Mf2Hd:
                            isoMetadata.AppendLine("\tImage emulates a 3.5\" high-density (MF2HD, 1.44Mb) floppy");

                            break;
                        case ElToritoEmulation.Mf2Ed:
                            isoMetadata.AppendLine("\tImage emulates a 3.5\" extra-density (MF2ED, 2.88Mb) floppy");

                            break;
                        default:
                            isoMetadata.AppendFormat("\tImage uses unknown emulation type {0}",
                                                     (byte)initialEntry.boot_type).AppendLine();

                            break;
                    }

                    isoMetadata.AppendFormat("\tSystem type: 0x{0:X2}", initialEntry.system_type).AppendLine();

                    if(bootImage != null)
                        isoMetadata.AppendFormat("\tBootable image's SHA1: {0}", Sha1Context.Data(bootImage, out _)).
                                    AppendLine();
                }
                else
                    isoMetadata.AppendLine("\tNot bootable");

                toritoOff += EL_TORITO_ENTRY_SIZE;

                const int sectionCounter = 2;

                while(toritoOff < vdSector.Length &&
                      (vdSector[toritoOff] == (byte)ElToritoIndicator.Header ||
                       vdSector[toritoOff] == (byte)ElToritoIndicator.LastHeader))
                {
                    ElToritoSectionHeaderEntry sectionHeader =
                        Marshal.ByteArrayToStructureLittleEndian<ElToritoSectionHeaderEntry>(vdSector, toritoOff,
                            EL_TORITO_ENTRY_SIZE);

                    toritoOff += EL_TORITO_ENTRY_SIZE;

                    isoMetadata.AppendFormat("Boot section {0}:", sectionCounter);

                    isoMetadata.AppendFormat("\tSection ID: {0}", Encoding.GetString(sectionHeader.identifier)).
                                AppendLine();

                    for(int entryCounter = 1; entryCounter <= sectionHeader.entries && toritoOff < vdSector.Length;
                        entryCounter++)
                    {
                        ElToritoSectionEntry sectionEntry =
                            Marshal.ByteArrayToStructureLittleEndian<ElToritoSectionEntry>(vdSector, toritoOff,
                                EL_TORITO_ENTRY_SIZE);

                        toritoOff += EL_TORITO_ENTRY_SIZE;

                        isoMetadata.AppendFormat("\tEntry {0}:", entryCounter);

                        if(sectionEntry.bootable == ElToritoIndicator.Bootable)
                        {
                            bootImage = null;

                            if(sectionEntry.load_rba + partition.Start + sectionEntry.sector_count - 1 <= partition.End)
                                imagePlugin.ReadSectors(sectionEntry.load_rba + partition.Start,
                                                        sectionEntry.sector_count, out bootImage);

                            isoMetadata.AppendFormat("\t\tBootable on {0}", sectionHeader.platform_id).AppendLine();

                            isoMetadata.AppendFormat("\t\tBootable image starts at sector {0} and runs for {1} sectors",
                                                     sectionEntry.load_rba, sectionEntry.sector_count).AppendLine();

                            if(valentry.platform_id == ElToritoPlatform.x86)
                                isoMetadata.AppendFormat("\t\tBootable image will be loaded at segment {0:X4}h",
                                                         sectionEntry.load_seg == 0 ? 0x7C0 : sectionEntry.load_seg).
                                            AppendLine();
                            else
                                isoMetadata.AppendFormat("\t\tBootable image will be loaded at 0x{0:X8}",
                                                         (uint)sectionEntry.load_seg * 10).AppendLine();

                            switch((ElToritoEmulation)((byte)sectionEntry.boot_type & 0xF))
                            {
                                case ElToritoEmulation.None:
                                    isoMetadata.AppendLine("\t\tImage uses no emulation");

                                    break;
                                case ElToritoEmulation.Md2Hd:
                                    isoMetadata.
                                        AppendLine("\t\tImage emulates a 5.25\" high-density (MD2HD, 1.2Mb) floppy");

                                    break;
                                case ElToritoEmulation.Mf2Hd:
                                    isoMetadata.
                                        AppendLine("\t\tImage emulates a 3.5\" high-density (MF2HD, 1.44Mb) floppy");

                                    break;
                                case ElToritoEmulation.Mf2Ed:
                                    isoMetadata.
                                        AppendLine("\t\tImage emulates a 3.5\" extra-density (MF2ED, 2.88Mb) floppy");

                                    break;
                                default:
                                    isoMetadata.AppendFormat("\t\tImage uses unknown emulation type {0}",
                                                             (byte)initialEntry.boot_type).AppendLine();

                                    break;
                            }

                            isoMetadata.AppendFormat("\t\tSelection criteria type: {0}",
                                                     sectionEntry.selection_criteria_type).AppendLine();

                            isoMetadata.AppendFormat("\t\tSystem type: 0x{0:X2}", sectionEntry.system_type).
                                        AppendLine();

                            if(bootImage != null)
                                isoMetadata.AppendFormat("\t\tBootable image's SHA1: {0}",
                                                         Sha1Context.Data(bootImage, out _)).AppendLine();
                        }
                        else
                            isoMetadata.AppendLine("\t\tNot bootable");

                        var flags = (ElToritoFlags)((byte)sectionEntry.boot_type & 0xF0);

                        if(flags.HasFlag(ElToritoFlags.ATAPI))
                            isoMetadata.AppendLine("\t\tImage contains ATAPI drivers");

                        if(flags.HasFlag(ElToritoFlags.SCSI))
                            isoMetadata.AppendLine("\t\tImage contains SCSI drivers");

                        if(!flags.HasFlag(ElToritoFlags.Continued))
                            continue;

                        while(toritoOff < vdSector.Length)
                        {
                            ElToritoSectionEntryExtension sectionExtension =
                                Marshal.ByteArrayToStructureLittleEndian<ElToritoSectionEntryExtension>(vdSector,
                                    toritoOff, EL_TORITO_ENTRY_SIZE);

                            toritoOff += EL_TORITO_ENTRY_SIZE;

                            if(!sectionExtension.extension_flags.HasFlag(ElToritoFlags.Continued))
                                break;
                        }
                    }

                    if(sectionHeader.header_id == ElToritoIndicator.LastHeader)
                        break;
                }
            }

            exit_torito:

            if(refareas.Count > 0)
                isoMetadata.Append(suspInformation);

            XmlFsType.Type = fsFormat;

            if(jolietvd != null)
            {
                XmlFsType.VolumeName = decodedJolietVd.VolumeIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier) ||
                   decodedVd.SystemIdentifier.Length > decodedJolietVd.SystemIdentifier.Length)
                    XmlFsType.SystemIdentifier = decodedVd.SystemIdentifier;
                else
                    XmlFsType.SystemIdentifier = string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier) ? null
                                                     : decodedJolietVd.SystemIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier) ||
                   decodedVd.VolumeSetIdentifier.Length > decodedJolietVd.VolumeSetIdentifier.Length)
                    XmlFsType.VolumeSetIdentifier = decodedVd.VolumeSetIdentifier;
                else
                    XmlFsType.VolumeSetIdentifier = string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier) ? null
                                                        : decodedJolietVd.VolumeSetIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier) ||
                   decodedVd.PublisherIdentifier.Length > decodedJolietVd.PublisherIdentifier.Length)
                    XmlFsType.PublisherIdentifier = decodedVd.PublisherIdentifier;
                else
                    XmlFsType.PublisherIdentifier = string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier) ? null
                                                        : decodedJolietVd.PublisherIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier) ||
                   decodedVd.DataPreparerIdentifier.Length > decodedJolietVd.DataPreparerIdentifier.Length)
                    XmlFsType.DataPreparerIdentifier = decodedVd.DataPreparerIdentifier;
                else
                    XmlFsType.DataPreparerIdentifier = string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier)
                                                           ? null : decodedJolietVd.DataPreparerIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier) ||
                   decodedVd.ApplicationIdentifier.Length > decodedJolietVd.ApplicationIdentifier.Length)
                    XmlFsType.ApplicationIdentifier = decodedVd.ApplicationIdentifier;
                else
                    XmlFsType.ApplicationIdentifier = string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier) ? null
                                                          : decodedJolietVd.ApplicationIdentifier;

                XmlFsType.CreationDate          = decodedJolietVd.CreationTime;
                XmlFsType.CreationDateSpecified = true;

                if(decodedJolietVd.HasModificationTime)
                {
                    XmlFsType.ModificationDate          = decodedJolietVd.ModificationTime;
                    XmlFsType.ModificationDateSpecified = true;
                }

                if(decodedJolietVd.HasExpirationTime)
                {
                    XmlFsType.ExpirationDate          = decodedJolietVd.ExpirationTime;
                    XmlFsType.ExpirationDateSpecified = true;
                }

                if(decodedJolietVd.HasEffectiveTime)
                {
                    XmlFsType.EffectiveDate          = decodedJolietVd.EffectiveTime;
                    XmlFsType.EffectiveDateSpecified = true;
                }
            }
            else
            {
                XmlFsType.SystemIdentifier       = decodedVd.SystemIdentifier;
                XmlFsType.VolumeName             = decodedVd.VolumeIdentifier;
                XmlFsType.VolumeSetIdentifier    = decodedVd.VolumeSetIdentifier;
                XmlFsType.PublisherIdentifier    = decodedVd.PublisherIdentifier;
                XmlFsType.DataPreparerIdentifier = decodedVd.DataPreparerIdentifier;
                XmlFsType.ApplicationIdentifier  = decodedVd.ApplicationIdentifier;
                XmlFsType.CreationDate           = decodedVd.CreationTime;
                XmlFsType.CreationDateSpecified  = true;

                if(decodedVd.HasModificationTime)
                {
                    XmlFsType.ModificationDate          = decodedVd.ModificationTime;
                    XmlFsType.ModificationDateSpecified = true;
                }

                if(decodedVd.HasExpirationTime)
                {
                    XmlFsType.ExpirationDate          = decodedVd.ExpirationTime;
                    XmlFsType.ExpirationDateSpecified = true;
                }

                if(decodedVd.HasEffectiveTime)
                {
                    XmlFsType.EffectiveDate          = decodedVd.EffectiveTime;
                    XmlFsType.EffectiveDateSpecified = true;
                }
            }

            XmlFsType.Bootable    |= bvd != null || segaCd != null || saturn != null || dreamcast != null;
            XmlFsType.Clusters    =  decodedVd.Blocks;
            XmlFsType.ClusterSize =  decodedVd.BlockSize;

            information = isoMetadata.ToString();
        }
    }
}