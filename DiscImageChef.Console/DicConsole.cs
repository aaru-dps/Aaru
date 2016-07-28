// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DicConsole.cs
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.Console
{
    public delegate void WriteLineHandler(string format, params object[] arg);
    public delegate void ErrorWriteLineHandler(string format, params object[] arg);
    public delegate void VerboseWriteLineHandler(string format, params object[] arg);
    public delegate void DebugWriteLineHandler(string format, params object[] arg);

    public delegate void WriteHandler(string format, params object[] arg);
    public delegate void ErrorWriteHandler(string format, params object[] arg);
    public delegate void VerboseWriteHandler(string format, params object[] arg);
    public delegate void DebugWriteHandler(string format, params object[] arg);

    public static class DicConsole
    {
        public static event WriteLineHandler WriteLineEvent;
        public static event ErrorWriteLineHandler ErrorWriteLineEvent;
        public static event VerboseWriteLineHandler VerboseWriteLineEvent;
        public static event DebugWriteLineHandler DebugWriteLineEvent;

        public static event WriteHandler WriteEvent;
        public static event ErrorWriteHandler ErrorWriteEvent;
        public static event VerboseWriteHandler VerboseWriteEvent;
        public static event DebugWriteHandler DebugWriteEvent;

        public static void WriteLine(string format, params object[] arg)
        {
            if(WriteLineEvent != null)
                WriteLineEvent(format, arg);
        }

        public static void ErrorWriteLine(string format, params object[] arg)
        {
            if(ErrorWriteLineEvent != null)
                ErrorWriteLineEvent(format, arg);
        }

        public static void VerboseWriteLine(string format, params object[] arg)
        {
            if(VerboseWriteLineEvent != null)
                VerboseWriteLineEvent(format, arg);
        }

        public static void DebugWriteLine(string module, string format, params object[] arg)
        {
            if(DebugWriteLineEvent != null)
                DebugWriteLineEvent("DEBUG (" + module + "): " + format, arg);
        }

        public static void WriteLine()
        {
            if(WriteLineEvent != null)
                WriteLineEvent("", null);
        }

        public static void ErrorWriteLine()
        {
            if(ErrorWriteLineEvent != null)
                ErrorWriteLineEvent("", null);
        }

        public static void VerboseWriteLine()
        {
            if(VerboseWriteLineEvent != null)
                VerboseWriteLineEvent("", null);
        }

        public static void DebugWriteLine()
        {
            if(DebugWriteLineEvent != null)
                DebugWriteLineEvent("", null);
        }

        public static void Write(string format, params object[] arg)
        {
            if(WriteEvent != null)
                WriteEvent(format, arg);
        }

        public static void ErrorWrite(string format, params object[] arg)
        {
            if(ErrorWriteEvent != null)
                ErrorWriteEvent(format, arg);
        }

        public static void VerboseWrite(string format, params object[] arg)
        {
            if(VerboseWriteEvent != null)
                VerboseWriteEvent(format, arg);
        }

        public static void DebugWrite(string module, string format, params object[] arg)
        {
            if(DebugWriteEvent != null)
                DebugWriteEvent("DEBUG (" + module + "): " + format, arg);
        }

        public static void Write()
        {
            if(WriteEvent != null)
                WriteEvent("", null);
        }

        public static void ErrorWrite()
        {
            if(ErrorWriteEvent != null)
                ErrorWriteEvent("", null);
        }

        public static void VerboseWrite()
        {
            if(VerboseWriteEvent != null)
                VerboseWriteEvent("", null);
        }

        public static void DebugWrite()
        {
            if(DebugWriteEvent != null)
                DebugWriteEvent("", null);
        }

        public static void WriteLine(string format)
        {
            if(WriteLineEvent != null)
                WriteLineEvent("{0}", format);
        }

        public static void ErrorWriteLine(string format)
        {
            if(ErrorWriteLineEvent != null)
                ErrorWriteLineEvent("{0}", format);
        }

        public static void VerboseWriteLine(string format)
        {
            if(VerboseWriteLineEvent != null)
                VerboseWriteLineEvent("{0}", format);
        }

        public static void DebugWriteLine(string module, string format)
        {
            if(DebugWriteLineEvent != null)
                DebugWriteLineEvent("{0}", "DEBUG (" + module + "): " + format);
        }

    }
}

