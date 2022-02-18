// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Error.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts system error numbers to human language.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Interop;

namespace Aaru.Core
{
    /// <summary>Prints the description of a system error number.</summary>
    public static class Error
    {
        /// <summary>Prints the description of a system error number.</summary>
        /// <param name="errno">System error number.</param>
        /// <returns>Error description.</returns>
        public static string Print(int errno)
        {
            switch(DetectOS.GetRealPlatformID())
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                case PlatformID.WindowsPhone:
                case PlatformID.Xbox: return PrintWin32Error(errno);
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                case PlatformID.iOS:
                case PlatformID.Linux:
                case PlatformID.Solaris:
                case PlatformID.NetBSD:
                case PlatformID.OpenBSD:
                case PlatformID.FreeBSD:
                case PlatformID.DragonFly:
                case PlatformID.Android:
                case PlatformID.Tizen:
                case PlatformID.Hurd:
                case PlatformID.Haiku:
                case PlatformID.HPUX:
                case PlatformID.AIX:
                case PlatformID.OS400:
                case PlatformID.IRIX:
                case PlatformID.Minix:
                case PlatformID.QNX:
                case PlatformID.SINIX:
                case PlatformID.Tru64:
                case PlatformID.Ultrix:
                case PlatformID.OpenServer:
                case PlatformID.UnixWare:
                case PlatformID.zOS: return PrintUnixError(errno);
                case PlatformID.Wii:          return $"Unknown error code {errno}";
                case PlatformID.WiiU:         return $"Unknown error code {errno}";
                case PlatformID.PlayStation3: return $"Unknown error code {errno}";
                case PlatformID.PlayStation4: return $"Unknown error code {errno}";
                case PlatformID.NonStop:      return $"Unknown error code {errno}";
                case PlatformID.Unknown:      return $"Unknown error code {errno}";
                default:                      return $"Unknown error code {errno}";
            }
        }

        static string PrintUnixError(int errno)
        {
            switch(errno)
            {
                case 2:  // ENOENT
                case 19: // ENODEV
                    return "The specified device cannot be found.";
                case 13: // EACCESS
                    return "Not enough permissions to open the device.";
                case 16: // EBUSY
                    return "The specified device is in use by another process.";
                case 30: // EROFS
                    return "Cannot open the device in writable mode, as needed by some commands.";
                default: return $"Unknown error code {errno}";
            }
        }

        static string PrintWin32Error(int errno)
        {
            switch(errno)
            {
                case 2: // ERROR_FILE_NOT_FOUND
                case 3: // ERROR_PATH_NOT_FOUND
                    return "The specified device cannot be found.";
                case 5: // ERROR_ACCESS_DENIED
                    return "Not enough permissions to open the device.";
                case 19: // ERROR_WRITE_PROTECT
                    return "Cannot open the device in writable mode, as needed by some commands.";
                case 32:  // ERROR_SHARING_VIOLATION
                case 33:  // ERROR_LOCK_VIOLATION
                case 108: // ERROR_DRIVE_LOCKED
                case 170: // ERROR_BUSY
                    return "The specified device is in use by another process.";
                case 130: // ERROR_DIRECT_ACCESS_HANDLE
                    return "Tried to open a file instead of a device.";
                default: return $"Unknown error code {errno}";
            }
        }
    }
}