// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
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
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;
using System.IO;
using System.Collections.Generic;

namespace DiscImageChef.Commands
{
    public static class DeviceReport
    {
        public static void doDeviceReport(DeviceReportOptions options)
        {
            DicConsole.DebugWriteLine("Device-Report command", "--debug={0}", options.Debug);
            DicConsole.DebugWriteLine("Device-Report command", "--verbose={0}", options.Verbose);
            DicConsole.DebugWriteLine("Device-Report command", "--device={0}", options.DevicePath);

            if(!System.IO.File.Exists(options.DevicePath))
            {
                DicConsole.ErrorWriteLine("Specified device does not exist.");
                return;
            }

            if(options.DevicePath.Length == 2 && options.DevicePath[1] == ':' &&
                options.DevicePath[0] != '/' && Char.IsLetter(options.DevicePath[0]))
            {
                options.DevicePath = "\\\\.\\" + Char.ToUpper(options.DevicePath[0]) + ':';
            }

            Device dev = new Device(options.DevicePath);

            if(dev.Error)
            {
                DicConsole.ErrorWriteLine("Error {0} opening device.", dev.LastError);
                return;
            }

            Core.Statistics.AddDevice(dev);

            switch(dev.Type)
            {
                case DeviceType.ATA:
                    doATADeviceReport(options, dev);
                    break;
                case DeviceType.MMC:
                case DeviceType.SecureDigital:
                    doSDDeviceReport(options, dev);
                    break;
                case DeviceType.NVMe:
                    doNVMeDeviceReport(options, dev);
                    break;
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    doSCSIDeviceReport(options, dev);
                    break;
                default:
                    throw new NotSupportedException("Unknown device type.");
            }

            Core.Statistics.AddCommand("device-report");
        }

        static void doATADeviceReport(DeviceReportOptions options, Device dev)
        {
            DiscImageChef.Decoders.ATA.AtaErrorRegistersCHS errorRegs;
            byte[] buffer;
            double duration;
            uint timeout = 5;
            Metadata.DeviceReport report = new Metadata.DeviceReport();
            string xmlFile;
            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) && !string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                xmlFile = dev.Manufacturer + "_" + dev.Model + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Model + "_" + dev.Revision + ".xml";
            else
                xmlFile = dev.Model + ".xml";

            ConsoleKeyInfo pressedKey;
            bool removable = false;

            if(dev.IsUSB)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively USB (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.USB = new usbType();
                    report.USB.Manufacturer = dev.USBManufacturerString;
                    report.USB.Product = dev.USBProductString;
                    report.USB.ProductID = dev.USBProductID;
                    report.USB.VendorID = dev.USBVendorID;

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.USB.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable = true;
                }
            }

            if(dev.IsFireWire)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively FireWire (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.FireWire = new firewireType();
                    report.FireWire.Manufacturer = dev.FireWireVendorName;
                    report.FireWire.Product = dev.FireWireModelName;
                    report.FireWire.ProductID = dev.FireWireModel;
                    report.FireWire.VendorID = dev.FireWireVendor;

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.FireWire.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable = true;
                }
            }

            DicConsole.WriteLine("Querying ATA IDENTIFY...");

            dev.AtaIdentify(out buffer, out errorRegs, timeout, out duration);

            if(Decoders.ATA.Identify.Decode(buffer).HasValue)
            {
                Decoders.ATA.Identify.IdentifyDevice ataId = Decoders.ATA.Identify.Decode(buffer).Value;

                if((ushort)ataId.GeneralConfiguration == 0x848A)
                {
                    report.CompactFlash = true;
                    report.CompactFlashSpecified = true;
                    removable = false;
                }
                else if(!removable && ataId.GeneralConfiguration.HasFlag(Decoders.ATA.Identify.GeneralConfigurationBit.Removable))
                {
                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    removable = pressedKey.Key == ConsoleKey.Y;
                }

                if(removable)
                {
                    DicConsole.WriteLine("Please remove any media from the device and press any key when it is out.");
                    System.Console.ReadKey(true);
                    DicConsole.WriteLine("Querying ATA IDENTIFY...");
                    dev.AtaIdentify(out buffer, out errorRegs, timeout, out duration);
                    ataId = Decoders.ATA.Identify.Decode(buffer).Value;
                }

                report.ATA = new ataType();

                if(!string.IsNullOrWhiteSpace(ataId.AdditionalPID))
                {
                    report.ATA.AdditionalPID = ataId.AdditionalPID;
                    report.ATA.AdditionalPIDSpecified = true;
                }
                if(ataId.APIOSupported != 0)
                {
                    report.ATA.APIOSupported = ataId.APIOSupported;
                    report.ATA.APIOSupportedSpecified = true;
                }
                if(ataId.BufferType != 0)
                {
                    report.ATA.BufferType = ataId.BufferType;
                    report.ATA.BufferTypeSpecified = true;
                }
                if(ataId.BufferSize != 0)
                {
                    report.ATA.BufferSize = ataId.BufferSize;
                    report.ATA.BufferSizeSpecified = true;
                }
                if(ataId.Capabilities != 0)
                {
                    report.ATA.Capabilities = ataId.Capabilities;
                    report.ATA.CapabilitiesSpecified = true;
                }
                if(ataId.Capabilities2 != 0)
                {
                    report.ATA.Capabilities2 = ataId.Capabilities2;
                    report.ATA.Capabilities2Specified = true;
                }
                if(ataId.Capabilities3 != 0)
                {
                    report.ATA.Capabilities3 = ataId.Capabilities3;
                    report.ATA.Capabilities3Specified = true;
                }
                if(ataId.CFAPowerMode != 0)
                {
                    report.ATA.CFAPowerMode = ataId.CFAPowerMode;
                    report.ATA.CFAPowerModeSpecified = true;
                }
                if(ataId.CommandSet != 0)
                {
                    report.ATA.CommandSet = ataId.CommandSet;
                    report.ATA.CommandSetSpecified = true;
                }
                if(ataId.CommandSet2 != 0)
                {
                    report.ATA.CommandSet2 = ataId.CommandSet2;
                    report.ATA.CommandSet2Specified = true;
                }
                if(ataId.CommandSet3 != 0)
                {
                    report.ATA.CommandSet3 = ataId.CommandSet3;
                    report.ATA.CommandSet3Specified = true;
                }
                if(ataId.CommandSet4 != 0)
                {
                    report.ATA.CommandSet4 = ataId.CommandSet4;
                    report.ATA.CommandSet4Specified = true;
                }
                if(ataId.CommandSet5 != 0)
                {
                    report.ATA.CommandSet5 = ataId.CommandSet5;
                    report.ATA.CommandSet5Specified = true;
                }
                if(ataId.CurrentAAM != 0)
                {
                    report.ATA.CurrentAAM = ataId.CurrentAAM;
                    report.ATA.CurrentAAMSpecified = true;
                }
                if(ataId.CurrentAPM != 0)
                {
                    report.ATA.CurrentAPM = ataId.CurrentAPM;
                    report.ATA.CurrentAPMSpecified = true;
                }
                if(ataId.DataSetMgmt != 0)
                {
                    report.ATA.DataSetMgmt = ataId.DataSetMgmt;
                    report.ATA.DataSetMgmtSpecified = true;
                }
                if(ataId.DataSetMgmtSize != 0)
                {
                    report.ATA.DataSetMgmtSize = ataId.DataSetMgmtSize;
                    report.ATA.DataSetMgmtSizeSpecified = true;
                }
                if(ataId.DeviceFormFactor != 0)
                {
                    report.ATA.DeviceFormFactor = ataId.DeviceFormFactor;
                    report.ATA.DeviceFormFactorSpecified = true;
                }
                if(ataId.DMAActive != 0)
                {
                    report.ATA.DMAActive = ataId.DMAActive;
                    report.ATA.DMAActiveSpecified = true;
                }
                if(ataId.DMASupported != 0)
                {
                    report.ATA.DMASupported = ataId.DMASupported;
                    report.ATA.DMASupportedSpecified = true;
                }
                if(ataId.DMATransferTimingMode != 0)
                {
                    report.ATA.DMATransferTimingMode = ataId.DMATransferTimingMode;
                    report.ATA.DMATransferTimingModeSpecified = true;
                }
                if(ataId.EnhancedSecurityEraseTime != 0)
                {
                    report.ATA.EnhancedSecurityEraseTime = ataId.EnhancedSecurityEraseTime;
                    report.ATA.EnhancedSecurityEraseTimeSpecified = true;
                }
                if(ataId.EnabledCommandSet != 0)
                {
                    report.ATA.EnabledCommandSet = ataId.EnabledCommandSet;
                    report.ATA.EnabledCommandSetSpecified = true;
                }
                if(ataId.EnabledCommandSet2 != 0)
                {
                    report.ATA.EnabledCommandSet2 = ataId.EnabledCommandSet2;
                    report.ATA.EnabledCommandSet2Specified = true;
                }
                if(ataId.EnabledCommandSet3 != 0)
                {
                    report.ATA.EnabledCommandSet3 = ataId.EnabledCommandSet3;
                    report.ATA.EnabledCommandSet3Specified = true;
                }
                if(ataId.EnabledCommandSet4 != 0)
                {
                    report.ATA.EnabledCommandSet4 = ataId.EnabledCommandSet4;
                    report.ATA.EnabledCommandSet4Specified = true;
                }
                if(ataId.EnabledSATAFeatures != 0)
                {
                    report.ATA.EnabledSATAFeatures = ataId.EnabledSATAFeatures;
                    report.ATA.EnabledSATAFeaturesSpecified = true;
                }
                if(ataId.ExtendedUserSectors != 0)
                {
                    report.ATA.ExtendedUserSectors = ataId.ExtendedUserSectors;
                    report.ATA.ExtendedUserSectorsSpecified = true;
                }
                if(ataId.FreeFallSensitivity != 0)
                {
                    report.ATA.FreeFallSensitivity = ataId.FreeFallSensitivity;
                    report.ATA.FreeFallSensitivitySpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(ataId.FirmwareRevision))
                {
                    report.ATA.FirmwareRevision = ataId.FirmwareRevision;
                    report.ATA.FirmwareRevisionSpecified = true;
                }
                if(ataId.GeneralConfiguration != 0)
                {
                    report.ATA.GeneralConfiguration = ataId.GeneralConfiguration;
                    report.ATA.GeneralConfigurationSpecified = true;
                }
                if(ataId.HardwareResetResult != 0)
                {
                    report.ATA.HardwareResetResult = ataId.HardwareResetResult;
                    report.ATA.HardwareResetResultSpecified = true;
                }
                if(ataId.InterseekDelay != 0)
                {
                    report.ATA.InterseekDelay = ataId.InterseekDelay;
                    report.ATA.InterseekDelaySpecified = true;
                }
                if(ataId.MajorVersion != 0)
                {
                    report.ATA.MajorVersion = ataId.MajorVersion;
                    report.ATA.MajorVersionSpecified = true;
                }
                if(ataId.MasterPasswordRevisionCode != 0)
                {
                    report.ATA.MasterPasswordRevisionCode = ataId.MasterPasswordRevisionCode;
                    report.ATA.MasterPasswordRevisionCodeSpecified = true;
                }
                if(ataId.MaxDownloadMicroMode3 != 0)
                {
                    report.ATA.MaxDownloadMicroMode3 = ataId.MaxDownloadMicroMode3;
                    report.ATA.MaxDownloadMicroMode3Specified = true;
                }
                if(ataId.MaxQueueDepth != 0)
                {
                    report.ATA.MaxQueueDepth = ataId.MaxQueueDepth;
                    report.ATA.MaxQueueDepthSpecified = true;
                }
                if(ataId.MDMAActive != 0)
                {
                    report.ATA.MDMAActive = ataId.MDMAActive;
                    report.ATA.MDMAActiveSpecified = true;
                }
                if(ataId.MDMASupported != 0)
                {
                    report.ATA.MDMASupported = ataId.MDMASupported;
                    report.ATA.MDMASupportedSpecified = true;
                }
                if(ataId.MinDownloadMicroMode3 != 0)
                {
                    report.ATA.MinDownloadMicroMode3 = ataId.MinDownloadMicroMode3;
                    report.ATA.MinDownloadMicroMode3Specified = true;
                }
                if(ataId.MinMDMACycleTime != 0)
                {
                    report.ATA.MinMDMACycleTime = ataId.MinMDMACycleTime;
                    report.ATA.MinMDMACycleTimeSpecified = true;
                }
                if(ataId.MinorVersion != 0)
                {
                    report.ATA.MinorVersion = ataId.MinorVersion;
                    report.ATA.MinorVersionSpecified = true;
                }
                if(ataId.MinPIOCycleTimeNoFlow != 0)
                {
                    report.ATA.MinPIOCycleTimeNoFlow = ataId.MinPIOCycleTimeNoFlow;
                    report.ATA.MinPIOCycleTimeNoFlowSpecified = true;
                }
                if(ataId.MinPIOCycleTimeFlow != 0)
                {
                    report.ATA.MinPIOCycleTimeFlow = ataId.MinPIOCycleTimeFlow;
                    report.ATA.MinPIOCycleTimeFlowSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(ataId.Model))
                {
                    report.ATA.Model = ataId.Model;
                    report.ATA.ModelSpecified = true;
                }
                if(ataId.MultipleMaxSectors != 0)
                {
                    report.ATA.MultipleMaxSectors = ataId.MultipleMaxSectors;
                    report.ATA.MultipleMaxSectorsSpecified = true;
                }
                if(ataId.MultipleSectorNumber != 0)
                {
                    report.ATA.MultipleSectorNumber = ataId.MultipleSectorNumber;
                    report.ATA.MultipleSectorNumberSpecified = true;
                }
                if(ataId.NVCacheCaps != 0)
                {
                    report.ATA.NVCacheCaps = ataId.NVCacheCaps;
                    report.ATA.NVCacheCapsSpecified = true;
                }
                if(ataId.NVCacheSize != 0)
                {
                    report.ATA.NVCacheSize = ataId.NVCacheSize;
                    report.ATA.NVCacheSizeSpecified = true;
                }
                if(ataId.NVCacheWriteSpeed != 0)
                {
                    report.ATA.NVCacheWriteSpeed = ataId.NVCacheWriteSpeed;
                    report.ATA.NVCacheWriteSpeedSpecified = true;
                }
                if(ataId.NVEstimatedSpinUp != 0)
                {
                    report.ATA.NVEstimatedSpinUp = ataId.NVEstimatedSpinUp;
                    report.ATA.NVEstimatedSpinUpSpecified = true;
                }
                if(ataId.PacketBusRelease != 0)
                {
                    report.ATA.PacketBusRelease = ataId.PacketBusRelease;
                    report.ATA.PacketBusReleaseSpecified = true;
                }
                if(ataId.PIOTransferTimingMode != 0)
                {
                    report.ATA.PIOTransferTimingMode = ataId.PIOTransferTimingMode;
                    report.ATA.PIOTransferTimingModeSpecified = true;
                }
                if(ataId.RecommendedAAM != 0)
                {
                    report.ATA.RecommendedAAM = ataId.RecommendedAAM;
                    report.ATA.RecommendedAAMSpecified = true;
                }
                if(ataId.RecMDMACycleTime != 0)
                {
                    report.ATA.RecommendedMDMACycleTime = ataId.RecMDMACycleTime;
                    report.ATA.RecommendedMDMACycleTimeSpecified = true;
                }
                if(ataId.RemovableStatusSet != 0)
                {
                    report.ATA.RemovableStatusSet = ataId.RemovableStatusSet;
                    report.ATA.RemovableStatusSetSpecified = true;
                }
                if(ataId.SATACapabilities != 0)
                {
                    report.ATA.SATACapabilities = ataId.SATACapabilities;
                    report.ATA.SATACapabilitiesSpecified = true;
                }
                if(ataId.SATACapabilities2 != 0)
                {
                    report.ATA.SATACapabilities2 = ataId.SATACapabilities2;
                    report.ATA.SATACapabilities2Specified = true;
                }
                if(ataId.SATAFeatures != 0)
                {
                    report.ATA.SATAFeatures = ataId.SATAFeatures;
                    report.ATA.SATAFeaturesSpecified = true;
                }
                if(ataId.SCTCommandTransport != 0)
                {
                    report.ATA.SCTCommandTransport = ataId.SCTCommandTransport;
                    report.ATA.SCTCommandTransportSpecified = true;
                }
                if(ataId.SectorsPerCard != 0)
                {
                    report.ATA.SectorsPerCard = ataId.SectorsPerCard;
                    report.ATA.SectorsPerCardSpecified = true;
                }
                if(ataId.SecurityEraseTime != 0)
                {
                    report.ATA.SecurityEraseTime = ataId.SecurityEraseTime;
                    report.ATA.SecurityEraseTimeSpecified = true;
                }
                if(ataId.SecurityStatus != 0)
                {
                    report.ATA.SecurityStatus = ataId.SecurityStatus;
                    report.ATA.SecurityStatusSpecified = true;
                }
                if(ataId.ServiceBusyClear != 0)
                {
                    report.ATA.ServiceBusyClear = ataId.ServiceBusyClear;
                    report.ATA.ServiceBusyClearSpecified = true;
                }
                if(ataId.SpecificConfiguration != 0)
                {
                    report.ATA.SpecificConfiguration = ataId.SpecificConfiguration;
                    report.ATA.SpecificConfigurationSpecified = true;
                }
                if(ataId.StreamAccessLatency != 0)
                {
                    report.ATA.StreamAccessLatency = ataId.StreamAccessLatency;
                    report.ATA.StreamAccessLatencySpecified = true;
                }
                if(ataId.StreamMinReqSize != 0)
                {
                    report.ATA.StreamMinReqSize = ataId.StreamMinReqSize;
                    report.ATA.StreamMinReqSizeSpecified = true;
                }
                if(ataId.StreamPerformanceGranularity != 0)
                {
                    report.ATA.StreamPerformanceGranularity = ataId.StreamPerformanceGranularity;
                    report.ATA.StreamPerformanceGranularitySpecified = true;
                }
                if(ataId.StreamTransferTimeDMA != 0)
                {
                    report.ATA.StreamTransferTimeDMA = ataId.StreamTransferTimeDMA;
                    report.ATA.StreamTransferTimeDMASpecified = true;
                }
                if(ataId.StreamTransferTimePIO != 0)
                {
                    report.ATA.StreamTransferTimePIO = ataId.StreamTransferTimePIO;
                    report.ATA.StreamTransferTimePIOSpecified = true;
                }
                if(ataId.TransportMajorVersion != 0)
                {
                    report.ATA.TransportMajorVersion = ataId.TransportMajorVersion;
                    report.ATA.TransportMajorVersionSpecified = true;
                }
                if(ataId.TransportMinorVersion != 0)
                {
                    report.ATA.TransportMinorVersion = ataId.TransportMinorVersion;
                    report.ATA.TransportMinorVersionSpecified = true;
                }
                if(ataId.TrustedComputing != 0)
                {
                    report.ATA.TrustedComputing = ataId.TrustedComputing;
                    report.ATA.TrustedComputingSpecified = true;
                }
                if(ataId.UDMAActive != 0)
                {
                    report.ATA.UDMAActive = ataId.UDMAActive;
                    report.ATA.UDMAActiveSpecified = true;
                }
                if(ataId.UDMASupported != 0)
                {
                    report.ATA.UDMASupported = ataId.UDMASupported;
                    report.ATA.UDMASupportedSpecified = true;
                }
                if(ataId.WRVMode != 0)
                {
                    report.ATA.WRVMode = ataId.WRVMode;
                    report.ATA.WRVModeSpecified = true;
                }
                if(ataId.WRVSectorCountMode3 != 0)
                {
                    report.ATA.WRVSectorCountMode3 = ataId.WRVSectorCountMode3;
                    report.ATA.WRVSectorCountMode3Specified = true;
                }
                if(ataId.WRVSectorCountMode2 != 0)
                {
                    report.ATA.WRVSectorCountMode2 = ataId.WRVSectorCountMode2;
                    report.ATA.WRVSectorCountMode2Specified = true;
                }

                if(removable)
                {
                    List<testedMediaType> mediaTests = new List<testedMediaType>();

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

                            testedMediaType mediaTest = new testedMediaType();
                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            mediaTest.MediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            mediaTest.Model = System.Console.ReadLine();

                            mediaTest.ManufacturerSpecified = true;
                            mediaTest.ModelSpecified = true;
                            mediaTest.MediaIsRecognized = true;

                            DicConsole.WriteLine("Querying ATA IDENTIFY...");
                            dev.AtaIdentify(out buffer, out errorRegs, timeout, out duration);

                            if(Decoders.ATA.Identify.Decode(buffer).HasValue)
                            {
                                ataId = Decoders.ATA.Identify.Decode(buffer).Value;

                                if(ataId.UnformattedBPT != 0)
                                {
                                    mediaTest.UnformattedBPT = ataId.UnformattedBPT;
                                    mediaTest.UnformattedBPTSpecified = true;
                                }
                                if(ataId.UnformattedBPS != 0)
                                {
                                    mediaTest.UnformattedBPS = ataId.UnformattedBPS;
                                    mediaTest.UnformattedBPSSpecified = true;
                                }

                                if(ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0)
                                {
                                    mediaTest.CHS = new chsType();
                                    mediaTest.CHS.Cylinders = ataId.Cylinders;
                                    mediaTest.CHS.Heads = ataId.Heads;
                                    mediaTest.CHS.Sectors = ataId.SectorsPerTrack;
                                    mediaTest.Blocks = (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                                    mediaTest.BlocksSpecified = true;
                                }

                                if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                                {
                                    mediaTest.CurrentCHS = new chsType();
                                    mediaTest.CurrentCHS.Cylinders = ataId.CurrentCylinders;
                                    mediaTest.CurrentCHS.Heads = ataId.CurrentHeads;
                                    mediaTest.CurrentCHS.Sectors = ataId.CurrentSectorsPerTrack;
                                    mediaTest.Blocks = (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads * ataId.CurrentSectorsPerTrack);
                                    mediaTest.BlocksSpecified = true;
                                }

                                if(ataId.Capabilities.HasFlag(Decoders.ATA.Identify.CapabilitiesBit.LBASupport))
                                {
                                    mediaTest.LBASectors = ataId.LBASectors;
                                    mediaTest.LBASectorsSpecified = true;
                                    mediaTest.Blocks = ataId.LBASectors;
                                    mediaTest.BlocksSpecified = true;
                                }

                                if(ataId.CommandSet2.HasFlag(Decoders.ATA.Identify.CommandSetBit2.LBA48))
                                {
                                    mediaTest.LBA48Sectors = ataId.LBA48Sectors;
                                    mediaTest.LBA48SectorsSpecified = true;
                                    mediaTest.Blocks = ataId.LBA48Sectors;
                                    mediaTest.BlocksSpecified = true;
                                }

                                if(ataId.NominalRotationRate != 0x0000 &&
                                ataId.NominalRotationRate != 0xFFFF)
                                {
                                    if(ataId.NominalRotationRate == 0x0001)
                                    {
                                        mediaTest.SolidStateDevice = true;
                                        mediaTest.SolidStateDeviceSpecified = true;
                                    }
                                    else
                                    {
                                        mediaTest.SolidStateDevice = false;
                                        mediaTest.SolidStateDeviceSpecified = true;
                                        mediaTest.NominalRotationRate = ataId.NominalRotationRate;
                                        mediaTest.NominalRotationRateSpecified = true;
                                    }
                                }

                                uint logicalsectorsize = 0;
                                uint physicalsectorsize;
                                if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                                (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                                {
                                    if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                                    {
                                        if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                                            logicalsectorsize = 512;
                                        else
                                            logicalsectorsize = ataId.LogicalSectorWords * 2;
                                    }
                                    else
                                        logicalsectorsize = 512;

                                    if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                                    {
                                        physicalsectorsize = logicalsectorsize * (uint)Math.Pow(2, (double)(ataId.PhysLogSectorSize & 0xF));
                                    }
                                    else
                                        physicalsectorsize = logicalsectorsize;
                                }
                                else
                                {
                                    logicalsectorsize = 512;
                                    physicalsectorsize = 512;
                                }

                                mediaTest.BlockSize = logicalsectorsize;
                                mediaTest.BlockSizeSpecified = true;
                                if(physicalsectorsize != logicalsectorsize)
                                {
                                    mediaTest.PhysicalBlockSize = physicalsectorsize;
                                    mediaTest.PhysicalBlockSizeSpecified = true;

                                    if((ataId.LogicalAlignment & 0x8000) == 0x0000 &&
                                    (ataId.LogicalAlignment & 0x4000) == 0x4000)
                                    {
                                        mediaTest.LogicalAlignment = (ushort)(ataId.LogicalAlignment & 0x3FFF);
                                        mediaTest.LogicalAlignmentSpecified = true;
                                    }
                                }

                                if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                                {
                                    mediaTest.LongBlockSize = logicalsectorsize + ataId.EccBytes;
                                    mediaTest.LongBlockSizeSpecified = true;
                                }

                                if(ataId.CommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MustBeSet) &&
                                !ataId.CommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MustBeClear) &&
                                ataId.EnabledCommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MediaSerial))
                                {
                                    mediaTest.CanReadMediaSerial = true;
                                    mediaTest.CanReadMediaSerialSpecified = true;
                                    if(!string.IsNullOrWhiteSpace(ataId.MediaManufacturer))
                                    {
                                        mediaTest.Manufacturer = ataId.MediaManufacturer;
                                        mediaTest.ManufacturerSpecified = true;
                                    }
                                }

                                mediaTest.SupportsReadLbaSpecified = true;
                                mediaTest.SupportsReadRetryLbaSpecified = true;
                                mediaTest.SupportsReadDmaLbaSpecified = true;
                                mediaTest.SupportsReadDmaRetryLbaSpecified = true;
                                mediaTest.SupportsReadLongLbaSpecified = true;
                                mediaTest.SupportsReadLongRetryLbaSpecified = true;
                                mediaTest.SupportsSeekLbaSpecified = true;

                                mediaTest.SupportsReadLba48Specified = true;
                                mediaTest.SupportsReadDmaLba48Specified = true;

                                mediaTest.SupportsReadSpecified = true;
                                mediaTest.SupportsReadRetrySpecified = true;
                                mediaTest.SupportsReadDmaSpecified = true;
                                mediaTest.SupportsReadDmaRetrySpecified = true;
                                mediaTest.SupportsReadLongSpecified = true;
                                mediaTest.SupportsReadLongRetrySpecified = true;
                                mediaTest.SupportsSeekSpecified = true;

                                Decoders.ATA.AtaErrorRegistersCHS errorChs;
                                Decoders.ATA.AtaErrorRegistersLBA28 errorLba;
                                Decoders.ATA.AtaErrorRegistersLBA48 errorLba48;

                                byte[] readBuf;
                                ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                                bool sense = true;

                                DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                                sense = dev.Read(out readBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                                mediaTest.SupportsRead = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                                sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                                mediaTest.SupportsReadRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                                sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                                mediaTest.SupportsReadDma = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                                sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                                mediaTest.SupportsReadDmaRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                                sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1, mediaTest.LongBlockSize, timeout, out duration);
                                mediaTest.SupportsReadLong = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                                DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                                sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1, mediaTest.LongBlockSize, timeout, out duration);
                                mediaTest.SupportsReadLongRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                                DicConsole.WriteLine("Trying SEEK in CHS mode...");
                                sense = dev.Seek(out errorChs, 0, 0, 1, timeout, out duration);
                                mediaTest.SupportsSeek = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0);

                                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                                sense = dev.Read(out readBuf, out errorLba, false, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                                sense = dev.Read(out readBuf, out errorLba, true, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                                sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadDmaLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                                sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadDmaRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                                sense = dev.ReadLong(out readBuf, out errorLba, false, 0, mediaTest.LongBlockSize, timeout, out duration);
                                mediaTest.SupportsReadLongLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                                DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                                sense = dev.ReadLong(out readBuf, out errorLba, true, 0, mediaTest.LongBlockSize, timeout, out duration);
                                mediaTest.SupportsReadLongRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                                DicConsole.WriteLine("Trying SEEK in LBA mode...");
                                sense = dev.Seek(out errorLba, 0, timeout, out duration);
                                mediaTest.SupportsSeekLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0);

                                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                                sense = dev.Read(out readBuf, out errorLba48, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadLba48 = (!sense && (errorLba48.status & 0x01) != 0x01 && errorLba48.error == 0 && readBuf.Length > 0);
                                DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                                sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, timeout, out duration);
                                mediaTest.SupportsReadDmaLba48 = (!sense && (errorLba48.status & 0x01) != 0x01 && errorLba48.error == 0 && readBuf.Length > 0);
                            }
                            else
                                mediaTest.MediaIsRecognized = false;

                            mediaTests.Add(mediaTest);
                        }
                    }
                    report.ATA.RemovableMedias = mediaTests.ToArray();
                }
                else
                {
                    report.ATA.ReadCapabilities = new testedMediaType();

                    if(ataId.UnformattedBPT != 0)
                    {
                        report.ATA.ReadCapabilities.UnformattedBPT = ataId.UnformattedBPT;
                        report.ATA.ReadCapabilities.UnformattedBPTSpecified = true;
                    }
                    if(ataId.UnformattedBPS != 0)
                    {
                        report.ATA.ReadCapabilities.UnformattedBPS = ataId.UnformattedBPS;
                        report.ATA.ReadCapabilities.UnformattedBPSSpecified = true;
                    }

                    if(ataId.Cylinders > 0 && ataId.Heads > 0 && ataId.SectorsPerTrack > 0)
                    {
                        report.ATA.ReadCapabilities.CHS = new chsType();
                        report.ATA.ReadCapabilities.CHS.Cylinders = ataId.Cylinders;
                        report.ATA.ReadCapabilities.CHS.Heads = ataId.Heads;
                        report.ATA.ReadCapabilities.CHS.Sectors = ataId.SectorsPerTrack;
                        report.ATA.ReadCapabilities.Blocks = (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                        report.ATA.ReadCapabilities.BlocksSpecified = true;
                    }

                    if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                    {
                        report.ATA.ReadCapabilities.CurrentCHS = new chsType();
                        report.ATA.ReadCapabilities.CurrentCHS.Cylinders = ataId.CurrentCylinders;
                        report.ATA.ReadCapabilities.CurrentCHS.Heads = ataId.CurrentHeads;
                        report.ATA.ReadCapabilities.CurrentCHS.Sectors = ataId.CurrentSectorsPerTrack;
                        report.ATA.ReadCapabilities.Blocks = (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads * ataId.CurrentSectorsPerTrack);
                        report.ATA.ReadCapabilities.BlocksSpecified = true;
                    }

                    if(ataId.Capabilities.HasFlag(Decoders.ATA.Identify.CapabilitiesBit.LBASupport))
                    {
                        report.ATA.ReadCapabilities.LBASectors = ataId.LBASectors;
                        report.ATA.ReadCapabilities.LBASectorsSpecified = true;
                        report.ATA.ReadCapabilities.Blocks = ataId.LBASectors;
                        report.ATA.ReadCapabilities.BlocksSpecified = true;
                    }

                    if(ataId.CommandSet2.HasFlag(Decoders.ATA.Identify.CommandSetBit2.LBA48))
                    {
                        report.ATA.ReadCapabilities.LBA48Sectors = ataId.LBA48Sectors;
                        report.ATA.ReadCapabilities.LBA48SectorsSpecified = true;
                        report.ATA.ReadCapabilities.Blocks = ataId.LBA48Sectors;
                        report.ATA.ReadCapabilities.BlocksSpecified = true;
                    }

                    if(ataId.NominalRotationRate != 0x0000 &&
                        ataId.NominalRotationRate != 0xFFFF)
                    {
                        if(ataId.NominalRotationRate == 0x0001)
                        {
                            report.ATA.ReadCapabilities.SolidStateDevice = true;
                            report.ATA.ReadCapabilities.SolidStateDeviceSpecified = true;
                        }
                        else
                        {
                            report.ATA.ReadCapabilities.SolidStateDevice = false;
                            report.ATA.ReadCapabilities.SolidStateDeviceSpecified = true;
                            report.ATA.ReadCapabilities.NominalRotationRate = ataId.NominalRotationRate;
                            report.ATA.ReadCapabilities.NominalRotationRateSpecified = true;
                        }
                    }

                    uint logicalsectorsize = 0;
                    uint physicalsectorsize;
                    if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                        (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                    {
                        if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                        {
                            if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                                logicalsectorsize = 512;
                            else
                                logicalsectorsize = ataId.LogicalSectorWords * 2;
                        }
                        else
                            logicalsectorsize = 512;

                        if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                        {
                            physicalsectorsize = logicalsectorsize * (uint)Math.Pow(2, (double)(ataId.PhysLogSectorSize & 0xF));
                        }
                        else
                            physicalsectorsize = logicalsectorsize;
                    }
                    else
                    {
                        logicalsectorsize = 512;
                        physicalsectorsize = 512;
                    }

                    report.ATA.ReadCapabilities.BlockSize = logicalsectorsize;
                    report.ATA.ReadCapabilities.BlockSizeSpecified = true;
                    if(physicalsectorsize != logicalsectorsize)
                    {
                        report.ATA.ReadCapabilities.PhysicalBlockSize = physicalsectorsize;
                        report.ATA.ReadCapabilities.PhysicalBlockSizeSpecified = true;

                        if((ataId.LogicalAlignment & 0x8000) == 0x0000 &&
                           (ataId.LogicalAlignment & 0x4000) == 0x4000)
                        {
                            report.ATA.ReadCapabilities.LogicalAlignment = (ushort)(ataId.LogicalAlignment & 0x3FFF);
                            report.ATA.ReadCapabilities.LogicalAlignmentSpecified = true;
                        }
                    }

                    if(ataId.EccBytes != 0x0000 && ataId.EccBytes != 0xFFFF)
                    {
                        report.ATA.ReadCapabilities.LongBlockSize = logicalsectorsize + ataId.EccBytes;
                        report.ATA.ReadCapabilities.LongBlockSizeSpecified = true;
                    }

                    if(ataId.CommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MustBeSet) &&
                        !ataId.CommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MustBeClear) &&
                        ataId.EnabledCommandSet3.HasFlag(Decoders.ATA.Identify.CommandSetBit3.MediaSerial))
                    {
                        report.ATA.ReadCapabilities.CanReadMediaSerial = true;
                        report.ATA.ReadCapabilities.CanReadMediaSerialSpecified = true;
                        if(!string.IsNullOrWhiteSpace(ataId.MediaManufacturer))
                        {
                            report.ATA.ReadCapabilities.Manufacturer = ataId.MediaManufacturer;
                            report.ATA.ReadCapabilities.ManufacturerSpecified = true;
                        }
                    }

                    report.ATA.ReadCapabilities.SupportsReadLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadRetryLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadDmaLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadDmaRetryLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadLongLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadLongRetryLbaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsSeekLbaSpecified = true;

                    report.ATA.ReadCapabilities.SupportsReadLba48Specified = true;
                    report.ATA.ReadCapabilities.SupportsReadDmaLba48Specified = true;

                    report.ATA.ReadCapabilities.SupportsReadSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadRetrySpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadDmaSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadDmaRetrySpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadLongSpecified = true;
                    report.ATA.ReadCapabilities.SupportsReadLongRetrySpecified = true;
                    report.ATA.ReadCapabilities.SupportsSeekSpecified = true;

                    Decoders.ATA.AtaErrorRegistersCHS errorChs;
                    Decoders.ATA.AtaErrorRegistersLBA28 errorLba;
                    Decoders.ATA.AtaErrorRegistersLBA48 errorLba48;

                    byte[] readBuf;
                    ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                    bool sense = true;

                    DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                    sense = dev.Read(out readBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsRead = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                    sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                    sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadDma = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                    sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadDmaRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                    sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1, report.ATA.ReadCapabilities.LongBlockSize, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLong = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                    DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                    sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1, report.ATA.ReadCapabilities.LongBlockSize, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLongRetry = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                    DicConsole.WriteLine("Trying SEEK in CHS mode...");
                    sense = dev.Seek(out errorChs, 0, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsSeek = (!sense && (errorChs.status & 0x01) != 0x01 && errorChs.error == 0);

                    DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                    sense = dev.Read(out readBuf, out errorLba, false, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                    sense = dev.Read(out readBuf, out errorLba, true, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                    sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadDmaLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                    sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadDmaRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                    sense = dev.ReadLong(out readBuf, out errorLba, false, 0, report.ATA.ReadCapabilities.LongBlockSize, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLongLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                    DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                    sense = dev.ReadLong(out readBuf, out errorLba, true, 0, report.ATA.ReadCapabilities.LongBlockSize, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLongRetryLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0 && readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead);
                    DicConsole.WriteLine("Trying SEEK in LBA mode...");
                    sense = dev.Seek(out errorLba, 0, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsSeekLba = (!sense && (errorLba.status & 0x01) != 0x01 && errorLba.error == 0);

                    DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                    sense = dev.Read(out readBuf, out errorLba48, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadLba48 = (!sense && (errorLba48.status & 0x01) != 0x01 && errorLba48.error == 0 && readBuf.Length > 0);
                    DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                    sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, timeout, out duration);
                    report.ATA.ReadCapabilities.SupportsReadDmaLba48 = (!sense && (errorLba48.status & 0x01) != 0x01 && errorLba48.error == 0 && readBuf.Length > 0);
                }
            }

            FileStream xmlFs = new FileStream(xmlFile, FileMode.Create);

            System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(Metadata.DeviceReport));
            xmlSer.Serialize(xmlFs, report);
            xmlFs.Close();
        }

        static void doNVMeDeviceReport(DeviceReportOptions options, Device dev)
        {
            throw new NotImplementedException("NVMe devices not yet supported.");
        }

        static void doSDDeviceReport(DeviceReportOptions options, Device dev)
        {
            throw new NotImplementedException("MMC/SD devices not yet supported.");
        }

        static void doSCSIDeviceReport(DeviceReportOptions options, Device dev)
        {
            byte[] senseBuffer;
            byte[] buffer;
            double duration;
            bool sense;
            uint timeout = 5;
            Metadata.DeviceReport report = new Metadata.DeviceReport();
            string xmlFile;
            if(!string.IsNullOrWhiteSpace(dev.Manufacturer) && !string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Manufacturer + "_" + dev.Model + "_" + dev.Revision + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Manufacturer))
                xmlFile = dev.Manufacturer + "_" + dev.Model + ".xml";
            else if(!string.IsNullOrWhiteSpace(dev.Revision))
                xmlFile = dev.Model + "_" + dev.Revision + ".xml";
            else
                xmlFile = dev.Model + ".xml";
            ConsoleKeyInfo pressedKey;
            bool removable = false;

            if(dev.IsUSB)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively USB (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.USB = new usbType();
                    report.USB.Manufacturer = dev.USBManufacturerString;
                    report.USB.Product = dev.USBProductString;
                    report.USB.ProductID = dev.USBProductID;
                    report.USB.VendorID = dev.USBVendorID;

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.USB.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable = report.USB.RemovableMedia;
                }
            }

            if(dev.IsFireWire)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the device natively FireWire (in case of doubt, press Y)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                if(pressedKey.Key == ConsoleKey.Y)
                {
                    report.FireWire = new firewireType();
                    report.FireWire.Manufacturer = dev.FireWireVendorName;
                    report.FireWire.Product = dev.FireWireModelName;
                    report.FireWire.ProductID = dev.FireWireModel;
                    report.FireWire.VendorID = dev.FireWireVendor;

                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    report.FireWire.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                    removable = report.FireWire.RemovableMedia;
                }
            }

            if(!dev.IsUSB && !dev.IsFireWire && dev.IsRemovable)
            {
                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                removable = pressedKey.Key == ConsoleKey.Y;
            }

            if(dev.Type == DeviceType.ATAPI)
            {
                DicConsole.WriteLine("Querying ATAPI IDENTIFY...");

                DiscImageChef.Decoders.ATA.AtaErrorRegistersCHS errorRegs;
                dev.AtapiIdentify(out buffer, out errorRegs, timeout, out duration);

                if(Decoders.ATA.Identify.Decode(buffer).HasValue)
                {
                    Decoders.ATA.Identify.IdentifyDevice atapiId = Decoders.ATA.Identify.Decode(buffer).Value;

                    report.ATAPI = new ataType();

                    if(!string.IsNullOrWhiteSpace(atapiId.AdditionalPID))
                    {
                        report.ATAPI.AdditionalPID = atapiId.AdditionalPID;
                        report.ATAPI.AdditionalPIDSpecified = true;
                    }
                    if(atapiId.APIOSupported != 0)
                    {
                        report.ATAPI.APIOSupported = atapiId.APIOSupported;
                        report.ATAPI.APIOSupportedSpecified = true;
                    }
                    if(atapiId.ATAPIByteCount != 0)
                    {
                        report.ATAPI.ATAPIByteCount = atapiId.ATAPIByteCount;
                        report.ATAPI.ATAPIByteCountSpecified = true;
                    }
                    if(atapiId.BufferType != 0)
                    {
                        report.ATAPI.BufferType = atapiId.BufferType;
                        report.ATAPI.BufferTypeSpecified = true;
                    }
                    if(atapiId.BufferSize != 0)
                    {
                        report.ATAPI.BufferSize = atapiId.BufferSize;
                        report.ATAPI.BufferSizeSpecified = true;
                    }
                    if(atapiId.Capabilities != 0)
                    {
                        report.ATAPI.Capabilities = atapiId.Capabilities;
                        report.ATAPI.CapabilitiesSpecified = true;
                    }
                    if(atapiId.Capabilities2 != 0)
                    {
                        report.ATAPI.Capabilities2 = atapiId.Capabilities2;
                        report.ATAPI.Capabilities2Specified = true;
                    }
                    if(atapiId.Capabilities3 != 0)
                    {
                        report.ATAPI.Capabilities3 = atapiId.Capabilities3;
                        report.ATAPI.Capabilities3Specified = true;
                    }
                    if(atapiId.CFAPowerMode != 0)
                    {
                        report.ATAPI.CFAPowerMode = atapiId.CFAPowerMode;
                        report.ATAPI.CFAPowerModeSpecified = true;
                    }
                    if(atapiId.CommandSet != 0)
                    {
                        report.ATAPI.CommandSet = atapiId.CommandSet;
                        report.ATAPI.CommandSetSpecified = true;
                    }
                    if(atapiId.CommandSet2 != 0)
                    {
                        report.ATAPI.CommandSet2 = atapiId.CommandSet2;
                        report.ATAPI.CommandSet2Specified = true;
                    }
                    if(atapiId.CommandSet3 != 0)
                    {
                        report.ATAPI.CommandSet3 = atapiId.CommandSet3;
                        report.ATAPI.CommandSet3Specified = true;
                    }
                    if(atapiId.CommandSet4 != 0)
                    {
                        report.ATAPI.CommandSet4 = atapiId.CommandSet4;
                        report.ATAPI.CommandSet4Specified = true;
                    }
                    if(atapiId.CommandSet5 != 0)
                    {
                        report.ATAPI.CommandSet5 = atapiId.CommandSet5;
                        report.ATAPI.CommandSet5Specified = true;
                    }
                    if(atapiId.CurrentAAM != 0)
                    {
                        report.ATAPI.CurrentAAM = atapiId.CurrentAAM;
                        report.ATAPI.CurrentAAMSpecified = true;
                    }
                    if(atapiId.CurrentAPM != 0)
                    {
                        report.ATAPI.CurrentAPM = atapiId.CurrentAPM;
                        report.ATAPI.CurrentAPMSpecified = true;
                    }
                    if(atapiId.DataSetMgmt != 0)
                    {
                        report.ATAPI.DataSetMgmt = atapiId.DataSetMgmt;
                        report.ATAPI.DataSetMgmtSpecified = true;
                    }
                    if(atapiId.DataSetMgmtSize != 0)
                    {
                        report.ATAPI.DataSetMgmtSize = atapiId.DataSetMgmtSize;
                        report.ATAPI.DataSetMgmtSizeSpecified = true;
                    }
                    if(atapiId.DeviceFormFactor != 0)
                    {
                        report.ATAPI.DeviceFormFactor = atapiId.DeviceFormFactor;
                        report.ATAPI.DeviceFormFactorSpecified = true;
                    }
                    if(atapiId.DMAActive != 0)
                    {
                        report.ATAPI.DMAActive = atapiId.DMAActive;
                        report.ATAPI.DMAActiveSpecified = true;
                    }
                    if(atapiId.DMASupported != 0)
                    {
                        report.ATAPI.DMASupported = atapiId.DMASupported;
                        report.ATAPI.DMASupportedSpecified = true;
                    }
                    if(atapiId.DMATransferTimingMode != 0)
                    {
                        report.ATAPI.DMATransferTimingMode = atapiId.DMATransferTimingMode;
                        report.ATAPI.DMATransferTimingModeSpecified = true;
                    }
                    if(atapiId.EnhancedSecurityEraseTime != 0)
                    {
                        report.ATAPI.EnhancedSecurityEraseTime = atapiId.EnhancedSecurityEraseTime;
                        report.ATAPI.EnhancedSecurityEraseTimeSpecified = true;
                    }
                    if(atapiId.EnabledCommandSet != 0)
                    {
                        report.ATAPI.EnabledCommandSet = atapiId.EnabledCommandSet;
                        report.ATAPI.EnabledCommandSetSpecified = true;
                    }
                    if(atapiId.EnabledCommandSet2 != 0)
                    {
                        report.ATAPI.EnabledCommandSet2 = atapiId.EnabledCommandSet2;
                        report.ATAPI.EnabledCommandSet2Specified = true;
                    }
                    if(atapiId.EnabledCommandSet3 != 0)
                    {
                        report.ATAPI.EnabledCommandSet3 = atapiId.EnabledCommandSet3;
                        report.ATAPI.EnabledCommandSet3Specified = true;
                    }
                    if(atapiId.EnabledCommandSet4 != 0)
                    {
                        report.ATAPI.EnabledCommandSet4 = atapiId.EnabledCommandSet4;
                        report.ATAPI.EnabledCommandSet4Specified = true;
                    }
                    if(atapiId.EnabledSATAFeatures != 0)
                    {
                        report.ATAPI.EnabledSATAFeatures = atapiId.EnabledSATAFeatures;
                        report.ATAPI.EnabledSATAFeaturesSpecified = true;
                    }
                    if(atapiId.ExtendedUserSectors != 0)
                    {
                        report.ATAPI.ExtendedUserSectors = atapiId.ExtendedUserSectors;
                        report.ATAPI.ExtendedUserSectorsSpecified = true;
                    }
                    if(atapiId.FreeFallSensitivity != 0)
                    {
                        report.ATAPI.FreeFallSensitivity = atapiId.FreeFallSensitivity;
                        report.ATAPI.FreeFallSensitivitySpecified = true;
                    }
                    if(!string.IsNullOrWhiteSpace(atapiId.FirmwareRevision))
                    {
                        report.ATAPI.FirmwareRevision = atapiId.FirmwareRevision;
                        report.ATAPI.FirmwareRevisionSpecified = true;
                    }
                    if(atapiId.GeneralConfiguration != 0)
                    {
                        report.ATAPI.GeneralConfiguration = atapiId.GeneralConfiguration;
                        report.ATAPI.GeneralConfigurationSpecified = true;
                    }
                    if(atapiId.HardwareResetResult != 0)
                    {
                        report.ATAPI.HardwareResetResult = atapiId.HardwareResetResult;
                        report.ATAPI.HardwareResetResultSpecified = true;
                    }
                    if(atapiId.InterseekDelay != 0)
                    {
                        report.ATAPI.InterseekDelay = atapiId.InterseekDelay;
                        report.ATAPI.InterseekDelaySpecified = true;
                    }
                    if(atapiId.MajorVersion != 0)
                    {
                        report.ATAPI.MajorVersion = atapiId.MajorVersion;
                        report.ATAPI.MajorVersionSpecified = true;
                    }
                    if(atapiId.MasterPasswordRevisionCode != 0)
                    {
                        report.ATAPI.MasterPasswordRevisionCode = atapiId.MasterPasswordRevisionCode;
                        report.ATAPI.MasterPasswordRevisionCodeSpecified = true;
                    }
                    if(atapiId.MaxDownloadMicroMode3 != 0)
                    {
                        report.ATAPI.MaxDownloadMicroMode3 = atapiId.MaxDownloadMicroMode3;
                        report.ATAPI.MaxDownloadMicroMode3Specified = true;
                    }
                    if(atapiId.MaxQueueDepth != 0)
                    {
                        report.ATAPI.MaxQueueDepth = atapiId.MaxQueueDepth;
                        report.ATAPI.MaxQueueDepthSpecified = true;
                    }
                    if(atapiId.MDMAActive != 0)
                    {
                        report.ATAPI.MDMAActive = atapiId.MDMAActive;
                        report.ATAPI.MDMAActiveSpecified = true;
                    }
                    if(atapiId.MDMASupported != 0)
                    {
                        report.ATAPI.MDMASupported = atapiId.MDMASupported;
                        report.ATAPI.MDMASupportedSpecified = true;
                    }
                    if(atapiId.MinDownloadMicroMode3 != 0)
                    {
                        report.ATAPI.MinDownloadMicroMode3 = atapiId.MinDownloadMicroMode3;
                        report.ATAPI.MinDownloadMicroMode3Specified = true;
                    }
                    if(atapiId.MinMDMACycleTime != 0)
                    {
                        report.ATAPI.MinMDMACycleTime = atapiId.MinMDMACycleTime;
                        report.ATAPI.MinMDMACycleTimeSpecified = true;
                    }
                    if(atapiId.MinorVersion != 0)
                    {
                        report.ATAPI.MinorVersion = atapiId.MinorVersion;
                        report.ATAPI.MinorVersionSpecified = true;
                    }
                    if(atapiId.MinPIOCycleTimeNoFlow != 0)
                    {
                        report.ATAPI.MinPIOCycleTimeNoFlow = atapiId.MinPIOCycleTimeNoFlow;
                        report.ATAPI.MinPIOCycleTimeNoFlowSpecified = true;
                    }
                    if(atapiId.MinPIOCycleTimeFlow != 0)
                    {
                        report.ATAPI.MinPIOCycleTimeFlow = atapiId.MinPIOCycleTimeFlow;
                        report.ATAPI.MinPIOCycleTimeFlowSpecified = true;
                    }
                    if(!string.IsNullOrWhiteSpace(atapiId.Model))
                    {
                        report.ATAPI.Model = atapiId.Model;
                        report.ATAPI.ModelSpecified = true;
                    }
                    if(atapiId.MultipleMaxSectors != 0)
                    {
                        report.ATAPI.MultipleMaxSectors = atapiId.MultipleMaxSectors;
                        report.ATAPI.MultipleMaxSectorsSpecified = true;
                    }
                    if(atapiId.MultipleSectorNumber != 0)
                    {
                        report.ATAPI.MultipleSectorNumber = atapiId.MultipleSectorNumber;
                        report.ATAPI.MultipleSectorNumberSpecified = true;
                    }
                    if(atapiId.NVCacheCaps != 0)
                    {
                        report.ATAPI.NVCacheCaps = atapiId.NVCacheCaps;
                        report.ATAPI.NVCacheCapsSpecified = true;
                    }
                    if(atapiId.NVCacheSize != 0)
                    {
                        report.ATAPI.NVCacheSize = atapiId.NVCacheSize;
                        report.ATAPI.NVCacheSizeSpecified = true;
                    }
                    if(atapiId.NVCacheWriteSpeed != 0)
                    {
                        report.ATAPI.NVCacheWriteSpeed = atapiId.NVCacheWriteSpeed;
                        report.ATAPI.NVCacheWriteSpeedSpecified = true;
                    }
                    if(atapiId.NVEstimatedSpinUp != 0)
                    {
                        report.ATAPI.NVEstimatedSpinUp = atapiId.NVEstimatedSpinUp;
                        report.ATAPI.NVEstimatedSpinUpSpecified = true;
                    }
                    if(atapiId.PacketBusRelease != 0)
                    {
                        report.ATAPI.PacketBusRelease = atapiId.PacketBusRelease;
                        report.ATAPI.PacketBusReleaseSpecified = true;
                    }
                    if(atapiId.PIOTransferTimingMode != 0)
                    {
                        report.ATAPI.PIOTransferTimingMode = atapiId.PIOTransferTimingMode;
                        report.ATAPI.PIOTransferTimingModeSpecified = true;
                    }
                    if(atapiId.RecommendedAAM != 0)
                    {
                        report.ATAPI.RecommendedAAM = atapiId.RecommendedAAM;
                        report.ATAPI.RecommendedAAMSpecified = true;
                    }
                    if(atapiId.RecMDMACycleTime != 0)
                    {
                        report.ATAPI.RecommendedMDMACycleTime = atapiId.RecMDMACycleTime;
                        report.ATAPI.RecommendedMDMACycleTimeSpecified = true;
                    }
                    if(atapiId.RemovableStatusSet != 0)
                    {
                        report.ATAPI.RemovableStatusSet = atapiId.RemovableStatusSet;
                        report.ATAPI.RemovableStatusSetSpecified = true;
                    }
                    if(atapiId.SATACapabilities != 0)
                    {
                        report.ATAPI.SATACapabilities = atapiId.SATACapabilities;
                        report.ATAPI.SATACapabilitiesSpecified = true;
                    }
                    if(atapiId.SATACapabilities2 != 0)
                    {
                        report.ATAPI.SATACapabilities2 = atapiId.SATACapabilities2;
                        report.ATAPI.SATACapabilities2Specified = true;
                    }
                    if(atapiId.SATAFeatures != 0)
                    {
                        report.ATAPI.SATAFeatures = atapiId.SATAFeatures;
                        report.ATAPI.SATAFeaturesSpecified = true;
                    }
                    if(atapiId.SCTCommandTransport != 0)
                    {
                        report.ATAPI.SCTCommandTransport = atapiId.SCTCommandTransport;
                        report.ATAPI.SCTCommandTransportSpecified = true;
                    }
                    if(atapiId.SectorsPerCard != 0)
                    {
                        report.ATAPI.SectorsPerCard = atapiId.SectorsPerCard;
                        report.ATAPI.SectorsPerCardSpecified = true;
                    }
                    if(atapiId.SecurityEraseTime != 0)
                    {
                        report.ATAPI.SecurityEraseTime = atapiId.SecurityEraseTime;
                        report.ATAPI.SecurityEraseTimeSpecified = true;
                    }
                    if(atapiId.SecurityStatus != 0)
                    {
                        report.ATAPI.SecurityStatus = atapiId.SecurityStatus;
                        report.ATAPI.SecurityStatusSpecified = true;
                    }
                    if(atapiId.ServiceBusyClear != 0)
                    {
                        report.ATAPI.ServiceBusyClear = atapiId.ServiceBusyClear;
                        report.ATAPI.ServiceBusyClearSpecified = true;
                    }
                    if(atapiId.SpecificConfiguration != 0)
                    {
                        report.ATAPI.SpecificConfiguration = atapiId.SpecificConfiguration;
                        report.ATAPI.SpecificConfigurationSpecified = true;
                    }
                    if(atapiId.StreamAccessLatency != 0)
                    {
                        report.ATAPI.StreamAccessLatency = atapiId.StreamAccessLatency;
                        report.ATAPI.StreamAccessLatencySpecified = true;
                    }
                    if(atapiId.StreamMinReqSize != 0)
                    {
                        report.ATAPI.StreamMinReqSize = atapiId.StreamMinReqSize;
                        report.ATAPI.StreamMinReqSizeSpecified = true;
                    }
                    if(atapiId.StreamPerformanceGranularity != 0)
                    {
                        report.ATAPI.StreamPerformanceGranularity = atapiId.StreamPerformanceGranularity;
                        report.ATAPI.StreamPerformanceGranularitySpecified = true;
                    }
                    if(atapiId.StreamTransferTimeDMA != 0)
                    {
                        report.ATAPI.StreamTransferTimeDMA = atapiId.StreamTransferTimeDMA;
                        report.ATAPI.StreamTransferTimeDMASpecified = true;
                    }
                    if(atapiId.StreamTransferTimePIO != 0)
                    {
                        report.ATAPI.StreamTransferTimePIO = atapiId.StreamTransferTimePIO;
                        report.ATAPI.StreamTransferTimePIOSpecified = true;
                    }
                    if(atapiId.TransportMajorVersion != 0)
                    {
                        report.ATAPI.TransportMajorVersion = atapiId.TransportMajorVersion;
                        report.ATAPI.TransportMajorVersionSpecified = true;
                    }
                    if(atapiId.TransportMinorVersion != 0)
                    {
                        report.ATAPI.TransportMinorVersion = atapiId.TransportMinorVersion;
                        report.ATAPI.TransportMinorVersionSpecified = true;
                    }
                    if(atapiId.TrustedComputing != 0)
                    {
                        report.ATAPI.TrustedComputing = atapiId.TrustedComputing;
                        report.ATAPI.TrustedComputingSpecified = true;
                    }
                    if(atapiId.UDMAActive != 0)
                    {
                        report.ATAPI.UDMAActive = atapiId.UDMAActive;
                        report.ATAPI.UDMAActiveSpecified = true;
                    }
                    if(atapiId.UDMASupported != 0)
                    {
                        report.ATAPI.UDMASupported = atapiId.UDMASupported;
                        report.ATAPI.UDMASupportedSpecified = true;
                    }
                    if(atapiId.WRVMode != 0)
                    {
                        report.ATAPI.WRVMode = atapiId.WRVMode;
                        report.ATAPI.WRVModeSpecified = true;
                    }
                    if(atapiId.WRVSectorCountMode3 != 0)
                    {
                        report.ATAPI.WRVSectorCountMode3 = atapiId.WRVSectorCountMode3;
                        report.ATAPI.WRVSectorCountMode3Specified = true;
                    }
                    if(atapiId.WRVSectorCountMode2 != 0)
                    {
                        report.ATAPI.WRVSectorCountMode2 = atapiId.WRVSectorCountMode2;
                        report.ATAPI.WRVSectorCountMode2Specified = true;
                    }
                }
            }

            DicConsole.WriteLine("Querying SCSI INQUIRY...");
            sense = dev.ScsiInquiry(out buffer, out senseBuffer);

            report.SCSI = new scsiType();

            if(!sense && Decoders.SCSI.Inquiry.Decode(buffer).HasValue)
            {
                Decoders.SCSI.Inquiry.SCSIInquiry inq = Decoders.SCSI.Inquiry.Decode(buffer).Value;

                List<UInt16> versionDescriptors = new List<UInt16>();
                report.SCSI.Inquiry = new scsiInquiryType();

                if(inq.DeviceTypeModifier != 0)
                {
                    report.SCSI.Inquiry.DeviceTypeModifier = inq.DeviceTypeModifier;
                    report.SCSI.Inquiry.DeviceTypeModifierSpecified = true;
                }
                if(inq.ISOVersion != 0)
                {
                    report.SCSI.Inquiry.ISOVersion = inq.ISOVersion;
                    report.SCSI.Inquiry.ISOVersionSpecified = true;
                }
                if(inq.ECMAVersion != 0)
                {
                    report.SCSI.Inquiry.ECMAVersion = inq.ECMAVersion;
                    report.SCSI.Inquiry.ECMAVersionSpecified = true;
                }
                if(inq.ANSIVersion != 0)
                {
                    report.SCSI.Inquiry.ANSIVersion = inq.ANSIVersion;
                    report.SCSI.Inquiry.ANSIVersionSpecified = true;
                }
                if(inq.ResponseDataFormat != 0)
                {
                    report.SCSI.Inquiry.ResponseDataFormat = inq.ResponseDataFormat;
                    report.SCSI.Inquiry.ResponseDataFormatSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.VendorIdentification)))
                {
                    report.SCSI.Inquiry.VendorIdentification = StringHandlers.CToString(inq.VendorIdentification).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.VendorIdentification))
                        report.SCSI.Inquiry.VendorIdentificationSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.ProductIdentification)))
                {
                    report.SCSI.Inquiry.ProductIdentification = StringHandlers.CToString(inq.ProductIdentification).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.ProductIdentification))
                        report.SCSI.Inquiry.ProductIdentificationSpecified = true;
                }
                if(!string.IsNullOrWhiteSpace(StringHandlers.CToString(inq.ProductRevisionLevel)))
                {
                    report.SCSI.Inquiry.ProductRevisionLevel = StringHandlers.CToString(inq.ProductRevisionLevel).Trim();
                    if(!string.IsNullOrWhiteSpace(report.SCSI.Inquiry.ProductRevisionLevel))
                        report.SCSI.Inquiry.ProductRevisionLevelSpecified = true;
                }
                if(inq.VersionDescriptors != null)
                {
                    foreach(UInt16 descriptor in inq.VersionDescriptors)
                    {
                        if(descriptor != 0)
                            versionDescriptors.Add(descriptor);
                    }

                    if(versionDescriptors.Count > 0)
                        report.SCSI.Inquiry.VersionDescriptors = versionDescriptors.ToArray();
                }

                report.SCSI.Inquiry.PeripheralQualifier = (Decoders.SCSI.PeripheralQualifiers)inq.PeripheralQualifier;
                report.SCSI.Inquiry.PeripheralDeviceType = (Decoders.SCSI.PeripheralDeviceTypes)inq.PeripheralDeviceType;
                report.SCSI.Inquiry.AsymmetricalLUNAccess = (Decoders.SCSI.TGPSValues)inq.TPGS;
                report.SCSI.Inquiry.SPIClocking = (Decoders.SCSI.SPIClocking)inq.Clocking;

                report.SCSI.Inquiry.AccessControlCoordinator = inq.ACC;
                report.SCSI.Inquiry.ACKRequests = inq.ACKREQQ;
                report.SCSI.Inquiry.AERCSupported = inq.AERC;
                report.SCSI.Inquiry.Address16 = inq.Addr16;
                report.SCSI.Inquiry.Address32 = inq.Addr32;
                report.SCSI.Inquiry.BasicQueueing = inq.BQue;
                report.SCSI.Inquiry.EnclosureServices = inq.EncServ;
                report.SCSI.Inquiry.HierarchicalLUN = inq.HiSup;
                report.SCSI.Inquiry.IUS = inq.IUS;
                report.SCSI.Inquiry.LinkedCommands = inq.Linked;
                report.SCSI.Inquiry.MediumChanger = inq.MChngr;
                report.SCSI.Inquiry.MultiPortDevice = inq.MultiP;
                report.SCSI.Inquiry.NormalACA = inq.NormACA;
                report.SCSI.Inquiry.Protection = inq.Protect;
                report.SCSI.Inquiry.QAS = inq.QAS;
                report.SCSI.Inquiry.RelativeAddressing = inq.RelAddr;
                report.SCSI.Inquiry.Removable = inq.RMB;
                report.SCSI.Inquiry.TaggedCommandQueue = inq.CmdQue;
                report.SCSI.Inquiry.TerminateTaskSupported = inq.TrmTsk;
                report.SCSI.Inquiry.ThirdPartyCopy = inq.ThreePC;
                report.SCSI.Inquiry.TranferDisable = inq.TranDis;
                report.SCSI.Inquiry.SoftReset = inq.SftRe;
                report.SCSI.Inquiry.StorageArrayController = inq.SCCS;
                report.SCSI.Inquiry.SyncTransfer = inq.Sync;
                report.SCSI.Inquiry.WideBus16 = inq.WBus16;
                report.SCSI.Inquiry.WideBus32 = inq.WBus32;
            }

            DicConsole.WriteLine("Querying list of SCSI EVPDs...");
            sense = dev.ScsiInquiry(out buffer, out senseBuffer, 0x00);

            if(!sense)
            {
                byte[] evpdPages = Decoders.SCSI.EVPD.DecodePage00(buffer);
                if(evpdPages != null && evpdPages.Length > 0)
                {
                    List<pageType> evpds = new List<pageType>();
                    foreach(byte page in evpdPages)
                    {
                        if(page != 0x80)
                        {
                            DicConsole.WriteLine("Querying SCSI EVPD {0:X2}h...", page);
                            sense = dev.ScsiInquiry(out buffer, out senseBuffer, page);
                            if(!sense)
                            {
                                pageType evpd = new pageType();
                                evpd.page = page;
                                evpd.value = buffer;
                                evpds.Add(evpd);
                            }
                        }
                    }
                    if(evpds.Count > 0)
                        report.SCSI.EVPDPages = evpds.ToArray();
                }
            }

            if(removable)
            {
                if(dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
                {
                    dev.AllowMediumRemoval(out senseBuffer, timeout, out duration);
                    dev.EjectTray(out senseBuffer, timeout, out duration);
                }
                else if(dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
                {
                    dev.SpcAllowMediumRemoval(out senseBuffer, timeout, out duration);
                    DicConsole.WriteLine("Asking drive to unload tape (can take a few minutes)...");
                    dev.Unload(out senseBuffer, timeout, out duration);
                }
                DicConsole.WriteLine("Please remove any media from the device and press any key when it is out.");
                System.Console.ReadKey(true);
            }

            Decoders.SCSI.Modes.DecodedMode? decMode = null;
            Decoders.SCSI.PeripheralDeviceTypes devType = dev.SCSIType;

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (10)...");
            sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Default, 0x3F, 0xFF, timeout, out duration);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (10)...");
                sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Default, 0x3F, 0x00, timeout, out duration);
                if(!sense && dev.Error)
                {
                    report.SCSI.SupportsModeSense10 = true;
                    report.SCSI.SupportsModeSubpages = false;
                    decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, devType);
                }
            }
            else
            {
                report.SCSI.SupportsModeSense10 = true;
                report.SCSI.SupportsModeSubpages = true;
                decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, devType);
            }

            DicConsole.WriteLine("Querying all mode pages and subpages using SCSI MODE SENSE (6)...");
            sense = dev.ModeSense6(out buffer, out senseBuffer, false, ScsiModeSensePageControl.Default, 0x3F, 0xFF, timeout, out duration);
            if(sense || dev.Error)
            {
                DicConsole.WriteLine("Querying all mode pages using SCSI MODE SENSE (6)...");
                sense = dev.ModeSense6(out buffer, out senseBuffer, false, ScsiModeSensePageControl.Default, 0x3F, 0x00, timeout, out duration);
                if(sense || dev.Error)
                {
                    DicConsole.WriteLine("Querying SCSI MODE SENSE (6)...");
                    sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                }
            }
            else
                report.SCSI.SupportsModeSubpages = true;

            if(!sense && !dev.Error && !decMode.HasValue)
                decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, devType);

            if(!sense && !dev.Error)
                report.SCSI.SupportsModeSense6 = true;

            Decoders.SCSI.Modes.ModePage_2A? cdromMode = null;

            if(decMode.HasValue)
            {
                report.SCSI.ModeSense = new modeType();
                report.SCSI.ModeSense.BlankCheckEnabled = decMode.Value.Header.EBC;
                report.SCSI.ModeSense.DPOandFUA = decMode.Value.Header.DPOFUA;
                report.SCSI.ModeSense.WriteProtected = decMode.Value.Header.WriteProtected;

                if(decMode.Value.Header.BufferedMode > 0)
                {
                    report.SCSI.ModeSense.BufferedMode = decMode.Value.Header.BufferedMode;
                    report.SCSI.ModeSense.BufferedModeSpecified = true;
                }

                if(decMode.Value.Header.Speed > 0)
                {
                    report.SCSI.ModeSense.Speed = decMode.Value.Header.Speed;
                    report.SCSI.ModeSense.SpeedSpecified = true;
                }

                if(decMode.Value.Pages != null)
                {
                    List<modePageType> modePages = new List<modePageType>();
                    foreach(Decoders.SCSI.Modes.ModePage page in decMode.Value.Pages)
                    {
                        modePageType modePage = new modePageType();
                        modePage.page = page.Page;
                        modePage.subpage = page.Subpage;
                        modePage.value = page.PageResponse;
                        modePages.Add(modePage);

                        if(modePage.page == 0x2A && modePage.subpage == 0x00)
                        {
                            cdromMode = Decoders.SCSI.Modes.DecodeModePage_2A(page.PageResponse);
                        }
                    }

                    if(modePages.Count > 0)
                        report.SCSI.ModeSense.ModePages = modePages.ToArray();
                }
            }

            List<string> mediaTypes = new List<string>();

            #region MultiMediaDevice
            if(dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.MultiMediaDevice)
            {
                report.SCSI.MultiMediaDevice = new mmcType();

                if(cdromMode.HasValue)
                {
                    report.SCSI.MultiMediaDevice.ModeSense2A = new mmcModeType();
                    if(cdromMode.Value.BufferSize != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.BufferSize = cdromMode.Value.BufferSize;
                        report.SCSI.MultiMediaDevice.ModeSense2A.BufferSizeSpecified = true;
                    }
                    if(cdromMode.Value.CurrentSpeed != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentSpeed = cdromMode.Value.CurrentSpeed;
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentSpeedSpecified = true;
                    }
                    if(cdromMode.Value.CurrentWriteSpeed != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeed = cdromMode.Value.CurrentWriteSpeed;
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSpecified = true;
                    }
                    if(cdromMode.Value.CurrentWriteSpeedSelected != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSelected = cdromMode.Value.CurrentWriteSpeedSelected;
                        report.SCSI.MultiMediaDevice.ModeSense2A.CurrentWriteSpeedSelectedSpecified = true;
                    }
                    if(cdromMode.Value.MaximumSpeed != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.MaximumSpeed = cdromMode.Value.MaximumSpeed;
                        report.SCSI.MultiMediaDevice.ModeSense2A.MaximumSpeedSpecified = true;
                    }
                    if(cdromMode.Value.MaxWriteSpeed != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.MaximumWriteSpeed = cdromMode.Value.MaxWriteSpeed;
                        report.SCSI.MultiMediaDevice.ModeSense2A.MaximumWriteSpeedSpecified = true;
                    }
                    if(cdromMode.Value.RotationControlSelected != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.RotationControlSelected = cdromMode.Value.RotationControlSelected;
                        report.SCSI.MultiMediaDevice.ModeSense2A.RotationControlSelectedSpecified = true;
                    }
                    if(cdromMode.Value.SupportedVolumeLevels != 0)
                    {
                        report.SCSI.MultiMediaDevice.ModeSense2A.SupportedVolumeLevels = cdromMode.Value.SupportedVolumeLevels;
                        report.SCSI.MultiMediaDevice.ModeSense2A.SupportedVolumeLevelsSpecified = true;
                    }

                    report.SCSI.MultiMediaDevice.ModeSense2A.AccurateCDDA = cdromMode.Value.AccurateCDDA;
                    report.SCSI.MultiMediaDevice.ModeSense2A.BCK = cdromMode.Value.BCK;
                    report.SCSI.MultiMediaDevice.ModeSense2A.BufferUnderRunProtection = cdromMode.Value.BUF;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CanEject = cdromMode.Value.Eject;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CanLockMedia = cdromMode.Value.Lock;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CDDACommand = cdromMode.Value.CDDACommand;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CompositeAudioVideo = cdromMode.Value.Composite;
                    report.SCSI.MultiMediaDevice.ModeSense2A.CSSandCPPMSupported = cdromMode.Value.CMRSupported == 1;
                    report.SCSI.MultiMediaDevice.ModeSense2A.DeterministicSlotChanger = cdromMode.Value.SDP;
                    report.SCSI.MultiMediaDevice.ModeSense2A.DigitalPort1 = cdromMode.Value.DigitalPort1;
                    report.SCSI.MultiMediaDevice.ModeSense2A.DigitalPort2 = cdromMode.Value.DigitalPort2;
                    report.SCSI.MultiMediaDevice.ModeSense2A.LeadInPW = cdromMode.Value.LeadInPW;
                    report.SCSI.MultiMediaDevice.ModeSense2A.LoadingMechanismType = cdromMode.Value.LoadingMechanism;
                    report.SCSI.MultiMediaDevice.ModeSense2A.LockStatus = cdromMode.Value.LockState;
                    report.SCSI.MultiMediaDevice.ModeSense2A.LSBF = cdromMode.Value.LSBF;
                    report.SCSI.MultiMediaDevice.ModeSense2A.PlaysAudio = cdromMode.Value.AudioPlay;
                    report.SCSI.MultiMediaDevice.ModeSense2A.PreventJumperStatus = cdromMode.Value.PreventJumper;
                    report.SCSI.MultiMediaDevice.ModeSense2A.RCK = cdromMode.Value.RCK;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsBarcode = cdromMode.Value.ReadBarcode;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsBothSides = cdromMode.Value.SCC;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsCDR = cdromMode.Value.ReadCDR;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsCDRW = cdromMode.Value.ReadCDRW;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDeinterlavedSubchannel = cdromMode.Value.DeinterlaveSubchannel;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDR = cdromMode.Value.ReadDVDR;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDRAM = cdromMode.Value.ReadDVDRAM;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsDVDROM = cdromMode.Value.ReadDVDROM;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsISRC = cdromMode.Value.ISRC;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsMode2Form2 = cdromMode.Value.Mode2Form2;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsMode2Form1 = cdromMode.Value.Mode2Form1;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsPacketCDR = cdromMode.Value.Method2;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsSubchannel = cdromMode.Value.Subchannel;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReadsUPC = cdromMode.Value.UPC;
                    report.SCSI.MultiMediaDevice.ModeSense2A.ReturnsC2Pointers = cdromMode.Value.C2Pointer;
                    report.SCSI.MultiMediaDevice.ModeSense2A.SeparateChannelMute = cdromMode.Value.SeparateChannelMute;
                    report.SCSI.MultiMediaDevice.ModeSense2A.SeparateChannelVolume = cdromMode.Value.SeparateChannelVolume;
                    report.SCSI.MultiMediaDevice.ModeSense2A.SSS = cdromMode.Value.SSS;
                    report.SCSI.MultiMediaDevice.ModeSense2A.SupportsMultiSession = cdromMode.Value.MultiSession;
                    report.SCSI.MultiMediaDevice.ModeSense2A.TestWrite = cdromMode.Value.TestWrite;
                    report.SCSI.MultiMediaDevice.ModeSense2A.WritesCDR = cdromMode.Value.WriteCDR;
                    report.SCSI.MultiMediaDevice.ModeSense2A.WritesCDRW = cdromMode.Value.WriteCDRW;
                    report.SCSI.MultiMediaDevice.ModeSense2A.WritesDVDR = cdromMode.Value.WriteDVDR;
                    report.SCSI.MultiMediaDevice.ModeSense2A.WritesDVDRAM = cdromMode.Value.WriteDVDRAM;
                    report.SCSI.MultiMediaDevice.ModeSense2A.WriteSpeedPerformanceDescriptors = cdromMode.Value.WriteSpeedPerformanceDescriptors;

                    mediaTypes.Add("CD-ROM");
                    mediaTypes.Add("Audio CD");
                    if(cdromMode.Value.ReadCDR)
                        mediaTypes.Add("CD-R");
                    if(cdromMode.Value.ReadCDRW)
                        mediaTypes.Add("CD-RW");
                    if(cdromMode.Value.ReadDVDROM)
                        mediaTypes.Add("DVD-ROM");
                    if(cdromMode.Value.ReadDVDRAM)
                        mediaTypes.Add("DVD-RAM");
                    if(cdromMode.Value.ReadDVDR)
                        mediaTypes.Add("DVD-R");
                }

                DicConsole.WriteLine("Querying MMC GET CONFIGURATION...");
                sense = dev.GetConfiguration(out buffer, out senseBuffer, timeout, out duration);

                if(!sense)
                {
                    Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(buffer);
                    if(ftr.Descriptors != null && ftr.Descriptors.Length > 0)
                    {
                        report.SCSI.MultiMediaDevice.Features = new mmcFeaturesType();
                        foreach(Decoders.SCSI.MMC.Features.FeatureDescriptor desc in ftr.Descriptors)
                        {
                            switch(desc.Code)
                            {
                                case 0x0001:
                                    {
                                        Decoders.SCSI.MMC.Feature_0001? ftr0001 = Decoders.SCSI.MMC.Features.Decode_0001(desc.Data);
                                        if(ftr0001.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandard = ftr0001.Value.PhysicalInterfaceStandard;
                                            report.SCSI.MultiMediaDevice.Features.PhysicalInterfaceStandardSpecified = true;
                                            report.SCSI.MultiMediaDevice.Features.SupportsDeviceBusyEvent = ftr0001.Value.DBE;
                                        }
                                    }
                                    break;
                                case 0x0003:
                                    {
                                        Decoders.SCSI.MMC.Feature_0003? ftr0003 = Decoders.SCSI.MMC.Features.Decode_0003(desc.Data);
                                        if(ftr0003.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.LoadingMechanismType = ftr0003.Value.LoadingMechanismType;
                                            report.SCSI.MultiMediaDevice.Features.LoadingMechanismTypeSpecified = true;
                                            report.SCSI.MultiMediaDevice.Features.CanLoad = ftr0003.Value.Load;
                                            report.SCSI.MultiMediaDevice.Features.CanEject = ftr0003.Value.Eject;
                                            report.SCSI.MultiMediaDevice.Features.PreventJumper = ftr0003.Value.PreventJumper;
                                            report.SCSI.MultiMediaDevice.Features.DBML = ftr0003.Value.DBML;
                                            report.SCSI.MultiMediaDevice.Features.Locked = ftr0003.Value.Lock;
                                        }
                                    }
                                    break;
                                case 0x0004:
                                    {
                                        Decoders.SCSI.MMC.Feature_0004? ftr0004 = Decoders.SCSI.MMC.Features.Decode_0004(desc.Data);
                                        if(ftr0004.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.SupportsWriteProtectPAC = ftr0004.Value.DWP;
                                            report.SCSI.MultiMediaDevice.Features.SupportsWriteInhibitDCB = ftr0004.Value.WDCB;
                                            report.SCSI.MultiMediaDevice.Features.SupportsPWP = ftr0004.Value.SPWP;
                                            report.SCSI.MultiMediaDevice.Features.SupportsSWPP = ftr0004.Value.SSWPP;
                                        }
                                    }
                                    break;
                                case 0x0010:
                                    {
                                        Decoders.SCSI.MMC.Feature_0010? ftr0010 = Decoders.SCSI.MMC.Features.Decode_0010(desc.Data);
                                        if(ftr0010.HasValue)
                                        {
                                            if(ftr0010.Value.LogicalBlockSize > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.LogicalBlockSize = ftr0010.Value.LogicalBlockSize;
                                                report.SCSI.MultiMediaDevice.Features.LogicalBlockSizeSpecified = true;
                                            }
                                            if(ftr0010.Value.Blocking > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.BlocksPerReadableUnit = ftr0010.Value.Blocking;
                                                report.SCSI.MultiMediaDevice.Features.BlocksPerReadableUnitSpecified = true;
                                            }
                                            report.SCSI.MultiMediaDevice.Features.ErrorRecoveryPage = ftr0010.Value.PP;
                                        }
                                    }
                                    break;
                                case 0x001D:
                                    report.SCSI.MultiMediaDevice.Features.MultiRead = true;
                                    break;
                                case 0x001E:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadCD = true;
                                        Decoders.SCSI.MMC.Feature_001E? ftr001E = Decoders.SCSI.MMC.Features.Decode_001E(desc.Data);
                                        if(ftr001E.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.SupportsDAP = ftr001E.Value.DAP;
                                            report.SCSI.MultiMediaDevice.Features.SupportsC2 = ftr001E.Value.C2;
                                            report.SCSI.MultiMediaDevice.Features.CanReadLeadInCDText = ftr001E.Value.CDText;
                                        }
                                    }
                                    break;
                                case 0x001F:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadDVD = true;
                                        Decoders.SCSI.MMC.Feature_001F? ftr001F = Decoders.SCSI.MMC.Features.Decode_001F(desc.Data);
                                        if(ftr001F.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.DVDMultiRead = ftr001F.Value.MULTI110;
                                            report.SCSI.MultiMediaDevice.Features.CanReadAllDualRW = ftr001F.Value.DualRW;
                                            report.SCSI.MultiMediaDevice.Features.CanReadAllDualR = ftr001F.Value.DualRW;
                                        }
                                    }
                                    break;
                                case 0x0022:
                                    report.SCSI.MultiMediaDevice.Features.CanEraseSector = true;
                                    break;
                                case 0x0023:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanFormat = true;
                                        Decoders.SCSI.MMC.Feature_0023? ftr0023 = Decoders.SCSI.MMC.Features.Decode_0023(desc.Data);
                                        if(ftr0023.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanFormatBDREWithoutSpare = ftr0023.Value.RENoSA;
                                            report.SCSI.MultiMediaDevice.Features.CanExpandBDRESpareArea = ftr0023.Value.Expand;
                                            report.SCSI.MultiMediaDevice.Features.CanFormatQCert = ftr0023.Value.QCert;
                                            report.SCSI.MultiMediaDevice.Features.CanFormatCert = ftr0023.Value.Cert;
                                            report.SCSI.MultiMediaDevice.Features.CanFormatFRF = ftr0023.Value.FRF;
                                            report.SCSI.MultiMediaDevice.Features.CanFormatRRM = ftr0023.Value.RRM;
                                        }
                                    }
                                    break;
                                case 0x0024:
                                    report.SCSI.MultiMediaDevice.Features.CanReadSpareAreaInformation = true;
                                    break;
                                case 0x0027:
                                    report.SCSI.MultiMediaDevice.Features.CanWriteCDRWCAV = true;
                                    break;
                                case 0x0028:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadCDMRW = true;
                                        Decoders.SCSI.MMC.Feature_0028? ftr0028 = Decoders.SCSI.MMC.Features.Decode_0028(desc.Data);
                                        if(ftr0028.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW = ftr0028.Value.DVDPRead;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusMRW = ftr0028.Value.DVDPWrite;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteCDMRW = ftr0028.Value.Write;
                                        }
                                    }
                                    break;
                                case 0x002A:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW = true;
                                        Decoders.SCSI.MMC.Feature_002A? ftr002A = Decoders.SCSI.MMC.Features.Decode_002A(desc.Data);
                                        if(ftr002A.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRW = ftr002A.Value.Write;
                                        }
                                    }
                                    break;
                                case 0x002B:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR = true;
                                        Decoders.SCSI.MMC.Feature_002B? ftr002B = Decoders.SCSI.MMC.Features.Decode_002B(desc.Data);
                                        if(ftr002B.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusR = ftr002B.Value.Write;
                                        }
                                    }
                                    break;
                                case 0x002D:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanWriteCDTAO = true;
                                        Decoders.SCSI.MMC.Feature_002D? ftr002D = Decoders.SCSI.MMC.Features.Decode_002D(desc.Data);
                                        if(ftr002D.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInTAO = ftr002D.Value.BUF;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteRawSubchannelInTAO = ftr002D.Value.RWRaw;
                                            report.SCSI.MultiMediaDevice.Features.CanWritePackedSubchannelInTAO = ftr002D.Value.RWPack;
                                            report.SCSI.MultiMediaDevice.Features.CanTestWriteInTAO = ftr002D.Value.TestWrite;
                                            report.SCSI.MultiMediaDevice.Features.CanOverwriteTAOTrack = ftr002D.Value.CDRW;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteRWSubchannelInTAO = ftr002D.Value.RWSubchannel;
                                        }
                                    }
                                    break;
                                case 0x002E:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanWriteCDSAO = true;
                                        Decoders.SCSI.MMC.Feature_002E? ftr002E = Decoders.SCSI.MMC.Features.Decode_002E(desc.Data);
                                        if(ftr002E.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInSAO = ftr002E.Value.BUF;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteRawMultiSession = ftr002E.Value.RAWMS;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteRaw = ftr002E.Value.RAW;
                                            report.SCSI.MultiMediaDevice.Features.CanTestWriteInSAO = ftr002E.Value.TestWrite;
                                            report.SCSI.MultiMediaDevice.Features.CanOverwriteSAOTrack = ftr002E.Value.CDRW;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteRWSubchannelInSAO = ftr002E.Value.RW;
                                        }
                                    }
                                    break;
                                case 0x002F:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanWriteDVDR = true;
                                        Decoders.SCSI.MMC.Feature_002F? ftr002F = Decoders.SCSI.MMC.Features.Decode_002F(desc.Data);
                                        if(ftr002F.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.BufferUnderrunFreeInDVD = ftr002F.Value.BUF;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDRDL = ftr002F.Value.RDL;
                                            report.SCSI.MultiMediaDevice.Features.CanTestWriteDVD = ftr002F.Value.TestWrite;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDRW = ftr002F.Value.DVDRW;
                                        }
                                    }
                                    break;
                                case 0x0030:
                                    report.SCSI.MultiMediaDevice.Features.CanReadDDCD = true;
                                    break;
                                case 0x0031:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanWriteDDCDR = true;
                                        ;
                                        Decoders.SCSI.MMC.Feature_0031? ftr0031 = Decoders.SCSI.MMC.Features.Decode_0031(desc.Data);
                                        if(ftr0031.HasValue)
                                            report.SCSI.MultiMediaDevice.Features.CanTestWriteDDCDR = ftr0031.Value.TestWrite;
                                    }
                                    break;
                                case 0x0032:
                                    report.SCSI.MultiMediaDevice.Features.CanWriteDDCDRW = true;
                                    break;
                                case 0x0037:
                                    report.SCSI.MultiMediaDevice.Features.CanWriteCDRW = true;
                                    break;
                                case 0x0038:
                                    report.SCSI.MultiMediaDevice.Features.CanPseudoOverwriteBDR = true;
                                    break;
                                case 0x003A:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL = true;
                                        Decoders.SCSI.MMC.Feature_003A? ftr003A = Decoders.SCSI.MMC.Features.Decode_003A(desc.Data);
                                        if(ftr003A.HasValue)
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRWDL = ftr003A.Value.Write;
                                    }
                                    break;
                                case 0x003B:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL = true;
                                        Decoders.SCSI.MMC.Feature_003B? ftr003B = Decoders.SCSI.MMC.Features.Decode_003B(desc.Data);
                                        if(ftr003B.HasValue)
                                            report.SCSI.MultiMediaDevice.Features.CanWriteDVDPlusRDL = ftr003B.Value.Write;
                                    }
                                    break;
                                case 0x0040:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadBD = true;
                                        Decoders.SCSI.MMC.Feature_0040? ftr0040 = Decoders.SCSI.MMC.Features.Decode_0040(desc.Data);
                                        if(ftr0040.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanReadBluBCA = ftr0040.Value.BCA;
                                            report.SCSI.MultiMediaDevice.Features.CanReadBDRE2 = ftr0040.Value.RE2;
                                            report.SCSI.MultiMediaDevice.Features.CanReadBDRE1 = ftr0040.Value.RE1;
                                            report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE = ftr0040.Value.OldRE;
                                            report.SCSI.MultiMediaDevice.Features.CanReadBDR = ftr0040.Value.R;
                                            report.SCSI.MultiMediaDevice.Features.CanReadOldBDR = ftr0040.Value.OldR;
                                            report.SCSI.MultiMediaDevice.Features.CanReadBDROM = ftr0040.Value.ROM;
                                            report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM = ftr0040.Value.OldROM;
                                        }
                                    }
                                    break;
                                case 0x0041:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanWriteBD = true;
                                        Decoders.SCSI.MMC.Feature_0041? ftr0041 = Decoders.SCSI.MMC.Features.Decode_0041(desc.Data);
                                        if(ftr0041.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanWriteBDRE2 = ftr0041.Value.RE2;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteBDRE1 = ftr0041.Value.RE1;
                                            report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE = ftr0041.Value.OldRE;
                                            report.SCSI.MultiMediaDevice.Features.CanReadBDR = ftr0041.Value.R;
                                            report.SCSI.MultiMediaDevice.Features.CanReadOldBDR = ftr0041.Value.OldR;
                                        }
                                    }
                                    break;
                                case 0x0050:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanReadHDDVD = true;
                                        Decoders.SCSI.MMC.Feature_0050? ftr0050 = Decoders.SCSI.MMC.Features.Decode_0050(desc.Data);
                                        if(ftr0050.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR = ftr0050.Value.HDDVDR;
                                            report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM = ftr0050.Value.HDDVDRAM;
                                        }
                                    }
                                    break;
                                case 0x0051:
                                    {
                                        Decoders.SCSI.MMC.Feature_0051? ftr0051 = Decoders.SCSI.MMC.Features.Decode_0051(desc.Data);
                                        if(ftr0051.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanWriteHDDVDR = ftr0051.Value.HDDVDR;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteHDDVDRAM = ftr0051.Value.HDDVDRAM;
                                        }
                                    }
                                    break;
                                case 0x0080:
                                    report.SCSI.MultiMediaDevice.Features.SupportsHybridDiscs = true;
                                    break;
                                case 0x0101:
                                    report.SCSI.MultiMediaDevice.Features.SupportsModePage1Ch = true;
                                    break;
                                case 0x0102:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.EmbeddedChanger = true;
                                        Decoders.SCSI.MMC.Feature_0102? ftr0102 = Decoders.SCSI.MMC.Features.Decode_0102(desc.Data);
                                        if(ftr0102.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.ChangerIsSideChangeCapable = ftr0102.Value.SCC;
                                            report.SCSI.MultiMediaDevice.Features.ChangerSupportsDiscPresent = ftr0102.Value.SDP;
                                            report.SCSI.MultiMediaDevice.Features.ChangerSlots = (byte)(ftr0102.Value.HighestSlotNumber + 1);
                                        }
                                    }
                                    break;
                                case 0x0103:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.CanPlayCDAudio = true;
                                        Decoders.SCSI.MMC.Feature_0103? ftr0103 = Decoders.SCSI.MMC.Features.Decode_0103(desc.Data);
                                        if(ftr0103.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanAudioScan = ftr0103.Value.Scan;
                                            report.SCSI.MultiMediaDevice.Features.CanMuteSeparateChannels = ftr0103.Value.SCM;
                                            report.SCSI.MultiMediaDevice.Features.SupportsSeparateVolume = ftr0103.Value.SV;
                                            if(ftr0103.Value.VolumeLevels > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.VolumeLevelsSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.VolumeLevels = ftr0103.Value.VolumeLevels;
                                            }
                                        }
                                    }
                                    break;
                                case 0x0104:
                                    report.SCSI.MultiMediaDevice.Features.CanUpgradeFirmware = true;
                                    break;
                                case 0x0106:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.SupportsCSS = true;
                                        Decoders.SCSI.MMC.Feature_0106? ftr0106 = Decoders.SCSI.MMC.Features.Decode_0106(desc.Data);
                                        if(ftr0106.HasValue)
                                        {
                                            if(ftr0106.Value.CSSVersion > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.CSSVersionSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.CSSVersion = ftr0106.Value.CSSVersion;
                                            }
                                        }
                                    }
                                    break;
                                case 0x0108:
                                    report.SCSI.MultiMediaDevice.Features.CanReportDriveSerial = true;
                                    break;
                                case 0x0109:
                                    report.SCSI.MultiMediaDevice.Features.CanReportMediaSerial = true;
                                    break;
                                case 0x010B:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.SupportsCPRM = true;
                                        Decoders.SCSI.MMC.Feature_010B? ftr010B = Decoders.SCSI.MMC.Features.Decode_010B(desc.Data);
                                        if(ftr010B.HasValue)
                                        {
                                            if(ftr010B.Value.CPRMVersion > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.CPRMVersionSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.CPRMVersion = ftr010B.Value.CPRMVersion;
                                            }
                                        }
                                    }
                                    break;
                                case 0x010C:
                                    {
                                        Decoders.SCSI.MMC.Feature_010C? ftr010C = Decoders.SCSI.MMC.Features.Decode_010C(desc.Data);
                                        if(ftr010C.HasValue)
                                        {
                                            string syear, smonth, sday, shour, sminute, ssecond;
                                            byte[] temp;

                                            temp = new byte[4];
                                            temp[0] = (byte)((ftr010C.Value.Century & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Century & 0xFF);
                                            temp[2] = (byte)((ftr010C.Value.Year & 0xFF00) >> 8);
                                            temp[3] = (byte)(ftr010C.Value.Year & 0xFF);
                                            syear = System.Text.Encoding.ASCII.GetString(temp);
                                            temp = new byte[2];
                                            temp[0] = (byte)((ftr010C.Value.Month & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Month & 0xFF);
                                            smonth = System.Text.Encoding.ASCII.GetString(temp);
                                            temp = new byte[2];
                                            temp[0] = (byte)((ftr010C.Value.Day & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Day & 0xFF);
                                            sday = System.Text.Encoding.ASCII.GetString(temp);
                                            temp = new byte[2];
                                            temp[0] = (byte)((ftr010C.Value.Hour & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Hour & 0xFF);
                                            shour = System.Text.Encoding.ASCII.GetString(temp);
                                            temp = new byte[2];
                                            temp[0] = (byte)((ftr010C.Value.Minute & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Minute & 0xFF);
                                            sminute = System.Text.Encoding.ASCII.GetString(temp);
                                            temp = new byte[2];
                                            temp[0] = (byte)((ftr010C.Value.Second & 0xFF00) >> 8);
                                            temp[1] = (byte)(ftr010C.Value.Second & 0xFF);
                                            ssecond = System.Text.Encoding.ASCII.GetString(temp);

                                            try
                                            {
                                                report.SCSI.MultiMediaDevice.Features.FirmwareDate = new DateTime(Int32.Parse(syear), Int32.Parse(smonth),
                                                    Int32.Parse(sday), Int32.Parse(shour), Int32.Parse(sminute),
                                                    Int32.Parse(ssecond), DateTimeKind.Utc);

                                                report.SCSI.MultiMediaDevice.Features.FirmwareDateSpecified = true;
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    break;
                                case 0x010D:
                                    {
                                        report.SCSI.MultiMediaDevice.Features.SupportsAACS = true;
                                        Decoders.SCSI.MMC.Feature_010D? ftr010D = Decoders.SCSI.MMC.Features.Decode_010D(desc.Data);
                                        if(ftr010D.HasValue)
                                        {
                                            report.SCSI.MultiMediaDevice.Features.CanReadDriveAACSCertificate = ftr010D.Value.RDC;
                                            report.SCSI.MultiMediaDevice.Features.CanReadCPRM_MKB = ftr010D.Value.RMC;
                                            report.SCSI.MultiMediaDevice.Features.CanWriteBusEncryptedBlocks = ftr010D.Value.WBE;
                                            report.SCSI.MultiMediaDevice.Features.SupportsBusEncryption = ftr010D.Value.BEC;
                                            report.SCSI.MultiMediaDevice.Features.CanGenerateBindingNonce = ftr010D.Value.BNG;

                                            if(ftr010D.Value.BindNonceBlocks > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.BindingNonceBlocksSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.BindingNonceBlocks = ftr010D.Value.BindNonceBlocks;
                                            }

                                            if(ftr010D.Value.AGIDs > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.AGIDsSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.AGIDs = ftr010D.Value.AGIDs;
                                            }

                                            if(ftr010D.Value.AACSVersion > 0)
                                            {
                                                report.SCSI.MultiMediaDevice.Features.AACSVersionSpecified = true;
                                                report.SCSI.MultiMediaDevice.Features.AACSVersion = ftr010D.Value.AACSVersion;
                                            }
                                        }
                                    }
                                    break;
                                case 0x010E:
                                    report.SCSI.MultiMediaDevice.Features.CanWriteCSSManagedDVD = true;
                                    break;
                                case 0x0113:
                                    report.SCSI.MultiMediaDevice.Features.SupportsSecurDisc = true;
                                    break;
                                case 0x0142:
                                    report.SCSI.MultiMediaDevice.Features.SupportsOSSC = true;
                                    break;
                                case 0x0110:
                                    report.SCSI.MultiMediaDevice.Features.SupportsVCPS = true;
                                    break;
                            }
                        }
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadBD ||
                        report.SCSI.MultiMediaDevice.Features.CanReadBDR ||
                        report.SCSI.MultiMediaDevice.Features.CanReadBDRE1 ||
                        report.SCSI.MultiMediaDevice.Features.CanReadBDRE2 ||
                        report.SCSI.MultiMediaDevice.Features.CanReadBDROM ||
                        report.SCSI.MultiMediaDevice.Features.CanReadOldBDR ||
                        report.SCSI.MultiMediaDevice.Features.CanReadOldBDRE ||
                        report.SCSI.MultiMediaDevice.Features.CanReadOldBDROM)
                    {
                        if(!mediaTypes.Contains("BD-ROM"))
                            mediaTypes.Add("BD-ROM");
                        if(!mediaTypes.Contains("BD-R"))
                            mediaTypes.Add("BD-R");
                        if(!mediaTypes.Contains("BD-RE"))
                            mediaTypes.Add("BD-RE");
                        if(!mediaTypes.Contains("BD-R LTH"))
                            mediaTypes.Add("BD-R LTH");
                        if(!mediaTypes.Contains("BD-R XL"))
                            mediaTypes.Add("BD-R XL");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadCD ||
                        report.SCSI.MultiMediaDevice.Features.MultiRead)
                    {
                        if(!mediaTypes.Contains("CD-ROM"))
                            mediaTypes.Add("CD-ROM");
                        if(!mediaTypes.Contains("Audio CD"))
                            mediaTypes.Add("Audio CD");
                        if(!mediaTypes.Contains("CD-R"))
                            mediaTypes.Add("CD-R");
                        if(!mediaTypes.Contains("CD-RW"))
                            mediaTypes.Add("CD-RW");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadCDMRW)
                    {
                        if(!mediaTypes.Contains("CD-MRW"))
                            mediaTypes.Add("CD-MRW");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadDDCD)
                    {
                        if(!mediaTypes.Contains("DDCD-ROM"))
                            mediaTypes.Add("DDCD-ROM");
                        if(!mediaTypes.Contains("DDCD-R"))
                            mediaTypes.Add("DDCD-R");
                        if(!mediaTypes.Contains("DDCD-RW"))
                            mediaTypes.Add("DDCD-RW");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadDVD ||
                        report.SCSI.MultiMediaDevice.Features.DVDMultiRead ||
                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusR ||
                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRDL ||
                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRW ||
                        report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusRWDL)
                    {
                        if(!mediaTypes.Contains("DVD-ROM"))
                            mediaTypes.Add("DVD-ROM");
                        if(!mediaTypes.Contains("DVD-R"))
                            mediaTypes.Add("DVD-R");
                        if(!mediaTypes.Contains("DVD-RW"))
                            mediaTypes.Add("DVD-RW");
                        if(!mediaTypes.Contains("DVD+R"))
                            mediaTypes.Add("DVD+R");
                        if(!mediaTypes.Contains("DVD+RW"))
                            mediaTypes.Add("DVD+RW");
                        if(!mediaTypes.Contains("DVD-R DL"))
                            mediaTypes.Add("DVD-R DL");
                        if(!mediaTypes.Contains("DVD+R DL"))
                            mediaTypes.Add("DVD+R DL");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadDVDPlusMRW)
                    {
                        if(!mediaTypes.Contains("DVD+MRW"))
                            mediaTypes.Add("DVD+MRW");
                    }


                    if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVD ||
                        report.SCSI.MultiMediaDevice.Features.CanReadHDDVDR)
                    {
                        if(!mediaTypes.Contains("HD DVD-ROM"))
                            mediaTypes.Add("HD DVD-ROM");
                        if(!mediaTypes.Contains("HD DVD-R"))
                            mediaTypes.Add("HD DVD-R");
                        if(!mediaTypes.Contains("HD DVD-RW"))
                            mediaTypes.Add("HD DVD-RW");
                    }

                    if(report.SCSI.MultiMediaDevice.Features.CanReadHDDVDRAM)
                    {
                        if(!mediaTypes.Contains("HD DVD-RAM"))
                            mediaTypes.Add("HD DVD-RAM");
                    }
                }

                bool tryPlextor = false, tryHLDTST = false, tryPioneer = false, tryNEC = false;

                tryPlextor |= dev.Manufacturer.ToLowerInvariant() == "plextor";
                tryHLDTST |= dev.Manufacturer.ToLowerInvariant() == "hl-dt-st";
                tryPioneer |= dev.Manufacturer.ToLowerInvariant() == "pioneer";
                tryNEC |= dev.Manufacturer.ToLowerInvariant() == "nec";

                // Very old CD drives do not contain mode page 2Ah neither GET CONFIGURATION, so just try all CDs on them
                // Also don't get confident, some drives didn't know CD-RW but are able to read them
                if(mediaTypes.Count == 0 || mediaTypes.Contains("CD-ROM"))
                {
                    if(!mediaTypes.Contains("CD-ROM"))
                        mediaTypes.Add("CD-ROM");
                    if(!mediaTypes.Contains("Audio CD"))
                        mediaTypes.Add("Audio CD");
                    if(!mediaTypes.Contains("CD-R"))
                        mediaTypes.Add("CD-R");
                    if(!mediaTypes.Contains("CD-RW"))
                        mediaTypes.Add("CD-RW");
                }

                mediaTypes.Sort();
                List<testedMediaType> mediaTests = new List<testedMediaType>();
                foreach(string mediaType in mediaTypes)
                {
                    pressedKey = new ConsoleKeyInfo();
                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                    {
                        DicConsole.Write("Do you have a {0} disc that you can insert in the drive? (Y/N): ", mediaType);
                        pressedKey = System.Console.ReadKey();
                        DicConsole.WriteLine();
                    }

                    if(pressedKey.Key == ConsoleKey.Y)
                    {
                        dev.AllowMediumRemoval(out senseBuffer, timeout, out duration);
                        dev.EjectTray(out senseBuffer, timeout, out duration);
                        DicConsole.WriteLine("Please insert it in the drive and press any key when it is ready.");
                        System.Console.ReadKey(true);

                        testedMediaType mediaTest = new testedMediaType();
                        mediaTest.MediumTypeName = mediaType;
                        mediaTest.MediaIsRecognized = true;

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

                                    mediaTest.MediaIsRecognized &= !sense;
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

                                    mediaTest.MediaIsRecognized &= !sense;
                                }
                                else
                                    mediaTest.MediaIsRecognized = false;
                            }
                            else
                                mediaTest.MediaIsRecognized = false;
                        }

                        if(mediaTest.MediaIsRecognized)
                        {
                            mediaTest.SupportsReadCapacitySpecified = true;
                            mediaTest.SupportsReadCapacity16Specified = true;

                            DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                            sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                mediaTest.SupportsReadCapacity = true;
                                mediaTest.Blocks = (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3])) + 1;
                                mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                                mediaTest.BlocksSpecified = true;
                                mediaTest.BlockSizeSpecified = true;
                            }

                            DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                            sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                mediaTest.SupportsReadCapacity16 = true;
                                byte[] temp = new byte[8];
                                Array.Copy(buffer, 0, temp, 0, 8);
                                Array.Reverse(temp);
                                mediaTest.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                                mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                                mediaTest.BlocksSpecified = true;
                                mediaTest.BlockSizeSpecified = true;
                            }

                            decMode = null;

                            DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                            sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.SupportsModeSense10 = true;
                                decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.SCSIType);
                            }
                            DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                            sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.SupportsModeSense6 = true;
                                if(!decMode.HasValue)
                                    decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.SCSIType);
                            }

                            if(decMode.HasValue)
                            {
                                mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                                mediaTest.MediumTypeSpecified = true;
                                if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                                {
                                    mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                    mediaTest.DensitySpecified = true;
                                }
                            }

                            if(mediaType.StartsWith("CD-") || mediaType == "Audio CD")
                            {
                                mediaTest.CanReadTOCSpecified = true;
                                mediaTest.CanReadFullTOCSpecified = true;
                                DicConsole.WriteLine("Querying CD TOC...");
                                mediaTest.CanReadTOC = !dev.ReadTocPmaAtip(out buffer, out senseBuffer, false, 0, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying CD Full TOC...");
                                mediaTest.CanReadFullTOC = !dev.ReadRawToc(out buffer, out senseBuffer, 1, timeout, out duration);
                            }

                            if(mediaType.StartsWith("CD-R"))
                            {
                                mediaTest.CanReadATIPSpecified = true;
                                mediaTest.CanReadPMASpecified = true;
                                DicConsole.WriteLine("Querying CD ATIP...");
                                mediaTest.CanReadATIP = !dev.ReadAtip(out buffer, out senseBuffer, timeout, out duration);
                                DicConsole.WriteLine("Querying CD PMA...");
                                mediaTest.CanReadPMA = !dev.ReadPma(out buffer, out senseBuffer, timeout, out duration);
                            }

                            if(mediaType.StartsWith("DVD-") || mediaType.StartsWith("HD DVD-"))
                            {
                                mediaTest.CanReadPFISpecified = true;
                                mediaTest.CanReadDMISpecified = true;
                                DicConsole.WriteLine("Querying DVD PFI...");
                                mediaTest.CanReadPFI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PhysicalInformation, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying DVD DMI...");
                                mediaTest.CanReadDMI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DiscManufacturingInformation, 0, timeout, out duration);
                            }

                            if(mediaType == "DVD-ROM")
                            {
                                mediaTest.CanReadCMISpecified = true;
                                DicConsole.WriteLine("Querying DVD CMI...");
                                mediaTest.CanReadCMI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.CopyrightInformation, 0, timeout, out duration);
                            }

                            if(mediaType == "DVD-ROM" || mediaType == "HD DVD-ROM")
                            {
                                mediaTest.CanReadBCASpecified = true;
                                DicConsole.WriteLine("Querying DVD BCA...");
                                mediaTest.CanReadBCA = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.BurstCuttingArea, 0, timeout, out duration);
                                mediaTest.CanReadAACSSpecified = true;
                                DicConsole.WriteLine("Querying DVD AACS...");
                                mediaTest.CanReadAACS = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVD_AACS, 0, timeout, out duration);
                            }

                            if(mediaType == "BD-ROM")
                            {
                                mediaTest.CanReadBCASpecified = true;
                                DicConsole.WriteLine("Querying BD BCA...");
                                mediaTest.CanReadBCA = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_BurstCuttingArea, 0, timeout, out duration);
                            }

                            if(mediaType == "DVD-RAM" || mediaType == "HD DVD-RAM")
                            {
                                mediaTest.CanReadDDSSpecified = true;
                                mediaTest.CanReadSpareAreaInformationSpecified = true;
                                mediaTest.CanReadDDS = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_DDS, 0, timeout, out duration);
                                mediaTest.CanReadSpareAreaInformation = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDRAM_SpareAreaInformation, 0, timeout, out duration);
                            }

                            if(mediaType.StartsWith("BD-R") && mediaType != "BD-ROM")
                            {
                                mediaTest.CanReadDDSSpecified = true;
                                mediaTest.CanReadSpareAreaInformationSpecified = true;
                                DicConsole.WriteLine("Querying BD DDS...");
                                mediaTest.CanReadDDS = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_DDS, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying BD SAI...");
                                mediaTest.CanReadSpareAreaInformation = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.BD_SpareAreaInformation, 0, timeout, out duration);
                            }

                            if(mediaType == "DVD-R" || mediaType == "DVD-RW")
                            {
                                mediaTest.CanReadPRISpecified = true;
                                DicConsole.WriteLine("Querying DVD PRI...");
                                mediaTest.CanReadPRI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.PreRecordedInfo, 0, timeout, out duration);
                            }

                            if(mediaType == "DVD-R" || mediaType == "DVD-RW" || mediaType == "HD DVD-R")
                            {
                                mediaTest.CanReadMediaIDSpecified = true;
                                mediaTest.CanReadRecordablePFISpecified = true;
                                DicConsole.WriteLine("Querying DVD Media ID...");
                                mediaTest.CanReadMediaID = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_MediaIdentifier, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying DVD Embossed PFI...");
                                mediaTest.CanReadRecordablePFI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_PhysicalInformation, 0, timeout, out duration);
                            }

                            if(mediaType.StartsWith("DVD+R"))
                            {
                                mediaTest.CanReadADIPSpecified = true;
                                mediaTest.CanReadDCBSpecified = true;
                                DicConsole.WriteLine("Querying DVD ADIP...");
                                mediaTest.CanReadADIP = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.ADIP, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying DVD DCB...");
                                mediaTest.CanReadDCB = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DCB, 0, timeout, out duration);
                            }

                            if(mediaType == "HD DVD-ROM")
                            {
                                mediaTest.CanReadHDCMISpecified = true;
                                DicConsole.WriteLine("Querying HD DVD CMI...");
                                mediaTest.CanReadHDCMI = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.HDDVD_CopyrightInformation, 0, timeout, out duration);
                            }

                            if(mediaType.EndsWith(" DL"))
                            {
                                mediaTest.CanReadLayerCapacitySpecified = true;
                                DicConsole.WriteLine("Querying DVD Layer Capacity...");
                                mediaTest.CanReadLayerCapacity = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.DVD, 0, 0, MmcDiscStructureFormat.DVDR_LayerCapacity, 0, timeout, out duration);
                            }

                            if(mediaType.StartsWith("BD-R"))
                            {
                                mediaTest.CanReadDiscInformationSpecified = true;
                                mediaTest.CanReadPACSpecified = true;
                                DicConsole.WriteLine("Querying BD Disc Information...");
                                mediaTest.CanReadDiscInformation = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.DiscInformation, 0, timeout, out duration);
                                DicConsole.WriteLine("Querying BD PAC...");
                                mediaTest.CanReadPAC = !dev.ReadDiscStructure(out buffer, out senseBuffer, MmcDiscStructureMediaType.BD, 0, 0, MmcDiscStructureFormat.PAC, 0, timeout, out duration);
                            }

                            mediaTest.SupportsReadSpecified = true;
                            mediaTest.SupportsRead10Specified = true;
                            mediaTest.SupportsRead12Specified = true;
                            mediaTest.SupportsRead16Specified = true;

                            DicConsole.WriteLine("Trying SCSI READ (6)...");
                            mediaTest.SupportsRead = !dev.Read6(out buffer, out senseBuffer, 0, 2048, timeout, out duration);
                            DicConsole.WriteLine("Trying SCSI READ (10)...");
                            mediaTest.SupportsRead10 = !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0, 2048, 0, 1, timeout, out duration);
                            DicConsole.WriteLine("Trying SCSI READ (12)...");
                            mediaTest.SupportsRead12 = !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0, 2048, 0, 1, false, timeout, out duration);
                            DicConsole.WriteLine("Trying SCSI READ (16)...");
                            mediaTest.SupportsRead16 = !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0, 2048, 0, 1, false, timeout, out duration);

                            if(options.Debug)
                            {
                                if(!tryPlextor)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.Write("Do you have want to try Plextor vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryPlextor |= pressedKey.Key == ConsoleKey.Y;
                                }
                            }

                            if(mediaType.StartsWith("CD-") || mediaType == "Audio CD")
                            {
                                mediaTest.CanReadC2PointersSpecified = true;
                                mediaTest.CanReadCorrectedSubchannelSpecified = true;
                                mediaTest.CanReadCorrectedSubchannelWithC2Specified = true;
                                mediaTest.CanReadLeadInSpecified = true;
                                mediaTest.CanReadLeadOutSpecified = true;
                                mediaTest.CanReadPQSubchannelSpecified = true;
                                mediaTest.CanReadPQSubchannelWithC2Specified = true;
                                mediaTest.CanReadRWSubchannelSpecified = true;
                                mediaTest.CanReadRWSubchannelWithC2Specified = true;
                                mediaTest.SupportsReadCdMsfSpecified = true;
                                mediaTest.SupportsReadCdSpecified = true;
                                mediaTest.SupportsReadCdMsfRawSpecified = true;
                                mediaTest.SupportsReadCdRawSpecified = true;

                                if(mediaType == "Audio CD")
                                {
                                    DicConsole.WriteLine("Trying SCSI READ CD...");
                                    mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                    DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                                    mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2352, MmcSectorTypes.CDDA, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                }
                                else
                                {
                                    DicConsole.WriteLine("Trying SCSI READ CD...");
                                    mediaTest.SupportsReadCd = !dev.ReadCd(out buffer, out senseBuffer, 0, 2048, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                    DicConsole.WriteLine("Trying SCSI READ CD MSF...");
                                    mediaTest.SupportsReadCdMsf = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2048, MmcSectorTypes.AllTypes, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                    DicConsole.WriteLine("Trying SCSI READ CD full sector...");
                                    mediaTest.SupportsReadCdRaw = !dev.ReadCd(out buffer, out senseBuffer, 0, 2352, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                    DicConsole.WriteLine("Trying SCSI READ CD MSF full sector...");
                                    mediaTest.SupportsReadCdMsfRaw = !dev.ReadCdMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2352, MmcSectorTypes.AllTypes, false, false, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                }

                                if(mediaTest.SupportsReadCdRaw || mediaType == "Audio CD")
                                {
                                    DicConsole.WriteLine("Trying to read CD Lead-In...");
                                    for(int i = -150; i < 0; i++)
                                    {
                                        if(mediaType == "Audio CD")
                                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                        else
                                            sense = dev.ReadCd(out buffer, out senseBuffer, (uint)i, 2352, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                        if(!sense)
                                        {
                                            mediaTest.CanReadLeadIn = true;
                                            break;
                                        }
                                    }

                                    DicConsole.WriteLine("Trying to read CD Lead-Out...");
                                    if(mediaType == "Audio CD")
                                        mediaTest.CanReadLeadOut = dev.ReadCd(out buffer, out senseBuffer, (uint)(mediaTest.Blocks + 1), 2352, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                    else
                                        mediaTest.CanReadLeadOut = !dev.ReadCd(out buffer, out senseBuffer, (uint)(mediaTest.Blocks + 1), 2352, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None, timeout, out duration);
                                }

                                if(mediaType == "Audio CD" && mediaTest.SupportsReadCd)
                                {
                                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.None, timeout, out duration);
                                    if(!mediaTest.CanReadC2Pointers)
                                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2648, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.None, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels...");
                                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Q16, timeout, out duration);
                                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Raw, timeout, out duration);
                                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.RW, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.Q16, timeout, out duration);
                                    if(!mediaTest.CanReadPQSubchannelWithC2)
                                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2664, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Q16, timeout, out duration);

                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.Raw, timeout, out duration);
                                    if(!mediaTest.CanReadRWSubchannelWithC2)
                                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Raw, timeout, out duration);

                                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.RW, timeout, out duration);
                                    if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.CDDA, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.RW, timeout, out duration);
                                }
                                else if(mediaTest.SupportsReadCdRaw)
                                {
                                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2Pointers, MmcSubchannel.None, timeout, out duration);
                                    if(!mediaTest.CanReadC2Pointers)
                                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2646, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2PointersAndBlock, MmcSubchannel.None, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels...");
                                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2368, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16, timeout, out duration);
                                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw, timeout, out duration);
                                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2448, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.RW, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2662, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2Pointers, MmcSubchannel.Q16, timeout, out duration);
                                    if(!mediaTest.CanReadPQSubchannelWithC2)
                                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2664, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Q16, timeout, out duration);

                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2Pointers, MmcSubchannel.Raw, timeout, out duration);
                                    if(!mediaTest.CanReadRWSubchannelWithC2)
                                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Raw, timeout, out duration);

                                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2712, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2Pointers, MmcSubchannel.RW, timeout, out duration);
                                    if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2714, 1, MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.C2PointersAndBlock, MmcSubchannel.RW, timeout, out duration);
                                }
                                else
                                {
                                    DicConsole.WriteLine("Trying to read C2 Pointers...");
                                    mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2342, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.None, timeout, out duration);
                                    if(!mediaTest.CanReadC2Pointers)
                                        mediaTest.CanReadC2Pointers = !dev.ReadCd(out buffer, out senseBuffer, 0, 2344, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.None, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels...");
                                    mediaTest.CanReadPQSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2064, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Q16, timeout, out duration);
                                    mediaTest.CanReadRWSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Raw, timeout, out duration);
                                    mediaTest.CanReadCorrectedSubchannel = !dev.ReadCd(out buffer, out senseBuffer, 0, 2144, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.RW, timeout, out duration);

                                    DicConsole.WriteLine("Trying to read subchannels with C2 Pointers...");
                                    mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2358, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.Q16, timeout, out duration);
                                    if(!mediaTest.CanReadPQSubchannelWithC2)
                                        mediaTest.CanReadPQSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2360, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Q16, timeout, out duration);

                                    mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2438, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.Raw, timeout, out duration);
                                    if(!mediaTest.CanReadRWSubchannelWithC2)
                                        mediaTest.CanReadRWSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2440, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.Raw, timeout, out duration);

                                    mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2438, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2Pointers, MmcSubchannel.RW, timeout, out duration);
                                    if(!mediaTest.CanReadCorrectedSubchannelWithC2)
                                        mediaTest.CanReadCorrectedSubchannelWithC2 = !dev.ReadCd(out buffer, out senseBuffer, 0, 2440, 1, MmcSectorTypes.AllTypes, false, false, false, MmcHeaderCodes.None, true, false, MmcErrorField.C2PointersAndBlock, MmcSubchannel.RW, timeout, out duration);
                                }

                                if(options.Debug)
                                {
                                    if(!tryNEC)
                                    {
                                        pressedKey = new ConsoleKeyInfo();
                                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                        {
                                            DicConsole.Write("Do you have want to try NEC vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                            pressedKey = System.Console.ReadKey();
                                            DicConsole.WriteLine();
                                        }

                                        tryNEC |= pressedKey.Key == ConsoleKey.Y;
                                    }

                                    if(!tryPioneer)
                                    {
                                        pressedKey = new ConsoleKeyInfo();
                                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                        {
                                            DicConsole.Write("Do you have want to try Pioneer vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                            pressedKey = System.Console.ReadKey();
                                            DicConsole.WriteLine();
                                        }

                                        tryPioneer |= pressedKey.Key == ConsoleKey.Y;
                                    }
                                }

                                if(tryPlextor)
                                {
                                    mediaTest.SupportsPlextorReadCDDASpecified = true;
                                    DicConsole.WriteLine("Trying Plextor READ CD-DA...");
                                    mediaTest.SupportsPlextorReadCDDA = !dev.PlextorReadCdDa(out buffer, out senseBuffer, 0, 2352, 1, PlextorSubchannel.None, timeout, out duration);
                                }

                                if(tryPioneer)
                                {
                                    mediaTest.SupportsPioneerReadCDDASpecified = true;
                                    mediaTest.SupportsPioneerReadCDDAMSFSpecified = true;
                                    DicConsole.WriteLine("Trying Pioneer READ CD-DA...");
                                    mediaTest.SupportsPioneerReadCDDA = !dev.PioneerReadCdDa(out buffer, out senseBuffer, 0, 2352, 1, PioneerSubchannel.None, timeout, out duration);
                                    DicConsole.WriteLine("Trying Pioneer READ CD-DA MSF...");
                                    mediaTest.SupportsPioneerReadCDDAMSF = !dev.PioneerReadCdDaMsf(out buffer, out senseBuffer, 0x00000200, 0x00000201, 2352, PioneerSubchannel.None, timeout, out duration);
                                }

                                if(tryNEC)
                                {
                                    mediaTest.SupportsNECReadCDDASpecified = true;
                                    DicConsole.WriteLine("Trying NEC READ CD-DA...");
                                    mediaTest.SupportsNECReadCDDA = !dev.NecReadCdDa(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                                }
                            }

                            mediaTest.LongBlockSize = mediaTest.BlockSize;
                            DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout, out duration);
                            if(sense && !dev.Error)
                            {
                                Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                                if(decSense.HasValue)
                                {
                                    if(decSense.Value.SenseKey == DiscImageChef.Decoders.SCSI.SenseKeys.IllegalRequest &&
                                        decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                    {
                                        mediaTest.SupportsReadLong = true;
                                        if(decSense.Value.InformationValid && decSense.Value.ILI)
                                        {
                                            mediaTest.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                }
                            }

                            if(options.Debug)
                            {
                                if(!tryHLDTST)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.Write("Do you have want to try HL-DT-ST (aka LG) vendor commands? THIS IS DANGEROUS AND CAN IRREVERSIBLY DESTROY YOUR DRIVE (IF IN DOUBT PRESS 'N') (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    tryHLDTST |= pressedKey.Key == ConsoleKey.Y;
                                }
                            }

                            if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                            {
                                if(mediaTest.BlockSize == 512)
                                {
                                    // Long sector sizes for 512-byte magneto-opticals
                                    foreach(ushort testSize in new[] { 600, 610, 630 })
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = testSize;
                                            mediaTest.LongBlockSizeSpecified = true;
                                            break;
                                        }
                                    }
                                }
                                else if(mediaTest.BlockSize == 1024)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 1200, timeout, out duration);
                                    if(!sense && !dev.Error)
                                    {
                                        mediaTest.SupportsReadLong = true;
                                        mediaTest.LongBlockSize = 1200;
                                        mediaTest.LongBlockSizeSpecified = true;
                                    }
                                }
                                else if(mediaTest.BlockSize == 2048)
                                {
                                    if(mediaType.StartsWith("DVD"))
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 37856, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 37856;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 2380;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                }
                                else if(mediaTest.BlockSize == 4096)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, timeout, out duration);
                                    if(!sense && !dev.Error)
                                    {
                                        mediaTest.SupportsReadLong = true;
                                        mediaTest.LongBlockSize = 4760;
                                        mediaTest.LongBlockSizeSpecified = true;
                                    }
                                }
                                else if(mediaTest.BlockSize == 8192)
                                {
                                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, timeout, out duration);
                                    if(!sense && !dev.Error)
                                    {
                                        mediaTest.SupportsReadLong = true;
                                        mediaTest.LongBlockSize = 9424;
                                        mediaTest.LongBlockSizeSpecified = true;
                                    }
                                }
                            }

                            if(tryPlextor)
                            {
                                mediaTest.SupportsPlextorReadRawDVDSpecified = true;
                                DicConsole.WriteLine("Trying Plextor trick to raw read DVDs...");
                                mediaTest.SupportsPlextorReadRawDVD = !dev.PlextorReadRawDvd(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                                if(mediaTest.SupportsPlextorReadRawDVD)
                                    mediaTest.SupportsPlextorReadRawDVD = !ArrayHelpers.ArrayIsNullOrEmpty(buffer);
                            }

                            if(tryHLDTST)
                            {
                                mediaTest.SupportsHLDTSTReadRawDVDSpecified = true;
                                DicConsole.WriteLine("Trying HL-DT-ST (aka LG) trick to raw read DVDs...");
                                mediaTest.SupportsHLDTSTReadRawDVD = !dev.HlDtStReadRawDvd(out buffer, out senseBuffer, 0, 1, timeout, out duration);
                            }

                            if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                            {
                                pressedKey = new ConsoleKeyInfo();
                                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                {
                                    DicConsole.Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                                    pressedKey = System.Console.ReadKey();
                                    DicConsole.WriteLine();
                                }

                                if(pressedKey.Key == ConsoleKey.Y)
                                {
                                    for(ushort i = (ushort)mediaTest.BlockSize; i < (ushort)mediaTest.BlockSize * 36; i++)
                                    {
                                        DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i, timeout, out duration);
                                        if(!sense)
                                        {
                                            if(options.Debug)
                                            {
                                                FileStream bingo = new FileStream(string.Format("{0}_readlong.bin", mediaType), FileMode.Create);
                                                bingo.Write(buffer, 0, buffer.Length);
                                                bingo.Close();
                                            }
                                            mediaTest.LongBlockSize = i;
                                            mediaTest.LongBlockSizeSpecified = true;
                                            break;
                                        }
                                    }
                                    DicConsole.WriteLine();
                                }
                            }
                        }
                        mediaTests.Add(mediaTest);
                    }
                    report.SCSI.MultiMediaDevice.TestedMedia = mediaTests.ToArray();
                }
            }
            #endregion MultiMediaDevice
            #region SequentialAccessDevice
            else if(dev.SCSIType == DiscImageChef.Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess)
            {
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
                                    report.SCSI.SequentialDevice.SupportedMediaTypes[i].DensityCodes[j] = (int)mtsh.Value.descriptors[i].densityCodes[j];
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
                            }

                            DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                            sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.SupportsModeSense6 = true;
                                if(!decMode.HasValue)
                                    decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.SCSIType);
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
                                            seqTest.SupportedMediaTypes[i].DensityCodes[j] = (int)mtsh.Value.descriptors[i].densityCodes[j];
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
            #endregion SequentialAccessDevice
            #region OtherDevices
            else
            {
                if(removable)
                {
                    List<testedMediaType> mediaTests = new List<testedMediaType>();

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

                            testedMediaType mediaTest = new testedMediaType();
                            DicConsole.Write("Please write a description of the media type and press enter: ");
                            mediaTest.MediumTypeName = System.Console.ReadLine();
                            DicConsole.Write("Please write the media manufacturer and press enter: ");
                            mediaTest.Manufacturer = System.Console.ReadLine();
                            DicConsole.Write("Please write the media model and press enter: ");
                            mediaTest.Model = System.Console.ReadLine();

                            mediaTest.ManufacturerSpecified = true;
                            mediaTest.ModelSpecified = true;
                            mediaTest.MediaIsRecognized = true;

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

                                        mediaTest.MediaIsRecognized &= !sense;
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

                                        mediaTest.MediaIsRecognized &= !sense;
                                    }
                                    else
                                        mediaTest.MediaIsRecognized = false;
                                }
                                else
                                    mediaTest.MediaIsRecognized = false;
                            }

                            if(mediaTest.MediaIsRecognized)
                            {
                                mediaTest.SupportsReadCapacitySpecified = true;
                                mediaTest.SupportsReadCapacity16Specified = true;

                                DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                                sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity = true;
                                    mediaTest.Blocks = (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3])) + 1;
                                    mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                                    mediaTest.BlocksSpecified = true;
                                    mediaTest.BlockSizeSpecified = true;
                                }

                                DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                                sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    mediaTest.SupportsReadCapacity16 = true;
                                    byte[] temp = new byte[8];
                                    Array.Copy(buffer, 0, temp, 0, 8);
                                    Array.Reverse(temp);
                                    mediaTest.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                                    mediaTest.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                                    mediaTest.BlocksSpecified = true;
                                    mediaTest.BlockSizeSpecified = true;
                                }

                                decMode = null;

                                DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                                sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense10 = true;
                                    decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.SCSIType);
                                }

                                DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                                sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.SupportsModeSense6 = true;
                                    if(!decMode.HasValue)
                                        decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.SCSIType);
                                }

                                if(decMode.HasValue)
                                {
                                    mediaTest.MediumType = (byte)decMode.Value.Header.MediumType;
                                    mediaTest.MediumTypeSpecified = true;
                                    if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                                    {
                                        mediaTest.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                                        mediaTest.DensitySpecified = true;
                                    }
                                }

                                mediaTest.SupportsReadSpecified = true;
                                mediaTest.SupportsRead10Specified = true;
                                mediaTest.SupportsRead12Specified = true;
                                mediaTest.SupportsRead16Specified = true;
                                mediaTest.SupportsReadLongSpecified = true;

                                DicConsole.WriteLine("Trying SCSI READ (6)...");
                                mediaTest.SupportsRead = !dev.Read6(out buffer, out senseBuffer, 0, mediaTest.BlockSize, timeout, out duration);
                                DicConsole.WriteLine("Trying SCSI READ (10)...");
                                mediaTest.SupportsRead10 = !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0, mediaTest.BlockSize, 0, 1, timeout, out duration);
                                DicConsole.WriteLine("Trying SCSI READ (12)...");
                                mediaTest.SupportsRead12 = !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0, mediaTest.BlockSize, 0, 1, false, timeout, out duration);
                                DicConsole.WriteLine("Trying SCSI READ (16)...");
                                mediaTest.SupportsRead16 = !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0, mediaTest.BlockSize, 0, 1, false, timeout, out duration);

                                mediaTest.LongBlockSize = mediaTest.BlockSize;
                                DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout, out duration);
                                if(sense && !dev.Error)
                                {
                                    Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                                    if(decSense.HasValue)
                                    {
                                        if(decSense.Value.SenseKey == DiscImageChef.Decoders.SCSI.SenseKeys.IllegalRequest &&
                                            decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            if(decSense.Value.InformationValid && decSense.Value.ILI)
                                            {
                                                mediaTest.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                                mediaTest.LongBlockSizeSpecified = true;
                                            }
                                        }
                                    }
                                }

                                if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                                {
                                    if(mediaTest.BlockSize == 512)
                                    {
                                        // Long sector sizes for 512-byte magneto-opticals
                                        foreach(ushort testSize in new[] { 600, 610, 630 })
                                        {
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, timeout, out duration);
                                            if(!sense && !dev.Error)
                                            {
                                                mediaTest.SupportsReadLong = true;
                                                mediaTest.LongBlockSize = testSize;
                                                mediaTest.LongBlockSizeSpecified = true;
                                                break;
                                            }
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 1024)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 1200, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 1200;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 2048)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 2380;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 4096)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 4760;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                    else if(mediaTest.BlockSize == 8192)
                                    {
                                        sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, timeout, out duration);
                                        if(!sense && !dev.Error)
                                        {
                                            mediaTest.SupportsReadLong = true;
                                            mediaTest.LongBlockSize = 9424;
                                            mediaTest.LongBlockSizeSpecified = true;
                                        }
                                    }
                                }

                                if(mediaTest.SupportsReadLong && mediaTest.LongBlockSize == mediaTest.BlockSize)
                                {
                                    pressedKey = new ConsoleKeyInfo();
                                    while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                                    {
                                        DicConsole.Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                                        pressedKey = System.Console.ReadKey();
                                        DicConsole.WriteLine();
                                    }

                                    if(pressedKey.Key == ConsoleKey.Y)
                                    {
                                        for(ushort i = (ushort)mediaTest.BlockSize; i < (ushort)mediaTest.BlockSize * 36; i++)
                                        {
                                            DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, i, timeout, out duration);
                                            if(!sense)
                                            {
                                                if(options.Debug)
                                                {
                                                    FileStream bingo = new FileStream(string.Format("{0}_readlong.bin", mediaTest.MediumTypeName), FileMode.Create);
                                                    bingo.Write(buffer, 0, buffer.Length);
                                                    bingo.Close();
                                                }
                                                mediaTest.LongBlockSize = i;
                                                mediaTest.LongBlockSizeSpecified = true;
                                                break;
                                            }
                                        }
                                        DicConsole.WriteLine();
                                    }
                                }

                                mediaTest.CanReadMediaSerialSpecified = true;
                                DicConsole.WriteLine("Trying SCSI READ MEDIA SERIAL NUMBER...");
                                mediaTest.CanReadMediaSerial = !dev.ReadMediaSerialNumber(out buffer, out senseBuffer, timeout, out duration);
                            }
                            mediaTests.Add(mediaTest);
                        }
                    }
                    report.SCSI.RemovableMedias = mediaTests.ToArray();
                }
                else
                {
                    report.SCSI.ReadCapabilities = new testedMediaType();
                    report.SCSI.ReadCapabilitiesSpecified = true;
                    report.SCSI.ReadCapabilities.MediaIsRecognized = true;

                    report.SCSI.ReadCapabilities.SupportsReadCapacitySpecified = true;
                    report.SCSI.ReadCapabilities.SupportsReadCapacity16Specified = true;

                    DicConsole.WriteLine("Querying SCSI READ CAPACITY...");
                    sense = dev.ReadCapacity(out buffer, out senseBuffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.ReadCapabilities.SupportsReadCapacity = true;
                        report.SCSI.ReadCapabilities.Blocks = (ulong)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + (buffer[3])) + 1;
                        report.SCSI.ReadCapabilities.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                        report.SCSI.ReadCapabilities.BlocksSpecified = true;
                        report.SCSI.ReadCapabilities.BlockSizeSpecified = true;
                    }

                    DicConsole.WriteLine("Querying SCSI READ CAPACITY (16)...");
                    sense = dev.ReadCapacity16(out buffer, out buffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.ReadCapabilities.SupportsReadCapacity16 = true;
                        byte[] temp = new byte[8];
                        Array.Copy(buffer, 0, temp, 0, 8);
                        Array.Reverse(temp);
                        report.SCSI.ReadCapabilities.Blocks = BitConverter.ToUInt64(temp, 0) + 1;
                        report.SCSI.ReadCapabilities.BlockSize = (uint)((buffer[5] << 24) + (buffer[5] << 16) + (buffer[6] << 8) + (buffer[7]));
                        report.SCSI.ReadCapabilities.BlocksSpecified = true;
                        report.SCSI.ReadCapabilities.BlockSizeSpecified = true;
                    }

                    decMode = null;

                    DicConsole.WriteLine("Querying SCSI MODE SENSE (10)...");
                    sense = dev.ModeSense10(out buffer, out senseBuffer, false, true, ScsiModeSensePageControl.Current, 0x3F, 0x00, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.SupportsModeSense10 = true;
                        decMode = Decoders.SCSI.Modes.DecodeMode10(buffer, dev.SCSIType);
                    }

                    DicConsole.WriteLine("Querying SCSI MODE SENSE...");
                    sense = dev.ModeSense(out buffer, out senseBuffer, timeout, out duration);
                    if(!sense && !dev.Error)
                    {
                        report.SCSI.SupportsModeSense6 = true;
                        if(!decMode.HasValue)
                            decMode = Decoders.SCSI.Modes.DecodeMode6(buffer, dev.SCSIType);
                    }

                    if(decMode.HasValue)
                    {
                        report.SCSI.ReadCapabilities.MediumType = (byte)decMode.Value.Header.MediumType;
                        report.SCSI.ReadCapabilities.MediumTypeSpecified = true;
                        if(decMode.Value.Header.BlockDescriptors != null && decMode.Value.Header.BlockDescriptors.Length > 0)
                        {
                            report.SCSI.ReadCapabilities.Density = (byte)decMode.Value.Header.BlockDescriptors[0].Density;
                            report.SCSI.ReadCapabilities.DensitySpecified = true;
                        }
                    }

                    report.SCSI.ReadCapabilities.SupportsReadSpecified = true;
                    report.SCSI.ReadCapabilities.SupportsRead10Specified = true;
                    report.SCSI.ReadCapabilities.SupportsRead12Specified = true;
                    report.SCSI.ReadCapabilities.SupportsRead16Specified = true;
                    report.SCSI.ReadCapabilities.SupportsReadLongSpecified = true;

                    DicConsole.WriteLine("Trying SCSI READ (6)...");
                    report.SCSI.ReadCapabilities.SupportsRead = !dev.Read6(out buffer, out senseBuffer, 0, report.SCSI.ReadCapabilities.BlockSize, timeout, out duration);
                    DicConsole.WriteLine("Trying SCSI READ (10)...");
                    report.SCSI.ReadCapabilities.SupportsRead10 = !dev.Read10(out buffer, out senseBuffer, 0, false, true, false, false, 0, report.SCSI.ReadCapabilities.BlockSize, 0, 1, timeout, out duration);
                    DicConsole.WriteLine("Trying SCSI READ (12)...");
                    report.SCSI.ReadCapabilities.SupportsRead12 = !dev.Read12(out buffer, out senseBuffer, 0, false, true, false, false, 0, report.SCSI.ReadCapabilities.BlockSize, 0, 1, false, timeout, out duration);
                    DicConsole.WriteLine("Trying SCSI READ (16)...");
                    report.SCSI.ReadCapabilities.SupportsRead16 = !dev.Read16(out buffer, out senseBuffer, 0, false, true, false, 0, report.SCSI.ReadCapabilities.BlockSize, 0, 1, false, timeout, out duration);

                    report.SCSI.ReadCapabilities.LongBlockSize = report.SCSI.ReadCapabilities.BlockSize;
                    DicConsole.WriteLine("Trying SCSI READ LONG (10)...");
                    sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 0xFFFF, timeout, out duration);
                    if(sense && !dev.Error)
                    {
                        Decoders.SCSI.FixedSense? decSense = Decoders.SCSI.Sense.DecodeFixed(senseBuffer);
                        if(decSense.HasValue)
                        {
                            if(decSense.Value.SenseKey == DiscImageChef.Decoders.SCSI.SenseKeys.IllegalRequest &&
                                decSense.Value.ASC == 0x24 && decSense.Value.ASCQ == 0x00)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                if(decSense.Value.InformationValid && decSense.Value.ILI)
                                {
                                    report.SCSI.ReadCapabilities.LongBlockSize = 0xFFFF - (decSense.Value.Information & 0xFFFF);
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                }
                            }
                        }
                    }

                    if(report.SCSI.ReadCapabilities.SupportsReadLong && report.SCSI.ReadCapabilities.LongBlockSize == report.SCSI.ReadCapabilities.BlockSize)
                    {
                        if(report.SCSI.ReadCapabilities.BlockSize == 512)
                        {
                            // Long sector sizes for 512-byte magneto-opticals
                            foreach(ushort testSize in new[] { 600, 610, 630 })
                            {
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, testSize, timeout, out duration);
                                if(!sense && !dev.Error)
                                {
                                    report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                    report.SCSI.ReadCapabilities.LongBlockSize = testSize;
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                    break;
                                }
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 1024)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 1200, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 1200;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 2048)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 2380, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 2380;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 4096)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 4760, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 4760;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                        else if(report.SCSI.ReadCapabilities.BlockSize == 8192)
                        {
                            sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, 9424, timeout, out duration);
                            if(!sense && !dev.Error)
                            {
                                report.SCSI.ReadCapabilities.SupportsReadLong = true;
                                report.SCSI.ReadCapabilities.LongBlockSize = 9424;
                                report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                            }
                        }
                    }

                    if(report.SCSI.ReadCapabilities.SupportsReadLong && report.SCSI.ReadCapabilities.LongBlockSize == report.SCSI.ReadCapabilities.BlockSize)
                    {
                        pressedKey = new ConsoleKeyInfo();
                        while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                        {
                            DicConsole.Write("Drive supports SCSI READ LONG but I cannot find the correct size. Do you want me to try? (This can take hours) (Y/N): ");
                            pressedKey = System.Console.ReadKey();
                            DicConsole.WriteLine();
                        }

                        if(pressedKey.Key == ConsoleKey.Y)
                        {
                            for(ushort i = (ushort)report.SCSI.ReadCapabilities.BlockSize; i < (ushort)report.SCSI.ReadCapabilities.BlockSize * 36; i++)
                            {
                                DicConsole.Write("\rTrying to READ LONG with a size of {0} bytes...", i);
                                sense = dev.ReadLong10(out buffer, out senseBuffer, false, false, 0, (ushort)i, timeout, out duration);
                                if(!sense)
                                {
                                    if(options.Debug)
                                    {
                                        FileStream bingo = new FileStream(string.Format("{0}_readlong.bin", dev.Model), FileMode.Create);
                                        bingo.Write(buffer, 0, buffer.Length);
                                        bingo.Close();
                                    }
                                    report.SCSI.ReadCapabilities.LongBlockSize = i;
                                    report.SCSI.ReadCapabilities.LongBlockSizeSpecified = true;
                                    break;
                                }
                            }
                            DicConsole.WriteLine();
                        }
                    }
                }
            }
            #endregion OtherDevices

            FileStream xmlFs = new FileStream(xmlFile, FileMode.Create);

            System.Xml.Serialization.XmlSerializer xmlSer = new System.Xml.Serialization.XmlSerializer(typeof(Metadata.DeviceReport));
            xmlSer.Serialize(xmlFs, report);
            xmlFs.Close();

            if(Settings.Settings.Current.SaveReportsGlobally && !String.IsNullOrEmpty(Settings.Settings.ReportsPath))
            {
                xmlFs = new FileStream(Path.Combine(Settings.Settings.ReportsPath, xmlFile), FileMode.Create);
                xmlSer.Serialize(xmlFs, report);
                xmlFs.Close();
            }

            if(Settings.Settings.Current.ShareReports)
            {
                SubmitReport(xmlSer);
            }
        }

        static void SubmitReport(System.Xml.Serialization.XmlSerializer xmlSer)
        {
            // TODO: Implement this
        }
    }
}

