// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;
using Schemas;
using System.Linq;
using DiscImageChef.Decoders.CD;
using Extents;

namespace DiscImageChef.Core.Devices.Dumping
{
    using ImagePlugins;
    using Metadata;
    using MediaType = CommonTypes.MediaType;
    using Session = Decoders.CD.Session;
    using TrackType = Schemas.TrackType;

    internal class CompactDisc
    {
        // TODO: Add support for resume file
        internal static void Dump(Device dev, string devicePath, string outputPrefix, ushort retryPasses, bool force, bool dumpRaw, bool persistent, bool stopOnError, ref CICMMetadataType sidecar, ref MediaType dskType, bool separateSubchannel, ref Resume resume, ref DumpLog dumpLog, Alcohol120 alcohol)
        {
            MHDDLog mhddLog;
            IBGLog ibgLog;
            bool sense = false;
            ulong blocks = 0;
            // TODO: Check subchannel support
            uint blockSize = 0;
            uint subSize = 0;
            byte[] tmpBuf;
            FullTOC.CDFullTOC? toc = null;
            DateTime start;
            DateTime end;
            double totalDuration = 0;
            double totalChkDuration = 0;
            double currentSpeed = 0;
            double maxSpeed = double.MinValue;
            double minSpeed = double.MaxValue;
            Checksum dataChk;
            bool readcd = false;
            byte[] readBuffer;
            uint blocksToRead = 64;
            ulong errored = 0;
            DataFile dumpFile = null;
            bool aborted = false;
            System.Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = aborted = true;
            };

            // We discarded all discs that falsify a TOC before requesting a real TOC
            // No TOC, no CD (or an empty one)
            dumpLog.WriteLine("Reading full TOC");
            bool tocSense = dev.ReadRawToc(out byte[] cmdBuf, out byte[] senseBuf, 1, dev.Timeout, out double duration);
            if(!tocSense)
            {
                toc = FullTOC.Decode(cmdBuf);
                if(toc.HasValue)
                {
                    tmpBuf = new byte[cmdBuf.Length - 2];
                    Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                    sidecar.OpticalDisc[0].TOC = new DumpType
                    {
                        Image = outputPrefix + ".toc.bin",
                        Size = tmpBuf.Length,
                        Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                    };
                    DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].TOC.Image, tmpBuf);

                    // ATIP exists on blank CDs
                    dumpLog.WriteLine("Reading ATIP");
                    sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                    if(!sense)
                    {
                        ATIP.CDATIP? atip = ATIP.Decode(cmdBuf);
                        if(atip.HasValue)
                        {
                            // Only CD-R and CD-RW have ATIP
                            dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;

                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].ATIP = new DumpType
                            {
                                Image = outputPrefix + ".atip.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].TOC.Image, tmpBuf);
                        }
                    }

                    dumpLog.WriteLine("Reading Disc Information");
                    sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf, MmcDiscInformationDataTypes.DiscInformation, dev.Timeout, out duration);
                    if(!sense)
                    {
                        Decoders.SCSI.MMC.DiscInformation.StandardDiscInformation? discInfo = Decoders.SCSI.MMC.DiscInformation.Decode000b(cmdBuf);
                        if(discInfo.HasValue)
                        {
                            // If it is a read-only CD, check CD type if available
                            if(dskType == MediaType.CD)
                            {
                                switch(discInfo.Value.DiscType)
                                {
                                    case 0x10:
                                        dskType = MediaType.CDI;
                                        break;
                                    case 0x20:
                                        dskType = MediaType.CDROMXA;
                                        break;
                                }
                            }
                        }
                    }

                    int sessions = 1;
                    int firstTrackLastSession = 0;

                    dumpLog.WriteLine("Reading Session Information");
                    sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                    if(!sense)
                    {
                        Session.CDSessionInfo? session = Session.Decode(cmdBuf);
                        if(session.HasValue)
                        {
                            sessions = session.Value.LastCompleteSession;
                            firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
                        }
                    }

                    if(dskType == MediaType.CD)
                    {
                        bool hasDataTrack = false;
                        bool hasAudioTrack = false;
                        bool allFirstSessionTracksAreAudio = true;
                        bool hasVideoTrack = false;

                        if(toc.HasValue)
                        {
                            foreach(FullTOC.TrackDataDescriptor track in toc.Value.TrackDescriptors)
                            {
                                if(track.TNO == 1 &&
                                    ((TOC_CONTROL)(track.CONTROL & 0x0D) == TOC_CONTROL.DataTrack ||
                                    (TOC_CONTROL)(track.CONTROL & 0x0D) == TOC_CONTROL.DataTrackIncremental))
                                {
                                    allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;
                                }

                                if((TOC_CONTROL)(track.CONTROL & 0x0D) == TOC_CONTROL.DataTrack ||
                                    (TOC_CONTROL)(track.CONTROL & 0x0D) == TOC_CONTROL.DataTrackIncremental)
                                {
                                    hasDataTrack = true;
                                    allFirstSessionTracksAreAudio &= track.TNO >= firstTrackLastSession;
                                }
                                else
                                    hasAudioTrack = true;

                                hasVideoTrack |= track.ADR == 4;
                            }
                        }

                        if(hasDataTrack && hasAudioTrack && allFirstSessionTracksAreAudio && sessions == 2)
                            dskType = MediaType.CDPLUS;
                        if(!hasDataTrack && hasAudioTrack && sessions == 1)
                            dskType = MediaType.CDDA;
                        if(hasDataTrack && !hasAudioTrack && sessions == 1)
                            dskType = MediaType.CDROM;
                        if(hasVideoTrack && !hasDataTrack && sessions == 1)
                            dskType = MediaType.CDV;
                    }

                    dumpLog.WriteLine("Reading PMA");
                    sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                    if(!sense)
                    {
                        if(PMA.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].PMA = new DumpType
                            {
                                Image = outputPrefix + ".pma.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].PMA.Image, tmpBuf);
                        }
                    }

                    dumpLog.WriteLine("Reading CD-Text from Lead-In");
                    sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out duration);
                    if(!sense)
                    {
                        if(CDTextOnLeadIn.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            sidecar.OpticalDisc[0].LeadInCdText = new DumpType
                            {
                                Image = outputPrefix + ".cdtext.bin",
                                Size = tmpBuf.Length,
                                Checksums = Checksum.GetChecksums(tmpBuf).ToArray()
                            };
                            DataFile.WriteTo("SCSI Dump", sidecar.OpticalDisc[0].LeadInCdText.Image, tmpBuf);
                        }
                    }
                }
            }

            // TODO: Support variable subchannel kinds
            blockSize = 2448;
            subSize = 96;
            int sectorSize;
            if(separateSubchannel)
                sectorSize = (int)(blockSize - subSize);
            else
                sectorSize = (int)blockSize;

            if(toc == null)
            {
                DicConsole.ErrorWriteLine("Error trying to decode TOC...");
                return;
            }

            ImagePlugins.Session[] sessionsForAlcohol = new ImagePlugins.Session[toc.Value.LastCompleteSession];
            for(int i = 0; i < sessionsForAlcohol.Length; i++)
            {
                sessionsForAlcohol[i].SessionSequence = (ushort)(i + 1);
                sessionsForAlcohol[i].StartTrack = ushort.MaxValue;
            }
            foreach(FullTOC.TrackDataDescriptor trk in toc.Value.TrackDescriptors)
            {
                if(trk.POINT > 0 && trk.POINT < 0xA0 && trk.SessionNumber <= sessionsForAlcohol.Length)
                {
                    if(trk.POINT < sessionsForAlcohol[trk.SessionNumber - 1].StartTrack)
                        sessionsForAlcohol[trk.SessionNumber - 1].StartTrack = trk.POINT;
                    if(trk.POINT > sessionsForAlcohol[trk.SessionNumber - 1].EndTrack)
                        sessionsForAlcohol[trk.SessionNumber - 1].EndTrack = trk.POINT;
                }
            }
            alcohol.AddSessions(sessionsForAlcohol);

            foreach(FullTOC.TrackDataDescriptor trk in toc.Value.TrackDescriptors)
            {
                alcohol.AddTrack((byte)((trk.ADR << 4) & trk.CONTROL), trk.TNO, trk.POINT, trk.Min, trk.Sec, trk.Frame,
                    trk.Zero, trk.PMIN, trk.PSEC, trk.PFRAME, trk.SessionNumber);
            }

            FullTOC.TrackDataDescriptor[] sortedTracks = toc.Value.TrackDescriptors.OrderBy(track => track.POINT).ToArray();
            List<TrackType> trackList = new List<TrackType>();
            long lastSector = 0;
            string lastMSF = null;
            foreach(FullTOC.TrackDataDescriptor trk in sortedTracks)
            {
                if(trk.ADR == 1 || trk.ADR == 4)
                {
                    if(trk.POINT >= 0x01 && trk.POINT <= 0x63)
                    {
                        TrackType track = new TrackType
                        {
                            Sequence = new TrackSequenceType
                            {
                                Session = trk.SessionNumber,
                                TrackNumber = trk.POINT
                            }
                        };
                        if((TOC_CONTROL)(trk.CONTROL & 0x0D) == TOC_CONTROL.DataTrack ||
                                                           (TOC_CONTROL)(trk.CONTROL & 0x0D) == TOC_CONTROL.DataTrackIncremental)
                            track.TrackType1 = TrackTypeTrackType.mode1;
                        else
                            track.TrackType1 = TrackTypeTrackType.audio;
                        if(trk.PHOUR > 0)
                            track.StartMSF = string.Format("{3:D2}:{0:D2}:{1:D2}:{2:D2}", trk.PMIN, trk.PSEC, trk.PFRAME, trk.PHOUR);
                        else
                            track.StartMSF = string.Format("{0:D2}:{1:D2}:{2:D2}", trk.PMIN, trk.PSEC, trk.PFRAME);
                        track.StartSector = trk.PHOUR * 3600 * 75 + trk.PMIN * 60 * 75 + trk.PSEC * 75 + trk.PFRAME - 150;
                        trackList.Add(track);
                    }
                    else if(trk.POINT == 0xA2)
                    {
                        int phour, pmin, psec, pframe;
                        if(trk.PFRAME == 0)
                        {
                            pframe = 74;

                            if(trk.PSEC == 0)
                            {
                                psec = 59;

                                if(trk.PMIN == 0)
                                {
                                    pmin = 59;
                                    phour = trk.PHOUR - 1;
                                }
                                else
                                {
                                    pmin = trk.PMIN - 1;
                                    phour = trk.PHOUR;
                                }
                            }
                            else
                            {
                                psec = trk.PSEC - 1;
                                pmin = trk.PMIN;
                                phour = trk.PHOUR;
                            }
                        }
                        else
                        {
                            pframe = trk.PFRAME - 1;
                            psec = trk.PSEC;
                            pmin = trk.PMIN;
                            phour = trk.PHOUR;
                        }

                        if(phour > 0)
                            lastMSF = string.Format("{3:D2}:{0:D2}:{1:D2}:{2:D2}", pmin, psec, pframe, phour);
                        else
                            lastMSF = string.Format("{0:D2}:{1:D2}:{2:D2}", pmin, psec, pframe);
                        lastSector = phour * 3600 * 75 + pmin * 60 * 75 + psec * 75 + pframe - 150;
                    }
                }
            }

            TrackType[] tracks = trackList.ToArray();
            for(int t = 1; t < tracks.Length;t++)
            {
                tracks[t - 1].EndSector = tracks[t].StartSector - 1;
                int phour = 0, pmin = 0, psec = 0;
                int pframe = (int)(tracks[t - 1].EndSector + 150);

                if(pframe > 3600 * 75)
                {
                    phour = pframe / (3600 * 75);
                    pframe -= phour * 3600 * 75;
                }
                if(pframe > 60 * 75)
                {
                    pmin = pframe / (60 * 75);
                    pframe -= pmin * 60 * 75;
                }
                if(pframe > 75)
                {
                    psec = pframe / 75;
                    pframe -= psec * 75;
                }

                if(phour > 0)
                    tracks[t - 1].EndMSF = string.Format("{3:D2}:{0:D2}:{1:D2}:{2:D2}", pmin, psec, pframe, phour);
                else
                    tracks[t - 1].EndMSF = string.Format("{0:D2}:{1:D2}:{2:D2}", pmin, psec, pframe);
            }
            tracks[tracks.Length - 1].EndMSF = lastMSF;
            tracks[tracks.Length - 1].EndSector = lastSector;
            blocks = (ulong)(lastSector + 1);

            if(blocks == 0)
            {
                DicConsole.ErrorWriteLine("Cannot dump blank media.");
                return;
            }

            if(dumpRaw)
            {
                throw new NotImplementedException("Raw CD dumping not yet implemented");
            }
            else
            {
                // TODO: Check subchannel capabilities
                readcd = !dev.ReadCd(out readBuffer, out senseBuf, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                    true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out duration);

                if(readcd)
                    DicConsole.WriteLine("Using MMC READ CD command.");
            }

            DumpHardwareType currentTry = null;
            ExtentsULong extents = null;
            ResumeSupport.Process(true, true, blocks, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformID, ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
                throw new Exception("Could not process resume file, not continuing...");

            DicConsole.WriteLine("Trying to read Lead-In...");
            bool gotLeadIn = false;
            int leadInSectorsGood = 0, leadInSectorsTotal = 0;

            dumpFile = new DataFile(outputPrefix + ".leadin.bin");
            dataChk = new Checksum();

            start = DateTime.UtcNow;

            readBuffer = null;

            dumpLog.WriteLine("Reading Lead-in");
            for(int leadInBlock = -150; leadInBlock < 0 && resume.NextBlock == 0; leadInBlock++)
            {
                if(aborted)
                {
                    dumpLog.WriteLine("Aborted!");
                    break;
                }

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if(currentSpeed > maxSpeed && currentSpeed != 0)
                    maxSpeed = currentSpeed;
                if(currentSpeed < minSpeed && currentSpeed != 0)
                    minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                DicConsole.Write("\rTrying to read lead-in sector {0} ({1:F3} MiB/sec.)", leadInBlock, currentSpeed);

                sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)leadInBlock, blockSize, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                    true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out double cmdDuration);

                if(!sense && !dev.Error)
                {
                    dataChk.Update(readBuffer);
                    dumpFile.Write(readBuffer);
                    gotLeadIn = true;
                    leadInSectorsGood++;
                    leadInSectorsTotal++;
                }
                else
                {
                    if(gotLeadIn)
                    {
                        // Write empty data
                        dataChk.Update(new byte[blockSize]);
                        dumpFile.Write(new byte[blockSize]);
                        leadInSectorsTotal++;
                    }
                }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                currentSpeed = ((double)blockSize / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
            }

            dumpFile.Close();
            if(leadInSectorsGood > 0)
            {
                sidecar.OpticalDisc[0].LeadIn = new BorderType[]
                {
	                new BorderType
	                {
	                    Image = outputPrefix + ".leadin.bin",
	                    Checksums = dataChk.End().ToArray(),
	                    Size = leadInSectorsTotal * blockSize
	                }
                };
            }
            else
                File.Delete(outputPrefix + ".leadin.bin");

            DicConsole.WriteLine();
            DicConsole.WriteLine("Got {0} lead-in sectors.", leadInSectorsGood);
            dumpLog.WriteLine("Got {0} Lead-in sectors.", leadInSectorsGood);

            while(true)
            {
                if(readcd)
                {
                    sense = dev.ReadCd(out readBuffer, out senseBuf, 0, blockSize, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                        true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out duration);
                    if(dev.Error || sense)
                        blocksToRead /= 2;
                }

                if(!dev.Error || blocksToRead == 1)
                    break;
            }

            if(dev.Error || sense)
            {
                DicConsole.WriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                DicConsole.ErrorWriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                return;
            }

            DicConsole.WriteLine("Reading {0} sectors at a time.", blocksToRead);

            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).", blocks, blocks * blockSize);
            dumpLog.WriteLine("Device can read {0} blocks at a time.", blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
            dumpLog.WriteLine("SCSI device type: {0}.", dev.SCSIType);
            dumpLog.WriteLine("Media identified as {0}.", dskType);
            alcohol.SetMediaType(dskType);

            dumpFile = new DataFile(outputPrefix + ".bin");
            alcohol.SetExtension(".bin");
            DataFile subFile = null;
            if(separateSubchannel)
                subFile = new DataFile(outputPrefix + ".sub");
            mhddLog = new MHDDLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            ibgLog = new IBGLog(outputPrefix + ".ibg", 0x0008);

            dumpFile.Seek(resume.NextBlock, (ulong)sectorSize);
            if(separateSubchannel)
                subFile.Seek(resume.NextBlock, subSize);

            if(resume.NextBlock > 0)
                dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);

            start = DateTime.UtcNow;
            for(int t = 0; t < tracks.Count(); t++)
            {
                dumpLog.WriteLine("Reading track {0}", t);

                tracks[t].BytesPerSector = sectorSize;
                tracks[t].Image = new ImageType
                {
                    format = "BINARY",
                    offset = dumpFile.Position,
                    offsetSpecified = true,
                    Value = outputPrefix + ".bin"
                };
                tracks[t].Size = (tracks[t].EndSector - tracks[t].StartSector + 1) * sectorSize;
                tracks[t].SubChannel = new SubChannelType
                {
                    Image = new ImageType
                    {
                        format = "rw_raw",
                        offsetSpecified = true
                    },
                    Size = (tracks[t].EndSector - tracks[t].StartSector + 1) * subSize
                };
                if(separateSubchannel)
                {
                    tracks[t].SubChannel.Image.offset = subFile.Position;
                    tracks[t].SubChannel.Image.Value = outputPrefix + ".sub";
                }
                else
                {
                    tracks[t].SubChannel.Image.offset = tracks[t].Image.offset;
                    tracks[t].SubChannel.Image.Value = tracks[t].Image.Value;
                }

                alcohol.SetTrackSizes((byte)(t + 1), sectorSize, tracks[t].StartSector, dumpFile.Position, (tracks[t].EndSector - tracks[t].StartSector + 1));
                
                bool checkedDataFormat = false;

                for(ulong i = resume.NextBlock; i <= (ulong)tracks[t].EndSector; i += blocksToRead)
                {
                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    double cmdDuration = 0;

                    if(((ulong)tracks[t].EndSector + 1 - i) < blocksToRead)
                        blocksToRead = (uint)((ulong)tracks[t].EndSector + 1 - i);

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0)
                        maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0)
                        minSpeed = currentSpeed;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    DicConsole.Write("\rReading sector {0} of {1} at track {3} ({2:F3} MiB/sec.)", i, blocks, currentSpeed, t + 1);

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, blockSize, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        extents.Add(i, blocksToRead, true);
                        if(separateSubchannel)
                        {
                            for(int b = 0; b < blocksToRead; b++)
                            {
                                dumpFile.Write(readBuffer, (int)(0 + b * blockSize), sectorSize);
                                subFile.Write(readBuffer, (int)(sectorSize + b * blockSize), (int)subSize);
                            }
                        }
                        else
                            dumpFile.Write(readBuffer);
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError)
                            return; // TODO: Return more cleanly

                        // Write empty data
                        if(separateSubchannel)
                        {
                            dumpFile.Write(new byte[sectorSize * blocksToRead]);
                            subFile.Write(new byte[subSize * blocksToRead]);
                        }
                        else
                            dumpFile.Write(new byte[blockSize * blocksToRead]);

                        errored += blocksToRead;
                        for(ulong b = i; b < i + blocksToRead; b++)
                            resume.BadBlocks.Add(b);
                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Decoders.SCSI.Sense.PrettifySense(senseBuf));
                        if(cmdDuration < 500)
                            mhddLog.Write(i, 65535);
                        else
                            mhddLog.Write(i, cmdDuration);

                        ibgLog.Write(i, 0);
                        dumpLog.WriteLine("Error reading {0} sectors from sector {1}.", blocksToRead, i);
                    }

                    if(tracks[t].TrackType1 == TrackTypeTrackType.mode1 && !checkedDataFormat)
                    {
                        byte[] sync = new byte[12];
                        Array.Copy(readBuffer, 0, sync, 0, 12);
                        if(sync.SequenceEqual(new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 }))
                        {
                            switch(readBuffer[15])
                            {
                                case 0:
                                    tracks[t].TrackType1 = TrackTypeTrackType.mode0;
                                    checkedDataFormat = true;
                                    break;
                                case 1:
                                    tracks[t].TrackType1 = TrackTypeTrackType.mode1;
                                    checkedDataFormat = true;
                                    break;
                                case 2:
                                    tracks[t].TrackType1 = TrackTypeTrackType.mode2;
                                    checkedDataFormat = true;
                                    break;
                            }
                        }
                    }

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (cmdDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                    resume.NextBlock = i + blocksToRead;
                }

                ImagePlugins.TrackType trkType;
                switch(tracks[t].TrackType1)
                {
                    case TrackTypeTrackType.audio:
                        trkType = ImagePlugins.TrackType.Audio;
                        break;
                    case TrackTypeTrackType.mode1:
                        trkType = ImagePlugins.TrackType.CDMode1;
                        break;
                    case TrackTypeTrackType.mode2:
                        trkType = ImagePlugins.TrackType.CDMode2Formless;
                        break;
                    case TrackTypeTrackType.m2f1:
                        trkType = ImagePlugins.TrackType.CDMode2Form1;
                        break;
                    case TrackTypeTrackType.m2f2:
                        trkType = ImagePlugins.TrackType.CDMode2Form2;
                        break;
                    case TrackTypeTrackType.dvd:
                    case TrackTypeTrackType.hddvd:
                    case TrackTypeTrackType.bluray:
                    case TrackTypeTrackType.ddcd:
                    case TrackTypeTrackType.mode0:
                        trkType = ImagePlugins.TrackType.Data;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                alcohol.SetTrackTypes((byte)(t + 1), trkType,
                    separateSubchannel ? TrackSubchannelType.None : TrackSubchannelType.RawInterleaved);
            }
            DicConsole.WriteLine();
            end = DateTime.UtcNow;
            mhddLog.Close();
#pragma warning disable IDE0004 // Remove Unnecessary Cast
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024, (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000), devicePath);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalDuration / 1000));

            #region Compact Disc Error handling
            if(resume.BadBlocks.Count > 0 && !aborted)
            {
                int pass = 0;
                bool forward = true;
                bool runningPersistent = false;

            cdRepeatRetry:
                ulong[] tmpArray = resume.BadBlocks.ToArray();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    double cmdDuration = 0;

                    DicConsole.Write("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass + 1, forward ? "forward" : "reverse", runningPersistent ? "recovering partial data, " : "");

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)badSector, blockSize, blocksToRead, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                            true, true, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }

                    if((!sense && !dev.Error) || runningPersistent)
                    {
                        if(!sense && !dev.Error)
                        {
                            resume.BadBlocks.Remove(badSector);
                            extents.Add(badSector);
                            dumpLog.WriteLine("Correctly retried sector {0} in pass {1}.", badSector, pass);
                        }
                        
                        if(separateSubchannel)
                        {
                            dumpFile.WriteAt(readBuffer, badSector, (uint)sectorSize, 0, sectorSize);
                            subFile.WriteAt(readBuffer, badSector, subSize, sectorSize, (int)subSize);
                        }
                        else
                            dumpFile.WriteAt(readBuffer, badSector, blockSize);
                    }
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto cdRepeatRetry;
                }

                Decoders.SCSI.Modes.DecodedMode? currentMode = null;
                Decoders.SCSI.Modes.ModePage? currentModePage = null;
                byte[] md6 = null;
                byte[] md10 = null;

                if(!runningPersistent && persistent)
                {
                    sense = dev.ModeSense6(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current, 0x01, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSense10(out readBuffer, out senseBuf, false, ScsiModeSensePageControl.Current, 0x01, dev.Timeout, out duration);
                        if(!sense)
                            currentMode = Decoders.SCSI.Modes.DecodeMode10(readBuffer, dev.SCSIType);
                    }
                    else
                        currentMode = Decoders.SCSI.Modes.DecodeMode6(readBuffer, dev.SCSIType);

                    if(currentMode.HasValue)
                        currentModePage = currentMode.Value.Pages[0];

                    Decoders.SCSI.Modes.ModePage_01_MMC pgMMC = new Decoders.SCSI.Modes.ModePage_01_MMC
                    {
                        PS = false,
                        ReadRetryCount = 255,
                        Parameter = 0x20
                    };
                    Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode
                    {
                        Header = new Decoders.SCSI.Modes.ModeHeader(),
                        Pages = new Decoders.SCSI.Modes.ModePage[]
	                    {
	                        new Decoders.SCSI.Modes.ModePage
	                        {
	                            Page = 0x01,
	                            Subpage = 0x00,
	                            PageResponse = Decoders.SCSI.Modes.EncodeModePage_01_MMC(pgMMC)
	                        }
	                    }
                    };
                    md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                    md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                    }

                    runningPersistent = true;
                    if(!sense && !dev.Error)
                    {
                        pass--;
                        goto cdRepeatRetry;
                    }
                }
                else if(runningPersistent && persistent && currentModePage.HasValue)
                {
                    Decoders.SCSI.Modes.DecodedMode md = new Decoders.SCSI.Modes.DecodedMode
                    {
                        Header = new Decoders.SCSI.Modes.ModeHeader(),
                        Pages = new Decoders.SCSI.Modes.ModePage[]
                    {
                        currentModePage.Value
                    }
                    };
                    md6 = Decoders.SCSI.Modes.EncodeMode6(md, dev.SCSIType);
                    md10 = Decoders.SCSI.Modes.EncodeMode10(md, dev.SCSIType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive.");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out duration);
                    if(sense)
                    {
                        sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out duration);
                    }
                }

                DicConsole.WriteLine();
            }
            #endregion Compact Disc Error handling
            resume.BadBlocks.Sort();
            currentTry.Extents = Metadata.ExtentsConverter.ToMetadata(extents);

            dataChk = new Checksum();
            dumpFile.Seek(0, SeekOrigin.Begin);
            if(separateSubchannel)
                subFile.Seek(0, SeekOrigin.Begin);
            blocksToRead = 500;

            dumpLog.WriteLine("Checksum starts.");
            for(int t = 0; t < tracks.Count(); t++)
            {
                Checksum trkChk = new Checksum();
                Checksum subChk = new Checksum();

                for(ulong i = (ulong)tracks[t].StartSector; i <= (ulong)tracks[t].EndSector; i += blocksToRead)
                {
                    if(aborted)
                    {
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    if(((ulong)tracks[t].EndSector + 1 - i) < blocksToRead)
                        blocksToRead = (uint)((ulong)tracks[t].EndSector + 1 - i);

                    DicConsole.Write("\rChecksumming sector {0} of {1} at track {3} ({2:F3} MiB/sec.)", i, blocks, currentSpeed, t + 1);

                    DateTime chkStart = DateTime.UtcNow;
                    byte[] dataToCheck = new byte[blockSize * blocksToRead];
                    dumpFile.Read(dataToCheck, 0, (int)(blockSize * blocksToRead));
                    if(separateSubchannel)
                    {
                        byte[] data = new byte[sectorSize];
                        byte[] sub = new byte[subSize];
                        for(int b = 0; b < blocksToRead; b++)
                        {
                            Array.Copy(dataToCheck, 0, data, 0, sectorSize);
                            Array.Copy(dataToCheck, sectorSize, sub, 0, subSize);
                            dataChk.Update(data);
                            trkChk.Update(data);
                            subChk.Update(sub);
                        }
                    }
                    else
                    {
                        dataChk.Update(dataToCheck);
                        trkChk.Update(dataToCheck);
                    }
                    DateTime chkEnd = DateTime.UtcNow;

                    double chkDuration = (chkEnd - chkStart).TotalMilliseconds;
                    totalChkDuration += chkDuration;

#pragma warning disable IDE0004 // Remove Unnecessary Cast
                    currentSpeed = ((double)blockSize * blocksToRead / (double)1048576) / (chkDuration / (double)1000);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
                }

                tracks[t].Checksums = trkChk.End().ToArray();
                if(separateSubchannel)
                    tracks[t].SubChannel.Checksums = subChk.End().ToArray();
                else
                    tracks[t].SubChannel.Checksums = tracks[t].Checksums;
            }
            DicConsole.WriteLine();
            dumpFile.Close();
            end = DateTime.UtcNow;
            dumpLog.WriteLine("Checksum finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.", (((double)blockSize * (double)(blocks + 1)) / 1024) / (totalChkDuration / 1000));

            // TODO: Correct this
            sidecar.OpticalDisc[0].Checksums = dataChk.End().ToArray();
            sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();
            sidecar.OpticalDisc[0].Image = new ImageType
            {
                format = "Raw disk image (sector by sector copy)",
                Value = outputPrefix + ".bin"
            };
            sidecar.OpticalDisc[0].Sessions = toc.Value.LastCompleteSession;
            sidecar.OpticalDisc[0].Tracks = new[] { tracks.Count() };
            sidecar.OpticalDisc[0].Track = tracks;
            sidecar.OpticalDisc[0].Dimensions = Metadata.Dimensions.DimensionsFromMediaType(dskType);
            Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp, out string xmlDskSubTyp);
            sidecar.OpticalDisc[0].DiscType = xmlDskTyp;
            sidecar.OpticalDisc[0].DiscSubType = xmlDskSubTyp;

            if(!aborted)
            {
                DicConsole.WriteLine("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml",
                                       FileMode.Create);

                System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
                alcohol.Close();
            }

            Statistics.AddMedia(dskType, true);
        }
    }
}
