// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATAPI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from ATAPI devices.
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

using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report
{
    public static class ATAPI
    {
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            if(report == null) return;

            byte[] buffer;
            double duration;
            uint timeout = 5;

            DicConsole.WriteLine("Querying ATAPI IDENTIFY...");

            Decoders.ATA.AtaErrorRegistersCHS errorRegs;
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
                if(debug) report.ATAPI.Identify = buffer;
            }
        }
    }
}