// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PlatformID.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Interop services.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains an enhanced PlatformID enumeration.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.CommonTypes.Interop;

/// <summary>Contains an arbitrary list of OSes, even if .NET does not run on them</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum PlatformID
{
    /// <summary>Win32s</summary>
    Win32S = 0,
    /// <summary>Win32 (Windows 9x)</summary>
    Win32Windows = 1,
    /// <summary>Windows NT</summary>
    Win32NT = 2,
    /// <summary>Windows Mobile</summary>
    WinCE = 3,
    /// <summary>UNIX (do not use, too generic)</summary>
    Unix = 4,
    /// <summary>Xbox 360</summary>
    Xbox = 5,
    /// <summary>OS X</summary>
    MacOSX = 6,
    /// <summary>iOS is not OS X</summary>
    iOS = 7,
    /// <summary>Linux</summary>
    Linux = 8,
    /// <summary>Sun Solaris</summary>
    Solaris = 9,
    /// <summary>NetBSD</summary>
    NetBSD = 10,
    /// <summary>OpenBSD</summary>
    OpenBSD = 11,
    /// <summary>FreeBSD</summary>
    FreeBSD = 12,
    /// <summary>DragonFly BSD</summary>
    DragonFly = 13,
    /// <summary>Nintendo Wii</summary>
    Wii = 14,
    /// <summary>Nintendo Wii U</summary>
    WiiU = 15,
    /// <summary>Sony PlayStation 3</summary>
    PlayStation3 = 16,
    /// <summary>Sony Playstation 4</summary>
    PlayStation4 = 17,
    /// <summary>Google Android</summary>
    Android = 18,
    /// <summary>Samsung Tizen</summary>
    Tizen = 19,
    /// <summary>Windows Phone</summary>
    WindowsPhone = 20,
    /// <summary>GNU/Hurd</summary>
    Hurd = 21,
    /// <summary>Haiku</summary>
    Haiku = 22,
    /// <summary>HP-UX</summary>
    HPUX = 23,
    /// <summary>AIX</summary>
    AIX = 24,
    /// <summary>OS/400</summary>
    OS400 = 25,
    /// <summary>IRIX</summary>
    IRIX = 26,
    /// <summary>Minix</summary>
    Minix = 27,
    /// <summary>NonStop</summary>
    NonStop = 28,
    /// <summary>QNX</summary>
    QNX = 29,
    /// <summary>SINIX</summary>
    SINIX = 30,
    /// <summary>Tru64 UNIX</summary>
    Tru64 = 31,
    /// <summary>Ultrix</summary>
    Ultrix = 32,
    /// <summary>SCO OpenServer / SCO UNIX</summary>
    OpenServer = 33,
    /// <summary>SCO UnixWare</summary>
    UnixWare = 34,
    /// <summary>IBM z/OS</summary>
    zOS = 35,
    /// <summary>Unknown</summary>
    Unknown = -1
}