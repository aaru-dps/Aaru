// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 05.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 05h: Flexible disk page.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x05: Flexible disk page
    /// <summary>Disconnect-reconnect page Page code 0x05 32 bytes in SCSI-2, SBC-1</summary>
    public struct ModePage_05
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Data rate of peripheral device on kbit/s</summary>
        public ushort TransferRate;
        /// <summary>Heads for reading and/or writing</summary>
        public byte Heads;
        /// <summary>Sectors per revolution per head</summary>
        public byte SectorsPerTrack;
        /// <summary>Bytes of data per sector</summary>
        public ushort BytesPerSector;
        /// <summary>Cylinders used for data storage</summary>
        public ushort Cylinders;
        /// <summary>Cylinder where write precompensation starts</summary>
        public ushort WritePrecompCylinder;
        /// <summary>Cylinder where write current reduction starts</summary>
        public ushort WriteReduceCylinder;
        /// <summary>Step rate in 100 μs units</summary>
        public ushort DriveStepRate;
        /// <summary>Width of step pulse in μs</summary>
        public byte DriveStepPulse;
        /// <summary>Head settle time in 100 μs units</summary>
        public ushort HeadSettleDelay;
        /// <summary>
        ///     If <see cref="TRDY" /> is <c>true</c>, specified in 1/10s of a second the time waiting for read status before
        ///     aborting medium access. Otherwise, indicates time to way before medimum access after motor on signal is asserted.
        /// </summary>
        public byte MotorOnDelay;
        /// <summary>
        ///     Time in 1/10s of a second to wait before releasing the motor on signal after an idle condition. 0xFF means to
        ///     never release the signal
        /// </summary>
        public byte MotorOffDelay;
        /// <summary>Specifies if a signal indicates that the medium is ready to be accessed</summary>
        public bool TRDY;
        /// <summary>If <c>true</c> sectors start with one. Otherwise, they start with zero.</summary>
        public bool SSN;
        /// <summary>If <c>true</c> specifies that motor on shall remain released.</summary>
        public bool MO;
        /// <summary>Number of additional step pulses per cylinder.</summary>
        public byte SPC;
        /// <summary>Write compensation value</summary>
        public byte WriteCompensation;
        /// <summary>Head loading time in ms.</summary>
        public byte HeadLoadDelay;
        /// <summary>Head unloading time in ms.</summary>
        public byte HeadUnloadDelay;
        /// <summary>Description of shugart's bus pin 34 usage</summary>
        public byte Pin34;
        /// <summary>Description of shugart's bus pin 2 usage</summary>
        public byte Pin2;
        /// <summary>Description of shugart's bus pin 4 usage</summary>
        public byte Pin4;
        /// <summary>Description of shugart's bus pin 1 usage</summary>
        public byte Pin1;
        /// <summary>Medium speed in rpm</summary>
        public ushort MediumRotationRate;
    }

    public static ModePage_05? DecodeModePage_05(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x05)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 32)
            return null;

        var decoded = new ModePage_05();

        decoded.PS                   |= (pageResponse[0] & 0x80) == 0x80;
        decoded.TransferRate         =  (ushort)((pageResponse[2] << 8) + pageResponse[3]);
        decoded.Heads                =  pageResponse[4];
        decoded.SectorsPerTrack      =  pageResponse[5];
        decoded.BytesPerSector       =  (ushort)((pageResponse[6]  << 8) + pageResponse[7]);
        decoded.Cylinders            =  (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.WritePrecompCylinder =  (ushort)((pageResponse[10] << 8) + pageResponse[11]);
        decoded.WriteReduceCylinder  =  (ushort)((pageResponse[12] << 8) + pageResponse[13]);
        decoded.DriveStepRate        =  (ushort)((pageResponse[14] << 8) + pageResponse[15]);
        decoded.DriveStepPulse       =  pageResponse[16];
        decoded.HeadSettleDelay      =  (ushort)((pageResponse[17] << 8) + pageResponse[18]);
        decoded.MotorOnDelay         =  pageResponse[19];
        decoded.MotorOffDelay        =  pageResponse[20];
        decoded.TRDY                 |= (pageResponse[21]       & 0x80) == 0x80;
        decoded.SSN                  |= (pageResponse[21]       & 0x40) == 0x40;
        decoded.MO                   |= (pageResponse[21]       & 0x20) == 0x20;
        decoded.SPC                  =  (byte)(pageResponse[22] & 0x0F);
        decoded.WriteCompensation    =  pageResponse[23];
        decoded.HeadLoadDelay        =  pageResponse[24];
        decoded.HeadUnloadDelay      =  pageResponse[25];
        decoded.Pin34                =  (byte)((pageResponse[26] & 0xF0) >> 4);
        decoded.Pin2                 =  (byte)(pageResponse[26] & 0x0F);
        decoded.Pin4                 =  (byte)((pageResponse[27] & 0xF0) >> 4);
        decoded.Pin1                 =  (byte)(pageResponse[27] & 0x0F);
        decoded.MediumRotationRate   =  (ushort)((pageResponse[28] << 8) + pageResponse[29]);

        return decoded;
    }

    public static string PrettifyModePage_05(byte[] pageResponse) =>
        PrettifyModePage_05(DecodeModePage_05(pageResponse));

    public static string PrettifyModePage_05(ModePage_05? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_05 page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI Flexible disk page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        sb.AppendFormat("\tTransfer rate: {0} kbit/s", page.TransferRate).AppendLine();
        sb.AppendFormat("\t{0} heads", page.Heads).AppendLine();
        sb.AppendFormat("\t{0} cylinders", page.Cylinders).AppendLine();
        sb.AppendFormat("\t{0} sectors per track", page.SectorsPerTrack).AppendLine();
        sb.AppendFormat("\t{0} bytes per sector", page.BytesPerSector).AppendLine();

        if(page.WritePrecompCylinder < page.Cylinders)
            sb.AppendFormat("\tWrite pre-compensation starts at cylinder {0}", page.WritePrecompCylinder).AppendLine();

        if(page.WriteReduceCylinder < page.Cylinders)
            sb.AppendFormat("\tWrite current reduction starts at cylinder {0}", page.WriteReduceCylinder).AppendLine();

        if(page.DriveStepRate > 0)
            sb.AppendFormat("\tDrive steps in {0} μs", (uint)page.DriveStepRate * 100).AppendLine();

        if(page.DriveStepPulse > 0)
            sb.AppendFormat("\tEach step pulse is {0} ms", page.DriveStepPulse).AppendLine();

        if(page.HeadSettleDelay > 0)
            sb.AppendFormat("\tHeads settles in {0} μs", (uint)page.HeadSettleDelay * 100).AppendLine();

        if(!page.TRDY)
            sb.
                AppendFormat("\tTarget shall wait {0} seconds before attempting to access the medium after motor on is asserted",
                             (double)page.MotorOnDelay * 10).AppendLine();
        else
            sb.
                AppendFormat("\tTarget shall wait {0} seconds after drive is ready before aborting medium access attempts",
                             (double)page.MotorOnDelay * 10).AppendLine();

        if(page.MotorOffDelay != 0xFF)
            sb.AppendFormat("\tTarget shall wait {0} seconds before releasing the motor on signal after becoming idle",
                            (double)page.MotorOffDelay * 10).AppendLine();
        else
            sb.AppendLine("\tTarget shall never release the motor on signal");

        if(page.TRDY)
            sb.AppendLine("\tThere is a drive ready signal");

        if(page.SSN)
            sb.AppendLine("\tSectors start at 1");

        if(page.MO)
            sb.AppendLine("\tThe motor on signal shall remain released");

        sb.AppendFormat("\tDrive needs to do {0} step pulses per cylinder", page.SPC + 1).AppendLine();

        if(page.WriteCompensation > 0)
            sb.AppendFormat("\tWrite pre-compensation is {0}", page.WriteCompensation).AppendLine();

        if(page.HeadLoadDelay > 0)
            sb.AppendFormat("\tHead takes {0} ms to load", page.HeadLoadDelay).AppendLine();

        if(page.HeadUnloadDelay > 0)
            sb.AppendFormat("\tHead takes {0} ms to unload", page.HeadUnloadDelay).AppendLine();

        if(page.MediumRotationRate > 0)
            sb.AppendFormat("\tMedium rotates at {0} rpm", page.MediumRotationRate).AppendLine();

        switch(page.Pin34 & 0x07)
        {
            case 0:
                sb.AppendLine("\tPin 34 is unconnected");

                break;
            case 1:
                sb.Append("\tPin 34 indicates drive is ready when active ");
                sb.Append((page.Pin34 & 0x08) == 0x08 ? "high" : "low");

                break;
            case 2:
                sb.Append("\tPin 34 indicates disk has changed when active ");
                sb.Append((page.Pin34 & 0x08) == 0x08 ? "high" : "low");

                break;
            default:
                sb.AppendFormat("\tPin 34 indicates unknown function {0} when active ", page.Pin34 & 0x07);
                sb.Append((page.Pin34 & 0x08) == 0x08 ? "high" : "low");

                break;
        }

        switch(page.Pin4 & 0x07)
        {
            case 0:
                sb.AppendLine("\tPin 4 is unconnected");

                break;
            case 1:
                sb.Append("\tPin 4 indicates drive is in use when active ");
                sb.Append((page.Pin4 & 0x08) == 0x08 ? "high" : "low");

                break;
            case 2:
                sb.Append("\tPin 4 indicates eject when active ");
                sb.Append((page.Pin4 & 0x08) == 0x08 ? "high" : "low");

                break;
            case 3:
                sb.Append("\tPin 4 indicates head load when active ");
                sb.Append((page.Pin4 & 0x08) == 0x08 ? "high" : "low");

                break;
            default:
                sb.AppendFormat("\tPin 4 indicates unknown function {0} when active ", page.Pin4 & 0x07);
                sb.Append((page.Pin4 & 0x08) == 0x08 ? "high" : "low");

                break;
        }

        switch(page.Pin2 & 0x07)
        {
            case 0:
                sb.AppendLine("\tPin 2 is unconnected");

                break;
            default:
                sb.AppendFormat("\tPin 2 indicates unknown function {0} when active ", page.Pin2 & 0x07);
                sb.Append((page.Pin2 & 0x08) == 0x08 ? "high" : "low");

                break;
        }

        switch(page.Pin1 & 0x07)
        {
            case 0:
                sb.AppendLine("\tPin 1 is unconnected");

                break;
            case 1:
                sb.Append("\tPin 1 indicates disk change reset when active ");
                sb.Append((page.Pin1 & 0x08) == 0x08 ? "high" : "low");

                break;
            default:
                sb.AppendFormat("\tPin 1 indicates unknown function {0} when active ", page.Pin1 & 0x07);
                sb.Append((page.Pin1 & 0x08) == 0x08 ? "high" : "low");

                break;
        }

        return sb.ToString();
    }
    #endregion Mode Page 0x05: Flexible disk page
}