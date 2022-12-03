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

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
static partial class AppleCommon
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
        sb.AppendLine(Localization.Boot_Block);

        if((bb.bbVersion & 0x8000) > 0)
        {
            sb.AppendLine(Localization.Boot_block_is_in_new_format);

            if((bb.bbVersion & 0x4000) > 0)
            {
                sb.AppendLine(Localization.Boot_block_should_be_executed);

                if((bb.bbVersion & 0x2000) > 0)
                    sb.
                        AppendFormat(Localization.System_heap_will_be_extended_by_0_bytes_and_a_1_fraction_of_the_available_RAM,
                                     bb.bbSysHeapExtra, bb.bbSysHeapFract).AppendLine();
            }
        }
        else if((bb.bbVersion & 0xFF) == 0x0D)
            sb.AppendLine(Localization.Boot_block_should_be_executed);

        switch(bb.bbPageFlags)
        {
            case > 0:
                sb.AppendLine(Localization.Allocate_secondary_sound_buffer_at_boot);

                break;
            case < 0:
                sb.AppendLine(Localization.Allocate_secondary_sound_and_video_buffers_at_boot);

                break;
        }

        sb.AppendFormat(Localization.System_filename_0, StringHandlers.PascalToString(bb.bbSysName, encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Finder_filename_0, StringHandlers.PascalToString(bb.bbShellName, encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Debugger_filename_0, StringHandlers.PascalToString(bb.bbDbg1Name, encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Disassembler_filename_0, StringHandlers.PascalToString(bb.bbDbg2Name, encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Startup_screen_filename_0,
                        StringHandlers.PascalToString(bb.bbScreenName, encoding)).AppendLine();

        sb.AppendFormat(Localization.First_program_to_execute_at_boot_0,
                        StringHandlers.PascalToString(bb.bbHelloName, encoding)).AppendLine();

        sb.AppendFormat(Localization.Clipboard_filename_0, StringHandlers.PascalToString(bb.bbScrapName, encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Maximum_opened_files_0, bb.bbCntFCBs * 4).AppendLine();
        sb.AppendFormat(Localization.Event_queue_size_0, bb.bbCntEvts).AppendLine();
        sb.AppendFormat(Localization.Heap_size_with_128KiB_of_RAM_0_bytes, bb.bb128KSHeap).AppendLine();
        sb.AppendFormat(Localization.Heap_size_with_256KiB_of_RAM_0_bytes, bb.bb256KSHeap).AppendLine();
        sb.AppendFormat(Localization.Heap_size_with_512KiB_of_RAM_or_more_0_bytes, bb.bbSysHeapSize).AppendLine();

        return sb.ToString();
    }
}