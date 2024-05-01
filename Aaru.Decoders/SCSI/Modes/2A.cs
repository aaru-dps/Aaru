// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 2Ah: CD-ROM capabilities page.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI.Modes;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static partial class Modes
{
#region Mode Page 0x2A: CD-ROM capabilities page

    public static string PrettifyModePage_2A(byte[] pageResponse) =>
        PrettifyModePage_2A(ModePage_2A.Decode(pageResponse));

    public static string PrettifyModePage_2A(ModePage_2A modePage)
    {
        if(modePage is null) return null;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.SCSI_CD_ROM_capabilities_page);

        if(modePage.PS) sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(modePage.AudioPlay) sb.AppendLine("\t" + Localization.Drive_can_play_audio);

        if(modePage.Mode2Form1) sb.AppendLine("\t" + Localization.Drive_can_read_sectors_in_Mode_2_Form_1_format);

        if(modePage.Mode2Form2) sb.AppendLine("\t" + Localization.Drive_can_read_sectors_in_Mode_2_Form_2_format);

        if(modePage.MultiSession) sb.AppendLine("\t" + Localization.Drive_supports_multi_session_discs_and_or_Photo_CD);

        if(modePage.CDDACommand) sb.AppendLine("\t" + Localization.Drive_can_read_digital_audio);

        if(modePage.AccurateCDDA) sb.AppendLine("\t" + Localization.Drive_can_continue_from_streaming_loss);

        if(modePage.Subchannel)
            sb.AppendLine("\t" + Localization.Drive_can_read_uncorrected_and_interleaved_R_W_subchannels);

        if(modePage.DeinterlaveSubchannel)
            sb.AppendLine("\t" + Localization.Drive_can_read__deinterleave_and_correct_R_W_subchannels);

        if(modePage.C2Pointer) sb.AppendLine("\t" + Localization.Drive_supports_C2_pointers);

        if(modePage.UPC) sb.AppendLine("\t" + Localization.Drive_can_read_Media_Catalogue_Number);

        if(modePage.ISRC) sb.AppendLine("\t" + Localization.Drive_can_read_ISRC);

        switch(modePage.LoadingMechanism)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Drive_uses_media_caddy);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Drive_uses_a_tray);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Drive_is_pop_up);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.Drive_is_a_changer_with_individually_changeable_discs);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Drive_is_a_changer_using_cartridges);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Drive_uses_unknown_loading_mechanism_type__0_,
                                modePage.LoadingMechanism)
                  .AppendLine();

                break;
        }

        if(modePage.Lock) sb.AppendLine("\t" + Localization.Drive_can_lock_media);

        if(modePage.PreventJumper)
        {
            sb.AppendLine("\t" + Localization.Drive_power_ups_locked);

            sb.AppendLine(modePage.LockState
                              ? "\t" + Localization.Drive_is_locked__media_cannot_be_ejected_or_inserted
                              : "\t" + Localization.Drive_is_not_locked__media_can_be_ejected_and_inserted);
        }
        else
        {
            sb.AppendLine(modePage.LockState
                              ? "\t" +
                                Localization.Drive_is_locked__media_cannot_be_ejected__but_if_empty__can_be_inserted
                              : "\t" + Localization.Drive_is_not_locked__media_can_be_ejected_and_inserted);
        }

        if(modePage.Eject) sb.AppendLine("\t" + Localization.Drive_can_eject_media);

        if(modePage.SeparateChannelMute) sb.AppendLine("\t" + Localization.Each_channel_can_be_muted_independently);

        if(modePage.SeparateChannelVolume)
            sb.AppendLine("\t" + Localization.Each_channel_s_volume_can_be_controlled_independently);

        if(modePage.SupportedVolumeLevels > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_supports_0_volume_levels, modePage.SupportedVolumeLevels)
              .AppendLine();
        }

        if(modePage.BufferSize > 0)
            sb.AppendFormat("\t" + Localization.Drive_has_0_Kbyte_of_buffer, modePage.BufferSize).AppendLine();

        if(modePage.MaximumSpeed > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_maximum_reading_speed_is_0_Kbyte_sec, modePage.MaximumSpeed)
              .AppendLine();
        }

        if(modePage.CurrentSpeed > 0)
        {
            sb.AppendFormat("\t" + Localization.Drive_current_reading_speed_is_0_Kbyte_sec, modePage.CurrentSpeed)
              .AppendLine();
        }

        if(modePage.ReadCDR)
        {
            sb.AppendLine(modePage.WriteCDR
                              ? "\t" + Localization.Drive_can_read_and_write_CD_R
                              : "\t" + Localization.Drive_can_read_CD_R);

            if(modePage.Method2) sb.AppendLine("\t" + Localization.Drive_supports_reading_CD_R_packet_media);
        }

        if(modePage.ReadCDRW)
        {
            sb.AppendLine(modePage.WriteCDRW
                              ? "\t" + Localization.Drive_can_read_and_write_CD_RW
                              : "\t" + Localization.Drive_can_read_CD_RW);
        }

        if(modePage.ReadDVDROM) sb.AppendLine("\t" + Localization.Drive_can_read_DVD_ROM);

        if(modePage.ReadDVDR)
        {
            sb.AppendLine(modePage.WriteDVDR
                              ? "\t" + Localization.Drive_can_read_and_write_DVD_R
                              : "\t" + Localization.Drive_can_read_DVD_R);
        }

        if(modePage.ReadDVDRAM)
        {
            sb.AppendLine(modePage.WriteDVDRAM
                              ? "\t" + Localization.Drive_can_read_and_write_DVD_RAM
                              : "\t" + Localization.Drive_can_read_DVD_RAM);
        }

        if(modePage.Composite)
            sb.AppendLine("\t" + Localization.Drive_can_deliver_a_composite_audio_and_video_data_stream);

        if(modePage.DigitalPort1) sb.AppendLine("\t" + Localization.Drive_supports_IEC_958_digital_output_on_port_1);

        if(modePage.DigitalPort2) sb.AppendLine("\t" + Localization.Drive_supports_IEC_958_digital_output_on_port_2);

        if(modePage.SDP)
            sb.AppendLine("\t" + Localization.Drive_contains_a_changer_that_can_report_the_exact_contents_of_the_slots);

        if(modePage.CurrentWriteSpeedSelected > 0)
        {
            switch(modePage.RotationControlSelected)
            {
                case 0:
                    sb.AppendFormat("\t" + Localization.Drive_current_writing_speed_is_0_Kbyte_sec_in_CLV_mode,
                                    modePage.CurrentWriteSpeedSelected)
                      .AppendLine();

                    break;
                case 1:
                    sb.AppendFormat("\t" + Localization.Drive_current_writing_speed_is_0_Kbyte_sec_in_pure_CAV_mode,
                                    modePage.CurrentWriteSpeedSelected)
                      .AppendLine();

                    break;
            }
        }
        else
        {
            if(modePage.MaxWriteSpeed > 0)
            {
                sb.AppendFormat("\t" + Localization.Drive_maximum_writing_speed_is_0_Kbyte_sec, modePage.MaxWriteSpeed)
                  .AppendLine();
            }

            if(modePage.CurrentWriteSpeed > 0)
            {
                sb.AppendFormat("\t" + Localization.Drive_current_writing_speed_is_0_Kbyte_sec,
                                modePage.CurrentWriteSpeed)
                  .AppendLine();
            }
        }

        if(modePage.WriteSpeedPerformanceDescriptors != null)
        {
            foreach(ModePage_2A_WriteDescriptor descriptor in
                    modePage.WriteSpeedPerformanceDescriptors.Where(descriptor => descriptor.WriteSpeed > 0))
            {
                switch(descriptor.RotationControl)
                {
                    case 0:
                        sb.AppendFormat("\t" + Localization.Drive_supports_writing_at_0_Kbyte_sec_in_CLV_mode,
                                        descriptor.WriteSpeed)
                          .AppendLine();

                        break;
                    case 1:
                        sb.AppendFormat("\t" + Localization.Drive_supports_writing_at_is_0_Kbyte_sec_in_pure_CAV_mode,
                                        descriptor.WriteSpeed)
                          .AppendLine();

                        break;
                }
            }
        }

        if(modePage.TestWrite) sb.AppendLine("\t" + Localization.Drive_supports_test_writing);

        if(modePage.ReadBarcode) sb.AppendLine("\t" + Localization.Drive_can_read_barcode);

        if(modePage.SCC) sb.AppendLine("\t" + Localization.Drive_can_read_both_sides_of_a_disc);

        if(modePage.LeadInPW) sb.AppendLine("\t" + Localization.Drive_an_read_raw_R_W_subchannel_from_the_Lead_In);

        if(modePage.CMRSupported == 1) sb.AppendLine("\t" + Localization.Drive_supports_DVD_CSS_and_or_DVD_CPPM);

        if(modePage.BUF) sb.AppendLine("\t" + Localization.Drive_supports_buffer_under_run_free_recording);

        return sb.ToString();
    }

#endregion Mode Page 0x2A: CD-ROM capabilities page
}