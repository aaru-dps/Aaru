// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle Apple DOS filesystem catalog (aka directory).
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
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class AppleDOS
{
    /// <inheritdoc />
    public ErrorNumber ReadLink(string path, out string dest)
    {
        dest = null;

        return !_mounted ? ErrorNumber.AccessDenied : ErrorNumber.NotSupported;
    }

    /// <inheritdoc />
    public ErrorNumber OpenDir(string path, out IDirNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(!string.IsNullOrEmpty(path) &&
           string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
            return ErrorNumber.NotSupported;

        List<string> contents = _catalogCache.Keys.ToList();

        if(_debug)
        {
            contents.Add("$");
            contents.Add("$Boot");
            contents.Add("$Vtoc");
        }

        contents.Sort();

        node = new AppleDosDirNode
        {
            Path      = path,
            _position = 0,
            _contents = contents.ToArray()
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadDir(string path, out List<string> contents)
    {
        contents = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(!string.IsNullOrEmpty(path) &&
           string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
            return ErrorNumber.NotSupported;

        contents = _catalogCache.Keys.ToList();

        if(_debug)
        {
            contents.Add("$");
            contents.Add("$Boot");
            contents.Add("$Vtoc");
        }

        contents.Sort();

        return ErrorNumber.NoError;
    }

    ErrorNumber ReadCatalog()
    {
        var   catalogMs = new MemoryStream();
        ulong lba       = (ulong)((_vtoc.catalogTrack * _sectorsPerTrack) + _vtoc.catalogSector);
        _totalFileEntries = 0;
        _catalogCache     = new Dictionary<string, ushort>();
        _fileTypeCache    = new Dictionary<string, byte>();
        _lockedFiles      = new List<string>();

        if(lba == 0 ||
           lba > _device.Info.Sectors)
            return ErrorNumber.InvalidArgument;

        while(lba != 0)
        {
            _usedSectors++;
            ErrorNumber errno = _device.ReadSector(lba, out byte[] catSectorB);

            if(errno != ErrorNumber.NoError)
                return errno;

            _totalFileEntries += 7;

            if(_debug)
                catalogMs.Write(catSectorB, 0, catSectorB.Length);

            // Read the catalog sector
            CatalogSector catSector = Marshal.ByteArrayToStructureLittleEndian<CatalogSector>(catSectorB);

            foreach(FileEntry entry in catSector.entries.Where(entry => entry.extentTrack > 0))
            {
                _track1UsedByFiles |= entry.extentTrack == 1;
                _track2UsedByFiles |= entry.extentTrack == 2;

                byte[] filenameB = new byte[30];
                ushort ts        = (ushort)((entry.extentTrack << 8) | entry.extentSector);

                // Apple DOS has high byte set over ASCII.
                for(int i = 0; i < 30; i++)
                    filenameB[i] = (byte)(entry.filename[i] & 0x7F);

                string filename = StringHandlers.SpacePaddedToString(filenameB, _encoding);

                if(!_catalogCache.ContainsKey(filename))
                    _catalogCache.Add(filename, ts);

                if(!_fileTypeCache.ContainsKey(filename))
                    _fileTypeCache.Add(filename, (byte)(entry.typeAndFlags & 0x7F));

                if((entry.typeAndFlags & 0x80) == 0x80 &&
                   !_lockedFiles.Contains(filename))
                    _lockedFiles.Add(filename);
            }

            lba = (ulong)((catSector.trackOfNext * _sectorsPerTrack) + catSector.sectorOfNext);

            if(lba > _device.Info.Sectors)
                break;
        }

        if(_debug)
            _catalogBlocks = catalogMs.ToArray();

        return ErrorNumber.NoError;
    }
}