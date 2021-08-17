// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceType.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines enumerations of device types.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes.Enums
{
    /// <summary>
    /// Device types
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Unknown device type
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// ATA device
        /// </summary>
        ATA          = 1,
        /// <summary>
        /// ATA Packet device (aka SCSI over ATA)
        /// </summary>
        ATAPI = 2,
        /// <summary>
        /// SCSI device (or USB-MSC, SBP2, FC, UAS, etc)
        /// </summary>
        SCSI    = 3,
        /// <summary>
        /// SecureDigital memory card
        /// </summary>
        SecureDigital = 4,
        /// <summary>
        /// MultiMediaCard memory card
        /// </summary>
        MMC   = 5,
        /// <summary>
        /// NVMe device
        /// </summary>
        NVMe    = 6
    }
}