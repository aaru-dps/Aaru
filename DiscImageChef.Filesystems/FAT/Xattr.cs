// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xattr.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Microsoft FAT extended attributes.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems.FAT
{
    public partial class FAT
    {
        Dictionary<string, Dictionary<string, byte[]>> eaCache;

        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        public Errno ListXAttr(string path, out List<string> xattrs)
        {
            xattrs = null;
            if(!mounted) return Errno.AccessDenied;

            // No other xattr recognized yet
            if(cachedEaData is null) return Errno.NotSupported;

            if(path[0] == '/') path = path.Substring(1);

            if(eaCache.TryGetValue(path.ToLower(cultureInfo), out Dictionary<string, byte[]> eas))
            {
                xattrs = eas.Keys.ToList();
                return Errno.NoError;
            }

            Errno err = GetFileEntry(path, out DirectoryEntry entry);

            if(err != Errno.NoError) return err;

            xattrs = new List<string>();

            if(entry.ea_handle == 0) return Errno.NoError;

            eas = GetEas(entry.ea_handle);

            if(eas is null) return Errno.NoError;

            eaCache.Add(path.ToLower(cultureInfo), eas);
            xattrs = eas.Keys.ToList();
            return Errno.NoError;
        }

        /// <summary>
        ///     Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extendad attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        public Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            Errno err = ListXAttr(path, out List<string> xattrs);

            if(err != Errno.NoError) return err;

            if(path[0] == '/') path = path.Substring(1);

            if(!xattrs.Contains(xattr.ToLower(cultureInfo))) return Errno.NoSuchExtendedAttribute;

            if(!eaCache.TryGetValue(path.ToLower(cultureInfo), out Dictionary<string, byte[]> eas))
                return Errno.InvalidArgument;

            if(!eas.TryGetValue(xattr.ToLower(cultureInfo), out byte[] data)) return Errno.InvalidArgument;

            buf = new byte[data.Length];
            data.CopyTo(buf, 0);

            return Errno.NoError;
        }

        Dictionary<string, byte[]> GetEas(ushort eaHandle)
        {
            int aIndex = eaHandle >> 7;
            // First 0x20 bytes are the magic number and unused words
            ushort a = BitConverter.ToUInt16(cachedEaData, aIndex * 2 + 0x20);

            ushort b = BitConverter.ToUInt16(cachedEaData, eaHandle * 2 + 0x200);

            uint eaCluster = (uint)(a + b);

            if(b == EA_UNUSED) return null;

            EaHeader header =
                Marshal.ByteArrayToStructureLittleEndian<EaHeader>(cachedEaData, (int)(eaCluster * bytesPerCluster),
                                                                   Marshal.SizeOf<EaHeader>());

            if(header.magic != 0x4145) return null;

            uint eaLen = BitConverter.ToUInt32(cachedEaData,
                                               (int)(eaCluster * bytesPerCluster) + Marshal.SizeOf<EaHeader>());

            byte[] eaData = new byte[eaLen];
            Array.Copy(cachedEaData, (int)(eaCluster * bytesPerCluster) + Marshal.SizeOf<EaHeader>(), eaData, 0, eaLen);

            Dictionary<string, byte[]> eas = new Dictionary<string, byte[]>();

            if(debug) eas.Add("com.microsoft.os2.fea", eaData);

            int pos = 4;
            while(pos < eaData.Length)
            {
                byte   fEA     = eaData[pos++];
                byte   cbName  = eaData[pos++];
                ushort cbValue = BitConverter.ToUInt16(eaData, pos);
                pos += 2;

                string name = Encoding.ASCII.GetString(eaData, pos, cbName);
                pos += cbName;
                pos++;
                byte[] data = new byte[cbValue];

                Array.Copy(eaData, pos, data, 0, cbValue);
                pos += cbValue;

                // OS/2 System Attributes
                if(name[0] == '.')
                {
                    // This is WorkPlace System information so it's IBM
                    if(name == ".CLASSINFO") name = "com.ibm.os2.classinfo";
                    else name                     = "com.microsoft.os2" + name.ToLower();
                }

                eas.Add(name, data);
            }

            return eas;
        }

        void CacheEaData()
        {
            if(eaDirEntry.start_cluster == 0) return;

            MemoryStream eaDataMs = new MemoryStream();

            foreach(byte[] buffer in GetClusters(eaDirEntry.start_cluster)
               .Select(cluster => image.ReadSectors(firstClusterSector + cluster * sectorsPerCluster,
                                                    sectorsPerCluster))) eaDataMs.Write(buffer, 0, buffer.Length);

            cachedEaData = eaDataMs.ToArray();
        }
    }
}