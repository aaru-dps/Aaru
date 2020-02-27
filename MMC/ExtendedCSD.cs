// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Aaru.Decoders.MMC
{
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "UnassignedField.Global"),
     StructLayout(LayoutKind.Sequential)]
    public class ExtendedCSD
    {
        public byte AccessSize;
        public byte AddressedGroupToBeReleased;
        public byte BackgroundOperationsStatus;
        public byte BackgroundOperationsSupport;
        public byte BadBlockManagementMode;
        public byte BarrierControl;
        public byte BarrierSupport;
        public byte BootAreaWriteProtectionRegister;
        public byte BootBusConditions;
        public byte BootConfigProtection;
        public byte BootInformation;
        public byte BootPartitionSize;
        public byte BootWriteProtectionStatus;
        public byte BusWidth;
        public byte CacheControl;
        public byte CacheFlushing;
        public byte CacheFlushingPolicy;
        public uint CacheSize;
        public byte Class6CommandsControl;
        public byte CMDQueuingDepth;
        public byte CMDQueuingSupport;
        public byte CommandQueueModeEnable;
        public byte CommandSet;
        public byte CommandSetRevision;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] ContextConfiguration;
        public byte   ContextManagementCaps;
        public uint   CorrectlyProgrammedSectors;
        public byte   DataTagSupport;
        public byte   DeviceLifeEstimationTypeA;
        public byte   DeviceLifeEstimationTypeB;
        public byte   DeviceType;
        public ushort DeviceVersion;
        public byte   DriverStrength;
        public byte   EnableBackgroundOperationsHandshake;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] EnhancedUserDataAreaSize;
        public uint   EnhancedUserDataStartAddress;
        public byte   ErasedMemoryContent;
        public ushort ExceptionEventsControl;
        public ushort ExceptionEventsStatus;
        public ushort ExtendedPartitionsAttribute;
        public byte   ExtendedPartitionsSupport;
        public byte   ExtendedSecurityCommandsError;
        public uint   FFUArgument;
        public byte   FFUFeatures;
        public byte   FFUStatus;
        public byte   FirmwareConfiguration;
        public ulong  FirmwareVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] GeneralPurposePartitionSize;
        public byte GenericCMD6Timeout;
        public byte HighCapacityEraseTimeout;
        public byte HighCapacityEraseUnitSize;
        public byte HighCapacityWriteProtectGroupSize;
        public byte HighDensityEraseGroupDefinition;
        public byte HighSpeedInterfaceTiming;
        public byte HPIFeatures;
        public byte HPIManagement;
        public byte HWResetFunction;
        public byte InitializationTimeAfterPartition;
        public byte InitializationTimeout;
        public byte LargeUnitSize;
        public byte ManuallyStartBackgroundOperations;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] MaxEnhancedAreaSize;
        public byte MaxPackedReadCommands;
        public byte MaxPackedWriteCommands;
        public uint MaxPreLoadingDataSize;
        public byte MinimumReadPerformance26;
        public byte MinimumReadPerformance26_4;
        public byte MinimumReadPerformance52;
        public byte MinimumReadPerformanceDDR52;
        public byte MinimumWritePerformance26;
        public byte MinimumWritePerformance26_4;
        public byte MinimumWritePerformance52;
        public byte MinimumWritePerformanceDDR52;
        public byte ModeConfig;
        public byte ModeOperationCodes;
        public byte NativeSectorSize;
        public uint NumberofFWSectorsCorrectlyProgrammed;
        public byte OperationCodesTimeout;
        public byte OptimalReadSize;
        public byte OptimalTrimUnitSize;
        public byte OptimalWriteSize;
        public byte OutOfInterruptBusyTiming;
        public byte PackageCaseTemperatureControl;
        public byte PackedCommandFailureIndex;
        public byte PackedCommandStatus;
        public byte PartitionConfiguration;
        public byte PartitioningSetting;
        public byte PartitioningSupport;
        public byte PartitionsAttribute;
        public byte PartitionSwitchingTime;
        public byte PeriodicWakeUp;
        public byte PowerClass;
        public byte PowerClass26;
        public byte PowerClass26_195;
        public byte PowerClass52;
        public byte PowerClass52_195;
        public byte PowerClassDDR200;
        public byte PowerClassDDR200_130;
        public byte PowerClassDDR200_195;
        public byte PowerClassDDR52;
        public byte PowerClassDDR52_195;
        public byte PowerOffNotification;
        public byte PowerOffNotificationTimeout;
        public byte PreEOLInformation;
        public uint PreLoadingDataSize;
        public byte ProductionStateAwareness;
        public byte ProductionStateAwarenessTimeout;
        public byte ProductStateAwarenessEnablement;
        public byte ReliableWriteSectorCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Reserved0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 177)]
        public byte[] Reserved1;
        public byte   Reserved10;
        public byte   Reserved11;
        public byte   Reserved12;
        public byte   Reserved13;
        public byte   Reserved14;
        public byte   Reserved15;
        public ushort Reserved16;
        public ushort Reserved17;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] Reserved18;
        public byte Reserved2;
        public byte Reserved3;
        public byte Reserved4;
        public byte Reserved5;
        public byte Reserved6;
        public byte Reserved7;
        public byte Reserved8;
        public byte Reserved9;
        public byte Revision;
        public byte RPMBSize;
        public uint SectorCount;
        public byte SectorSize;
        public byte SectorSizeEmulation;
        public byte SecureEraseMultiplier;
        public byte SecureFeatureSupport;
        public byte SecureRemovalType;
        public byte SecureTRIMMultiplier;
        public byte SecureWriteProtectInformation;
        public byte SleepAwakeTimeout;
        public byte SleepCurrentVcc;
        public byte SleepCurrentVccq;
        public byte SleepNotificationTimeout;
        public byte StartSanitizeOperation;
        public byte StrobeSupport;
        public byte Structure;
        public byte SupportedCommandSets;
        public byte SupportedModes;
        public byte SupportsProgramCxDInDDR;
        public byte TagResourcesSize;
        public byte TagUnitSize;
        public byte TRIMMultiplier;
        public byte UserAreaWriteProtectionRegister;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] VendorHealthReport;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] VendorSpecific;
        public byte WriteReliabilityParameterRegister;
        public byte WriteReliabilitySettingRegister;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static partial class Decoders
    {
        public static ExtendedCSD DecodeExtendedCSD(byte[] response)
        {
            if(response == null)
                return null;

            if(response.Length != 512)
                return null;

            GCHandle handle = GCHandle.Alloc(response, GCHandleType.Pinned);
            var      csd    = (ExtendedCSD)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ExtendedCSD));
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

            if((csd.HPIFeatures & 0x01) == 0x01)
                sb.AppendLine((csd.HPIFeatures & 0x02) == 0x02 ? "\tDevice implements HPI using CMD12"
                                  : "\tDevice implements HPI using CMD13");

            if((csd.BackgroundOperationsSupport & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports background operations");

            sb.AppendFormat("\tDevice supports a maximum of {0} packed reads and {1} packed writes",
                            csd.MaxPackedReadCommands, csd.MaxPackedWriteCommands).AppendLine();

            if((csd.DataTagSupport & 0x01) == 0x01)
            {
                sb.AppendLine("\tDevice supports Data Tag");
                sb.AppendFormat("\tTags must be in units of {0} sectors", Math.Pow(2, csd.TagUnitSize)).AppendLine();
            }

            if((csd.ExtendedPartitionsSupport & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports non-persistent extended partitions");

            if((csd.ExtendedPartitionsSupport & 0x02) == 0x02)
                sb.AppendLine("\tDevice supports system code extended partitions");

            if((csd.SupportedModes & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports FFU");

            if((csd.SupportedModes & 0x02) == 0x02)
                sb.AppendLine("\tDevice supports Vendor Specific Mode");

            if((csd.CMDQueuingSupport & 0x01) == 0x01)
                sb.AppendFormat("\tDevice supports command queuing with a depth of {0}", csd.CMDQueuingDepth + 1).
                   AppendLine();

            sb.AppendFormat("\t{0} firmware sectors correctly programmed", csd.NumberofFWSectorsCorrectlyProgrammed).
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

            sb.AppendFormat("\tOptimal read size is {0} logical sectors", csd.OptimalReadSize).AppendLine();
            sb.AppendFormat("\tOptimal write size is {0} logical sectors", csd.OptimalWriteSize).AppendLine();
            sb.AppendFormat("\tOptimal trim size is {0} logical sectors", csd.OptimalTrimUnitSize).AppendLine();

            sb.AppendFormat("\tDevice version: {0}", csd.DeviceVersion).AppendLine();
            sb.AppendFormat("\tFirmware version: {0}", csd.FirmwareVersion).AppendLine();

            if(csd.CacheSize == 0)
                sb.AppendLine("\tDevice has no cache");
            else
                sb.AppendFormat("\tDevice has {0} KiB of cache", csd.CacheSize).AppendLine();

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

            if((csd.CacheFlushingPolicy & 0x01) == 0x01)
                sb.AppendLine("\tDevice uses a FIFO policy for cache flushing");

            if(csd.TRIMMultiplier > 0)
                sb.AppendFormat("\tDevice takes a maximum of {0} ms for trimming a single erase group",
                                csd.TRIMMultiplier * 300).AppendLine();

            if((csd.SecureFeatureSupport & 0x40) == 0x40)
                sb.AppendLine("\tDevice supports the sanitize operation");

            if((csd.SecureFeatureSupport & 0x10) == 0x10)
                sb.AppendLine("\tDevice supports supports the secure and insecure trim operations");

            if((csd.SecureFeatureSupport & 0x04) == 0x04)
                sb.AppendLine("\tDevice supports automatic erase on retired defective blocks");

            if((csd.SecureFeatureSupport & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports secure purge operations");

            if(csd.SecureEraseMultiplier > 0)
                sb.AppendFormat("\tDevice takes a maximum of {0} ms for securely erasing a single erase group",
                                csd.SecureEraseMultiplier * 300).AppendLine();

            if(csd.SecureTRIMMultiplier > 0)
                sb.AppendFormat("\tDevice takes a maximum of {0} ms for securely trimming a single erase group",
                                csd.SecureTRIMMultiplier * 300).AppendLine();

            if((csd.BootInformation & 0x04) == 0x04)
                sb.AppendLine("\tDevice supports high speed timing on boot");

            if((csd.BootInformation & 0x02) == 0x02)
                sb.AppendLine("\tDevice supports dual data rate on boot");

            if((csd.BootInformation & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports alternative boot method");

            if(csd.BootPartitionSize > 0)
                sb.AppendFormat("\tDevice has a {0} KiB boot partition", csd.BootPartitionSize * 128).AppendLine();

            if((csd.AccessSize & 0x0F) > 0)
                sb.AppendFormat("\tDevice has a page size of {0} KiB", (csd.AccessSize & 0x0F) * 512.0 / 1024.0).
                   AppendLine();

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

            if(csd.SleepCurrentVccq > 0)
            {
                unit = Math.Pow(2, csd.SleepCurrentVccq);

                if(unit > 1000)
                    sb.AppendFormat("\tDevice uses {0} mA on Vccq when sleeping", unit / 1000).AppendLine();
                else
                    sb.AppendFormat("\tDevice uses {0} μA on Vccq when sleeping", unit).AppendLine();
            }

            if(csd.ProductionStateAwarenessTimeout > 0)
            {
                unit = Math.Pow(2, csd.ProductionStateAwareness) * 100;

                if(unit > 1000000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} s to switch production state awareness",
                                    unit / 1000000).AppendLine();
                else if(unit > 1000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to switch production state awareness",
                                    unit / 1000).AppendLine();
                else
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to switch production state awareness", unit).
                       AppendLine();
            }

            if(csd.SleepAwakeTimeout > 0)
            {
                unit = Math.Pow(2, csd.SleepAwakeTimeout) * 100;

                if(unit > 1000000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to transition between sleep and standby states",
                                    unit / 1000000).AppendLine();
                else if(unit > 1000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to transition between sleep and standby states",
                                    unit / 1000).AppendLine();
                else
                    sb.AppendFormat("\tDevice takes a maximum of {0} ns to transition between sleep and standby states",
                                    unit).AppendLine();
            }

            if(csd.SleepNotificationTimeout > 0)
            {
                unit = Math.Pow(2, csd.SleepNotificationTimeout) * 10;

                if(unit > 1000000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} s to move to sleep state", unit / 1000000).
                       AppendLine();
                else if(unit > 1000)
                    sb.AppendFormat("\tDevice takes a maximum of {0} ms to move to sleep state", unit / 1000).
                       AppendLine();
                else
                    sb.AppendFormat("\tDevice takes a maximum of {0} μs to move to sleep state", unit).AppendLine();
            }

            sb.AppendFormat("\tDevice has {0} sectors", csd.SectorCount).AppendLine();

            if((csd.SecureWriteProtectInformation & 0x01) == 0x01)
            {
                sb.AppendLine("\tDevice supports secure write protection");

                if((csd.SecureWriteProtectInformation & 0x02) == 0x02)
                    sb.AppendLine("\tDevice has secure write protection enabled");
            }

            unit = csd.MinimumReadPerformance26 * 150;

            if(csd.MinimumReadPerformance26 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 26Mhz mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 26Mhz mode", unit / 1000).
                   AppendLine();

            unit = csd.MinimumReadPerformance26_4 * 150;

            if(csd.MinimumReadPerformance26_4 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 26Mhz 4-bit mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 26Mhz 4-bit mode",
                                unit / 1000).AppendLine();

            unit = csd.MinimumReadPerformance52 * 150;

            if(csd.MinimumReadPerformance52 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s reading in SDR 52Mhz mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in SDR 52Mhz mode", unit / 1000).
                   AppendLine();

            unit = csd.MinimumReadPerformanceDDR52 * 300;

            if(csd.MinimumReadPerformanceDDR52 == 0)
                sb.AppendLine("\tDevice cannot achieve 4.8MB/s reading in DDR 52Mhz mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s reading in DDR 52Mhz mode", unit / 1000).
                   AppendLine();

            unit = csd.MinimumWritePerformance26 * 150;

            if(csd.MinimumWritePerformance26 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 26Mhz mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 26Mhz mode", unit / 1000).
                   AppendLine();

            unit = csd.MinimumWritePerformance26_4 * 150;

            if(csd.MinimumWritePerformance26_4 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 26Mhz 4-bit mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 26Mhz 4-bit mode",
                                unit / 1000).AppendLine();

            unit = csd.MinimumWritePerformance52 * 150;

            if(csd.MinimumWritePerformance52 == 0)
                sb.AppendLine("\tDevice cannot achieve 2.4MB/s writing in SDR 52Mhz mode");
            else
                sb.AppendFormat("\tDevice can achieve a minimum of {0}MB/s writing in SDR 52Mhz mode", unit / 1000).
                   AppendLine();

            unit = csd.MinimumWritePerformanceDDR52 * 300;

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

            if((csd.DeviceType & 0x01) == 0x01)
                sb.AppendLine("\tDevice supports 26 Mhz mode");

            if((csd.DeviceType & 0x02) == 0x02)
                sb.AppendLine("\tDevice supports 52 Mhz mode");

            if((csd.DeviceType & 0x04) == 0x04)
                sb.AppendLine("\tDevice supports DDR 52 Mhz mode at 1.8V or 3V");

            if((csd.DeviceType & 0x08) == 0x08)
                sb.AppendLine("\tDevice supports DDR 52 Mhz mode 1.2V");

            if((csd.DeviceType & 0x10) == 0x10)
                sb.AppendLine("\tDevice supports HS-200 mode (SDR 200Mhz) at 1.8V");

            if((csd.DeviceType & 0x20) == 0x20)
                sb.AppendLine("\tDevice supports HS-200 mode (SDR 200Mhz) at 1.2V");

            if((csd.DeviceType & 0x40) == 0x40)
                sb.AppendLine("\tDevice supports HS-400 mode (DDR 200Mhz) at 1.8V");

            if((csd.DeviceType & 0x80) == 0x80)
                sb.AppendLine("\tDevice supports HS-400 mode (DDR 200Mhz) at 1.2V");

            sb.AppendFormat("\tCSD version 1.{0} revision 1.{1}", csd.Structure, csd.Revision).AppendLine();

            if((csd.StrobeSupport & 0x01) == 0x01)
            {
                sb.AppendLine("\tDevice supports enhanced strobe mode");

                sb.AppendLine((csd.BusWidth & 0x80) == 0x80
                                  ? "\tDevice uses strobe during Data Out, CRC and CMD responses"
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

            if((csd.PartitionConfiguration & 0x80) == 0x80)
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
                    sb.AppendFormat("\tUnknown enabled boot partition code {0}",
                                    (csd.PartitionConfiguration & 0x38) >> 3).AppendLine();

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

            if((csd.FirmwareConfiguration & 0x01) == 0x01)
                sb.AppendLine("\tFirmware updates are permanently disabled");

            if(csd.RPMBSize > 0)
                sb.AppendFormat("\tDevice has a {0} KiB replay protected memory block", csd.RPMBSize * 128).
                   AppendLine();

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
                    sb.AppendFormat("\tDevice emulates unknown sector size indicated by code {0}",
                                    csd.NativeSectorSize).AppendLine();

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
}