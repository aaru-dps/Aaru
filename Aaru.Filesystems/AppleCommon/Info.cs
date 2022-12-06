// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common Apple file systems.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Macintosh Boot Block information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Text;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    internal static partial class AppleCommon
    {
        internal static string GetBootBlockInformation(byte[] bbSector, Encoding encoding)
        {
            if(bbSector is null ||
               bbSector.Length < 0x100)
                return null;

            BootBlock bb = Marshal.ByteArrayToStructureBigEndian<BootBlock>(bbSector);

            if(bb.bbID != BB_MAGIC)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine("Boot Block:");

            if((bb.bbVersion & 0x8000) > 0)
            {
                sb.AppendLine("Boot block is in new format.");

                if((bb.bbVersion & 0x4000) > 0)
                {
                    sb.AppendLine("Boot block should be executed.");

                    if((bb.bbVersion & 0x2000) > 0)
                        sb.
                            AppendFormat("System heap will be extended by {0} bytes and a {1} fraction of the available RAM",
                                         bb.bbSysHeapExtra, bb.bbSysHeapFract).AppendLine();
                }
            }
            else if((bb.bbVersion & 0xFF) == 0x0D)
                sb.AppendLine("Boot block should be executed.");

            if(bb.bbPageFlags > 0)
                sb.AppendLine("Allocate secondary sound buffer at boot.");
            else if(bb.bbPageFlags < 0)
                sb.AppendLine("Allocate secondary sound and video buffers at boot.");

            sb.AppendFormat("System filename: {0}", StringHandlers.PascalToString(bb.bbSysName, encoding)).AppendLine();

            sb.AppendFormat("Finder filename: {0}", StringHandlers.PascalToString(bb.bbShellName, encoding)).
               AppendLine();

            sb.AppendFormat("Debugger filename: {0}", StringHandlers.PascalToString(bb.bbDbg1Name, encoding)).
               AppendLine();

            sb.AppendFormat("Disassembler filename: {0}", StringHandlers.PascalToString(bb.bbDbg2Name, encoding)).
               AppendLine();

            sb.AppendFormat("Startup screen filename: {0}", StringHandlers.PascalToString(bb.bbScreenName, encoding)).
               AppendLine();

            sb.AppendFormat("First program to execute at boot: {0}",
                            StringHandlers.PascalToString(bb.bbHelloName, encoding)).AppendLine();

            sb.AppendFormat("Clipboard filename: {0}", StringHandlers.PascalToString(bb.bbScrapName, encoding)).
               AppendLine();

            sb.AppendFormat("Maximum opened files: {0}", bb.bbCntFCBs * 4).AppendLine();
            sb.AppendFormat("Event queue size: {0}", bb.bbCntEvts).AppendLine();
            sb.AppendFormat("Heap size with 128KiB of RAM: {0} bytes", bb.bb128KSHeap).AppendLine();
            sb.AppendFormat("Heap size with 256KiB of RAM: {0} bytes", bb.bb256KSHeap).AppendLine();
            sb.AppendFormat("Heap size with 512KiB of RAM or more: {0} bytes", bb.bbSysHeapSize).AppendLine();

            return sb.ToString();
        }
    }
}