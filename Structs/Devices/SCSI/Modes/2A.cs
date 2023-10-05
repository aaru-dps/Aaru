// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2A.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Common structures for SCSI devices.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aaru.CommonTypes.Structs.Devices.SCSI.Modes;

#region Mode Page 0x2A: CD-ROM capabilities page

/// <summary>
///     CD-ROM capabilities page Page code 0x2A 16 bytes in OB-U0077C 20 bytes in SFF-8020i 22 bytes in MMC-1 26 bytes
///     in MMC-2 Variable bytes in MMC-3
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class ModePage_2A
{
    /// <summary>Write speed performance descriptors</summary>
    public ModePage_2A_WriteDescriptor[] WriteSpeedPerformanceDescriptors;

    /// <summary>Parameters can be saved</summary>
    public bool PS { get; set; }

    /// <summary>Drive supports multi-session and/or Photo-CD</summary>
    public bool MultiSession { get; set; }

    /// <summary>Drive is capable of reading sectors in Mode 2 Form 2 format</summary>
    public bool Mode2Form2 { get; set; }

    /// <summary>Drive is capable of reading sectors in Mode 2 Form 1 format</summary>
    public bool Mode2Form1 { get; set; }

    /// <summary>Drive is capable of playing audio</summary>
    public bool AudioPlay { get; set; }

    /// <summary>Drive can return the ISRC</summary>
    public bool ISRC { get; set; }

    /// <summary>Drive can return the media catalogue number</summary>
    public bool UPC { get; set; }

    /// <summary>Drive can return C2 pointers</summary>
    public bool C2Pointer { get; set; }

    /// <summary>Drive can read, deinterlave and correct R-W subchannels</summary>
    public bool DeinterlaveSubchannel { get; set; }

    /// <summary>Drive can read interleaved and uncorrected R-W subchannels</summary>
    public bool Subchannel { get; set; }

    /// <summary>Drive can continue from a loss of streaming on audio reading</summary>
    public bool AccurateCDDA { get; set; }

    /// <summary>Audio can be read as digital data</summary>
    public bool CDDACommand { get; set; }

    /// <summary>Loading Mechanism Type</summary>
    public byte LoadingMechanism { get; set; }

    /// <summary>Drive can eject discs</summary>
    public bool Eject { get; set; }

    /// <summary>Drive's optional prevent jumper status</summary>
    public bool PreventJumper { get; set; }

    /// <summary>Current lock status</summary>
    public bool LockState { get; set; }

    /// <summary>Drive can lock media</summary>
    public bool Lock { get; set; }

    /// <summary>Each channel can be muted independently</summary>
    public bool SeparateChannelMute { get; set; }

    /// <summary>Each channel's volume can be controlled independently</summary>
    public bool SeparateChannelVolume { get; set; }

    /// <summary>Maximum drive speed in Kbytes/second</summary>
    public ushort MaximumSpeed { get; set; }

    /// <summary>Supported volume levels</summary>
    public ushort SupportedVolumeLevels { get; set; }

    /// <summary>Buffer size in Kbytes</summary>
    public ushort BufferSize { get; set; }

    /// <summary>Current drive speed in Kbytes/second</summary>
    public ushort CurrentSpeed { get; set; }

    /// <summary>Can read packet media</summary>
    public bool Method2 { get; set; }

    /// <summary>Can read CD-RW</summary>
    public bool ReadCDRW { get; set; }

    /// <summary>Can read CD-R</summary>
    public bool ReadCDR { get; set; }

    /// <summary>Can write CD-RW</summary>
    public bool WriteCDRW { get; set; }

    /// <summary>Can write CD-R</summary>
    public bool WriteCDR { get; set; }

    /// <summary>Supports IEC-958 digital output on port 2</summary>
    public bool DigitalPort2 { get; set; }

    /// <summary>Supports IEC-958 digital output on port 1</summary>
    public bool DigitalPort1 { get; set; }

    /// <summary>Can deliver a composite audio and video data stream</summary>
    public bool Composite { get; set; }

    /// <summary>This bit controls the behavior of the LOAD/UNLOAD command when trying to load a Slot with no Disc present</summary>
    public bool SSS { get; set; }

    /// <summary>Contains a changer that can report the exact contents of the slots</summary>
    public bool SDP { get; set; }

    /// <summary>Page length</summary>
    public byte Length { get; set; }

    /// <summary>Set if LSB comes first</summary>
    public bool LSBF { get; set; }

    /// <summary>Set if HIGH on LRCK indicates left channel. Clear if HIGH on LRCK indicates right channel.</summary>
    public bool RCK { get; set; }

    /// <summary>
    ///     Set if data valid on the falling edge of the BCK signal. Clear if data valid on the rising edge of the BCK
    ///     signal
    /// </summary>
    public bool BCK { get; set; }

    /// <summary>Can do a test write</summary>
    public bool TestWrite { get; set; }

    /// <summary>Maximum write speed</summary>
    public ushort MaxWriteSpeed { get; set; }

    /// <summary>Current write speed</summary>
    public ushort CurrentWriteSpeed { get; set; }

    /// <summary>Can read disc's barcode</summary>
    public bool ReadBarcode { get; set; }

    /// <summary>Can read DVD-RAM</summary>
    public bool ReadDVDRAM { get; set; }

    /// <summary>Can read DVD-R</summary>
    public bool ReadDVDR { get; set; }

    /// <summary>Can read DVD-ROM</summary>
    public bool ReadDVDROM { get; set; }

    /// <summary>Can write DVD-RAM</summary>
    public bool WriteDVDRAM { get; set; }

    /// <summary>Can write DVD-R</summary>
    public bool WriteDVDR { get; set; }

    /// <summary>Can read raw R-W subchannel from the Lead-In</summary>
    public bool LeadInPW { get; set; }

    /// <summary>Can read both sides of a disc</summary>
    public bool SCC { get; set; }

    /// <summary>Support copyright management</summary>
    public ushort CMRSupported { get; set; }

    /// <summary>Supports buffer under-run free recording</summary>
    public bool BUF { get; set; }

    /// <summary>Selected rotational control</summary>
    public byte RotationControlSelected { get; set; }

    /// <summary>Current write speed selected</summary>
    public ushort CurrentWriteSpeedSelected { get; set; }

    /// <summary>Database ID</summary>
    [JsonIgnore]
    [Key]
    // ReSharper disable once UnusedMember.Global
    public int Id { get; set; }

    /// <summary>Decodes the page 2Ah of a MODE SENSE response</summary>
    /// <param name="pageResponse">Raw page 2Ah</param>
    /// <returns>Decoded page 2Ah</returns>
    public static ModePage_2A Decode(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x2A)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_2A();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.AudioPlay    |= (pageResponse[4] & 0x01) == 0x01;
        decoded.Mode2Form1   |= (pageResponse[4] & 0x10) == 0x10;
        decoded.Mode2Form2   |= (pageResponse[4] & 0x20) == 0x20;
        decoded.MultiSession |= (pageResponse[4] & 0x40) == 0x40;

        decoded.CDDACommand           |= (pageResponse[5] & 0x01) == 0x01;
        decoded.AccurateCDDA          |= (pageResponse[5] & 0x02) == 0x02;
        decoded.Subchannel            |= (pageResponse[5] & 0x04) == 0x04;
        decoded.DeinterlaveSubchannel |= (pageResponse[5] & 0x08) == 0x08;
        decoded.C2Pointer             |= (pageResponse[5] & 0x10) == 0x10;
        decoded.UPC                   |= (pageResponse[5] & 0x20) == 0x20;
        decoded.ISRC                  |= (pageResponse[5] & 0x40) == 0x40;

        decoded.LoadingMechanism =  (byte)((pageResponse[6] & 0xE0) >> 5);
        decoded.Lock             |= (pageResponse[6] & 0x01) == 0x01;
        decoded.LockState        |= (pageResponse[6] & 0x02) == 0x02;
        decoded.PreventJumper    |= (pageResponse[6] & 0x04) == 0x04;
        decoded.Eject            |= (pageResponse[6] & 0x08) == 0x08;

        decoded.SeparateChannelVolume |= (pageResponse[7] & 0x01) == 0x01;
        decoded.SeparateChannelMute   |= (pageResponse[7] & 0x02) == 0x02;

        decoded.MaximumSpeed          = (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.SupportedVolumeLevels = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
        decoded.BufferSize            = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
        decoded.CurrentSpeed          = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

        if(pageResponse.Length < 20)
            return decoded;

        decoded.Method2  |= (pageResponse[2] & 0x04) == 0x04;
        decoded.ReadCDRW |= (pageResponse[2] & 0x02) == 0x02;
        decoded.ReadCDR  |= (pageResponse[2] & 0x01) == 0x01;

        decoded.WriteCDRW |= (pageResponse[3] & 0x02) == 0x02;
        decoded.WriteCDR  |= (pageResponse[3] & 0x01) == 0x01;

        decoded.Composite    |= (pageResponse[4] & 0x02) == 0x02;
        decoded.DigitalPort1 |= (pageResponse[4] & 0x04) == 0x04;
        decoded.DigitalPort2 |= (pageResponse[4] & 0x08) == 0x08;

        decoded.SDP |= (pageResponse[7] & 0x04) == 0x04;
        decoded.SSS |= (pageResponse[7] & 0x08) == 0x08;

        decoded.Length =  (byte)((pageResponse[17] & 0x30) >> 4);
        decoded.LSBF   |= (pageResponse[17] & 0x08) == 0x08;
        decoded.RCK    |= (pageResponse[17] & 0x04) == 0x04;
        decoded.BCK    |= (pageResponse[17] & 0x02) == 0x02;

        if(pageResponse.Length < 22)
            return decoded;

        decoded.TestWrite         |= (pageResponse[3] & 0x04) == 0x04;
        decoded.MaxWriteSpeed     =  (ushort)((pageResponse[18] << 8) + pageResponse[19]);
        decoded.CurrentWriteSpeed =  (ushort)((pageResponse[20] << 8) + pageResponse[21]);

        decoded.ReadBarcode |= (pageResponse[5] & 0x80) == 0x80;

        if(pageResponse.Length < 26)
            return decoded;

        decoded.ReadDVDRAM |= (pageResponse[2] & 0x20) == 0x20;
        decoded.ReadDVDR   |= (pageResponse[2] & 0x10) == 0x10;
        decoded.ReadDVDROM |= (pageResponse[2] & 0x08) == 0x08;

        decoded.WriteDVDRAM |= (pageResponse[3] & 0x20) == 0x20;
        decoded.WriteDVDR   |= (pageResponse[3] & 0x10) == 0x10;

        decoded.LeadInPW |= (pageResponse[3] & 0x20) == 0x20;
        decoded.SCC      |= (pageResponse[3] & 0x10) == 0x10;

        decoded.CMRSupported = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

        if(pageResponse.Length < 32)
            return decoded;

        decoded.BUF                       |= (pageResponse[4]        & 0x80) == 0x80;
        decoded.RotationControlSelected   =  (byte)(pageResponse[27] & 0x03);
        decoded.CurrentWriteSpeedSelected =  (ushort)((pageResponse[28] << 8) + pageResponse[29]);

        var descriptors = (ushort)((pageResponse.Length - 32) / 4);
        decoded.WriteSpeedPerformanceDescriptors = new ModePage_2A_WriteDescriptor[descriptors];

        for(var i = 0; i < descriptors; i++)
        {
            decoded.WriteSpeedPerformanceDescriptors[i] = new ModePage_2A_WriteDescriptor
            {
                RotationControl = (byte)(pageResponse[1                                         + 32 + i * 4] & 0x07),
                WriteSpeed      = (ushort)((pageResponse[2 + 32 + i * 4] << 8) + pageResponse[3 + 32 + i * 4])
            };
        }

        return decoded;
    }

    /// <summary>Encodes a page 2Ah of a MODE SENSE response</summary>
    /// <param name="decoded">Decoded page 2Ah</param>
    /// <returns>Raw page 2Ah</returns>
    public static byte[] Encode(ModePage_2A decoded)
    {
        var  pageResponse = new byte[512];
        byte length       = 16;

        pageResponse[0] = 0x2A;

        if(decoded.PS)
            pageResponse[0] += 0x80;

        if(decoded.AudioPlay)
            pageResponse[4] += 0x01;

        if(decoded.Mode2Form1)
            pageResponse[4] += 0x10;

        if(decoded.Mode2Form2)
            pageResponse[4] += 0x20;

        if(decoded.MultiSession)
            pageResponse[4] += 0x40;

        if(decoded.CDDACommand)
            pageResponse[5] += 0x01;

        if(decoded.AccurateCDDA)
            pageResponse[5] += 0x02;

        if(decoded.Subchannel)
            pageResponse[5] += 0x04;

        if(decoded.DeinterlaveSubchannel)
            pageResponse[5] += 0x08;

        if(decoded.C2Pointer)
            pageResponse[5] += 0x10;

        if(decoded.UPC)
            pageResponse[5] += 0x20;

        if(decoded.ISRC)
            pageResponse[5] += 0x40;

        decoded.LoadingMechanism = (byte)((pageResponse[6] & 0xE0) >> 5);

        if(decoded.Lock)
            pageResponse[6] += 0x01;

        if(decoded.LockState)
            pageResponse[6] += 0x02;

        if(decoded.PreventJumper)
            pageResponse[6] += 0x04;

        if(decoded.Eject)
            pageResponse[6] += 0x08;

        if(decoded.SeparateChannelVolume)
            pageResponse[7] += 0x01;

        if(decoded.SeparateChannelMute)
            pageResponse[7] += 0x02;

        decoded.MaximumSpeed          = (ushort)((pageResponse[8]  << 8) + pageResponse[9]);
        decoded.SupportedVolumeLevels = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
        decoded.BufferSize            = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
        decoded.CurrentSpeed          = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

        if(decoded.Method2      ||
           decoded.ReadCDRW     ||
           decoded.ReadCDR      ||
           decoded.WriteCDRW    ||
           decoded.WriteCDR     ||
           decoded.Composite    ||
           decoded.DigitalPort1 ||
           decoded.DigitalPort2 ||
           decoded.SDP          ||
           decoded.SSS          ||
           decoded.Length > 0   ||
           decoded.LSBF         ||
           decoded.RCK          ||
           decoded.BCK)
        {
            length = 20;

            if(decoded.Method2)
                pageResponse[2] += 0x04;

            if(decoded.ReadCDRW)
                pageResponse[2] += 0x02;

            if(decoded.ReadCDR)
                pageResponse[2] += 0x01;

            if(decoded.WriteCDRW)
                pageResponse[3] += 0x02;

            if(decoded.WriteCDR)
                pageResponse[3] += 0x01;

            if(decoded.Composite)
                pageResponse[4] += 0x02;

            if(decoded.DigitalPort1)
                pageResponse[4] += 0x04;

            if(decoded.DigitalPort2)
                pageResponse[4] += 0x08;

            if(decoded.SDP)
                pageResponse[7] += 0x04;

            if(decoded.SSS)
                pageResponse[7] += 0x08;

            pageResponse[17] = (byte)(decoded.Length << 4);

            if(decoded.LSBF)
                pageResponse[17] += 0x08;

            if(decoded.RCK)
                pageResponse[17] += 0x04;

            if(decoded.BCK)
                pageResponse[17] += 0x02;
        }

        if(decoded.TestWrite || decoded.MaxWriteSpeed > 0 || decoded.CurrentWriteSpeed > 0 || decoded.ReadBarcode)
        {
            length = 22;

            if(decoded.TestWrite)
                pageResponse[3] += 0x04;

            pageResponse[18] = (byte)((decoded.MaxWriteSpeed & 0xFF00) >> 8);
            pageResponse[19] = (byte)(decoded.MaxWriteSpeed & 0xFF);
            pageResponse[20] = (byte)((decoded.CurrentWriteSpeed & 0xFF00) >> 8);
            pageResponse[21] = (byte)(decoded.CurrentWriteSpeed & 0xFF);

            if(decoded.ReadBarcode)
                pageResponse[5] += 0x80;
        }

        if(decoded.ReadDVDRAM  ||
           decoded.ReadDVDR    ||
           decoded.ReadDVDROM  ||
           decoded.WriteDVDRAM ||
           decoded.WriteDVDR   ||
           decoded.LeadInPW    ||
           decoded.SCC         ||
           decoded.CMRSupported > 0)

        {
            length = 26;

            if(decoded.ReadDVDRAM)
                pageResponse[2] += 0x20;

            if(decoded.ReadDVDR)
                pageResponse[2] += 0x10;

            if(decoded.ReadDVDROM)
                pageResponse[2] += 0x08;

            if(decoded.WriteDVDRAM)
                pageResponse[3] += 0x20;

            if(decoded.WriteDVDR)
                pageResponse[3] += 0x10;

            if(decoded.LeadInPW)
                pageResponse[3] += 0x20;

            if(decoded.SCC)
                pageResponse[3] += 0x10;

            pageResponse[22] = (byte)((decoded.CMRSupported & 0xFF00) >> 8);
            pageResponse[23] = (byte)(decoded.CMRSupported & 0xFF);
        }

        if(decoded.BUF || decoded.RotationControlSelected > 0 || decoded.CurrentWriteSpeedSelected > 0)
        {
            length = 32;

            if(decoded.BUF)
                pageResponse[4] += 0x80;

            pageResponse[27] += decoded.RotationControlSelected;
            pageResponse[28] =  (byte)((decoded.CurrentWriteSpeedSelected & 0xFF00) >> 8);
            pageResponse[29] =  (byte)(decoded.CurrentWriteSpeedSelected & 0xFF);
        }

        if(decoded.WriteSpeedPerformanceDescriptors != null)
        {
            length = 32;

            for(var i = 0; i < decoded.WriteSpeedPerformanceDescriptors.Length; i++)
            {
                length                       += 4;
                pageResponse[1 + 32 + i * 4] =  decoded.WriteSpeedPerformanceDescriptors[i].RotationControl;

                pageResponse[2 + 32 + i * 4] =
                    (byte)((decoded.WriteSpeedPerformanceDescriptors[i].WriteSpeed & 0xFF00) >> 8);

                pageResponse[3 + 32 + i * 4] = (byte)(decoded.WriteSpeedPerformanceDescriptors[i].WriteSpeed & 0xFF);
            }
        }

        pageResponse[1] = (byte)(length - 2);
        var buf = new byte[length];
        Array.Copy(pageResponse, 0, buf, 0, length);

        return buf;
    }
}

/// <summary>Page 2Ah write descriptor</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public struct ModePage_2A_WriteDescriptor
{
    /// <summary>Rotational control</summary>
    public byte RotationControl;
    /// <summary>Write speed</summary>
    public ushort WriteSpeed;
}

#endregion Mode Page 0x2A: CD-ROM capabilities page