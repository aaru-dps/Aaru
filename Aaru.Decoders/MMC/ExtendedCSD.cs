// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ExtendedCSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MultiMediaCard extended CSD.
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

// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Aaru.Decoders.MMC;

[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class ExtendedCSD
{
    public byte                            AccessSize;
    public byte                            BackgroundOperationsStatus;
    public BackgroundOperationsSupport     BackgroundOperationsSupport;
    public byte                            BadBlockManagementMode;
    public byte                            BarrierControl;
    public byte                            BarrierSupport;
    public BootAreaWriteProtectionRegister BootAreaWriteProtectionRegister;
    public byte                            BootBusConditions;
    public BootConfigProtection            BootConfigProtection;
    public BootInformation                 BootInformation;
    public byte                            BootPartitionSize;
    public byte                            BootWriteProtectionStatus;
    public byte                            BusWidth;
    public byte                            CacheControl;
    public byte                            CacheFlushing;
    public CacheFlushingPolicy             CacheFlushingPolicy;
    public uint                            CacheSize;
    public byte                            Class6CommandsControl;
    public byte                            CMDQueuingDepth;
    public CMDQueuingSupport               CMDQueuingSupport;
    public byte                            CommandQueueModeEnable;
    public byte                            CommandSet;
    public byte                            CommandSetRevision;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public byte[] ContextConfiguration;
    public byte           ContextManagementCaps;
    public uint           CorrectlyProgrammedSectors;
    public DataTagSupport DataTagSupport;
    public byte           DeviceLifeEstimationTypeA;
    public byte           DeviceLifeEstimationTypeB;
    public DeviceType     DeviceType;
    public ushort         DeviceVersion;
    public DriverStrength DriverStrength;
    public byte           DyncapNeeded;
    public byte           EnableBackgroundOperationsHandshake;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] EnhancedUserDataAreaSize;
    public uint                      EnhancedUserDataStartAddress;
    public byte                      ErasedMemoryContent;
    public ushort                    ExceptionEventsControl;
    public ushort                    ExceptionEventsStatus;
    public ushort                    ExtendedPartitionsAttribute;
    public ExtendedPartitionsSupport ExtendedPartitionsSupport;
    public byte                      ExtendedSecurityCommandsError;
    public uint                      FFUArgument;
    public FFUFeatures               FFUFeatures;
    public byte                      FFUStatus;
    public byte                      FirmwareConfiguration;
    public ulong                     FirmwareVersion;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] GeneralPurposePartitionSize;
    public byte                             GenericCMD6Timeout;
    public HighCapacityEraseGroupDefinition HighCapacityEraseGroupDefinition;
    public byte                             HighCapacityEraseTimeout;
    public byte                             HighCapacityEraseUnitSize;
    public byte                             HighCapacityWriteProtectGroupSize;
    public byte                             HighSpeedInterfaceTiming;
    public HPIFeatures                      HPIFeatures;
    public byte                             HPIManagement;
    public byte                             HWResetFunction;
    public byte                             InitializationTimeAfterPartition;
    public byte                             InitializationTimeoutAfterEmulationChange;
    public byte                             LargeUnitSize;
    public byte                             ManuallyStartBackgroundOperations;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] MaxEnhancedAreaSize;
    public byte                MaxPackedReadCommands;
    public byte                MaxPackedWriteCommands;
    public uint                MaxPreLoadingDataSize;
    public byte                MinimumReadPerformance26;
    public byte                MinimumReadPerformance26_4;
    public byte                MinimumReadPerformance52;
    public byte                MinimumReadPerformanceDDR52;
    public byte                MinimumWritePerformance26;
    public byte                MinimumWritePerformance26_4;
    public byte                MinimumWritePerformance52;
    public byte                MinimumWritePerformanceDDR52;
    public byte                ModeConfig;
    public byte                ModeOperationCodes;
    public byte                NativeSectorSize;
    public uint                NumberOfFWSectorsCorrectlyProgrammed;
    public byte                OperationCodesTimeout;
    public byte                OptimalReadSize;
    public byte                OptimalTrimUnitSize;
    public byte                OptimalWriteSize;
    public byte                OutOfInterruptBusyTiming;
    public byte                PackageCaseTemperatureControl;
    public byte                PackedCommandFailureIndex;
    public byte                PackedCommandStatus;
    public byte                PartitionConfiguration;
    public byte                PartitioningSetting;
    public PartitioningSupport PartitioningSupport;
    public byte                PartitionsAttribute;
    public byte                PartitionSwitchingTime;
    public byte                PeriodicWakeUp;
    public byte                PowerClass;
    public byte                PowerClass26;
    public byte                PowerClass26_195;
    public byte                PowerClass52;
    public byte                PowerClass52_195;
    public byte                PowerClassDDR200;
    public byte                PowerClassDDR200_130;
    public byte                PowerClassDDR200_195;
    public byte                PowerClassDDR52;
    public byte                PowerClassDDR52_195;
    public byte                PowerOffNotification;
    public byte                PowerOffNotificationTimeout;
    public byte                PreEOLInformation;
    public uint                PreLoadingDataSize;
    public byte                ProductionStateAwareness;
    public byte                ProductionStateAwarenessTimeout;
    public byte                ProductStateAwarenessEnablement;
    public byte                ReliableWriteSectorCount;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public byte[] Reserved0;
    public ushort Reserved1;
    public byte   Reserved10;
    public byte   Reserved11;
    public byte   Reserved12;
    public byte   Reserved13;
    public byte   Reserved14;
    public byte   Reserved15;
    public byte   Reserved16;
    public byte   Reserved17;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 177)]
    public byte[] Reserved18;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] Reserved19;
    public ushort                          Reserved2;
    public byte                            Reserved3;
    public byte                            Reserved4;
    public byte                            Reserved5;
    public byte                            Reserved6;
    public byte                            Reserved7;
    public byte                            Reserved8;
    public byte                            Reserved9;
    public byte                            Revision;
    public byte                            RPMBSize;
    public uint                            SectorCount;
    public byte                            SectorSize;
    public byte                            SectorSizeEmulation;
    public byte                            SecureEraseMultiplier;
    public SecureFeatureSupport            SecureFeatureSupport;
    public byte                            SecureRemovalType;
    public byte                            SecureTRIMMultiplier;
    public SecureWriteProtectInformation   SecureWriteProtectInformation;
    public byte                            SleepAwakeTimeout;
    public byte                            SleepCurrentVcc;
    public byte                            SleepCurrentVccQ;
    public byte                            SleepNotificationTimeout;
    public byte                            StartSanitizeOperation;
    public byte                            StrobeSupport;
    public byte                            Structure;
    public DeviceSupportedCommandSets      SupportedCommandSets;
    public SupportedModes                  SupportedModes;
    public byte                            SupportsProgramCxDInDDR;
    public byte                            TagResourcesSize;
    public byte                            TagUnitSize;
    public byte                            TRIMMultiplier;
    public UserAreaWriteProtectionRegister UserAreaWriteProtectionRegister;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] VendorHealthReport;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] VendorSpecific;
    public byte WriteReliabilityParameterRegister;
    public byte WriteReliabilitySettingRegister;
}

[Flags]
public enum DeviceSupportedCommandSets : byte
{
    Standard = 1 << 0
}

[Flags]
public enum HPIFeatures : byte
{
    Supported = 1 << 0,
    CMD12     = 1 << 1
}

[Flags]
public enum BackgroundOperationsSupport : byte
{
    Supported = 1 << 0
}

[Flags]
public enum DataTagSupport : byte
{
    Supported = 1 << 0
}

[Flags]
public enum ExtendedPartitionsSupport : byte
{
    SystemCode    = 1 << 0,
    NonPersistent = 1 << 1
}

[Flags]
public enum SupportedModes : byte
{
    FFU            = 1 << 0,
    VendorSpecific = 1 << 1
}

[Flags]
public enum FFUFeatures : byte
{
    SupportedModeOperationCodes = 1 << 0
}

[Flags]
public enum CMDQueuingSupport : byte
{
    Supported = 1 << 0
}

[Flags]
public enum CacheFlushingPolicy : byte
{
    FIFO = 1 << 0
}

[Flags]
public enum SecureFeatureSupport : byte
{
    Purge     = 1 << 0,
    Defective = 1 << 2,
    Trim      = 1 << 4,
    Sanitize  = 1 << 6
}

[Flags]
public enum BootInformation : byte
{
    Alternative = 1 << 0,
    DDR         = 1 << 1,
    HighSpeed   = 1 << 2
}

[Flags]
public enum SecureWriteProtectInformation : byte
{
    Supported = 1 << 0,
    Enabled   = 1 << 1
}

[Flags]
public enum DriverStrength : byte
{
    Type0 = 1 << 0,
    Type1 = 1 << 1,
    Type2 = 1 << 2,
    Type3 = 1 << 3,
    Type4 = 1 << 4
}

[Flags]
public enum DeviceType : byte
{
    HS_26        = 1 << 0,
    HS_52        = 1 << 1,
    HS_DDR_52    = 1 << 2,
    HS_DDR_52_LV = 1 << 3,
    HS200_18     = 1 << 4,
    HS200_12     = 1 << 5,
    HS400_18     = 1 << 6,
    HS400_12     = 1 << 7
}

[Flags]
public enum BootConfigProtection : byte
{
    PowerCycle = 1 << 0,
    Permanent  = 1 << 4
}

[Flags]
public enum HighCapacityEraseGroupDefinition : byte
{
    Enabled = 1 << 0
}

[Flags]
public enum BootAreaWriteProtectionRegister : byte
{
    PowerOn          = 1 << 0,
    PowerOnArea2     = 1 << 1,
    Permanent        = 1 << 2,
    PermanentArea2   = 1 << 3,
    PermanentDisable = 1 << 4,
    PowerOnDisable   = 1 << 6,
    Selected         = 1 << 7
}

[Flags]
public enum UserAreaWriteProtectionRegister : byte
{
    ApplyPowerOn        = 1 << 0,
    ApplyPermanent      = 1 << 2,
    DisablePowerOn      = 1 << 3,
    DisablePermanent    = 1 << 4,
    DisableWriteProtect = 1 << 6,
    DisablePassword     = 1 << 7
}

[Flags]
public enum PartitioningSupport : byte
{
    Supported = 1 << 0,
    Enhanced  = 1 << 1,
    Extended  = 1 << 2
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Decoders
{
    public static ExtendedCSD DecodeExtendedCSD(byte[] response)
    {
        if(response?.Length != 512)
            return null;

        var handle = GCHandle.Alloc(response, GCHandleType.Pinned);
        var csd    = (ExtendedCSD)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ExtendedCSD));
        handle.Free();

        return csd;
    }

    public static string PrettifyExtendedCSD(ExtendedCSD csd)
    {
        if(csd == null)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine(Localization.MultiMediaCard_Extended_Device_Specific_Data_Register);

        double unit;

        if(csd.ExtendedSecurityCommandsError != 0)
        {
            sb.AppendFormat("\t" + Localization.Last_extended_security_error_was_0, csd.ExtendedSecurityCommandsError).
               AppendLine();
        }

        if(csd.SupportedCommandSets.HasFlag(DeviceSupportedCommandSets.Standard))
            sb.AppendLine("\t" + Localization.Device_supports_standard_MMC_command_set);

        if(((int)csd.SupportedCommandSets & 0xFE) != 0)
            sb.AppendFormat("\t" + Localization.Device_supports_unknown_command_sets_0, (int)csd.SupportedCommandSets);

        if(csd.HPIFeatures.HasFlag(HPIFeatures.Supported))
        {
            sb.AppendLine(csd.HPIFeatures.HasFlag(HPIFeatures.CMD12)
                              ? "\t" + Localization.Device_implements_HPI_using_CMD12
                              : "\t" + Localization.Device_implements_HPI_using_CMD13);
        }

        if(csd.BackgroundOperationsSupport.HasFlag(BackgroundOperationsSupport.Supported))
            sb.AppendLine("\t" + Localization.Device_supports_background_operations);

        sb.AppendFormat("\t" + Localization.Device_supports_a_maximum_of_0_packed_reads_and_1_packed_writes,
                        csd.MaxPackedReadCommands, csd.MaxPackedWriteCommands).
           AppendLine();

        if(csd.DataTagSupport.HasFlag(DataTagSupport.Supported))
        {
            sb.AppendLine("\t" + Localization.Device_supports_Data_Tag);

            sb.AppendFormat("\t" + Localization.Tags_must_be_in_units_of_0_sectors, Math.Pow(2, csd.TagUnitSize)).
               AppendLine();

            sb.AppendFormat("\t" + Localization.Tag_resources_size_is_0, csd.TagResourcesSize).AppendLine();
        }

        if(csd.ContextManagementCaps != 0)
        {
            sb.AppendFormat("\t" + Localization.Max_context_ID_is_0, csd.ContextManagementCaps & 0xF).AppendLine();

            sb.AppendFormat("\t"                                      + Localization.Large_unit_maximum_multiplier_is_0,
                            ((csd.ContextManagementCaps & 0x70) >> 4) + 1).
               AppendLine();
        }

        sb.AppendFormat("\t" + Localization.Large_unit_size_is_0_MiB, csd.LargeUnitSize + 1).AppendLine();

        if(csd.ExtendedPartitionsSupport.HasFlag(ExtendedPartitionsSupport.NonPersistent))
            sb.AppendLine("\t" + Localization.Device_supports_non_persistent_extended_partitions);

        if(csd.ExtendedPartitionsSupport.HasFlag(ExtendedPartitionsSupport.SystemCode))
            sb.AppendLine("\t" + Localization.Device_supports_system_code_extended_partitions);

        if(csd.SupportedModes.HasFlag(SupportedModes.FFU))
        {
            sb.AppendLine("\t" + Localization.Device_supports_FFU);

            if(csd.FFUFeatures.HasFlag(FFUFeatures.SupportedModeOperationCodes))

                // todo public byte ModeOperationCodes

            {
                if(csd.OperationCodesTimeout > 0)
                {
                    unit = Math.Pow(2, csd.OperationCodesTimeout) * 100;

                    switch(unit)
                    {
                        case > 1000000:
                            sb.
                                AppendFormat("\t" + Localization.Maximum_timeout_for_switch_command_when_setting_a_value_to_the_mode_operation_codes_field_is_0_s,
                                             unit / 1000000).
                                AppendLine();

                            break;
                        case > 1000:
                            sb.
                                AppendFormat("\t" + Localization.Maximum_timeout_for_switch_command_when_setting_a_value_to_the_mode_operation_codes_field_is_0_ms,
                                             unit / 1000).
                                AppendLine();

                            break;
                        default:
                            sb.
                                AppendFormat("\t" + Localization.Maximum_timeout_for_switch_command_when_setting_a_value_to_the_mode_operation_codes_field_is_0_µs,
                                             unit).
                                AppendLine();

                            break;
                    }
                }
            }
        }

        if(csd.SupportedModes.HasFlag(SupportedModes.VendorSpecific))
            sb.AppendLine("\t" + Localization.Device_supports_Vendor_Specific_Mode);

        if(csd.BarrierSupport == 0x01)
            sb.AppendLine("\t" + Localization.Device_supports_the_barrier_command);

        if(csd.CMDQueuingSupport.HasFlag(CMDQueuingSupport.Supported))
        {
            sb.AppendFormat("\t"                + Localization.Device_supports_command_queuing_with_a_depth_of_0,
                            csd.CMDQueuingDepth + 1).
               AppendLine();
        }

        sb.AppendFormat("\t" + Localization._0_firmware_sectors_correctly_programmed,
                        csd.NumberOfFWSectorsCorrectlyProgrammed).
           AppendLine();

        switch(csd.DeviceLifeEstimationTypeB)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Device_used_between_zero_and_10_of_its_estimated_life_time);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_used_between_10_and_20_of_its_estimated_life_time);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_used_between_20_and_30_of_its_estimated_life_time);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.Device_used_between_30_and_40_of_its_estimated_life_time);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Device_used_between_40_and_50_of_its_estimated_life_time);

                break;
            case 6:
                sb.AppendLine("\t" + Localization.Device_used_between_50_and_60_of_its_estimated_life_time);

                break;
            case 7:
                sb.AppendLine("\t" + Localization.Device_used_between_60_and_70_of_its_estimated_life_time);

                break;
            case 8:
                sb.AppendLine("\t" + Localization.Device_used_between_70_and_80_of_its_estimated_life_time);

                break;
            case 9:
                sb.AppendLine("\t" + Localization.Device_used_between_80_and_90_of_its_estimated_life_time);

                break;
            case 10:
                sb.AppendLine("\t" + Localization.Device_used_between_90_and_100_of_its_estimated_life_time);

                break;
            case 11:
                sb.AppendLine("\t" + Localization.Device_exceeded_its_maximum_estimated_life_time);

                break;
        }

        switch(csd.DeviceLifeEstimationTypeA)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Device_used_between_zero_and_10_of_its_estimated_life_time);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_used_between_10_and_20_of_its_estimated_life_time);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_used_between_20_and_30_of_its_estimated_life_time);

                break;
            case 4:
                sb.AppendLine("\t" + Localization.Device_used_between_30_and_40_of_its_estimated_life_time);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Device_used_between_40_and_50_of_its_estimated_life_time);

                break;
            case 6:
                sb.AppendLine("\t" + Localization.Device_used_between_50_and_60_of_its_estimated_life_time);

                break;
            case 7:
                sb.AppendLine("\t" + Localization.Device_used_between_60_and_70_of_its_estimated_life_time);

                break;
            case 8:
                sb.AppendLine("\t" + Localization.Device_used_between_70_and_80_of_its_estimated_life_time);

                break;
            case 9:
                sb.AppendLine("\t" + Localization.Device_used_between_80_and_90_of_its_estimated_life_time);

                break;
            case 10:
                sb.AppendLine("\t" + Localization.Device_used_between_90_and_100_of_its_estimated_life_time);

                break;
            case 11:
                sb.AppendLine("\t" + Localization.Device_exceeded_its_maximum_estimated_life_time);

                break;
        }

        switch(csd.PreEOLInformation)
        {
            case 1:
                sb.AppendLine("\t" + Localization.Device_informs_its_in_good_health);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_informs_it_should_be_replaced_soon);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_informs_it_should_be_replace_immediately);

                break;
        }

        if(csd.OptimalReadSize == 0)
            sb.AppendLine("\t" + Localization.Device_does_not_report_an_optimal_read_size);
        else
            sb.AppendFormat("\t" + Localization.Optimal_read_size_is_0_KiB, 4 * csd.OptimalReadSize).AppendLine();

        if(csd.OptimalWriteSize == 0)
            sb.AppendLine("\t" + Localization.Device_does_not_report_an_optimal_write_size);
        else
            sb.AppendFormat("\t" + Localization.Optimal_write_size_is_0_KiB, 4 * csd.OptimalWriteSize).AppendLine();

        if(csd.OptimalTrimUnitSize == 0)
            sb.AppendLine("\t" + Localization.Device_does_not_report_an_optimal_trim_size);
        else
        {
            sb.AppendFormat("\t" + Localization.Optimal_trim_size_is_0_KiB,
                            4 * Math.Pow(2, csd.OptimalTrimUnitSize - 1)).
               AppendLine();
        }

        sb.AppendFormat("\t" + Localization.Device_version_0,   csd.DeviceVersion).AppendLine();
        sb.AppendFormat("\t" + Localization.Firmware_version_0, csd.FirmwareVersion).AppendLine();

        if(csd.CacheSize == 0)
            sb.AppendLine("\t" + Localization.Device_has_no_cache);
        else
            sb.AppendFormat("\t" + Localization.Device_has_0_KiB_of_cache, csd.CacheSize / 8).AppendLine();

        if(csd.GenericCMD6Timeout > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_by_default_for_a_SWITCH_command,
                            csd.GenericCMD6Timeout * 10).
               AppendLine();
        }

        if(csd.PowerOffNotificationTimeout > 0)
        {
            sb.
                AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_by_default_to_power_off_from_a_SWITCH_command_notification,
                             csd.PowerOffNotificationTimeout * 10).
                AppendLine();
        }

        switch(csd.BackgroundOperationsStatus & 0x03)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_has_no_pending_background_operations);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_has_non_critical_operations_outstanding);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_has_performance_impacted_operations_outstanding);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_has_critical_operations_outstanding);

                break;
        }

        sb.AppendFormat("\t" + Localization.Last_WRITE_MULTIPLE_command_correctly_programmed_0_sectors,
                        csd.CorrectlyProgrammedSectors).
           AppendLine();

        if(csd.InitializationTimeAfterPartition > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_for_initialization_after_partition,
                            csd.InitializationTimeAfterPartition * 100).
               AppendLine();
        }

        if(csd.CacheFlushingPolicy.HasFlag(CacheFlushingPolicy.FIFO))
            sb.AppendLine("\t" + Localization.Device_uses_a_FIFO_policy_for_cache_flushing);

        if(csd.TRIMMultiplier > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_for_trimming_a_single_erase_group,
                            csd.TRIMMultiplier * 300).
               AppendLine();
        }

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Sanitize))
            sb.AppendLine("\t" + Localization.Device_supports_the_sanitize_operation);

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Trim))
            sb.AppendLine("\t" + Localization.Device_supports_supports_the_secure_and_insecure_trim_operations);

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Defective))
            sb.AppendLine("\t" + Localization.Device_supports_automatic_erase_on_retired_defective_blocks);

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Purge))
            sb.AppendLine("\t" + Localization.Device_supports_secure_purge_operations);

        if(csd.SecureEraseMultiplier > 0)
        {
            sb.
                AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_for_securely_erasing_a_single_erase_group,
                             csd.SecureEraseMultiplier * 300).
                AppendLine();
        }

        if(csd.SecureTRIMMultiplier > 0)
        {
            sb.
                AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_for_securely_trimming_a_single_erase_group,
                             csd.SecureTRIMMultiplier * 300).
                AppendLine();
        }

        if(csd.BootInformation.HasFlag(BootInformation.HighSpeed))
            sb.AppendLine("\t" + Localization.Device_supports_high_speed_timing_on_boot);

        if(csd.BootInformation.HasFlag(BootInformation.DDR))
            sb.AppendLine("\t" + Localization.Device_supports_dual_data_rate_on_boot);

        if(csd.BootInformation.HasFlag(BootInformation.Alternative))
            sb.AppendLine("\t" + Localization.Device_supports_alternative_boot_method);

        if(csd.BootPartitionSize > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_has_a_0_KiB_boot_partition, csd.BootPartitionSize * 128).
               AppendLine();
        }

        if((csd.AccessSize & 0x0F) > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_has_a_page_size_of_0_KiB,
                            512 * Math.Pow(2, (csd.AccessSize & 0x0F) - 1) / 1024.0).
               AppendLine();
        }

        if(csd.HighCapacityEraseUnitSize > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_erase_groups_are_0_KiB, csd.HighCapacityEraseUnitSize * 512).
               AppendLine();
        }

        if(csd.HighCapacityEraseTimeout > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_for_erasing_a_single_erase_group,
                            csd.HighCapacityEraseTimeout * 300).
               AppendLine();
        }

        if(csd.HighCapacityWriteProtectGroupSize > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_smallest_write_protect_group_is_made_of_0_erase_groups,
                            csd.HighCapacityWriteProtectGroupSize).
               AppendLine();
        }

        if(csd.SleepCurrentVcc > 0)
        {
            unit = Math.Pow(2, csd.SleepCurrentVcc);

            if(unit > 1000)
                sb.AppendFormat("\t" + Localization.Device_uses_0_mA_on_Vcc_when_sleeping, unit / 1000).AppendLine();
            else
                sb.AppendFormat("\t" + Localization.Device_uses_0_μA_on_Vcc_when_sleeping, unit).AppendLine();
        }

        if(csd.SleepCurrentVccQ > 0)
        {
            unit = Math.Pow(2, csd.SleepCurrentVccQ);

            if(unit > 1000)
                sb.AppendFormat("\t" + Localization.Device_uses_0_mA_on_Vccq_when_sleeping, unit / 1000).AppendLine();
            else
                sb.AppendFormat("\t" + Localization.Device_uses_0_μA_on_Vccq_when_sleeping, unit).AppendLine();
        }

        if(csd.ProductionStateAwarenessTimeout > 0)
        {
            unit = Math.Pow(2, csd.ProductionStateAwareness) * 100;

            switch(unit)
            {
                case > 1000000:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_s_to_switch_production_state_awareness,
                                     unit / 1000000).
                        AppendLine();

                    break;
                case > 1000:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_to_switch_production_state_awareness,
                                     unit / 1000).
                        AppendLine();

                    break;
                default:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_μs_to_switch_production_state_awareness,
                                     unit).
                        AppendLine();

                    break;
            }
        }

        if(csd.SleepAwakeTimeout > 0)
        {
            unit = Math.Pow(2, csd.SleepAwakeTimeout) * 100;

            switch(unit)
            {
                case > 1000000:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_to_transition_between_sleep_and_standby_states,
                                     unit / 1000000).
                        AppendLine();

                    break;
                case > 1000:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_μs_to_transition_between_sleep_and_standby_states,
                                     unit / 1000).
                        AppendLine();

                    break;
                default:
                    sb.
                        AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ns_to_transition_between_sleep_and_standby_states,
                                     unit).
                        AppendLine();

                    break;
            }
        }

        if(csd.SleepNotificationTimeout > 0)
        {
            unit = Math.Pow(2, csd.SleepNotificationTimeout) * 10;

            switch(unit)
            {
                case > 1000000:
                    sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_s_to_move_to_sleep_state,
                                    unit / 1000000).
                       AppendLine();

                    break;
                case > 1000:
                    sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_ms_to_move_to_sleep_state,
                                    unit / 1000).
                       AppendLine();

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Device_takes_a_maximum_of_0_μs_to_move_to_sleep_state, unit).
                       AppendLine();

                    break;
            }
        }

        sb.AppendFormat("\t" + Localization.Device_has_0_sectors, csd.SectorCount).AppendLine();

        if(csd.SecureWriteProtectInformation.HasFlag(SecureWriteProtectInformation.Supported))
        {
            sb.AppendLine("\t" + Localization.Device_supports_secure_write_protection);

            if(csd.SecureWriteProtectInformation.HasFlag(SecureWriteProtectInformation.Enabled))
                sb.AppendLine("\t" + Localization.Device_has_secure_write_protection_enabled);
        }

        unit = csd.MinimumReadPerformance26 * 300;

        if(csd.MinimumReadPerformance26 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_reading_in_SDR_26Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_reading_in_SDR_26Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumReadPerformance26_4 * 300;

        if(csd.MinimumReadPerformance26_4 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_reading_in_SDR_26Mhz_4_bit_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_reading_in_SDR_26Mhz_4_bit_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumReadPerformance52 * 300;

        if(csd.MinimumReadPerformance52 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_reading_in_SDR_52Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_reading_in_SDR_52Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumReadPerformanceDDR52 * 600;

        if(csd.MinimumReadPerformanceDDR52 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_4_8MB_s_reading_in_DDR_52Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_reading_in_DDR_52Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumWritePerformance26 * 300;

        if(csd.MinimumWritePerformance26 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_writing_in_SDR_26Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_writing_in_SDR_26Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumWritePerformance26_4 * 300;

        if(csd.MinimumWritePerformance26_4 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_writing_in_SDR_26Mhz_4_bit_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_writing_in_SDR_26Mhz_4_bit_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumWritePerformance52 * 300;

        if(csd.MinimumWritePerformance52 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_2_4MB_s_writing_in_SDR_52Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_writing_in_SDR_52Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        unit = csd.MinimumWritePerformanceDDR52 * 600;

        if(csd.MinimumWritePerformanceDDR52 == 0)
            sb.AppendLine("\t" + Localization.Device_cannot_achieve_4_8MB_s_writing_in_DDR_52Mhz_mode);
        else
        {
            sb.AppendFormat("\t" + Localization.Device_can_achieve_a_minimum_of_0_MB_s_writing_in_DDR_52Mhz_mode,
                            unit / 1000).
               AppendLine();
        }

        if(csd.PartitionSwitchingTime > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_can_take_a_maximum_of_0_ms_when_switching_partitions,
                            csd.PartitionSwitchingTime * 10).
               AppendLine();
        }

        if(csd.OutOfInterruptBusyTiming > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_can_take_a_maximum_of_0_ms_when_releasing_from_an_interrupt,
                            csd.OutOfInterruptBusyTiming * 10).
               AppendLine();
        }

        if(csd.DriverStrength.HasFlag(DriverStrength.Type0))
            sb.AppendLine("\t" + Localization.Device_supports_IO_driver_strength_type_zero);

        if(csd.DriverStrength.HasFlag(DriverStrength.Type1))
            sb.AppendLine("\t" + Localization.Device_supports_IO_driver_strength_type_one);

        if(csd.DriverStrength.HasFlag(DriverStrength.Type2))
            sb.AppendLine("\t" + Localization.Device_supports_IO_driver_strength_type_two);

        if(csd.DriverStrength.HasFlag(DriverStrength.Type3))
            sb.AppendLine("\t" + Localization.Device_supports_IO_driver_strength_type_three);

        if(csd.DriverStrength.HasFlag(DriverStrength.Type4))
            sb.AppendLine("\t" + Localization.Device_supports_IO_driver_strength_type_four);

        if(csd.DeviceType.HasFlag(DeviceType.HS_26))
            sb.AppendLine("\t" + Localization.Device_supports_26_Mhz_mode);

        if(csd.DeviceType.HasFlag(DeviceType.HS_52))
            sb.AppendLine("\t" + Localization.Device_supports_52_Mhz_mode);

        if(csd.DeviceType.HasFlag(DeviceType.HS_DDR_52))
            sb.AppendLine("\t" + Localization.Device_supports_DDR_52_Mhz_mode_at_1_8V_or_3V);

        if(csd.DeviceType.HasFlag(DeviceType.HS_DDR_52_LV))
            sb.AppendLine("\t" + Localization.Device_supports_DDR_52_Mhz_mode_1_2V);

        if(csd.DeviceType.HasFlag(DeviceType.HS200_18))
            sb.AppendLine("\t" + Localization.Device_supports_HS_200_mode_SDR_200Mhz_at_1_8V);

        if(csd.DeviceType.HasFlag(DeviceType.HS200_12))
            sb.AppendLine("\t" + Localization.Device_supports_HS_200_mode_SDR_200Mhz_at_1_2V);

        if(csd.DeviceType.HasFlag(DeviceType.HS400_18))
            sb.AppendLine("\t" + Localization.Device_supports_HS_400_mode_DDR_200Mhz_at_1_8V);

        if(csd.DeviceType.HasFlag(DeviceType.HS400_12))
            sb.AppendLine("\t" + Localization.Device_supports_HS_400_mode_DDR_200Mhz_at_1_2V);

        sb.AppendFormat("\t" + Localization.CSD_version_one_0_revision_one_1, csd.Structure, csd.Revision).AppendLine();

        switch(csd.CommandSet)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_follows_compatibility_MMC_command_set);

                break;
            case 1:
                switch(csd.CommandSetRevision)
                {
                    case 0:
                        sb.AppendLine("\t" + Localization.Device_follows_standard_MMC_command_set_v4_0);

                        break;
                    default:
                        sb.
                            AppendFormat("\t" + Localization.Device_follows_standard_MMC_command_set_with_unknown_version_code_0,
                                         csd.CommandSetRevision).
                            AppendLine();

                        break;
                }

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_follows_unknown_MMC_command_set_code_0_with_revision_code_1,
                                csd.CommandSet, csd.CommandSetRevision).
                   AppendLine();

                break;
        }

        switch(csd.HighSpeedInterfaceTiming & 0x0F)
        {
            case 0:
                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_is_in_High_Speed_mode);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_is_in_HS200_mode);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_is_in_HS400_mode);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_has_unknown_timing_mode_0,
                                csd.HighSpeedInterfaceTiming & 0x0F).
                   AppendLine();

                break;
        }

        sb.AppendFormat("\t" + Localization.Selected_driver_strength_is_type_0,
                        (csd.HighSpeedInterfaceTiming & 0xF0) >> 4).
           AppendLine();

        if((csd.StrobeSupport & 0x01) == 0x01)
        {
            sb.AppendLine("\t" + Localization.Device_supports_enhanced_strobe_mode);

            sb.AppendLine((csd.BusWidth & 0x80) == 0x80
                              ? "\t" + Localization.Device_uses_strobe_during_Data_Out_CRC_and_CMD_responses
                              : "\t" + Localization.Device_uses_strobe_during_Data_Out_and_CRC_responses);
        }

        switch(csd.BusWidth & 0x0F)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_is_using_1bit_data_bus);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_is_using_4bit_data_bus);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_is_using_8bit_data_bus);

                break;
            case 5:
                sb.AppendLine("\t" + Localization.Device_is_using_4bit_DDR_data_bus);

                break;
            case 6:
                sb.AppendLine("\t" + Localization.Device_is_using_8bit_DDR_data_bus);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_is_using_unknown_data_bus_code_0, csd.BusWidth & 0x0F).
                   AppendLine();

                break;
        }

        switch(csd.ErasedMemoryContent)
        {
            case 0:
            case 1:
                sb.AppendFormat("\t" + Localization.Erased_memory_range_shall_be_0, csd.ErasedMemoryContent).
                   AppendLine();

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_erased_memory_content_code_0, csd.ErasedMemoryContent).
                   AppendLine();

                break;
        }

        if((csd.PartitionConfiguration & 0x40) == 0x40)
            sb.AppendLine("\t" + Localization.Device_sends_boot_acknowledge);

        switch((csd.PartitionConfiguration & 0x38) >> 3)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_is_not_boot_enabled);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_boot_partition_one_is_enabled);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_boot_partition_two_is_enabled);

                break;
            case 7:
                sb.AppendLine("\t" + Localization.Device_user_area_is_enable_for_boot);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Unknown_enabled_boot_partition_code_0,
                                (csd.PartitionConfiguration & 0x38) >> 3).
                   AppendLine();

                break;
        }

        switch(csd.PartitionConfiguration & 0x07)
        {
            case 0:
                sb.AppendLine("\t" + Localization.There_is_no_access_to_boot_partition);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.There_is_read_write_access_to_boot_partition_one);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.There_is_read_write_access_to_boot_partition_two);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.There_is_read_write_access_to_replay_protected_memory_block);

                break;
            default:
                sb.AppendFormat("\t" + Localization.There_is_access_to_general_purpose_partition_0,
                                (csd.PartitionConfiguration & 0x07) - 3).
                   AppendLine();

                break;
        }

        if(csd.BootConfigProtection.HasFlag(BootConfigProtection.Permanent))
            sb.AppendLine("\t" + Localization.Change_of_the_boot_configuration_register_bits_is_permanently_disabled);
        else if(csd.BootConfigProtection.HasFlag(BootConfigProtection.PowerCycle))
        {
            sb.AppendLine("\t" +
                          Localization.
                              Change_of_the_boot_configuration_register_bits_is_disabled_until_the_next_power_cycle);
        }

        switch(csd.BootBusConditions & 0x03)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_will_boot_up_in_x1_SDR_or_x4_DDR_bus_width);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_will_boot_up_in_x4_SDR_or_DDR_bus_width);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_will_boot_up_in_x8_SDR_or_DDR_bus_width);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Unknown_boot_condition_for_bus_width_with_code_three);

                break;
        }

        sb.AppendLine((csd.BootBusConditions & 4) == 4
                          ? "\t" + Localization.Device_will_retain_boot_conditions_after_boot_operation
                          : "\t" +
                            Localization.Device_will_reset_boot_conditions_to_compatibility_mode_after_boot_operation);

        switch((csd.BootBusConditions & 0x24) >> 3)
        {
            case 0:
                sb.AppendLine("\t" +
                              Localization.Device_will_use_single_data_rate_with_compatible_timings_in_boot_operation);

                break;
            case 1:
                sb.AppendLine("\t" +
                              Localization.Device_will_use_single_data_rate_with_high_speed_timings_in_boot_operation);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Device_will_use_dual_data_rate_in_boot_operation);

                break;
            case 3:
                sb.AppendLine("\t" + Localization.Device_will_use_unknown_boot_mode_with_code_three);

                break;
        }

        if(csd.HighCapacityEraseGroupDefinition.HasFlag(HighCapacityEraseGroupDefinition.Enabled))
        {
            sb.AppendLine("\t" +
                          Localization.
                              Device_will_use_high_capacity_erase_unit_size__timeout_and_write_protect_group_size_definitions);
        }

        switch(csd.BootWriteProtectionStatus & 0x03)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Boot_area_one_is_not_protected);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Boot_area_one_is_power_on_protected);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Boot_area_one_is_permanently_protected);

                break;
        }

        switch((csd.BootWriteProtectionStatus & 0x0C) >> 2)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Boot_area_two_is_not_protected);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Boot_area_two_is_power_on_protected);

                break;
            case 2:
                sb.AppendLine("\t" + Localization.Boot_area_two_is_permanently_protected);

                break;
        }

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Permanent))
        {
            if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Selected))
            {
                sb.AppendLine(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.
                                                                              PermanentArea2)
                                  ? "\t" + Localization.Boot_area_two_is_permanently_write_protected
                                  : "\t" + Localization.Boot_area_one_is_permanently_write_protected);
            }
            else
                sb.AppendLine("\t" + Localization.Both_boot_areas_are_permanently_write_protected);
        }
        else if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOn))
        {
            if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Selected))
            {
                sb.AppendLine(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOnArea2)
                                  ? "\t" + Localization.Boot_area_two_is_write_protected_until_next_power_cycle
                                  : "\t" + Localization.Boot_area_one_is_write_protected_until_next_power_cycle);
            }
            else
                sb.AppendLine("\t" + Localization.Both_boot_areas_are_write_protected_until_next_power_cycle);
        }

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PermanentDisable))
            sb.AppendLine("\t" + Localization.Permanent_write_protection_of_boot_areas_is_disabled);

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOnDisable))
            sb.AppendLine("\t" + Localization.Power_cycled_write_protection_of_boot_areas_is_disabled);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePassword))
            sb.AppendLine("\t" + Localization.Use_of_password_protection_features_is_permanently_disabled);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisableWriteProtect))
            sb.AppendLine("\t" + Localization.Use_of_permanent_write_protection_is_disabled);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePermanent))
            sb.AppendLine("\t" + Localization.Permanent_write_protection_is_disabled);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePowerOn))
            sb.AppendLine("\t" + Localization.Power_cycled_write_protection_is_disabled);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.ApplyPermanent))
            sb.AppendLine("\t" + Localization.Permanent_write_protection_will_be_applied_to_selected_group);

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.ApplyPowerOn))
            sb.AppendLine("\t" + Localization.Power_cycled_write_protection_will_be_applied_to_selected_group);

        if((csd.FirmwareConfiguration & 0x01) == 0x01)
            sb.AppendLine("\t" + Localization.Firmware_updates_are_permanently_disabled);

        if(csd.RPMBSize > 0)
        {
            sb.AppendFormat("\t" + Localization.Device_has_a_0_KiB_replay_protected_memory_block, csd.RPMBSize * 128).
               AppendLine();
        }

        if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Supported))
        {
            sb.AppendLine("\t" + Localization.Device_supports_partitioning_features);

            if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Enhanced))
            {
                sb.AppendLine("\t" +
                              Localization.
                                  Device_can_have_enhanced_technological_features_in_partitions_and_user_data_area);
            }

            if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Extended))
                sb.AppendLine("\t" + Localization.Device_can_have_extended_partitions_attribute);
        }

        switch(csd.NativeSectorSize)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_natively_uses_512_byte_sectors);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_natively_uses_4096_byte_sectors);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_natively_uses_unknown_sector_size_indicated_by_code_0,
                                csd.NativeSectorSize).
                   AppendLine();

                break;
        }

        switch(csd.SectorSizeEmulation)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_is_emulating_512_byte_sectors);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_is_using_natively_sized_sectors);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_emulates_unknown_sector_size_indicated_by_code_0,
                                csd.NativeSectorSize).
                   AppendLine();

                break;
        }

        switch(csd.SectorSize)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Device_currently_addresses_512_byte_sectors);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Device_currently_addresses_4096_byte_sectors);

                break;
            default:
                sb.AppendFormat("\t" + Localization.Device_currently_addresses_unknown_sector_size_indicated_by_code_0,
                                csd.NativeSectorSize).
                   AppendLine();

                break;
        }

        if((csd.CacheControl & 0x01) == 0x01)
            sb.AppendLine("\t" + Localization.Devices_cache_is_enabled);

        if((csd.CommandQueueModeEnable & 0x01) == 0x01)
            sb.AppendLine("\t" + Localization.Device_has_enabled_command_queuing);

        return sb.ToString();
    }

    public static string PrettifyExtendedCSD(byte[] response) => PrettifyExtendedCSD(DecodeExtendedCSD(response));
}