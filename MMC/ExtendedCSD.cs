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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Aaru.Decoders.MMC;

[SuppressMessage("ReSharper", "MemberCanBeInternal"), SuppressMessage("ReSharper", "MemberCanBePrivate.Global"),
 SuppressMessage("ReSharper", "UnassignedField.Global"), StructLayout(LayoutKind.Sequential, Pack = 1)]
public class ExtendedCSD
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public byte[] Reserved0;
    public byte   CommandQueueModeEnable;
    public byte   SecureRemovalType;
    public byte   ProductStateAwarenessEnablement;
    public uint   MaxPreLoadingDataSize;
    public uint   PreLoadingDataSize;
    public byte   FFUStatus;
    public ushort Reserved1;
    public byte   ModeOperationCodes;
    public byte   ModeConfig;
    public byte   BarrierControl;
    public byte   CacheFlushing;
    public byte   CacheControl;
    public byte   PowerOffNotification;
    public byte   PackedCommandFailureIndex;
    public byte   PackedCommandStatus;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
    public byte[] ContextConfiguration;
    public ushort ExtendedPartitionsAttribute;
    public ushort ExceptionEventsStatus;
    public ushort ExceptionEventsControl;
    public byte   DyncapNeeded;
    public byte   Class6CommandsControl;
    public byte   InitializationTimeoutAfterEmulationChange;
    public byte   SectorSize;
    public byte   SectorSizeEmulation;
    public byte   NativeSectorSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] VendorSpecific;
    public ushort Reserved2;
    public byte   SupportsProgramCxDInDDR;
    public byte   PeriodicWakeUp;
    public byte   PackageCaseTemperatureControl;
    public byte   ProductionStateAwareness;
    public byte   BadBlockManagementMode;
    public byte   Reserved3;
    public uint   EnhancedUserDataStartAddress;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] EnhancedUserDataAreaSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] GeneralPurposePartitionSize;
    public byte PartitioningSetting;
    public byte PartitionsAttribute;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] MaxEnhancedAreaSize;
    public PartitioningSupport              PartitioningSupport;
    public byte                             HPIManagement;
    public byte                             HWResetFunction;
    public byte                             EnableBackgroundOperationsHandshake;
    public byte                             ManuallyStartBackgroundOperations;
    public byte                             StartSanitizeOperation;
    public byte                             WriteReliabilityParameterRegister;
    public byte                             WriteReliabilitySettingRegister;
    public byte                             RPMBSize;
    public byte                             FirmwareConfiguration;
    public byte                             Reserved4;
    public UserAreaWriteProtectionRegister  UserAreaWriteProtectionRegister;
    public byte                             Reserved5;
    public BootAreaWriteProtectionRegister  BootAreaWriteProtectionRegister;
    public byte                             BootWriteProtectionStatus;
    public HighCapacityEraseGroupDefinition HighCapacityEraseGroupDefinition;
    public byte                             Reserved6;
    public byte                             BootBusConditions;
    public BootConfigProtection             BootConfigProtection;
    public byte                             PartitionConfiguration;
    public byte                             Reserved7;
    public byte                             ErasedMemoryContent;
    public byte                             Reserved8;
    public byte                             BusWidth;
    public byte                             StrobeSupport;
    public byte                             HighSpeedInterfaceTiming;
    public byte                             Reserved9;
    public byte                             PowerClass;
    public byte                             Reserved10;
    public byte                             CommandSetRevision;
    public byte                             Reserved11;
    public byte                             CommandSet;
    public byte                             Revision;
    public byte                             Reserved12;
    public byte                             Structure;
    public byte                             Reserved13;
    public DeviceType                       DeviceType;
    public DriverStrength                   DriverStrength;
    public byte                             OutOfInterruptBusyTiming;
    public byte                             PartitionSwitchingTime;
    public byte                             PowerClass52_195;
    public byte                             PowerClass26_195;
    public byte                             PowerClass52;
    public byte                             PowerClass26;
    public byte                             Reserved14;
    public byte                             MinimumReadPerformance26_4;
    public byte                             MinimumWritePerformance26_4;
    public byte                             MinimumReadPerformance26;
    public byte                             MinimumWritePerformance26;
    public byte                             MinimumReadPerformance52;
    public byte                             MinimumWritePerformance52;
    public SecureWriteProtectInformation    SecureWriteProtectInformation;
    public uint                             SectorCount;
    public byte                             SleepNotificationTimeout;
    public byte                             SleepAwakeTimeout;
    public byte                             ProductionStateAwarenessTimeout;
    public byte                             SleepCurrentVccQ;
    public byte                             SleepCurrentVcc;
    public byte                             HighCapacityWriteProtectGroupSize;
    public byte                             ReliableWriteSectorCount;
    public byte                             HighCapacityEraseTimeout;
    public byte                             HighCapacityEraseUnitSize;
    public byte                             AccessSize;
    public byte                             BootPartitionSize;
    public byte                             Reserved15;
    public BootInformation                  BootInformation;
    public byte                             SecureTRIMMultiplier;
    public byte                             SecureEraseMultiplier;
    public SecureFeatureSupport             SecureFeatureSupport;
    public byte                             TRIMMultiplier;
    public byte                             Reserved16;
    public byte                             MinimumReadPerformanceDDR52;
    public byte                             MinimumWritePerformanceDDR52;
    public byte                             PowerClassDDR200_130;
    public byte                             PowerClassDDR200_195;
    public byte                             PowerClassDDR52_195;
    public byte                             PowerClassDDR52;
    public CacheFlushingPolicy              CacheFlushingPolicy;
    public byte                             InitializationTimeAfterPartition;
    public uint                             CorrectlyProgrammedSectors;
    public byte                             BackgroundOperationsStatus;
    public byte                             PowerOffNotificationTimeout;
    public byte                             GenericCMD6Timeout;
    public uint                             CacheSize;
    public byte                             PowerClassDDR200;
    public ulong                            FirmwareVersion;
    public ushort                           DeviceVersion;
    public byte                             OptimalTrimUnitSize;
    public byte                             OptimalWriteSize;
    public byte                             OptimalReadSize;
    public byte                             PreEOLInformation;
    public byte                             DeviceLifeEstimationTypeA;
    public byte                             DeviceLifeEstimationTypeB;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public byte[] VendorHealthReport;
    public uint              NumberOfFWSectorsCorrectlyProgrammed;
    public byte              Reserved17;
    public byte              CMDQueuingDepth;
    public CMDQueuingSupport CMDQueuingSupport;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 177)]
    public byte[] Reserved18;
    public byte                        BarrierSupport;
    public uint                        FFUArgument;
    public byte                        OperationCodesTimeout;
    public FFUFeatures                 FFUFeatures;
    public SupportedModes              SupportedModes;
    public ExtendedPartitionsSupport   ExtendedPartitionsSupport;
    public byte                        LargeUnitSize;
    public byte                        ContextManagementCaps;
    public byte                        TagResourcesSize;
    public byte                        TagUnitSize;
    public DataTagSupport              DataTagSupport;
    public byte                        MaxPackedWriteCommands;
    public byte                        MaxPackedReadCommands;
    public BackgroundOperationsSupport BackgroundOperationsSupport;
    public HPIFeatures                 HPIFeatures;
    public DeviceSupportedCommandSets  SupportedCommandSets;
    public byte                        ExtendedSecurityCommandsError;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] Reserved19;
}

[Flags]
public enum DeviceSupportedCommandSets : byte
{
    Standard = 1 << 0
}

[Flags]
public enum HPIFeatures : byte
{
    Supported = 1 << 0, CMD12 = 1 << 1
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
    SystemCode = 1 << 0, NonPersistent = 1 << 1
}

[Flags]
public enum SupportedModes : byte
{
    FFU = 1 << 0, VendorSpecific = 1 << 1
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
    Purge    = 1 << 0, Defective = 1 << 2, Trim = 1 << 4,
    Sanitize = 1 << 6
}

[Flags]
public enum BootInformation : byte
{
    Alternative = 1 << 0, DDR = 1 << 1, HighSpeed = 1 << 2
}

[Flags]
public enum SecureWriteProtectInformation : byte
{
    Supported = 1 << 0, Enabled = 1 << 1
}

[Flags]
public enum DriverStrength : byte
{
    Type0 = 1 << 0, Type1 = 1 << 1, Type2 = 1 << 2,
    Type3 = 1 << 3, Type4 = 1 << 4
}

[Flags]
public enum DeviceType : byte
{
    HS_26        = 1 << 0, HS_52    = 1 << 1, HS_DDR_52 = 1 << 2,
    HS_DDR_52_LV = 1 << 3, HS200_18 = 1 << 4, HS200_12  = 1 << 5,
    HS400_18     = 1 << 6, HS400_12 = 1 << 7
}

[Flags]
public enum BootConfigProtection : byte
{
    PowerCycle = 1 << 0, Permanent = 1 << 4
}

[Flags]
public enum HighCapacityEraseGroupDefinition : byte
{
    Enabled = 1 << 0
}

[Flags]
public enum BootAreaWriteProtectionRegister : byte
{
    PowerOn        = 1 << 0, PowerOnArea2     = 1 << 1, Permanent      = 1 << 2,
    PermanentArea2 = 1 << 3, PermanentDisable = 1 << 4, PowerOnDisable = 1 << 6,
    Selected       = 1 << 7
}

[Flags]
public enum UserAreaWriteProtectionRegister : byte
{
    ApplyPowerOn     = 1 << 0, ApplyPermanent      = 1 << 2, DisablePowerOn  = 1 << 3,
    DisablePermanent = 1 << 4, DisableWriteProtect = 1 << 6, DisablePassword = 1 << 7
}

[Flags]
public enum PartitioningSupport : byte
{
    Supported = 1 << 0, Enhanced = 1 << 1, Extended = 1 << 2
}

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
        sb.AppendLine("MultiMediaCard Extended Device Specific Data Register:");

        double unit;

        if(csd.ExtendedSecurityCommandsError != 0)
            sb.AppendFormat("\tLast extended security error was {0}", csd.ExtendedSecurityCommandsError).AppendLine();

        if(csd.SupportedCommandSets.HasFlag(DeviceSupportedCommandSets.Standard))
            sb.AppendLine("\tDevice supports standard MMC command set");

        if(((int)csd.SupportedCommandSets & 0xFE) != 0)
            sb.AppendFormat("\tDevice supports unknown command sets 0x{0:X2}", (int)csd.SupportedCommandSets);

        if(csd.HPIFeatures.HasFlag(HPIFeatures.Supported))
            sb.AppendLine(csd.HPIFeatures.HasFlag(HPIFeatures.CMD12) ? "\tDevice implements HPI using CMD12"
                              : "\tDevice implements HPI using CMD13");

        if(csd.BackgroundOperationsSupport.HasFlag(BackgroundOperationsSupport.Supported))
            sb.AppendLine("\tDevice supports background operations");

        sb.AppendFormat("\tDevice supports a maximum of {0} packed reads and {1} packed writes",
                        csd.MaxPackedReadCommands, csd.MaxPackedWriteCommands).AppendLine();

        if(csd.DataTagSupport.HasFlag(DataTagSupport.Supported))
        {
            sb.AppendLine("\tDevice supports Data Tag");
            sb.AppendFormat("\tTags must be in units of {0} sectors", Math.Pow(2, csd.TagUnitSize)).AppendLine();
            sb.AppendFormat("\tTag resources size is {0}.", csd.TagResourcesSize).AppendLine();
        }

        if(csd.ContextManagementCaps != 0)
        {
            sb.AppendFormat("\tMax context ID is {0}.", csd.ContextManagementCaps & 0xF).AppendLine();

            sb.AppendFormat("\tLarge unit maximum multiplier is {0}.", ((csd.ContextManagementCaps & 0x70) >> 4) + 1).
               AppendLine();
        }

        sb.AppendFormat("\tLarge unit size is {0} MiB", csd.LargeUnitSize + 1).AppendLine();

        if(csd.ExtendedPartitionsSupport.HasFlag(ExtendedPartitionsSupport.NonPersistent))
            sb.AppendLine("\tDevice supports non-persistent extended partitions");

        if(csd.ExtendedPartitionsSupport.HasFlag(ExtendedPartitionsSupport.SystemCode))
            sb.AppendLine("\tDevice supports system code extended partitions");

        if(csd.SupportedModes.HasFlag(SupportedModes.FFU))
        {
            sb.AppendLine("\tDevice supports FFU");

            if(csd.FFUFeatures.HasFlag(FFUFeatures.SupportedModeOperationCodes))

                // todo public byte ModeOperationCodes

                if(csd.OperationCodesTimeout > 0)
                {
                    unit = Math.Pow(2, csd.OperationCodesTimeout) * 100;

                    switch(unit)
                    {
                        case > 1000000:
                            sb.
                                AppendFormat("\t\tMaximum timeout for switch command when setting a value to the mode operation codes field is {0:D2}s",
                                             unit / 1000000).AppendLine();

                            break;
                        case > 1000:
                            sb.
                                AppendFormat("\tMaximum timeout for switch command when setting a value to the mode operation codes field is {0:D2}ms",
                                             unit / 1000).AppendLine();

                            break;
                        default:
                            sb.
                                AppendFormat("\tMaximum timeout for switch command when setting a value to the mode operation codes field is {0:D2}µs",
                                             unit).AppendLine();

                            break;
                    }
                }
        }

        if(csd.SupportedModes.HasFlag(SupportedModes.VendorSpecific))
            sb.AppendLine("\tDevice supports Vendor Specific Mode");

        if(csd.BarrierSupport == 0x01)
            sb.AppendLine("\tDevice supports the barrier command");

        if(csd.CMDQueuingSupport.HasFlag(CMDQueuingSupport.Supported))
            sb.AppendFormat("\tDevice supports command queuing with a depth of {0}", csd.CMDQueuingDepth + 1).
               AppendLine();

        sb.AppendFormat("\t{0} firmware sectors correctly programmed", csd.NumberOfFWSectorsCorrectlyProgrammed).
           AppendLine();

        switch(csd.DeviceLifeEstimationTypeB)
        {
            case 1:
                sb.AppendLine("\tDevice used between 0% and 10% of its estimated life time");

                break;
            case 2:
                sb.AppendLine("\tDevice used between 10% and 20% of its estimated life time");

                break;
            case 3:
                sb.AppendLine("\tDevice used between 20% and 30% of its estimated life time");

                break;
            case 4:
                sb.AppendLine("\tDevice used between 30% and 40% of its estimated life time");

                break;
            case 5:
                sb.AppendLine("\tDevice used between 40% and 50% of its estimated life time");

                break;
            case 6:
                sb.AppendLine("\tDevice used between 50% and 60% of its estimated life time");

                break;
            case 7:
                sb.AppendLine("\tDevice used between 60% and 70% of its estimated life time");

                break;
            case 8:
                sb.AppendLine("\tDevice used between 70% and 80% of its estimated life time");

                break;
            case 9:
                sb.AppendLine("\tDevice used between 80% and 90% of its estimated life time");

                break;
            case 10:
                sb.AppendLine("\tDevice used between 90% and 100% of its estimated life time");

                break;
            case 11:
                sb.AppendLine("\tDevice exceeded its maximum estimated life time");

                break;
        }

        switch(csd.DeviceLifeEstimationTypeA)
        {
            case 1:
                sb.AppendLine("\tDevice used between 0% and 10% of its estimated life time");

                break;
            case 2:
                sb.AppendLine("\tDevice used between 10% and 20% of its estimated life time");

                break;
            case 3:
                sb.AppendLine("\tDevice used between 20% and 30% of its estimated life time");

                break;
            case 4:
                sb.AppendLine("\tDevice used between 30% and 40% of its estimated life time");

                break;
            case 5:
                sb.AppendLine("\tDevice used between 40% and 50% of its estimated life time");

                break;
            case 6:
                sb.AppendLine("\tDevice used between 50% and 60% of its estimated life time");

                break;
            case 7:
                sb.AppendLine("\tDevice used between 60% and 70% of its estimated life time");

                break;
            case 8:
                sb.AppendLine("\tDevice used between 70% and 80% of its estimated life time");

                break;
            case 9:
                sb.AppendLine("\tDevice used between 80% and 90% of its estimated life time");

                break;
            case 10:
                sb.AppendLine("\tDevice used between 90% and 100% of its estimated life time");

                break;
            case 11:
                sb.AppendLine("\tDevice exceeded its maximum estimated life time");

                break;
        }

        switch(csd.PreEOLInformation)
        {
            case 1:
                sb.AppendLine("\tDevice informs it's in good health");

                break;
            case 2:
                sb.AppendLine("\tDevice informs it should be replaced soon");

                break;
            case 3:
                sb.AppendLine("\tDevice informs it should be replace immediately");

                break;
        }

        if(csd.OptimalReadSize == 0)
            sb.AppendLine("\tDevice does not report an optimal read size");
        else
            sb.AppendFormat("\tOptimal read size is {0} KiB", 4 * csd.OptimalReadSize).AppendLine();

        if(csd.OptimalWriteSize == 0)
            sb.AppendLine("\tDevice does not report an optimal write size");
        else
            sb.AppendFormat("\tOptimal write size is {0} KiB", 4 * csd.OptimalWriteSize).AppendLine();

        if(csd.OptimalTrimUnitSize == 0)
            sb.AppendLine("\tDevice does not report an optimal trim size");
        else
            sb.AppendFormat("\tOptimal trim size is {0} KiB", 4 * Math.Pow(2, csd.OptimalTrimUnitSize - 1)).
               AppendLine();

        sb.AppendFormat("\tDevice version: {0}", csd.DeviceVersion).AppendLine();
        sb.AppendFormat("\tFirmware version: {0}", csd.FirmwareVersion).AppendLine();

        if(csd.CacheSize == 0)
            sb.AppendLine("\tDevice has no cache");
        else
            sb.AppendFormat("\tDevice has {0} KiB of cache", csd.CacheSize / 8).AppendLine();

        if(csd.GenericCMD6Timeout > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms by default for a SWITCH command",
                            csd.GenericCMD6Timeout * 10).AppendLine();

        if(csd.PowerOffNotificationTimeout > 0)
            sb.
                AppendFormat("\tDevice takes a maximum of {0} by default to power off from a SWITCH command notification",
                             csd.PowerOffNotificationTimeout * 10).AppendLine();

        switch(csd.BackgroundOperationsStatus & 0x03)
        {
            case 0:
                sb.AppendLine("\tDevice has no pending background operations");

                break;
            case 1:
                sb.AppendLine("\tDevice has non critical operations outstanding");

                break;
            case 2:
                sb.AppendLine("\tDevice has performance impacted operations outstanding");

                break;
            case 3:
                sb.AppendLine("\tDevice has critical operations outstanding");

                break;
        }

        sb.AppendFormat("\tLast WRITE MULTIPLE command correctly programmed {0} sectors",
                        csd.CorrectlyProgrammedSectors).AppendLine();

        if(csd.InitializationTimeAfterPartition > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms for initialization after partition",
                            csd.InitializationTimeAfterPartition * 100).AppendLine();

        if(csd.CacheFlushingPolicy.HasFlag(CacheFlushingPolicy.FIFO))
            sb.AppendLine("\tDevice uses a FIFO policy for cache flushing");

        if(csd.TRIMMultiplier > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms for trimming a single erase group",
                            csd.TRIMMultiplier * 300).AppendLine();

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Sanitize))
            sb.AppendLine("\tDevice supports the sanitize operation");

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Trim))
            sb.AppendLine("\tDevice supports supports the secure and insecure trim operations");

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Defective))
            sb.AppendLine("\tDevice supports automatic erase on retired defective blocks");

        if(csd.SecureFeatureSupport.HasFlag(SecureFeatureSupport.Purge))
            sb.AppendLine("\tDevice supports secure purge operations");

        if(csd.SecureEraseMultiplier > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms for securely erasing a single erase group",
                            csd.SecureEraseMultiplier * 300).AppendLine();

        if(csd.SecureTRIMMultiplier > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms for securely trimming a single erase group",
                            csd.SecureTRIMMultiplier * 300).AppendLine();

        if(csd.BootInformation.HasFlag(BootInformation.HighSpeed))
            sb.AppendLine("\tDevice supports high speed timing on boot");

        if(csd.BootInformation.HasFlag(BootInformation.DDR))
            sb.AppendLine("\tDevice supports dual data rate on boot");

        if(csd.BootInformation.HasFlag(BootInformation.Alternative))
            sb.AppendLine("\tDevice supports alternative boot method");

        if(csd.BootPartitionSize > 0)
            sb.AppendFormat("\tDevice has a {0} KiB boot partition", csd.BootPartitionSize * 128).AppendLine();

        if((csd.AccessSize & 0x0F) > 0)
            sb.AppendFormat("\tDevice has a page size of {0} KiB",
                            512 * Math.Pow(2, (csd.AccessSize & 0x0F) - 1) / 1024.0).AppendLine();

        if(csd.HighCapacityEraseUnitSize > 0)
            sb.AppendFormat("\tDevice erase groups are {0} KiB", csd.HighCapacityEraseUnitSize * 512).AppendLine();

        if(csd.HighCapacityEraseTimeout > 0)
            sb.AppendFormat("\tDevice takes a maximum of {0} ms for erasing a single erase group",
                            csd.HighCapacityEraseTimeout * 300).AppendLine();

        if(csd.HighCapacityWriteProtectGroupSize > 0)
            sb.AppendFormat("\tDevice smallest write protect group is made of {0} erase groups",
                            csd.HighCapacityWriteProtectGroupSize).AppendLine();

        if(csd.SleepCurrentVcc > 0)
        {
            unit = Math.Pow(2, csd.SleepCurrentVcc);

            if(unit > 1000)
                sb.AppendFormat("\tDevice uses {0} mA on Vcc when sleeping", unit / 1000).AppendLine();
            else
                sb.AppendFormat("\tDevice uses {0} μA on Vcc when sleeping", unit).AppendLine();
        }

        if(csd.SleepCurrentVccQ > 0)
        {
            unit = Math.Pow(2, csd.SleepCurrentVccQ);

            if(unit > 1000)
                sb.AppendFormat("\tDevice uses {0} mA on Vccq when sleeping", unit / 1000).AppendLine();
            else
                sb.AppendFormat("\tDevice uses {0} μA on Vccq when sleeping", unit).AppendLine();
        }

        if(csd.ProductionStateAwarenessTimeout > 0)
        {
            unit = Math.Pow(2, csd.ProductionStateAwareness) * 100;

            switch(unit)
            {
                case > 1000000:
                    sb.AppendFormat("\tDevice takes a maximum of {0} s to switch production state awareness",
                                    unit / 1000000).AppendLine();

                    break;
                case > 1000:
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to switch production state awareness",
                                    unit / 1000).AppendLine();

                    break;
                default:
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to switch production state awareness", unit).
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
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to transition between sleep and standby states",
                                    unit / 1000000).AppendLine();

                    break;
                case > 1000:
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to transition between sleep and standby states",
                                    unit / 1000).AppendLine();

                    break;
                default:
                    sb.AppendFormat("\tDevice takes a maximum of {0} ns to transition between sleep and standby states",
                                    unit).AppendLine();

                    break;
            }
        }

        if(csd.SleepNotificationTimeout > 0)
        {
            unit = Math.Pow(2, csd.SleepNotificationTimeout) * 10;

            switch(unit)
            {
                case > 1000000:
                    sb.AppendFormat("\tDevice takes a maximum of {0} s to move to sleep state", unit / 1000000).
                       AppendLine();

                    break;
                case > 1000:
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to move to sleep state", unit / 1000).
                       AppendLine();

                    break;
                default:
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to move to sleep state", unit).AppendLine();

                    break;
            }
        }

        sb.AppendFormat("\tDevice has {0} sectors", csd.SectorCount).AppendLine();

        if(csd.SecureWriteProtectInformation.HasFlag(SecureWriteProtectInformation.Supported))
        {
            sb.AppendLine("\tDevice supports secure write protection");

            if(csd.SecureWriteProtectInformation.HasFlag(SecureWriteProtectInformation.Enabled))
                sb.AppendLine("\tDevice has secure write protection enabled");
        }

        unit = csd.MinimumReadPerformance26 * 300;

        if(csd.MinimumReadPerformance26 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 26Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 26Mhz mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumReadPerformance26_4 * 300;

        if(csd.MinimumReadPerformance26_4 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 26Mhz 4-bit mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 26Mhz 4-bit mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumReadPerformance52 * 300;

        if(csd.MinimumReadPerformance52 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 52Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 52Mhz mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumReadPerformanceDDR52 * 600;

        if(csd.MinimumReadPerformanceDDR52 == 0)
            sb.AppendLine("\tDevice cannot achieve 4.8MB/s reading in DDR 52Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in DDR 52Mhz mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumWritePerformance26 * 300;

        if(csd.MinimumWritePerformance26 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 26Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 26Mhz mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumWritePerformance26_4 * 300;

        if(csd.MinimumWritePerformance26_4 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 26Mhz 4-bit mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 26Mhz 4-bit mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumWritePerformance52 * 300;

        if(csd.MinimumWritePerformance52 == 0)
            sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 52Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 52Mhz mode", unit / 1000).
               AppendLine();

        unit = csd.MinimumWritePerformanceDDR52 * 600;

        if(csd.MinimumWritePerformanceDDR52 == 0)
            sb.AppendLine("\tDevice cannot achieve 4.8MB/s writing in DDR 52Mhz mode");
        else
            sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in DDR 52Mhz mode", unit / 1000).
               AppendLine();

        if(csd.PartitionSwitchingTime > 0)
            sb.AppendFormat("\tDevice can take a maximum of {0} ms when switching partitions",
                            csd.PartitionSwitchingTime * 10).AppendLine();

        if(csd.OutOfInterruptBusyTiming > 0)
            sb.AppendFormat("\tDevice can take a maximum of {0} ms when releasing from an interrupt",
                            csd.OutOfInterruptBusyTiming * 10).AppendLine();

        if(csd.DriverStrength.HasFlag(DriverStrength.Type0))
            sb.AppendLine("\tDevice supports I/O driver strength type 0.");

        if(csd.DriverStrength.HasFlag(DriverStrength.Type1))
            sb.AppendLine("\tDevice supports I/O driver strength type 1.");

        if(csd.DriverStrength.HasFlag(DriverStrength.Type2))
            sb.AppendLine("\tDevice supports I/O driver strength type 2.");

        if(csd.DriverStrength.HasFlag(DriverStrength.Type3))
            sb.AppendLine("\tDevice supports I/O driver strength type 3.");

        if(csd.DriverStrength.HasFlag(DriverStrength.Type4))
            sb.AppendLine("\tDevice supports I/O driver strength type 4.");

        if(csd.DeviceType.HasFlag(DeviceType.HS_26))
            sb.AppendLine("\tDevice supports 26 Mhz mode");

        if(csd.DeviceType.HasFlag(DeviceType.HS_52))
            sb.AppendLine("\tDevice supports 52 Mhz mode");

        if(csd.DeviceType.HasFlag(DeviceType.HS_DDR_52))
            sb.AppendLine("\tDevice supports DDR 52 Mhz mode at 1.8V or 3V");

        if(csd.DeviceType.HasFlag(DeviceType.HS_DDR_52_LV))
            sb.AppendLine("\tDevice supports DDR 52 Mhz mode 1.2V");

        if(csd.DeviceType.HasFlag(DeviceType.HS200_18))
            sb.AppendLine("\tDevice supports HS-200 mode (SDR 200Mhz) at 1.8V");

        if(csd.DeviceType.HasFlag(DeviceType.HS200_12))
            sb.AppendLine("\tDevice supports HS-200 mode (SDR 200Mhz) at 1.2V");

        if(csd.DeviceType.HasFlag(DeviceType.HS400_18))
            sb.AppendLine("\tDevice supports HS-400 mode (DDR 200Mhz) at 1.8V");

        if(csd.DeviceType.HasFlag(DeviceType.HS400_12))
            sb.AppendLine("\tDevice supports HS-400 mode (DDR 200Mhz) at 1.2V");

        sb.AppendFormat("\tCSD version 1.{0} revision 1.{1}", csd.Structure, csd.Revision).AppendLine();

        switch(csd.CommandSet)
        {
            case 0:
                sb.AppendLine("\tDevice follows compatibility MMC command set.");

                break;
            case 1:
                switch(csd.CommandSetRevision)
                {
                    case 0:
                        sb.AppendLine("\tDevice follows standard MMC command set v4.0.");

                        break;
                    default:
                        sb.AppendFormat("\tDevice follows standard MMC command set with unknown version code {0}.",
                                        csd.CommandSetRevision).AppendLine();

                        break;
                }

                break;
            default:
                sb.AppendFormat("\tDevice follows unknown MMC command set code {0} with revision code {1}.",
                                csd.CommandSet, csd.CommandSetRevision).AppendLine();

                break;
        }

        switch(csd.HighSpeedInterfaceTiming & 0x0F)
        {
            case 0: break;
            case 1:
                sb.AppendLine("\tDevice is in High Speed mode.");

                break;
            case 2:
                sb.AppendLine("\tDevice is in HS-200 mode.");

                break;
            case 3:
                sb.AppendLine("\tDevice is in HS-400 mode.");

                break;
            default:
                sb.AppendFormat("\tDevice has unknown timing mode {0}.", csd.HighSpeedInterfaceTiming & 0x0F).
                   AppendLine();

                break;
        }

        sb.AppendFormat("\tSelected driver strength is type {0}.", (csd.HighSpeedInterfaceTiming & 0xF0) >> 4).
           AppendLine();

        if((csd.StrobeSupport & 0x01) == 0x01)
        {
            sb.AppendLine("\tDevice supports enhanced strobe mode");

            sb.AppendLine((csd.BusWidth & 0x80) == 0x80 ? "\tDevice uses strobe during Data Out, CRC and CMD responses"
                              : "\tDevice uses strobe during Data Out and CRC responses");
        }

        switch(csd.BusWidth & 0x0F)
        {
            case 0:
                sb.AppendLine("\tDevice is using 1-bit data bus");

                break;
            case 1:
                sb.AppendLine("\tDevice is using 4-bit data bus");

                break;
            case 2:
                sb.AppendLine("\tDevice is using 8-bit data bus");

                break;
            case 5:
                sb.AppendLine("\tDevice is using 4-bit DDR data bus");

                break;
            case 6:
                sb.AppendLine("\tDevice is using 8-bit DDR data bus");

                break;
            default:
                sb.AppendFormat("\tDevice is using unknown data bus code {0}", csd.BusWidth & 0x0F).AppendLine();

                break;
        }

        switch(csd.ErasedMemoryContent)
        {
            case 0:
            case 1:
                sb.AppendFormat("\tErased memory range shall be '{0}'.", csd.ErasedMemoryContent).AppendLine();

                break;
            default:
                sb.AppendFormat("\tUnknown erased memory content code {0}", csd.ErasedMemoryContent).AppendLine();

                break;
        }

        if((csd.PartitionConfiguration & 0x40) == 0x40)
            sb.AppendLine("\tDevice sends boot acknowledge");

        switch((csd.PartitionConfiguration & 0x38) >> 3)
        {
            case 0:
                sb.AppendLine("\tDevice is not boot enabled");

                break;
            case 1:
                sb.AppendLine("\tDevice boot partition 1 is enabled");

                break;
            case 2:
                sb.AppendLine("\tDevice boot partition 2 is enabled");

                break;
            case 7:
                sb.AppendLine("\tDevice user area is enable for boot");

                break;
            default:
                sb.AppendFormat("\tUnknown enabled boot partition code {0}", (csd.PartitionConfiguration & 0x38) >> 3).
                   AppendLine();

                break;
        }

        switch(csd.PartitionConfiguration & 0x07)
        {
            case 0:
                sb.AppendLine("\tThere is no access to boot partition");

                break;
            case 1:
                sb.AppendLine("\tThere is read/write access to boot partition 1");

                break;
            case 2:
                sb.AppendLine("\tThere is read/write access to boot partition 2");

                break;
            case 3:
                sb.AppendLine("\tThere is read/write access to replay protected memory block");

                break;
            default:
                sb.AppendFormat("\tThere is access to general purpose partition {0}",
                                (csd.PartitionConfiguration & 0x07) - 3).AppendLine();

                break;
        }

        if(csd.BootConfigProtection.HasFlag(BootConfigProtection.Permanent))
            sb.AppendLine("\tChange of the boot configuration register bits is permanently disabled.");
        else if(csd.BootConfigProtection.HasFlag(BootConfigProtection.PowerCycle))
            sb.AppendLine("\tChange of the boot configuration register bits is disabled until the next power cycle.");

        switch(csd.BootBusConditions & 0x03)
        {
            case 0:
                sb.AppendLine("\tDevice will boot up in x1 SDR or x4 DDR bus width.");

                break;
            case 1:
                sb.AppendLine("\tDevice will boot up in x4 SDR or DDR bus width.");

                break;
            case 2:
                sb.AppendLine("\tDevice will boot up in x8 SDR or DDR bus width.");

                break;
            case 3:
                sb.AppendLine("\tUnknown boot condition for bus width with code 3.");

                break;
        }

        sb.AppendLine((csd.BootBusConditions & 4) == 4 ? "\tDevice will retain boot conditions after boot operation."
                          : "\tDevice will reset boot conditions to compatibility mode after boot operation.");

        switch((csd.BootBusConditions & 0x24) >> 3)
        {
            case 0:
                sb.AppendLine("\tDevice will use single data rate with compatible timings in boot operation.");

                break;
            case 1:
                sb.AppendLine("\tDevice will use single data rate with high speed timings in boot operation.");

                break;
            case 2:
                sb.AppendLine("\tDevice will use dual data rate in boot operation.");

                break;
            case 3:
                sb.AppendLine("\tDevice will use unknown boot mode with code 3.");

                break;
        }

        if(csd.HighCapacityEraseGroupDefinition.HasFlag(HighCapacityEraseGroupDefinition.Enabled))
            sb.AppendLine("\tDevice will use high capacity erase unit size, timeout and write protect group size definitions.");

        switch(csd.BootWriteProtectionStatus & 0x03)
        {
            case 0:
                sb.AppendLine("\tBoot area 1 is not protected");

                break;
            case 1:
                sb.AppendLine("\tBoot area 1 is power on protected");

                break;
            case 2:
                sb.AppendLine("\tBoot area 1 is permanently protected");

                break;
        }

        switch((csd.BootWriteProtectionStatus & 0x0C) >> 2)
        {
            case 0:
                sb.AppendLine("\tBoot area 2 is not protected");

                break;
            case 1:
                sb.AppendLine("\tBoot area 2 is power on protected");

                break;
            case 2:
                sb.AppendLine("\tBoot area 2 is permanently protected");

                break;
        }

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Permanent))
        {
            if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Selected))
                sb.AppendLine(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.
                                                                              PermanentArea2)
                                  ? "\tBoot area 2 is permanently write protected."
                                  : "\tBoot area 1 is permanently write protected.");
            else
                sb.AppendLine("\tBoth boot areas are permanently write protected.");
        }
        else if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOn))
        {
            if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.Selected))
                sb.AppendLine(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOnArea2)
                                  ? "\tBoot area 2 is write protected until next power cycle."
                                  : "\tBoot area 1 is write protected until next power cycle.");
            else
                sb.AppendLine("\tBoth boot areas are write protected until next power cycle.");
        }

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PermanentDisable))
            sb.AppendLine("\tPermanent write protection of boot areas is disabled.");

        if(csd.BootAreaWriteProtectionRegister.HasFlag(BootAreaWriteProtectionRegister.PowerOnDisable))
            sb.AppendLine("\tPower cycled write protection of boot areas is disabled.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePassword))
            sb.AppendLine("\tUse of password protection features is permanently disabled.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisableWriteProtect))
            sb.AppendLine("\tUse of permanent write protection is disabled.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePermanent))
            sb.AppendLine("\tPermanent write protection is disabled.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.DisablePowerOn))
            sb.AppendLine("\tPower cycled write protection is disabled.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.ApplyPermanent))
            sb.AppendLine("\tPermanent write protection will be applied to selected group.");

        if(csd.UserAreaWriteProtectionRegister.HasFlag(UserAreaWriteProtectionRegister.ApplyPowerOn))
            sb.AppendLine("\tPower cycled write protection will be applied to selected group.");

        if((csd.FirmwareConfiguration & 0x01) == 0x01)
            sb.AppendLine("\tFirmware updates are permanently disabled");

        if(csd.RPMBSize > 0)
            sb.AppendFormat("\tDevice has a {0} KiB replay protected memory block", csd.RPMBSize * 128).AppendLine();

        if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Supported))
        {
            sb.AppendLine("\tDevice supports partitioning features");

            if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Enhanced))
                sb.AppendLine("\tDevice can have enhanced technological features in partitions and user data area");

            if(csd.PartitioningSupport.HasFlag(PartitioningSupport.Extended))
                sb.AppendLine("\tDevice can have extended partitions attribute.");
        }

        switch(csd.NativeSectorSize)
        {
            case 0:
                sb.AppendLine("\tDevice natively uses 512 byte sectors");

                break;
            case 1:
                sb.AppendLine("\tDevice natively uses 4096 byte sectors");

                break;
            default:
                sb.AppendFormat("\tDevice natively uses unknown sector size indicated by code {0}",
                                csd.NativeSectorSize).AppendLine();

                break;
        }

        switch(csd.SectorSizeEmulation)
        {
            case 0:
                sb.AppendLine("\tDevice is emulating 512 byte sectors");

                break;
            case 1:
                sb.AppendLine("\tDevice is using natively sized sectors");

                break;
            default:
                sb.AppendFormat("\tDevice emulates unknown sector size indicated by code {0}", csd.NativeSectorSize).
                   AppendLine();

                break;
        }

        switch(csd.SectorSize)
        {
            case 0:
                sb.AppendLine("\tDevice currently addresses 512 byte sectors");

                break;
            case 1:
                sb.AppendLine("\tDevice currently addresses 4096 byte sectors");

                break;
            default:
                sb.AppendFormat("\tDevice currently addresses unknown sector size indicated by code {0}",
                                csd.NativeSectorSize).AppendLine();

                break;
        }

        if((csd.CacheControl & 0x01) == 0x01)
            sb.AppendLine("\tDevice's cache is enabled");

        if((csd.CommandQueueModeEnable & 0x01) == 0x01)
            sb.AppendLine("\tDevice has enabled command queuing");

        return sb.ToString();
    }

    public static string PrettifyExtendedCSD(byte[] response) => PrettifyExtendedCSD(DecodeExtendedCSD(response));
}