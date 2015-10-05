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
            /// SASI 0a
            /// </summary>
            TestUnitReady = 0x00,
            /// <summary>
            /// Sets the unit to a specific known state.
            /// SASI 0a
            /// </summary>
            RezeroUnit = 0x01,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            RequestSyndrome = 0x02,
            /// <summary>
            /// Returns unit sense.
            /// SASI 0a
            /// </summary>
            RequestSense = 0x03,
            /// <summary>
            /// Formats the entire media.
            /// SASI 0a
            /// </summary>
            FormatUnit = 0x04,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            CheckTrackFormat = 0x05,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            FormatTrack = 0x06,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            FormatBadTrack = 0x06,
            /// <summary>
            /// Reads a block from the device.
            /// SASI 0a
            /// </summary>
            Read = 0x08,
            /// <summary>
            /// SASI 0a
            /// Unknown
            /// </summary>
            WriteProtectSector = 0x09,
            /// <summary>
            /// Writes a block to the device.
            /// SASI 0a
            /// </summary>
            Write = 0x0A,
            /// <summary>
            /// Moves the device reading mechanism to the specified block.
            /// SASI 0a
            /// </summary>
            Seek = 0x0B,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            VerifyRestore = 0x0D,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            AssignAlternateDiskTrack = 0x0E,
            /// <summary>
            /// Writes a File Mark on the device.
            /// SASI 0c
            /// </summary>
            WriteFileMark = 0x0F,
            /// <summary>
            /// Reserves the device for use by the iniator.
            /// SASI 0a
            /// </summary>
            ReserveUnit = 0x12,
            /// <summary>
            /// Release the device from the reservation.
            /// SASI 0a
            /// </summary>
            ReleaseUnit= 0x13,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            WriteProtectDrive = 0x14,
            /// <summary>
            /// Writes and verifies blocks to the device.
            /// SASI 0c
            /// </summary>
            WriteVerify = 0x14,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            ReleaseWriteProtect = 0x15,
            /// <summary>
            /// Verifies blocks.
            /// SASI 0c
            /// </summary>
            Verify = 0x15,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            ReadNoSeek = 0x16,
            /// <summary>
            /// Gets the number of blocks in device.
            /// SASI 0c
            /// </summary>
            ReadCapacity = 0x16,
            /// <summary>
            /// Searches data on blocks
            /// SASI 0a
            /// </summary>
            SearchDataEqual = 0x17,
            /// <summary>
            /// Searches data on blocks using major than or equal comparison
            /// SASI 0a
            /// </summary>
            SearchDataHigh = 0x18,
            /// <summary>
            /// Searches data on blocks using minor than or equal comparison
            /// SASI 0a
            /// </summary>
            SearchDataLow = 0x19,
            /// <summary>
            /// Reads analysis data from a device
            /// SASI 0a
            /// </summary>
            ReadDiagnostic = 0x1A,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            VerifyData = 0x1B,
            /// <summary>
            /// Requests a device to run a diagnostic
            /// SASI 0c
            /// </summary>
            WriteDiagnostic = 0x1B,
            /// <summary>
            /// Gets information about a device
            /// SASI 0c
            /// </summary>
            Inquiry = 0x1F,
            #endregion SASI Class 0 commands

            #region SASI Class 1 commands
            /// <summary>
            /// SASI 0a
            /// Unknown
            /// </summary>
            Copy = 0x20,
            /// <summary>
            /// SASI 0a
            /// Unknown
            /// </summary>
            Restore = 0x21,
            /// <summary>
            /// SASI 0a
            /// Unknown
            /// </summary>
            Backup = 0x22,
            /// <summary>
            /// SASI 0a
            /// Unknown
            /// </summary>
            SetBlockLimitsOld = 0x26,
            /// <summary>
            /// Sets write or read limits from a specified block
            /// SASI 0c
            /// </summary>
            SetBlockLimits = 0x28,
            #endregion SASI Class 1 commands

            #region SASI Class 2 commands
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            Load = 0x40,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            Unload = 0x41,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            Rewind = 0x42,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            SpaceForward = 0x43,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            SpaceForwardFileMark = 0x44,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            SpaceReserve = 0x45,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            SpaceReserveFileMark = 0x46,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            TrackSelect = 0x47,
            /// <summary>
            /// Reads blocks from device
            /// SASI 0a
            /// </summary>
            Read10 = 0x48,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            ReadVerify = 0x49,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            ReadDiagnosticClass2 = 0x4A,
            /// <summary>
            /// Writes blocks to device
            /// SASI 0a
            /// </summary>
            Write10 = 0x4B,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            WriteFileMarkClass2 = 0x4C,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            WriteExtended = 0x4D,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            WriteExtendedFileMark = 0x4E,
            /// <summary>
            /// Unknown
            /// SASI 0a
            /// </summary>
            WriteErase = 0x4F,
            #endregion SASI Class 2 commands

            #region SASI Class 3 commands
            /// <summary>
            /// SASI 0a
            /// </summary>
            Skip = 0x60,
            /// <summary>
            /// SASI 0a
            /// </summary>
            Space = 0x61,
            /// <summary>
            /// SASI 0a
            /// </summary>
            Return = 0x62,
            /// <summary>
            /// SASI 0a
            /// </summary>
            Tab = 0x63,
            /// <summary>
            /// SASI 0a
            /// </summary>
            ReadControl = 0x64,
            /// <summary>
            /// SASI 0a
            /// </summary>
            Write3 = 0x65,
            /// <summary>
            /// SASI 0a
            /// </summary>
            WriteControl = 0x66,
            #endregion SASI Class 3 commands

            #region SASI Class 6 commands
            /// <summary>
            /// SASI 0a
            /// </summary>
            DefineFloppyDiskTracFormat = 0xC0,
            /// <summary>
            /// SASI 0a
            /// </summary>
            FormatDriveErrorMap = 0xC4,
            /// <summary>
            /// SASI 0a
            /// </summary>
            ReadErrorMap = 0xC5,
            /// <summary>
            /// SASI 0a
            /// </summary>
            ReadDriveType = 0xC6,
            #endregion SASI Class 6 commands

            #region SASI Class 7 commands
            /// <summary>
            /// SASI 0a
            /// </summary>
            RamDiagnostic = 0xE0,
            /// <summary>
            /// SASI 0a
            /// </summary>
            WriteECC = 0xE1,
            /// <summary>
            /// SASI 0a
            /// </summary>
            ReadID = 0xE2,
            /// <summary>
            /// SASI 0a
            /// </summary>
            DriveDiagnostic = 0xE3
            #endregion SASI Class 7 commands
        }
        #endregion SASI Commands
    }
}

