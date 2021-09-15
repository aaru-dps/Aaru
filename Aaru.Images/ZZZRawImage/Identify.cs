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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.DiscImages
{
    public sealed partial class ZZZRawImage
    {
        /// <inheritdoc />
        public bool Identify(IFilter imageFilter)
        {
            _extension = Path.GetExtension(imageFilter.Filename)?.ToLower();

            switch(_extension)
            {
                case ".1kn":  return imageFilter.DataForkLength % 1024  == 0;
                case ".2kn":  return imageFilter.DataForkLength % 2048  == 0;
                case ".4kn":  return imageFilter.DataForkLength % 4096  == 0;
                case ".8kn":  return imageFilter.DataForkLength % 8192  == 0;
                case ".16kn": return imageFilter.DataForkLength % 16384 == 0;
                case ".32kn": return imageFilter.DataForkLength % 32768 == 0;
                case ".64kn": return imageFilter.DataForkLength % 65536 == 0;
                case ".512":
                case ".512e": return imageFilter.DataForkLength % 512 == 0;
                case ".128": return imageFilter.DataForkLength % 128 == 0;
                case ".256": return imageFilter.DataForkLength % 256 == 0;
                case ".2352" when imageFilter.DataForkLength % 2352 == 0 && imageFilter.DataForkLength <= 846720000:
                case ".2448" when imageFilter.DataForkLength % 2448 == 0 && imageFilter.DataForkLength <= 881280000:
                    byte[] sync   = new byte[12];
                    Stream stream = imageFilter.GetDataForkStream();
                    stream.Position = 0;
                    stream.Read(sync, 0, 12);

                    return _cdSync.SequenceEqual(sync);
            }

            // Check if file is not multiple of 512
            if(imageFilter.DataForkLength % 512 == 0)
                return true;

            if(_extension                       == ".hdf" &&
               imageFilter.DataForkLength % 256 == 0)
                return true;

            // Only for single track data CDs
            if((imageFilter.DataForkLength % 2352 == 0 && imageFilter.DataForkLength <= 846720000) ||
               (imageFilter.DataForkLength % 2448 == 0 && imageFilter.DataForkLength <= 881280000))
            {
                byte[] sync   = new byte[12];
                Stream stream = imageFilter.GetDataForkStream();
                stream.Position = 0;
                stream.Read(sync, 0, 12);

                return _cdSync.SequenceEqual(sync);
            }

            // Check known disk sizes with sectors smaller than 512
            switch(imageFilter.DataForkLength)
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