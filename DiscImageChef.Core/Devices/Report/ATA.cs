// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from ATA devices.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    /// Implements creating a report for an ATA device
    /// </summary>
    public static class Ata
    {
        /// <summary>
        /// Creates a report of an ATA device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="debug">If debug is enabled</param>
        /// <param name="removable">If device is removable</param>
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            if(report == null) return;

            const uint TIMEOUT = 5;

            if(dev.IsUsb) Usb.Report(dev, ref report, debug, ref removable);

            if(dev.IsFireWire) FireWire.Report(dev, ref report, ref removable);

            if(dev.IsPcmcia) Pcmcia.Report(dev, ref report);

            DicConsole.WriteLine("Querying ATA IDENTIFY...");

            dev.AtaIdentify(out byte[] buffer, out _, TIMEOUT, out _);

            if(!Identify.Decode(buffer).HasValue) return;

            Identify.IdentifyDevice? ataIdNullable = Identify.Decode(buffer);
            if(ataIdNullable == null) return;

            Identify.IdentifyDevice ataId = ataIdNullable.Value;

            ConsoleKeyInfo pressedKey;
            if((ushort)ataId.GeneralConfiguration == 0x848A)
            {
                report.CompactFlash = true;
                report.CompactFlashSpecified = true;
                removable = false;
            }
            else if(!removable &&
                    ataId.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable))
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
                dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);
                ataId = Identify.Decode(buffer).Value;
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
            if(debug) report.ATA.Identify = buffer;

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

                    if(pressedKey.Key != ConsoleKey.Y) continue;

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
                    dev.AtaIdentify(out buffer, out _, TIMEOUT, out _);

                    if(Identify.Decode(buffer).HasValue)
                    {
                        ataId = Identify.Decode(buffer).Value;

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
                            mediaTest.CHS = new chsType
                            {
                                Cylinders = ataId.Cylinders,
                                Heads = ataId.Heads,
                                Sectors = ataId.SectorsPerTrack
                            };
                            mediaTest.Blocks = (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                            mediaTest.BlocksSpecified = true;
                        }

                        if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 &&
                           ataId.CurrentSectorsPerTrack > 0)
                        {
                            mediaTest.CurrentCHS = new chsType
                            {
                                Cylinders = ataId.CurrentCylinders,
                                Heads = ataId.CurrentHeads,
                                Sectors = ataId.CurrentSectorsPerTrack
                            };
                            if(mediaTest.Blocks == 0)
                                mediaTest.Blocks =
                                    (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads *
                                            ataId.CurrentSectorsPerTrack);
                            mediaTest.BlocksSpecified = true;
                        }

                        if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                        {
                            mediaTest.LBASectors = ataId.LBASectors;
                            mediaTest.LBASectorsSpecified = true;
                            mediaTest.Blocks = ataId.LBASectors;
                            mediaTest.BlocksSpecified = true;
                        }

                        if(ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                        {
                            mediaTest.LBA48Sectors = ataId.LBA48Sectors;
                            mediaTest.LBA48SectorsSpecified = true;
                            mediaTest.Blocks = ataId.LBA48Sectors;
                            mediaTest.BlocksSpecified = true;
                        }

                        if(ataId.NominalRotationRate != 0x0000 && ataId.NominalRotationRate != 0xFFFF)
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

                        uint logicalsectorsize;
                        uint physicalsectorsize;
                        if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 &&
                           (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                        {
                            if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                                if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                                    logicalsectorsize = 512;
                                else logicalsectorsize = ataId.LogicalSectorWords * 2;
                            else logicalsectorsize = 512;

                            if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                            {
                                physicalsectorsize =
                                    (uint)(logicalsectorsize * ((1 << ataId.PhysLogSectorSize) & 0xF));
                            }
                            else physicalsectorsize = logicalsectorsize;
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

                        if(ataId.UnformattedBPS > logicalsectorsize &&
                           (!mediaTest.LongBlockSizeSpecified || mediaTest.LongBlockSize == 516))
                        {
                            mediaTest.LongBlockSize = ataId.UnformattedBPS;
                            mediaTest.LongBlockSizeSpecified = true;
                        }

                        if(ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet) &&
                           !ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear) &&
                           ataId.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial))
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

                        ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                        bool sense;

                        DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                        sense = dev.Read(out byte[] readBuf, out AtaErrorRegistersChs errorChs, false, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsRead =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorschs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                        sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadRetry =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorsretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                        sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, TIMEOUT,
                                            out _);
                        mediaTest.SupportsReadDma =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmachs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                        sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaRetry =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmaretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying SEEK in CHS mode...");
                        sense = dev.Seek(out errorChs, 0, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsSeek =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                                  errorChs.Status, errorChs.Error);

                        DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                        sense = dev.Read(out readBuf, out AtaErrorRegistersLba28 errorLba, false, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectors",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                        sense = dev.Read(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadRetryLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectorsretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdma",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaRetryLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdmaretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying SEEK in LBA mode...");
                        sense = dev.Seek(out errorLba, 0, TIMEOUT, out _);
                        mediaTest.SupportsSeekLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                                  errorChs.Status, errorChs.Error);

                        DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                        sense = dev.Read(out readBuf, out AtaErrorRegistersLba48 errorLba48, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadLba48 =
                            !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readsectors48",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                        sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, TIMEOUT, out _);
                        mediaTest.SupportsReadDmaLba48 =
                            !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 &&
                            readBuf.Length > 0;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readdma48",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                        sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1, mediaTest.LongBlockSize,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLong =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongchs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                        sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1, mediaTest.LongBlockSize,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLongRetry =
                            !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 &&
                            readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongretrychs",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                        sense = dev.ReadLong(out readBuf, out errorLba, false, 0, mediaTest.LongBlockSize,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLongLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlong",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);

                        DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                        sense = dev.ReadLong(out readBuf, out errorLba, true, 0, mediaTest.LongBlockSize,
                                             TIMEOUT, out _);
                        mediaTest.SupportsReadLongRetryLba =
                            !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 &&
                            readBuf.Length > 0 && BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                        DicConsole.DebugWriteLine("ATA Report",
                                                  "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}",
                                                  sense, errorChs.Status, errorChs.Error, readBuf.Length);
                        if(debug)
                            DataFile.WriteTo("ATA Report", "readlongretry",
                                             "_debug_" + mediaTest.MediumTypeName + ".bin", "read results",
                                             readBuf);
                    }
                    else mediaTest.MediaIsRecognized = false;

                    mediaTests.Add(mediaTest);
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
                    report.ATA.ReadCapabilities.CHS = new chsType
                    {
                        Cylinders = ataId.Cylinders,
                        Heads = ataId.Heads,
                        Sectors = ataId.SectorsPerTrack
                    };
                    report.ATA.ReadCapabilities.Blocks =
                        (ulong)(ataId.Cylinders * ataId.Heads * ataId.SectorsPerTrack);
                    report.ATA.ReadCapabilities.BlocksSpecified = true;
                }

                if(ataId.CurrentCylinders > 0 && ataId.CurrentHeads > 0 && ataId.CurrentSectorsPerTrack > 0)
                {
                    report.ATA.ReadCapabilities.CurrentCHS = new chsType
                    {
                        Cylinders = ataId.CurrentCylinders,
                        Heads = ataId.CurrentHeads,
                        Sectors = ataId.CurrentSectorsPerTrack
                    };
                    report.ATA.ReadCapabilities.Blocks =
                        (ulong)(ataId.CurrentCylinders * ataId.CurrentHeads * ataId.CurrentSectorsPerTrack);
                    report.ATA.ReadCapabilities.BlocksSpecified = true;
                }

                if(ataId.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                {
                    report.ATA.ReadCapabilities.LBASectors = ataId.LBASectors;
                    report.ATA.ReadCapabilities.LBASectorsSpecified = true;
                    report.ATA.ReadCapabilities.Blocks = ataId.LBASectors;
                    report.ATA.ReadCapabilities.BlocksSpecified = true;
                }

                if(ataId.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                {
                    report.ATA.ReadCapabilities.LBA48Sectors = ataId.LBA48Sectors;
                    report.ATA.ReadCapabilities.LBA48SectorsSpecified = true;
                    report.ATA.ReadCapabilities.Blocks = ataId.LBA48Sectors;
                    report.ATA.ReadCapabilities.BlocksSpecified = true;
                }

                if(ataId.NominalRotationRate != 0x0000 && ataId.NominalRotationRate != 0xFFFF)
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

                uint logicalsectorsize;
                uint physicalsectorsize;
                if((ataId.PhysLogSectorSize & 0x8000) == 0x0000 && (ataId.PhysLogSectorSize & 0x4000) == 0x4000)
                {
                    if((ataId.PhysLogSectorSize & 0x1000) == 0x1000)
                        if(ataId.LogicalSectorWords <= 255 || ataId.LogicalAlignment == 0xFFFF)
                            logicalsectorsize = 512;
                        else logicalsectorsize = ataId.LogicalSectorWords * 2;
                    else logicalsectorsize = 512;

                    if((ataId.PhysLogSectorSize & 0x2000) == 0x2000)
                    {
                        physicalsectorsize = logicalsectorsize *
                                             (uint)Math.Pow(2, ataId.PhysLogSectorSize & 0xF);
                    }
                    else physicalsectorsize = logicalsectorsize;
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

                    if((ataId.LogicalAlignment & 0x8000) == 0x0000 && (ataId.LogicalAlignment & 0x4000) == 0x4000)
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

                if(ataId.UnformattedBPS > logicalsectorsize &&
                   (!report.ATA.ReadCapabilities.LongBlockSizeSpecified ||
                    report.ATA.ReadCapabilities.LongBlockSize == 516))
                {
                    report.ATA.ReadCapabilities.LongBlockSize = ataId.UnformattedBPS;
                    report.ATA.ReadCapabilities.LongBlockSizeSpecified = true;
                }

                if(ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet) &&
                   !ataId.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear) &&
                   ataId.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial))
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

                ulong checkCorrectRead = BitConverter.ToUInt64(buffer, 0);
                bool sense;

                DicConsole.WriteLine("Trying READ SECTOR(S) in CHS mode...");
                sense = dev.Read(out byte[] readBuf, out AtaErrorRegistersChs errorChs, false, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsRead =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorschs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in CHS mode...");
                sense = dev.Read(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorsretrychs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in CHS mode...");
                sense = dev.ReadDma(out readBuf, out errorChs, false, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDma =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmachs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA RETRY in CHS mode...");
                sense = dev.ReadDma(out readBuf, out errorChs, true, 0, 0, 1, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmaretrychs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying SEEK in CHS mode...");
                sense = dev.Seek(out errorChs, 0, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsSeek =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0;
                DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                          errorChs.Status, errorChs.Error);

                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA mode...");
                sense = dev.Read(out readBuf, out AtaErrorRegistersLba28 errorLba, false, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectors", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ SECTOR(S) RETRY in LBA mode...");
                sense = dev.Read(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectorsretry", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in LBA mode...");
                sense = dev.ReadDma(out readBuf, out errorLba, false, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdma", "_debug_" + report.ATA.Model + ".bin", "read results",
                                     readBuf);

                DicConsole.WriteLine("Trying READ DMA RETRY in LBA mode...");
                sense = dev.ReadDma(out readBuf, out errorLba, true, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdmaretry", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying SEEK in LBA mode...");
                sense = dev.Seek(out errorLba, 0, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsSeekLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0;
                DicConsole.DebugWriteLine("ATA Report", "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}", sense,
                                          errorLba.Status, errorLba.Error);

                DicConsole.WriteLine("Trying READ SECTOR(S) in LBA48 mode...");
                sense = dev.Read(out readBuf, out AtaErrorRegistersLba48 errorLba48, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLba48 =
                    !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba48.Status, errorLba48.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readsectors48", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ DMA in LBA48 mode...");
                sense = dev.ReadDma(out readBuf, out errorLba48, 0, 1, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadDmaLba48 =
                    !sense && (errorLba48.Status & 0x01) != 0x01 && errorLba48.Error == 0 && readBuf.Length > 0;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba48.Status, errorLba48.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readdma48", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG in CHS mode...");
                sense = dev.ReadLong(out readBuf, out errorChs, false, 0, 0, 1,
                                     report.ATA.ReadCapabilities.LongBlockSize, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLong =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongchs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG RETRY in CHS mode...");
                sense = dev.ReadLong(out readBuf, out errorChs, true, 0, 0, 1,
                                     report.ATA.ReadCapabilities.LongBlockSize, TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongRetry =
                    !sense && (errorChs.Status & 0x01) != 0x01 && errorChs.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorChs.Status, errorChs.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongretrychs", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG in LBA mode...");
                sense = dev.ReadLong(out readBuf, out errorLba, false, 0, report.ATA.ReadCapabilities.LongBlockSize,
                                     TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlong", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);

                DicConsole.WriteLine("Trying READ LONG RETRY in LBA mode...");
                sense = dev.ReadLong(out readBuf, out errorLba, true, 0, report.ATA.ReadCapabilities.LongBlockSize,
                                     TIMEOUT, out _);
                report.ATA.ReadCapabilities.SupportsReadLongRetryLba =
                    !sense && (errorLba.Status & 0x01) != 0x01 && errorLba.Error == 0 && readBuf.Length > 0 &&
                    BitConverter.ToUInt64(readBuf, 0) != checkCorrectRead;
                DicConsole.DebugWriteLine("ATA Report",
                                          "Sense = {0}, Status = 0x{1:X2}, Error = 0x{2:X2}, Length = {3}", sense,
                                          errorLba.Status, errorLba.Error, readBuf.Length);
                if(debug)
                    DataFile.WriteTo("ATA Report", "readlongretry", "_debug_" + report.ATA.Model + ".bin",
                                     "read results", readBuf);
            }
        }
    }
}