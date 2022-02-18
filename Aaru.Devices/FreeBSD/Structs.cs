// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using lun_id_t = System.UInt32;
using path_id_t = System.UInt32;
using target_id_t = System.UInt32;

// ReSharper disable BuiltInTypeReferenceStyle

#pragma warning disable 649
#pragma warning disable 169

namespace Aaru.Devices.FreeBSD
{
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct AtaCmd
    {
        public CamAtaIoFlags flags;
        public byte          command;
        public byte          features;
        public byte          lba_low;
        public byte          lba_mid;
        public byte          lba_high;
        public byte          device;
        public byte          lba_low_exp;
        public byte          lba_mid_exp;
        public byte          lba_high_exp;
        public byte          features_exp;
        public byte          sector_count;
        public byte          sector_count_exp;
        public byte          control;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct AtaRes
    {
        public CamAtaIoFlags flags;
        public byte          status;
        public byte          error;
        public byte          lba_low;
        public byte          lba_mid;
        public byte          lba_high;
        public byte          device;
        public byte          lba_low_exp;
        public byte          lba_mid_exp;
        public byte          lba_high_exp;
        public byte          sector_count;
        public byte          sector_count_exp;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CamPinfo
    {
        public uint priority;
        public uint generation;
        public int  index;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct ListEntry
    {
        /// <summary>LIST_ENTRY(ccb_hdr)=le->*le_next</summary>
        public IntPtr LeNext;
        /// <summary>LIST_ENTRY(ccb_hdr)=le->**le_prev</summary>
        public IntPtr LePrev;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct SlistEntry
    {
        /// <summary>SLIST_ENTRY(ccb_hdr)=sle->*sle_next</summary>
        public IntPtr SleNext;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct TailqEntry
    {
        /// <summary>TAILQ_ENTRY(ccb_hdr)=tqe->*tqe_next</summary>
        public IntPtr TqeNext;
        /// <summary>TAILQ_ENTRY(ccb_hdr)=tqe->**tqe_prev</summary>
        public IntPtr TqePrev;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct StailqEntry
    {
        /// <summary>STAILQ_ENTRY(ccb_hdr)=stqe->*stqe_next</summary>
        public IntPtr StqeNext;
    }

    [StructLayout(LayoutKind.Explicit), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CamqEntry
    {
        [FieldOffset(0)]
        public ListEntry le;
        [FieldOffset(0)]
        public SlistEntry sle;
        [FieldOffset(0)]
        public TailqEntry tqe;
        [FieldOffset(0)]
        public StailqEntry stqe;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct Timeval
    {
        public long tv_sec;
        /// <summary>long</summary>
        public IntPtr tv_usec;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbQosArea
    {
        public Timeval etime;
        public UIntPtr sim_data;
        public UIntPtr periph_data;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbHdr
    {
        public CamPinfo  pinfo;
        public CamqEntry xpt_links;
        public CamqEntry sim_links;
        public CamqEntry periph_links;
        public uint      retry_count;
        public IntPtr    cbfcnp;
        public XptOpcode func_code;
        public CamStatus status;
        public IntPtr    path;
        public uint      path_id;
        public uint      target_id;
        public ulong     target_lun;
        public CcbFlags  flags;
        public uint      xflags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] periph_priv;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] sim_priv;
        public CcbQosArea qos;
        public uint       timeout;
        public Timeval    softtimeout;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct ScsiSenseData
    {
        const  int  SSD_FULL_SIZE = 252;
        public byte error_code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SSD_FULL_SIZE - 1)]
        public byte[] sense_buf;
    }

    /// <summary>SCSI I/O Request CCB used for the XPT_SCSI_IO and XPT_CONT_TARGET_IO function codes.</summary>
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbScsiio
    {
        public CcbHdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>
        public IntPtr next_ccb;
        /// <summary>Ptr to mapping info</summary>
        public IntPtr req_map;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Autosense storage</summary>
        public ScsiSenseData sense_data;
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
        /// <summary>Area for the CDB send, or pointer to the CDB bytes to send</summary>
        const int IOCDBLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IOCDBLEN)]
        public byte[] cdb_bytes;
        /// <summary>Pointer to the message buffer</summary>
        public IntPtr msg_ptr;
        /// <summary>Number of bytes for the Message</summary>
        public short msg_len;
        /// <summary>
        ///     What to do for tag queueing. The tag action should be either the define below (to send a non-tagged
        ///     transaction) or one of the defined scsi tag messages from scsi_message.h.
        /// </summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public uint tag_id;
        /// <summary>initiator id of who selected</summary>
        public uint init_id;
    }

    /// <summary>SCSI I/O Request CCB used for the XPT_SCSI_IO and XPT_CONT_TARGET_IO function codes.</summary>
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbScsiio64
    {
        public CcbHdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>
        public IntPtr next_ccb;
        /// <summary>Ptr to mapping info</summary>
        public IntPtr req_map;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Autosense storage</summary>
        public ScsiSenseData sense_data;
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
        /// <summary>Area for the CDB send, or pointer to the CDB bytes to send</summary>
        const int IOCDBLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = IOCDBLEN)]
        public byte[] cdb_bytes;
        /// <summary>Pointer to the message buffer</summary>
        public IntPtr msg_ptr;
        /// <summary>Number of bytes for the Message</summary>
        public short msg_len;
        /// <summary>
        ///     What to do for tag queueing. The tag action should be either the define below (to send a non-tagged
        ///     transaction) or one of the defined scsi tag messages from scsi_message.h.
        /// </summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public uint tag_id;
        /// <summary>initiator id of who selected</summary>
        public uint init_id;
    }

    /// <summary>ATA I/O Request CCB used for the XPT_ATA_IO function code.</summary>
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbAtaio
    {
        public CcbHdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>
        public IntPtr next_ccb;
        /// <summary>ATA command register set</summary>
        public AtaCmd cmd;
        /// <summary>ATA result register set</summary>
        public AtaRes res;
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

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct NvmeCommand
    {
        readonly ushort opc_fuse_rsvd1;
        /// <summary>command identifier</summary>
        public ushort cid;
        /// <summary>namespace identifier</summary>
        public uint nsid;
        /// <summary>reserved</summary>
        public uint rsvd2;
        /// <summary>reserved</summary>
        public uint rsvd3;
        /// <summary>metadata pointer</summary>
        public ulong mptr;
        /// <summary>prp entry 1</summary>
        public ulong prp1;
        /// <summary>prp entry 2</summary>
        public ulong prp2;
        /// <summary>command-specific</summary>
        public uint cdw10;
        /// <summary>command-specific</summary>
        public uint cdw11;
        /// <summary>command-specific</summary>
        public uint cdw12;
        /// <summary>command-specific</summary>
        public uint cdw13;
        /// <summary>command-specific</summary>
        public uint cdw14;
        /// <summary>command-specific</summary>
        public uint cdw15;

        /// <summary>opcode</summary>
        public byte Opc => (byte)((opc_fuse_rsvd1 & 0xFF00) >> 8);
        /// <summary>fused operation</summary>
        public byte Fuse => (byte)((opc_fuse_rsvd1 & 0xC0) >> 6);
        /// <summary>reserved</summary>
        public byte Rsvd1 => (byte)(opc_fuse_rsvd1 & 0x3F);
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct NvmeStatus
    {
        readonly ushort status;

        /// <summary>phase tag</summary>
        public byte P => (byte)((status & 0x8000) >> 15);

        /// <summary>status code</summary>
        public byte Sc => (byte)((status & 0x7F80) >> 7);

        /// <summary>status code type</summary>
        public byte Sct => (byte)((status & 0x70) >> 4);

        /// <summary>reserved</summary>
        public byte Rsvd2 => (byte)((status & 0xC) >> 15);

        /// <summary>more</summary>
        public byte M => (byte)((status & 0x2) >> 1);

        /// <summary>do not retry</summary>
        public byte Dnr => (byte)(status & 0x1);
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct NvmeCompletion
    {
        /// <summary>command-specific</summary>
        public uint cdw0;

        /// <summary>reserved</summary>
        public uint rsvd1;

        /// <summary>submission queue head pointer</summary>
        public ushort sqhd;

        /// <summary>submission queue identifier</summary>
        public ushort sqid;

        /// <summary>command identifier</summary>
        public ushort cid;

        public NvmeStatus status;
    }

    /// <summary>NVMe I/O Request CCB used for the XPT_NVME_IO and XPT_NVME_ADMIN function codes.</summary>
    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbNvmeio
    {
        public CcbHdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>
        public IntPtr next_ccb;
        /// <summary>NVME command, per NVME standard</summary>
        public NvmeCommand cmd;
        /// <summary>NVME completion, per NVME standard</summary>
        public NvmeCompletion cpl;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public uint dxfer_len;
        /// <summary>Number of SG list entries</summary>
        public ushort sglist_cnt;
        /// <summary>padding for removed uint32_t</summary>
        public ushort unused;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct PeriphMatchPattern
    {
        const int DEV_IDLEN = 16;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)]
        public byte[] periph_name;
        public uint               unit_number;
        public path_id_t          path_id;
        public target_id_t        target_id;
        public lun_id_t           target_lun;
        public PeriphPatternFlags flags;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DeviceIdMatchPattern
    {
        public byte id_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] id;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct ScsiStaticInquiryPattern
    {
        const  int  SID_VENDOR_SIZE   = 8;
        const  int  SID_PRODUCT_SIZE  = 16;
        const  int  SID_REVISION_SIZE = 4;
        public byte type;
        public byte media_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_VENDOR_SIZE + 1)]
        public byte[] vendor;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_PRODUCT_SIZE + 1)]
        public byte[] product;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SID_REVISION_SIZE + 1)]
        public byte[] revision;
    }

    [StructLayout(LayoutKind.Explicit), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DeviceMatchPatternData
    {
        [FieldOffset(0)]
        public ScsiStaticInquiryPattern inq_pat;
        [FieldOffset(0)]
        public DeviceIdMatchPattern devid_pat;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DeviceMatchPattern
    {
        public uint                   path_id;
        public uint                   target_id;
        public uint                   target_lun;
        public DevPatternFlags        flags;
        public DeviceMatchPatternData data;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct BusMatchPattern
    {
        const int DEV_IDLEN = 16;

        public path_id_t path_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)]
        public byte[] dev_name;
        public   uint            unit_number;
        public   uint            bus_id;
        readonly BusPatternFlags flags;
    }

    [StructLayout(LayoutKind.Explicit), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct MatchPattern
    {
        [FieldOffset(0)]
        public PeriphMatchPattern periph_pattern;
        [FieldOffset(0)]
        public DeviceMatchPattern device_pattern;
        [FieldOffset(0)]
        public BusMatchPattern bus_pattern;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DevMatchPattern
    {
        public DevMatchType type;
        public MatchPattern pattern;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct PeriphMatchResult
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] periph_name;
        public uint        unit_number;
        public path_id_t   path_id;
        public target_id_t target_id;
        public lun_id_t    target_lun;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct MmcCid
    {
        public uint mid;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] pnm;
        public uint   psn;
        public ushort oid;
        public ushort mdt_year;
        public byte   mdt_month;
        public byte   prv;
        public byte   fwrev;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct MmcParams
    {
        /// <summary>Card model</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] model;

        /// <summary>Card OCR</summary>
        public uint card_ocr;

        /// <summary>OCR of the IO portion of the card</summary>
        public uint io_ocr;

        /// <summary>Card CID -- raw</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] card_cid;

        /// <summary>Card CID -- parsed</summary>
        public MmcCid cid;

        /// <summary>Card CSD -- raw</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] card_csd;

        /// <summary>Card RCA</summary>
        public ushort card_rca;

        /// <summary>What kind of card is it</summary>
        public MmcCardFeatures card_features;

        public byte sdio_func_count;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DeviceMatchResult
    {
        public path_id_t   path_id;
        public target_id_t target_id;
        public lun_id_t    target_lun;
        public CamProto    protocol;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] inq_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ident_data;
        public DevResultFlags flags;
        public MmcParams      mmc_ident_data;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct BusMatchResult
    {
        public path_id_t path_id;
        const  int       DEV_IDLEN = 16;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN)]
        public byte[] dev_name;
        public uint unit_number;
        public uint bus_id;
    }

    [StructLayout(LayoutKind.Explicit), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct MatchResult
    {
        [FieldOffset(0)]
        public PeriphMatchResult periph_result;
        [FieldOffset(0)]
        public DeviceMatchResult device_result;
        [FieldOffset(0)]
        public BusMatchResult bus_result;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct DevMatchResult
    {
        public DevMatchType type;
        public MatchResult  result;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbDmCookie
    {
        public IntPtr bus;
        public IntPtr target;
        public IntPtr device;
        public IntPtr periph;
        public IntPtr pdrv;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbDevPosition
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public CamGenerations[] generations;
        readonly DevPosType  position_type;
        public   CcbDmCookie cookie;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbDevMatch
    {
        public   CcbHdr            ccb_h;
        readonly CcbDevMatchStatus status;
        public   uint              num_patterns;
        public   uint              pattern_buf_len;

        /// <summary>dev_match_pattern*</summary>
        public IntPtr patterns;

        public uint num_matches;
        public uint match_buf_len;

        /// <summary>dev_match_result*</summary>
        public IntPtr matches;

        public CcbDevPosition pos;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CamDevice
    {
        const int MAXPATHLEN = 1024;
        const int DEV_IDLEN  = 16;
        const int SIM_IDLEN  = 16;
        /// <summary>
        ///     Pathname of the device given by the user. This may be null if the user states the device name and unit number
        ///     separately.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXPATHLEN)]
        public byte[] DevicePath;
        /// <summary>Device name given by the user.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN + 1)]
        public byte[] GivenDevName;
        /// <summary>Unit number given by the user.</summary>
        public uint GivenUnitNumber;
        /// <summary>Name of the device, e.g. 'pass'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = DEV_IDLEN + 1)]
        public byte[] DeviceName;
        /// <summary>Unit number of the passthrough device associated with this particular device.</summary>
        public uint DevUnitNum;
        /// <summary>Controller name, e.g. 'ahc'</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = SIM_IDLEN + 1)]
        public byte[] SimName;
        /// <summary>Controller unit number</summary>
        public uint SimUnitNumber;
        /// <summary>Controller bus number</summary>
        public uint BusId;
        /// <summary>Logical Unit Number</summary>
        public lun_id_t TargetLun;
        /// <summary>Target ID</summary>
        public target_id_t TargetId;
        /// <summary>System SCSI bus number</summary>
        public path_id_t PathId;
        /// <summary>type of peripheral device</summary>
        public ushort PdType;
        /// <summary>SCSI Inquiry data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] InqData;
        /// <summary>device serial number</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)]
        public byte[] SerialNum;
        /// <summary>length of the serial number</summary>
        public byte SerialNumLen;
        /// <summary>Negotiated sync period</summary>
        public byte SyncPeriod;
        /// <summary>Negotiated sync offset</summary>
        public byte SyncOffset;
        /// <summary>Negotiated bus width</summary>
        public byte BusWidth;
        /// <summary>file descriptor for device</summary>
        public int Fd;
    }

    [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), Obsolete]
    internal struct CcbGetdev
    {
        public CcbHdr   ccb_h;
        public CamProto protocol;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] inq_data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ident_data;
        /// <summary>device serial number</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)]
        public byte[] serial_num;
        public byte inq_flags;
        /// <summary>length of the serial number</summary>
        public byte serial_num_len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] padding;
    }
}