// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCEngine.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NEC PC-FX plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NEC PC-FX track header and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Not a filesystem, more like an executable header
    public class PCFX : IFilesystem
    {
        const  string         IDENTIFIER = "PC-FX:Hu_CD-ROM ";
        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "PC-FX Plugin";
        public Guid           Id        => new Guid("8BC27CCE-D9E9-48F8-BA93-C66A86EB565A");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start           >= partition.End ||
               imagePlugin.Info.XmlMediaType != XmlMediaType.OpticalDisc) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, 2);

            Encoding encoding = Encoding.GetEncoding("shift_jis");

            return encoding.GetString(sector, 0, 16) == IDENTIFIER;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            // Always Shift-JIS
            Encoding    = Encoding.GetEncoding("shift_jis");
            information = "";

            byte[]     sector = imagePlugin.ReadSectors(partition.Start, 2);
            PcfxHeader header = new PcfxHeader();
            IntPtr     sbPtr  = Marshal.AllocHGlobal(Marshal.SizeOf(header));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(header));
            header = (PcfxHeader)Marshal.PtrToStructure(sbPtr, typeof(PcfxHeader));
            Marshal.FreeHGlobal(sbPtr);

            string   date;
            DateTime dateTime = DateTime.MinValue;

            try
            {
                date      = Encoding.GetString(header.date);
                int year  = int.Parse(date.Substring(0, 4));
                int month = int.Parse(date.Substring(4, 2));
                int day   = int.Parse(date.Substring(6, 2));
                dateTime  = new DateTime(year, month, day);
            }
            catch { date = null; }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PC-FX executable:");
            sb.AppendFormat("Identifier: {0}",    StringHandlers.CToString(header.signature, Encoding)).AppendLine();
            sb.AppendFormat("Copyright: {0}", StringHandlers.CToString(header.copyright, Encoding)).AppendLine();
            sb.AppendFormat("Title: {0}", StringHandlers.CToString(header.title, Encoding)).AppendLine();
            sb.AppendFormat("Maker ID: {0}", StringHandlers.CToString(header.makerId, Encoding)).AppendLine();
            sb.AppendFormat("Maker name: {0}", StringHandlers.CToString(header.makerName, Encoding)).AppendLine();
            sb.AppendFormat("Volume number: {0}", header.volumeNumber).AppendLine();
            sb.AppendFormat("Country code: {0}",  header.country).AppendLine();
            sb.AppendFormat("Version: {0}.{1}",   header.minorVersion, header.majorVersion)
              .AppendLine();
            if(date != null) sb.AppendFormat("Dated {0}",       dateTime).AppendLine();
            sb.AppendFormat("Load {0} sectors from sector {1}", header.loadCount, header.loadOffset)
              .AppendLine();
            sb.AppendFormat("Load at 0x{0:X8} and jump to 0x{1:X8}", header.loadAddress, header.entryPoint)
              .AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                  = "PC-FX",
                Clusters              = (long)partition.Length,
                ClusterSize           = 2048,
                Bootable              = true,
                CreationDate          = dateTime,
                CreationDateSpecified = date != null,
                PublisherIdentifier   = StringHandlers.CToString(header.makerName, Encoding),
                VolumeName            = StringHandlers.CToString(header.title, Encoding),
                SystemIdentifier      = "PC-FX"
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PcfxHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
            public byte[] copyright;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x710)]
            public byte[] unknown;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] title;
            public uint   loadOffset;
            public uint   loadCount;
            public uint   loadAddress;
            public uint   entryPoint;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] makerId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public byte[] makerName;
            public uint   volumeNumber;
            public byte   majorVersion;
            public byte   minorVersion;
            public ushort country;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] date;
        }
    }
}