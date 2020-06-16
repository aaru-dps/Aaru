namespace Aaru.Decoders.SCSI.MMC
{
    public class TrackInformation
    {
        public bool                     Blank;
        public bool                     Copy;
        public bool                     Damage;
        public ushort                   DataLength;
        public byte                     DataMode;
        public uint                     FixedPacketSize;
        public bool                     FP;
        public uint                     FreeBlocks;
        public uint                     LastLayerJumpAddress;
        public uint                     LastRecordedAddress;
        public LayerJumpRecordingStatus LayerJumpRecordingStatus;
        public ushort                   LogicalTrackNumber;
        public uint                     LogicalTrackSize;
        public uint                     LogicalTrackStartAddress;
        public bool                     LraV;
        public uint                     NextLayerJumpAddress;
        public uint                     NextWritableAddress;
        public bool                     NwaV;
        public bool                     Packet;
        public uint                     ReadCompatibilityLba;
        public bool                     RT;
        public ushort                   SessionNumber;
        public byte                     TrackMode;

        public static TrackInformation Decode(byte[] response)
        {
            if(response.Length < 32)
                return null;

            var decoded = new TrackInformation
            {
                DataLength    = (ushort)((response[0] << 8) + response[1]), LogicalTrackNumber = response[2],
                SessionNumber = response[3], LayerJumpRecordingStatus = (LayerJumpRecordingStatus)(response[5] >> 6),
                Damage        = (response[5]       & 0x20) == 0x20, Copy = (response[5] & 0x10) == 0x10,
                TrackMode     = (byte)(response[5] & 0xF), RT = (response[6] & 0x80) == 0x80,
                Blank         = (response[6]       & 0x40) == 0x40, Packet = (response[6] & 0x20) == 0x20,
                FP            = (response[6]       & 0x10) == 0x10, DataMode = (byte)(response[6] & 0xF),
                LraV          = (response[7]       & 0x02) == 0x02, NwaV = (response[7] & 0x01) == 0x01,
                LogicalTrackStartAddress =
                    (uint)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]),
                NextWritableAddress =
                    (uint)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15]),
                FreeBlocks = (uint)((response[16] << 24) + (response[17] << 16) + (response[18] << 8) + response[19]),
                FixedPacketSize =
                    (uint)((response[20] << 24) + (response[21] << 16) + (response[22] << 8) + response[23]),
                LogicalTrackSize =
                    (uint)((response[24] << 24) + (response[25] << 16) + (response[26] << 8) + response[27]),
                LastRecordedAddress =
                    (uint)((response[28] << 24) + (response[29] << 16) + (response[30] << 8) + response[31])
            };

            if(response.Length < 48)
                return decoded;

            decoded.LogicalTrackNumber += (ushort)(response[32] << 8);

            decoded.SessionNumber += (ushort)(response[33] << 8);

            decoded.ReadCompatibilityLba =
                (uint)((response[36] << 24) + (response[37] << 16) + (response[38] << 8) + response[39]);

            decoded.NextLayerJumpAddress =
                (uint)((response[40] << 24) + (response[41] << 16) + (response[42] << 8) + response[43]);

            decoded.LastLayerJumpAddress =
                (uint)((response[44] << 24) + (response[45] << 16) + (response[46] << 8) + response[47]);

            return decoded;
        }
    }
}