using System.Linq;
using System.Threading;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices;

internal static partial class ScsiMmc
{
    static void CheckGdromReadability(string devPath, Device dev)
    {
        string strDev;
        int    item;
        bool   tocIsNotBcd = false;
        bool   sense;
        byte[] buffer;
        byte[] senseBuffer;
        int    retries;

        start:
        System.Console.Clear();

        AaruConsole.WriteLine("Ejecting disc...");

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine("Please insert trap disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        System.Console.ReadLine();

        AaruConsole.WriteLine("Sending READ FULL TOC to the device...");

        retries = 0;

        do
        {
            retries++;
            sense = dev.ScsiTestUnitReady(out senseBuffer, dev.Timeout, out _);

            if(!sense)
                break;

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);

            if(decodedSense.Value.ASC != 0x04)
                break;

            if(decodedSense.Value.ASCQ != 0x01)
                break;

            Thread.Sleep(2000);
        } while(retries < 25);

        sense = dev.ReadRawToc(out buffer, out senseBuffer, 1, dev.Timeout, out _);

        if(sense)
        {
            AaruConsole.WriteLine("READ FULL TOC failed...");
            AaruConsole.WriteLine("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        int min = 0, sec, frame;

        if(leadOutTrack.PMIN == 122)
            tocIsNotBcd = true;

        if(leadOutTrack.PMIN >= 0xA0 &&
           !tocIsNotBcd)
        {
            min               += 90;
            leadOutTrack.PMIN -= 0x90;
        }

        if(tocIsNotBcd)
        {
            min   = leadOutTrack.PMIN;
            sec   = leadOutTrack.PSEC;
            frame = leadOutTrack.PFRAME;
        }
        else
        {
            min   += ((leadOutTrack.PMIN   >> 4) * 10) + (leadOutTrack.PMIN   & 0x0F);
            sec   =  ((leadOutTrack.PSEC   >> 4) * 10) + (leadOutTrack.PSEC   & 0x0F);
            frame =  ((leadOutTrack.PFRAME >> 4) * 10) + (leadOutTrack.PFRAME & 0x0F);
        }

        int sectors = (min * 60 * 75) + (sec * 75) + frame - 150;

        AaruConsole.WriteLine("Trap disc shows {0} sectors...", sectors);

        if(sectors < 450000)
        {
            AaruConsole.WriteLine("Trap disc doesn't have enough sectors...");
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        AaruConsole.WriteLine("Stopping motor...");

        dev.StopUnit(out _, dev.Timeout, out _);

        AaruConsole.WriteLine("Please MANUALLY get the trap disc out and put the GD-ROM disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        System.Console.ReadLine();

        AaruConsole.WriteLine("Waiting 5 seconds...");
        Thread.Sleep(5000);

        AaruConsole.WriteLine("Sending READ FULL TOC to the device...");

        retries = 0;

        do
        {
            retries++;
            sense = dev.ReadRawToc(out buffer, out senseBuffer, 1, dev.Timeout, out _);

            if(!sense)
                break;

            DecodedSense? decodedSense = Sense.Decode(senseBuffer);

            if(decodedSense.Value.ASC != 0x04)
                break;

            if(decodedSense.Value.ASCQ != 0x01)
                break;
        } while(retries < 25);

        if(sense)
        {
            AaruConsole.WriteLine("READ FULL TOC failed...");
            AaruConsole.WriteLine("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor newLeadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(newLeadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

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
            AaruConsole.WriteLine("Press any key to continue...");
            System.Console.ReadLine();

            return;
        }

        dev.SetCdSpeed(out _, RotationalControl.PureCav, 170, 0, dev.Timeout, out _);

        AaruConsole.Write("Reading LBA 0... ");

        bool lba0Result = dev.ReadCd(out byte[] lba0Buffer, out byte[] lba0Sense, 0, 2352, 1,
                                     MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                     true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba0Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 0 as audio (scrambled)... ");

        bool lba0ScrambledResult = dev.ReadCd(out byte[] lba0ScrambledBuffer, out byte[] lba0ScrambledSense, 0,
                                              2352, 1, MmcSectorTypes.Cdda, false, false, false,
                                              MmcHeaderCodes.None, true, false, MmcErrorField.None,
                                              MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba0ScrambledResult ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 100000... ");

        bool lba100000Result = dev.ReadCd(out byte[] lba100000Buffer, out byte[] lba100000Sense, 100000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                          true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba100000Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 50000... ");

        bool lba50000Result = dev.ReadCd(out byte[] lba50000Buffer, out byte[] lba50000Sense, 50000, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                         true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba50000Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 450000... ");

        bool lba450000Result = dev.ReadCd(out byte[] lba450000Buffer, out byte[] lba450000Sense, 450000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                          true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba450000Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 400000... ");

        bool lba400000Result = dev.ReadCd(out byte[] lba400000Buffer, out byte[] lba400000Sense, 400000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                          true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba400000Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 45000... ");

        bool lba45000Result = dev.ReadCd(out byte[] lba45000Buffer, out byte[] lba45000Sense, 45000, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                         true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba45000Result ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA 44990... ");

        bool lba44990Result = dev.ReadCd(out byte[] lba44990Buffer, out byte[] lba44990Sense, 44990, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                         true, true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba44990Result ? "FAIL!" : "Success!");

        menu:
        System.Console.Clear();
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Device {0} read HD area.", lba450000Result ? "cannot" : "can");

        AaruConsole.WriteLine("LBA 0 sense is {0}, buffer is {1}, sense buffer is {2}.", lba0Result,
                              lba0Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba0Buffer)
                                      ? "empty"
                                      : $"{lba0Buffer.Length} bytes", lba0Sense is null
                                                                          ? "null"
                                                                          : ArrayHelpers.
                                                                              ArrayIsNullOrEmpty(lba0Sense)
                                                                              ? "empty"
                                                                              : $"{lba0Sense.Length}");

        AaruConsole.WriteLine("LBA 0 (scrambled) sense is {0}, buffer is {1}, sense buffer is {2}.",
                              lba0ScrambledResult, lba0ScrambledBuffer is null
                                                       ? "null"
                                                       : ArrayHelpers.ArrayIsNullOrEmpty(lba0ScrambledBuffer)
                                                           ? "empty"
                                                           : $"{lba0ScrambledBuffer.Length} bytes",
                              lba0ScrambledSense is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba0ScrambledSense)
                                      ? "empty"
                                      : $"{lba0ScrambledSense.Length}");

        AaruConsole.WriteLine("LBA 44990 sense is {0}, buffer is {1}, sense buffer is {2}.", lba44990Result,
                              lba44990Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba44990Buffer)
                                      ? "empty"
                                      : $"{lba44990Buffer.Length} bytes", lba44990Sense is null
                                                                              ? "null"
                                                                              : ArrayHelpers.
                                                                                  ArrayIsNullOrEmpty(lba44990Sense)
                                                                                  ? "empty"
                                                                                  : $"{lba44990Sense.Length}");

        AaruConsole.WriteLine("LBA 45000 sense is {0}, buffer is {1}, sense buffer is {2}.", lba45000Result,
                              lba45000Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba45000Buffer)
                                      ? "empty"
                                      : $"{lba45000Buffer.Length} bytes", lba45000Sense is null
                                                                              ? "null"
                                                                              : ArrayHelpers.
                                                                                  ArrayIsNullOrEmpty(lba45000Sense)
                                                                                  ? "empty"
                                                                                  : $"{lba45000Sense.Length}");

        AaruConsole.WriteLine("LBA 50000 sense is {0}, buffer is {1}, sense buffer is {2}.", lba50000Result,
                              lba50000Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba50000Buffer)
                                      ? "empty"
                                      : $"{lba50000Buffer.Length} bytes", lba50000Sense is null
                                                                              ? "null"
                                                                              : ArrayHelpers.
                                                                                  ArrayIsNullOrEmpty(lba50000Sense)
                                                                                  ? "empty"
                                                                                  : $"{lba50000Sense.Length}");

        AaruConsole.WriteLine("LBA 100000 sense is {0}, buffer is {1}, sense buffer is {2}.", lba100000Result,
                              lba100000Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba100000Buffer)
                                      ? "empty"
                                      : $"{lba100000Buffer.Length} bytes", lba100000Sense is null
                                                                               ? "null"
                                                                               : ArrayHelpers.
                                                                                   ArrayIsNullOrEmpty(lba100000Sense)
                                                                                   ? "empty"
                                                                                   : $"{lba100000Sense.Length}");

        AaruConsole.WriteLine("LBA 400000 sense is {0}, buffer is {1}, sense buffer is {2}.", lba400000Result,
                              lba400000Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba400000Buffer)
                                      ? "empty"
                                      : $"{lba400000Buffer.Length} bytes", lba400000Sense is null
                                                                               ? "null"
                                                                               : ArrayHelpers.
                                                                                   ArrayIsNullOrEmpty(lba400000Sense)
                                                                                   ? "empty"
                                                                                   : $"{lba400000Sense.Length}");

        AaruConsole.WriteLine("LBA 450000 sense is {0}, buffer is {1}, sense buffer is {2}.", lba450000Result,
                              lba450000Buffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(lba450000Buffer)
                                      ? "empty"
                                      : $"{lba450000Buffer.Length} bytes", lba450000Sense is null
                                                                               ? "null"
                                                                               : ArrayHelpers.
                                                                                   ArrayIsNullOrEmpty(lba450000Sense)
                                                                                   ? "empty"
                                                                                   : $"{lba450000Sense.Length}");

        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print LBA 0 buffer.");
        AaruConsole.WriteLine("2.- Print LBA 0 sense buffer.");
        AaruConsole.WriteLine("3.- Decode LBA 0 sense buffer.");
        AaruConsole.WriteLine("4.- Print LBA 0 (scrambled) buffer.");
        AaruConsole.WriteLine("5.- Print LBA 0 (scrambled) sense buffer.");
        AaruConsole.WriteLine("6.- Decode LBA 0 (scrambled) sense buffer.");
        AaruConsole.WriteLine("7.- Print LBA 44990 buffer.");
        AaruConsole.WriteLine("8.- Print LBA 44990 sense buffer.");
        AaruConsole.WriteLine("9.- Decode LBA 44990 sense buffer.");
        AaruConsole.WriteLine("10.- Print LBA 45000 buffer.");
        AaruConsole.WriteLine("11.- Print LBA 45000 sense buffer.");
        AaruConsole.WriteLine("12.- Decode LBA 45000 sense buffer.");
        AaruConsole.WriteLine("13.- Print LBA 50000 buffer.");
        AaruConsole.WriteLine("14.- Print LBA 50000 sense buffer.");
        AaruConsole.WriteLine("15.- Decode LBA 50000 sense buffer.");
        AaruConsole.WriteLine("16.- Print LBA 100000 buffer.");
        AaruConsole.WriteLine("17.- Print LBA 100000 sense buffer.");
        AaruConsole.WriteLine("18.- Decode LBA 100000 sense buffer.");
        AaruConsole.WriteLine("19.- Print LBA 400000 buffer.");
        AaruConsole.WriteLine("20.- Print LBA 400000 sense buffer.");
        AaruConsole.WriteLine("21.- Decode LBA 400000 sense buffer.");
        AaruConsole.WriteLine("22.- Print LBA 450000 buffer.");
        AaruConsole.WriteLine("23.- Print LBA 450000 sense buffer.");
        AaruConsole.WriteLine("24.- Decode LBA 450000 sense buffer.");
        AaruConsole.WriteLine("25.- Send command again.");
        AaruConsole.WriteLine("0.- Return to special SCSI MultiMedia Commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to special SCSI MultiMedia Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba0Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba0Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba0Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 (scrambled) response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba0ScrambledBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 5:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 (scrambled) sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba0ScrambledSense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 6:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 0 (scrambled) decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba0ScrambledSense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 7:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 44990 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba44990Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 8:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 44990 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba44990Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 9:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 44990 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba44990Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 10:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 45000 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba45000Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 11:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 45000 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba45000Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 12:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 45000 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba45000Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 13:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 50000 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba50000Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 14:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 50000 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba50000Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 15:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 50000 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba50000Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 16:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 100000 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba100000Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 17:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 100000 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba100000Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 18:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 100000 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba100000Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 19:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 400000 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba400000Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 20:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 400000 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba400000Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 21:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 400000 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba400000Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 22:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 450000 response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(lba450000Buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 23:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 450000 sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba450000Sense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 24:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA 450000 decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(lba450000Sense));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();

                goto menu;
            case 25: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}