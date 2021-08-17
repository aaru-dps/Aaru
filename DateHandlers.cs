// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using Aaru.Console;

namespace Aaru.Helpers
{
    /// <summary>
    /// Helper operations for timestamp management (date and time)
    /// </summary>
    public static class DateHandlers
    {
        static readonly DateTime _lisaEpoch = new DateTime(1901, 1, 1, 0, 0, 0);
        static readonly DateTime _macEpoch  = new DateTime(1904, 1, 1, 0, 0, 0);
        static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        /// <summary>Day 0 of Julian Date system</summary>
        static readonly DateTime _julianEpoch = new DateTime(1858, 11, 17, 0, 0, 0);
        static readonly DateTime _amigaEpoch = new DateTime(1978, 1, 1, 0, 0, 0);

        /// <summary>Converts a Macintosh timestamp to a .NET DateTime</summary>
        /// <param name="macTimeStamp">Macintosh timestamp (seconds since 1st Jan. 1904)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime MacToDateTime(ulong macTimeStamp) => _macEpoch.AddTicks((long)(macTimeStamp * 10000000));

        /// <summary>Converts a Lisa timestamp to a .NET DateTime</summary>
        /// <param name="lisaTimeStamp">Lisa timestamp (seconds since 1st Jan. 1901)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime LisaToDateTime(uint lisaTimeStamp) => _lisaEpoch.AddSeconds(lisaTimeStamp);

        /// <summary>Converts a UNIX timestamp to a .NET DateTime</summary>
        /// <param name="unixTimeStamp">UNIX timestamp (seconds since 1st Jan. 1970)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixToDateTime(int unixTimeStamp) => _unixEpoch.AddSeconds(unixTimeStamp);

        /// <summary>Converts a UNIX timestamp to a .NET DateTime</summary>
        /// <param name="unixTimeStamp">UNIX timestamp (seconds since 1st Jan. 1970)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixToDateTime(long unixTimeStamp) => _unixEpoch.AddSeconds(unixTimeStamp);

        /// <summary>Converts a UNIX timestamp to a .NET DateTime</summary>
        /// <param name="unixTimeStamp">UNIX timestamp (seconds since 1st Jan. 1970)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixUnsignedToDateTime(uint unixTimeStamp) => _unixEpoch.AddSeconds(unixTimeStamp);

        /// <summary>Converts a UNIX timestamp to a .NET DateTime</summary>
        /// <param name="seconds">Seconds since 1st Jan. 1970)</param>
        /// <param name="nanoseconds">Nanoseconds</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixUnsignedToDateTime(uint seconds, uint nanoseconds) =>
            _unixEpoch.AddSeconds(seconds).AddTicks((long)nanoseconds / 100);

        /// <summary>Converts a UNIX timestamp to a .NET DateTime</summary>
        /// <param name="unixTimeStamp">UNIX timestamp (seconds since 1st Jan. 1970)</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixUnsignedToDateTime(ulong unixTimeStamp) => _unixEpoch.AddSeconds(unixTimeStamp);

        /// <summary>Converts a High Sierra Format timestamp to a .NET DateTime</summary>
        /// <param name="vdDateTime">High Sierra Format timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime HighSierraToDateTime(byte[] vdDateTime)
        {
            byte[] isoTime = new byte[17];
            Array.Copy(vdDateTime, 0, isoTime, 0, 16);

            return Iso9660ToDateTime(isoTime);
        }

        // TODO: Timezone
        /// <summary>Converts an ISO9660 timestamp to a .NET DateTime</summary>
        /// <param name="vdDateTime">ISO9660 timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime Iso9660ToDateTime(byte[] vdDateTime)
        {
            byte[] twoCharValue  = new byte[2];
            byte[] fourCharValue = new byte[4];

            fourCharValue[0] = vdDateTime[0];
            fourCharValue[1] = vdDateTime[1];
            fourCharValue[2] = vdDateTime[2];
            fourCharValue[3] = vdDateTime[3];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "year = \"{0}\"",
                                       StringHandlers.CToString(fourCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(fourCharValue, Encoding.ASCII), out int year))
                year = 0;

            twoCharValue[0] = vdDateTime[4];
            twoCharValue[1] = vdDateTime[5];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "month = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int month))
                month = 0;

            twoCharValue[0] = vdDateTime[6];
            twoCharValue[1] = vdDateTime[7];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "day = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int day))
                day = 0;

            twoCharValue[0] = vdDateTime[8];
            twoCharValue[1] = vdDateTime[9];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "hour = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int hour))
                hour = 0;

            twoCharValue[0] = vdDateTime[10];
            twoCharValue[1] = vdDateTime[11];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "minute = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int minute))
                minute = 0;

            twoCharValue[0] = vdDateTime[12];
            twoCharValue[1] = vdDateTime[13];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "second = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int second))
                second = 0;

            twoCharValue[0] = vdDateTime[14];
            twoCharValue[1] = vdDateTime[15];

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler", "hundredths = \"{0}\"",
                                       StringHandlers.CToString(twoCharValue, Encoding.ASCII));

            if(!int.TryParse(StringHandlers.CToString(twoCharValue, Encoding.ASCII), out int hundredths))
                hundredths = 0;

            AaruConsole.DebugWriteLine("ISO9600ToDateTime handler",
                                       "decodedDT = new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.Unspecified);",
                                       year, month, day, hour, minute, second, hundredths * 10);

            sbyte difference = (sbyte)vdDateTime[16];

            var decodedDt = new DateTime(year, month, day, hour, minute, second, hundredths * 10, DateTimeKind.Utc);

            return decodedDt.AddMinutes(difference * -15);
        }

        /// <summary>Converts a VMS timestamp to a .NET DateTime</summary>
        /// <param name="vmsDate">VMS timestamp (tenths of microseconds since day 0 of the Julian Date)</param>
        /// <returns>.NET DateTime</returns>
        /// <remarks>C# works in UTC, VMS on Julian Date, some displacement may occur on disks created outside UTC</remarks>
        public static DateTime VmsToDateTime(ulong vmsDate)
        {
            double delta = vmsDate * 0.0001; // Tenths of microseconds to milliseconds, will lose some detail

            return _julianEpoch.AddMilliseconds(delta);
        }

        /// <summary>Converts an Amiga timestamp to a .NET DateTime</summary>
        /// <param name="days">Days since the 1st Jan. 1978</param>
        /// <param name="minutes">Minutes since o'clock</param>
        /// <param name="ticks">Ticks</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime AmigaToDateTime(uint days, uint minutes, uint ticks)
        {
            DateTime temp = _amigaEpoch.AddDays(days);
            temp = temp.AddMinutes(minutes);

            return temp.AddMilliseconds(ticks * 20);
        }

        /// <summary>Converts an UCSD Pascal timestamp to a .NET DateTime</summary>
        /// <param name="dateRecord">UCSD Pascal timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UcsdPascalToDateTime(short dateRecord)
        {
            int year  = ((dateRecord & 0xFE00) >> 9) + 1900;
            int day   = (dateRecord & 0x01F0) >> 4;
            int month = dateRecord & 0x000F;

            AaruConsole.DebugWriteLine("UCSDPascalToDateTime handler",
                                       "dateRecord = 0x{0:X4}, year = {1}, month = {2}, day = {3}", dateRecord, year,
                                       month, day);

            return new DateTime(year, month, day);
        }

        /// <summary>Converts a DOS timestamp to a .NET DateTime</summary>
        /// <param name="date">Date</param>
        /// <param name="time">Time</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime DosToDateTime(ushort date, ushort time)
        {
            int year   = ((date & 0xFE00) >> 9) + 1980;
            int month  = (date & 0x1E0) >> 5;
            int day    = date & 0x1F;
            int hour   = (time & 0xF800) >> 11;
            int minute = (time & 0x7E0)  >> 5;
            int second = (time & 0x1F) * 2;

            AaruConsole.DebugWriteLine("DOSToDateTime handler", "date = 0x{0:X4}, year = {1}, month = {2}, day = {3}",
                                       date, year, month, day);

            AaruConsole.DebugWriteLine("DOSToDateTime handler",
                                       "time = 0x{0:X4}, hour = {1}, minute = {2}, second = {3}", time, hour, minute,
                                       second);

            DateTime dosDate;

            try
            {
                dosDate = new DateTime(year, month, day, hour, minute, second);
            }
            catch(ArgumentOutOfRangeException)
            {
                dosDate = new DateTime(1980, 1, 1, 0, 0, 0);
            }

            return dosDate;
        }

        /// <summary>Converts a CP/M timestamp to .NET DateTime</summary>
        /// <param name="timestamp">CP/M timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime CpmToDateTime(byte[] timestamp)
        {
            ushort days    = BitConverter.ToUInt16(timestamp, 0);
            int    hours   = timestamp[2];
            int    minutes = timestamp[3];

            DateTime temp = _amigaEpoch.AddDays(days);
            temp = temp.AddHours(hours);
            temp = temp.AddMinutes(minutes);

            return temp;
        }

        /// <summary>Converts an ECMA timestamp to a .NET DateTime</summary>
        /// <param name="typeAndTimeZone">Timezone</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="hour">Hour</param>
        /// <param name="minute">Minute</param>
        /// <param name="second">Second</param>
        /// <param name="centiseconds">Centiseconds</param>
        /// <param name="hundredsOfMicroseconds">Hundreds of microseconds</param>
        /// <param name="microseconds">Microseconds</param>
        /// <returns></returns>
        public static DateTime EcmaToDateTime(ushort typeAndTimeZone, short year, byte month, byte day, byte hour,
                                              byte minute, byte second, byte centiseconds, byte hundredsOfMicroseconds,
                                              byte microseconds)
        {
            byte specification = (byte)((typeAndTimeZone & 0xF000) >> 12);

            long ticks = ((long)centiseconds * 100000) + ((long)hundredsOfMicroseconds * 1000) +
                         ((long)microseconds * 10);

            if(specification == 0)
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks);

            ushort preOffset = (ushort)(typeAndTimeZone & 0xFFF);
            short  offset;

            if((preOffset & 0x800) == 0x800)
                offset = (short)(preOffset | 0xF000);
            else
                offset = (short)(preOffset & 0x7FF);

            if(offset == -2047)
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified).AddTicks(ticks);

            if(offset < -1440 ||
               offset > 1440)
                offset = 0;

            return new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(0, offset, 0)).
                   AddTicks(ticks).DateTime;
        }

        /// <summary>Converts a Solaris high resolution timestamp to .NET DateTime</summary>
        /// <param name="hrTimeStamp">Solaris high resolution timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime UnixHrTimeToDateTime(ulong hrTimeStamp) =>
            _unixEpoch.AddTicks((long)(hrTimeStamp / 100));

        /// <summary>Converts an OS-9 timestamp to .NET DateTime</summary>
        /// <param name="date">OS-9 timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime Os9ToDateTime(byte[] date)
        {
            if(date == null ||
               (date.Length != 3 && date.Length != 5))
                return DateTime.MinValue;

            DateTime os9Date;

            try
            {
                os9Date = date.Length == 5 ? new DateTime(1900 + date[0], date[1], date[2], date[3], date[4], 0)
                              : new DateTime(1900              + date[0], date[1], date[2], 0, 0, 0);
            }
            catch(ArgumentOutOfRangeException)
            {
                os9Date = new DateTime(1900, 0, 0, 0, 0, 0);
            }

            return os9Date;
        }

        /// <summary>Converts a LIF timestamp to .NET DateTime</summary>
        /// <param name="date">LIF timestamp</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime LifToDateTime(byte[] date)
        {
            if(date        == null ||
               date.Length != 6)
                return new DateTime(1970, 1, 1, 0, 0, 0);

            return LifToDateTime(date[0], date[1], date[2], date[3], date[4], date[5]);
        }

        /// <summary>Converts a LIF timestamp to .NET DateTime</summary>
        /// <param name="year">Yer</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <param name="hour">Hour</param>
        /// <param name="minute">Minute</param>
        /// <param name="second">Second</param>
        /// <returns>.NET DateTime</returns>
        public static DateTime LifToDateTime(byte year, byte month, byte day, byte hour, byte minute, byte second)
        {
            try
            {
                int iyear   = ((year   >> 4) * 10) + (year   & 0xF);
                int imonth  = ((month  >> 4) * 10) + (month  & 0xF);
                int iday    = ((day    >> 4) * 10) + (day    & 0xF);
                int iminute = ((minute >> 4) * 10) + (minute & 0xF);
                int ihour   = ((hour   >> 4) * 10) + (hour   & 0xF);
                int isecond = ((second >> 4) * 10) + (second & 0xF);

                if(iyear >= 70)
                    iyear += 1900;
                else
                    iyear += 2000;

                return new DateTime(iyear, imonth, iday, ihour, iminute, isecond);
            }
            catch(ArgumentOutOfRangeException)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0);
            }
        }
    }
}