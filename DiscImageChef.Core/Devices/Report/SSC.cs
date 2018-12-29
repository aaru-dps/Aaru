// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from SCSI Streaming devices.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Linq;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;

namespace DiscImageChef.Core.Devices.Report
{
    public partial class DeviceReport
    {
        public Ssc ReportScsiSsc()
        {
            Ssc report = new Ssc();
            DicConsole.WriteLine("Querying SCSI READ BLOCK LIMITS...");
            bool sense = dev.ReadBlockLimits(out byte[] buffer, out byte[] _, dev.Timeout, out _);
            if(!sense)
            {
                BlockLimits.BlockLimitsData? decBl = BlockLimits.Decode(buffer);
                if(decBl.HasValue)
                {
                    if(decBl.Value.granularity > 0) report.BlockSizeGranularity = decBl.Value.granularity;

                    if(decBl.Value.maxBlockLen > 0) report.MaxBlockLength = decBl.Value.maxBlockLen;

                    if(decBl.Value.minBlockLen > 0) report.MinBlockLength = decBl.Value.minBlockLen;
                }
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT...");
            sense = dev.ReportDensitySupport(out buffer, out byte[] _, false, false, dev.Timeout, out _);
            if(!sense)
            {
                DensitySupport.DensitySupportHeader? dsh = DensitySupport.DecodeDensity(buffer);
                if(dsh.HasValue)
                {
                    SupportedDensity[] array = new SupportedDensity[dsh.Value.descriptors.Length];
                    for(int i = 0; i < dsh.Value.descriptors.Length; i++)
                    {
                        report.SupportedDensities[i].BitsPerMm      = dsh.Value.descriptors[i].bpmm;
                        report.SupportedDensities[i].Capacity       = dsh.Value.descriptors[i].capacity;
                        report.SupportedDensities[i].DefaultDensity = dsh.Value.descriptors[i].defaultDensity;
                        report.SupportedDensities[i].Description    = dsh.Value.descriptors[i].description;
                        report.SupportedDensities[i].Duplicate      = dsh.Value.descriptors[i].duplicate;
                        report.SupportedDensities[i].Name           = dsh.Value.descriptors[i].name;
                        report.SupportedDensities[i].Organization   = dsh.Value.descriptors[i].organization;
                        report.SupportedDensities[i].PrimaryCode    = dsh.Value.descriptors[i].primaryCode;
                        report.SupportedDensities[i].SecondaryCode  = dsh.Value.descriptors[i].secondaryCode;
                        report.SupportedDensities[i].Tracks         = dsh.Value.descriptors[i].tracks;
                        report.SupportedDensities[i].Width          = dsh.Value.descriptors[i].width;
                        report.SupportedDensities[i].Writable       = dsh.Value.descriptors[i].writable;
                    }

                    report.SupportedDensities = array.ToList();
                }
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for medium types...");
            sense = dev.ReportDensitySupport(out buffer, out byte[] _, true, false, dev.Timeout, out _);
            if(sense) return report;

            DensitySupport.MediaTypeSupportHeader? mtsh = DensitySupport.DecodeMediumType(buffer);
            if(!mtsh.HasValue) return report;

            SscSupportedMedia[] array2 = new SscSupportedMedia[mtsh.Value.descriptors.Length];
            for(int i = 0; i < mtsh.Value.descriptors.Length; i++)
            {
                report.SupportedMediaTypes[i].Description  = mtsh.Value.descriptors[i].description;
                report.SupportedMediaTypes[i].Length       = mtsh.Value.descriptors[i].length;
                report.SupportedMediaTypes[i].MediumType   = mtsh.Value.descriptors[i].mediumType;
                report.SupportedMediaTypes[i].Name         = mtsh.Value.descriptors[i].name;
                report.SupportedMediaTypes[i].Organization = mtsh.Value.descriptors[i].organization;
                report.SupportedMediaTypes[i].Width        = mtsh.Value.descriptors[i].width;
                if(mtsh.Value.descriptors[i].densityCodes == null) continue;

                DensityCode[] array3 = new DensityCode[mtsh.Value.descriptors[i].densityCodes.Length];
                for(int j = 0; j < mtsh.Value.descriptors.Length; j++)
                    report.SupportedMediaTypes[i].DensityCodes[j] =
                        new DensityCode {Code = mtsh.Value.descriptors[i].densityCodes[j]};
                report.SupportedMediaTypes[i].DensityCodes = array3.ToList();
            }

            report.SupportedMediaTypes = array2.ToList();

            return report;
        }

        public TestedSequentialMedia ReportSscMedia()
        {
            TestedSequentialMedia seqTest = new TestedSequentialMedia();

            Modes.DecodedMode? decMode = null;

            DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
            bool sense = dev.ModeSense10(out byte[] buffer, out byte[] _, false, true, ScsiModeSensePageControl.Current,
                                         0x3F, 0x00, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                decMode = Modes.DecodeMode10(buffer, dev.ScsiType);
                if(debug) seqTest.ModeSense10Data = buffer;
            }

            DicConsole.WriteLine("Querying SCSI MODE SENSE...");
            sense = dev.ModeSense(out buffer, out byte[] _, dev.Timeout, out _);
            if(!sense && !dev.Error)
            {
                if(!decMode.HasValue) decMode    = Modes.DecodeMode6(buffer, dev.ScsiType);
                if(debug) seqTest.ModeSense6Data = buffer;
            }

            if(decMode.HasValue)
            {
                seqTest.MediumType = (byte)decMode.Value.Header.MediumType;
                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                    seqTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for current media...");
            sense = dev.ReportDensitySupport(out buffer, out byte[] _, false, true, dev.Timeout, out _);
            if(!sense)
            {
                DensitySupport.DensitySupportHeader? dsh = DensitySupport.DecodeDensity(buffer);
                if(dsh.HasValue)
                {
                    SupportedDensity[] array = new SupportedDensity[dsh.Value.descriptors.Length];
                    for(int i = 0; i < dsh.Value.descriptors.Length; i++)
                    {
                        seqTest.SupportedDensities[i].BitsPerMm      = dsh.Value.descriptors[i].bpmm;
                        seqTest.SupportedDensities[i].Capacity       = dsh.Value.descriptors[i].capacity;
                        seqTest.SupportedDensities[i].DefaultDensity = dsh.Value.descriptors[i].defaultDensity;
                        seqTest.SupportedDensities[i].Description    = dsh.Value.descriptors[i].description;
                        seqTest.SupportedDensities[i].Duplicate      = dsh.Value.descriptors[i].duplicate;
                        seqTest.SupportedDensities[i].Name           = dsh.Value.descriptors[i].name;
                        seqTest.SupportedDensities[i].Organization   = dsh.Value.descriptors[i].organization;
                        seqTest.SupportedDensities[i].PrimaryCode    = dsh.Value.descriptors[i].primaryCode;
                        seqTest.SupportedDensities[i].SecondaryCode  = dsh.Value.descriptors[i].secondaryCode;
                        seqTest.SupportedDensities[i].Tracks         = dsh.Value.descriptors[i].tracks;
                        seqTest.SupportedDensities[i].Width          = dsh.Value.descriptors[i].width;
                        seqTest.SupportedDensities[i].Writable       = dsh.Value.descriptors[i].writable;
                    }

                    seqTest.SupportedDensities = array.ToList();
                }
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for medium types for current media...");
            sense = dev.ReportDensitySupport(out buffer, out byte[] _, true, true, dev.Timeout, out _);
            if(!sense)
            {
                DensitySupport.MediaTypeSupportHeader? mtsh = DensitySupport.DecodeMediumType(buffer);
                if(mtsh.HasValue)
                {
                    SscSupportedMedia[] array = new SscSupportedMedia[mtsh.Value.descriptors.Length];
                    for(int i = 0; i < mtsh.Value.descriptors.Length; i++)
                    {
                        seqTest.SupportedMediaTypes[i].Description  = mtsh.Value.descriptors[i].description;
                        seqTest.SupportedMediaTypes[i].Length       = mtsh.Value.descriptors[i].length;
                        seqTest.SupportedMediaTypes[i].MediumType   = mtsh.Value.descriptors[i].mediumType;
                        seqTest.SupportedMediaTypes[i].Name         = mtsh.Value.descriptors[i].name;
                        seqTest.SupportedMediaTypes[i].Organization = mtsh.Value.descriptors[i].organization;
                        seqTest.SupportedMediaTypes[i].Width        = mtsh.Value.descriptors[i].width;
                        if(mtsh.Value.descriptors[i].densityCodes == null) continue;

                        DensityCode[] array2 = new DensityCode[mtsh.Value.descriptors[i].densityCodes.Length];
                        for(int j = 0; j < mtsh.Value.descriptors.Length; j++)
                            seqTest.SupportedMediaTypes[i].DensityCodes[j] =
                                new DensityCode {Code = mtsh.Value.descriptors[i].densityCodes[j]};
                        seqTest.SupportedMediaTypes[i].DensityCodes = array2.ToList();
                    }

                    seqTest.SupportedMediaTypes = array.ToList();
                }
            }

            DicConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");
            seqTest.CanReadMediaSerial = !dev.ReadMediaSerialNumber(out buffer, out byte[] _, dev.Timeout, out _);

            return seqTest;
        }
    }
}