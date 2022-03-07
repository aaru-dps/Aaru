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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.SCSI;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

        sb.AppendLine("SCSI medium partition page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        sb.AppendFormat("\t{0} maximum additional partitions", page.MaxAdditionalPartitions).AppendLine();
        sb.AppendFormat("\t{0} additional partitions defined", page.AdditionalPartitionsDefined).AppendLine();

        if(page.FDP)
            sb.AppendLine("\tPartitions are fixed under device definitions");

        if(page.SDP)
            sb.AppendLine("\tNumber of partitions can be defined but their size is defined by the device");

        if(page.IDP)
            sb.AppendLine("\tNumber and size of partitions can be manually defined");

        if(page.POFM)
            sb.AppendLine("\tPartition parameters will not be applied until a FORMAT MEDIUM command is received");

        if(!page.CLEAR &&
           !page.ADDP)
            sb.AppendLine("\tDevice may erase any or all partitions on MODE SELECT for partitioning");
        else if(page.CLEAR &&
                !page.ADDP)
            sb.AppendLine("\tDevice shall erase all partitions on MODE SELECT for partitioning");
        else if(!page.CLEAR)
            sb.AppendLine("\tDevice shall not erase any partition on MODE SELECT for partitioning");
        else
            sb.AppendLine("\tDevice shall erase all partitions differing on size on MODE SELECT for partitioning");

        string measure;

        switch(page.PSUM)
        {
            case PartitionSizeUnitOfMeasures.Bytes:
                sb.AppendLine("\tPartitions are defined in bytes");
                measure = "bytes";

                break;
            case PartitionSizeUnitOfMeasures.Kilobytes:
                sb.AppendLine("\tPartitions are defined in kilobytes");
                measure = "kilobytes";

                break;
            case PartitionSizeUnitOfMeasures.Megabytes:
                sb.AppendLine("\tPartitions are defined in megabytes");
                measure = "megabytes";

                break;
            case PartitionSizeUnitOfMeasures.Exponential:
                sb.AppendFormat("\tPartitions are defined in units of {0} bytes", Math.Pow(10, page.PartitionUnits)).
                   AppendLine();

                measure = $"units of {Math.Pow(10, page.PartitionUnits)} bytes";

                break;
            default:
                sb.AppendFormat("\tUnknown partition size unit code {0}", (byte)page.PSUM).AppendLine();
                measure = "units";

                break;
        }

        switch(page.MediumFormatRecognition)
        {
            case MediumFormatRecognitionValues.Capable:
                sb.AppendLine("\tDevice is capable of recognizing both medium partitions and format");

                break;
            case MediumFormatRecognitionValues.FormatCapable:
                sb.AppendLine("\tDevice is capable of recognizing medium format");

                break;
            case MediumFormatRecognitionValues.PartitionCapable:
                sb.AppendLine("\tDevice is capable of recognizing medium partitions");

                break;
            case MediumFormatRecognitionValues.Incapable:
                sb.AppendLine("\tDevice is not capable of recognizing neither medium partitions nor format");

                break;
            default:
                sb.AppendFormat("\tUnknown medium recognition code {0}", (byte)page.MediumFormatRecognition).
                   AppendLine();

                break;
        }

        sb.AppendFormat("\tMedium has defined {0} partitions", page.PartitionSizes.Length).AppendLine();

        for(var i = 0; i < page.PartitionSizes.Length; i++)
            if(page.PartitionSizes[i] == 0)
                if(page.PartitionSizes.Length == 1)
                    sb.AppendLine("\tDevice recognizes one single partition spanning whole medium");
                else
                    sb.AppendFormat("\tPartition {0} runs for rest of medium", i).AppendLine();
            else
                sb.AppendFormat("\tPartition {0} is {1} {2} long", i, page.PartitionSizes[i], measure).AppendLine();

        return sb.ToString();
    }
    #endregion Mode Page 0x11: Medium partition page (1)
}