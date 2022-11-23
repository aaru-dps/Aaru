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

namespace Aaru.Core;

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
            case PlatformID.Wii:          return string.Format(Localization.Core.error_code_0, errno);
            case PlatformID.WiiU:         return string.Format(Localization.Core.error_code_0, errno);
            case PlatformID.PlayStation3: return string.Format(Localization.Core.error_code_0, errno);
            case PlatformID.PlayStation4: return string.Format(Localization.Core.error_code_0, errno);
            case PlatformID.NonStop:      return string.Format(Localization.Core.error_code_0, errno);
            case PlatformID.Unknown:      return string.Format(Localization.Core.error_code_0, errno);
            default:                      return string.Format(Localization.Core.error_code_0, errno);
        }
    }

    static string PrintUnixError(int errno)
    {
        switch(errno)
        {
            case 2:  // ENOENT
            case 19: // ENODEV
                return Localization.Core.The_specified_device_cannot_be_found;
            case 13: // EACCESS
                return Localization.Core.Not_enough_permissions_to_open_the_device;
            case 16: // EBUSY
                return Localization.Core.The_specified_device_is_in_use_by_another_process;
            case 30: // EROFS
                return Localization.Core.Cannot_open_the_device_in_writable_mode_as_needed_by_some_commands;
            default: return string.Format(Localization.Core.error_code_0, errno);
        }
    }

    static string PrintWin32Error(int errno)
    {
        switch(errno)
        {
            case 2: // ERROR_FILE_NOT_FOUND
            case 3: // ERROR_PATH_NOT_FOUND
                return Localization.Core.The_specified_device_cannot_be_found;
            case 5: // ERROR_ACCESS_DENIED
                return Localization.Core.Not_enough_permissions_to_open_the_device;
            case 19: // ERROR_WRITE_PROTECT
                return Localization.Core.Cannot_open_the_device_in_writable_mode_as_needed_by_some_commands;
            case 32:  // ERROR_SHARING_VIOLATION
            case 33:  // ERROR_LOCK_VIOLATION
            case 108: // ERROR_DRIVE_LOCKED
            case 170: // ERROR_BUSY
                return Localization.Core.The_specified_device_is_in_use_by_another_process;
            case 130: // ERROR_DIRECT_ACCESS_HANDLE
                return Localization.Core.Tried_to_open_a_file_instead_of_a_device;
            default: return string.Format(Localization.Core.error_code_0, errno);
        }
    }
}