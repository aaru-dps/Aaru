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
//     Receives AaruLogging events and stores them for showing in the console
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Threading;
using JetBrains.Annotations;

namespace Aaru.Gui;

static class ConsoleHandler
{
    public static bool Debug
    {
        set
        {
            if(field == value) return;

            field = value;

            if(field)
            {
                AaruLogging.DebugEvent          += OnDebugWriteHandler;
                AaruLogging.WriteExceptionEvent += OnWriteExceptionEvent;
            }
            else
            {
                AaruLogging.DebugEvent          -= OnDebugWriteHandler;
                AaruLogging.WriteExceptionEvent -= OnWriteExceptionEvent;
            }
        }
    }

    public static ObservableCollection<LogEntry> Entries { get; } = [];

    static void AddEntry(string message, string module, string type)
    {
        var entry = new LogEntry
        {
            Message   = message,
            Module    = module,
            Timestamp = DateTime.Now,
            Type      = type
        };

        // Log events can come from any worker thread, but Entries is bound to the console window
        Dispatcher.UIThread.Post(() => Entries.Add(entry));
    }

    static void OnWriteExceptionEvent([NotNull] Exception ex, string message, params object[] objects) =>
        AddEntry(SafeFormat(message, objects), null, UI.LogEntry_Type_Exception);

    internal static void Init()
    {
        AaruLogging.WriteLineEvent += OnWriteHandler;
        AaruLogging.ErrorEvent     += OnErrorWriteHandler;
    }

    static string SafeFormat([NotNull] string format, [NotNull] object[] arg)
    {
        if(arg.Length == 0) return format;

        try
        {
            return string.Format(format, arg);
        }
        catch(FormatException)
        {
            return format;
        }
    }

    static void OnWriteHandler([CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        AddEntry(SafeFormat(format, arg), null, UI.LogEntry_Type_Info);
    }

    static void OnErrorWriteHandler([CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        AddEntry(SafeFormat(format, arg), null, UI.LogEntry_Type_Error);
    }

    static void OnDebugWriteHandler(string module, [CanBeNull] string format, [CanBeNull] params object[] arg)
    {
        if(format == null || arg == null) return;

        AddEntry(SafeFormat(format, arg), module, UI.LogEntry_Type_Debug);
    }
}

public sealed class LogEntry
{
    public string   Message   { get; set; }
    public string   Module    { get; set; }
    public DateTime Timestamp { get; set; }
    public string   Type      { get; set; }
}