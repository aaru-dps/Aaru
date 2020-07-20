// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Aaru.Filters
{
    /// <summary>Decodes PCExchange files</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class PCExchange : IFilter
    {
        const string FILE_ID     = "FILEID.DAT";
        const string FINDER_INFO = "FINDER.DAT";
        const string RESOURCES   = "RESOURCE.FRK";
        string       basePath;
        DateTime     creationTime;
        long         dataLen;
        string       dataPath;
        DateTime     lastWriteTime;

        bool   opened;
        long   rsrcLen;
        string rsrcPath;

        public string Name   => "PCExchange";
        public Guid   Id     => new Guid("9264EB9F-D634-4F9B-BE12-C24CD44988C6");
        public string Author => "Natalia Portillo";

        public void Close() => opened = false;

        public string GetBasePath() => basePath;

        public DateTime GetCreationTime() => creationTime;

        public long GetDataForkLength() => dataLen;

        public Stream GetDataForkStream() => new FileStream(dataPath, FileMode.Open, FileAccess.Read);

        public string GetFilename() => Path.GetFileName(basePath);

        public DateTime GetLastWriteTime() => lastWriteTime;

        public long GetLength() => dataLen + rsrcLen;

        public string GetParentFolder() => Path.GetDirectoryName(basePath);

        public string GetPath() => basePath;

        public long GetResourceForkLength() => rsrcLen;

        public Stream GetResourceForkStream() => new FileStream(rsrcPath, FileMode.Open, FileAccess.Read);

        public bool HasResourceFork() => rsrcPath != null;

        public bool Identify(byte[] buffer) => false;

        public bool Identify(Stream stream) => false;

        public bool Identify(string path)
        {
            string parentFolder = Path.GetDirectoryName(path);

            parentFolder = parentFolder ?? "";

            if(!File.Exists(Path.Combine(parentFolder, FINDER_INFO)))
                return false;

            if(!Directory.Exists(Path.Combine(parentFolder, RESOURCES)))
                return false;

            string baseFilename = Path.GetFileName(path);

            bool dataFound = false;
            bool rsrcFound = false;

            var finderDatStream =
                new FileStream(Path.Combine(parentFolder, FINDER_INFO), FileMode.Open, FileAccess.Read);

            while(finderDatStream.Position + 0x5C <= finderDatStream.Length)
            {
                var    datEntry   = new PCExchangeEntry();
                byte[] datEntry_b = new byte[Marshal.SizeOf(datEntry)];
                finderDatStream.Read(datEntry_b, 0, Marshal.SizeOf(datEntry));
                datEntry = Helpers.Marshal.ByteArrayToStructureBigEndian<PCExchangeEntry>(datEntry_b);

                // TODO: Add support for encoding on filters
                string macName = StringHandlers.PascalToString(datEntry.macName, Encoding.GetEncoding("macintosh"));

                byte[] tmpDosName_b = new byte[8];
                byte[] tmpDosExt_b  = new byte[3];
                Array.Copy(datEntry.dosName, 0, tmpDosName_b, 0, 8);
                Array.Copy(datEntry.dosName, 8, tmpDosExt_b, 0, 3);

                string dosName = Encoding.ASCII.GetString(tmpDosName_b).Trim() + "." +
                                 Encoding.ASCII.GetString(tmpDosExt_b).Trim();

                string dosNameLow = dosName.ToLower(CultureInfo.CurrentCulture);

                if(baseFilename != macName &&
                   baseFilename != dosName &&
                   baseFilename != dosNameLow)
                    continue;

                dataFound |=
                    File.Exists(Path.Combine(parentFolder, macName ?? throw new InvalidOperationException())) ||
                    File.Exists(Path.Combine(parentFolder, dosName))                                          ||
                    File.Exists(Path.Combine(parentFolder, dosNameLow));

                rsrcFound |= File.Exists(Path.Combine(parentFolder, RESOURCES, dosName)) ||
                             File.Exists(Path.Combine(parentFolder, RESOURCES, dosNameLow));

                break;
            }

            finderDatStream.Close();

            return dataFound && rsrcFound;
        }

        public bool IsOpened() => opened;

        public void Open(byte[] buffer) => throw new NotSupportedException();

        public void Open(Stream stream) => throw new NotSupportedException();

        public void Open(string path)
        {
            string parentFolder = Path.GetDirectoryName(path);
            string baseFilename = Path.GetFileName(path);

            parentFolder = parentFolder ?? "";

            var finderDatStream =
                new FileStream(Path.Combine(parentFolder, FINDER_INFO), FileMode.Open, FileAccess.Read);

            while(finderDatStream.Position + 0x5C <= finderDatStream.Length)
            {
                var    datEntry   = new PCExchangeEntry();
                byte[] datEntry_b = new byte[Marshal.SizeOf(datEntry)];
                finderDatStream.Read(datEntry_b, 0, Marshal.SizeOf(datEntry));
                datEntry = Helpers.Marshal.ByteArrayToStructureBigEndian<PCExchangeEntry>(datEntry_b);

                string macName = StringHandlers.PascalToString(datEntry.macName, Encoding.GetEncoding("macintosh"));

                byte[] tmpDosName_b = new byte[8];
                byte[] tmpDosExt_b  = new byte[3];
                Array.Copy(datEntry.dosName, 0, tmpDosName_b, 0, 8);
                Array.Copy(datEntry.dosName, 8, tmpDosExt_b, 0, 3);

                string dosName = Encoding.ASCII.GetString(tmpDosName_b).Trim() + "." +
                                 Encoding.ASCII.GetString(tmpDosExt_b).Trim();

                string dosNameLow = dosName.ToLower(CultureInfo.CurrentCulture);

                if(baseFilename != macName &&
                   baseFilename != dosName &&
                   baseFilename != dosNameLow)
                    continue;

                if(File.Exists(Path.Combine(parentFolder, macName ?? throw new InvalidOperationException())))
                    dataPath = Path.Combine(parentFolder, macName);
                else if(File.Exists(Path.Combine(parentFolder, dosName)))
                    dataPath = Path.Combine(parentFolder, dosName);
                else if(File.Exists(Path.Combine(parentFolder, dosNameLow)))
                    dataPath = Path.Combine(parentFolder, dosNameLow);
                else
                    dataPath = null;

                if(File.Exists(Path.Combine(parentFolder, RESOURCES, dosName)))
                    rsrcPath = Path.Combine(parentFolder, RESOURCES, dosName);
                else if(File.Exists(Path.Combine(parentFolder, RESOURCES, dosNameLow)))
                    rsrcPath = Path.Combine(parentFolder, RESOURCES, dosNameLow);
                else
                    rsrcPath = null;

                lastWriteTime = DateHandlers.MacToDateTime(datEntry.modificationDate);
                creationTime  = DateHandlers.MacToDateTime(datEntry.creationDate);

                break;
            }

            dataLen = new FileInfo(dataPath ?? throw new InvalidOperationException()).Length;
            rsrcLen = new FileInfo(rsrcPath ?? throw new InvalidOperationException()).Length;

            basePath = path;
            opened   = true;

            finderDatStream.Close();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PCExchangeEntry
        {
            /// <summary>
            ///     Name in Macintosh. If PCExchange version supports FAT's LFN they are the same. Illegal characters for FAT get
            ///     substituted with '_' both here and in FAT's LFN entry.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] macName;
            /// <summary>File type</summary>
            public readonly uint type;
            /// <summary>File creator</summary>
            public readonly uint creator;
            /// <summary>Finder flags</summary>
            public readonly ushort fdFlags;
            /// <summary>File's icon vertical position within its window</summary>
            public readonly ushort verticalPosition;
            /// <summary>File's icon horizontal position within its window</summary>
            public readonly ushort horizontalPosition;
            /// <summary>Unknown, all bytes are empty but last, except in volume's label entry</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public readonly byte[] unknown1;
            /// <summary>File's creation date</summary>
            public readonly uint creationDate;
            /// <summary>File's modification date</summary>
            public readonly uint modificationDate;
            /// <summary>File's last backup date</summary>
            public readonly uint backupDate;
            /// <summary>Unknown, but is unique, starts 0x7FFFFFFF and counts in reverse. Probably file ID for alias look up?</summary>
            public readonly uint unknown2;
            /// <summary>Name as in FAT entry (not LFN). Resource fork file is always using this name, never LFN.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public readonly byte[] dosName;
            /// <summary>Unknown, flags?</summary>
            public readonly byte unknown3;
        }
    }
}