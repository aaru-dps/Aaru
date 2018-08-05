// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Database model for ATA/ATAPI device information.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Database.Models
{
    public class ATA : BaseEntity
    {
        public string                              AdditionalPID                { get; set; }
        public Identify.TransferMode?              APIOSupported                { get; set; }
        public ushort?                             ATAPIByteCount               { get; set; }
        public ushort?                             BufferType                   { get; set; }
        public ushort?                             BufferSize                   { get; set; }
        public Identify.CapabilitiesBit?           Capabilities                 { get; set; }
        public Identify.CapabilitiesBit2?          Capabilities2                { get; set; }
        public Identify.CapabilitiesBit3?          Capabilities3                { get; set; }
        public ushort?                             CFAPowerMode                 { get; set; }
        public Identify.CommandSetBit?             CommandSet                   { get; set; }
        public Identify.CommandSetBit2?            CommandSet2                  { get; set; }
        public Identify.CommandSetBit3?            CommandSet3                  { get; set; }
        public Identify.CommandSetBit4?            CommandSet4                  { get; set; }
        public Identify.CommandSetBit5?            CommandSet5                  { get; set; }
        public byte?                               CurrentAAM                   { get; set; }
        public ushort?                             CurrentAPM                   { get; set; }
        public Identify.DataSetMgmtBit?            DataSetMgmt                  { get; set; }
        public ushort?                             DataSetMgmtSize              { get; set; }
        public Identify.DeviceFormFactorEnum?      DeviceFormFactor             { get; set; }
        public Identify.TransferMode?              DMAActive                    { get; set; }
        public Identify.TransferMode?              DMASupported                 { get; set; }
        public byte?                               DMATransferTimingMode        { get; set; }
        public ushort?                             EnhancedSecurityEraseTime    { get; set; }
        public Identify.CommandSetBit?             EnabledCommandSet            { get; set; }
        public Identify.CommandSetBit2?            EnabledCommandSet2           { get; set; }
        public Identify.CommandSetBit3?            EnabledCommandSet3           { get; set; }
        public Identify.CommandSetBit4?            EnabledCommandSet4           { get; set; }
        public Identify.SATAFeaturesBit?           EnabledSATAFeatures          { get; set; }
        public ulong?                              ExtendedUserSectors          { get; set; }
        public byte?                               FreeFallSensitivity          { get; set; }
        public string                              FirmwareRevision             { get; set; }
        public Identify.GeneralConfigurationBit?   GeneralConfiguration         { get; set; }
        public ushort?                             HardwareResetResult          { get; set; }
        public ushort?                             InterseekDelay               { get; set; }
        public Identify.MajorVersionBit?           MajorVersion                 { get; set; }
        public ushort?                             MasterPasswordRevisionCode   { get; set; }
        public ushort?                             MaxDownloadMicroMode3        { get; set; }
        public ushort?                             MaxQueueDepth                { get; set; }
        public Identify.TransferMode?              MDMAActive                   { get; set; }
        public Identify.TransferMode?              MDMASupported                { get; set; }
        public ushort?                             MinDownloadMicroMode3        { get; set; }
        public ushort?                             MinMDMACycleTime             { get; set; }
        public ushort?                             MinorVersion                 { get; set; }
        public ushort?                             MinPIOCycleTimeNoFlow        { get; set; }
        public ushort?                             MinPIOCycleTimeFlow          { get; set; }
        public string                              Model                        { get; set; }
        public byte?                               MultipleMaxSectors           { get; set; }
        public byte?                               MultipleSectorNumber         { get; set; }
        public ushort?                             NVCacheCaps                  { get; set; }
        public uint?                               NVCacheSize                  { get; set; }
        public ushort?                             NVCacheWriteSpeed            { get; set; }
        public byte?                               NVEstimatedSpinUp            { get; set; }
        public ushort?                             PacketBusRelease             { get; set; }
        public byte?                               PIOTransferTimingMode        { get; set; }
        public byte?                               RecommendedAAM               { get; set; }
        public ushort?                             RecommendedMDMACycleTime     { get; set; }
        public ushort?                             RemovableStatusSet           { get; set; }
        public Identify.SATACapabilitiesBit?       SATACapabilities             { get; set; }
        public Identify.SATACapabilitiesBit2?      SATACapabilities2            { get; set; }
        public Identify.SATAFeaturesBit?           SATAFeatures                 { get; set; }
        public Identify.SCTCommandTransportBit?    SCTCommandTransport          { get; set; }
        public uint?                               SectorsPerCard               { get; set; }
        public ushort?                             SecurityEraseTime            { get; set; }
        public Identify.SecurityStatusBit?         SecurityStatus               { get; set; }
        public ushort?                             ServiceBusyClear             { get; set; }
        public Identify.SpecificConfigurationEnum? SpecificConfiguration        { get; set; }
        public ushort?                             StreamAccessLatency          { get; set; }
        public ushort?                             StreamMinReqSize             { get; set; }
        public uint?                               StreamPerformanceGranularity { get; set; }
        public ushort?                             StreamTransferTimeDMA        { get; set; }
        public ushort?                             StreamTransferTimePIO        { get; set; }
        public ushort?                             TransportMajorVersion        { get; set; }
        public ushort?                             TransportMinorVersion        { get; set; }
        public Identify.TrustedComputingBit?       TrustedComputing             { get; set; }
        public Identify.TransferMode?              UDMAActive                   { get; set; }
        public Identify.TransferMode?              UDMASupported                { get; set; }
        public byte?                               WRVMode                      { get; set; }
        public uint?                               WRVSectorCountMode3          { get; set; }
        public uint?                               WRVSectorCountMode2          { get; set; }
        public byte[]                              Identify                     { get; set; }
        public TestedMedia                         ReadCapabilities             { get; set; }
        public List<TestedMedia>                   RemovableMedias              { get; set; }

        public static ATA MapAta(ataType oldAta)
        {
            if(oldAta == null) return null;

            ATA newAta = new ATA
            {
                Identify         = oldAta.Identify,
                ReadCapabilities = TestedMedia.MapTestedMedia(oldAta.ReadCapabilities)
            };
            if(oldAta.AdditionalPIDSpecified) newAta.AdditionalPID                 = oldAta.AdditionalPID;
            if(oldAta.APIOSupportedSpecified) newAta.APIOSupported                 = oldAta.APIOSupported;
            if(oldAta.ATAPIByteCountSpecified) newAta.ATAPIByteCount               = oldAta.ATAPIByteCount;
            if(oldAta.BufferTypeSpecified) newAta.BufferType                       = oldAta.BufferType;
            if(oldAta.BufferSizeSpecified) newAta.BufferSize                       = oldAta.BufferSize;
            if(oldAta.CapabilitiesSpecified) newAta.Capabilities                   = oldAta.Capabilities;
            if(oldAta.Capabilities2Specified) newAta.Capabilities2                 = oldAta.Capabilities2;
            if(oldAta.Capabilities3Specified) newAta.Capabilities3                 = oldAta.Capabilities3;
            if(oldAta.CFAPowerModeSpecified) newAta.CFAPowerMode                   = oldAta.CFAPowerMode;
            if(oldAta.CommandSetSpecified) newAta.CommandSet                       = oldAta.CommandSet;
            if(oldAta.CommandSet2Specified) newAta.CommandSet2                     = oldAta.CommandSet2;
            if(oldAta.CommandSet3Specified) newAta.CommandSet3                     = oldAta.CommandSet3;
            if(oldAta.CommandSet4Specified) newAta.CommandSet4                     = oldAta.CommandSet4;
            if(oldAta.CommandSet5Specified) newAta.CommandSet5                     = oldAta.CommandSet5;
            if(oldAta.CurrentAAMSpecified) newAta.CurrentAAM                       = oldAta.CurrentAAM;
            if(oldAta.CurrentAPMSpecified) newAta.CurrentAPM                       = oldAta.CurrentAPM;
            if(oldAta.DataSetMgmtSpecified) newAta.DataSetMgmt                     = oldAta.DataSetMgmt;
            if(oldAta.DataSetMgmtSizeSpecified) newAta.DataSetMgmtSize             = oldAta.DataSetMgmtSize;
            if(oldAta.DeviceFormFactorSpecified) newAta.DeviceFormFactor           = oldAta.DeviceFormFactor;
            if(oldAta.DMAActiveSpecified) newAta.DMAActive                         = oldAta.DMAActive;
            if(oldAta.DMASupportedSpecified) newAta.DMASupported                   = oldAta.DMASupported;
            if(oldAta.DMATransferTimingModeSpecified) newAta.DMATransferTimingMode = oldAta.DMATransferTimingMode;
            if(oldAta.EnhancedSecurityEraseTimeSpecified)
                newAta.EnhancedSecurityEraseTime = oldAta.EnhancedSecurityEraseTime;
            if(oldAta.EnabledCommandSetSpecified) newAta.EnabledCommandSet       = oldAta.EnabledCommandSet;
            if(oldAta.EnabledCommandSet2Specified) newAta.EnabledCommandSet2     = oldAta.EnabledCommandSet2;
            if(oldAta.EnabledCommandSet3Specified) newAta.EnabledCommandSet3     = oldAta.EnabledCommandSet3;
            if(oldAta.EnabledCommandSet4Specified) newAta.EnabledCommandSet4     = oldAta.EnabledCommandSet4;
            if(oldAta.EnabledSATAFeaturesSpecified) newAta.EnabledSATAFeatures   = oldAta.EnabledSATAFeatures;
            if(oldAta.ExtendedUserSectorsSpecified) newAta.ExtendedUserSectors   = oldAta.ExtendedUserSectors;
            if(oldAta.FreeFallSensitivitySpecified) newAta.FreeFallSensitivity   = oldAta.FreeFallSensitivity;
            if(oldAta.FirmwareRevisionSpecified) newAta.FirmwareRevision         = oldAta.FirmwareRevision;
            if(oldAta.GeneralConfigurationSpecified) newAta.GeneralConfiguration = oldAta.GeneralConfiguration;
            if(oldAta.HardwareResetResultSpecified) newAta.HardwareResetResult   = oldAta.HardwareResetResult;
            if(oldAta.InterseekDelaySpecified) newAta.InterseekDelay             = oldAta.InterseekDelay;
            if(oldAta.MajorVersionSpecified) newAta.MajorVersion                 = oldAta.MajorVersion;
            if(oldAta.MasterPasswordRevisionCodeSpecified)
                newAta.MasterPasswordRevisionCode = oldAta.MasterPasswordRevisionCode;
            if(oldAta.MaxDownloadMicroMode3Specified) newAta.MaxDownloadMicroMode3 = oldAta.MaxDownloadMicroMode3;
            if(oldAta.MaxQueueDepthSpecified) newAta.MaxQueueDepth                 = oldAta.MaxQueueDepth;
            if(oldAta.MDMAActiveSpecified) newAta.MDMAActive                       = oldAta.MDMAActive;
            if(oldAta.MDMASupportedSpecified) newAta.MDMASupported                 = oldAta.MDMASupported;
            if(oldAta.MinDownloadMicroMode3Specified) newAta.MinDownloadMicroMode3 = oldAta.MinDownloadMicroMode3;
            if(oldAta.MinMDMACycleTimeSpecified) newAta.MinMDMACycleTime           = oldAta.MinMDMACycleTime;
            if(oldAta.MinorVersionSpecified) newAta.MinorVersion                   = oldAta.MinorVersion;
            if(oldAta.MinPIOCycleTimeNoFlowSpecified) newAta.MinPIOCycleTimeNoFlow = oldAta.MinPIOCycleTimeNoFlow;
            if(oldAta.MinPIOCycleTimeFlowSpecified) newAta.MinPIOCycleTimeFlow     = oldAta.MinPIOCycleTimeFlow;
            if(oldAta.ModelSpecified) newAta.Model                                 = oldAta.Model;
            if(oldAta.MultipleMaxSectorsSpecified) newAta.MultipleMaxSectors       = oldAta.MultipleMaxSectors;
            if(oldAta.MultipleSectorNumberSpecified) newAta.MultipleSectorNumber   = oldAta.MultipleSectorNumber;
            if(oldAta.NVCacheCapsSpecified) newAta.NVCacheCaps                     = oldAta.NVCacheCaps;
            if(oldAta.NVCacheSizeSpecified) newAta.NVCacheSize                     = oldAta.NVCacheSize;
            if(oldAta.NVCacheWriteSpeedSpecified) newAta.NVCacheWriteSpeed         = oldAta.NVCacheWriteSpeed;
            if(oldAta.NVEstimatedSpinUpSpecified) newAta.NVEstimatedSpinUp         = oldAta.NVEstimatedSpinUp;
            if(oldAta.PacketBusReleaseSpecified) newAta.PacketBusRelease           = oldAta.PacketBusRelease;
            if(oldAta.PIOTransferTimingModeSpecified) newAta.PIOTransferTimingMode = oldAta.PIOTransferTimingMode;
            if(oldAta.RecommendedAAMSpecified) newAta.RecommendedAAM               = oldAta.RecommendedAAM;
            if(oldAta.RecommendedMDMACycleTimeSpecified)
                newAta.RecommendedMDMACycleTime = oldAta.RecommendedMDMACycleTime;
            if(oldAta.RemovableStatusSetSpecified) newAta.RemovableStatusSet       = oldAta.RemovableStatusSet;
            if(oldAta.SATACapabilitiesSpecified) newAta.SATACapabilities           = oldAta.SATACapabilities;
            if(oldAta.SATACapabilities2Specified) newAta.SATACapabilities2         = oldAta.SATACapabilities2;
            if(oldAta.SATAFeaturesSpecified) newAta.SATAFeatures                   = oldAta.SATAFeatures;
            if(oldAta.SCTCommandTransportSpecified) newAta.SCTCommandTransport     = oldAta.SCTCommandTransport;
            if(oldAta.SectorsPerCardSpecified) newAta.SectorsPerCard               = oldAta.SectorsPerCard;
            if(oldAta.SecurityEraseTimeSpecified) newAta.SecurityEraseTime         = oldAta.SecurityEraseTime;
            if(oldAta.SecurityStatusSpecified) newAta.SecurityStatus               = oldAta.SecurityStatus;
            if(oldAta.ServiceBusyClearSpecified) newAta.ServiceBusyClear           = oldAta.ServiceBusyClear;
            if(oldAta.SpecificConfigurationSpecified) newAta.SpecificConfiguration = oldAta.SpecificConfiguration;
            if(oldAta.StreamAccessLatencySpecified) newAta.StreamAccessLatency     = oldAta.StreamAccessLatency;
            if(oldAta.StreamMinReqSizeSpecified) newAta.StreamMinReqSize           = oldAta.StreamMinReqSize;
            if(oldAta.StreamPerformanceGranularitySpecified)
                newAta.StreamPerformanceGranularity = oldAta.StreamPerformanceGranularity;
            if(oldAta.StreamTransferTimeDMASpecified) newAta.StreamTransferTimeDMA = oldAta.StreamTransferTimeDMA;
            if(oldAta.StreamTransferTimePIOSpecified) newAta.StreamTransferTimePIO = oldAta.StreamTransferTimePIO;
            if(oldAta.TransportMajorVersionSpecified) newAta.TransportMajorVersion = oldAta.TransportMajorVersion;
            if(oldAta.TransportMinorVersionSpecified) newAta.TransportMinorVersion = oldAta.TransportMinorVersion;
            if(oldAta.TrustedComputingSpecified) newAta.TrustedComputing           = oldAta.TrustedComputing;
            if(oldAta.UDMAActiveSpecified) newAta.UDMAActive                       = oldAta.UDMAActive;
            if(oldAta.UDMASupportedSpecified) newAta.UDMASupported                 = oldAta.UDMASupported;
            if(oldAta.WRVModeSpecified) newAta.WRVMode                             = oldAta.WRVMode;
            if(oldAta.WRVSectorCountMode3Specified) newAta.WRVSectorCountMode3     = oldAta.WRVSectorCountMode3;
            if(oldAta.WRVSectorCountMode2Specified) newAta.WRVSectorCountMode2     = oldAta.WRVSectorCountMode2;

            if(oldAta.RemovableMedias == null) return newAta;

            newAta.RemovableMedias = new List<TestedMedia>(oldAta.RemovableMedias.Select(TestedMedia.MapTestedMedia));

            return newAta;
        }
    }
}