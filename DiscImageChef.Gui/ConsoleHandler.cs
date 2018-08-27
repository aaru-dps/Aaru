// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleHandler.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI console.
//
// --[ Description ] ----------------------------------------------------------
//
//     Receives DicConsole events and stores them for showing in the console
//     window.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.ObjectModel;
using DiscImageChef.Console;

namespace DiscImageChef.Gui
{
    static class ConsoleHandler
    {
        static bool _debug;
        static bool _verbose;

        public static bool Debug
        {
            get => _debug;
            set
            {
                if(_debug == value) return;

                _debug = value;

                if(_debug) DicConsole.DebugWithModuleWriteLineEvent += OnDebugWriteHandler;
                else DicConsole.DebugWithModuleWriteLineEvent       -= OnDebugWriteHandler;
            }
        }
        public static bool Verbose
        {
            get => _verbose;
            set
            {
                if(_verbose == value) return;

                _verbose = value;

                if(_verbose) DicConsole.VerboseWriteLineEvent += OnVerboseWriteHandler;
                else DicConsole.VerboseWriteLineEvent         -= OnVerboseWriteHandler;
            }
        }

        public static ObservableCollection<LogEntry> Entries { get; } = new ObservableCollection<LogEntry>();

        internal static void Init()
        {
            DicConsole.WriteLineEvent      += OnWriteHandler;
            DicConsole.ErrorWriteLineEvent += OnErrorWriteHandler;
        }

        static void OnWriteHandler(string format, params object[] arg)
        {
            Entries.Add(new LogEntry
            {
                Message   = string.Format(format, arg),
                Module    = null,
                Timestamp = DateTime.Now,
                Type      = "Info"
            });
        }

        static void OnErrorWriteHandler(string format, params object[] arg)
        {
            Entries.Add(new LogEntry
            {
                Message   = string.Format(format, arg),
                Module    = null,
                Timestamp = DateTime.Now,
                Type      = "Error"
            });
        }

        static void OnVerboseWriteHandler(string format, params object[] arg)
        {
            Entries.Add(new LogEntry
            {
                Message   = string.Format(format, arg),
                Module    = null,
                Timestamp = DateTime.Now,
                Type      = "Verbose"
            });
        }

        static void OnDebugWriteHandler(string module, string format, params object[] arg)
        {
            Entries.Add(new LogEntry
            {
                Message   = string.Format(format, arg),
                Module    = module,
                Timestamp = DateTime.Now,
                Type      = "Debug"
            });
        }
    }

    class LogEntry
    {
        public string   Message   { get; set; }
        public string   Module    { get; set; }
        public DateTime Timestamp { get; set; }
        public string   Type      { get; set; }
    }
}