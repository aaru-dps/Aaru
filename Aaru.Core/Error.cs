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
// Copyright © 2011-2024 Natalia Portillo
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
        return DetectOS.GetRealPlatformID() switch
               {
                   PlatformID.Win32S
                    or PlatformID.Win32Windows
                    or PlatformID.Win32NT
                    or PlatformID.WinCE
                    or PlatformID.WindowsPhone
                    or PlatformID.Xbox => PrintWin32Error(errno),
                   PlatformID.Unix
                    or PlatformID.MacOSX
                    or PlatformID.iOS
                    or PlatformID.Linux
                    or PlatformID.Solaris
                    or PlatformID.NetBSD
                    or PlatformID.OpenBSD
                    or PlatformID.FreeBSD
                    or PlatformID.DragonFly
                    or PlatformID.Android
                    or PlatformID.Tizen
                    or PlatformID.Hurd
                    or PlatformID.Haiku
                    or PlatformID.HPUX
                    or PlatformID.AIX
                    or PlatformID.OS400
                    or PlatformID.IRIX
                    or PlatformID.Minix
                    or PlatformID.QNX
                    or PlatformID.SINIX
                    or PlatformID.Tru64
                    or PlatformID.Ultrix
                    or PlatformID.OpenServer
                    or PlatformID.UnixWare
                    or PlatformID.zOS => PrintUnixError(errno),
                   PlatformID.Wii          => string.Format(Localization.Core.error_code_0, errno),
                   PlatformID.WiiU         => string.Format(Localization.Core.error_code_0, errno),
                   PlatformID.PlayStation3 => string.Format(Localization.Core.error_code_0, errno),
                   PlatformID.PlayStation4 => string.Format(Localization.Core.error_code_0, errno),
                   PlatformID.NonStop      => string.Format(Localization.Core.error_code_0, errno),
                   PlatformID.Unknown      => string.Format(Localization.Core.error_code_0, errno),
                   _                       => string.Format(Localization.Core.error_code_0, errno)
               };
    }

    static string PrintUnixError(int errno)
    {
        return errno switch
               {
                   2 or 19 => // ENODEV
                       // ENOENT
                       Localization.Core.The_specified_device_cannot_be_found,
                   13 => // EACCESS
                       Localization.Core.Not_enough_permissions_to_open_the_device,
                   16 => // EBUSY
                       Localization.Core.The_specified_device_is_in_use_by_another_process,
                   30 => // EROFS
                       Localization.Core.Cannot_open_the_device_in_writable_mode_as_needed_by_some_commands,
                   _ => string.Format(Localization.Core.error_code_0, errno)
               };
    }

    static string PrintWin32Error(int errno)
    {
        return errno switch
               {
                   2 or 3 => // ERROR_PATH_NOT_FOUND
                       // ERROR_FILE_NOT_FOUND
                       Localization.Core.The_specified_device_cannot_be_found,
                   5 => // ERROR_ACCESS_DENIED
                       Localization.Core.Not_enough_permissions_to_open_the_device,
                   19 => // ERROR_WRITE_PROTECT
                       Localization.Core.Cannot_open_the_device_in_writable_mode_as_needed_by_some_commands,
                   32 or 33 or 108 or 170 => // ERROR_BUSY
                       // ERROR_DRIVE_LOCKED
                       // ERROR_LOCK_VIOLATION
                       // ERROR_SHARING_VIOLATION
                       Localization.Core.The_specified_device_is_in_use_by_another_process,
                   130 => // ERROR_DIRECT_ACCESS_HANDLE
                       Localization.Core.Tried_to_open_a_file_instead_of_a_device,
                   _ => string.Format(Localization.Core.error_code_0, errno)
               };
    }
}