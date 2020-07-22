// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies raw image, that is, user data sector by sector copy.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages
{
    public sealed partial class ZZZRawImage
    {
        public bool Identify(IFilter imageFilter)
        {
            // Check if file is not multiple of 512
            if(imageFilter.GetDataForkLength() % 512 == 0)
                return true;

            _extension = Path.GetExtension(imageFilter.GetFilename())?.ToLower();

            if(_extension                            == ".hdf" &&
               imageFilter.GetDataForkLength() % 256 == 0)
                return true;

            // Only for single track data CDs
            if((imageFilter.GetDataForkLength() % 2352 == 0 && imageFilter.GetDataForkLength() <= 846720000) ||
               (imageFilter.GetDataForkLength() % 2448 == 0 && imageFilter.GetDataForkLength() <= 881280000))
            {
                byte[] sync   = new byte[12];
                Stream stream = imageFilter.GetDataForkStream();
                stream.Position = 0;
                stream.Read(sync, 0, 12);

                return _cdSync.SequenceEqual(sync);
            }

            // Check known disk sizes with sectors smaller than 512
            switch(imageFilter.GetDataForkLength())
            {
                #region Commodore
                case 174848:
                case 175531:
                case 197376:
                case 351062:
                case 822400:
                #endregion Commodore

                case 81664:
                case 116480:
                case 242944:
                case 256256:
                case 287488:
                case 306432:
                case 495872:
                case 988416:
                case 995072:
                case 1021696:
                case 1146624:
                case 1177344:
                case 1222400:
                case 1304320:
                case 1255168: return true;
                default: return false;
            }
        }
    }
}