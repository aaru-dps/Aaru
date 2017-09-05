// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
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
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report.SCSI
{
    internal static class SSC
    {
        internal static void Report(Device dev, ref DeviceReport report, bool debug)
        {
            if(report == null)
                return;

            byte[] senseBuffer;
            byte[] buffer;
            double duration;
            bool sense;
            uint timeout = 5;
            ConsoleKeyInfo pressedKey;
            Decoders.SCSI.Modes.DecodedMode? decMode = null;

            report.SCSI.SequentialDevice = new sscType();
            DicConsole.WriteLine("Querying SCSI READ BLOCK LIMITS...");
            sense = dev.ReadBlockLimits(out buffer, out senseBuffer, timeout, out duration);
            if(!sense)
            {
                Decoders.SCSI.SSC.BlockLimits.BlockLimitsData? decBL = Decoders.SCSI.SSC.BlockLimits.Decode(buffer);
                if(decBL.HasValue)
                {
                    if(decBL.Value.granularity > 0)
                    {
                        report.SCSI.SequentialDevice.BlockSizeGranularitySpecified = true;
                        report.SCSI.SequentialDevice.BlockSizeGranularity = decBL.Value.granularity;
                    }
                    if(decBL.Value.maxBlockLen > 0)
                    {
                        report.SCSI.SequentialDevice.MaxBlockLengthSpecified = true;
                        report.SCSI.SequentialDevice.MaxBlockLength = decBL.Value.maxBlockLen;
                    }
                    if(decBL.Value.minBlockLen > 0)
                    {
                        report.SCSI.SequentialDevice.MinBlockLengthSpecified = true;
                        report.SCSI.SequentialDevice.MinBlockLength = decBL.Value.minBlockLen;
                    }
                }
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT...");
            sense = dev.ReportDensitySupport(out buffer, out senseBuffer, false, false, timeout, out duration);
            if(!sense)
            {
                Decoders.SCSI.SSC.DensitySupport.DensitySupportHeader? dsh = Decoders.SCSI.SSC.DensitySupport.DecodeDensity(buffer);
                if(dsh.HasValue)
                {
                    report.SCSI.SequentialDevice.SupportedDensities = new SupportedDensity[dsh.Value.descriptors.Length];
                    for(int i = 0; i < dsh.Value.descriptors.Length; i++)
                    {
                        report.SCSI.SequentialDevice.SupportedDensities[i].BitsPerMm = dsh.Value.descriptors[i].bpmm;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Capacity = dsh.Value.descriptors[i].capacity;
                        report.SCSI.SequentialDevice.SupportedDensities[i].DefaultDensity = dsh.Value.descriptors[i].defaultDensity;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Description = dsh.Value.descriptors[i].description;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Duplicate = dsh.Value.descriptors[i].duplicate;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Name = dsh.Value.descriptors[i].name;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Organization = dsh.Value.descriptors[i].organization;
                        report.SCSI.SequentialDevice.SupportedDensities[i].PrimaryCode = dsh.Value.descriptors[i].primaryCode;
                        report.SCSI.SequentialDevice.SupportedDensities[i].SecondaryCode = dsh.Value.descriptors[i].secondaryCode;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Tracks = dsh.Value.descriptors[i].tracks;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Width = dsh.Value.descriptors[i].width;
                        report.SCSI.SequentialDevice.SupportedDensities[i].Writable = dsh.Value.descriptors[i].writable;
                    }
                }
            }

            DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for medium types...");
            sense = dev.ReportDensitySupport(out buffer, out senseBuffer, true, false, timeout, out duration);
            if(!sense)
            {
                Decoders.SCSI.SSC.DensitySupport.MediaTypeSupportHeader? mtsh = Decoders.SCSI.SSC.DensitySupport.DecodeMediumType(buffer);
                if(mtsh.HasValue)
                {
                    report.SCSI.SequentialDevice.SupportedMediaTypes = new SupportedMedia[mtsh.Value.descriptors.Length];
                    for(int i = 0; i < mtsh.Value.descriptors.Length; i++)
                    {
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].Description = mtsh.Value.descriptors[i].description;
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].Length = mtsh.Value.descriptors[i].length;
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].MediumType = mtsh.Value.descriptors[i].mediumType;
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].Name = mtsh.Value.descriptors[i].name;
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].Organization = mtsh.Value.descriptors[i].organization;
                        report.SCSI.SequentialDevice.SupportedMediaTypes[i].Width = mtsh.Value.descriptors[i].width;
                        if(mtsh.Value.descriptors[i].densityCodes != null)
                        {
                            report.SCSI.SequentialDevice.SupportedMediaTypes[i].DensityCodes = new int[mtsh.Value.descriptors[i].densityCodes.Length];
                            for(int j = 0; j < mtsh.Value.descriptors.Length; j++)
                                report.SCSI.SequentialDevice.SupportedMediaTypes[i].DensityCodes[j] = mtsh.Value.descriptors[i].densityCodes[j];
                        }
                    }
                }
            }

            List<SequentialMedia> seqTests = new List<SequentialMedia>();

            pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.N)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Do you have media that you can insert in the drive? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                    System.Console.ReadKey(true);

                    SequentialMedia seqTest = new SequentialMedia();
                    DicConsole.Write("Please write a description of the media type and press enter: ");
                    seqTest.MediumTypeName = System.Console.ReadLine();
                    DicConsole.Write("Please write the media manufacturer and press enter: ");
                    seqTest.Manufacturer = System.Console.ReadLine();
                    DicConsole.Write("Please write the media model and press enter: ");
                    seqTest.Model = System.Console.ReadLine();

                    seqTest.MediaIsRecognized = true;

                    sense = dev.Load(out senseBuffer, timeout, out duration);
                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                    if(sense)
                    {
                        Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                        if(decSense.HasValue)
                        {
                            if(decSense.Value.ASC == 0x3A)
                            {
                                int leftRetries = 20;
                                while(leftRetries > 0)
                                {
                                    DicConsole.Write("\rWaiting for drive to become ready");
                                    System.Threading.Thread.Sleep(2000);
                                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                    if(!sense)
                                        break;

                                    leftRetries--;
                                }

                                seqTest.MediaIsRecognized &= !sense;
                            }
                            else if(decSense.Value.ASC == 0x04 && decSense.Value.ASCQ == 0x01)
                            {
                                int leftRetries = 20;
                                while(leftRetries > 0)
                                {
                                    DicConsole.Write("\rWaiting for drive to become ready");
                                    System.Threading.Thread.Sleep(2000);
                                    sense = dev.ScsiTestUnitReady(out senseBuffer, timeout, out duration);
                                    if(!sense)
                                        break;

                                    leftRetries--;
                                }

                                seqTest.MediaIsRecognized &= !sense;
                            }
                            else
                                seqTest.MediaIsRecognized = false;
                        }
                        else
                            seqTest.MediaIsRecognized = false;
                    }

                    if(seqTest.MediaIsRecognized)
                    {
                        decMode = null;

                        DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                        sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense10 = true;
                            decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.SCSIType);
                            if(debug)
                                seqTest.ModeSense10Data = buffer;
                        }

                        DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                        sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                        if(!sense && !dev.Error)
                        {
                            report.SCSI.SupportsModeSense6 = true;
                            if(!decMode.HasValue)
                                decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.SCSIType);
                            if(debug)
                                seqTest.ModeSense6Data = buffer;
                        }

                        if(decMode.HasValue)
                        {
                            seqTest.MediumType = (byte)decMode.Value.Header.MediumType;
                            seqTest.MediumTypeSpecified = true;
                            if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                            {
                                seqTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                seqTest.DensitySpecified = true;
                            }
                        }
                    }

                    DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for current media...");
                    sense = dev.ReportDensitySupport(out buffer, out senseBuffer, false, true, timeout, out duration);
                    if(!sense)
                    {
                        Decoders.SCSI.SSC.DensitySupport.DensitySupportHeader? dsh = Decoders.SCSI.SSC.DensitySupport.DecodeDensity(buffer);
                        if(dsh.HasValue)
                        {
                            seqTest.SupportedDensities = new SupportedDensity[dsh.Value.descriptors.Length];
                            for(int i = 0; i < dsh.Value.descriptors.Length; i++)
                            {
                                seqTest.SupportedDensities[i].BitsPerMm = dsh.Value.descriptors[i].bpmm;
                                seqTest.SupportedDensities[i].Capacity = dsh.Value.descriptors[i].capacity;
                                seqTest.SupportedDensities[i].DefaultDensity = dsh.Value.descriptors[i].defaultDensity;
                                seqTest.SupportedDensities[i].Description = dsh.Value.descriptors[i].description;
                                seqTest.SupportedDensities[i].Duplicate = dsh.Value.descriptors[i].duplicate;
                                seqTest.SupportedDensities[i].Name = dsh.Value.descriptors[i].name;
                                seqTest.SupportedDensities[i].Organization = dsh.Value.descriptors[i].organization;
                                seqTest.SupportedDensities[i].PrimaryCode = dsh.Value.descriptors[i].primaryCode;
                                seqTest.SupportedDensities[i].SecondaryCode = dsh.Value.descriptors[i].secondaryCode;
                                seqTest.SupportedDensities[i].Tracks = dsh.Value.descriptors[i].tracks;
                                seqTest.SupportedDensities[i].Width = dsh.Value.descriptors[i].width;
                                seqTest.SupportedDensities[i].Writable = dsh.Value.descriptors[i].writable;
                            }
                        }
                    }

                    DicConsole.WriteLine("Querying SCSI REPORT DENSITY SUPPORT for medium types for current media...");
                    sense = dev.ReportDensitySupport(out buffer, out senseBuffer, true, true, timeout, out duration);
                    if(!sense)
                    {
                        Decoders.SCSI.SSC.DensitySupport.MediaTypeSupportHeader? mtsh = Decoders.SCSI.SSC.DensitySupport.DecodeMediumType(buffer);
                        if(mtsh.HasValue)
                        {
                            seqTest.SupportedMediaTypes = new SupportedMedia[mtsh.Value.descriptors.Length];
                            for(int i = 0; i < mtsh.Value.descriptors.Length; i++)
                            {
                                seqTest.SupportedMediaTypes[i].Description = mtsh.Value.descriptors[i].description;
                                seqTest.SupportedMediaTypes[i].Length = mtsh.Value.descriptors[i].length;
                                seqTest.SupportedMediaTypes[i].MediumType = mtsh.Value.descriptors[i].mediumType;
                                seqTest.SupportedMediaTypes[i].Name = mtsh.Value.descriptors[i].name;
                                seqTest.SupportedMediaTypes[i].Organization = mtsh.Value.descriptors[i].organization;
                                seqTest.SupportedMediaTypes[i].Width = mtsh.Value.descriptors[i].width;
                                if(mtsh.Value.descriptors[i].densityCodes != null)
                                {
                                    seqTest.SupportedMediaTypes[i].DensityCodes = new int[mtsh.Value.descriptors[i].densityCodes.Length];
                                    for(int j = 0; j < mtsh.Value.descriptors.Length; j++)
                                        seqTest.SupportedMediaTypes[i].DensityCodes[j] = mtsh.Value.descriptors[i].densityCodes[j];
                                }
                            }
                        }
                    }

                    seqTest.CanReadMediaSerialSpecified = true;
                    DicConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");
                    seqTest.CanReadMediaSerial = !dev.ReadMediaSerialNumber(out buffer, out senseBuffer, timeout, out duration);
                    seqTests.Add(seqTest);
                }
            }
            report.SCSI.SequentialDevice.TestedMedia = seqTests.ToArray();
        }
    }
}
