// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Mounts ISO9660, CD-i and High Sierra Format filesystems.
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
// Copyright © 2011-2024 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.Sega;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _encoding = encoding ?? Encoding.GetEncoding(1252);
        var vdMagic = new byte[5]; // Volume Descriptor magic "CD001"
        var hsMagic = new byte[5]; // Volume Descriptor magic "CDROM"

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        if(options.TryGetValue("use_path_table", out string usePathTableString))
            bool.TryParse(usePathTableString, out _usePathTable);

        if(options.TryGetValue("use_trans_tbl", out string useTransTblString))
            bool.TryParse(useTransTblString, out _useTransTbl);

        if(options.TryGetValue("use_evd", out string useEvdString)) bool.TryParse(useEvdString, out _useEvd);

        // Default namespace
        @namespace ??= "joliet";

        switch(@namespace.ToLowerInvariant())
        {
            case "normal":
                _namespace = Namespace.Normal;

                break;
            case "vms":
                _namespace = Namespace.Vms;

                break;
            case "joliet":
                _namespace = Namespace.Joliet;

                break;
            case "rrip":
                _namespace = Namespace.Rrip;

                break;
            case "romeo":
                _namespace = Namespace.Romeo;

                break;
            default:
                return ErrorNumber.InvalidArgument;
        }

        PrimaryVolumeDescriptor?           pvd      = null;
        PrimaryVolumeDescriptor?           jolietvd = null;
        BootRecord?                        bvd      = null;
        HighSierraPrimaryVolumeDescriptor? hsvd     = null;
        FileStructureVolumeDescriptor?     fsvd     = null;

        // ISO9660 is designed for 2048 bytes/sector devices
        if(imagePlugin.Info.SectorSize < 2048) return ErrorNumber.InvalidArgument;

        // ISO9660 Primary Volume Descriptor starts at sector 16, so that's minimal size.
        if(partition.End < 16) return ErrorNumber.InvalidArgument;

        ulong counter = 0;

        ErrorNumber errno = imagePlugin.ReadSector(16 + partition.Start, out byte[] vdSector);

        if(errno != ErrorNumber.NoError) return errno;

        int xaOff = vdSector.Length == 2336 ? 8 : 0;
        Array.Copy(vdSector, 0x009 + xaOff, hsMagic, 0, 5);
        _highSierra = _encoding.GetString(hsMagic) == HIGH_SIERRA_MAGIC;
        var hsOff = 0;

        if(_highSierra) hsOff = 8;

        _cdi = false;
        List<ulong> bvdSectors = [];
        List<ulong> pvdSectors = [];
        List<ulong> svdSectors = [];
        List<ulong> evdSectors = [];
        List<ulong> vpdSectors = [];

        while(true)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Processing_VD_loop_no_0, counter);

            // Seek to Volume Descriptor
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Reading_sector_0, 16 + counter + partition.Start);
            errno = imagePlugin.ReadSector(16 + counter + partition.Start, out byte[] vdSectorTmp);

            if(errno != ErrorNumber.NoError) return errno;

            vdSector = new byte[vdSectorTmp.Length - xaOff];
            Array.Copy(vdSectorTmp, xaOff, vdSector, 0, vdSector.Length);

            byte vdType = vdSector[0 + hsOff]; // Volume Descriptor Type, should be 1 or 2.
            AaruConsole.DebugWriteLine(MODULE_NAME, "VDType = {0}", vdType);

            if(vdType == 255) // Supposedly we are in the PVD.
            {
                if(counter == 0) return ErrorNumber.InvalidArgument;

                break;
            }

            Array.Copy(vdSector, 0x001, vdMagic, 0, 5);
            Array.Copy(vdSector, 0x009, hsMagic, 0, 5);

            if(_encoding.GetString(vdMagic) != ISO_MAGIC         &&
               _encoding.GetString(hsMagic) != HIGH_SIERRA_MAGIC &&
               _encoding.GetString(vdMagic) != CDI_MAGIC) // Recognized, it is an ISO9660, now check for rest of data.
            {
                if(counter == 0) return ErrorNumber.InvalidArgument;

                break;
            }

            _cdi |= _encoding.GetString(vdMagic) == CDI_MAGIC;

            switch(vdType)
            {
                case 0:
                {
                    if(_debug) bvdSectors.Add(16 + counter + partition.Start);

                    break;
                }

                case 1:
                {
                    if(_highSierra)
                        hsvd = Marshal.ByteArrayToStructureLittleEndian<HighSierraPrimaryVolumeDescriptor>(vdSector);
                    else if(_cdi)
                        fsvd = Marshal.ByteArrayToStructureBigEndian<FileStructureVolumeDescriptor>(vdSector);
                    else
                        pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(vdSector);

                    if(_debug) pvdSectors.Add(16 + counter + partition.Start);

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
                        {
                            if(svd.escape_sequences[2] == '@' ||
                               svd.escape_sequences[2] == 'C' ||
                               svd.escape_sequences[2] == 'E')
                                jolietvd = svd;
                            else
                            {
                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.Found_unknown_supplementary_volume_descriptor);
                            }
                        }

                        if(_debug) svdSectors.Add(16 + counter + partition.Start);
                    }
                    else
                    {
                        if(_debug) evdSectors.Add(16 + counter + partition.Start);

                        if(_useEvd)
                        {
                            // Basically until escape sequences are implemented, let the user chose the encoding.
                            // This is the same as user choosing Romeo namespace, but using the EVD instead of the PVD
                            _namespace = Namespace.Romeo;
                            pvd        = svd;
                        }
                    }

                    break;
                }

                case 3:
                {
                    if(_debug) vpdSectors.Add(16 + counter + partition.Start);

                    break;
                }
            }

            counter++;
        }

        DecodedVolumeDescriptor decodedVd;
        var                     decodedJolietVd = new DecodedVolumeDescriptor();

        Metadata = new FileSystem();

        if(pvd == null && hsvd == null && fsvd == null)
        {
            AaruConsole.ErrorWriteLine(Localization.ERROR_Could_not_find_primary_volume_descriptor);

            return ErrorNumber.InvalidArgument;
        }

        if(_highSierra)
            decodedVd = DecodeVolumeDescriptor(hsvd.Value);
        else if(_cdi)
            decodedVd = DecodeVolumeDescriptor(fsvd.Value);
        else
            decodedVd = DecodeVolumeDescriptor(pvd.Value, _namespace == Namespace.Romeo ? _encoding : Encoding.ASCII);

        if(jolietvd != null) decodedJolietVd = DecodeJolietDescriptor(jolietvd.Value);

        if(_namespace != Namespace.Romeo) _encoding = Encoding.ASCII;

        string fsFormat;
        byte[] pathTableData;

        uint pathTableMsbLocation;
        uint pathTableLsbLocation = 0; // Initialize to 0 as ignored in CD-i

        _image = imagePlugin;

        if(_highSierra)
        {
            _blockSize = hsvd.Value.logical_block_size;

            errno = ReadSingleExtent(hsvd.Value.path_table_size,
                                     Swapping.Swap(hsvd.Value.mandatory_path_table_msb),
                                     out pathTableData);

            if(errno != ErrorNumber.NoError) pathTableData = null;

            fsFormat = FS_TYPE_HSF;

            pathTableMsbLocation = hsvd.Value.mandatory_path_table_msb;
            pathTableLsbLocation = hsvd.Value.mandatory_path_table_lsb;
        }
        else if(_cdi)
        {
            _blockSize = fsvd.Value.logical_block_size;

            errno = ReadSingleExtent(fsvd.Value.path_table_size, fsvd.Value.path_table_addr, out pathTableData);

            if(errno != ErrorNumber.NoError) pathTableData = null;

            fsFormat = FS_TYPE_CDI;

            pathTableMsbLocation = fsvd.Value.path_table_addr;

            // TODO: Until escape sequences are implemented this is the default CD-i encoding.
            _encoding = Encoding.GetEncoding("iso8859-1");
        }
        else
        {
            _blockSize = pvd.Value.logical_block_size;

            errno = ReadSingleExtent(pvd.Value.path_table_size,
                                     Swapping.Swap(pvd.Value.type_m_path_table),
                                     out pathTableData);

            if(errno != ErrorNumber.NoError) pathTableData = null;

            fsFormat = FS_TYPE_ISO;

            pathTableMsbLocation = pvd.Value.type_m_path_table;
            pathTableLsbLocation = pvd.Value.type_l_path_table;
        }

        _pathTable = _highSierra ? DecodeHighSierraPathTable(pathTableData) : DecodePathTable(pathTableData);

        if(_pathTable is null)
        {
            ReadSingleExtent(pathTableData.Length, pathTableLsbLocation, out pathTableData);

            _pathTable = _highSierra ? DecodeHighSierraPathTable(pathTableData) : DecodePathTable(pathTableData);
        }

        // High Sierra and CD-i do not support Joliet or RRIP
        if((_highSierra || _cdi) && _namespace != Namespace.Normal && _namespace != Namespace.Vms)
            _namespace = Namespace.Normal;

        if(jolietvd is null && _namespace == Namespace.Joliet) _namespace = Namespace.Normal;

        uint rootLocation;
        uint rootSize;
        byte rootXattrLength = 0;

        if(!_cdi)
        {
            rootLocation = _highSierra
                               ? hsvd.Value.root_directory_record.extent
                               : pvd.Value.root_directory_record.extent;

            rootXattrLength = _highSierra
                                  ? hsvd.Value.root_directory_record.xattr_len
                                  : pvd.Value.root_directory_record.xattr_len;

            rootSize = _highSierra ? hsvd.Value.root_directory_record.size : pvd.Value.root_directory_record.size;

            if(_pathTable?.Length > 1 && rootLocation != _pathTable[0].Extent)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization
                                              .Path_table_and_PVD_do_not_point_to_the_same_location_for_the_root_directory);

                errno = ReadSector(rootLocation, out byte[] firstRootSector);

                if(errno != ErrorNumber.NoError) return errno;

                var pvdWrongRoot = false;

                if(_highSierra)
                {
                    HighSierraDirectoryRecord rootEntry =
                        Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(firstRootSector);

                    if(rootEntry.extent != rootLocation) pvdWrongRoot = true;
                }
                else
                {
                    DirectoryRecord rootEntry =
                        Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(firstRootSector);

                    if(rootEntry.extent != rootLocation) pvdWrongRoot = true;
                }

                if(pvdWrongRoot)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization
                                                  .PVD_does_not_point_to_correct_root_directory_checking_path_table);

                    var pathTableWrongRoot = false;

                    rootLocation = _pathTable[0].Extent;

                    ReadSector(_pathTable[0].Extent, out firstRootSector);

                    if(_highSierra)
                    {
                        HighSierraDirectoryRecord rootEntry =
                            Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(firstRootSector);

                        if(rootEntry.extent != rootLocation) pathTableWrongRoot = true;
                    }
                    else
                    {
                        DirectoryRecord rootEntry =
                            Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(firstRootSector);

                        if(rootEntry.extent != rootLocation) pathTableWrongRoot = true;
                    }

                    if(pathTableWrongRoot)
                    {
                        AaruConsole.ErrorWriteLine(Localization.Cannot_find_root_directory);

                        return ErrorNumber.InvalidArgument;
                    }

                    _usePathTable = true;
                }
            }
        }
        else
        {
            rootLocation = _pathTable[0].Extent;

            errno = ReadSector(rootLocation, out byte[] firstRootSector);

            if(errno != ErrorNumber.NoError) return errno;

            CdiDirectoryRecord rootEntry = Marshal.ByteArrayToStructureBigEndian<CdiDirectoryRecord>(firstRootSector);

            rootSize = rootEntry.size;

            _usePathTable = _usePathTable || _pathTable.Length == 1;
            _useTransTbl  = false;
        }

        // In case the path table is incomplete
        if(_usePathTable && (_pathTable is null || _pathTable?.Length <= 1)) _usePathTable = false;

        if(_usePathTable && !_cdi)
        {
            rootLocation = _pathTable[0].Extent;

            errno = ReadSector(rootLocation, out byte[] firstRootSector);

            if(errno != ErrorNumber.NoError) return errno;

            if(_highSierra)
            {
                HighSierraDirectoryRecord rootEntry =
                    Marshal.ByteArrayToStructureLittleEndian<HighSierraDirectoryRecord>(firstRootSector);

                rootSize = rootEntry.size;
            }
            else
            {
                DirectoryRecord rootEntry = Marshal.ByteArrayToStructureLittleEndian<DirectoryRecord>(firstRootSector);

                rootSize = rootEntry.size;
            }

            rootXattrLength = _pathTable[0].XattrLength;
        }

        try
        {
            ReadSingleExtent(rootSize, rootLocation, out byte[] _);
        }
        catch
        {
            return ErrorNumber.InvalidArgument;
        }

        errno = ReadSector(partition.Start, out byte[] ipbinSector);

        if(errno != ErrorNumber.NoError) return errno;

        CD.IPBin?        segaCd    = CD.DecodeIPBin(ipbinSector);
        Saturn.IPBin?    saturn    = Saturn.DecodeIPBin(ipbinSector);
        Dreamcast.IPBin? dreamcast = Dreamcast.DecodeIPBin(ipbinSector);

        if(_namespace is Namespace.Joliet or Namespace.Rrip)
        {
            _usePathTable = false;
            _useTransTbl  = false;
        }

        // Cannot traverse path table if we substitute the names for the ones in TRANS.TBL
        if(_useTransTbl) _usePathTable = false;

        if(_namespace != Namespace.Joliet)
        {
            _rootDirectoryCache = _cdi
                                      ? DecodeCdiDirectory(rootLocation + rootXattrLength, rootSize)
                                      : _highSierra
                                          ? DecodeHighSierraDirectory(rootLocation + rootXattrLength, rootSize)
                                          : DecodeIsoDirectory(rootLocation        + rootXattrLength, rootSize);
        }

        Metadata.Type = fsFormat;

        if(jolietvd != null && _namespace is Namespace.Joliet or Namespace.Rrip)
        {
            rootLocation    = jolietvd.Value.root_directory_record.extent;
            rootXattrLength = jolietvd.Value.root_directory_record.xattr_len;

            rootSize = jolietvd.Value.root_directory_record.size;

            _joliet = true;

            _rootDirectoryCache = DecodeIsoDirectory(rootLocation + rootXattrLength, rootSize);

            Metadata.VolumeName = decodedJolietVd.VolumeIdentifier;

            if(string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier) ||
               decodedVd.SystemIdentifier.Length > decodedJolietVd.SystemIdentifier.Length)
                Metadata.SystemIdentifier = decodedVd.SystemIdentifier;
            else
            {
                Metadata.SystemIdentifier = string.IsNullOrEmpty(decodedJolietVd.SystemIdentifier)
                                                ? null
                                                : decodedJolietVd.SystemIdentifier;
            }

            if(string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier) ||
               decodedVd.VolumeSetIdentifier.Length > decodedJolietVd.VolumeSetIdentifier.Length)
                Metadata.VolumeSetIdentifier = decodedVd.VolumeSetIdentifier;
            else
            {
                Metadata.VolumeSetIdentifier = string.IsNullOrEmpty(decodedJolietVd.VolumeSetIdentifier)
                                                   ? null
                                                   : decodedJolietVd.VolumeSetIdentifier;
            }

            if(string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier) ||
               decodedVd.PublisherIdentifier.Length > decodedJolietVd.PublisherIdentifier.Length)
                Metadata.PublisherIdentifier = decodedVd.PublisherIdentifier;
            else
            {
                Metadata.PublisherIdentifier = string.IsNullOrEmpty(decodedJolietVd.PublisherIdentifier)
                                                   ? null
                                                   : decodedJolietVd.PublisherIdentifier;
            }

            if(string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier) ||
               decodedVd.DataPreparerIdentifier.Length > decodedJolietVd.DataPreparerIdentifier.Length)
                Metadata.DataPreparerIdentifier = decodedVd.DataPreparerIdentifier;
            else
            {
                Metadata.DataPreparerIdentifier = string.IsNullOrEmpty(decodedJolietVd.DataPreparerIdentifier)
                                                      ? null
                                                      : decodedJolietVd.DataPreparerIdentifier;
            }

            if(string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier) ||
               decodedVd.ApplicationIdentifier.Length > decodedJolietVd.ApplicationIdentifier.Length)
                Metadata.ApplicationIdentifier = decodedVd.ApplicationIdentifier;
            else
            {
                Metadata.ApplicationIdentifier = string.IsNullOrEmpty(decodedJolietVd.ApplicationIdentifier)
                                                     ? null
                                                     : decodedJolietVd.ApplicationIdentifier;
            }

            Metadata.CreationDate = decodedJolietVd.CreationTime;

            if(decodedJolietVd.HasModificationTime) Metadata.ModificationDate = decodedJolietVd.ModificationTime;

            if(decodedJolietVd.HasExpirationTime) Metadata.ExpirationDate = decodedJolietVd.ExpirationTime;

            if(decodedJolietVd.HasEffectiveTime) Metadata.EffectiveDate = decodedJolietVd.EffectiveTime;

            decodedVd = decodedJolietVd;
        }
        else
        {
            Metadata.SystemIdentifier       = decodedVd.SystemIdentifier;
            Metadata.VolumeName             = decodedVd.VolumeIdentifier;
            Metadata.VolumeSetIdentifier    = decodedVd.VolumeSetIdentifier;
            Metadata.PublisherIdentifier    = decodedVd.PublisherIdentifier;
            Metadata.DataPreparerIdentifier = decodedVd.DataPreparerIdentifier;
            Metadata.ApplicationIdentifier  = decodedVd.ApplicationIdentifier;
            Metadata.CreationDate           = decodedVd.CreationTime;

            if(decodedVd.HasModificationTime) Metadata.ModificationDate = decodedVd.ModificationTime;

            if(decodedVd.HasExpirationTime) Metadata.ExpirationDate = decodedVd.ExpirationTime;

            if(decodedVd.HasEffectiveTime) Metadata.EffectiveDate = decodedVd.EffectiveTime;
        }

        if(_debug)
        {
            _rootDirectoryCache.Add("$",
                                    new DecodedDirectoryEntry
                                    {
                                        Extents   = [(rootLocation, rootSize)],
                                        Filename  = "$",
                                        Size      = rootSize,
                                        Timestamp = decodedVd.CreationTime
                                    });

            if(!_cdi)
            {
                _rootDirectoryCache.Add("$PATH_TABLE.LSB",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [(pathTableLsbLocation, (uint)pathTableData.Length)],
                                            Filename  = "$PATH_TABLE.LSB",
                                            Size      = (uint)pathTableData.Length,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            _rootDirectoryCache.Add("$PATH_TABLE.MSB",
                                    new DecodedDirectoryEntry
                                    {
                                        Extents   = [(Swapping.Swap(pathTableMsbLocation), (uint)pathTableData.Length)],
                                        Filename  = "$PATH_TABLE.MSB",
                                        Size      = (uint)pathTableData.Length,
                                        Timestamp = decodedVd.CreationTime
                                    });

            for(var i = 0; i < bvdSectors.Count; i++)
            {
                _rootDirectoryCache.Add(i == 0 ? "$BOOT" : $"$BOOT_{i}",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)i, 2048)],
                                            Filename  = i == 0 ? "$BOOT" : $"$BOOT_{i}",
                                            Size      = 2048,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            for(var i = 0; i < pvdSectors.Count; i++)
            {
                _rootDirectoryCache.Add(i == 0 ? "$PVD" : $"$PVD{i}",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)i, 2048)],
                                            Filename  = i == 0 ? "$PVD" : $"PVD_{i}",
                                            Size      = 2048,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            for(var i = 0; i < svdSectors.Count; i++)
            {
                _rootDirectoryCache.Add(i == 0 ? "$SVD" : $"$SVD_{i}",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)i, 2048)],
                                            Filename  = i == 0 ? "$SVD" : $"$SVD_{i}",
                                            Size      = 2048,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            for(var i = 0; i < evdSectors.Count; i++)
            {
                _rootDirectoryCache.Add(i == 0 ? "$EVD" : $"$EVD_{i}",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)i, 2048)],
                                            Filename  = i == 0 ? "$EVD" : $"$EVD_{i}",
                                            Size      = 2048,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            for(var i = 0; i < vpdSectors.Count; i++)
            {
                _rootDirectoryCache.Add(i == 0 ? "$VPD" : $"$VPD_{i}",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)i, 2048)],
                                            Filename  = i == 0 ? "$VPD" : $"$VPD_{i}",
                                            Size      = 2048,
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            if(segaCd != null)
            {
                _rootDirectoryCache.Add("$IP.BIN",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)partition.Start, (uint)Marshal.SizeOf<CD.IPBin>())],
                                            Filename  = "$IP.BIN",
                                            Size      = (uint)Marshal.SizeOf<CD.IPBin>(),
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            if(saturn != null)
            {
                _rootDirectoryCache.Add("$IP.BIN",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents   = [((uint)partition.Start, (uint)Marshal.SizeOf<Saturn.IPBin>())],
                                            Filename  = "$IP.BIN",
                                            Size      = (uint)Marshal.SizeOf<Saturn.IPBin>(),
                                            Timestamp = decodedVd.CreationTime
                                        });
            }

            if(dreamcast != null)
            {
                _rootDirectoryCache.Add("$IP.BIN",
                                        new DecodedDirectoryEntry
                                        {
                                            Extents =
                                            [
                                                ((uint)partition.Start, (uint)Marshal.SizeOf<Dreamcast.IPBin>())
                                            ],
                                            Filename  = "$IP.BIN",
                                            Size      = (uint)Marshal.SizeOf<Dreamcast.IPBin>(),
                                            Timestamp = decodedVd.CreationTime
                                        });
            }
        }

        Metadata.Bootable    |= bvd != null || segaCd != null || saturn != null || dreamcast != null;
        Metadata.Clusters    =  decodedVd.Blocks;
        Metadata.ClusterSize =  decodedVd.BlockSize;

        _statfs = new FileSystemInfo
        {
            Blocks         = decodedVd.Blocks,
            FilenameLength = (ushort)(jolietvd != null ? _namespace == Namespace.Joliet ? 110 : 255 : 255),
            PluginId       = Id,
            Type           = fsFormat
        };

        _directoryCache = new Dictionary<string, Dictionary<string, DecodedDirectoryEntry>>();

        if(_usePathTable)
        {
            foreach(DecodedDirectoryEntry subDirectory in _cdi
                                                              ? GetSubdirsFromCdiPathTable("")
                                                              : _highSierra
                                                                  ? GetSubdirsFromHighSierraPathTable("")
                                                                  : GetSubdirsFromIsoPathTable(""))
                _rootDirectoryCache[subDirectory.Filename] = subDirectory;
        }

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _rootDirectoryCache = null;
        _directoryCache     = null;
        _mounted            = false;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        stat = _statfs.ShallowCopy();

        return ErrorNumber.NoError;
    }

#endregion
}