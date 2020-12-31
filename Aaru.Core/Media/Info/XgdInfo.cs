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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Media.Info
{
    public sealed class XgdInfo
    {
        public ulong GameSize;
        public ulong L0Video;
        public ulong L1Video;
        public ulong LayerBreak;
        public ulong MiddleZone;
        public ulong TotalSize;
    }
}