// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : General.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from SCSI devices.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Core.Devices.Report
{
    public partial class DeviceReport
    {
        public Scsi ReportScsiInquiry()
        {
            AaruConsole.WriteLine("Querying SCSI INQUIRY...");
            bool sense = _dev.ScsiInquiry(out byte[] buffer, out byte[] senseBuffer);

            var report = new Scsi();

            if(sense)
                return null;

            Inquiry? decodedNullable = Inquiry.Decode(buffer);

            if(!decodedNullable.HasValue)
                return null;

            report.InquiryData = ClearInquiry(buffer);

            return report;
        }

        public static byte[] ClearInquiry(byte[] inquiry)
        {
            Inquiry? decodedNullable = Inquiry.Decode(inquiry);

            if(!decodedNullable.HasValue)
                return inquiry;

            Inquiry decoded = decodedNullable.Value;

            // Clear Seagate serial number
            if(decoded.SeagatePresent &&
               StringHandlers.CToString(decoded.VendorIdentification)?.Trim().ToLowerInvariant() == "seagate")
                for(int i = 36; i <= 43; i++)
                    inquiry[i] = 0;

            return inquiry;
        }

        public List<ScsiPage> ReportEvpdPages(string vendor)
        {
            AaruConsole.WriteLine("Querying list of SCSI EVPDs...");
            bool sense = _dev.ScsiInquiry(out byte[] buffer, out _, 0x00);

            if(sense)
                return null;

            byte[] evpdPages = EVPD.DecodePage00(buffer);

            if(evpdPages        == null ||
               evpdPages.Length <= 0)
                return null;

            List<ScsiPage> evpds = new List<ScsiPage>();

            foreach(byte page in evpdPages.Where(page => page != 0x80))
            {
                AaruConsole.WriteLine("Querying SCSI EVPD {0:X2}h...", page);
                sense = _dev.ScsiInquiry(out buffer, out _, page);

                if(sense)
                    continue;

                byte[] empty;

                switch(page)
                {
                    case 0x83:
                        buffer = ClearPage83(buffer);

                        break;
                    case 0x80:
                        byte[] identify = new byte[512];
                        Array.Copy(buffer, 60, identify, 0, 512);
                        identify = ClearIdentify(identify);
                        Array.Copy(identify, 0, buffer, 60, 512);

                        break;
                    case 0xB1:
                    case 0xB3:
                        empty = new byte[buffer.Length - 4];
                        Array.Copy(empty, 0, buffer, 4, buffer.Length - 4);

                        break;
                    case 0xC1 when vendor == "ibm":
                        empty = new byte[12];
                        Array.Copy(empty, 0, buffer, 4, 12);
                        Array.Copy(empty, 0, buffer, 16, 12);

                        break;
                    case 0xC2 when vendor == "certance":
                    case 0xC3 when vendor == "certance":
                    case 0xC4 when vendor == "certance":
                    case 0xC5 when vendor == "certance":
                    case 0xC6 when vendor == "certance":
                        Array.Copy(new byte[12], 0, buffer, 4, 12);

                        break;
                }

                var evpd = new ScsiPage
                {
                    page  = page,
                    value = buffer
                };

                evpds.Add(evpd);
            }

            return evpds.Count > 0 ? evpds : null;
        }

        byte[] ClearPage83(byte[] pageResponse)
        {
            if(pageResponse?[1] != 0x83)
                return null;

            if(pageResponse[3] + 4 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 6)
                return null;

            int position = 4;

            while(position < pageResponse.Length)
            {
                byte length = pageResponse[position + 3];

                if(length + position + 4 >= pageResponse.Length)
                    length = (byte)(pageResponse.Length - position - 4);

                byte[] empty = new byte[length];
                Array.Copy(empty, 0, pageResponse, position + 4, length);

                position += 4 + length;
            }

            return pageResponse;
        }

        public void ReportScsiModes(ref DeviceReportV2 report, out byte[] cdromMode)
        {
            Modes.DecodedMode?    decMode = null;
            PeripheralDeviceTypes devType = _dev.ScsiType;
            byte[]                mode10CurrentBuffer;
            byte[]                mode10ChangeableBuffer;
            byte[]                mode6CurrentBuffer;
            byte[]                mode6ChangeableBuffer;

            AaruConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (10)...");

            bool sense = _dev.ModeSense10(out byte[] mode10Buffer, out _, false, true, ScsiModeSensePageControl.Default,
                                          0x3F, 0xFF, _dev.Timeout, out _);

            if(sense || _dev.Error)
            {
                AaruConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (10)...");

                sense = _dev.ModeSense10(out mode10Buffer, out _, false, true, ScsiModeSensePageControl.Default, 0x3F,
                                         0x00, _dev.Timeout, out _);

                if(!sense &&
                   !_dev.Error)
                {
                    report.SCSI.SupportsModeSense10  = true;
                    report.SCSI.SupportsModeSubpages = false;
                    decMode                          = Modes.DecodeMode10(mode10Buffer, devType);

                    {
                        sense = _dev.ModeSense10(out mode10CurrentBuffer, out _, false, true,
                                                 ScsiModeSensePageControl.Current, 0x3F, 0x00, _dev.Timeout, out _);

                        if(!sense &&
                           !_dev.Error)
                            report.SCSI.ModeSense10CurrentData = mode10CurrentBuffer;

                        sense = _dev.ModeSense10(out mode10ChangeableBuffer, out _, false, true,
                                                 ScsiModeSensePageControl.Changeable, 0x3F, 0x00, _dev.Timeout, out _);

                        if(!sense &&
                           !_dev.Error)
                            report.SCSI.ModeSense10ChangeableData = mode10ChangeableBuffer;
                    }
                }
            }
            else
            {
                report.SCSI.SupportsModeSense10  = true;
                report.SCSI.SupportsModeSubpages = true;
                decMode                          = Modes.DecodeMode10(mode10Buffer, devType);

                {
                    sense = _dev.ModeSense10(out mode10CurrentBuffer, out _, false, true,
                                             ScsiModeSensePageControl.Current, 0x3F, 0xFF, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense10CurrentData = mode10CurrentBuffer;

                    sense = _dev.ModeSense10(out mode10ChangeableBuffer, out _, false, true,
                                             ScsiModeSensePageControl.Changeable, 0x3F, 0xFF, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense10ChangeableData = mode10ChangeableBuffer;
                }
            }

            AaruConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (6)...");

            sense = _dev.ModeSense6(out byte[] mode6Buffer, out _, false, ScsiModeSensePageControl.Default, 0x3F, 0xFF,
                                    _dev.Timeout, out _);

            if(sense || _dev.Error)
            {
                AaruConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (6)...");

                sense = _dev.ModeSense6(out mode6Buffer, out _, false, ScsiModeSensePageControl.Default, 0x3F, 0x00,
                                        _dev.Timeout, out _);

                if(sense || _dev.Error)
                {
                    AaruConsole.WriteLine("Querying SCSI MODE SENSE (6)...");
                    sense = _dev.ModeSense(out mode6Buffer, out _, _dev.Timeout, out _);
                }
                else
                {
                    sense = _dev.ModeSense6(out mode6CurrentBuffer, out _, false, ScsiModeSensePageControl.Current,
                                            0x3F, 0x00, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense6CurrentData = mode6CurrentBuffer;

                    sense = _dev.ModeSense6(out mode6ChangeableBuffer, out _, false,
                                            ScsiModeSensePageControl.Changeable, 0x3F, 0x00, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense6ChangeableData = mode6ChangeableBuffer;
                }
            }
            else
            {
                report.SCSI.SupportsModeSubpages = true;

                {
                    sense = _dev.ModeSense6(out mode6CurrentBuffer, out _, false, ScsiModeSensePageControl.Current,
                                            0x3F, 0xFF, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense6CurrentData = mode6CurrentBuffer;

                    sense = _dev.ModeSense6(out mode6ChangeableBuffer, out _, false,
                                            ScsiModeSensePageControl.Changeable, 0x3F, 0xFF, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                        report.SCSI.ModeSense6ChangeableData = mode6ChangeableBuffer;
                }
            }

            if(!sense      &&
               !_dev.Error &&
               !decMode.HasValue)
                decMode = Modes.DecodeMode6(mode6Buffer, devType);

            report.SCSI.SupportsModeSense6 |= !sense && !_dev.Error;

            cdromMode = null;

            if(report.SCSI.SupportsModeSense6)
                report.SCSI.ModeSense6Data = mode6Buffer;

            if(report.SCSI.SupportsModeSense10)
                report.SCSI.ModeSense10Data = mode10Buffer;

            if(!decMode.HasValue)
                return;

            report.SCSI.ModeSense = new ScsiMode
            {
                BlankCheckEnabled = decMode.Value.Header.EBC,
                DPOandFUA         = decMode.Value.Header.DPOFUA,
                WriteProtected    = decMode.Value.Header.WriteProtected
            };

            if(decMode.Value.Header.BufferedMode > 0)
                report.SCSI.ModeSense.BufferedMode = decMode.Value.Header.BufferedMode;

            if(decMode.Value.Header.Speed > 0)
                report.SCSI.ModeSense.Speed = decMode.Value.Header.Speed;

            if(decMode.Value.Pages == null)
                return;

            List<ScsiPage> modePages = new List<ScsiPage>();

            foreach(Modes.ModePage page in decMode.Value.Pages)
            {
                var modePage = new ScsiPage
                {
                    page    = page.Page,
                    subpage = page.Subpage,
                    value   = page.PageResponse
                };

                modePages.Add(modePage);

                if(modePage.page    == 0x2A &&
                   modePage.subpage == 0x00)
                    cdromMode = page.PageResponse;
            }

            if(modePages.Count > 0)
                report.SCSI.ModeSense.ModePages = modePages;
        }

        public TestedMedia ReportScsiMedia()
        {
            var mediaTest = new TestedMedia();
            AaruConsole.WriteLine("Querying SCSI READ CAPACITY...");
            bool sense = _dev.ReadCapacity(out byte[] buffer, out byte[] senseBuffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                mediaTest.SupportsReadCapacity = true;

                mediaTest.Blocks = (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;

                mediaTest.BlockSize = (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
            }

            AaruConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
            sense = _dev.ReadCapacity16(out buffer, out buffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                mediaTest.SupportsReadCapacity16 = true;
                byte[] temp = new byte[8];
                Array.Copy(buffer, 0, temp, 0, 8);
                Array.Reverse(temp);
                mediaTest.Blocks    = BitConverter.ToUInt64(temp, 0) + 1;
                mediaTest.BlockSize = (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + buffer[11]);
            }

            Modes.DecodedMode? decMode = null;

            AaruConsole.WriteLine("Querying SCSI MODE SENSE (10)...");

            sense = _dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                     0x00, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                decMode                   = Modes.DecodeMode10(buffer, _dev.ScsiType);
                mediaTest.ModeSense10Data = buffer;
            }

            AaruConsole.WriteLine("Querying SCSI MODE SENSE...");
            sense = _dev.ModeSense(out buffer, out senseBuffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                if(!decMode.HasValue)
                    decMode = Modes.DecodeMode6(buffer, _dev.ScsiType);

                mediaTest.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;

                if(decMode.Value.Header.BlockDescriptors        != null &&
                   decMode.Value.Header.BlockDescriptors.Length > 0)
                    mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            AaruConsole.WriteLine("Trying SCSI READ (6)...");

            mediaTest.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 0,
                                                  mediaTest.BlockSize ?? 512, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);
            mediaTest.Read6Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (10)...");

            mediaTest.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
            mediaTest.Read10Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (12)...");

            mediaTest.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
            mediaTest.Read12Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (16)...");

            mediaTest.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
            mediaTest.Read16Data = buffer;

            mediaTest.LongBlockSize = mediaTest.BlockSize;
            AaruConsole.WriteLine("Trying SCSI READ LONG (10)...");
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                FixedSense? decSense = Sense.DecodeFixed(senseBuffer);

                if(decSense.HasValue)
                    if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                       decSense.Value.ASC      == 0x24                     &&
                       decSense.Value.ASCQ     == 0x00)
                    {
                        mediaTest.SupportsReadLong = true;

                        if(decSense.Value.InformationValid &&
                           decSense.Value.ILI)
                            mediaTest.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                    }
            }

            if(mediaTest.SupportsReadLong == true &&
               mediaTest.LongBlockSize    == mediaTest.BlockSize)
                if(mediaTest.BlockSize == 512)
                    foreach(int i in new[]
                    {
                        // Long sector sizes for floppies
                        514,

                        // Long sector sizes for SuperDisk
                        536, 558,

                        // Long sector sizes for 512-byte magneto-opticals
                        600, 610, 630
                    })
                    {
                        ushort testSize = (ushort)i;

                        sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, _dev.Timeout,
                                                out _);

                        if(sense || _dev.Error)
                            continue;

                        mediaTest.SupportsReadLong = true;
                        mediaTest.LongBlockSize    = testSize;

                        break;
                    }
                else if(mediaTest.BlockSize == 1024)
                    foreach(int i in new[]
                    {
                        // Long sector sizes for floppies
                        1026,

                        // Long sector sizes for 1024-byte magneto-opticals
                        1200
                    })
                    {
                        ushort testSize = (ushort)i;

                        sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i, _dev.Timeout,
                                                out _);

                        if(sense || _dev.Error)
                            continue;

                        mediaTest.SupportsReadLong = true;
                        mediaTest.LongBlockSize    = testSize;

                        break;
                    }
                else if(mediaTest.BlockSize == 2048)
                {
                    sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                    {
                        mediaTest.SupportsReadLong = true;
                        mediaTest.LongBlockSize    = 2380;
                    }
                }
                else if(mediaTest.BlockSize == 4096)
                {
                    sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                    {
                        mediaTest.SupportsReadLong = true;
                        mediaTest.LongBlockSize    = 4760;
                    }
                }
                else if(mediaTest.BlockSize == 8192)
                {
                    sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, _dev.Timeout, out _);

                    if(!sense &&
                       !_dev.Error)
                    {
                        mediaTest.SupportsReadLong = true;
                        mediaTest.LongBlockSize    = 9424;
                    }
                }

            AaruConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");

            mediaTest.CanReadMediaSerial =
                !_dev.ReadMediaSerialNumber(out buffer, out senseBuffer, _dev.Timeout, out _);

            return mediaTest;
        }

        public TestedMedia ReportScsi()
        {
            var capabilities = new TestedMedia
            {
                MediaIsRecognized = true
            };

            AaruConsole.WriteLine("Querying SCSI READ CAPACITY...");
            bool sense = _dev.ReadCapacity(out byte[] buffer, out byte[] senseBuffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                capabilities.SupportsReadCapacity = true;

                capabilities.Blocks = (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) + 1;

                capabilities.BlockSize = (uint)((buffer[4] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + buffer[7]);
            }

            AaruConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
            sense = _dev.ReadCapacity16(out buffer, out buffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                capabilities.SupportsReadCapacity16 = true;
                byte[] temp = new byte[8];
                Array.Copy(buffer, 0, temp, 0, 8);
                Array.Reverse(temp);
                capabilities.Blocks    = BitConverter.ToUInt64(temp, 0) + 1;
                capabilities.BlockSize = (uint)((buffer[8] << 24) + (buffer[9] << 16) + (buffer[10] << 8) + buffer[11]);
            }

            Modes.DecodedMode? decMode = null;

            AaruConsole.WriteLine("Querying SCSI MODE SENSE (10)...");

            sense = _dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F,
                                     0x00, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                decMode                      = Modes.DecodeMode10(buffer, _dev.ScsiType);
                capabilities.ModeSense10Data = buffer;
            }

            AaruConsole.WriteLine("Querying SCSI MODE SENSE...");
            sense = _dev.ModeSense(out buffer, out senseBuffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                if(!decMode.HasValue)
                    decMode = Modes.DecodeMode6(buffer, _dev.ScsiType);

                capabilities.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                capabilities.MediumType = (byte)decMode.Value.Header.MediumType;

                if(decMode.Value.Header.BlockDescriptors        != null &&
                   decMode.Value.Header.BlockDescriptors.Length > 0)
                    capabilities.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            AaruConsole.WriteLine("Trying SCSI READ (6)...");

            capabilities.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 0, capabilities.BlockSize ?? 512,
                                                     _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead6);
            capabilities.Read6Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (10)...");

            capabilities.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead10);
            capabilities.Read10Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (12)...");

            capabilities.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead12);
            capabilities.Read12Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (16)...");

            capabilities.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead16);
            capabilities.Read16Data = buffer;

            capabilities.LongBlockSize = capabilities.BlockSize;
            AaruConsole.WriteLine("Trying SCSI READ LONG (10)...");
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                FixedSense? decSense = Sense.DecodeFixed(senseBuffer);

                if(decSense.HasValue)
                    if(decSense.Value.SenseKey == SenseKeys.IllegalRequest &&
                       decSense.Value.ASC      == 0x24                     &&
                       decSense.Value.ASCQ     == 0x00)
                    {
                        capabilities.SupportsReadLong = true;

                        if(decSense.Value.InformationValid &&
                           decSense.Value.ILI)
                            capabilities.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                    }
            }

            if(capabilities.SupportsReadLong != true ||
               capabilities.LongBlockSize    != capabilities.BlockSize)
                return capabilities;

            if(capabilities.BlockSize == 512)
                foreach(int i in new[]
                {
                    // Long sector sizes for floppies
                    514,

                    // Long sector sizes for SuperDisk
                    536, 558,

                    // Long sector sizes for 512-byte magneto-opticals
                    600, 610, 630
                })
                {
                    ushort testSize = (ushort)i;

                    sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i, _dev.Timeout,
                                            out _);

                    if(sense || _dev.Error)
                        continue;

                    capabilities.SupportsReadLong = true;
                    capabilities.LongBlockSize    = testSize;

                    break;
                }
            else if(capabilities.BlockSize == 1024)
                foreach(int i in new[]
                {
                    // Long sector sizes for floppies
                    1026,

                    // Long sector sizes for 1024-byte magneto-opticals
                    1200
                })
                {
                    ushort testSize = (ushort)i;

                    sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i, _dev.Timeout,
                                            out _);

                    if(sense || _dev.Error)
                        continue;

                    capabilities.SupportsReadLong = true;
                    capabilities.LongBlockSize    = testSize;

                    break;
                }
            else if(capabilities.BlockSize == 2048)
            {
                sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, _dev.Timeout, out _);

                if(sense || _dev.Error)
                    return capabilities;

                capabilities.SupportsReadLong = true;
                capabilities.LongBlockSize    = 2380;
            }
            else if(capabilities.BlockSize == 4096)
            {
                sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, _dev.Timeout, out _);

                if(sense || _dev.Error)
                    return capabilities;

                capabilities.SupportsReadLong = true;
                capabilities.LongBlockSize    = 4760;
            }
            else if(capabilities.BlockSize == 8192)
            {
                sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, _dev.Timeout, out _);

                if(sense || _dev.Error)
                    return capabilities;

                capabilities.SupportsReadLong = true;
                capabilities.LongBlockSize    = 9424;
            }

            return capabilities;
        }
    }
}