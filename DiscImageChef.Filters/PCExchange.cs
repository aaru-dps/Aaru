// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PCExchange.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filters.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides a filter to open handle files written by PCExchange in FAT
//     volumes
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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filters
{
    public class PCExchange : Filter
    {
        const string FILE_ID = "FILEID.DAT";
        const string FINDER_INFO = "FINDER.DAT";
        const string RESOURCES = "RESOURCE.FRK";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PCExchangeEntry
        {
            /// <summary>
            /// Name in Macintosh. If PCExchange version supports FAT's LFN they are the same.
            /// Illegal characters for FAT get substituted with '_' both here and in FAT's LFN entry.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] macName;
            /// <summary>
            /// File type
            /// </summary>
            public uint type;
            /// <summary>
            /// File creator
            /// </summary>
            public uint creator;
            /// <summary>
            /// Finder flags
            /// </summary>
            public ushort fdFlags;
            /// <summary>
            /// File's icon vertical position within its window
            /// </summary>
            public ushort verticalPosition;
            /// <summary>
            /// File's icon horizontal position within its window
            /// </summary>
            public ushort horizontalPosition;
            /// <summary>
            /// Unknown, all bytes are empty but last, except in volume's label entry
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)] public byte[] unknown1;
            /// <summary>
            /// File's creation date
            /// </summary>
            public uint creationDate;
            /// <summary>
            /// File's modification date
            /// </summary>
            public uint modificationDate;
            /// <summary>
            /// File's last backup date
            /// </summary>
            public uint backupDate;
            /// <summary>
            /// Unknown, but is unique, starts 0x7FFFFFFF and counts in reverse.
            /// Probably file ID for alias look up?
            /// </summary>
            public uint unknown2;
            /// <summary>
            /// Name as in FAT entry (not LFN).
            /// Resource fork file is always using this name, never LFN.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)] public byte[] dosName;
            /// <summary>
            /// Unknown, flags?
            /// </summary>
            public byte unknown3;
        }

        bool opened;
        string basePath;
        string dataPath;
        string rsrcPath;
        DateTime lastWriteTime;
        DateTime creationTime;
        long dataLen;
        long rsrcLen;

        public PCExchange()
        {
            Name = "PCExchange";
            UUID = new Guid("9264EB9F-D634-4F9B-BE12-C24CD44988C6");
        }

        public override void Close()
        {
            opened = false;
        }

        public override string GetBasePath()
        {
            return basePath;
        }

        public override DateTime GetCreationTime()
        {
            return creationTime;
        }

        public override long GetDataForkLength()
        {
            return dataLen;
        }

        public override Stream GetDataForkStream()
        {
            return new FileStream(dataPath, FileMode.Open, FileAccess.Read);
        }

        public override string GetFilename()
        {
            return Path.GetFileName(basePath);
        }

        public override DateTime GetLastWriteTime()
        {
            return lastWriteTime;
        }

        public override long GetLength()
        {
            return dataLen + rsrcLen;
        }

        public override string GetParentFolder()
        {
            return Path.GetDirectoryName(basePath);
        }

        public override string GetPath()
        {
            return basePath;
        }

        public override long GetResourceForkLength()
        {
            return rsrcLen;
        }

        public override Stream GetResourceForkStream()
        {
            return new FileStream(rsrcPath, FileMode.Open, FileAccess.Read);
        }

        public override bool HasResourceFork()
        {
            return rsrcPath != null;
        }

        public override bool Identify(byte[] buffer)
        {
            return false;
        }

        public override bool Identify(Stream stream)
        {
            System.Console.WriteLine("parentFolder");
            return false;
        }

        public override bool Identify(string path)
        {
            string parentFolder = Path.GetDirectoryName(path);

            if(!File.Exists(Path.Combine(parentFolder ?? throw new InvalidOperationException(), FINDER_INFO))) return false;

            if(!Directory.Exists(Path.Combine(parentFolder, RESOURCES))) return false;

            string baseFilename = Path.GetFileName(path);

            bool dataFound = false;
            bool rsrcFound = false;

            FileStream finderDatStream =
                new FileStream(Path.Combine(parentFolder, FINDER_INFO), FileMode.Open, FileAccess.Read);

            while(finderDatStream.Position + 0x5C <= finderDatStream.Length)
            {
                PCExchangeEntry datEntry = new PCExchangeEntry();
                byte[] datEntry_b = new byte[Marshal.SizeOf(datEntry)];
                finderDatStream.Read(datEntry_b, 0, Marshal.SizeOf(datEntry));
                datEntry = BigEndianMarshal.ByteArrayToStructureBigEndian<PCExchangeEntry>(datEntry_b);
                // TODO: Add support for encoding on filters
                string macName = StringHandlers.PascalToString(datEntry.macName, Encoding.GetEncoding("macintosh"));
                byte[] tmpDosName_b = new byte[8];
                byte[] tmpDosExt_b = new byte[3];
                Array.Copy(datEntry.dosName, 0, tmpDosName_b, 0, 8);
                Array.Copy(datEntry.dosName, 8, tmpDosExt_b, 0, 3);
                string dosName = Encoding.ASCII.GetString(tmpDosName_b).Trim() + "." +
                                 Encoding.ASCII.GetString(tmpDosExt_b).Trim();
                string dosNameLow = dosName.ToLower(CultureInfo.CurrentCulture);

                if(baseFilename != macName && baseFilename != dosName && baseFilename != dosNameLow) continue;

                dataFound |= File.Exists(Path.Combine(parentFolder, macName ?? throw new InvalidOperationException())) ||
                             File.Exists(Path.Combine(parentFolder, dosName)) ||
                             File.Exists(Path.Combine(parentFolder, dosNameLow));

                rsrcFound |= File.Exists(Path.Combine(parentFolder, RESOURCES, dosName)) ||
                             File.Exists(Path.Combine(parentFolder, RESOURCES, dosNameLow));

                break;
            }

            finderDatStream.Close();

            return dataFound && rsrcFound;
        }

        public override bool IsOpened()
        {
            return opened;
        }

        public override void Open(byte[] buffer)
        {
            throw new NotSupportedException();
        }

        public override void Open(Stream stream)
        {
            throw new NotSupportedException();
        }

        public override void Open(string path)
        {
            string parentFolder = Path.GetDirectoryName(path);
            string baseFilename = Path.GetFileName(path);

            FileStream finderDatStream =
                new FileStream(Path.Combine(parentFolder ?? throw new InvalidOperationException(), FINDER_INFO), FileMode.Open, FileAccess.Read);

            while(finderDatStream.Position + 0x5C <= finderDatStream.Length)
            {
                PCExchangeEntry datEntry = new PCExchangeEntry();
                byte[] datEntry_b = new byte[Marshal.SizeOf(datEntry)];
                finderDatStream.Read(datEntry_b, 0, Marshal.SizeOf(datEntry));
                datEntry = BigEndianMarshal.ByteArrayToStructureBigEndian<PCExchangeEntry>(datEntry_b);
                string macName = StringHandlers.PascalToString(datEntry.macName, Encoding.GetEncoding("macintosh"));
                byte[] tmpDosName_b = new byte[8];
                byte[] tmpDosExt_b = new byte[3];
                Array.Copy(datEntry.dosName, 0, tmpDosName_b, 0, 8);
                Array.Copy(datEntry.dosName, 8, tmpDosExt_b, 0, 3);
                string dosName = Encoding.ASCII.GetString(tmpDosName_b).Trim() + "." +
                                 Encoding.ASCII.GetString(tmpDosExt_b).Trim();
                string dosNameLow = dosName.ToLower(CultureInfo.CurrentCulture);

                if(baseFilename != macName && baseFilename != dosName && baseFilename != dosNameLow) continue;

                if(File.Exists(Path.Combine(parentFolder, macName ?? throw new InvalidOperationException()))) dataPath = Path.Combine(parentFolder, macName);
                else if(File.Exists(Path.Combine(parentFolder, dosName)))
                    dataPath = Path.Combine(parentFolder, dosName);
                else if(File.Exists(Path.Combine(parentFolder, dosNameLow)))
                    dataPath = Path.Combine(parentFolder, dosNameLow);
                else dataPath = null;

                if(File.Exists(Path.Combine(parentFolder, RESOURCES, dosName)))
                    rsrcPath = Path.Combine(parentFolder, RESOURCES, dosName);
                else if(File.Exists(Path.Combine(parentFolder, RESOURCES, dosNameLow)))
                    rsrcPath = Path.Combine(parentFolder, RESOURCES, dosNameLow);
                else rsrcPath = null;

                lastWriteTime = DateHandlers.MacToDateTime(datEntry.modificationDate);
                creationTime = DateHandlers.MacToDateTime(datEntry.creationDate);

                break;
            }

            dataLen = new FileInfo(dataPath ?? throw new InvalidOperationException()).Length;
            rsrcLen = new FileInfo(rsrcPath ?? throw new InvalidOperationException()).Length;

            basePath = path;
            opened = true;

            finderDatStream.Close();
        }
    }
}