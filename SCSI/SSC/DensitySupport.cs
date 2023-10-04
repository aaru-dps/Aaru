// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DensitySupport.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI SSC density support structures.
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI.SSC;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class DensitySupport
{
    public static DensitySupportHeader? DecodeDensity(byte[] response)
    {
        if(response is not { Length: > 56 })
            return null;

        var responseLen = (ushort)((response[0] << 8) + response[1] + 2);

        if(response.Length != responseLen)
            return null;

        List<DensitySupportDescriptor> descriptors = new();
        var                            offset      = 4;

        while(offset < response.Length)
        {
            var descriptor = new DensitySupportDescriptor
            {
                primaryCode = response[offset + 0],
                secondaryCode = response[offset + 1],
                writable = (response[offset + 2] & 0x80) == 0x80,
                duplicate = (response[offset + 2] & 0x40) == 0x40,
                defaultDensity = (response[offset + 2] & 0x20) == 0x20,
                reserved = (byte)((response[offset + 2] & 0x1E) >> 1),
                lenvalid = (response[offset + 2] & 0x01) == 0x01,
                len = (ushort)((response[offset + 3] << 8) + response[offset + 4]),
                bpmm = (uint)((response[offset + 5] << 16) + (response[offset + 6] << 8) + response[offset + 7]),
                width = (ushort)((response[offset + 8] << 8) + response[offset + 9]),
                tracks = (ushort)((response[offset + 10] << 8) + response[offset + 11]),
                capacity = (uint)((response[offset + 12] << 24) + (response[offset + 13] << 16) +
                                  (response[offset + 14] << 8)  + response[offset + 15])
            };

            var tmp = new byte[8];
            Array.Copy(response, offset + 16, tmp, 0, 8);
            descriptor.organization = StringHandlers.CToString(tmp).Trim();
            tmp                     = new byte[8];
            Array.Copy(response, offset + 24, tmp, 0, 8);
            descriptor.name = StringHandlers.CToString(tmp).Trim();
            tmp             = new byte[20];
            Array.Copy(response, offset + 32, tmp, 0, 20);
            descriptor.description = StringHandlers.CToString(tmp).Trim();

            if(descriptor.lenvalid)
                offset += descriptor.len + 5;
            else
                offset += 52;

            descriptors.Add(descriptor);
        }

        var decoded = new DensitySupportHeader
        {
            length      = responseLen,
            reserved    = (ushort)((response[2] << 8) + response[3] + 2),
            descriptors = descriptors.ToArray()
        };

        return decoded;
    }

    public static string PrettifyDensity(DensitySupportHeader? density)
    {
        if(density == null)
            return null;

        DensitySupportHeader decoded = density.Value;
        var                  sb      = new StringBuilder();

        foreach(DensitySupportDescriptor descriptor in decoded.descriptors)
        {
            sb.AppendFormat(Localization.Density_0_defined_by_1, descriptor.name, descriptor.organization).AppendLine();

            sb.AppendFormat("\t" + Localization.Primary_code_0, descriptor.primaryCode).AppendLine();

            if(descriptor.primaryCode != descriptor.secondaryCode)
                sb.AppendFormat("\t" + Localization.Secondary_code_0, descriptor.secondaryCode).AppendLine();

            if(descriptor.writable)
                sb.AppendLine("\t" + Localization.Drive_can_write_this_density);

            if(descriptor.duplicate)
                sb.AppendLine("\t" + Localization.This_descriptor_is_duplicated);

            if(descriptor.defaultDensity)
                sb.AppendLine("\t" + Localization.This_is_the_default_density_on_the_drive);

            sb.AppendFormat("\t" + Localization.Density_has_0_bits_per_mm__with_1_tracks_in_a_2_mm_width_tape,
                            descriptor.bpmm, descriptor.tracks, descriptor.width / (double)10).AppendLine();

            sb.AppendFormat("\t" + Localization.Density_maximum_capacity_is_0_megabytes, descriptor.capacity).
               AppendLine();

            sb.AppendFormat("\t" + Localization.Density_description_0, descriptor.description).AppendLine();
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string PrettifyDensity(byte[] response) => PrettifyDensity(DecodeDensity(response));

    public static MediaTypeSupportHeader? DecodeMediumType(byte[] response)
    {
        if(response is not { Length: > 60 })
            return null;

        var responseLen = (ushort)((response[0] << 8) + response[1] + 2);

        if(response.Length != responseLen)
            return null;

        List<MediaTypeSupportDescriptor> descriptors = new();
        var                              offset      = 4;

        while(offset < response.Length)
        {
            var descriptor = new MediaTypeSupportDescriptor
            {
                mediumType = response[offset + 0],
                reserved1  = response[offset + 1],
                len        = (ushort)((response[offset + 2] << 8) + response[offset + 3])
            };

            if(descriptor.len != 52)
                return null;

            descriptor.numberOfCodes = response[offset + 4];
            descriptor.densityCodes  = new byte[9];
            Array.Copy(response, offset                                  + 5, descriptor.densityCodes, 0, 9);
            descriptor.width     = (ushort)((response[offset + 14] << 8) + response[offset + 15]);
            descriptor.length    = (ushort)((response[offset + 16] << 8) + response[offset + 17]);
            descriptor.reserved1 = response[offset + 18];
            descriptor.reserved1 = response[offset + 19];
            var tmp = new byte[8];
            Array.Copy(response, offset + 20, tmp, 0, 8);
            descriptor.organization = StringHandlers.CToString(tmp).Trim();
            tmp                     = new byte[8];
            Array.Copy(response, offset + 28, tmp, 0, 8);
            descriptor.name = StringHandlers.CToString(tmp).Trim();
            tmp             = new byte[20];
            Array.Copy(response, offset + 36, tmp, 0, 20);
            descriptor.description = StringHandlers.CToString(tmp).Trim();

            offset += 56;

            descriptors.Add(descriptor);
        }

        var decoded = new MediaTypeSupportHeader
        {
            length      = responseLen,
            reserved    = (ushort)((response[2] << 8) + response[3] + 2),
            descriptors = descriptors.ToArray()
        };

        return decoded;
    }

    public static string PrettifyMediumType(MediaTypeSupportHeader? mediumType)
    {
        if(mediumType == null)
            return null;

        MediaTypeSupportHeader decoded = mediumType.Value;
        var                    sb      = new StringBuilder();

        foreach(MediaTypeSupportDescriptor descriptor in decoded.descriptors)
        {
            sb.AppendFormat(Localization.Medium_type_0_defined_by_1, descriptor.name, descriptor.organization).
               AppendLine();

            sb.AppendFormat("\t" + Localization.Medium_type_code_0, descriptor.mediumType).AppendLine();

            if(descriptor.numberOfCodes > 0)
            {
                sb.AppendFormat("\t" + Localization.Medium_supports_following_density_codes);

                for(var i = 0; i < descriptor.numberOfCodes; i++)
                    sb.Append($" {descriptor.densityCodes[i]:X2}h");

                sb.AppendLine();
            }

            sb.AppendFormat("\t" + Localization.Medium_has_a_nominal_length_of_0_m_in_a_1_mm_width_tape,
                            descriptor.length, descriptor.width / (double)10).AppendLine();

            sb.AppendFormat("\t" + Localization.Medium_description_0, descriptor.description).AppendLine();
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string PrettifyMediumType(byte[] response) => PrettifyMediumType(DecodeMediumType(response));

#region Nested type: DensitySupportDescriptor

    public struct DensitySupportDescriptor
    {
        public byte   primaryCode;
        public byte   secondaryCode;
        public bool   writable;
        public bool   duplicate;
        public bool   defaultDensity;
        public byte   reserved;
        public bool   lenvalid;
        public ushort len;
        public uint   bpmm;
        public ushort width;
        public ushort tracks;
        public uint   capacity;
        public string organization;
        public string name;
        public string description;
    }

#endregion

#region Nested type: DensitySupportHeader

    public struct DensitySupportHeader
    {
        public ushort                     length;
        public ushort                     reserved;
        public DensitySupportDescriptor[] descriptors;
    }

#endregion

#region Nested type: MediaTypeSupportDescriptor

    public struct MediaTypeSupportDescriptor
    {
        public byte   mediumType;
        public byte   reserved1;
        public ushort len;
        public byte   numberOfCodes;
        public byte[] densityCodes;
        public ushort width;
        public ushort length;
        public byte   reserved2;
        public byte   reserved3;
        public string organization;
        public string name;
        public string description;
    }

#endregion

#region Nested type: MediaTypeSupportHeader

    public struct MediaTypeSupportHeader
    {
        public ushort                       length;
        public ushort                       reserved;
        public MediaTypeSupportDescriptor[] descriptors;
    }

#endregion
}