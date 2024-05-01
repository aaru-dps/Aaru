// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.CommonTypes.Metadata;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Spectre.Console;

namespace Aaru.Core.Devices.Report;

public sealed partial class DeviceReport
{
    /// <summary>Creates a report from a SCSI Sequential Commands device</summary>
    /// <returns>SSC report</returns>
    public Ssc ReportScsiSsc()
    {
        var    report = new Ssc();
        var    sense  = true;
        byte[] buffer = Array.Empty<byte>();

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_READ_BLOCK_LIMITS).IsIndeterminate();
            sense = _dev.ReadBlockLimits(out buffer, out _, _dev.Timeout, out _);
        });

        if(!sense)
        {
            BlockLimits.BlockLimitsData? decBl = BlockLimits.Decode(buffer);

            if(decBl?.granularity > 0) report.BlockSizeGranularity = decBl.Value.granularity;

            if(decBl?.maxBlockLen > 0) report.MaxBlockLength = decBl.Value.maxBlockLen;

            if(decBl?.minBlockLen > 0) report.MinBlockLength = decBl.Value.minBlockLen;
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_REPORT_DENSITY_SUPPORT).IsIndeterminate();
            sense = _dev.ReportDensitySupport(out buffer, out _, false, false, _dev.Timeout, out _);
        });

        if(!sense)
        {
            DensitySupport.DensitySupportHeader? dsh = DensitySupport.DecodeDensity(buffer);

            if(dsh.HasValue)
            {
                var array = new SupportedDensity[dsh.Value.descriptors.Length];

                for(var i = 0; i < dsh.Value.descriptors.Length; i++)
                {
                    array[i] = new SupportedDensity
                    {
                        BitsPerMm      = dsh.Value.descriptors[i].bpmm,
                        Capacity       = dsh.Value.descriptors[i].capacity,
                        DefaultDensity = dsh.Value.descriptors[i].defaultDensity,
                        Description    = dsh.Value.descriptors[i].description,
                        Duplicate      = dsh.Value.descriptors[i].duplicate,
                        Name           = dsh.Value.descriptors[i].name,
                        Organization   = dsh.Value.descriptors[i].organization,
                        PrimaryCode    = dsh.Value.descriptors[i].primaryCode,
                        SecondaryCode  = dsh.Value.descriptors[i].secondaryCode,
                        Tracks         = dsh.Value.descriptors[i].tracks,
                        Width          = dsh.Value.descriptors[i].width,
                        Writable       = dsh.Value.descriptors[i].writable
                    };
                }

                report.SupportedDensities = array.ToList();
            }
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_REPORT_DENSITY_SUPPORT_for_medium_types).IsIndeterminate();
            sense = _dev.ReportDensitySupport(out buffer, out _, true, false, _dev.Timeout, out _);
        });

        if(sense) return report;

        DensitySupport.MediaTypeSupportHeader? mtsh = DensitySupport.DecodeMediumType(buffer);

        if(!mtsh.HasValue) return report;

        var array2 = new SscSupportedMedia[mtsh.Value.descriptors.Length];

        for(var i = 0; i < mtsh.Value.descriptors.Length; i++)
        {
            array2[i] = new SscSupportedMedia
            {
                Description  = mtsh.Value.descriptors[i].description,
                Length       = mtsh.Value.descriptors[i].length,
                MediumType   = mtsh.Value.descriptors[i].mediumType,
                Name         = mtsh.Value.descriptors[i].name,
                Organization = mtsh.Value.descriptors[i].organization,
                Width        = mtsh.Value.descriptors[i].width
            };

            if(mtsh.Value.descriptors[i].densityCodes == null) continue;

            var array3 = new DensityCode[mtsh.Value.descriptors[i].densityCodes.Length];

            for(var j = 0; j < mtsh.Value.descriptors[i].densityCodes.Length; j++)
            {
                array3[j] = new DensityCode
                {
                    Code = mtsh.Value.descriptors[i].densityCodes[j]
                };
            }

            array2[i].DensityCodes = array3.Distinct().ToList();
        }

        report.SupportedMediaTypes = array2.ToList();

        return report;
    }

    /// <summary>Creates a report for media inserted into an SSC device</summary>
    /// <returns>Media report</returns>
    public TestedSequentialMedia ReportSscMedia()
    {
        var    seqTest = new TestedSequentialMedia();
        var    sense   = true;
        byte[] buffer  = Array.Empty<byte>();

        Modes.DecodedMode? decMode = null;

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_MODE_SENSE_10).IsIndeterminate();

            sense = _dev.ModeSense10(out buffer,
                                     out _,
                                     false,
                                     true,
                                     ScsiModeSensePageControl.Current,
                                     0x3F,
                                     0x00,
                                     _dev.Timeout,
                                     out _);
        });

        if(!sense && !_dev.Error)
        {
            decMode                 = Modes.DecodeMode10(buffer, _dev.ScsiType);
            seqTest.ModeSense10Data = buffer;
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_MODE_SENSE).IsIndeterminate();
            sense = _dev.ModeSense(out buffer, out _, _dev.Timeout, out _);
        });

        if(!sense && !_dev.Error)
        {
            decMode ??= Modes.DecodeMode6(buffer, _dev.ScsiType);

            seqTest.ModeSense6Data = buffer;
        }

        if(decMode.HasValue)
        {
            seqTest.MediumType = (byte)decMode.Value.Header.MediumType;

            if(decMode.Value.Header.BlockDescriptors?.Length > 0)
                seqTest.Density = (byte)(decMode.Value.Header.BlockDescriptors?[0].Density ?? default(DensityType));
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_REPORT_DENSITY_SUPPORT_for_current_media).IsIndeterminate();
            sense = _dev.ReportDensitySupport(out buffer, out _, false, true, _dev.Timeout, out _);
        });

        if(!sense)
        {
            DensitySupport.DensitySupportHeader? dsh = DensitySupport.DecodeDensity(buffer);

            if(dsh.HasValue)
            {
                var array = new SupportedDensity[dsh.Value.descriptors.Length];

                for(var i = 0; i < dsh.Value.descriptors.Length; i++)
                {
                    array[i] = new SupportedDensity
                    {
                        BitsPerMm      = dsh.Value.descriptors[i].bpmm,
                        Capacity       = dsh.Value.descriptors[i].capacity,
                        DefaultDensity = dsh.Value.descriptors[i].defaultDensity,
                        Description    = dsh.Value.descriptors[i].description,
                        Duplicate      = dsh.Value.descriptors[i].duplicate,
                        Name           = dsh.Value.descriptors[i].name,
                        Organization   = dsh.Value.descriptors[i].organization,
                        PrimaryCode    = dsh.Value.descriptors[i].primaryCode,
                        SecondaryCode  = dsh.Value.descriptors[i].secondaryCode,
                        Tracks         = dsh.Value.descriptors[i].tracks,
                        Width          = dsh.Value.descriptors[i].width,
                        Writable       = dsh.Value.descriptors[i].writable
                    };
                }

                seqTest.SupportedDensities = array.ToList();
            }
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Querying_SCSI_REPORT_DENSITY_SUPPORT_for_medium_types_for_current_media)
               .IsIndeterminate();

            sense = _dev.ReportDensitySupport(out buffer, out _, true, true, _dev.Timeout, out _);
        });

        if(!sense)
        {
            DensitySupport.MediaTypeSupportHeader? mtsh = DensitySupport.DecodeMediumType(buffer);

            if(mtsh.HasValue)
            {
                var array = new SscSupportedMedia[mtsh.Value.descriptors.Length];

                for(var i = 0; i < mtsh.Value.descriptors.Length; i++)
                {
                    array[i] = new SscSupportedMedia
                    {
                        Description  = mtsh.Value.descriptors[i].description,
                        Length       = mtsh.Value.descriptors[i].length,
                        MediumType   = mtsh.Value.descriptors[i].mediumType,
                        Name         = mtsh.Value.descriptors[i].name,
                        Organization = mtsh.Value.descriptors[i].organization,
                        Width        = mtsh.Value.descriptors[i].width
                    };

                    if(mtsh.Value.descriptors[i].densityCodes == null) continue;

                    var array2 = new DensityCode[mtsh.Value.descriptors[i].densityCodes.Length];

                    for(var j = 0; j < mtsh.Value.descriptors[i].densityCodes.Length; j++)
                    {
                        array2[j] = new DensityCode
                        {
                            Code = mtsh.Value.descriptors[i].densityCodes[j]
                        };
                    }

                    array[i].DensityCodes = array2.Distinct().ToList();
                }

                seqTest.SupportedMediaTypes = array.ToList();
            }
        }

        Spectre.ProgressSingleSpinner(ctx =>
        {
            ctx.AddTask(Localization.Core.Trying_SCSI_READ_MEDIA_SERIAL_NUMBER).IsIndeterminate();
            seqTest.CanReadMediaSerial = !_dev.ReadMediaSerialNumber(out buffer, out _, _dev.Timeout, out _);
        });

        return seqTest;
    }
}