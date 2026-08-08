// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArchiveSubdirectoryViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the archive subdirectory contents panel.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Helpers;
using Aaru.Localization;
using Aaru.Logging;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sentry;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class ArchiveSubdirectoryViewModel
{
    const    string                   MODULE_NAME = "Archive Subdirectory ViewModel";
    readonly ArchiveSubdirectoryModel _model;
    readonly Window                   _view;

    public ArchiveSubdirectoryViewModel(ArchiveSubdirectoryModel model, Window view)
    {
        Entries             = [];
        SelectedEntries     = [];
        ExtractFilesCommand = new AsyncRelayCommand(ExtractFiles);
        _model              = model;
        _view               = view;

        foreach(ArchiveFileModel entry in model.Entries)
        {
            entry.Color =
                new SolidColorBrush(Color.Parse(DirColorsParser.Instance.ExtensionColors
                                                               .TryGetValue(Path.GetExtension(entry.Name),
                                                                            out string hex)
                                                    ? hex
                                                    : DirColorsParser.Instance.NormalColor));

            Entries.Add(entry);
        }
    }

    public ObservableCollection<ArchiveFileModel> Entries             { get; }
    public List<ArchiveFileModel>                 SelectedEntries     { get; }
    public ICommand                               ExtractFilesCommand { get; }

    async Task ExtractFiles()
    {
        if(SelectedEntries.Count == 0) return;

        IReadOnlyList<IStorageFolder> result =
            await _view.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title         = UI.Dialog_Choose_destination_folder,
                AllowMultiple = false
            });

        if(result.Count != 1) return;

        Statistics.AddCommand("extract-files");

        string folder = result[0].Path.LocalPath;

        foreach(ArchiveFileModel file in SelectedEntries)
        {
            string filename = file.Name;

            ButtonResult mboxResult;

            if(DetectOS.IsWindows)
            {
                if(filename.Contains('<')                                               ||
                   filename.Contains('>')                                               ||
                   filename.Contains(':')                                               ||
                   filename.Contains('\\')                                              ||
                   filename.Contains('/')                                               ||
                   filename.Contains('|')                                               ||
                   filename.Contains('?')                                               ||
                   filename.Contains('*')                                               ||
                   filename.Any(static c => c < 32)                                     ||
                   filename.Equals("CON",  StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("PRN",  StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("AUX",  StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM1", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM2", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM3", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM4", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM5", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM6", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM7", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM8", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("COM9", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT1", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT2", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT3", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT4", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT5", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT6", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT7", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT8", StringComparison.InvariantCultureIgnoreCase) ||
                   filename.Equals("LPT9", StringComparison.InvariantCultureIgnoreCase) ||
                   filename[^1] == '.'                                                  ||
                   filename[^1] == ' ')
                {
                    char[] chars;

                    if(filename[^1] == '.' || filename[^1] == ' ')
                        chars = new char[filename.Length - 1];
                    else
                        chars = new char[filename.Length];

                    for(var ci = 0; ci < chars.Length; ci++)
                    {
                        chars[ci] = filename[ci] switch
                                    {
                                        '<'
                                         or '>'
                                         or ':'
                                         or '\\'
                                         or '/'
                                         or '|'
                                         or '?'
                                         or '*'
                                         or >= '\u0000' and <= '\u001F' => '_',
                                        _ => filename[ci]
                                    };
                    }

                    if(filename.StartsWith("CON", StringComparison.InvariantCultureIgnoreCase) ||
                       filename.StartsWith("PRN", StringComparison.InvariantCultureIgnoreCase) ||
                       filename.StartsWith("AUX", StringComparison.InvariantCultureIgnoreCase) ||
                       filename.StartsWith("COM", StringComparison.InvariantCultureIgnoreCase) ||
                       filename.StartsWith("LPT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        chars[0] = '_';
                        chars[1] = '_';
                        chars[2] = '_';
                    }

                    string corrected = new(chars);

                    mboxResult = await MessageBoxManager.GetMessageBoxStandard(UI.Unsupported_filename,
                                                                               string
                                                                                  .Format(UI
                                                                                          .Filename_0_not_supported_want_to_rename_to_1,
                                                                                       filename,
                                                                                       corrected),
                                                                               ButtonEnum.YesNoCancel,
                                                                               Icon.Warning)
                                                        .ShowWindowDialogAsync(_view);

                    switch(mboxResult)
                    {
                        case ButtonResult.Cancel:
                            return;
                        case ButtonResult.No:
                            continue;
                        default:
                            filename = corrected;

                            break;
                    }
                }
            }

            string outputPath = Path.Combine(folder, filename);

            if(File.Exists(outputPath))
            {
                mboxResult = await MessageBoxManager.GetMessageBoxStandard(UI.Existing_file,
                                                                           string
                                                                              .Format(UI.File_named_0_exists_overwrite_Q,
                                                                                   filename),
                                                                           ButtonEnum.YesNoCancel,
                                                                           Icon.Warning)
                                                    .ShowWindowDialogAsync(_view);

                switch(mboxResult)
                {
                    case ButtonResult.Cancel:
                        return;
                    case ButtonResult.No:
                        continue;
                    default:
                        try
                        {
                            File.Delete(outputPath);
                        }
                        catch(IOException)
                        {
                            mboxResult = await MessageBoxManager.GetMessageBoxStandard(UI.Cannot_delete,
                                                                     UI.Could_note_delete_existe_file_continue_Q,
                                                                     ButtonEnum.YesNo,
                                                                     Icon.Error)
                                                                .ShowWindowDialogAsync(_view);

                            if(mboxResult == ButtonResult.No) return;
                        }

                        break;
                }
            }

            try
            {
                ErrorNumber error = _model.Archive.GetEntry(file.EntryNumber, out IFilter entryFilter);

                if(error != ErrorNumber.NoError || entryFilter is null)
                {
                    AaruLogging.Debug(MODULE_NAME, string.Format(UI.Skipped_non_regular_archive_entry_0, file.Name));

                    continue;
                }

                using(Stream inStream = entryFilter.GetDataForkStream())
                {
                    await using(var fs = new FileStream(outputPath,
                                                        FileMode.CreateNew,
                                                        FileAccess.ReadWrite,
                                                        FileShare.None))
                    {
                        await inStream.CopyToAsync(fs);
                    }
                }

                var fi = new FileInfo(outputPath);

                try
                {
                    if(file.Stat?.CreationTimeUtc is not null) fi.CreationTimeUtc = file.Stat.CreationTimeUtc.Value;
                }
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }

                try
                {
                    if(file.Stat?.LastWriteTimeUtc is not null) fi.LastWriteTimeUtc = file.Stat.LastWriteTimeUtc.Value;
                }
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }

                try
                {
                    if(file.Stat?.AccessTimeUtc is not null) fi.LastAccessTimeUtc = file.Stat.AccessTimeUtc.Value;
                }
                catch(Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                }
            }
            catch(IOException)
            {
                mboxResult = await MessageBoxManager.GetMessageBoxStandard(UI.Cannot_create_file,
                                                                           UI
                                                                              .Could_not_create_destination_file_continue_Q,
                                                                           ButtonEnum.YesNo,
                                                                           Icon.Error)
                                                    .ShowWindowDialogAsync(_view);

                if(mboxResult == ButtonResult.No) return;
            }
        }
    }
}