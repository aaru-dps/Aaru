// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RT11.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : RT-11 file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the RT-11 file system and shows information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from http://www.trailing-edge.com/~shoppa/rt11fs/
    // TODO: Implement Radix-50
    public class RT11 : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;

        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "RT-11 file system";
        public virtual Guid Id => new Guid("DB3E2F98-8F98-463C-8126-E937843DA024");

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(1 + partition.Start >= partition.End) return false;

            if(imagePlugin.Info.SectorSize < 512) return false;

            byte[] magicB = new byte[12];
            byte[] hbSector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(hbSector, 0x1F0, magicB, 0, 12);
            string magic = Encoding.ASCII.GetString(magicB);

            return magic == "DECRT11A    ";
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] hbSector = imagePlugin.ReadSector(1 + partition.Start);

            GCHandle handle = GCHandle.Alloc(hbSector, GCHandleType.Pinned);
            RT11HomeBlock homeblock =
                (RT11HomeBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(RT11HomeBlock));
            handle.Free();

            /* TODO: Is this correct?
             * Assembler:
             *      MOV address, R0
             *      CLR R1
             *      MOV #255., R2
             * 10$: ADD (R0)+, R1
             *      SOB R2, 10$
             *      MOV 1,@R0
             */
            ushort check = 0;
            for(int i = 0; i < 512; i += 2) check += BitConverter.ToUInt16(hbSector, i);

            sb.AppendFormat("Volume format is {0}",
                            StringHandlers.SpacePaddedToString(homeblock.format, currentEncoding)).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", homeblock.cluster, homeblock.cluster * 512)
              .AppendLine();
            sb.AppendFormat("First directory segment starts at block {0}", homeblock.rootBlock).AppendLine();
            sb.AppendFormat("Volume owner is \"{0}\"",
                            StringHandlers.SpacePaddedToString(homeblock.ownername, currentEncoding)).AppendLine();
            sb.AppendFormat("Volume label: \"{0}\"",
                            StringHandlers.SpacePaddedToString(homeblock.volname, currentEncoding)).AppendLine();
            sb.AppendFormat("Checksum: 0x{0:X4} (calculated 0x{1:X4})", homeblock.checksum, check).AppendLine();

            byte[] bootBlock = imagePlugin.ReadSector(0);

            xmlFsType = new FileSystemType
            {
                Type = "RT-11",
                ClusterSize = homeblock.cluster * 512,
                Clusters = homeblock.cluster,
                VolumeName = StringHandlers.SpacePaddedToString(homeblock.volname, currentEncoding),
                Bootable = !ArrayHelpers.ArrayIsNullOrEmpty(bootBlock)
            };

            information = sb.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RT11HomeBlock
        {
            /// <summary>Bad block replacement table</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 130)] public byte[] badBlockTable;
            /// <summary>Unused</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] unused;
            /// <summary>INITIALIZE/RESTORE data area</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)] public byte[] initArea;
            /// <summary>BUP information area</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)] public byte[] bupInformation;
            /// <summary>Empty</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)] public byte[] empty;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] reserved1;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] reserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)] public byte[] empty2;
            /// <summary>Cluster size</summary>
            public ushort cluster;
            /// <summary>Block of the first directory segment</summary>
            public ushort rootBlock;
            /// <summary>"V3A" in Radix-50</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] systemVersion;
            /// <summary>Name of the volume, 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] volname;
            /// <summary>Name of the volume owner, 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] ownername;
            /// <summary>RT11 defines it as "DECRT11A    ", 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] format;
            /// <summary>Unused</summary>
            public ushort unused2;
            /// <summary>Checksum of preceding 255 words (16 bit units)</summary>
            public ushort checksum;
        }
    }
}