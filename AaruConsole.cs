// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AaruConsole.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Console.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handlers for normal, verbose and debug consoles.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Console
{
    public delegate void WriteLineHandler(string format, params object[] arg);

    public delegate void ErrorWriteLineHandler(string format, params object[] arg);

    public delegate void VerboseWriteLineHandler(string format, params object[] arg);

    public delegate void DebugWriteLineHandler(string format, params object[] arg);

    public delegate void WriteHandler(string format, params object[] arg);

    public delegate void ErrorWriteHandler(string format, params object[] arg);

    public delegate void VerboseWriteHandler(string format, params object[] arg);

    public delegate void DebugWriteHandler(string format, params object[] arg);

    public delegate void DebugWithModuleWriteLineHandler(string module, string format, params object[] arg);

    /// <summary>
    ///     Implements a console abstraction that defines four level of messages that can be routed to different consoles:
    ///     standard, error, verbose and debug.
    /// </summary>
    public static class AaruConsole
    {
        public static event WriteLineHandler                WriteLineEvent;
        public static event ErrorWriteLineHandler           ErrorWriteLineEvent;
        public static event VerboseWriteLineHandler         VerboseWriteLineEvent;
        public static event DebugWriteLineHandler           DebugWriteLineEvent;
        public static event DebugWithModuleWriteLineHandler DebugWithModuleWriteLineEvent;

        public static event WriteHandler        WriteEvent;
        public static event ErrorWriteHandler   ErrorWriteEvent;
        public static event VerboseWriteHandler VerboseWriteEvent;
        public static event DebugWriteHandler   DebugWriteEvent;

        public static void WriteLine(string format, params object[] arg) => WriteLineEvent?.Invoke(format, arg);

        public static void ErrorWriteLine(string format, params object[] arg) =>
            ErrorWriteLineEvent?.Invoke(format, arg);

        public static void VerboseWriteLine(string format, params object[] arg) =>
            VerboseWriteLineEvent?.Invoke(format, arg);

        public static void DebugWriteLine(string module, string format, params object[] arg)
        {
            DebugWriteLineEvent?.Invoke("DEBUG (" + module + "): " + format, arg);
            DebugWithModuleWriteLineEvent?.Invoke(module, format, arg);
        }

        public static void WriteLine() => WriteLineEvent?.Invoke("", null);

        public static void ErrorWriteLine() => ErrorWriteLineEvent?.Invoke("", null);

        public static void VerboseWriteLine() => VerboseWriteLineEvent?.Invoke("", null);

        public static void DebugWriteLine() => DebugWriteLineEvent?.Invoke("", null);

        public static void Write(string format, params object[] arg) => WriteEvent?.Invoke(format, arg);

        public static void ErrorWrite(string format, params object[] arg) => ErrorWriteEvent?.Invoke(format, arg);

        public static void VerboseWrite(string format, params object[] arg) => VerboseWriteEvent?.Invoke(format, arg);

        public static void DebugWrite(string module, string format, params object[] arg) =>
            DebugWriteEvent?.Invoke("DEBUG (" + module + "): " + format, arg);

        public static void Write() => WriteEvent?.Invoke("", null);

        public static void ErrorWrite() => ErrorWriteEvent?.Invoke("", null);

        public static void VerboseWrite() => VerboseWriteEvent?.Invoke("", null);

        public static void DebugWrite() => DebugWriteEvent?.Invoke("", null);

        public static void WriteLine(string format) => WriteLineEvent?.Invoke("{0}", format);

        public static void ErrorWriteLine(string format) => ErrorWriteLineEvent?.Invoke("{0}", format);

        public static void VerboseWriteLine(string format) => VerboseWriteLineEvent?.Invoke("{0}", format);

        public static void DebugWriteLine(string module, string format) =>
            DebugWriteLineEvent?.Invoke("{0}", "DEBUG (" + module + "): " + format);
    }
}