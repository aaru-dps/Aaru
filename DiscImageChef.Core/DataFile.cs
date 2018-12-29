// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DataFile.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Abstracts writing to files.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using DiscImageChef.Console;

namespace DiscImageChef.Core
{
    /// <summary>
    ///     Abstracts a datafile with a block based interface
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public class DataFile
    {
        FileStream dataFs;

        /// <summary>
        ///     Opens, or create, a new file
        /// </summary>
        /// <param name="outputFile">File</param>
        public DataFile(string outputFile)
        {
            dataFs = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        /// <summary>
        ///     Closes the file
        /// </summary>
        public void Close()
        {
            dataFs?.Close();
        }

        /// <summary>
        ///     Reads bytes at current position
        /// </summary>
        /// <param name="array">Array to place read data within</param>
        /// <param name="offset">Offset of <see cref="array" /> where data will be read</param>
        /// <param name="count">How many bytes to read</param>
        /// <returns>How many bytes were read</returns>
        public int Read(byte[] array, int offset, int count)
        {
            return dataFs.Read(array, offset, count);
        }

        /// <summary>
        ///     Seeks to the specified block
        /// </summary>
        /// <param name="block">Block to seek to</param>
        /// <param name="blockSize">Block size in bytes</param>
        /// <returns>Position</returns>
        public long Seek(ulong block, ulong blockSize)
        {
            return dataFs.Seek((long)(block * blockSize), SeekOrigin.Begin);
        }

        /// <summary>
        ///     Seeks to specified byte position
        /// </summary>
        /// <param name="offset">Byte position</param>
        /// <param name="origin">Where to count for position</param>
        /// <returns>Position</returns>
        public long Seek(ulong offset, SeekOrigin origin)
        {
            return dataFs.Seek((long)offset, origin);
        }

        /// <summary>
        ///     Seeks to specified byte position
        /// </summary>
        /// <param name="offset">Byte position</param>
        /// <param name="origin">Where to count for position</param>
        /// <returns>Position</returns>
        public long Seek(long offset, SeekOrigin origin)
        {
            return dataFs.Seek(offset, origin);
        }

        /// <summary>
        ///     Writes data at current position
        /// </summary>
        /// <param name="data">Data</param>
        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        ///     Writes data at current position
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="offset">Offset of data from where to start taking data to write</param>
        /// <param name="count">How many bytes to write</param>
        public void Write(byte[] data, int offset, int count)
        {
            dataFs.Write(data, offset, count);
        }

        /// <summary>
        ///     Writes data at specified block
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="block">Block</param>
        /// <param name="blockSize">Bytes per block</param>
        public void WriteAt(byte[] data, ulong block, uint blockSize)
        {
            WriteAt(data, block, blockSize, 0, data.Length);
        }

        /// <summary>
        ///     Writes data at specified block
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="block">Block</param>
        /// <param name="blockSize">Bytes per block</param>
        /// <param name="offset">Offset of data from where to start taking data to write</param>
        /// <param name="count">How many bytes to write</param>
        public void WriteAt(byte[] data, ulong block, uint blockSize, int offset, int count)
        {
            dataFs.Seek((long)(block * blockSize), SeekOrigin.Begin);
            dataFs.Write(data, offset, count);
        }

        /// <summary>
        ///     Current file position
        /// </summary>
        public long Position => dataFs.Position;

        /// <summary>
        ///     Writes data to a newly created file
        /// </summary>
        /// <param name="who">Who asked the file to be written (class, plugin, etc.)</param>
        /// <param name="data">Data to write</param>
        /// <param name="outputPrefix">First part of the file name</param>
        /// <param name="outputSuffix">Last part of the file name</param>
        /// <param name="whatWriting">What is the data about?</param>
        public static void WriteTo(string who, string outputPrefix, string outputSuffix, string whatWriting,
                                   byte[] data)
        {
            if(!string.IsNullOrEmpty(outputPrefix) && !string.IsNullOrEmpty(outputSuffix))
                WriteTo(who, outputPrefix + outputSuffix, data, whatWriting);
        }

        /// <summary>
        ///     Writes data to a newly created file
        /// </summary>
        /// <param name="who">Who asked the file to be written (class, plugin, etc.)</param>
        /// <param name="filename">Filename to create</param>
        /// <param name="data">Data to write</param>
        /// <param name="whatWriting">What is the data about?</param>
        /// <param name="overwrite">If set to <c>true</c> overwrites the file, does nothing otherwise</param>
        public static void WriteTo(string who, string filename, byte[] data, string whatWriting = null,
                                   bool   overwrite = false)
        {
            if(string.IsNullOrEmpty(filename)) return;

            if(File.Exists(filename))
                if(overwrite)
                    File.Delete(filename);
                else
                {
                    DicConsole.ErrorWriteLine("Not overwriting file {0}", filename);
                    return;
                }

            try
            {
                DicConsole.DebugWriteLine(who, "Writing " + whatWriting + " to {0}", filename);
                FileStream outputFs = new FileStream(filename, FileMode.CreateNew);
                outputFs.Write(data, 0, data.Length);
                outputFs.Close();
            }
            catch { DicConsole.ErrorWriteLine("Unable to write file {0}", filename); }
        }
    }
}