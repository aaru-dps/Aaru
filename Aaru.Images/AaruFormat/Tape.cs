// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Tape.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Aaru Format tape images.
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

using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    public sealed partial class AaruFormat
    {
        /// <inheritdoc />
        public List<TapeFile> Files { get; private set; }
        /// <inheritdoc />
        public List<TapePartition> TapePartitions { get; private set; }
        /// <inheritdoc />
        public bool IsTape { get; private set; }

        /// <inheritdoc />
        public bool AddFile(TapeFile file)
        {
            if(Files.Any(f => f.File == file.File))
            {
                TapeFile removeMe = Files.FirstOrDefault(f => f.File == file.File);
                Files.Remove(removeMe);
            }

            Files.Add(file);

            return true;
        }

        /// <inheritdoc />
        public bool AddPartition(TapePartition partition)
        {
            if(TapePartitions.Any(f => f.Number == partition.Number))
            {
                TapePartition removeMe = TapePartitions.FirstOrDefault(f => f.Number == partition.Number);
                TapePartitions.Remove(removeMe);
            }

            TapePartitions.Add(partition);

            return true;
        }

        /// <inheritdoc />
        public bool SetTape()
        {
            Files          = new List<TapeFile>();
            TapePartitions = new List<TapePartition>();

            return IsTape = true;
        }
    }
}