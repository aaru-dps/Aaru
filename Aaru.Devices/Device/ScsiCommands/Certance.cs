// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Certance.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Certance vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Certance SCSI devices.
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
using System.Diagnostics.CodeAnalysis;
using Aaru.Console;

namespace Aaru.Devices;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class Device
{
    /// <summary>Parks the load arm in preparation for transport</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool CertancePark(out byte[] senseBuffer, uint timeout, out double duration) =>
        CertanceParkUnpark(out senseBuffer, true, timeout, out duration);

    /// <summary>Unparks the load arm prior to operation</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool CertanceUnpark(out byte[] senseBuffer, uint timeout, out double duration) =>
        CertanceParkUnpark(out senseBuffer, false, timeout, out duration);

    /// <summary>Parks the load arm in preparation for transport or unparks it prior to operation</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="park">If set to <c>true</c>, parks the load arm</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool CertanceParkUnpark(out byte[] senseBuffer, bool park, uint timeout, out double duration)
    {
        byte[] buffer = Array.Empty<byte>();
        var    cdb    = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.CertanceParkUnpark;

        if(park) cdb[4] = 1;

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.None,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.CERTANCE_PARK_UNPARK_took_0_ms, duration);

        return sense;
    }
}