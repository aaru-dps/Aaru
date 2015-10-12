// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// High level commands used to directly access devices
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$

using System;
using DiscImageChef.Interop;
using Microsoft.Win32.SafeHandles;

namespace DiscImageChef.Devices
{
    public static class Command
    {
        public static int SendScsiCommand(object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, Enums.ScsiDirection direction, out double duration, out bool sense)
        {
            Interop.PlatformID ptID = DetectOS.GetRealPlatformID();

            return SendScsiCommand(ptID, (SafeFileHandle)fd, cdb, ref buffer, out senseBuffer, timeout, direction, out duration, out sense);
        }

        public static int SendScsiCommand(Interop.PlatformID ptID, object fd, byte[] cdb, ref byte[] buffer, out byte[] senseBuffer, uint timeout, Enums.ScsiDirection direction, out double duration, out bool sense)
        {
            switch (ptID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        Windows.ScsiIoctlDirection dir;

                        switch (direction)
                        {
                            case Enums.ScsiDirection.In:
                                dir = Windows.ScsiIoctlDirection.In;
                                break;
                            case Enums.ScsiDirection.Out:
                                dir = Windows.ScsiIoctlDirection.Out;
                                break;
                            default:
                                dir = Windows.ScsiIoctlDirection.Unspecified;
                                break;
                        }

                        return Windows.Command.SendScsiCommand((SafeFileHandle)fd, cdb, ref buffer, out senseBuffer, timeout, dir, out duration, out sense);
                    }
                case Interop.PlatformID.Linux:
                    {
                        Linux.ScsiIoctlDirection dir;

                        switch (direction)
                        {
                            case Enums.ScsiDirection.In:
                                dir = Linux.ScsiIoctlDirection.In;
                                break;
                            case Enums.ScsiDirection.Out:
                                dir = Linux.ScsiIoctlDirection.Out;
                                break;
                            case Enums.ScsiDirection.Bidirectional:
                                dir = Linux.ScsiIoctlDirection.Unspecified;
                                break;
                            case Enums.ScsiDirection.None:
                                dir = Linux.ScsiIoctlDirection.None;
                                break;
                            default:
                                dir = Linux.ScsiIoctlDirection.Unknown;
                                break;
                        }

                        return Linux.Command.SendScsiCommand((int)fd, cdb, ref buffer, out senseBuffer, timeout, dir, out duration, out sense);
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", ptID));
            }
        }
    }
}

