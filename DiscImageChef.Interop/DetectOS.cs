//
// Interop.DetectOS.cs
//
// Author:
//       Natalia Portillo <claunia@claunia.com>
//
// Copyright (c) 2015 © Claunia.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Interop
{
    public static class DetectOS
    {
        /// <summary>
        /// POSIX uname structure, size from OSX, big enough to handle extra fields
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct utsname
        {
            /// <summary>
            /// System name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string sysname;
            /// <summary>
            /// Node name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string nodename;
            /// <summary>
            /// Release level
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string release;
            /// <summary>
            /// Version level
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string version;
            /// <summary>
            /// Hardware level
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string machine;
        }

        [DllImport("libc", SetLastError = true)]
        static extern int uname(out utsname name);

        [DllImport("libc", SetLastError = true, EntryPoint = "sysctlbyname", CharSet = CharSet.Ansi)]
        static extern int OSX_sysctlbyname(string name, IntPtr oldp, IntPtr oldlenp, IntPtr newp, uint newlen);

        public static Interop.PlatformID GetRealPlatformID()
        {
            if ((int)Environment.OSVersion.Platform < 4 ||
                (int)Environment.OSVersion.Platform == 5)
            {
                return (Interop.PlatformID)((int)Environment.OSVersion.Platform);
            }

            utsname unixname;
            int error = uname(out unixname);
            if (error != 0)
                throw new Exception(String.Format("Unhandled exception calling uname: {0}", Marshal.GetLastWin32Error()));

            switch (unixname.sysname)
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
                        int osx_error;

                        IntPtr pLen = Marshal.AllocHGlobal(sizeof(int));
                        osx_error = OSX_sysctlbyname("hw.machine", IntPtr.Zero, pLen, IntPtr.Zero, 0);
                        if (osx_error != 0)
                        {
                            Marshal.FreeHGlobal(pLen);

                            throw new Exception(String.Format("Unhandled exception calling uname: {0}", Marshal.GetLastWin32Error()));
                        }

                        int length = Marshal.ReadInt32(pLen);
                        IntPtr pStr = Marshal.AllocHGlobal(length);
                        osx_error = OSX_sysctlbyname("hw.machine", pStr, pLen, IntPtr.Zero, 0);
                        if (osx_error != 0)
                        {
                            Marshal.FreeHGlobal(pStr);
                            Marshal.FreeHGlobal(pLen);

                            throw new Exception(String.Format("Unhandled exception calling uname: {0}", Marshal.GetLastWin32Error()));
                        }

                        string machine = Marshal.PtrToStringAnsi(pStr);

                        Marshal.FreeHGlobal(pStr);
                        Marshal.FreeHGlobal(pLen);

                        if (machine.StartsWith("iPad", StringComparison.Ordinal) ||
                            machine.StartsWith("iPod", StringComparison.Ordinal) ||
                            machine.StartsWith("iPhone", StringComparison.Ordinal))
                            return PlatformID.iOS;

                        return PlatformID.MacOSX;
                    }
                case "GNU":
                    return PlatformID.Hurd;
                case "FreeBSD":
                case "GNU/kFreeBSD":
                    return PlatformID.FreeBSD;
                case "DragonFly":
                    return PlatformID.DragonFly;
                case "Haiku":
                    return PlatformID.Haiku;
                case "HP-UX":
                    return PlatformID.HPUX;
                case "AIX":
                    return PlatformID.AIX;
                case "OS400":
                    return PlatformID.OS400;
                case "IRIX":
                case "IRIX64":
                    return PlatformID.IRIX;
                case "Minix":
                    return PlatformID.Minix;
                case "NetBSD":
                    return PlatformID.NetBSD;
                case "NONSTOP_KERNEL":
                    return PlatformID.NonStop;
                case "OpenBSD":
                    return PlatformID.OpenBSD;
                case "QNX":
                    return PlatformID.QNX;
                case "SINIX-Y":
                    return PlatformID.SINIX;
                case "SunOS":
                    return PlatformID.Solaris;
                case "OSF1":
                    return PlatformID.Tru64;
                case "ULTRIX":
                    return PlatformID.Ultrix;
                case "SCO_SV":
                    return PlatformID.OpenServer;
                case "UnixWare":
                    return PlatformID.UnixWare;
                case "Interix":
                case "UWIN-W7":
                    return PlatformID.Win32NT;
                default:
                    {
                        if (unixname.sysname.StartsWith("CYGWIN_NT", StringComparison.Ordinal) ||
                            unixname.sysname.StartsWith("MINGW32_NT", StringComparison.Ordinal) ||
                            unixname.sysname.StartsWith("MSYS_NT", StringComparison.Ordinal) ||
                            unixname.sysname.StartsWith("UWIN", StringComparison.Ordinal))
                            return PlatformID.Win32NT;

                        return PlatformID.Unknown;
                    }
            }
        }

        /// <summary>
        /// Checks if the underlying runtime runs in 64-bit mode
        /// </summary>
        public static bool Is64Bit()
        {
            return IntPtr.Size == 8;
        }

        /// <summary>
        /// Checks if the underlying runtime runs in 32-bit mode
        /// </summary>
        public static bool Is32Bit()
        {
            return IntPtr.Size == 4;
        }
    }
}