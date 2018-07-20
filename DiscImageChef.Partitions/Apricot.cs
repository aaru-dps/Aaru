// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Apricot.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages ACT Apricot partitions.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;

namespace DiscImageChef.Partitions
{
    public class Apricot : IPartition
    {
        readonly int[] baudRates = {50, 75, 110, 134, 150, 300, 600, 1200, 1800, 2400, 3600, 4800, 7200, 9600, 19200};
        readonly string[] bootTypeCodes =
        {
            "Non-bootable", "Apricot & XI RAM BIOS", "Generic ROM BIOS", "Apricot & XI ROM BIOS",
            "Apricot Portable ROM BIOS", "Apricot F1 ROM BIOS"
        };
        readonly string[] diskTypeCodes =
            {"MF1DD 70-track", "MF1DD", "MF2DD", "Winchester 5M", "Winchester 10M", "Winchester 20M"};
        readonly int[]    lineModes            = {256, 200};
        readonly int[]    lineWidths           = {80, 40};
        readonly string[] operatingSystemCodes = {"Invalid", "MS-DOS", "UCSD Pascal", "CP/M", "Concurrent CP/M"};
        readonly string[] parityTypes          = {"None", "Odd", "Even", "Mark", "Space"};
        readonly string[] printDevices         = {"Parallel", "Serial"};
        readonly double[] stopBits             = {1, 1.5, 2};

        public string Name => "ACT Apricot partitions";
        public Guid   Id   => new Guid("8CBF5864-7B5A-47A0-8CEB-199C74FA22DE");

        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            // I think Apricot can't chain partitions so.
            if(sectorOffset != 0) return false;

            byte[] sector = imagePlugin.ReadSector(0);

            if(sector.Length < 512) return false;

            IntPtr lblPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, lblPtr, 512);
            ApricotLabel label = (ApricotLabel)Marshal.PtrToStructure(lblPtr, typeof(ApricotLabel));
            Marshal.FreeHGlobal(lblPtr);

            // Not much to check but...
            ulong deviceSectors              = imagePlugin.Info.Sectors;
            ulong deviceSizeAccordingToLabel = label.cylinders * label.heads * label.spt;
            if(label.operatingSystem      > 4             || label.bootType       > 5 || label.partitionCount > 8 ||
               deviceSizeAccordingToLabel > deviceSectors || label.firstDataBlock > deviceSectors) return false;

            DicConsole.DebugWriteLine("Apricot partitions", "label.version = \"{0}\"",
                                      StringHandlers.CToString(label.version));
            DicConsole.DebugWriteLine("Apricot partitions", "label.operatingSystem = {0} ({1})", label.operatingSystem,
                                      label.operatingSystem < operatingSystemCodes.Length
                                          ? operatingSystemCodes[label.operatingSystem]
                                          : "Unknown");
            DicConsole.DebugWriteLine("Apricot partitions", "label.writeProtected = {0}", label.writeProtected);
            DicConsole.DebugWriteLine("Apricot partitions", "label.copyProtected = {0}",  label.copyProtected);
            DicConsole.DebugWriteLine("Apricot partitions", "label.bootType = {0} ({1})", label.bootType,
                                      label.bootType < bootTypeCodes.Length
                                          ? bootTypeCodes[label.bootType]
                                          : "Unknown");
            DicConsole.DebugWriteLine("Apricot partitions", "label.partitionCount = {0}",   label.partitionCount);
            DicConsole.DebugWriteLine("Apricot partitions", "label.winchester = {0}",       label.winchester);
            DicConsole.DebugWriteLine("Apricot partitions", "label.sectorSize = {0}",       label.sectorSize);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spt = {0}",              label.spt);
            DicConsole.DebugWriteLine("Apricot partitions", "label.cylinders = {0}",        label.cylinders);
            DicConsole.DebugWriteLine("Apricot partitions", "label.heads = {0}",            label.heads);
            DicConsole.DebugWriteLine("Apricot partitions", "label.interleave = {0}",       label.interleave);
            DicConsole.DebugWriteLine("Apricot partitions", "label.skew = {0}",             label.skew);
            DicConsole.DebugWriteLine("Apricot partitions", "label.bootLocation = {0}",     label.bootLocation);
            DicConsole.DebugWriteLine("Apricot partitions", "label.bootSize = {0}",         label.bootSize);
            DicConsole.DebugWriteLine("Apricot partitions", "label.bootAddress = 0x{0:X8}", label.bootAddress);
            DicConsole.DebugWriteLine("Apricot partitions", "label.bootOffset:label.bootSegment = {0:X4}:{1:X4}",
                                      label.bootOffset, label.bootSegment);
            DicConsole.DebugWriteLine("Apricot partitions", "label.firstDataBlock = {0}", label.firstDataBlock);
            DicConsole.DebugWriteLine("Apricot partitions", "label.generation = {0}",     label.generation);
            DicConsole.DebugWriteLine("Apricot partitions", "label.copyCount = {0}",      label.copyCount);
            DicConsole.DebugWriteLine("Apricot partitions", "label.maxCopies = {0}",      label.maxCopies);
            DicConsole.DebugWriteLine("Apricot partitions", "label.serialNumber = \"{0}\"",
                                      StringHandlers.CToString(label.serialNumber));
            DicConsole.DebugWriteLine("Apricot partitions", "label.partNumber = \"{0}\"",
                                      StringHandlers.CToString(label.partNumber));
            DicConsole.DebugWriteLine("Apricot partitions", "label.copyright = \"{0}\"",
                                      StringHandlers.CToString(label.copyright));
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.bps = {0}",      label.mainBPB.bps);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.spc = {0}",      label.mainBPB.spc);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.rsectors = {0}", label.mainBPB.rsectors);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.fats_no = {0}",  label.mainBPB.fats_no);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.root_ent = {0}", label.mainBPB.root_ent);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.sectors = {0}",  label.mainBPB.sectors);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.media = {0}",    label.mainBPB.media);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.spfat = {0}",    label.mainBPB.spfat);
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.diskType = {0} ({1})",
                                      label.mainBPB.diskType,
                                      label.mainBPB.diskType < diskTypeCodes.Length
                                          ? diskTypeCodes[label.mainBPB.diskType]
                                          : "Unknown");
            DicConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.startSector = {0}",
                                      label.mainBPB.startSector);
            DicConsole.DebugWriteLine("Apricot partitions", "label.fontName = \"{0}\"",
                                      StringHandlers.CToString(label.fontName));
            DicConsole.DebugWriteLine("Apricot partitions", "label.keyboardName = \"{0}\"",
                                      StringHandlers.CToString(label.keyboardName));
            DicConsole.DebugWriteLine("Apricot partitions", "label.biosMajorVersion = {0}", label.biosMajorVersion);
            DicConsole.DebugWriteLine("Apricot partitions", "label.biosMinorVersion = {0}", label.biosMinorVersion);
            DicConsole.DebugWriteLine("Apricot partitions", "label.diagnosticsFlag = {0}",  label.diagnosticsFlag);
            DicConsole.DebugWriteLine("Apricot partitions", "label.prnDevice = {0} ({1})", label.prnDevice,
                                      label.prnDevice < printDevices.Length
                                          ? printDevices[label.prnDevice]
                                          : "Unknown");
            DicConsole.DebugWriteLine("Apricot partitions", "label.bellVolume = {0}",         label.bellVolume);
            DicConsole.DebugWriteLine("Apricot partitions", "label.enableCache = {0}",        label.enableCache);
            DicConsole.DebugWriteLine("Apricot partitions", "label.enableGraphics = {0}",     label.enableGraphics);
            DicConsole.DebugWriteLine("Apricot partitions", "label.dosLength = {0}",          label.dosLength);
            DicConsole.DebugWriteLine("Apricot partitions", "label.fontLength = {0}",         label.fontLength);
            DicConsole.DebugWriteLine("Apricot partitions", "label.keyboardLength = {0}",     label.keyboardLength);
            DicConsole.DebugWriteLine("Apricot partitions", "label.dosStart = {0}",           label.dosStart);
            DicConsole.DebugWriteLine("Apricot partitions", "label.fontStart = {0}",          label.fontStart);
            DicConsole.DebugWriteLine("Apricot partitions", "label.keyboardStart = {0}",      label.keyboardStart);
            DicConsole.DebugWriteLine("Apricot partitions", "label.keyboardVolume = {0}",     label.keyboardVolume);
            DicConsole.DebugWriteLine("Apricot partitions", "label.autorepeat = {0}",         label.autorepeat);
            DicConsole.DebugWriteLine("Apricot partitions", "label.autorepeatLeadIn = {0}",   label.autorepeatLeadIn);
            DicConsole.DebugWriteLine("Apricot partitions", "label.autorepeatInterval = {0}", label.autorepeatInterval);
            DicConsole.DebugWriteLine("Apricot partitions", "label.microscreenMode = {0}",    label.microscreenMode);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareKeyboard is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareKeyboard));
            DicConsole.DebugWriteLine("Apricot partitions", "label.lineMode = {0} ({1} lines)", label.lineMode,
                                      label.lineMode < lineModes.Length ? lineModes[label.lineMode] : 0);
            DicConsole.DebugWriteLine("Apricot partitions", "label.lineWidth = {0} ({1} columns)", label.lineWidth,
                                      label.lineWidth < lineWidths.Length ? lineWidths[label.lineWidth] : 0);
            DicConsole.DebugWriteLine("Apricot partitions", "label.imageOff = {0}", label.imageOff);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareScreen is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareScreen));
            DicConsole.DebugWriteLine("Apricot partitions", "label.txBaudRate = {0} ({1} bps)", label.txBaudRate,
                                      label.txBaudRate < baudRates.Length ? baudRates[label.txBaudRate] : 0);
            DicConsole.DebugWriteLine("Apricot partitions", "label.rxBaudRate = {0} ({1} bps)", label.rxBaudRate,
                                      label.rxBaudRate < baudRates.Length ? baudRates[label.rxBaudRate] : 0);
            DicConsole.DebugWriteLine("Apricot partitions", "label.txBits = {0}", label.txBits);
            DicConsole.DebugWriteLine("Apricot partitions", "label.rxBits = {0}", label.rxBits);
            DicConsole.DebugWriteLine("Apricot partitions", "label.stopBits = {0} ({1} bits)", label.stopBits,
                                      label.stopBits < stopBits.Length ? stopBits[label.stopBits] : 0);
            DicConsole.DebugWriteLine("Apricot partitions", "label.parityCheck = {0}", label.parityCheck);
            DicConsole.DebugWriteLine("Apricot partitions", "label.parityType = {0} ({1})", label.parityType,
                                      label.parityType < parityTypes.Length
                                          ? parityTypes[label.parityType]
                                          : "Unknown");
            DicConsole.DebugWriteLine("Apricot partitions", "label.txXonXoff = {0}",       label.txXonXoff);
            DicConsole.DebugWriteLine("Apricot partitions", "label.rxXonXoff = {0}",       label.rxXonXoff);
            DicConsole.DebugWriteLine("Apricot partitions", "label.xonCharacter = {0}",    label.xonCharacter);
            DicConsole.DebugWriteLine("Apricot partitions", "label.xoffCharacter = {0}",   label.xoffCharacter);
            DicConsole.DebugWriteLine("Apricot partitions", "label.rxXonXoffBuffer = {0}", label.rxXonXoffBuffer);
            DicConsole.DebugWriteLine("Apricot partitions", "label.dtrDsr = {0}",          label.dtrDsr);
            DicConsole.DebugWriteLine("Apricot partitions", "label.ctsRts = {0}",          label.ctsRts);
            DicConsole.DebugWriteLine("Apricot partitions", "label.nullsAfterCr = {0}",    label.nullsAfterCr);
            DicConsole.DebugWriteLine("Apricot partitions", "label.nullsAfterFF = {0}",    label.nullsAfterFF);
            DicConsole.DebugWriteLine("Apricot partitions", "label.lfAfterCRSerial = {0}", label.lfAfterCRSerial);
            DicConsole.DebugWriteLine("Apricot partitions", "label.biosErrorReportSerial = {0}",
                                      label.biosErrorReportSerial);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareSerial is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareSerial));
            DicConsole.DebugWriteLine("Apricot partitions", "label.lfAfterCrParallel = {0}", label.lfAfterCrParallel);
            DicConsole.DebugWriteLine("Apricot partitions", "label.selectLine = {0}",        label.selectLine);
            DicConsole.DebugWriteLine("Apricot partitions", "label.paperEmpty = {0}",        label.paperEmpty);
            DicConsole.DebugWriteLine("Apricot partitions", "label.faultLine = {0}",         label.faultLine);
            DicConsole.DebugWriteLine("Apricot partitions", "label.biosErrorReportParallel = {0}",
                                      label.biosErrorReportParallel);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareParallel is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareParallel));
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareWinchester is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareWinchester));
            DicConsole.DebugWriteLine("Apricot partitions", "label.parkingEnabled = {0}",   label.parkingEnabled);
            DicConsole.DebugWriteLine("Apricot partitions", "label.formatProtection = {0}", label.formatProtection);
            DicConsole.DebugWriteLine("Apricot partitions", "label.spareRamDisk is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spareRamDisk));
            for(int i = 0; i < 32; i++)
                DicConsole.DebugWriteLine("Apricot partitions", "label.badBlocks[{1}] = {0}", label.badBlocks[i], i);
            for(int i = 0; i < 8; i++)
            {
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].bps = {0}",
                                          label.partitions[i].bps, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].spc = {0}",
                                          label.partitions[i].spc, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].rsectors = {0}",
                                          label.partitions[i].rsectors, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].fats_no = {0}",
                                          label.partitions[i].fats_no, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].root_ent = {0}",
                                          label.partitions[i].root_ent, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].sectors = {0}",
                                          label.partitions[i].sectors, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].media = {0}",
                                          label.partitions[i].media, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].spfat = {0}",
                                          label.partitions[i].spfat, i);
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].diskType = {0} ({2})",
                                          label.partitions[i].diskType, i,
                                          label.partitions[i].diskType < diskTypeCodes.Length
                                              ? diskTypeCodes[label.partitions[i].diskType]
                                              : "Unknown");
                DicConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].startSector = {0}",
                                          label.partitions[i].startSector, i);
            }

            DicConsole.DebugWriteLine("Apricot partitions", "label.spare is null? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(label.spare));
            DicConsole.DebugWriteLine("Apricot partitions", "label.cpmDoubleSided = {0}", label.cpmDoubleSided);

            // Only hard disks can contain partitions
            if(!label.winchester) return false;

            for(byte i = 0; i < label.partitionCount; i++)
            {
                Partition part = new Partition
                {
                    Start    = label.partitions[i].startSector,
                    Size     = (ulong)(label.partitions[i].sectors * label.sectorSize),
                    Length   = label.partitions[i].sectors,
                    Type     = "ACT Apricot partition",
                    Sequence = i,
                    Scheme   = Name,
                    Offset   = (ulong)(label.partitions[i].startSector * label.sectorSize)
                };
                if(part.Start < deviceSectors && part.End < deviceSectors) partitions.Add(part);
            }

            return partitions.Count > 0;
        }

        /// <summary>Apricot Label.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApricotLabel
        {
            /// <summary>Version of format which created disk</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] version;
            /// <summary>Operating system.</summary>
            public byte operatingSystem;
            /// <summary>Software write protection.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool writeProtected;
            /// <summary>Copy protected.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool copyProtected;
            /// <summary>Boot type.</summary>
            public byte bootType;
            /// <summary>Partitions.</summary>
            public byte partitionCount;
            /// <summary>Is hard disk?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool winchester;
            /// <summary>Sector size.</summary>
            public ushort sectorSize;
            /// <summary>Sectors per track.</summary>
            public ushort spt;
            /// <summary>Tracks per side.</summary>
            public uint cylinders;
            /// <summary>Sides.</summary>
            public byte heads;
            /// <summary>Interleave factor.</summary>
            public byte interleave;
            /// <summary>Skew factor.</summary>
            public ushort skew;
            /// <summary>Sector where boot code starts.</summary>
            public uint bootLocation;
            /// <summary>Size in sectors of boot code.</summary>
            public ushort bootSize;
            /// <summary>Address at which to load boot code.</summary>
            public uint bootAddress;
            /// <summary>Offset where to jump to boot.</summary>
            public ushort bootOffset;
            /// <summary>Segment where to jump to boot.</summary>
            public ushort bootSegment;
            /// <summary>First data sector.</summary>
            public uint firstDataBlock;
            /// <summary>Generation.</summary>
            public ushort generation;
            /// <summary>Copy count.</summary>
            public ushort copyCount;
            /// <summary>Maximum number of copies.</summary>
            public ushort maxCopies;
            /// <summary>Serial number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] serialNumber;
            /// <summary>Part number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] partNumber;
            /// <summary>Copyright.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] copyright;
            /// <summary>BPB for whole disk.</summary>
            public ApricotParameterBlock mainBPB;
            /// <summary>Name of FONT file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] fontName;
            /// <summary>Name of KEYBOARD file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] keyboardName;
            /// <summary>Minor BIOS version.</summary>
            public byte biosMinorVersion;
            /// <summary>Major BIOS version.</summary>
            public byte biosMajorVersion;
            /// <summary>Diagnostics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool diagnosticsFlag;
            /// <summary>Printer device.</summary>
            public byte prnDevice;
            /// <summary>Bell volume.</summary>
            public byte bellVolume;
            /// <summary>Cache enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool enableCache;
            /// <summary>Graphics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool enableGraphics;
            /// <summary>Length in sectors of DOS.</summary>
            public byte dosLength;
            /// <summary>Length in sectors of FONT file.</summary>
            public byte fontLength;
            /// <summary>Length in sectors of KEYBOARD file.</summary>
            public byte keyboardLength;
            /// <summary>Starting sector of DOS.</summary>
            public ushort dosStart;
            /// <summary>Starting sector of FONT file.</summary>
            public ushort fontStart;
            /// <summary>Starting sector of KEYBOARD file.</summary>
            public ushort keyboardStart;
            /// <summary>Keyboard click volume.</summary>
            public byte keyboardVolume;
            /// <summary>Auto-repeat enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool autorepeat;
            /// <summary>Auto-repeat lead-in.</summary>
            public byte autorepeatLeadIn;
            /// <summary>Auto-repeat interval.</summary>
            public byte autorepeatInterval;
            /// <summary>Microscreen mode.</summary>
            public byte microscreenMode;
            /// <summary>Spare area for keyboard values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] spareKeyboard;
            /// <summary>Screen line mode.</summary>
            public byte lineMode;
            /// <summary>Screen line width.</summary>
            public byte lineWidth;
            /// <summary>Screen disabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool imageOff;
            /// <summary>Spare area for screen values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] spareScreen;
            /// <summary>TX baud rate.</summary>
            public byte txBaudRate;
            /// <summary>RX baud rate.</summary>
            public byte rxBaudRate;
            /// <summary>TX bits.</summary>
            public byte txBits;
            /// <summary>RX bits.</summary>
            public byte rxBits;
            /// <summary>Stop bits.</summary>
            public byte stopBits;
            /// <summary>Parity enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool parityCheck;
            /// <summary>Parity type.</summary>
            public byte parityType;
            /// <summary>Xon/Xoff enabled on TX.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool txXonXoff;
            /// <summary>Xon/Xoff enabled on RX.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool rxXonXoff;
            /// <summary>Xon character.</summary>
            public byte xonCharacter;
            /// <summary>Xoff character.</summary>
            public byte xoffCharacter;
            /// <summary>Xon/Xoff buffer on RX.</summary>
            public ushort rxXonXoffBuffer;
            /// <summary>DTR/DSR enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool dtrDsr;
            /// <summary>CTS/RTS enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool ctsRts;
            /// <summary>NULLs after CR.</summary>
            public byte nullsAfterCr;
            /// <summary>NULLs after 0xFF.</summary>
            public byte nullsAfterFF;
            /// <summary>Send LF after CR in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool lfAfterCRSerial;
            /// <summary>BIOS error report in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool biosErrorReportSerial;
            /// <summary>Spare area for serial port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] spareSerial;
            /// <summary>Send LF after CR in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool lfAfterCrParallel;
            /// <summary>Select line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool selectLine;
            /// <summary>Paper empty supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool paperEmpty;
            /// <summary>Fault line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool faultLine;
            /// <summary>BIOS error report in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool biosErrorReportParallel;
            /// <summary>Spare area for parallel port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] spareParallel;
            /// <summary>Spare area for Winchester values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public byte[] spareWinchester;
            /// <summary>Parking enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool parkingEnabled;
            /// <summary>Format protection?.</summary>
            [MarshalAs(UnmanagedType.U1)] public bool formatProtection;
            /// <summary>Spare area for RAM disk values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] spareRamDisk;
            /// <summary>List of bad blocks.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public ushort[] badBlocks;
            /// <summary>Array of partition BPBs.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public ApricotParameterBlock[] partitions;
            /// <summary>Spare area.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
            public byte[] spare;
            /// <summary>CP/M double side indicator?.</summary>
            public bool cpmDoubleSided;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ApricotParameterBlock
        {
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors per cluster</summary>
            public byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public ushort rsectors;
            /// <summary>Number of FATs</summary>
            public byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public ushort sectors;
            /// <summary>Media descriptor</summary>
            public byte media;
            /// <summary>Sectors per FAT</summary>
            public ushort spfat;
            /// <summary>Disk type</summary>
            public byte diskType;
            /// <summary>Volume starting sector</summary>
            public ushort startSector;
        }
    }
}