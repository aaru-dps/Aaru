using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices;

static partial class ScsiMmc
{
    static void MediaTekReadCache(string devPath, Device dev)
    {
        uint   address = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for MediaTek READ CACHE command:");
            AaruConsole.WriteLine("LBA: {0}", address);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to special SCSI MultiMedia Commands menu.");

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to special SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("LBA?: ");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        AaruConsole.WriteLine("Sending READ CD to the device...");

        bool sense = dev.ReadCd(out byte[] buffer, out byte[] senseBuffer, address, 2352, 1, MmcSectorTypes.AllTypes,
                                false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                MmcSubchannel.None, dev.Timeout, out double duration);

        if(sense)
            AaruConsole.WriteLine("READ CD failed...");

        AaruConsole.WriteLine("Sending MediaTek READ DRAM to the device...");
        sense = dev.MediaTekReadDram(out buffer, out senseBuffer, 0, 0xB00, dev.Timeout, out duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("System error status is {0} and error number is {1}.", dev.Error, dev.LastError);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Print sense buffer.");
        AaruConsole.WriteLine("3.- Decode sense buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("5.- Change parameters.");
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
                AaruConsole.WriteLine("MediaTek's READ CACHE response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MediaTek's READ CACHE sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MediaTek's READ CACHE decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}