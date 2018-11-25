// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from ATA devices.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    ///     Implements creating a report for an ATA device
    /// </summary>
    public static class Ata
    {
        /// <summary>
        ///     Creates a report of an ATA device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="debug">If debug is enabled</param>
        /// <param name="removable">If device is removable</param>
        /// <param name="report2"></param>
        public static void Report(Device dev, ref DeviceReportV2 report, bool debug, ref bool removable)
        {
            if(report == null) return;

            const uint TIMEOUT = 5;

            DicConsole.WriteLine("Querying ATA IDENTIFY...");

            dev.AtaIdentify(out byte[] buffer, out _, TIMEOUT, out _);

            if(!Identify.Decode(buffer).HasValue) return;

            report.ATA                = new CommonTypes.Metadata.Ata();
            report.ATA.IdentifyDevice = Identify.Decode(buffer);
            ;

            if(report.ATA.IdentifyDevice == null) return;

            ConsoleKeyInfo pressedKey;
            if((ushort)report.ATA.IdentifyDevice?.GeneralConfiguration == 0x848A)
            {
                report.CompactFlash = true;
                removable           = false;
            }
            else if(!removable &&
                    report.ATA.IdentifyDevice?.GeneralConfiguration
                          .HasFlag(Identify.GeneralConfigurationBit.Removable) == true)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                removable = pressedKey.Key == ConsoleKey.Y;
            }

            if(removable)
            {
                DicConsole.WriteLine("Please remove any media from the device and press any key when it is out.");
                System.Console.ReadKey(true);
                DicConsole.WriteLine("Querying ATA IDENTIFY...");
                dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);
                report.ATA.IdentifyDevice = Identify.Decode(buffer).Value;
            }

            if(debug) report.ATA.Identify = buffer;

            if(removable)
            {
                List<TestedMedia> mediaTests = new List<TestedMedia>();

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.N)
                {
                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    if(pressedKey.Key != ConsoleKey.Y) continue;

                    DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                    System.Console.ReadKey(true);

                    TestedMedia mediaTest = new TestedMedia();
                    DicConsole.Write("Please write a description of the media type and press enter: ");
                    mediaTest.MediumTypeName = System.Console.ReadLine();
                    DicConsole.Write("Please write the media model and press enter: ");
                    mediaTest.Model = System.Console.ReadLine();

                    mediaTest.MediaIsRecognized = true;

                    DicConsole.WriteLine("Querying ATA IDENTIFY...");
                    dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);

                    mediaTest.IdentifyData   = buffer;
                    mediaTest.IdentifyDevice = Identify.Decode(buffer);

                    if(mediaTest.IdentifyDevice.HasValue)
                    {
                        Identify.IdentifyDevice ataId = mediaTest.IdentifyDevice.Value;

                        if(ataId.UnformattedBPT != 0) mediaTest.UnformattedBPT = ataId.UnformattedBPT;

                        if(ataId.UnformattedBPS != 0) mediaTest.UnformattedBPS = ataId.UnformattedBPS;

                        if(ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0)
                        {
                            mediaTest.CHS = new Chs
                            {
                                Cylinders = ataId.Cylinders, Heads = ataId.Heads, Sectors = ataId.SectorsPerTrack
                            };
                            mediaTest.Blocks = (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                        }

                        if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                        {
                            mediaTest.CurrentCHS = new Chs
                            {
                                Cylinders = ataId.CurrentCylinders,
                                Heads     = ataId.CurrentHeads,
                                Sectors   = ataId.CurrentSectorsPerTrack
                            };
                            if(mediaTest.Blocks == 0)
                                mediaTest.Blocks =
                                    (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads * ataId.CurrentSectorsPerTrack);
                        }

                        if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                        {
                            mediaTest.LBASectors = ataId.LBASectors;
                            mediaTest.Blocks     = ataId.LBASectors;
                        }

                        if(ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                        {
                            mediaTest.LBA48Sectors = ataId.LBA48Sectors;
                            mediaTest.Blocks       = ataId.LBA48Sectors;
                        }

                        if(ataId.NominalRotationRate != 0x0000 && ataId.NominalRotationRate != 0xFFFF)
                            if(ataId.NominalRotationRate == 0x0001)
                                mediaTest.SolidStateDevice = true;
                            else
                            {
                                mediaTest.SolidStateDevice    = false;
                                mediaTest.NominalRotationRate = ataId.NominalRotationRate;
                            }

                        uint logicalsectorsize;
                        uint physicalsectorsize;
                        if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 && (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                        {
                            if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                                if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                                    logicalsectorsize = 512;
                                else
                                    logicalsectorsize = ataId.LogicalSectorWords * 2;
                            else logicalsectorsize = 512;

                            if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                                physicalsectorsize =
                                    (uint)(logicalsectorsize * ((1 << ataId.PhysLogSectorSize) & 0xF));
                            else physicalsectorsize = logicalsectorsize;
                        }
                        else
                        {
                            logicalsectorsize  = 512;
                            physicalsectorsize = 512;
                        }

                        mediaTest.BlockSize = logicalsectorsize;
                        if(physicalsectorsize != logicalsectorsize)
                        {
                            mediaTest.PhysicalBlockSize = physicalsectorsize;

                            if((ataId.LogicalAlignment & 0x8000) == 0x0000 &&
                               (ataId.LogicalAlignment & 0x4000) == 0x4000)
                                mediaTest.LogicalAlignment = (ushort)(ataId.LogicalAlignment & 0x3FFF);
                        }

                        if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                            mediaTest.LongBlockSize = logicalsectorsize + ataId.EccBytes;

                        if(ataId.UnformattedBPS > logicalsectorsize &&
                           (!(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF) || mediaTest.LongBlockSize == 516))
                            mediaTest.LongBlockSize = ataId.UnformattedBPS;

                        if(ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet)    &&
                           !ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear) &&
                           ataId.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial))
                        {
                            mediaTest.CanReadMediaSerial = true;
                            if(!string.IsNullOrWhiteSpace(ataId.MediaManufacturer))
                                mediaTest.Manufacturer = ataId.MediaManufacturer;
                        }

                        ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                        bool  sense;

                        DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                        sense = dev.Read(out byte[] readBuf, out AtaErrorRegistersChs errorChs, false, 0, 0, 1, 1,
                                         TIMEOUT, out _);
                        mediaTest.SupportsReadSectors = !sense && (errorChs.Status & 0x01) != 0x01 &&
                                                        errorChs.Error                     == 0    &&
                                                        readBuf.Length                     > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorschs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                        sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadRetry = !sense && (errorChs.Status & 0x01) != 0x01 &&
                                                      errorChs.Error                     == 0    && readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorsretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                        sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDma = !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                                                    readBuf.Length                     > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmachs", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                        sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaRetry = !sense && (errorChs.Status & 0x01) != 0x01 &&
                                                         errorChs.Error                     == 0    &&
                                                         readBuf.Length                     > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmaretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying SEEK in CHS mode...");
                        sense                  = dev.Seek(out errorChs, 0, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsSeek = !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0;
                        DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}",
                                                  sense, errorChs.Status, errorChs.Error);

                        DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                        sense = dev.Read(out readBuf, out AtaErrorRegistersLba28 errorLba, false, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadLba = !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                                                    readBuf.Length                     > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectors", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                        sense = dev.Read(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadRetryLba = !sense && (errorLba.Status & 0x01) != 0x01 &&
                                                         errorLba.Error                     == 0    &&
                                                         readBuf.Length                     > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorsretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaLba = !sense && (errorLba.Status & 0x01) != 0x01 &&
                                                       errorLba.Error                     == 0    && readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdma", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaRetryLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmaretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying SEEK in LBA mode...");
                        sense                     = dev.Seek(out errorLba, 0, TIMEOUT, out _);
                        mediaTest.SupportsSeekLba = !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0;
                        DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}",
                                                  sense, errorChs.Status, errorChs.Error);

                        DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                        sense = dev.Read(out readBuf, out AtaErrorRegistersLba48 errorLba48, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadLba48 = !sense && (errorLba48.Status & 0x01) != 0x01 &&
                                                      errorLba48.Error                     == 0    &&
                                                      readBuf.Length                       > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectors48",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaLba48 = !sense && (errorLba48.Status & 0x01) != 0x01 &&
                                                         errorLba48.Error                     == 0    &&
                                                         readBuf.Length                       > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdma48", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        // Send SET FEATURES before sending READ LONG commands, retrieve IDENTIFY again and
                        // check if ECC size changed. Sector is set to 1 because without it most drives just return
                        // CORRECTABLE ERROR for this command.
                        dev.SetFeatures(out _, AtaFeatures.EnableReadLongVendorLength, 0, 0, 1, 0, TIMEOUT, out _);

                        dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);
                        if(Identify.Decode(buffer).HasValue)
                        {
                            ataId = Identify.Decode(buffer).Value;
                            if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                                mediaTest.LongBlockSize = logicalsectorsize + ataId.EccBytes;

                            if(ataId.UnformattedBPS > logicalsectorsize &&
                               (!(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF) ||
                                mediaTest.LongBlockSize == 516)) mediaTest.LongBlockSize = ataId.UnformattedBPS;
                        }

                        DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                        sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1, mediaTest.LongBlockSize ?? 0,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLong = !sense && (errorChs.Status & 0x01) != 0x01 &&
                                                     errorChs.Error                     == 0    && readBuf.Length > 0 &&
                                                     BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongchs", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                        sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1, mediaTest.LongBlockSize ?? 0,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLongRetry =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0 &&
                            BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);

                        DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                        sense = dev.ReadLong(out readBuf, out errorLba, false, 0, mediaTest.LongBlockSize ?? 0, TIMEOUT,
                                             out _);
                        mediaTest.SupportsReadLongLba = !sense && (errorLba.Status & 0x01) != 0x01 &&
                                                        errorLba.Error                     == 0    &&
                                                        readBuf.Length                     > 0     &&
                                                        BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlong", "_debug_" + mediaTest.MediumTypeName + ".bin",
                                             "read results", readBuf);

                        DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                        sense = dev.ReadLong(out readBuf, out errorLba, true, 0, mediaTest.LongBlockSize ?? 0, TIMEOUT,
                                             out _);
                        mediaTest.SupportsReadLongRetryLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0 &&
                            BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results", readBuf);
                    }
                    else mediaTest.MediaIsRecognized = false;

                    mediaTests.Add(mediaTest);
                }

                report.ATA.RemovableMedias = mediaTests.ToArray();
            }
            else
            {
                Identify.IdentifyDevice ataId = report.ATA.IdentifyDevice.Value;

                report.ATA.ReadCapabilities = new TestedMedia();

                if(ataId.UnformattedBPT != 0) report.ATA.ReadCapabilities.UnformattedBPT = ataId.UnformattedBPT;

                if(ataId.UnformattedBPS != 0) report.ATA.ReadCapabilities.UnformattedBPS = ataId.UnformattedBPS;

                if(ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0)
                {
                    report.ATA.ReadCapabilities.CHS = new Chs
                    {
                        Cylinders = ataId.Cylinders, Heads = ataId.Heads, Sectors = ataId.SectorsPerTrack
                    };
                    report.ATA.ReadCapabilities.Blocks = (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                }

                if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                {
                    report.ATA.ReadCapabilities.CurrentCHS = new Chs
                    {
                        Cylinders = ataId.CurrentCylinders,
                        Heads     = ataId.CurrentHeads,
                        Sectors   = ataId.CurrentSectorsPerTrack
                    };
                    report.ATA.ReadCapabilities.Blocks =
                        (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads * ataId.CurrentSectorsPerTrack);
                }

                if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                {
                    report.ATA.ReadCapabilities.LBASectors = ataId.LBASectors;
                    report.ATA.ReadCapabilities.Blocks     = ataId.LBASectors;
                }

                if(ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                {
                    report.ATA.ReadCapabilities.LBA48Sectors = ataId.LBA48Sectors;
                    report.ATA.ReadCapabilities.Blocks       = ataId.LBA48Sectors;
                }

                if(ataId.NominalRotationRate != 0x0000 && ataId.NominalRotationRate != 0xFFFF)
                    if(ataId.NominalRotationRate == 0x0001)
                        report.ATA.ReadCapabilities.SolidStateDevice = true;
                    else
                    {
                        report.ATA.ReadCapabilities.SolidStateDevice    = false;
                        report.ATA.ReadCapabilities.NominalRotationRate = ataId.NominalRotationRate;
                    }

                uint logicalsectorsize;
                uint physicalsectorsize;
                if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 && (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                {
                    if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                        if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                            logicalsectorsize = 512;
                        else
                            logicalsectorsize = ataId.LogicalSectorWords * 2;
                    else logicalsectorsize = 512;

                    if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                        physicalsectorsize  = logicalsectorsize * (uint)Math.Pow(2, ataId.PhysLogSectorSize & 0xF);
                    else physicalsectorsize = logicalsectorsize;
                }
                else
                {
                    logicalsectorsize  = 512;
                    physicalsectorsize = 512;
                }

                report.ATA.ReadCapabilities.BlockSize = logicalsectorsize;
                if(physicalsectorsize != logicalsectorsize)
                {
                    report.ATA.ReadCapabilities.PhysicalBlockSize = physicalsectorsize;

                    if((ataId.LogicalAlignment & 0x8000) == 0x0000 && (ataId.LogicalAlignment & 0x4000) == 0x4000)
                        report.ATA.ReadCapabilities.LogicalAlignment = (ushort)(ataId.LogicalAlignment & 0x3FFF);
                }

                if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                    report.ATA.ReadCapabilities.LongBlockSize = logicalsectorsize + ataId.EccBytes;

                if(ataId.UnformattedBPS > logicalsectorsize &&
                   (!(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF) ||
                    report.ATA.ReadCapabilities.LongBlockSize == 516))
                    report.ATA.ReadCapabilities.LongBlockSize = ataId.UnformattedBPS;

                if(ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet)    &&
                   !ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear) &&
                   ataId.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial))
                {
                    report.ATA.ReadCapabilities.CanReadMediaSerial = true;
                    if(!string.IsNullOrWhiteSpace(ataId.MediaManufacturer))
                        report.ATA.ReadCapabilities.Manufacturer = ataId.MediaManufacturer;
                }

                ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                bool  sense;

                DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                sense = dev.Read(out byte[] readBuf, out AtaErrorRegistersChs errorChs, false, 0, 0, 1, 1, TIMEOUT,
                                 out _);
                report.ATA.ReadCapabilities.SupportsReadSectors =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorschs",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorsretrychs",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDma =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmachs", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmaretrychs",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying SEEK in CHS mode...");
                sense = dev.Seek(out errorChs, 0, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsSeek =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0;
                DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                          errorChs.Status, errorChs.Error);

                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                sense = dev.Read(out readBuf, out AtaErrorRegistersLba28 errorLba, false, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectors", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                sense = dev.Read(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorsretry",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdma", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmaretry",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying SEEK in LBA mode...");
                sense = dev.Seek(out errorLba, 0, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsSeekLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0;
                DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                          errorLba.Status, errorLba.Error);

                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                sense = dev.Read(out readBuf, out AtaErrorRegistersLba48 errorLba48, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLba48 =
                    !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba48.Status, errorLba48.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectors48",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaLba48 =
                    !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba48.Status, errorLba48.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdma48", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                // Send SET FEATURES before sending READ LONG commands, retrieve IDENTIFY again and
                // check if ECC size changed. Sector is set to 1 because without it most drives just return
                // CORRECTABLE ERROR for this command.
                dev.SetFeatures(out _, AtaFeatures.EnableReadLongVendorLength, 0, 0, 1, 0, TIMEOUT, out _);

                dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);
                if(Identify.Decode(buffer).HasValue)
                {
                    ataId = Identify.Decode(buffer).Value;
                    if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                        report.ATA.ReadCapabilities.LongBlockSize = logicalsectorsize + ataId.EccBytes;

                    if(ataId.UnformattedBPS > logicalsectorsize &&
                       (!(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF) ||
                        report.ATA.ReadCapabilities.LongBlockSize == 516))
                        report.ATA.ReadCapabilities.LongBlockSize = ataId.UnformattedBPS;
                }

                DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1,
                                     report.ATA.ReadCapabilities.LongBlockSize ?? 0, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLong =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongchs", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1,
                                     report.ATA.ReadCapabilities.LongBlockSize ?? 0, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongretrychs",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                sense = dev.ReadLong(out readBuf, out errorLba, false, 0,
                                     report.ATA.ReadCapabilities.LongBlockSize ?? 0, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlong", "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                sense = dev.ReadLong(out readBuf, out errorLba, true, 0, report.ATA.ReadCapabilities.LongBlockSize ?? 0,
                                     TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0)  != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongretry",
                                     "_debug_" + report.ATA.IdentifyDevice?.Model + ".bin", "read results", readBuf);
            }
        }
    }
}