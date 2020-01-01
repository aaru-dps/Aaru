// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CDs and DDCDs.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    partial class Dump
    {
        /// <summary>Reads all CD user data</summary>
        /// <param name="audioExtents">Extents with audio sectors</param>
        /// <param name="blocks">Total number of positive sectors</param>
        /// <param name="blockSize">Size of the read sector in bytes</param>
        /// <param name="currentSpeed">Current read speed</param>
        /// <param name="currentTry">Current dump hardware try</param>
        /// <param name="extents">Extents</param>
        /// <param name="ibgLog">IMGBurn log</param>
        /// <param name="imageWriteDuration">Duration of image write</param>
        /// <param name="lastSector">Last sector number</param>
        /// <param name="leadOutExtents">Lead-out extents</param>
        /// <param name="maxSpeed">Maximum speed</param>
        /// <param name="mhddLog">MHDD log</param>
        /// <param name="minSpeed">Minimum speed</param>
        /// <param name="newTrim">Is trim a new one?</param>
        /// <param name="nextData">Next cluster of sectors is all data</param>
        /// <param name="offsetBytes">Read offset</param>
        /// <param name="read6">Device supports READ(6)</param>
        /// <param name="read10">Device supports READ(10)</param>
        /// <param name="read12">Device supports READ(12)</param>
        /// <param name="read16">Device supports READ(16)</param>
        /// <param name="readcd">Device supports READ CD</param>
        /// <param name="sectorsForOffset">Sectors needed to fix offset</param>
        /// <param name="subSize">Subchannel size in bytes</param>
        /// <param name="supportedSubchannel">Drive's maximum supported subchannel</param>
        /// <param name="supportsLongSectors">Supports reading EDC and ECC</param>
        /// <param name="totalDuration">Total commands duration</param>
        void ReadCdData(ExtentsULong audioExtents, ulong blocks, uint blockSize, ref double currentSpeed,
                        DumpHardwareType currentTry, ExtentsULong extents, IbgLog ibgLog, ref double imageWriteDuration,
                        long lastSector, ExtentsULong leadOutExtents, ref double maxSpeed, MhddLog mhddLog,
                        ref double minSpeed, out bool newTrim, bool nextData, int offsetBytes, bool read6, bool read10,
                        bool read12, bool read16, bool readcd, int sectorsForOffset, uint subSize,
                        MmcSubchannel supportedSubchannel, bool supportsLongSectors, ref double totalDuration)
        {
            ulong      sectorSpeedStart = 0;               // Used to calculate correct speed
            DateTime   timeSpeedStart   = DateTime.UtcNow; // Time of start for speed calculation
            uint       blocksToRead     = 0;               // How many sectors to read at once
            bool       sense            = true;            // Sense indicator
            byte[]     cmdBuf           = null;            // Data buffer
            byte[]     senseBuf         = null;            // Sense buffer
            double     cmdDuration      = 0;               // Command execution time
            const uint sectorSize       = 2352;            // Full sector size
            byte[]     tmpBuf;                             // Temporary buffer
            newTrim = false;

            InitProgress?.Invoke();

            for(ulong i = _resume.NextBlock; (long)i <= lastSector; i += blocksToRead)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                while(leadOutExtents.Contains(i))
                {
                    i++;
                }

                if((long)i > lastSector)
                    break;

                uint firstSectorToRead = (uint)i;

                if((lastSector + 1) - (long)i < _maximumReadable)
                    _maximumReadable = (uint)((lastSector + 1) - (long)i);

                blocksToRead = 0;
                bool inData = nextData;

                for(ulong j = i; j < i + _maximumReadable; j++)
                {
                    if(nextData)
                    {
                        if(audioExtents.Contains(j))
                        {
                            nextData = false;

                            break;
                        }

                        blocksToRead++;
                    }
                    else
                    {
                        if(!audioExtents.Contains(j))
                        {
                            nextData = true;

                            break;
                        }

                        blocksToRead++;
                    }
                }

                if(blocksToRead == 1)
                    blocksToRead = 2;

                if(_fixOffset && !inData)
                {
                    // TODO: FreeBSD bug
                    if(offsetBytes < 0)
                    {
                        if(i == 0)
                            firstSectorToRead = uint.MaxValue - (uint)(sectorsForOffset - 1); // -1
                        else
                            firstSectorToRead -= (uint)sectorsForOffset;
                    }
                }

                #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

                // ReSharper disable CompareOfFloatsByEqualityOperator
                if(currentSpeed > maxSpeed &&
                   currentSpeed != 0)
                    maxSpeed = currentSpeed;

                if(currentSpeed < minSpeed &&
                   currentSpeed != 0)
                    minSpeed = currentSpeed;

                // ReSharper restore CompareOfFloatsByEqualityOperator

                #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                UpdateProgress?.Invoke($"Reading sector {i} of {blocks} ({currentSpeed:F3} MiB/sec.)", (long)i,
                                       (long)blocks);

                if(readcd)
                {
                    sense = _dev.ReadCd(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, blocksToRead,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;
                }
                else if(read16)
                {
                    sense = _dev.Read16(out cmdBuf, out senseBuf, 0, false, true, false, firstSectorToRead, blockSize,
                                        0, blocksToRead, false, _dev.Timeout, out cmdDuration);
                }
                else if(read12)
                {
                    sense = _dev.Read12(out cmdBuf, out senseBuf, 0, false, true, false, false, firstSectorToRead,
                                        blockSize, 0, blocksToRead, false, _dev.Timeout, out cmdDuration);
                }
                else if(read10)
                {
                    sense = _dev.Read10(out cmdBuf, out senseBuf, 0, false, true, false, false, firstSectorToRead,
                                        blockSize, 0, (ushort)blocksToRead, _dev.Timeout, out cmdDuration);
                }
                else if(read6)
                {
                    sense = _dev.Read6(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, (byte)blocksToRead,
                                       _dev.Timeout, out cmdDuration);
                }

                // Because one block has been partially used to fix the offset
                if(_fixOffset &&
                   !inData    &&
                   offsetBytes != 0)
                {
                    int offsetFix = offsetBytes < 0 ? (int)(sectorSize - (offsetBytes * -1)) : offsetBytes;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        // De-interleave subchannel
                        byte[] data = new byte[sectorSize * blocksToRead];
                        byte[] sub  = new byte[subSize    * blocksToRead];

                        for(int b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0          + (b * blockSize)), data, sectorSize * b, sectorSize);
                            Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize     * b, subSize);
                        }

                        tmpBuf = new byte[sectorSize * (blocksToRead - sectorsForOffset)];
                        Array.Copy(data, offsetFix, tmpBuf, 0, tmpBuf.Length);
                        data = tmpBuf;

                        blocksToRead -= (uint)sectorsForOffset;

                        // Re-interleave subchannel
                        cmdBuf = new byte[blockSize * blocksToRead];

                        for(int b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(data, sectorSize * b, cmdBuf, (int)(0          + (b * blockSize)), sectorSize);
                            Array.Copy(sub, subSize     * b, cmdBuf, (int)(sectorSize + (b * blockSize)), subSize);
                        }
                    }
                    else
                    {
                        tmpBuf = new byte[blockSize * (blocksToRead - sectorsForOffset)];
                        Array.Copy(cmdBuf, offsetFix, tmpBuf, 0, tmpBuf.Length);
                        cmdBuf       =  tmpBuf;
                        blocksToRead -= (uint)sectorsForOffset;
                    }
                }

                if(!sense &&
                   !_dev.Error)
                {
                    mhddLog.Write(i, cmdDuration);
                    ibgLog.Write(i, currentSpeed * 1024);
                    extents.Add(i, blocksToRead, true);
                    DateTime writeStart = DateTime.Now;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[sectorSize * blocksToRead];
                        byte[] sub  = new byte[subSize    * blocksToRead];

                        for(int b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0 + (b * blockSize)), data, sectorSize * b, sectorSize);

                            Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize * b, subSize);
                        }

                        _outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                        _outputPlugin.WriteSectorsTag(sub, i, blocksToRead, SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorsLong(cmdBuf, i, blocksToRead);
                        }
                        else
                        {
                            if(cmdBuf.Length % sectorSize == 0)
                            {
                                byte[] data = new byte[2048 * blocksToRead];

                                for(int b = 0; b < blocksToRead; b++)
                                    Array.Copy(cmdBuf, (int)(16 + (b * blockSize)), data, 2048 * b, 2048);

                                _outputPlugin.WriteSectors(data, i, blocksToRead);
                            }
                            else
                            {
                                _outputPlugin.WriteSectorsLong(cmdBuf, i, blocksToRead);
                            }
                        }
                    }

                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                }
                else
                {
                    // TODO: Reset device after X errors
                    if(_stopOnError)
                        return; // TODO: Return more cleanly

                    if(i + _skip > blocks)
                        _skip = (uint)(blocks - i);

                    // Write empty data
                    DateTime writeStart = DateTime.Now;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        _outputPlugin.WriteSectorsLong(new byte[sectorSize * _skip], i, _skip);

                        _outputPlugin.WriteSectorsTag(new byte[subSize * _skip], i, _skip,
                                                      SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors)
                        {
                            _outputPlugin.WriteSectorsLong(new byte[blockSize * _skip], i, _skip);
                        }
                        else
                        {
                            if(cmdBuf.Length % sectorSize == 0)
                                _outputPlugin.WriteSectors(new byte[2048 * _skip], i, _skip);
                            else
                                _outputPlugin.WriteSectorsLong(new byte[blockSize * _skip], i, _skip);
                        }
                    }

                    imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                    for(ulong b = i; b < i + _skip; b++)
                        _resume.BadBlocks.Add(b);

                    DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Sense.PrettifySense(senseBuf));
                    mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                    ibgLog.Write(i, 0);
                    _dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", _skip, i);
                    i       += _skip - blocksToRead;
                    newTrim =  true;
                }

                sectorSpeedStart += blocksToRead;

                _resume.NextBlock = i + blocksToRead;

                double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

                if(elapsed < 1)
                    continue;

                currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            EndProgress?.Invoke();
        }
    }
}