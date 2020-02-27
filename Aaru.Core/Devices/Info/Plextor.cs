// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plextor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core.
//
// --[ Description ] ----------------------------------------------------------
//
//     Structures for Plextor features.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Devices.Info
{
    public class Plextor
    {
        public byte   AccessTimeLimit;
        public bool   BitSetting;
        public bool   BitSettingDl;
        public byte   CdReadSpeedLimit;
        public uint   CdReadTime;
        public byte   CdWriteSpeedLimit;
        public uint   CdWriteTime;
        public ushort Discs;
        public bool   DvdPlusWriteTest;
        public byte   DvdReadSpeedLimit;
        public uint   DvdReadTime;
        public uint   DvdWriteTime;
        public byte[] Eeprom;
        public bool   GigaRec;
        public bool   HidesRecordables;
        public bool   HidesSessions;
        public bool   Hiding;
        public bool   IsDvd;
        public bool   PoweRec;
        public bool   PoweRecEnabled;
        public ushort PoweRecLast;
        public ushort PoweRecMax;
        public ushort PoweRecRecommendedSpeed;
        public ushort PoweRecSelected;
        public bool   SecuRec;
        public bool   SilentMode;
        public bool   SilentModeEnabled;
        public bool   SpeedRead;
        public bool   SpeedReadEnabled;
        public bool   VariRec;
        public bool   VariRecDvd;
    }
}