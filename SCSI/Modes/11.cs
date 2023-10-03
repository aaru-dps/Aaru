// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 11.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 11h: Medium partition page (1).
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
#region Mode Page 0x11: Medium partition page (1)

    public enum PartitionSizeUnitOfMeasures : byte
    {
        /// <summary>Partition size is measures in bytes</summary>
        Bytes = 0,
        /// <summary>Partition size is measures in Kilobytes</summary>
        Kilobytes = 1,
        /// <summary>Partition size is measures in Megabytes</summary>
        Megabytes = 2,
        /// <summary>Partition size is 10eUNITS bytes</summary>
        Exponential = 3
    }

    public enum MediumFormatRecognitionValues : byte
    {
        /// <summary>Logical unit is incapable of format or partition recognition</summary>
        Incapable = 0,
        /// <summary>Logical unit is capable of format recognition only</summary>
        FormatCapable = 1,
        /// <summary>Logical unit is capable of partition recognition only</summary>
        PartitionCapable = 2,
        /// <summary>Logical unit is capable of both format and partition recognition</summary>
        Capable = 3
    }

    /// <summary>Medium partition page(1) Page code 0x11</summary>
    public struct ModePage_11
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Maximum number of additional partitions supported</summary>
        public byte MaxAdditionalPartitions;
        /// <summary>Number of additional partitions to be defined for a volume</summary>
        public byte AdditionalPartitionsDefined;
        /// <summary>Device defines partitions based on its fixed definition</summary>
        public bool FDP;
        /// <summary>Device should divide medium according to the additional partitions defined field using sizes defined by device</summary>
        public bool SDP;
        /// <summary>Initiator defines number and size of partitions</summary>
        public bool IDP;
        /// <summary>Defines the unit on which the partition sizes are defined</summary>
        public PartitionSizeUnitOfMeasures PSUM;
        public bool POFM;
        public bool CLEAR;
        public bool ADDP;
        /// <summary>Defines the capabilities for the unit to recognize media partitions and format</summary>
        public MediumFormatRecognitionValues MediumFormatRecognition;
        public byte PartitionUnits;
        /// <summary>Array of partition sizes in units defined above</summary>
        public ushort[] PartitionSizes;
    }

    public static ModePage_11? DecodeModePage_11(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x11)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 8)
            return null;

        var decoded = new ModePage_11();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.MaxAdditionalPartitions     =  pageResponse[2];
        decoded.AdditionalPartitionsDefined =  pageResponse[3];
        decoded.FDP                         |= (pageResponse[4] & 0x80) == 0x80;
        decoded.SDP                         |= (pageResponse[4] & 0x40) == 0x40;
        decoded.IDP                         |= (pageResponse[4] & 0x20) == 0x20;
        decoded.PSUM                        =  (PartitionSizeUnitOfMeasures)((pageResponse[4] & 0x18) >> 3);
        decoded.POFM                        |= (pageResponse[4]       & 0x04) == 0x04;
        decoded.CLEAR                       |= (pageResponse[4]       & 0x02) == 0x02;
        decoded.ADDP                        |= (pageResponse[4]       & 0x01) == 0x01;
        decoded.PartitionUnits              =  (byte)(pageResponse[6] & 0x0F);
        decoded.MediumFormatRecognition     =  (MediumFormatRecognitionValues)pageResponse[5];
        decoded.PartitionSizes              =  new ushort[(pageResponse.Length - 8) / 2];

        for(var i = 8; i < pageResponse.Length; i += 2)
        {
            decoded.PartitionSizes[(i - 8) / 2] =  (ushort)(pageResponse[i] << 8);
            decoded.PartitionSizes[(i - 8) / 2] += pageResponse[i + 1];
        }

        return decoded;
    }

    public static string PrettifyModePage_11(byte[] pageResponse) =>
        PrettifyModePage_11(DecodeModePage_11(pageResponse));

    public static string PrettifyModePage_11(ModePage_11? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_11 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine(Localization.SCSI_medium_partition_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        sb.AppendFormat("\t" + Localization._0_maximum_additional_partitions, page.MaxAdditionalPartitions).
           AppendLine();

        sb.AppendFormat("\t" + Localization._0_additional_partitions_defined, page.AdditionalPartitionsDefined).
           AppendLine();

        if(page.FDP)
            sb.AppendLine("\t" + Localization.Partitions_are_fixed_under_device_definitions);

        if(page.SDP)
        {
            sb.AppendLine("\t" + Localization.
                              Number_of_partitions_can_be_defined_but_their_size_is_defined_by_the_device);
        }

        if(page.IDP)
            sb.AppendLine("\t" + Localization.Number_and_size_of_partitions_can_be_manually_defined);

        if(page.POFM)
        {
            sb.AppendLine("\t" + Localization.
                              Partition_parameters_will_not_be_applied_until_a_FORMAT_MEDIUM_command_is_received);
        }

        switch(page.CLEAR)
        {
            case false when !page.ADDP:
                sb.AppendLine("\t" + Localization.
                                  Device_may_erase_any_or_all_partitions_on_MODE_SELECT_for_partitioning);

                break;
            case true when !page.ADDP:
                sb.AppendLine("\t" + Localization.Device_shall_erase_all_partitions_on_MODE_SELECT_for_partitioning);

                break;
            case false:
                sb.AppendLine("\t" + Localization.Device_shall_not_erase_any_partition_on_MODE_SELECT_for_partitioning);

                break;
            default:
                sb.AppendLine("\t" + Localization.
                                  Device_shall_erase_all_partitions_differing_on_size_on_MODE_SELECT_for_partitioning);

                break;
        }

        string measure;

        switch(page.PSUM)
        {
            case PartitionSizeUnitOfMeasures.Bytes:
                sb.AppendLine("\t" + Localization.Partitions_are_defined_in_bytes);
                measure = Localization.bytes;

                break;
            case PartitionSizeUnitOfMeasures.Kilobytes:
                sb.AppendLine("\t" + Localization.Partitions_are_defined_in_kilobytes);
                measure = Localization.kilobytes;

                break;
            case PartitionSizeUnitOfMeasures.Megabytes:
                sb.AppendLine("\t" + Localization.Partitions_are_defined_in_megabytes);
                measure = Localization.megabytes;

                break;
            case PartitionSizeUnitOfMeasures.Exponential:
                sb.AppendFormat("\t" + Localization.Partitions_are_defined_in_units_of_0_bytes,
                                Math.Pow(10, page.PartitionUnits)).AppendLine();

                measure = string.Format(Localization.units_of_0_bytes, Math.Pow(10, page.PartitionUnits));

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_partition_size_unit_code_0, (byte)page.PSUM).AppendLine();
                measure = Localization.units;

                break;
        }

        switch(page.MediumFormatRecognition)
        {
            case MediumFormatRecognitionValues.Capable:
                sb.AppendLine("\t" + Localization.Device_is_capable_of_recognizing_both_medium_partitions_and_format);

                break;
            case MediumFormatRecognitionValues.FormatCapable:
                sb.AppendLine("\t" + Localization.Device_is_capable_of_recognizing_medium_format);

                break;
            case MediumFormatRecognitionValues.PartitionCapable:
                sb.AppendLine("\t" + Localization.Device_is_capable_of_recognizing_medium_partitions);

                break;
            case MediumFormatRecognitionValues.Incapable:
                sb.AppendLine("\t" + Localization.
                                  Device_is_not_capable_of_recognizing_neither_medium_partitions_nor_format);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_medium_recognition_code_0,
                                (byte)page.MediumFormatRecognition).AppendLine();

                break;
        }

        sb.AppendFormat("\t" + Localization.Medium_has_defined_0_partitions, page.PartitionSizes.Length).AppendLine();

        for(var i = 0; i < page.PartitionSizes.Length; i++)
        {
            if(page.PartitionSizes[i] == 0)
            {
                if(page.PartitionSizes.Length == 1)
                    sb.AppendLine("\t" + Localization.Device_recognizes_one_single_partition_spanning_whole_medium);
                else
                    sb.AppendFormat("\t" + Localization.Partition_0_runs_for_rest_of_medium, i).AppendLine();
            }
            else
            {
                sb.AppendFormat("\t" + Localization.Partition_0_is_1_2_long, i, page.PartitionSizes[i], measure).
                   AppendLine();
            }
        }

        return sb.ToString();
    }

#endregion Mode Page 0x11: Medium partition page (1)
}