using System;
using Aaru.CommonTypes.Interop;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

namespace Aaru.Core
{
    public static class Error
    {
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

            throw new Exception("Arrived an unexpected place");
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