using System;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    public partial class OperaFS
    {
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            byte[] syncBytes = new byte[5];

            byte recordType = sbSector[0x000];
            Array.Copy(sbSector, 0x001, syncBytes, 0, 5);
            byte recordVersion = sbSector[0x006];

            if(recordType    != 1 ||
               recordVersion != 1)
                return false;

            return Encoding.ASCII.GetString(syncBytes) == SYNC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            // TODO: Find correct default encoding
            Encoding    = Encoding.ASCII;
            information = "";
            var superBlockMetadata = new StringBuilder();

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            SuperBlock sb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

            if(sb.record_type    != 1 ||
               sb.record_version != 1)
                return;

            if(Encoding.ASCII.GetString(sb.sync_bytes) != SYNC)
                return;

            superBlockMetadata.AppendFormat("Opera filesystem disc.").AppendLine();

            if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_label, Encoding)))
                superBlockMetadata.
                    AppendFormat("Volume label: {0}", StringHandlers.CToString(sb.volume_label, Encoding)).AppendLine();

            if(!string.IsNullOrEmpty(StringHandlers.CToString(sb.volume_comment, Encoding)))
                superBlockMetadata.
                    AppendFormat("Volume comment: {0}", StringHandlers.CToString(sb.volume_comment, Encoding)).
                    AppendLine();

            superBlockMetadata.AppendFormat("Volume identifier: 0x{0:X8}", sb.volume_id).AppendLine();
            superBlockMetadata.AppendFormat("Block size: {0} bytes", sb.block_size).AppendLine();

            if(imagePlugin.Info.SectorSize == 2336 ||
               imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
            {
                if(sb.block_size != 2048)
                    superBlockMetadata.
                        AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block",
                                     sb.block_size, 2048);
            }
            else if(imagePlugin.Info.SectorSize != sb.block_size)
                superBlockMetadata.
                    AppendFormat("WARNING: Filesystem indicates {0} bytes/block while device indicates {1} bytes/block",
                                 sb.block_size, imagePlugin.Info.SectorSize);

            superBlockMetadata.
                AppendFormat("Volume size: {0} blocks, {1} bytes", sb.block_count, sb.block_size * sb.block_count).
                AppendLine();

            if(sb.block_count > imagePlugin.Info.Sectors)
                superBlockMetadata.
                    AppendFormat("WARNING: Filesystem indicates {0} blocks while device indicates {1} blocks",
                                 sb.block_count, imagePlugin.Info.Sectors);

            superBlockMetadata.AppendFormat("Root directory identifier: 0x{0:X8}", sb.root_dirid).AppendLine();
            superBlockMetadata.AppendFormat("Root directory block size: {0} bytes", sb.rootdir_bsize).AppendLine();

            superBlockMetadata.AppendFormat("Root directory size: {0} blocks, {1} bytes", sb.rootdir_blocks,
                                            sb.rootdir_bsize * sb.rootdir_blocks).AppendLine();

            superBlockMetadata.AppendFormat("Last root directory copy: {0}", sb.last_root_copy).AppendLine();

            information = superBlockMetadata.ToString();

            XmlFsType = new FileSystemType
            {
                Type        = "Opera", VolumeName     = StringHandlers.CToString(sb.volume_label, Encoding),
                ClusterSize = sb.block_size, Clusters = sb.block_count
            };
        }
    }
}