// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Kreon.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Kreon vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Kreon hacked drives.
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

using System;
using Aaru.Console;

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Sets the drive to the xtreme unlocked state</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool KreonDeprecatedUnlock(out byte[] senseBuffer, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.KreonCommand;
        cdb[1] = 0x08;
        cdb[2] = 0x01;
        cdb[3] = 0x01;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.KREON_DEPRECATED_UNLOCK_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sets the drive to the locked state.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool KreonLock(out byte[] senseBuffer, uint timeout, out double duration) =>
        KreonSetLockState(out senseBuffer, KreonLockStates.Locked, timeout, out duration);

    /// <summary>Sets the drive to the xtreme unlocked state</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool KreonUnlockXtreme(out byte[] senseBuffer, uint timeout, out double duration) =>
        KreonSetLockState(out senseBuffer, KreonLockStates.Xtreme, timeout, out duration);

    /// <summary>Sets the drive to the wxripper unlocked state</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool KreonUnlockWxripper(out byte[] senseBuffer, uint timeout, out double duration) =>
        KreonSetLockState(out senseBuffer, KreonLockStates.Wxripper, timeout, out duration);

    /// <summary>Sets the drive to the specified lock state</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="state">Lock state</param>
    public bool KreonSetLockState(out byte[] senseBuffer, KreonLockStates state, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var    cdb    = new byte[6];
        byte[] buffer = Array.Empty<byte>();

        cdb[0] = (byte)ScsiCommands.KreonCommand;
        cdb[1] = 0x08;
        cdb[2] = 0x01;
        cdb[3] = 0x11;
        cdb[4] = (byte)state;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.KREON_SET_LOCK_STATE_took_0_ms, duration);

        return sense || Error;
    }

    /// <summary>Gets a list of supported features</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="features">Features supported by drive.</param>
    public bool KreonGetFeatureList(out byte[] senseBuffer, out KreonFeatures features, uint timeout,
                                    out double duration)
    {
        senseBuffer = new byte[64];
        var cdb    = new byte[6];
        var buffer = new byte[26];
        features = 0;

        cdb[0] = (byte)ScsiCommands.KreonCommand;
        cdb[1] = 0x08;
        cdb[2] = 0x01;
        cdb[3] = 0x10;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.KREON_GET_FEATURE_LIST_took_0_ms, duration);

        if(sense)
            return true;

        if(buffer[0] != 0xA5 ||
           buffer[1] != 0x5A ||
           buffer[2] != 0x5A ||
           buffer[3] != 0xA5)
            return true;

        for(var i = 4; i < 26; i += 2)
        {
            var feature = BitConverter.ToUInt16(buffer, i);

            if(feature == 0x0000)
                break;

            switch(feature)
            {
                case 0x0001:
                    features |= KreonFeatures.XtremeUnlock360;

                    break;
                case 0x0101:
                    features |= KreonFeatures.WxripperUnlock360;

                    break;
                case 0x2001:
                    features |= KreonFeatures.DecryptSs360;

                    break;
                case 0x2101:
                    features |= KreonFeatures.ChallengeResponse360;

                    break;
                case 0x0002:
                    features |= KreonFeatures.XtremeUnlock;

                    break;
                case 0x0102:
                    features |= KreonFeatures.WxripperUnlock;

                    break;
                case 0x2002:
                    features |= KreonFeatures.DecryptSs;

                    break;
                case 0x2102:
                    features |= KreonFeatures.ChallengeResponse;

                    break;
                case 0x00F0:
                    features |= KreonFeatures.Lock;

                    break;
                case 0x01F0:
                    features |= KreonFeatures.ErrorSkipping;

                    break;
            }
        }

        return false;
    }

    /// <summary>Gets the SS sector.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="buffer">The SS sector.</param>
    /// <param name="requestNumber">Request number.</param>
    public bool KreonExtractSs(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration,
                               byte       requestNumber = 0x00)
    {
        buffer = new byte[2048];
        var cdb = new byte[12];
        senseBuffer = new byte[64];

        cdb[0]  = (byte)ScsiCommands.KreonSsCommand;
        cdb[1]  = 0x00;
        cdb[2]  = 0xFF;
        cdb[3]  = 0x02;
        cdb[4]  = 0xFD;
        cdb[5]  = 0xFF;
        cdb[6]  = 0xFE;
        cdb[7]  = 0x00;
        cdb[8]  = 0x08;
        cdb[9]  = 0x00;
        cdb[10] = requestNumber;
        cdb[11] = 0xC0;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.KREON_EXTRACT_SS_took_0_ms, duration);

        return sense || Error;
    }
}