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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Console
{
    /// <summary>
    ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
    ///     the standard output console using the specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void WriteLineHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
    ///     the error output console using the specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void ErrorWriteLineHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
    ///     the verbose output console using the specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void VerboseWriteLineHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
    ///     the debug output console using the specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void DebugWriteLineHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, to the standard output console using the
    ///     specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void WriteHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, to the error output console using the
    ///     specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void ErrorWriteHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, to the verbose output console using the
    ///     specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void VerboseWriteHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, to the debug output console using the
    ///     specified format information.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void DebugWriteHandler(string format, params object[] arg);

    /// <summary>
    ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
    ///     the debug output console using the specified format information.
    /// </summary>
    /// <param name="module">Description of the module writing to the debug console</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
    public delegate void DebugWithModuleWriteLineHandler(string module, string format, params object[] arg);

    /// <summary>
    ///     Implements a console abstraction that defines four level of messages that can be routed to different consoles:
    ///     standard, error, verbose and debug.
    /// </summary>
    public static class AaruConsole
    {
        /// <summary>Event to receive writings to the standard output console that should be followed by a line termination.</summary>
        public static event WriteLineHandler WriteLineEvent;
        /// <summary>Event to receive writings to the error output console that should be followed by a line termination.</summary>
        public static event ErrorWriteLineHandler ErrorWriteLineEvent;
        /// <summary>Event to receive writings to the verbose output console that should be followed by a line termination.</summary>
        public static event VerboseWriteLineHandler VerboseWriteLineEvent;
        /// <summary>Event to receive line terminations to the debug output console.</summary>
        public static event DebugWriteLineHandler DebugWriteLineEvent;
        /// <summary>Event to receive writings to the debug output console that should be followed by a line termination.</summary>
        public static event DebugWithModuleWriteLineHandler DebugWithModuleWriteLineEvent;
        /// <summary>Event to receive writings to the standard output console.</summary>
        public static event WriteHandler WriteEvent;
        /// <summary>Event to receive writings to the error output console.</summary>
        public static event ErrorWriteHandler ErrorWriteEvent;
        /// <summary>Event to receive writings to the verbose output console.</summary>
        public static event VerboseWriteHandler VerboseWriteEvent;
        /// <summary>Event to receive writings to the debug output console.</summary>
        public static event DebugWriteHandler DebugWriteEvent;

        /// <summary>
        ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
        ///     the standard output console using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void WriteLine(string format, params object[] arg) => WriteLineEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
        ///     the error output console using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void ErrorWriteLine(string format, params object[] arg) =>
            ErrorWriteLineEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
        ///     the verbose output console using the specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void VerboseWriteLine(string format, params object[] arg) =>
            VerboseWriteLineEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects, followed by the current line terminator, to
        ///     the debug output console using the specified format information.
        /// </summary>
        /// <param name="module">Description of the module writing to the debug console</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void DebugWriteLine(string module, string format, params object[] arg)
        {
            DebugWriteLineEvent?.Invoke("DEBUG (" + module + "): " + format, arg);
            DebugWithModuleWriteLineEvent?.Invoke(module, format, arg);
        }

        /// <summary>Writes the current line terminator to the standard output console.</summary>
        public static void WriteLine() => WriteLineEvent?.Invoke("", null);

        /// <summary>Writes the current line terminator to the error output console.</summary>
        public static void ErrorWriteLine() => ErrorWriteLineEvent?.Invoke("", null);

        /// <summary>Writes the current line terminator to the verbose output console.</summary>
        public static void VerboseWriteLine() => VerboseWriteLineEvent?.Invoke("", null);

        /// <summary>Writes the current line terminator to the debug output console.</summary>
        public static void DebugWriteLine() => DebugWriteLineEvent?.Invoke("", null);

        /// <summary>
        ///     Writes the text representation of the specified array of objects to the standard output console using the
        ///     specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void Write(string format, params object[] arg) => WriteEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects to the error output console using the
        ///     specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void ErrorWrite(string format, params object[] arg) => ErrorWriteEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects to the verbose output console using the
        ///     specified format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void VerboseWrite(string format, params object[] arg) => VerboseWriteEvent?.Invoke(format, arg);

        /// <summary>
        ///     Writes the text representation of the specified array of objects to the debug output console using the
        ///     specified format information.
        /// </summary>
        /// <param name="module">Description of the module writing to the debug console</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using <paramref name="format" />.</param>
        public static void DebugWrite(string module, string format, params object[] arg) =>
            DebugWriteEvent?.Invoke("DEBUG (" + module + "): " + format, arg);

        /// <summary>Writes the specified string value, followed by the current line terminator, to the standard output console.</summary>
        /// <param name="value">The value to write.</param>
        public static void WriteLine(string value) => WriteLineEvent?.Invoke("{0}", value);

        /// <summary>Writes the specified string value, followed by the current line terminator, to the error output console.</summary>
        /// <param name="value">The value to write.</param>
        public static void ErrorWriteLine(string value) => ErrorWriteLineEvent?.Invoke("{0}", value);

        /// <summary>Writes the specified string value, followed by the current line terminator, to the verbose output console.</summary>
        /// <param name="value">The value to write.</param>
        public static void VerboseWriteLine(string value) => VerboseWriteLineEvent?.Invoke("{0}", value);

        /// <summary>Writes the specified string value, followed by the current line terminator, to the debug output console.</summary>
        /// <param name="module">Description of the module writing to the debug console</param>
        /// <param name="value">The value to write.</param>
        public static void DebugWriteLine(string module, string value) =>
            DebugWriteLineEvent?.Invoke("{0}", "DEBUG (" + module + "): " + value);
    }
}