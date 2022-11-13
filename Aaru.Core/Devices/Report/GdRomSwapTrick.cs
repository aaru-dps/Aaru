// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable InlineOutVariableDeclaration

namespace Aaru.Core.Devices.Report;

using System;
using System.Linq;
using System.Threading;
using Aaru.CommonTypes.Metadata;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;

public sealed partial class DeviceReport
{
    /// <summary>Tries and checks reading a GD-ROM disc using the swap disc trick and adds the result to a device report</summary>
    /// <param name="report">Device report</param>
    public void ReportGdRomSwapTrick(ref DeviceReportV2 report)
    {
        report.GdRomSwapDiscCapabilities = new GdRomSwapDiscCapabilities();

        var pressedKey = new ConsoleKeyInfo();

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("Have you previously tried with a GD-ROM disc and did the computer hang or crash? (Y/N): ");

            pressedKey = Console.ReadKey();
            AaruConsole.WriteLine();
        }

        if(pressedKey.Key == ConsoleKey.Y)
        {
            report.GdRomSwapDiscCapabilities.TestCrashed = true;

            return;
        }

        AaruConsole.WriteLine("Ejecting disc...");

        _dev.AllowMediumRemoval(out _, _dev.Timeout, out _);
        _dev.EjectTray(out _, _dev.Timeout, out _);

        AaruConsole.WriteLine("Please insert trap disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        Console.ReadLine();

        AaruConsole.WriteLine("Sending READ FULL TOC to the device...");

        var    retries = 0;
        bool   sense;
        byte[] buffer;
        byte[] senseBuffer;

        do
        {
            retries++;
            sense = _dev.ScsiTestUnitReady(out senseBuffer, _dev.Timeout, out _);

            if(!sense)
                break;

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);

            if(decodedSense is not { ASC: 0x04, ASCQ: 0x01 })
                break;

            Thread.Sleep(2000);
        } while(retries < 25);

        sense = _dev.ReadRawToc(out buffer, out senseBuffer, 1, _dev.Timeout, out _);

        if(sense)
        {
            AaruConsole.WriteLine("READ FULL TOC failed...");
            AaruConsole.DebugWriteLine("GD-ROM reporter", "{0}", Sense.PrettifySense(senseBuffer));

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        FullTOC.CDFullTOC toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        int min         = 0, sec, frame;
        var tocIsNotBcd = false;

        report.GdRomSwapDiscCapabilities.SwapDiscLeadOutPMIN  = leadOutTrack.PMIN;
        report.GdRomSwapDiscCapabilities.SwapDiscLeadOutPSEC  = leadOutTrack.PSEC;
        report.GdRomSwapDiscCapabilities.SwapDiscLeadOutPFRAM = leadOutTrack.PFRAME;

        switch(leadOutTrack.PMIN)
        {
            case 122:
                tocIsNotBcd = true;

                break;
            case >= 0xA0 when !tocIsNotBcd:
                min               += 90;
                leadOutTrack.PMIN -= 0x90;

                break;
        }

        if(tocIsNotBcd)
        {
            min   = leadOutTrack.PMIN;
            sec   = leadOutTrack.PSEC;
            frame = leadOutTrack.PFRAME;
        }
        else
        {
            min   += (leadOutTrack.PMIN   >> 4) * 10 + (leadOutTrack.PMIN   & 0x0F);
            sec   =  (leadOutTrack.PSEC   >> 4) * 10 + (leadOutTrack.PSEC   & 0x0F);
            frame =  (leadOutTrack.PFRAME >> 4) * 10 + (leadOutTrack.PFRAME & 0x0F);
        }

        int sectors = min * 60 * 75 + sec * 75 + frame - 150;

        AaruConsole.WriteLine("Trap disc shows {0} sectors...", sectors);

        if(sectors < 450000)
        {
            AaruConsole.WriteLine("Trap disc doesn't have enough sectors...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = true;

        AaruConsole.WriteLine("Stopping motor...");

        _dev.StopUnit(out _, _dev.Timeout, out _);

        AaruConsole.WriteLine("Please MANUALLY get the trap disc out and put the GD-ROM disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        Console.ReadLine();

        AaruConsole.WriteLine("Waiting 5 seconds...");
        Thread.Sleep(5000);

        AaruConsole.WriteLine("Sending READ FULL TOC to the device...");

        retries = 0;

        do
        {
            retries++;
            sense = _dev.ReadRawToc(out buffer, out senseBuffer, 1, _dev.Timeout, out _);

            if(!sense)
                break;

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);

            if(decodedSense is not { ASC: 0x04, ASCQ: 0x01 })
                break;
        } while(retries < 25);

        if(sense)
        {
            AaruConsole.WriteLine("READ FULL TOC failed...");
            AaruConsole.DebugWriteLine("GD-ROM reporter", "{0}", Sense.PrettifySense(senseBuffer));

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor newLeadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(newLeadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        if(newLeadOutTrack.PMIN >= 0xA0 &&
           !tocIsNotBcd)
            newLeadOutTrack.PMIN -= 0x90;

        if(newLeadOutTrack.PMIN   != leadOutTrack.PMIN ||
           newLeadOutTrack.PSEC   != leadOutTrack.PSEC ||
           newLeadOutTrack.PFRAME != leadOutTrack.PFRAME)
        {
            AaruConsole.WriteLine("Lead-out has changed, this drive does not support hot swapping discs...");

            report.GdRomSwapDiscCapabilities.RecognizedSwapDisc = false;
            report.GdRomSwapDiscCapabilities.TestCrashed        = false;

            return;
        }

        _dev.SetCdSpeed(out _, RotationalControl.PureCav, 170, 0, _dev.Timeout, out _);

        AaruConsole.Write("Reading LBA 0... ");

        report.GdRomSwapDiscCapabilities.Lba0Readable = !_dev.ReadCd(out byte[] lba0Buffer, out byte[] lba0Sense, 0,
                                                                     2352, 1, MmcSectorTypes.AllTypes, false, false,
                                                                     true, MmcHeaderCodes.AllHeaders, true, true,
                                                                     MmcErrorField.None, MmcSubchannel.None,
                                                                     _dev.Timeout, out _);

        report.GdRomSwapDiscCapabilities.Lba0Data         = lba0Buffer;
        report.GdRomSwapDiscCapabilities.Lba0Sense        = lba0Sense;
        report.GdRomSwapDiscCapabilities.Lba0DecodedSense = Sense.PrettifySense(lba0Sense);

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba0Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 0 as audio (scrambled)... ");

        report.GdRomSwapDiscCapabilities.Lba0ScrambledReadable = !_dev.ReadCd(out byte[] lba0ScrambledBuffer,
                                                                              out byte[] lba0ScrambledSense, 0, 2352, 1,
                                                                              MmcSectorTypes.Cdda, false, false, false,
                                                                              MmcHeaderCodes.None, true, false,
                                                                              MmcErrorField.None, MmcSubchannel.None,
                                                                              _dev.Timeout, out _);

        report.GdRomSwapDiscCapabilities.Lba0ScrambledData         = lba0ScrambledBuffer;
        report.GdRomSwapDiscCapabilities.Lba0ScrambledSense        = lba0ScrambledSense;
        report.GdRomSwapDiscCapabilities.Lba0ScrambledDecodedSense = Sense.PrettifySense(lba0ScrambledSense);

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba0ScrambledReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000 as audio... ");
        uint cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000AudioReadable = !_dev.ReadCd(out byte[] lba100000AudioBuffer,
                                                                          out byte[] lba100000AudioSenseBuffer,
                                                                          100000, 2352, cluster,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.None, MmcSubchannel.None,
                                                                          _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba100000AudioData  = lba100000AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000AudioSense = lba100000AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba100000AudioDecodedSense =
                Sense.PrettifySense(lba100000AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba100000AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000 as audio... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000AudioReadable = !_dev.ReadCd(out byte[] lba50000AudioBuffer,
                                                                         out byte[] lba50000AudioSenseBuffer,
                                                                         50000, 2352, cluster, MmcSectorTypes.Cdda,
                                                                         false, false, false, MmcHeaderCodes.None,
                                                                         true, false, MmcErrorField.None,
                                                                         MmcSubchannel.None, _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000AudioData  = lba50000AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000AudioSense = lba50000AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba50000AudioDecodedSense = Sense.PrettifySense(lba50000AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba50000AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000 as audio... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000AudioReadable = !_dev.ReadCd(out byte[] lba450000AudioBuffer,
                                                                          out byte[] lba450000AudioSenseBuffer,
                                                                          450000, 2352, cluster,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.None, MmcSubchannel.None,
                                                                          _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba450000AudioData  = lba450000AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000AudioSense = lba450000AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba450000AudioDecodedSense =
                Sense.PrettifySense(lba450000AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba450000AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000 as audio... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000AudioReadable = !_dev.ReadCd(out byte[] lba400000AudioBuffer,
                                                                          out byte[] lba400000AudioSenseBuffer,
                                                                          400000, 2352, cluster,
                                                                          MmcSectorTypes.Cdda, false, false, false,
                                                                          MmcHeaderCodes.None, true, false,
                                                                          MmcErrorField.None, MmcSubchannel.None,
                                                                          _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba400000AudioData  = lba400000AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000AudioSense = lba400000AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba400000AudioDecodedSense =
                Sense.PrettifySense(lba400000AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba400000AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000 as audio... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000AudioReadable = !_dev.ReadCd(out byte[] lba45000AudioBuffer,
                                                                         out byte[] lba45000AudioSenseBuffer,
                                                                         45000, 2352, cluster, MmcSectorTypes.Cdda,
                                                                         false, false, false, MmcHeaderCodes.None,
                                                                         true, false, MmcErrorField.None,
                                                                         MmcSubchannel.None, _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000AudioData  = lba45000AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000AudioSense = lba45000AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba45000AudioDecodedSense = Sense.PrettifySense(lba45000AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba45000AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990 as audio... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990AudioReadable = !_dev.ReadCd(out byte[] lba44990AudioBuffer,
                                                                         out byte[] lba44990AudioSenseBuffer,
                                                                         44990, 2352, cluster, MmcSectorTypes.Cdda,
                                                                         false, false, false, MmcHeaderCodes.None,
                                                                         true, false, MmcErrorField.None,
                                                                         MmcSubchannel.None, _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990AudioData  = lba44990AudioBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990AudioSense = lba44990AudioSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba44990AudioDecodedSense = Sense.PrettifySense(lba44990AudioSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba44990AudioReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990AudioReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990AudioReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000AudioPqReadable = !_dev.ReadCd(out byte[] lba100000AudioPqBuffer,
                                                                            out byte[] lba100000AudioPqSenseBuffer,
                                                                            100000, 2368, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Q16, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba100000AudioPqData  = lba100000AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000AudioPqSense = lba100000AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba100000AudioPqDecodedSense =
                Sense.PrettifySense(lba100000AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba100000AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000AudioPqReadable = !_dev.ReadCd(out byte[] lba50000AudioPqBuffer,
                                                                           out byte[] lba50000AudioPqSenseBuffer,
                                                                           50000, 2368, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000AudioPqData  = lba50000AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000AudioPqSense = lba50000AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba50000AudioPqDecodedSense =
                Sense.PrettifySense(lba50000AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba50000AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000AudioPqReadable = !_dev.ReadCd(out byte[] lba450000AudioPqBuffer,
                                                                            out byte[] lba450000AudioPqSenseBuffer,
                                                                            450000, 2368, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Q16, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba450000AudioPqData  = lba450000AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000AudioPqSense = lba450000AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba450000AudioPqDecodedSense =
                Sense.PrettifySense(lba450000AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba450000AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000AudioPqReadable = !_dev.ReadCd(out byte[] lba400000AudioPqBuffer,
                                                                            out byte[] lba400000AudioPqSenseBuffer,
                                                                            400000, 2368, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Q16, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba400000AudioPqData  = lba400000AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000AudioPqSense = lba400000AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba400000AudioPqDecodedSense =
                Sense.PrettifySense(lba400000AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba400000AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000AudioPqReadable = !_dev.ReadCd(out byte[] lba45000AudioPqBuffer,
                                                                           out byte[] lba45000AudioPqSenseBuffer,
                                                                           45000, 2368, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000AudioPqData  = lba45000AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000AudioPqSense = lba45000AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba45000AudioPqDecodedSense =
                Sense.PrettifySense(lba45000AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba45000AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990 as audio with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990AudioPqReadable = !_dev.ReadCd(out byte[] lba44990AudioPqBuffer,
                                                                           out byte[] lba44990AudioPqSenseBuffer,
                                                                           44990, 2368, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Q16,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990AudioPqData  = lba44990AudioPqBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990AudioPqSense = lba44990AudioPqSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba44990AudioPqDecodedSense =
                Sense.PrettifySense(lba44990AudioPqSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba44990AudioPqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990AudioPqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990AudioPqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000AudioRwReadable = !_dev.ReadCd(out byte[] lba100000AudioRwBuffer,
                                                                            out byte[] lba100000AudioRwSenseBuffer,
                                                                            100000, 2448, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Raw, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba100000AudioRwData  = lba100000AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000AudioRwSense = lba100000AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba100000AudioRwDecodedSense =
                Sense.PrettifySense(lba100000AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba100000AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000AudioRwReadable = !_dev.ReadCd(out byte[] lba50000AudioRwBuffer,
                                                                           out byte[] lba50000AudioRwSenseBuffer,
                                                                           50000, 2448, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000AudioRwData  = lba50000AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000AudioRwSense = lba50000AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba50000AudioRwDecodedSense =
                Sense.PrettifySense(lba50000AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba50000AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000AudioRwReadable = !_dev.ReadCd(out byte[] lba450000AudioRwBuffer,
                                                                            out byte[] lba450000AudioRwSenseBuffer,
                                                                            450000, 2448, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Raw, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba450000AudioRwData  = lba450000AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000AudioRwSense = lba450000AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba450000AudioRwDecodedSense =
                Sense.PrettifySense(lba450000AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba450000AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000AudioRwReadable = !_dev.ReadCd(out byte[] lba400000AudioRwBuffer,
                                                                            out byte[] lba400000AudioRwSenseBuffer,
                                                                            400000, 2448, cluster,
                                                                            MmcSectorTypes.Cdda, false, false,
                                                                            false, MmcHeaderCodes.None, true,
                                                                            false, MmcErrorField.None,
                                                                            MmcSubchannel.Raw, _dev.Timeout,
                                                                            out _);

            report.GdRomSwapDiscCapabilities.Lba400000AudioRwData  = lba400000AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000AudioRwSense = lba400000AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba400000AudioRwDecodedSense =
                Sense.PrettifySense(lba400000AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba400000AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000AudioRwReadable = !_dev.ReadCd(out byte[] lba45000AudioRwBuffer,
                                                                           out byte[] lba45000AudioRwSenseBuffer,
                                                                           45000, 2448, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000AudioRwData  = lba45000AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000AudioRwSense = lba45000AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba45000AudioRwDecodedSense =
                Sense.PrettifySense(lba45000AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba45000AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990 as audio with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990AudioRwReadable = !_dev.ReadCd(out byte[] lba44990AudioRwBuffer,
                                                                           out byte[] lba44990AudioRwSenseBuffer,
                                                                           44990, 2448, cluster,
                                                                           MmcSectorTypes.Cdda, false, false,
                                                                           false, MmcHeaderCodes.None, true, false,
                                                                           MmcErrorField.None, MmcSubchannel.Raw,
                                                                           _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990AudioRwData  = lba44990AudioRwBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990AudioRwSense = lba44990AudioRwSenseBuffer;

            report.GdRomSwapDiscCapabilities.Lba44990AudioRwDecodedSense =
                Sense.PrettifySense(lba44990AudioRwSenseBuffer);

            report.GdRomSwapDiscCapabilities.Lba44990AudioRwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990AudioRwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990AudioRwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000Readable = !_dev.ReadCd(out byte[] lba100000Buffer,
                                                                              out byte[] lba100000SenseBuffer, 100000,
                                                                              2352, cluster, MmcSectorTypes.AllTypes,
                                                                              false, false, true,
                                                                              MmcHeaderCodes.AllHeaders, true, true,
                                                                              MmcErrorField.None, MmcSubchannel.None,
                                                                              _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba100000Data            = lba100000Buffer;
            report.GdRomSwapDiscCapabilities.Lba100000Sense           = lba100000SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000DecodedSense    = Sense.PrettifySense(lba100000SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba100000ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000Readable = !_dev.ReadCd(out byte[] lba50000Buffer,
                                                                             out byte[] lba50000SenseBuffer, 50000,
                                                                             2352, cluster, MmcSectorTypes.AllTypes,
                                                                             false, false, true,
                                                                             MmcHeaderCodes.AllHeaders, true, true,
                                                                             MmcErrorField.None, MmcSubchannel.None,
                                                                             _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000Data            = lba50000Buffer;
            report.GdRomSwapDiscCapabilities.Lba50000Sense           = lba50000SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000DecodedSense    = Sense.PrettifySense(lba50000SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba50000ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000Readable = !_dev.ReadCd(out byte[] lba450000Buffer,
                                                                              out byte[] lba450000SenseBuffer, 450000,
                                                                              2352, cluster, MmcSectorTypes.AllTypes,
                                                                              false, false, true,
                                                                              MmcHeaderCodes.AllHeaders, true, true,
                                                                              MmcErrorField.None, MmcSubchannel.None,
                                                                              _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba450000Data            = lba450000Buffer;
            report.GdRomSwapDiscCapabilities.Lba450000Sense           = lba450000SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000DecodedSense    = Sense.PrettifySense(lba450000SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba450000ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000Readable = !_dev.ReadCd(out byte[] lba400000Buffer,
                                                                              out byte[] lba400000SenseBuffer, 400000,
                                                                              2352, cluster, MmcSectorTypes.AllTypes,
                                                                              false, false, true,
                                                                              MmcHeaderCodes.AllHeaders, true, true,
                                                                              MmcErrorField.None, MmcSubchannel.None,
                                                                              _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba400000Data            = lba400000Buffer;
            report.GdRomSwapDiscCapabilities.Lba400000Sense           = lba400000SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000DecodedSense    = Sense.PrettifySense(lba400000SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba400000ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000Readable = !_dev.ReadCd(out byte[] lba45000Buffer,
                                                                             out byte[] lba45000SenseBuffer, 45000,
                                                                             2352, cluster, MmcSectorTypes.AllTypes,
                                                                             false, false, true,
                                                                             MmcHeaderCodes.AllHeaders, true, true,
                                                                             MmcErrorField.None, MmcSubchannel.None,
                                                                             _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000Data            = lba45000Buffer;
            report.GdRomSwapDiscCapabilities.Lba45000Sense           = lba45000SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000DecodedSense    = Sense.PrettifySense(lba45000SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba45000ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990Readable = !_dev.ReadCd(out byte[] lba44990Buffer,
                                                                             out byte[] lba44990SenseBuffer, 44990,
                                                                             2352, cluster, MmcSectorTypes.AllTypes,
                                                                             false, false, true,
                                                                             MmcHeaderCodes.AllHeaders, true, true,
                                                                             MmcErrorField.None, MmcSubchannel.None,
                                                                             _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990Data            = lba44990Buffer;
            report.GdRomSwapDiscCapabilities.Lba44990Sense           = lba44990SenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990DecodedSense    = Sense.PrettifySense(lba44990SenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba44990ReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990Readable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990Readable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000PqReadable = !_dev.ReadCd(out byte[] lba100000PqBuffer,
                                                                                    out byte[] lba100000PqSenseBuffer,
                                                                                    100000, 2368, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Q16, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba100000PqData            = lba100000PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000PqSense           = lba100000PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000PqDecodedSense    = Sense.PrettifySense(lba100000PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba100000PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000PqReadable = !_dev.ReadCd(out byte[] lba50000PqBuffer,
                                                                               out byte[] lba50000PqSenseBuffer, 50000,
                                                                               2368, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Q16,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000PqData            = lba50000PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000PqSense           = lba50000PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000PqDecodedSense    = Sense.PrettifySense(lba50000PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba50000PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000PqReadable = !_dev.ReadCd(out byte[] lba450000PqBuffer,
                                                                                    out byte[] lba450000PqSenseBuffer,
                                                                                    450000, 2368, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Q16, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba450000PqData            = lba450000PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000PqSense           = lba450000PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000PqDecodedSense    = Sense.PrettifySense(lba450000PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba450000PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000PqReadable = !_dev.ReadCd(out byte[] lba400000PqBuffer,
                                                                                    out byte[] lba400000PqSenseBuffer,
                                                                                    400000, 2368, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Q16, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba400000PqData            = lba400000PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000PqSense           = lba400000PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000PqDecodedSense    = Sense.PrettifySense(lba400000PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba400000PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000PqReadable = !_dev.ReadCd(out byte[] lba45000PqBuffer,
                                                                               out byte[] lba45000PqSenseBuffer, 45000,
                                                                               2368, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Q16,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000PqData            = lba45000PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000PqSense           = lba45000PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000PqDecodedSense    = Sense.PrettifySense(lba45000PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba45000PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990 with PQ subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990PqReadable = !_dev.ReadCd(out byte[] lba44990PqBuffer,
                                                                               out byte[] lba44990PqSenseBuffer, 44990,
                                                                               2368, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Q16,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990PqData            = lba44990PqBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990PqSense           = lba44990PqSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990PqDecodedSense    = Sense.PrettifySense(lba44990PqSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba44990PqReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990PqReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990PqReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 100000 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba100000RwReadable = !_dev.ReadCd(out byte[] lba100000RwBuffer,
                                                                                    out byte[] lba100000RwSenseBuffer,
                                                                                    100000, 2448, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Raw, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba100000RwData            = lba100000RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000RwSense           = lba100000RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba100000RwDecodedSense    = Sense.PrettifySense(lba100000RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba100000RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba100000RwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 50000 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba50000RwReadable = !_dev.ReadCd(out byte[] lba50000RwBuffer,
                                                                               out byte[] lba50000RwSenseBuffer, 50000,
                                                                               2448, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Raw,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba50000RwData            = lba50000RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000RwSense           = lba50000RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba50000RwDecodedSense    = Sense.PrettifySense(lba50000RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba50000RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba50000RwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 450000 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba450000RwReadable = !_dev.ReadCd(out byte[] lba450000RwBuffer,
                                                                                    out byte[] lba450000RwSenseBuffer,
                                                                                    450000, 2448, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Raw, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba450000RwData            = lba450000RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000RwSense           = lba450000RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba450000RwDecodedSense    = Sense.PrettifySense(lba450000RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba450000RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba450000RwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 400000 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba400000RwReadable = !_dev.ReadCd(out byte[] lba400000RwBuffer,
                                                                                    out byte[] lba400000RwSenseBuffer,
                                                                                    400000, 2448, cluster,
                                                                                    MmcSectorTypes.AllTypes, false,
                                                                                    false, true,
                                                                                    MmcHeaderCodes.AllHeaders, true,
                                                                                    true, MmcErrorField.None,
                                                                                    MmcSubchannel.Raw, _dev.Timeout,
                                                                                    out _);

            report.GdRomSwapDiscCapabilities.Lba400000RwData            = lba400000RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000RwSense           = lba400000RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba400000RwDecodedSense    = Sense.PrettifySense(lba400000RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba400000RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba400000RwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 45000 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba45000RwReadable = !_dev.ReadCd(out byte[] lba45000RwBuffer,
                                                                               out byte[] lba45000RwSenseBuffer, 45000,
                                                                               2448, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Raw,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba45000RwData            = lba45000RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000RwSense           = lba45000RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba45000RwDecodedSense    = Sense.PrettifySense(lba45000RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba45000RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba45000RwReadable ? "Success!" : "FAIL!");

        AaruConsole.Write("Reading LBA 44990 with RW subchannel... ");
        cluster = 16;

        while(true)
        {
            report.GdRomSwapDiscCapabilities.Lba44990RwReadable = !_dev.ReadCd(out byte[] lba44990RwBuffer,
                                                                               out byte[] lba44990RwSenseBuffer, 44990,
                                                                               2448, cluster, MmcSectorTypes.AllTypes,
                                                                               false, false, true,
                                                                               MmcHeaderCodes.AllHeaders, true, true,
                                                                               MmcErrorField.None, MmcSubchannel.Raw,
                                                                               _dev.Timeout, out _);

            report.GdRomSwapDiscCapabilities.Lba44990RwData            = lba44990RwBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990RwSense           = lba44990RwSenseBuffer;
            report.GdRomSwapDiscCapabilities.Lba44990RwDecodedSense    = Sense.PrettifySense(lba44990RwSenseBuffer);
            report.GdRomSwapDiscCapabilities.Lba44990RwReadableCluster = (int)cluster;

            if(report.GdRomSwapDiscCapabilities.Lba44990RwReadable)
                break;

            if(cluster == 1)
                break;

            cluster /= 2;
        }

        AaruConsole.WriteLine(report.GdRomSwapDiscCapabilities.Lba44990RwReadable ? "Success!" : "FAIL!");

        if(report.GdRomSwapDiscCapabilities.Lba45000Readable       == false &&
           report.GdRomSwapDiscCapabilities.Lba50000Readable       == false &&
           report.GdRomSwapDiscCapabilities.Lba100000Readable      == false &&
           report.GdRomSwapDiscCapabilities.Lba400000Readable      == false &&
           report.GdRomSwapDiscCapabilities.Lba450000Readable      == false &&
           report.GdRomSwapDiscCapabilities.Lba45000AudioReadable  == false &&
           report.GdRomSwapDiscCapabilities.Lba50000AudioReadable  == false &&
           report.GdRomSwapDiscCapabilities.Lba100000AudioReadable == false &&
           report.GdRomSwapDiscCapabilities.Lba400000AudioReadable == false &&
           report.GdRomSwapDiscCapabilities.Lba450000AudioReadable == false)
            return;

        pressedKey = new ConsoleKeyInfo();

        while(pressedKey.Key != ConsoleKey.Y &&
              pressedKey.Key != ConsoleKey.N)
        {
            AaruConsole.
                Write("The next part of the test will read the whole high density area of a GD-ROM from the smallest known readable sector until the first error happens\n" +
                      "Do you want to proceed? (Y/N): ");

            pressedKey = Console.ReadKey();
            AaruConsole.WriteLine();
        }

        if(pressedKey.Key == ConsoleKey.N)
            return;

        uint          startingSector = 45000;
        var           readAsAudio    = false;
        var           aborted        = false;
        MmcSubchannel subchannel     = MmcSubchannel.None;
        uint          blockSize      = 2352;

        if(report.GdRomSwapDiscCapabilities.Lba45000Readable == false)
        {
            startingSector = 45000;
            readAsAudio    = false;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba45000ReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000RwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba45000PqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba50000Readable == false)
        {
            startingSector = 50000;
            readAsAudio    = false;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba50000ReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000RwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba50000PqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba100000Readable == false)
        {
            startingSector = 100000;
            readAsAudio    = false;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba100000ReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000RwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba100000PqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba400000Readable == false)
        {
            startingSector = 400000;
            readAsAudio    = false;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba400000ReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000RwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba400000PqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba450000Readable == false)
        {
            startingSector = 450000;
            readAsAudio    = false;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba450000ReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000RwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba450000PqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba45000AudioReadable == false)
        {
            startingSector = 45000;
            readAsAudio    = true;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba45000AudioReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba45000AudioRwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba45000AudioPqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba50000AudioReadable == false)
        {
            startingSector = 50000;
            readAsAudio    = true;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba50000AudioReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba50000AudioRwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba50000AudioPqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba100000AudioReadable == false)
        {
            startingSector = 100000;
            readAsAudio    = true;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba100000AudioReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba100000AudioRwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba100000AudioPqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba400000AudioReadable == false)
        {
            startingSector = 400000;
            readAsAudio    = true;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba400000AudioReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba400000AudioRwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba400000AudioPqReadable)
                subchannel = MmcSubchannel.Q16;
        }
        else if(report.GdRomSwapDiscCapabilities.Lba450000AudioReadable == false)
        {
            startingSector = 450000;
            readAsAudio    = true;
            cluster        = (uint)report.GdRomSwapDiscCapabilities.Lba450000AudioReadableCluster;

            if(report.GdRomSwapDiscCapabilities.Lba450000AudioRwReadable)
                subchannel = MmcSubchannel.Raw;
            else if(report.GdRomSwapDiscCapabilities.Lba450000AudioPqReadable)
                subchannel = MmcSubchannel.Q16;
        }

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            aborted  = true;
        };

        report.GdRomSwapDiscCapabilities.MinimumReadableSectorInHdArea = startingSector;

        switch(subchannel)
        {
            case MmcSubchannel.Raw:
                blockSize += 96;

                break;
            case MmcSubchannel.Q16:
                blockSize += 16;

                break;
        }

        byte[] lastSuccessfulPq = null;
        byte[] lastSuccessfulRw = null;
        var    trackModeChange  = false;

        AaruConsole.WriteLine();

        for(uint lba = startingSector; lba < sectors; lba += cluster)
        {
            if(aborted)
            {
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Aborted!");

                break;
            }

            AaruConsole.Write("\rReading LBA {0} of {1}", lba, sectors);

            sense = readAsAudio
                        ? _dev.ReadCd(out buffer, out senseBuffer, lba, blockSize, cluster, MmcSectorTypes.Cdda, false,
                                      false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, subchannel,
                                      _dev.Timeout, out _) : _dev.ReadCd(out buffer, out senseBuffer, lba, blockSize,
                                                                         cluster, MmcSectorTypes.AllTypes, false, false,
                                                                         true, MmcHeaderCodes.AllHeaders, true, true,
                                                                         MmcErrorField.None, subchannel, _dev.Timeout,
                                                                         out _);

            if(sense)
            {
                if(trackModeChange)
                    break;

                DecodedSense? decoded = Sense.Decode(senseBuffer);

                if(decoded is not { ASC: 0x64, ASCQ: 0x00 })
                    break;

                trackModeChange = true;
                readAsAudio     = !readAsAudio;

                continue;
            }

            trackModeChange = false;

            switch(subchannel)
            {
                case MmcSubchannel.Raw:
                    lastSuccessfulRw = buffer;

                    break;
                case MmcSubchannel.Q16:
                    lastSuccessfulPq = buffer;

                    break;
            }

            report.GdRomSwapDiscCapabilities.MaximumReadableSectorInHdArea = lba + cluster - 1;
        }

        AaruConsole.WriteLine();

        report.GdRomSwapDiscCapabilities.MaximumReadablePqInHdArea = lastSuccessfulPq;
        report.GdRomSwapDiscCapabilities.MaximumReadableRwInHdArea = lastSuccessfulRw;
    }
}