using System.Collections.Generic;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public partial class OperaFS
    {
        public Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options,     string    @namespace)
        {
            // TODO: Find correct default encoding
            Encoding = Encoding.ASCII;

            if(options == null) options = GetDefaultOptions();
            if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out debug);

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            SuperBlock sb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

            if(sb.record_type != 1 || sb.record_version != 1) return Errno.InvalidArgument;
            if(Encoding.ASCII.GetString(sb.sync_bytes) != SYNC) return Errno.InvalidArgument;

            if(imagePlugin.Info.SectorSize == 2336 || imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448) volumeBlockSizeRatio = sb.block_size / 2048;
            else volumeBlockSizeRatio                                    = sb.block_size / imagePlugin.Info.SectorSize;

            XmlFsType = new FileSystemType
            {
                Type         = "Opera",
                VolumeName   = StringHandlers.CToString(sb.volume_label, Encoding),
                ClusterSize  = sb.block_size,
                Clusters     = sb.block_count,
                Bootable     = true,
                VolumeSerial = $"{sb.volume_id:X8}"
            };

            statfs = new FileSystemInfo
            {
                Blocks         = sb.block_count,
                FilenameLength = MAX_NAME,
                FreeBlocks     = 0,
                Id             = new FileSystemId {IsInt = true, Serial32 = sb.volume_id},
                PluginId       = Id,
                Type           = "Opera"
            };

            image = imagePlugin;
            int firstRootBlock = BigEndianBitConverter.ToInt32(sbSector, Marshal.SizeOf<SuperBlock>());
            rootDirectoryCache = DecodeDirectory(firstRootBlock);
            directoryCache     = new Dictionary<string, Dictionary<string, DirectoryEntryWithPointers>>();
            mounted            = true;

            return Errno.NoError;
        }

        public Errno Unmount()
        {
            if(!mounted) return Errno.AccessDenied;

            mounted = false;

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