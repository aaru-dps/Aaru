// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ConsoleViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the console dialog.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Aaru.CommonTypes.Interop;
using Aaru.Console;
using Aaru.Localization;
using Avalonia.Controls;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed class ConsoleViewModel : ViewModelBase
{
    readonly Views.Dialogs.Console _view;
    bool                           _debugChecked;

    public ConsoleViewModel(Views.Dialogs.Console view)
    {
        _view        = view;
        SaveCommand  = ReactiveCommand.Create(ExecuteSaveCommand);
        ClearCommand = ReactiveCommand.Create(ExecuteClearCommand);
    }

    [NotNull]
    public string Title => UI.Title_Console;

    public ReactiveCommand<Unit, Unit>    ClearCommand { get; }
    public ReactiveCommand<Unit, Task>    SaveCommand  { get; }
    public ObservableCollection<LogEntry> Entries      => ConsoleHandler.Entries;

    [NotNull]
    public string DebugText => UI.Enable_debug_console;

    [NotNull]
    public string SaveLabel => UI.ButtonLabel_Save;

    [NotNull]
    public string ClearLabel => UI.ButtonLabel_Clear;

    public string TimeLabel    => UI.Title_Time;
    public string TypeLabel    => UI.Title_Type;
    public string ModuleLabel  => UI.Title_Module;
    public string MessageLabel => UI.Title_Message;

    public bool DebugChecked
    {
        get => _debugChecked;
        set
        {
            ConsoleHandler.Debug = value;
            this.RaiseAndSetIfChanged(ref _debugChecked, value);
        }
    }

    async Task ExecuteSaveCommand()
    {
        var dlgSave = new SaveFileDialog();

        dlgSave.Filters?.Add(new FileDialogFilter
        {
            Extensions = new List<string>(new[]
            {
                "log"
            }),
            Name = UI.Dialog_Log_files
        });

        string result = await dlgSave.ShowAsync(_view);

        if(result is null)
            return;

        try
        {
            var logFs = new FileStream(result, FileMode.Create, FileAccess.ReadWrite);
            var logSw = new StreamWriter(logFs);

            logSw.WriteLine(UI.Log_saved_at_0, DateTime.Now);

            PlatformID platId  = DetectOS.GetRealPlatformID();
            string     platVer = DetectOS.GetVersion();

            var assemblyVersion =
                Attribute.GetCustomAttribute(typeof(AaruConsole).Assembly,
                                             typeof(AssemblyInformationalVersionAttribute)) as
                    AssemblyInformationalVersionAttribute;

            logSw.WriteLine(Localization.Core.System_information);

            logSw.WriteLine("{0} {1} ({2}-bit)", DetectOS.GetPlatformName(platId, platVer), platVer,
                            Environment.Is64BitOperatingSystem ? 64 : 32);

            logSw.WriteLine(".NET Core {0}", Version.GetNetCoreVersion());

            logSw.WriteLine();

            logSw.WriteLine(Localization.Core.Program_information);
            logSw.WriteLine("Aaru {0}",                         assemblyVersion?.InformationalVersion);
            logSw.WriteLine(Localization.Core.Running_in_0_bit, Environment.Is64BitProcess ? 64 : 32);
        #if DEBUG
            logSw.WriteLine(Localization.Core.DEBUG_version);
        #endif
            logSw.WriteLine(Localization.Core.Command_line_0, Environment.CommandLine);
            logSw.WriteLine();

            logSw.WriteLine(UI.Console_with_ornament);

            foreach(LogEntry entry in ConsoleHandler.Entries)
            {
                if(entry.Type != UI.LogEntry_Type_Info)
                    logSw.WriteLine("{0}: ({1}) {2}", entry.Timestamp, entry.Type.ToLower(), entry.Message);
                else
                    logSw.WriteLine("{0}: {1}", entry.Timestamp, entry.Message);
            }

            logSw.Close();
            logFs.Close();
        }
        catch(Exception exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                          string.
                                                              Format(UI.Exception_0_trying_to_save_logfile_details_has_been_sent_to_console,
                                                                     exception.Message), ButtonEnum.Ok, Icon.Error).
                                    ShowWindowDialogAsync(_view);

            AaruConsole.ErrorWriteLine("Console", exception.Message);
            AaruConsole.ErrorWriteLine("Console", exception.StackTrace);
        }
    }

    static void ExecuteClearCommand() => ConsoleHandler.Entries.Clear();
}