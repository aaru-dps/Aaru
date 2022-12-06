// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common Apple file systems.
//
// --[ Description ] ----------------------------------------------------------
//
//     Common Apple file systems structures.
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

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh
    // https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
    internal static partial class AppleCommon
    {
        /// <summary>Should be sectors 0 and 1 in volume, followed by boot code</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct BootBlock // Should be sectors 0 and 1 in volume
        {
            /// <summary>0x000, Signature, 0x4C4B if bootable</summary>
            public readonly ushort bbID;
            /// <summary>0x002, Branch</summary>
            public readonly uint bbEntry;
            /// <summary>0x007, Boot block version and flags</summary>
            public readonly ushort bbVersion;
            /// <summary>0x006, Boot block page flags</summary>
            public readonly short bbPageFlags;
            /// <summary>0x00A, System file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbSysName;
            /// <summary>0x01A, Finder file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbShellName;
            /// <summary>0x02A, Debugger file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbDbg1Name;
            /// <summary>0x03A, Disassembler file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbDbg2Name;
            /// <summary>0x04A, Startup screen file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbScreenName;
            /// <summary>0x05A, First program to execute on boot (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbHelloName;
            /// <summary>0x06A, Clipboard file name (16 bytes)</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] bbScrapName;
            /// <summary>0x07A, 1/4 of maximum opened at a time files</summary>
            public readonly ushort bbCntFCBs;
            /// <summary>0x07C, Event queue size</summary>
            public readonly ushort bbCntEvts;
            /// <summary>0x07E, Heap size on a Mac with 128KiB of RAM</summary>
            public readonly uint bb128KSHeap;
            /// <summary>0x082, Heap size on a Mac with 256KiB of RAM</summary>
            public readonly uint bb256KSHeap;
            /// <summary>0x086, Heap size on a Mac with 512KiB of RAM or more</summary>
            public readonly uint bbSysHeapSize;
            /// <summary>0x08A, Padding</summary>
            public readonly ushort filler;
            /// <summary>0x08C, Additional system heap space</summary>
            public readonly uint bbSysHeapExtra;
            /// <summary>0x090, Fraction of RAM for system heap</summary>
            public readonly uint bbSysHeapFract;
        }

        internal struct Rect
        {
            public ushort top;
            public ushort left;
            public ushort bottom;
            public ushort right;
        }

        internal struct Point
        {
            public ushort v;
            public ushort h;
        }

        internal struct FInfo
        {
            /// <summary>The type of the file.</summary>
            public uint fdType;
            /// <summary>The file's creator.</summary>
            public uint fdCreator;
            /// <summary>Flags.</summary>
            public FinderFlags fdFlags;
            /// <summary>File's location in the folder.</summary>
            public Point fdLocation;
            /// <summary>Folder file belongs to (used only in flat filesystems like MFS).</summary>
            public FinderFolder fdFldr;
        }

        internal struct FXInfo
        {
            /// <summary>Resource fork ID of file icon.</summary>
            public ushort fdIconID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] fdUnused;
            /// <summary>Extended flags. If high-bit is set, most significant byte is script code and least significant byte are flags.</summary>
            public ExtendedFinderFlags fdXFlags;
            /// <summary>Resource fork ID of directory comment if high bit is clear.</summary>
            public ushort fdComment;
            /// <summary>Put away folder ID.</summary>
            public uint fdPutAway;
        }

        internal struct DInfo
        {
            /// <summary>Position and dimensions of the folder's window.</summary>
            public Rect frRect;
            /// <summary>Flags.</summary>
            public FinderFlags frFlags;
            /// <summary>Folder's location in the parent folder.</summary>
            public Point frLocation;
            /// <summary>Finder view selected for folder.</summary>
            public ushort frView;
        }

        internal struct DXInfo
        {
            /// <summary>Scroll position for icon views.</summary>
            public Point frScroll;
            /// <summary>Directory ID chain of open folders.</summary>
            public uint frOpenChain;
            /// <summary>Extended flags. If high-bit is set, most significant byte is script code and least significant byte are flags.</summary>
            public ExtendedFinderFlags frXFlags;
            /// <summary>Resource fork ID of directory comment if high bit is clear.</summary>
            public ushort frComment;
            /// <summary>Put away folder ID.</summary>
            public uint frPutAway;
        }
    }
}