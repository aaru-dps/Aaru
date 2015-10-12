using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    struct sg_io_hdr_t
    {
        /// <summary>
        /// Always 'S' for SG v3
        /// </summary>
        public int interface_id;           /* [i] 'S' (required) */
        public ScsiIoctlDirection dxfer_direction;        /* [i] */
        public byte cmd_len;      /* [i] */
        public byte mx_sb_len;    /* [i] */
        public ushort iovec_count; /* [i] */
        public uint dxfer_len;     /* [i] */
        public IntPtr dxferp;              /* [i], [*io] */
        public IntPtr cmdp;       /* [i], [*i]  */
        public IntPtr sbp;        /* [i], [*o]  */
        public uint timeout;       /* [i] unit: millisecs */
        public uint flags;         /* [i] */
        public int pack_id;                /* [i->o] */
        public IntPtr usr_ptr;             /* [i->o] */
        public byte status;       /* [o] */
        public byte masked_status;/* [o] */
        public byte msg_status;   /* [o] */
        public byte sb_len_wr;    /* [o] */
        public ushort host_status; /* [o] */
        public ushort driver_status;/* [o] */
        public int resid;                  /* [o] */
        public uint duration;      /* [o] */
        public SgInfo info;          /* [o] */
    }

    [StructLayout(LayoutKind.Sequential)]
    struct hd_drive_task_hdr
    {
        public byte data;
        public byte feature;
        public byte sector_count;
        public byte sector_number;
        public byte low_cylinder;
        public byte high_cylinder;
        public byte device_head;
        public byte command;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct hd_drive_task_array
    {
        public byte command;
        public byte features;
        public byte sector_count;
        public byte sector_number;
        public byte low_cylinder;
        public byte high_cylinder;
        public byte device_head;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct hd_drive_cmd
    {
        public byte command;
        public byte sector;
        public byte feature;
        public byte nsector;
    }
}

