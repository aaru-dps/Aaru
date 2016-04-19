// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains enumerations that are common to all operating systems
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

namespace DiscImageChef.Devices
{
    public enum DeviceType
    {
        Unknown,
        ATA,
        ATAPI,
        SCSI,
        SecureDigital,
        MMC,
        NVMe
    }

    #region ATA Commands
    /// <summary>
    /// All known ATA commands
    /// </summary>
    public enum AtaCommands : byte
    {
        #region Commands defined on Western Digital WD1000 Winchester Disk Controller

        /// <summary>
        /// Formats a track
        /// </summary>
        FormatTrack = 0x50,
        /// <summary>
        /// Reads sectors
        /// </summary>
        ReadOld = 0x20,
        /// <summary>
        /// Reads sectors using DMA
        /// </summary>
        ReadDmaOld = 0x28,
        /// <summary>
        /// Calibrates the position of the heads
        /// Includes all commands from 0x10 to 0x1F
        /// </summary>
        Restore = 0x10,
        /// <summary>
        /// Seeks to a certain cylinder
        /// </summary>
        Seek = 0x70,
        /// <summary>
        /// Writes sectors
        /// </summary>
        WriteOld = 0x30,
        #endregion Commands defined on Western Digital WD1000 Winchester Disk Controller

        #region Commands defined on ATA rev. 4c

        /// <summary>
        /// Acknowledges media change
        /// </summary>
        AckMediaChange = 0xDB,
        /// <summary>
        /// Sends vendor-specific information that may be required in order to pass diagnostics
        /// </summary>
        PostBoot = 0xDC,
        /// <summary>
        /// Prepares a removable drive to respond to boot
        /// </summary>
        PreBoot = 0xDD,
        /// <summary>
        /// Checks drive power mode
        /// </summary>
        CheckPowerMode = 0xE5,
        /// <summary>
        /// Checks drive power mode
        /// </summary>
        CheckPowerModeAlternate = 0x98,
        /// <summary>
        /// Locks the door of the drive
        /// </summary>
        DoorLock = 0xDE,
        /// <summary>
        /// Unlocks the door of the drive
        /// </summary>
        DoorUnLock = 0xDF,
        /// <summary>
        /// Executes internal drive diagnostics
        /// </summary>
        ExecuteDriveDiagnostic = 0x90,
        /// <summary>
        /// Gets a sector containing drive identification and capabilities
        /// </summary>
        IdentifyDrive = 0xEC,
        /// <summary>
        /// Requests the drive to enter idle status
        /// </summary>
        Idle = 0xE3,
        /// <summary>
        /// Requests the drive to enter idle status
        /// </summary>
        IdleAlternate = 0x97,
        /// <summary>
        /// Requests the drive to enter idle status immediately
        /// </summary>
        IdleImmediate = 0xE1,
        /// <summary>
        /// Requests the drive to enter idle status immediately
        /// </summary>
        IdleImmediateAlternate = 0x95,
        /// <summary>
        /// Changes heads and sectors per cylinder for the drive
        /// </summary>
        InitializeDriveParameters = 0x91,
        /// <summary>
        /// Does nothing
        /// </summary>
        Nop = 0x00,
        /// <summary>
        /// Reads sectors using PIO transfer
        /// </summary>
        Read = 0x21,
        /// <summary>
        /// Reads the content of the drive's buffer
        /// </summary>
        ReadBuffer = 0xE4,
        /// <summary>
        /// Reads sectors using DMA transfer
        /// </summary>
        ReadDma = 0xC9,
        /// <summary>
        /// Reads sectors using DMA transfer, retrying on error
        /// </summary>
        ReadDmaRetry = 0xC8,
        /// <summary>
        /// Reads a sector including ECC bytes without checking them
        /// </summary>
        ReadLong = 0x23,
        /// <summary>
        /// Reads a sector including ECC bytes without checking them, retrying on error
        /// </summary>
        ReadLongRetry = 0x22,
        /// <summary>
        /// Reads multiple sectors generating interrupts at block transfers
        /// </summary>
        ReadMultiple = 0xC4,
        /// <summary>
        /// Reads sectors using PIO transfer, retrying on error
        /// </summary>
        ReadRetry = 0x20,
        /// <summary>
        /// Verifies sectors readability without transferring them
        /// </summary>
        ReadVerify = 0x41,
        /// <summary>
        /// Verifies sectors readability without transferring them, retrying on error
        /// </summary>
        ReadVerifyRetry = 0x40,
        /// <summary>
        /// Moves the heads to cylinder 0
        /// </summary>
        Recalibrate = Restore,
        /// <summary>
        /// Sets drive parameters
        /// </summary>
        SetFeatures = 0xEF,
        /// <summary>
        /// Enables <see cref="ReadMultiple"/> and <see cref="WriteMultiple"/> and sets the block length for these commands
        /// </summary>
        SetMultipleMode = 0xC6,
        /// <summary>
        /// Causes the drive to stop and sleep until a hardware or software reset
        /// </summary>
        Sleep = 0xE6,
        /// <summary>
        /// Causes the drive to stop and sleep until a hardware or software reset
        /// </summary>
        SleepAlternate = 0x99,
        /// <summary>
        /// Sets the drive to enter Standby mode
        /// </summary>
        Standby = 0xE2,
        /// <summary>
        /// Sets the drive to enter Standby mode
        /// </summary>
        StandbyAlternate = 0x96,
        /// <summary>
        /// Sets the drive to enter Standby mode, immediately
        /// </summary>
        StandbyImmediate = 0xE0,
        /// <summary>
        /// Sets the drive to enter Standby mode, immediately
        /// </summary>
        StandbyImmediateAlternate = 0x94,
        /// <summary>
        /// Writes sectors using PIO transfer
        /// </summary>
        Write = 0x31,
        /// <summary>
        /// Writes data to the drive's sector buffer
        /// </summary>
        WriteBuffer = 0xE8,
        /// <summary>
        /// Writes sectors using DMA transfer
        /// </summary>
        WriteDma = 0xCB,
        /// <summary>
        /// Writes sectors using DMA transfer, retrying on error
        /// </summary>
        WriteDmaRetry = 0xCA,
        /// <summary>
        /// Writes sectors with custom ECC
        /// </summary>
        WriteLong = 0x33,
        /// <summary>
        /// Writes sectors with custom ECC, retrying on error
        /// </summary>
        WriteLongRetry = 0x32,
        /// <summary>
        /// Writes several sectors at once setting interrupts on end of block
        /// </summary>
        WriteMultiple = 0xC5,
        /// <summary>
        /// Writes the same data to several sector
        /// </summary>
        WriteSame = 0xE9,
        /// <summary>
        /// Writes sectors using PIO transfer, retrying on error
        /// </summary>
        WriteRetry = 0x30,
        /// <summary>
        /// Writes sectors verifying them immediately after write
        /// </summary>
        WriteVerify = 0x3C,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_8x = 0x80,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_9A = 0x9A,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_C0 = 0xC0,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_C1 = 0xC1,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_C2 = 0xC2,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_C3 = 0xC3,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F0 = 0xF0,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F1 = 0xF1,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F2 = 0xF2,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F3 = 0xF3,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F4 = 0xF4,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F5 = 0xF5,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F6 = 0xF6,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F7 = 0xF7,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F8 = 0xF8,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_F9 = 0xF9,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FA = 0xFA,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FB = 0xFB,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FC = 0xFC,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FD = 0xFD,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FE = 0xFE,
        /// <summary>
        /// Unknown vendor command
        /// </summary>
        Vendor_FF = 0xFF,
        #endregion Commands defined on ATA rev. 4c

        #region Commands defined on ATA-2 rev. 4c

        /// <summary>
        /// Alters the device microcode
        /// </summary>
        DownloadMicrocode = 0x92,
        /// <summary>
        /// Ejects the removable medium on the device
        /// </summary>
        MediaEject = 0xED,
        #endregion Commands defined on ATA-2 rev. 4c

        #region Commands defined on ATA-3 rev. 7b

        /// <summary>
        /// Gets a sector containing drive identification and capabilities
        /// </summary>
        IdentifyDriveDma = 0xEE,
        /// <summary>
        /// Disables the security lock
        /// </summary>
        SecurityDisablePassword = 0xF6,
        /// <summary>
        /// Enables usage of <see cref="SecurityEraseUnit"/> command
        /// </summary>
        SecurityErasePrepare = 0xF3,
        /// <summary>
        /// Erases all user data and isables the security lock
        /// </summary>
        SecurityEraseUnit = 0xF4,
        /// <summary>
        /// Sets the security freeze lock preventing any security command from working until hardware reset
        /// </summary>
        SecurityFreezeLock = 0xF5,
        /// <summary>
        /// Sets the device user or master password
        /// </summary>
        SecuritySetPassword = 0xF1,
        /// <summary>
        /// Unlocks device
        /// </summary>
        SecurityUnlock = 0xF2,
        /// <summary>
        /// SMART operations
        /// </summary>
        Smart = 0xB0,
        #endregion Commands defined on ATA-3 rev. 7b

        #region Commands defined on CompactFlash Specification

        /// <summary>
        /// Pre-erases and conditions data sectors
        /// </summary>
        EraseSectors = 0xC0,
        /// <summary>
        /// Requests extended error information
        /// </summary>
        RequestSense = 0x03,
        /// <summary>
        /// Provides a way to determine the exact number of times a sector has been erases and programmed
        /// </summary>
        TranslateSector = 0x87,
        /// <summary>
        /// For CompactFlash cards that do not support security mode, this commands is equal to <see cref="Nop"/>
        /// For those that do, this command is equal to <see cref="SecurityFreezeLock"/>
        /// </summary>
        WearLevel = 0xF5,
        /// <summary>
        /// Writes a block of sectors without erasing them previously
        /// </summary>
        WriteMultipleWithoutErase = 0xCD,
        /// <summary>
        /// Writes sectors without erasing them previously
        /// </summary>
        WriteWithoutErase = 0x38,
        #endregion Commands defined on CompactFlash Specification

        #region Commands defined on ATA/ATAPI-4 rev. 18

        /// <summary>
        /// Resets a device
        /// </summary>
        DeviceReset = 0x08,
        /// <summary>
        /// Requests the device to flush the write cache and write it to the media
        /// </summary>
        FlushCache = 0xE7,
        /// <summary>
        /// Gets media status
        /// </summary>
        GetMediaStatus = 0xDA,
        /// <summary>
        /// Gets a sector containing drive identification and capabilities, for ATA devices
        /// </summary>
        IdentifyDevice = IdentifyDrive,
        /// <summary>
        /// Gets a sector containing drive identification and capabilities, for ATAPI devices
        /// </summary>
        IdentifyPacketDevice = 0xA1,
        /// <summary>
        /// Locks the media on the device
        /// </summary>
        MediaLock = DoorLock,
        /// <summary>
        /// Unlocks the media on the device
        /// </summary>
        MediaUnLock = DoorUnLock,
        /// <summary>
        /// Sends a command packet
        /// </summary>
        Packet = 0xA0,
        /// <summary>
        /// Queues a read of sectors
        /// </summary>
        ReadDmaQueued = 0xC7,
        /// <summary>
        /// Returns the native maximum address in factory default condition
        /// </summary>
        ReadNativeMaxAddress = 0xF8,
        /// <summary>
        /// Used to provide data transfer and/or status of a previous command (queue or packet)
        /// </summary>
        Service = 0xA2,
        /// <summary>
        /// Redefines the maximum user-accessible address space
        /// </summary>
        SetMaxAddress = 0xF9,
        /// <summary>
        /// Queues a write of sectors
        /// </summary>
        WriteDmaQueued = 0xCC,
        #endregion Commands defined on ATA/ATAPI-4 rev. 18

        #region Commands defined on ATA/ATAPI-6 rev. 3b

        /// <summary>
        /// Determines if the device supports the Media Card Pass Through Command feature set
        /// </summary>
        CheckMediaCardType = 0xD1,
        /// <summary>
        /// Device Configuration Overlay feature set
        /// </summary>
        DevideConfiguration = 0xB1,
        /// <summary>
        /// Requests the device to flush the write cache and write it to the media (48-bit)
        /// </summary>
        FlushCacheExt = 0xEA,
        /// <summary>
        /// Reads sectors using DMA transfer, retrying on error (48-bit)
        /// </summary>
        ReadDmaExt = 0x25,
        /// <summary> (48-bit)
        /// Queues a read of sectors
        /// </summary>
        ReadDmaQueuedExt = 0x26,
        /// <summary>
        /// Reads sectors using PIO transfer, retrying on error (48-bit)
        /// </summary>
        ReadExt = 0x24,
        /// <summary>
        /// Returns the indicated log to the host (48-bit)
        /// </summary>
        ReadLogExt = 0x2F,
        /// <summary>
        /// Reads multiple sectors generating interrupts at block transfers (48-bit)
        /// </summary>
        ReadMultipleExt = 0x29,
        /// <summary>
        /// Returns the native maximum address in factory default condition (48-bit)
        /// </summary>
        ReadNativeMaxAddressExt = 0x27,
        /// <summary>
        /// Verifies sectors readability without transferring them, retrying on error (48-bit)
        /// </summary>
        ReadVerifyExt = 0x42,
        /// <summary>
        /// Sends a SET MAX subcommand, <see cref="AtaSetMaxSubCommands"/>
        /// </summary>
        SetMaxCommands = 0xF9,
        /// <summary>
        /// Redefines the maximum user-accessible address space (48-bit)
        /// </summary>
        SetMaxAddressExt = 0x37,
        /// <summary>
        /// Writes sectors using DMA transfer, retrying on error (48-bit)
        /// </summary>
        WriteDmaExt = 0x35,
        /// <summary>
        /// Queues a write of sectors (48-bit)
        /// </summary>
        WriteDmaQueuedExt = 0x36,
        /// <summary>
        /// Writes sectors using PIO transfer, retrying on error (48-bit)
        /// </summary>
        WriteExt = 0x34,
        /// <summary>
        /// Writes data to the indicated log (48-bit)
        /// </summary>
        WriteLogExt = 0x3F,
        /// <summary>
        /// Writes several sectors at once setting interrupts on end of block (48-bit)
        /// </summary>
        WriteMultipleExt = 0x39,
        #endregion Commands defined on ATA/ATAPI-6 rev. 3b

        #region Commands defined on ATA/ATAPI-7 rev. 4b

        /// <summary>
        /// Configurates the operating parameters for a stream
        /// </summary>
        ConfigureStream = 0x51,
        /// <summary>
        /// Reads data on an alloted time using DMA
        /// </summary>
        ReadStreamDmaExt = 0x2A,
        /// <summary>
        /// Reads data on an alloted time using PIO
        /// </summary>
        ReadStreamExt = 0x2B,
        /// <summary>
        /// Writes data on an alloted time using DMA
        /// </summary>
        WriteStreamDmaExt = 0x3A,
        /// <summary>
        /// Writes data on an alloted time using PIO
        /// </summary>
        WriteStreamExt = 0x3B,
        #endregion Commands defined on ATA/ATAPI-7 rev. 4b

        #region Commands defined on ATA/ATAPI-8 rev. 3f

        /// <summary>
        /// Sends a Non Volatile Cache subcommand. <see cref="AtaNonVolatileCacheSubCommands"/> 
        /// </summary>
        NonVolatileCacheCommand = 0xB6,
        /// <summary>
        /// Retrieves security protocol information or the results from <see cref="TrustedSend"/> commands
        /// </summary>
        TrustedReceive = 0x5C,
        /// <summary>
        /// Retrieves security protocol information or the results from <see cref="TrustedSend"/> commands, using DMA transfers
        /// </summary>
        TrustedReceiveDma = 0x5D,
        /// <summary>
        /// Sends one or more Security Protocol commands
        /// </summary>
        TrustedSend = 0x5E,
        /// <summary>
        /// Sends one or more Security Protocol commands, using DMA transfers
        /// </summary>
        TrustedSendDma = 0x5F,
        /// <summary>
        /// Writes sectors using DMA transfer, retrying on error (48-bit), not returning until the operation is complete
        /// </summary>
        WriteDmaFuaExt = 0x3D,
        /// <summary>
        /// Queues a write of sectors (48-bit), not returning until the operation is complete
        /// </summary>
        WriteDmaQueuedFuaExt = 0x3E,
        /// <summary>
        /// Writes several sectors at once setting interrupts on end of block (48-bit), not returning until the operation is complete
        /// </summary>
        WriteMultipleFuaExt = 0xCE,
        /// <summary>
        /// Writes a sector that will give an uncorrectable error on any read operation
        /// </summary>
        WriteUncorrectableExt = 0x45,
        #endregion Commands defined on ATA/ATAPI-8 rev. 3f

        #region Commands defined on ATA/ATAPI Command Set 2 (ACS-2) rev. 2

        /// <summary>
        /// Provides information for device optimization
        /// In SSDs, this contains trimming
        /// </summary>
        DataSetManagement = 0x06,
        /// <summary>
        /// Alters the device microcode, using DMA transfers
        /// </summary>
        DownloadMicrocodeDma = 0x93,
        /// <summary>
        /// Reads the content of the drive's buffer, using DMA transfers
        /// </summary>
        ReadBufferDma = 0xE9,
        /// <summary>
        /// Reads sectors using NCQ
        /// </summary>
        ReadFpDmaQueued = 0x60,
        /// <summary>
        /// Returns the indicated log to the host (48-bit)
        /// </summary>
        ReadLogDmaExt = 0x47,
        /// <summary>
        /// Requests SPC-4 style error data
        /// </summary>
        RequestSenseDataExt = 0x0B,
        SanitizeCommands = 0xB4,
        /// <summary>
        /// Executes a Security Protocol command that does not require a transfer of data
        /// </summary>
        TrustedNonData = 0x5B,
        /// <summary>
        /// Writes data to the drive's sector buffer, using DMA transfers
        /// </summary>
        WriteBufferDma = 0xE8,
        /// <summary>
        /// Writes sectors using NCQ
        /// </summary>
        WriteFpDmaQueued = 0x61,
        #endregion Commands defined on ATA/ATAPI Command Set 2 (ACS-2) rev. 2

        #region Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5

        /// <summary>
        /// Sends <see cref="AtaNCQQueueManagementSubcommands"/>
        /// </summary>
        NCQQueueManagement = 0x63,
        /// <summary>
        /// Sets the device date and time
        /// </summary>
        SetDateAndTimeExt = 0x77,

        #endregion Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5

        #region Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 6
        NativeMaxAddress = 0x78
        #endregion Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 6
    }
    #endregion ATA Commands

    #region ATA SMART SubCommands
    /// <summary>
    /// All known ATA SMART sub-commands
    /// </summary>
    public enum AtaSmartSubCommands : byte
    {
        #region Commands defined on ATA-3 rev. 7b

        /// <summary>
        /// Disables all SMART capabilities
        /// </summary>
        Disable = 0xD9,
        /// <summary>
        /// Enables/disables SMART attribute autosaving
        /// </summary>
        EnableDisableAttributeAutosave = 0xD2,
        /// <summary>
        /// Enables all SMART capabilities
        /// </summary>
        Enable = 0xD8,
        /// <summary>
        /// Returns the device's SMART attributes thresholds
        /// </summary>
        ReadAttributeThresholds = 0xD1,
        /// <summary>
        /// Returns the device's SMART attributes values
        /// </summary>
        ReadAttributeValues = 0xD0,
        /// <summary>
        /// Communicates device reliability status
        /// </summary>
        ReturnStatus = 0xDA,
        /// <summary>
        /// Saves any attribute values immediately
        /// </summary>
        SaveAttributeValues = 0xD3,
        #endregion Commands defined on ATA-3 rev. 7b

        #region Commands defined on ATA/ATAPI-4 rev. 18

        /// <summary>
        /// Causes the device to immediately initiate a SMART data collection and saves it to the device
        /// </summary>
        ExecuteOfflineImmediate = 0xD4,
        /// <summary>
        /// Returns the device's SMART attributes values
        /// </summary>
        ReadData = ReadAttributeValues,
        #endregion Commands defined on ATA/ATAPI-4 rev. 18

        #region Commands defined on ATA/ATAPI-5 rev. 3

        /// <summary>
        /// Returns the indicated log to the host
        /// </summary>
        ReadLog = 0xD5,
        /// <summary>
        /// Writes data to the indicated log
        /// </summary>
        WriteLog = 0xD6

        #endregion Commands defined on ATA/ATAPI-5 rev. 3
    }
    #endregion ATA SMART SubCommands

    #region ATA Device Configuration Overlay SubCommands
    /// <summary>
    /// All known ATA DEVICE CONFIGURATION sub-commands
    /// </summary>
    public enum AtaDeviceConfigurationSubCommands : byte
    {
        #region Commands defined on ATA/ATAPI-6 rev. 3b

        /// <summary>
        /// Disables any change made by <see cref="Set"/> 
        /// </summary>
        Restore = 0xC0,
        /// <summary>
        /// Prevents any <see cref="AtaDeviceConfigurationSubCommands"/> from working until a power down cycle.
        /// </summary>
        FreezeLock = 0xC1,
        /// <summary>
        /// Indicates the selectable commands, modes, and feature sets the device supports
        /// </summary>
        Identify = 0xC2,
        /// <summary>
        /// Modifies the commands, modes and features sets the device will obey to
        /// </summary>
        Set = 0xC3

        #endregion Commands defined on ATA/ATAPI-6 rev. 3b
    }
    #endregion ATA Device Configuration Overlay SubCommands

    #region ATA SET MAX SubCommands
    /// <summary>
    /// All known ATA SET MAX sub-commands
    /// </summary>
    public enum AtaSetMaxSubCommands : byte
    {
        #region Commands defined on ATA/ATAPI-6 rev. 3b

        /// <summary>
        /// Redefines the maximum user-accessible address space
        /// </summary>
        Address = 0x00,
        /// <summary>
        /// Disables any other <see cref="AtaSetMaxSubCommands"/> until power cycle
        /// </summary>
        FreezeLock = 0x04,
        /// <summary>
        /// Disables any other <see cref="AtaSetMaxSubCommands"/> except <see cref="UnLock"/> and <see cref="FreezeLock"/> until power cycle
        /// </summary>
        Lock = 0x02,
        /// <summary>
        /// Sets the device password
        /// </summary>
        SetPassword = 0x01,
        /// <summary>
        /// Disables <see cref="Lock"/> 
        /// </summary>
        UnLock = 0x03,
        #endregion Commands defined on ATA/ATAPI-6 rev. 3b
    }
    #endregion ATA SET MAX SubCommands

    #region ATA Non Volatile Cache SubCommands
    /// <summary>
    /// All known ATA NV CACHE sub-commands
    /// </summary>
    public enum AtaNonVolatileCacheSubCommands : byte
    {
        #region Commands defined on ATA/ATAPI-8 rev. 3f

        /// <summary>
        /// Adds the specified LBA to the Non Volatile Cache
        /// </summary>
        AddLbaToNvCache = 0x10,
        /// <summary>
        /// Ensures there is enough free space in the Non Volatile Cache
        /// </summary>
        FlushNvCache = 0x14,
        /// <summary>
        /// Requests a list of LBAs actually stored in the Non Volatile Cache
        /// </summary>
        QueryNvCachePinnedSet = 0x12,
        /// <summary>
        /// Requests a list of LBAs accessed but not in the Non Volatile Cache
        /// </summary>
        QueryNvCacheMisses = 0x13,
        /// <summary>
        /// Removes the specified LBA from the Non Volatile Cache Pinned Set
        /// </summary>
        RemoveLbaFromNvCache = 0x11,
        /// <summary>
        /// Disables the Non Volatile Cache Power Mode
        /// <see cref="SetNvCachePowerMode"/> 
        /// </summary>
        ReturnFromNvCachePowerMode = 0x01,
        /// <summary>
        /// Enables the Non Volatile Cache Power Mode, so the device tries to serve all accesses from the Non Volatile Cache
        /// </summary>
        SetNvCachePowerMode = 0x00

        #endregion Commands defined on ATA/ATAPI-8 rev. 3f
    }
    #endregion ATA Non Volatile Cache SubCommands

    #region ATA Sanitize SubCommands
    /// <summary>
    /// All known ATA SANITIZE sub-commands
    /// </summary>
    public enum AtaSanitizeSubCommands : ushort
    {
        #region Commands defined on ATA/ATAPI Command Set 2 (ACS-2) rev. 2

        /// <summary>
        /// Causes a block erase on all user data
        /// </summary>
        BlockEraseExt = 0x0012,
        /// <summary>
        /// Changes the internal encryption keys. Renders user data unusable
        /// </summary>
        CryptoScrambleExt = 0x0011,
        /// <summary>
        /// Fills user data with specified pattern
        /// </summary>
        OverwriteExt = 0x0014,
        /// <summary>
        /// Disables all <see cref="AtaSanitizeSubCommands"/> except <see cref="Status"/>
        /// </summary>
        FreezeLockExt = 0x0020,
        /// <summary>
        /// Gets the status of the sanitizing
        /// </summary>
        Status = 0x0000,
        #endregion Commands defined on ATA/ATAPI Command Set 2 (ACS-2) rev. 2

        #region Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5

        /// <summary>
        /// Disables the <see cref="FreezeLockExt"/> command
        /// </summary>
        AntiFreezeLockExt = 0x0040

        #endregion Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5
    }
    #endregion ATA Sanitize SubCommands

    #region ATA NCQ Queue Management SubCommands
    /// <summary>
    /// All known ATA NCQ QUEUE MANAGEMENT sub-commands
    /// </summary>
    public enum AtaNCQQueueManagementSubcommands : byte
    {
        #region Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5

        /// <summary>
        /// Aborts pending NCQ commands
        /// </summary>
        AbortNCQQueue = 0x00,
        /// <summary>
        /// Controls how NCQ Streaming commands are processed by the device
        /// </summary>
        DeadlineHandling = 0x01,
        #endregion Commands defined on ATA/ATAPI Command Set 3 (ACS-3) rev. 5
    }
    #endregion ATA NCQ Queue Management SubCommands

    /// <summary>
    /// All known SASI commands
    /// Commands 0x00 to 0x1F are 6-byte
    /// Commands 0x20 to 0x3F are 10-byte
    /// Commands 0x40 to 0x5F are 8-byte
    /// Commands 0xA0 to 0xBF are 12-byte
    /// </summary>
    #region SASI Commands
    public enum SasiCommands : byte
    {
        #region SASI Class 0 commands

        /// <summary>
        /// Returns zero status if requested unit is on and ready.
        /// SASI rev. 0a
        /// </summary>
        TestUnitReady = 0x00,
        /// <summary>
        /// Sets the unit to a specific known state.
        /// SASI rev. 0a
        /// </summary>
        RezeroUnit = 0x01,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        RequestSyndrome = 0x02,
        /// <summary>
        /// Returns unit sense.
        /// SASI rev. 0a
        /// </summary>
        RequestSense = 0x03,
        /// <summary>
        /// Formats the entire media.
        /// SASI rev. 0a
        /// </summary>
        FormatUnit = 0x04,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        CheckTrackFormat = 0x05,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        FormatTrack = 0x06,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        FormatBadTrack = 0x06,
        /// <summary>
        /// Reads a block from the device.
        /// SASI rev. 0a
        /// </summary>
        Read = 0x08,
        /// <summary>
        /// SASI rev. 0a
        /// Unknown
        /// </summary>
        WriteProtectSector = 0x09,
        /// <summary>
        /// Writes a block to the device.
        /// SASI rev. 0a
        /// </summary>
        Write = 0x0A,
        /// <summary>
        /// Moves the device reading mechanism to the specified block.
        /// SASI rev. 0a
        /// </summary>
        Seek = 0x0B,
        /// <summary>
        /// Found on a vendor source code
        /// </summary>
        InitDriveCharacteristics = 0x0C,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        VerifyRestore = 0x0D,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        AssignAlternateDiskTrack = 0x0E,
        /// <summary>
        /// Writes a File Mark on the device.
        /// SASI rev. 0c
        /// </summary>
        WriteFileMark = 0x0F,
        /// <summary>
        /// Reserves the device for use by the iniator.
        /// SASI rev. 0a
        /// </summary>
        ReserveUnitOld = 0x12,
        /// <summary>
        /// Gets information about a device
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        Inquiry = 0x12,
        /// <summary>
        /// Release the device from the reservation.
        /// SASI rev. 0a
        /// </summary>
        ReleaseUnitOld = 0x13,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        WriteProtectDrive = 0x14,
        /// <summary>
        /// Writes and verifies blocks to the device.
        /// SASI rev. 0c
        /// </summary>
        WriteAndVerifyOld = 0x14,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        ReleaseWriteProtect = 0x15,
        /// <summary>
        /// Verifies blocks.
        /// SASI rev. 0c
        /// </summary>
        VerifyOld = 0x15,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        ReadNoSeek = 0x16,
        /// <summary>
        /// Gets the number of blocks in device.
        /// SASI rev. 0c
        /// </summary>
        ReadCapacityOld = 0x16,
        /// <summary>
        /// Reserves the device for use by the iniator.
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ReserveUnit = 0x16,
        /// <summary>
        /// Release the device from the reservation.
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ReleaseUnit = 0x17,
        /// <summary>
        /// Searches data on blocks
        /// SASI rev. 0a
        /// </summary>
        SearchDataEqualOld = 0x17,
        /// <summary>
        /// Searches data on blocks using major than or equal comparison
        /// SASI rev. 0a
        /// </summary>
        SearchDataHighOld = 0x18,
        /// <summary>
        /// Searches data on blocks using minor than or equal comparison
        /// SASI rev. 0a
        /// </summary>
        SearchDataLowOld = 0x19,
        /// <summary>
        /// Reads analysis data from a device
        /// SASI rev. 0a
        /// </summary>
        ReadDiagnosticOld = 0x1A,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        VerifyData = 0x1B,
        /// <summary>
        /// Requests a device to run a diagnostic
        /// SASI rev. 0c
        /// </summary>
        WriteDiagnosticOld = 0x1B,
        /// <summary>
        /// Requests the data after completion of a <see cref="WriteDiagnostic"/> 
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ReadDiagnostic = 0x1C,
        /// <summary>
        /// Requests the device to perform diagnostics
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        WriteDiagnostic = 0x1D,
        /// <summary>
        /// Gets information about a device
        /// SASI rev. 0c
        /// </summary>
        InquiryOld = 0x1F,
        #endregion SASI Class 0 commands

        #region SASI Class 1 commands

        /// <summary>
        /// SASI rev. 0a
        /// Unknown
        /// </summary>
        Copy = 0x20,
        /// <summary>
        /// SASI rev. 0a
        /// Unknown
        /// </summary>
        Restore = 0x21,
        /// <summary>
        /// SASI rev. 0a
        /// Unknown
        /// </summary>
        Backup = 0x22,
        /// <summary>
        /// SASI rev. 0a
        /// Unknown
        /// </summary>
        SetBlockLimitsOlder = 0x26,
        /// <summary>
        /// Sets write or read limits from a specified block
        /// SASI rev. 0c
        /// </summary>
        SetBlockLimitsOld = 0x28,
        /// <summary>
        /// Reads blocks from device
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ExtendedAddressRead = 0x28,
        /// <summary>
        /// Writes blocks to the device
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ExtendedAddressWrite = 0x2A,
        /// <summary>
        /// Writes blocks to the device and then verifies them
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        WriteAndVerify = 0x2E,
        /// <summary>
        /// Verifies blocks on the device
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        Verify = 0x2F,
        /// <summary>
        /// Searches data on blocks
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        SearchDataEqual = 0x31,
        /// <summary>
        /// Searches data on blocks using major than or equal comparison
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        SearchDataHigh = 0x30,
        /// <summary>
        /// Searches data on blocks using minor than or equal comparison
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        SearchDataLow = 0x32,

        #endregion SASI Class 1 commands

        #region SASI Class 2 commands

        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        Load = 0x40,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        Unload = 0x41,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        Rewind = 0x42,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        SpaceForward = 0x43,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        SpaceForwardFileMark = 0x44,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        SpaceReverse = 0x45,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        SpaceReverseFileMark = 0x46,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        TrackSelect = 0x47,
        /// <summary>
        /// Reads blocks from device
        /// SASI rev. 0a
        /// </summary>
        Read8 = 0x48,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        ReadVerify = 0x49,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        ReadDiagnosticClass2 = 0x4A,
        /// <summary>
        /// Writes blocks to device
        /// SASI rev. 0a
        /// </summary>
        Write8 = 0x4B,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        WriteFileMarkClass2 = 0x4C,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        WriteExtended = 0x4D,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        WriteExtendedFileMark = 0x4E,
        /// <summary>
        /// Unknown
        /// SASI rev. 0a
        /// </summary>
        WriteErase = 0x4F,
        /// <summary>
        /// Writes and verifies blocks to the device.
        /// SASI rev. 0c
        /// </summary>
        WriteVerify8 = 0x54,
        /// <summary>
        /// Verifies blocks.
        /// SASI rev. 0c
        /// </summary>
        Verify8 = 0x55,
        /// <summary>
        /// Searches data on blocks using major than or equal comparison
        /// SASI rev. 0c
        /// </summary>
        SearchDataHigh8 = 0x57,
        /// <summary>
        /// Searches data on blocks
        /// SASI rev. 0c
        /// </summary>
        SearchDataEqual8 = 0x58,
        /// <summary>
        /// Searches data on blocks using minor than or equal comparison
        /// SASI rev. 0c
        /// </summary>
        SearchDataLow8 = 0x59,
        #endregion SASI Class 2 commands

        #region SASI Class 3 commands

        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        Skip = 0x60,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        Space = 0x61,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        Return = 0x62,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        Tab = 0x63,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        ReadControl = 0x64,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        Write3 = 0x65,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        WriteControl = 0x66,
        #endregion SASI Class 3 commands

        #region SASI Class 5 commands

        /// <summary>
        /// Gets the number of blocks in device.
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        ReadCapacity = 0xA5,
        /// <summary>
        /// Sets write or read limits from a specified block
        /// ANSI X3T9.3 No. 185 (SASI)
        /// </summary>
        SetBlockLimits = 0xA9,

        #endregion SASI Class 5 commands

        #region SASI Class 6 commands

        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        DefineFloppyDiskTrackFormat = 0xC0,
        /// <summary>
        /// Unknown vendor command in X68000
        /// </summary>
        Specify = 0xC2,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        FormatDriveErrorMap = 0xC4,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        ReadErrorMap = 0xC5,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        ReadDriveType = 0xC6,
        #endregion SASI Class 6 commands

        #region SASI Class 7 commands

        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        RamDiagnostic = 0xE0,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        WriteECC = 0xE1,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        ReadID = 0xE2,
        /// <summary>
        /// SASI rev. 0a
        /// </summary>
        DriveDiagnostic = 0xE3,
        /// <summary>
        /// Found on a vendor source code
        /// </summary>
        ControllerDiagnostic = 0xE4,
        /// <summary>
        /// Found on a vendor document
        /// </summary>
        ReadLong = 0xE5,
        /// <summary>
        /// Found on a vendor document
        /// </summary>
        WriteLong = 0xE6

        #endregion SASI Class 7 commands
    }
    #endregion SASI Commands

    #region SCSI Commands
    /// <summary>
    /// All known SCSI and ATAPI commands
    /// </summary>
    public enum ScsiCommands : byte
    {
        #region SCSI Primary Commands (SPC)

        /// <summary>
        /// Commands used to obtain information about the access controls that are active
        /// SPC-4 rev. 16
        /// </summary>
        AccessControlIn = 0x86,
        /// <summary>
        /// Commands used to limit or grant access to LUNs
        /// SPC-4 rev. 16
        /// </summary>
        AccessControlOut = 0x87,
        /// <summary>
        /// Modifies the operating definition of the device with respect to commmands.
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ChangeDefinition = 0x40,
        /// <summary>
        /// Compares data between two devices
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Compare = 0x39,
        /// <summary>
        /// Copies data between two devices
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Copy = 0x18,
        /// <summary>
        /// Copies data between two devices and verifies the copy is correct.
        /// ECMA-111 (SCSI-1)
        /// </summary>
        CopyAndVerify = 0x3A,
        /// <summary>
        /// Copies data between two devices
        /// SPC-2 rev. 20
        /// </summary>
        ExtendedCopy = 0x83,
        /// <summary>
        /// Requests information about the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Inquiry = SasiCommands.Inquiry,
        /// <summary>
        /// Manages device statistics
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        LogSelect = 0x4C,
        /// <summary>
        /// Gets device statistics
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        LogSense = 0x4D,
        /// <summary>
        /// Retrieves management protocol information
        /// SPC-2 rev. 20
        /// </summary>
        ManagementProtocolIn = 0xA3,
        /// <summary>
        /// Transfers management protocol information
        /// SPC-2 rev. 20
        /// </summary>
        ManagementProtocolOut = 0xA4,
        /// <summary>
        /// Sets device parameters
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ModeSelect = 0x15,
        /// <summary>
        /// Sets device parameters
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ModeSelect10 = 0x55,
        /// <summary>
        /// Gets device parameters
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ModeSense = 0x1A,
        /// <summary>
        /// Gets device parameters
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ModeSense10 = 0x5A,
        /// <summary>
        /// Obtains information about persistent reservations and reservation keys
        /// SPC-1 rev. 10
        /// </summary>
        PersistentReserveIn = 0x5E,
        /// <summary>
        /// Reserves a LUN or an extent within a LUN for exclusive or shared use
        /// SPC-1 rev. 10
        /// </summary>
        PersistentReserveOut = 0x5F,
        /// <summary>
        /// Requests the device to disable or enable the removal of the medium inside it
        /// ECMA-111 (SCSI-1)
        /// </summary>
        PreventAllowMediumRemoval = 0x1E,
        /// <summary>
        /// Reads attribute values from medium auxiliary memory
        /// SPC-3 rev. 21b
        /// </summary>
        ReadAttribute = 0x8C,
        /// <summary>
        /// Reads the device buffer
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadBuffer = 0x3C,
        /// <summary>
        /// Reads the media serial number
        /// SPC-3 rev. 21b
        /// </summary>
        ReadSerialNumber = 0xAB,
        /// <summary>
        /// Receives information about a previous or current <see cref="ExtendedCopy"/> 
        /// SPC-2 rev. 20
        /// </summary>
        ReceiveCopyResults = 0x84,
        /// <summary>
        /// Requests the data after completion of a <see cref="SendDiagnostic"/> 
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ReceiveDiagnostic = SasiCommands.ReadDiagnostic,
        /// <summary>
        /// Releases a previously reserved LUN or extents
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Release = SasiCommands.ReleaseUnit,
        /// <summary>
        /// Releases a previously reserved LUN or extents
        /// SPC-1 rev. 10
        /// </summary>
        Release10 = 0x57,
        /// <summary>
        /// Requests the LUNs that are present on the device
        /// SPC-1 rev. 10
        /// </summary>
        ReportLuns = 0xA0,
        /// <summary>
        /// Requests the device's sense
        /// ECMA-111 (SCSI-1)
        /// </summary>
        RequestSense = SasiCommands.RequestSense,
        /// <summary>
        /// Reserves a LUN or extent
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Reserve = SasiCommands.ReserveUnit,
        /// <summary>
        /// Reserves a LUN or extent
        /// SPC-1 rev. 10
        /// </summary>
        Reserve10 = 0x56,
        /// <summary>
        /// Retrieves security protocol information
        /// SPC-4 rev. 16
        /// </summary>
        SecurityProtocolIn = 0xA2,
        /// <summary>
        /// Transfers security protocol information
        /// SPC-4 rev. 16
        /// </summary>
        SecurityProtocolOut = 0xB5,
        /// <summary>
        /// Requests the device to perform diagnostics
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SendDiagnostic = SasiCommands.WriteDiagnostic,
        /// <summary>
        /// Extended commands
        /// SPC-4
        /// </summary>
        ServiceActionIn = 0x9E,
        /// <summary>
        /// Extended commands
        /// SPC-4 
        /// </summary>
        ServiceActionOut = 0x9F,
        /// <summary>
        /// Checks if a LUN is ready to access its medium
        /// ECMA-111 (SCSI-1)
        /// </summary>
        TestUnitReady = SasiCommands.TestUnitReady,
        /// <summary>
        /// Writes attribute values to medium auxiliary memory
        /// SPC-3 rev. 21b
        /// </summary>
        WriteAttribute = 0x8D,
        /// <summary>
        /// Writes to the device's buffer
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        WriteBuffer = 0x3B,
        #endregion SCSI Primary Commands (SPC)

        #region SCSI Block Commands (SBC)

        /// <summary>
        /// Compares blocks with sent data, and if equal, writes those block to device, atomically
        /// SBC-3 rev. 25
        /// </summary>
        CompareAndWrite = 0x89,
        /// <summary>
        /// Formats the medium into addressable logical blocks
        /// ECMA-111 (SCSI-1)
        /// </summary>
        FormatUnit = SasiCommands.FormatUnit,
        /// <summary>
        /// Locks blocks from eviction of device's cache
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        LockUnlockCache = 0x36,
        /// <summary>
        /// Locks blocks from eviction of device's cache
        /// SBC-2 rev. 4
        /// </summary>
        LockUnlockCache16 = 0x92,
        /// <summary>
        /// Requests the device to perform the following uninterrupted series of actions:
        /// 1.- Read the specified blocks
        /// 2.- Transfer blocks from the data out buffer
        /// 3.- Perform an OR operation between the read blocks and the buffer
        /// 4.- Write the buffer to the blocks
        /// SBC-3 rev. 16
        /// </summary>
        ORWrite = 0x8B,
        /// <summary>
        /// Transfers requested blocks to devices' cache
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PreFetch = 0x34,
        /// <summary>
        /// Transfers requested blocks to devices' cache
        /// SBC-3 rev. 16
        /// </summary>
        PreFetch16 = 0x90,
        /// <summary>
        /// Reads blocks from device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Read = SasiCommands.Read,
        /// <summary>
        /// Reads blocks from device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Read10 = SasiCommands.ExtendedAddressRead,
        /// <summary>
        /// Reads blocks from device
        /// SBC-2 rev. 4
        /// </summary>
        Read16 = 0x88,
        /// <summary>
        /// Gets device capacity
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ReadCapacity = 0x25,
        /// <summary>
        /// Gets device's defect data
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadDefectData = 0x37,
        /// <summary>
        /// Reads blocks from device in a vendor-specific way that should include the ECC alongside the data
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadLong = 0x3E,
        /// <summary>
        /// Requests the device to reassign the defective blocks to another area of the medium
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ReassignBlocks = 0x07,
        /// <summary>
        /// Requests the target write to the medium the XOR data generated from the specified source devices
        /// SBC-1 rev. 8c
        /// </summary>
        Rebuild = 0x81,
        /// <summary>
        /// Requests the target write to the buffer the XOR data from its own medium and the specified source devices
        /// SBC-1 rev. 8c
        /// </summary>
        Regenerate = 0x82,
        /// <summary>
        /// Requests the device to set the LUN in a vendor specific state
        /// ECMA-111 (SCSI-1)
        /// </summary>
        RezeroUnit = SasiCommands.RezeroUnit,
        /// <summary>
        /// Searches data on blocks
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SearchDataEqual = SasiCommands.SearchDataEqual,
        /// <summary>
        /// Searches data on blocks using major than or equal comparison
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SearchDataHigh = SasiCommands.SearchDataHigh,
        /// <summary>
        /// Searches data on blocks using minor than or equal comparison
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SearchDataLow = SasiCommands.SearchDataLow,
        /// <summary>
        /// Requests the device to seek to a specified blocks
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Seek = SasiCommands.Seek,
        /// <summary>
        /// Requests the device to seek to a specified blocks
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Seek10 = 0x2B,
        /// <summary>
        /// Defines the range within which subsequent linked commands may operate
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SetLimits = 0x33,
        /// <summary>
        /// Requests the device to enable or disable the LUN for media access operations
        /// ECMA-111 (SCSI-1)
        /// </summary>
        StartStopUnit = 0x1B,
        /// <summary>
        /// Ensures that the blocks in the cache are written to the medium
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SynchronizeCache = 0x35,
        /// <summary>
        /// Ensures that the blocks in the cache are written to the medium
        /// SBC-2 rev. 4
        /// </summary>
        SynchronizeCache16 = 0x91,
        /// <summary>
        /// Unmaps one or more LBAs
        /// In SSDs, this is trimming
        /// SBC-3 rev. 25
        /// </summary>
        Unmap = 0x42,
        /// <summary>
        /// Verifies blocks on the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Verify10 = SasiCommands.Verify,
        /// <summary>
        /// Verifies blocks on the device
        /// SBC-2 rev. 4
        /// </summary>
        Verify16 = 0x8F,
        /// <summary>
        /// Writes blocks to the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Write = SasiCommands.Write,
        /// <summary>
        /// Writes blocks to the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Write10 = SasiCommands.ExtendedAddressWrite,
        /// <summary>
        /// Writes blocks to the device
        /// SBC-2 rev. 4
        /// </summary>
        Write16 = 0x8A,
        /// <summary>
        /// Writes blocks to the device and then verifies them
        /// ECMA-111 (SCSI-1)
        /// </summary>
        WriteAndVerify = SasiCommands.WriteAndVerify,
        /// <summary>
        /// Writes blocks to the device and then verifies them
        /// SBC-2 rev. 4
        /// </summary>
        WriteAndVerify16 = 0x8E,
        /// <summary>
        /// Writes blocks to the device with a vendor specified format that shall include the ECC alongside the data
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        WriteLong = 0x3F,
        /// <summary>
        /// Writes a single block several times
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        WriteSame = 0x41,
        /// <summary>
        /// Writes a single block several times
        /// SBC-2 rev. 4
        /// </summary>
        WriteSame16 = 0x93,
        /// <summary>
        /// Requets XOR data generated by an <see cref="XDWrite"/> or <see cref="Regenerate"/> command
        /// SBC-1 rev. 8c
        /// </summary>
        XDRead = 0x52,
        /// <summary>
        /// XORs the data sent with data on the medium and stores it until an <see cref="XDRead"/> is issued
        /// SBC-1 rev. 8c
        /// </summary>
        XDWrite = 0x50,
        /// <summary>
        /// XORs the data sent with data on the medium and stores it until an <see cref="XDRead"/> is issued
        /// SBC-1 rev. 8c
        /// </summary>
        XDWrite16 = 0x80,
        /// <summary>
        /// Requets the target to XOR the sent data with the data on the medium and return the results
        /// </summary>
        XDWriteRead = 0x53,
        /// <summary>
        /// Requests the target to XOR the data transferred with the data on the medium and writes it to the medium
        /// SBC-1 rev. 8c
        /// </summary>
        XPWrite = 0x51,
        #endregion SCSI Block Commands (SBC)

        #region SCSI Streaming Commands (SSC)

        /// <summary>
        /// Prepares the medium for use by the LUN
        /// SSC-1 rev. 22
        /// </summary>
        FormatMedium = 0x04,
        /// <summary>
        /// Erases part of all of the medium from the current position
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Erase = 0x19,
        /// <summary>
        /// Enables or disables the LUN for further operations
        /// ECMA-111 (SCSI-1)
        /// </summary>
        LoadUnload = 0x1B,
        /// <summary>
        /// Positions the LUN to a specified block in a specified partition
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Locate = 0x2B,
        /// <summary>
        /// Positions the LUN to a specified block in a specified partition
        /// SSC-2 rev. 09
        /// </summary>
        Locate16 = 0x92,
        /// <summary>
        /// Requests the block length limits capability
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ReadBlockLimits = 0x05,
        /// <summary>
        /// Reads the current position
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadPosition = 0x34,
        /// <summary>
        /// Reads blocks from the device, in reverse order
        /// ECMA-111 (SCSI-1)
        /// </summary>
        ReadReverse = 0x0F,
        /// <summary>
        /// Retrieves data from the device buffer that has not been successfully written to the medium (or printed)
        /// ECMA-111 (SCSI-1)
        /// </summary>
        RecoverBufferedData = 0x14,
        /// <summary>
        /// Requests information regarding the supported densities for the logical unit
        /// SSC-1 rev. 22
        /// </summary>
        ReportDensitySupport = 0x44,
        /// <summary>
        /// Seeks the medium to the beginning of partition in current partition
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Rewind = 0x01,
        /// <summary>
        /// A variety of positioning functions
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Space = 0x11,
        /// <summary>
        /// A variety of positioning functions
        /// SSC-2 rev. 09
        /// </summary>
        Space16 = 0x91,
        /// <summary>
        /// Selects the specified track
        /// ECMA-111 (SCSI-1)
        /// </summary>
        TrackSelect = 0x0B,
        /// <summary>
        /// Verifies one or more blocks from the next one
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Verify = 0x13,
        /// <summary>
        /// Writes the specified number of filemarks or setmarks in the current position
        /// ECMA-111 (SCSI-1)
        /// </summary>
        WriteFileMarks = 0x10,
        #endregion SCSI Streaming Commands (SSC)

        #region SCSI Streaming Commands for Printers (SSC)

        /// <summary>
        /// Assures that the data in the buffer has been printed, or, for other devices, written to media
        /// ECMA-111 (SCSI-1)
        /// </summary>
        FlushBuffer = 0x10,
        /// <summary>
        /// Specifies forms or fronts
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Format = 0x04,
        /// <summary>
        /// Transfers data to be printed
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Print = 0x0A,
        /// <summary>
        /// Transfers data to be printed with a slew value
        /// ECMA-111 (SCSI-1)
        /// </summary>
        SlewAndPrint = 0x0B,
        /// <summary>
        /// Halts printing
        /// ECMA-111 (SCSI-1)
        /// </summary>
        StopPrint = 0x1B,
        /// <summary>
        /// Assures that the data in the buffer has been printed, or, for other devices, written to media
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SynchronizeBuffer = FlushBuffer,
        #endregion SCSI Streaming Commands for Printers (SSC)

        #region SCSI Processor Commands

        /// <summary>
        /// Transfers data from the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Receive = 0x08,
        /// <summary>
        /// Sends data to the device
        /// ECMA-111 (SCSI-1)
        /// </summary>
        Send = 0x0A,
        #endregion SCSI Processor Commands

        #region SCSI Multimedia Commands (MMC)

        /// <summary>
        /// Erases any part of a CD-RW
        /// MMC-1 rev. 9
        /// </summary>
        Blank = 0xA1,
        /// <summary>
        /// Closes a track or session
        /// MMC-1 rev. 9
        /// </summary>
        CloseTrackSession = 0x5B,
        /// <summary>
        /// Gets information about the overall capabilities of the device and the current capabilities of the device
        /// MMC-2 rev. 11a
        /// </summary>
        GetConfiguration = 0x46,
        /// <summary>
        /// Requests the LUN to report events and statuses
        /// MMC-2 rev. 11a
        /// </summary>
        GetEventStatusNotification = 0x4A,
        /// <summary>
        /// Provides a mehotd to profile the performance of the drive
        /// MMC-2 rev. 11a
        /// </summary>
        GetPerformance = 0xAC,
        /// <summary>
        /// Requests the device changer to load or unload a disc
        /// MMC-1 rev. 9
        /// </summary>
        LoadUnloadCd = 0xA6,
        /// <summary>
        /// Requests the device changer to load or unload a disc
        /// MMC-2 rev. 11a
        /// </summary>
        LoadUnloadMedium = 0xA6,
        /// <summary>
        /// Requests information about the current status of the CD device, including any changer mechanism
        /// MMC-1 rev. 9
        /// </summary>
        MechanicalStatus = 0xBD,
        /// <summary>
        /// Requests the device to start or stop an audio play operation
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PauseResume = 0x4B,
        /// <summary>
        /// Begins an audio playback
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayAudio = 0x45,
        /// <summary>
        /// Begins an audio playback
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayAudio12 = 0xA5,
        /// <summary>
        /// Begins an audio playback using MSF addressing
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayAudioMsf = 0x47,
        /// <summary>
        /// Begins an audio playback from the specified index of the specified track
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayAudioTrackIndex = 0x48,
        /// <summary>
        /// Begins an audio playback from the position relative of a track
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayTrackRelative = 0x49,
        /// <summary>
        /// Begins an audio playback from the position relative of a track
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PlayTrackRelative12 = 0xA9,
        /// <summary>
        /// Reports the total and blank area of the device buffer
        /// MMC-1 rev. 9
        /// </summary>
        ReadBufferCapacity = 0x5C,
        /// <summary>
        /// Reads a block from a CD with any of the requested CD data streams
        /// MMC-1 rev. 9
        /// </summary>
        ReadCd = 0xBE,
        /// <summary>
        /// Reads a block from a CD with any of the requested CD data streams using MSF addressing
        /// MMC-1 rev. 9
        /// </summary>
        ReadCdMsf = 0xB9,
        /// <summary>
        /// Returns the recorded size of the CD
        /// MMC-1 rev. 9
        /// </summary>
        ReadCdRecordedCapacity = 0x25,
        /// <summary>
        /// Gets informationn about all discs: CD-ROM, CD-R and CD-RW
        /// MMC-1 rev. 9
        /// </summary>
        ReadDiscInformation = 0x51,
        /// <summary>
        /// Reads areas from the DVD or BD media
        /// MMC-5 rev. 2c
        /// </summary>
        ReadDiscStructure = 0xAD,
        /// <summary>
        /// Reads areas from the DVD media
        /// MMC-2 rev. 11a
        /// </summary>
        ReadDvdStructure = 0xAD,
        /// <summary>
        /// Requests a list of the possible format capacities for an installed random-writable media
        /// MMC-2 rev. 11a
        /// </summary>
        ReadFormatCapacities = 0x23,
        /// <summary>
        /// Reads the data block header of the specified CD-ROM block
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadHeader = 0x44,
        /// <summary>
        /// Reads the mastering information from a Master CD.
        /// MMC-1 rev. 9
        /// </summary>
        ReadMasterCue = 0x59,
        /// <summary>
        /// Requests the Q subchannel and the current audio playback status
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadSubChannel = 0x42,
        /// <summary>
        /// Requests the medium TOC, PMA or ATIP from the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadTocPmaAtip = 0x43,
        /// <summary>
        /// Gets information about a track regardless of its status
        /// MMC-1 rev. 9
        /// </summary>
        ReadTrackInformation = 0x52,
        /// <summary>
        /// Repairs an incomplete ECC block at the end of an RZone
        /// Mt. Fuji ver. 7 rev. 1.21
        /// </summary>
        RepairRZone = 0x58,
        /// <summary>
        /// Repairs an incomplete packet at the end of a packet writing track
        /// MMC-1 rev. 9
        /// </summary>
        RepairTrack = 0x58,
        /// <summary>
        /// Requests the start of the authentication process and provides data necessary for authentication and for generating a Bus Key
        /// MMC-2 rev. 11a
        /// </summary>
        ReportKey = 0xA4,
        /// <summary>
        /// Reserves disc space for a track
        /// MMC-1 rev. 9
        /// </summary>
        ReserveTrack = 0x53,
        /// <summary>
        /// Fast-forwards or fast-reverses the audio playback to the specified block. Stops if it encounters a data track
        /// MMC-1 rev. 9
        /// </summary>
        ScanMmc = 0xBA,
        /// <summary>
        /// Sends a cue sheet for session-at-once recording
        /// MMC-1 rev. 9
        /// </summary>
        SendCueSheet = 0x5D,
        /// <summary>
        /// Transfer a DVD or BD structure for media writing
        /// MMC-5 rev. 2c
        /// </summary>
        SendDiscStructure = 0xAD,
        /// <summary>
        /// Transfer a DVD structure for media writing
        /// MMC-2 rev. 11a
        /// </summary>
        SendDvdStructure = 0xAD,
        /// <summary>
        /// Requests the LUN to process an event
        /// MMC-2 rev. 11a
        /// </summary>
        SendEvent = 0xA2,
        /// <summary>
        /// Provides data necessary for authentication and for generating a Bus Key
        /// MMC-2 rev. 11a
        /// </summary>
        SendKey = 0xA3,
        /// <summary>
        /// Restores the Optimum Power Calibration values to the drive for a specific disc
        /// MMC-1 rev. 9
        /// </summary>
        SendOpcInformation = 0x54,
        /// <summary>
        /// Sets the spindle speed to be used while reading/writing data to a CD
        /// MMC-1 rev. 9
        /// </summary>
        SetCdRomSpeed = 0xBB,
        /// <summary>
        /// Requests the LUN to perform read ahead caching operations from the specified block
        /// MMC-2 rev. 11a
        /// </summary>
        SetReadAhead = 0xA7,
        /// <summary>
        /// Indicates the LUN to try to achieve a specified performance
        /// MMC-2 rev. 11a
        /// </summary>
        SetStreaming = 0xB6,
        /// <summary>
        /// Stops a scan and continues audio playback from current scanning position
        /// MMC-1 rev. 9
        /// </summary>
        StopPlayScan = 0x4E,
        #endregion SCSI Multimedia Commands (MMC)

        #region SCSI Scanner Commands

        /// <summary>
        /// Gets information about the data buffer
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        GetDataBufferStatus = 0x34,
        /// <summary>
        /// Gets information about previously defined windows
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        GetWindow = 0x25,
        /// <summary>
        /// Provides positioning functions
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ObjectPosition = 0x31,
        /// <summary>
        /// Begins a scan operation
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Scan = 0x1B,
        /// <summary>
        /// Specifies one or more windows within the device's scanning range
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SetWindow = 0x24,
        /// <summary>
        /// Sends data to the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Send10 = 0x2A,
        #endregion SCSI Scanner Commands

        #region SCSI Block Commands for Optical Media (SBC)

        /// <summary>
        /// Erases the specified number of blocks
        /// </summary>
        Erase10 = 0x2C,
        /// <summary>
        /// Erases the specified number of blocks
        /// </summary>
        Erase12 = 0xAC,
        /// <summary>
        /// Searches the medium for a contiguos set of written or blank blocks
        /// </summary>
        MediumScan = 0x38,
        /// <summary>
        /// Reads blocks from device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Read12 = 0xA8,
        /// <summary>
        /// Gets medium's defect data
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadDefectData12 = 0xB7,
        /// <summary>
        /// Gets the maxium generation address for the specified block
        /// </summary>
        ReadGeneration = 0x29,
        /// <summary>
        /// Reads a specified generation of a specified block
        /// </summary>
        ReadUpdatedBlock = 0x2D,
        /// <summary>
        /// Searches data on blocks
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SearchDataEqual12 = 0xB1,
        /// <summary>
        /// Searches data on blocks using major than or equal comparison
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SearchDataHigh12 = 0xB0,
        /// <summary>
        /// Searches data on blocks using minor than or equal comparison
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SearchDataLow12 = 0xB2,
        /// <summary>
        /// Defines the range within which subsequent linked commands may operate
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SetLimits12 = 0xB3,
        /// <summary>
        /// Replaces a block with data
        /// </summary>
        UpdateBlock = 0x3D,
        /// <summary>
        /// Verifies blocks on the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Verify12 = 0xAF,
        /// <summary>
        /// Writes blocks to the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        Write12 = 0xAA,
        /// <summary>
        /// Writes blocks to the device and then verifies them
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        WriteAndVerify12 = 0xAE,
        #endregion SCSI Block Commands for Optical Media (SBC)

        #region SCSI Medium Changer Commands (SMC)

        /// <summary>
        /// Provides a means to exchange the medium in the source element with the medium at destination element
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ExchangeMedium = 0xA6,
        /// <summary>
        /// Checks all elements for medium and any other relevant status
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        InitializeElementStatus = 0x07,
        /// <summary>
        /// Checks all elements for medium and any other relevant status in the specified range of elements
        /// SMC-2 rev. 7
        /// </summary>
        InitializeElementStatusWithRange = 0x37,
        /// <summary>
        /// Moves a medium from an element to another
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        MoveMedium = 0xA5,
        /// <summary>
        /// Moves a medium that's currently attached to another element
        /// SPC-1 rev. 10
        /// </summary>
        MoveMediumAttached = 0xA7,
        /// <summary>
        /// Provides a method to change the open/closed state of the specified import/export element
        /// SMC-3 rev. 12
        /// </summary>
        OpenCloseImportExportElement = 0x1B,
        /// <summary>
        /// Positions the transport element in front of the destination element
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        PositionToElement = 0x2B,
        /// <summary>
        /// Requests the status of the elements
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReadElementStatus = 0xB8,
        /// <summary>
        /// Requests the status of the attached element
        /// SPC-1 rev. 10
        /// </summary>
        ReadElementStatusAttached = 0xB4,
        /// <summary>
        /// Releases a reserved LUN
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReleaseElement = 0x17,
        /// <summary>
        /// Releases a reserved LUN
        /// SMC-1 rev. 10a
        /// </summary>
        ReleaseElement10 = 0x57,
        /// <summary>
        /// Requests information regarding the supported volume types for the device
        /// SMC-3 rev. 12
        /// </summary>
        ReportVolumeTypesSupported = 0x44,
        /// <summary>
        /// Gets the results of <see cref="SendVolumeTag"/> 
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        RequestVolumeElementAddress = 0xB5,
        /// <summary>
        /// Reserves a LUN
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        ReserveElement = 0x16,
        /// <summary>
        /// Reserves a LUN
        /// SMC-1 rev. 10a
        /// </summary>
        ReserveElement10 = 0x56,
        /// <summary>
        /// Transfers a volume tag template to be searched or new volume tag information for one or more elements
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SendVolumeTag = 0xB6,
        #endregion SCSI Medium Changer Commands (SMC)

        #region SCSI Communication Commands

        /// <summary>
        /// Gets data from the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        GetMessage = 0x08,
        /// <summary>
        /// Gets data from the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        GetMessage10 = 0x28,
        /// <summary>
        /// Gets data from the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        GetMessage12 = 0xA8,
        /// <summary>
        /// Sends data to the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SendMessage = 0x0A,
        /// <summary>
        /// Sends data to the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SendMessage10 = 0x2A,
        /// <summary>
        /// Sends data to the device
        /// SCSI-2 X3T9.2/375R rev. 10l
        /// </summary>
        SendMessage12 = 0xAA,
        #endregion SCSI Communication Commands

        #region SCSI Controller Commands

        /// <summary>
        /// Commands that get information about redundancy groups
        /// SCC-2 rev. 4
        /// </summary>
        RedundancyGroupIn = 0xBA,
        /// <summary>
        /// Commands that set information about redundancy groups
        /// SCC-2 rev. 4
        /// </summary>
        RedundancyGroupOut = 0xBB,
        /// <summary>
        /// Commands that get information about volume sets
        /// SCC-2 rev. 4
        /// </summary>
        VolumeSetIn = 0xBE,
        /// <summary>
        /// Commands that set information about volume sets
        /// SCC-2 rev. 4
        /// </summary>
        VolumeSetOut = 0xBF,
        #endregion SCSI Controller Commands

        #region Pioneer CD-ROM SCSI-2 Command Set

        /// <summary>
        /// Scans for a block playing a block on each track cross
        /// </summary>
        AudioScan = 0xCD,
        /// <summary>
        /// Requests the drive the status from the previous WriteCDP command.
        /// </summary>
        ReadCDP = 0xE4,
        /// <summary>
        /// Requests status from the drive
        /// </summary>
        ReadDriveStatus = 0xE0,
        /// <summary>
        /// Reads CD-DA data and/or subcode data
        /// </summary>
        ReadCdDa = 0xD8,
        /// <summary>
        /// Reads CD-DA data and/or subcode data using MSF addressing
        /// </summary>
        ReadCdDaMsf = 0xD9,
        /// <summary>
        /// Reads CD-XA data
        /// </summary>
        ReadCdXa = 0xDB,
        /// <summary>
        /// Reads all subcode data
        /// </summary>
        ReadAllSubCode = 0xDF,
        /// <summary>
        /// Sets the spindle speed to be used while reading/writing data to a CD
        /// </summary>
        SetCdSpeed = 0xDA,
        WriteCdp = 0xE3,
        #endregion

        #region ATA Command Pass-Through

        /// <summary>
        /// Sends a 24-bit ATA command to the device
        /// Clashes with <see cref="Blank"/>
        /// ATA CPT rev. 8a
        /// </summary>
        AtaPassThrough = 0xA1,
        /// <summary>
        /// Sends a 48-bit ATA command to the device
        /// ATA CPT rev. 8a
        /// </summary>
        AtaPassThrough16 = 0x85,
        #endregion ATA Command Pass-Through

        #region 6-byte CDB aliases

        ModeSelect6 = ModeSelect,
        ModeSense6 = ModeSense,
        Read6 = Read,
        Seek6 = Seek,
        Write6 = Write,
        #endregion 6-byte CDB aliases

        #region SCSI Zoned Block Commands

        /// <summary>
        /// ZBC commands with host->device information
        /// </summary>
        ZbcOut = 0x94,
        /// <summary>
        /// ZBC commands with device->host information
        /// </summary>
        ZbcIn = 0x95,
        #endregion

        #region SCSI Commands with unknown meaning, mostly vendor specific

        SetCdSpeedUnk = 0xB8,
        WriteCdMsf = 0xA2,
        WriteCd = 0xAA,
        ReadDefectTag = 0xB7,
        PlayCd = 0xBC,
        SpareIn = 0xBC,
        SpareOut = 0xBD,
        WriteStream16 = 0x9A,
        WriteAtomic = 0x9C,
        ServiceActionBidirectional = 0x9D,
        WriteLong2 = 0xEA,
        UnknownCdCommand = 0xD4,
        UnknownCdCommand2 = 0xD5,
        #endregion SCSI Commands with unknown meaning, mostly vendor specific

        #region SEGA Packet Interface (all are 12-byte CDB)

        /// <summary>
        /// Verifies that the device can be accessed
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_TestUnit = TestUnitReady,
        /// <summary>
        /// Gets current CD status
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_RequestStatus = 0x10,
        /// <summary>
        /// Gets CD block mode info
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_RequestMode = 0x11,
        /// <summary>
        /// Sets CD block mode
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_SetMode = 0x12,
        /// <summary>
        /// Requests device error info
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_RequestError = 0x13,
        /// <summary>
        /// Gets disc TOC
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_GetToc = 0x14,
        /// <summary>
        /// Gets specified session data
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_RequestSession = 0x15,
        /// <summary>
        /// Stops the drive and opens the drive tray, or, on manual trays, stays busy until it is opened
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_OpenTray = 0x16,
        /// <summary>
        /// Starts audio playback
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_PlayCd = 0x20,
        /// <summary>
        /// Moves drive pickup to specified block
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_Seek = 0x21,
        /// <summary>
        /// Fast-forwards or fast-reverses until Lead-In or Lead-Out arrive, or until another command is issued
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_Scan = 0x22,
        /// <summary>
        /// Reads blocks from the disc
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_Read = 0x30,
        /// <summary>
        /// Reads blocks from the disc seeking to another position at end
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_Read2 = 0x31,
        /// <summary>
        /// Reads disc subcode
        /// Sega SPI ver. 1.30
        /// </summary>
        Sega_GetSubcode = 0x40,
        #endregion SEGA Packet Interface (all are 12-byte CDB)

        /// <summary>
        /// Variable sized Command Description Block
        /// SPC-4 rev. 16
        /// </summary>
        VariableSizedCDB = 0x7F,

        #region Plextor vendor commands
        /// <summary>
        /// Sends extended commands (like SpeedRead) to Plextor drives
        /// </summary>
        Plextor_Extend = 0xE9,
        /// <summary>
        /// Command for Plextor PoweRec
        /// </summary>
        Plextor_PoweRec = 0xEB,
        /// <summary>
        /// Sends extended commands (like PoweRec) to Plextor drives
        /// </summary>
        Plextor_Extend2 = 0xED,
        /// <summary>
        /// Resets Plextor drives
        /// </summary>
        Plextor_Reset = 0xEE,
        /// <summary>
        /// Reads drive statistics from Plextor drives EEPROM
        /// </summary>
        Plextor_ReadEeprom = 0xF1,
        #endregion Plextor vendor commands

        #region HL-DT-ST vendor commands
        /// <summary>
        /// Sends debugging commands to HL-DT-ST DVD drives
        /// </summary>
        HlDtSt_Vendor = 0xE7,
        #endregion HL-DT-ST vendor commands

        #region NEC vendor commands
        /// <summary>
        /// Reads CD-DA data
        /// </summary>
        NEC_ReadCdDa = 0xD4,
        #endregion NEC vendor commands

        #region Adaptec vendor commands
        /// <summary>
        /// Translates a SCSI LBA to a drive's CHS
        /// </summary>
        Adaptec_Translate = 0x0F,
        /// <summary>
        /// Configures Adaptec controller error threshold
        /// </summary>
        Adaptec_SetErrorThreshold = 0x10,
        /// <summary>
        /// Reads and resets error and statistical counters
        /// </summary>
        Adaptec_ReadCounters = 0x11,
        /// <summary>
        /// Writes to controller's RAM
        /// </summary>
        Adaptec_WriteBuffer = 0x13,
        /// <summary>
        /// Reads controller's RAM
        /// </summary>
        Adaptec_ReadBuffer = 0x14,
        #endregion Adaptec vendor commands

        #region Archive Corp. vendor commands
        /// <summary>
        /// Gets current position's block address
        /// </summary>
        Archive_RequestBlockAddress = 0x02,
        /// <summary>
        /// Seeks to specified block address
        /// </summary>
        Archive_SeekBlock = 0x0C,
        #endregion Archive Corp. vendor commands

        #region Certance vendor commands
        /// <summary>
        /// Parks the load arm in preparation for transport
        /// </summary>
        Certance_ParkUnpark = 0x06,
        #endregion Certance vendor commands

        #region Fujitsu vendor commands
        /// <summary>
        /// Used to check the controller's data and control path
        /// </summary>
        Fujitsu_LoopWriteToRead = 0xC1,
        /// <summary>
        /// Used to display a message on the operator panel
        /// </summary>
        Fujitsu_Display = 0xCF,
        #endregion Fujitsu vendor commands

        #region M-Systems vendor commands
        /// <summary>
        /// Securely erases all flash blocks, including defective, spared and unused
        /// </summary>
        MSystems_SecurityErase = 0xFF,
        /// <summary>
        /// Securely erases all flash blocks, including defective, spared and unused
        /// </summary>
        MSystems_SecurityEraseOld = 0xDF,
        #endregion M-Systems vendor commands

        #region Plasmon vendor commands
        /// <summary>
        /// Retrieves sector address
        /// </summary>
        Plasmon_ReadSectorLocation = 0xE6,
        /// <summary>
        /// Makes a Compliant WORM block completely unreadable
        /// </summary>
        Plasmon_Shred = 0xEE,
        #endregion Plasmon vendor commands
    }
    #endregion SCSI Commands

    /// <summary>
    /// SCSI command transfer direction
    /// </summary>
    public enum ScsiDirection
    {
        /// <summary>
        /// No data transfer happens
        /// </summary>
        None,
        /// <summary>
        /// From host to device
        /// </summary>
        Out,
        /// <summary>
        /// From device to host
        /// </summary>
        In,
        /// <summary>
        /// Bidirectional device/host
        /// </summary>
        Bidirectional,
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified
    }

    #region SCSI's ATA Command Pass-Through
    public enum AtaProtocol : byte
    {
        /// <summary>
        /// Requests a device hard reset (pin 1)
        /// </summary>
        HardReset = 0,
        /// <summary>
        /// Requests a device soft reset (COMRESET issue)
        /// </summary>
        SoftReset = 1,
        /// <summary>
        /// No data is to be transfered
        /// </summary>
        NonData = 3,
        /// <summary>
        /// Requests a device->host transfer using PIO
        /// </summary>
        PioIn = 4,
        /// <summary>
        /// Requests a host->device transfer using PIO
        /// </summary>
        PioOut = 5,
        /// <summary>
        /// Requests a DMA transfer
        /// </summary>
        Dma = 6,
        /// <summary>
        /// Requests to queue a DMA transfer
        /// </summary>
        DmaQueued = 7,
        /// <summary>
        /// Requests device diagnostics
        /// </summary>
        DeviceDiagnostic = 8,
        /// <summary>
        /// Requets device reset
        /// </summary>
        DeviceReset = 9,
        /// <summary>
        /// Requests a device->host transfer using UltraDMA
        /// </summary>
        UDmaIn = 10,
        /// <summary>
        /// Requests a host->device transfer using UltraDMA
        /// </summary>
        UDmaOut = 11,
        /// <summary>
        /// Unknown Serial ATA
        /// </summary>
        FPDma = 12,
        /// <summary>
        /// Requests the Extended ATA Status Return Descriptor
        /// </summary>
        ReturnResponse = 15
    }

    /// <summary>
    /// Indicates the STL which ATA register contains the length of data to
    /// be transfered
    /// </summary>
    public enum AtaTransferRegister : byte
    {
        /// <summary>
        /// There is no transfer
        /// </summary>
        NoTransfer = 0,
        /// <summary>
        /// FEATURE register contains the data length
        /// </summary>
        Feature = 1,
        /// <summary>
        /// SECTOR_COUNT register contains the data length
        /// </summary>
        SectorCount = 2,
        /// <summary>
        /// The STPSIU contains the data length
        /// </summary>
        SPTSIU = 3
    }
    #endregion SCSI's ATA Command Pass-Through

    /// <summary>
    /// ZBC sub-commands, mask 0x1F
    /// </summary>
    public enum ZBCSubCommands : byte
    {
        /// <summary>
        /// Returns list with zones of specified types
        /// </summary>
        ReportZones = 0x00,
        /// <summary>
        /// Closes a zone
        /// </summary>
        CloseZone = 0x01,
        /// <summary>
        /// Finishes a zone
        /// </summary>
        FinishZone = 0x02,
        /// <summary>
        /// Opens a zone
        /// </summary>
        OpenZone = 0x03,
        /// <summary>
        /// Resets zone's write pointer to zone start
        /// </summary>
        ResetWritePointer = 0x04
    }

    /// <summary>
    /// MODE SENSE page control, mask 0xC0
    /// </summary>
    public enum ScsiModeSensePageControl : byte
    {
        /// <summary>
        /// Current values
        /// </summary>
        Current = 0x00,
        /// <summary>
        /// Changeable values
        /// </summary>
        Changeable = 0x40,
        /// <summary>
        /// Default values
        /// </summary>
        Default = 0x80,
        /// <summary>
        /// Saved values
        /// </summary>
        Saved = 0xC0
    }

    public enum ScsiPreventAllowMode : byte
    {
        /// <summary>
        /// Allows medium removal from data transport and from medium changer
        /// </summary>
        Allow = 0x00,
        /// <summary>
        /// Prevents medium removal from data transport but allows it from medium changer
        /// </summary>
        Prevent = 0x01,
        /// <summary>
        /// Allows medium removal from data transport but prevents it from medium changer
        /// </summary>
        [Obsolete]
        PreventChanger = 0x02,
        /// <summary>
        /// Prevents medium removal from both data transport and medium changer
        /// </summary>
        [Obsolete]
        PreventAll = 0x03
    }

    public enum MmcGetConfigurationRt : byte
    {
        /// <summary>
        /// Drive shall return the Feature Header and all Feature Descriptors
        /// </summary>
        All = 0x00,
        /// <summary>
        /// Drive shall return the Feature Header and current Feature Descriptors
        /// </summary>
        Current = 0x01,
        /// <summary>
        /// Drive shall return only the Feature Header with the chosen Feature Descriptor
        /// </summary>
        Single = 0x02,
        Reserved = 0x03
    }

    public enum MmcDiscStructureMediaType : byte
    {
        /// <summary>
        /// Disc Structures for DVD and HD DVD
        /// </summary>
        DVD = 0x00,
        /// <summary>
        /// Disc Structures for BD
        /// </summary>
        BD = 0x01
    }

    public enum MmcDiscStructureFormat : byte
    {
        // Generic Format Codes

        /// <summary>
        /// AACS Volume Identifier
        /// </summary>
        AACSVolId = 0x80,
        /// <summary>
        /// AACS Pre-recorded Media Serial Number
        /// </summary>
        AACSMediaSerial = 0x81,
        /// <summary>
        /// AACS Media Identifier
        /// </summary>
        AACSMediaId = 0x82,
        /// <summary>
        /// AACS Lead-in Media Key Block
        /// </summary>
        AACSMKB = 0x83,
        /// <summary>
        /// AACS Data Keys
        /// </summary>
        AACSDataKeys = 0x84,
        /// <summary>
        /// AACS LBA extents
        /// </summary>
        AACSLBAExtents = 0x85,
        /// <summary>
        /// CPRM Media Key Block specified by AACS
        /// </summary>
        AACSMKBCPRM = 0x86,
        /// <summary>
        /// Recognized format layers
        /// </summary>
        RecognizedFormatLayers = 0x90,
        /// <summary>
        /// Write protection status
        /// </summary>
        WriteProtectionStatus = 0xC0,
        /// <summary>
        /// READ/SEND DISC STRUCTURE capability list
        /// </summary>
        CapabilityList = 0xFF,

        // DVD Disc Structures
        /// <summary>
        /// DVD Lead-in Physical Information
        /// </summary>
        PhysicalInformation = 0x00,
        /// <summary>
        /// DVD Lead-in Copyright Information
        /// </summary>
        CopyrightInformation = 0x01,
        /// <summary>
        /// CSS/CPPM Disc key
        /// </summary>
        DiscKey = 0x02,
        /// <summary>
        /// DVD Burst Cutting Area
        /// </summary>
        BurstCuttingArea = 0x03,
        /// <summary>
        /// DVD Lead-in Disc Manufacturing Information
        /// </summary>
        DiscManufacturingInformation = 0x04,
        /// <summary>
        /// DVD Copyright Information from specified sector
        /// </summary>
        SectorCopyrightInformation = 0x05,
        /// <summary>
        /// CSS/CPPM Media Identifier
        /// </summary>
        MediaIdentifier = 0x06,
        /// <summary>
        /// CSS/CPPM Media Key Block
        /// </summary>
        MediaKeyBlock = 0x07,
        /// <summary>
        /// DDS from DVD-RAM
        /// </summary>
        DVDRAM_DDS = 0x08,
        /// <summary>
        /// DVD-RAM Medium Status
        /// </summary>
        DVDRAM_MediumStatus = 0x09,
        /// <summary>
        /// DVD-RAM Spare Area Information
        /// </summary>
        DVDRAM_SpareAreaInformation = 0x0A,
        /// <summary>
        /// DVD-RAM Recording Type Information
        /// </summary>
        DVDRAM_RecordingType = 0x0B,
        /// <summary>
        /// DVD-R/-RW RMD in last Border-out
        /// </summary>
        LastBorderOutRMD = 0x0C,
        /// <summary>
        /// Specified RMD from last recorded Border-out
        /// </summary>
        SpecifiedRMD = 0x0D,
        /// <summary>
        /// DVD-R/-RW Lead-in pre-recorded information
        /// </summary>
        PreRecordedInfo = 0x0E,
        /// <summary>
        /// DVD-R/-RW Media Identifier
        /// </summary>
        DVDR_MediaIdentifier = 0x0F,
        /// <summary>
        /// DVD-R/-RW Physical Format Information
        /// </summary>
        DVDR_PhysicalInformation = 0x10,
        /// <summary>
        /// ADIP
        /// </summary>
        ADIP = 0x11,
        /// <summary>
        /// HD DVD Lead-in Copyright Protection Information
        /// </summary>
        HDDVD_CopyrightInformation = 0x12,
        /// <summary>
        /// AACS Lead-in Copyright Data Section
        /// </summary>
        DVD_AACS = 0x15,
        /// <summary>
        /// HD DVD-R Medium Status
        /// </summary>
        HDDVDR_MediumStatus = 0x19,
        /// <summary>
        /// HD DVD-R Last recorded RMD in the latest RMZ
        /// </summary>
        HDDVDR_LastRMD = 0x1A,
        /// <summary>
        /// DVD+/-R DL and DVD-Download DL layer capacity
        /// </summary>
        DVDR_LayerCapacity = 0x20,
        /// <summary>
        /// DVD-R DL Middle Zone start address
        /// </summary>
        MiddleZoneStart = 0x21,
        /// <summary>
        /// DVD-R DL Jump Interval Size
        /// </summary>
        JumpIntervalSize = 0x22,
        /// <summary>
        /// DVD-R DL Start LBA of the manual layer jump
        /// </summary>
        ManualLayerJumpStartLBA = 0x23,
        /// <summary>
        /// DVD-R DL Remapping information of the specified Anchor Point
        /// </summary>
        RemapAnchorPoint = 0x24,
        /// <summary>
        /// Disc Control Block
        /// </summary>
        DCB = 0x30,

        // BD Disc Structures
        /// <summary>
        /// Blu-ray Disc Information
        /// </summary>
        DiscInformation = 0x00,
        /// <summary>
        /// Blu-ray Burst Cutting Area
        /// </summary>
        BD_BurstCuttingArea = 0x03,
        /// <summary>
        /// Blu-ray DDS
        /// </summary>
        BD_DDS = 0x08,
        /// <summary>
        /// Blu-ray Cartridge Status
        /// </summary>
        CartridgeStatus = 0x09,
        /// <summary>
        /// Blu-ray Spare Area Information
        /// </summary>
        BD_SpareAreaInformation = 0x0A,
        /// <summary>
        /// Unmodified DFL
        /// </summary>
        RawDFL = 0x12,
        /// <summary>
        /// Physical Access Control
        /// </summary>
        PAC = 0x30
    }

    public enum ScsiServiceActions : byte
    {
        // SERVICE ACTION IN

        /// <summary>
        /// Requests parameter data describing provisioning status for the specified LBA
        /// SBC-3 rev. 25
        /// </summary>
        GetLBAStatus = 0x12,
        /// <summary>
        /// Gets device capacity
        /// SBC-2 rev. 4
        /// </summary>
        ReadCapacity16 = 0x10,
        /// <summary>
        /// Reads blocks from device in a vendor-specific way that should include the ECC alongside the data
        /// SBC-2 rev. 4
        /// </summary>
        ReadLong16 = 0x11,
        /// <summary>
        /// Requests information indicating the user data segments on the ports and LUNs to access them
        /// SBC-3 rev. 25
        /// </summary>
        ReportReferrals = 0x13,

        // SERVICE ACTION OUT

        /// <summary>
        /// Writes blocks to the device with a vendor specified format that shall include the ECC alongside the data
        /// SBC-2 rev. 4
        /// </summary>
        WriteLong16 = ReadLong16
    }

    public enum MmcDiscInformationDataTypes : byte
    {
        /// <summary>
        /// Standard Disc Information
        /// </summary>
        DiscInformation = 0x00,
        /// <summary>
        /// Track Resources Information
        /// </summary>
        TrackResources = 0x01,
        /// <summary>
        /// POW Resources Information
        /// </summary>
        POWResources = 0x02
    }

    public enum MmcSectorTypes : byte
    {
        /// <summary>
        /// No checking of data type is performed
        /// </summary>
        AllTypes = 0x00,
        /// <summary>
        /// Only CD-DA sectors shall be returned
        /// </summary>
        CDDA = 0x01,
        /// <summary>
        /// Only Mode 1 sectors shall be returned
        /// </summary>
        Mode1 = 0x02,
        /// <summary>
        /// Only Mode 2 formless sectors shall be returned
        /// </summary>
        Mode2 = 0x03,
        /// <summary>
        /// Only Mode 2 Form 1 sectors shall be returned
        /// </summary>
        Mode2Form1 = 0x04,
        /// <summary>
        /// Only Mode 2 Form 2 sectors shall be returned
        /// </summary>
        Mode2Form2 = 0x05
    }

    public enum MmcHeaderCodes : byte
    {
        /// <summary>
        /// No header information shall be returned
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Only the four byte header shall be returned
        /// </summary>
        HeaderOnly = 0x01,
        /// <summary>
        /// Only the mode 2 form x subheader shall be returned
        /// </summary>
        SubHeaderOnly = 0x02,
        /// <summary>
        /// Return both header and subheader
        /// </summary>
        AllHeaders = 0x03
    }

    public enum MmcErrorField : byte
    {
        /// <summary>
        /// No error information is returned
        /// </summary>
        None = 0x00,
        /// <summary>
        /// The C2 pointer bits will be included
        /// </summary>
        C2Pointers = 0x01,
        /// <summary>
        /// The C2 pointer bits will be included as well as the block error byte with a padding byte
        /// </summary>
        C2PointersAndBlock = 0x02
    }

    public enum MmcSubchannel : byte
    {
        /// <summary>
        /// No subchannel shall be returned
        /// </summary>
        None = 0x00,
        /// <summary>
        /// The raw P to W subchannel data shall be transferred
        /// </summary>
        Raw = 0x01,
        /// <summary>
        /// Q data shall be transferred
        /// </summary>
        Q16 = 0x02,
        /// <summary>
        /// De-interleaved and error-corrected R to W subchannel data shall be transferred
        /// </summary>
        RW = 0x04
    }

    public enum PioneerSubchannel : byte
    {
        /// <summary>
        /// No subchannel shall be returned
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Q data shall be transferred
        /// </summary>
        Q16 = 0x01,
        /// <summary>
        /// The raw P to W subchannel data shall be transferred
        /// </summary>
        All = 0x02,
        /// <summary>
        /// The raw P to W subchannel data shall be transferred WITHOUT user data
        /// </summary>
        Only = 0x03
    }

    public enum PlextorSubchannel : byte
    {
        /// <summary>
        /// No subchannel shall be returned
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Q data shall be transferred
        /// </summary>
        Q16 = 0x01,
        /// <summary>
        /// The packed and corrected P to W subchannel data shall be transferred
        /// </summary>
        Pack = 0x02,
        /// <summary>
        /// The raw P to W subchannel data shall be transferred
        /// </summary>
        All = 0x03,
        /// <summary>
        /// The raw P to W subchannel data, plus C2 error data shall be transferred
        /// </summary>
        RawC2 = 0x08
    }

    public enum PlextorSubCommands : byte
    {
        /// <summary>
        /// Gets Plextor mode
        /// </summary>
        GetMode = 0x00,
        /// <summary>
        /// Sets Plextor mode
        /// </summary>
        SetMode = 0x10,

        /// <summary>
        /// Plextor force single session or hide CD-R
        /// </summary>
        SessionHide = 0x01,
        /// <summary>
        /// Plextor VariRec
        /// </summary>
        VariRec = 0x02,
        /// <summary>
        /// Plextor GigaRec
        /// </summary>
        GigaRec = 0x04,
        /// <summary>
        /// Plextor accoustic management (disc related)
        /// </summary>
        SilentDisc = 0x06,
        /// <summary>
        /// Plextor accoustic management (tra related)
        /// </summary>
        SilentTray = 0x07,
        /// <summary>
        /// Plextor accoustic management
        /// </summary>
        Silent = 0x08,
        /// <summary>
        /// Plextor test write DVD+
        /// </summary>
        TestWriteDvdPlus = 0x21,
        /// <summary>
        /// Plextor book setting
        /// </summary>
        BitSet = 0x22,
        /// <summary>
        /// Plextor SecuRec
        /// </summary>
        SecuRec = 0xD5,

        /// <summary>
        /// Book setting for DVD+R
        /// </summary>
        BitSetR = 0x0A,
        /// <summary>
        /// Book setting for DVD+R DL
        /// </summary>
        BitSetRDL = 0x0E,
        /// <summary>
        /// Plextor SpeedRead
        /// </summary>
        SpeedRead = 0xBB
    }

    public enum SscLogicalIdTypes : byte
    {
        /// <summary>
        /// Logical object identifier
        /// </summary>
        ObjectId = 0,
        /// <summary>
        /// Logical file identifier
        /// </summary>
        FileId = 1,
        /// <summary>
        /// Logical set identifier
        /// </summary>
        SetId = 2,
        /// <summary>
        /// Reserved
        /// </summary>
        Reserved = 3
    }

    public enum SscPositionForms : byte
    {
        /// <summary>
        /// 20 bytes using logical block addresses
        /// </summary>
        Short = 0,
        /// <summary>
        /// 20 bytes using vendor-specified values
        /// </summary>
        VendorShort = 1,
        /// <summary>
        /// Equivalent to <see cref="Long"/> on SSC-1
        /// </summary>
        OldLong = 2,
        /// <summary>
        /// Invalid: Equivalent to LONG + BT on SSC-1
        /// </summary>
        OldLongVendor = 3,
        /// <summary>
        /// Invalid: Equivalent to TCLP on SSC-1
        /// </summary>
        OldTclp = 4,
        /// <summary>
        /// Invalid: Equivalent to TCLP + BT on SSC-1
        /// </summary>
        OldTclpVendor = 5,
        /// <summary>
        /// 32 bytes
        /// </summary>
        Long = 6,
        /// <summary>
        /// Invalid: Equivalent to TCLP + LONG + BT on SSC-1
        /// </summary>
        OldLongTclpVendor = 7,
        /// <summary>
        /// From 28 bytes to allocation length
        /// </summary>
        Extended = 8
    }

    public enum ScsiAttributeAction : byte
    {
        /// <summary>
        /// Return attribute values
        /// </summary>
        Values = 0,
        /// <summary>
        /// Return a list of available attributes
        /// </summary>
        List = 1,
        /// <summary>
        /// Returns a list of known logical volume numbers
        /// </summary>
        VolumeList = 2,
        /// <summary>
        /// Returns a list of known partition numbers
        /// </summary>
        PartitionList = 3,
        /// <summary>
        /// Returns a list of elements containing volumes with MAM
        /// </summary>
        ElementList = 4,
        /// <summary>
        /// Returns a list of supported attribute identifiers
        /// </summary>
        Supported = 5
    }

    public enum FujitsuDisplayModes : byte
    {
        /// <summary>
        /// Message is displayed until next tape operation starts
        /// </summary>
        Idle = 0,
        /// <summary>
        /// Message is displayed only if a cartridge is inserted, until its removal
        /// </summary>
        Cart = 1,
        /// <summary>
        /// Message is only displayed when drive is ready
        /// </summary>
        Ready = 2,
        /// <summary>
        /// Cancels current display
        /// </summary>
        Cancel = 3,
        /// <summary>
        /// Message is displayed only if a cartridge is inserted. When removed, only second half of the message is displayed.
        /// </summary>
        Half = 7
    }
}

