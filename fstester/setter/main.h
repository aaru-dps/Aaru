/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : main.h
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Contains global definitions

--[ License ] ---------------------------------------------------------------
     This program is free software: you can redistribute it and/or modify
     it under the terms of the GNU General Public License as
     published by the Free Software Foundation, either version 3 of the
     License, or (at your option) any later version.

     This program is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warraty of
     MERCHANTIBILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.

     You should have received a copy of the GNU General Public License
     along with this program.  If not, see <http://www.gnu.org/licenses/>.

-----------------------------------------------------------------------------
Copyright (C) 2011-2018 Natalia Portillo
*****************************************************************************/

#ifndef DIC_FSTESTER_GETTER_MAIN_H
#define DIC_FSTESTER_GETTER_MAIN_H

#define DIC_FSTESTER_VERSION "4.5.99.1663"
#define DIC_COPYRIGHT "Copyright (C) 2011-2018 Natalia Portillo"

#if defined(__alpha__) || defined (_M_ALPHA)
#define OS_ARCH "axp"
#elif defined(__aarch64__)
#define OS_ARCH "aarch64"
#elif defined(__arm__)
#define OS_ARCH "arm"
#elif defined(__I86__) || defined (__i86__) || defined (_M_I86)
#define OS_ARCH "x86"
#elif defined(__I386__) || defined (__i386__) || defined (__THW_INTEL) || defined (_M_IX86)
#define OS_ARCH "ia32"
#elif defined(__ia64__) || defined (_M_IA64)
#define OS_ARCH "ia64"
#elif defined(__m68k__) || defined (_M_M68K) || defined (M68000) || defined (__MC68K__)
#define OS_ARCH "m68k"
#elif defined(__mips__) || defined (__mips) || defined (__MIPS__)
#define OS_ARCH "mips"
#elif defined(__hppa__) || defined (__hppa)
#define OS_ARCH "parisc"
#elif defined(__ppc64__) || defined (__PPC64__) || defined (_ARCH_PPC64)
#define OS_ARCH "ppc64"
#elif defined(__powerpc__) || defined (_M_PPC) || defined (__PPC__) || defined (_ARCH_PPC) || defined (__POWERPC__)
#define OS_ARCH "ppc"
#elif defined(_POWER)
#define OS_ARCH "power"
#elif defined(__sparc__) || defined (__SPARC__) || defined (__sparc)
#define OS_ARCH "sparc"
#elif defined(vax)
#define OS_ARCH "vax"
#elif defined(__x86_64__) || defined (__amd64)
#define OS_ARCH "x86_64"
#else
#define OS_ARCH "unknown"
#endif

#if defined (_AIX) || defined (__TOS_AIX__)
#define OS_NAME "AIX"
#elif defined(__ANDROID__)
#define OS_NAME "Android"
#elif defined(AMIGA) || defined (__amigaos__)
#define OS_NAME "AmigaOS"
#elif defined(__BEOS__)
#define OS_NAME "BeOS"
#elif defined(__bsdi__)
#define OS_NAME "BSD/OS"
#elif defined(__CYGWIN__)
#define OS_NAME "Windows NT with Cygwin"
#elif defined(__DOS__) || defined (MSDOS)
#define OS_NAME "DOS"
#elif defined(__DragonFly__)
#define OS_NAME "DragonFly BSD"
#elif defined(__FreeBSD__)
#define OS_NAME "FreeBSD"
#elif defined(__gnu_hurd__)
#define OS_NAME "GNU/Hurd"
#elif defined(__linux__) || defined (__LINUX__) || defined (__gnu_linux)
#define OS_NAME "Linux"
#elif defined(_hpux) || defined (hpux) || defined (__hpux)
#define OS_NAME "HP-UX"
#elif defined(__INTERIX)
#define OS_NAME "Windows NT with POSIX subsystem"
#elif defined(sgi) || defined (__sgi)
#define OS_NAME "IRIX"
#elif defined(__Lynx__)
#define OS_NAME "LynxOS"
#elif defined(macintosh)
#define OS_NAME "Mac OS"
#elif defined(__APPLE__) && defined(__MACH__)
#define OS_NAME "Mac OS X"
#elif defined(__minix)
#define OS_NAME "MINIX"
#elif defined(__MORPHOS__)
#define OS_NAME "MorphOS"
#elif defined(__NetBSD__)
#define OS_NAME "NetBSD"
#elif defined(__NETWARE__) || defined (__netware__)
#define OS_NAME "NetWare"
#elif defined(__OpenBSD__)
#define OS_NAME "OpenBSD"
#elif defined(__OS2__) || defined (__os2__) && !defined (__DOS__)
#define OS_NAME "OS/2"
#elif defined(__palmos__)
#define OS_NAME "PalmOS"
#elif defined(EPLAN9)
#define OS_NAME "Plan 9"
#elif defined(__QNX__) || defined (__QNXNTO__)
#define OS_NAME "QNX"
#elif defined(_UNIXWARE7)
#define OS_NAME "UnixWare"
#elif defined(_SCO_DS)
#define OS_NAME "SCO OpenServer"
#elif defined(sun) || defined (__sun) || defined (__sun__)
#if defined (__SVR4) || defined (__svr4__)
#define OS_NAME "Solaris"
#else
#define OS_NAME "SunOS"
#endif
#elif defined(__SYLLABLE__)
#define OS_NAME "Syllable"
#elif defined(__osf__) || defined (__osf)
#define OS_NAME "Tru64 UNIX"
#elif defined(ultrix) || defined (__ultrix) || defined (__ultrix__)
#define OS_NAME "Ultrix"
#elif defined(VMS) || defined (__VMS)
#define OS_NAME "VMS"
#elif defined(__VXWORKS__) || defined (__vxworks)
#define OS_NAME "VxWorks"
#elif defined(__WINDOWS__) || defined (__TOS_WIN__) || defined (__WIN32__) || defined (_WIN64) || defined (_WIN32) || defined (__NT__)
#define OS_NAME "Windows"
#elif defined(M_XENIX)
#define OS_NAME "XENIX"
#elif defined(__MVS__)
#define OS_NAME "z/OS"
#elif defined (unix) || defined (UNIX) || defined (__unix) || defined (__unix__) || defined (__UNIX__)
#define OS_NAME "Unknown UNIX"
#else
#define OS_NAME "Unknown"
#endif

#endif

