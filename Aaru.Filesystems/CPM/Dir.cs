// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//      Methods to show the CP/M filesystem directory.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class CPM
{
    /// <inheritdoc />
    public ErrorNumber ReadDir(string path, out List<string> contents)
    {
        contents = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(!string.IsNullOrEmpty(path) &&
           string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
            return ErrorNumber.NotSupported;

        contents = new List<string>(_dirList);

        return ErrorNumber.NoError;
    }

    /// <summary>
    ///     Checks that the given directory blocks follow the CP/M filesystem directory specification Corrupted
    ///     directories will fail. FAT directories will false positive if all files start with 0x05, and do not use full
    ///     extensions, for example: "σAFILE.GZ" (using code page 437)
    /// </summary>
    /// <returns>False if the directory does not follow the directory specification</returns>
    /// <param name="directory">Directory blocks.</param>
    bool CheckDir(byte[] directory)
    {
        try
        {
            if(directory == null)
                return false;

            int fileCount = 0;

            for(int off = 0; off < directory.Length; off += 32)
            {
                DirectoryEntry entry = Marshal.ByteArrayToStructureLittleEndian<DirectoryEntry>(directory, off, 32);

                if((entry.statusUser & 0x7F) < 0x20)
                {
                    for(int f = 0; f < 8; f++)
                        if(entry.filename[f] < 0x20 &&
                           entry.filename[f] != 0x00)
                            return false;

                    for(int e = 0; e < 3; e++)
                        if(entry.extension[e] < 0x20 &&
                           entry.extension[e] != 0x00)
                            return false;

                    if(!ArrayHelpers.ArrayIsNullOrWhiteSpace(entry.filename))
                        fileCount++;
                }
                else
                    switch(entry.statusUser)
                    {
                        case 0x20:
                        {
                            for(int f = 0; f < 8; f++)
                                if(entry.filename[f] < 0x20 &&
                                   entry.filename[f] != 0x00)
                                    return false;

                            for(int e = 0; e < 3; e++)
                                if(entry.extension[e] < 0x20 &&
                                   entry.extension[e] != 0x00)
                                    return false;

                            _label             = Encoding.ASCII.GetString(directory, off + 1, 11).Trim();
                            _labelCreationDate = new byte[4];
                            _labelUpdateDate   = new byte[4];
                            Array.Copy(directory, off + 24, _labelCreationDate, 0, 4);
                            Array.Copy(directory, off + 28, _labelUpdateDate, 0, 4);

                            break;
                        }
                        case 0x21 when directory[off + 1] == 0x00:
                            _thirdPartyTimestamps = true;

                            break;
                        case 0x21:
                            _standardTimestamps |= directory[off + 21] == 0x00 && directory[off + 31] == 0x00;

                            break;
                    }
            }

            return fileCount > 0;
        }
        catch
        {
            return false;
        }
    }
}