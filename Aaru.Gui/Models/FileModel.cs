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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Structs;

namespace Aaru.Gui.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Size => $"{Stat.Length}";
        public string CreationTime => Stat.CreationTime == default(DateTime) ? "" : $"{Stat.CreationTime:G}";
        public string LastAccessTime => Stat.AccessTime == default(DateTime) ? "" : $"{Stat.AccessTime:G}";
        public string ChangedTime => Stat.StatusChangeTime == default(DateTime) ? "" : $"{Stat.StatusChangeTime:G}";
        public string LastBackupTime => Stat.BackupTime == default(DateTime) ? "" : $"{Stat.BackupTime:G}";
        public string LastWriteTime => Stat.LastWriteTime == default(DateTime) ? "" : $"{Stat.LastWriteTime:G}";
        public string Attributes => $"{Stat.Attributes}";
        public string Gid => $"{Stat.GID}";
        public string Uid => $"{Stat.UID}";
        public string Inode => $"{Stat.Inode}";
        public string Links => $"{Stat.Links}";
        public string Mode => $"{Stat.Mode}";
        public FileEntryInfo Stat { get; set; }
    }
}