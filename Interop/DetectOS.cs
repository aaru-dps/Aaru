// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DetectOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Interop services.
//
// --[ Description ] ----------------------------------------------------------
//
//     Detects underlying operating system.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes.Interop;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

/// <summary>Detects the underlying execution framework and operating system</summary>
public static class DetectOS
{
    /// <summary>Are we running under Mono?</summary>
    public static readonly bool IsMono =
        RuntimeInformation.FrameworkDescription.StartsWith("Mono", StringComparison.Ordinal);
    /// <summary>Are we running under .NET Framework?</summary>
    public static readonly bool IsNetFramework =
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal);
    /// <summary>Are we running under .NET Core?</summary>
    public static readonly bool IsNetCore =
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.Ordinal);
    /// <summary>Are we running under .NET Native?</summary>
    public static readonly bool IsNetNative =
        RuntimeInformation.FrameworkDescription.StartsWith(".NET Native", StringComparison.Ordinal);

    /// <summary>Checks if the underlying runtime runs in 64-bit mode</summary>
    public static readonly bool Is64Bit = IntPtr.Size == 8;

    /// <summary>Checks if the underlying runtime runs in 32-bit mode</summary>
    public static readonly bool Is32Bit = IntPtr.Size == 4;

    /// <summary>Are we running under Windows?</summary>
    public static bool IsWindows => GetRealPlatformID() == PlatformID.Win32NT      ||
                                    GetRealPlatformID() == PlatformID.Win32S       ||
                                    GetRealPlatformID() == PlatformID.Win32Windows ||
                                    GetRealPlatformID() == PlatformID.WinCE        ||
                                    GetRealPlatformID() == PlatformID.WindowsPhone ||
                                    GetRealPlatformID() == PlatformID.Xbox;

    /// <summary>Are we running with administrative (root) privileges?</summary>
    public static bool IsAdmin
    {
        get
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Environment.UserName == "root";

            bool            isAdmin;
            WindowsIdentity user = null;

            try
            {
                user = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch(UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch(Exception)
            {
                isAdmin = false;
            }
            finally
            {
                user?.Dispose();
            }

            return isAdmin;
        }
    }

    [DllImport("libc", SetLastError = true)]
    static extern int uname(out utsname name);

    [DllImport("libc", SetLastError = true, EntryPoint = "sysctlbyname", CharSet = CharSet.Ansi)]
    static extern int OSX_sysctlbyname(string name, IntPtr oldp, IntPtr oldlenp, IntPtr newp, uint newlen);

    /// <summary>Gets the real platform ID, not the incomplete .NET framework one</summary>
    /// <returns>Platform ID</returns>
    /// <exception cref="Exception">Unhandled exception</exception>
    public static PlatformID GetRealPlatformID()
    {
        if((int)Environment.OSVersion.Platform < 4 ||
           (int)Environment.OSVersion.Platform == 5)
            return (PlatformID)(int)Environment.OSVersion.Platform;

        int error = uname(out utsname unixname);

        if(error != 0)
            throw new Exception($"Unhandled exception calling uname: {Marshal.GetLastWin32Error()}");

        switch(unixname.sysname)
        {
            // TODO: Differentiate Linux, Android, Tizen
            case "Linux":
            {
            #if __ANDROID__
                        return PlatformID.Android;
            #else
                return PlatformID.Linux;
            #endif
            }

            case "Darwin":
            {
                IntPtr pLen     = Marshal.AllocHGlobal(sizeof(int));
                int    osxError = OSX_sysctlbyname("hw.machine", IntPtr.Zero, pLen, IntPtr.Zero, 0);

                if(osxError != 0)
                {
                    Marshal.FreeHGlobal(pLen);

                    throw new Exception($"Unhandled exception calling uname: {Marshal.GetLastWin32Error()}");
                }

                int    length = Marshal.ReadInt32(pLen);
                IntPtr pStr   = Marshal.AllocHGlobal(length);
                osxError = OSX_sysctlbyname("hw.machine", pStr, pLen, IntPtr.Zero, 0);

                if(osxError != 0)
                {
                    Marshal.FreeHGlobal(pStr);
                    Marshal.FreeHGlobal(pLen);

                    throw new Exception($"Unhandled exception calling uname: {Marshal.GetLastWin32Error()}");
                }

                string machine = Marshal.PtrToStringAnsi(pStr);

                Marshal.FreeHGlobal(pStr);
                Marshal.FreeHGlobal(pLen);

                if(machine != null &&
                   (machine.StartsWith("iPad", StringComparison.Ordinal) ||
                    machine.StartsWith("iPod", StringComparison.Ordinal) ||
                    machine.StartsWith("iPhone", StringComparison.Ordinal)))
                    return PlatformID.iOS;

                return PlatformID.MacOSX;
            }

            case "GNU": return PlatformID.Hurd;
            case "FreeBSD":
            case "GNU/kFreeBSD": return PlatformID.FreeBSD;
            case "DragonFly": return PlatformID.DragonFly;
            case "Haiku":     return PlatformID.Haiku;
            case "HP-UX":     return PlatformID.HPUX;
            case "AIX":       return PlatformID.AIX;
            case "OS400":     return PlatformID.OS400;
            case "IRIX":
            case "IRIX64": return PlatformID.IRIX;
            case "Minix":          return PlatformID.Minix;
            case "NetBSD":         return PlatformID.NetBSD;
            case "NONSTOP_KERNEL": return PlatformID.NonStop;
            case "OpenBSD":        return PlatformID.OpenBSD;
            case "QNX":            return PlatformID.QNX;
            case "SINIX-Y":        return PlatformID.SINIX;
            case "SunOS":          return PlatformID.Solaris;
            case "OSF1":           return PlatformID.Tru64;
            case "ULTRIX":         return PlatformID.Ultrix;
            case "SCO_SV":         return PlatformID.OpenServer;
            case "UnixWare":       return PlatformID.UnixWare;
            case "Interix":
            case "UWIN-W7": return PlatformID.Win32NT;
            default:
            {
                if(unixname.sysname.StartsWith("CYGWIN_NT", StringComparison.Ordinal)  ||
                   unixname.sysname.StartsWith("MINGW32_NT", StringComparison.Ordinal) ||
                   unixname.sysname.StartsWith("MSYS_NT", StringComparison.Ordinal)    ||
                   unixname.sysname.StartsWith("UWIN", StringComparison.Ordinal))
                    return PlatformID.Win32NT;

                return PlatformID.Unknown;
            }
        }
    }

    /// <summary>Gets a string for the current operating system REAL version (handles Darwin 1.4 and Windows 10 falsifying)</summary>
    /// <returns>Current operating system version</returns>
    public static string GetVersion()
    {
        var environ = Environment.OSVersion.Version.ToString();

        switch(GetRealPlatformID())
        {
            case PlatformID.MacOSX:
                if(Environment.OSVersion.Version.Major >= 11)
                    return environ;

                if(Environment.OSVersion.Version.Major != 1)
                    return $"10.{Environment.OSVersion.Version.Major - 4}.{Environment.OSVersion.Version.Minor}";

                switch(Environment.OSVersion.Version.Minor)
                {
                    case 3: return "10.0";
                    case 4: return "10.1";
                }

                goto default;
            case PlatformID.Win32NT:
                // From Windows 8.1 the reported version is simply falsified...
                if(Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Major >= 2 ||
                   Environment.OSVersion.Version.Major > 6)
                    return FileVersionInfo.
                           GetVersionInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                                       "KERNEL32.DLL")).ProductVersion;

                return environ;
            default: return environ;
        }
    }

    /// <summary>From a platform ID and version returns a human-readable version</summary>
    /// <param name="id">Platform ID</param>
    /// <param name="version">Version number</param>
    /// <returns>Operating system name</returns>
    public static string GetPlatformName(PlatformID id, string version = null)
    {
        switch(id)
        {
            case PlatformID.AIX:       return "AIX";
            case PlatformID.Android:   return "Android";
            case PlatformID.DragonFly: return "DragonFly BSD";
            case PlatformID.FreeBSD:   return "FreeBSD";
            case PlatformID.Haiku:     return "Haiku";
            case PlatformID.HPUX:      return "HP/UX";
            case PlatformID.Hurd:      return "Hurd";
            case PlatformID.iOS:       return "iOS";
            case PlatformID.IRIX:      return "IRIX";
            case PlatformID.Linux:
                if(!File.Exists("/proc/version"))
                    return "Linux";

                string s = File.ReadAllText("/proc/version");

                return s.Contains("Microsoft") || s.Contains("WSL") ? "Windows Subsystem for Linux" : "Linux";

            case PlatformID.MacOSX:
                if(string.IsNullOrEmpty(version))
                    return "macOS";

                string[] pieces = version.Split('.');

                if(pieces.Length < 2 ||
                   !int.TryParse(pieces[1], out int minor))
                    return "macOS";

                int.TryParse(pieces[0], out int major);

                if(minor >= 12 ||
                   major >= 11)
                    return "macOS";

                return minor >= 8 ? "OS X" : "Mac OS X";

            case PlatformID.Minix:        return "MINIX";
            case PlatformID.NetBSD:       return "NetBSD";
            case PlatformID.NonStop:      return "NonStop OS";
            case PlatformID.OpenBSD:      return "OpenBSD";
            case PlatformID.OpenServer:   return "SCO OpenServer";
            case PlatformID.OS400:        return "OS/400";
            case PlatformID.PlayStation3: return "Sony CellOS";
            case PlatformID.PlayStation4: return "Sony Orbis OS";
            case PlatformID.QNX:          return "QNX";
            case PlatformID.SINIX:        return "SINIX";
            case PlatformID.Solaris:      return "Sun Solaris";
            case PlatformID.Tizen:        return "Samsung Tizen";
            case PlatformID.Tru64:        return "Tru64 UNIX";
            case PlatformID.Ultrix:       return "Ultrix";
            case PlatformID.Unix:         return "UNIX";
            case PlatformID.UnixWare:     return "SCO UnixWare";
            case PlatformID.Wii:          return "Nintendo Wii";
            case PlatformID.WiiU:         return "Nintendo Wii U";
            case PlatformID.Win32NT:
                if(string.IsNullOrEmpty(version))
                    return "Windows NT/2000/XP/Vista/7/10";

                if(version.StartsWith("3.", StringComparison.Ordinal) ||
                   version.StartsWith("4.", StringComparison.Ordinal))
                    return "Windows NT";

                if(version.StartsWith("5.0", StringComparison.Ordinal))
                    return "Windows 2000";

                if(version.StartsWith("5.1", StringComparison.Ordinal))
                    return "Windows XP";

                if(version.StartsWith("5.2", StringComparison.Ordinal))
                    return "Windows 2003";

                if(version.StartsWith("6.0", StringComparison.Ordinal))
                    return "Windows Vista";

                if(version.StartsWith("6.1", StringComparison.Ordinal))
                    return "Windows 7";

                if(version.StartsWith("6.2", StringComparison.Ordinal))
                    return "Windows 8";

                if(version.StartsWith("6.3", StringComparison.Ordinal))
                    return "Windows 8.1";

                if(version.StartsWith("10.0", StringComparison.Ordinal))
                    return "Windows 10";

                return "Windows NT/2000/XP/Vista/7/10";
            case PlatformID.Win32S: return "Windows 3.x with win32s";
            case PlatformID.Win32Windows:
                if(string.IsNullOrEmpty(version))
                    return "Windows 9x/Me";

                if(version.StartsWith("4.0", StringComparison.Ordinal))
                    return "Windows 95";

                if(version.StartsWith("4.10.2222", StringComparison.Ordinal))
                    return "Windows 98 SE";

                if(version.StartsWith("4.1", StringComparison.Ordinal))
                    return "Windows 98";

                if(version.StartsWith("4.9", StringComparison.Ordinal))
                    return "Windows Me";

                return "Windows 9x/Me";
            case PlatformID.WinCE:        return "Windows CE/Mobile";
            case PlatformID.WindowsPhone: return "Windows Phone";
            case PlatformID.Xbox:         return "Xbox OS";
            case PlatformID.zOS:          return "z/OS";
            default:                      return id.ToString();
        }
    }

    /// <summary>POSIX uname structure, size from OSX, big enough to handle extra fields</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct utsname
    {
        /// <summary>System name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string sysname;
        /// <summary>Node name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string nodename;
        /// <summary>Release level</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string release;
        /// <summary>Version level</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string version;
        /// <summary>Hardware level</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public readonly string machine;
    }
}