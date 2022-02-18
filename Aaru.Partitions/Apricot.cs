// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions
{
    /// <inheritdoc />
    /// <summary>Implements decoding of Apricot partitions</summary>
    public sealed class Apricot : IPartition
    {
        readonly int[] _baudRates =
        {
            50, 75, 110, 134, 150, 300, 600, 1200, 1800, 2400, 3600, 4800, 7200, 9600, 19200
        };
        readonly string[] _bootTypeCodes =
        {
            "Non-bootable", "Apricot & XI RAM BIOS", "Generic ROM BIOS", "Apricot & XI ROM BIOS",
            "Apricot Portable ROM BIOS", "Apricot F1 ROM BIOS"
        };
        readonly string[] _diskTypeCodes =
        {
            "MF1DD 70-track", "MF1DD", "MF2DD", "Winchester 5M", "Winchester 10M", "Winchester 20M"
        };
        readonly int[] _lineModes =
        {
            256, 200
        };
        readonly int[] _lineWidths =
        {
            80, 40
        };
        readonly string[] _operatingSystemCodes =
        {
            "Invalid", "MS-DOS", "UCSD Pascal", "CP/M", "Concurrent CP/M"
        };
        readonly string[] _parityTypes =
        {
            "None", "Odd", "Even", "Mark", "Space"
        };
        readonly string[] _printDevices =
        {
            "Parallel", "Serial"
        };
        readonly double[] _stopBits =
        {
            1, 1.5, 2
        };

        /// <inheritdoc />
        public string Name => "ACT Apricot partitions";
        /// <inheritdoc />
        public Guid Id => new("8CBF5864-7B5A-47A0-8CEB-199C74FA22DE");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            // I think Apricot can't chain partitions so.
            if(sectorOffset != 0)
                return false;

            ErrorNumber errno = imagePlugin.ReadSector(0, out byte[] sector);

            if(errno         != ErrorNumber.NoError ||
               sector.Length < 512)
                return false;

            Label label = Marshal.ByteArrayToStructureLittleEndian<Label>(sector);

            // Not much to check but...
            ulong deviceSectors              = imagePlugin.Info.Sectors;
            ulong deviceSizeAccordingToLabel = label.cylinders * label.heads * label.spt;

            if(label.operatingSystem      > 4             ||
               label.bootType             > 5             ||
               label.partitionCount       > 8             ||
               deviceSizeAccordingToLabel > deviceSectors ||
               label.firstDataBlock       > deviceSectors)
                return false;

            AaruConsole.DebugWriteLine("Apricot partitions", "label.version = \"{0}\"",
                                       StringHandlers.CToString(label.version));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.operatingSystem = {0} ({1})", label.operatingSystem,
                                       label.operatingSystem < _operatingSystemCodes.Length
                                           ? _operatingSystemCodes[label.operatingSystem] : "Unknown");

            AaruConsole.DebugWriteLine("Apricot partitions", "label.writeProtected = {0}", label.writeProtected);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.copyProtected = {0}", label.copyProtected);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.bootType = {0} ({1})", label.bootType,
                                       label.bootType < _bootTypeCodes.Length ? _bootTypeCodes[label.bootType]
                                           : "Unknown");

            AaruConsole.DebugWriteLine("Apricot partitions", "label.partitionCount = {0}", label.partitionCount);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.winchester = {0}", label.winchester);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.sectorSize = {0}", label.sectorSize);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.spt = {0}", label.spt);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.cylinders = {0}", label.cylinders);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.heads = {0}", label.heads);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.interleave = {0}", label.interleave);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.skew = {0}", label.skew);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.bootLocation = {0}", label.bootLocation);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.bootSize = {0}", label.bootSize);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.bootAddress = 0x{0:X8}", label.bootAddress);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.bootOffset:label.bootSegment = {0:X4}:{1:X4}",
                                       label.bootOffset, label.bootSegment);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.firstDataBlock = {0}", label.firstDataBlock);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.generation = {0}", label.generation);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.copyCount = {0}", label.copyCount);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.maxCopies = {0}", label.maxCopies);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.serialNumber = \"{0}\"",
                                       StringHandlers.CToString(label.serialNumber));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.partNumber = \"{0}\"",
                                       StringHandlers.CToString(label.partNumber));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.copyright = \"{0}\"",
                                       StringHandlers.CToString(label.copyright));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.bps = {0}", label.mainBPB.bps);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.spc = {0}", label.mainBPB.spc);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.rsectors = {0}", label.mainBPB.rsectors);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.fats_no = {0}", label.mainBPB.fats_no);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.root_ent = {0}", label.mainBPB.root_ent);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.sectors = {0}", label.mainBPB.sectors);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.media = {0}", label.mainBPB.media);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.spfat = {0}", label.mainBPB.spfat);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.diskType = {0} ({1})",
                                       label.mainBPB.diskType,
                                       label.mainBPB.diskType < _diskTypeCodes.Length
                                           ? _diskTypeCodes[label.mainBPB.diskType] : "Unknown");

            AaruConsole.DebugWriteLine("Apricot partitions", "label.mainBPB.startSector = {0}",
                                       label.mainBPB.startSector);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.fontName = \"{0}\"",
                                       StringHandlers.CToString(label.fontName));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.keyboardName = \"{0}\"",
                                       StringHandlers.CToString(label.keyboardName));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.biosMajorVersion = {0}", label.biosMajorVersion);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.biosMinorVersion = {0}", label.biosMinorVersion);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.diagnosticsFlag = {0}", label.diagnosticsFlag);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.prnDevice = {0} ({1})", label.prnDevice,
                                       label.prnDevice < _printDevices.Length ? _printDevices[label.prnDevice]
                                           : "Unknown");

            AaruConsole.DebugWriteLine("Apricot partitions", "label.bellVolume = {0}", label.bellVolume);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.enableCache = {0}", label.enableCache);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.enableGraphics = {0}", label.enableGraphics);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.dosLength = {0}", label.dosLength);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.fontLength = {0}", label.fontLength);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.keyboardLength = {0}", label.keyboardLength);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.dosStart = {0}", label.dosStart);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.fontStart = {0}", label.fontStart);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.keyboardStart = {0}", label.keyboardStart);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.keyboardVolume = {0}", label.keyboardVolume);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.autorepeat = {0}", label.autorepeat);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.autorepeatLeadIn = {0}", label.autorepeatLeadIn);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.autorepeatInterval = {0}",
                                       label.autorepeatInterval);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.microscreenMode = {0}", label.microscreenMode);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareKeyboard is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareKeyboard));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.lineMode = {0} ({1} lines)", label.lineMode,
                                       label.lineMode < _lineModes.Length ? _lineModes[label.lineMode] : 0);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.lineWidth = {0} ({1} columns)", label.lineWidth,
                                       label.lineWidth < _lineWidths.Length ? _lineWidths[label.lineWidth] : 0);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.imageOff = {0}", label.imageOff);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareScreen is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareScreen));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.txBaudRate = {0} ({1} bps)", label.txBaudRate,
                                       label.txBaudRate < _baudRates.Length ? _baudRates[label.txBaudRate] : 0);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.rxBaudRate = {0} ({1} bps)", label.rxBaudRate,
                                       label.rxBaudRate < _baudRates.Length ? _baudRates[label.rxBaudRate] : 0);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.txBits = {0}", label.txBits);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.rxBits = {0}", label.rxBits);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.stopBits = {0} ({1} bits)", label.stopBits,
                                       label.stopBits < _stopBits.Length ? _stopBits[label.stopBits] : 0);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.parityCheck = {0}", label.parityCheck);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.parityType = {0} ({1})", label.parityType,
                                       label.parityType < _parityTypes.Length ? _parityTypes[label.parityType]
                                           : "Unknown");

            AaruConsole.DebugWriteLine("Apricot partitions", "label.txXonXoff = {0}", label.txXonXoff);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.rxXonXoff = {0}", label.rxXonXoff);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.xonCharacter = {0}", label.xonCharacter);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.xoffCharacter = {0}", label.xoffCharacter);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.rxXonXoffBuffer = {0}", label.rxXonXoffBuffer);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.dtrDsr = {0}", label.dtrDsr);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.ctsRts = {0}", label.ctsRts);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.nullsAfterCr = {0}", label.nullsAfterCr);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.nullsAfterFF = {0}", label.nullsAfterFF);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.lfAfterCRSerial = {0}", label.lfAfterCRSerial);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.biosErrorReportSerial = {0}",
                                       label.biosErrorReportSerial);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareSerial is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareSerial));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.lfAfterCrParallel = {0}", label.lfAfterCrParallel);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.selectLine = {0}", label.selectLine);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.paperEmpty = {0}", label.paperEmpty);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.faultLine = {0}", label.faultLine);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.biosErrorReportParallel = {0}",
                                       label.biosErrorReportParallel);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareParallel is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareParallel));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareWinchester is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareWinchester));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.parkingEnabled = {0}", label.parkingEnabled);
            AaruConsole.DebugWriteLine("Apricot partitions", "label.formatProtection = {0}", label.formatProtection);

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spareRamDisk is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spareRamDisk));

            for(int i = 0; i < 32; i++)
                AaruConsole.DebugWriteLine("Apricot partitions", "label.badBlocks[{1}] = {0}", label.badBlocks[i], i);

            for(int i = 0; i < 8; i++)
            {
                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].bps = {0}",
                                           label.partitions[i].bps, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].spc = {0}",
                                           label.partitions[i].spc, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].rsectors = {0}",
                                           label.partitions[i].rsectors, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].fats_no = {0}",
                                           label.partitions[i].fats_no, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].root_ent = {0}",
                                           label.partitions[i].root_ent, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].sectors = {0}",
                                           label.partitions[i].sectors, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].media = {0}",
                                           label.partitions[i].media, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].spfat = {0}",
                                           label.partitions[i].spfat, i);

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].diskType = {0} ({2})",
                                           label.partitions[i].diskType, i,
                                           label.partitions[i].diskType < _diskTypeCodes.Length
                                               ? _diskTypeCodes[label.partitions[i].diskType] : "Unknown");

                AaruConsole.DebugWriteLine("Apricot partitions", "label.partitions[{1}].startSector = {0}",
                                           label.partitions[i].startSector, i);
            }

            AaruConsole.DebugWriteLine("Apricot partitions", "label.spare is null? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(label.spare));

            AaruConsole.DebugWriteLine("Apricot partitions", "label.cpmDoubleSided = {0}", label.cpmDoubleSided);

            // Only hard disks can contain partitions
            if(!label.winchester)
                return false;

            for(byte i = 0; i < label.partitionCount; i++)
            {
                var part = new Partition
                {
                    Start    = label.partitions[i].startSector,
                    Size     = (ulong)(label.partitions[i].sectors * label.sectorSize),
                    Length   = label.partitions[i].sectors,
                    Type     = "ACT Apricot partition",
                    Sequence = i,
                    Scheme   = Name,
                    Offset   = (ulong)(label.partitions[i].startSector * label.sectorSize)
                };

                if(part.Start < deviceSectors &&
                   part.End   < deviceSectors)
                    partitions.Add(part);
            }

            return partitions.Count > 0;
        }

        /// <summary>Apricot Label.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Label
        {
            /// <summary>Version of format which created disk</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] version;
            /// <summary>Operating system.</summary>
            public readonly byte operatingSystem;
            /// <summary>Software write protection.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool writeProtected;
            /// <summary>Copy protected.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool copyProtected;
            /// <summary>Boot type.</summary>
            public readonly byte bootType;
            /// <summary>Partitions.</summary>
            public readonly byte partitionCount;
            /// <summary>Is hard disk?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool winchester;
            /// <summary>Sector size.</summary>
            public readonly ushort sectorSize;
            /// <summary>Sectors per track.</summary>
            public readonly ushort spt;
            /// <summary>Tracks per side.</summary>
            public readonly uint cylinders;
            /// <summary>Sides.</summary>
            public readonly byte heads;
            /// <summary>Interleave factor.</summary>
            public readonly byte interleave;
            /// <summary>Skew factor.</summary>
            public readonly ushort skew;
            /// <summary>Sector where boot code starts.</summary>
            public readonly uint bootLocation;
            /// <summary>Size in sectors of boot code.</summary>
            public readonly ushort bootSize;
            /// <summary>Address at which to load boot code.</summary>
            public readonly uint bootAddress;
            /// <summary>Offset where to jump to boot.</summary>
            public readonly ushort bootOffset;
            /// <summary>Segment where to jump to boot.</summary>
            public readonly ushort bootSegment;
            /// <summary>First data sector.</summary>
            public readonly uint firstDataBlock;
            /// <summary>Generation.</summary>
            public readonly ushort generation;
            /// <summary>Copy count.</summary>
            public readonly ushort copyCount;
            /// <summary>Maximum number of copies.</summary>
            public readonly ushort maxCopies;
            /// <summary>Serial number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] serialNumber;
            /// <summary>Part number.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] partNumber;
            /// <summary>Copyright.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public readonly byte[] copyright;
            /// <summary>BPB for whole disk.</summary>
            public readonly ParameterBlock mainBPB;
            /// <summary>Name of FONT file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] fontName;
            /// <summary>Name of KEYBOARD file.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] keyboardName;
            /// <summary>Minor BIOS version.</summary>
            public readonly byte biosMinorVersion;
            /// <summary>Major BIOS version.</summary>
            public readonly byte biosMajorVersion;
            /// <summary>Diagnostics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool diagnosticsFlag;
            /// <summary>Printer device.</summary>
            public readonly byte prnDevice;
            /// <summary>Bell volume.</summary>
            public readonly byte bellVolume;
            /// <summary>Cache enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool enableCache;
            /// <summary>Graphics enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool enableGraphics;
            /// <summary>Length in sectors of DOS.</summary>
            public readonly byte dosLength;
            /// <summary>Length in sectors of FONT file.</summary>
            public readonly byte fontLength;
            /// <summary>Length in sectors of KEYBOARD file.</summary>
            public readonly byte keyboardLength;
            /// <summary>Starting sector of DOS.</summary>
            public readonly ushort dosStart;
            /// <summary>Starting sector of FONT file.</summary>
            public readonly ushort fontStart;
            /// <summary>Starting sector of KEYBOARD file.</summary>
            public readonly ushort keyboardStart;
            /// <summary>Keyboard click volume.</summary>
            public readonly byte keyboardVolume;
            /// <summary>Auto-repeat enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool autorepeat;
            /// <summary>Auto-repeat lead-in.</summary>
            public readonly byte autorepeatLeadIn;
            /// <summary>Auto-repeat interval.</summary>
            public readonly byte autorepeatInterval;
            /// <summary>Microscreen mode.</summary>
            public readonly byte microscreenMode;
            /// <summary>Spare area for keyboard values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public readonly byte[] spareKeyboard;
            /// <summary>Screen line mode.</summary>
            public readonly byte lineMode;
            /// <summary>Screen line width.</summary>
            public readonly byte lineWidth;
            /// <summary>Screen disabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool imageOff;
            /// <summary>Spare area for screen values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public readonly byte[] spareScreen;
            /// <summary>TX baud rate.</summary>
            public readonly byte txBaudRate;
            /// <summary>RX baud rate.</summary>
            public readonly byte rxBaudRate;
            /// <summary>TX bits.</summary>
            public readonly byte txBits;
            /// <summary>RX bits.</summary>
            public readonly byte rxBits;
            /// <summary>Stop bits.</summary>
            public readonly byte stopBits;
            /// <summary>Parity enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool parityCheck;
            /// <summary>Parity type.</summary>
            public readonly byte parityType;
            /// <summary>Xon/Xoff enabled on TX.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool txXonXoff;
            /// <summary>Xon/Xoff enabled on RX.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool rxXonXoff;
            /// <summary>Xon character.</summary>
            public readonly byte xonCharacter;
            /// <summary>Xoff character.</summary>
            public readonly byte xoffCharacter;
            /// <summary>Xon/Xoff buffer on RX.</summary>
            public readonly ushort rxXonXoffBuffer;
            /// <summary>DTR/DSR enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool dtrDsr;
            /// <summary>CTS/RTS enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool ctsRts;
            /// <summary>NULLs after CR.</summary>
            public readonly byte nullsAfterCr;
            /// <summary>NULLs after 0xFF.</summary>
            public readonly byte nullsAfterFF;
            /// <summary>Send LF after CR in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool lfAfterCRSerial;
            /// <summary>BIOS error report in serial port.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool biosErrorReportSerial;
            /// <summary>Spare area for serial port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public readonly byte[] spareSerial;
            /// <summary>Send LF after CR in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool lfAfterCrParallel;
            /// <summary>Select line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool selectLine;
            /// <summary>Paper empty supported?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool paperEmpty;
            /// <summary>Fault line supported?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool faultLine;
            /// <summary>BIOS error report in parallel port.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool biosErrorReportParallel;
            /// <summary>Spare area for parallel port values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public readonly byte[] spareParallel;
            /// <summary>Spare area for Winchester values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public readonly byte[] spareWinchester;
            /// <summary>Parking enabled?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool parkingEnabled;
            /// <summary>Format protection?.</summary>
            [MarshalAs(UnmanagedType.U1)]
            public readonly bool formatProtection;
            /// <summary>Spare area for RAM disk values expansion.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] spareRamDisk;
            /// <summary>List of bad blocks.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly ushort[] badBlocks;
            /// <summary>Array of partition BPBs.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly ParameterBlock[] partitions;
            /// <summary>Spare area.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
            public readonly byte[] spare;
            /// <summary>CP/M double side indicator?.</summary>
            public readonly bool cpmDoubleSided;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ParameterBlock
        {
            /// <summary>Bytes per sector</summary>
            public readonly ushort bps;
            /// <summary>Sectors per cluster</summary>
            public readonly byte spc;
            /// <summary>Reserved sectors between BPB and FAT</summary>
            public readonly ushort rsectors;
            /// <summary>Number of FATs</summary>
            public readonly byte fats_no;
            /// <summary>Number of entries on root directory</summary>
            public readonly ushort root_ent;
            /// <summary>Sectors in volume</summary>
            public readonly ushort sectors;
            /// <summary>Media descriptor</summary>
            public readonly byte media;
            /// <summary>Sectors per FAT</summary>
            public readonly ushort spfat;
            /// <summary>Disk type</summary>
            public readonly byte diskType;
            /// <summary>Volume starting sector</summary>
            public readonly ushort startSector;
        }
    }
}