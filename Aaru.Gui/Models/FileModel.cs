// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FileModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI data models.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains information about files.
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

namespace Aaru.Gui.Models;

using System;
using Aaru.CommonTypes.Structs;
using JetBrains.Annotations;

public sealed class FileModel
{
    public string Name { get; set; }
    [NotNull]
    public string Size => $"{Stat.Length}";
    [NotNull]
    public string CreationTime => Stat.CreationTime == default(DateTime) ? "" : $"{Stat.CreationTime:G}";
    [NotNull]
    public string LastAccessTime => Stat.AccessTime == default(DateTime) ? "" : $"{Stat.AccessTime:G}";
    [NotNull]
    public string ChangedTime => Stat.StatusChangeTime == default(DateTime) ? "" : $"{Stat.StatusChangeTime:G}";
    [NotNull]
    public string LastBackupTime => Stat.BackupTime == default(DateTime) ? "" : $"{Stat.BackupTime:G}";
    [NotNull]
    public string LastWriteTime => Stat.LastWriteTime == default(DateTime) ? "" : $"{Stat.LastWriteTime:G}";
    [NotNull]
    public string Attributes => $"{Stat.Attributes}";
    [NotNull]
    public string Gid => $"{Stat.GID}";
    [NotNull]
    public string Uid => $"{Stat.UID}";
    [NotNull]
    public string Inode => $"{Stat.Inode}";
    [NotNull]
    public string Links => $"{Stat.Links}";
    [NotNull]
    public string Mode => $"{Stat.Mode}";
    public FileEntryInfo Stat { get; set; }
}