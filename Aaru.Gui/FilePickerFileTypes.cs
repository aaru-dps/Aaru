// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FilePickerFileTypes.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI.
//
// --[ Description ] ----------------------------------------------------------
//
//     Common file types to use with Avalonia file pickers.
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

using Aaru.Localization;
using Avalonia.Platform.Storage;

namespace Aaru.Gui;

/// <summary>
///     Dictionary of well known file types.
/// </summary>
public static class FilePickerFileTypes
{
    public static FilePickerFileType All { get; } = new(UI.Dialog_All_files)
    {
        Patterns  = ["*.*"],
        MimeTypes = ["*/*"]
    };

    public static FilePickerFileType PlainText { get; } = new(UI.Dialog_Text_files)
    {
        Patterns                    = ["*.txt"],
        AppleUniformTypeIdentifiers = ["public.plain-text"],
        MimeTypes                   = ["text/plain"]
    };

    public static FilePickerFileType Log { get; } = new(UI.Dialog_Log_files)
    {
        Patterns                    = ["*.log"],
        AppleUniformTypeIdentifiers = ["public.plain-text"],
        MimeTypes                   = ["text/plain"]
    };

    public static FilePickerFileType Binary { get; } = new(UI.Dialog_Binary_files)
    {
        Patterns  = ["*.bin"],
        MimeTypes = ["application/octet-stream"]
    };

    public static FilePickerFileType AaruMetadata { get; } = new(UI.Dialog_Aaru_Metadata)
    {
        Patterns                    = ["*.json"],
        AppleUniformTypeIdentifiers = ["public.json"],
        MimeTypes                   = ["application/json"]
    };

    public static FilePickerFileType AaruResumeFile { get; } = new(UI.Dialog_Aaru_Resume)
    {
        Patterns                    = ["*.json"],
        AppleUniformTypeIdentifiers = ["public.json"],
        MimeTypes                   = ["application/json"]
    };
}