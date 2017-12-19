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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
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
        static readonly DateTime AppleDoubleEpoch = new DateTime(1970, 1, 1, 0, 0, 0);

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

        public static DateTime UNIXToDateTime(long UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime UNIXUnsignedToDateTime(uint UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime UNIXUnsignedToDateTime(uint seconds, uint nanoseconds)
        {
            return UNIXEpoch.AddSeconds(seconds).AddTicks((long)nanoseconds / 100);
        }

        public static DateTime UNIXUnsignedToDateTime(ulong UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime HighSierraToDateTime(byte[] VDDateTime)
        {
            byte[] isotime = new byte[17];
            Array.Copy(VDDateTime, 0, isotime, 0, 16);
            return ISO9660ToDateTime(isotime);
        }

        // TODO: Timezone
        public static DateTime ISO9660ToDateTime(byte[] VDDateTime)
        {
            int year, month, day, hour, minute, second, hundredths;
            byte[] twocharvalue = new byte[2];
            byte[] fourcharvalue = new byte[4];

            fourcharvalue[0] = VDDateTime[0];
            fourcharvalue[1] = VDDateTime[1];
            fourcharvalue[2] = VDDateTime[2];
            fourcharvalue[3] = VDDateTime[3];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "year = \"{0}\"",
                                      StringHandlers.CToString(fourcharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(fourcharvalue, Encoding.ASCII), out year)) year = 0;
            //			year = Convert.ToInt32(StringHandlers.CToString(fourcharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[4];
            twocharvalue[1] = VDDateTime[5];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "month = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out month)) month = 0;
            //			month = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[6];
            twocharvalue[1] = VDDateTime[7];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "day = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out day)) day = 0;
            //			day = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[8];
            twocharvalue[1] = VDDateTime[9];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "hour = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out hour)) hour = 0;
            //			hour = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[10];
            twocharvalue[1] = VDDateTime[11];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "minute = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out minute)) minute = 0;
            //			minute = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[12];
            twocharvalue[1] = VDDateTime[13];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "second = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out second)) second = 0;
            //			second = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            twocharvalue[0] = VDDateTime[14];
            twocharvalue[1] = VDDateTime[15];
            DicConsole.DebugWriteLine("ISO9600ToDateTime handler", "hundredths = \"{0}\"",
                                      StringHandlers.CToString(twocharvalue, Encoding.ASCII));
            if(!int.TryParse(StringHandlers.CToString(twocharvalue, Encoding.ASCII), out hundredths)) hundredths = 0;
            //			hundredths = Convert.ToInt32(StringHandlers.CToString(twocharvalue, Encoding.ASCII));

            DicConsole.DebugWriteLine("ISO9600ToDateTime handler",
                                      "decodedDT = new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.Unspecified);",
                                      year, month, day, hour, minute, second, hundredths * 10);
            DateTime decodedDT = new DateTime(year, month, day, hour, minute, second, hundredths * 10,
                                              DateTimeKind.Unspecified);

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

            DicConsole.DebugWriteLine("UCSDPascalToDateTime handler",
                                      "dateRecord = 0x{0:X4}, year = {1}, month = {2}, day = {3}", dateRecord, year,
                                      month, day);
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

            DicConsole.DebugWriteLine("DOSToDateTime handler", "date = 0x{0:X4}, year = {1}, month = {2}, day = {3}",
                                      date, year, month, day);
            DicConsole.DebugWriteLine("DOSToDateTime handler",
                                      "time = 0x{0:X4}, hour = {1}, minute = {2}, second = {3}", time, hour, minute,
                                      second);

            DateTime dosdate;
            try { dosdate = new DateTime(year, month, day, hour, minute, second); }
            catch(ArgumentOutOfRangeException) { dosdate = new DateTime(1980, 1, 1, 0, 0, 0); }

            return dosdate;
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

        public static DateTime AppleDoubleToDateTime(ulong AppleDoubleTimeStamp)
        {
            return AppleDoubleEpoch.AddSeconds(AppleDoubleTimeStamp);
        }

        public static DateTime ECMAToDateTime(ushort typeAndTimeZone, short year, byte month, byte day, byte hour,
                                              byte minute, byte second, byte centiseconds, byte hundredsOfMicroseconds,
                                              byte microseconds)
        {
            byte specification = (byte)((typeAndTimeZone & 0xF000) >> 12);
            long ticks = (long)centiseconds * 100000 + (long)hundredsOfMicroseconds * 1000 + (long)microseconds * 10;
            if(specification == 0)
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks);

            ushort preOffset = (ushort)(typeAndTimeZone & 0xFFF);
            short offset;

            if((preOffset & 0x800) == 0x800) { offset = (short)(preOffset | 0xF000); }
            else offset = (short)(preOffset & 0x7FF);

            if(offset == -2047)
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified).AddTicks(ticks);

            if(offset < -1440 || offset > 1440) offset = 0;

            return new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(0, offset, 0))
                .AddTicks(ticks).DateTime;
        }

        public static DateTime UNIXHrTimeToDateTime(ulong HRTimeStamp)
        {
            return UNIXEpoch.AddTicks((long)(HRTimeStamp / 100));
        }

        public static DateTime OS9ToDateTime(byte[] date)
        {
            if(date == null || (date.Length != 3 && date.Length != 5)) return DateTime.MinValue;

            DateTime os9date;

            try
            {
                os9date = date.Length == 5
                              ? new DateTime(1900 + date[0], date[1], date[2], date[3], date[4], 0)
                              : new DateTime(1900 + date[0], date[1], date[2], 0, 0, 0);
            }
            catch(ArgumentOutOfRangeException) { os9date = new DateTime(1900, 0, 0, 0, 0, 0); }

            return os9date;
        }

        public static DateTime LifToDateTime(byte[] date)
        {
            if(date == null || date.Length != 6) return new DateTime(1970, 1, 1, 0, 0, 0);

            return LifToDateTime(date[0], date[1], date[2], date[3], date[4], date[5]);
        }

        public static DateTime LifToDateTime(byte year, byte month, byte day, byte hour, byte minute, byte second)
        {
            try
            {
                int iyear = ((year >> 4) * 10 + (year & 0xF));
                int imonth = (month >> 4) * 10 + (month & 0xF);
                int iday = (day >> 4) * 10 + (day & 0xF);
                int iminute = (minute >> 4) * 10 + (minute & 0xF);
                int ihour = (hour >> 4) * 10 + (hour & 0xF);
                int isecond = (second >> 4) * 10 + (second & 0xF);

                if(iyear >= 70) iyear += 1900;
                else iyear += 2000;

                return new DateTime(iyear, imonth, iday, ihour, iminute, isecond);
            }
            catch(ArgumentOutOfRangeException) { return new DateTime(1970, 1, 1, 0, 0, 0); }
        }
    }
}