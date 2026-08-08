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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.Interop;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Console = Aaru.Gui.Views.Dialogs.Console;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;
using Version = Aaru.CommonTypes.Interop.Version;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed class ConsoleViewModel : ViewModelBase
{
    readonly Console _view;

    public ConsoleViewModel(Console view)
    {
        _view        = view;
        SaveCommand  = new AsyncRelayCommand(SaveAsync);
        ClearCommand = new RelayCommand(Clear);
    }

    public ICommand                       ClearCommand { get; }
    public ICommand                       SaveCommand  { get; }
    public ObservableCollection<LogEntry> Entries      => ConsoleHandler.Entries;

    public bool DebugChecked
    {
        get;
        set
        {
            ConsoleHandler.Debug = value;
            SetProperty(ref field, value);
        }
    }

    async Task SaveAsync()
    {
        IStorageFile result = await _view.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [FilePickerFileTypes.Log]
        });

        if(result is null) return;

        try
        {
            var logFs = new FileStream(result.Path.LocalPath, FileMode.Create, FileAccess.ReadWrite);
            var logSw = new StreamWriter(logFs);

            await logSw.WriteLineAsync(string.Format(UI.Log_saved_at_0, DateTime.Now));

            PlatformID platId  = DetectOS.GetRealPlatformID();
            string     platVer = DetectOS.GetVersion();

            var assemblyVersion =
                Attribute.GetCustomAttribute(typeof(AaruLogging).Assembly,
                                             typeof(AssemblyInformationalVersionAttribute)) as
                    AssemblyInformationalVersionAttribute;

            await logSw.WriteLineAsync(Aaru.Localization.Core.System_information);

            await
                logSw.WriteLineAsync($"{DetectOS.GetPlatformName(platId, platVer)} {platVer} ({(Environment.Is64BitOperatingSystem ? 64 : 32)}-bit)");

            await logSw.WriteLineAsync($".NET Core {Version.GetNetCoreVersion()}");

            await logSw.WriteLineAsync();

            await logSw.WriteLineAsync(Aaru.Localization.Core.Program_information);
            await logSw.WriteLineAsync($"Aaru {assemblyVersion?.InformationalVersion}");

            await logSw.WriteLineAsync(string.Format(Aaru.Localization.Core.Running_in_0_bit,
                                                     Environment.Is64BitProcess ? 64 : 32));
#if DEBUG
            await logSw.WriteLineAsync(Aaru.Localization.Core.DEBUG_version);
#endif
            await logSw.WriteLineAsync(string.Format(Aaru.Localization.Core.Command_line_0, Environment.CommandLine));
            await logSw.WriteLineAsync();

            await logSw.WriteLineAsync(UI.Console_with_ornament);

            foreach(LogEntry entry in ConsoleHandler.Entries)
            {
                if(entry.Type != UI.LogEntry_Type_Info)
                    await logSw.WriteLineAsync($"{entry.Timestamp}: ({entry.Type.ToLower()}) {entry.Message}");
                else
                    await logSw.WriteLineAsync($"{entry.Timestamp}: {entry.Message}");
            }

            logSw.Close();
            logFs.Close();
        }
        catch(Exception exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                          string
                                                             .Format(UI
                                                                        .Exception_0_trying_to_save_logfile_details_has_been_sent_to_console,
                                                                     exception.Message),
                                                          ButtonEnum.Ok,
                                                          Icon.Error)
                                   .ShowWindowDialogAsync(_view);

            AaruLogging.Exception(exception,
                                  UI.Exception_0_trying_to_save_logfile_details_has_been_sent_to_console,
                                  exception.Message);
        }
    }

    static void Clear() => ConsoleHandler.Entries.Clear();
}