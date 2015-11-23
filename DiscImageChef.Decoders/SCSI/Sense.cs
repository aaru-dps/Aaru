// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Sense.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders.SCSI
{
    public enum SenseType
    {
        StandardSense,
        ExtendedSenseFixedCurrent,
        ExtendedSenseFixedPast,
        ExtendedSenseDescriptorCurrent,
        ExtendedSenseDescriptorPast,
        Invalid,
        Unknown
    }

    public struct StandardSense
    {
        /// <summary>
        /// If set, <see cref="LBA"/> is valid
        /// </summary>
        public bool AddressValid;
        /// <summary>
        /// Error class, 0 to 6
        /// </summary>
        public byte ErrorClass;
        /// <summary>
        /// Error type
        /// </summary>
        public byte ErrorType;
        /// <summary>
        /// Private usage
        /// </summary>
        public byte Private;
        /// <summary>
        /// LBA where error happened
        /// </summary>
        public uint LBA;
    }

    public enum SenseKeys : byte
    {
        /// <summary>
        /// No information to be reported, but bits should be checked
        /// </summary>
        NoSense = 0,
        /// <summary>
        /// Target performed some recovery to successfully complete last command
        /// </summary>
        RecoveredError = 1,
        /// <summary>
        /// Target is not ready
        /// </summary>
        NotReady = 2,
        /// <summary>
        /// Non-recoverable medium error occurred
        /// </summary>
        MediumError = 3,
        /// <summary>
        /// Non-recoverable hardware error occurred
        /// </summary>
        HardwareError = 4,
        /// <summary>
        /// Target has received an illegal request
        /// </summary>
        IllegalRequest = 5,
        /// <summary>
        /// Target requires initiator attention
        /// </summary>
        UnitAttention = 6,
        /// <summary>
        /// A protected command has been denied
        /// </summary>
        DataProtect = 7,
        /// <summary>
        /// A blank block has been tried to read or a non-rewritable one to write
        /// </summary>
        BlankCheck = 8,
        /// <summary>
        /// For private/vendor usage
        /// </summary>
        PrivateUse = 9,
        /// <summary>
        /// COPY command aborted
        /// </summary>
        CopyAborted = 0xA,
        /// <summary>
        /// Command aborted
        /// </summary>
        AbortedCommand = 0xB,
        /// <summary>
        /// SEARCH command has been satisfied
        /// </summary>
        Equal = 0xC,
        /// <summary>
        /// End-of-medium reached with data remaining in buffer
        /// </summary>
        VolumeOverflow = 0xD,
        /// <summary>
        /// COMPARE failed
        /// </summary>
        Miscompare = 0xE,
        /// <summary>
        /// Reserved
        /// </summary>
        Reserved = 0xF
    }

    public struct FixedSense
    {
        /// <summary>
        /// If set, <see cref="Information"/> is valid
        /// </summary>
        public bool InformationValid;
        /// <summary>
        /// Contains number of current segment descriptor
        /// </summary>
        public byte SegmentNumber;
        /// <summary>
        /// If set indicates current command has read a filemark or a setmark
        /// </summary>
        public bool Filemark;
        /// <summary>
        /// If set indicates device has arrived end-of-medium
        /// </summary>
        public bool EOM;
        /// <summary>
        /// Means the requested logical block length did not match the logical block length on the medium
        /// </summary>
        public bool ILI;
        /// <summary>
        /// Contains the sense key
        /// </summary>
        public SenseKeys SenseKey;
        /// <summary>
        /// Additional information
        /// </summary>
        public uint Information;
        /// <summary>
        /// Additional sense length
        /// </summary>
        public byte AdditionalLength;
        /// <summary>
        /// Command specific information field
        /// </summary>
        public uint CommandSpecific;
        /// <summary>
        /// Additional sense code
        /// </summary>
        public byte ASC;
        /// <summary>
        /// Additional sense code qualifier
        /// </summary>
        public byte ASCQ;
        public byte FieldReplaceable;
        /// <summary>
        /// If set, <see cref="SenseKeySpecific"/> is valid
        /// </summary>
        public bool SKSV;
        public uint SenseKeySpecific;
        public byte[] AdditionalSense;
    }

    public static class Sense
    {
        /// <summary>
        /// Gets the SCSI SENSE type to help chosing the correct decoding function
        /// </summary>
        /// <returns>The type.</returns>
        /// <param name="sense">Sense bytes.</param>
        public static SenseType GetType(byte[] sense)
        {
            if (sense == null)
                return SenseType.Invalid;

            if (sense.Length < 4)
                return SenseType.Invalid;

            if ((sense[0] & 0x70) != 0x70)
                return sense.Length != 4 ? SenseType.Invalid : SenseType.StandardSense;

            switch (sense[0] & 0x0F)
            {
                case 0:
                    return SenseType.ExtendedSenseFixedCurrent;
                case 1:
                    return SenseType.ExtendedSenseFixedPast;
                case 2:
                    return SenseType.ExtendedSenseDescriptorCurrent;
                case 3:
                    return SenseType.ExtendedSenseDescriptorPast;
                default:
                    return SenseType.Unknown;
            }
        }

        public static StandardSense? DecodeStandard(byte[] sense)
        {
            if (GetType(sense) != SenseType.StandardSense)
                return null;

            StandardSense decoded = new StandardSense();
            decoded.AddressValid |= (sense[0] & 0x80) == 0x80;
            decoded.ErrorClass = (byte)((sense[0] & 0x70) >> 4);
            decoded.ErrorType = (byte)(sense[0] & 0x0F);
            decoded.Private = (byte)((sense[1] & 0x80) >> 4);
            decoded.LBA = (uint)(((sense[1] & 0x0F) << 16) + (sense[2] << 8) + sense[3]);

            return decoded;
        }

        public static FixedSense? DecodeFixed(byte[] sense)
        {
            string foo;
            return DecodeFixed(sense, out foo);
        }

        public static FixedSense? DecodeFixed(byte[] sense, out string senseDescription)
        {
            senseDescription = null;
            if((sense[0] & 0x7F) != 0x70 || 
                (sense[0] & 0x7F) != 0x71)
                return null;

            if (sense.Length < 8)
                return null;

            FixedSense decoded = new FixedSense();

            decoded.InformationValid |= (sense[0] & 0x80) == 0x80;
            decoded.SegmentNumber = sense[1];
            decoded.Filemark |= (sense[2] & 0x80) == 0x80;
            decoded.EOM |= (sense[2] & 0x40) == 0x40;
            decoded.ILI |= (sense[2] & 0x20) == 0x20;
            decoded.SenseKey = (SenseKeys)(sense[2] & 0x0F);
            decoded.Information = (uint)((sense[3] << 24) + (sense[4] << 16) + (sense[5] << 8) + sense[6]);
            decoded.AdditionalLength = sense[7];

            if (sense.Length != decoded.AdditionalLength + 8)
                return decoded;

            if(sense.Length >= 12)
                decoded.CommandSpecific = (uint)((sense[8] << 24) + (sense[9] << 16) + (sense[10] << 8) + sense[11]);

            if (sense.Length >= 14)
            {
                decoded.ASC = sense[12];
                decoded.ASCQ = sense[13];
                senseDescription = GetSenseDescription(decoded.ASC, decoded.ASCQ);
            }

            if (sense.Length >= 15)
                decoded.FieldReplaceable = sense[14];

            if(sense.Length >= 18)
                decoded.SenseKeySpecific = (uint)((sense[15] << 16) + (sense[16] << 8) + sense[17]);

            if (sense.Length > 18)
            {
                decoded.AdditionalSense = new byte[sense.Length - 18];
                Array.Copy(sense, 18, decoded.AdditionalSense, 0, decoded.AdditionalSense.Length);
            }

            return decoded;
        }

        public static string PrettifySense(byte[] sense)
        {
            SenseType type = GetType(sense);
                
            switch (type)
            {
                case SenseType.StandardSense:
                    return PrettifySense(DecodeStandard(sense));
                case SenseType.ExtendedSenseFixedCurrent:
                case SenseType.ExtendedSenseFixedPast:
                    return PrettifySense(DecodeFixed(sense));
                default:
                    return null;
            }
        }

        public static string PrettifySense(StandardSense? sense)
        {
            if (!sense.HasValue)
                return null;

            return sense.Value.AddressValid ? String.Format("Error class {0} type {1} happened on block {2}\n",
                sense.Value.ErrorClass, sense.Value.ErrorType, sense.Value.LBA) :
                String.Format("Error class {0} type {1}\n", sense.Value.ErrorClass,
                    sense.Value.ErrorType);
        }

        public static string PrettifySense(FixedSense? sense)
        {
            if (!sense.HasValue)
                return null;

            FixedSense decoded = sense.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("SCSI SENSE: {0}", GetSenseKey(decoded.SenseKey)).AppendLine();
            if (decoded.SegmentNumber > 0)
                sb.AppendFormat("On segment {0}", decoded.SegmentNumber).AppendLine();
            if (decoded.Filemark)
                sb.AppendLine("Filemark or setmark found");
            if (decoded.EOM)
                sb.AppendLine("End-of-medium/partition found");
            if (decoded.ILI)
                sb.AppendLine("Incorrect length indicator");
            if (decoded.InformationValid)
                sb.AppendFormat("On logical block {0}", decoded.Information);

            if (decoded.AdditionalLength < 6)
                return sb.ToString();

            sb.AppendLine(GetSenseDescription(decoded.ASC, decoded.ASCQ));

            if (decoded.AdditionalLength < 10)
                return sb.ToString();

            if (decoded.SKSV)
            {
                switch (decoded.SenseKey)
                {
                    case SenseKeys.IllegalRequest:
                        {
                            if ((decoded.SenseKeySpecific & 0x400000) == 0x400000)
                                sb.AppendLine("Illegal field in CDB");
                            else
                                sb.AppendLine("Illegal field in data parameters");

                            if ((decoded.SenseKeySpecific & 0x200000) == 0x200000)
                                sb.AppendFormat("Invalid value in bit {0} in field {1} of CDB",
                                    (decoded.SenseKeySpecific & 0x70000) >> 16,
                                    decoded.SenseKeySpecific & 0xFFFF).AppendLine();
                            else
                                sb.AppendFormat("Invalid value in field {0} of CDB",
                                    decoded.SenseKeySpecific & 0xFFFF).AppendLine();
                        }
                        break;
                    case SenseKeys.NotReady:
                        sb.AppendFormat("Format progress {0:P}", (double)(decoded.SenseKeySpecific & 0xFFFF) / 65536).AppendLine();
                        break;
                    case SenseKeys.RecoveredError:
                    case SenseKeys.HardwareError:
                    case SenseKeys.MediumError:
                        sb.AppendFormat("Actual retry count is {0}", decoded.SenseKeySpecific & 0xFFFF).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }

        public static string GetSenseKey(SenseKeys key)
        {
            switch (key)
            {
                case SenseKeys.AbortedCommand:
                    return "ABORTED COMMAND";
                case SenseKeys.BlankCheck:
                    return "BLANK CHECK";
                case SenseKeys.CopyAborted:
                    return "COPY ABORTED";
                case SenseKeys.DataProtect:
                    return "DATA PROTECT";
                case SenseKeys.Equal:
                    return "EQUAL";
                case SenseKeys.HardwareError:
                    return "HARDWARE ERROR";
                case SenseKeys.IllegalRequest:
                    return "ILLEGAL REQUEST";
                case SenseKeys.MediumError:
                    return "MEDIUM ERROR";
                case SenseKeys.Miscompare:
                    return "MISCOMPARE";
                case SenseKeys.NoSense:
                    return "NO SENSE";
                case SenseKeys.PrivateUse:
                    return "PRIVATE USE";
                case SenseKeys.RecoveredError:
                    return "RECOVERED ERROR";
                case SenseKeys.Reserved:
                    return "RETURN";
                case SenseKeys.UnitAttention:
                    return "UNIT ATTENTION";
                case SenseKeys.VolumeOverflow:
                    return "VOLUME OVERFLOW";
                default:
                    return "UNKNOWN";
            }
        }

        public static string GetSenseDescription(byte ASC, byte ASCQ)
        {
            switch (ASC)
            {
                case 0x00:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "NO ADDITIONAL SENSE INFORMATION";
                        case 0x01:
                            return "FILEMARK DETECTED";
                        case 0x02:
                            return "END-OF-PARTITION/MEDIUM DETECTED";
                        case 0x03:
                            return "SETMARK DETECTED";
                        case 0x04:
                            return "BEGINNING-OF-PARTITION/MEDIUM DETECTED";
                        case 0x05:
                            return "END-OF-DATA DETECTED";
                        case 0x06:
                            return "I/O PROCESS TERMINATED";
                        case 0x11:
                            return "AUDIO PLAY OPERATION IN PROGRESS";
                        case 0x12:
                            return "AUDIO PLAY OPERATION PAUSED";
                        case 0x13:
                            return "AUDIO PLAY OPERATION SUCCESSFULLY COMPLETED";
                        case 0x14:
                            return "AUDIO PLAY OPERATION STOPPED DUE TO ERROR";
                        case 0x15:
                            return "NO CURRENT AUDIO STATUS TO RETURN";
                    }
                    break;
                case 0x01:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "NO INDEX/SECTOR SIGNAL";
                    }
                    break;
                case 0x02:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "NO SEEK COMPLETE";
                    }
                    break;
                case 0x03:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "PERIPHERAL DEVICE WRITE FAULT";
                        case 0x01:
                            return "NO WRITE CURRENT";
                        case 0x02:
                            return "EXCESSIVE WRITE ERRORS";
                    }
                    break;
                case 0x04:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT NOT READY, CAUSE NOT REPORTABLE";
                        case 0x01:
                            return "LOGICAL UNIT IS IN PROCESS OF BECOMING READY";
                        case 0x02:
                            return "LOGICAL UNIT NOT READY, INITIALIZING COMMAND REQUIRED";
                        case 0x03:
                            return "LOGICAL UNIT NOT READY, MANUAL INTERVENTION REQUIRED";
                        case 0x04:
                            return "LOGICAL UNIT NOT READY, FORMAT IN PROGRESS";
                    }
                    break;
                case 0x05:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT DOES NOT RESPOND TO SELECTION";
                    }
                    break;
                case 0x06:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "REFERENCE POSITION FOUND";
                    }
                    break;
                case 0x07:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MULTIPLE PERIPHERAL DEVICES SELECTED";
                    }
                    break;
                case 0x08:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT COMMUNICATION FAILURE";
                        case 0x01:
                            return "LOGICAL UNIT COMMUNICATION TIME-OUT";
                        case 0x02:
                            return "LOGICAL UNIT COMMUNICATION PARITY ERROR";
                    }
                    break;
                case 0x09:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "TRACK FLOLLOWING ERROR";
                        case 0x01:
                            return "TRACKING SERVO FAILURE";
                        case 0x02:
                            return "FOCUS SERVO FAILURE";
                        case 0x03:
                            return "SPINDLE SERVO FAILURE";
                    }
                    break;
                case 0x0A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ERROR LOG OVERFLOW";
                    }
                    break;
                case 0x0C:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "WRITE ERROR";
                        case 0x01:
                            return "WRITE ERROR RECOVERED WITH AUTO REALLOCATION";
                        case 0x02:
                            return "WRITE ERROR - AUTO REALLOCATION FAILED";
                    }
                    break;
                case 0x10:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ID CRC OR ECC ERROR";
                    }
                    break;
                case 0x11:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "UNRECOVERED READ ERROR";
                        case 0x01:
                            return "READ RETRIES EXHAUSTED";
                        case 0x02:
                            return "ERROR TOO LONG TO CORRECT";
                        case 0x03:
                            return "MULTIPLE READ ERRORS";
                        case 0x04:
                            return "UNRECOVERED READ ERROR - AUTO REALLOCATE FAILED";
                        case 0x05:
                            return "L-EC UNCORRECTABLE ERROR";
                        case 0x06:
                            return "CIRC UNRECOVERED ERROR";
                        case 0x07:
                            return "DATA RESYNCHRONIZATION ERROR";
                        case 0x08:
                            return "INCOMPLETE BLOCK READ";
                        case 0x09:
                            return "NO GAP FOUND";
                        case 0x0A:
                            return "MISCORRECTED ERROR";
                        case 0x0B:
                            return "UNRECOVERED READ ERROR - RECOMMENDED REASSIGNMENT";
                        case 0x0C:
                            return "UNRECOVERED READ ERROR - RECOMMENDED REWRITE THE DATA";
                    }
                    break;
                case 0x12:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ADDRESS MARK NOT FOUND FOR ID FIELD";
                    }
                    break;
                case 0x13:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ADDRESS MARK NOT FOUND FOR DATA FIELD";
                    }
                    break;
                case 0x14:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RECORDED ENTITY NOT FOUND";
                        case 0x01:
                            return "RECORD NOT FOUND";
                        case 0x02:
                            return "FILEMARK OR SETMARK NOT FOUND";
                        case 0x03:
                            return "END-OF-DATA NOT FOUND";
                        case 0x04:
                            return "BLOCK SEQUENCE ERROR";
                    }
                    break;
                case 0x15:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RANDOM POSITIONING ERROR";
                        case 0x01:
                            return "MECHANICAL POSITIONING ERROR";
                        case 0x02:
                            return "POSITIONING ERROR DETECTED BY READ OF MEDIUM";
                    }
                    break;
                case 0x16:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "DATA SYNCHRONIZATION MARK ERROR";
                    }
                    break;
                case 0x17:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RECOVERED DATA WITH NO ERROR CORRECTION APPLIED";
                        case 0x01:
                            return "RECOVERED DATA WITH RETRIES";
                        case 0x02:
                            return "RECOVERED DATA WITH POSITIVE HEAD OFFSET";
                        case 0x03:
                            return "RECOVERED DATA WITH NEGATIVE HEAD OFFSET";
                        case 0x04:
                            return "RECOVERED DATA WITH RETRIES AND/OR CIRC APPLIED";
                        case 0x05:
                            return "RECOVERED DATA USING PREVIOUS SECTOR ID";
                        case 0x06:
                            return "RECOVERED DATA WITHOUT ECC - DATA AUTO-REALLOCATED";
                        case 0x07:
                            return "RECOVERED DATA WITHOUT ECC - RECOMMENDED REASSIGNMENT";
                        case 0x08:
                            return "RECOVERED DATA WITHOUT ECC - RECOMMENDED REWRITE";
                    }
                    break;
                case 0x18:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RECOVERED DATA WITH ERROR CORRECTION APPLIED";
                        case 0x01:
                            return "RECOVERED DATA WITH ERROR CORRECTION & RETRIES APPLIED";
                        case 0x02:
                            return "RECOVERED DATA - DATA AUTO-REALLOCATED";
                        case 0x03:
                            return "RECOVERED DATA WITH CIRC";
                        case 0x04:
                            return "RECOVERED DATA WITH L-EC";
                        case 0x05:
                            return "RECOVERED DATA - RECOMMENDED REASSIGNMENT";
                        case 0x06:
                            return "RECOVERED DATA - RECOMMENDED REWRITE";
                    }
                    break;
                case 0x19:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "DEFECT LIST ERROR";
                        case 0x01:
                            return "DEFECT LIST NOT AVAILABLE";
                        case 0x02:
                            return "DEFECT LIST ERROR IN PRIMARY LIST";
                        case 0x03:
                            return "DEFECT LIST ERROR IN GROWN LIST";
                    }
                    break;
                case 0x1A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "PARAMETER LIST LENGTH ERROR";
                    }
                    break;
                case 0x1B:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SYNCHRONOUS DATA TRANSFER ERROR";
                    }
                    break;
                case 0x1C:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "DEFECT LIST NOT FOUND";
                        case 0x01:
                            return "PRIMARY DEFECT LIST NOT FOUND";
                        case 0x02:
                            return "GROWN DEFECT LIST NOT FOUND";
                    }
                    break;
                case 0x1D:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MISCOMPARE DURING VERIFY OPERATION";
                    }
                    break;
                case 0x1E:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RECOVERED ID WITH ECC";
                    }
                    break;
                case 0x20:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INVALID COMMAND OPERATION CODE";
                    }
                    break;
                case 0x21:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL BLOCK ADDRESS OUT OF RANGE";
                        case 0x01:
                            return "INVALID ELEMENT ADDRESS";
                    }
                    break;
                case 0x22:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ILLEGAL FUNCTION";
                    }
                    break;
                case 0x24:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ILLEGAL FIELD IN CDB";
                    }
                    break;
                case 0x25:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT NOT SUPPORTED";
                    }
                    break;
                case 0x26:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INVALID FIELD IN PARAMETER LIST";
                        case 0x01:
                            return "PARAMETER NOT SUPPORTED";
                        case 0x02:
                            return "PARAMETER VALUE INVALID";
                        case 0x03:
                            return "THRESHOLD PARAMETERS NOT SUPPORTED";
                    }
                    break;
                case 0x27:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "WRITE PROTECTED";
                    }
                    break;
                case 0x28:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "NOT READY TO READY TRANSITION (MEDIUM MAY HAVE CHANGED)";
                        case 0x01:
                            return "IMPORT OR EXPORT ELEMENT ACCESSED";
                    }
                    break;
                case 0x29:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "POWER ON, RESET, OR BUS DEVICE RESET OCCURRED";
                    }
                    break;
                case 0x2A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "PARAMETERS CHANGED";
                        case 0x01:
                            return "MODE PARAMETERS CHANGED";
                        case 0x02:
                            return "LOG PARAMETERS CHANGED";
                    }
                    break;
                case 0x2B:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "COPY CANNOT EXECUTE SINCE HOST CANNOT DISCONNECT";
                    }
                    break;
                case 0x2C:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "COMMAND SEQUENCE ERROR";
                        case 0x01:
                            return "TOO MANY WINDOWS SPECIFIED";
                        case 0x02:
                            return "INVALID COMBINATION OF WINDOWS SPECIFIED";
                    }
                    break;
                case 0x2D:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "OVERWRITE ERROR ON UPDATE IN PLACE";
                    }
                    break;
                case 0x2F:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "COMMANDS CLEARED BY ANOTHER INITIATOR";
                    }
                    break;
                case 0x30:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INCOMPATIBLE MEDIUM INSTALLED";
                        case 0x01:
                            return "CANNOT READ MEDIUM - UNKNOWN FORMAT";
                        case 0x02:
                            return "CANNOT READ MEDIUM - INCOMPATIBLE FORMAT";
                        case 0x03:
                            return "CLEANING CARTRIDGE INSTALLED";
                    }
                    break;
                case 0x31:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MEDIUM FORMAT CORRUPTED";
                        case 0x01:
                            return "FORMAT COMMAND FAILED";
                    }
                    break;
                case 0x32:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "NO DEFECT SPARE LOCATION AVAILABLE";
                        case 0x01:
                            return "DEFECT LIST UPDATE FAILURE";
                    }
                    break;
                case 0x33:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "TAPE LENGTH ERROR";
                    }
                    break;
                case 0x36:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RIBBON, INK, OR TONER FAILURE";
                    }
                    break;
                case 0x37:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ROUNDED PARAMETER";
                    }
                    break;
                case 0x39:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SAVING PARAMETERS NOT SUPPORTED";
                    }
                    break;
                case 0x3A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MEDIUM NOT PRESENT";
                    }
                    break;
                case 0x3B:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SEQUENTIAL POSITIONING ERROR";
                        case 0x01:
                            return "TAPE POSITION ERROR AT BEGINNING-OF-MEDIUM";
                        case 0x02:
                            return "TAPE POSITION ERROR AT END-OF-MEDIUM";
                        case 0x03:
                            return "TAPE OR ELECTRONIC VERTICAL FORMS UNIT NOT READY";
                        case 0x04:
                            return "SLEW FAILURE";
                        case 0x05:
                            return "PAPER JAM";
                        case 0x06:
                            return "FAILED TO SENSE TOP-OF-FORM";
                        case 0x07:
                            return "FAILED TO SENSE BOTTOM-OF-FORM";
                        case 0x08:
                            return "REPOSITION ERROR";
                        case 0x09:
                            return "READ PAST END OF MEDIUM";
                        case 0x0A:
                            return "READ PAST BEGINNING OF MEDIUM";
                        case 0x0B:
                            return "POSITION PAST END OF MEDIUM";
                        case 0x0C:
                            return "POSITION PAST BEGINNING OF MEDIUM";
                        case 0x0D:
                            return "MEDIUM DESTINATION ELEMENT FULL";
                        case 0x0E:
                            return "MEDIUM SOURCE ELEMENT EMPTY";
                    }
                    break;
                case 0x3D:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INVALID BITS IN IDENTIFY MESSAGE";
                    }
                    break;
                case 0x3E:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT HAS NOT SELF-CONFIGURED YET";
                    }
                    break;
                case 0x3F:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "TARGET OPERATING CONDITIONS HAVE CHANGED";
                        case 0x01:
                            return "MICROCODE HAS BEEN CHANGED";
                        case 0x02:
                            return "CHANGED OPERATING DEFINITION";
                        case 0x03:
                            return "INQUIRY DATA HAS CHANGED";
                    }
                    break;
                case 0x40:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RAM FAILURE";
                        default:
                            return String.Format("DIAGNOSTIC FAILURE ON COMPONENT {0:X2}h", ASCQ);
                    }
                case 0x41:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "DATA PATH FAILURE";
                    }
                    break;
                case 0x42:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "POWER-ON OR SELF-TEST FAILURE";
                    }
                    break;
                case 0x43:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MESSAGE ERROR";
                    }
                    break;
                case 0x44:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INTERNAL TARGET FAILURE";
                    }
                    break;
                case 0x45:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SELECT OR RESELECT FAILURE";
                    }
                    break;
                case 0x46:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "UNSUCCESSFUL SOFT RESET";
                    }
                    break;
                case 0x47:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SCSI PARITY ERROR";
                    }
                    break;
                case 0x48:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INITIATOR DETECTED ERROR MESSAGE RECEIVED";
                    }
                    break;
                case 0x49:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "INVALID MESSAGE ERROR";
                    }
                    break;
                case 0x4A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "COMMAND PHASE ERROR";
                    }
                    break;
                case 0x4B:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "DATA PHASE ERROR";
                    }
                    break;
                case 0x4C:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOGICAL UNIT FAILED SELF-CONFIGURATION";
                    }
                    break;
                case 0x4E:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "OVERLAPPED COMMANDS ATTEMPTED";
                    }
                    break;
                case 0x50:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "WRITE APPEND ERROR";
                        case 0x01:
                            return "WRITE APPEND POSITION ERROR";
                        case 0x02:
                            return "POSITION ERROR RELATED TO TIMING";
                    }
                    break;
                case 0x51:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ERASE FAILURE";
                    }
                    break;
                case 0x52:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "CARTRIDGE FAULT";
                    }
                    break;
                case 0x53:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "MEDIA LOAD OR EJECT FAILED";
                        case 0x01:
                            return "UNLOAD TAPE FAILURE";
                        case 0x02:
                            return "MEDIUM REMOVAL PREVENTED";
                    }
                    break;
                case 0x54:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SCSI TO HOST SYSTEM INTERFACE FAILURE";
                    }
                    break;
                case 0x55:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SYSTEM RESOURCE FAILURE";
                    }
                    break;
                case 0x57:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "UNABLE TO RECOVER TABLE-OF-CONTENTS";
                    }
                    break;
                case 0x58:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "GENERATION DOES NOT EXIST";
                    }
                    break;
                case 0x59:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "UPDATED BLOCK READ";
                    }
                    break;
                case 0x5A:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "OPERATOR REQUEST OR STATE CHANGE INPUT";
                        case 0x01:
                            return "OPERATOR MEDIUM REMOVAL REQUEST";
                        case 0x02:
                            return "OPERATOR SELECTED WRITE PROTECT";
                        case 0x03:
                            return "OPERATOR SELECTED WRITE PERMIT";
                    }
                    break;
                case 0x5B:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LOG EXCEPTION";
                        case 0x01:
                            return "THRESHOLD CONDITION MET";
                        case 0x02:
                            return "LOG COUNTER AT MAXIMUM";
                        case 0x03:
                            return "LOG LIST CODES EXHAUSTED";
                    }
                    break;
                case 0x5C:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "RPL STATUS CHANGE";
                        case 0x01:
                            return "SPINDLES SYNCHRONIZED";
                        case 0x02:
                            return "SPINDLES NOT SYNCHRONIZED";
                    }
                    break;
                case 0x60:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "LAMP FAILURE";
                    }
                    break;
                case 0x61:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "VIDEO ACQUISTION ERROR";
                        case 0x01:
                            return "UNABLE TO ACQUIRE VIDEO";
                        case 0x02:
                            return "OUT OF FOCUS";
                    }
                    break;
                case 0x62:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "SCAN HEAD POSITIONING ERROR";
                    }
                    break;
                case 0x63:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "END OF USER AREA ENCOUNTERED ON THIS TRACK";
                    }
                    break;
                case 0x64:
                    switch (ASCQ)
                    {
                        case 0x00:
                            return "ILLEGAL MODE FOR THIS TRACK";
                    }
                    break;
            }

            return ASC >= 0x80 ? ASCQ >= 0x80 ?
                String.Format("VENDOR-SPECIFIC ASC {0:X2}h WITH VENDOR-SPECIFIC ASCQ {1:X2}h", ASC, ASCQ) :
                String.Format("VENDOR-SPECIFIC ASC {0:X2}h WITH ASCQ {1:X2}h", ASC, ASCQ) :
                ASCQ >= 0x80 ? String.Format("ASC {0:X2}h WITH VENDOR-SPECIFIC ASCQ {1:X2}h", ASC, ASCQ) :
                String.Format("ASC {0:X2}h WITH ASCQ {1:X2}h", ASC, ASCQ);
        }
    }
}

