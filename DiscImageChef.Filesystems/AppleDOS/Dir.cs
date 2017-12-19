// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple DOS filesystem catalog (aka directory).
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
using System.IO;
using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.AppleDOS
{
    public partial class AppleDOS : Filesystem
    {
        /// <summary>
        /// Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        public override Errno ReadLink(string path, ref string dest)
        {
            if(!mounted) return Errno.AccessDenied;

            return Errno.NotSupported;
        }

        /// <summary>
        /// Lists contents from a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="contents">Directory contents.</param>
        public override Errno ReadDir(string path, ref List<string> contents)
        {
            if(!mounted) return Errno.AccessDenied;

            if(!string.IsNullOrEmpty(path) && string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
                return Errno.NotSupported;

            contents = new List<string>();
            foreach(string ent in catalogCache.Keys) contents.Add(ent);

            if(debug)
            {
                contents.Add("$");
                contents.Add("$Boot");
                contents.Add("$Vtoc");
            }

            contents.Sort();
            return Errno.NoError;
        }

        Errno ReadCatalog()
        {
            MemoryStream catalogMs = new MemoryStream();
            ulong lba = (ulong)((vtoc.catalogTrack * sectorsPerTrack) + vtoc.catalogSector);
            totalFileEntries = 0;
            catalogCache = new Dictionary<string, ushort>();
            fileTypeCache = new Dictionary<string, byte>();
            fileSizeCache = new Dictionary<string, int>();
            lockedFiles = new List<string>();

            if(lba == 0 || lba > device.ImageInfo.sectors) return Errno.InvalidArgument;

            while(lba != 0)
            {
                usedSectors++;
                byte[] catSector_b = device.ReadSector(lba);
                totalFileEntries += 7;
                if(debug) catalogMs.Write(catSector_b, 0, catSector_b.Length);

                // Read the catalog sector
                CatalogSector catSector = new CatalogSector();
                IntPtr catPtr = Marshal.AllocHGlobal(256);
                Marshal.Copy(catSector_b, 0, catPtr, 256);
                catSector = (CatalogSector)Marshal.PtrToStructure(catPtr, typeof(CatalogSector));
                Marshal.FreeHGlobal(catPtr);

                foreach(FileEntry entry in catSector.entries)
                {
                    if(entry.extentTrack > 0)
                    {
                        track1UsedByFiles |= entry.extentTrack == 1;
                        track2UsedByFiles |= entry.extentTrack == 2;

                        byte[] filename_b = new byte[30];
                        ushort ts = (ushort)((entry.extentTrack << 8) | entry.extentSector);

                        // Apple DOS has high byte set over ASCII.
                        for(int i = 0; i < 30; i++) filename_b[i] = (byte)(entry.filename[i] & 0x7F);

                        string filename = StringHandlers.SpacePaddedToString(filename_b, CurrentEncoding);

                        if(!catalogCache.ContainsKey(filename)) catalogCache.Add(filename, ts);

                        if(!fileTypeCache.ContainsKey(filename))
                            fileTypeCache.Add(filename, (byte)(entry.typeAndFlags & 0x7F));

                        if(!fileSizeCache.ContainsKey(filename))
                            fileSizeCache.Add(filename, entry.length * vtoc.bytesPerSector);

                        if((entry.typeAndFlags & 0x80) == 0x80 && !lockedFiles.Contains(filename))
                            lockedFiles.Add(filename);
                    }
                }

                lba = (ulong)((catSector.trackOfNext * sectorsPerTrack) + catSector.sectorOfNext);

                if(lba > device.ImageInfo.sectors) break;
            }

            if(debug) catalogBlocks = catalogMs.ToArray();

            return Errno.NoError;
        }
    }
}