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
    static void CheckGdromReadability(string devPath, Device dev)
    {
        var    tocIsNotBcd = false;
        bool   sense;
        byte[] senseBuffer;

    start:
        System.Console.Clear();

        AaruConsole.WriteLine(Localization.Ejecting_disc);

        dev.AllowMediumRemoval(out _, dev.Timeout, out _);
        dev.EjectTray(out _, dev.Timeout, out _);

        AaruConsole.WriteLine(Localization.Please_insert_trap_disc_inside);
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

        int min = 0, sec, frame;

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

        AaruConsole.WriteLine(Localization.Trap_disc_shows_0_sectors, sectors);

        if(sectors < 450000)
        {
            AaruConsole.WriteLine(Localization.Trap_disc_doesnt_have_enough_sectors);
            AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
            System.Console.ReadLine();

            return;
        }

        AaruConsole.WriteLine(Localization.Stopping_motor);

        dev.StopUnit(out _, dev.Timeout, out _);

        AaruConsole.WriteLine(Localization.Please_MANUALLY_get_the_trap_disc_out_and_put_the_GD_ROM_disc_inside);
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

        dev.SetCdSpeed(out _, RotationalControl.PureCav, 170, 0, dev.Timeout, out _);

        AaruConsole.Write(Localization.Reading_LBA_zero);

        bool lba0Result = dev.ReadCd(out byte[] lba0Buffer, out byte[] lba0Sense, 0, 2352, 1, MmcSectorTypes.AllTypes,
                                     false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                     MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba0Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_zero_as_audio_scrambled);

        bool lba0ScrambledResult = dev.ReadCd(out byte[] lba0ScrambledBuffer, out byte[] lba0ScrambledSense, 0, 2352, 1,
                                              MmcSectorTypes.Cdda, false, false, false, MmcHeaderCodes.None, true,
                                              false, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba0ScrambledResult ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_100000);

        bool lba100000Result = dev.ReadCd(out byte[] lba100000Buffer, out byte[] lba100000Sense, 100000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                          true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba100000Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_50000);

        bool lba50000Result = dev.ReadCd(out byte[] lba50000Buffer, out byte[] lba50000Sense, 50000, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                         true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba50000Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_450000);

        bool lba450000Result = dev.ReadCd(out byte[] lba450000Buffer, out byte[] lba450000Sense, 450000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                          true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba450000Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_400000);

        bool lba400000Result = dev.ReadCd(out byte[] lba400000Buffer, out byte[] lba400000Sense, 400000, 2352, 1,
                                          MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                          true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba400000Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_45000);

        bool lba45000Result = dev.ReadCd(out byte[] lba45000Buffer, out byte[] lba45000Sense, 45000, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                         true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba45000Result ? Localization.FAIL : Localization.Success);

        AaruConsole.Write(Localization.Reading_LBA_44990);

        bool lba44990Result = dev.ReadCd(out byte[] lba44990Buffer, out byte[] lba44990Sense, 44990, 2352, 1,
                                         MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                         true, MmcErrorField.None, MmcSubchannel.None, dev.Timeout, out _);

        AaruConsole.WriteLine(lba44990Result ? Localization.FAIL : Localization.Success);

    menu:
        System.Console.Clear();
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(lba450000Result
                                  ? Localization.Device_cannot_read_HD_area
                                  : Localization.Device_can_read_HD_area);

        AaruConsole.WriteLine(Localization.LBA_zero_sense_is_0_buffer_is_1_sense_buffer_is_2, lba0Result,
                              lba0Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba0Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba0Buffer.Length),
                              lba0Sense is null                          ? Localization._null :
                              ArrayHelpers.ArrayIsNullOrEmpty(lba0Sense) ? Localization.empty : $"{lba0Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_zero_scrambled_sense_is_0_buffer_is_1_sense_buffer_is_2_,
                              lba0ScrambledResult,
                              lba0ScrambledBuffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba0ScrambledBuffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba0ScrambledBuffer.Length),
                              lba0ScrambledSense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba0ScrambledSense)
                                      ?
                                      Localization.empty
                                      : $"{lba0ScrambledSense.Length}");

        AaruConsole.WriteLine(Localization.LBA_44990_sense_is_0_buffer_is_1_sense_buffer_is_2, lba44990Result,
                              lba44990Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba44990Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba44990Buffer.Length),
                              lba44990Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba44990Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba44990Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_45000_sense_is_0_buffer_is_1_sense_buffer_is_2, lba45000Result,
                              lba45000Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba45000Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba45000Buffer.Length),
                              lba45000Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba45000Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba45000Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_50000_sense_is_0_buffer_is_1_sense_buffer_is_2, lba50000Result,
                              lba50000Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba50000Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba50000Buffer.Length),
                              lba50000Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba50000Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba50000Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_100000_sense_is_0_buffer_is_1_sense_buffer_is_2, lba100000Result,
                              lba100000Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba100000Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba100000Buffer.Length),
                              lba100000Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba100000Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba100000Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_400000_sense_is_0_buffer_is_1_sense_buffer_is_2, lba400000Result,
                              lba400000Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba400000Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba400000Buffer.Length),
                              lba400000Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba400000Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba400000Sense.Length}");

        AaruConsole.WriteLine(Localization.LBA_450000_sense_is_0_buffer_is_1_sense_buffer_is_2, lba450000Result,
                              lba450000Buffer is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba450000Buffer)
                                      ?
                                      Localization.empty
                                      : string.Format(Localization._0_bytes, lba450000Buffer.Length),
                              lba450000Sense is null
                                  ? Localization._null
                                  :
                                  ArrayHelpers.ArrayIsNullOrEmpty(lba450000Sense)
                                      ?
                                      Localization.empty
                                      : $"{lba450000Sense.Length}");

        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_LBA_zero_buffer);
        AaruConsole.WriteLine(Localization._2_Print_LBA_zero_sense_buffer);
        AaruConsole.WriteLine(Localization._3_Decode_LBA_zero_sense_buffer);
        AaruConsole.WriteLine(Localization._4_Print_LBA_zero_scrambled_buffer);
        AaruConsole.WriteLine(Localization._5_Print_LBA_zero_scrambled_sense_buffer);
        AaruConsole.WriteLine(Localization._6_Decode_LBA_zero_scrambled_sense_buffer);
        AaruConsole.WriteLine(Localization._7_Print_LBA_44990_buffer);
        AaruConsole.WriteLine(Localization._8_Print_LBA_44990_sense_buffer);
        AaruConsole.WriteLine(Localization._9_Decode_LBA_44990_sense_buffer);
        AaruConsole.WriteLine(Localization._10_Print_LBA_45000_buffer);
        AaruConsole.WriteLine(Localization._11_Print_LBA_45000_sense_buffer);
        AaruConsole.WriteLine(Localization._12_Decode_LBA_45000_sense_buffer);
        AaruConsole.WriteLine(Localization._13_Print_LBA_50000_buffer);
        AaruConsole.WriteLine(Localization._14_Print_LBA_50000_sense_buffer);
        AaruConsole.WriteLine(Localization._15_Decode_LBA_50000_sense_buffer);
        AaruConsole.WriteLine(Localization._16_Print_LBA_100000_buffer);
        AaruConsole.WriteLine(Localization._17_Print_LBA_100000_sense_buffer);
        AaruConsole.WriteLine(Localization._18_Decode_LBA_100000_sense_buffer);
        AaruConsole.WriteLine(Localization._19_Print_LBA_400000_buffer);
        AaruConsole.WriteLine(Localization._20_Print_LBA_400000_sense_buffer);
        AaruConsole.WriteLine(Localization._21_Decode_LBA_400000_sense_buffer);
        AaruConsole.WriteLine(Localization._22_Print_LBA_450000_buffer);
        AaruConsole.WriteLine(Localization._23_Print_LBA_450000_sense_buffer);
        AaruConsole.WriteLine(Localization._24_Decode_LBA_450000_sense_buffer);
        AaruConsole.WriteLine(Localization._25_Send_command_again);
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
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba0Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba0Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba0Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_scrambled_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba0ScrambledBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 5:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_scrambled_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba0ScrambledSense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 6:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_zero_scrambled_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba0ScrambledSense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 7:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_44990_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba44990Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 8:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_44990_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba44990Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 9:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_44990_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba44990Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 10:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_45000_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba45000Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 11:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_45000_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba45000Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 12:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_45000_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba45000Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 13:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_50000_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba50000Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 14:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_50000_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba50000Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 15:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_50000_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba50000Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 16:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_100000_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba100000Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 17:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_100000_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba100000Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 18:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_100000_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba100000Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 19:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_400000_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba400000Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 20:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_400000_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba400000Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 21:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_400000_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba400000Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 22:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_450000_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(lba450000Buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 23:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_450000_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(lba450000Sense, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 24:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LBA_450000_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(lba450000Sense));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();

                goto menu;
            case 25:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}