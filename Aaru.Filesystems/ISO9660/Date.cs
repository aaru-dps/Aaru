// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Date.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes timestamps.
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
// Copyright © 2011-2022 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

namespace Aaru.Filesystems;

using System;
using Aaru.Helpers;

public sealed partial class ISO9660
{
    static DateTime? DecodeIsoDateTime(byte[] timestamp) => timestamp?.Length switch
                                                            {
                                                                7 => DecodeIsoDateTime(Marshal.
                                                                    ByteArrayToStructureLittleEndian<
                                                                        IsoTimestamp>(timestamp)),
                                                                17 => DateHandlers.Iso9660ToDateTime(timestamp),
                                                                _  => null
                                                            };

    static DateTime? DecodeIsoDateTime(IsoTimestamp timestamp)
    {
        try
        {
            var date = new DateTime(timestamp.Years + 1900, timestamp.Month, timestamp.Day, timestamp.Hour,
                                    timestamp.Minute, timestamp.Second, DateTimeKind.Unspecified);

            date = date.AddMinutes(timestamp.GmtOffset * 15);

            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById("GMT"));
        }
        catch(Exception)
        {
            // ISO says timestamp can be unspecified
            return null;
        }
    }

    static DateTime? DecodeHighSierraDateTime(HighSierraTimestamp timestamp)
    {
        try
        {
            var date = new DateTime(timestamp.Years + 1900, timestamp.Month, timestamp.Day, timestamp.Hour,
                                    timestamp.Minute, timestamp.Second, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById("GMT"));
        }
        catch(Exception)
        {
            // ISO says timestamp can be unspecified, suppose same for High Sierra
            return null;
        }
    }
}