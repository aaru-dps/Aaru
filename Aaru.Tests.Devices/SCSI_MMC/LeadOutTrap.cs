namespace Aaru.Tests.Devices;

using System;
using System.Linq;
using System.Threading;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

static partial class ScsiMmc
{
    static void ReadLeadOutUsingTrapDisc(string devPath, Device dev)
    {
        int    item;
        var    tocIsNotBcd = false;
        bool   sense;
        byte[] buffer;
        byte[] senseBuffer;

    start:
        Console.Clear();

        AaruConsole.WriteLine("Ejecting disc...");

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine("Please insert a data only disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        Console.ReadLine();

        AaruConsole.WriteLine("Sending READ FULL TOC to the device...");

        var retries = 0;

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
            Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        int min   = (leadOutTrack.PMIN   >> 4) * 10 + (leadOutTrack.PMIN   & 0x0F);
        int sec   = (leadOutTrack.PSEC   >> 4) * 10 + (leadOutTrack.PSEC   & 0x0F);
        int frame = (leadOutTrack.PFRAME >> 4) * 10 + (leadOutTrack.PFRAME & 0x0F);

        int sectors = min * 60 * 75 + sec * 75 + frame - 150;

        AaruConsole.WriteLine("Data disc shows {0} sectors...", sectors);

        AaruConsole.WriteLine("Ejecting disc...");

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine("Please insert the trap disc inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        Console.ReadLine();

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
            Console.ReadLine();

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        toc = decodedToc.Value;

        leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        min = 0;

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

        int trapSectors = min * 60 * 75 + sec * 75 + frame - 150;

        AaruConsole.WriteLine("Trap disc shows {0} sectors...", trapSectors);

        if(trapSectors < sectors + 100)
        {
            AaruConsole.WriteLine("Trap disc doesn't have enough sectors...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        AaruConsole.WriteLine("Stopping motor...");

        dev.StopUnit(out _, dev.Timeout, out _);

        AaruConsole.WriteLine("Please MANUALLY get the trap disc out and put the data disc back inside...");
        AaruConsole.WriteLine("Press any key to continue...");
        Console.ReadLine();

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
            Console.ReadLine();

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine("Could not decode TOC...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

            return;
        }

        toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor newLeadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(newLeadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine("Cannot find lead-out...");
            AaruConsole.WriteLine("Press any key to continue...");
            Console.ReadLine();

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
            Console.ReadLine();

            return;
        }

        AaruConsole.Write("Reading LBA {0}... ", sectors + 5);

        bool dataResult = dev.ReadCd(out byte[] dataBuffer, out byte[] dataSense, (uint)(sectors + 5), 2352, 1,
                                     MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                     MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(dataResult ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA {0} as audio (scrambled)... ", sectors + 5);

        bool scrambledResult = dev.ReadCd(out byte[] scrambledBuffer, out byte[] scrambledSense, (uint)(sectors + 5),
                                          2352, 1, MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                          false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(scrambledResult ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA {0}'s PQ subchannel... ", sectors + 5);

        bool pqResult = dev.ReadCd(out byte[] pqBuffer, out byte[] pqSense, (uint)(sectors + 5), 16, 1,
                                   MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, false, false,
                                   MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout, out _);

        if(pqResult)
            pqResult = dev.ReadCd(out pqBuffer, out pqSense, (uint)(sectors + 5), 16, 1, MmcSectorTypes.AllTypes, false,
                                  false, false, MmcHeaderCodes.None, false, false, MmcErrorField.None,
                                  MmcSubchannel.Q16, dev.Timeout, out _);

        AaruConsole.WriteLine(pqResult ? "FAIL!" : "Success!");

        AaruConsole.Write("Reading LBA {0}'s PQ subchannel... ", sectors + 5);

        bool rwResult = dev.ReadCd(out byte[] rwBuffer, out byte[] rwSense, (uint)(sectors + 5), 16, 1,
                                   MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, false, false,
                                   MmcErrorField.None, MmcSubchannel.Rw, dev.Timeout, out _);

        if(rwResult)
            rwResult = dev.ReadCd(out rwBuffer, out rwSense, (uint)(sectors + 5), 16, 1, MmcSectorTypes.Cdda, false,
                                  false, false, MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Rw,
                                  dev.Timeout, out _);

        AaruConsole.WriteLine(pqResult ? "FAIL!" : "Success!");

    menu:
        Console.Clear();
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Device {0} read Lead-Out.", dataResult && scrambledResult ? "cannot" : "can");

        AaruConsole.WriteLine("LBA {0} sense is {1}, buffer is {2}, sense buffer is {3}.", sectors + 5, dataResult,
                              dataBuffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(dataBuffer)
                                      ? "empty"
                                      : $"{dataBuffer.Length} bytes", dataSense is null
                                                                          ? "null"
                                                                          : ArrayHelpers.ArrayIsNullOrEmpty(dataSense)
                                                                              ? "empty"
                                                                              : $"{dataSense.Length}");

        AaruConsole.WriteLine("LBA {0} (scrambled) sense is {1}, buffer is {2}, sense buffer is {3}.", sectors + 5,
                              scrambledResult, scrambledBuffer is null
                                                   ? "null"
                                                   : ArrayHelpers.ArrayIsNullOrEmpty(scrambledBuffer)
                                                       ? "empty"
                                                       : $"{scrambledBuffer.Length} bytes", scrambledSense is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(scrambledSense)
                                      ? "empty"
                                      : $"{scrambledSense.Length}");

        AaruConsole.WriteLine("LBA {0}'s PQ sense is {1}, buffer is {2}, sense buffer is {3}.", sectors + 5, pqResult,
                              pqBuffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(pqBuffer)
                                      ? "empty"
                                      : $"{pqBuffer.Length} bytes", pqSense is null
                                                                        ? "null"
                                                                        : ArrayHelpers.ArrayIsNullOrEmpty(pqSense)
                                                                            ? "empty"
                                                                            : $"{pqSense.Length}");

        AaruConsole.WriteLine("LBA {0}'s RW sense is {1}, buffer is {2}, sense buffer is {3}.", sectors + 5, rwResult,
                              rwBuffer is null
                                  ? "null"
                                  : ArrayHelpers.ArrayIsNullOrEmpty(rwBuffer)
                                      ? "empty"
                                      : $"{rwBuffer.Length} bytes", rwSense is null
                                                                        ? "null"
                                                                        : ArrayHelpers.ArrayIsNullOrEmpty(rwSense)
                                                                            ? "empty"
                                                                            : $"{rwSense.Length}");

        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print LBA {0} buffer.", sectors                    + 5);
        AaruConsole.WriteLine("2.- Print LBA {0} sense buffer.", sectors              + 5);
        AaruConsole.WriteLine("3.- Decode LBA {0} sense buffer.", sectors             + 5);
        AaruConsole.WriteLine("4.- Print LBA {0} (scrambled) buffer.", sectors        + 5);
        AaruConsole.WriteLine("5.- Print LBA {0} (scrambled) sense buffer.", sectors  + 5);
        AaruConsole.WriteLine("6.- Decode LBA {0} (scrambled) sense buffer.", sectors + 5);
        AaruConsole.WriteLine("7.- Print LBA {0}'s PQ buffer.", sectors               + 5);
        AaruConsole.WriteLine("8.- Print LBA {0}'s PQ sense buffer.", sectors         + 5);
        AaruConsole.WriteLine("9.- Decode LBA {0}'s PQ sense buffer.", sectors        + 5);
        AaruConsole.WriteLine("10.- Print LBA {0}'s RW buffer.", sectors              + 5);
        AaruConsole.WriteLine("11.- Print LBA {0}'s RW sense buffer.", sectors        + 5);
        AaruConsole.WriteLine("12.- Decode LBA {0}'s RW sense buffer.", sectors       + 5);
        AaruConsole.WriteLine("13.- Send command again.");
        AaruConsole.WriteLine("0.- Return to special SCSI MultiMedia Commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to special SCSI MultiMedia Commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} response:", sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(dataBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 2:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} sense:", sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(dataSense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 3:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} decoded sense:", sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(dataSense));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 4:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} (scrambled) response:", sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(scrambledBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 5:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} (scrambled) sense:", sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(scrambledSense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 6:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA {0} (scrambled) decoded sense:", sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(scrambledSense));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 7:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's PQ {0} response:", sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(pqBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 8:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's PQ {0} sense:", sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(pqSense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 9:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's PQ {0} decoded sense:", sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(pqSense));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 10:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's RW {0} response:", sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(rwBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 11:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's RW {0} sense:", sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(rwSense, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 12:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("LBA's RW {0} decoded sense:", sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(rwSense));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();

                goto menu;
            case 13: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }
}