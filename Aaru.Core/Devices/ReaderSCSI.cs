// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ReaderSCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common code for reading SCSI devices.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;

namespace Aaru.Core.Devices
{
    internal partial class Reader
    {
        // TODO: Raw reading
        bool hldtstReadRaw;
        bool plextorReadRaw;
        bool read10;
        bool read12;
        bool read16;
        bool read6;
        bool readLong10;
        bool readLong16;
        bool readLongDvd;
        bool seek10;
        bool seek6;
        bool syqReadLong10;
        bool syqReadLong6;

        ulong ScsiGetBlocks() => ScsiGetBlockSize() ? 0 : Blocks;

        bool ScsiFindReadCommand()
        {
            read6 = !dev.Read6(out _, out byte[] senseBuf, 0, LogicalBlockSize, timeout, out _);

            read10 = !dev.Read10(out _, out senseBuf, 0, false, true, false, false, 0, LogicalBlockSize, 0, 1, timeout,
                                 out _);

            read12 = !dev.Read12(out _, out senseBuf, 0, false, true, false, false, 0, LogicalBlockSize, 0, 1, false,
                                 timeout, out _);

            read16 = !dev.Read16(out _, out senseBuf, 0, false, true, false, 0, LogicalBlockSize, 0, 1, false, timeout,
                                 out _);

            seek6 = !dev.Seek6(out senseBuf, 0, timeout, out _);

            seek10 = !dev.Seek10(out senseBuf, 0, timeout, out _);

            if(!read6  &&
               !read10 &&
               !read12 &&
               !read16)
            {
                ErrorMessage = "Cannot read medium, aborting scan...";

                return true;
            }

            if(read6   &&
               !read10 &&
               !read12 &&
               !read16 &&
               Blocks > 0x001FFFFF + 1)
            {
                ErrorMessage =
                    $"Device only supports SCSI READ (6) but has more than {0x001FFFFF + 1} blocks ({Blocks} blocks total)";

                return true;
            }

            if(!read16 &&
               Blocks > 0xFFFFFFFF + (long)1)
            {
                ErrorMessage =
                    $"Device only supports SCSI READ (10) but has more than {0xFFFFFFFF + (long)1} blocks ({Blocks} blocks total)";

                return true;
            }

            if(CanReadRaw)
            {
                bool testSense;
                CanReadRaw = false;

                if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
                {
                    /*testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, 0xFFFF, timeout, out duration);
                    if (testSense && !dev.Error)
                    {
                        decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                        if (decSense.HasValue)
                        {
                            if (decSense.Value.SenseKey == DiscImageChef.Decoders.SCSI.SenseKeys.IllegalRequest &&
                                decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                            {
                                readRaw = true;
                                if (decSense.Value.InformationValid && decSense.Value.ILI)
                                {
                                    longBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                    readLong16 = !dev.ReadLong16(out readBuffer, out senseBuf, false, 0, longBlockSize, timeout, out duration);
                                }
                            }
                        }
                    }*/

                    testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, 0xFFFF, timeout, out _);
                    FixedSense? decSense;

                    if(testSense && !dev.Error)
                    {
                        decSense = Sense.DecodeFixed(senseBuf);

                        if(decSense.HasValue)
                            if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                               decSense.Value.ASC      == 0x24                     &&
                               decSense.Value.ASCQ     == 0x00)
                            {
                                CanReadRaw = true;

                                if(decSense.Value.InformationValid &&
                                   decSense.Value.ILI)
                                {
                                    LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);

                                    readLong10 = !dev.ReadLong10(out _, out senseBuf, false, false, 0,
                                                                 (ushort)LongBlockSize, timeout, out _);
                                }
                            }
                    }

                    if(CanReadRaw && LongBlockSize == LogicalBlockSize)
                        if(LogicalBlockSize == 512)
                            foreach(int i in new[]
                            {
                                // Long sector sizes for floppies
                                514,

                                // Long sector sizes for SuperDisk
                                536, 558,

                                // Long sector sizes for 512-byte magneto-opticals
                                600, 610, 630
                            })
                            {
                                ushort testSize = (ushort)i;
                                testSense = dev.ReadLong16(out _, out senseBuf, false, 0, testSize, timeout, out _);

                                if(!testSense &&
                                   !dev.Error)
                                {
                                    readLong16    = true;
                                    LongBlockSize = testSize;
                                    CanReadRaw    = true;

                                    break;
                                }

                                testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, testSize, timeout,
                                                           out _);

                                if(testSense || dev.Error)
                                    continue;

                                readLong10    = true;
                                LongBlockSize = testSize;
                                CanReadRaw    = true;

                                break;
                            }
                        else if(LogicalBlockSize == 1024)
                            foreach(int i in new[]
                            {
                                // Long sector sizes for floppies
                                1026,

                                // Long sector sizes for 1024-byte magneto-opticals
                                1200
                            })
                            {
                                ushort testSize = (ushort)i;
                                testSense = dev.ReadLong16(out _, out senseBuf, false, 0, testSize, timeout, out _);

                                if(!testSense &&
                                   !dev.Error)
                                {
                                    readLong16    = true;
                                    LongBlockSize = testSize;
                                    CanReadRaw    = true;

                                    break;
                                }

                                testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, testSize, timeout,
                                                           out _);

                                if(testSense || dev.Error)
                                    continue;

                                readLong10    = true;
                                LongBlockSize = testSize;
                                CanReadRaw    = true;

                                break;
                            }
                        else if(LogicalBlockSize == 2048)
                        {
                            testSense = dev.ReadLong16(out _, out senseBuf, false, 0, 2380, timeout, out _);

                            if(!testSense &&
                               !dev.Error)
                            {
                                readLong16    = true;
                                LongBlockSize = 2380;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, 2380, timeout, out _);

                                if(!testSense &&
                                   !dev.Error)
                                {
                                    readLong10    = true;
                                    LongBlockSize = 2380;
                                    CanReadRaw    = true;
                                }
                            }
                        }
                        else if(LogicalBlockSize == 4096)
                        {
                            testSense = dev.ReadLong16(out _, out senseBuf, false, 0, 4760, timeout, out _);

                            if(!testSense &&
                               !dev.Error)
                            {
                                readLong16    = true;
                                LongBlockSize = 4760;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, 4760, timeout, out _);

                                if(!testSense &&
                                   !dev.Error)
                                {
                                    readLong10    = true;
                                    LongBlockSize = 4760;
                                    CanReadRaw    = true;
                                }
                            }
                        }
                        else if(LogicalBlockSize == 8192)
                        {
                            testSense = dev.ReadLong16(out _, out senseBuf, false, 0, 9424, timeout, out _);

                            if(!testSense &&
                               !dev.Error)
                            {
                                readLong16    = true;
                                LongBlockSize = 9424;
                                CanReadRaw    = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, 9424, timeout, out _);

                                if(!testSense &&
                                   !dev.Error)
                                {
                                    readLong10    = true;
                                    LongBlockSize = 9424;
                                    CanReadRaw    = true;
                                }
                            }
                        }

                    if(!CanReadRaw &&
                       dev.Manufacturer == "SYQUEST")
                    {
                        testSense = dev.SyQuestReadLong10(out _, out senseBuf, 0, 0xFFFF, timeout, out _);

                        if(testSense)
                        {
                            decSense = Sense.DecodeFixed(senseBuf);

                            if(decSense.HasValue)
                                if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                                   decSense.Value.ASC      == 0x24                     &&
                                   decSense.Value.ASCQ     == 0x00)
                                {
                                    CanReadRaw = true;

                                    if(decSense.Value.InformationValid &&
                                       decSense.Value.ILI)
                                    {
                                        LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);

                                        syqReadLong10 =
                                            !dev.SyQuestReadLong10(out _, out senseBuf, 0, LongBlockSize, timeout,
                                                                   out _);
                                    }
                                }
                                else
                                {
                                    testSense = dev.SyQuestReadLong6(out _, out senseBuf, 0, 0xFFFF, timeout, out _);

                                    if(testSense)
                                    {
                                        decSense = Sense.DecodeFixed(senseBuf);

                                        if(decSense.HasValue)
                                            if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                                               decSense.Value.ASC      == 0x24                     &&
                                               decSense.Value.ASCQ     == 0x00)
                                            {
                                                CanReadRaw = true;

                                                if(decSense.Value.InformationValid &&
                                                   decSense.Value.ILI)
                                                {
                                                    LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);

                                                    syqReadLong6 =
                                                        !dev.SyQuestReadLong6(out _, out senseBuf, 0, LongBlockSize,
                                                                              timeout, out _);
                                                }
                                            }
                                    }
                                }
                        }

                        if(!CanReadRaw &&
                           LogicalBlockSize == 256)
                        {
                            testSense = dev.SyQuestReadLong6(out _, out senseBuf, 0, 262, timeout, out _);

                            if(!testSense &&
                               !dev.Error)
                            {
                                syqReadLong6  = true;
                                LongBlockSize = 262;
                                CanReadRaw    = true;
                            }
                        }
                    }
                }
                else
                {
                    switch(dev.Manufacturer)
                    {
                        case"HL-DT-ST":
                            hldtstReadRaw = !dev.HlDtStReadRawDvd(out _, out senseBuf, 0, 1, timeout, out _);

                            break;
                        case"PLEXTOR":
                            plextorReadRaw = !dev.PlextorReadRawDvd(out _, out senseBuf, 0, 1, timeout, out _);

                            break;
                    }

                    if(hldtstReadRaw || plextorReadRaw)
                    {
                        CanReadRaw    = true;
                        LongBlockSize = 2064;
                    }

                    // READ LONG (10) for some DVD drives
                    if(!CanReadRaw &&
                       dev.Manufacturer == "MATSHITA")
                    {
                        testSense = dev.ReadLong10(out _, out senseBuf, false, false, 0, 37856, timeout, out _);

                        if(!testSense &&
                           !dev.Error)
                        {
                            readLongDvd   = true;
                            LongBlockSize = 37856;
                            CanReadRaw    = true;
                        }
                    }
                }
            }

            if(CanReadRaw)
            {
                if(readLong16)
                    DicConsole.WriteLine("Using SCSI READ LONG (16) command.");
                else if(readLong10 || readLongDvd)
                    DicConsole.WriteLine("Using SCSI READ LONG (10) command.");
                else if(syqReadLong10)
                    DicConsole.WriteLine("Using SyQuest READ LONG (10) command.");
                else if(syqReadLong6)
                    DicConsole.WriteLine("Using SyQuest READ LONG (6) command.");
                else if(hldtstReadRaw)
                    DicConsole.WriteLine("Using HL-DT-ST raw DVD reading.");
                else if(plextorReadRaw)
                    DicConsole.WriteLine("Using Plextor raw DVD reading.");
            }
            else if(read16)
                DicConsole.WriteLine("Using SCSI READ (16) command.");
            else if(read12)
                DicConsole.WriteLine("Using SCSI READ (12) command.");
            else if(read10)
                DicConsole.WriteLine("Using SCSI READ (10) command.");
            else if(read6)
                DicConsole.WriteLine("Using SCSI READ (6) command.");

            return false;
        }

        bool ScsiGetBlockSize()
        {
            bool sense;
            Blocks = 0;

            sense = dev.ReadCapacity(out byte[] cmdBuf, out byte[] senseBuf, timeout, out _);

            if(!sense)
            {
                Blocks           = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]);
                LogicalBlockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8)  + cmdBuf[7]);
            }

            if(sense || Blocks == 0xFFFFFFFF)
            {
                sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, timeout, out _);

                if(sense && Blocks == 0)
                    if(dev.ScsiType != PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        ErrorMessage = "Unable to get media capacity\n" + $"{Sense.PrettifySense(senseBuf)}";

                        return true;
                    }

                if(!sense)
                {
                    byte[] temp = new byte[8];

                    Array.Copy(cmdBuf, 0, temp, 0, 8);
                    Array.Reverse(temp);
                    Blocks           = BitConverter.ToUInt64(temp, 0);
                    LogicalBlockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                }
            }

            PhysicalBlockSize = LogicalBlockSize;
            LongBlockSize     = LogicalBlockSize;

            return false;
        }

        bool ScsiGetBlocksToRead(uint startWithBlocks)
        {
            BlocksToRead = startWithBlocks;

            while(true)
            {
                if(read16)
                {
                    dev.Read16(out _, out _, 0, false, true, false, 0, LogicalBlockSize, 0, BlocksToRead, false,
                               timeout, out _);

                    if(dev.Error)
                        BlocksToRead /= 2;
                }
                else if(read12)
                {
                    dev.Read12(out _, out _, 0, false, false, false, false, 0, LogicalBlockSize, 0, BlocksToRead, false,
                               timeout, out _);

                    if(dev.Error)
                        BlocksToRead /= 2;
                }
                else if(read10)
                {
                    dev.Read10(out _, out _, 0, false, true, false, false, 0, LogicalBlockSize, 0, (ushort)BlocksToRead,
                               timeout, out _);

                    if(dev.Error)
                        BlocksToRead /= 2;
                }
                else if(read6)
                {
                    dev.Read6(out _, out _, 0, LogicalBlockSize, (byte)BlocksToRead, timeout, out _);

                    if(dev.Error)
                        BlocksToRead /= 2;
                }

                if(!dev.Error ||
                   BlocksToRead == 1)
                    break;
            }

            if(!dev.Error)
                return false;

            BlocksToRead = 1;
            ErrorMessage = $"Device error {dev.LastError} trying to guess ideal transfer length.";

            return true;
        }

        bool ScsiReadBlocks(out byte[] buffer, ulong block, uint count, out double duration)
        {
            bool   sense;
            byte[] senseBuf;
            buffer   = null;
            duration = 0;

            if(CanReadRaw)
                if(readLong16)
                    sense = dev.ReadLong16(out buffer, out senseBuf, false, block, LongBlockSize, timeout,
                                           out duration);
                else if(readLong10)
                    sense = dev.ReadLong10(out buffer, out senseBuf, false, false, (uint)block, (ushort)LongBlockSize,
                                           timeout, out duration);
                else if(syqReadLong10)
                    sense = dev.SyQuestReadLong10(out buffer, out senseBuf, (uint)block, LongBlockSize, timeout,
                                                  out duration);
                else if(syqReadLong6)
                    sense = dev.SyQuestReadLong6(out buffer, out senseBuf, (uint)block, LongBlockSize, timeout,
                                                 out duration);
                else if(hldtstReadRaw)
                    sense = dev.HlDtStReadRawDvd(out buffer, out senseBuf, (uint)block, LongBlockSize, timeout,
                                                 out duration);
                else if(plextorReadRaw)
                    sense = dev.PlextorReadRawDvd(out buffer, out senseBuf, (uint)block, LongBlockSize, timeout,
                                                  out duration);
                else
                    return true;
            else
            {
                if(read16)
                    sense = dev.Read16(out buffer, out senseBuf, 0, false, true, false, block, LogicalBlockSize, 0,
                                       count, false, timeout, out duration);
                else if(read12)
                    sense = dev.Read12(out buffer, out senseBuf, 0, false, false, false, false, (uint)block,
                                       LogicalBlockSize, 0, count, false, timeout, out duration);
                else if(read10)
                    sense = dev.Read10(out buffer, out senseBuf, 0, false, true, false, false, (uint)block,
                                       LogicalBlockSize, 0, (ushort)count, timeout, out duration);
                else if(read6)
                    sense = dev.Read6(out buffer, out senseBuf, (uint)block, LogicalBlockSize, (byte)count, timeout,
                                      out duration);
                else
                    return true;
            }

            if(!sense &&
               !dev.Error)
                return false;

            DicConsole.DebugWriteLine("SCSI Reader", "READ error:\n{0}", Sense.PrettifySense(senseBuf));

            return sense;
        }

        bool ScsiSeek(ulong block, out double duration)
        {
            bool sense = true;
            duration = 0;

            if(seek6)
                sense = dev.Seek6(out _, (uint)block, timeout, out duration);
            else if(seek10)
                sense = dev.Seek10(out _, (uint)block, timeout, out duration);

            return sense;
        }
    }
}