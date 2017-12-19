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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.Console;

namespace DiscImageChef.Core
{
    public class DataFile
    {
        FileStream dataFs;

        public DataFile(string outputFile)
        {
            dataFs = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public void Close()
        {
            if(dataFs != null)
                dataFs.Close();
        }

        public int Read(byte[] array, int offset, int count)
        {
            return dataFs.Read(array, offset, count);
        }

        public long Seek(ulong block, ulong blockSize)
        {
            return dataFs.Seek((long)(block * blockSize), SeekOrigin.Begin);
        }

        public long Seek(ulong offset, SeekOrigin origin)
        {
            return dataFs.Seek((long)offset, origin);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return dataFs.Seek(offset, origin);
        }

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        public void Write(byte[] data, int offset, int count)
        {
            dataFs.Write(data, offset, count);
        }

        public void WriteAt(byte[] data, ulong block, uint blockSize)
        {
            WriteAt(data, block, blockSize, 0, data.Length);
        }

        public void WriteAt(byte[] data, ulong block, uint blockSize, int offset, int count)
        {
            dataFs.Seek((long)(block * blockSize), SeekOrigin.Begin);
            dataFs.Write(data, offset, count);
        }

        public long Position { get { return dataFs.Position; }}

        public static void WriteTo(string who, string outputPrefix, string outputSuffix, string what, byte[] data)
        {
            if(!string.IsNullOrEmpty(outputPrefix) && !string.IsNullOrEmpty(outputSuffix))
                WriteTo(who, outputPrefix + outputSuffix, data, what);
        }

        public static void WriteTo(string who, string filename, byte[] data, string whatWriting = null, bool overwrite = false)
        {
            if(!string.IsNullOrEmpty(filename))
            {
                if(File.Exists(filename))
                {
                    if(overwrite)
                        File.Delete(filename);
                    else
                    {
                        DicConsole.ErrorWriteLine("Not overwriting file {0}", filename);
                        return;
                    }
                }

                try
                {
                    DicConsole.DebugWriteLine(who, "Writing " + whatWriting + " to {0}", filename);
                    FileStream outputFs = new FileStream(filename, FileMode.CreateNew);
                    outputFs.Write(data, 0, data.Length);
                    outputFs.Close();
                }
                catch
                {
                    DicConsole.ErrorWriteLine("Unable to write file {0}", filename);
                }
            }
        }
    }
}
