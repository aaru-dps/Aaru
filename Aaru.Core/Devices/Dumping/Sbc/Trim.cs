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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Extents;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
    partial class Dump
    {
        void TrimSbcData(Reader scsiReader, ExtentsULong extents, DumpHardwareType currentTry)
        {
            ulong[] tmpArray = _resume.BadBlocks.ToArray();
            bool    sense;
            bool    recoveredError;
            byte[]  buffer;

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

                sense = scsiReader.ReadBlock(out buffer, badSector, out double _, out recoveredError);

                if((sense || _dev.Error) &&
                   !recoveredError)
                    continue;

                _resume.BadBlocks.Remove(badSector);
                extents.Add(badSector);
                _outputPlugin.WriteSector(buffer, badSector);
            }
        }
    }
}