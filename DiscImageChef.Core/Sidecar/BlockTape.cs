// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Schemas;

namespace DiscImageChef.Core
{
    public partial class Sidecar
    {
        /// <summary>
        ///     Creates a metadata sidecar for a block tape (e.g. scsi streaming)
        /// </summary>
        /// <param name="files">List of files</param>
        /// <param name="folderName">Dump path</param>
        /// <param name="blockSize">Expected block size in bytes</param>
        public CICMMetadataType BlockTape(string folderName, List<string> files, int blockSize)
        {
            sidecar = new CICMMetadataType
            {
                BlockMedia = new[]
                {
                    new BlockMediaType
                    {
                        Image =
                            new ImageType
                            {
                                format = "Directory", offsetSpecified = false, Value = folderName
                            },
                        Sequence =
                            new SequenceType {MediaTitle = folderName, MediaSequence = 1, TotalMedia = 1},
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

            long               currentBlock = 0;
            long               totalSize    = 0;
            Checksum           tapeWorker   = new Checksum();
            List<TapeFileType> tapeFiles    = new List<TapeFileType>();

            for(int i = 0; i < files.Count; i++)
            {
                fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                Checksum fileWorker = new Checksum();
                TapeFileType tapeFile = new TapeFileType
                {
                    Image = new ImageType
                    {
                        format = "Raw disk image (sector by sector copy)",
                        offset = 0,
                        Value  = Path.GetFileName(files[i])
                    },
                    Size       = fs.Length,
                    BlockSize  = blockSize,
                    StartBlock = currentBlock,
                    Sequence   = i
                };

                const uint SECTORS_TO_READ = 512;
                long       sectors         = fs.Length / blockSize;
                long       doneSectors     = 0;

                InitProgress2();
                while(doneSectors < sectors)
                {
                    byte[] sector;

                    if(sectors - doneSectors >= SECTORS_TO_READ)
                    {
                        sector = new byte[SECTORS_TO_READ * blockSize];
                        fs.Read(sector, 0, sector.Length);
                        UpdateProgress2($"Hashing block {doneSectors} of {sectors} on file {i + 1} of {files.Count}",
                                        doneSectors, sectors);
                        doneSectors += SECTORS_TO_READ;
                    }
                    else
                    {
                        sector = new byte[(uint)(sectors - doneSectors) * blockSize];
                        fs.Read(sector, 0, sector.Length);
                        UpdateProgress2($"Hashing block {doneSectors} of {sectors} on file {i + 1} of {files.Count}",
                                        doneSectors, sectors);
                        doneSectors += sectors - doneSectors;
                    }

                    fileWorker.Update(sector);
                    tapeWorker.Update(sector);
                }

                tapeFile.EndBlock  =  tapeFile.StartBlock + sectors - 1;
                currentBlock       += sectors;
                totalSize          += fs.Length;
                tapeFile.Checksums =  fileWorker.End().ToArray();
                tapeFiles.Add(tapeFile);

                EndProgress2();
            }

            sidecar.BlockMedia[0].Checksums                    = tapeWorker.End().ToArray();
            sidecar.BlockMedia[0].ContentChecksums             = sidecar.BlockMedia[0].Checksums;
            sidecar.BlockMedia[0].Size                         = totalSize;
            sidecar.BlockMedia[0].LogicalBlocks                = currentBlock;
            sidecar.BlockMedia[0].TapeInformation[0].EndBlock  = currentBlock - 1;
            sidecar.BlockMedia[0].TapeInformation[0].Size      = totalSize;
            sidecar.BlockMedia[0].TapeInformation[0].Checksums = sidecar.BlockMedia[0].Checksums;
            sidecar.BlockMedia[0].TapeInformation[0].File      = tapeFiles.ToArray();

            // This is purely for convenience, as typically these kind of data represents QIC tapes
            if(blockSize == 512)
            {
                sidecar.BlockMedia[0].DiskType = "Quarter-inch cartridge";

                if(totalSize      <= 20         * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-11";
                else if(totalSize <= 40         * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-40";
                else if(totalSize <= 60         * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-24";
                else if(totalSize <= 80         * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-80";
                else if(totalSize <= 120        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-120";
                else if(totalSize <= 150        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-150";
                else if(totalSize <= 320        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-320";
                else if(totalSize <= 340        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-3010";
                else if(totalSize <= 525        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-525";
                else if(totalSize <= 670        * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-3020";
                else if(totalSize <= 1200       * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-3080";
                else if(totalSize <= 1350       * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-1350";
                else if(totalSize <= (long)4000 * 1048576) sidecar.BlockMedia[0].DiskSubType = "QIC-3095";
                else
                {
                    sidecar.BlockMedia[0].DiskType    = "Unknown tape";
                    sidecar.BlockMedia[0].DiskSubType = "Unknown tape";
                }
            }
            else
            {
                sidecar.BlockMedia[0].DiskType    = "Unknown tape";
                sidecar.BlockMedia[0].DiskSubType = "Unknown tape";
            }

            return sidecar;
        }
    }
}