// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlockTape.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains logic to create sidecar from a block tape media dump.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.Helpers;

namespace Aaru.Core;

/// <summary>Sidecar operations</summary>
public sealed partial class Sidecar
{
    /// <summary>Creates a metadata sidecar for a block tape (e.g. scsi streaming)</summary>
    /// <param name="files">List of files</param>
    /// <param name="folderName">Dump path</param>
    /// <param name="blockSize">Expected block size in bytes</param>
    public Metadata BlockTape(string folderName, List<string> files, uint blockSize)
    {
        _sidecar = new Metadata
        {
            BlockMedias = new List<BlockMedia>
            {
                new()
                {
                    Image = new Image
                    {
                        Format = "Directory",
                        Value  = folderName
                    },
                    Sequence = new Sequence
                    {
                        Title         = folderName,
                        MediaSequence = 1,
                        TotalMedia    = 1
                    },
                    PhysicalBlockSize = blockSize,
                    LogicalBlockSize  = blockSize,
                    TapeInformation = new List<TapePartition>
                    {
                        new()
                        {
                            Image = new Image
                            {
                                Format = "Directory",
                                Value  = folderName
                            }
                        }
                    }
                }
            }
        };

        if(_aborted)
            return _sidecar;

        ulong          currentBlock = 0;
        ulong          totalSize    = 0;
        var            tapeWorker   = new Checksum();
        List<TapeFile> tapeFiles    = new();

        UpdateStatus(Localization.Core.Hashing_files);

        for(int i = 0; i < files.Count; i++)
        {
            if(_aborted)
                return _sidecar;

            _fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);
            var fileWorker = new Checksum();

            var tapeFile = new TapeFile
            {
                Image = new Image
                {
                    Format = "Raw disk image (sector by sector copy)",
                    Offset = 0,
                    Value  = Path.GetFileName(files[i])
                },
                Size       = (ulong)_fs.Length,
                BlockSize  = blockSize,
                StartBlock = currentBlock,
                Sequence   = (ulong)i
            };

            const uint sectorsToRead = 512;
            ulong      sectors       = (ulong)_fs.Length / blockSize;
            ulong      doneSectors   = 0;

            InitProgress2();

            while(doneSectors < sectors)
            {
                if(_aborted)
                {
                    EndProgress2();

                    return _sidecar;
                }

                byte[] sector;

                if(sectors - doneSectors >= sectorsToRead)
                {
                    sector = new byte[sectorsToRead * blockSize];
                    _fs.EnsureRead(sector, 0, sector.Length);

                    UpdateProgress2($"Hashing block {doneSectors} of {sectors} on file {i + 1} of {files.Count}",
                                    (long)doneSectors, (long)sectors);

                    doneSectors += sectorsToRead;
                }
                else
                {
                    sector = new byte[(uint)(sectors - doneSectors) * blockSize];
                    _fs.EnsureRead(sector, 0, sector.Length);

                    UpdateProgress2($"Hashing block {doneSectors} of {sectors} on file {i + 1} of {files.Count}",
                                    (long)doneSectors, (long)sectors);

                    doneSectors += sectors - doneSectors;
                }

                fileWorker.Update(sector);
                tapeWorker.Update(sector);
            }

            tapeFile.EndBlock  =  tapeFile.StartBlock + sectors - 1;
            currentBlock       += sectors;
            totalSize          += (ulong)_fs.Length;
            tapeFile.Checksums =  fileWorker.End();
            tapeFiles.Add(tapeFile);

            EndProgress2();
        }

        UpdateStatus("Setting metadata...");
        _sidecar.BlockMedias[0].Checksums                    = tapeWorker.End();
        _sidecar.BlockMedias[0].ContentChecksums             = _sidecar.BlockMedias[0].Checksums;
        _sidecar.BlockMedias[0].Size                         = totalSize;
        _sidecar.BlockMedias[0].LogicalBlocks                = currentBlock;
        _sidecar.BlockMedias[0].TapeInformation[0].EndBlock  = currentBlock - 1;
        _sidecar.BlockMedias[0].TapeInformation[0].Size      = totalSize;
        _sidecar.BlockMedias[0].TapeInformation[0].Checksums = _sidecar.BlockMedias[0].Checksums;
        _sidecar.BlockMedias[0].TapeInformation[0].File      = tapeFiles;

        // This is purely for convenience, as typically these kind of data represents QIC tapes
        if(blockSize == 512)
        {
            _sidecar.BlockMedias[0].MediaType = "Quarter-inch cartridge";

            switch(totalSize)
            {
                case <= 20 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-11";

                    break;
                case <= 40 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-40";

                    break;
                case <= 60 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-24";

                    break;
                case <= 80 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-80";

                    break;
                case <= 120 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-120";

                    break;
                case <= 150 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-150";

                    break;
                case <= 320 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-320";

                    break;
                case <= 340 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-3010";

                    break;
                case <= 525 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-525";

                    break;
                case <= 670 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-3020";

                    break;
                case <= 1200 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-3080";

                    break;
                case <= 1350 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-1350";

                    break;
                case <= (long)4000 * 1048576:
                    _sidecar.BlockMedias[0].MediaSubType = "QIC-3095";

                    break;
                default:
                    _sidecar.BlockMedias[0].MediaType    = "Unknown tape";
                    _sidecar.BlockMedias[0].MediaSubType = "Unknown tape";

                    break;
            }
        }
        else
        {
            _sidecar.BlockMedias[0].MediaType    = "Unknown tape";
            _sidecar.BlockMedias[0].MediaSubType = "Unknown tape";
        }

        return _sidecar;
    }
}