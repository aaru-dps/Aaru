// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle the Apple Macintosh File System directory.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh Volume II
    public sealed partial class AppleMFS
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

            contents = _idToFilename.Select(kvp => kvp.Value).ToList();

            if(_debug)
            {
                contents.Add("$");
                contents.Add("$Bitmap");
                contents.Add("$MDB");

                if(_bootBlocks != null)
                    contents.Add("$Boot");
            }

            contents.Sort();

            return ErrorNumber.NoError;
        }

        bool FillDirectory()
        {
            _idToFilename = new Dictionary<uint, string>();
            _idToEntry    = new Dictionary<uint, FileEntry>();
            _filenameToId = new Dictionary<string, uint>();

            int offset = 0;

            while(offset + 51 < _directoryBlocks.Length)
            {
                var entry = new FileEntry
                {
                    flFlags = (FileFlags)_directoryBlocks[offset + 0]
                };

                if(!entry.flFlags.HasFlag(FileFlags.Used))
                    break;

                entry.flTyp = _directoryBlocks[offset + 1];

                entry.flUsrWds =
                    Marshal.ByteArrayToStructureBigEndian<AppleCommon.FInfo>(_directoryBlocks, offset + 2, 16);

                entry.flFlNum  = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 18);
                entry.flStBlk  = BigEndianBitConverter.ToUInt16(_directoryBlocks, offset + 22);
                entry.flLgLen  = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 24);
                entry.flPyLen  = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 28);
                entry.flRStBlk = BigEndianBitConverter.ToUInt16(_directoryBlocks, offset + 32);
                entry.flRLgLen = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 34);
                entry.flRPyLen = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 38);
                entry.flCrDat  = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 42);
                entry.flMdDat  = BigEndianBitConverter.ToUInt32(_directoryBlocks, offset + 46);
                entry.flNam    = new byte[_directoryBlocks[offset + 50] + 1];
                Array.Copy(_directoryBlocks, offset + 50, entry.flNam, 0, entry.flNam.Length);

                string lowerFilename = StringHandlers.PascalToString(entry.flNam, Encoding).ToLowerInvariant().
                                                      Replace('/', ':');

                if(entry.flFlags.HasFlag(FileFlags.Used)     &&
                   !_idToFilename.ContainsKey(entry.flFlNum) &&
                   !_idToEntry.ContainsKey(entry.flFlNum)    &&
                   !_filenameToId.ContainsKey(lowerFilename) &&
                   entry.flFlNum > 0)
                {
                    _idToEntry.Add(entry.flFlNum, entry);

                    _idToFilename.Add(entry.flFlNum,
                                      StringHandlers.PascalToString(entry.flNam, Encoding).Replace('/', ':'));

                    _filenameToId.Add(lowerFilename, entry.flFlNum);

                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flFlags = {0}", entry.flFlags);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flTyp = {0}", entry.flTyp);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flFlNum = {0}", entry.flFlNum);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flStBlk = {0}", entry.flStBlk);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flLgLen = {0}", entry.flLgLen);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flPyLen = {0}", entry.flPyLen);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flRStBlk = {0}", entry.flRStBlk);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flRLgLen = {0}", entry.flRLgLen);
                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flRPyLen = {0}", entry.flRPyLen);

                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flCrDat = {0}",
                                               DateHandlers.MacToDateTime(entry.flCrDat));

                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flMdDat = {0}",
                                               DateHandlers.MacToDateTime(entry.flMdDat));

                    AaruConsole.DebugWriteLine("DEBUG (AppleMFS plugin)", "entry.flNam0 = {0}",
                                               StringHandlers.PascalToString(entry.flNam, Encoding));
                }

                offset += 50 + entry.flNam.Length;

                // "Entries are always an integral number of words"
                if(offset % 2 != 0)
                    offset++;

                // TODO: "Entries don't cross logical block boundaries"
            }

            return true;
        }
    }
}