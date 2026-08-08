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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of Apricot partitions</summary>
public sealed class Apricot : IPartition
{
    const    string MODULE_NAME = "Apricot partitions plugin";
    readonly int[]  _baudRates  = [50, 75, 110, 134, 150, 300, 600, 1200, 1800, 2400, 3600, 4800, 7200, 9600, 19200];
    readonly string[] _bootTypeCodes =
    [
        Localization.Non_bootable, Localization.Apricot_XI_RAM_BIOS, Localization.Generic_ROM_BIOS,
        Localization.Apricot_XI_ROM_BIOS, Localization.Apricot_Portable_ROM_BIOS, Localization.Apricot_F1_ROM_BIOS
    ];
    readonly string[] _diskTypeCodes =
    [
        Localization.MF1DD_70_track, "MF1DD", "MF2DD", "Winchester 5M", "Winchester 10M", "Winchester 20M"
    ];
    readonly int[] _lineModes  = [256, 200];
    readonly int[] _lineWidths = [80, 40];
    readonly string[] _operatingSystemCodes =
    [
        Localization.Invalid_operating_system, "MS-DOS", "UCSD Pascal", Localization.CPM, "Concurrent CP/M"
    ];
    readonly string[] _parityTypes =
    [
        Localization.None_parity, Localization.Odd_parity, Localization.Even_parity, Localization.Mark_parity,
        Localization.Space_parity
    ];
    readonly string[] _printDevices = [Localization.Parallel_print_device, Localization.Serial_print_device];
    readonly double[] _stopBits     = [1, 1.5, 2];

#region Nested type: Label

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
        public readonly uint   cylinders;
        /// <summary>Sides.</summary>
        public readonly byte   heads;
        /// <summary>Interleave factor.</summary>
        public readonly byte   interleave;
        /// <summary>Skew factor.</summary>
        public readonly ushort skew;
        /// <summary>Sector where boot code starts.</summary>
        public readonly uint   bootLocation;
        /// <summary>Size in sectors of boot code.</summary>
        public readonly ushort bootSize;
        /// <summary>Address at which to load boot code.</summary>
        public readonly uint   bootAddress;
        /// <summary>Offset where to jump to boot.</summary>
        public readonly ushort bootOffset;
        /// <summary>Segment where to jump to boot.</summary>
        public readonly ushort bootSegment;
        /// <summary>First data sector.</summary>
        public readonly uint   firstDataBlock;
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
        public readonly byte   dosLength;
        /// <summary>Length in sectors of FONT file.</summary>
        public readonly byte   fontLength;
        /// <summary>Length in sectors of KEYBOARD file.</summary>
        public readonly byte   keyboardLength;
        /// <summary>Starting sector of DOS.</summary>
        public readonly ushort dosStart;
        /// <summary>Starting sector of FONT file.</summary>
        public readonly ushort fontStart;
        /// <summary>Starting sector of KEYBOARD file.</summary>
        public readonly ushort keyboardStart;
        /// <summary>Keyboard click volume.</summary>
        public readonly byte   keyboardVolume;
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
        public readonly byte   xonCharacter;
        /// <summary>Xoff character.</summary>
        public readonly byte   xoffCharacter;
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
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool cpmDoubleSided;
    }

#endregion

#region Nested type: ParameterBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ParameterBlock
    {
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte   spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte   fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public readonly ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte   media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Disk type</summary>
        public readonly byte   diskType;
        /// <summary>Volume starting sector</summary>
        public readonly ushort startSector;
    }

#endregion

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.Apricot_Name;

    /// <inheritdoc />
    public Guid Id => new("8CBF5864-7B5A-47A0-8CEB-199C74FA22DE");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];

        // I think Apricot can't chain partitions so.
        if(sectorOffset != 0) return false;

        ErrorNumber errno = imagePlugin.ReadSector(0, false, out byte[] sector, out _);

        if(errno != ErrorNumber.NoError || sector.Length < 512) return false;

        Label label = Marshal.ByteArrayToStructureLittleEndian<Label>(sector);

        // Not much to check but...
        ulong deviceSectors              = imagePlugin.Info.Sectors;
        ulong deviceSizeAccordingToLabel = (ulong)label.cylinders * label.heads * label.spt;

        if(label.operatingSystem      > 4             ||
           label.bootType             > 5             ||
           label.partitionCount       > 8             ||
           deviceSizeAccordingToLabel > deviceSectors ||
           label.firstDataBlock       > deviceSectors)
            return false;

        AaruLogging.Debug(MODULE_NAME, "label.version = \"{0}\"", StringHandlers.CToString(label.version));

        AaruLogging.Debug(MODULE_NAME,
                          "label.operatingSystem = {0} ({1})",
                          label.operatingSystem,
                          label.operatingSystem < _operatingSystemCodes.Length
                              ? _operatingSystemCodes[label.operatingSystem]
                              : Localization.Unknown_operating_system);

        AaruLogging.Debug(MODULE_NAME, "label.writeProtected = {0}", label.writeProtected);
        AaruLogging.Debug(MODULE_NAME, "label.copyProtected = {0}",  label.copyProtected);

        AaruLogging.Debug(MODULE_NAME,
                          "label.bootType = {0} ({1})",
                          label.bootType,
                          label.bootType < _bootTypeCodes.Length
                              ? _bootTypeCodes[label.bootType]
                              : Localization.Unknown_boot_type);

        AaruLogging.Debug(MODULE_NAME, "label.partitionCount = {0}",   label.partitionCount);
        AaruLogging.Debug(MODULE_NAME, "label.winchester = {0}",       label.winchester);
        AaruLogging.Debug(MODULE_NAME, "label.sectorSize = {0}",       label.sectorSize);
        AaruLogging.Debug(MODULE_NAME, "label.spt = {0}",              label.spt);
        AaruLogging.Debug(MODULE_NAME, "label.cylinders = {0}",        label.cylinders);
        AaruLogging.Debug(MODULE_NAME, "label.heads = {0}",            label.heads);
        AaruLogging.Debug(MODULE_NAME, "label.interleave = {0}",       label.interleave);
        AaruLogging.Debug(MODULE_NAME, "label.skew = {0}",             label.skew);
        AaruLogging.Debug(MODULE_NAME, "label.bootLocation = {0}",     label.bootLocation);
        AaruLogging.Debug(MODULE_NAME, "label.bootSize = {0}",         label.bootSize);
        AaruLogging.Debug(MODULE_NAME, "label.bootAddress = 0x{0:X8}", label.bootAddress);

        AaruLogging.Debug(MODULE_NAME,
                          "label.bootOffset:label.bootSegment = {0:X4}:{1:X4}",
                          label.bootOffset,
                          label.bootSegment);

        AaruLogging.Debug(MODULE_NAME, "label.firstDataBlock = {0}", label.firstDataBlock);
        AaruLogging.Debug(MODULE_NAME, "label.generation = {0}",     label.generation);
        AaruLogging.Debug(MODULE_NAME, "label.copyCount = {0}",      label.copyCount);
        AaruLogging.Debug(MODULE_NAME, "label.maxCopies = {0}",      label.maxCopies);

        AaruLogging.Debug(MODULE_NAME, "label.serialNumber = \"{0}\"", StringHandlers.CToString(label.serialNumber));

        AaruLogging.Debug(MODULE_NAME, "label.partNumber = \"{0}\"", StringHandlers.CToString(label.partNumber));

        AaruLogging.Debug(MODULE_NAME, "label.copyright = \"{0}\"", StringHandlers.CToString(label.copyright));

        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.bps = {0}",      label.mainBPB.bps);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.spc = {0}",      label.mainBPB.spc);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.rsectors = {0}", label.mainBPB.rsectors);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.fats_no = {0}",  label.mainBPB.fats_no);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.root_ent = {0}", label.mainBPB.root_ent);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.sectors = {0}",  label.mainBPB.sectors);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.media = {0}",    label.mainBPB.media);
        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.spfat = {0}",    label.mainBPB.spfat);

        AaruLogging.Debug(MODULE_NAME,
                          "label.mainBPB.diskType = {0} ({1})",
                          label.mainBPB.diskType,
                          label.mainBPB.diskType < _diskTypeCodes.Length
                              ? _diskTypeCodes[label.mainBPB.diskType]
                              : Localization.Unknown_disk_type);

        AaruLogging.Debug(MODULE_NAME, "label.mainBPB.startSector = {0}", label.mainBPB.startSector);

        AaruLogging.Debug(MODULE_NAME, "label.fontName = \"{0}\"", StringHandlers.CToString(label.fontName));

        AaruLogging.Debug(MODULE_NAME, "label.keyboardName = \"{0}\"", StringHandlers.CToString(label.keyboardName));

        AaruLogging.Debug(MODULE_NAME, "label.biosMajorVersion = {0}", label.biosMajorVersion);
        AaruLogging.Debug(MODULE_NAME, "label.biosMinorVersion = {0}", label.biosMinorVersion);
        AaruLogging.Debug(MODULE_NAME, "label.diagnosticsFlag = {0}",  label.diagnosticsFlag);

        AaruLogging.Debug(MODULE_NAME,
                          "label.prnDevice = {0} ({1})",
                          label.prnDevice,
                          label.prnDevice < _printDevices.Length
                              ? _printDevices[label.prnDevice]
                              : Localization.Unknown_print_device);

        AaruLogging.Debug(MODULE_NAME, "label.bellVolume = {0}",       label.bellVolume);
        AaruLogging.Debug(MODULE_NAME, "label.enableCache = {0}",      label.enableCache);
        AaruLogging.Debug(MODULE_NAME, "label.enableGraphics = {0}",   label.enableGraphics);
        AaruLogging.Debug(MODULE_NAME, "label.dosLength = {0}",        label.dosLength);
        AaruLogging.Debug(MODULE_NAME, "label.fontLength = {0}",       label.fontLength);
        AaruLogging.Debug(MODULE_NAME, "label.keyboardLength = {0}",   label.keyboardLength);
        AaruLogging.Debug(MODULE_NAME, "label.dosStart = {0}",         label.dosStart);
        AaruLogging.Debug(MODULE_NAME, "label.fontStart = {0}",        label.fontStart);
        AaruLogging.Debug(MODULE_NAME, "label.keyboardStart = {0}",    label.keyboardStart);
        AaruLogging.Debug(MODULE_NAME, "label.keyboardVolume = {0}",   label.keyboardVolume);
        AaruLogging.Debug(MODULE_NAME, "label.autorepeat = {0}",       label.autorepeat);
        AaruLogging.Debug(MODULE_NAME, "label.autorepeatLeadIn = {0}", label.autorepeatLeadIn);

        AaruLogging.Debug(MODULE_NAME, "label.autorepeatInterval = {0}", label.autorepeatInterval);

        AaruLogging.Debug(MODULE_NAME, "label.microscreenMode = {0}", label.microscreenMode);

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareKeyboard is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareKeyboard));

        AaruLogging.Debug(MODULE_NAME,
                          "label.lineMode = {0} ({1} lines)",
                          label.lineMode,
                          label.lineMode < _lineModes.Length ? _lineModes[label.lineMode] : 0);

        AaruLogging.Debug(MODULE_NAME,
                          "label.lineWidth = {0} ({1} columns)",
                          label.lineWidth,
                          label.lineWidth < _lineWidths.Length ? _lineWidths[label.lineWidth] : 0);

        AaruLogging.Debug(MODULE_NAME, "label.imageOff = {0}", label.imageOff);

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareScreen is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareScreen));

        AaruLogging.Debug(MODULE_NAME,
                          "label.txBaudRate = {0} ({1} bps)",
                          label.txBaudRate,
                          label.txBaudRate < _baudRates.Length ? _baudRates[label.txBaudRate] : 0);

        AaruLogging.Debug(MODULE_NAME,
                          "label.rxBaudRate = {0} ({1} bps)",
                          label.rxBaudRate,
                          label.rxBaudRate < _baudRates.Length ? _baudRates[label.rxBaudRate] : 0);

        AaruLogging.Debug(MODULE_NAME, "label.txBits = {0}", label.txBits);
        AaruLogging.Debug(MODULE_NAME, "label.rxBits = {0}", label.rxBits);

        AaruLogging.Debug(MODULE_NAME,
                          "label.stopBits = {0} ({1} bits)",
                          label.stopBits,
                          label.stopBits < _stopBits.Length ? _stopBits[label.stopBits] : 0);

        AaruLogging.Debug(MODULE_NAME, "label.parityCheck = {0}", label.parityCheck);

        AaruLogging.Debug(MODULE_NAME,
                          "label.parityType = {0} ({1})",
                          label.parityType,
                          label.parityType < _parityTypes.Length
                              ? _parityTypes[label.parityType]
                              : Localization.Unknown_parity_type);

        AaruLogging.Debug(MODULE_NAME, "label.txXonXoff = {0}",       label.txXonXoff);
        AaruLogging.Debug(MODULE_NAME, "label.rxXonXoff = {0}",       label.rxXonXoff);
        AaruLogging.Debug(MODULE_NAME, "label.xonCharacter = {0}",    label.xonCharacter);
        AaruLogging.Debug(MODULE_NAME, "label.xoffCharacter = {0}",   label.xoffCharacter);
        AaruLogging.Debug(MODULE_NAME, "label.rxXonXoffBuffer = {0}", label.rxXonXoffBuffer);
        AaruLogging.Debug(MODULE_NAME, "label.dtrDsr = {0}",          label.dtrDsr);
        AaruLogging.Debug(MODULE_NAME, "label.ctsRts = {0}",          label.ctsRts);
        AaruLogging.Debug(MODULE_NAME, "label.nullsAfterCr = {0}",    label.nullsAfterCr);
        AaruLogging.Debug(MODULE_NAME, "label.nullsAfterFF = {0}",    label.nullsAfterFF);
        AaruLogging.Debug(MODULE_NAME, "label.lfAfterCRSerial = {0}", label.lfAfterCRSerial);

        AaruLogging.Debug(MODULE_NAME, "label.biosErrorReportSerial = {0}", label.biosErrorReportSerial);

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareSerial is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareSerial));

        AaruLogging.Debug(MODULE_NAME, "label.lfAfterCrParallel = {0}", label.lfAfterCrParallel);
        AaruLogging.Debug(MODULE_NAME, "label.selectLine = {0}",        label.selectLine);
        AaruLogging.Debug(MODULE_NAME, "label.paperEmpty = {0}",        label.paperEmpty);
        AaruLogging.Debug(MODULE_NAME, "label.faultLine = {0}",         label.faultLine);

        AaruLogging.Debug(MODULE_NAME, "label.biosErrorReportParallel = {0}", label.biosErrorReportParallel);

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareParallel is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareParallel));

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareWinchester is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareWinchester));

        AaruLogging.Debug(MODULE_NAME, "label.parkingEnabled = {0}",   label.parkingEnabled);
        AaruLogging.Debug(MODULE_NAME, "label.formatProtection = {0}", label.formatProtection);

        AaruLogging.Debug(MODULE_NAME,
                          "label.spareRamDisk is null? = {0}",
                          ArrayHelpers.ArrayIsNullOrEmpty(label.spareRamDisk));

        for(var i = 0; i < 32; i++) AaruLogging.Debug(MODULE_NAME, "label.badBlocks[{1}] = {0}", label.badBlocks[i], i);

        for(var i = 0; i < 8; i++)
        {
            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].bps = {0}", label.partitions[i].bps, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].spc = {0}", label.partitions[i].spc, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].rsectors = {0}", label.partitions[i].rsectors, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].fats_no = {0}", label.partitions[i].fats_no, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].root_ent = {0}", label.partitions[i].root_ent, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].sectors = {0}", label.partitions[i].sectors, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].media = {0}", label.partitions[i].media, i);

            AaruLogging.Debug(MODULE_NAME, "label.partitions[{1}].spfat = {0}", label.partitions[i].spfat, i);

            AaruLogging.Debug(MODULE_NAME,
                              "label.partitions[{1}].diskType = {0} ({2})",
                              label.partitions[i].diskType,
                              i,
                              label.partitions[i].diskType < _diskTypeCodes.Length
                                  ? _diskTypeCodes[label.partitions[i].diskType]
                                  : Localization.Unknown_disk_type);

            AaruLogging.Debug(MODULE_NAME,
                              "label.partitions[{1}].startSector = {0}",
                              label.partitions[i].startSector,
                              i);
        }

        AaruLogging.Debug(MODULE_NAME, "label.spare is null? = {0}", ArrayHelpers.ArrayIsNullOrEmpty(label.spare));

        AaruLogging.Debug(MODULE_NAME, "label.cpmDoubleSided = {0}", label.cpmDoubleSided);

        // Only hard disks can contain partitions
        if(!label.winchester) return false;

        for(byte i = 0; i < label.partitionCount; i++)
        {
            var part = new Partition
            {
                Start    = label.partitions[i].startSector,
                Size     = (ulong)label.partitions[i].sectors * label.sectorSize,
                Length   = label.partitions[i].sectors,
                Type     = "ACT Apricot partition",
                Sequence = i,
                Scheme   = Name,
                Offset   = (ulong)label.partitions[i].startSector * label.sectorSize
            };

            if(part.Start < deviceSectors && part.End < deviceSectors) partitions.Add(part);
        }

        return partitions.Count > 0;
    }

#endregion
}