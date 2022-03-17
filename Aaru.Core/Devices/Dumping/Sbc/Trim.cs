// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Trim.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping;

using Aaru.CommonTypes.Extents;
using Aaru.CommonTypes.Interfaces;
using Schemas;

partial class Dump
{
    /// <summary>Trims data when dumping from a SCSI Block Commands compliant device</summary>
    /// <param name="scsiReader">SCSI reader</param>
    /// <param name="extents">Correctly dump extents</param>
    /// <param name="currentTry">Resume information</param>
    /// <param name="blankExtents">Blank extents</param>
    void TrimSbcData(Reader scsiReader, ExtentsULong extents, DumpHardwareType currentTry, ExtentsULong blankExtents)
    {
        ulong[] tmpArray = _resume.BadBlocks.ToArray();
        bool    sense;
        bool    recoveredError;
        bool    blankCheck;
        byte[]  buffer;
        var     newBlank     = false;

        if(_outputPlugin is not IWritableImage outputFormat)
        {
            StoppingErrorMessage?.Invoke("Image is not writable, aborting...");

            return;
        }

        foreach(ulong badSector in tmpArray)
        {
            if(_aborted)
            {
                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                UpdateStatus?.Invoke("Aborted!");
                _dumpLog.WriteLine("Aborted!");

                break;
            }

            PulseProgress?.Invoke($"Trimming sector {badSector}");

            sense = scsiReader.ReadBlock(out buffer, badSector, out double _, out recoveredError, out blankCheck);

            if(blankCheck)
            {
                blankExtents.Add(badSector, badSector);
                newBlank = true;
                _resume.BadBlocks.Remove(badSector);

                UpdateStatus?.Invoke($"Found blank block {badSector}.");
                _dumpLog.WriteLine("Found blank block {0}.", badSector);

                continue;
            }

            if((sense || _dev.Error) &&
               !recoveredError)
                continue;

            _resume.BadBlocks.Remove(badSector);
            extents.Add(badSector);
            outputFormat.WriteSector(buffer, badSector);
        }

        if(newBlank)
            _resume.BlankExtents = ExtentsConverter.ToMetadata(blankExtents);
    }
}