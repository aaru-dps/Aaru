using System;
using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Decoders.Sega;
using DiscImageChef.Helpers;
using Schemas;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options,     string    @namespace)
        {
            Encoding = encoding ?? Encoding.GetEncoding(1252);
            byte[] vdMagic = new byte[5]; // Volume Descriptor magic "CD001"
            byte[] hsMagic = new byte[5]; // Volume Descriptor magic "CDROM"

            if(options == null) options = GetDefaultOptions();
            if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out debug);
            if(options.TryGetValue("use_path_table", out string usePathTableString))
                bool.TryParse(usePathTableString, out usePathTable);
            if(options.TryGetValue("use_trans_tbl", out string useTransTblString))
                bool.TryParse(useTransTblString, out useTransTbl);
            if(options.TryGetValue("use_evd", out string useEvdString)) bool.TryParse(useEvdString, out useEvd);

            // Default namespace
            if(@namespace is null) @namespace = "joliet";

            switch(@namespace.ToLowerInvariant())
            {
                case "normal":
                    this.@namespace = Namespace.Normal;
                    break;
                case "vms":
                    this.@namespace = Namespace.Vms;
                    break;
                case "joliet":
                    this.@namespace = Namespace.Joliet;
                    break;
                case "rrip":
                    this.@namespace = Namespace.Rrip;
                    break;
                case "romeo":
                    this.@namespace = Namespace.Romeo;
                    break;
                default: return Errno.InvalidArgument;
            }

            PrimaryVolumeDescriptor?           pvd      = null;
            PrimaryVolumeDescriptor?           jolietvd = null;
            BootRecord?                        bvd      = null;
            HighSierraPrimaryVolumeDescriptor? hsvd     = null;
            FileStructureVolumeDescriptor?     fsvd     = null;

            // ISO9660 is designed for 2048 bytes/sector devices
            if(imagePlugin.Info.SectorSize < 2048) return Errno.InvalidArgument;

            // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
            if(partition.End < 16) return Errno.InvalidArgument;

            ulong counter = 0;

            byte[] vdSector = imagePlugin.ReadSector(16 + counter + partition.Start);
            int    xaOff    = vdSector.Length == 2336 ? 8 : 0;
            Array.Copy(vdSector, 0x009 + xaOff, hsMagic, 0, 5);
            highSierra = Encoding.GetString(hsMagic) == HIGH_SIERRA_MAGIC;
            int hsOff            = 0;
            if(highSierra) hsOff = 8;
            cdi = false;
            List<ulong> bvdSectors = new List<ulong>();
            List<ulong> pvdSectors = new List<ulong>();
            List<ulong> svdSectors = new List<ulong>();
            List<ulong> evdSectors = new List<ulong>();
            List<ulong> vpdSectors = new List<ulong>();

            while(true)
            {
                DicConsole.DebugWriteLine("ISO9660 plugin", "Processing VD loop no. {0}", counter);
                // Seek to Volume Descriptor
                DicConsole.DebugWriteLine("ISO9660 plugin", "Reading sector {0}", 16 + counter + partition.Start);
                byte[] vdSectorTmp = imagePlugin.ReadSector(16 + counter + partition.Start);
                vdSector = new byte[vdSectorTmp.Length - xaOff];
                Array.Copy(vdSectorTmp, xaOff, vdSector, 0, vdSector.Length);

                byte vdType = vdSector[0 + hsOff]; // Volume Descriptor Type, should be 1 or 2.
                DicConsole.DebugWriteLine("ISO9660 plugin", "VDType = {0}", vdType);

                if(vdType == 255) // Supposedly we are in the PVD.
                {
                    if(counter == 0) return Errno.InvalidArgument;

                    break;
                }

                Array.Copy(vdSector, 0x001, vdMagic, 0, 5);
                Array.Copy(vdSector, 0x009, hsMagic, 0, 5);

                if(Encoding.GetString(vdMagic) != ISO_MAGIC && Encoding.GetString(hsMagic) != HIGH_SIERRA_MAGIC &&
                   Encoding.GetString(vdMagic) != CDI_MAGIC
                ) // Recognized, it is an ISO9660, now check for rest of data.
                {
                    if(counter == 0) return Errno.InvalidArgument;

                    break;
                }

                cdi |= Encoding.GetString(vdMagic) == CDI_MAGIC;

                switch(vdType)
                {
                    case 0:
                    {
                        if(debug) bvdSectors.Add(16 + counter + partition.Start);

                        break;
                    }

                    case 1:
                    {
                        if(highSierra)
                            hsvd = Marshal
                               .ByteArrayToStructureLittleEndian<HighSierraPrimaryVolumeDescriptor>(vdSector);
                        else if(cdi)
                            fsvd = Marshal.ByteArrayToStructureBigEndian<FileStructureVolumeDescriptor>(vdSector);
                        else pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(vdSector);

                        if(debug) pvdSectors.Add(16 + counter + partition.Start);

                        break;
                    }

                    case 2:
                    {
                        PrimaryVolumeDescriptor svd =
                            Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(vdSector);

                        // TODO: Other escape sequences
                        // Check if this is Joliet
                        if(svd.version == 1)
                        {
                            if(svd.escape_sequences[0] == '%' && svd.escape_sequences[1] == '/')
                                if(svd.escape_sequences[2] == '@' || svd.escape_sequences[2] == 'C' ||
                                   svd.escape_sequences[2] == 'E') jolietvd = svd;
                                else
                                    DicConsole.WriteLine("ISO9660 plugin",
                                                         "Found unknown supplementary volume descriptor");
                            if(debug) svdSectors.Add(16 + counter + partition.Start);
                        }
                        else
                        {
                            if(debug) evdSectors.Add(16 + counter + partition.Start);

                            if(useEvd)
                            {
                                // Basically until escape sequences are implemented, let the user chose the encoding.
                                // This is the same as user chosing Romeo namespace, but using the EVD instead of the PVD
                                this.@namespace = Namespace.Romeo;
                                pvd             = svd;
                            }
                        }

                        break;
                    }

                    case 3:
                    {
                        if(debug) vpdSectors.Add(16 + counter + partition.Start);

                        break;
                    }
                }

                counter++;
            }

            DecodedVolumeDescriptor decodedVd;
            DecodedVolumeDescriptor decodedJolietVd = new DecodedVolumeDescriptor();

            XmlFsType = new FileSystemType();

            if(pvd == null && hsvd == null && fsvd == null)
            {
                DicConsole.ErrorWriteLine("ERROR: Could not find primary volume descriptor");
                return Errno.InvalidArgument;
            }

            if(highSierra) decodedVd = DecodeVolumeDescriptor(hsvd.Value);
            else if(cdi) decodedVd   = DecodeVolumeDescriptor(fsvd.Value);
            else decodedVd           = DecodeVolumeDescriptor(pvd.Value);

            if(jolietvd != null) decodedJolietVd = DecodeJolietDescriptor(jolietvd.Value);

            if(this.@namespace != Namespace.Romeo) Encoding = Encoding.ASCII;

            string fsFormat;
            byte[] pathTableData;
            uint   pathTableSizeInSectors = 0;

            uint pathTableMsbLocation;
            uint pathTableLsbLocation;

            if(highSierra)
            {
                pathTableSizeInSectors = hsvd.Value.path_table_size / 2048;
                if(hsvd.Value.path_table_size                       % 2048 > 0) pathTableSizeInSectors++;

                pathTableData =
                    imagePlugin.ReadSectors(Swapping.Swap(hsvd.Value.mandatory_path_table_msb), pathTableSizeInSectors);

                fsFormat = "High Sierra Format";

                pathTableMsbLocation = hsvd.Value.mandatory_path_table_msb;
                pathTableLsbLocation = hsvd.Value.mandatory_path_table_lsb;
            }
            else if(cdi)
            {
                pathTableSizeInSectors = fsvd.Value.path_table_size / 2048;
                if(fsvd.Value.path_table_size                       % 2048 > 0) pathTableSizeInSectors++;

                pathTableData = imagePlugin.ReadSectors(fsvd.Value.path_table_addr, pathTableSizeInSectors);

                fsFormat = "CD-i";

                pathTableMsbLocation = fsvd.Value.path_table_addr;

                // TODO: Until escape sequences are implemented this is the default CD-i encoding.
                Encoding = System.Text.Encoding.GetEncoding("iso8859-1");

                // TODO: Implement CD-i
                return Errno.NotImplemented;
            }
            else
            {
                pathTableSizeInSectors = pvd.Value.path_table_size / 2048;
                if(pvd.Value.path_table_size                       % 2048 > 0) pathTableSizeInSectors++;

                pathTableData =
                    imagePlugin.ReadSectors(Swapping.Swap(pvd.Value.type_m_path_table), pathTableSizeInSectors);

                fsFormat = "ISO9660";

                pathTableMsbLocation = pvd.Value.type_m_path_table;
                pathTableLsbLocation = pvd.Value.type_l_path_table;
            }

            pathTable = highSierra ? DecodeHighSierraPathTable(pathTableData) : DecodePathTable(pathTableData);

            // High Sierra and CD-i do not support Joliet or RRIP
            if((highSierra || cdi) && this.@namespace != Namespace.Normal && this.@namespace != Namespace.Vms)
                this.@namespace = Namespace.Normal;

            if(jolietvd is null && this.@namespace == Namespace.Joliet) this.@namespace = Namespace.Normal;

            uint rootLocation = 0;
            uint rootSize     = 0;

            if(!cdi)
            {
                rootLocation = highSierra
                                   ? hsvd.Value.root_directory_record.extent
                                   : pvd.Value.root_directory_record.extent;

                if(highSierra)
                {
                    rootSize = hsvd.Value.root_directory_record.size / hsvd.Value.logical_block_size;
                    if(hsvd.Value.root_directory_record.size         % hsvd.Value.logical_block_size > 0) rootSize++;
                }
                else
                {
                    rootSize = pvd.Value.root_directory_record.size / pvd.Value.logical_block_size;
                    if(pvd.Value.root_directory_record.size         % pvd.Value.logical_block_size > 0) rootSize++;
                }
            }
            else
            {
                rootLocation = pathTable[0].Extent;

                byte[] firstRootSector = imagePlugin.ReadSector(rootLocation);
                CdiDirectoryRecord rootEntry =
                    Marshal.ByteArrayToStructureBigEndian<CdiDirectoryRecord>(firstRootSector);
                rootSize = rootEntry.size / fsvd.Value.logical_block_size;
                if(rootEntry.size         % fsvd.Value.logical_block_size > 0) rootSize++;

                usePathTable = true;
                useTransTbl  = false;
            }

            if(rootLocation + rootSize >= imagePlugin.Info.Sectors) return Errno.InvalidArgument;

            byte[] rootDir = imagePlugin.ReadSectors(rootLocation, rootSize);

            byte[]           ipbinSector = imagePlugin.ReadSector(partition.Start);
            CD.IPBin?        segaCd      = CD.DecodeIPBin(ipbinSector);
            Saturn.IPBin?    saturn      = Saturn.DecodeIPBin(ipbinSector);
            Dreamcast.IPBin? dreamcast   = Dreamcast.DecodeIPBin(ipbinSector);

            if(this.@namespace == Namespace.Joliet || this.@namespace == Namespace.Rrip)
            {
                usePathTable = false;
                useTransTbl  = false;
            }

            // Cannot traverse path table if we substitute the names for the ones in TRANS.TBL
            if(useTransTbl) usePathTable = false;

            if(this.@namespace != Namespace.Joliet)
                rootDirectoryCache = cdi
                                         ? DecodeCdiDirectory(rootDir)
                                         : highSierra
                                             ? DecodeHighSierraDirectory(rootDir)
                                             : DecodeIsoDirectory(rootDir);

            XmlFsType.Type = fsFormat;

            if(debug)
            {
                rootDirectoryCache.Add("$",
                                       new DecodedDirectoryEntry
                                       {
                                           Extent    = rootLocation,
                                           Filename  = "$",
                                           Size      = 2048,
                                           Timestamp = decodedVd.CreationTime
                                       });

                if(!cdi)
                    rootDirectoryCache.Add("$PATH_TABLE.LSB",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = pathTableLsbLocation,
                                               Filename  = "$PATH_TABLE.LSB",
                                               Size      = (uint)pathTableData.Length,
                                               Timestamp = decodedVd.CreationTime
                                           });

                rootDirectoryCache.Add("$PATH_TABLE.MSB",
                                       new DecodedDirectoryEntry
                                       {
                                           Extent    = pathTableMsbLocation,
                                           Filename  = "$PATH_TABLE.MSB",
                                           Size      = (uint)pathTableData.Length,
                                           Timestamp = decodedVd.CreationTime
                                       });

                for(int i = 0; i < bvdSectors.Count; i++)
                    rootDirectoryCache.Add(i == 0 ? "$BOOT" : $"$BOOT_{i}",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = (uint)i,
                                               Filename  = i == 0 ? "$BOOT" : $"$BOOT_{i}",
                                               Size      = 2048,
                                               Timestamp = decodedVd.CreationTime
                                           });

                for(int i = 0; i < pvdSectors.Count; i++)
                    rootDirectoryCache.Add(i == 0 ? "$PVD" : $"$PVD{i}",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = (uint)i,
                                               Filename  = i == 0 ? "$PVD" : $"PVD_{i}",
                                               Size      = 2048,
                                               Timestamp = decodedVd.CreationTime
                                           });

                for(int i = 0; i < svdSectors.Count; i++)
                    rootDirectoryCache.Add(i == 0 ? "$SVD" : $"$SVD_{i}",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = (uint)i,
                                               Filename  = i == 0 ? "$SVD" : $"$SVD_{i}",
                                               Size      = 2048,
                                               Timestamp = decodedVd.CreationTime
                                           });

                for(int i = 0; i < evdSectors.Count; i++)
                    rootDirectoryCache.Add(i == 0 ? "$EVD" : $"$EVD_{i}",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = (uint)i,
                                               Filename  = i == 0 ? "$EVD" : $"$EVD_{i}",
                                               Size      = 2048,
                                               Timestamp = decodedVd.CreationTime
                                           });

                for(int i = 0; i < vpdSectors.Count; i++)
                    rootDirectoryCache.Add(i == 0 ? "$VPD" : $"$VPD_{i}",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = (uint)i,
                                               Filename  = i == 0 ? "$VPD" : $"$VPD_{i}",
                                               Size      = 2048,
                                               Timestamp = decodedVd.CreationTime
                                           });

                if(segaCd != null)
                    rootDirectoryCache.Add("$IP.BIN",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = 0,
                                               Filename  = "$IP.BIN",
                                               Size      = (uint)Marshal.SizeOf<CD.IPBin>(),
                                               Timestamp = decodedVd.CreationTime
                                           });

                if(saturn != null)
                    rootDirectoryCache.Add("$IP.BIN",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = 0,
                                               Filename  = "$IP.BIN",
                                               Size      = (uint)Marshal.SizeOf<Saturn.IPBin>(),
                                               Timestamp = decodedVd.CreationTime
                                           });

                if(dreamcast != null)
                    rootDirectoryCache.Add("$IP.BIN",
                                           new DecodedDirectoryEntry
                                           {
                                               Extent    = 0,
                                               Filename  = "$IP.BIN",
                                               Size      = (uint)Marshal.SizeOf<Dreamcast.IPBin>(),
                                               Timestamp = decodedVd.CreationTime
                                           });
            }

            if(jolietvd != null && (this.@namespace == Namespace.Joliet || this.@namespace == Namespace.Rrip))
            {
                rootLocation = jolietvd.Value.root_directory_record.extent;

                rootSize = jolietvd.Value.root_directory_record.size / jolietvd.Value.logical_block_size;
                if(pvd.Value.root_directory_record.size % jolietvd.Value.logical_block_size > 0)
                    rootSize++;

                if(rootLocation + rootSize >= imagePlugin.Info.Sectors) return Errno.InvalidArgument;

                joliet = true;

                rootDir = imagePlugin.ReadSectors(rootLocation, rootSize);

                rootDirectoryCache = DecodeIsoDirectory(rootDir);

                XmlFsType.VolumeName = decodedJolietVd.VolumeIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier) ||
                   decodedVd.SystemIdentifier.Length > decodedJolietVd.SystemIdentifier.Length)
                    XmlFsType.SystemIdentifier = decodedVd.SystemIdentifier;
                else
                    XmlFsType.SystemIdentifier = string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier)
                                                     ? null
                                                     : decodedJolietVd.SystemIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier) || decodedVd.VolumeSetIdentifier.Length >
                   decodedJolietVd.VolumeSetIdentifier.Length)
                    XmlFsType.VolumeSetIdentifier = decodedVd.VolumeSetIdentifier;
                else
                    XmlFsType.VolumeSetIdentifier = string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier)
                                                        ? null
                                                        : decodedJolietVd.VolumeSetIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier) || decodedVd.PublisherIdentifier.Length >
                   decodedJolietVd.PublisherIdentifier.Length)
                    XmlFsType.PublisherIdentifier = decodedVd.PublisherIdentifier;
                else
                    XmlFsType.PublisherIdentifier = string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier)
                                                        ? null
                                                        : decodedJolietVd.PublisherIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier) ||
                   decodedVd.DataPreparerIdentifier.Length > decodedJolietVd.DataPreparerIdentifier.Length)
                    XmlFsType.DataPreparerIdentifier = decodedVd.DataPreparerIdentifier;
                else
                    XmlFsType.DataPreparerIdentifier = string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier)
                                                           ? null
                                                           : decodedJolietVd.DataPreparerIdentifier;

                if(string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier) ||
                   decodedVd.ApplicationIdentifier.Length > decodedJolietVd.ApplicationIdentifier.Length)
                    XmlFsType.ApplicationIdentifier = decodedVd.ApplicationIdentifier;
                else
                    XmlFsType.ApplicationIdentifier = string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier)
                                                          ? null
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

            statfs = new FileSystemInfo
            {
                Blocks = decodedVd.Blocks,
                FilenameLength = (ushort)(jolietvd != null
                                              ? this.@namespace == Namespace.Joliet
                                                    ? 110
                                                    : 255
                                              : 255),
                PluginId = Id,
                Type     = fsFormat
            };

            directoryCache = new Dictionary<string, Dictionary<string, DecodedDirectoryEntry>>();
            image          = imagePlugin;

            if(usePathTable)
                foreach(DecodedDirectoryEntry subDirectory in cdi
                                                                  ? GetSubdirsFromCdiPathTable("")
                                                                  : highSierra
                                                                      ? GetSubdirsFromHighSierraPathTable("")
                                                                      : GetSubdirsFromIsoPathTable(""))
                    rootDirectoryCache[subDirectory.Filename] = subDirectory;

            mounted = true;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            if(!mounted) return Errno.AccessDenied;

            rootDirectoryCache = null;
            directoryCache     = null;
            mounted            = false;

            return Errno.NoError;
        }

        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            stat = statfs.ShallowCopy();
            return Errno.NoError;
        }
    }
}