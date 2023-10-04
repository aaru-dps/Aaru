using System.Linq;
using System.Threading;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices;

static partial class ScsiMmc
{
    static void ReadLeadOutUsingTrapDisc(string devPath, Device dev)
    {
        var    tocIsNotBcd = false;
        bool   sense;
        byte[] senseBuffer;

    start:
        System.Console.Clear();

        AaruConsole.WriteLine(Localization.Ejecting_disc);

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine(Localization.Please_insert_a_data_only_disc_inside);
        AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
        System.Console.ReadLine();

        AaruConsole.WriteLine(Localization.Sending_READ_FULL_TOC_to_the_device);

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

        sense = dev.ReadRawToc(out byte[] buffer, out senseBuffer, 1, dev.Timeout, out _);

        if(sense)
        {
            AaruConsole.WriteLine(Localization.READ_FULL_TOC_failed);
            AaruConsole.WriteLine("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC? decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine(Localization.Could_not_decode_TOC);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        FullTOC.CDFullTOC toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine(Localization.Cannot_find_lead_out);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        int min   = (leadOutTrack.PMIN   >> 4) * 10 + (leadOutTrack.PMIN   & 0x0F);
        int sec   = (leadOutTrack.PSEC   >> 4) * 10 + (leadOutTrack.PSEC   & 0x0F);
        int frame = (leadOutTrack.PFRAME >> 4) * 10 + (leadOutTrack.PFRAME & 0x0F);

        int sectors = min * 60 * 75 + sec * 75 + frame - 150;

        AaruConsole.WriteLine(Localization.Data_disc_shows_0_sectors, sectors);

        AaruConsole.WriteLine(Localization.Ejecting_disc);

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine(Localization.Please_insert_trap_disc_inside);
        AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
        System.Console.ReadLine();

        AaruConsole.WriteLine(Localization.Sending_READ_FULL_TOC_to_the_device);

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
            AaruConsole.WriteLine(Localization.READ_FULL_TOC_failed);
            AaruConsole.WriteLine("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine(Localization.Could_not_decode_TOC);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        toc = decodedToc.Value;

        leadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(leadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine(Localization.Cannot_find_lead_out);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

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

        AaruConsole.WriteLine(Localization.Trap_disc_shows_0_sectors, trapSectors);

        if(trapSectors < sectors + 100)
        {
            AaruConsole.WriteLine(Localization.Trap_disc_doesnt_have_enough_sectors);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        AaruConsole.WriteLine(Localization.Stopping_motor);

        dev.StopUnit(out _, dev.Timeout, out _);

        AaruConsole.WriteLine(Localization.Please_MANUALLY_get_the_trap_disc_out_and_put_the_data_disc_back_inside);
        AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
        System.Console.ReadLine();

        AaruConsole.WriteLine(Localization.Waiting_5_seconds);
        Thread.Sleep(5000);

        AaruConsole.WriteLine(Localization.Sending_READ_FULL_TOC_to_the_device);

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
            AaruConsole.WriteLine(Localization.READ_FULL_TOC_failed);
            AaruConsole.WriteLine("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        decodedToc = FullTOC.Decode(buffer);

        if(decodedToc is null)
        {
            AaruConsole.WriteLine(Localization.Could_not_decode_TOC);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        toc = decodedToc.Value;

        FullTOC.TrackDataDescriptor newLeadOutTrack = toc.TrackDescriptors.FirstOrDefault(t => t.POINT == 0xA2);

        if(newLeadOutTrack.POINT != 0xA2)
        {
            AaruConsole.WriteLine(Localization.Cannot_find_lead_out);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        if(newLeadOutTrack.PMIN >= 0xA0 && !tocIsNotBcd)
            newLeadOutTrack.PMIN -= 0x90;

        if(newLeadOutTrack.PMIN   != leadOutTrack.PMIN ||
           newLeadOutTrack.PSEC   != leadOutTrack.PSEC ||
           newLeadOutTrack.PFRAME != leadOutTrack.PFRAME)
        {
            AaruConsole.WriteLine(Localization.Lead_out_has_changed_this_drive_does_not_support_hot_swapping_discs);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        AaruConsole.Write(Localization.Reading_LBA_0, sectors + 5);

        bool dataResult = dev.ReadCd(out byte[] dataBuffer, out byte[] dataSense, (uint)(sectors + 5), 2352, 1,
                                     MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                     MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(dataResult ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_0_as_audio_scrambled, sectors + 5);

        bool scrambledResult = dev.ReadCd(out byte[] scrambledBuffer, out byte[] scrambledSense, (uint)(sectors + 5),
                                          2352, 1, MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                          false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(scrambledResult ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_0_PQ_subchannel, sectors + 5);

        bool pqResult = dev.ReadCd(out byte[] pqBuffer, out byte[] pqSense, (uint)(sectors + 5), 16, 1,
                                   MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, false, false,
                                   MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout, out _);

        if(pqResult)
        {
            pqResult = dev.ReadCd(out pqBuffer, out pqSense, (uint)(sectors + 5), 16, 1, MmcSectorTypes.AllTypes, false,
                                  false, false, MmcHeaderCodes.None, false, false, MmcErrorField.None,
                                  MmcSubchannel.Q16, dev.Timeout, out _);
        }

        AaruConsole.WriteLine(pqResult ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_0_RW_subchannel, sectors + 5);

        bool rwResult = dev.ReadCd(out byte[] rwBuffer, out byte[] rwSense, (uint)(sectors + 5), 16, 1,
                                   MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, false, false,
                                   MmcErrorField.None, MmcSubchannel.Rw, dev.Timeout, out _);

        if(rwResult)
        {
            rwResult = dev.ReadCd(out rwBuffer, out rwSense, (uint)(sectors + 5), 16, 1, MmcSectorTypes.Cdda, false,
                                  false, false, MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Rw,
                                  dev.Timeout, out _);
        }

        AaruConsole.WriteLine(pqResult ? Localization.FAIL : Localization.Success);

    menu:
        System.Console.Clear();
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(dataResult && scrambledResult
                                  ? Localization.Device_cannot_read_Lead_Out
                                  : Localization.Device_can_read_Lead_Out);

        AaruConsole.WriteLine(Localization.LBA_0_sense_is_1_buffer_is_2_sense_buffer_is_3, sectors + 5, dataResult,
                              dataBuffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(dataBuffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, dataBuffer.Length),
                              dataSense is null                          ? Localization._null :
                              ArrayHelpers.ArrayIsNullOrEmpty(dataSense) ? Localization.empty : $"{dataSense.Length}");

        AaruConsole.WriteLine(Localization.LBA_0_scrambled_sense_is_1_buffer_is_2_sense_buffer_is_3, sectors + 5,
                              scrambledResult,
                              scrambledBuffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(scrambledBuffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, scrambledBuffer.Length),
                              scrambledSense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(scrambledSense)
                                      ?
                                      Localization.empty
                                      : $"{scrambledSense.Length}");

        AaruConsole.WriteLine(Localization.LBA_0_PQ_sense_is_1_buffer_is_2_sense_buffer_is_3, sectors + 5, pqResult,
                              pqBuffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(pqBuffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, pqBuffer.Length),
                              pqSense is null                          ? Localization._null :
                              ArrayHelpers.ArrayIsNullOrEmpty(pqSense) ? Localization.empty : $"{pqSense.Length}");

        AaruConsole.WriteLine(Localization.LBA_0_RW_sense_is_1_buffer_is_2_sense_buffer_is_3, sectors + 5, rwResult,
                              rwBuffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(rwBuffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, rwBuffer.Length),
                              rwSense is null                          ? Localization._null :
                              ArrayHelpers.ArrayIsNullOrEmpty(rwSense) ? Localization.empty : $"{rwSense.Length}");

        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_LBA_0_buffer,                  sectors + 5);
        AaruConsole.WriteLine(Localization._2_Print_LBA_0_sense_buffer,            sectors + 5);
        AaruConsole.WriteLine(Localization._3_Decode_LBA_0_sense_buffer,           sectors + 5);
        AaruConsole.WriteLine(Localization._4_Print_LBA_0_scrambled_buffer,        sectors + 5);
        AaruConsole.WriteLine(Localization._5_Print_LBA_0_scrambled_sense_buffer,  sectors + 5);
        AaruConsole.WriteLine(Localization._6_Decode_LBA_0_scrambled_sense_buffer, sectors + 5);
        AaruConsole.WriteLine(Localization._7_Print_LBA_0_PQ_buffer,               sectors + 5);
        AaruConsole.WriteLine(Localization._8_Print_LBA_0_PQ_sense_buffer,         sectors + 5);
        AaruConsole.WriteLine(Localization._9_Decode_LBA_0_PQ_sense_buffer,        sectors + 5);
        AaruConsole.WriteLine(Localization._10_Print_LBA_0_RW_buffer,              sectors + 5);
        AaruConsole.WriteLine(Localization._11_Print_LBA_0_RW_sense_buffer,        sectors + 5);
        AaruConsole.WriteLine(Localization._12_Decode_LBA_0_RW_sense_buffer,       sectors + 5);
        AaruConsole.WriteLine(Localization._13_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_special_SCSI_MultiMedia_Commands_menu);
        AaruConsole.Write(Localization.Choose);

        string strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_special_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,       devPath);
                AaruConsole.WriteLine(Localization.LBA_0_response, sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(dataBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,    devPath);
                AaruConsole.WriteLine(Localization.LBA_0_sense, sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(dataSense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,            devPath);
                AaruConsole.WriteLine(Localization.LBA_0_decoded_sense, sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(dataSense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,                 devPath);
                AaruConsole.WriteLine(Localization.LBA_0_scrambled_response, sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(scrambledBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 5:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,              devPath);
                AaruConsole.WriteLine(Localization.LBA_0_scrambled_sense, sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(scrambledSense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 6:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,                      devPath);
                AaruConsole.WriteLine(Localization.LBA_0_scrambled_decoded_sense, sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(scrambledSense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 7:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,          devPath);
                AaruConsole.WriteLine(Localization.LBA_PQ_0_response, sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(pqBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 8:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,       devPath);
                AaruConsole.WriteLine(Localization.LBA_PQ_0_sense, sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(pqSense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 9:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,             devPath);
                AaruConsole.WriteLine(Localization.LBA_PQ_decoded_sense, sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(pqSense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 10:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,          devPath);
                AaruConsole.WriteLine(Localization.LBA_RW_0_response, sectors + 5);

                if(buffer != null)
                    PrintHex.PrintHexArray(rwBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 11:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,       devPath);
                AaruConsole.WriteLine(Localization.LBA_RW_0_sense, sectors + 5);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(rwSense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 12:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0,               devPath);
                AaruConsole.WriteLine(Localization.LBA_RW_0_decoded_sense, sectors + 5);
                AaruConsole.Write("{0}", Sense.PrettifySense(rwSense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 13:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}