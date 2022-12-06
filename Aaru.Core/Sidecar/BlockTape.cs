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
using Schemas;

namespace Aaru.Core
{
    /// <summary>Sidecar operations</summary>
    public sealed partial class Sidecar
    {
        /// <summary>Creates a metadata sidecar for a block tape (e.g. scsi streaming)</summary>
        /// <param name="files">List of files</param>
        /// <param name="folderName">Dump path</param>
        /// <param name="blockSize">Expected block size in bytes</param>
        public CICMMetadataType BlockTape(string folderName, List<string> files, uint blockSize)
        {
            _sidecar = new CICMMetadataType
            {
                BlockMedia = new[]
                {
                    new BlockMediaType
                    {
                        Image = new ImageType
                        {
                            format          = "Directory",
                            offsetSpecified = false,
                            Value           = folderName
                        },
                        Sequence = new SequenceType
                        {
                            MediaTitle    = folderName,
                            MediaSequence = 1,
                            TotalMedia    = 1
                        },
                        PhysicalBlockSize = blockSize,
                        LogicalBlockSize  = blockSize,
                        TapeInformation = new[]
                        {
                            new TapePartitionType
                            {
                                Image = new ImageType
                                {
                                    format          = "Directory",
                                    offsetSpecified = false,
                                    Value           = folderName
                                }
                            }
                        }
                    }
                }
            };

            if(_aborted)
                return _sidecar;

            ulong              currentBlock = 0;
            ulong              totalSize    = 0;
            var                tapeWorker   = new Checksum();
            List<TapeFileType> tapeFiles    = new List<TapeFileType>();

            UpdateStatus("Hashing files...");

            for(int i = 0; i < files.Count; i++)
            {
                if(_aborted)
                    return _sidecar;

                _fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                var fileWorker = new Checksum();

                var tapeFile = new TapeFileType
                {
                    Image = new ImageType
                    {
                        format = "Raw disk image (sector by sector copy)",
                        offset = 0,
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
                        _fs.Read(sector, 0, sector.Length);

                        UpdateProgress2($"Hashing block {doneSectors} of {sectors} on file {i + 1} of {files.Count}",
                                        (long)doneSectors, (long)sectors);

                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector = new byte[(uint)(sectors - doneSectors) * blockSize];
                        _fs.Read(sector, 0, sector.Length);

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
                tapeFile.Checksums =  fileWorker.End().ToArray();
                tapeFiles.Add(tapeFile);

                EndProgress2();
            }

            UpdateStatus("Setting metadata...");
            _sidecar.BlockMedia[0].Checksums                    = tapeWorker.End().ToArray();
            _sidecar.BlockMedia[0].ContentChecksums             = _sidecar.BlockMedia[0].Checksums;
            _sidecar.BlockMedia[0].Size                         = totalSize;
            _sidecar.BlockMedia[0].LogicalBlocks                = currentBlock;
            _sidecar.BlockMedia[0].TapeInformation[0].EndBlock  = currentBlock - 1;
            _sidecar.BlockMedia[0].TapeInformation[0].Size      = totalSize;
            _sidecar.BlockMedia[0].TapeInformation[0].Checksums = _sidecar.BlockMedia[0].Checksums;
            _sidecar.BlockMedia[0].TapeInformation[0].File      = tapeFiles.ToArray();

            // This is purely for convenience, as typically these kind of data represents QIC tapes
            if(blockSize == 512)
            {
                _sidecar.BlockMedia[0].DiskType = "Quarter-inch cartridge";

                if(totalSize <= 20 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-11";
                else if(totalSize <= 40 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-40";
                else if(totalSize <= 60 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-24";
                else if(totalSize <= 80 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-80";
                else if(totalSize <= 120 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-120";
                else if(totalSize <= 150 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-150";
                else if(totalSize <= 320 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-320";
                else if(totalSize <= 340 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-3010";
                else if(totalSize <= 525 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-525";
                else if(totalSize <= 670 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-3020";
                else if(totalSize <= 1200 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-3080";
                else if(totalSize <= 1350 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-1350";
                else if(totalSize <= (long)4000 * 1048576)
                    _sidecar.BlockMedia[0].DiskSubType = "QIC-3095";
                else
                {
                    _sidecar.BlockMedia[0].DiskType    = "Unknown tape";
                    _sidecar.BlockMedia[0].DiskSubType = "Unknown tape";
                }
            }
            else
            {
                _sidecar.BlockMedia[0].DiskType    = "Unknown tape";
                _sidecar.BlockMedia[0].DiskSubType = "Unknown tape";
            }

            return _sidecar;
        }
    }
}