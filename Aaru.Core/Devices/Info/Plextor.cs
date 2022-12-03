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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Devices.Info;

/// <summary>Contains information about Plextor features</summary>
public class Plextor
{
    /// <summary>Access time limit</summary>
    public byte AccessTimeLimit;
    /// <summary>Drive supports setting book type bit for DVD+R</summary>
    public bool BitSetting;
    /// <summary>Drive supports setting book type bit for DVD+R DL</summary>
    public bool BitSettingDl;
    /// <summary>CD read speed limit</summary>
    public byte CdReadSpeedLimit;
    /// <summary>Time drive has spent reading CDs</summary>
    public uint CdReadTime;
    /// <summary>CD write speed limit</summary>
    public byte CdWriteSpeedLimit;
    /// <summary>Time drive has spent writing CDs</summary>
    public uint CdWriteTime;
    /// <summary>Total number of loaded discs</summary>
    public ushort Discs;
    /// <summary>Drive supports test writing DVD+</summary>
    public bool DvdPlusWriteTest;
    /// <summary>DVD read limit</summary>
    public byte DvdReadSpeedLimit;
    /// <summary>Time drive has spent reading DVDs</summary>
    public uint DvdReadTime;
    /// <summary>Time drive has spent writing DVDs</summary>
    public uint DvdWriteTime;
    /// <summary>Raw contents of EEPROM</summary>
    public byte[] Eeprom;
    /// <summary>Drive supports GigaRec</summary>
    public bool GigaRec;
    /// <summary>Drive will show recordable CDs as embossed</summary>
    public bool HidesRecordables;
    /// <summary>Drive will hide sessions</summary>
    public bool HidesSessions;
    /// <summary>Drive supports hiding recordable CDs and sessions</summary>
    public bool Hiding;
    /// <summary>Drive is a DVD capable drive</summary>
    public bool IsDvd;
    /// <summary>Drive supports PoweRec</summary>
    public bool PoweRec;
    /// <summary>Drive has PoweRec enabled</summary>
    public bool PoweRecEnabled;
    /// <summary>Last used PoweRec in KiB/sec</summary>
    public ushort PoweRecLast;
    /// <summary>Maximum supported PoweRec for currently inserted media in KiB/sec</summary>
    public ushort PoweRecMax;
    /// <summary>Recommended supported PoweRec for currently inserted media in KiB/sec</summary>
    public ushort PoweRecRecommendedSpeed;
    /// <summary>Selected supported PoweRec for currently inserted media in KiB/sec</summary>
    public ushort PoweRecSelected;
    /// <summary>Drive supports SecuRec</summary>
    public bool SecuRec;
    /// <summary>Drive supports SilentMode</summary>
    public bool SilentMode;
    /// <summary>Drive has SilentMode enabled</summary>
    public bool SilentModeEnabled;
    /// <summary>Drive supports SpeedRead</summary>
    public bool SpeedRead;
    /// <summary>Drive has SpeedRead enabled</summary>
    public bool SpeedReadEnabled;
    /// <summary>Drive supports VariRec</summary>
    public bool VariRec;
    /// <summary>Drive supports VariRec for DVDs</summary>
    public bool VariRecDvd;
}