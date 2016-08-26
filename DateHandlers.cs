// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DateHandlers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Convert several timestamp formats to C# DateTime.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;

namespace DiscImageChef
{
    public static class DateHandlers
    {
        static readonly DateTime LisaEpoch = new DateTime(1901, 1, 1, 0, 0, 0);
        static readonly DateTime MacEpoch = new DateTime(1904, 1, 1, 0, 0, 0);
        static readonly DateTime UNIXEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        // Day 0 of Julian Date system
        static readonly DateTime JulianEpoch = new DateTime(1858, 11, 17, 0, 0, 0);
        static readonly DateTime AmigaEpoch = new DateTime(1978, 1, 1, 0, 0, 0);

        public static DateTime MacToDateTime(ulong MacTimeStamp)
        {
            return MacEpoch.AddTicks((long)(MacTimeStamp * 10000000));
        }

        public static DateTime LisaToDateTime(uint LisaTimeStamp)
        {
            return LisaEpoch.AddSeconds(LisaTimeStamp);
        }

        public static DateTime UNIXToDateTime(int UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime UNIXUnsignedToDateTime(uint UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime ISO9660ToDateTime(byte[] VDDateTime)
        {
            int year, month, day, hour, minute, second, hundredths;
            byte[] twocharvalue = new byte[2];
            byte[] fourcharvalue = new byte[4];

            fourcharvalue[0] = VDDateTime[0];
            fourcharvalue[1] = VDDateTime[1];
            fourcharvalue[2] = VDDateTime[2];
            fourcharvalue[3] = VDDateTime[3];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "year = \"{0}\"", StringHandlers.CToString(fourcharvalue));
            if(!int.TryParse(StringHandlers.CToString(fourcharvalue), out year))
                year = 0;
            //			year = Convert.ToInt32(StringHandlers.CToString(fourcharvalue));

            twocharvalue[0] = VDDateTime[4];
            twocharvalue[1] = VDDateTime[5];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "month = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out month))
                month = 0;
            //			month = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            twocharvalue[0] = VDDateTime[6];
            twocharvalue[1] = VDDateTime[7];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "day = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out day))
                day = 0;
            //			day = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            twocharvalue[0] = VDDateTime[8];
            twocharvalue[1] = VDDateTime[9];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "hour = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out hour))
                hour = 0;
            //			hour = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            twocharvalue[0] = VDDateTime[10];
            twocharvalue[1] = VDDateTime[11];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "minute = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out minute))
                minute = 0;
            //			minute = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            twocharvalue[0] = VDDateTime[12];
            twocharvalue[1] = VDDateTime[13];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "second = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out second))
                second = 0;
            //			second = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            twocharvalue[0] = VDDateTime[14];
            twocharvalue[1] = VDDateTime[15];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "hundredths = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue), out hundredths))
                hundredths = 0;
            //			hundredths = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "decodedDT = new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.Unspecified);", year, month, day, hour, minute, second, hundredths * 10);
            DateTime decodedDT = new DateTime(year, month, day, hour, minute, second, hundredths * 10, DateTimeKind.Unspecified);

            return decodedDT;
        }

        // C# works in UTC, VMS on Julian Date, some displacement may occur on disks created outside UTC
        public static DateTime VMSToDateTime(ulong vmsDate)
        {
            double delta = vmsDate * 0.0001; // Tenths of microseconds to milliseconds, will lose some detail
            return JulianEpoch.AddMilliseconds(delta);
        }

        public static DateTime AmigaToDateTime(uint days, uint minutes, uint ticks)
        {
            DateTime temp = AmigaEpoch.AddDays(days);
            temp = temp.AddMinutes(minutes);
            return temp.AddMilliseconds(ticks * 20);
        }

        public static DateTime UCSDPascalToDateTime(short dateRecord)
        {
            int year = ((dateRecord & 0xFE00) >> 9) + 1900;
            int day = (dateRecord & 0x01F0) >> 4;
            int month = (dateRecord & 0x000F);

            DicConsole.DebugWriteLine("UCSDPascalToDateTime handler", "dateRecord = 0x{0:X4}, year = {1}, month = {2}, day = {3}", dateRecord, year, month, day);
            return new DateTime(year, month, day);
        }

        public static DateTime DOSToDateTime(ushort date, ushort time)
        {
            int year = ((date & 0xFE00) >> 9) + 1980;
            int month = (date & 0x1E0) >> 5;
            int day = date & 0x1F;
            int hour = (time & 0xF800) >> 11;
            int minute = (time & 0x7E0) >> 5;
            int second = (time & 0x1F) * 2;

            DicConsole.DebugWriteLine("DOSToDateTime handler", "date = 0x{0:X4}, year = {1}, month = {2}, day = {3}", date, year, month, day);
            DicConsole.DebugWriteLine("DOSToDateTime handler", "time = 0x{0:X4}, hour = {1}, minute = {2}, second = {3}", time, hour, minute, second);
            return new DateTime(year, month, day, hour, minute, second);
        }

        public static DateTime CPMToDateTime(byte[] timestamp)
        {
            ushort days = BitConverter.ToUInt16(timestamp, 0);
            int hours = timestamp[2];
            int minutes = timestamp[3];

            DateTime temp = AmigaEpoch.AddDays(days);
            temp = temp.AddHours(hours);
            temp = temp.AddMinutes(minutes);

            return temp;
        }
    }
}

