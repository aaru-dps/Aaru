using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class OperaFS : Plugin
	{
		public OperaFS(PluginBase Core)
        {
            base.Name = "Opera Filesystem Plugin";
            base.PluginUUID = new Guid("0ec84ec7-eae6-4196-83fe-943b3fe46dbd");
        }
		
		public override bool Identify(FileStream fileStream, long offset)
		{
            fileStream.Seek(0 + offset, SeekOrigin.Begin);

            byte record_type;
            byte[] sync_bytes = new byte[5];
            byte record_version;
			
			record_type = (byte)fileStream.ReadByte();
            fileStream.Read(sync_bytes, 0, 5);
			record_version = (byte)fileStream.ReadByte();
			
			if (record_type != 1 || record_version != 1)
                return false;
            if(Encoding.ASCII.GetString(sync_bytes) != "ZZZZZ")
                return false;
			
			return true;
		}
		
		public override void GetInformation (FileStream fileStream, long offset, out string information)
		{
			information = "";
            StringBuilder SuperBlockMetadata = new StringBuilder();

            fileStream.Seek(0 + offset, SeekOrigin.Begin);

            byte[] record_type = new byte[1];
            byte[] sync_bytes = new byte[5];
            byte[] record_version = new byte[1];
            byte[] volume_flags = new byte[1];
            byte[] volume_comment = new byte[32];
            byte[] volume_label = new byte[32];
            byte[] volume_id = new byte[4];
            byte[] block_size = new byte[4];
            byte[] block_count = new byte[4];
            byte[] root_dirid = new byte[4];
            byte[] rootdir_blocks = new byte[4];
            byte[] rootdir_bsize = new byte[4];
            byte[] last_root_copy = new byte[4];

            fileStream.Read(record_type, 0, 1);
            fileStream.Read(sync_bytes, 0, 5);
            fileStream.Read(record_version, 0, 1);
            fileStream.Read(volume_flags, 0, 1);
            fileStream.Read(volume_comment, 0, 32);
            fileStream.Read(volume_label, 0, 32);
            fileStream.Read(volume_id, 0, 4);
            fileStream.Read(block_size, 0, 4);
            fileStream.Read(block_count, 0, 4);
            fileStream.Read(root_dirid, 0, 4);
            fileStream.Read(rootdir_blocks, 0, 4);
            fileStream.Read(rootdir_bsize, 0, 4);
            fileStream.Read(last_root_copy, 0, 4);

            if (record_type[0] != 1 || record_version[0] != 1)
                return;
            if(Encoding.ASCII.GetString(sync_bytes) != "ZZZZZ")
                return;

            // Swapping data (C# is LE, Opera is BE)
            volume_id = Swapping.SwapFourBytes(volume_id);
            block_size = Swapping.SwapFourBytes(block_size);
            block_count = Swapping.SwapFourBytes(block_count);
            root_dirid = Swapping.SwapFourBytes(root_dirid);
            rootdir_blocks = Swapping.SwapFourBytes(rootdir_blocks);
            rootdir_bsize = Swapping.SwapFourBytes(rootdir_bsize);
            last_root_copy = Swapping.SwapFourBytes(last_root_copy);

            int vid = BitConverter.ToInt32(volume_id, 0);
            int rdid = BitConverter.ToInt32(root_dirid, 0);

            StringBuilder VolumeComment = new StringBuilder();

            for (int i = 0; i < volume_comment.Length; i++)
            {
                if (volume_comment[i] != 0x00)
                    VolumeComment.Append((char)volume_comment[i]);
                else
                    break;
            }
            if (VolumeComment.Length == 0)
                VolumeComment.Append("Not set.");

            StringBuilder VolumeLabel = new StringBuilder();

            for (int i = 0; i < volume_label.Length; i++)
            {
                if (volume_label[i] != 0x00)
                    VolumeLabel.Append((char)volume_label[i]);
                else
                    break;
            }
            if (VolumeLabel.Length == 0)
                VolumeLabel.Append("Not set.");

            int bs = BitConverter.ToInt32(block_size, 0);
            int vblocks = BitConverter.ToInt32(block_count, 0);
            int rbs = BitConverter.ToInt32(rootdir_bsize, 0);
            int rblocks = BitConverter.ToInt32(rootdir_blocks, 0);

            SuperBlockMetadata.AppendFormat("Opera filesystem disc.").AppendLine();
            SuperBlockMetadata.AppendFormat("Volume label: {0}", VolumeLabel.ToString()).AppendLine();
            SuperBlockMetadata.AppendFormat("Volume comment: {0}", VolumeComment.ToString()).AppendLine();
            SuperBlockMetadata.AppendFormat("Volume identifier: 0x{0}", vid.ToString("X")).AppendLine();
            SuperBlockMetadata.AppendFormat("Block size: {0} bytes", bs).AppendLine();
            SuperBlockMetadata.AppendFormat("Volume size: {0} blocks, {1} bytes", vblocks, bs*vblocks).AppendLine();
            SuperBlockMetadata.AppendFormat("Root directory identifier: 0x{0}", rdid.ToString("X")).AppendLine();
            SuperBlockMetadata.AppendFormat("Root directory block size: {0} bytes", rbs).AppendLine();
            SuperBlockMetadata.AppendFormat("Root directory size: {0} blocks, {1} bytes", rblocks, rbs*rblocks).AppendLine();
            SuperBlockMetadata.AppendFormat("Last root directory copy: {0}", BitConverter.ToInt32(last_root_copy, 0)).AppendLine();

            information = SuperBlockMetadata.ToString();
		}
	}
}

