using System;

namespace FileSystemIDandChk
{
    public static class DateHandlers
    {
        static readonly DateTime LisaEpoch = new DateTime(1901, 1, 1, 0, 0, 0);
        static readonly DateTime MacEpoch = new DateTime(1904, 1, 1, 0, 0, 0);
        static readonly DateTime UNIXEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        // Day 0 of Julian Date system
        static readonly DateTime JulianEpoch = new DateTime(1858, 11, 17, 0, 0, 0);

        public static DateTime MacToDateTime(ulong MacTimeStamp)
        {
            return MacEpoch.AddTicks((long)(MacTimeStamp * 10000000));
        }

        public static DateTime LisaToDateTime(UInt32 LisaTimeStamp)
        {
            return LisaEpoch.AddSeconds(LisaTimeStamp);
        }

        public static DateTime UNIXToDateTime(Int32 UNIXTimeStamp)
        {
            return UNIXEpoch.AddSeconds(UNIXTimeStamp);
        }

        public static DateTime UNIXUnsignedToDateTime(UInt32 UNIXTimeStamp)
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
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: year = \"{0}\"", StringHandlers.CToString(fourcharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(fourcharvalue), out year))
                year = 0;
//			year = Convert.ToInt32(StringHandlers.CToString(fourcharvalue));
			
            twocharvalue[0] = VDDateTime[4];
            twocharvalue[1] = VDDateTime[5];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: month = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out month))
                month = 0;
//			month = Convert.ToInt32(StringHandlers.CToString(twocharvalue));
			
            twocharvalue[0] = VDDateTime[6];
            twocharvalue[1] = VDDateTime[7];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: day = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out day))
                day = 0;
//			day = Convert.ToInt32(StringHandlers.CToString(twocharvalue));
			
            twocharvalue[0] = VDDateTime[8];
            twocharvalue[1] = VDDateTime[9];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: hour = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out hour))
                hour = 0;
//			hour = Convert.ToInt32(StringHandlers.CToString(twocharvalue));
			
            twocharvalue[0] = VDDateTime[10];
            twocharvalue[1] = VDDateTime[11];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: minute = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out minute))
                minute = 0;
//			minute = Convert.ToInt32(StringHandlers.CToString(twocharvalue));
			
            twocharvalue[0] = VDDateTime[12];
            twocharvalue[1] = VDDateTime[13];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: second = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out second))
                second = 0;
//			second = Convert.ToInt32(StringHandlers.CToString(twocharvalue));
			
            twocharvalue[0] = VDDateTime[14];
            twocharvalue[1] = VDDateTime[15];
            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: hundredths = \"{0}\"", StringHandlers.CToString(twocharvalue));
            if (!Int32.TryParse(StringHandlers.CToString(twocharvalue), out hundredths))
                hundredths = 0;
//			hundredths = Convert.ToInt32(StringHandlers.CToString(twocharvalue));

            if (MainClass.isDebug)
                Console.WriteLine("ISO9600ToDateTime: decodedDT = new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.Unspecified);", year, month, day, hour, minute, second, hundredths * 10);
            DateTime decodedDT = new DateTime(year, month, day, hour, minute, second, hundredths * 10, DateTimeKind.Unspecified);
			
            return decodedDT;
        }

        // C# works in UTC, VMS on Julian Date, some displacement may occur on disks created outside UTC
        public static DateTime VMSToDateTime(UInt64 vmsDate)
        {
            double delta = vmsDate * 0.0001; // Tenths of microseconds to milliseconds, will lose some detail
            return JulianEpoch.AddMilliseconds(delta);
        }
    }
}

