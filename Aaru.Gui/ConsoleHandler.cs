// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleHandler.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru GUI.
//
// --[ Description ] ----------------------------------------------------------
//
//     Receives AaruConsole events and stores them for showing in the console
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using Aaru.Console;
using Aaru.Localization;
using JetBrains.Annotations;

namespace Aaru.Gui;

static class ConsoleHandler
{
    static bool _debug;
    static bool _verbose;

    public static bool Debug
    {
        set
        {
            if(_debug == value) return;

            _debug = value;

            if(_debug)
            {
                AaruConsole.DebugWithModuleWriteLineEvent += OnDebugWriteHandler;
                AaruConsole.WriteExceptionEvent           += OnWriteExceptionEvent;
            }
            else
            {
                AaruConsole.DebugWithModuleWriteLineEvent -= OnDebugWriteHandler;
                AaruConsole.WriteExceptionEvent           -= OnWriteExceptionEvent;
            }
        }
    }

    public static ObservableCollection<LogEntry> Entries { get; } = [];

    static void OnWriteExceptionEvent([NotNull] Exception ex) => Entries.Add(new LogEntry
    {
        Message   = ex.ToString(),
        Module    = null,
        Timestamp = DateTime.Now,
        Type      = UI.LogEntry_Type_Exception
    });

    internal static void Init()
    {
        AaruConsole.WriteLineEvent      += OnWriteHandler;
        AaruConsole.ErrorWriteLineEvent += OnErrorWriteHandler;
    }

    static void OnWriteHandler([CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        Entries.Add(new LogEntry
        {
            Message   = string.Format(format, arg),
            Module    = null,
            Timestamp = DateTime.Now,
            Type      = UI.LogEntry_Type_Info
        });
    }

    static void OnErrorWriteHandler([CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        Entries.Add(new LogEntry
        {
            Message   = string.Format(format, arg),
            Module    = null,
            Timestamp = DateTime.Now,
            Type      = UI.LogEntry_Type_Error
        });
    }

    static void OnDebugWriteHandler(string module, [CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        Entries.Add(new LogEntry
        {
            Message   = string.Format(format, arg),
            Module    = module,
            Timestamp = DateTime.Now,
            Type      = UI.LogEntry_Type_Debug
        });
    }
}

public sealed class LogEntry
{
    public string   Message   { get; set; }
    public string   Module    { get; set; }
    public DateTime Timestamp { get; set; }
    public string   Type      { get; set; }
}