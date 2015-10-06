using System;

namespace DiscImageChef.Devices
{
    public static class Enums
    {
        /// <summary>
        /// SASI commands
        /// Commands 0x00 to 0x1F are 6-byte
        /// Commands 0x20 to 0x3F are 10-byte
        /// Commands 0x40 to 0x5F are 8-byte
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
            ReserveUnit = 0x12,
            /// <summary>
            /// Release the device from the reservation.
            /// SASI rev. 0a
            /// </summary>
            ReleaseUnit= 0x13,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            WriteProtectDrive = 0x14,
            /// <summary>
            /// Writes and verifies blocks to the device.
            /// SASI rev. 0c
            /// </summary>
            WriteVerify = 0x14,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            ReleaseWriteProtect = 0x15,
            /// <summary>
            /// Verifies blocks.
            /// SASI rev. 0c
            /// </summary>
            Verify = 0x15,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            ReadNoSeek = 0x16,
            /// <summary>
            /// Gets the number of blocks in device.
            /// SASI rev. 0c
            /// </summary>
            ReadCapacity = 0x16,
            /// <summary>
            /// Searches data on blocks
            /// SASI rev. 0a
            /// </summary>
            SearchDataEqual = 0x17,
            /// <summary>
            /// Searches data on blocks using major than or equal comparison
            /// SASI rev. 0a
            /// </summary>
            SearchDataHigh = 0x18,
            /// <summary>
            /// Searches data on blocks using minor than or equal comparison
            /// SASI rev. 0a
            /// </summary>
            SearchDataLow = 0x19,
            /// <summary>
            /// Reads analysis data from a device
            /// SASI rev. 0a
            /// </summary>
            ReadDiagnostic = 0x1A,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            VerifyData = 0x1B,
            /// <summary>
            /// Requests a device to run a diagnostic
            /// SASI rev. 0c
            /// </summary>
            WriteDiagnostic = 0x1B,
            /// <summary>
            /// Gets information about a device
            /// SASI rev. 0c
            /// </summary>
            Inquiry = 0x1F,
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
            SetBlockLimitsOld = 0x26,
            /// <summary>
            /// Sets write or read limits from a specified block
            /// SASI rev. 0c
            /// </summary>
            SetBlockLimits = 0x28,
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
            SpaceReserve = 0x45,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            SpaceReserveFileMark = 0x46,
            /// <summary>
            /// Unknown
            /// SASI rev. 0a
            /// </summary>
            TrackSelect = 0x47,
            /// <summary>
            /// Reads blocks from device
            /// SASI rev. 0a
            /// </summary>
            Read10 = 0x48,
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
            Write10 = 0x4B,
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

            #region SASI Class 6 commands
            /// <summary>
            /// SASI rev. 0a
            /// </summary>
            DefineFloppyDiskTracFormat = 0xC0,
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
            DriveDiagnostic = 0xE3
            #endregion SASI Class 7 commands
        }
        #endregion SASI Commands

        #region SCSI Commands
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Compare = 0x39,
            /// <summary>
            /// Copies data between two devices
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Copy = 0x18,
            /// <summary>
            /// Copies data between two devices and verifies the copy is correct.
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            CopyAndVerify = 0x3A,
            /// <summary>
            /// Copies data between two devices
            /// SPC-2 rev. 20
            /// </summary>
            ExtendedCopy = 0x83,
            /// <summary>
            /// Requests information about the device
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Inquiry = 0x12,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ModeSelect = 0x15,
            /// <summary>
            /// Sets device parameters
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ModeSelect10 = 0x55,
            /// <summary>
            /// Gets device parameters
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ReceiveDiagnostic = 0x1C,
            /// <summary>
            /// Releases a previously reserved LUN or extents
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Release = 0x17,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            RequestSense = 0x03,
            /// <summary>
            /// Reserves a LUN or extent
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Reserve = 0x16,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SendDiagnostic = 0x1D,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            TestUnitReady = 0x00,
            /// <summary>
            /// Writes attribute values to medium auxiliary memory
            /// SPC-3 rev. 21b
            /// </summary>
            WriteAttribute = 0x8C,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            FormatUnit = 0x04,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Read = 0x08,
            /// <summary>
            /// Reads blocks from device
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Read10 = 0x28,
            /// <summary>
            /// Reads blocks from device
            /// SBC-2 rev. 4
            /// </summary>
            Read16 = 0x88,
            /// <summary>
            /// Gets device capacity
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            RezeroUnit = 0x01,
            /// <summary>
            /// Searches data on blocks
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SearchDataEqual = 0x31,
            /// <summary>
            /// Searches data on blocks using major than or equal comparison
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SearchDataHigh = 0x30,
            /// <summary>
            /// Searches data on blocks using minor than or equal comparison
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SearchDataLow = 0x32,
            /// <summary>
            /// Requests the device to seek to a specified blocks
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Seek = 0x0B,
            /// <summary>
            /// Requests the device to seek to a specified blocks
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Seek10 = 0x2B,
            /// <summary>
            /// Defines the range within which subsequent linked commands may operate
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SetLimits = 0x33,
            /// <summary>
            /// Requests the device to enable or disable the LUN for media access operations
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Verify10 = 0x2F,
            /// <summary>
            /// Verifies blocks on the device
            /// SBC-2 rev. 4
            /// </summary>
            Verify16 = 0x8F,
            /// <summary>
            /// Writes blocks to the device
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Write = 0x0A,
            /// <summary>
            /// Writes blocks to the device
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Write10 = 0x2A,
            /// <summary>
            /// Writes blocks to the device
            /// SBC-2 rev. 4
            /// </summary>
            Write16 = 0x8A,
            /// <summary>
            /// Writes blocks to the device and then verifies them
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            WriteAndVerify = 0x2E,
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Erase = 0x19,
            /// <summary>
            /// Enables or disables the LUN for further operations
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ReadBlockLimits = 0x05,
            /// <summary>
            /// Reads the current position
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ReadPosition = 0x34,
            /// <summary>
            /// Reads blocks from the device, in reverse order
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            ReadReverse = 0x0F,
            /// <summary>
            /// Retrieves data from the device buffer that has not been successfully written to the medium (or printed)
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            RecoverBufferedData = 0x14,
            /// <summary>
            /// Requests information regarding the supported densities for the logical unit
            /// SSC-1 rev. 22
            /// </summary>
            ReportDensitySupport = 0x44,
            /// <summary>
            /// Seeks the medium to the beginning of partition in current partition
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Rewind = 0x01,
            /// <summary>
            /// A variety of positioning functions
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Space = 0x11,
            /// <summary>
            /// A variety of positioning functions
            /// SSC-2 rev. 09
            /// </summary>
            Space16 = 0x91,
            /// <summary>
            /// Verifies one or more blocks from the next one
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Verify = 0x13,
            /// <summary>
            /// Writes the specified number of filemarks or setmarks in the current position
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            WriteFileMarks=0x10,
            #endregion SCSI Streaming Commands (SSC)

            #region SCSI Streaming Commands for Printers (SSC)
            /// <summary>
            /// Specifies forms or fronts
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Format = 0x04,
            /// <summary>
            /// Transfers data to be printed
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Print = 0x0A,
            /// <summary>
            /// Transfers data to be printed with a slew value
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SlewAndPrint = 0x0B,
            /// <summary>
            /// Halts printing
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            StopPrint = 0x1B,
            /// <summary>
            /// Assures that the data in the buffer has been printed, or, for other devices, written to media
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            SynchronizeBuffer = 0x10,
            #endregion SCSI Streaming Commands for Printers (SSC)

            #region SCSI Processor Commands
            /// <summary>
            /// Transfers data from the device
            /// SCSI-2 X3T9.2/375R rev. 10l
            /// </summary>
            Receive = 0x08,
            /// <summary>
            /// Sends data to the device
            /// SCSI-2 X3T9.2/375R rev. 10l
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
            #endregion

            #region ATA Command Pass-Through
            /// <summary>
            /// Sends a 24-bit ATA command to the device
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

            #region SCSI Commands with unknown meaning, mostly vendor specific
            ZbcOut = 0x94,
            ZbcOut2 = 0x95,
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
            VariableSizedCDB = 0x7F
        }
        #endregion SCSI Commands
    }
}

