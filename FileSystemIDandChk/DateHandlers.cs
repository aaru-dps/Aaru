using System;

namespace FileSystemIDandChk
{
	public static class DateHandlers
	{
		private static DateTime MacEpoch = new DateTime(1904, 1, 1, 0, 0, 0);
		private static DateTime UNIXEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
		private static DateTime JulianEpoch = new DateTime(1858, 11, 17, 0, 0, 0); // Day 0 of Julian Date system

		public static DateTime MacToDateTime(ulong MacTimeStamp)
		{
			return MacEpoch.AddTicks((long)(MacTimeStamp*10000000));
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
			year = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(fourcharvalue));
			
			twocharvalue[0] = VDDateTime[4];
			twocharvalue[1] = VDDateTime[5];
			month = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
			twocharvalue[0] = VDDateTime[6];
			twocharvalue[1] = VDDateTime[7];
			day = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
			twocharvalue[0] = VDDateTime[8];
			twocharvalue[1] = VDDateTime[9];
			hour = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
			twocharvalue[0] = VDDateTime[10];
			twocharvalue[1] = VDDateTime[11];
			minute = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
			twocharvalue[0] = VDDateTime[12];
			twocharvalue[1] = VDDateTime[13];
			second = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
			twocharvalue[0] = VDDateTime[14];
			twocharvalue[1] = VDDateTime[15];
			hundredths = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(twocharvalue));
			
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

