// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Apple DOS filesystem.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems.AppleDOS
{
    partial class AppleDOS : Filesystem
    {
        /// <summary>
        /// Mounts an Apple Lisa filesystem
        /// </summary>
        public override Errno Mount()
        {
            return Mount(false);
        }

        /// <summary>
        /// Mounts an Apple Lisa filesystem
        /// </summary>
        public override Errno Mount(bool debug)
        {
            if(device.ImageInfo.sectors != 455 && device.ImageInfo.sectors != 560)
            {
                DicConsole.DebugWriteLine("Apple DOS plugin", "Incorrect device size.");
                return Errno.InOutError;
            }

            if(start > 0)
            {
                DicConsole.DebugWriteLine("Apple DOS plugin", "Partitions are not supported.");
                return Errno.InOutError;
            }

            if(device.ImageInfo.sectorSize != 256)
            {
                DicConsole.DebugWriteLine("Apple DOS plugin", "Incorrect sector size.");
                return Errno.InOutError;
            }

            if(device.ImageInfo.sectors == 455)
                sectorsPerTrack = 13;
            else
                sectorsPerTrack = 16;

            // Read the VTOC
            byte[] vtoc_b = device.ReadSector((ulong)(17 * sectorsPerTrack));
            vtoc = new VTOC();
            IntPtr vtocPtr = Marshal.AllocHGlobal(256);
            Marshal.Copy(vtoc_b, 0, vtocPtr, 256);
            vtoc = (VTOC)Marshal.PtrToStructure(vtocPtr, typeof(VTOC));
            Marshal.FreeHGlobal(vtocPtr);

            track1UsedByFiles = false;
            track2UsedByFiles = false;
            usedSectors = 1;

            Errno error;

            error = ReadCatalog();
            if(error != Errno.NoError)
            {
                DicConsole.DebugWriteLine("Apple DOS plugin", "Unable to read catalog.");
                return error;
            }

            error = CacheAllFiles();
            if(error != Errno.NoError)
            {
                DicConsole.DebugWriteLine("Apple DOS plugin", "Unable cache all files.");
                return error;
            }

            // Create XML metadata for mounted filesystem
            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Bootable = true;
            xmlFSType.Clusters = (long)device.ImageInfo.sectors;
            xmlFSType.ClusterSize = vtoc.bytesPerSector;
            xmlFSType.Files = catalogCache.Count;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = xmlFSType.Clusters - usedSectors;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Type = "Apple DOS";

            this.debug = debug;
            mounted = true;
            return Errno.NoError;
        }

        /// <summary>
        /// Umounts this Lisa filesystem
        /// </summary>
        public override Errno Unmount()
        {
            mounted = false;
            extentCache = null;
            fileCache = null;
            catalogCache = null;
            fileSizeCache = null;

            return Errno.NoError;
        }

        /// <summary>
        /// Gets information about the mounted volume.
        /// </summary>
        /// <param name="stat">Information about the mounted volume.</param>
        public override Errno StatFs(ref FileSystemInfo stat)
        {
            stat = new FileSystemInfo();
            stat.Blocks = (long)device.ImageInfo.sectors;
            stat.FilenameLength = 30;
            stat.Files = (ulong)catalogCache.Count;
            stat.FreeBlocks = stat.Blocks - usedSectors;
            stat.FreeFiles = totalFileEntries - stat.Files;
            stat.PluginId = PluginUUID;
            stat.Type = "Apple DOS";

            return Errno.NoError;
        }
    }
}
