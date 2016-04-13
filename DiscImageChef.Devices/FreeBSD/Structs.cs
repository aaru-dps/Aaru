// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains structures necessary for directly interfacing devices under FreeBSD
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.FreeBSD
{
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

    struct cam_pinfo
    {
        public UInt32 priority;
        public UInt32 generation;
        public int index;
    }

    struct camq_entry
    {
        /// <summary>
        /// LIST_ENTRY(ccb_hdr)=le->*le_next
        /// </summary>
        public IntPtr le_next;
        /// <summary>
        /// LIST_ENTRY(ccb_hdr)=le->**le_prev
        /// </summary>
        public IntPtr le_prev;
        /// <summary>
        /// SLIST_ENTRY(ccb_hdr)=sle->*sle_next
        /// </summary>
        public IntPtr sle_next;
        /// <summary>
        /// TAILQ_ENTRY(ccb_hdr)=tqe->*tqe_next
        /// </summary>
        public IntPtr tqe_next;
        /// <summary>
        /// TAILQ_ENTRY(ccb_hdr)=tqe->**tqe_prev
        /// </summary>
        public IntPtr tqe_prev;
        /// <summary>
        /// STAILQ_ENTRY(ccb_hdr)=stqe->*stqe_next
        /// </summary>
        public IntPtr stqe_next;
    }

    struct timeval
    {
        public Int64 tv_sec;
        /// <summary>long</summary>
        public IntPtr tv_usec;
    }

    struct ccb_qos_area
    {
        public timeval etime;
        public UIntPtr sim_data;
        public UIntPtr periph_data;
    }

    struct ccb_hdr
    {
        public cam_pinfo pinfo;
        public camq_entry xpt_links;
        public camq_entry sim_links;
        public camq_entry periph_links;
        public UInt32 retry_count;
        public IntPtr cbfcnp;
        public xpt_opcode func_code;
        public UInt32 status;
        public IntPtr path;
        public uint path_id;
        public uint target_id;
        public UInt64 target_lun;
        public UInt32 flags;
        public UInt32 xflags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] periph_priv;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public IntPtr[] sim_priv;
        public ccb_qos_area qos;
        public UInt32 timeout;
        public timeval softtimeout;
    }

    struct scsi_sense_data
    {
        public byte error_code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 251)]
        public byte[] sense_buf;
    }

    /// <summary>
    /// SCSI I/O Request CCB used for the XPT_SCSI_IO and XPT_CONT_TARGET_IO function codes.
    /// </summary>
    struct cdb_scsiio
    {
        public ccb_hdr ccb_h;
        /// <summary>Ptr for next CCB for action</summary>    
        public IntPtr next_ccb;
        /// <summary>Ptr to mapping info</summary>
        public IntPtr req_map;
        /// <summary>Ptr to the data buf/SG list</summary>
        public IntPtr data_ptr;
        /// <summary>Data transfer length</summary>
        public UInt32 dxfer_len;
        /// <summary>Autosense storage</summary>
        public scsi_sense_data sense_data;
        /// <summary>Number of bytes to autosense</summary>
        public byte sense_len;
        /// <summary>Number of bytes for the CDB</summary>
        public byte cdb_len;
        /// <summary>Number of SG list entries</summary>
        public UInt16 sglist_cnt;
        /// <summary>Returned SCSI status</summary>
        public byte scsi_status;
        /// <summary>Autosense resid length: 2's comp</summary>
        public sbyte sense_resid;
        /// <summary>Transfer residual length: 2's comp</summary>
        public Int32 resid;
        /// <summary>Union for CDB bytes/pointer</summary>
        public IntPtr cdb_io;
        /// <summary>Pointer to the message buffer</summary>
        public IntPtr msg_ptr;
        /// <summary>Number of bytes for the Message</summary>
        public UInt16 msg_len;
        /// <summary>What to do for tag queueing. The tag action should be either the define below (to send a non-tagged transaction) or one of the defined scsi tag messages from scsi_message.h.</summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public UInt32 tag_id;
        /// <summary>initiator id of who selected</summary>
        public UInt32 init_id;
    }

    /// <summary>
    /// ATA I/O Request CCB used for the XPT_ATA_IO function code.
    /// </summary>
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
        public UInt32 dxfer_len;
        /// <summary>Transfer residual length: 2's comp</summary>
        public Int32 resid;
        /// <summary>What to do for tag queueing. The tag action should be either the define below (to send a non-tagged transaction) or one of the defined scsi tag messages from scsi_message.h.</summary>
        public byte tag_action;
        /// <summary>tag id from initator (target mode)</summary>
        public UInt32 tag_id;
        /// <summary>initiator id of who selected</summary>
        public UInt32 init_id;
    }
}

