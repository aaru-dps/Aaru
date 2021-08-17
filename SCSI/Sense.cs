// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sense.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI SENSE.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Decoders.ATA;

namespace Aaru.Decoders.SCSI
{
    public enum SenseType
    {
        StandardSense, ExtendedSenseFixedCurrent, ExtendedSenseFixedPast,
        ExtendedSenseDescriptorCurrent, ExtendedSenseDescriptorPast, Invalid,
        Unknown
    }

    public struct DecodedSense
    {
        public          FixedSense?      Fixed;
        public          DescriptorSense? Descriptor;
        public readonly byte             ASC         => Descriptor?.ASC      ?? (Fixed?.ASC      ?? 0);
        public readonly byte             ASCQ        => Descriptor?.ASCQ     ?? (Fixed?.ASCQ     ?? 0);
        public readonly SenseKeys        SenseKey    => Descriptor?.SenseKey ?? (Fixed?.SenseKey ?? SenseKeys.NoSense);
        public readonly string           Description => Sense.GetSenseDescription(ASC, ASCQ);
    }

    [SuppressMessage("ReSharper", "MemberCanBeInternal"), SuppressMessage("ReSharper", "NotAccessedField.Global"),
     SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct StandardSense
    {
        /// <summary>If set, <see cref="LBA" /> is valid</summary>
        public bool AddressValid;
        /// <summary>Error class, 0 to 6</summary>
        public byte ErrorClass;
        /// <summary>Error type</summary>
        public byte ErrorType;
        /// <summary>Private usage</summary>
        public byte Private;
        /// <summary>LBA where error happened</summary>
        public uint LBA;
    }

    public enum SenseKeys : byte
    {
        /// <summary>No information to be reported, but bits should be checked</summary>
        NoSense = 0,
        /// <summary>Target performed some recovery to successfully complete last command</summary>
        RecoveredError = 1,
        /// <summary>Target is not ready</summary>
        NotReady = 2,
        /// <summary>Non-recoverable medium error occurred</summary>
        MediumError = 3,
        /// <summary>Non-recoverable hardware error occurred</summary>
        HardwareError = 4,
        /// <summary>Target has received an illegal request</summary>
        IllegalRequest = 5,
        /// <summary>Target requires initiator attention</summary>
        UnitAttention = 6,
        /// <summary>A protected command has been denied</summary>
        DataProtect = 7,
        /// <summary>A blank block has been tried to read or a non-rewritable one to write</summary>
        BlankCheck = 8,
        /// <summary>For private/vendor usage</summary>
        PrivateUse = 9,
        /// <summary>COPY command aborted</summary>
        CopyAborted = 0xA,
        /// <summary>Command aborted</summary>
        AbortedCommand = 0xB,
        /// <summary>SEARCH command has been satisfied</summary>
        Equal = 0xC,
        /// <summary>End-of-medium reached with data remaining in buffer</summary>
        VolumeOverflow = 0xD,
        /// <summary>COMPARE failed</summary>
        Miscompare = 0xE,
        /// <summary>Complated</summary>
        Completed = 0xF
    }

    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global"),
     SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public struct FixedSense
    {
        /// <summary>If set, <see cref="Information" /> is valid</summary>
        public bool InformationValid;
        /// <summary>Contains number of current segment descriptor</summary>
        public byte SegmentNumber;
        /// <summary>If set indicates current command has read a filemark or a setmark</summary>
        public bool Filemark;
        /// <summary>If set indicates device has arrived end-of-medium</summary>
        public bool EOM;
        /// <summary>Means the requested logical block length did not match the logical block length on the medium</summary>
        public bool ILI;
        /// <summary>Contains the sense key</summary>
        public SenseKeys SenseKey;
        /// <summary>Additional information</summary>
        public uint Information;
        /// <summary>Additional sense length</summary>
        public byte AdditionalLength;
        /// <summary>Command specific information field</summary>
        public uint CommandSpecific;
        /// <summary>Additional sense code</summary>
        public byte ASC;
        /// <summary>Additional sense code qualifier</summary>
        public byte ASCQ;
        public byte FieldReplaceable;
        /// <summary>If set, <see cref="SenseKeySpecific" /> is valid</summary>
        public bool SKSV;
        public uint   SenseKeySpecific;
        public byte[] AdditionalSense;
    }

    [SuppressMessage("ReSharper", "MemberCanBeInternal"), SuppressMessage("ReSharper", "InconsistentNaming"),
     SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public struct DescriptorSense
    {
        /// <summary>Contains the sense key</summary>
        public SenseKeys SenseKey;
        /// <summary>Additional sense code</summary>
        public byte ASC;
        /// <summary>Additional sense code qualifier</summary>
        public byte ASCQ;
        public bool Overflow;
        /// <summary>The descriptors, indexed by type</summary>
        public Dictionary<byte, byte[]> Descriptors;
    }

    [SuppressMessage("ReSharper", "MemberCanBeInternal"), SuppressMessage("ReSharper", "InconsistentNaming"),
     SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public struct AnotherProgressIndicationSenseDescriptor
    {
        public SenseKeys SenseKey;
        public byte      ASC;
        public byte      ASCQ;
        public ushort    Progress;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Sense
    {
        /// <summary>Gets the SCSI SENSE type to help chosing the correct decoding function</summary>
        /// <returns>The type.</returns>
        /// <param name="sense">Sense bytes.</param>
        public static SenseType GetType(byte[] sense)
        {
            if(sense == null)
                return SenseType.Invalid;

            if(sense.Length < 4)
                return SenseType.Invalid;

            if((sense[0] & 0x70) != 0x70)
                return sense.Length != 4 ? SenseType.Invalid : SenseType.StandardSense;

            switch(sense[0] & 0x0F)
            {
                case 0:  return SenseType.ExtendedSenseFixedCurrent;
                case 1:  return SenseType.ExtendedSenseFixedPast;
                case 2:  return SenseType.ExtendedSenseDescriptorCurrent;
                case 3:  return SenseType.ExtendedSenseDescriptorPast;
                default: return SenseType.Unknown;
            }
        }

        public static StandardSense? DecodeStandard(byte[] sense)
        {
            if(GetType(sense) != SenseType.StandardSense)
                return null;

            var decoded = new StandardSense();
            decoded.AddressValid |= (sense[0] & 0x80) == 0x80;
            decoded.ErrorClass   =  (byte)((sense[0] & 0x70) >> 4);
            decoded.ErrorType    =  (byte)(sense[0] & 0x0F);
            decoded.Private      =  (byte)((sense[1] & 0x80) >> 4);
            decoded.LBA          =  (uint)(((sense[1] & 0x0F) << 16) + (sense[2] << 8) + sense[3]);

            return decoded;
        }

        public static DecodedSense? Decode(byte[] sense)
        {
            var decoded = new DecodedSense();

            switch(sense[0])
            {
                case 0x70:
                case 0x71:
                    decoded.Fixed = DecodeFixed(sense);

                    break;
                case 0x72:
                case 0x73:
                    decoded.Descriptor = DecodeDescriptor(sense);

                    break;
            }

            return decoded.Fixed is null && decoded.Descriptor is null ? (DecodedSense?)null : decoded;
        }

        public static FixedSense? DecodeFixed(byte[] sense) => DecodeFixed(sense, out _);

        public static FixedSense? DecodeFixed(byte[] sense, out string senseDescription)
        {
            senseDescription = null;

            if(sense is null ||
               sense.Length == 0)
                return null;

            if((sense[0] & 0x7F) != 0x70 &&
               (sense[0] & 0x7F) != 0x71)
                return null;

            if(sense.Length < 8)
                return null;

            var decoded = new FixedSense
            {
                InformationValid = (sense[0] & 0x80) == 0x80,
                SegmentNumber    = sense[1],
                Filemark         = (sense[2]            & 0x80) == 0x80,
                EOM              = (sense[2]            & 0x40) == 0x40,
                ILI              = (sense[2]            & 0x20) == 0x20,
                SenseKey         = (SenseKeys)(sense[2] & 0x0F),
                Information      = (uint)((sense[3] << 24) + (sense[4] << 16) + (sense[5] << 8) + sense[6]),
                AdditionalLength = sense[7]
            };

            if(sense.Length >= 12)
                decoded.CommandSpecific = (uint)((sense[8] << 24) + (sense[9] << 16) + (sense[10] << 8) + sense[11]);

            if(sense.Length >= 14)
            {
                decoded.ASC      = sense[12];
                decoded.ASCQ     = sense[13];
                senseDescription = GetSenseDescription(decoded.ASC, decoded.ASCQ);
            }

            if(sense.Length >= 15)
                decoded.FieldReplaceable = sense[14];

            if(sense.Length >= 18)
                decoded.SenseKeySpecific = (uint)((sense[15] << 16) + (sense[16] << 8) + sense[17]);

            if(sense.Length <= 18)
                return decoded;

            decoded.AdditionalSense = new byte[sense.Length - 18];
            Array.Copy(sense, 18, decoded.AdditionalSense, 0, decoded.AdditionalSense.Length);

            return decoded;
        }

        public static DescriptorSense? DecodeDescriptor(byte[] sense) => DecodeDescriptor(sense, out _);

        public static DescriptorSense? DecodeDescriptor(byte[] sense, out string senseDescription)
        {
            senseDescription = null;

            if(sense == null)
                return null;

            if(sense.Length < 8)
                return null;

            // Fixed sense
            if((sense[0] & 0x7F) == 0x70 ||
               (sense[0] & 0x7F) == 0x71)
                return null;

            var decoded = new DescriptorSense
            {
                SenseKey    = (SenseKeys)(sense[1] & 0x0F),
                ASC         = sense[2],
                ASCQ        = sense[3],
                Overflow    = (sense[4] & 0x80) == 0x80,
                Descriptors = new Dictionary<byte, byte[]>()
            };

            senseDescription = GetSenseDescription(decoded.ASC, decoded.ASCQ);

            int offset = 8;

            while(offset < sense.Length)
                if(offset + 2 < sense.Length)
                {
                    byte descType = sense[offset];
                    int  descLen  = sense[offset + 1] + 2;

                    byte[] desc = new byte[descLen];

                    if(offset + descLen >= sense.Length)
                        descLen = sense.Length - offset;

                    Array.Copy(sense, offset, desc, 0, descLen);

                    if(!decoded.Descriptors.ContainsKey(descType))
                        decoded.Descriptors.Add(descType, desc);

                    offset += descLen;
                }
                else
                    break;

            return decoded;
        }

        public static string PrettifySense(byte[] sense)
        {
            SenseType type = GetType(sense);

            switch(type)
            {
                case SenseType.StandardSense: return PrettifySense(DecodeStandard(sense));
                case SenseType.ExtendedSenseFixedCurrent:
                case SenseType.ExtendedSenseFixedPast: return PrettifySense(DecodeFixed(sense));
                case SenseType.ExtendedSenseDescriptorCurrent:
                case SenseType.ExtendedSenseDescriptorPast: return PrettifySense(DecodeDescriptor(sense));
                default: return null;
            }
        }

        public static string PrettifySense(StandardSense? sense)
        {
            if(!sense.HasValue)
                return null;

            return sense.Value.AddressValid
                       ? $"Error class {sense.Value.ErrorClass} type {sense.Value.ErrorType} happened on block {sense.Value.LBA}\n"
                       : $"Error class {sense.Value.ErrorClass} type {sense.Value.ErrorType}\n";
        }

        public static string PrettifySense(FixedSense? sense)
        {
            if(!sense.HasValue)
                return null;

            FixedSense decoded = sense.Value;

            var sb = new StringBuilder();

            sb.AppendFormat("SCSI SENSE: {0}", GetSenseKey(decoded.SenseKey)).AppendLine();

            if(decoded.SegmentNumber > 0)
                sb.AppendFormat("On segment {0}", decoded.SegmentNumber).AppendLine();

            if(decoded.Filemark)
                sb.AppendLine("Filemark or setmark found");

            if(decoded.EOM)
                sb.AppendLine("End-of-medium/partition found");

            if(decoded.ILI)
                sb.AppendLine("Incorrect length indicator");

            if(decoded.InformationValid)
                sb.AppendFormat("On logical block {0}", decoded.Information).AppendLine();

            if(decoded.AdditionalLength < 6)
                return sb.ToString();

            sb.AppendLine(GetSenseDescription(decoded.ASC, decoded.ASCQ));

            if(decoded.AdditionalLength < 10)
                return sb.ToString();

            if(!decoded.SKSV)
                return sb.ToString();

            switch(decoded.SenseKey)
            {
                case SenseKeys.IllegalRequest:
                {
                    sb.AppendLine((decoded.SenseKeySpecific & 0x400000) == 0x400000 ? "Illegal field in CDB"
                                      : "Illegal field in data parameters");

                    if((decoded.SenseKeySpecific & 0x200000) == 0x200000)
                        sb.AppendFormat("Invalid value in bit {0} in field {1} of CDB",
                                        (decoded.SenseKeySpecific & 0x70000) >> 16, decoded.SenseKeySpecific & 0xFFFF).
                           AppendLine();
                    else
                        sb.AppendFormat("Invalid value in field {0} of CDB", decoded.SenseKeySpecific & 0xFFFF).
                           AppendLine();
                }

                    break;
                case SenseKeys.NotReady:
                    sb.AppendFormat("Format progress {0:P}", (double)(decoded.SenseKeySpecific & 0xFFFF) / 65536).
                       AppendLine();

                    break;
                case SenseKeys.RecoveredError:
                case SenseKeys.HardwareError:
                case SenseKeys.MediumError:
                    sb.AppendFormat("Actual retry count is {0}", decoded.SenseKeySpecific & 0xFFFF).AppendLine();

                    break;
            }

            return sb.ToString();
        }

        public static string PrettifySense(DescriptorSense? sense)
        {
            if(!sense.HasValue)
                return null;

            DescriptorSense decoded = sense.Value;

            var sb = new StringBuilder();

            sb.AppendFormat("SCSI SENSE: {0}", GetSenseKey(decoded.SenseKey)).AppendLine();
            sb.AppendLine(GetSenseDescription(decoded.ASC, decoded.ASCQ));

            if(decoded.Descriptors       == null ||
               decoded.Descriptors.Count == 0)
                return sb.ToString();

            foreach(KeyValuePair<byte, byte[]> kvp in decoded.Descriptors)
                switch(kvp.Key)
                {
                    case 0x00:
                        sb.AppendLine(PrettifyDescriptor00(kvp.Value));

                        break;
                }

            return sb.ToString();
        }

        /// <summary>Decodes the information sense data descriptor</summary>
        /// <returns>The information value</returns>
        /// <param name="descriptor">Descriptor.</param>
        public static ulong DecodeDescriptor00(byte[] descriptor)
        {
            if(descriptor.Length != 12 ||
               descriptor[0]     != 0x00)
                return 0;

            byte[] temp = new byte[8];

            temp[0] = descriptor[11];
            temp[1] = descriptor[10];
            temp[2] = descriptor[9];
            temp[3] = descriptor[8];
            temp[4] = descriptor[7];
            temp[5] = descriptor[6];
            temp[6] = descriptor[5];
            temp[7] = descriptor[4];

            return BitConverter.ToUInt64(temp, 0);
        }

        /// <summary>Decodes the command-specific information sense data descriptor</summary>
        /// <returns>The command-specific information sense data descriptor.</returns>
        /// <param name="descriptor">Descriptor.</param>
        public static ulong DecodeDescriptor01(byte[] descriptor)
        {
            if(descriptor.Length != 12 ||
               descriptor[0]     != 0x01)
                return 0;

            byte[] temp = new byte[8];

            temp[0] = descriptor[11];
            temp[1] = descriptor[10];
            temp[2] = descriptor[9];
            temp[3] = descriptor[8];
            temp[4] = descriptor[7];
            temp[5] = descriptor[6];
            temp[6] = descriptor[5];
            temp[7] = descriptor[4];

            return BitConverter.ToUInt64(temp, 0);
        }

        /// <summary>Decodes the sense key specific sense data descriptor</summary>
        /// <returns>The sense key specific sense data descriptor.</returns>
        /// <param name="descriptor">Descriptor.</param>
        public static byte[] DecodeDescriptor02(byte[] descriptor)
        {
            if(descriptor.Length != 8 ||
               descriptor[0]     != 0x02)
                return null;

            byte[] temp = new byte[3];
            Array.Copy(descriptor, 4, temp, 0, 3);

            return temp;
        }

        /// <summary>Decodes the field replaceable unit sense data descriptor</summary>
        /// <returns>The field replaceable unit sense data descriptor.</returns>
        /// <param name="descriptor">Descriptor.</param>
        public static byte DecodeDescriptor03(byte[] descriptor)
        {
            if(descriptor.Length != 4 ||
               descriptor[0]     != 0x03)
                return 0;

            return descriptor[3];
        }

        /// <summary>Decodes the another progress indication sense data descriptor</summary>
        /// <returns>The another progress indication sense data descriptor.</returns>
        /// <param name="descriptor">Descriptor.</param>
        public static AnotherProgressIndicationSenseDescriptor? DecodeDescriptor0A(byte[] descriptor)
        {
            if(descriptor.Length != 8 ||
               descriptor[0]     != 0x0A)
                return null;

            return new AnotherProgressIndicationSenseDescriptor
            {
                SenseKey = (SenseKeys)descriptor[2],
                ASC      = descriptor[3],
                ASCQ     = descriptor[4],
                Progress = (ushort)((descriptor[6] << 8) + descriptor[7])
            };
        }

        public static void DecodeDescriptor04(byte[] descriptor, out bool filemark, out bool eom, out bool ili)
        {
            filemark = (descriptor[3] & 0x80) > 0;
            eom      = (descriptor[3] & 0x40) > 0;
            ili      = (descriptor[3] & 0x20) > 0;
        }

        public static void DecodeDescriptor05(byte[] descriptor) => throw new NotImplementedException("Check SBC-3");

        public static void DecodeDescriptor06(byte[] descriptor) => throw new NotImplementedException("Check OSD");

        public static void DecodeDescriptor07(byte[] descriptor) => throw new NotImplementedException("Check OSD");

        public static void DecodeDescriptor08(byte[] descriptor) => throw new NotImplementedException("Check OSD");

        public static AtaErrorRegistersLba48 DecodeDescriptor09(byte[] descriptor) => new AtaErrorRegistersLba48
        {
            Error           = descriptor[3],
            SectorCount     = (ushort)((descriptor[4] << 8) + descriptor[5]),
            LbaLowCurrent   = descriptor[6],
            LbaLowPrevious  = descriptor[7],
            LbaMidCurrent   = descriptor[8],
            LbaMidPrevious  = descriptor[9],
            LbaHighCurrent  = descriptor[10],
            LbaHighPrevious = descriptor[11],
            DeviceHead      = descriptor[12],
            Status          = descriptor[13]
        };

        public static void DecodeDescriptor0B(byte[] descriptor) => throw new NotImplementedException("Check SBC-3");

        public static void DecodeDescriptor0D(byte[] descriptor) => throw new NotImplementedException("Check SBC-3");

        public static string PrettifyDescriptor00(ulong information) => $"On logical block {information}\n";

        public static string PrettifyDescriptor00(byte[] descriptor) =>
            PrettifyDescriptor00(DecodeDescriptor00(descriptor));

        public static string GetSenseKey(SenseKeys key)
        {
            switch(key)
            {
                case SenseKeys.AbortedCommand: return "ABORTED COMMAND";
                case SenseKeys.BlankCheck:     return "BLANK CHECK";
                case SenseKeys.CopyAborted:    return "COPY ABORTED";
                case SenseKeys.DataProtect:    return "DATA PROTECT";
                case SenseKeys.Equal:          return "EQUAL";
                case SenseKeys.HardwareError:  return "HARDWARE ERROR";
                case SenseKeys.IllegalRequest: return "ILLEGAL REQUEST";
                case SenseKeys.MediumError:    return "MEDIUM ERROR";
                case SenseKeys.Miscompare:     return "MISCOMPARE";
                case SenseKeys.NoSense:        return "NO SENSE";
                case SenseKeys.PrivateUse:     return "PRIVATE USE";
                case SenseKeys.RecoveredError: return "RECOVERED ERROR";
                case SenseKeys.Completed:      return "COMPLETED";
                case SenseKeys.UnitAttention:  return "UNIT ATTENTION";
                case SenseKeys.VolumeOverflow: return "VOLUME OVERFLOW";
                default:                       return "UNKNOWN";
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static string GetSenseDescription(byte ASC, byte ASCQ)
        {
            switch(ASC)
            {
                case 0x00:
                    switch(ASCQ)
                    {
                        case 0x00: return "NO ADDITIONAL SENSE INFORMATION";
                        case 0x01: return "FILEMARK DETECTED";
                        case 0x02: return "END-OF-PARTITION/MEDIUM DETECTED";
                        case 0x03: return "SETMARK DETECTED";
                        case 0x04: return "BEGINNING-OF-PARTITION/MEDIUM DETECTED";
                        case 0x05: return "END-OF-DATA DETECTED";
                        case 0x06: return "I/O PROCESS TERMINATED";
                        case 0x07: return "PROGRAMMABLE EARLY WARNING DETECTED";
                        case 0x11: return "AUDIO PLAY OPERATION IN PROGRESS";
                        case 0x12: return "AUDIO PLAY OPERATION PAUSED";
                        case 0x13: return "AUDIO PLAY OPERATION SUCCESSFULLY COMPLETED";
                        case 0x14: return "AUDIO PLAY OPERATION STOPPED DUE TO ERROR";
                        case 0x15: return "NO CURRENT AUDIO STATUS TO RETURN";
                        case 0x16: return "OPERATION IN PROGRESS";
                        case 0x17: return "CLEANING REQUESTED";
                        case 0x18: return "ERASE OPERATION IN PROGRESS";
                        case 0x19: return "LOCATE OPERATION IN PROGRESS";
                        case 0x1A: return "REWIND OPERATION IN PROGRESS";
                        case 0x1B: return "SET CAPACITY OPERATION IN PROGRESS";
                        case 0x1C: return "VERIFY OPERATION IN PROGRESS";
                        case 0x1D: return "ATA PASS THROUGH INFORMATION AVAILABLE";
                        case 0x1E: return "CONFLICTING SA CREATION REQUEST";
                        case 0x1F: return "LOGICAL UNIT TRANSITIONING TO ANOTHER POWER CONDITION";
                        case 0x20: return "EXTENDED COPY INFORMATION AVAILABLE";
                        case 0x21: return "ATOMIC COMMAND ABORTED DUE TO ACA";
                    }

                    break;
                case 0x01:
                    switch(ASCQ)
                    {
                        case 0x00: return "NO INDEX/SECTOR SIGNAL";
                    }

                    break;
                case 0x02:
                    switch(ASCQ)
                    {
                        case 0x00: return "NO SEEK COMPLETE";
                    }

                    break;
                case 0x03:
                    switch(ASCQ)
                    {
                        case 0x00: return "PERIPHERAL DEVICE WRITE FAULT";
                        case 0x01: return "NO WRITE CURRENT";
                        case 0x02: return "EXCESSIVE WRITE ERRORS";
                    }

                    break;
                case 0x04:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT NOT READY, CAUSE NOT REPORTABLE";
                        case 0x01: return "LOGICAL UNIT IS IN PROCESS OF BECOMING READY";
                        case 0x02: return "LOGICAL UNIT NOT READY, INITIALIZING COMMAND REQUIRED";
                        case 0x03: return "LOGICAL UNIT NOT READY, MANUAL INTERVENTION REQUIRED";
                        case 0x04: return "LOGICAL UNIT NOT READY, FORMAT IN PROGRESS";
                        case 0x05: return "LOGICAL UNIT NOT READY, REBUILD IN PROGRESS";
                        case 0x06: return "LOGICAL UNIT NOT READY, RECALCULATION IN PROGRESS";
                        case 0x07: return "LOGICAL UNIT NOT READY, OPERATION IN PROGRESS";
                        case 0x08: return "LOGICAL UNIT NOT READY, LONG WRITE IN PROGRESS";
                        case 0x09: return "LOGICAL UNIT NOT READY, SELF-TEST IN PROGRESS";
                        case 0x0A: return "LOGICAL UNIT NOT ACCESSIBLE, ASYMMETRIC ACCESS STATE TRANSITION";
                        case 0x0B: return "LOGICAL UNIT NOT ACCESSIBLE, TARGET IN STANDBY STATE";
                        case 0x0C: return "LOGICAL UNIT NOT ACCESSIBLE, TARGET PORT IN UNAVAILABLE STATE";
                        case 0x0D: return "LOGICAL UNIT NOT READY, STRUCTURE CHECK REQUIRED";
                        case 0x0E: return "LOGICAL UNIT NOT READY, SECURITY SESSION IN PROGRESS";
                        case 0x10: return "LOGICAL UNIT NOT READY, AUXILIARY MEMORY NOT ACCESSIBLE";
                        case 0x11: return "LOGICAL UNIT NOT READY, NOTIFY (ENABLE SPINUP) REQUIRED";
                        case 0x12: return "LOGICAL UNIT NOT READY, OFFLINE";
                        case 0x13: return "LOGICAL UNIT NOT READY, SA CREATION IN PROGRESS";
                        case 0x14: return "LOGICAL UNIT NOT READY, SPACE ALLOCATION IN PROGRESS";
                        case 0x15: return "LOGICAL UNIT NOT READY, ROBOTICS DISABLED";
                        case 0x16: return "LOGICAL UNIT NOT READY, CONFIGURATION REQUIRED";
                        case 0x17: return "LOGICAL UNIT NOT READY, CALIBRATION REQUIRED";
                        case 0x18: return "LOGICAL UNIT NOT READY, A DOOR IS OPEN";
                        case 0x19: return "LOGICAL UNIT NOT READY, OPERATING IN SEQUENTIAL MODE";
                        case 0x1A: return "LOGICAL UNIT NOT READY, START STOP UNIT IN PROGRESS";
                        case 0x1B: return "LOGICAL UNIT NOT READY, SANITIZE IN PROGRESS";
                        case 0x1C: return "LOGICAL UNIT NOT READY, ADDITIONAL POWER USE NOT YET GRANTED";
                        case 0x1D: return "LOGICAL UNIT NOT READY, CONFIGURATION IN PROGRESS";
                        case 0x1E: return "LOGICAL UNIT NOT READY, MICROCODE ACTIVATION REQUIRED";
                        case 0x1F: return "LOGICAL UNIT NOT READY, MICROCODE DOWNLOAD REQUIRED";
                        case 0x20: return "LOGICAL UNIT NOT READY, LOGICAL UNIT RESET REQUIRED";
                        case 0x21: return "LOGICAL UNIT NOT READY, HARD RESET REQUIRED";
                        case 0x22: return "LOGICAL UNIT NOT READY, POWER CYCLE REQUIRED";
                    }

                    break;
                case 0x05:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT DOES NOT RESPOND TO SELECTION";
                    }

                    break;
                case 0x06:
                    switch(ASCQ)
                    {
                        case 0x00: return "NO REFERENCE POSITION FOUND";
                    }

                    break;
                case 0x07:
                    switch(ASCQ)
                    {
                        case 0x00: return "MULTIPLE PERIPHERAL DEVICES SELECTED";
                    }

                    break;
                case 0x08:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT COMMUNICATION FAILURE";
                        case 0x01: return "LOGICAL UNIT COMMUNICATION TIME-OUT";
                        case 0x02: return "LOGICAL UNIT COMMUNICATION PARITY ERROR";
                        case 0x03: return "LOGICAL UNIT COMMUNICATION CRC ERROR";
                        case 0x04: return "UNREACHABLE COPY TARGET";
                    }

                    break;
                case 0x09:
                    switch(ASCQ)
                    {
                        case 0x00: return "TRACK FLOLLOWING ERROR";
                        case 0x01: return "TRACKING SERVO FAILURE";
                        case 0x02: return "FOCUS SERVO FAILURE";
                        case 0x03: return "SPINDLE SERVO FAILURE";
                        case 0x04: return "HEAD SELECT FAULT";
                        case 0x05: return "VIBRATION INDUCED TRACKING ERROR";
                    }

                    break;
                case 0x0A:
                    switch(ASCQ)
                    {
                        case 0x00: return "ERROR LOG OVERFLOW";
                    }

                    break;
                case 0x0B:
                    switch(ASCQ)
                    {
                        case 0x00: return "WARNING";
                        case 0x01: return "WARNING - SPECIFIED TEMPERATURE EXCEEDED";
                        case 0x02: return "WARNING - ENCLOSURE DEGRADED";
                        case 0x03: return "WARNING - BACKGROUND SELF-TEST FAILED";
                        case 0x04: return "WARNING - BACKGROUND PRE-SCAN DETECTED MEDIUM ERROR";
                        case 0x05: return "WARNING - BACKGROUND MEDIUM SCAN DETECTED MEDIUM ERROR";
                        case 0x06: return "WARNING - NON-VOLATILE CACHE NOW VOLATILE";
                        case 0x07: return "WARNING - DEGRADED POWER TO NON-VOLATILE CACHE";
                        case 0x08: return "WARNING - POWER LOSS EXPECTED";
                        case 0x09: return "WARNING - DEVICE STATISTICS NOTIFICATION ACTIVE";
                        case 0x0A: return "WARNING - HIGH CRITICAL TEMPERATURE LIMIT EXCEEDED";
                        case 0x0B: return "WARNING - LOW CRITICAL TEMPERATURE LIMIT EXCEEDED";
                        case 0x0C: return "WARNING - HIGH OPERATING TEMPERATURE LIMIT EXCEEDED";
                        case 0x0D: return "WARNING - LOW OPERATING TEMPERATURE LIMIT EXCEEDED";
                        case 0x0E: return "WARNING - HIGH CRITICAL HUMIDITY LIMIT EXCEEDED";
                        case 0x0F: return "WARNING - LOW CRITICAL HUMIDITY LIMIT EXCEEDED";
                        case 0x10: return "WARNING - HIGH OPERATING HUMIDITY LIMIT EXCEEDED";
                        case 0x11: return "WARNING - LOW OPERATING HUMIDITY LIMIT EXCEEDED";
                    }

                    break;
                case 0x0C:
                    switch(ASCQ)
                    {
                        case 0x00: return "WRITE ERROR";
                        case 0x01: return "WRITE ERROR - RECOVERED WITH AUTO REALLOCATION";
                        case 0x02: return "WRITE ERROR - AUTO REALLOCATION FAILED";
                        case 0x03: return "WRITE ERROR - RECOMMENDED REASSIGNMENT";
                        case 0x04: return "COMPRESSION CHECK MISCOMPARE ERROR";
                        case 0x05: return "DATA EXPANSION OCCURRED DURING COMPRESSION";
                        case 0x06: return "BLOCK NOT COMPRESSIBLE";
                        case 0x07: return "WRITE ERROR - RECOVERY NEEDED";
                        case 0x08: return "WRITE ERROR - RECOVERY FAILED";
                        case 0x09: return "WRITE ERROR - LOSS OF STREAMING";
                        case 0x0A: return "WRITE ERROR - PADDING BLOCKS ADDED";
                        case 0x0B: return "AUXILIARY MEMORY WRITE ERROR";
                        case 0x0C: return "WRITE ERROR - UNEXPECTED UNSOLICITED DATA";
                        case 0x0D: return "WRITE ERROR - NOT ENOUGH UNSOLICITED DATA";
                        case 0x0E: return "MULTIPLE WRITE ERRORS";
                        case 0x0F: return "DEFECTS IN ERROR WINDOW";
                        case 0x10: return "INCOMPLETE MULTIPLE ATOMIC WRITE OPERATIONS";
                        case 0x11: return "WRITE ERROR - RECOVERY SCAN NEEDED";
                        case 0x12: return "WRITE ERROR - INSUFFICIENT ZONE RESOURCES";
                    }

                    break;
                case 0x0D:
                    switch(ASCQ)
                    {
                        case 0x00: return "ERROR DETECTED BY THIRD PARTY TEMPORARY INITIATOR";
                        case 0x01: return "THIRD PARTY DEVICE FAILURE";
                        case 0x02: return "COPY TARGET DEVICE NOT REACHABLE";
                        case 0x03: return "INCORRECT COPY TARGET DEVICE TYPE";
                        case 0x04: return "COPY TARGET DEVICE DATA UNDERRUN";
                        case 0x05: return "COPY TARGET DEVICE DATA OVERRUN";
                    }

                    break;
                case 0x0E:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID INFORMATION UNIT";
                        case 0x01: return "INFORMATION UNIT TOO SHORT";
                        case 0x02: return "INFORMATION UNIT TOO LONG";
                        case 0x03: return "INVALID FIELD IN COMMAND INFORMATION UNIT";
                    }

                    break;
                case 0x10:
                    switch(ASCQ)
                    {
                        case 0x00: return "ID CRC OR ECC ERROR";
                        case 0x01: return "LOGICAL BLOCK GUARD CHECK FAILED";
                        case 0x02: return "LOGICAL BLOCK APPLICATION TAG CHECK FAILED";
                        case 0x03: return "LOGICAL BLOCK REFERENCE TAG CHECK FAILED";
                        case 0x04: return "LOGICAL BLOCK PROTECTION ERROR ON RECOVER BUFFERED DATA";
                        case 0x05: return "LOGICAL BLOCK PROTECTION METHOD ERROR";
                    }

                    break;
                case 0x11:
                    switch(ASCQ)
                    {
                        case 0x00: return "UNRECOVERED READ ERROR";
                        case 0x01: return "READ RETRIES EXHAUSTED";
                        case 0x02: return "ERROR TOO LONG TO CORRECT";
                        case 0x03: return "MULTIPLE READ ERRORS";
                        case 0x04: return "UNRECOVERED READ ERROR - AUTO REALLOCATE FAILED";
                        case 0x05: return "L-EC UNCORRECTABLE ERROR";
                        case 0x06: return "CIRC UNRECOVERED ERROR";
                        case 0x07: return "DATA RESYNCHRONIZATION ERROR";
                        case 0x08: return "INCOMPLETE BLOCK READ";
                        case 0x09: return "NO GAP FOUND";
                        case 0x0A: return "MISCORRECTED ERROR";
                        case 0x0B: return "UNRECOVERED READ ERROR - RECOMMENDED REASSIGNMENT";
                        case 0x0C: return "UNRECOVERED READ ERROR - RECOMMENDED REWRITE THE DATA";
                        case 0x0D: return "DE-COMPRESSION CRC ERROR";
                        case 0x0E: return "CANNOT DECOMPRESS USING DECLARED ALGORITHM";
                        case 0x0F: return "ERROR READING UPC/EAN NUMBER";
                        case 0x10: return "ERROR READING ISRC NUMBER";
                        case 0x11: return "READ ERROR - LOSS OF STREAMING";
                        case 0x12: return "AUXILIARY MEMORY READ ERROR";
                        case 0x13: return "READ ERROR - FAILED RETRANSMISSITION REQUEST";
                        case 0x14: return "READ ERROR - LBA MARKED BAD BY APPLICATION CLIENT";
                        case 0x15: return "WRITE AFTER SANITIZE REQUIRED";
                    }

                    break;
                case 0x12:
                    switch(ASCQ)
                    {
                        case 0x00: return "ADDRESS MARK NOT FOUND FOR ID FIELD";
                    }

                    break;
                case 0x13:
                    switch(ASCQ)
                    {
                        case 0x00: return "ADDRESS MARK NOT FOUND FOR DATA FIELD";
                    }

                    break;
                case 0x14:
                    switch(ASCQ)
                    {
                        case 0x00: return "RECORDED ENTITY NOT FOUND";
                        case 0x01: return "RECORD NOT FOUND";
                        case 0x02: return "FILEMARK OR SETMARK NOT FOUND";
                        case 0x03: return "END-OF-DATA NOT FOUND";
                        case 0x04: return "BLOCK SEQUENCE ERROR";
                        case 0x05: return "RECORD NOT FOUND - RECOMMENDAD REASSIGNMENT";
                        case 0x06: return "RECORD NOT FOUND - DATA AUTO-REALLOCATED";
                        case 0x07: return "LOCATE OPERATION FAILURE";
                    }

                    break;
                case 0x15:
                    switch(ASCQ)
                    {
                        case 0x00: return "RANDOM POSITIONING ERROR";
                        case 0x01: return "MECHANICAL POSITIONING ERROR";
                        case 0x02: return "POSITIONING ERROR DETECTED BY READ OF MEDIUM";
                    }

                    break;
                case 0x16:
                    switch(ASCQ)
                    {
                        case 0x00: return "DATA SYNCHRONIZATION MARK ERROR";
                        case 0x01: return "DATA SYNC ERROR - DATA REWRITTEN";
                        case 0x02: return "DATA SYNC ERROR - RECOMMENDED REWRITE";
                        case 0x03: return "DATA SYNC ERROR - DATA AUTO-REALLOCATED";
                        case 0x04: return "DATA SYNC ERROR - RECOMMENDED REASSIGNMENT";
                    }

                    break;
                case 0x17:
                    switch(ASCQ)
                    {
                        case 0x00: return "RECOVERED DATA WITH NO ERROR CORRECTION APPLIED";
                        case 0x01: return "RECOVERED DATA WITH RETRIES";
                        case 0x02: return "RECOVERED DATA WITH POSITIVE HEAD OFFSET";
                        case 0x03: return "RECOVERED DATA WITH NEGATIVE HEAD OFFSET";
                        case 0x04: return "RECOVERED DATA WITH RETRIES AND/OR CIRC APPLIED";
                        case 0x05: return "RECOVERED DATA USING PREVIOUS SECTOR ID";
                        case 0x06: return "RECOVERED DATA WITHOUT ECC - DATA AUTO-REALLOCATED";
                        case 0x07: return "RECOVERED DATA WITHOUT ECC - RECOMMENDED REASSIGNMENT";
                        case 0x08: return "RECOVERED DATA WITHOUT ECC - RECOMMENDED REWRITE";
                        case 0x09: return "RECOVERED DATA WITHOUT ECC - DATA REWRITTEN";
                    }

                    break;
                case 0x18:
                    switch(ASCQ)
                    {
                        case 0x00: return "RECOVERED DATA WITH ERROR CORRECTION APPLIED";
                        case 0x01: return "RECOVERED DATA WITH ERROR CORRECTION & RETRIES APPLIED";
                        case 0x02: return "RECOVERED DATA - DATA AUTO-REALLOCATED";
                        case 0x03: return "RECOVERED DATA WITH CIRC";
                        case 0x04: return "RECOVERED DATA WITH L-EC";
                        case 0x05: return "RECOVERED DATA - RECOMMENDED REASSIGNMENT";
                        case 0x06: return "RECOVERED DATA - RECOMMENDED REWRITE";
                        case 0x07: return "RECOVERED DATA WITH ECC - DATA REWRITTEN";
                        case 0x08: return "RECOVERED DATA WITH LINKING";
                    }

                    break;
                case 0x19:
                    switch(ASCQ)
                    {
                        case 0x00: return "DEFECT LIST ERROR";
                        case 0x01: return "DEFECT LIST NOT AVAILABLE";
                        case 0x02: return "DEFECT LIST ERROR IN PRIMARY LIST";
                        case 0x03: return "DEFECT LIST ERROR IN GROWN LIST";
                    }

                    break;
                case 0x1A:
                    switch(ASCQ)
                    {
                        case 0x00: return "PARAMETER LIST LENGTH ERROR";
                    }

                    break;
                case 0x1B:
                    switch(ASCQ)
                    {
                        case 0x00: return "SYNCHRONOUS DATA TRANSFER ERROR";
                    }

                    break;
                case 0x1C:
                    switch(ASCQ)
                    {
                        case 0x00: return "DEFECT LIST NOT FOUND";
                        case 0x01: return "PRIMARY DEFECT LIST NOT FOUND";
                        case 0x02: return "GROWN DEFECT LIST NOT FOUND";
                    }

                    break;
                case 0x1D:
                    switch(ASCQ)
                    {
                        case 0x00: return "MISCOMPARE DURING VERIFY OPERATION";
                        case 0x01: return "MISCOMPARE VERIFY OF UNMAPPED LBA";
                    }

                    break;
                case 0x1E:
                    switch(ASCQ)
                    {
                        case 0x00: return "RECOVERED ID WITH ECC CORRECTION";
                    }

                    break;
                case 0x1F:
                    switch(ASCQ)
                    {
                        case 0x00: return "PARTIAL DEFECT LIST TRANSFER";
                    }

                    break;
                case 0x20:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID COMMAND OPERATION CODE";
                        case 0x01: return "ACCESS DENIED - INITIATOR PENDING-ENROLLED";
                        case 0x02: return "ACCESS DENIED - NO ACCESS RIGHTS";
                        case 0x03: return "ACCESS DENIED - INVALID MGMT ID KEY";
                        case 0x04: return "ILLEGAL COMMAND WHILE IN WRITE CAPABLE STATE";
                        case 0x05: return "ILLEGAL COMMAND WHILE IN READ CAPABLE STATE";
                        case 0x06: return "ILLEGAL COMMAND WHILE IN EXPLICIT ADDRESS MODE";
                        case 0x07: return "ILLEGAL COMMAND WHILE IN IMPLICIT ADDRESS MODE";
                        case 0x08: return "ACCESS DENIED - ENROLLMENT CONFLICT";
                        case 0x09: return "ACCESS DENIED - INVALID LUN IDENTIFIER";
                        case 0x0A: return "ACCESS DENIED - INVALID PROXY TOKEN";
                        case 0x0B: return "ACCESS DENIED - ACL LUN CONFLICT";
                        case 0x0C: return "ILLEGAL COMMAND WHEN NOT IN APPEND-ONLY MODE";
                    }

                    break;
                case 0x21:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL BLOCK ADDRESS OUT OF RANGE";
                        case 0x01: return "INVALID ELEMENT ADDRESS";
                        case 0x02: return "INVALID ADDRESS FOR WRITE";
                        case 0x03: return "INVALID WRITE CROSSING LAYER JUMP";
                        case 0x04: return "UNALIGNED WRITE COMMAND";
                        case 0x05: return "WRITE BOUNDARY VIOLATION";
                        case 0x06: return "ATTEMPT TO READ INVALID DATA";
                        case 0x07: return "READ BOUNDARY VIOLATION";
                    }

                    break;
                case 0x22:
                    switch(ASCQ)
                    {
                        case 0x00: return "ILLEGAL FUNCTION";
                    }

                    break;
                case 0x23:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID TOKEN OPERATION, CAUSE NOT REPORTABLE";
                        case 0x01: return "INVALID TOKEN OPERATION, UNSUPPORTED TOKEN TYPE";
                        case 0x02: return "INVALID TOKEN OPERATION, REMOTE TOKEN USAGE NOT SUPPORTED";
                        case 0x03: return "INVALID TOKEN OPERATION, REMOTE ROD TOKEN CREATION NOT SUPPORTED";
                        case 0x04: return "INVALID TOKEN OPERATION, TOKEN UNKNOWN";
                        case 0x05: return "INVALID TOKEN OPERATION, TOKEN CORRUPT";
                        case 0x06: return "INVALID TOKEN OPERATION, TOKEN REVOKED";
                        case 0x07: return "INVALID TOKEN OPERATION, TOKEN EXPIRED";
                        case 0x08: return "INVALID TOKEN OPERATION, TOKEN CANCELLED";
                        case 0x09: return "INVALID TOKEN OPERATION, TOKEN DELETED";
                        case 0x0A: return "INVALID TOKEN OPERATION, INVALID TOKEN LENGTH";
                    }

                    break;
                case 0x24:
                    switch(ASCQ)
                    {
                        case 0x00: return "ILLEGAL FIELD IN CDB";
                        case 0x01: return "CDB DECRYPTION ERROR";
                        case 0x02: return "INVALID CDB FIELD WHILE IN EXPLICIT BLOCK ADDRESS MODEL";
                        case 0x03: return "INVALID CDB FIELD WHILE IN IMPLICIT BLOCK ADDRESS MODEL";
                        case 0x04: return "SECURITY AUDIT VALUE FROZEN";
                        case 0x05: return "SECURITY WORKING KEY FROZEN";
                        case 0x06: return "NONCE NOT UNIQUE";
                        case 0x07: return "NONCE TIMESTAMP OUT OF RANGE";
                        case 0x08: return "INVALID XCDB";
                    }

                    break;
                case 0x25:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT NOT SUPPORTED";
                    }

                    break;
                case 0x26:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID FIELD IN PARAMETER LIST";
                        case 0x01: return "PARAMETER NOT SUPPORTED";
                        case 0x02: return "PARAMETER VALUE INVALID";
                        case 0x03: return "THRESHOLD PARAMETERS NOT SUPPORTED";
                        case 0x04: return "INVALID RELEASE OF PERSISTENT RESERVATION";
                        case 0x05: return "DATA DECRYPTION ERROR";
                        case 0x06: return "TOO MANY TARGET DESCRIPTORS";
                        case 0x07: return "UNSUPPORTED TARGET DESCRIPTOR TYPE CODE";
                        case 0x08: return "TOO MANY SEGMENT DESCRIPTORS";
                        case 0x09: return "UNSUPPORTED SEGMENT DESCRIPTOR TYPE CODE";
                        case 0x0A: return "UNEXPECTED INEXACT SEGMENT";
                        case 0x0B: return "INLINE DATA LENGTH EXCEEDED";
                        case 0x0C: return "INVALID OPERATION FOR COPY SOURCE OR DESTINATION";
                        case 0x0D: return "COPY SEGMENT GRANULARITY VIOLATION";
                        case 0x0E: return "INVALID PARAMETER WHILE PORT IS ENABLED";
                        case 0x0F: return "INVALID DATA-OUT BUFFER INTEGRITY CHECK VALUE";
                        case 0x10: return "DATA DECRYPTION KEY FAIL LIMIT REACHED";
                        case 0x11: return "INCOMPLETE KEY-ASSOCIATED DATA SET";
                        case 0x12: return "VENDOR SPECIFIC KEY REFERENCE NOT FOUND";
                        case 0x13: return "APPLICATION TAG MODE PAGE IS INVALID";
                    }

                    break;
                case 0x27:
                    switch(ASCQ)
                    {
                        case 0x00: return "WRITE PROTECTED";
                        case 0x01: return "HARDWARE WRITE PROTECTED";
                        case 0x02: return "LOGICAL UNIT SOFTWARE WRITE PROTECTED";
                        case 0x03: return "ASSOCIATED WRITE PROTECT";
                        case 0x04: return "PERSISTENT WRITE PROTECT";
                        case 0x05: return "PERMANENT WRITE PROTECT";
                        case 0x06: return "CONDITIONAL WRITE PROTECT";
                        case 0x07: return "SPACE ALLOCATION FAILED WRITE PROTECT";
                        case 0x08: return "ZONE IS READ ONLY";
                    }

                    break;
                case 0x28:
                    switch(ASCQ)
                    {
                        case 0x00: return "NOT READY TO READY CHANGE (MEDIUM MAY HAVE CHANGED)";
                        case 0x01: return "IMPORT OR EXPORT ELEMENT ACCESSED";
                        case 0x02: return "FORMAT-LAYER MAY HAVE CHANGED";
                        case 0x03: return "IMPORT/EXPORT ELEMENT ACCESSED, MEDIUM CHANGED";
                    }

                    break;
                case 0x29:
                    switch(ASCQ)
                    {
                        case 0x00: return "POWER ON, RESET, OR BUS DEVICE RESET OCCURRED";
                        case 0x01: return "POWER ON OCCURRED";
                        case 0x02: return "SCSI BUS RESET OCCURRED";
                        case 0x03: return "BUS DEVICE RESET FUNCTION OCCURRED";
                        case 0x04: return "DEVICE INTERNAL RESET";
                        case 0x05: return "TRANSCEIVER MODE CHANGED TO SINGLE-ENDED";
                        case 0x06: return "TRANSCEIVER MODE CHANGED TO LVD";
                        case 0x07: return "I_T NEXUS LOSS OCCURRED";
                    }

                    break;
                case 0x2A:
                    switch(ASCQ)
                    {
                        case 0x00: return "PARAMETERS CHANGED";
                        case 0x01: return "MODE PARAMETERS CHANGED";
                        case 0x02: return "LOG PARAMETERS CHANGED";
                        case 0x03: return "RESERVATIONS PREEMPTED";
                        case 0x04: return "RESERVATIONS RELEASED";
                        case 0x05: return "REGISTRATIONS PREEMPTED";
                        case 0x06: return "ASYMMETRIC ACCESS STATE CHANGED";
                        case 0x07: return "IMPLICIT ASYMMETRIC ACCESS STATE TRANSITION FAILED";
                        case 0x08: return "PRIORITY CHANGED";
                        case 0x09: return "CAPACITY DATA HAS CHANGED";
                        case 0x0A: return "ERROR HISTORY I_T NEXUS CLEARED";
                        case 0x0B: return "ERROR HISTORY SNAPSHOT RELEASED";
                        case 0x0C: return "ERROR RECOVERY ATTRIBUTES HAVE CHANGED";
                        case 0x0D: return "DATA ENCRYPTION CAPABILITIES CHANGED";
                        case 0x10: return "TIMESTAMP CHANGED";
                        case 0x11: return "DATA ENCRYPTION PARAMETERS CHANGED BY ANOTHER I_T NEXUS";
                        case 0x12: return "DATA ENCRYPTION PARAMETERS CHANGED BY VENDOR SPECIFIC EVENT";
                        case 0x13: return "DATA ENCRYPTION KEY INSTANCE COUNTER HAS CHANGED";
                        case 0x14: return "SA CREATION CAPABILITIES DATA HAS CHANGED";
                        case 0x15: return "MEDIUM REMOVAL PREVENTION PREEMPTED";
                    }

                    break;
                case 0x2B:
                    switch(ASCQ)
                    {
                        case 0x00: return "COPY CANNOT EXECUTE SINCE HOST CANNOT DISCONNECT";
                    }

                    break;
                case 0x2C:
                    switch(ASCQ)
                    {
                        case 0x00: return "COMMAND SEQUENCE ERROR";
                        case 0x01: return "TOO MANY WINDOWS SPECIFIED";
                        case 0x02: return "INVALID COMBINATION OF WINDOWS SPECIFIED";
                        case 0x03: return "CURRENT PROGRAM AREA IS NOT EMPTY";
                        case 0x04: return "CURRENT PROGRAM AREA IS EMPTY";
                        case 0x05: return "ILLEGAL POWER CONDITION REQUEST";
                        case 0x06: return "PERSISTENT PREVENT CONFLICT";
                        case 0x07: return "PREVIOUS BUSY STATUS";
                        case 0x08: return "PREVIOUS TASK SET FULL STATUS";
                        case 0x09: return "PREVIOUS RESERVATION CONFLICT STATUS";
                        case 0x0A: return "PARTITION OR COLLECTION CONTAINS USER OBJECTS";
                        case 0x0B: return "NOT RESERVED";
                        case 0x0C: return "ORWRITE GENERATION DOES NOT MATCH";
                        case 0x0D: return "RESET WRITE POINTER NOT ALLOWED";
                        case 0x0E: return "ZONE IS OFFLINE";
                        case 0x0F: return "STREAM NOT OPEN";
                        case 0x10: return "UNWRITTEN DATA IN ZONE";
                    }

                    break;
                case 0x2D:
                    switch(ASCQ)
                    {
                        case 0x00: return "OVERWRITE ERROR ON UPDATE IN PLACE";
                    }

                    break;
                case 0x2E:
                    switch(ASCQ)
                    {
                        case 0x00: return "INSUFFICIENT TIME FOR OPERATION";
                        case 0x01: return "COMMAND TIMEOUT BEFORE PROCESSING";
                        case 0x02: return "COMMAND TIMEOUT DURING PROCESSING";
                        case 0x03: return "COMMAND TIMEOUT DURING PROCESSING DUE TO ERROR RECOVERY";
                    }

                    break;
                case 0x2F:
                    switch(ASCQ)
                    {
                        case 0x00: return "COMMANDS CLEARED BY ANOTHER INITIATOR";
                        case 0x01: return "COMMANDS CLEARED BY POWER LOSS NOTIFICATION";
                        case 0x02: return "COMMANDS CLEARED BY DEVICE SERVER";
                        case 0x03: return "SOME COMMANDS CLEARED BY QUEUING LAYER EVENT";
                    }

                    break;
                case 0x30:
                    switch(ASCQ)
                    {
                        case 0x00: return "INCOMPATIBLE MEDIUM INSTALLED";
                        case 0x01: return "CANNOT READ MEDIUM - UNKNOWN FORMAT";
                        case 0x02: return "CANNOT READ MEDIUM - INCOMPATIBLE FORMAT";
                        case 0x03: return "CLEANING CARTRIDGE INSTALLED";
                        case 0x04: return "CANNOT WRITE MEDIUM - UNKNOWN FORMAT";
                        case 0x05: return "CANNOT WRITE MEDIUM - INCOMPATIBLE FORMAT";
                        case 0x06: return "CANNOT FORMAT MEDIUM - INCOMPATIBLE MEDIUM";
                        case 0x07: return "CLEANING FAILURE";
                        case 0x08: return "CANNOT WRITE - APPLICATION CODE MISMATCH";
                        case 0x09: return "CURRENT SESSION NOT FIXATED FOR APPEND";
                        case 0x0A: return "CLEANING REQUEST REJECTED";
                        case 0x0C: return "WORM MEDIUM - OVERWRITE ATTEMPTED";
                        case 0x0D: return "WORM MEDIUM - INTEGRITY CHECK";
                        case 0x10: return "MEDIUM NOT FORMATTED";
                        case 0x11: return "INCOMPATIBLE VOLUME TYPE";
                        case 0x12: return "INCOMPATIBLE VOLUME QUALIFIER";
                        case 0x13: return "CLEANING VOLUME EXPIRED";
                    }

                    break;
                case 0x31:
                    switch(ASCQ)
                    {
                        case 0x00: return "MEDIUM FORMAT CORRUPTED";
                        case 0x01: return "FORMAT COMMAND FAILED";
                        case 0x02: return "ZONED FORMATTING FAILED DUE TO SPARE LINKING";
                        case 0x03: return "SANITIZE COMMAND FAILED";
                    }

                    break;
                case 0x32:
                    switch(ASCQ)
                    {
                        case 0x00: return "NO DEFECT SPARE LOCATION AVAILABLE";
                        case 0x01: return "DEFECT LIST UPDATE FAILURE";
                    }

                    break;
                case 0x33:
                    switch(ASCQ)
                    {
                        case 0x00: return "TAPE LENGTH ERROR";
                    }

                    break;
                case 0x34:
                    switch(ASCQ)
                    {
                        case 0x00: return "ENCLOSURE FAILURE";
                    }

                    break;
                case 0x35:
                    switch(ASCQ)
                    {
                        case 0x00: return "ENCLOSURE SERVICES FAILURE";
                        case 0x01: return "UNSUPPORTED ENCLOSURE FUNCTION";
                        case 0x02: return "ENCLOSURE SERVICES UNAVAILABLE";
                        case 0x03: return "ENCLOSURE SERVICES TRANSFER FAILURE";
                        case 0x04: return "ENCLOSURE SERVICES TRANSFER REFUSED";
                        case 0x05: return "ENCLOSURE SERVICES CHECKSUM ERROR";
                    }

                    break;
                case 0x36:
                    switch(ASCQ)
                    {
                        case 0x00: return "RIBBON, INK, OR TONER FAILURE";
                    }

                    break;
                case 0x37:
                    switch(ASCQ)
                    {
                        case 0x00: return "ROUNDED PARAMETER";
                    }

                    break;
                case 0x38:
                    switch(ASCQ)
                    {
                        case 0x00: return "EVENT STATUS NOTIFICATION";
                        case 0x02: return "ESN - POWER MANAGEMENT CLASS EVENT";
                        case 0x04: return "ESN - MEDIA CLASS EVENT";
                        case 0x06: return "ESN - DEVICE BUSY CLASS EVENT";
                        case 0x07: return "THIN PROVISIONING SOFT THRESHOLD REACHED";
                    }

                    break;
                case 0x39:
                    switch(ASCQ)
                    {
                        case 0x00: return "SAVING PARAMETERS NOT SUPPORTED";
                    }

                    break;
                case 0x3A:
                    switch(ASCQ)
                    {
                        case 0x00: return "MEDIUM NOT PRESENT";
                        case 0x01: return "MEDIUM NOT PRESENT - TRAY CLOSED";
                        case 0x02: return "MEDIUM NOT PRESENT - TRAY OPEN";
                        case 0x03: return "MEDIUM NOT PRESENT - LOADABLE";
                        case 0x04: return "MEDIUM NOT PRESENT - MEDIUM AUXILIARY MEMORY ACCESSIBLE";
                    }

                    break;
                case 0x3B:
                    switch(ASCQ)
                    {
                        case 0x00: return "SEQUENTIAL POSITIONING ERROR";
                        case 0x01: return "TAPE POSITION ERROR AT BEGINNING-OF-MEDIUM";
                        case 0x02: return "TAPE POSITION ERROR AT END-OF-MEDIUM";
                        case 0x03: return "TAPE OR ELECTRONIC VERTICAL FORMS UNIT NOT READY";
                        case 0x04: return "SLEW FAILURE";
                        case 0x05: return "PAPER JAM";
                        case 0x06: return "FAILED TO SENSE TOP-OF-FORM";
                        case 0x07: return "FAILED TO SENSE BOTTOM-OF-FORM";
                        case 0x08: return "REPOSITION ERROR";
                        case 0x09: return "READ PAST END OF MEDIUM";
                        case 0x0A: return "READ PAST BEGINNING OF MEDIUM";
                        case 0x0B: return "POSITION PAST END OF MEDIUM";
                        case 0x0C: return "POSITION PAST BEGINNING OF MEDIUM";
                        case 0x0D: return "MEDIUM DESTINATION ELEMENT FULL";
                        case 0x0E: return "MEDIUM SOURCE ELEMENT EMPTY";
                        case 0x0F: return "END OF MEDIUM REACHED";
                        case 0x11: return "MEDIUM MAGAZINE NOT ACCESSIBLE";
                        case 0x12: return "MEDIUM MAGAZINE REMOVED";
                        case 0x13: return "MEDIUM MAGAZINE INSERTED";
                        case 0x14: return "MEDIUM MAGAZINE LOCKED";
                        case 0x15: return "MEDIUM MAGAZINE UNLOCKED";
                        case 0x16: return "MECHANICAL POSITIONING OR CHANGER ERROR";
                        case 0x17: return "READ PAST END OF USER OBJECT";
                        case 0x18: return "ELEMENT DISABLED";
                        case 0x19: return "ELEMENT ENABLED";
                        case 0x1A: return "DATA TRANSFER DEVICE REMOVED";
                        case 0x1B: return "DATA TRANSFER DEVICE INSERTED";
                        case 0x1C: return "TOO MANY LOGICAL OBJECTS ON PARTITION TO SUPPORT OPERATION";
                    }

                    break;
                case 0x3D:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID BITS IN IDENTIFY MESSAGE";
                    }

                    break;
                case 0x3E:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT HAS NOT SELF-CONFIGURED YET";
                        case 0x01: return "LOGICAL UNIT FAILURE";
                        case 0x02: return "TIMEOUT ON LOGICAL UNIT";
                        case 0x03: return "LOGICAL UNIT FAILED SELF-TEST";
                        case 0x04: return "LOGICAL UNIT UNABLE TO UPDATE SELF-TEST LOG";
                    }

                    break;
                case 0x3F:
                    switch(ASCQ)
                    {
                        case 0x00: return "TARGET OPERATING CONDITIONS HAVE CHANGED";
                        case 0x01: return "MICROCODE HAS BEEN CHANGED";
                        case 0x02: return "CHANGED OPERATING DEFINITION";
                        case 0x03: return "INQUIRY DATA HAS CHANGED";
                        case 0x04: return "COMPONENT DEVICE ATTACHED";
                        case 0x05: return "DEVICE IDENTIFIED CHANGED";
                        case 0x06: return "REDUNDANCY GROUP CREATED OR MODIFIED";
                        case 0x07: return "REDUNDANCY GROUP DELETED";
                        case 0x08: return "SPARE CREATED OR MODIFIED";
                        case 0x09: return "SPARE DELETED";
                        case 0x0A: return "VOLUME SET CREATED OR MODIFIED";
                        case 0x0B: return "VOLUME SET DELETED";
                        case 0x0C: return "VOLUME SET DEASSIGNED";
                        case 0x0D: return "VOLUME SET REASSIGNED";
                        case 0x0E: return "REPORTED LUNS DATA HAS CHANGED";
                        case 0x0F: return "ECHO BUFFER OVERWRITTEN";
                        case 0x10: return "MEDIUM LOADABLE";
                        case 0x11: return "MEDIUM AUXILIARY MEMORY ACCESSIBLE";
                        case 0x12: return "iSCSI IP ADDRESS ADDED";
                        case 0x13: return "iSCSI IP ADDRESS REMOVED";
                        case 0x14: return "iSCSI IP ADDRESS CHANGED";
                        case 0x15: return "INSPECT REFERRALS SENSE DESCRIPTORS";
                        case 0x16: return "MICROCODE HAS BEEN CHANGED WITHOUT RESET";
                        case 0x17: return "ZONE TRANSITION TO FULL";
                    }

                    break;
                case 0x40:
                    switch(ASCQ)
                    {
                        case 0x00: return "RAM FAILURE";
                        default:   return $"DIAGNOSTIC FAILURE ON COMPONENT {ASCQ:X2}h";
                    }
                case 0x41:
                    switch(ASCQ)
                    {
                        case 0x00: return "DATA PATH FAILURE";
                    }

                    break;
                case 0x42:
                    switch(ASCQ)
                    {
                        case 0x00: return "POWER-ON OR SELF-TEST FAILURE";
                    }

                    break;
                case 0x43:
                    switch(ASCQ)
                    {
                        case 0x00: return "MESSAGE ERROR";
                    }

                    break;
                case 0x44:
                    switch(ASCQ)
                    {
                        case 0x00: return "INTERNAL TARGET FAILURE";
                        case 0x01: return "PERSISTENT RESERVATION INFORMATION LOST";
                        case 0x71: return "ATA DEVICE FAILED SET FEATURES";
                    }

                    break;
                case 0x45:
                    switch(ASCQ)
                    {
                        case 0x00: return "SELECT OR RESELECT FAILURE";
                    }

                    break;
                case 0x46:
                    switch(ASCQ)
                    {
                        case 0x00: return "UNSUCCESSFUL SOFT RESET";
                    }

                    break;
                case 0x47:
                    switch(ASCQ)
                    {
                        case 0x00: return "SCSI PARITY ERROR";
                        case 0x01: return "DATA PHASE CRC ERROR DETECTED";
                        case 0x02: return "SCSI PARITY ERROR DETECTED DURING ST DATA PHASE";
                        case 0x03: return "INFORMATION UNIT iuCRC ERROR DETECTED";
                        case 0x04: return "ASYNCHRONOUS INFORMATION PROTECTION ERROR DETECTED";
                        case 0x05: return "PROTOCOL SERVICE CRC ERROR";
                        case 0x06: return "PHY TEST FUNCTION IN PROGRESS";
                        case 0x7F: return "SOME COMMANDS CLEARED BY iSCSI PROTOCOL EVENT";
                    }

                    break;
                case 0x48:
                    switch(ASCQ)
                    {
                        case 0x00: return "INITIATOR DETECTED ERROR MESSAGE RECEIVED";
                    }

                    break;
                case 0x49:
                    switch(ASCQ)
                    {
                        case 0x00: return "INVALID MESSAGE ERROR";
                    }

                    break;
                case 0x4A:
                    switch(ASCQ)
                    {
                        case 0x00: return "COMMAND PHASE ERROR";
                    }

                    break;
                case 0x4B:
                    switch(ASCQ)
                    {
                        case 0x00: return "DATA PHASE ERROR";
                        case 0x01: return "INVALID TARGET PORT TRANSFER TAG RECEIVED";
                        case 0x02: return "TOO MUCH WRITE DATA";
                        case 0x03: return "ACK/NAK TIMEOUT";
                        case 0x04: return "NAK RECEIVED";
                        case 0x05: return "DATA OFFSET ERROR";
                        case 0x06: return "INITIATOR RESPONSE TIMEOUT";
                        case 0x07: return "CONNECTION LOST";
                        case 0x08: return "DATA-IN BUFFER OVERFLOW - DATA BUFFER SIZE";
                        case 0x09: return "DATA-IN BUFFER OVERFLOW - DATA BUFFER DESCRIPTOR AREA";
                        case 0x0A: return "DATA-IN BUFFER ERROR";
                        case 0x0B: return "DATA-OUT BUFFER OVERFLOW - DATA BUFFER SIZE";
                        case 0x0C: return "DATA-OUT BUFFER OVERFLOW - DATA BUFFER DESCRIPTOR AREA";
                        case 0x0D: return "DATA-OUT BUFFER ERROR";
                        case 0x0E: return "PCIe FABRIC ERROR";
                        case 0x0F: return "PCIe COMPLETION TIMEOUT";
                        case 0x10: return "PCIe COMPLETION ABORT";
                        case 0x11: return "PCIe POISONED TLP RECEIVED";
                        case 0x12: return "PCIe ECRC CHECK FAILED";
                        case 0x13: return "PCIe UNSUPPORTED REQUEST";
                        case 0x14: return "PCIe ACS VIOLATION";
                        case 0x15: return "PCIe TLP PREFIX BLOCKED";
                    }

                    break;
                case 0x4C:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT FAILED SELF-CONFIGURATION";
                    }

                    break;
                case 0x4E: return $"OVERLAPPED COMMANDS ATTEMPTED FOR TASK TAG {ASCQ:X2}h";
                case 0x50:
                    switch(ASCQ)
                    {
                        case 0x00: return "WRITE APPEND ERROR";
                        case 0x01: return "WRITE APPEND POSITION ERROR";
                        case 0x02: return "POSITION ERROR RELATED TO TIMING";
                    }

                    break;
                case 0x51:
                    switch(ASCQ)
                    {
                        case 0x00: return "ERASE FAILURE";
                        case 0x01: return "ERASE FAILURE - INCOMPLETE ERASE OPERATION DETECTED";
                    }

                    break;
                case 0x52:
                    switch(ASCQ)
                    {
                        case 0x00: return "CARTRIDGE FAULT";
                    }

                    break;
                case 0x53:
                    switch(ASCQ)
                    {
                        case 0x00: return "MEDIA LOAD OR EJECT FAILED";
                        case 0x01: return "UNLOAD TAPE FAILURE";
                        case 0x02: return "MEDIUM REMOVAL PREVENTED";
                        case 0x03: return "MEDIUM REMOVAL PREVENTED BY DATA TRANSFER ELEMENT";
                        case 0x04: return "MEDIUM THREAD OR UNTHREAD FAILURE";
                        case 0x05: return "VOLUME IDENTIFIER INVALID";
                        case 0x06: return "VOLUME IDENTIFIED MISSING";
                        case 0x07: return "DUPLICATE VOLUME IDENTIFIER";
                        case 0x08: return "ELEMENT STATUS UNKNOWN";
                        case 0x09: return "DATA TRANSFER DEVICE ERROR - LOAD FAILED";
                        case 0x0A: return "DATA TRANSFER DEVICE ERROR - UNLOAD FAILED";
                        case 0x0B: return "DATA TRANSFER DEVICE ERROR - UNLOAD MISSING";
                        case 0x0C: return "DATA TRANSFER DEVICE ERROR - EJECT FAILED";
                        case 0x0D: return "DATA TRANSFER DEVICE ERROR - LIBRARY COMMUNICATION FAILED";
                    }

                    break;
                case 0x54:
                    switch(ASCQ)
                    {
                        case 0x00: return "SCSI TO HOST SYSTEM INTERFACE FAILURE";
                    }

                    break;
                case 0x55:
                    switch(ASCQ)
                    {
                        case 0x00: return "SYSTEM RESOURCE FAILURE";
                        case 0x01: return "SYSTEM BUFFER FULL";
                        case 0x02: return "INSUFFICIENT RESERVATION RESOURCES";
                        case 0x03: return "INSUFFICIENT RESOURCES";
                        case 0x04: return "INSUFFICIENT REGISTRATION RESOURCES";
                        case 0x05: return "INSUFFICIENT ACCESS CONTROL RESOURCES";
                        case 0x06: return "AUXILIARY MEMORY OUT OF SPACE";
                        case 0x07: return "QUOTA ERROR";
                        case 0x08: return "MAXIMUM NUMBER OF SUPPLEMENTAL DECRYPTION KEYS EXCEEDED";
                        case 0x09: return "MEDIUM AUXILIARY MEMORY NOT ACCESSIBLE";
                        case 0x0A: return "DATA CURRENTLY UNAVAILABLE";
                        case 0x0B: return "INSUFFICIENT POWER FOR OPERATION";
                        case 0x0C: return "INSUFFICIENT RESOURCES TO CREATE ROD";
                        case 0x0D: return "INSUFFICIENT RESOURCES TO CREATE ROD TOKEN";
                        case 0x0E: return "INSUFFICIENT ZONE RESOURCES";
                        case 0x0F: return "INSUFFICIENT ZONE RESOURCES TO COMPLETE WRITE";
                        case 0x10: return "MAXIMUM NUMBER OF STREAMS OPEN";
                    }

                    break;
                case 0x57:
                    switch(ASCQ)
                    {
                        case 0x00: return "UNABLE TO RECOVER TABLE-OF-CONTENTS";
                    }

                    break;
                case 0x58:
                    switch(ASCQ)
                    {
                        case 0x00: return "GENERATION DOES NOT EXIST";
                    }

                    break;
                case 0x59:
                    switch(ASCQ)
                    {
                        case 0x00: return "UPDATED BLOCK READ";
                    }

                    break;
                case 0x5A:
                    switch(ASCQ)
                    {
                        case 0x00: return "OPERATOR REQUEST OR STATE CHANGE INPUT";
                        case 0x01: return "OPERATOR MEDIUM REMOVAL REQUEST";
                        case 0x02: return "OPERATOR SELECTED WRITE PROTECT";
                        case 0x03: return "OPERATOR SELECTED WRITE PERMIT";
                    }

                    break;
                case 0x5B:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOG EXCEPTION";
                        case 0x01: return "THRESHOLD CONDITION MET";
                        case 0x02: return "LOG COUNTER AT MAXIMUM";
                        case 0x03: return "LOG LIST CODES EXHAUSTED";
                    }

                    break;
                case 0x5C:
                    switch(ASCQ)
                    {
                        case 0x00: return "RPL STATUS CHANGE";
                        case 0x01: return "SPINDLES SYNCHRONIZED";
                        case 0x02: return "SPINDLES NOT SYNCHRONIZED";
                        case 0x03: return "SPARE AREA EXHAUSTION PREDICTION THRESHOLD EXCEEDED";
                        case 0x10: return "HARDWARE IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x11: return "HARDWARE IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x12: return "HARDWARE IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x13: return "HARDWARE IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x14: return "HARDWARE IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x15: return "HARDWARE IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x16: return "HARDWARE IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x17: return "HARDWARE IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x18: return "HARDWARE IMPENDING FAILURE CONTROLLER DETECTED";
                        case 0x19: return "HARDWARE IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x1A: return "HARDWARE IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x1B: return "HARDWARE IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x1C: return "HARDWARE IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0x20: return "CONTROLLER IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x21: return "CONTROLLER IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x22: return "CONTROLLER IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x23: return "CONTROLLER IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x24: return "CONTROLLER IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x25: return "CONTROLLER IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x26: return "CONTROLLER IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x27: return "CONTROLLER IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x28: return "CONTROLLER IMPENDING FAILURE CONTROLLER DETECTED";
                        case 0x29: return "CONTROLLER IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x2A: return "CONTROLLER IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x2B: return "CONTROLLER IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x2C: return "CONTROLLER IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0x30: return "DATA CHANNEL IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x31: return "DATA CHANNEL IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x32: return "DATA CHANNEL IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x33: return "DATA CHANNEL IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x34: return "DATA CHANNEL IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x35: return "DATA CHANNEL IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x36: return "DATA CHANNEL IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x37: return "DATA CHANNEL IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x38: return "DATA CHANNEL IMPENDING FAILURE DATA CHANNEL DETECTED";
                        case 0x39: return "DATA CHANNEL IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x3A: return "DATA CHANNEL IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x3B: return "DATA CHANNEL IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x3C: return "DATA CHANNEL IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0x40: return "SERVO IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x41: return "SERVO IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x42: return "SERVO IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x43: return "SERVO IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x44: return "SERVO IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x45: return "SERVO IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x46: return "SERVO IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x47: return "SERVO IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x48: return "SERVO IMPENDING FAILURE SERVO DETECTED";
                        case 0x49: return "SERVO IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x4A: return "SERVO IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x4B: return "SERVO IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x4C: return "SERVO IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0x50: return "SPINDLE IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x51: return "SPINDLE IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x52: return "SPINDLE IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x53: return "SPINDLE IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x54: return "SPINDLE IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x55: return "SPINDLE IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x56: return "SPINDLE IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x57: return "SPINDLE IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x58: return "SPINDLE IMPENDING FAILURE SPINDLE DETECTED";
                        case 0x59: return "SPINDLE IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x5A: return "SPINDLE IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x5B: return "SPINDLE IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x5C: return "SPINDLE IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0x60: return "FIRMWARE IMPENDING FAILURE GENERAL HARD DRIVE FAILURE";
                        case 0x61: return "FIRMWARE IMPENDING FAILURE DRIVE ERROR RATE TOO HIGH";
                        case 0x62: return "FIRMWARE IMPENDING FAILURE DATA ERROR RATE TOO HIGH";
                        case 0x63: return "FIRMWARE IMPENDING FAILURE SEEK ERROR RATE TOO HIGH";
                        case 0x64: return "FIRMWARE IMPENDING FAILURE TOO MANY BLOCK REASSIGNS";
                        case 0x65: return "FIRMWARE IMPENDING FAILURE ACCESS TIME TOO HIGH";
                        case 0x66: return "FIRMWARE IMPENDING FAILURE START UNIT TIMES TOO HIGH";
                        case 0x67: return "FIRMWARE IMPENDING FAILURE CHANNEL PARAMETRICS";
                        case 0x68: return "FIRMWARE IMPENDING FAILURE FIRMWARE DETECTED";
                        case 0x69: return "FIRMWARE IMPENDING FAILURE THROUGHPUT PERFORMANCE";
                        case 0x6A: return "FIRMWARE IMPENDING FAILURE SEEK TIME PERFORMANCE";
                        case 0x6B: return "FIRMWARE IMPENDING FAILURE SPIN-UP RETRY COUNT";
                        case 0x6C: return "FIRMWARE IMPENDING FAILURE DRIVE CALIBRATION RETRY COUNT";
                        case 0xFF: return "FAILURE PREDICTION THRESHOLD EXCEEDED (FALSE)";
                    }

                    break;
                case 0x5E:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOW POWER CONDITION ON";
                        case 0x01: return "IDLE CONDITION ACTIVATED BY TIMER";
                        case 0x02: return "STANDBY CONDITION ACTIVATED BY TIMER";
                        case 0x03: return "IDLE CONDITION ACTIVATED BY COMMAND";
                        case 0x04: return "STANDBY CONDITION ACTIVATED BY COMMAND";
                        case 0x05: return "IDLE_B CONDITION ACTIVATED BY TIMER";
                        case 0x06: return "IDLE_B CONDITION ACTIVATED BY COMMAND";
                        case 0x07: return "IDLE_C CONDITION ACTIVATED BY TIMER";
                        case 0x08: return "IDLE_C CONDITION ACTIVATED BY COMMAND";
                        case 0x09: return "STANDBY_Y CONDITION ACTIVATED BY TIMER";
                        case 0x0A: return "STANDBY_Y CONDITION ACTIVATED BY COMMAND";
                        case 0x41: return "POWER STATE CHANGED TO ACTIVE";
                        case 0x42: return "POWER STATE CHANGED TO IDLE";
                        case 0x43: return "POWER STATE CHANGED TO STANDBY";
                        case 0x45: return "POWER STATE CHANGED TO SLEEP";
                        case 0x47: return "POWER STATE CHANGED TO DEVICE CONTROL";
                    }

                    break;
                case 0x60:
                    switch(ASCQ)
                    {
                        case 0x00: return "LAMP FAILURE";
                    }

                    break;
                case 0x61:
                    switch(ASCQ)
                    {
                        case 0x00: return "VIDEO ACQUISTION ERROR";
                        case 0x01: return "UNABLE TO ACQUIRE VIDEO";
                        case 0x02: return "OUT OF FOCUS";
                    }

                    break;
                case 0x62:
                    switch(ASCQ)
                    {
                        case 0x00: return "SCAN HEAD POSITIONING ERROR";
                    }

                    break;
                case 0x63:
                    switch(ASCQ)
                    {
                        case 0x00: return "END OF USER AREA ENCOUNTERED ON THIS TRACK";
                        case 0x01: return "PACKET DOES NOT FIT IN AVAILABLE SPACE";
                    }

                    break;
                case 0x64:
                    switch(ASCQ)
                    {
                        case 0x00: return "ILLEGAL MODE FOR THIS TRACK";
                        case 0x01: return "INVALID PACKET SIZE";
                    }

                    break;
                case 0x65:
                    switch(ASCQ)
                    {
                        case 0x00: return "VOLTAGE FAULT";
                    }

                    break;
                case 0x66:
                    switch(ASCQ)
                    {
                        case 0x00: return "AUTOMATIC DOCUMENT FEEDER COVER UP";
                        case 0x01: return "AUTOMATIC DOCUMENT FEEDER LIFT UP";
                        case 0x02: return "DOCUMENT JAM IN AUTOMATIC DOCUMENT FEEDER";
                        case 0x03: return "DOCUMENT MISS FEED AUTOMATIC IN DOCUMENT FEEDER";
                    }

                    break;
                case 0x67:
                    switch(ASCQ)
                    {
                        case 0x00: return "CONFIGURATION FAILURE";
                        case 0x01: return "CONFIGURATION OF INCAPABLE LOGICAL UNITS FAILED";
                        case 0x02: return "ADD LOGICAL UNIT FAILED";
                        case 0x03: return "MODIFICATION OF LOGICAL UNIT FAILED";
                        case 0x04: return "EXCHANGE OF LOGICAL UNIT FAILED";
                        case 0x05: return "REMOVE OF LOGICAL UNIT FAILED";
                        case 0x06: return "ATTACHMENT OF LOGICAL UNIT FAILED";
                        case 0x07: return "CREATION OF LOGICAL UNIT FAILED";
                        case 0x08: return "ASSIGN FAILURE OCCURRED";
                        case 0x09: return "MULTIPLY ASSIGNED LOGICAL UNIT";
                        case 0x0A: return "SET TARGET PORT GROUPS COMMAND FAILED";
                        case 0x0B: return "ATA DEVICE FEATURE NOT ENABLED";
                    }

                    break;
                case 0x68:
                    switch(ASCQ)
                    {
                        case 0x00: return "LOGICAL UNIT NOT CONFIGURED";
                        case 0x01: return "SUBSIDIARY LOGICAL UNIT NOT CONFIGURED";
                    }

                    break;
                case 0x69:
                    switch(ASCQ)
                    {
                        case 0x00: return "DATA LOSS ON LOGICAL UNIT";
                        case 0x01: return "MULTIPLE LOGICAL UNIT FAILURES";
                        case 0x02: return "PARITY/DATA MISMATCH";
                    }

                    break;
                case 0x6A:
                    switch(ASCQ)
                    {
                        case 0x00: return "INFORMATIONAL, REFER TO LOG";
                    }

                    break;
                case 0x6B:
                    switch(ASCQ)
                    {
                        case 0x00: return "STATE CHANGE HAS OCCURRED";
                        case 0x01: return "REDUNDANCY LEVEL GOT BETTER";
                        case 0x02: return "REDUNDANCY LEVEL GOT WORSE";
                    }

                    break;
                case 0x6C:
                    switch(ASCQ)
                    {
                        case 0x00: return "REBUILD FAILURE OCCURRED";
                    }

                    break;
                case 0x6D:
                    switch(ASCQ)
                    {
                        case 0x00: return "RECALCULATE FAILURE OCCURRED";
                    }

                    break;
                case 0x6E:
                    switch(ASCQ)
                    {
                        case 0x00: return "COMMAND TO LOGICAL UNIT FAILED";
                    }

                    break;
                case 0x6F:
                    switch(ASCQ)
                    {
                        case 0x00: return "COPY PROTECTION KEY EXCHANGE FAILURE - AUTHENTICATION FAILURE";
                        case 0x01: return "COPY PROTECTION KEY EXCHANGE FAILURE - KEY NOT PRESENT";
                        case 0x02: return "COPY PROTECTION KEY EXCHANGE FAILURE - KEY NOT ESTABLISHED";
                        case 0x03: return "READ OF SCRAMBLED SECTOR WITHOUT AUTHENTICATION";
                        case 0x04: return "MEDIA REGION CODE IS MISMATCHED TO LOGICAL UNIT REGION";
                        case 0x05: return "DRIVE REGION MUST BE PERMANENT/REGION RESET COUNT ERROR";
                        case 0x06: return "INSUFFICIENT BLOCK COUNT FOR BINDING NONCE RECORDING";
                        case 0x07: return "CONFLICT IN BINDING NONCE RECORDING";
                    }

                    break;
                case 0x70: return $"DECOMPRESSION EXCEPTION SHORT ALGORITHM ID OF {ASCQ:X2}h";
                case 0x71:
                    switch(ASCQ)
                    {
                        case 0x00: return "DECOMPRESSIONG EXCEPTION LONG ALGORITHM ID";
                    }

                    break;
                case 0x72:
                    switch(ASCQ)
                    {
                        case 0x00: return "SESSION FIXATION ERROR";
                        case 0x01: return "SESSION FIXATION ERROR WRITING LEAD-IN";
                        case 0x02: return "SESSION FIXATION ERROR WRITING LEAD-OUT";
                        case 0x03: return "SESSION FIXATION ERROR - INCOMPLETE TRACK IN SESSION";
                        case 0x04: return "EMPTY OR PARTIALLY WRITTEN RESERVED TRACK";
                        case 0x05: return "NO MORE TRACK RESERVATIONS ALLOWED";
                        case 0x06: return "RMZ EXTENSION IS NOT ALLOWED";
                        case 0x07: return "NO MORE TEST ZONE EXTENSIONS ARE ALLOWED";
                    }

                    break;
                case 0x73:
                    switch(ASCQ)
                    {
                        case 0x00: return "CD CONTROL ERROR";
                        case 0x01: return "POWER CALIBRATION AREA ALMOST FULL";
                        case 0x02: return "POWER CALIBRATION AREA IS FULL";
                        case 0x03: return "POWER CALIBRATION AREA ERROR";
                        case 0x04: return "PROGRAM MEMORY AREA UPDATE FAILURE";
                        case 0x05: return "PROGRAM MEMORY AREA IS FULL";
                        case 0x06: return "RMA/PMA IS ALMOST FULL";
                        case 0x10: return "CURRENT POWER CALIBRATION AREA ALMOST FULL";
                        case 0x11: return "CURRENT POWER CALIBRATION AREA IS FULL";
                        case 0x17: return "RDZ IS FULL";
                    }

                    break;
                case 0x74:
                    switch(ASCQ)
                    {
                        case 0x00: return "SECURITY ERROR";
                        case 0x01: return "UNABLE TO DECRYPT DATA";
                        case 0x02: return "UNENCRYPTED DATA ENCOUNTERED WHILE DECRYPTING";
                        case 0x03: return "INCORRECT DATA ENCRYPTION KEY";
                        case 0x04: return "CRYPTOGRAPHIC INTEGRITY VALIDATION FAILED";
                        case 0x05: return "ERROR DECRYPTING DATA";
                        case 0x06: return "UNKNOWN SIGNATURE VERIFICATION KEY";
                        case 0x07: return "ENCRYPTION PARAMETERS NOT USEABLE";
                        case 0x08: return "DIGITAL SIGNATURE VALIDATION FAILURE";
                        case 0x09: return "ENCRYPTION MODE MISMATCH ON READ";
                        case 0x0A: return "ENCRYPTED BLOCK NOT RAW READ ENABLED";
                        case 0x0B: return "INCORRECT ENCRYPTION PARAMETERS";
                        case 0x0C: return "UNABLE TO DECRYPT PARAMETER LIST";
                        case 0x0D: return "ENCRYPTION ALGORITHM DISABLED";
                        case 0x10: return "SA CREATION PARAMETER VALUE INVALID";
                        case 0x11: return "SA CREATION PARAMETER VALUE REJECTED";
                        case 0x12: return "INVALID SA USAGE";
                        case 0x21: return "DATA ENCRYPTION CONFIGURATION PREVENTED";
                        case 0x30: return "SA CREATION PARAMETER NOT SUPPORTED";
                        case 0x40: return "AUTHENTICATION FAILED";
                        case 0x61: return "EXTERNAL DATA ENCRYPTION KEY MANAGER ACCESS ERROR";
                        case 0x62: return "EXTERNAL DATA ENCRYPTION KEY MANAGER ERROR";
                        case 0x63: return "EXTERNAL DATA ENCRYPTION KEY NOT FOUND";
                        case 0x64: return "EXTERNAL DATA ENCRYPTION REQUEST NOT AUTHORIZED";
                        case 0x6E: return "EXTERNAL DATA ENCRYPTION CONTROL TIMEOUT";
                        case 0x6F: return "EXTERNAL DATA ENCRYPTION CONTROL ERROR";
                        case 0x71: return "LOGICAL UNIT ACCESS NOT AUTHORIZED";
                        case 0x79: return "SECURITY CONFLICT IN TRANSLATED DEVICE";
                    }

                    break;
            }

            return ASC >= 0x80
                       ? ASCQ >= 0x80
                             ? $"VENDOR-SPECIFIC ASC {ASC:X2}h WITH VENDOR-SPECIFIC ASCQ {ASCQ:X2}h"
                             : $"VENDOR-SPECIFIC ASC {ASC:X2}h WITH ASCQ {ASCQ:X2}h"
                       : ASCQ >= 0x80
                           ? $"ASC {ASC:X2}h WITH VENDOR-SPECIFIC ASCQ {ASCQ:X2}h"
                           : $"ASC {ASC:X2}h WITH ASCQ {ASCQ:X2}h";
        }
    }
}