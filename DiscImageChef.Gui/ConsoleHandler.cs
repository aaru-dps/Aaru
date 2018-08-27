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