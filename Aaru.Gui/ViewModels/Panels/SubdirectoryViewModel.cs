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
// Copyright © 2011-2023 Natalia Portillo
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
using JetBrains.Annotations;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Gui.ViewModels.Panels;

public sealed class SubdirectoryViewModel
{
    readonly SubdirectoryModel _model;
    readonly Window            _view;

    public SubdirectoryViewModel([NotNull] SubdirectoryModel model, Window view)
    {
        Entries             = new ObservableCollection<FileModel>();
        SelectedEntries     = new List<FileModel>();
        ExtractFilesCommand = ReactiveCommand.Create(ExecuteExtractFilesCommand);
        _model              = model;
        _view               = view;

        ErrorNumber errno = model.Plugin.OpenDir(model.Path, out IDirNode node);

        if(errno != ErrorNumber.NoError)
        {
            MessageBoxManager.GetMessageBoxStandardWindow(UI.Title_Error,
                                                          string.
                                                              Format(UI.Error_0_trying_to_read_1_of_chosen_filesystem,
                                                                     errno, model.Path), ButtonEnum.Ok, Icon.Error).
                              ShowDialog(view);

            return;
        }

        while(model.Plugin.ReadDir(node, out string dirent) == ErrorNumber.NoError &&
              dirent is not null)
        {
            errno = model.Plugin.Stat(model.Path + "/" + dirent, out FileEntryInfo stat);

            if(errno != ErrorNumber.NoError)
            {
                AaruConsole.
                    ErrorWriteLine(string.Format(UI.Error_0_trying_to_get_information_about_filesystem_entry_named_1,
                                                 errno, dirent));

                continue;
            }

            if(stat.Attributes.HasFlag(FileAttributes.Directory) &&
               !model.Listed)
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
        if(SelectedEntries.Count == 0)
            return;

        var saveFilesFolderDialog = new OpenFolderDialog
        {
            Title = UI.Dialog_Choose_destination_folder
        };

        string result = await saveFilesFolderDialog.ShowAsync(_view);

        if(result is null)
            return;

        Statistics.AddCommand("extract-files");

        string folder = saveFilesFolderDialog.Directory;

        foreach(FileModel file in SelectedEntries)
        {
            string filename = file.Name;

            ButtonResult mboxResult;

            if(DetectOS.IsWindows)
                if(filename.Contains('<')                ||
                   filename.Contains('>')                ||
                   filename.Contains(':')                ||
                   filename.Contains('\\')               ||
                   filename.Contains('/')                ||
                   filename.Contains('|')                ||
                   filename.Contains('?')                ||
                   filename.Contains('*')                ||
                   filename.Any(c => c < 32)             ||
                   filename.ToUpperInvariant() == "CON"  ||
                   filename.ToUpperInvariant() == "PRN"  ||
                   filename.ToUpperInvariant() == "AUX"  ||
                   filename.ToUpperInvariant() == "COM1" ||
                   filename.ToUpperInvariant() == "COM2" ||
                   filename.ToUpperInvariant() == "COM3" ||
                   filename.ToUpperInvariant() == "COM4" ||
                   filename.ToUpperInvariant() == "COM5" ||
                   filename.ToUpperInvariant() == "COM6" ||
                   filename.ToUpperInvariant() == "COM7" ||
                   filename.ToUpperInvariant() == "COM8" ||
                   filename.ToUpperInvariant() == "COM9" ||
                   filename.ToUpperInvariant() == "LPT1" ||
                   filename.ToUpperInvariant() == "LPT2" ||
                   filename.ToUpperInvariant() == "LPT3" ||
                   filename.ToUpperInvariant() == "LPT4" ||
                   filename.ToUpperInvariant() == "LPT5" ||
                   filename.ToUpperInvariant() == "LPT6" ||
                   filename.ToUpperInvariant() == "LPT7" ||
                   filename.ToUpperInvariant() == "LPT8" ||
                   filename.ToUpperInvariant() == "LPT9" ||
                   filename.Last()             == '.'    ||
                   filename.Last()             == ' ')
                {
                    char[] chars;

                    if(filename.Last() == '.' ||
                       filename.Last() == ' ')
                        chars = new char[filename.Length - 1];
                    else
                        chars = new char[filename.Length];

                    for(int ci = 0; ci < chars.Length; ci++)
                        switch(filename[ci])
                        {
                            case '<':
                            case '>':
                            case ':':
                            case '\\':
                            case '/':
                            case '|':
                            case '?':
                            case '*':
                            case '\u0000':
                            case '\u0001':
                            case '\u0002':
                            case '\u0003':
                            case '\u0004':
                            case '\u0005':
                            case '\u0006':
                            case '\u0007':
                            case '\u0008':
                            case '\u0009':
                            case '\u000A':
                            case '\u000B':
                            case '\u000C':
                            case '\u000D':
                            case '\u000E':
                            case '\u000F':
                            case '\u0010':
                            case '\u0011':
                            case '\u0012':
                            case '\u0013':
                            case '\u0014':
                            case '\u0015':
                            case '\u0016':
                            case '\u0017':
                            case '\u0018':
                            case '\u0019':
                            case '\u001A':
                            case '\u001B':
                            case '\u001C':
                            case '\u001D':
                            case '\u001E':
                            case '\u001F':
                                chars[ci] = '_';

                                break;
                            default:
                                chars[ci] = filename[ci];

                                break;
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

                    mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow(UI.Unsupported_filename,
                                                             string.
                                                                 Format(UI.Filename_0_not_supported_want_to_rename_to_1,
                                                                        filename,
                                                                        corrected), ButtonEnum.YesNoCancel,
                                                             Icon.Warning).
                                                         ShowDialog(_view);

                    switch(mboxResult)
                    {
                        case ButtonResult.Cancel: return;
                        case ButtonResult.No:     continue;
                        default:
                            filename = corrected;

                            break;
                    }
                }

            string outputPath = Path.Combine(folder, filename);

            if(File.Exists(outputPath))
            {
                mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow(UI.Existing_file,
                                 string.Format(UI.File_named_0_exists_overwrite_Q, filename),
                                 ButtonEnum.YesNoCancel, Icon.Warning).ShowDialog(_view);

                switch(mboxResult)
                {
                    case ButtonResult.Cancel: return;
                    case ButtonResult.No:     continue;
                    default:
                        try
                        {
                            File.Delete(outputPath);
                        }
                        catch(IOException)
                        {
                            mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow(UI.Cannot_delete,
                                             UI.Could_note_delete_existe_file_continue_Q, ButtonEnum.YesNo,
                                             Icon.Error).ShowDialog(_view);

                            if(mboxResult == ButtonResult.No)
                                return;
                        }

                        break;
                }
            }

            try
            {
                byte[] outBuf = new byte[file.Stat.Length];

                ErrorNumber error = _model.Plugin.OpenFile(_model.Path + "/" + file.Name, out IFileNode fileNode);

                if(error == ErrorNumber.NoError)
                {
                    error = _model.Plugin.ReadFile(fileNode, file.Stat.Length, outBuf, out _);
                    _model.Plugin.CloseFile(fileNode);
                }

                if(error != ErrorNumber.NoError)
                {
                    mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow(UI.Error_reading_file,
                                     string.Format(UI.Error_0_reading_file_continue_Q, error), ButtonEnum.YesNo,
                                     Icon.Error).ShowDialog(_view);

                    if(mboxResult == ButtonResult.No)
                        return;

                    continue;
                }

                var fs = new FileStream(outputPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

                fs.Write(outBuf, 0, outBuf.Length);
                fs.Close();
                var fi = new FileInfo(outputPath);
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                try
                {
                    if(file.Stat.CreationTimeUtc.HasValue)
                        fi.CreationTimeUtc = file.Stat.CreationTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if(file.Stat.LastWriteTimeUtc.HasValue)
                        fi.LastWriteTimeUtc = file.Stat.LastWriteTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if(file.Stat.AccessTimeUtc.HasValue)
                        fi.LastAccessTimeUtc = file.Stat.AccessTimeUtc.Value;
                }
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }
            catch(IOException)
            {
                mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow(UI.Cannot_create_file,
                                                         UI.Could_not_create_destination_file_continue_Q,
                                                         ButtonEnum.YesNo, Icon.Error).
                                                     ShowDialog(_view);

                if(mboxResult == ButtonResult.No)
                    return;
            }
        }
    }
}