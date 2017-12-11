// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FreeBSD direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures necessary for directly interfacing devices under
//     FreeBSD.
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
using path_id_t = System.UInt32;
using target_id_t = System.UInt32;
using lun_id_t = System.UInt32;

namespace DiscImageChef.Devices.FreeBSD
{
    [StructLayout(LayoutKind.Sequential)]
    struct ata_cmd
    {
        public CamAtaIoFlags flags;
        public byte command;
        public byte features;
        public byte lba_low;
        public byte lba_mid;
        public byte lba_high;
        public byte device;
        public byte lba_low_exp;
        public byte lba_mid_exp;
        public byte lba_high_exp;
        public byte features_exp;
        public byte sector_count;
        public byte sector_count_exp;
        public byte control;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ata_res
    {
        public CamAtaIoFlags flags;
        public byte status;
        public byte error;
        public byte lba_low;
        public byte lba_mid;
        public byte lba_high;
        public byte device;
        public byte lba_low_exp;
        public byte lba_mid_exp;
        public byte lba_high_exp;
        public byte sector_count;
        public byte sector_count_exp;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct cam_pinfo
    {
        public uint priority;
        public uint generation;
        public int index;
    }

    struct LIST_ENTRY
    {
        /// <summary>
        /// LIST_ENTRY(ccb_hdr)=le->*le_next
        /// </summary>
        public IntPtr le_next;
        /// <summary>
        /// LIST_ENTRY(ccb_hdr)=le->**le_prev
        /// </summary>
        public IntPtr le_prev;
    }

    struct SLIST_ENTRY
    {
        /// <summary>
        /// SLIST_ENTRY(ccb_hdr)=sle->*sle_next
        /// </summary>
        public IntPtr sle_next;
    }

    struct TAILQ_ENTRY
    {
        /// <summary>
        /// TAILQ_ENTRY(ccb_hdr)=tqe->*tqe_next
        /// </summary>
        public IntPtr tqe_next;
        /// <summary>
        /// TAILQ_ENTRY(ccb_hdr)=tqe->**tqe_prev
        /// </summary>
        public IntPtr tqe_prev;
    }

    struct STAILQ_ENTRY
    {
        /// <summary>
        /// STAILQ_ENTRY(ccb_hdr)=stqe->*stqe_next
        /// </summary>
        public IntPtr stqe_next;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct camq_entry
    {
        [FieldOffset(0)] public LIST_ENTRY le;
        [FieldOffset(0)] public SLIST_ENTRY sle;
        [FieldOffset(0)] public TAILQ_ENTRY tqe;
        [FieldOffset(0)] public STAILQ_ENTRY stqe;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct timeval
    {
        public long tv_sec;
        /// <summary>long</summary>
        public IntPtr tv_usec;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ccb_qos_area
    {
        public timeval etime;
        public UIntPtr sim_data;
        public UIntPtr periph_data;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ccb_hdr
    {
        public cam_pinfo pinfo;
        public camq_entry xpt_links;
        public camq_entry sim_links;
        public camq_entry periph_links;
        public uint retry_count;
        public IntPtr cbfcnp;
        public xpt_opcode func_code;
        public cam_status status;
        public IntPtr path;
        public uint path_id;
        public uint target_id;
        public ulong target_lun;
        public ccb_flags flags;
        public uint xflags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] periph_priv;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] sim_priv;
        public ccb_qos_area qos;
        public uint timeout;
        public timeval softtimeout;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct scsi_sense_data
    {
        const int SSD_FULL_SIZE = 252;
        public byte error_code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SSD_FULL_SIZE - 1)] public byte[] sense_buf;
    }

    /// <summary>
    /// SCSI I/O Request CCB used for the XPT_SCSI_IO and XPT_CONT_TARGET_IO function codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct ccb_scsiio
    {
        public ccb_hdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>    
        public IntPtr next_ccb;
        /// <summary>Ptr to mapping info</summary>
        public IntPtr req_map;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Autosense storage</summary>
        public scsi_sense_data sense_data;
        /// <summary>Number of bytes to autosense</summary>
        public byte sense_len;
        /// <summary>Number of bytes for the CDB</summary>
        public byte cdb_len;
        /// <summary>Number of SG list entries</summary>
        public short sglist_cnt;
        /// <summary>Returned SCSI status</summary>
        public byte scsi_status;
        /// <summary>Autosense resid length: 2's comp</summary>
        public sbyte sense_resid;
        /// <summary>Transfer residual length: 2's comp</summary>
        public int resid;
        /// <summary>
        /// Area for the CDB send, or pointer to the CDB bytes to send
        /// </summary>
        const int IOCDBLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IOCDBLEN)] public byte[] cdb_bytes;
        /// <summary>Pointer to the message buffer</summary>
        public IntPtr msg_ptr;
        /// <summary>Number of bytes for the Message</summary>
        public short msg_len;
        /// <summary>What to do for tag queueing. The tag action should be either the define below (to send a non-tagged transaction) or one of the defined scsi tag messages from scsi_message.h.</summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public uint tag_id;
        /// <summary>initiator id of who selected</summary>
        public uint init_id;
    }

    /// <summary>
    /// SCSI I/O Request CCB used for the XPT_SCSI_IO and XPT_CONT_TARGET_IO function codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct ccb_scsiio64
    {
        public ccb_hdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>    
        public IntPtr next_ccb;
        /// <summary>Ptr to mapping info</summary>
        public IntPtr req_map;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Autosense storage</summary>
        public scsi_sense_data sense_data;
        /// <summary>Number of bytes to autosense</summary>
        public byte sense_len;
        /// <summary>Number of bytes for the CDB</summary>
        public byte cdb_len;
        /// <summary>Number of SG list entries</summary>
        public short sglist_cnt;
        /// <summary>Returned SCSI status</summary>
        public byte scsi_status;
        /// <summary>Autosense resid length: 2's comp</summary>
        public sbyte sense_resid;
        /// <summary>Transfer residual length: 2's comp</summary>
        public int resid;
        public uint alignment; 
        /// <summary>
        /// Area for the CDB send, or pointer to the CDB bytes to send
        /// </summary>
        const int IOCDBLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IOCDBLEN)] public byte[] cdb_bytes;
        /// <summary>Pointer to the message buffer</summary>
        public IntPtr msg_ptr;
        /// <summary>Number of bytes for the Message</summary>
        public short msg_len;
        /// <summary>What to do for tag queueing. The tag action should be either the define below (to send a non-tagged transaction) or one of the defined scsi tag messages from scsi_message.h.</summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public uint tag_id;
        /// <summary>initiator id of who selected</summary>
        public uint init_id;
    }
    /// <summary>
    /// ATA I/O Request CCB used for the XPT_ATA_IO function code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct ccb_ataio
    {
        public ccb_hdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>    
        public IntPtr next_ccb;
        /// <summary>ATA command register set</summary>
        public ata_cmd cmd;
        /// <summary>ATA result register set</summary>
        public ata_res res;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Transfer residual length: 2's comp</summary>
        public int resid;
        /// <summary>Flags for the rest of the buffer</summary>
        public byte ata_flags;
        public uint aux;
        public uint unused;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct nvme_command
    {
        private ushort opc_fuse_rsvd1;
        /// <summary>
        /// command identifier
        /// </summary>
        public ushort cid;
        /// <summary>
        /// namespace identifier
        /// </summary>
        public uint nsid;
        /// <summary>
        /// reserved
        /// </summary>
        public uint rsvd2;
        /// <summary>
        /// reserved
        /// </summary>
        public uint rsvd3;
        /// <summary>
        /// metadata pointer
        /// </summary>
        public ulong mptr;
        /// <summary>
        /// prp entry 1
        /// </summary>
        public ulong prp1;
        /// <summary>
        /// prp entry 2
        /// </summary>
        public ulong prp2;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw10;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw11;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw12;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw13;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw14;
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw15;

        /// <summary>
        /// opcode
        /// </summary>
        public byte opc => (byte)((opc_fuse_rsvd1 & 0xFF00) >> 8);
        /// <summary>
        /// fused operation
        /// </summary>
        public byte fuse => (byte)((opc_fuse_rsvd1 & 0xC0) >> 6);
        /// <summary>
        /// reserved
        /// </summary>
        public byte rsvd1 => (byte)(opc_fuse_rsvd1 & 0x3F);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct nvme_status
    {
        private ushort status;

        /// <summary>
        /// phase tag
        /// </summary>
        public byte p => (byte)((status & 0x8000) >> 15);

        /// <summary>
        /// status code
        /// </summary>
        public byte sc => (byte)((status & 0x7F80) >> 7);

        /// <summary>
        /// status code type
        /// </summary>
        public byte sct => (byte)((status & 0x70) >> 4);

        /// <summary>
        /// reserved
        /// </summary>
        public byte rsvd2 => (byte)((status & 0xC) >> 15);

        /// <summary>
        /// more
        /// </summary>
        public byte m => (byte)((status & 0x2) >> 1);

        /// <summary>
        /// do not retry
        /// </summary>
        public byte dnr => (byte)(status & 0x1);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct nvme_completion
    {
        /// <summary>
        /// command-specific
        /// </summary>
        public uint cdw0;

        /// <summary>
        /// reserved
        /// </summary>
        public uint rsvd1;

        /// <summary>
        /// submission queue head pointer
        /// </summary>
        public ushort sqhd;

        /// <summary>
        /// submission queue identifier
        /// </summary>
        public ushort sqid;

        /// <summary>
        /// command identifier
        /// </summary>
        public ushort cid;

        public nvme_status status;
    }

    /// <summary>
    /// NVMe I/O Request CCB used for the XPT_NVME_IO and XPT_NVME_ADMIN function codes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct ccb_nvmeio
    {
        public ccb_hdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>    
        public IntPtr next_ccb;
        /// <summary>NVME command, per NVME standard</summary>
        public nvme_command cmd;
        /// <summary>NVME completion, per NVME standard</summary>
        public nvme_completion cpl;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Number of SG list entries</summary>
        public ushort sglist_cnt;
        /// <summary>padding for removed uint32_t</summary>
        public ushort unused;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct periph_match_pattern
    {
        private const int DEV_IDLEN = 16;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)] public byte[] periph_name;
        public uint unit_number;
        public path_id_t path_id;
        public target_id_t target_id;
        public lun_id_t target_lun;
        public periph_pattern_flags flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct device_id_match_pattern
    {
        public byte id_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] id;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct scsi_static_inquiry_pattern
    {
        private const int SID_VENDOR_SIZE = 8;
        private const int SID_PRODUCT_SIZE = 16;
        private const int SID_REVISION_SIZE = 4;
        public byte type;
        public byte media_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_VENDOR_SIZE + 1)] public byte[] vendor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_PRODUCT_SIZE + 1)] public byte[] product;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_REVISION_SIZE + 1)] public byte[] revision;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct device_match_pattern_data
    {
        [FieldOffset(0)] public scsi_static_inquiry_pattern inq_pat;
        [FieldOffset(0)] public device_id_match_pattern devid_pat;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct device_match_pattern
    {
        public path_id_t path_id;
        public target_id_t target_id;
        public lun_id_t target_lun;
        public dev_pattern_flags flags;
        public device_match_pattern_data data;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct bus_match_pattern
    {
        private const int DEV_IDLEN = 16;

        public path_id_t path_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)] public byte[] dev_name;
        public uint unit_number;
        public uint bus_id;
        bus_pattern_flags flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct match_pattern
    {
        [FieldOffset(0)] public periph_match_pattern periph_pattern;
        [FieldOffset(0)] public device_match_pattern device_pattern;
        [FieldOffset(0)] public bus_match_pattern bus_pattern;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct dev_match_pattern
    {
        public dev_match_type type;
        public match_pattern pattern;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct periph_match_result
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] periph_name;
        public uint unit_number;
        public path_id_t path_id;
        public target_id_t target_id;
        public lun_id_t target_lun;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct mmc_cid
    {
        public uint mid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] pnm;
        public uint psn;
        public ushort oid;
        public ushort mdt_year;
        public byte mdt_month;
        public byte prv;
        public byte fwrev;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct mmc_params
    {
        /// <summary>
        /// Card model
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)] public byte[] model;

        /// <summary>
        /// Card OCR
        /// </summary>
        public uint card_ocr;

        /// <summary>
        /// OCR of the IO portion of the card
        /// </summary>
        public uint io_ocr;

        /// <summary>
        /// Card CID -- raw 
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public uint[] card_cid;

        /// <summary>
        /// Card CID -- parsed 
        /// </summary>
        public mmc_cid cid;

        /// <summary>
        /// Card CSD -- raw
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public uint[] card_csd;

        /// <summary>
        /// Card RCA
        /// </summary>
        public ushort card_rca;

        /// <summary>
        /// What kind of card is it
        /// </summary>
        public mmc_card_features card_features;

        public byte sdio_func_count;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct device_match_result
    {
        public path_id_t path_id;
        public target_id_t target_id;
        public lun_id_t target_lun;
        public cam_proto protocol;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] inq_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] ident_data;
        public dev_result_flags flags;
        public mmc_params mmc_ident_data;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct bus_match_result
    {
        public path_id_t path_id;
        private const int DEV_IDLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)] public byte[] dev_name;
        public uint unit_number;
        public uint bus_id;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct match_result
    {
        [FieldOffset(0)] public periph_match_result periph_result;
        [FieldOffset(0)] public device_match_result device_result;
        [FieldOffset(0)] public bus_match_result bus_result;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct dev_match_result
    {
        public dev_match_type type;
        public match_result result;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ccb_dm_cookie
    {
        public IntPtr bus;
        public IntPtr target;
        public IntPtr device;
        public IntPtr periph;
        public IntPtr pdrv;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ccb_dev_position
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public cam_generations[] generations;
        dev_pos_type position_type;
        public ccb_dm_cookie cookie;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ccb_dev_match
    {
        public ccb_hdr ccb_h;
        ccb_dev_match_status status;
        public uint num_patterns;
        public uint pattern_buf_len;

        /// <summary>
        /// dev_match_pattern*
        /// </summary>
        public IntPtr patterns;

        public uint num_matches;
        public uint match_buf_len;

        /// <summary>
        /// dev_match_result*
        /// </summary>
        public IntPtr matches;

        public ccb_dev_position pos;
    }

    struct cam_device
    {
        private const int MAXPATHLEN = 1024;
        private const int DEV_IDLEN = 16;
        private const int SIM_IDLEN = 16;
        /// <summary>
        /// Pathname of the device given by the user. This may be null if the user states the device name and unit number separately.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPATHLEN)] public byte[] device_path;
        /// <summary>
        /// Device name given by the user.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN + 1)] public byte[] given_dev_name;
        /// <summary>
        /// Unit number given by the user.
        /// </summary>
        public uint given_unit_number;
        /// <summary>
        /// Name of the device, e.g. 'pass'
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN + 1)] public byte[] device_name;
        /// <summary>
        /// Unit number of the passthrough device associated with this particular device.
        /// </summary>
        public uint dev_unit_num;
        /// <summary>
        /// Controller name, e.g. 'ahc'
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIM_IDLEN + 1)] public byte[] sim_name;
        /// <summary>
        /// Controller unit number
        /// </summary>
        public uint sim_unit_number;
        /// <summary>
        /// Controller bus number
        /// </summary>
        public uint bus_id;
        /// <summary>
        /// Logical Unit Number
        /// </summary>
        public lun_id_t target_lun;
        /// <summary>
        /// Target ID
        /// </summary>
        public target_id_t target_id;
        /// <summary>
        /// System SCSI bus number
        /// </summary>
        public path_id_t path_id;
        /// <summary>
        /// type of peripheral device
        /// </summary>
        public ushort pd_type;
        /// <summary>
        /// SCSI Inquiry data
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] inq_data;
        /// <summary>
        /// device serial number
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)] public byte[] serial_num;
        /// <summary>
        /// length of the serial number
        /// </summary>
        public byte serial_num_len;
        /// <summary>
        /// Negotiated sync period
        /// </summary>
        public byte sync_period;
        /// <summary>
        /// Negotiated sync offset
        /// </summary>
        public byte sync_offset;
        /// <summary>
        /// Negotiated bus width
        /// </summary>
        public byte bus_width;
        /// <summary>
        /// file descriptor for device
        /// </summary>
        public int fd;
    }
}

