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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Metadata;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;
using Inquiry = Aaru.CommonTypes.Structs.Devices.SCSI.Inquiry;

namespace Aaru.Core.Devices.Report
{
    public sealed partial class DeviceReport
    {
        /// <summary>Creates a report for the SCSI INQUIRY response</summary>
        /// <returns>SCSI report</returns>
        public Scsi ReportScsiInquiry()
        {
            AaruConsole.WriteLine("Querying SCSI INQUIRY...");
            bool sense = _dev.ScsiInquiry(out byte[] buffer, out _);

            var report = new Scsi();

            if(sense)
                return null;

            Inquiry? decodedNullable = Inquiry.Decode(buffer);

            if(!decodedNullable.HasValue)
                return null;

            report.InquiryData = ClearInquiry(buffer);

            return report;
        }

        internal static byte[] ClearInquiry(byte[] inquiry)
        {
            Inquiry? decodedNullable = Inquiry.Decode(inquiry);

            if(!decodedNullable.HasValue)
                return inquiry;

            Inquiry decoded = decodedNullable.Value;

            if(!decoded.SeagatePresent ||
               StringHandlers.CToString(decoded.VendorIdentification)?.Trim().ToLowerInvariant() != "seagate")
                return inquiry;

            // Clear Seagate serial number
            for(int i = 36; i <= 43; i++)
                inquiry[i] = 0;

            return inquiry;
        }

        /// <summary>Returns a list of decoded SCSI EVPD pages</summary>
        /// <param name="vendor">Decoded SCSI vendor identification</param>
        /// <returns>List of decoded SCSI EVPD pages</returns>
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

        /// <summary>Adds reports for the decoded SCSI MODE SENSE pages to a device report</summary>
        /// <param name="report">Device report</param>
        /// <param name="cdromMode">Returns raw MODE SENSE page 2Ah, aka CD-ROM page</param>
        /// <param name="mediumType">Returns decoded list of supported media types response</param>
        public void ReportScsiModes(ref DeviceReportV2 report, out byte[] cdromMode, out MediumTypes mediumType)
        {
            Modes.DecodedMode?    decMode = null;
            PeripheralDeviceTypes devType = _dev.ScsiType;
            byte[]                mode10Buffer;
            byte[]                mode6Buffer;
            bool                  sense;
            mediumType = 0;

            AaruConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (10)...");

            foreach(ScsiModeSensePageControl pageControl in new[]
            {
                ScsiModeSensePageControl.Default, ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Changeable
            })
            {
                bool saveBuffer = false;

                sense = _dev.ModeSense10(out mode10Buffer, out _, false, true, pageControl, 0x3F, 0xFF, _dev.Timeout,
                                         out _);

                if(sense || _dev.Error)
                {
                    sense = _dev.ModeSense10(out mode10Buffer, out _, false, false, pageControl, 0x3F, 0xFF,
                                             _dev.Timeout, out _);

                    if(sense || _dev.Error)
                    {
                        sense = _dev.ModeSense10(out mode10Buffer, out _, false, true, pageControl, 0x3F, 0x00,
                                                 _dev.Timeout, out _);

                        if(sense || _dev.Error)
                        {
                            sense = _dev.ModeSense10(out mode10Buffer, out _, false, false, pageControl, 0x3F, 0x00,
                                                     _dev.Timeout, out _);

                            if(!sense &&
                               !_dev.Error)
                            {
                                report.SCSI.SupportsModeSense10 =   true;
                                decMode                         ??= Modes.DecodeMode10(mode10Buffer, devType);
                                saveBuffer                      =   true;
                            }
                        }
                        else
                        {
                            report.SCSI.SupportsModeSense10 =   true;
                            decMode                         ??= Modes.DecodeMode10(mode10Buffer, devType);
                            saveBuffer                      =   true;
                        }
                    }
                    else
                    {
                        report.SCSI.SupportsModeSense10  =   true;
                        report.SCSI.SupportsModeSubpages =   true;
                        decMode                          ??= Modes.DecodeMode10(mode10Buffer, devType);
                        saveBuffer                       =   true;
                    }
                }
                else
                {
                    report.SCSI.SupportsModeSense10  =   true;
                    report.SCSI.SupportsModeSubpages =   true;
                    decMode                          ??= Modes.DecodeMode10(mode10Buffer, devType);
                    saveBuffer                       =   true;
                }

                if(!saveBuffer)
                    continue;

                switch(pageControl)
                {
                    case ScsiModeSensePageControl.Default:
                        report.SCSI.ModeSense10Data = mode10Buffer;

                        break;
                    case ScsiModeSensePageControl.Changeable:
                        report.SCSI.ModeSense10ChangeableData = mode10Buffer;

                        break;
                    case ScsiModeSensePageControl.Current:
                        report.SCSI.ModeSense10CurrentData = mode10Buffer;

                        break;
                }
            }

            AaruConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (6)...");

            foreach(ScsiModeSensePageControl pageControl in new[]
            {
                ScsiModeSensePageControl.Default, ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Changeable
            })
            {
                bool saveBuffer = false;
                sense = _dev.ModeSense6(out mode6Buffer, out _, true, pageControl, 0x3F, 0xFF, _dev.Timeout, out _);

                if(sense || _dev.Error)
                {
                    sense = _dev.ModeSense6(out mode6Buffer, out _, false, pageControl, 0x3F, 0xFF, _dev.Timeout,
                                            out _);

                    if(sense || _dev.Error)
                    {
                        sense = _dev.ModeSense6(out mode6Buffer, out _, true, pageControl, 0x3F, 0x00, _dev.Timeout,
                                                out _);

                        if(sense || _dev.Error)
                        {
                            sense = _dev.ModeSense6(out mode6Buffer, out _, false, pageControl, 0x3F, 0x00,
                                                    _dev.Timeout, out _);

                            if(sense || _dev.Error)
                            {
                                sense = _dev.ModeSense6(out mode6Buffer, out _, true, pageControl, 0x00, 0x00,
                                                        _dev.Timeout, out _);

                                if(sense || _dev.Error)
                                {
                                    sense = _dev.ModeSense6(out mode6Buffer, out _, false, pageControl, 0x00, 0x00,
                                                            _dev.Timeout, out _);

                                    if(!sense &&
                                       !_dev.Error)
                                    {
                                        report.SCSI.SupportsModeSense6 =   true;
                                        decMode                        ??= Modes.DecodeMode6(mode6Buffer, devType);
                                        saveBuffer                     =   true;
                                    }
                                }
                                else
                                {
                                    report.SCSI.SupportsModeSense6 =   true;
                                    decMode                        ??= Modes.DecodeMode6(mode6Buffer, devType);
                                    saveBuffer                     =   true;
                                }
                            }
                            else
                            {
                                report.SCSI.SupportsModeSense6 =   true;
                                decMode                        ??= Modes.DecodeMode6(mode6Buffer, devType);
                                saveBuffer                     =   true;
                            }
                        }
                        else
                        {
                            report.SCSI.SupportsModeSense6 =   true;
                            decMode                        ??= Modes.DecodeMode6(mode6Buffer, devType);
                            saveBuffer                     =   true;
                        }
                    }
                    else
                    {
                        report.SCSI.SupportsModeSense10  =   true;
                        report.SCSI.SupportsModeSubpages =   true;
                        decMode                          ??= Modes.DecodeMode6(mode6Buffer, devType);
                        saveBuffer                       =   true;
                    }
                }
                else
                {
                    report.SCSI.SupportsModeSense6   =   true;
                    report.SCSI.SupportsModeSubpages =   true;
                    decMode                          ??= Modes.DecodeMode6(mode6Buffer, devType);
                    saveBuffer                       =   true;
                }

                if(!saveBuffer)
                    continue;

                switch(pageControl)
                {
                    case ScsiModeSensePageControl.Default:
                        report.SCSI.ModeSense6Data = mode6Buffer;

                        break;
                    case ScsiModeSensePageControl.Changeable:
                        report.SCSI.ModeSense6ChangeableData = mode6Buffer;

                        break;
                    case ScsiModeSensePageControl.Current:
                        report.SCSI.ModeSense6CurrentData = mode6Buffer;

                        break;
                }
            }

            cdromMode = null;

            if(!decMode.HasValue)
                return;

            mediumType = decMode.Value.Header.MediumType;

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

        /// <summary>Creates a report for media inserted into a SCSI device</summary>
        /// <returns>Media report</returns>
        public TestedMedia ReportScsiMedia()
        {
            var mediaTest = new TestedMedia();
            AaruConsole.WriteLine("Querying SCSI READ CAPACITY...");
            bool sense = _dev.ReadCapacity(out byte[] buffer, out byte[] senseBuffer, _dev.Timeout, out _);

            if(!sense &&
               !_dev.Error)
            {
                mediaTest.SupportsReadCapacity = true;

                mediaTest.Blocks = ((ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) &
                                    0xFFFFFFFF) + 1;

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
                decMode ??= Modes.DecodeMode6(buffer, _dev.ScsiType);

                mediaTest.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;

                if(decMode.Value.Header.BlockDescriptors?.Length > 0)
                    mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            AaruConsole.WriteLine("Trying SCSI READ (6)...");

            mediaTest.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 0,
                                                  mediaTest.BlockSize ?? 512, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead6);
            mediaTest.Read6Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (10)...");

            mediaTest.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, false, false, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead10);
            mediaTest.Read10Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (12)...");

            mediaTest.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, false, false, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead12);
            mediaTest.Read12Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (16)...");

            mediaTest.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, false, false, 0,
                                                    mediaTest.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !mediaTest.SupportsRead16);
            mediaTest.Read16Data = buffer;

            mediaTest.LongBlockSize = mediaTest.BlockSize;
            AaruConsole.WriteLine("Trying SCSI READ LONG (10)...");
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                DecodedSense? decSense = Sense.Decode(senseBuffer);

                if(decSense?.SenseKey  == SenseKeys.IllegalRequest &&
                   decSense.Value.ASC  == 0x24                     &&
                   decSense.Value.ASCQ == 0x00)
                {
                    mediaTest.SupportsReadLong = true;

                    bool valid       = decSense.Value.Fixed?.InformationValid == true;
                    bool ili         = decSense.Value.Fixed?.ILI              == true;
                    uint information = decSense.Value.Fixed?.Information ?? 0;

                    if(decSense.Value.Descriptor.HasValue &&
                       decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                    {
                        valid       = true;
                        ili         = true;
                        information = (uint)Sense.DecodeDescriptor00(desc00);
                    }

                    if(valid && ili)
                        mediaTest.LongBlockSize = 0xFFFF - (information & 0xFFFF);
                }
            }

            AaruConsole.WriteLine("Trying SCSI READ LONG (16)...");
            sense = _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                DecodedSense? decSense = Sense.Decode(senseBuffer);

                if(decSense?.SenseKey  == SenseKeys.IllegalRequest &&
                   decSense.Value.ASC  == 0x24                     &&
                   decSense.Value.ASCQ == 0x00)
                {
                    mediaTest.SupportsReadLong16 = true;

                    bool valid       = decSense.Value.Fixed?.InformationValid == true;
                    bool ili         = decSense.Value.Fixed?.ILI              == true;
                    uint information = decSense.Value.Fixed?.Information ?? 0;

                    if(decSense.Value.Descriptor.HasValue &&
                       decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                    {
                        valid       = true;
                        ili         = true;
                        information = (uint)Sense.DecodeDescriptor00(desc00);
                    }

                    if(valid && ili)
                        mediaTest.LongBlockSize = 0xFFFF - (information & 0xFFFF);
                }
            }

            if((mediaTest.SupportsReadLong == true || mediaTest.SupportsReadLong16 == true) &&
               mediaTest.LongBlockSize == mediaTest.BlockSize)
                switch(mediaTest.BlockSize)
                {
                    case 512:
                    {
                        foreach(ushort testSize in new ushort[]
                        {
                            // Long sector sizes for floppies
                            514,

                            // Long sector sizes for SuperDisk
                            536, 558,

                            // Long sector sizes for 512-byte magneto-opticals
                            600, 610, 630
                        })
                        {
                            sense = mediaTest.SupportsReadLong16 == true
                                        ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, testSize, _dev.Timeout,
                                                          out _) : _dev.ReadLong10(out buffer, out senseBuffer, false,
                                            false, 0, testSize, _dev.Timeout, out _);

                            if(sense || _dev.Error)
                                continue;

                            mediaTest.LongBlockSize = testSize;

                            break;
                        }

                        break;
                    }
                    case 1024:
                    {
                        foreach(ushort testSize in new ushort[]
                        {
                            // Long sector sizes for floppies
                            1026,

                            // Long sector sizes for 1024-byte magneto-opticals
                            1200
                        })
                        {
                            sense = mediaTest.SupportsReadLong16 == true
                                        ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, testSize, _dev.Timeout,
                                                          out _) : _dev.ReadLong10(out buffer, out senseBuffer, false,
                                            false, 0, testSize, _dev.Timeout, out _);

                            if(sense || _dev.Error)
                                continue;

                            mediaTest.LongBlockSize = testSize;

                            break;
                        }

                        break;
                    }
                    case 2048:
                    {
                        sense = mediaTest.SupportsReadLong16 == true
                                    ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 2380, _dev.Timeout, out _)
                                    : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, _dev.Timeout,
                                                      out _);

                        if(!sense &&
                           !_dev.Error)
                            mediaTest.LongBlockSize = 2380;

                        break;
                    }
                    case 4096:
                    {
                        sense = mediaTest.SupportsReadLong16 == true
                                    ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 4760, _dev.Timeout, out _)
                                    : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, _dev.Timeout,
                                                      out _);

                        if(!sense &&
                           !_dev.Error)
                            mediaTest.LongBlockSize = 4760;

                        break;
                    }
                    case 8192:
                    {
                        sense = mediaTest.SupportsReadLong16 == true
                                    ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 9424, _dev.Timeout, out _)
                                    : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, _dev.Timeout,
                                                      out _);

                        if(!sense &&
                           !_dev.Error)
                            mediaTest.LongBlockSize = 9424;

                        break;
                    }
                }

            AaruConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");

            mediaTest.CanReadMediaSerial =
                !_dev.ReadMediaSerialNumber(out buffer, out senseBuffer, _dev.Timeout, out _);

            return mediaTest;
        }

        /// <summary>Creates a media report for a non-removable SCSI device</summary>
        /// <returns>Media report</returns>
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

                capabilities.Blocks = ((ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]) &
                                       0xFFFFFFFF) + 1;

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
                decMode ??= Modes.DecodeMode6(buffer, _dev.ScsiType);

                capabilities.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                capabilities.MediumType = (byte)decMode.Value.Header.MediumType;

                if(decMode.Value.Header.BlockDescriptors?.Length > 0)
                    capabilities.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            AaruConsole.WriteLine("Trying SCSI READ (6)...");

            capabilities.SupportsRead6 = !_dev.Read6(out buffer, out senseBuffer, 0, capabilities.BlockSize ?? 512,
                                                     _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead6);
            capabilities.Read6Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (10)...");

            capabilities.SupportsRead10 = !_dev.Read10(out buffer, out senseBuffer, 0, false, false, false, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead10);
            capabilities.Read10Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (12)...");

            capabilities.SupportsRead12 = !_dev.Read12(out buffer, out senseBuffer, 0, false, false, false, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead12);
            capabilities.Read12Data = buffer;

            AaruConsole.WriteLine("Trying SCSI READ (16)...");

            capabilities.SupportsRead16 = !_dev.Read16(out buffer, out senseBuffer, 0, false, false, false, 0,
                                                       capabilities.BlockSize ?? 512, 0, 1, false, _dev.Timeout, out _);

            AaruConsole.DebugWriteLine("SCSI Report", "Sense = {0}", !capabilities.SupportsRead16);
            capabilities.Read16Data = buffer;

            capabilities.LongBlockSize = capabilities.BlockSize;
            AaruConsole.WriteLine("Trying SCSI READ LONG (10)...");
            sense = _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                DecodedSense? decSense = Sense.Decode(senseBuffer);

                if(decSense?.SenseKey  == SenseKeys.IllegalRequest &&
                   decSense.Value.ASC  == 0x24                     &&
                   decSense.Value.ASCQ == 0x00)
                {
                    capabilities.SupportsReadLong = true;

                    bool valid       = decSense.Value.Fixed?.InformationValid == true;
                    bool ili         = decSense.Value.Fixed?.ILI              == true;
                    uint information = decSense.Value.Fixed?.Information ?? 0;

                    if(decSense.Value.Descriptor.HasValue &&
                       decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                    {
                        valid       = true;
                        ili         = true;
                        information = (uint)Sense.DecodeDescriptor00(desc00);
                    }

                    if(valid && ili)
                        capabilities.LongBlockSize = 0xFFFF - (information & 0xFFFF);
                }
            }

            AaruConsole.WriteLine("Trying SCSI READ LONG (16)...");
            sense = _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 0xFFFF, _dev.Timeout, out _);

            if(sense && !_dev.Error)
            {
                capabilities.SupportsReadLong16 = true;
                DecodedSense? decSense = Sense.Decode(senseBuffer);

                if(decSense?.SenseKey  == SenseKeys.IllegalRequest &&
                   decSense.Value.ASC  == 0x24                     &&
                   decSense.Value.ASCQ == 0x00)
                {
                    capabilities.SupportsReadLong16 = true;

                    bool valid       = decSense.Value.Fixed?.InformationValid == true;
                    bool ili         = decSense.Value.Fixed?.ILI              == true;
                    uint information = decSense.Value.Fixed?.Information ?? 0;

                    if(decSense.Value.Descriptor.HasValue &&
                       decSense.Value.Descriptor.Value.Descriptors.TryGetValue(0, out byte[] desc00))
                    {
                        valid       = true;
                        ili         = true;
                        information = (uint)Sense.DecodeDescriptor00(desc00);
                    }

                    if(valid && ili)
                        capabilities.LongBlockSize = 0xFFFF - (information & 0xFFFF);
                }
            }

            if((capabilities.SupportsReadLong != true && capabilities.SupportsReadLong16 != true) ||
               capabilities.LongBlockSize != capabilities.BlockSize)
                return capabilities;

            switch(capabilities.BlockSize)
            {
                case 512:
                {
                    foreach(ushort testSize in new ushort[]
                    {
                        // Long sector sizes for floppies
                        514,

                        // Long sector sizes for SuperDisk
                        536, 558,

                        // Long sector sizes for 512-byte magneto-opticals
                        600, 610, 630
                    })
                    {
                        sense = capabilities.SupportsReadLong16 == true
                                    ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, testSize, _dev.Timeout,
                                                      out _) : _dev.ReadLong10(out buffer, out senseBuffer, false,
                                                                               false, 0, testSize, _dev.Timeout, out _);

                        if(sense || _dev.Error)
                            continue;

                        capabilities.SupportsReadLong = true;
                        capabilities.LongBlockSize    = testSize;

                        break;
                    }

                    break;
                }
                case 1024:
                {
                    foreach(ushort testSize in new ushort[]
                    {
                        // Long sector sizes for floppies
                        1026,

                        // Long sector sizes for 1024-byte magneto-opticals
                        1200
                    })
                    {
                        sense = capabilities.SupportsReadLong16 == true
                                    ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, testSize, _dev.Timeout,
                                                      out _) : _dev.ReadLong10(out buffer, out senseBuffer, false,
                                                                               false, 0, testSize, _dev.Timeout, out _);

                        if(sense || _dev.Error)
                            continue;

                        capabilities.SupportsReadLong = true;
                        capabilities.LongBlockSize    = testSize;

                        break;
                    }

                    break;
                }
                case 2048:
                {
                    sense = capabilities.SupportsReadLong16 == true
                                ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 2380, _dev.Timeout, out _)
                                : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, _dev.Timeout,
                                                  out _);

                    if(sense || _dev.Error)
                        return capabilities;

                    capabilities.SupportsReadLong = true;
                    capabilities.LongBlockSize    = 2380;

                    break;
                }
                case 4096:
                {
                    sense = capabilities.SupportsReadLong16 == true
                                ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 4760, _dev.Timeout, out _)
                                : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, _dev.Timeout,
                                                  out _);

                    if(sense || _dev.Error)
                        return capabilities;

                    capabilities.SupportsReadLong = true;
                    capabilities.LongBlockSize    = 4760;

                    break;
                }
                case 8192:
                {
                    sense = capabilities.SupportsReadLong16 == true
                                ? _dev.ReadLong16(out buffer, out senseBuffer, false, 0, 9424, _dev.Timeout, out _)
                                : _dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, _dev.Timeout,
                                                  out _);

                    if(sense || _dev.Error)
                        return capabilities;

                    capabilities.SupportsReadLong = true;
                    capabilities.LongBlockSize    = 9424;

                    break;
                }
            }

            return capabilities;
        }
    }
}