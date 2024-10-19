// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SubdirectoryViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the subdirectory contents panel.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Aaru.Localization;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using JetBrains.Annotations;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class SubdirectoryViewModel
{
    readonly SubdirectoryModel _model;
    readonly Window            _view;

    public SubdirectoryViewModel([NotNull] SubdirectoryModel model, Window view)
    {
        Entries             = [];
        SelectedEntries     = [];
        ExtractFilesCommand = ReactiveCommand.Create(ExecuteExtractFilesCommand);
        _model              = model;
        _view               = view;

        ErrorNumber errno = model.Plugin.OpenDir(model.Path, out IDirNode node);

        if(errno != ErrorNumber.NoError)
        {
            MessageBoxManager.GetMessageBoxStandard(UI.Title_Error,
                                                    string.Format(UI.Error_0_trying_to_read_1_of_chosen_filesystem,
                                                                  errno,
                                                                  model.Path),
                                                    ButtonEnum.Ok,
                                                    Icon.Error)
                             .ShowWindowDialogAsync(view);

            return;
        }

        while(model.Plugin.ReadDir(node, out string dirent) == ErrorNumber.NoError && dirent is not null)
        {
            errno = model.Plugin.Stat(model.Path + "/" + dirent, out FileEntryInfo stat);

            if(errno != ErrorNumber.NoError)
            {
                AaruConsole
                   .ErrorWriteLine(string.Format(UI.Error_0_trying_to_get_information_about_filesystem_entry_named_1,
                                                 errno,
                                                 dirent));

                continue;
            }

            if(stat.Attributes.HasFlag(FileAttributes.Directory) && !model.Listed)
            {
                model.Subdirectories.Add(new SubdirectoryModel
                {
                    Name   = dirent,
                    Path   = model.Path + "/" + dirent,
                    Plugin = model.Plugin
                });

                continue;
            }

            Entries.Add(new FileModel
            {
                Name = dirent,
                Stat = stat
            });
        }

        model.Plugin.CloseDir(node);
    }

    public ObservableCollection<FileModel> Entries             { get; }
    public List<FileModel>                 SelectedEntries     { get; }
    public ReactiveCommand<Unit, Task>     ExtractFilesCommand { get; }

    public string ExtractFilesLabel => UI.ButtonLabel_Extract_to;
    public string NameLabel         => UI.Title_Name;
    public string LengthLabel       => UI.Title_Length;
    public string CreationLabel     => UI.Title_Creation;
    public string LastAccessLabel   => UI.Title_Last_access;
    public string ChangedLabel      => UI.Title_Changed;
    public string LastBackupLabel   => UI.Title_Last_backup;
    public string LastWriteLabel    => UI.Title_Last_write;
    public string AttributesLabel   => UI.Title_Attributes;
    public string GIDLabel          => UI.Title_GID;
    public string UIDLabel          => UI.Title_UID;
    public string InodeLabel        => UI.Title_Inode;
    public string LinksLabel        => UI.Title_Links;
    public string ModeLabel         => UI.Title_Mode;

    async Task ExecuteExtractFilesCommand()
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

        string folder = result[0].Path.AbsolutePath;

        foreach(FileModel file in SelectedEntries)
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
                   filename.Any(c => c < 32)                                            ||
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
                   filename.Last() == '.'                                               ||
                   filename.Last() == ' ')
                {
                    char[] chars;

                    if(filename.Last() == '.' || filename.Last() == ' ')
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
                                         or '\u0000'
                                         or '\u0001'
                                         or '\u0002'
                                         or '\u0003'
                                         or '\u0004'
                                         or '\u0005'
                                         or '\u0006'
                                         or '\u0007'
                                         or '\u0008'
                                         or '\u0009'
                                         or '\u000A'
                                         or '\u000B'
                                         or '\u000C'
                                         or '\u000D'
                                         or '\u000E'
                                         or '\u000F'
                                         or '\u0010'
                                         or '\u0011'
                                         or '\u0012'
                                         or '\u0013'
                                         or '\u0014'
                                         or '\u0015'
                                         or '\u0016'
                                         or '\u0017'
                                         or '\u0018'
                                         or '\u0019'
                                         or '\u001A'
                                         or '\u001B'
                                         or '\u001C'
                                         or '\u001D'
                                         or '\u001E'
                                         or '\u001F' => '_',
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
                var outBuf = new byte[file.Stat.Length];

                ErrorNumber error = _model.Plugin.OpenFile(_model.Path + "/" + file.Name, out IFileNode fileNode);

                if(error == ErrorNumber.NoError)
                {
                    error = _model.Plugin.ReadFile(fileNode, file.Stat.Length, outBuf, out _);
                    _model.Plugin.CloseFile(fileNode);
                }

                if(error != ErrorNumber.NoError)
                {
                    mboxResult = await MessageBoxManager.GetMessageBoxStandard(UI.Error_reading_file,
                                                                               string
                                                                                  .Format(UI
                                                                                          .Error_0_reading_file_continue_Q,
                                                                                       error),
                                                                               ButtonEnum.YesNo,
                                                                               Icon.Error)
                                                        .ShowWindowDialogAsync(_view);

                    if(mboxResult == ButtonResult.No) return;

                    continue;
                }

                var fs = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                fs.Write(outBuf, 0, outBuf.Length);
                fs.Close();
                var fi = new FileInfo(outputPath);
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                try
                {
                    if(file.Stat.CreationTimeUtc.HasValue) fi.CreationTimeUtc = file.Stat.CreationTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if(file.Stat.LastWriteTimeUtc.HasValue) fi.LastWriteTimeUtc = file.Stat.LastWriteTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if(file.Stat.AccessTimeUtc.HasValue) fi.LastAccessTimeUtc = file.Stat.AccessTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
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