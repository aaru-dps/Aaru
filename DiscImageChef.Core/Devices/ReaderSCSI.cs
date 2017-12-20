// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;

namespace DiscImageChef.Core.Devices
{
    partial class Reader
    {
        // TODO: Raw reading
        bool read6;
        bool read10;
        bool read12;
        bool read16;
        bool readLong10;
        bool readLong16;
        bool hldtstReadRaw;
        bool readLongDvd;
        bool plextorReadRaw;
        bool syqReadLong6;
        bool syqReadLong10;
        bool seek6;
        bool seek10;

        ulong ScsiGetBlocks()
        {
            return ScsiGetBlockSize() ? 0 : blocks;
        }

        bool ScsiFindReadCommand()
        {
            byte[] readBuffer;
            byte[] senseBuf;
            double duration;

            read6 = !dev.Read6(out readBuffer, out senseBuf, 0, blockSize, timeout, out duration);

            read10 = !dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1,
                                 timeout, out duration);

            read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0, 1, false,
                                 timeout, out duration);

            read16 = !dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, blockSize, 0, 1, false,
                                 timeout, out duration);

            seek6 = !dev.Seek6(out senseBuf, 0, timeout, out duration);

            seek10 = !dev.Seek10(out senseBuf, 0, timeout, out duration);

            if(!read6 && !read10 && !read12 && !read16)
            {
                errorMessage = "Cannot read medium, aborting scan...";
                return true;
            }

            if(read6 && !read10 && !read12 && !read16 && blocks > (0x001FFFFF + 1))
            {
                errorMessage =
                    string.Format("Device only supports SCSI READ (6) but has more than {0} blocks ({1} blocks total)",
                                  0x001FFFFF + 1, blocks);
                return true;
            }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
            if(!read16 && blocks > ((long)0xFFFFFFFF + (long)1))
#pragma warning restore IDE0004 // Remove Unnecessary Cast
            {
#pragma warning disable IDE0004 // Remove Unnecessary Cast
                errorMessage =
                    string.Format("Device only supports SCSI READ (10) but has more than {0} blocks ({1} blocks total)",
                                  (long)0xFFFFFFFF + (long)1, blocks);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                return true;
            }

            if(readRaw)
            {
                bool testSense;
                Decoders.SCSI.FixedSense? decSense;
                readRaw = false;

                if(dev.SCSIType != Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
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

                    testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, 0xFFFF, timeout,
                                               out duration);
                    if(testSense && !dev.Error)
                    {
                        decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                        if(decSense.HasValue)
                        {
                            if(decSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest &&
                               decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                            {
                                readRaw = true;
                                if(decSense.Value.InformationValid && decSense.Value.ILI)
                                {
                                    longBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                    readLong10 = !dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0,
                                                                 (ushort)longBlockSize, timeout, out duration);
                                }
                            }
                        }
                    }

                    if(readRaw && longBlockSize == blockSize)
                    {
                        if(blockSize == 512)
                        {
                            foreach(ushort testSize in new[]
                            {
                                // Long sector sizes for floppies
                                514,
                                // Long sector sizes for SuperDisk
                                536, 558,
                                // Long sector sizes for 512-byte magneto-opticals
                                600, 610, 630
                            })
                            {
                                testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, testSize, timeout,
                                                           out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong16 = true;
                                    longBlockSize = testSize;
                                    readRaw = true;
                                    break;
                                }

                                testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, testSize,
                                                           timeout, out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong10 = true;
                                    longBlockSize = testSize;
                                    readRaw = true;
                                    break;
                                }
                            }
                        }
                        else if(blockSize == 1024)
                        {
                            foreach(ushort testSize in new[]
                            {
                                // Long sector sizes for floppies
                                1026,
                                // Long sector sizes for 1024-byte magneto-opticals
                                1200
                            })
                            {
                                testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, testSize, timeout,
                                                           out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong16 = true;
                                    longBlockSize = testSize;
                                    readRaw = true;
                                    break;
                                }

                                testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, testSize,
                                                           timeout, out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong10 = true;
                                    longBlockSize = testSize;
                                    readRaw = true;
                                    break;
                                }
                            }
                        }
                        else if(blockSize == 2048)
                        {
                            testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, 2380, timeout,
                                                       out duration);
                            if(!testSense && !dev.Error)
                            {
                                readLong16 = true;
                                longBlockSize = 2380;
                                readRaw = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, 2380, timeout,
                                                           out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong10 = true;
                                    longBlockSize = 2380;
                                    readRaw = true;
                                }
                            }
                        }
                        else if(blockSize == 4096)
                        {
                            testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, 4760, timeout,
                                                       out duration);
                            if(!testSense && !dev.Error)
                            {
                                readLong16 = true;
                                longBlockSize = 4760;
                                readRaw = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, 4760, timeout,
                                                           out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong10 = true;
                                    longBlockSize = 4760;
                                    readRaw = true;
                                }
                            }
                        }
                        else if(blockSize == 8192)
                        {
                            testSense = dev.ReadLong16(out readBuffer, out senseBuf, false, 0, 9424, timeout,
                                                       out duration);
                            if(!testSense && !dev.Error)
                            {
                                readLong16 = true;
                                longBlockSize = 9424;
                                readRaw = true;
                            }
                            else
                            {
                                testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, 9424, timeout,
                                                           out duration);
                                if(!testSense && !dev.Error)
                                {
                                    readLong10 = true;
                                    longBlockSize = 9424;
                                    readRaw = true;
                                }
                            }
                        }
                    }

                    if(!readRaw && dev.Manufacturer == "SYQUEST")
                    {
                        testSense = dev.SyQuestReadLong10(out readBuffer, out senseBuf, 0, 0xFFFF, timeout,
                                                          out duration);
                        if(testSense)
                        {
                            decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                            if(decSense.HasValue)
                            {
                                if(decSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest &&
                                   decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                {
                                    readRaw = true;
                                    if(decSense.Value.InformationValid && decSense.Value.ILI)
                                    {
                                        longBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                        syqReadLong10 =
                                            !dev.SyQuestReadLong10(out readBuffer, out senseBuf, 0, longBlockSize,
                                                                   timeout, out duration);
                                    }
                                }
                                else
                                {
                                    testSense = dev.SyQuestReadLong6(out readBuffer, out senseBuf, 0, 0xFFFF, timeout,
                                                                     out duration);
                                    if(testSense)
                                    {
                                        decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuf);
                                        if(decSense.HasValue)
                                        {
                                            if(decSense.Value.SenseKey == Decoders.SCSI.SenseKeys.IllegalRequest &&
                                               decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                            {
                                                readRaw = true;
                                                if(decSense.Value.InformationValid && decSense.Value.ILI)
                                                {
                                                    longBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                                    syqReadLong6 =
                                                        !dev.SyQuestReadLong6(out readBuffer, out senseBuf, 0,
                                                                              longBlockSize, timeout, out duration);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(!readRaw && blockSize == 256)
                        {
                            testSense = dev.SyQuestReadLong6(out readBuffer, out senseBuf, 0, 262, timeout,
                                                             out duration);
                            if(!testSense && !dev.Error)
                            {
                                syqReadLong6 = true;
                                longBlockSize = 262;
                                readRaw = true;
                            }
                        }
                    }
                }
                else
                {
                    if(dev.Manufacturer == "HL-DT-ST")
                        hldtstReadRaw =
                            !dev.HlDtStReadRawDvd(out readBuffer, out senseBuf, 0, 1, timeout, out duration);

                    if(dev.Manufacturer == "PLEXTOR")
                        plextorReadRaw =
                            !dev.PlextorReadRawDvd(out readBuffer, out senseBuf, 0, 1, timeout, out duration);

                    if(hldtstReadRaw || plextorReadRaw)
                    {
                        readRaw = true;
                        longBlockSize = 2064;
                    }

                    // READ LONG (10) for some DVD drives
                    if(!readRaw && dev.Manufacturer == "MATSHITA")
                    {
                        testSense = dev.ReadLong10(out readBuffer, out senseBuf, false, false, 0, 37856, timeout,
                                                   out duration);
                        if(!testSense && !dev.Error)
                        {
                            readLongDvd = true;
                            longBlockSize = 37856;
                            readRaw = true;
                        }
                    }
                }
            }

            if(readRaw)
            {
                if(readLong16) DicConsole.WriteLine("Using SCSI READ LONG (16) command.");
                else if(readLong10 || readLongDvd) DicConsole.WriteLine("Using SCSI READ LONG (10) command.");
                else if(syqReadLong10) DicConsole.WriteLine("Using SyQuest READ LONG (10) command.");
                else if(syqReadLong6) DicConsole.WriteLine("Using SyQuest READ LONG (6) command.");
                else if(hldtstReadRaw) DicConsole.WriteLine("Using HL-DT-ST raw DVD reading.");
                else if(plextorReadRaw) DicConsole.WriteLine("Using Plextor raw DVD reading.");
            }
            else if(read16) DicConsole.WriteLine("Using SCSI READ (16) command.");
            else if(read12) DicConsole.WriteLine("Using SCSI READ (12) command.");
            else if(read10) DicConsole.WriteLine("Using SCSI READ (10) command.");
            else if(read6) DicConsole.WriteLine("Using SCSI READ (6) command.");

            return false;
        }

        bool ScsiGetBlockSize()
        {
            bool sense;
            byte[] cmdBuf;
            byte[] senseBuf;
            double duration;
            blocks = 0;

            sense = dev.ReadCapacity(out cmdBuf, out senseBuf, timeout, out duration);
            if(!sense)
            {
                blocks = (ulong)((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + (cmdBuf[3]));
                blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
            }

            if(sense || blocks == 0xFFFFFFFF)
            {
                sense = dev.ReadCapacity16(out cmdBuf, out senseBuf, timeout, out duration);

                if(sense && blocks == 0)
                {
                    // Not all MMC devices support READ CAPACITY, as they have READ TOC
                    if(dev.SCSIType != Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                    {
                        errorMessage = string.Format("Unable to get media capacity\n" + "{0}",
                                                     Decoders.SCSI.Sense.PrettifySense(senseBuf));

                        return true;
                    }
                }

                if(!sense)
                {
                    byte[] temp = new byte[8];

                    Array.Copy(cmdBuf, 0, temp, 0, 8);
                    Array.Reverse(temp);
                    blocks = BitConverter.ToUInt64(temp, 0);
                    blockSize = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + (cmdBuf[7]));
                }
            }

            physicalsectorsize = blockSize;
            longBlockSize = blockSize;
            return false;
        }

        bool ScsiGetBlocksToRead(uint startWithBlocks)
        {
            bool sense;
            byte[] readBuffer;
            byte[] senseBuf;
            double duration;
            blocksToRead = startWithBlocks;

            while(true)
            {
                if(read16)
                {
                    sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, blockSize, 0,
                                       blocksToRead, false, timeout, out duration);
                    if(dev.Error) blocksToRead /= 2;
                }
                else if(read12)
                {
                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, false, false, false, 0, blockSize, 0,
                                       blocksToRead, false, timeout, out duration);
                    if(dev.Error) blocksToRead /= 2;
                }
                else if(read10)
                {
                    sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0,
                                       (ushort)blocksToRead, timeout, out duration);
                    if(dev.Error) blocksToRead /= 2;
                }
                else if(read6)
                {
                    sense = dev.Read6(out readBuffer, out senseBuf, 0, blockSize, (byte)blocksToRead, timeout,
                                      out duration);
                    if(dev.Error) blocksToRead /= 2;
                }

                if(!dev.Error || blocksToRead == 1) break;
            }

            if(dev.Error)
            {
                blocksToRead = 1;
                errorMessage = string.Format("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return true;
            }

            return false;
        }

        bool ScsiReadBlocks(out byte[] buffer, ulong block, uint count, out double duration)
        {
            bool sense = false;
            byte[] senseBuf = null;
            buffer = null;
            duration = 0;

            if(readRaw)
            {
                if(readLong16)
                    sense = dev.ReadLong16(out buffer, out senseBuf, false, block, longBlockSize, timeout,
                                           out duration);
                else if(readLong10)
                    sense = dev.ReadLong10(out buffer, out senseBuf, false, false, (uint)block, (ushort)longBlockSize,
                                           timeout, out duration);
                else if(syqReadLong10)
                    sense = dev.SyQuestReadLong10(out buffer, out senseBuf, (uint)block, longBlockSize, timeout,
                                                  out duration);
                else if(syqReadLong6)
                    sense = dev.SyQuestReadLong6(out buffer, out senseBuf, (uint)block, longBlockSize, timeout,
                                                 out duration);
                else if(hldtstReadRaw)
                    sense = dev.HlDtStReadRawDvd(out buffer, out senseBuf, (uint)block, longBlockSize, timeout,
                                                 out duration);
                else if(plextorReadRaw)
                    sense = dev.PlextorReadRawDvd(out buffer, out senseBuf, (uint)block, longBlockSize, timeout,
                                                  out duration);
                else return true;
            }
            else
            {
                if(read16)
                    sense = dev.Read16(out buffer, out senseBuf, 0, false, true, false, block, blockSize, 0, count,
                                       false, timeout, out duration);
                else if(read12)
                    sense = dev.Read12(out buffer, out senseBuf, 0, false, false, false, false, (uint)block, blockSize,
                                       0, count, false, timeout, out duration);
                else if(read10)
                    sense = dev.Read10(out buffer, out senseBuf, 0, false, true, false, false, (uint)block, blockSize,
                                       0, (ushort)count, timeout, out duration);
                else if(read6)
                    sense = dev.Read6(out buffer, out senseBuf, (uint)block, blockSize, (byte)count, timeout,
                                      out duration);
                else return true;
            }

            if(sense || dev.Error)
            {
                DicConsole.DebugWriteLine("SCSI Reader", "READ error:\n{0}",
                                          Decoders.SCSI.Sense.PrettifySense(senseBuf));
                return true;
            }

            return false;
        }

        bool ScsiSeek(ulong block, out double duration)
        {
            byte[] senseBuf;
            bool sense = true;
            duration = 0;

            if(seek6) sense = dev.Seek6(out senseBuf, (uint)block, timeout, out duration);
            else if(seek10) sense = dev.Seek10(out senseBuf, (uint)block, timeout, out duration);

            return sense;
        }
    }
}