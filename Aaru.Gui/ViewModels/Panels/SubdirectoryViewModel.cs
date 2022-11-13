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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Gui.ViewModels.Panels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interop;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core;
using Aaru.Gui.Models;
using Avalonia.Controls;
using JetBrains.Annotations;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

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

        ErrorNumber errno = model.Plugin.ReadDir(model.Path, out List<string> dirents);

        if(errno != ErrorNumber.NoError)
        {
            MessageBoxManager.
                GetMessageBoxStandardWindow("Error",
                                            $"Error {errno} trying to read \"{model.Path}\" of chosen filesystem",
                                            ButtonEnum.Ok, Icon.Error).ShowDialog(view);

            return;
        }

        foreach(string dirent in dirents)
        {
            errno = model.Plugin.Stat(model.Path + "/" + dirent, out FileEntryInfo stat);

            if(errno != ErrorNumber.NoError)
            {
                AaruConsole.
                    ErrorWriteLine($"Error {errno} trying to get information about filesystem entry named {dirent}");

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
    }

    public ObservableCollection<FileModel> Entries             { get; }
    public List<FileModel>                 SelectedEntries     { get; }
    public ReactiveCommand<Unit, Task>     ExtractFilesCommand { get; }

    async Task ExecuteExtractFilesCommand()
    {
        if(SelectedEntries.Count == 0)
            return;

        var saveFilesFolderDialog = new OpenFolderDialog
        {
            Title = "Choose destination folder..."
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

                    for(var ci = 0; ci < chars.Length; ci++)
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

                    mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow("Unsupported filename",
                                     $"The file name {filename} is not supported on this platform.\nDo you want to rename it to {corrected}?",
                                     ButtonEnum.YesNoCancel, Icon.Warning).ShowDialog(_view);

                    if(mboxResult == ButtonResult.Cancel)
                        return;

                    if(mboxResult == ButtonResult.No)
                        continue;

                    filename = corrected;
                }

            string outputPath = Path.Combine(folder, filename);

            if(File.Exists(outputPath))
            {
                mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow("Existing file",
                                 $"A file named {filename} already exists on the destination folder.\nDo you want to overwrite it?",
                                 ButtonEnum.YesNoCancel, Icon.Warning).ShowDialog(_view);

                if(mboxResult == ButtonResult.Cancel)
                    return;

                if(mboxResult == ButtonResult.No)
                    continue;

                try
                {
                    File.Delete(outputPath);
                }
                catch(IOException)
                {
                    mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow("Cannot delete",
                                     "Could not delete existing file.\nDo you want to continue?", ButtonEnum.YesNo,
                                     Icon.Error).ShowDialog(_view);

                    if(mboxResult == ButtonResult.No)
                        return;
                }
            }

            try
            {
                byte[] outBuf = Array.Empty<byte>();

                ErrorNumber error = _model.Plugin.Read(_model.Path + "/" + file.Name, 0, file.Stat.Length, ref outBuf);

                if(error != ErrorNumber.NoError)
                {
                    mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow("Error reading file",
                                     $"Error {error} reading file.\nDo you want to continue?", ButtonEnum.YesNo,
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
                mboxResult = await MessageBoxManager.GetMessageBoxStandardWindow("Cannot create file",
                                 "Could not create destination file.\nDo you want to continue?", ButtonEnum.YesNo,
                                 Icon.Error).ShowDialog(_view);

                if(mboxResult == ButtonResult.No)
                    return;
            }
        }
    }
}