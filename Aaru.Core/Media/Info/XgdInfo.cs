// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XgdInfo.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Structure containing information about a XGD.
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

namespace Aaru.Core.Media.Info
{
    /// <summary>Information about an XGD, XGD2 or XGD3 media.</summary>
    public sealed class XgdInfo
    {
        /// <summary>Size of the game partition</summary>
        public ulong GameSize;
        /// <summary>Size of layer 0 of the video partition</summary>
        public ulong L0Video;
        /// <summary>Size of layer 1 of the video partition</summary>
        public ulong L1Video;
        /// <summary>Real layer break</summary>
        public ulong LayerBreak;
        /// <summary>Size of the middle zone</summary>
        public ulong MiddleZone;
        /// <summary>Total size of media</summary>
        public ulong TotalSize;
    }
}